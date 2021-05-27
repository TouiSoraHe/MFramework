using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Common
{
	public static class PathMgr
	{
		public static readonly string Application_DataPath = Application.dataPath;
		public static readonly string Application_PersistentDataPath = Application.persistentDataPath;
		public static readonly string Application_StreamingAssetsPath = Application.streamingAssetsPath;

		public static readonly string Build_Dir = "Build";
		public static readonly string Bundle_Dir = "Bundle";

		public static readonly string Editor_Bundle_Path = Utility.CombinePaths(Editor_BuildFullPath(Platform.GetCurrentPlatform()), Bundle_Dir);
		public static readonly string Runtime_Bundle_Path = Utility.CombinePaths(Application_PersistentDataPath, Build_Dir, Platform.GetCurBuildPlatformName(), Bundle_Dir);

		public static string Editor_BuildRelativePath(BuildPlatform platform)
		{
			return Utility.CombinePaths("../..", Build_Dir, Platform.GetBuildPlatformName(platform));
		}

		public static string Editor_BuildFullPath(BuildPlatform platform)
		{
			return Utility.CombinePaths(Application_DataPath, Editor_BuildRelativePath(platform));
		}
		public static readonly string Runtime_BuildFullPath = Utility.CombinePaths(Application_PersistentDataPath, Build_Dir, Platform.GetCurBuildPlatformName());
	}
}
