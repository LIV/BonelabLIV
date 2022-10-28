using System;
using SLZ.Rig;
using UnityEngine;

namespace BonelabLIV
{
  // Bonelab does this annoying thing where if the game gets unfocused (like if you open the SteamVR dashboard, or if
  // you open the LIV menu), it will change the VR camera position. This makes it difficult to adjust the LIV cameras.
  // I couldn't figure out a good way to automate this fix, so for now I just added a debug key that needs to be held
  // while pausing the game.
  public class PreventCameraChangingWhilePaused : MonoBehaviour
  {
    private const KeyCode Key = KeyCode.L;
    private RigManager _rigManager;

    public PreventCameraChangingWhilePaused(IntPtr ptr) : base(ptr)
    {
    }

    public static void Create(RigManager rigManager)
    {
      var instance = rigManager.gameObject.AddComponent<PreventCameraChangingWhilePaused>();
      instance._rigManager = rigManager;
    }

    private void LateUpdate()
    {
      if (Input.GetKey(Key))
      {
        _rigManager.enabled = false;
      } else if (Input.GetKeyUp(Key))
      {
        _rigManager.enabled = true;
      }
    }
  }
}
