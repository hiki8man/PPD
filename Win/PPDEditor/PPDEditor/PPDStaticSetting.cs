using PPDConfiguration;
using PPDEditor.Forms;
using PPDEditor.Models;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using WindowState = System.Windows.WindowState;

namespace PPDEditor
{
    partial class PPDStaticSetting
    {
        static readonly Regex rgb = RgbRegex();
        static readonly Regex argb = ArgbRegex();

        public static WindowState LastWindowState = WindowState.Normal;
        public static Size LastWindowSize = new(800, 600);
        public static Point LastWindowLocation = new(0, 0);
        public static string AuthorName = "";
        public static float MovieLatency;
        public static string langFileISO = "";
        public static bool HideToggleRectangle;
        public static bool HideToggleArrow;
        public static bool FixDockPanel;
        public static bool EnableToChangeMarkTypeAndTime;
        public static float MemoFontSize;
        public static bool ApplyToIniMovieTrimming;
        public static Color CustomCanvasBackColor;
        public static int CanvasSizeIndex;
        public static int CanvasColorIndex;
        public static int[] Moves = new int[8];
        public static int[] Angles = new int[8];
        public static EditorScriptMode ScriptMode = EditorScriptMode.Vet08H;
        public static bool RememberScriptMode;
        public static float Vet08MarkDistance = 300f;
        public static float Vet08Curvature = 20f;
        public static float Vet08CurvatureMulti;
        public static float Vet08MarkSpeed = 1f;
        public static float Vet08DecelerationRate = 0.72f;
        public static float Vet08HMarkDistance = 250f;
        public static float Vet08HCurvature = 17.36111111f;
        public static float Vet08HCurvatureMulti;
        public static float Vet08HMarkSpeed = 1f;
        public static float Vet08HDecelerationRate = 0.733333333f;
        public static float Frequency = 2f;
        public static float MarkDistanceRatio = 1f;
        public static float SubtractBPM;

        public static float MarkDistance => ScriptMode == EditorScriptMode.Vet08 ? Vet08MarkDistance : Vet08HMarkDistance;
        public static float Curvature => ScriptMode == EditorScriptMode.Vet08 ? Vet08Curvature : Vet08HCurvature;
        public static float CurvatureMulti => ScriptMode == EditorScriptMode.Vet08 ? Vet08CurvatureMulti : Vet08HCurvatureMulti;
        public static float MarkSpeed => ScriptMode == EditorScriptMode.Vet08 ? Vet08MarkSpeed : Vet08HMarkSpeed;
        public static float DecelerationRate => ScriptMode == EditorScriptMode.Vet08 ? Vet08DecelerationRate : Vet08HDecelerationRate;

        static PPDStaticSetting()
        {
            var path = Path.Combine(Utility.AppDir, Models.EditorSettingsStore.IniFileName);
            if (!File.Exists(path))
            {
                return;
            }
            Load(File.ReadAllText(path));
        }

