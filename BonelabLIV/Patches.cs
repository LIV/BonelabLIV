using BoneworksLIV;
using HarmonyLib;
using SLZ.Rig;
using UnityEngine;
using UnityEngine.Rendering;

namespace BonelabLIV
{
  [HarmonyPatch]
  public static class Patches
  {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(XRLODBias), "Start")]
    private static void SetUpLiv(XRLODBias __instance)
    {
      BonelabLivMod.OnCameraReady((__instance).GetComponent<Camera>());
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SLZ.VRMK.Avatar), "Start")]
    private static void FixHair(SLZ.VRMK.Avatar __instance)
    {
      foreach (var hairMesh in __instance.hairMeshes)
      {
        hairMesh.shadowCastingMode = ShadowCastingMode.On;
        hairMesh.gameObject.layer = (int) GameLayer.LivOnly;
      }
    }
  }
}
