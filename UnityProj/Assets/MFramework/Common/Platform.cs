using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Common
{
	public enum BuildPlatform
	{
		Windows,
		Android,
		IOS
	}

	public static class Platform
	{
		public static BuildPlatform GetCurrentPlatform()
		{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			return BuildPlatform.Windows;
#elif UNITY_IOS
            return BuildPlatform.IOS;
#elif UNITY_ANDROID
            return BuildPlatform.Android;
#else
            throw new System.Exception("BuildConfig.GetCurrentPlatform:未知的平台");
#endif
		}

		public static string GetBuildPlatformName(BuildPlatform buildPlatform)
		{
			return buildPlatform.ToString();
		}

		public static string GetCurBuildPlatformName()
		{
			return GetBuildPlatformName(GetCurrentPlatform());
		}
	}
}
