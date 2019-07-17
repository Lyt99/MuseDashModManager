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
            CustomMaps.Patch.InstallPatch(harmonyInstance);
            
        }
    }
}
