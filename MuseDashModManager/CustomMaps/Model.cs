using Assets.Scripts.GameCore;
using Assets.Scripts.GameCore.Managers;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuseDashModManager.CustomMaps
{
    public class CustomMap
    {
        public string Name;
        public string Artist;
        public string Music;
        public string MusicDemo;
        public string BPM;
        public string Scene;
        public string UID;
        public AudioClip MusicClip = null;
        public AudioClip MusicDemoClip = null;
        public Sprite CoverSprite = null;
        public string[] Difficulty = new string[3];
        public StageInfo[] stages = new StageInfo[] { null, null, null };
        public string Cover
        {
            get => UID + "_cover";
        }

        public string Map
        {
            get => UID + "_map";
        }
  
    }


    public class CustomMapNameListVariable : IVariable
    {
        public object result { get => Global.CustomMapList; set => throw new NotImplementedException(); }
    }

    public class CustomMapMasterUnlocker : IVariable
    {
        public IVariable original;
        public CustomMapMasterUnlocker(IVariable original) : base() { this.original = original; }
        public object result
        {
            get
            {
                try
                {
                    var uid = Singleton<DataManager>.instance["Account"]["SelectedAlbumUid"].result;
                    if (uid != null && (string)uid == "custom")
                    {
                        return true;
                    }

                } catch {}

                try
                {
                    return original.result;
                }
                catch
                {
                    return null;
                }
                
            }
            set
            {
                original.result = value;
            }
        }
    }

    public class CustomImageVariable : IVariable
    {
        public IVariable variable;
        public object result
        {
            get
            {
                int i = this.variable.GetResult<int>();
                return Global.CustomMapList[i].CoverSprite;
            }

            set { }
        }


    }

    public class CustomInfo
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

    public class BMSAndStage
    {
        public iBMSCManager.BMS BMS = null;
        public StageInfo StageInfo = null;
    }
}
