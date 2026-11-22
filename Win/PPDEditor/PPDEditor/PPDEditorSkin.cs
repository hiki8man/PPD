using PPDConfiguration;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PPDEditor
{
    public partial class PPDEditorSkin
    {
        public enum BackGroundDisplayMode
        {
            right = 0,
            left = 1,
            center = 2,
            fill = 3
        }

        private static readonly PPDEditorSkin skin = new();
        internal static bool EnableEmbeddedSkin { get; set; } = true;
        readonly string[] smallcolorimagepaths = new string[(int)ButtonType.Start];
        readonly PathObject[] colorimagepaths = new PathObject[((int)ButtonType.Start + 4) * 2];
        readonly PathObject[] imagepaths = new PathObject[((int)ButtonType.Start + 4) * 2];
        readonly PathObject[] traceimagepaths = new PathObject[(int)ButtonType.Start + 4];
        readonly PathObject longnotecirclepath;
        readonly float _innerradius;
        readonly float _outerradius;
        readonly PathObject axisimagepath;
        readonly PathObject clockaxisimagepath;
        readonly PathObject[] effectpaths = new PathObject[(int)MarkEvaluateType.Worst + 1];
        readonly PathObject appeareffectpath;
        readonly PathObject holdpath;
        readonly PathObject exholdpath;
        readonly float holdx;
        readonly float holdy;
        readonly float exholdx;
        readonly float exholdy;

        public static PPDEditorSkin Skin
        {
            get
            {
                return skin;
            }
        }

        private PPDEditorSkin()
        {
            OverLayColor = Color.FromArgb(220, 255, 255, 255);
            FillColor = Color.FromArgb(255, 255, 255);
            BorderColor = Color.FromArgb(90, 90, 90);
            TransparentTextColor = Color.Black;
            TimeLineCurrentTimeColor = Color.Red;
            TimeLineHorizontalLineColor = Color.Gray;
            TimeLineSeekAreaColor = Color.LightGray;
            TimeLineSeekAreaBorderColor = Color.Gray;
            TimeLineVerticalLineColor1 = Color.DarkSlateGray;
            TimeLineVerticalLineColor2 = Color.Silver;
            TimeLineVerticalLineColor3 = Color.FromArgb(204, 204, 204);
            TimeLineVerticalLineColor4 = Color.Green;
            TimeLineVerticalLineColor5 = Color.LightGreen;
            TimeLineBackGroundColors = [];
            TimeLineHoldExtentColor = Color.Orange;
            TimeLineRowHeight = 26;
            TimeLineHeight = 22;
            var sb = new SolidBrush(Color.FromArgb(128, Color.Yellow));
            for (int i = 0; i < 10; i++)
            {
                TimeLineBackGroundColors.Add(i, sb);
            }
            DisplayMode = BackGroundDisplayMode.right;
            if (File.Exists("defaultskin.ini"))
            {
                var sr = new StreamReader("defaultskin.ini");
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                bool useNewNote = false;
                string embeddedSkinRoot = null;
                if (File.Exists("PPDeditor_Note_Skin.ini"))
                {
                    using var exSr = new StreamReader("PPDeditor_Note_Skin.ini");
                    var exSetting = new SettingReader(exSr.ReadToEnd());
                    if (exSetting.ReadBoolean("Enable"))
                    {
                        useNewNote = true;
                        foreach (KeyValuePair<string, string> kvp in exSetting.Dictionary)
                        {
                            setting.ReplaceOrAdd(kvp.Key, kvp.Value);
                        }
                    }
                }
                else if (EnableEmbeddedSkin && TryLoadEmbeddedNoteSkin(out SettingReader embeddedSetting, out embeddedSkinRoot))
                {
                    if (embeddedSetting.ReadBoolean("Enable"))
                    {
                        useNewNote = true;
                        foreach (KeyValuePair<string, string> kvp in embeddedSetting.Dictionary)
                        {
                            setting.ReplaceOrAdd(kvp.Key, kvp.Value);
                        }
                    }
                    else
                    {
                        embeddedSkinRoot = null;
                    }
                }

                PathObject ResolveSkinPath(string relativePath)
                {
                    if (embeddedSkinRoot != null
                        && (relativePath.StartsWith(@"button_ex\", StringComparison.OrdinalIgnoreCase)
                            || relativePath.StartsWith("button_ex/", StringComparison.OrdinalIgnoreCase)))
                    {
                        return PathObject.Absolute(Path.Combine(embeddedSkinRoot, relativePath));
                    }
                    return Utility.Path.Combine(relativePath);
                }

                for (ButtonType type = ButtonType.Square; type < ButtonType.Start; type++)
                {
                    colorimagepaths[(int)type] = ResolveSkinPath(setting.ReadString("Color" + type));
                    imagepaths[(int)type] = ResolveSkinPath(setting.ReadString(type.ToString()));
                    traceimagepaths[(int)type] = ResolveSkinPath(setting.ReadString("Trace" + type));
                }
                colorimagepaths[(int)ButtonType.Start] = ResolveSkinPath(setting["ColorSliderRight"]);
                colorimagepaths[(int)ButtonType.Start + 1] = ResolveSkinPath(setting["ColorSliderLeft"]);
                colorimagepaths[(int)ButtonType.Start + 2] = ResolveSkinPath(setting["ColorSliderRightExtra"]);
                colorimagepaths[(int)ButtonType.Start + 3] = ResolveSkinPath(setting["ColorSliderLeftExtra"]);
                imagepaths[(int)ButtonType.Start] = ResolveSkinPath(setting["SliderRight"]);
                imagepaths[(int)ButtonType.Start + 1] = ResolveSkinPath(setting["SliderLeft"]);
                imagepaths[(int)ButtonType.Start + 2] = ResolveSkinPath(setting["SliderRightExtra"]);
                imagepaths[(int)ButtonType.Start + 3] = ResolveSkinPath(setting["SliderLeftExtra"]);
                traceimagepaths[(int)ButtonType.Start] =
                    traceimagepaths[(int)ButtonType.Start + 1] =
                    traceimagepaths[(int)ButtonType.Start + 2] =
                    traceimagepaths[(int)ButtonType.Start + 3] = ResolveSkinPath(setting["TraceSlider"]);
                longnotecirclepath = ResolveSkinPath(setting.ReadString("LongNoteCirle"));
                _innerradius = float.Parse(setting.ReadString("InnerRadius"));
                _outerradius = float.Parse(setting.ReadString("OuterRadius"));
                axisimagepath = ResolveSkinPath(setting.ReadString("CircleAxis"));
                clockaxisimagepath = ResolveSkinPath(setting.ReadString("ClockAxis"));
                for (MarkEvaluateType type = MarkEvaluateType.Cool; type <= MarkEvaluateType.Worst; type++)
                {
                    effectpaths[(int)type] = ResolveSkinPath(setting.ReadString("Effect" + type));
                }
                appeareffectpath = ResolveSkinPath(setting.ReadString("EffectAppear"));
                holdpath = ResolveSkinPath(setting.ReadString("Hold"));
                holdx = float.Parse(setting.ReadString("HoldX"));
                holdy = float.Parse(setting.ReadString("HoldY"));
                for (ButtonType type = ButtonType.Square; type < ButtonType.Start; type++)
                {
                    colorimagepaths[(int)type + 14] = (useNewNote && setting.ContainsKey("MultiColor" + type))
                        ? ResolveSkinPath(setting.ReadString("MultiColor" + type))
                        : colorimagepaths[(int)type];
                    imagepaths[(int)type + 14] = (useNewNote && setting.ContainsKey("Multi" + type.ToString()))
                        ? ResolveSkinPath(setting.ReadString("Multi" + type.ToString()))
                        : imagepaths[(int)type];
                }
                colorimagepaths[(int)ButtonType.Start + 14] = (useNewNote && setting.ContainsKey("MultiColorSliderRight"))
                    ? ResolveSkinPath(setting["MultiColorSliderRight"])
                    : colorimagepaths[(int)ButtonType.Start];
                colorimagepaths[(int)ButtonType.Start + 1 + 14] = (useNewNote && setting.ContainsKey("MultiColorSliderLeft"))
                    ? ResolveSkinPath(setting["MultiColorSliderLeft"])
                    : colorimagepaths[(int)ButtonType.Start + 1];
                colorimagepaths[(int)ButtonType.Start + 2 + 14] = (useNewNote && setting.ContainsKey("MultiColorSliderRightExtra"))
                    ? ResolveSkinPath(setting["MultiColorSliderRightExtra"])
                    : colorimagepaths[(int)ButtonType.Start + 2];
                colorimagepaths[(int)ButtonType.Start + 3 + 14] = (useNewNote && setting.ContainsKey("MultiColorSliderLeftExtra"))
                    ? ResolveSkinPath(setting["MultiColorSliderLeftExtra"])
                    : colorimagepaths[(int)ButtonType.Start + 3];
                imagepaths[(int)ButtonType.Start + 14] = (useNewNote && setting.ContainsKey("MultiSliderRight"))
                    ? ResolveSkinPath(setting["MultiSliderRight"])
                    : imagepaths[(int)ButtonType.Start];
                imagepaths[(int)ButtonType.Start + 1 + 14] = (useNewNote && setting.ContainsKey("MultiSliderLeft"))
                    ? ResolveSkinPath(setting["MultiSliderLeft"])
                    : imagepaths[(int)ButtonType.Start + 1];
                imagepaths[(int)ButtonType.Start + 2 + 14] = (useNewNote && setting.ContainsKey("MultiSliderRightExtra"))
                    ? ResolveSkinPath(setting["MultiSliderRightExtra"])
                    : imagepaths[(int)ButtonType.Start + 2];
                imagepaths[(int)ButtonType.Start + 3 + 14] = (useNewNote && setting.ContainsKey("MultiSliderLeftExtra"))
                    ? ResolveSkinPath(setting["MultiSliderLeftExtra"])
                    : (useNewNote && setting.ContainsKey("MultiColorSliderLeftExtra"))
                        ? ResolveSkinPath(setting["MultiColorSliderLeftExtra"])
                        : imagepaths[(int)ButtonType.Start + 3];
                exholdpath = (useNewNote && setting.ContainsKey("MultiHold"))
                    ? ResolveSkinPath(setting.ReadString("MultiHold"))
                    : holdpath;
                exholdx = (useNewNote && setting.ContainsKey("MultiHoldX"))
                    ? float.Parse(setting.ReadString("MultiHoldX"))
                    : holdx;
                exholdy = (useNewNote && setting.ContainsKey("MultiHoldY"))
                    ? float.Parse(setting.ReadString("MultiHoldY"))
                    : holdy;
            }
        }

        private static bool TryLoadEmbeddedNoteSkin(out SettingReader setting, out string skinRoot)
        {
            setting = null;
            skinRoot = null;
            var assembly = typeof(PPDEditorSkin).Assembly;
            using var iniStream = assembly.GetManifestResourceStream("EmbeddedSkin/PPDeditor_Note_Skin.ini");
            if (iniStream == null)
            {
                return false;
            }
            using (var reader = new StreamReader(iniStream))
            {
                setting = new SettingReader(reader.ReadToEnd());
            }

            skinRoot = Path.Combine(Path.GetTempPath(), "PPDEditor_Mod", "skin");
            var buttonExDir = Path.Combine(skinRoot, "button_ex");
            Directory.CreateDirectory(buttonExDir);
            const string prefix = "EmbeddedSkin/button_ex/";
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }
                var fileName = resourceName[prefix.Length..];
                var targetPath = Path.Combine(buttonExDir, fileName);
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    continue;
                }
                if (File.Exists(targetPath) && new FileInfo(targetPath).Length == stream.Length)
                {
                    continue;
                }
                using var fs = File.Create(targetPath);
                stream.CopyTo(fs);
            }
            return true;
        }

        public void Initialize(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    var rgb = RgbRegex();
                    var argb = ArgbRegex();
                    var sr = new StreamReader(filename);
                    var data = sr.ReadToEnd();
                    sr.Close();
                    var setting = new SettingReader(data);
                    var backfilename = setting.ReadString("BackGround");
                    if (File.Exists(backfilename))
                    {
                        BackGround = new Bitmap(backfilename);
                    }
                    var displaymode = int.Parse(setting.ReadString("DisplayMode"));
                    if (displaymode >= 0 && displaymode < 4)
                    {
                        DisplayMode = (BackGroundDisplayMode)displaymode;
                    }
                    var fillcolor = setting.ReadString("FillColor");
                    FillColor = GetColorFromString(rgb, argb, fillcolor);
                    var overraycolor = setting.ReadString("OverLayColor");
                    OverLayColor = GetColorFromString(rgb, argb, overraycolor);
                    var bordercolor = setting.ReadString("BorderColor");
                    BorderColor = GetColorFromString(rgb, argb, bordercolor);
                    var transparenttextcolor = setting.ReadString("TransparentTextColor");
                    TransparentTextColor = GetColorFromString(rgb, argb, transparenttextcolor);
                    var timelinetextcolor = setting.ReadString("TimeLineTextColor");
                    TimeLineTextColor = GetColorFromString(rgb, argb, timelinetextcolor);
                    var timelineareacolor = setting.ReadString("TimeLineSeekAreaColor");
                    TimeLineSeekAreaColor = GetColorFromString(rgb, argb, timelineareacolor);
                    var timelineseekareabordercolor = setting.ReadString("TimeLineSeekAreaBorderColor");
                    TimeLineSeekAreaBorderColor = GetColorFromString(rgb, argb, timelineseekareabordercolor);
                    var timelinehorizontallinecolor = setting.ReadString("TimeLineHorizontalLineColor");
                    TimeLineHorizontalLineColor = GetColorFromString(rgb, argb, timelinehorizontallinecolor);
                    var timelineverticallinecolor1 = setting.ReadString("TimeLineVerticalLineColor1");
                    TimeLineVerticalLineColor1 = GetColorFromString(rgb, argb, timelineverticallinecolor1);
                    var timelineverticallinecolor2 = setting.ReadString("TimeLineVerticalLineColor2");
                    TimeLineVerticalLineColor2 = GetColorFromString(rgb, argb, timelineverticallinecolor2);
                    var timelineverticallinecolor3 = setting.ReadString("TimeLineVerticalLineColor3");
                    TimeLineVerticalLineColor3 = GetColorFromString(rgb, argb, timelineverticallinecolor3);
                    var timelineverticallinecolor4 = setting.ReadString("TimeLineVerticalLineColor4");
                    TimeLineVerticalLineColor4 = GetColorFromString(rgb, argb, timelineverticallinecolor4);
                    var timelineverticallinecolor5 = setting.ReadString("TimeLineVerticalLineColor5");
                    TimeLineVerticalLineColor5 = GetColorFromString(rgb, argb, timelineverticallinecolor5);
                    var timelinecurrenttimecolor = setting.ReadString("TimeLineCurrentTimeColor");
                    TimeLineCurrentTimeColor = GetColorFromString(rgb, argb, timelinecurrenttimecolor);
                    var timelineselectionareacolor = setting.ReadString("TimeLineSelectAreaColor");
                    TimeLineSelectionAreaColor = GetColorFromString(rgb, argb, timelineselectionareacolor);
                    var timelineselectedmarkcolor = setting.ReadString("TimeLineSelectedMarkColor");
                    TimeLineSelectedMarkColor = GetColorFromString(rgb, argb, timelineselectedmarkcolor);
                    var timelinemarkcolor = setting.ReadString("TimeLineMarkColor");
                    TimeLineMarkColor = GetColorFromString(rgb, argb, timelinemarkcolor);
                    var timelineHoldExtentColor = setting.ReadString("TimeLineHoldExtentColor");
                    TimeLineHoldExtentColor = GetColorFromString(rgb, argb, timelineHoldExtentColor, TimeLineHoldExtentColor);
                    for (int i = 0; i < 10; i++)
                    {
                        TimeLineBackGroundColors[i].Dispose();
                    }
                    TimeLineBackGroundColors[0] = GetBrush(rgb, argb, setting, "TimeLineBackSquare", Color.White);
                    TimeLineBackGroundColors[1] = GetBrush(rgb, argb, setting, "TimeLineBackCross", Color.White);
                    TimeLineBackGroundColors[2] = GetBrush(rgb, argb, setting, "TimeLineBackCircle", Color.White);
                    TimeLineBackGroundColors[3] = GetBrush(rgb, argb, setting, "TimeLineBackTriangle", Color.White);
                    TimeLineBackGroundColors[4] = GetBrush(rgb, argb, setting, "TimeLineBackLeft", Color.White);
                    TimeLineBackGroundColors[5] = GetBrush(rgb, argb, setting, "TimeLineBackDown", Color.White);
                    TimeLineBackGroundColors[6] = GetBrush(rgb, argb, setting, "TimeLineBackRight", Color.White);
                    TimeLineBackGroundColors[7] = GetBrush(rgb, argb, setting, "TimeLineBackUp", Color.White);
                    TimeLineBackGroundColors[8] = GetBrush(rgb, argb, setting, "TimeLineBackR", Color.White);
                    TimeLineBackGroundColors[9] = GetBrush(rgb, argb, setting, "TimeLineBackL", Color.White);

                    smallcolorimagepaths[0] = setting.ReadString("TimeLineSquareImagePath");
                    smallcolorimagepaths[1] = setting.ReadString("TimeLineCrossImagePath");
                    smallcolorimagepaths[2] = setting.ReadString("TimeLineCircleImagePath");
                    smallcolorimagepaths[3] = setting.ReadString("TimeLineTriangleImagePath");
                    smallcolorimagepaths[4] = setting.ReadString("TimeLineLeftImagePath");
                    smallcolorimagepaths[5] = setting.ReadString("TimeLineDownImagePath");
                    smallcolorimagepaths[6] = setting.ReadString("TimeLineRightImagePath");
                    smallcolorimagepaths[7] = setting.ReadString("TimeLineUpImagePath");
                    smallcolorimagepaths[8] = setting.ReadString("TimeLineRImagePath");
                    smallcolorimagepaths[9] = setting.ReadString("TimeLineLImagePath");

                    if (int.TryParse(setting.ReadString("TimeLineHeight"), out int num))
                    {
                        TimeLineHeight = num;
                    }
                    if (int.TryParse(setting.ReadString("TimeLineRowHeight"), out num))
                    {
                        TimeLineRowHeight = num;
                    }

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }
        private static SolidBrush GetBrush(Regex rgb, Regex argb, SettingReader setting, string exp, Color ErrorColor)
        {
            return new SolidBrush(GetColor(rgb, argb, setting, exp, ErrorColor));
        }
        private static Color GetColor(Regex rgb, Regex argb, SettingReader setting, string exp, Color ErrorColor)
        {
            var color = setting.ReadString(exp);
            return GetColorFromString(rgb, argb, color, ErrorColor);
        }
        private static Color GetColorFromString(Regex rgb, Regex argb, string data)
        {
            return GetColorFromString(rgb, argb, data, SystemColors.Control);
        }
        private static Color GetColorFromString(Regex rgb, Regex argb, string data, Color ErrorColor)
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
        public Bitmap BackGround
        {
            get;
            set;
        }
        public Color OverLayColor
        {
            get;
            set;
        }
        public Color FillColor
        {
            get;
            set;
        }
        public Color BorderColor
        {
            get;
            set;
        }
        public BackGroundDisplayMode DisplayMode
        {
            get;
            set;
        }
        public Color TransparentTextColor
        {
            get;
            set;
        }
        public Color TimeLineTextColor
        {
            get;
            set;
        }
        public Color TimeLineSeekAreaColor
        {
            get;
            set;
        }
        public Color TimeLineSeekAreaBorderColor
        {
            get;
            set;
        }
        public Color TimeLineHorizontalLineColor
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor1
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor2
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor3
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor4
        {
            get;
            set;
        }
        public Color TimeLineVerticalLineColor5
        {
            get;
            set;
        }
        public Color TimeLineCurrentTimeColor
        {
            get;
            set;
        }
        public Color TimeLineSelectionAreaColor
        {
            get;
            set;
        }
        public Color TimeLineSelectedMarkColor
        {
            get;
            set;
        }
        public Color TimeLineMarkColor
        {
            get;
            set;
        }
        public Dictionary<int, Brush> TimeLineBackGroundColors
        {
            get;
            set;
        }
        public Color TimeLineHoldExtentColor
        {
            get;
            set;
        }

        public int TimeLineHeight
        {
            get;
            set;
        }

        public int TimeLineRowHeight
        {
            get;
            set;
        }

        public PathObject GetMarkColorImagePath(ButtonType buttontype)
        {
            return colorimagepaths[(int)buttontype];
        }

        public PathObject GetMarkImagePath(ButtonType buttontype)
        {
            return imagepaths[(int)buttontype];
        }

        public PathObject GetMarkColorImagePath(int buttontype)
        {
            return colorimagepaths[buttontype];
        }

        public PathObject GetMarkImagePath(int buttontype)
        {
            return imagepaths[buttontype];
        }

        public PathObject GetTraceImagePath(ButtonType buttontype)
        {
            return traceimagepaths[(int)buttontype];
        }

        public string GetSmallMarkColorImagePath(ButtonType buttontype)
        {
            return smallcolorimagepaths[(int)buttontype];
        }

        public void GetLongNoteCircleInfo(out PathObject imagepath, out float innerradius, out float outerradius)
        {
            imagepath = longnotecirclepath;
            innerradius = _innerradius;
            outerradius = _outerradius;
        }

        public PathObject GetClockAxisImagePath()
        {
            return clockaxisimagepath;
        }

        public PathObject GetCircleAxisImagePath()
        {
            return axisimagepath;
        }

        public PathObject GetEvaluateEffectPath(MarkEvaluateType evatype)
        {
            return effectpaths[(int)evatype];
        }

        public PathObject GetAppearEffectPath()
        {
            return appeareffectpath;
        }

        public void GetHoldInfo(out PathObject imagepath, out float x, out float y)
        {
            imagepath = holdpath;
            x = holdx;
            y = holdy;
        }

        public void GetMultiHoldInfo(out PathObject imagepath, out float x, out float y)
        {
            imagepath = exholdpath;
            x = exholdx;
            y = exholdy;
        }

        [GeneratedRegex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)")]
        private static partial Regex RgbRegex();
        [GeneratedRegex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)")]
        private static partial Regex ArgbRegex();
    }
}
