#if !LIV_UNIVERSAL_RENDER
using UnityEngine;
using UnityEngine.Rendering;

namespace LIV.SDK.Unity
{
    public partial class SDKRender : System.IDisposable
    {
        // Renders the clip plane in the foreground texture
        private CommandBuffer _clipPlaneCommandBuffer = null;
        // Renders the clipped opaque content in to the foreground texture alpha
        private CommandBuffer _combineAlphaCommandBuffer = null;
        // Captures texture before post-effects
        private CommandBuffer _captureTextureCommandBuffer = null;
        // Renders captured texture
        private CommandBuffer _applyTextureCommandBuffer = null;
        // Renders background and foreground in single render
        private CommandBuffer _optimizedRenderingCommandBuffer = null;

        private CameraEvent _clipPlaneCameraEvent = CameraEvent.AfterForwardOpaque;
        private CameraEvent _clipPlaneCombineAlphaCameraEvent = CameraEvent.AfterEverything;
        private CameraEvent _captureTextureEvent = CameraEvent.BeforeImageEffects;
        private CameraEvent _applyTextureEvent = CameraEvent.AfterEverything;
        private CameraEvent _optimizedRenderingCameraEvent = CameraEvent.AfterForwardAlpha;

        // Clear material
        private Material _clipPlaneSimpleMaterial = null;
        // Transparent material for visual debugging
        private Material _clipPlaneSimpleDebugMaterial = null;
        // Tessellated height map clear material
        private Material _clipPlaneComplexMaterial = null;
        // Tessellated height map clear material for visual debugging
        private Material _clipPlaneComplexDebugMaterial = null;
        // Reveal opaque geometry in alpha channel
        private Material _writeOpaqueToAlphaMaterial = null;
        // Combine existing alpha channel with another texture alpha channel
        private Material _combineAlphaMaterial = null;
        // Simple blit material
        private Material _writeMaterial = null;
        // Enforce that forward rendering is being executed during deffered rendering
        private Material _forceForwardRenderingMaterial = null;

        private RenderTexture _backgroundRenderTexture = null;
        private RenderTexture _foregroundRenderTexture = null;
        private RenderTexture _optimizedRenderTexture = null;
        private RenderTexture _complexClipPlaneRenderTexture = null;

        bool UseDeferredRendering {
            get {
                return _cameraInstance.actualRenderingPath == RenderingPath.DeferredLighting ||
                _cameraInstance.actualRenderingPath == RenderingPath.DeferredShading;
            }
        }

        bool InterlacedRendering {
            get {
                return SDKUtils.FeatureEnabled(InputFrame.features, Features.InterlacedRender);
            }
        }

        bool CanRenderBackground {
            get {
                if (InterlacedRendering)
                {
                    // Render only if frame is even
                    if (Time.frameCount % 2 != 0) return false;
                }
                return SDKUtils.FeatureEnabled(InputFrame.features, Features.BackgroundRender) && _backgroundRenderTexture != null;
            }
        }

        bool CanRenderForeground {
            get {
                if (InterlacedRendering)
                {
                    // Render only if frame is odd
                    if (Time.frameCount % 2 != 1) return false;
                }
                return SDKUtils.FeatureEnabled(InputFrame.features, Features.ForegroundRender) && _foregroundRenderTexture != null;
            }
        }

        bool CanRenderOptimized {
            get {
                return SDKUtils.FeatureEnabled(InputFrame.features, Features.OptimizedRender) && _optimizedRenderTexture != null; ;
            }
        }

        public SDKRender(Liv liv)
        {
            _liv = liv;
            CreateAssets();
        }

        public void Render()
        {
            UpdateBridgeResolution();
            UpdateBridgeInputFrame();
            SDKUtils.ApplyUserSpaceTransform(this);
            UpdateTextures();
            InvokePreRender();
            if (CanRenderBackground) RenderBackground();
            if (CanRenderForeground) RenderForeground();
            if (CanRenderOptimized) RenderOptimized();
            IvokePostRender();
            SDKUtils.CreateBridgeOutputFrame(this);
            SDKBridge.IssuePluginEvent();
        }

