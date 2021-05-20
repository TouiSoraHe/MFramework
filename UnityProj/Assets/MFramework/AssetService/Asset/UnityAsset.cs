using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
    public class UnityAsset : AssetBase
    {

        protected override void InitVerify(string resPath, object data)
        {
            if (!typeof(UnityEngine.Object).IsAssignableFrom(data.GetType()))
            {
                throw new System.ArgumentException("data 参数类型错误,type:" + data.GetType().Name);
            }
        }

        protected override object CopyData()
        {
            if (Data == null || (Data as UnityEngine.Object) == null)
            {
                Log.LogE("UnityAsset.CopyData:严重错误，拷贝源数据为空,path:{0}",ResPath);
                return null;
            }
            return UnityEngine.Object.Instantiate(Data as UnityEngine.Object);
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
            Data = null;
        }
    }
}
