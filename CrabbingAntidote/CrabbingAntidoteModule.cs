using Celeste;
using Celeste.Mod;
using System;

namespace Celeste.Mod.CrabbingAntidote {
    public class CrabbingAntidoteModule : EverestModule {
        public override void Load() {
            On.Celeste.SaveData.AfterInitialize += onSaveDataAfterInitialize;
        }

        public override void Unload() {
            On.Celeste.SaveData.AfterInitialize -= onSaveDataAfterInitialize;
        }

        private void onSaveDataAfterInitialize(On.Celeste.SaveData.orig_AfterInitialize orig, SaveData self) {
            orig(self);

            bool saveFileWasCrabbed = false;

            foreach (AreaStats areaStats in self.Areas) {
                if (areaStats.Modes.Length < 3) {
                    AreaModeStats[] decrabbedStats = new AreaModeStats[3];

                    for (int i = 0; i < areaStats.Modes.Length; i++) {
                        decrabbedStats[i] = areaStats.Modes[i];
                    }
                    for (int i = areaStats.Modes.Length; i < 3; i++) {
                        Logger.Log(LogLevel.Warn, "CrabbingAntidote", "Chapter " + areaStats.ID + " and side " + (i + 1) + " of save file " + (self.FileSlot + 1) + " was decrabbed");
                        decrabbedStats[i] = new AreaModeStats();
                    }
                    areaStats.Modes = decrabbedStats;
                    saveFileWasCrabbed = true;
                }
            }

            if (saveFileWasCrabbed) {
                UserIO.Save<SaveData>(SaveData.GetFilename(self.FileSlot), UserIO.Serialize<SaveData>(self));
                Logger.Log(LogLevel.Warn, "CrabbingAntidote", "Decrabbed file " + (self.FileSlot + 1) + " was saved");
            }
        }
    }
}