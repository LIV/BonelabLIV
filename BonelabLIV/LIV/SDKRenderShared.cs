﻿using UnityEngine;
using UnityEngine.Rendering;

namespace LIV.SDK.Unity
{
    public partial class SDKRender : System.IDisposable
    {
        private Liv _liv = null;
        public Liv Liv {
            get {
                return _liv;
            }
        }

        // quad
        private Mesh _quadMesh = null;
        // Tessellated quad
        private Mesh _clipPlaneMesh = null;
        // box
        private Mesh _boxMesh = null;
        // debug font
        private SDKFont _sdkFont = null;

        private MaterialPropertyBlock _clipPlaneMaterialProperty;
        private MaterialPropertyBlock _groundPlaneMaterialProperty;
        private MaterialPropertyBlock _hmdMaterialProperty;

        private SDKOutputFrame _outputFrame = SDKOutputFrame.Empty;
        public SDKOutputFrame OutputFrame {
            get {
                return _outputFrame;
            }
        }

        private SDKInputFrame _inputFrame = SDKInputFrame.Empty;
        public SDKInputFrame InputFrame {
            get {
                return _inputFrame;
            }
        }

        private SDKResolution _resolution = SDKResolution.Zero;
        public SDKResolution Resolution {
            get {
                return _resolution;
            }
        }

        private Camera _cameraInstance = null;
        public Camera CameraInstance {
            get {
                return _cameraInstance;
            }
        }

        public Camera CameraReference {
            get {
                return _liv.MrCameraPrefab == null ? _liv.HmdCamera : _liv.MrCameraPrefab;
            }
        }

        public Camera HmdCamera {
            get {
                return _liv.HmdCamera;
            }
        }

        public Transform Stage {
            get {
                return _liv.Stage;
            }
        }

        public Transform StageTransform {
            get {
                return _liv.StageTransform;
            }
        }

        public Matrix4x4 StageLocalToWorldMatrix {
            get {
                return _liv.Stage == null ? Matrix4x4.identity : _liv.Stage.localToWorldMatrix;
            }
        }

        public Matrix4x4 LocalToWorldMatrix {
            get {
                return _liv.StageTransform == null ? StageLocalToWorldMatrix : _liv.StageTransform.localToWorldMatrix;
            }
        }

        public int SpectatorLayerMask {
            get {
                return _liv.SpectatorLayerMask;
            }
        }

        public bool DisableStandardAssets {
            get {
                return _liv.DisableStandardAssets;
            }
        }

        private SDKPose _requestedPose = SDKPose.Empty;
        private int _requestedPoseFrameIndex = 0;

        /// <summary>
        /// Detect if the game can actually change the pose during this frame.
        /// </summary>
        /// <remarks>
        /// <para>Because other applications can take over the pose, the game has to know if it can take over the pose or not.</para>        
        /// </remarks>
        /// <example>
        /// <code>
        /// public class CanControlCameraPose : MonoBehaviour
        /// {
        ///     [SerializeField] LIV.SDK.Unity.LIV _liv;
        ///
        ///     private void Update()
        ///     {
        ///         if(_liv.isActive) 
        ///         {
        ///             Debug.Log(_liv.render.canSetPose);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool CanSetPose {
            get {
                if (_inputFrame.frameid == 0) return false;
                return _inputFrame.priority.pose <= (sbyte)Priority.Game;
            }
        }

