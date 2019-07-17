using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuseDashModManager
{
    class Global
    {
        public delegate object GetObject();

        public static Dictionary<string, object> CustomAssetsList = new Dictionary<string, object>();
        public static Dictionary<string, GetObject> CustomAssetsListEx = new Dictionary<string, GetObject>();
    }
}
