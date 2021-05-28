using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XLua;
using Action = System.Action;

namespace MFramework.Lua
{
    public static class Generator
    {
        // Lua call C#
        [LuaCallCSharp]
        public static List<Type> ClassExport
        {
            get
            {
                List<Type> list = new List<Type>()
                {
                    typeof(System.Object),
                    typeof(UnityEngine.Object),
                    typeof(Ray2D),
                    typeof(GameObject),
                    typeof(Component),
                    typeof(Behaviour),
                    typeof(Transform),
                    typeof(Resources),
                    typeof(TextAsset),
                    typeof(Keyframe),
                    typeof(AnimationCurve),
                    typeof(AnimationClip),
                    typeof(MonoBehaviour),
                    typeof(ParticleSystem),
                    typeof(SkinnedMeshRenderer),
                    typeof(Renderer),
                    typeof(WWW),
                    typeof(List<int>),
                    typeof(Action<string>),
                    typeof(UnityEngine.Debug),
                    typeof(Delegate),
                    typeof(Dictionary<string, GameObject>),
                    typeof(UnityEngine.Events.UnityEvent),

                    // unity结合lua，这部分导出很多功能在lua侧重新实现，没有实现的功能才会跑到cs侧
                    typeof(Bounds),
                    typeof(Color),
                    typeof(LayerMask),
                    typeof(Mathf),
                    typeof(Plane),
                    typeof(Quaternion),
                    typeof(Ray),
                    typeof(RaycastHit),
                    typeof(Time),
                    typeof(Touch),
                    typeof(TouchPhase),
                    typeof(Vector2),
                    typeof(Vector3),
                    typeof(Vector4),
                    
                    // 渲染
                    typeof(RenderMode),
                    
                    // UGUI  
                    typeof(UnityEngine.Canvas),
                    typeof(UnityEngine.Rect),
                    typeof(UnityEngine.RectTransform),
                    typeof(UnityEngine.RectOffset),
                    typeof(UnityEngine.Sprite),
                    typeof(UnityEngine.UI.CanvasScaler),
                    typeof(UnityEngine.UI.CanvasScaler.ScaleMode),
                    typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode),
                    typeof(UnityEngine.UI.GraphicRaycaster),
                    typeof(UnityEngine.UI.Text),
                    typeof(UnityEngine.UI.InputField),
                    typeof(UnityEngine.UI.Button),
                    typeof(UnityEngine.UI.Image),
                    typeof(UnityEngine.UI.ScrollRect),
                    typeof(UnityEngine.UI.Scrollbar),
                    typeof(UnityEngine.UI.Toggle),
                    typeof(UnityEngine.UI.ToggleGroup),
                    typeof(UnityEngine.UI.Button.ButtonClickedEvent),
                    typeof(UnityEngine.UI.ScrollRect.ScrollRectEvent),
                    typeof(UnityEngine.UI.GridLayoutGroup),
                    typeof(UnityEngine.UI.ContentSizeFitter),
                    typeof(UnityEngine.UI.Slider),

                    // easy touch
                    // TODO：后续需要什么脚本再添加进来
                    //typeof(ETCArea),
                    //typeof(ETCAxis),
                    //typeof(ETCButton),
                    //typeof(ETCInput),
                    //typeof(ETCJoystick),

                    // 场景、资源加载
                    typeof(UnityEngine.Resources),
                    typeof(UnityEngine.ResourceRequest),
                    typeof(UnityEngine.SceneManagement.SceneManager),
                    
                    // 其它
                    typeof(PlayerPrefs),
                    typeof(System.GC),
                    typeof(AsyncOperation),
                };
                return list;
            }
        }

        // C# call Lua
        [CSharpCallLua]
        public static List<Type> DelegateExport = new List<Type>()
        {
            typeof(Action),
            typeof(Action<int>),
            typeof(Action<WWW>),
            typeof(UnityEngine.Event),
            typeof(UnityEngine.Events.UnityAction),
            typeof(System.Collections.IEnumerator),
            typeof(UnityEngine.Events.UnityAction<Vector2>),
        };

        // 避免在IL2CPP下被裁剪
        [ReflectionUse]
        public static List<Type> ReflectionUse = new List<Type>(){
            typeof(AsyncOperation),
        };

        // black list
        [BlackList]
        public static List<List<string>> BlackList = new List<List<string>>()
        {
		    // unity
		    new List<string>(){"UnityEngine.WWW", "movie"},
            new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
            new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
            new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
            new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
            new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
            new List<string>(){"UnityEngine.Light", "areaSize"},
            new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
		    #if !UNITY_WEBPLAYER
		    new List<string>(){"UnityEngine.Application", "ExternalEval"},
		    #endif
		    new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
		    new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
		    new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
            new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
            new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
            new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
        };
    }

    public static class HotfixConfig
    {
        private static List<string> ExcludeNamespace = new List<string>()
        {
            //格式："FFmpeg"
        };

        private static List<Type> ExcludeType = new List<Type>()
        {
        };

        [Hotfix]
        public static List<Type> ClassHotfix
        {
            get
            {
                List<Type> list = Assembly.Load("Assembly-CSharp").GetTypes().Where(t => !Exclude(t)).ToList();
                list.AddRange(Assembly.Load("Assembly-CSharp-firstpass").GetTypes().Where(t => !Exclude(t)).ToList());
                return list;
            }
        }

        public static bool Exclude(Type t)
        {
            // namespace
            if (!string.IsNullOrEmpty(t.Namespace))
            {
                foreach (string ns in ExcludeNamespace)
                {
                    if (t.Namespace.Contains(ns))
                    {
                        return true;
                    }
                }
            }

            // type
            if (ExcludeType.Contains(t))
            {
                return true;
            }

            // properties
            foreach (PropertyInfo p in t.GetProperties())
            {
                if (ExcludeType.Contains(p.PropertyType))
                {
                    return true;
                }
            }

            // param & return
            MethodInfo[] methods = t.GetMethods();
            foreach (MethodInfo m in methods)
            {
                // return
                if (ExcludeType.Contains(m.ReturnType))
                {
                    return true;
                }

                // params
                foreach (ParameterInfo p in m.GetParameters())
                {
                    if (ExcludeType.Contains(p.ParameterType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

}
