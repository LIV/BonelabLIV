using System;
using System.Collections;
using MelonLoader;
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#endif

namespace LIV.SDK.Unity
{
	[Flags]
	public enum InvalidationFlags : uint
	{
		None = 0,
		HmdCamera = 1,
		Stage = 2,
		MrCameraPrefab = 4,
		ExcludeBehaviours = 8
	}

    /// <summary>
    ///   The LIV SDK provides a spectator view of your application.
    /// </summary>
    /// <remarks>
    ///   <para>It contextualizes what the user feels & experiences by capturing their body directly inside your world!</para>
    ///   <para>Thanks to our software, creators can film inside your app and have full control over the camera.</para>
    ///   <para>With the power of out-of-engine compositing, a creator can express themselves freely without limits;</para>
    ///   <para>as a real person or an avatar!</para>
    /// </remarks>
    /// <example>
    ///   <code>
    /// public class StartFromScriptExample : MonoBehaviour
    /// {
    ///     [SerializeField] Camera _hmdCamera;
    ///     [SerializeField] Transform _stage;
    ///     [SerializeField] Transform _stageTransform;
    ///     [SerializeField] Camera _mrCameraPrefab;
    ///     LIV.SDK.Unity.LIV _liv;
    ///     private void OnEnable()
    ///     {
    ///         _liv = gameObject.AddComponent<LIV.SDK.Unity.LIV>
    ///       ();
    ///       _liv.HMDCamera = _hmdCamera;
    ///       _liv.stage = _stage;
    ///       _liv.stageTransform = _stageTransform;
    ///       _liv.MRCameraPrefab = _mrCameraPrefab;
    ///       }
    ///       private void OnDisable()
    ///       {
    ///       if (_liv == null) return;
    ///       Destroy(_liv);
    ///       _liv = null;
    ///       }
    ///       }
    /// </code>
    /// </example>
#if UNITY_EDITOR
    [HelpURL("https://liv.tv/sdk-unity-docs")]
    [AddComponentMenu("LIV/LIV")]
#endif
	public class Liv : MonoBehaviour
	{
		public static Liv Instance;

        /// <summary>
        ///   triggered when the LIV SDK is activated by the LIV App and enabled by the game.
        /// </summary>
        public Action OnActivate = null;

        /// <summary>
        ///   triggered before the Mixed Reality camera is about to render.
        /// </summary>
        public Action<SDKRender> OnPreRender = null;

        /// <summary>
        ///   triggered before the LIV SDK starts rendering background image.
        /// </summary>
        public Action<SDKRender> OnPreRenderBackground = null;

        /// <summary>
        ///   triggered after the LIV SDK starts rendering background image.
        /// </summary>
        public Action<SDKRender> OnPostRenderBackground = null;

        /// <summary>
        ///   triggered before the LIV SDK starts rendering the foreground image.
        /// </summary>
        public Action<SDKRender> OnPreRenderForeground = null;

        /// <summary>
        ///   triggered after the LIV SDK starts rendering the foreground image.
        /// </summary>
        public Action<SDKRender> OnPostRenderForeground = null;

        /// <summary>
        ///   triggered after the Mixed Reality camera has finished rendering.
        /// </summary>
        public Action<SDKRender> OnPostRender = null;

        /// <summary>
        ///   triggered when the LIV SDK is deactivated by the LIV App or disabled by the game.
        /// </summary>
        public Action OnDeactivate = null;

#if UNITY_EDITOR
        [Tooltip("This is the topmost transform of your VR rig.")]
        [FormerlySerializedAs("TrackedSpaceOrigin")]
        [SerializeField]
#endif
		private Transform _stage;

        /// <summary>
        ///   This is the topmost transform of your VR rig.
        /// </summary>
        /// <remarks>
        ///   <para>When implementing VR locomotion(teleporting, joystick, etc),</para>
        ///   <para>this is the GameObject that you should move around your scene.</para>
        ///   <para>It represents the centre of the user’s playspace.</para>
        /// </remarks>
        public Transform Stage
		{
			get => _stage == null ? transform.parent : _stage;
			set
			{
				if (value == null)
				{
					Debug.LogWarning("LIV: Stage cannot be null!");
				}

				if (_stage != value)
				{
					_stageCandidate = value;
					_invalidate = (InvalidationFlags) SDKUtils.SetFlag((uint) _invalidate, (uint) InvalidationFlags.Stage, true);
				}
			}
		}

