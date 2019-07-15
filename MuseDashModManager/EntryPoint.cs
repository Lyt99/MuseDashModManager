using System;
using System.Collections.Generic;
using System.Text;

namespace MuseDashModManager
{
    public class EntryPoint
    {

        private static bool hasInited = false;

        public static void InitManager()
        {
            if (hasInited) return;
            hasInited = true;
            Patches.CustomMap.Init();
            // 安装 Patch
            Patch.InstallPatches();
        }

        public static object LoadCustomAssetProxy(string name)
        {
            switch (name)
            {
                case "AlbumCustom":
                    return Patches.CustomMap.CreateCustomMusicCells();
                case "custom_map2":
                    return Patches.CustomMap.compiledStageInfo;
            }

            return null;
        }
    }
}
