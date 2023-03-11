using System;
using System.Collections;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.CabobBerrySwap {
    public class CabobBerrySwapModule : EverestModule {
        private const string areaLevelSet = "Cabob/FrozenHeights";

        private static bool isFileSelect;

        public override void Load() {
            IL.Celeste.StrawberriesCounter.Render += modStrawberrySkin;
            IL.Celeste.OuiChapterPanel.Render += modStrawberrySkin;
            On.Celeste.OuiFileSelect.Enter += onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave += onFileSelectLeave;
            IL.Celeste.OuiChapterPanel.Render += modOuiChapterPanelRender;
        }

        public override void Unload() {
            IL.Celeste.StrawberriesCounter.Render -= modStrawberrySkin;
            IL.Celeste.OuiChapterPanel.Render -= modStrawberrySkin;
            On.Celeste.OuiFileSelect.Enter -= onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave -= onFileSelectLeave;
            IL.Celeste.OuiChapterPanel.Render -= modOuiChapterPanelRender;
        }

        private static void modStrawberrySkin(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("collectables/strawberry"))) {
                Logger.Log("CabobBerrySwap", $"Changing strawberry icon w/ custom one at {cursor.Index} in IL for {cursor.Method.FullName}");

                cursor.EmitDelegate<Func<string, string>>(orig => {
                    if (!isFileSelect
                        && ((Engine.Scene as Overworld)?.GetUI<OuiChapterPanel>()?.Area.GetLevelSet() == areaLevelSet
                        && ((Engine.Scene as Level)?.Session?.Area.GetLevelSet() == areaLevelSet))) {

                        return areaLevelSet + "/customberry";
                    }

                    return orig;
                });
            }
        }

        private static IEnumerator onFileSelectEnter(On.Celeste.OuiFileSelect.orig_Enter orig, OuiFileSelect self, Oui from) {
            isFileSelect = true;
            return orig.Invoke(self, from);
        }

        private static IEnumerator onFileSelectLeave(On.Celeste.OuiFileSelect.orig_Leave orig, OuiFileSelect self, Oui next) {
            IEnumerator origEnum = orig.Invoke(self, next);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }
            isFileSelect = false;
        }

        private void modOuiChapterPanelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("areaselect/cardtop_golden")
                || instr.MatchLdstr("areaselect/card_golden")
                || instr.MatchLdstr("areaselect/card")
                || instr.MatchLdstr("areaselect/cardtop"))) {

                Logger.Log("CabobBerrySwap", $"Modding chapter panel card at {cursor.Index} in IL for OuiChapterPanel.Render");
                cursor.EmitDelegate<Func<string, string>>(orig => {
                    if ((Engine.Scene as Overworld)?.GetUI<OuiChapterPanel>()?.Area.GetLevelSet() == areaLevelSet) {
                        return orig.Replace("areaselect/", areaLevelSet + "/");
                    }

                    return orig;
                });
            }
        }
    }
}