		[Obsolete("Use stage instead")]
		public Transform TrackedSpaceOrigin
		{
			get => Stage;
			set => Stage = value;
		}

		public Matrix4x4 StageLocalToWorldMatrix => Stage != null ? Stage.localToWorldMatrix : Matrix4x4.identity;

		public Matrix4x4 StageWorldToLocalMatrix => Stage != null ? Stage.worldToLocalMatrix : Matrix4x4.identity;

#if UNITY_EDITOR
        [Tooltip("This transform is an additional wrapper to the user’s playspace.")]
        [FormerlySerializedAs("StageTransform")]
        [SerializeField]
#endif
        /// <summary>
        ///   This transform is an additional wrapper to the user’s playspace.
        /// </summary>
        /// <remarks>
        ///   <para>It allows for user-controlled transformations for special camera effects & transitions.</para>
        ///   <para>If a creator is using a static camera, this transformation can give the illusion of camera movement.</para>
        /// </remarks>
        public Transform StageTransform { get; set; }

#if UNITY_EDITOR
        [Tooltip("This is the camera responsible for rendering the user’s HMD.")]
        [FormerlySerializedAs("HMDCamera")]
        [SerializeField]
#endif
		private Camera _hmdCamera;

        /// <summary>
        ///   This is the camera responsible for rendering the user’s HMD.
        /// </summary>
        /// <remarks>
        ///   <para>The LIV SDK, by default clones this object to match your application’s rendering setup.</para>
        ///   <para>You can use your own camera prefab should you want to!</para>
        /// </remarks>
        public Camera HmdCamera
		{
			get => _hmdCamera;
			set
			{
				if (value == null)
				{
					Debug.LogWarning("LIV: HMD Camera cannot be null!");
				}

				if (_hmdCamera != value)
				{
					_hmdCameraCandidate = value;
					_invalidate = (InvalidationFlags) SDKUtils.SetFlag((uint) _invalidate, (uint) InvalidationFlags.HmdCamera, true);
				}
			}
		}

#if UNITY_EDITOR
        [Tooltip("Camera prefab for customized rendering.")]
        [FormerlySerializedAs("MRCameraPrefab")]
        [SerializeField]
#endif
		private Camera _mrCameraPrefab;

        /// <summary>
        ///   Camera prefab for customized rendering.
        /// </summary>
        /// <remarks>
        ///   <para>By default, LIV uses the HMD camera as a reference for the Mixed Reality camera.</para>
        ///   <para>It is cloned and set up as a Mixed Reality camera.This approach works for most apps.</para>
        ///   <para>However, some games can experience issues because of custom MonoBehaviours attached to this camera.</para>
        ///   <para>You can use a custom camera prefab for those cases.</para>
        /// </remarks>
        public Camera MrCameraPrefab
		{
			get => _mrCameraPrefab;
			set
			{
				if (_mrCameraPrefab != value)
				{
					_mrCameraPrefabCandidate = value;
					_invalidate = (InvalidationFlags) SDKUtils.SetFlag((uint) _invalidate, (uint) InvalidationFlags.MrCameraPrefab, true);
				}
			}
		}

#if UNITY_EDITOR
        [Tooltip("This option disables all standard Unity assets for the Mixed Reality rendering.")]
        [FormerlySerializedAs("DisableStandardAssets")]
        [SerializeField]
#endif
        /// <summary>
        ///   This option disables all standard Unity assets for the Mixed Reality rendering.
        /// </summary>
        /// <remarks>
        ///   <para>Unity’s standard assets can interfere with the alpha channel that LIV needs to composite MR correctly.</para>
        /// </remarks>
        public bool DisableStandardAssets { get; set; }

#if UNITY_EDITOR
        [Tooltip("The layer mask defines exactly which object layers should be rendered in MR.")]
        [FormerlySerializedAs("SpectatorLayerMask")]
        [SerializeField]
#endif
        /// <summary>
        ///   The layer mask defines exactly which object layers should be rendered in MR.
        /// </summary>
        /// <remarks>
        ///   <para>You should use this to hide any in-game avatar that you’re using.</para>
        ///   <para>LIV is meant to include the user’s body for you!</para>
        ///   <para>Certain HMD-based effects should be disabled here too.</para>
        ///   <para>Also, this can be used to render special effects or additional UI only to the MR camera.</para>
        ///   <para>Useful for showing the player’s health, or current score!</para>
        /// </remarks>
        public LayerMask SpectatorLayerMask { get; set; } = ~0;

#if UNITY_EDITOR
        [Tooltip("This is for removing unwanted scripts from the cloned MR camera.")]
        [FormerlySerializedAs("ExcludeBehaviours")]
        [SerializeField]
#endif
		private string[] _excludeBehaviours =
		{
			"AudioListener",
			"Collider",
			"SteamVR_Camera",
			"SteamVR_Fade",
			"SteamVR_ExternalCamera"
		};

