using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
    public class UnityAsset : AssetBase
    {
        protected override void Init(string resPath, object data)
        {
            if (!typeof(UnityEngine.Object).IsAssignableFrom(data.GetType()))
            {
                throw new System.ArgumentException("data 参数类型错误,type:" + data.GetType().Name);
            }
            base.Init(resPath, data);
        }

        protected override AssetBase Copy()
        {
            UnityEngine.Object copyData = UnityEngine.Object.Instantiate(Data as UnityEngine.Object);
            UnityAsset unityAsset = new UnityAsset();
            unityAsset.Init(ResPath, copyData);
            return unityAsset;
        }

        protected override void OnUnload()
        {
            if (Data != null)
            {
                UnityEngine.Object.Destroy(Data as UnityEngine.Object);
                Data = null;
            }
        }

        protected override void OnRealUnload()
        {
        }
    }
}
