using PPDConfiguration;
using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Sprites;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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

        internal static bool EnableEmbeddedSkin { get; set; } = true;
        private static readonly PPDEditorSkin skin = new();
        readonly string[] smallcolorimagepaths = new string[(int)ButtonType.Start];
        readonly PathObject[] defaultColorImagePaths = new PathObject[((int)ButtonType.Start + 4) * 2];
        readonly PathObject[] defaultImagePaths = new PathObject[((int)ButtonType.Start + 4) * 2];
        readonly PathObject[] colorimagepaths = new PathObject[((int)ButtonType.Start + 4) * 2];
        readonly PathObject[] imagepaths = new PathObject[((int)ButtonType.Start + 4) * 2];
        readonly PathObject[] traceimagepaths = new PathObject[(int)ButtonType.Start + 4];
        readonly PathObject longnotecirclepath;
        readonly float _innerradius;
        readonly float _outerradius;
        readonly PathObject axisimagepath;
        readonly PathObject defaultClockAxisImagePath;
        readonly PathObject clockaxisimagepath;
        readonly PathObject[] effectpaths = new PathObject[(int)MarkEvaluateType.Worst + 1];
        readonly PathObject appeareffectpath;
        readonly PathObject defaultHoldPath;
        readonly PathObject holdpath;
        readonly PathObject exholdpath;
        readonly float defaultHoldX;
        readonly float defaultHoldY;
        readonly float holdx;
        readonly float holdy;
        readonly float exholdx;
        readonly float exholdy;
        readonly bool hasCustomNoteSkin;

        private bool UseCustomSkinActive => hasCustomNoteSkin && PPDStaticSetting.UseCustomNoteSkin;

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
                var defaultText = sr.ReadToEnd();
                sr.Close();
                var defaultSetting = new SettingReader(defaultText);
                LoadNotePaths(defaultSetting, false, defaultColorImagePaths, defaultImagePaths, out defaultClockAxisImagePath, out defaultHoldPath, out defaultHoldX, out defaultHoldY, out _, out _, out _);

                for (ButtonType type = ButtonType.Square; type < ButtonType.Start; type++)
                {
                    traceimagepaths[(int)type] = Utility.Path.Combine(defaultSetting.ReadString("Trace" + type));
                }
                traceimagepaths[(int)ButtonType.Start] =
                    traceimagepaths[(int)ButtonType.Start + 1] =
                    traceimagepaths[(int)ButtonType.Start + 2] =
                    traceimagepaths[(int)ButtonType.Start + 3] = Utility.Path.Combine(defaultSetting["TraceSlider"]);
                longnotecirclepath = Utility.Path.Combine(defaultSetting.ReadString("LongNoteCirle"));
                _innerradius = float.Parse(defaultSetting.ReadString("InnerRadius"));
                _outerradius = float.Parse(defaultSetting.ReadString("OuterRadius"));
                axisimagepath = Utility.Path.Combine(defaultSetting.ReadString("CircleAxis"));
                for (MarkEvaluateType type = MarkEvaluateType.Cool; type <= MarkEvaluateType.Worst; type++)
                {
                    effectpaths[(int)type] = Utility.Path.Combine(defaultSetting.ReadString("Effect" + type));
                }
                appeareffectpath = Utility.Path.Combine(defaultSetting.ReadString("EffectAppear"));

                var customSetting = new SettingReader(defaultText);
                if (File.Exists("PPDeditor_Note_Skin.ini"))
                {
                    using var exSr = new StreamReader("PPDeditor_Note_Skin.ini");
                    var exSetting = new SettingReader(exSr.ReadToEnd());
                    if (exSetting.ReadBoolean("Enable"))
                    {
                        hasCustomNoteSkin = true;
                        foreach (KeyValuePair<string, string> kvp in exSetting.Dictionary)
                        {
                            customSetting.ReplaceOrAdd(kvp.Key, kvp.Value);
                        }
                    }
                }
                else if (EnableEmbeddedSkin && TryReadEmbeddedNoteSkinIni(out SettingReader embeddedSetting))
                {
                    if (embeddedSetting.ReadBoolean("Enable"))
                    {
                        hasCustomNoteSkin = true;
                        foreach (KeyValuePair<string, string> kvp in embeddedSetting.Dictionary)
                        {
                            customSetting.ReplaceOrAdd(kvp.Key, kvp.Value);
                        }
                    }
                }

                LoadNotePaths(customSetting, hasCustomNoteSkin, colorimagepaths, imagepaths, out clockaxisimagepath, out holdpath, out holdx, out holdy, out exholdpath, out exholdx, out exholdy);
            }
        }

        private static void LoadNotePaths(
            SettingReader setting,
            bool useNewNote,
            PathObject[] colorPaths,
            PathObject[] markPaths,
            out PathObject clockAxisPath,
            out PathObject holdImgPath,
            out float holdOffsetX,
            out float holdOffsetY,
            out PathObject multiHoldImgPath,
            out float multiHoldOffsetX,
            out float multiHoldOffsetY)
        {
            for (ButtonType type = ButtonType.Square; type < ButtonType.Start; type++)
            {
                colorPaths[(int)type] = Utility.Path.Combine(setting.ReadString("Color" + type));
                markPaths[(int)type] = Utility.Path.Combine(setting.ReadString(type.ToString()));
            }
            colorPaths[(int)ButtonType.Start] = Utility.Path.Combine(setting["ColorSliderRight"]);
            colorPaths[(int)ButtonType.Start + 1] = Utility.Path.Combine(setting["ColorSliderLeft"]);
            colorPaths[(int)ButtonType.Start + 2] = Utility.Path.Combine(setting["ColorSliderRightExtra"]);
            colorPaths[(int)ButtonType.Start + 3] = Utility.Path.Combine(setting["ColorSliderLeftExtra"]);
            markPaths[(int)ButtonType.Start] = Utility.Path.Combine(setting["SliderRight"]);
            markPaths[(int)ButtonType.Start + 1] = Utility.Path.Combine(setting["SliderLeft"]);
            markPaths[(int)ButtonType.Start + 2] = Utility.Path.Combine(setting["SliderRightExtra"]);
            markPaths[(int)ButtonType.Start + 3] = Utility.Path.Combine(setting["SliderLeftExtra"]);
            clockAxisPath = Utility.Path.Combine(setting.ReadString("ClockAxis"));
            holdImgPath = Utility.Path.Combine(setting.ReadString("Hold"));
            holdOffsetX = float.Parse(setting.ReadString("HoldX"));
            holdOffsetY = float.Parse(setting.ReadString("HoldY"));

            for (ButtonType type = ButtonType.Square; type < ButtonType.Start; type++)
            {
                colorPaths[(int)type + 14] = (useNewNote && setting.ContainsKey("MultiColor" + type))
                    ? Utility.Path.Combine(setting.ReadString("MultiColor" + type))
                    : colorPaths[(int)type];
                markPaths[(int)type + 14] = (useNewNote && setting.ContainsKey("Multi" + type.ToString()))
                    ? Utility.Path.Combine(setting.ReadString("Multi" + type.ToString()))
                    : markPaths[(int)type];
            }
            colorPaths[(int)ButtonType.Start + 14] = (useNewNote && setting.ContainsKey("MultiColorSliderRight"))
                ? Utility.Path.Combine(setting["MultiColorSliderRight"])
                : colorPaths[(int)ButtonType.Start];
            colorPaths[(int)ButtonType.Start + 1 + 14] = (useNewNote && setting.ContainsKey("MultiColorSliderLeft"))
                ? Utility.Path.Combine(setting["MultiColorSliderLeft"])
                : colorPaths[(int)ButtonType.Start + 1];
            colorPaths[(int)ButtonType.Start + 2 + 14] = (useNewNote && setting.ContainsKey("MultiColorSliderRightExtra"))
                ? Utility.Path.Combine(setting["MultiColorSliderRightExtra"])
                : colorPaths[(int)ButtonType.Start + 2];
            colorPaths[(int)ButtonType.Start + 3 + 14] = (useNewNote && setting.ContainsKey("MultiColorSliderLeftExtra"))
                ? Utility.Path.Combine(setting["MultiColorSliderLeftExtra"])
                : colorPaths[(int)ButtonType.Start + 3];
            markPaths[(int)ButtonType.Start + 14] = (useNewNote && setting.ContainsKey("MultiSliderRight"))
                ? Utility.Path.Combine(setting["MultiSliderRight"])
                : markPaths[(int)ButtonType.Start];
            markPaths[(int)ButtonType.Start + 1 + 14] = (useNewNote && setting.ContainsKey("MultiSliderLeft"))
                ? Utility.Path.Combine(setting["MultiSliderLeft"])
                : markPaths[(int)ButtonType.Start + 1];
            markPaths[(int)ButtonType.Start + 2 + 14] = (useNewNote && setting.ContainsKey("MultiSliderRightExtra"))
                ? Utility.Path.Combine(setting["MultiSliderRightExtra"])
                : markPaths[(int)ButtonType.Start + 2];
            markPaths[(int)ButtonType.Start + 3 + 14] = (useNewNote && setting.ContainsKey("MultiSliderLeftExtra"))
                ? Utility.Path.Combine(setting["MultiSliderLeftExtra"])
                : (useNewNote && setting.ContainsKey("MultiColorSliderLeftExtra"))
                    ? Utility.Path.Combine(setting["MultiColorSliderLeftExtra"])
                    : markPaths[(int)ButtonType.Start + 3];
            multiHoldImgPath = (useNewNote && setting.ContainsKey("MultiHold"))
                ? Utility.Path.Combine(setting.ReadString("MultiHold"))
                : holdImgPath;
            multiHoldOffsetX = (useNewNote && setting.ContainsKey("MultiHoldX"))
                ? float.Parse(setting.ReadString("MultiHoldX"))
                : holdOffsetX;
            multiHoldOffsetY = (useNewNote && setting.ContainsKey("MultiHoldY"))
                ? float.Parse(setting.ReadString("MultiHoldY"))
                : holdOffsetY;
        }

        private static bool TryReadEmbeddedNoteSkinIni(out SettingReader setting)
        {
            setting = null;
            using var iniStream = typeof(PPDEditorSkin).Assembly.GetManifestResourceStream("EmbeddedSkin/PPDeditor_Note_Skin.ini");
            if (iniStream == null)
            {
                return false;
            }
            using var reader = new StreamReader(iniStream);
            setting = new SettingReader(reader.ReadToEnd());
            return true;
        }

        public static void RegisterEmbeddedResources(PPDDevice device, SpriteResourceManager resourceManager)
        {
            var assembly = typeof(PPDEditorSkin).Assembly;
            const string prefix = "EmbeddedSkin/button_ex/";
            const string hiResSuffix = ".12_5.png";
            var variants = new Dictionary<string, (string Normal, string HiRes)>(StringComparer.OrdinalIgnoreCase);
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.StartsWith(prefix, StringComparison.Ordinal)
                    || !resourceName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var fileName = resourceName[prefix.Length..];
                if (fileName.EndsWith(hiResSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    var baseFile = fileName[..^hiResSuffix.Length] + ".png";
                    variants.TryGetValue(baseFile, out var entry);
                    variants[baseFile] = (entry.Normal, resourceName);
                }
                else
                {
                    variants.TryGetValue(fileName, out var entry);
                    variants[fileName] = (resourceName, entry.HiRes);
                }
            }

            bool preferHiRes = !PPDSetting.Setting.HighResolutionImageDisabled && device.Scale.X > 1f;
            foreach (var pair in variants)
            {
                string selectedResource;
                ImageScale scale;
                if (preferHiRes && pair.Value.HiRes != null)
                {
                    selectedResource = pair.Value.HiRes;
                    scale = new ImageScale(12, 5);
                }
                else if (pair.Value.Normal != null)
                {
                    selectedResource = pair.Value.Normal;
                    scale = new ImageScale(1, 1);
                }
                else
                {
                    selectedResource = pair.Value.HiRes;
                    scale = new ImageScale(12, 5);
                }

                using var stream = assembly.GetManifestResourceStream(selectedResource);
                if (stream == null)
                {
                    continue;
                }
                string key = Utility.Path.Combine(@"button_ex\" + pair.Key);
                resourceManager.AddImageFromStream(device, key, stream, scale);
            }
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
            return (UseCustomSkinActive ? colorimagepaths : defaultColorImagePaths)[(int)buttontype];
        }

        public PathObject GetMarkImagePath(ButtonType buttontype)
        {
            return (UseCustomSkinActive ? imagepaths : defaultImagePaths)[(int)buttontype];
        }

        public PathObject GetMarkColorImagePath(int buttontype)
        {
            return (UseCustomSkinActive ? colorimagepaths : defaultColorImagePaths)[buttontype];
        }

        public PathObject GetMarkImagePath(int buttontype)
        {
            return (UseCustomSkinActive ? imagepaths : defaultImagePaths)[buttontype];
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
            return UseCustomSkinActive ? clockaxisimagepath : defaultClockAxisImagePath;
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
            if (UseCustomSkinActive)
            {
                imagepath = holdpath;
                x = holdx;
                y = holdy;
            }
            else
            {
                imagepath = defaultHoldPath;
                x = defaultHoldX;
                y = defaultHoldY;
            }
        }

        public void GetMultiHoldInfo(out PathObject imagepath, out float x, out float y)
        {
            if (UseCustomSkinActive)
            {
                imagepath = exholdpath;
                x = exholdx;
                y = exholdy;
            }
            else
            {
                imagepath = defaultHoldPath;
                x = defaultHoldX;
                y = defaultHoldY;
            }
        }

        [GeneratedRegex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)")]
        private static partial Regex RgbRegex();
        [GeneratedRegex("(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)")]
        private static partial Regex ArgbRegex();
    }
}
