using System.Linq;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Valve.VR;

namespace BonelabLIV
{
  [HarmonyPatch]
  public static class Patches
  {
    // Names of objects belonging to the head of the default Bonelab player model.
    // TODO: investigate a good way to do this for custom models.
    private static readonly string[] FaceObjectNames =
    {
      "brett_face",
      "brett_hairCap",
      "brett_hairCards"
    };
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(XRLODBias), "Start")]
    private static void SetUpLiv(XRLODBias __instance)
    {
      BonelabLivMod.OnCameraReady((__instance).GetComponent<Camera>());
    }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(CharacterAnimationManager), "OnEnable")]
    // private static void SetUpBodyVisibility(CharacterAnimationManager __instance)
    // {
    // 	var renderers = __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
    // 	var bodyRenderer = renderers.First(renderer => renderer.name == "brett_body");
    // 	var bodyRendererCopyEnabledState = bodyRenderer.gameObject.AddComponent<BodyRendererManager>();
    //
    // 	foreach (var renderer in __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
    // 	{
    // 		var rendererObject = renderer.gameObject;
    // 		var isHeadObject = faceObjectNames.Contains(rendererObject.name);
    //
    // 		if (isHeadObject)
    // 		{
    // 			bodyRendererCopyEnabledState.headRenderers.Add(renderer);
    // 			rendererObject.SetActive(true);
    // 		}
    // 		rendererObject.layer = isHeadObject ? (int) GameLayer.LivOnly : (int) GameLayer.Player;
    // 	}
    // }
    //
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(RigEvent), "Awake")]
    // private static void HideHeadEffectsFromLiv(RigEvent __instance)
    // {
    // 	__instance.gameObject.layer = (int) GameLayer.Player;
    // }
  }
}
