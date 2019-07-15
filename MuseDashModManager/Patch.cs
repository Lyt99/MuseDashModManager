using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Harmony;

namespace MuseDashModManager
{
    class Patch
    {
        private static HarmonyInstance harmonyInstance = HarmonyInstance.Create("pw.baka.musedash.modmanager");

        public static void InstallPatches()
        {
            // CustomMap
            MethodInfo methodInitAlbumDatas = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "InitAlbumDatas");
            MethodInfo methodPathIADPostfix = AccessTools.Method(typeof(MuseDashModManager.Patches.CustomMap), "InitAlbumDatasPostfix");
            harmonyInstance.Patch(methodInitAlbumDatas, null, new HarmonyMethod(methodPathIADPostfix), null);

            MethodInfo methodRebuildChildren = AccessTools.Method(typeof(Assets.Scripts.PeroTools.Nice.Components.FancyScrollView), "RebuildChildren");
            MethodInfo methodRCPrefix = AccessTools.Method(typeof(MuseDashModManager.Patches.CustomMap), "FancyScrollAlbumRebuildChildrenPrefix");
            harmonyInstance.Patch(methodRebuildChildren, new HarmonyMethod(methodRCPrefix), null, null);

            MethodInfo methodIsCanPreparationOut = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "IsCanPreparationOut");
            MethodInfo methodICPOPrefix = AccessTools.Method(typeof(MuseDashModManager.Patches.CustomMap), "IsCanPreparationOutPrefix");
            harmonyInstance.Patch(methodIsCanPreparationOut, new HarmonyMethod(methodICPOPrefix), null, null);

            MethodInfo methodSetBgLockAction = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "SetBgLockAction");
            MethodInfo methodSBLAPrefix = AccessTools.Method(typeof(MuseDashModManager.Patches.CustomMap), "SetBgLockActionPrefix");
            harmonyInstance.Patch(methodSetBgLockAction, new HarmonyMethod(methodSBLAPrefix), null, null);

            MethodInfo methodOnBattleEnd = AccessTools.Method(typeof(Assets.Scripts.GameCore.Managers.StatisticsManager), "OnBattleEnd");
            MethodInfo methodOBEPrefix = AccessTools.Method(typeof(MuseDashModManager.Patches.CustomMap), "OnBattleEndPrefix");
            harmonyInstance.Patch(methodOnBattleEnd, new HarmonyMethod(methodOBEPrefix), null, null);

            //MethodInfo methodOnChangeMusic = AccessTools.Method(typeof(Assets.Scripts.UI.Panels.PnlStage), "ChangeMusic");
            //MethodInfo methodOCMPostfix = AccessTools.Method(typeof(MuseDashModManager.Patches.CustomMap), "ChangeMusicPostfix");
            //harmonyInstance.Patch(methodOnChangeMusic, null, new HarmonyMethod(methodOCMPostfix), null);
        }
    }
}
