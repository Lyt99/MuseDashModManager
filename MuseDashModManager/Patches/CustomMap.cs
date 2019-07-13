using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Assets.Scripts.UI.Panels;
using Newtonsoft.Json.Linq;
using Harmony;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.GameCore;
using Assets.Scripts.GameCore.Managers;
using UnityEngine;
using Assets.Scripts.PeroTools.Nice.Components;
using Assets.Scripts.PeroTools.Nice.Variables;
using Assets.Scripts.PeroTools.Nice.Interface;
using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Nice.Events;
using Assets.Scripts.PeroTools.Nice.Actions;
using System.IO;

namespace MuseDashModManager.Patches
{
    class CustomMap
    {
        public static Sprite spriteNoCover = null;

        public static CustomListVariable variable = new CustomListVariable();

        public static void Init()
        {
            var path = Application.streamingAssetsPath + "/nocover.png";
            var fileStream = new FileStream(path, FileMode.Open);
            var tex = new Texture2D(440, 440);

            fileStream.Seek(0, SeekOrigin.Begin);

            byte[] binary = new byte[fileStream.Length]; //创建文件长度的buffer
            fileStream.Read(binary, 0, (int)fileStream.Length);

            fileStream.Close();
            fileStream.Dispose();

            ImageConversion.LoadImage(tex, binary);
            spriteNoCover = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            InitCustomJArray();
        }

        public static bool HasInitCustomJArray = false;
        public static void InitCustomJArray()
        {
            // 添加Custom
            if (HasInitCustomJArray) return;
            HasInitCustomJArray = true;

            var array = new JArray();

            var @object = new JObject();
            @object.Add("uid", "custom-1");
            @object.Add("name", "自制谱面1");
            @object.Add("author", "我自己");
            @object.Add("bpm", "280");
            @object.Add("music", "iyaiya_music");
            @object.Add("demo", "iyaiya_demo");
            @object.Add("cover", "iyaiya_cover");
            @object.Add("noteJson", "iyaiya_map");
            @object.Add("scene", "scene_01");
            @object.Add("difficulty1", "10");
            @object.Add("difficulty2", "11");
            @object.Add("difficulty3", "18");
            @object.Add("unlockLevel", "0");

            array.Add(@object);

            var configManager = Singleton<ConfigManager>.instance;
            var mData = (Dictionary<string, JArray>)AccessTools.Field(typeof(ConfigManager), "m_Dictionary").GetValue(configManager);

            mData["custom"] = array;
        }


        public class CustomListVariable : IVariable
        {
            public List<string> CustomMaps = new List<string>();
            public object result { get => CustomMaps; set => CustomMaps = (List<string>)value; }
        }

        public static CustomImageVariable imageVariable = new CustomImageVariable();
        public class CustomImageVariable : IVariable
        {
            public IVariable variable;
            public object result {
                get {
                    if (spriteNoCover == null) Init();
                    return spriteNoCover;
                }
                set => throw new NotImplementedException();
            }


        }

        private class CustomInfo
        {
            public static JToken UID()
            {
                return "custom";
            }

            public static JToken Title()
            {
                return "自定义谱面";
            }

            public static JToken PrefabsName()
            {
                return "AlbumCustom";
            }

            public static JToken Price()
            {
                return "Free";
            }

            public static JToken JsonName()
            {
                return "custom";
            }

            public static JToken NeedPurchase()
            {
                return "false";
            }
        }

