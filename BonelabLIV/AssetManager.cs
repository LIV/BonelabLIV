using System;
using System.IO;
using MelonLoader;
using UnityEngine;

namespace BonelabLIV
{
  public class AssetManager
  {
    private readonly string _assetsDirectory;

    public AssetManager(string assetsDirectory)
    {
      _assetsDirectory = assetsDirectory;
    }

    private static AssetBundle _bundle;

    public AssetBundle LoadBundle(string assetName)
    {
      var bundlePath = Path.Combine(_assetsDirectory, assetName);
      _bundle = AssetBundle.LoadFromFile(bundlePath);

      MelonLogger.Msg("### Loading bundle from " + bundlePath);

      if (_bundle == null)
      {
        throw new Exception("Failed to load asset bundle " + bundlePath);
      }

      return _bundle;
    }
  }
}