        // Default render without any special changes
        private void RenderBackground()
        {
            bool debugClipPlane = SDKUtils.FeatureEnabled(InputFrame.features, Features.DebugClipPlane);
            bool debug = debugClipPlane;

            SDKUtils.SetCamera(_cameraInstance, _cameraInstance.transform, _inputFrame, LocalToWorldMatrix, SpectatorLayerMask);
            _cameraInstance.targetTexture = _backgroundRenderTexture;

            RenderTexture tempRenderTexture = null;

            bool overridePostProcessing = SDKUtils.FeatureEnabled(InputFrame.features, Features.OverridePostProcessing);
            if (overridePostProcessing)
            {
                tempRenderTexture = RenderTexture.GetTemporary(_backgroundRenderTexture.width, _backgroundRenderTexture.height, 0, _backgroundRenderTexture.format);
#if UNITY_EDITOR
                tempRenderTexture.name = "LIV.TemporaryRenderTexture";
#endif
                _captureTextureCommandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, tempRenderTexture);
                _applyTextureCommandBuffer.Blit(tempRenderTexture, BuiltinRenderTextureType.CurrentActive);

                _cameraInstance.AddCommandBuffer(_captureTextureEvent, _captureTextureCommandBuffer);
                _cameraInstance.AddCommandBuffer(_applyTextureEvent, _applyTextureCommandBuffer);
            }

            SDKShaders.StartRendering();
            SDKShaders.StartBackgroundRendering();
            InvokePreRenderBackground();
            SendTextureToBridge(_backgroundRenderTexture, TextureID.BackgroundColorBufferID);
            if (debug) RenderDebugPreRender();
            _cameraInstance.Render();
            InvokePostRenderBackground();
            if (debug) RenderDebugPostRender(_backgroundRenderTexture);
            _cameraInstance.targetTexture = null;
            SDKShaders.StopBackgroundRendering();
            SDKShaders.StopRendering();

            if (overridePostProcessing)
            {
                _cameraInstance.RemoveCommandBuffer(_captureTextureEvent, _captureTextureCommandBuffer);
                _cameraInstance.RemoveCommandBuffer(_applyTextureEvent, _applyTextureCommandBuffer);

                _captureTextureCommandBuffer.Clear();
                _applyTextureCommandBuffer.Clear();

                RenderTexture.ReleaseTemporary(tempRenderTexture);
            }
        }

        // Extract the image which is in front of our clip plane
        // The compositing is heavily relying on the alpha channel, therefore we want to make sure it does
        // not get corrupted by the postprocessing or any shader
        private void RenderForeground()
        {
            bool debugClipPlane = SDKUtils.FeatureEnabled(InputFrame.features, Features.DebugClipPlane);
            bool renderComplexClipPlane = SDKUtils.FeatureEnabled(InputFrame.features, Features.ComplexClipPlane);
            bool renderGroundClipPlane = SDKUtils.FeatureEnabled(InputFrame.features, Features.GroundClipPlane);
            bool overridePostProcessing = SDKUtils.FeatureEnabled(InputFrame.features, Features.OverridePostProcessing);
            bool fixPostEffectsAlpha = SDKUtils.FeatureEnabled(InputFrame.features, Features.FixForegroundAlpha) | _liv.FixPostEffectsAlpha;
            bool debug = debugClipPlane;

            MonoBehaviour[] behaviours = null;
            bool[] wasBehaviourEnabled = null;
            if (DisableStandardAssets) SDKUtils.DisableStandardAssets(_cameraInstance, ref behaviours, ref wasBehaviourEnabled);

            // Capture camera defaults
            CameraClearFlags capturedClearFlags = _cameraInstance.clearFlags;
            Color capturedBgColor = _cameraInstance.backgroundColor;
            Color capturedFogColor = RenderSettings.fogColor;

            // Make sure that fog does not corrupt alpha channel
            RenderSettings.fogColor = new Color(capturedFogColor.r, capturedFogColor.g, capturedFogColor.b, 0f);
            SDKUtils.SetCamera(_cameraInstance, _cameraInstance.transform, _inputFrame, LocalToWorldMatrix, SpectatorLayerMask);
            _cameraInstance.clearFlags = CameraClearFlags.Color;
            _cameraInstance.backgroundColor = Color.clear;
            _cameraInstance.targetTexture = _foregroundRenderTexture;

            RenderTexture capturedAlphaRenderTexture = RenderTexture.GetTemporary(_foregroundRenderTexture.width, _foregroundRenderTexture.height, 0, _foregroundRenderTexture.format);
#if UNITY_EDITOR
            capturedAlphaRenderTexture.name = "LIV.CapturedAlphaRenderTexture";
#endif
            // Render opaque pixels into alpha channel
            _clipPlaneCommandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, _writeOpaqueToAlphaMaterial, 0, 0);

