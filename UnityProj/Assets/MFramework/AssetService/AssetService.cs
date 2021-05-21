using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace MFramework.AssetService
{
    public class AssetService : SingletonMB<AssetService>
    {
        private AssetLoader AssetLoader;
        protected override void Awake()
        {
            base.Awake();
#if !UNITY_EDITOR || USE_BUNDLE
            AssetLoader = new BundleAssetLoader();
#else
            AssetLoader = new LocalAssetLoader();
#endif
    }

        public AssetBase LoadAsset(string path)
        {
            return AssetLoader.LoadAsset(path);
        }

        public AssetRequest LoadAssetAsync(string path)
        {
            return AssetLoader.LoadAssetAsync(path);
        }
    }
}
