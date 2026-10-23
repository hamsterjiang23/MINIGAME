using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public enum PixelDetail { High = 8, Medium = 4, Low = 2 }

    /// <summary>Local player preference, independent of authored looks and game-session saves.</summary>
    public static class PlayerGraphicsSettings
    {
        public const string PreferenceKey = "NORM.Graphics.PixelDetail.v1";
        public const PixelDetail DefaultDetail = PixelDetail.Medium;
        private static bool loaded;
        private static PixelDetail detail;
        public static PixelDetail Detail
        {
            get
            {
                if (!loaded) Reload();
                return detail;
            }
        }
        public static int PixelSize => (int)Detail;

        public static void SetDetail(PixelDetail value)
        {
            if (value != PixelDetail.High && value != PixelDetail.Medium && value != PixelDetail.Low)
                throw new ArgumentOutOfRangeException(nameof(value));
            detail = value; loaded = true;
            PlayerPrefs.SetInt(PreferenceKey, (int)value); PlayerPrefs.Save();
        }

        public static void Reload()
        {
            int saved = PlayerPrefs.GetInt(PreferenceKey, (int)DefaultDetail);
            detail = saved == 8 ? PixelDetail.High : saved == 4 ? PixelDetail.Medium
                : saved == 2 ? PixelDetail.Low : DefaultDetail;
            loaded = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => loaded = false;
    }
}
