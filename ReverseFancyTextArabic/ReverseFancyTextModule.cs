using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.ReverseFancyText {
    public class ReverseFancyTextModule : EverestModule {
        private static MethodInfo memorialTextCountToNewline = typeof(MemorialText).GetMethod("CountToNewline", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly char[] arabicChars = new char[] {
            'ﺔ', 'ﻴ', 'ﺑ', 'ﺮ', 'ﻌ', 'ﻟ', 'ﺍ', 'ﻐ', 'ﻠ', '、', '。', '！', '」', '，', '？', 'ﻲ', 'ﺋ', 'ﺎ', 'ﻘ', 'ﺘ', 'ﻆ', 'ﻔ', 'ﺤ', 'ﺒ', 'ﻩ', 'ﺬ', 'ﻫ', 'ﻢ',
            'ﻋ', 'ﺪ', 'ﺗ', 'ﺡ', 'ﻼ', 'ﺻ', 'ﺇ', 'ﻖ', 'ﺴ', '٨', 'ﻮ', 'ﻜ', 'ﺕ', 'ﺩ', 'ﻹ', 'ﻥ', 'ﻤ', 'ﺼ', 'ﺝ', 'ﻭ', 'ﺧ', 'ﺭ', 'ﺯ', 'ﺃ', 'ﺰ', 'ﺀ', 'ﻃ', 'ﻞ', 'ﻂ', 'ﻧ', 'ﻨ',
            'ع', 'ﻓ', 'ﻻ', 'ﺿ', 'ﻚ', 'ﻣ', 'ﺲ', 'ر', 'ﺢ', 'ﺣ', 'ﻉ', 'ﺫ', 'ﻳ', 'ﻛ', 'ﺷ', 'ﺓ', 'ﺠ', 'ﻱ', 'ﻦ', 'ﺸ', 'ﻰ', 'ﻊ', 'ﺛ', 'ﺄ', 'ﺳ', 'ﻷ', 'ﺐ', 'ﻒ', 'ﻑ', 'ﻕ', 'ﺖ',
            'ﻗ', 'ﺚ', 'ﻝ', 'ﺟ', 'ﻺ', 'م', 'ا', 'ً', 'ﻵ', 'ﺌ', 'ﺨ', 'ُ', '؟', '،', 'ﻄ', 'ﺜ', 'ﻀ', 'ﻬ', 'ﻯ', 'ﺏ', 'ﺊ', 'ﺉ', 'ﺅ', 'ﺈ', 'ﺂ', 'ﺁ', 'ﺱ', 'ﺦ', 'ﺥ', 'ﺞ', 'ﺙ', 'ﻏ',
            'ﻎ', 'ﻍ', 'ﻅ', 'ﻇ', 'ﻁ', 'ﺽ', 'ﺹ', 'ﺵ', 'ﻪ', 'ﻡ', 'ﻙ', '٠', '٩', '٧', '٦', '٥', '٤', '٣', '٢', '١', 'ة', 'ﺾ', 'ﻈ', 'ﺺ', 'ﺶ', 'ﺆ', 'و', 'أ', 'ﻸ'
        };

        public override void Load() {
            On.Celeste.FancyText.Parse_string_int_int_float_Nullable1_Language += onFancyTextParse;
            On.Celeste.FancyText.Text.Draw += onFancyTextDraw;
            On.Celeste.MemorialText.Render += onMemorialTextRender;
        }

        public override void Unload() {
            On.Celeste.FancyText.Parse_string_int_int_float_Nullable1_Language -= onFancyTextParse;
            On.Celeste.FancyText.Text.Draw -= onFancyTextDraw;
            On.Celeste.MemorialText.Render -= onMemorialTextRender;
        }

        private static FancyText.Text onFancyTextParse(On.Celeste.FancyText.orig_Parse_string_int_int_float_Nullable1_Language orig, string text, int maxLineWidth, int linesPerPage, float startFade, Color? defaultColor, Language language) {
            FancyText.Text vanillaText = orig.Invoke(text, maxLineWidth, linesPerPage, startFade, defaultColor, language);
            if ((language ?? Dialog.Language).Label != "ﺔﻴﺑﺮﻌﻟﺍ ﺔﻐﻠﻟﺍ") {
                return vanillaText;
            }

            bool hasArabicChars = false;
            foreach (char c in arabicChars) {
                if (text.Contains(c.ToString() ?? "")) {
                    hasArabicChars = true;
                    break;
                }
            }
            if (!hasArabicChars) {
                return vanillaText;
            }

            List<FancyText.Node> allNodes = new List<FancyText.Node>();
            List<FancyText.Node> currentNodes = new List<FancyText.Node>();

            foreach (FancyText.Node node in vanillaText.Nodes) {
                if (node is FancyText.Char || node is FancyText.Wait) {
                    currentNodes.Insert(0, node);
                    continue;
                }

                int startIndex = -1;
                for (int index = currentNodes.Count - 1; index >= 0; index--) {
                    if (isPunctuation(currentNodes[index])) {
                        if (index == 0 || !isPunctuation(currentNodes[index - 1])) {
                            if (startIndex != -1) {
                                float minDelay = float.MaxValue;
                                float maxDelay = float.MinValue;
                                for (int j = index; j <= startIndex; j++) {
                                    minDelay = Math.Min(minDelay, (currentNodes[j] as FancyText.Char).Delay);
                                    maxDelay = Math.Max(maxDelay, (currentNodes[j] as FancyText.Char).Delay);
                                }
                                for (int k = index; k <= startIndex - 1; k++) {
                                    (currentNodes[k] as FancyText.Char).Delay = minDelay;
                                }
                                (currentNodes[startIndex] as FancyText.Char).Delay = maxDelay;
                                startIndex = -1;
                            }
                        } else if (startIndex == -1) {
                            startIndex = index;
                        }
                    }
                }
                allNodes.AddRange(currentNodes);
                currentNodes.Clear();
                allNodes.Add(node);
            }

            allNodes.AddRange(currentNodes);
            vanillaText.Nodes = allNodes;

            new DynData<FancyText.Text>(vanillaText)["backwards"] = true;
            return vanillaText;
        }

        private static bool isPunctuation(FancyText.Node node) {
            return (node as FancyText.Char)?.IsPunctuation ?? false;
        }

        private void onFancyTextDraw(On.Celeste.FancyText.Text.orig_Draw orig, FancyText.Text self, Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start, int end) {
            if (Dialog.Language.Label != "ﺔﻴﺑﺮﻌﻟﺍ ﺔﻐﻠﻟﺍ") {
                orig.Invoke(self, position, justify, scale, alpha, start, end);
                return;
            }
            DynData<FancyText.Text> selfData = new DynData<FancyText.Text>(self);
            if (!selfData.Data.ContainsKey("backwards") || !selfData.Get<bool>("backwards")) {
                orig.Invoke(self, position, justify, scale, alpha, start, end);
                return;
            }

            int endCapped = Math.Min(self.Nodes.Count, end);
            int maxLineWidth = 0;
            float maxScale = 0f;
            float totalHeight = 0f;

            PixelFontSize pixelFontSize = self.Font.Get(self.BaseSize);
            for (int i = start; i < self.Nodes.Count; i++) {
                if (self.Nodes[i] is FancyText.NewLine) {
                    if (maxScale == 0f) {
                        maxScale = 1f;
                    }
                    totalHeight += maxScale;
                    maxScale = 0f;
                } else if (self.Nodes[i] is FancyText.Char) {
                    maxLineWidth = Math.Max(maxLineWidth, (int) (self.Nodes[i] as FancyText.Char).LineWidth);
                    maxScale = Math.Max(maxScale, (self.Nodes[i] as FancyText.Char).Scale);
                } else if (self.Nodes[i] is FancyText.NewPage) {
                    break;
                }
            }
            totalHeight += maxScale;

            position -= justify * new Vector2(maxLineWidth, totalHeight * pixelFontSize.LineHeight) * scale;
            maxScale = 0f;
            for (int j = start; j < endCapped && !(self.Nodes[j] is FancyText.NewPage); j++) {
                if (self.Nodes[j] is FancyText.NewLine) {
                    if (maxScale == 0f) {
                        maxScale = 1f;
                    }
                    position.Y += pixelFontSize.LineHeight * maxScale * scale.Y;
                    maxScale = 0f;
                }
                if (self.Nodes[j] is FancyText.Char) {
                    FancyText.Char c = self.Nodes[j] as FancyText.Char;
                    c.Draw(self.Font, self.BaseSize, position + Vector2.UnitX * (maxLineWidth - c.LineWidth) * scale, scale, alpha);
                    maxScale = Math.Max(maxScale, c.Scale);
                }
            }
        }

        private void onMemorialTextRender(On.Celeste.MemorialText.orig_Render orig, MemorialText self) {
            if (Dialog.Language.Label != "ﺔﻴﺑﺮﻌﻟﺍ ﺔﻐﻠﻟﺍ") {
                orig.Invoke(self);
                return;
            }

            DynData<MemorialText> selfData = new DynData<MemorialText>(self);
            float index = selfData.Get<float>("index");
            float alpha = selfData.Get<float>("alpha");
            string message = selfData.Get<string>("message");
            float widestCharacter = selfData.Get<float>("widestCharacter");
            float timer = selfData.Get<float>("timer");

            bool hasArabicChars = false;
            foreach (char c in arabicChars) {
                if (message.Contains(c.ToString() ?? "")) {
                    hasArabicChars = true;
                    break;
                }
            }
            if (!hasArabicChars) {
                orig.Invoke(self);
            } else {
                if ((self.Scene as Level).FrozenOrPaused || (self.Scene as Level).Completed || index <= 0f || alpha <= 0f) {
                    return;
                }
                Camera camera = self.SceneAs<Level>().Camera;
                Vector2 positionOnScreen = new Vector2((self.Memorial.X - camera.X) * 6f, (self.Memorial.Y - camera.Y) * 6f - 350f - ActiveFont.LineHeight * 3.3f);
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode) {
                    positionOnScreen.X = 1920f - positionOnScreen.X;
                }
                float easedAlpha = Ease.CubeInOut(alpha);
                int cappedIndex = (int) Math.Min(message.Length, index);
                float linePosition = 64f * (1f - easedAlpha);
                int currentLineLength = (int) memorialTextCountToNewline.Invoke(self, new object[1] { 0 });
                int currentLineLengthMinusOne = currentLineLength - 1;
                int startOfCurrentLine = currentLineLengthMinusOne;
                for (int j = 0; j < cappedIndex; j++) {
                    if (startOfCurrentLine < 0 || message[startOfCurrentLine] == '\n') {
                        startOfCurrentLine += currentLineLength + 1;
                        currentLineLength = (int) memorialTextCountToNewline.Invoke(self, new object[1] { startOfCurrentLine + 1 });
                        startOfCurrentLine += currentLineLength;
                        currentLineLengthMinusOne = currentLineLength - 1;
                        linePosition += ActiveFont.LineHeight * 1.1f;
                        continue;
                    }

                    char c = message[startOfCurrentLine];
                    float horizontalOffset = (currentLineLengthMinusOne + 0.5f) * widestCharacter - currentLineLength * widestCharacter / 2f;
                    float verticalOffset = 0f;
                    float horizontalScale = 1f;

                    if (self.Dreamy && c != ' ' && c != '-' && c != '\n') {
                        c = message[(startOfCurrentLine + (int) (Math.Sin(timer * 2f + startOfCurrentLine / 8f) * 4.0) + message.Length) % message.Length];
                        verticalOffset = (float) Math.Sin(timer * 2f + startOfCurrentLine / 8f) * 8f;
                        horizontalScale = Math.Sin(timer * 4f + startOfCurrentLine / 16f) < 0.0 ? -1 : 1;
                    }
                    ActiveFont.Draw(c, positionOnScreen + new Vector2(horizontalOffset, linePosition + verticalOffset), new Vector2(0.5f, 1f), new Vector2(horizontalScale, 1f), Color.White * easedAlpha);
                    currentLineLengthMinusOne--;
                    startOfCurrentLine--;
                }
            }
        }
    }
}