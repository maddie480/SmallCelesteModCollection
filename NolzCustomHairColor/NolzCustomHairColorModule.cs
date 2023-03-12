using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.NolzCustomHairColor {
    public class NolzCustomHairColorModule : EverestModule {
        private static Color ZeroDashColor = Calc.HexToColor("121212");
        private static Color OneDashColor = Calc.HexToColor("EAEAEA");
        private static Color TwoDashColor = Calc.HexToColor("FFD700");

        private static ParticleType P_DashA;
        private static ParticleType P_DashB;

        public override void Load() {
            On.Celeste.Player.GetCurrentTrailColor += onPlayerGetTrailColor;
            On.Celeste.Player.UpdateHair += onPlayerUpdateHair;
            On.Celeste.Player.DashUpdate += onPlayerDashUpdate;
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);
            P_DashA = new ParticleType(Player.P_DashA) {
                Color = ZeroDashColor,
                Color2 = Calc.HexToColor("3F3F3F")
            };
            P_DashB = new ParticleType(Player.P_DashA) {
                Color = OneDashColor,
                Color2 = Calc.HexToColor("CECECE")
            };
        }

        public override void Unload() {
            On.Celeste.Player.GetCurrentTrailColor -= onPlayerGetTrailColor;
            On.Celeste.Player.UpdateHair -= onPlayerUpdateHair;
            On.Celeste.Player.DashUpdate -= onPlayerDashUpdate;
        }

        private Color onPlayerGetTrailColor(On.Celeste.Player.orig_GetCurrentTrailColor orig, Player self) {
            return self.Dashes switch {
                0 => ZeroDashColor,
                1 => OneDashColor,
                _ => TwoDashColor,
            };
        }

        private void onPlayerUpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity) {
            orig.Invoke(self, applyGravity);

            switch (self.Dashes) {
                case 0:
                    self.Hair.Color = ZeroDashColor;
                    break;
                case 1:
                    self.Hair.Color = OneDashColor;
                    break;
                default:
                    self.Hair.Color = TwoDashColor;
                    break;
            }
        }

        private int onPlayerDashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self) {
            ParticleType p_DashA = Player.P_DashA;
            ParticleType p_DashB = Player.P_DashB;
            ParticleType p_DashBadB = Player.P_DashBadB;
            Player.P_DashA = P_DashA;
            Player.P_DashB = P_DashB;
            Player.P_DashBadB = P_DashB;
            int result = orig.Invoke(self);
            Player.P_DashA = p_DashA;
            Player.P_DashB = p_DashB;
            Player.P_DashBadB = p_DashBadB;
            return result;
        }
    }
}