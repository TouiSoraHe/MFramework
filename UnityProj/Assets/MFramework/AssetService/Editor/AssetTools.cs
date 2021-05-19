using MFramework.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MFramework.AssetService
{
    public class AssetTools : EditorWindow
    {

        [MenuItem("MFramework/AssetTools/设置资源加载模式/远程Bundle")]
        public static void SetBundleModeRemote()
        {
            Utility.AddDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "USE_BUNDLE");
            Utility.RemoveDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "LOCAL_BUNDLE");
        }

        [MenuItem("MFramework/AssetTools/设置资源加载模式/本地Bundle")]
        public static void SetBundleModeLocal()
        {
            Utility.AddDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "USE_BUNDLE");
            Utility.AddDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "LOCAL_BUNDLE");
        }

        [MenuItem("MFramework/AssetTools/设置资源加载模式/本地Resource")]
        public static void SetBundleModeResource()
        {
            Utility.RemoveDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "USE_BUNDLE");
        }
    }
}
