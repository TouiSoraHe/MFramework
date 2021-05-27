using MFramework.AssetService;
using MFramework.Common;
using MFramework.Config;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MFramework.Editor
{
    public class MenuEditor : OdinMenuEditorWindow
    {
        private List<ConfigBase> AllConfig;

        [MenuItem("MFramework/Menu")]
        private static void OpenWindow()
        {
            GetWindow<MenuEditor>().Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree odinMenuTree = new OdinMenuTree();

            AllConfig = new List<ConfigBase>();
            AllConfig.Add(ConfigBase.LoadConfig<BuildConfig>());
            AllConfig.Add(ConfigBase.LoadConfig<HotUpdateConfig>());
            AllConfig.Add(ConfigBase.LoadConfig<AssetConfig>());

            foreach (var item in AllConfig)
            {
                odinMenuTree.Add("Config/" + item.GetType().Name, item);
            }

            return odinMenuTree;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SaveAndClear();
        }

        private void SaveAndClear()
        {
            if (AllConfig != null)
            {
                foreach (var item in AllConfig)
                {
                    item.SaveConfig();
                }
            }
        }




        [MenuItem("Assets/MFramework/BuildTools/复制资源加载路径")]
        public static void CopyLoadPath()
        {
            if (Selection.objects.Length == 1)
            {
                Utility.SetSystemCopyBuffer(AssetDatabase.GetAssetPath(Selection.objects[0]));
            }
            else
            {
                Log.LogE("参数错误,需要选择一个物体");
            }
        }
    }
}
