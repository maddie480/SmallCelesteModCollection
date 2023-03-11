using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Celeste.Mod.FullClearFlag {
    public class FullClearFlagModule : EverestModule {

        public override Type SettingsType => null;

        public override void Load() {
            IL.Celeste.OuiFileSelectSlot.Render += modOuiFileSelectRender;
        }

        public override void Unload() {
            IL.Celeste.OuiFileSelectSlot.Render -= modOuiFileSelectRender;
        }

        private void modOuiFileSelectRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while(cursor.TryGotoNext(instr => instr.MatchLdstr("flag"))) {
                Logger.Log("FullClearFlag", $"Injecting code to change flag graphic at {cursor.Index} in IL code for OuiFileSelectSlot.Render");

                // replace "flag" with a call to getClearStamp() defined below.
                cursor.Remove();
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<OuiFileSelectSlot, string>>(getClearStamp);
            }
        }

        private static string getClearStamp(OuiFileSelectSlot self) {
            foreach(AreaStats areaStats in self.SaveData.Areas) {
                AreaData area = AreaData.Areas[areaStats.ID];
                Logger.Log("ckc", $"save {self.FileSlot}: id = {areaStats.ID}, celeste = {area.GetLevelSet() == "Celeste"}, interlude = {area.Interlude}, canFullClear = {area.CanFullClear}, fullCleared = {areaStats.Modes[0].FullClear}");
                if (area.GetLevelSet() == "Celeste" && !area.Interlude && area.CanFullClear && !areaStats.Modes[0].FullClear) {
                    // the current area is part of Celeste, not an interlude, can be full cleared, and has not been.
                    return "flag";
                }
            }

            // we slipped through everything without encountering a non-full-cleared area => this file is full cleared.
            return "fullclearstamp_flag";
        }
    }
}
