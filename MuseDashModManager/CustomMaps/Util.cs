using Assets.Scripts.GameCore;
using Assets.Scripts.GameCore.Managers;
using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Actions;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Events;
using Assets.Scripts.PeroTools.Nice.Interface;
using Assets.Scripts.PeroTools.Nice.Variables;
using GameLogic;
using Harmony;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuseDashModManager.CustomMaps
{
    class Util
    {

        public static void Init()
        {
            Global.SpriteNoCover = LoadNoCoverSpirte();
            // 注册资源
            MuseDashModManager.Util.AddAssetEx("AlbumCustom", CreateCustomMusicCells);

            // 设置谱面存储位置
            AccessTools.Method(typeof(iBMSCManager), "set_bmsFile").Invoke(iBMSCManager.instance, new object[] { Path.Combine(Directory.GetCurrentDirectory(), Global.MapDirectory).Replace("\\", "/") });
            // 修改大触难度解锁条件，使自定义谱面全解锁
            var original = Singleton<DataManager>.instance["Account"]["IsSelectedUnlockMaster"];
            var newpatch = new CustomMapMasterUnlocker(original);
            Singleton<DataManager>.instance["Account"].Set("IsSelectedUnlockMaster", newpatch);

            // 加载谱面
            DirectoryInfo di = new DirectoryInfo(Global.MapDirectory);
            if (!di.Exists) di.Create();

            foreach(var i in di.GetDirectories())
            {
                CustomMap cm = LoadCustomMapFromDirectory(i);
                Global.CustomMapList.Add(cm);
            }

            // 添加谱面到游戏
            AddAllCustomMapToGame();
        }

        public static CustomMap LoadCustomMapFromDirectory(DirectoryInfo di)
        {
            CustomMap cm = new CustomMap();
            bool setInfo = false;

            for(int i = 0; i < 3; ++i)
            {
                string mapFileName = String.Format("{0}\\{1}_map{2}.bms", di.FullName, di.Name, i + 1);
                if(File.Exists(mapFileName))
                {
                    string rmapFileName = String.Format("{0}\\{0}_map{1}", di.Name, i + 1);
                    var inf = LoadAndCreateStageInfo(rmapFileName);
                    cm.stages[i] = inf.StageInfo;
                    cm.Difficulty[i] = inf.StageInfo.difficulty.ToString();
                    if(!setInfo)
                    {
                        setInfo = true;
                        cm.Name = inf.StageInfo.mapName;
                        cm.Artist = (string)inf.BMS.info["ARTIST"];
                        cm.Scene = inf.StageInfo.scene;
                        cm.BPM = inf.StageInfo.bpm.ToString();
                        cm.Music = (string)inf.BMS.info["WAV10"];
                    }
                }
            }

            string coverFileName = String.Format("{0}\\cover.png", di.FullName);
            cm.CoverSprite = File.Exists(coverFileName) ? MuseDashModManager.Util.LoadSpirteFromFile(coverFileName, 440, 440) : Global.SpriteNoCover;
            cm.MusicClip = MuseDashModManager.Util.LoadAudioClipFromFile(String.Format("{0}\\{1}", di.FullName, cm.Music));

            var n = cm.Music.Split('.');
            string demoMusicFileName = String.Format("{0}\\{1}_demo.{2}", di.FullName, n[0], n[1]);
            if (File.Exists(demoMusicFileName))
            {
                cm.MusicDemo = n[0];
                cm.MusicDemoClip = MuseDashModManager.Util.LoadAudioClipFromFile(demoMusicFileName);
            } else
            {
                cm.MusicDemo = cm.Music;
                cm.MusicDemoClip = cm.MusicClip;
            }

            cm.Music = n[0];

            // 分配一个UID，取文件夹名去掉空格
            string uid = String.Join("", di.Name.Split(' ', '_'));
            // UID重复怎么办？
            cm.UID = String.Format("custom_{0}", uid);
            return cm;
        }

        public static void AddAllCustomMapToGame()
        {
            // 添加到ConfigManager，以及添加资源
            foreach(var i in Global.CustomMapList)
            {
                AddCustomMapToGame(i);
            }
        }

        public static void AddCustomMapToGame(CustomMap cm)
        {
            AddCustomMapToConfigManager(cm);

            // 添加资源
            MuseDashModManager.Util.AddAsset(cm.Cover, cm.CoverSprite);
            MuseDashModManager.Util.AddAsset(cm.Music, cm.MusicClip);
            if(cm.Music != cm.MusicDemo)
                MuseDashModManager.Util.AddAsset(cm.MusicDemo, cm.MusicDemoClip);

            for(int i = 0; i < 3; ++i)
            {
                if(cm.stages[i] != null)
                {
                    string map = String.Format("{0}{1}", cm.Map, i + 1);
                    MuseDashModManager.Util.AddAsset(map, cm.stages[i]);
                }
                
            }
        }

        public static bool HasInitCustomJArray = false;

        public static Sprite LoadNoCoverSpirte()
        {
            var path = Application.streamingAssetsPath + "/nocover.png";
            return MuseDashModManager.Util.LoadSpirteFromFile(path, 440, 440);
        }

        public static void AddCustomMapToConfigManager(CustomMap cm)
        {
            var configManager = Singleton<ConfigManager>.instance;
            var mData = (Dictionary<string, JArray>)AccessTools.Field(typeof(ConfigManager), "m_Dictionary").GetValue(configManager);

            // 添加Custom
            if (!HasInitCustomJArray)
            {
                mData["custom"] = new JArray();
                HasInitCustomJArray = true;
            }

            var @object = new JObject();
            @object.Add("uid", cm.UID);
            @object.Add("name", cm.Name);
            @object.Add("author", cm.Artist);
            @object.Add("bpm", cm.BPM);
            @object.Add("music", cm.Music);
            @object.Add("demo", cm.MusicDemo);
            @object.Add("cover", cm.Cover);
            @object.Add("noteJson", cm.Map);
            @object.Add("scene", cm.Scene);
            @object.Add("difficulty1", cm.stages[0] != null ? cm.stages[0].difficulty.ToString() : "0");
            @object.Add("difficulty2", cm.stages[1] != null ? cm.stages[1].difficulty.ToString() : "0");
            @object.Add("difficulty3", cm.stages[2] != null ? cm.stages[2].difficulty.ToString() : "0");
            @object.Add("unlockLevel", "0");



            mData["custom"].Add(@object);
        }


        public static UnityEngine.GameObject CreateCustomMusicCells()
        {
            if (Global.CustomAlbumCells == null || Global.CustomAlbumCells.Equals(null))
            {
                GameObject prefabs = Singleton<AssetBundleManager>.instance.LoadFromName<GameObject>("AlbumCollection");
                prefabs = GameObject.Instantiate(prefabs);
                prefabs.name = "albumCustom";
                var component = prefabs.GetComponent<OnFancyScrollViewCellUpdate>();
                var ifp = component.playables[0];

                var fexecute = AccessTools.Field(typeof(If), "m_IsExecute");
                Formula f = (Formula)fexecute.GetValue(ifp);
                var mparams = (List<IVariable>)AccessTools.Field(typeof(Formula), "m_Params").GetValue(f);
                mparams[1] = Global.CustomMapNameListVariable; // 替换收藏源为自己的列表

                var simage = (SetImage)(((If)ifp).playables[0]);
                Global.ImageVariable.variable = prefabs.GetComponent<VariableBehaviour>();
                AccessTools.Field(typeof(SetImage), "m_ImageSource").SetValue(simage, Global.ImageVariable);
                AccessTools.Field(typeof(SetImage), "m_Path").SetValue(simage, "custom");

                Global.CustomAlbumCells = prefabs;
            }

            var ret = GameObject.Instantiate(Global.CustomAlbumCells);
            ret.name.Replace("(Clone)", string.Empty);
            return ret;
        }

        public static BMSAndStage LoadAndCreateStageInfo(string filename) // 不需要.bms
        {
            /* 1.加载bms
             * 2.转换为MusicData
             * 3.创建StageInfo
             * */
            var bms = iBMSCManager.instance.Load(filename);

            if (bms == null)
            {
                return null;
            }

            Global.MusicConfigReader.ClearData();
            Global.MusicConfigReader.bms = bms;
            Global.MusicConfigReader.Init("");
 

            var info = from m in Global.MusicConfigReader.GetData().ToArray() select (MusicData)m;

            StageInfo stgInfo = new StageInfo
            {
                musicDatas = info,
                delay = Global.MusicConfigReader.delay,
                mapName = (string)Global.MusicConfigReader.bms.info["TITLE"],
                music = ((string)Global.MusicConfigReader.bms.info["WAV10"]).BeginBefore('.'),
                scene = (string)Global.MusicConfigReader.bms.info["GENRE"],
                difficulty = int.Parse((string)Global.MusicConfigReader.bms.info["RANK"]),
                bpm = Global.MusicConfigReader.bms.GetBpm(),
                md5 = Global.MusicConfigReader.bms.md5,
                sceneEvents = Global.MusicConfigReader.sceneEvents
            };


            return new BMSAndStage
            {
                StageInfo = stgInfo,
                BMS = bms
            };
        }
    }
}
