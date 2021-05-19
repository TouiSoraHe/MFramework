using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
    public abstract class AssetLoader
    {
        public abstract AssetBase LoadAsset(string path);
        public abstract AssetRequest LoadAssetAsync(string path);
    }
}
