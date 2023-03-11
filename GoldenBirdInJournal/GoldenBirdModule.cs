using System;
using On.Celeste;

namespace Celeste.Mod.GoldenBird {
    public class GoldenBirdModule : EverestModule {

        public override Type SettingsType => null;

        public override void Load() {
            On.Celeste.OuiJournalProgress.CompletionIcon += ModCompletionIcon;
        }

        public override void Unload() {
            On.Celeste.OuiJournalProgress.CompletionIcon -= ModCompletionIcon;
        }

        private string ModCompletionIcon(On.Celeste.OuiJournalProgress.orig_CompletionIcon orig, OuiJournalProgress self, AreaStats data) {
            if(!AreaData.Get(data.ID).CanFullClear && data.Modes[0].SingleRunCompleted) {
                foreach (EntityData strawberry in AreaData.Areas[data.ID].Mode[0].MapData.Strawberries) {
                    EntityID item = new EntityID(strawberry.Level.Name, strawberry.ID);
                    if (strawberry.Bool("moon", false) && data.Modes[0].Strawberries.Contains(item)) {
                        return "max480/goldenbird/goldenbird";
                    }
                }
            }
            return orig(self, data);
        }
    }
}
