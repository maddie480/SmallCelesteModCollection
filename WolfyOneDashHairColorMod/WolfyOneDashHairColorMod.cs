using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using On.Celeste;

namespace Celeste.Mod.ShirbSleepTeleport {
    public class WolfyOneDashHairColorMod : EverestModule {
        private static Color OneDashColor = Calc.HexToColor("E3742A");
        private static ParticleType P_DashB;

        public override void Load() {
            On.Celeste.Player.GetCurrentTrailColor += onPlayerGetTrailColor;
            On.Celeste.Player.UpdateHair += onPlayerUpdateHair;
            On.Celeste.Player.DashUpdate += onPlayerDashUpdate;
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);
            P_DashB = new ParticleType(Player.P_DashA) {
                Color = OneDashColor,
                Color2 = Calc.HexToColor("9E4F1E")
            };
        }

        public override void Unload() {
            On.Celeste.Player.GetCurrentTrailColor -= onPlayerGetTrailColor;
            On.Celeste.Player.UpdateHair -= onPlayerUpdateHair;
            On.Celeste.Player.DashUpdate -= onPlayerDashUpdate;
        }

        private Color onPlayerGetTrailColor(On.Celeste.Player.orig_GetCurrentTrailColor orig, Player self) {
            if (self.Dashes == 1) {
                return OneDashColor;
            }
            return orig.Invoke(self);
        }

        private void onPlayerUpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity) {
            orig.Invoke(self, applyGravity);
            if (self.Dashes == 1) {
                self.Hair.Color = OneDashColor;
            }
        }

        private int onPlayerDashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self) {
            ParticleType p_DashB = Player.P_DashB;
            ParticleType p_DashBadB = Player.P_DashBadB;
            Player.P_DashB = P_DashB;
            Player.P_DashBadB = P_DashB;
            int result = orig.Invoke(self);
            Player.P_DashB = p_DashB;
            Player.P_DashBadB = p_DashBadB;
            return result;
        }
    }

}