        /// <summary>
        /// 設定のファイル（PPDEditor.ini）の内容を読み込みます。
        /// </summary>
        internal static void Load(string text)
        {
            var setting = new SettingReader(text);
            if (setting.ReadString("Maximised") == "1")
            {
                LastWindowState = WindowState.Maximized;
            }
            var size = (setting.ReadString("Size")).Split(',');
            if (size.Length == 2)
            {
                LastWindowSize = new Size(int.Parse(size[0], CultureInfo.InvariantCulture), int.Parse(size[1], CultureInfo.InvariantCulture));
            }
            else
            {
                LastWindowSize = new Size(800, 600);
            }
            var location = (setting.ReadString("Location")).Split(',');
            if (location.Length == 2)
            {
                LastWindowLocation = new Point(int.Parse(location[0], CultureInfo.InvariantCulture), int.Parse(location[1], CultureInfo.InvariantCulture));
            }
            else
            {
                LastWindowLocation = new Point(0, 0);
            }
            AuthorName = setting.ReadString("AuthorName");
            // 保存するときと同じく、言語の設定によらない形式で読む
            MovieLatency = ReadFloat(setting, "MovieLatency", 0);
            langFileISO = setting.ReadString("Language");
            if (langFileISO == "")
            {
                langFileISO = "jp";
            }
            HideToggleRectangle = ReadBoolean(setting, "HideToggleRectangle");
            HideToggleArrow = ReadBoolean(setting, "HideToggleArrow");
            FixDockPanel = ReadBoolean(setting, "FixDockPanel");
            EnableToChangeMarkTypeAndTime = ReadBoolean(setting, "EnableToChangeMarkTypeAndTime");
            ApplyToIniMovieTrimming = ReadBoolean(setting, "ApplyToIniMovieTrimming");
            CustomCanvasBackColor = GetColorFromString(setting.ReadString("CustomCanvasBackColor"), Color.White);
            CanvasSizeIndex = ReadInt(setting, "CanvasSizeIndex", 0);
            CanvasColorIndex = ReadInt(setting, "CanvasColorIndex", 0);
            MemoFontSize = ReadFloat(setting, "MemoFontSize", 10);
            Moves[0] = ReadInt(setting, String.Format("Move{0}", 0), 1);
            Moves[1] = ReadInt(setting, String.Format("Move{0}", 1), 10);
            Moves[2] = ReadInt(setting, String.Format("Move{0}", 2), 5);
            Angles[0] = ReadInt(setting, String.Format("Angle{0}", 0), 0);
            Angles[1] = ReadInt(setting, String.Format("Angle{0}", 1), 10);
            Angles[2] = ReadInt(setting, String.Format("Angle{0}", 2), 15);
            for (int i = 3; i < Moves.Length; i++)
            {
                Moves[i] = ReadInt(setting, String.Format("Move{0}", i), 1);
                Angles[i] = ReadInt(setting, String.Format("Angle{0}", i), 10);
            }
            if (Enum.TryParse(setting.ReadString("ScriptMode"), true, out EditorScriptMode parsedMode))
            {
                ScriptMode = parsedMode;
            }
            else
            {
                ScriptMode = EditorScriptMode.Vet08H;
            }
            RememberScriptMode = ReadBoolean(setting, "RememberScriptMode");
            Vet08MarkDistance = ReadFloat(setting, "Vet08_MarkDistance", 300f);
            Vet08Curvature = ReadFloat(setting, "Vet08_Curvature", 20f);
            Vet08CurvatureMulti = ReadFloat(setting, "Vet08_CurvatureMulti", 0f);
            Vet08MarkSpeed = ReadFloat(setting, "Vet08_MarkSpeed", 1f);
            Vet08DecelerationRate = ReadFloat(setting, "Vet08_DecelerationRate", 0.72f);
            Vet08HMarkDistance = ReadFloatWithFallback(setting, "Vet08H_MarkDistance", "MarkDistance", 250f);
            Vet08HCurvature = ReadFloatWithFallback(setting, "Vet08H_Curvature", "Curvature", 17.36111111f);
            Vet08HCurvatureMulti = ReadFloatWithFallback(setting, "Vet08H_CurvatureMulti", "CurvatureMulti", 0f);
            Vet08HMarkSpeed = ReadFloatWithFallback(setting, "Vet08H_MarkSpeed", "MarkSpeed", 1f);
            Vet08HDecelerationRate = ReadFloatWithFallback(setting, "Vet08H_DecelerationRate", "DecelerationRate", 0.733333333f);
            Frequency = ReadFloat(setting, "Frequency", 2f);
            MarkDistanceRatio = ReadFloat(setting, "MarkDistanceRatio", 1f);
            SubtractBPM = ReadFloat(setting, "SubtractBPM", 0f);
        }

        private static float ReadFloatWithFallback(SettingReader setting, string primaryKey, string fallbackKey, float error)
        {
            var val = setting.ReadString(primaryKey);
            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float ret))
            {
                return ret;
            }
            return ReadFloat(setting, fallbackKey, error);
        }

        private static int ReadInt(SettingReader setting, string key, int error)
        {
            var val = setting.ReadString(key);
            if (!int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ret))
            {
                return error;
            }

            return ret;
        }

        private static float ReadFloat(SettingReader setting, string key, float error)
        {
            var val = setting.ReadString(key);
            if (!float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float ret))
            {
                return error;
            }

            return ret;

        }

        private static bool ReadBoolean(SettingReader setting, string key)
        {
            return setting.ReadString(key) == "1";
        }

        private static Color GetColorFromString(string data, Color ErrorColor)
        {
            Color ret = ErrorColor;
            var m = argb.Match(data);
            if (!m.Success)
            {
                m = rgb.Match(data);
            }
            if (m.Success)
            {
                if (m.Groups.Count == 4)
                {
                    ret = Color.FromArgb(int.Parse(m.Groups[1].Captures[0].Value), int.Parse(m.Groups[2].Captures[0].Value), int.Parse(m.Groups[3].Captures[0].Value));
                }
                else if (m.Groups.Count == 5)
                {
                    ret = Color.FromArgb(int.Parse(m.Groups[1].Captures[0].Value), int.Parse(m.Groups[2].Captures[0].Value), int.Parse(m.Groups[3].Captures[0].Value), int.Parse(m.Groups[4].Captures[0].Value));
                }
            }
            return ret;
        }

        [GeneratedRegex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)")]
        private static partial Regex RgbRegex();
        [GeneratedRegex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)")]
        private static partial Regex ArgbRegex();
    }
}
