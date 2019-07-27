using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuseDashModManager.CustomMaps
{
    class Global
    {
        public static MusicConfigReader MusicConfigReader = (MusicConfigReader)Activator.CreateInstance(typeof(MusicConfigReader), true);

        public static Sprite SpriteNoCover = null;

        public static CustomMapNameListVariable CustomMapNameListVariable = new CustomMapNameListVariable();

        public static CustomImageVariable ImageVariable = new CustomImageVariable();

        public static string MapDirectory = "maps";

        public static List<CustomMap> CustomMapList = new List<CustomMap>();

        public static GameObject CustomAlbumCells;
    }
}
