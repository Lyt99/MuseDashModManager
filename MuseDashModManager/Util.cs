using Assets.Scripts.PeroTools.Nice.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuseDashModManager
{
    class Util
    {
        // TODO: 向ConfigManager和AudioManager以及AssetBundleManager中添加东西
        public static Sprite LoadSpirteFromFile(string path, int width, int height)
        {
            var fileStream = new FileStream(path, FileMode.Open);
            var tex = new UnityEngine.Texture2D(width, height);

            fileStream.Seek(0, SeekOrigin.Begin);

            byte[] binary = new byte[fileStream.Length]; //创建文件长度的buffer
            fileStream.Read(binary, 0, (int)fileStream.Length);

            fileStream.Close();

            ImageConversion.LoadImage(tex, binary);
            return Sprite.Create(tex, new UnityEngine.Rect(0, 0, tex.width, tex.height), new UnityEngine.Vector2(0.0f, 0.0f));
        }

        public static UnityEngine.AudioClip LoadAudioClipFromFile(string filename)
        {
            WWW www = new WWW("file://" + filename);
            while (!www.isDone) ;
            return www.GetAudioClip();
        }

        public static void AddAsset(string key, object value)
        {
            if (!Global.CustomAssetsList.ContainsKey(key))
            Global.CustomAssetsList.Add(key, value);
        }

        public static void AddAssetEx(string key, Global.GetObject value)
        {
            if (!Global.CustomAssetsListEx.ContainsKey(key))
                Global.CustomAssetsListEx.Add(key, value);
        }
    }
}
