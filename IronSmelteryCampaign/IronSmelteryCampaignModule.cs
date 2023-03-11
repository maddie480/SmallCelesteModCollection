using System;
using IL.Celeste;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.IronSmelteryCampaign {
    public class IronSmelteryCampaignModule : EverestModule {
        private const string areaLevelSet = "IronSmeltery/IronSmelterysCampaign";

        public override void Load() {
            IL.Celeste.OuiChapterPanel.Render += modOuiChapterPanelRender;
            DreamMirrorChangeInventory.Load();
        }

        public override void Unload() {
            IL.Celeste.OuiChapterPanel.Render -= modOuiChapterPanelRender;
            DreamMirrorChangeInventory.Unload();
        }

        private void modOuiChapterPanelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, (Instruction instr) =>
                instr.MatchLdstr("areaselect/cardtop_golden")
                || instr.MatchLdstr("areaselect/card_golden")
                || instr.MatchLdstr("areaselect/card")
                || instr.MatchLdstr("areaselect/cardtop"))) {

                Logger.Log("IronSmelteryChapterCard", $"Modding chapter panel card at {cursor.Index} in IL for OuiChapterPanel.Render");
                cursor.EmitDelegate<Func<string, string>>((string orig) => {
                    if ((Engine.Scene as Overworld)?.GetUI<OuiChapterPanel>()?.Area.GetLevelSet() == areaLevelSet) {
                        return orig.Replace("areaselect/", "IronSmeltery/");
                    }

                    return orig;
                });
            }
        }
    }
}
