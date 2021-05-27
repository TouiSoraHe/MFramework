using MFramework.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
	public abstract class AssetBase
    {
        #region private variable
        private string resPath;
		private object data;
        private bool isUnload = false;
        #endregion

        #region property
        /// <summary>
        /// 资源路径
        /// </summary>
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

        /// <summary>
        /// 包裹的资源数据
        /// </summary>
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
        #endregion

        #region public function
        /// <summary>
        /// 卸载资源
        /// </summary>
        public void Unload()
        {
            AssetManager.Unload(this);
        }
        #endregion

        #region protected function
        /// <summary>
        /// 子类校验初始化参数
        /// </summary>
        /// <param name="resPath">资源路径</param>
        /// <param name="data">包裹的资源数据</param>
        protected abstract void InitVerify(string resPath, object data);

        /// <summary>
        /// 拷贝 Data 时被调用
        /// </summary>
        /// <returns>拷贝出来的 Data</returns>
        protected abstract object CopyData();

        /// <summary>
        /// 卸载资源时被调用，供子类清理Data
        /// </summary>
        protected abstract void OnUnload();

        /// <summary>
        /// 所有拷贝资源都被卸载，最原始的资源卸载时被调用
        /// </summary>
        protected abstract void OnRealUnload();
        #endregion

        #region private function
        /// <summary>
        /// 初始化方法
        /// </summary>
        /// <param name="resPath">资源路径</param>
        /// <param name="data">资源数据</param>
        private void Init(string resPath, object data)
        {
            ResPath = resPath;
            Data = data;
        }

        /// <summary>
        /// 真正的卸载方法
        /// </summary>
        /// <param name="isReal">是否为原始资源</param>
        /// <returns>是否卸载成功</returns>
        private bool Unload(bool isReal)
        {
            if (isUnload)
            {
                Log.LogE("Asset.Unload:资源已卸载，请勿重复卸载");
                return false;
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
            return true;
        }

        /// <summary>
        /// 用于拷贝一份资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T Copy<T>() where T : AssetBase, new()
        {
            object obj = CopyData();
            T asset = new T();
            asset.Init(ResPath, obj);
            return asset;
        }

        /// <summary>
        /// 仅作提示，在编辑器模式下，没有卸载资源直接停止运行，该方法可能会被调用
        /// </summary>
        ~AssetBase()
        {
            if (!isUnload)
            {
                Log.LogE("AssetBase:严重错误，资源未卸载，path:" + ResPath);
            }
        }
        #endregion

        #region inner class
        public class AssetManager
        {
            #region private variable
            /// <summary>
            /// 资源卸载事件，当资源被彻底卸载时触发
            /// </summary>
            public static event Action<AssetBase> AssetUnload;
            /// <summary>
            /// 缓存信息，引用计数信息
            /// </summary>
            private static Dictionary<string, CacheInfo> AssetCache;
            #endregion

            private class CacheInfo
            {
                public AssetBase asset;
                public int referenceCount = 1;

                public CacheInfo(AssetBase asset)
                {
                    this.asset = asset;
                }
            }

            /// <summary>
            /// Eidtor中静态变量不会被清理，所以每次运行时初始化一下
            /// </summary>
            public static void Init()
            {
                AssetCache = new Dictionary<string, CacheInfo>();
            }

            /// <summary>
            /// 创建一个 AssetBase 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="resPath"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public static T Create<T>(string resPath, object data) where T : AssetBase, new()
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

            /// <summary>
            /// 根据路径 尝试获取一份AssetBase的拷贝
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="resPath"></param>
            /// <returns></returns>
            public static T TryCopy<T>(string resPath) where T : AssetBase, new()
            {
                if (AssetCache.ContainsKey(resPath))
                {
                    return Copy<T>(resPath);
                }
                return null;
            }

            /// <summary>
            /// 根据路径 获取一份AssetBase的拷贝
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="resPath"></param>
            /// <returns></returns>
            public static T Copy<T>(string resPath) where T : AssetBase, new()
            {
                if (AssetCache.ContainsKey(resPath))
                {
                    AssetCache[resPath].referenceCount++;
                    return AssetCache[resPath].asset.Copy<T>();
                }
                else
                {
                    Log.LogE("AssetManager.Copy:严重错误，拷贝源数据不存在,path:{0}", resPath);
                }
                return null;
            }

            /// <summary>
            /// 卸载一个AssetBase
            /// </summary>
            /// <param name="asset"></param>
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
                if (asset.Unload(false))
                {
                    assetCache.referenceCount--;
                    if (assetCache.referenceCount <= 1)
                    {
                        if (assetCache.asset.Unload(true) && AssetCache.Remove(asset.ResPath))
                        {
                            if (AssetUnload != null)
                            {
                                AssetUnload.Invoke(assetCache.asset);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