            // Render clip plane
            Matrix4x4 clipPlaneTransform = LocalToWorldMatrix * (Matrix4x4)_inputFrame.clipPlane.transform;
            _clipPlaneCommandBuffer.DrawMesh(_clipPlaneMesh, clipPlaneTransform,
                GetClipPlaneMaterial(debugClipPlane, renderComplexClipPlane, ColorWriteMask.All, ref _clipPlaneMaterialProperty), 0, 0, _clipPlaneMaterialProperty);

            // Render ground clip plane
            if (renderGroundClipPlane)
            {
                Matrix4x4 groundClipPlaneTransform = LocalToWorldMatrix * (Matrix4x4)_inputFrame.groundClipPlane.transform;
                _clipPlaneCommandBuffer.DrawMesh(_clipPlaneMesh, groundClipPlaneTransform,
                GetClipPlaneMaterial(debugClipPlane, false, ColorWriteMask.All, ref _groundPlaneMaterialProperty), 0, 0, _groundPlaneMaterialProperty);
            }

            // Copy alpha in to texture
            _clipPlaneCommandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, capturedAlphaRenderTexture);
            _cameraInstance.AddCommandBuffer(_clipPlaneCameraEvent, _clipPlaneCommandBuffer);

            // Fix alpha corruption by post processing
            RenderTexture tempRenderTexture = null;
            if (overridePostProcessing || fixPostEffectsAlpha)
            {
                tempRenderTexture = RenderTexture.GetTemporary(_foregroundRenderTexture.width, _foregroundRenderTexture.height, 0, _foregroundRenderTexture.format);
#if UNITY_EDITOR
                tempRenderTexture.name = "LIV.TemporaryRenderTexture";
#endif
                _captureTextureCommandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, tempRenderTexture);
                _cameraInstance.AddCommandBuffer(_captureTextureEvent, _captureTextureCommandBuffer);

                _writeMaterial.SetInt(SDKShaders.LivColorMask, overridePostProcessing ? (int)ColorWriteMask.All : (int)ColorWriteMask.Alpha);
                _writeMaterial.mainTexture = tempRenderTexture;
                _applyTextureCommandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, _writeMaterial);
                _cameraInstance.AddCommandBuffer(_applyTextureEvent, _applyTextureCommandBuffer);
            }

            // Combine captured alpha with result alpha
            _combineAlphaMaterial.SetInt(SDKShaders.LivColorMask, (int)ColorWriteMask.Alpha);
            _combineAlphaMaterial.mainTexture = capturedAlphaRenderTexture;
            _combineAlphaCommandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, _combineAlphaMaterial);
            _cameraInstance.AddCommandBuffer(_clipPlaneCombineAlphaCameraEvent, _combineAlphaCommandBuffer);

            if (UseDeferredRendering) SDKUtils.ForceForwardRendering(CameraInstance, _clipPlaneMesh, _forceForwardRenderingMaterial);

            SDKShaders.StartRendering();
            SDKShaders.StartForegroundRendering();
            InvokePreRenderForeground();
            SendTextureToBridge(_foregroundRenderTexture, TextureID.ForegroundColorBufferID);
            if (debug) RenderDebugPreRender();
            _cameraInstance.Render();
            InvokePostRenderForeground();
            if (debug) RenderDebugPostRender(_foregroundRenderTexture);
            _cameraInstance.targetTexture = null;
            SDKShaders.StopForegroundRendering();
            SDKShaders.StopRendering();

            if (overridePostProcessing || fixPostEffectsAlpha)
            {
                _cameraInstance.RemoveCommandBuffer(_captureTextureEvent, _captureTextureCommandBuffer);
                _cameraInstance.RemoveCommandBuffer(_applyTextureEvent, _applyTextureCommandBuffer);

                _captureTextureCommandBuffer.Clear();
                _applyTextureCommandBuffer.Clear();

                RenderTexture.ReleaseTemporary(tempRenderTexture);
            }

            _cameraInstance.RemoveCommandBuffer(_clipPlaneCameraEvent, _clipPlaneCommandBuffer);
            _cameraInstance.RemoveCommandBuffer(_clipPlaneCombineAlphaCameraEvent, _combineAlphaCommandBuffer);

            RenderTexture.ReleaseTemporary(capturedAlphaRenderTexture);

            _clipPlaneCommandBuffer.Clear();
            _combineAlphaCommandBuffer.Clear();

            // Revert camera defaults
            _cameraInstance.clearFlags = capturedClearFlags;
            _cameraInstance.backgroundColor = capturedBgColor;
            RenderSettings.fogColor = capturedFogColor;

            SDKUtils.RestoreStandardAssets(ref behaviours, ref wasBehaviourEnabled);
        }

        // Renders a single camera in a single texture with occlusion only from opaque objects.
        // This is the most performant option for mixed reality.
        // It does not support any transparency in the foreground layer.
        private void RenderOptimized()
        {
            bool debugClipPlane = SDKUtils.FeatureEnabled(InputFrame.features, Features.DebugClipPlane);
            bool renderComplexClipPlane = SDKUtils.FeatureEnabled(InputFrame.features, Features.ComplexClipPlane);
            bool renderGroundClipPlane = SDKUtils.FeatureEnabled(InputFrame.features, Features.GroundClipPlane);
            bool debug = debugClipPlane;

            SDKUtils.SetCamera(_cameraInstance, _cameraInstance.transform, _inputFrame, LocalToWorldMatrix, SpectatorLayerMask);
            _cameraInstance.targetTexture = _optimizedRenderTexture;

            RenderTexture capturedAlphaRenderTexture = RenderTexture.GetTemporary(_optimizedRenderTexture.width, _optimizedRenderTexture.height, 0, _optimizedRenderTexture.format);
#if UNITY_EDITOR
            capturedAlphaRenderTexture.name = "LIV.CapturedAlphaRenderTexture";
#endif
            // Clear alpha channel
            _writeMaterial.SetInt(SDKShaders.LivColorMask, (int)ColorWriteMask.Alpha);
            _optimizedRenderingCommandBuffer.DrawMesh(_quadMesh, Matrix4x4.identity, _writeMaterial);

            // Render opaque pixels into alpha channel
            _writeOpaqueToAlphaMaterial.SetInt(SDKShaders.LivColorMask, (int)ColorWriteMask.Alpha);
            _optimizedRenderingCommandBuffer.DrawMesh(_clipPlaneMesh, Matrix4x4.identity, _writeOpaqueToAlphaMaterial, 0, 0);

            // Render clip plane
            Matrix4x4 clipPlaneTransform = LocalToWorldMatrix * (Matrix4x4)_inputFrame.clipPlane.transform;
            _optimizedRenderingCommandBuffer.DrawMesh(_clipPlaneMesh, clipPlaneTransform,
                GetClipPlaneMaterial(debugClipPlane, renderComplexClipPlane, ColorWriteMask.Alpha, ref _clipPlaneMaterialProperty), 0, 0, _clipPlaneMaterialProperty);

            // Render ground clip plane
            if (renderGroundClipPlane)
            {
                Matrix4x4 groundClipPlaneTransform = LocalToWorldMatrix * (Matrix4x4)_inputFrame.groundClipPlane.transform;
                _optimizedRenderingCommandBuffer.DrawMesh(_clipPlaneMesh, groundClipPlaneTransform,
                    GetClipPlaneMaterial(debugClipPlane, false, ColorWriteMask.Alpha, ref _groundPlaneMaterialProperty), 0, 0, _groundPlaneMaterialProperty);
            }

            _optimizedRenderingCommandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, capturedAlphaRenderTexture);

            _cameraInstance.AddCommandBuffer(_optimizedRenderingCameraEvent, _optimizedRenderingCommandBuffer);

            if (UseDeferredRendering) SDKUtils.ForceForwardRendering(CameraInstance, _clipPlaneMesh, _forceForwardRenderingMaterial);

            // TODO: this is just proprietary
            SDKShaders.StartRendering();
            SDKShaders.StartBackgroundRendering();
            InvokePreRenderBackground();
            SendTextureToBridge(_optimizedRenderTexture, TextureID.OptimizedColorBufferID);
            if (debug) RenderDebugPreRender();
            _cameraInstance.Render();

            // Recover alpha
            RenderBuffer activeColorBuffer = Graphics.activeColorBuffer;
            RenderBuffer activeDepthBuffer = Graphics.activeDepthBuffer;
            Graphics.Blit(capturedAlphaRenderTexture, _optimizedRenderTexture, _writeMaterial);
            Graphics.SetRenderTarget(activeColorBuffer, activeDepthBuffer);

            InvokePostRenderBackground();
            if (debug) RenderDebugPostRender(_optimizedRenderTexture);

            _cameraInstance.targetTexture = null;
            SDKShaders.StopBackgroundRendering();
            SDKShaders.StopRendering();

            _cameraInstance.RemoveCommandBuffer(_optimizedRenderingCameraEvent, _optimizedRenderingCommandBuffer);
            _optimizedRenderingCommandBuffer.Clear();

            RenderTexture.ReleaseTemporary(capturedAlphaRenderTexture);
        }

        private void CreateAssets()
        {
            bool cameraReferenceEnabled = CameraReference.enabled;
            if (cameraReferenceEnabled)
            {
                CameraReference.enabled = false;
            }
            bool cameraReferenceActive = CameraReference.gameObject.activeSelf;
            if (cameraReferenceActive)
            {
                CameraReference.gameObject.SetActive(false);
            }

            // TODO: Document change. Il2cpp mod will give "invalid cast" errors.
            // GameObject cloneGO = (GameObject)Object.Instantiate(cameraReference.gameObject, _liv.stage);
            // _cameraInstance = (Camera)cloneGO.GetComponent("Camera");
            GameObject cloneGo = Object.Instantiate(CameraReference.gameObject, _liv.Stage);
            _cameraInstance = cloneGo.GetComponent<Camera>();

            SDKUtils.CleanCameraBehaviours(_cameraInstance, _liv.ExcludeBehaviours);

            if (cameraReferenceActive != CameraReference.gameObject.activeSelf)
            {
                CameraReference.gameObject.SetActive(cameraReferenceActive);
            }
            if (cameraReferenceEnabled != CameraReference.enabled)
            {
                CameraReference.enabled = cameraReferenceEnabled;
            }

            _cameraInstance.name = "LIV Camera";
            if (_cameraInstance.tag == "MainCamera")
            {
                _cameraInstance.tag = "Untagged";
            }

            _cameraInstance.transform.localScale = Vector3.one;
            _cameraInstance.rect = new Rect(0, 0, 1, 1);
            _cameraInstance.depth = 0;
#if UNITY_5_4_OR_NEWER
            _cameraInstance.stereoTargetEye = StereoTargetEyeMask.None;
#endif
#if UNITY_5_6_OR_NEWER
            _cameraInstance.allowMSAA = false;
#endif
            _cameraInstance.enabled = false;
            _cameraInstance.gameObject.SetActive(true);

            _clipPlaneMesh = new Mesh();
            SDKUtils.CreateClipPlane(_clipPlaneMesh, 10, 10, true, 1000f);

            _quadMesh = new Mesh();
            SDKUtils.CreateQuad(_quadMesh);

            _boxMesh = new Mesh();
            SDKUtils.CreateBox(_boxMesh, Vector3.one);

            _clipPlaneSimpleMaterial = new Material(SDKShaders.GetShader(SDKShaders.LivClipPlaneSimpleShader));
            _clipPlaneSimpleDebugMaterial = new Material(SDKShaders.GetShader(SDKShaders.LivClipPlaneSimpleDebugShader));
            _clipPlaneComplexMaterial = new Material(SDKShaders.GetShader(SDKShaders.LivClipPlaneComplexShader));
            _clipPlaneComplexDebugMaterial = new Material(SDKShaders.GetShader(SDKShaders.LivClipPlaneComplexDebugShader));
            _writeOpaqueToAlphaMaterial = new Material(SDKShaders.GetShader(SDKShaders.LivWriteOpaqueToAlphaShader));
            _combineAlphaMaterial = new Material(SDKShaders.GetShader(SDKShaders.LivCombineAlphaShader));
            _writeMaterial = new Material(SDKShaders.GetShader(SDKShaders.LivWriteShader));
            _forceForwardRenderingMaterial = new Material(SDKShaders.GetShader(SDKShaders.LivForceForwardRenderingShader));

            _clipPlaneMaterialProperty = new MaterialPropertyBlock();
            _clipPlaneMaterialProperty.SetColor(SDKShaders.LivColor, SDKShaders.GreenColor);
            _groundPlaneMaterialProperty = new MaterialPropertyBlock();
            _groundPlaneMaterialProperty.SetColor(SDKShaders.LivColor, SDKShaders.BlueColor);
            _hmdMaterialProperty = new MaterialPropertyBlock();
            _hmdMaterialProperty.SetColor(SDKShaders.LivColor, SDKShaders.RedColor);

            _clipPlaneCommandBuffer = new CommandBuffer();
            _combineAlphaCommandBuffer = new CommandBuffer();
            _captureTextureCommandBuffer = new CommandBuffer();
            _applyTextureCommandBuffer = new CommandBuffer();
            _optimizedRenderingCommandBuffer = new CommandBuffer();

#if UNITY_EDITOR
            _quadMesh.name = "LIV.quad";
            _clipPlaneMesh.name = "LIV.clipPlane";
            _clipPlaneSimpleMaterial.name = "LIV.clipPlaneSimple";
            _clipPlaneSimpleDebugMaterial.name = "LIV.clipPlaneSimpleDebug";
            _clipPlaneComplexMaterial.name = "LIV.clipPlaneComplex";
            _clipPlaneComplexDebugMaterial.name = "LIV.clipPlaneComplexDebug";
            _writeOpaqueToAlphaMaterial.name = "LIV.writeOpaqueToAlpha";
            _combineAlphaMaterial.name = "LIV.combineAlpha";
            _writeMaterial.name = "LIV.write";
            _forceForwardRenderingMaterial.name = "LIV.forceForwardRendering";
            _clipPlaneCommandBuffer.name = "LIV.renderClipPlanes";
            _combineAlphaCommandBuffer.name = "LIV.foregroundCombineAlpha";
            _captureTextureCommandBuffer.name = "LIV.captureTexture";
            _applyTextureCommandBuffer.name = "LIV.applyTexture";
            _optimizedRenderingCommandBuffer.name = "LIV.optimizedRendering";
#endif
        }

        private void DestroyAssets()
        {
            if (_cameraInstance != null)
            {
                Object.Destroy(_cameraInstance.gameObject);
                _cameraInstance = null;
            }

            SDKUtils.DestroyObject<Mesh>(ref _quadMesh);
            SDKUtils.DestroyObject<Mesh>(ref _clipPlaneMesh);
            SDKUtils.DestroyObject<Mesh>(ref _boxMesh);

            SDKUtils.DestroyObject<Material>(ref _clipPlaneSimpleMaterial);
            SDKUtils.DestroyObject<Material>(ref _clipPlaneSimpleDebugMaterial);
            SDKUtils.DestroyObject<Material>(ref _clipPlaneComplexMaterial);
            SDKUtils.DestroyObject<Material>(ref _clipPlaneComplexDebugMaterial);
            SDKUtils.DestroyObject<Material>(ref _writeOpaqueToAlphaMaterial);
            SDKUtils.DestroyObject<Material>(ref _combineAlphaMaterial);
            SDKUtils.DestroyObject<Material>(ref _writeMaterial);
            SDKUtils.DestroyObject<Material>(ref _forceForwardRenderingMaterial);

            SDKUtils.DisposeObject<CommandBuffer>(ref _clipPlaneCommandBuffer);
            SDKUtils.DisposeObject<CommandBuffer>(ref _combineAlphaCommandBuffer);
            SDKUtils.DisposeObject<CommandBuffer>(ref _captureTextureCommandBuffer);
            SDKUtils.DisposeObject<CommandBuffer>(ref _applyTextureCommandBuffer);
            SDKUtils.DisposeObject<CommandBuffer>(ref _optimizedRenderingCommandBuffer);

            SDKUtils.DestroyTexture(ref _backgroundRenderTexture);
            SDKUtils.DestroyTexture(ref _foregroundRenderTexture);
            SDKUtils.DestroyTexture(ref _optimizedRenderTexture);
            SDKUtils.DestroyTexture(ref _complexClipPlaneRenderTexture);
        }

        public void Dispose()
        {
            ReleaseBridgePoseControl();
            DestroyAssets();
            DisposeDebug();
        }
    }
}
#endif