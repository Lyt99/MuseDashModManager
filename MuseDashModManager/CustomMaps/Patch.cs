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
using GameLogic;
using Assets.Scripts.PeroTools.Nice.Datas;


namespace MuseDashModManager.CustomMaps
{
    class Patch
    {
        public static bool HasInitAlbumDatas;

        // 安装Patch
        public static void InstallPatch(HarmonyInstance harmonyInstance)
        {
            MethodInfo methodInitAlbumDatas = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "InitAlbumDatas");
            MethodInfo methodPathIADPostfix = AccessTools.Method(typeof(Patch), "InitAlbumDatasPostfix");
            harmonyInstance.Patch(methodInitAlbumDatas, null, new HarmonyMethod(methodPathIADPostfix), null);

            MethodInfo methodRebuildChildren = AccessTools.Method(typeof(Assets.Scripts.PeroTools.Nice.Components.FancyScrollView), "RebuildChildren");
            MethodInfo methodRCPrefix = AccessTools.Method(typeof(Patch), "FancyScrollAlbumRebuildChildrenPrefix");
            harmonyInstance.Patch(methodRebuildChildren, new HarmonyMethod(methodRCPrefix), null, null);

            MethodInfo methodIsCanPreparationOut = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "IsCanPreparationOut");
            MethodInfo methodICPOPrefix = AccessTools.Method(typeof(Patch), "IsCanPreparationOutPrefix");
            harmonyInstance.Patch(methodIsCanPreparationOut, new HarmonyMethod(methodICPOPrefix), null, null);

            MethodInfo methodSetBgLockAction = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "SetBgLockAction");
            MethodInfo methodSBLAPrefix = AccessTools.Method(typeof(Patch), "SetBgLockActionPrefix");
            harmonyInstance.Patch(methodSetBgLockAction, new HarmonyMethod(methodSBLAPrefix), null, null);

            MethodInfo methodOnBattleEnd = AccessTools.Method(typeof(Assets.Scripts.GameCore.Managers.StatisticsManager), "OnBattleEnd");
            MethodInfo methodOBEPrefix = AccessTools.Method(typeof(Patch), "OnBattleEndPrefix");
            harmonyInstance.Patch(methodOnBattleEnd, new HarmonyMethod(methodOBEPrefix), null, null);

            MethodInfo methodChangeMusic = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "ChangeMusic");
            MethodInfo methodCMPostfix = AccessTools.Method(typeof(Patch), "ChangeMusicPostfix");
            harmonyInstance.Patch(methodChangeMusic, null, new HarmonyMethod(methodCMPostfix), null);
        }

        public static void InitAlbumDatasPostfix(PnlStage __instance)
        {
            var dtype = typeof(PnlStage).GetNestedType("GetData", BindingFlags.NonPublic);

            // 反射获得m_AlbumDatas
            Type configManagerType = typeof(PnlStage);
            var field = configManagerType.GetField("m_AlbumDatas", BindingFlags.NonPublic | BindingFlags.Instance);
            var m_AlbumDatas =
                (object[])field.GetValue(__instance);

            if (HasInitAlbumDatas)
            {
                // 把第2个挪到最后去，别问我为什么
                for (int i = 2; i < m_AlbumDatas.Length - 1; ++i)
                {
                    object swap = m_AlbumDatas[i + 1];
                    m_AlbumDatas[i + 1] = m_AlbumDatas[i];
                    m_AlbumDatas[i] = swap;
                }
                return;
            }

            // 添加Album信息到m_AlbumDatas中
            HasInitAlbumDatas = true;

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


            // 添加Album信息到ConfigManager中
            var configManager = Singleton<ConfigManager>.instance;
            var mData = (Dictionary<string, JArray>)AccessTools.Field(typeof(ConfigManager), "m_Dictionary").GetValue(configManager);

            var @object = new JObject();
            @object.Add("uid", "custom");
            @object.Add("title", "Custom Maps");
            @object.Add("prefabsName", "AlbumCustom");
            @object.Add("price", "Free");
            @object.Add("jsonName", "custom");
            @object.Add("needPurchase", "false");
            @object.Add("free", true);

            mData["albums"].Add(@object);

            var title = new JObject();
            title.Add("title", "自定义谱面");
            configManager["albums"].Add(title);

            Console.WriteLine("[MuseDashModManager]自定义谱面菜单添加成功");
        }

        public static void FancyScrollAlbumRebuildChildrenPrefix(FancyScrollView __instance)
        {
            if (__instance.name == "FancyScrollAlbum")
            {
                // 创建自定义谱面的Album按键
                GameObject custom = GameObject.Instantiate(__instance.content.GetChild(0).gameObject);
                custom.name = "ImgAlbumCustom";

                RectTransform transform = (RectTransform)custom.transform;
                transform.name = "ImgAlbumCustom";
                var text = transform.GetChild(1).GetComponent<UnityEngine.UI.Text>();
                text.text = "自定义谱面";

                transform.GetComponents(typeof(UnityEngine.Object));
                //custom.transform.SetSiblingIndex(__instance.content.childCount);
                custom.transform.SetParent(__instance.content);

            }
        }

        public static bool IsCanPreparationOutPrefix(PnlStage __instance, ref bool __result)
        {
            // 解除自定义谱面的上锁状态
            if (__instance.GetSelectedMusicAlbumJsonName() == "custom")
            {
                __result = true;
                return false;
            }

            return true;
        }

        public static bool SetBgLockActionPrefix(PnlStage __instance)
        {
            // 解除自定义谱面的上锁背景
            if (__instance.GetSelectedMusicAlbumJsonName() == "custom")
            {
                __instance.bgAlbumLock.SetActive(false);
                __instance.txtBudgetIsBurning15.SetActive(false);
                __instance.txtBudgetIsBurning15.SetActive(false);
                __instance.bgAlbumFree.SetActive(false);
                __instance.txtNotPurchase.SetActive(false);

                return false;
            }

            return true;
        }

        public static bool OnBattleEndPrefix()
        {
            // 禁用自定义谱面的成绩上传
            if (Singleton<DataManager>.instance["Account"]["SelectedMusicUid"].GetResult<string>().StartsWith("custom_")) return false;
            return true;
        }

        public static void ChangeMusicPostfix(PnlStage __instance)
        {
            // 禁用掉收藏按钮
            if (__instance.GetSelectedMusicAlbumJsonName() == "custom")
            {
                //__instance.difficulty3Lock.SetActive(false);
                //__instance.difficulty3Master.SetActive(__instance.difficulty3.text != "0");

                __instance.tglLike.gameObject.SetActive(false);
                return;
            }

            __instance.tglLike.gameObject.SetActive(true);
        }
    }
}
