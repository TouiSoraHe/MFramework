using MFramework.Common;
using MFramework.Config;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Config
{
    public class AssetConfig : ConfigBase
    {
        [TitleGroup("资源加载模式"),Button(ButtonSizes.Large),ButtonGroup("资源加载模式/Button"), LabelText("远程Bundle")]
        public static void SetBundleModeRemote()
        {
            Utility.AddDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "USE_BUNDLE");
            Utility.RemoveDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "LOCAL_BUNDLE");
        }

        [TitleGroup("资源加载模式"),Button(ButtonSizes.Large), ButtonGroup("资源加载模式/Button"), LabelText("本地Bundle")]
        public static void SetBundleModeLocal()
        {
            Utility.AddDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "USE_BUNDLE");
            Utility.AddDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "LOCAL_BUNDLE");
        }

        [TitleGroup("资源加载模式"),Button(ButtonSizes.Large), ButtonGroup("资源加载模式/Button"), LabelText("本地")]
        public static void SetBundleModeResource()
        {
            Utility.RemoveDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "USE_BUNDLE");
            Utility.RemoveDefineSymbol(UnityEditor.BuildTargetGroup.Standalone, "LOCAL_BUNDLE");
        }

        protected override bool IsSerialize()
        {
            return false;
        }
    }
}
