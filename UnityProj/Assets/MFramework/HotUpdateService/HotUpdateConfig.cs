using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.HotUpdateService
{
	public class HotUpdateConfig : ConfigBase<HotUpdateConfig>
	{
		[SerializeField] private string updateUrl = "";

        public string UpdateUrl
        {
            get
            {
                return updateUrl;
            }
            private set
            {
                updateUrl = value;
            }
        }
    }
}
