using System;
using System.Collections;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.CabobBerrySwap {
    public class DonkerBerrySwapModule : EverestModule {
        private const string areaSID = "Donker19/Solaris/solaris";

        private static bool isFileSelect;

        public override void Load() {
            IL.Celeste.StrawberriesCounter.Render += modStrawberrySkin;
            IL.Celeste.OuiChapterPanel.DrawCheckpoint += modStrawberrySkin;
            On.Celeste.OuiFileSelect.Enter += onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave += onFileSelectLeave;
        }

        public override void Unload() {
            IL.Celeste.StrawberriesCounter.Render -= modStrawberrySkin;
            IL.Celeste.OuiChapterPanel.DrawCheckpoint -= modStrawberrySkin;
            On.Celeste.OuiFileSelect.Enter -= onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave -= onFileSelectLeave;
        }

        private static void modStrawberrySkin(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("collectables/strawberry"))) {
                Logger.Log("DonkerBerrySwap", $"Changing strawberry icon w/ custom one at {cursor.Index} in IL for {cursor.Method.FullName}");
                cursor.EmitDelegate<Func<string, string>>(orig => {
                    if (!isFileSelect
                        && ((Engine.Scene as Overworld)?.GetUI<OuiChapterPanel>()?.Area.GetSID() == areaSID)
                        && ((Engine.Scene as Level)?.Session?.Area.GetSID() == areaSID)) {

                        return "Donker19/Solaris/customberry";
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
    }
}