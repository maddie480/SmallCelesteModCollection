using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.LifeOnTheMoon {
    static class CustomGuiManager {
        private static readonly Dictionary<string, string> customAssetMap = new Dictionary<string, string> {
            { "collectables/skullBlue", "collectables/muntheory/lifeonthemoon/skullBlue" },
            { "collectables/skullRed", "collectables/muntheory/lifeonthemoon/skullBlue" },
            { "collectables/skullGold", "collectables/muntheory/lifeonthemoon/skullBlue" },
            { "collectables/cassette", "collectables/muntheory/lifeonthemoon/cassette" },
            { "collectables/strawberry", "collectables/muntheory/lifeonthemoon/strawberry" },
            { "lookout/summit", "lookout/muntheory/lifeonthemoon/summit" },
            { "menu/play", "menu/muntheory/lifeonthemoon/play" },
            { "menu/remix", "menu/muntheory/lifeonthemoon/remix" },
            { "menu/rmx2", "menu/muntheory/lifeonthemoon/rmx2" }
        };

        private static bool lastPickedLOTM = false;
        private static ILHook hookOnSummitHudRender;
        private static ILHook hookOnUnlockedBSideRender;

        private static bool isFileSelect = false;

        public static void Load() {
            IL.Celeste.DeathsCounter.SetMode += replaceAllReferencesToGuiForMap;
            IL.Celeste.StrawberriesCounter.Render += replaceAllReferencesToGuiForMap;
            IL.Celeste.OuiChapterPanel.Reset += replaceAllReferencesToGuiForMap;
            IL.Celeste.OuiChapterPanel.AddRemixButton += replaceAllReferencesToGuiForMap;

            On.Celeste.OuiFileSelect.Enter += onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave += onFileSelectLeave;

            hookOnSummitHudRender = new ILHook(typeof(Lookout).GetNestedType("Hud", BindingFlags.NonPublic).GetMethod("Render"), replaceAllReferencesToGuiForMap);
            hookOnUnlockedBSideRender = new ILHook(typeof(Cassette).GetNestedType("UnlockedBSide", BindingFlags.NonPublic).GetMethod("Render"), replaceAllReferencesToGuiForMap);

            On.Celeste.OuiChapterPanel.Reset += onOuiChapterPanelReset;
            On.Celeste.Overworld.End += onOverworldEnd;

            On.Celeste.Poem.ctor += onPoemConstructor;
        }

        public static void Unload() {
            IL.Celeste.DeathsCounter.SetMode -= replaceAllReferencesToGuiForMap;
            IL.Celeste.StrawberriesCounter.Render -= replaceAllReferencesToGuiForMap;
            IL.Celeste.OuiChapterPanel.Reset -= replaceAllReferencesToGuiForMap;
            IL.Celeste.OuiChapterPanel.AddRemixButton -= replaceAllReferencesToGuiForMap;

            On.Celeste.OuiFileSelect.Enter -= onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave -= onFileSelectLeave;

            hookOnSummitHudRender?.Dispose();
            hookOnSummitHudRender = null;
            hookOnUnlockedBSideRender?.Dispose();
            hookOnUnlockedBSideRender = null;

            On.Celeste.OuiChapterPanel.Reset -= onOuiChapterPanelReset;
            On.Celeste.Overworld.End -= onOverworldEnd;

            On.Celeste.Poem.ctor -= onPoemConstructor;
        }

        private static void replaceAllReferencesToGuiForMap(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldstr && customAssetMap.ContainsKey(instr.Operand as string))) {
                string replaceWith = customAssetMap[cursor.Prev.Operand as string];
                Logger.Log("LifeOnTheMoon/CustomGuiManager", $"Modding GUI icon {cursor.Prev.Operand} => {replaceWith} at {cursor.Index} in IL for {cursor.Method.Name}");
                cursor.EmitDelegate<Func<string, string>>(orig => {
                    if (SaveData.Instance?.LastArea_Safe.GetSID() == LifeOnTheMoonModule.Map && !isFileSelect) {
                        return replaceWith;
                    }
                    return orig;
                });
            }
        }

        private static IEnumerator onFileSelectEnter(On.Celeste.OuiFileSelect.orig_Enter orig, OuiFileSelect self, Oui from) {
            isFileSelect = true;

            return orig(self, from);
        }

        private static IEnumerator onFileSelectLeave(On.Celeste.OuiFileSelect.orig_Leave orig, OuiFileSelect self, Oui next) {
            IEnumerator origEnum = orig(self, next);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            isFileSelect = false;
        }

        private static void onOuiChapterPanelReset(On.Celeste.OuiChapterPanel.orig_Reset orig, OuiChapterPanel self) {
            if ((SaveData.Instance.LastArea_Safe.GetSID() == LifeOnTheMoonModule.Map) != lastPickedLOTM) {
                lastPickedLOTM = !lastPickedLOTM;
                self.Area = SaveData.Instance.LastArea_Safe;

                DynData<OuiChapterPanel> selfData = new DynData<OuiChapterPanel>(self);

                // rebuild the heart gem display.
                selfData.Get<HeartGemDisplay>("heart").RemoveSelf();
                HeartGemDisplay heart = new HeartGemDisplay(0, hasGem: false);
                selfData["heart"] = heart;
                self.Add(heart);

                if (lastPickedLOTM) {
                    // replace the hearts with our custom one.
                    for (int i = 0; i < 3; i++) {
                        Sprite heartSprite = LifeOnTheMoonModule.GuiSpriteBank.Create("customHeart");
                        heart.Sprites[i] = heartSprite;
                        heartSprite.Play("spin");
                    }
                }
            }

            orig(self);
        }

        private static void onPoemConstructor(On.Celeste.Poem.orig_ctor orig, Poem self, string text, int heartIndex, float heartAlpha) {
            orig(self, text, heartIndex, heartAlpha);

            if (SaveData.Instance.LastArea_Safe.GetSID() == LifeOnTheMoonModule.Map) {
                // replace heart with custom one
                LifeOnTheMoonModule.GuiSpriteBank.CreateOn(self.Heart, "customHeart");
                self.Heart.Play("spin");
                new DynData<Poem>(self)["Color"] = Color.LightGray;
            }
        }

        private static void onOverworldEnd(On.Celeste.Overworld.orig_End orig, Overworld self) {
            orig(self);

            lastPickedLOTM = false;
        }
    }
}
