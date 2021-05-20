using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework.ScheduleService;

namespace MFramework.AssetService
{
    public class LocalAssetRequest : AssetRequest, IScheduleHandler
    {
        private ResourceRequest ResourceRequest;
        private string assetPath;

        public LocalAssetRequest(string assetPath, UnityAsset asset)
        {
            if (asset == null)
            {
                throw new System.Exception("参数 asset 为 null");
            }
            this.assetPath = assetPath;
            Asset = asset;
            NextTickCompletedInvoke(null);
        }

        public LocalAssetRequest(string assetPath,ResourceRequest resourceRequest)
        {
            if (resourceRequest == null)
            {
                throw new System.Exception("参数 resourceRequest 为 null");
            }
            this.assetPath = assetPath;
            ResourceRequest = resourceRequest;
            ResourceRequest.completed += OnCompleted;
            ResourceRequest.completed += NextTickCompletedInvoke;
        }

        protected override AssetRequest OnClone()
        {
            if (this.ResourceRequest != null)
            {
                return new LocalAssetRequest(this.assetPath, this.ResourceRequest);
            }
            else
            {
                return new LocalAssetRequest(this.assetPath, this.Asset as UnityAsset);
            }
        }

        protected void OnCompleted(AsyncOperation obj)
        {
            ResourceRequest resourceRequest = obj as ResourceRequest;
            if (resourceRequest != null)
            {
                if (!isClone)
                {
                    Asset = AssetBase.AssetManager.Create<UnityAsset>(assetPath, resourceRequest.asset);
                }
                else
                {
                    Asset = AssetBase.AssetManager.Copy<UnityAsset>(assetPath);
                }
            }
        }

        private void NextTickCompletedInvoke(object obj)
        {
            ScheduleService.ScheduleService.GetInstance().AddFrame(1, false, this);
        }

        public void OnScheduleHandle(ScheduleType type, uint id)
        {
            CompletedInvoke();
            ScheduleService.ScheduleService.GetInstance().RemoveFrame(id);
        }

        protected override float OnProgress()
        {
            return ResourceRequest.progress;
        }
    }
}
