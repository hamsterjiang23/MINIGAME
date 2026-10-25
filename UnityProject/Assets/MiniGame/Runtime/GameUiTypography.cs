using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>One face for world, pause, settings and ending UI; terminal glyphs use their own pixel atlas.</summary>
    public static class GameUiTypography
    {
        public const int ReferenceSize = 28;
        private static string family;
        public static string Family
        {
            get
            {
                if (family != null) return family;
                var installed = Font.GetOSInstalledFontNames();
                // The existing "继续检查" label uses the SimSun-first game font.
                foreach (var candidate in new[] { "SimSun", "宋体", "NSimSun", "新宋体", "Noto Serif CJK SC",
                    "Source Han Serif SC", "Source Han Serif CN", "Songti SC", "STSong", "Microsoft YaHei", "Arial" })
                {
                    var match = Array.Find(installed, name => string.Equals(name, candidate, StringComparison.OrdinalIgnoreCase));
                    if (match != null) return family = match;
                }
                return family = "Arial";
            }
        }

        // Each UI owner retains its existing font destruction lifecycle. Do not share a destroyable Font instance.
        public static Font CreateFont()
        {
            var font = Font.CreateDynamicFontFromOSFont(Family, ReferenceSize);
            font.name = "Game UI - " + Family;
            return font;
        }

        /// <summary>Clean regular-weight subtitles, independent of the room's serif labels and terminal atlas.</summary>
        public static Font CreateDialogueFont()
        {
            var subtitle = Font.CreateDynamicFontFromOSFont(new[] {
                "Microsoft YaHei", "微软雅黑", "Noto Sans CJK SC", "Source Han Sans SC", "PingFang SC", "Arial"
            }, ReferenceSize);
            subtitle.name = "Dialogue Sans";
            return subtitle;
        }
    }
}
