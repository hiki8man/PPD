using System;
using System.Collections.Generic;

namespace PPDEditor.Models
{
    /// <summary>
    /// Mod 新增界面的多语言支持（优先读取语言包 ini，缺项时按 PPDEditor.ini 的 Language 自动兜底中/英/日）。
    /// </summary>
    internal static class ModLocalization
    {
        private static readonly Dictionary<string, (string Zh, string En, string Jp)> Fallback = new(StringComparer.Ordinal)
        {
            ["ModSelectVersionTitle"] = ("选择版本", "Select Version", "バージョン選択"),
            ["ModSelectVersionPrompt"] = ("请选择版本：", "Please select a version:", "バージョンを選択してください："),
            ["ModRememberChoice"] = ("记住选择，下次直接启动", "Remember choice for next launch", "選択を記憶して次回から直接起動"),
            ["ModOk"] = ("确定", "OK", "OK"),
            ["ModCancel"] = ("取消", "Cancel", "キャンセル"),
            ["ModPreviewScriptMenu"] = ("预览脚本", "Preview Script", "プレビュースクリプト"),
            ["ModUseCustomNoteSkin"] = ("使用自定义 Note 皮肤", "Use Custom Note Skin", "カスタムノーツスキンを使用")
        };

        public static string Get(string key)
        {
            try
            {
                var fromFile = Utility.Language?[key];
                if (!string.IsNullOrEmpty(fromFile))
                {
                    return fromFile;
                }
            }
            catch
            {
            }

            if (!Fallback.TryGetValue(key, out var texts))
            {
                return key;
            }

            var iso = PPDStaticSetting.langFileISO ?? "";
            if (iso.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            {
                return texts.Zh;
            }
            if (iso.StartsWith("jp", StringComparison.OrdinalIgnoreCase)
                || iso.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
            {
                return texts.Jp;
            }
            return texts.En;
        }
    }
}
