using System;
using System.Collections.Generic;
using UnityEngine;

namespace BonelabLIV
{
  public class BodyRendererManager : MonoBehaviour
  {
    public List<Renderer> headRenderers = new List<Renderer>();
    private SkinnedMeshRenderer _renderer;

    public BodyRendererManager(IntPtr ptr) : base(ptr)
    {
    }

    private void Awake()
    {
      _renderer = GetComponent<SkinnedMeshRenderer>();
    }

    private void Update()
    {
      foreach (var headRenderer in headRenderers)
      {
        headRenderer.enabled = _renderer.enabled;
      }
    }
  }
}
