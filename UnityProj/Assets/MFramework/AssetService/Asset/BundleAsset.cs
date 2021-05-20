using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
    public class BundleAsset : AssetBase
	{
        protected override void InitVerify(string resPath, object data)
        {
            if (!typeof(UnityEngine.AssetBundle).IsAssignableFrom(data.GetType()))
            {
                throw new System.ArgumentException("data 参数类型错误,type:" + data.GetType().Name);
            }
        }

        protected override object CopyData()
        {
            return Data;
        }

        protected override void OnUnload()
        {
            Data = null;
        }

        protected override void OnRealUnload()
        {
            if (Data == null || (Data as AssetBundle) == null)
            {
                Log.LogE("BundleAsset.OnRealUnload:严重错误，需要卸载的AssetBundle为空");
                return;
            }
            (Data as AssetBundle).Unload(true);
            Data = null;
        }
    }
}
