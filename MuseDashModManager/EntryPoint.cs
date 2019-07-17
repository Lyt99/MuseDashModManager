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

            CustomMaps.Util.Init();
            // 安装 Patch
            Patch.InstallPatches();
        }

        public static object LoadCustomAssetProxy(string name)
        {
            if(name == null) {
                return null;
            }

            if(Global.CustomAssetsList.ContainsKey(name))
            {
                return Global.CustomAssetsList[name];
            }

            if(Global.CustomAssetsListEx.ContainsKey(name))
            {
                return Global.CustomAssetsListEx[name]();
            }

            return null;
        }
    }
}
