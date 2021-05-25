using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.HotUpdateService
{
	public class HotUpdateService: SingletonMB<HotUpdateService>
	{
		public HotUpdateAsyncOperation StartUpdate()
		{
			return new HotUpdateAsyncOperation();
		}
	}
}
