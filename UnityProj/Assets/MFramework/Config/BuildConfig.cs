using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using System;
using MFramework.Build;
using MFramework.Common;
using UnityEngine;

namespace MFramework.Config
{
    public class BuildConfig : ConfigBase
	{
		[ReadOnly, LabelText("构建版本")]
		public int BuildVersion = 1;

		[Required, LabelText("构建输出相对路径"),ReadOnly]
        public string BaseOutPutPath = PathMgr.Editor_BuildRelativePath(BuildPlatform.Windows);

		[Required, LabelText("构建平台"),OnValueChanged("BuildPlatformChange")]
		public BuildPlatform Platform = BuildPlatform.Windows;

		[ShowInInspector,LabelText("构建列表"),TableList]
		public List<Builder> Builders = new List<Builder>();

		[Button(ButtonSizes.Large),LabelText("构建")]
		public void StartBuild()
		{
#if UNITY_EDITOR
			var buildPath = PathMgr.Editor_BuildFullPath(Platform);
			Utility.DeleteDirectory(buildPath);
			foreach (var builder in Builders)
            {
                if (builder == null)
                {
					throw new Exception("构建器不能为空");
                }
				builder.Build(Platform, buildPath);
			}
			GenerateFileInfo();
			BuildVersion++;
			SaveConfig();
#endif
		}

		public class FileInfo
		{
			public string RelativePath;
			public long FileSize;
			public string MD5;

            public FileInfo(string relativePath, long fileSize, string mD5)
            {
                RelativePath = relativePath;
                FileSize = fileSize;
                MD5 = mD5;
            }
        }

		public class BuildInfo
		{
			public static readonly string BuildInfoFileName = "BuildInfo.txt";

			public int Version;
			public Dictionary<string,FileInfo> FileInfos = new Dictionary<string, FileInfo>();

            public BuildInfo(int version)
            {
                Version = version;
            }
        }

		private void GenerateFileInfo()
		{
			var rootPath = PathMgr.Editor_BuildFullPath(Platform);
            if (Directory.Exists(rootPath))
            {
				BuildInfo buildInfo = new BuildInfo(BuildVersion);
				foreach (var item in Directory.GetFiles(rootPath,"*",SearchOption.AllDirectories))
				{
					var relativePath = item.Replace("\\", "/").Replace(rootPath + "/", "");
					var md5 = Utility.GetMD5HashByPath(item);
					System.IO.FileInfo sysFileInfo = new System.IO.FileInfo(item);
					buildInfo.FileInfos.Add(relativePath,new FileInfo(relativePath, sysFileInfo.Length, md5));
				}
				string fileInfoPath = Utility.CombinePaths(PathMgr.Editor_BuildFullPath(Platform), BuildInfo.BuildInfoFileName);
				Utility.WriteFile(fileInfoPath, Utility.Serialize(buildInfo));
			}
		}

		private void BuildPlatformChange()
		{
			BaseOutPutPath = PathMgr.Editor_BuildRelativePath(Platform); 
		}
	}
}
