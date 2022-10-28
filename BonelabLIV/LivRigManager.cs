using System;
using BoneworksLIV;
using SLZ.Rig;
using UnityEngine;
using UnityEngine.Rendering;
using Avatar = SLZ.VRMK.Avatar;

namespace BonelabLIV
{
  public class LivRigManager : MonoBehaviour
  {
    private const KeyCode Key = KeyCode.L;
    private RigManager _rigManager;
    private bool _state = true;

    public LivRigManager(IntPtr ptr) : base(ptr)
    {
    }

    public static void Create(RigManager rigManager)
    {
      if (rigManager.GetComponent<LivRigManager>()) return;
      
      var instance = rigManager.gameObject.AddComponent<LivRigManager>();
      instance._rigManager = rigManager;
    }

    private void Start()
    {
      InvokeRepeating(nameof(FixHairMesh), 1, 1);
    }

    // The game hides the hair by default, since it can get in the way of important stuff, like seeing.
    // We gotta make it visible to the LIV camera but keep it invisible from the game camera, so we use layers.
    private void FixHairMesh()
    {
      foreach (var hairMesh in _rigManager.avatar.hairMeshes)
      {
        hairMesh.shadowCastingMode = ShadowCastingMode.On;
        hairMesh.gameObject.layer = (int) GameLayer.LivOnly;
      }
    }

    // Bonelab does this annoying thing where if the game gets unfocused (like if you open the SteamVR dashboard, or if
    // you open the LIV menu), it will change the VR camera position. This makes it difficult to adjust the LIV cameras.
    // I couldn't figure out a good way to automate this fix, so for now I just added a debug key that needs to be held
    // while pausing the game.
    private void LateUpdate()
    {
      if (Input.GetKeyDown(Key))
      {
        _state = !_state;
        
        // We only set it to enabled once.
        // Just to make sure we're not overriding any game behaviour that tries to disable the rig manager.
        if (_state)
        {
          _rigManager.enabled = true;
        }
      }

      // We set it to disabled every update, to make sure we override anything in the game that sets it to enabled.
      if (!_state)
      {
        _rigManager.enabled = false;
      }
    }
  }
}
