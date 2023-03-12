using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.DonkerCassetteSongSwap {
    public class DonkerCassetteSongSwapModule : EverestModule {

        private const string areaSID = "Donker19/Forsaken Map/1forsaken";

        public override void Load() {
            On.Celeste.AreaDataExt.SetASideAreaDataBackup += onSetASideAreaDataBackup;
            On.Celeste.Mod.Meta.MapMeta.ApplyToForOverride += onApplyToForOverride;
            On.Celeste.AreaDataExt.RestoreASideAreaData += onRestoreASideAreaData;

            Everest.Events.Level.OnEnter += onLevelEnter;
        }

        public override void Unload() {
            On.Celeste.AreaDataExt.SetASideAreaDataBackup -= onSetASideAreaDataBackup;
            On.Celeste.Mod.Meta.MapMeta.ApplyToForOverride -= onApplyToForOverride;
            On.Celeste.AreaDataExt.RestoreASideAreaData -= onRestoreASideAreaData;

            Everest.Events.Level.OnEnter -= onLevelEnter;
        }

        private AreaData onSetASideAreaDataBackup(On.Celeste.AreaDataExt.orig_SetASideAreaDataBackup orig, AreaData self, AreaData value) {
            if (self.GetSID() == areaSID) {
                value.CassetteSong = self.CassetteSong;
            }
            return orig(self, value);
        }

        private void onApplyToForOverride(On.Celeste.Mod.Meta.MapMeta.orig_ApplyToForOverride orig, Meta.MapMeta self, AreaData area) {
            orig(self, area);

            if (area.GetSID() == areaSID) {
                if (!string.IsNullOrEmpty(self.CassetteSong)) {
                    area.CassetteSong = self.CassetteSong;
                }
            }
        }

        private void onRestoreASideAreaData(On.Celeste.AreaDataExt.orig_RestoreASideAreaData orig, AreaData self) {
            orig(self);

            if (self.GetSID() == areaSID) {
                AreaData aSideAreaDataBackup = self.GetASideAreaDataBackup();
                if (aSideAreaDataBackup != null) {
                    self.CassetteSong = aSideAreaDataBackup.CassetteSong;
                }
            }
        }

        private void onLevelEnter(Session session, bool fromSaveData) {
            if (fromSaveData && session?.Area.GetSID() == areaSID) {
                if (session.Area.Mode != AreaMode.Normal) {
                    AreaData.Get(session.Area).OverrideASideMeta(session.Area.Mode);
                }
            }
        }
    }
}
