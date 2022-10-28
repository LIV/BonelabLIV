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
    [HarmonyPatch(typeof(RigManager), "Start")]
    private static void CreateDebugKeys(RigManager __instance)
    {
      LivRigManager.Create(__instance);
    }
  }
}
