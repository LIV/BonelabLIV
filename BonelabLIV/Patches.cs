using HarmonyLib;
using UnityEngine;

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
  }
}