        /// <summary>
        ///   This is for removing unwanted scripts from the cloned MR camera.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     By default, we remove the AudioListener, Colliders and SteamVR scripts, as these are not necessary for
        ///     rendering MR!
        ///   </para>
        ///   <para>The excluded string must match the name of the MonoBehaviour.</para>
        /// </remarks>
        public string[] ExcludeBehaviours
		{
			get => _excludeBehaviours;
			set
			{
				if (_excludeBehaviours != value)
				{
					_excludeBehavioursCandidate = value;
					_invalidate = (InvalidationFlags) SDKUtils.SetFlag((uint) _invalidate, (uint) InvalidationFlags.ExcludeBehaviours, true);
				}
			}
		}


        /// <summary>
        ///   Recovers corrupted alpha channel when using post-effects.
        /// </summary>
        public bool FixPostEffectsAlpha { get; set; }

        /// <summary>
        ///   Is the curret LIV SDK setup valid.
        /// </summary>
        public bool IsValid
		{
			get
			{
				if (_invalidate != InvalidationFlags.None) return false;

				if (_hmdCamera == null)
				{
					Debug.LogError("LIV: HMD Camera is a required parameter!");
					return false;
				}

				if (_stage == null)
				{
					Debug.LogWarning("LIV: Tracked space origin should be assigned!");
				}

				if (SpectatorLayerMask == 0)
				{
					Debug.LogWarning("LIV: The spectator layer mask is set to not show anything. Is this correct?");
				}

				return true;
			}
		}

        /// <summary>
        ///   Is the LIV SDK currently active.
        /// </summary>
        public bool IsActive { get; private set; }

		private bool IsReady => IsValid && _enabled && SDKBridge.IsActive;

		public SDKRender Render { get; private set; }

        /// <summary>
        ///   Script responsible for the MR rendering.
        /// </summary>
        // private SDKRender render { get { return _render; } }
		private bool _wasReady;

		private InvalidationFlags _invalidate = InvalidationFlags.None;
		private Transform _stageCandidate;
		private Camera _hmdCameraCandidate;
		private Camera _mrCameraPrefabCandidate;
		private string[] _excludeBehavioursCandidate;

		private bool _enabled;
		private object _waitForEndOfFrameCoroutine;

		// TODO: Document change. Constructor required for IL2cpp mods.
		public Liv(IntPtr ptr) : base(ptr)
		{
		}

		private void Awake()
		{
			Instance = this;
		}

		private void OnEnable()
		{
			_enabled = true;
			UpdateSDKReady();
		}

		private void Update()
		{
			UpdateSDKReady();
			Invalidate();
		}

		private void OnDisable()
		{
			_enabled = false;
			UpdateSDKReady();
		}

		private IEnumerator WaitForUnityEndOfFrame()
		{
			while (Application.isPlaying && enabled)
			{
				yield return new WaitForEndOfFrame();
				if (IsActive)
				{
					Render.Render();
				}
			}
		}

		private void UpdateSDKReady()
		{
			var ready = IsReady;
			if (ready != _wasReady)
			{
				OnSDKReadyChanged(ready);
				_wasReady = ready;
			}
		}

