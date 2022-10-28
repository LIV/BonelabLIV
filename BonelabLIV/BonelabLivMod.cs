using System;
using LIV.SDK.Unity;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BonelabLIV
{
  public class BonelabLivMod : MelonMod
  {
    public static Action<Camera> OnCameraReady;

    private static GameObject _livObject;
    private Camera _spawnedCamera;
    private static Liv LivInstance => Liv.Instance;

    public override void OnInitializeMelon()
    {
      base.OnInitializeMelon();
      
      HarmonyInstance.PatchAll();
      
      SetUpLiv();
      ClassInjector.RegisterTypeInIl2Cpp<Liv>();
      ClassInjector.RegisterTypeInIl2Cpp<BodyRendererManager>();
      OnCameraReady += SetUpLiv;
    }

    public override void OnUpdate()
    {
      base.OnUpdate();

      UpdateFollowSpawnedCamera();
    }

    private void UpdateFollowSpawnedCamera()
    {
      var livRender = GetLivRender();
      if (livRender == null || _spawnedCamera == null) return;

      // When spawned objects get removed in Bonelab, they might not be destroyed and just be disabled.
      if (!_spawnedCamera.gameObject.activeInHierarchy)
      {
        _spawnedCamera = null;
        return;
      }

      var cameraTransform = _spawnedCamera.transform;
      livRender.SetPose(cameraTransform.position, cameraTransform.rotation, _spawnedCamera.fieldOfView);
    }

    private static void SetUpLiv()
    {
      MelonLogger.Msg("### setting up LIV...");
      
      // Since the mod manager doesn't copy stuff to the game directory,
      // we're loading the dll manually from the mod directory,
      // to make sure DllImport works as expected in the LIV SDK.
      SystemLibrary.LoadLibrary($@"{MelonUtils.BaseDirectory}\Mods\LIVAssets\LIV_Bridge.dll");

      var assetManager = new AssetManager($@"{MelonUtils.BaseDirectory}\Mods\LIVAssets\");
      var livAssetBundle = assetManager.LoadBundle("liv-shaders");
      SDKShaders.LoadFromAssetBundle(livAssetBundle);
    }

    private static Camera GetLivCamera()
    {
      try
      {
        return !LivInstance ? null : LivInstance.HmdCamera;
      }
      catch (Exception)
      {
        Liv.Instance = null;
      }
      return null;
    }


    private static SDKRender GetLivRender()
    {
      try
      {
        return !LivInstance ? null : LivInstance.Render;
      }
      catch (Exception)
      {
        Liv.Instance = null;
      }
      return null;
    }

    private static void SetUpLiv(Camera camera)
    {
      if (!camera)
      {
        MelonLogger.Msg("No camera provided, aborting LIV setup.");
        return;
      }

      var livCamera = GetLivCamera();
      if (livCamera == camera)
      {
        MelonLogger.Msg("LIV already set up with this camera, aborting LIV setup.");
        return;
      }

      MelonLogger.Msg($"Setting up LIV with camera {camera.name}...");
      if (_livObject)
      {
        Object.Destroy(_livObject);
      }

      var cameraParent = camera.transform.parent;
      var cameraPrefab = new GameObject("LivCameraPrefab");
      cameraPrefab.SetActive(false);
      var cameraPrefabCamera = cameraPrefab.AddComponent<Camera>();
      cameraPrefabCamera.CopyFrom(camera);
      cameraPrefab.transform.SetParent(cameraParent, false);

      _livObject = new GameObject("LIV");
      _livObject.SetActive(false);

      var liv = _livObject.AddComponent<Liv>();
      liv.HmdCamera = camera;
      liv.MrCameraPrefab = cameraPrefab.GetComponent<Camera>();
      liv.Stage = cameraParent;
      liv.FixPostEffectsAlpha = true;
      _livObject.SetActive(true);
    }
  }
}
