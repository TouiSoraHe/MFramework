using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.AssetService
{
    public abstract class AssetRequest : CustomAsyncOperation
    {
        private AssetBase asset;
        protected bool isClone = false;

        public AssetBase Asset
        {
            get
            {
                return asset;
            }
            protected set
            {
                asset = value;
            }
        }

        protected AssetRequest()
        {
        }

        public AssetRequest Clone()
        {
            AssetRequest assetRequest = OnClone();
            assetRequest.isClone = true;
            return assetRequest;
        }

        protected abstract AssetRequest OnClone();
    }
}