		private void OnSDKReadyChanged(bool value)
		{
			if (value)
			{
				OnSDKActivate();
			}
			else
			{
				OnSDKDeactivate();
			}
		}

		private void OnSDKActivate()
		{
			Debug.Log("LIV: Compositor connected, setting up Mixed Reality!");
			SubmitSDKOutput();
			CreateAssets();
			StartRenderCoroutine();
			IsActive = true;
			if (OnActivate != null) OnActivate.Invoke();
		}

		private void OnSDKDeactivate()
		{
			Debug.Log("LIV: Compositor disconnected, cleaning up Mixed Reality.");
			if (OnDeactivate != null) OnDeactivate.Invoke();
			StopRenderCoroutine();
			DestroyAssets();
			IsActive = false;
		}

		private void CreateAssets()
		{
			DestroyAssets();
			Render = new SDKRender(this);
		}

		private void DestroyAssets()
		{
			if (Render != null)
			{
				Render.Dispose();
				Render = null;
			}
		}

		private void StartRenderCoroutine()
		{
			StopRenderCoroutine();
			// TODO: Document change. Need to use MelonCoroutines in IL2cpp mods.
			_waitForEndOfFrameCoroutine = MelonCoroutines.Start(WaitForUnityEndOfFrame());
		}

		private void StopRenderCoroutine()
		{
			if (_waitForEndOfFrameCoroutine != null)
			{
				// TODO: Document change. Need to use MelonCoroutines in IL2cpp mods.
				MelonCoroutines.Stop(_waitForEndOfFrameCoroutine);
				_waitForEndOfFrameCoroutine = null;
			}
		}

		private void SubmitSDKOutput()
		{
			var output = SDKApplicationOutput.Empty;
			output.supportedFeatures = Features.BackgroundRender |
			                           Features.ForegroundRender |
			                           Features.OverridePostProcessing |
			                           Features.FixForegroundAlpha;

			output.sdkID = SDKConstants.SDKID;
			output.sdkVersion = SDKConstants.SDKVersion;
			output.engineName = SDKConstants.EngineName;
			output.engineVersion = Application.unityVersion;
			output.applicationName = Application.productName;
			output.applicationVersion = Application.version;
			output.graphicsAPI = SystemInfo.graphicsDeviceType.ToString();
#if UNITY_2017_2_OR_NEWER
			output.xrDeviceName = XRSettings.loadedDeviceName;
#endif
			SDKBridge.SubmitApplicationOutput(output);
		}

		private void Invalidate()
		{
			if (SDKUtils.ContainsFlag((uint) _invalidate, (uint) InvalidationFlags.Stage))
			{
				_stage = _stageCandidate;
				_stageCandidate = null;
				_invalidate = (InvalidationFlags) SDKUtils.SetFlag((uint) _invalidate, (uint) InvalidationFlags.Stage, false);
			}

			if (SDKUtils.ContainsFlag((uint) _invalidate, (uint) InvalidationFlags.HmdCamera))
			{
				_hmdCamera = _hmdCameraCandidate;
				_hmdCameraCandidate = null;
				_invalidate = (InvalidationFlags) SDKUtils.SetFlag((uint) _invalidate, (uint) InvalidationFlags.HmdCamera, false);
			}

			if (SDKUtils.ContainsFlag((uint) _invalidate, (uint) InvalidationFlags.MrCameraPrefab))
			{
				_mrCameraPrefab = _mrCameraPrefabCandidate;
				_mrCameraPrefabCandidate = null;
				_invalidate = (InvalidationFlags) SDKUtils.SetFlag((uint) _invalidate, (uint) InvalidationFlags.MrCameraPrefab, false);
			}

			if (SDKUtils.ContainsFlag((uint) _invalidate, (uint) InvalidationFlags.ExcludeBehaviours))
			{
				_excludeBehaviours = _excludeBehavioursCandidate;
				_excludeBehavioursCandidate = null;
				_invalidate = (InvalidationFlags) SDKUtils.SetFlag((uint) _invalidate, (uint) InvalidationFlags.ExcludeBehaviours, false);
			}
		}
	}
}