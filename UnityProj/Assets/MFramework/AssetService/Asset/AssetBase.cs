using MFramework.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
	public abstract class AssetBase
    {
        private string resPath;
		private object data;
        private bool isUnload = false;

        public string ResPath
        {
            get
            {
                return resPath;
            }
            protected set
            {
                resPath = value;
            }
        }

        public object Data
        {
            get
            {
                return data;
            }
            protected set
            {
                data = value;
            }
        }

        public AssetBase(){ }
        protected virtual void Init(string resPath, object data)
        {
            this.ResPath = resPath;
            Data = data;
        }

        public void Unload()
        {
            AssetManager.Unload(this);
        }

        private void Unload(bool isReal)
        {
            if (isUnload)
            {
                Log.LogE("Asset.Unload:资源已卸载，请勿重复卸载");
                return;
            }
            isUnload = true;
            if (isReal)
            {
                OnRealUnload();
            }
            else
            {
                OnUnload();
            }
        }

        protected abstract AssetBase Copy();

        protected abstract void OnUnload();

        protected abstract void OnRealUnload();

        public class AssetManager
        {
            public static event Action<AssetBase> AssetUnload;
            private static Dictionary<string, CacheInfo> AssetCache = new Dictionary<string, CacheInfo>();
            private class CacheInfo
            {
                public AssetBase asset;
                public int referenceCount = 1;

                public CacheInfo(AssetBase asset)
                {
                    this.asset = asset;
                }
            }

            public static T Create<T>(string resPath, object data) where T : AssetBase,new()
            {
                if (string.IsNullOrEmpty(resPath))
                {
                    Log.LogE("Asset.Create:参数 resPath 不能为空");
                    return null;
                }
                if (data == null)
                {
                    Log.LogE("Asset.Create:参数 data 不能为空");
                    return null;
                }
                if (AssetCache.ContainsKey(resPath))
                {
                    Log.LogE("Asset.Create:资源已存在缓存列表，禁止重复创建,path:{0}", resPath);
                    return AssetCache[resPath].asset as T;
                }
                T asset = new T();
                asset.Init(resPath, data);
                AssetCache.Add(resPath, new CacheInfo(asset));
                return Copy<T>(resPath);
            }

            public static T Copy<T>(string resPath) where T : AssetBase,new()
            {
                if (AssetCache.ContainsKey(resPath))
                {
                    AssetCache[resPath].referenceCount++;
                    return AssetCache[resPath].asset.Copy() as T;
                }
                return null;
            }

            public static void Unload(AssetBase asset)
            {
                if (!AssetCache.ContainsKey(asset.ResPath))
                {
                    throw new Exception("Asset.Unload:销毁的资源在缓存列表不存在,path:" + asset.ResPath);
                }
                CacheInfo assetCache = AssetCache[asset.ResPath];
                if (assetCache.asset == asset)
                {
                    throw new Exception("Asset.Unload:严重错误，意外卸载Cache资源");
                }
                assetCache.referenceCount--;
                asset.Unload(false);
                if (assetCache.referenceCount <= 1)
                {
                    assetCache.asset.Unload(true);
                    AssetCache.Remove(asset.ResPath);
                    if (AssetUnload != null)
                    {
                        AssetUnload.Invoke(assetCache.asset);
                    }
                }
            }
        }
    }
}
