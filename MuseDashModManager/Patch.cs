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
        }
    }
}