        /// <summary>
        /// Control camera pose by calling this method each frame. The pose is released when you stop calling it.
        /// </summary>
        /// <remarks>
        /// <para>By default the pose is set in worldspace, turn on local space for using the stage relative space instead.</para>        
        /// </remarks>
        /// <example>
        /// <code>
        /// public class ControlCameraPose : MonoBehaviour
        /// {
        ///     [SerializeField] LIV.SDK.Unity.LIV _liv;
        ///     [SerializeField] float _fov = 60f;
        ///
        ///     private void Update()
        ///     {
        ///         if(_liv.isActive) 
        ///         {
        ///             _liv.render.SetPose(transform.position, transform.rotation, _fov);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool SetPose(Vector3 position, Quaternion rotation, float verticalFieldOfView = 60f, bool useLocalSpace = false)
        {
            if (_inputFrame.frameid == 0) return false;
            SDKPose inputPose = _inputFrame.pose;
            float aspect = 1f;
            if (_resolution.height > 0)
            {
                aspect = (float)_resolution.width / (float)_resolution.height;
            }

            if (!useLocalSpace)
            {
                Matrix4x4 worldToLocal = Matrix4x4.identity;
                Transform localTransform = StageTransform == null ? Stage : StageTransform;
                if (localTransform != null) worldToLocal = localTransform.worldToLocalMatrix;
                position = worldToLocal.MultiplyPoint(position);
                rotation = SDKUtils.RotateQuaternionByMatrix(worldToLocal, rotation);
            }

            _requestedPose = new SDKPose()
            {
                localPosition = position,
                localRotation = rotation,
                verticalFieldOfView = verticalFieldOfView,
                projectionMatrix = Matrix4x4.Perspective(verticalFieldOfView, aspect, inputPose.nearClipPlane, inputPose.farClipPlane)
            };

            _requestedPoseFrameIndex = Time.frameCount;
            return _inputFrame.priority.pose <= (sbyte)Priority.Game;
        }

        /// <summary>
        /// Set the game ground plane.
        /// </summary>
        /// <remarks>
        /// <para>If you wisth to use local space coordinates use local space instead. 
        /// The local space has to be relative to stage or stage transform if set.
        /// </para>
        /// </remarks>        
        public void SetGroundPlane(float distance, Vector3 normal, bool useLocalSpace = false)
        {
            float outputDistance = distance;
            Vector3 outputNormal = normal;

            if (!useLocalSpace)
            {
                Transform localTransform = StageTransform == null ? Stage : StageTransform;
                Matrix4x4 worldToLocal = localTransform.worldToLocalMatrix;
                Vector3 localPosition = worldToLocal.MultiplyPoint(normal * distance);
                outputNormal = worldToLocal.MultiplyVector(normal);
                outputDistance = -Vector3.Dot(normal, localPosition);
            }

            SDKBridge.SetGroundPlane(new SDKPlane() { distance = outputDistance, normal = outputNormal });
        }

        /// <summary>
        /// Set the game ground plane.
        /// </summary>
        /// <remarks>
        /// <para>If you wisth to use local space coordinates use local space instead. 
        /// The local space has to be relative to stage or stage transform if set.
        /// </para>
        /// </remarks>        
        public void SetGroundPlane(Plane plane, bool useLocalSpace = false)
        {
            SetGroundPlane(plane.distance, plane.normal, useLocalSpace);
        }

        /// <summary>
        /// Set the game ground plane.
        /// </summary>
        /// <remarks>
        /// <para>The transform up vector defines the normal of the plane and the position defines the distance.
        /// By default, the transform uses world space coordinates. If you wisth to use local space coordinates
        /// use local space instead. The local space has to be relative to stage or stage transform if set.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class SetGround : MonoBehaviour 
        /// {
        ///     [SerializeField] LIV.SDK.Unity.LIV _liv = null;
        /// 
        ///     void Update () 
        ///     {
        ///         if(_liv.isActive)
        ///         {        
        ///             _liv.render.SetGroundPlane(transform);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void SetGroundPlane(Transform transform, bool useLocalSpace = false)
        {
            if (transform == null) return;
            Quaternion rotation = useLocalSpace ? transform.localRotation : transform.rotation;
            Vector3 position = useLocalSpace ? transform.localPosition : transform.position;
            Vector3 normal = rotation * Vector3.up;
            SetGroundPlane(-Vector3.Dot(normal, position), normal, useLocalSpace);
        }

        private void ReleaseBridgePoseControl()
        {
            _inputFrame.ReleaseControl();
            SDKBridge.UpdateInputFrame(ref _inputFrame);
        }

        private void UpdateBridgeResolution()
        {
            SDKBridge.GetResolution(ref _resolution);
        }

