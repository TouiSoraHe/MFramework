using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
    public class BundleAsset : AssetBase
	{
        protected override void Init(string resPath, object data)
        {
            if (!typeof(UnityEngine.AssetBundle).IsAssignableFrom(data.GetType()))
            {
                throw new System.ArgumentException("data 参数类型错误,type:" + data.GetType().Name);
            }
            base.Init(resPath, data);
        }

        protected override AssetBase Copy()
        {
            BundleAsset bundleAsset = new BundleAsset();
            bundleAsset.Init(ResPath, Data);
            return bundleAsset;
        }

        protected override void OnUnload()
        {
            this.Data = null;
        }

        protected override void OnRealUnload()
        {
            if (this.Data != null)
            {
                AssetBundle assetBundle = this.Data as AssetBundle;
                if (assetBundle != null)
                {
                    assetBundle.Unload(true);
                }
                this.Data = null;
            }
        }
    }
}
