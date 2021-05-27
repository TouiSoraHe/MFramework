using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Config
{
	public class HotUpdateConfig : ConfigBase
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
