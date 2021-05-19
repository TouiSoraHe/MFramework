using MFramework.Common;
using MFramework.ScheduleService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
    public class BundleAssetRequest : AssetRequest, IScheduleHandler
    {
        private string assetPath;
        private BundleCreateAssetRequest mainBundleCreateAssetRequest;
        List<BundleCreateAssetRequest> dependencyList;
        private AssetBundleRequest assetBundleRequest;
        private int loadingCount;

        public BundleAssetRequest(string assetPath,BundleCreateAssetRequest mainBundleCreateAssetRequest,List<BundleCreateAssetRequest> dependencyList)
        {
            if (mainBundleCreateAssetRequest == null)
            {
                throw new System.Exception("参数 mainBundleCreateAssetRequest 为 null");
            }
            this.assetPath = assetPath;
            this.mainBundleCreateAssetRequest = mainBundleCreateAssetRequest;
            this.dependencyList = dependencyList;
            loadingCount = 1;

            this.mainBundleCreateAssetRequest.Completed += BundleCreateAssetRequestCompleted;
            if (this.dependencyList != null)
            {
                loadingCount = loadingCount + this.dependencyList.Count;
                foreach (var dependency in this.dependencyList)
                {
                    dependency.Completed += BundleCreateAssetRequestCompleted;
                }
            }
        }

        public BundleAssetRequest(string assetPath,UnityAsset unityAsset)
        {
            if (unityAsset == null)
            {
                throw new System.Exception("参数 unityAsset 为 null");
            }
            this.assetPath = assetPath;
            Asset = unityAsset;
            ScheduleService.ScheduleService.GetInstance().AddFrame(1, false, this);
        }

        protected override AssetRequest OnClone()
        {
            if (this.mainBundleCreateAssetRequest != null)
            {
                return new BundleAssetRequest(this.assetPath, this.mainBundleCreateAssetRequest, this.dependencyList);
            }
            else
            {
                return new BundleAssetRequest(this.assetPath, this.Asset as UnityAsset);
            }
        }

        protected void BundleCreateAssetRequestCompleted(CustomAsyncOperation obj)
        {
            loadingCount--;
            AllLoaded();
        }

        private void AllLoaded()
        {
            if (loadingCount == 0)
            {
                if (mainBundleCreateAssetRequest.BundleAsset != null && (mainBundleCreateAssetRequest.BundleAsset.Data as AssetBundle) != null)
                {
                    LoadFromAssetBundle(mainBundleCreateAssetRequest.BundleAsset.Data as AssetBundle);
                }
                else
                {
                    Log.LogE("BundleAssetRequest.BundleCreateAssetRequestCompleted:AssetBundle加载失败,路径:{0}", assetPath);
                    CompletedInvoke();
                }
            }
        }

        private void LoadFromAssetBundle(AssetBundle assetBundle)
        {
            assetBundleRequest = assetBundle.LoadAssetAsync(assetPath);
            assetBundleRequest.completed += OnCompleted;
            assetBundleRequest.completed += CompletedInvoke;
        }

        protected void OnCompleted(AsyncOperation obj)
        {
            AssetBundleRequest assetBundleRequest = obj as AssetBundleRequest;
            if (assetBundleRequest != null)
            {
                if (isClone)
                {
                    Asset = AssetBase.AssetManager.Copy<UnityAsset>(assetPath);
                }
                else
                {
                    Asset = AssetBase.AssetManager.Create<UnityAsset>(assetPath, assetBundleRequest.asset);
                }
            }
        }

        private void CompletedInvoke(object obj)
        {
            CompletedInvoke();
        }

        protected override float OnProgress()
        {
            if (mainBundleCreateAssetRequest == null)
            {
                return assetBundleRequest.progress;
            }
            if (assetBundleRequest == null)
            {
                return mainBundleCreateAssetRequest.Progress / 2;
            }
            else
            {
                return 0.5f + assetBundleRequest.progress / 2;
            }
        }

        public void OnScheduleHandle(ScheduleType type, uint id)
        {
            CompletedInvoke(null);
            ScheduleService.ScheduleService.GetInstance().RemoveFrame(id);
        }
    }
}
