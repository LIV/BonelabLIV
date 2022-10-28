using System;
using MelonLoader;
using UnityEngine;

namespace LIV.SDK.Unity
{
    public static class SDKShaders
    {

        public static readonly Color GreenColor = new Color(0f, 1f, 0f, 0.5f);
        public static readonly Color BlueColor = new Color(0f, 0f, 1f, 0.5f);
        public static readonly Color RedColor = new Color(1f, 0f, 0f, 0.5f);

        public static readonly int LivColor = Shader.PropertyToID("_LivColor");
        public static readonly int LivColorMask = Shader.PropertyToID("_LivColorMask");
        public static readonly int LivTessellationProperty = Shader.PropertyToID("_LivTessellation");
        public static readonly int LivClipPlaneHeightMapProperty = Shader.PropertyToID("_LivClipPlaneHeightMap");

        public const string LivMrForegroundKeyword = "LIV_MR_FOREGROUND";
        public const string LivMrBackgroundKeyword = "LIV_MR_BACKGROUND";
        public const string LivMrKeyword = "LIV_MR";

        public const string LivClipPlaneSimpleShader = "Hidden/LIV_ClipPlaneSimple";
        public const string LivClipPlaneSimpleDebugShader = "Hidden/LIV_ClipPlaneSimpleDebug";
        public const string LivClipPlaneComplexShader = "Hidden/LIV_ClipPlaneComplex";
        public const string LivClipPlaneComplexDebugShader = "Hidden/LIV_ClipPlaneComplexDebug";
        public const string LivWriteOpaqueToAlphaShader = "Hidden/LIV_WriteOpaqueToAlpha";
        public const string LivCombineAlphaShader = "Hidden/LIV_CombineAlpha";
        public const string LivWriteShader = "Hidden/LIV_Write";
        public const string LivForceForwardRenderingShader = "Hidden/LIV_ForceForwardRendering";

        private class ShaderCache
        {
            public ShaderCache(AssetBundle bundle)
            {
                try
                {
                    ClipPlaneSimple = LoadShader(bundle, "LIV_ClipPlaneSimple");
                    ClipPlaneSimpleDebug = LoadShader(bundle, "LIV_ClipPlaneSimpleDebug");
                    ClipPlaneComplex = LoadShader(bundle, "LIV_ClipPlaneComplex");
                    ClipPlaneComplexDebug = LoadShader(bundle, "LIV_ClipPlaneComplexDebug");
                    WriteOpaqueToAlpha = LoadShader(bundle, "LIV_WriteOpaqueToAlpha");
                    CombineAlpha = LoadShader(bundle, "LIV_CombineAlpha");
                    Write = LoadShader(bundle, "LIV_Write");
                    ForceForwardRendering = LoadShader(bundle, "LIV_ForceForwardRendering");

                    State = ClipPlaneSimple != null &&
                        ClipPlaneSimpleDebug != null &&
                        ClipPlaneComplex != null &&
                        ClipPlaneComplexDebug != null &&
                        WriteOpaqueToAlpha != null &&
                        CombineAlpha != null &&
                        Write != null &&
                        ForceForwardRendering != null;

                    if (!State)
                        MelonLogger.Error($"Failed to retreive at least one of the LIV shaders from {bundle.name}");
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Failed to initialize the ShaderCache from asset bundle {bundle.name} : {e}");
                    State = false;
                }
            }

            // TODO: Document change. Il2cpp mods will unload shader assets unless we change the hideFlags.
            private static Shader LoadShader(AssetBundle bundle, string name)
            {
                var shader = bundle.LoadAsset(name);
                shader.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                return shader.Cast<Shader>();
            }

            //Note, if we come up with *one more shader* to put in there, make a damn Dictonary
            public readonly Shader ClipPlaneSimple = null;
            public readonly Shader ClipPlaneSimpleDebug = null;
            public readonly Shader ClipPlaneComplex = null;
            public readonly Shader ClipPlaneComplexDebug = null;
            public readonly Shader WriteOpaqueToAlpha = null;
            public readonly Shader CombineAlpha = null;
            public readonly Shader Write = null;
            public readonly Shader ForceForwardRendering = null;
            public readonly bool State = false;
        }

        private static ShaderCache _shaderCache = null;


        public static bool LoadFromAssetBundle(AssetBundle bundle)
        {
            _shaderCache = new ShaderCache(bundle);
            return _shaderCache.State;
        }

        public static Shader GetShader(string name)
        {
            if (_shaderCache != null && _shaderCache.State)
                return GetShaderFromCache(name);
            else
                return Shader.Find(name); //Attempt to find it in the player build instead

            return null;
        }

        private static Shader GetShaderFromCache(string name)
        {
            switch (name)
            {
                default: return null;
                case LivClipPlaneSimpleShader:
                    return _shaderCache.ClipPlaneSimple;
                case LivClipPlaneSimpleDebugShader:
                    return _shaderCache.ClipPlaneSimpleDebug;
                case LivClipPlaneComplexShader:
                    return _shaderCache.ClipPlaneComplex;
                case LivClipPlaneComplexDebugShader:
                    return _shaderCache.ClipPlaneComplexDebug;
                case LivWriteOpaqueToAlphaShader:
                    return _shaderCache.WriteOpaqueToAlpha;
                case LivCombineAlphaShader:
                    return _shaderCache.CombineAlpha;
                case LivWriteShader:
                    return _shaderCache.Write;
                case LivForceForwardRenderingShader:
                    return _shaderCache.ForceForwardRendering;
            }
        }


        public static void StartRendering()
        {
            Shader.EnableKeyword(LivMrKeyword);
        }

        public static void StopRendering()
        {
            Shader.DisableKeyword(LivMrKeyword);
        }

        public static void StartForegroundRendering()
        {
            Shader.EnableKeyword(LivMrForegroundKeyword);
        }

        public static void StopForegroundRendering()
        {
            Shader.DisableKeyword(LivMrForegroundKeyword);
        }

        public static void StartBackgroundRendering()
        {
            Shader.EnableKeyword(LivMrBackgroundKeyword);
        }

        public static void StopBackgroundRendering()
        {
            Shader.DisableKeyword(LivMrBackgroundKeyword);
        }
    }
}