        public static void InitAlbumDatasPostfix(PnlStage __instance)
        {
            var dtype = typeof(PnlStage).GetNestedType("GetData", BindingFlags.NonPublic);

            // 反射获得m_AlbumDatas
            Type configManagerType = typeof(PnlStage);
            var field = configManagerType.GetField("m_AlbumDatas", BindingFlags.NonPublic | BindingFlags.Instance);
            var m_AlbumDatas =
                (object[])field.GetValue(__instance);

            var dictype = m_AlbumDatas[0].GetType();

            var newAlbumdata = Array.CreateInstance(dictype, m_AlbumDatas.Length + 1);
            m_AlbumDatas.CopyTo(newAlbumdata, 0);

            object newDict = Activator.CreateInstance(dictype);
            var addMethod = AccessTools.Method(dictype, "Add", new Type[] { typeof(string), dtype });
            addMethod.Invoke(newDict, new object[] { "uid", Delegate.CreateDelegate(dtype, typeof(CustomInfo), "UID") });
            addMethod.Invoke(newDict, new object[] { "title", Delegate.CreateDelegate(dtype, typeof(CustomInfo), "Title") });
            addMethod.Invoke(newDict, new object[] { "prefabsName", Delegate.CreateDelegate(dtype, typeof(CustomInfo), "PrefabsName") });
            addMethod.Invoke(newDict, new object[] { "price", Delegate.CreateDelegate(dtype, typeof(CustomInfo), "Price") });
            addMethod.Invoke(newDict, new object[] { "jsonName", Delegate.CreateDelegate(dtype, typeof(CustomInfo), "JsonName") });
            addMethod.Invoke(newDict, new object[] { "needPurchase", Delegate.CreateDelegate(dtype, typeof(CustomInfo), "NeedPurchase") });

            newAlbumdata.SetValue(newDict, m_AlbumDatas.Length);

            field.SetValue(__instance, newAlbumdata);

            GameInit.maxAlbumIndex += 1;

            var sView = __instance.albumFancyScrollView;

            //Transform aChild = (Transform)sView.content.GetChild(0);
            //aChild = (Transform)aChild.UnityClone();
            //aChild.name = "AlbumCustom";
            //aChild.SetParent(sView.content);
        }

        private static bool albumChildrenAdded = false;
        public static void FancyScrollAlbumRebuildChildrenPrefix(FancyScrollView __instance)
        {
            if (__instance.name == "FancyScrollAlbum" && !albumChildrenAdded)
            {
                GameObject custom = GameObject.Instantiate(__instance.content.GetChild(0).gameObject);
                custom.name = "ImgAlbumCustom";

                RectTransform transform = (RectTransform)custom.transform;
                transform.name = "ImgAlbumCustom";
                var text = transform.GetChild(1).GetComponent<UnityEngine.UI.Text>();
                text.text = "自定义谱面";

                transform.GetComponents(typeof(UnityEngine.Object));
                custom.transform.SetParent(__instance.content);

                albumChildrenAdded = true;
            }
        }

        public static bool musicAdded = false;
        public static GameObject AlbumCustom = null;
        public static GameObject CreateCustomMusicCells()
        {
            //if (musicAdded) return;

            GameObject prefabs = Singleton<AssetBundleManager>.instance.LoadFromName<GameObject>("AlbumCollection");
            prefabs = GameObject.Instantiate(prefabs);
            prefabs.name = "albumCustom";
            var component = prefabs.GetComponent<OnFancyScrollViewCellUpdate>();
            var ifp = component.playables[0];

            var fexecute = AccessTools.Field(typeof(If), "m_IsExecute");
            Formula f = (Formula)fexecute.GetValue(ifp);
            var mparams = (List<IVariable>)AccessTools.Field(typeof(Formula), "m_Params").GetValue(f);
            mparams[1] = variable; // 替换收藏源为自己的列表
            variable.GetResult<List<string>>().Add("0-0");

            var simage = (SetImage)(((If)ifp).playables[0]);
            imageVariable.variable = prefabs.GetComponent<VariableBehaviour>();
            AccessTools.Field(typeof(SetImage), "m_ImageSource").SetValue(simage, imageVariable);
            AccessTools.Field(typeof(SetImage), "m_Path").SetValue(simage, "custom");
            //AlbumCustom = prefabs;

            return prefabs;
            //musicAdded = true;
        }
    }
}