        private void UpdateBridgeInputFrame()
        {
            if (_requestedPoseFrameIndex == Time.frameCount)
            {
                _inputFrame.ObtainControl();
                _inputFrame.pose = _requestedPose;
                _requestedPose = SDKPose.Empty;
            }
            else
            {
                _inputFrame.ReleaseControl();
            }

            if (_cameraInstance != null)
            {
                // Near and far is always driven by game
                _inputFrame.pose.nearClipPlane = _cameraInstance.nearClipPlane;
                _inputFrame.pose.farClipPlane = _cameraInstance.farClipPlane;
            }

            SDKBridge.UpdateInputFrame(ref _inputFrame);
        }

        private void InvokePreRender()
        {
            if (_liv.OnPreRender != null) _liv.OnPreRender(this);
        }

        private void IvokePostRender()
        {
            if (_liv.OnPostRender != null) _liv.OnPostRender(this);
        }

        private void InvokePreRenderBackground()
        {
            if (_liv.OnPreRenderBackground != null) _liv.OnPreRenderBackground(this);
        }

        private void InvokePostRenderBackground()
        {
            if (_liv.OnPostRenderBackground != null) _liv.OnPostRenderBackground(this);
        }

        private void InvokePreRenderForeground()
        {
            if (_liv.OnPreRenderForeground != null) _liv.OnPreRenderForeground(this);
        }

        private void InvokePostRenderForeground()
        {
            if (_liv.OnPostRenderForeground != null) _liv.OnPostRenderForeground(this);
        }

        private void CreateBackgroundTexture()
        {
            if (SDKUtils.CreateTexture(ref _backgroundRenderTexture, _resolution.width, _resolution.height, 24, RenderTextureFormat.ARGB32))
            {
#if UNITY_EDITOR
                _backgroundRenderTexture.name = "LIV.BackgroundRenderTexture";
#endif               
            }
            else
            {
                Debug.LogError("LIV: Unable to create background texture!");
            }
        }

        private void CreateForegroundTexture()
        {
            if (SDKUtils.CreateTexture(ref _foregroundRenderTexture, _resolution.width, _resolution.height, 24, RenderTextureFormat.ARGB32))
            {
#if UNITY_EDITOR
                _foregroundRenderTexture.name = "LIV.ForegroundRenderTexture";
#endif
            }
            else
            {
                Debug.LogError("LIV: Unable to create foreground texture!");
            }
        }

        private void CreateOptimizedTexture()
        {
            if (SDKUtils.CreateTexture(ref _optimizedRenderTexture, _resolution.width, _resolution.height, 24, RenderTextureFormat.ARGB32))
            {
#if UNITY_EDITOR
                _optimizedRenderTexture.name = "LIV.OptimizedRenderTexture";
#endif               
            }
            else
            {
                Debug.LogError("LIV: Unable to create optimized texture!");
            }
        }

        private void CreateComplexClipPlaneTexture()
        {
            if (SDKUtils.CreateTexture(ref _complexClipPlaneRenderTexture, _inputFrame.clipPlane.width, _inputFrame.clipPlane.height, 0, RenderTextureFormat.ARGB32))
            {
#if UNITY_EDITOR
                _complexClipPlaneRenderTexture.name = "LIV.ComplexClipPlaneRenderTexture";
#endif
            }
            else
            {
                Debug.LogError("LIV: Unable to create complex clip plane texture!");
            }
        }

        private void UpdateTextures()
        {
            if (SDKUtils.FeatureEnabled(InputFrame.features, Features.BackgroundRender))
            {
                if (
                    _backgroundRenderTexture == null ||
                    _backgroundRenderTexture.width != _resolution.width ||
                    _backgroundRenderTexture.height != _resolution.height
                )
                {
                    CreateBackgroundTexture();
                }
            }
            else
            {
                SDKUtils.DestroyTexture(ref _backgroundRenderTexture);
            }

            if (SDKUtils.FeatureEnabled(InputFrame.features, Features.ForegroundRender))
            {
                if (
                    _foregroundRenderTexture == null ||
                    _foregroundRenderTexture.width != _resolution.width ||
                    _foregroundRenderTexture.height != _resolution.height
                )
                {
                    CreateForegroundTexture();
                }
            }
            else
            {
                SDKUtils.DestroyTexture(ref _foregroundRenderTexture);
            }

            if (SDKUtils.FeatureEnabled(InputFrame.features, Features.OptimizedRender))
            {
                if (
                    _optimizedRenderTexture == null ||
                    _optimizedRenderTexture.width != _resolution.width ||
                    _optimizedRenderTexture.height != _resolution.height
                )
                {
                    CreateOptimizedTexture();
                }
            }
            else
            {
                SDKUtils.DestroyTexture(ref _optimizedRenderTexture);
            }

            if (SDKUtils.FeatureEnabled(InputFrame.features, Features.ComplexClipPlane))
            {
                if (
                    _complexClipPlaneRenderTexture == null ||
                    _complexClipPlaneRenderTexture.width != _inputFrame.clipPlane.width ||
                    _complexClipPlaneRenderTexture.height != _inputFrame.clipPlane.height
                )
                {
                    CreateComplexClipPlaneTexture();
                }
            }
            else
            {
                SDKUtils.DestroyTexture(ref _complexClipPlaneRenderTexture);
            }
        }

