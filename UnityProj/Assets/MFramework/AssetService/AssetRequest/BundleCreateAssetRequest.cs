using MFramework.ScheduleService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
    public class BundleCreateAssetRequest : AssetRequest, IScheduleHandler
    {
        private AssetBundleCreateRequest assetBundleCreateRequest;
        private string path;

        public BundleAsset BundleAsset
        {
            get
            {
                return Asset as BundleAsset;
            }
            private set
            {
                Asset = value;
            }
        }

        public BundleCreateAssetRequest(string path,AssetBundleCreateRequest assetBundleCreateRequest)
        {
            if (assetBundleCreateRequest == null)
            {
                throw new System.Exception("参数 assetBundleCreateRequest 为 null");
            }
            this.path = path;
            this.assetBundleCreateRequest = assetBundleCreateRequest;
            this.assetBundleCreateRequest.completed += OnCompleted;
            this.assetBundleCreateRequest.completed += CompletedInvoke;
        }

        public BundleCreateAssetRequest(string path,BundleAsset bundleAsset)
        {
            if (bundleAsset == null)
            {
                throw new System.Exception("参数 bundleAsset 为 null");
            }
            this.path = path;
            this.BundleAsset = bundleAsset;
            ScheduleService.ScheduleService.GetInstance().AddFrame(1, false, this);
        }

        protected override AssetRequest OnClone()
        {
            if (this.assetBundleCreateRequest != null)
            {
                return new BundleCreateAssetRequest(this.path, this.assetBundleCreateRequest);
            }
            else
            {
                return new BundleCreateAssetRequest(this.path, this.BundleAsset); 
            }
        }

        protected void OnCompleted(AsyncOperation obj)
        {
            AssetBundleCreateRequest assetBundleCreateRequest = obj as AssetBundleCreateRequest;
            if (assetBundleCreateRequest != null)
            {
                if (!isClone)
                {
                    BundleAsset = AssetBase.AssetManager.Create<BundleAsset>(path, assetBundleCreateRequest.assetBundle);
                }
                else
                {
                    BundleAsset = AssetBase.AssetManager.Copy<BundleAsset>(path);
                }
            }
        }

        private void CompletedInvoke(object obj)
        {
            CompletedInvoke();
        }

        protected override float OnProgress()
        {
            return this.assetBundleCreateRequest.progress;
        }

        public void OnScheduleHandle(ScheduleType type, uint id)
        {
            CompletedInvoke(null);
            ScheduleService.ScheduleService.GetInstance().RemoveFrame(id);
        }
    }
}