        void SendTextureToBridge(RenderTexture texture, TextureID id)
        {
            SDKBridge.AddTexture(new SDKTexture()
            {
                id = id,
                texturePtr = texture.GetNativeTexturePtr(),
                SharedHandle = System.IntPtr.Zero,
                device = SDKUtils.GetDevice(),
                dummy = 0,
                type = TextureType.ColorBuffer,
                format = TextureFormat.Argb32,
                colorSpace = SDKUtils.GetColorSpace(texture),
                width = texture.width,
                height = texture.height
            });
        }

        Material GetClipPlaneMaterial(bool debugClipPlane, bool complexClipPlane, ColorWriteMask colorWriteMask, ref MaterialPropertyBlock materialPropertyBlock)
        {
            Material output;

            if (complexClipPlane)
            {
                output = debugClipPlane ? _clipPlaneComplexDebugMaterial : _clipPlaneComplexMaterial;
                materialPropertyBlock.SetTexture(SDKShaders.LivClipPlaneHeightMapProperty, _complexClipPlaneRenderTexture);
                materialPropertyBlock.SetFloat(SDKShaders.LivTessellationProperty, _inputFrame.clipPlane.tesselation);
            }
            else
            {
                output = debugClipPlane ? _clipPlaneSimpleDebugMaterial : _clipPlaneSimpleMaterial;
            }

            output.SetInt(SDKShaders.LivColorMask, (int)colorWriteMask);
            return output;
        }

        void RenderFrameStamps(RenderTexture renderTexture)
        {
            float aspect = (float)renderTexture.width / (float)renderTexture.height;
            int height = 50;
            int width = (int)(height * aspect);
            if (_sdkFont == null) _sdkFont = new SDKFont(width, height);
            _sdkFont.Resize(width, height);
            _sdkFont.Clear();
            string frameCount = Time.frameCount.ToString();

            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(Time.realtimeSinceStartup);
            string timeStamp = string.Format($"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}:{timeSpan.Milliseconds:000}");
            _sdkFont.SetText(0, 0, frameCount);
            _sdkFont.SetText(width - frameCount.Length, 0, frameCount);
            _sdkFont.SetText(0, height - 1, $"{frameCount} {timeStamp}");
            _sdkFont.SetText(width - frameCount.Length, height - 1, frameCount);
            _sdkFont.Apply();

            Graphics.Blit(null, renderTexture, _sdkFont.FontMaterial);
        }

        void RenderHmd()
        {
            Graphics.DrawMesh(_boxMesh,
                _liv.HmdCamera.transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * 0.1f),
                _clipPlaneSimpleDebugMaterial,
                0,
                _cameraInstance,
                0,
                _hmdMaterialProperty,
                false,
                false,
                false);
        }

        void RenderDebugPreRender()
        {
            RenderHmd();
        }

        void RenderDebugPostRender(RenderTexture renderTexture)
        {
            RenderBuffer activeColorBuffer = Graphics.activeColorBuffer;
            RenderBuffer activeDepthBuffer = Graphics.activeDepthBuffer;
            RenderFrameStamps(renderTexture);
            Graphics.SetRenderTarget(activeColorBuffer, activeDepthBuffer);
        }

        protected void DisposeDebug()
        {
            if (_sdkFont != null)
            {
                _sdkFont.Dispose();
                _sdkFont = null;
            }
        }
    }
}