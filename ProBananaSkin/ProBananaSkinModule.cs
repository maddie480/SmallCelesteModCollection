using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.ProBananaSkin {
    // This code adds a toggle and sets up colors for the Pro Banana skin.
    public class ProBananaSkinModule : EverestModule {
        public static ProBananaSkinModule Instance;
        public override Type SettingsType => typeof(ProBananaSkinSettings);
        public static ProBananaSkinSettings Settings => (ProBananaSkinSettings) Instance._Settings;

        // the dash colors
        private static Color ZeroDashColor = Calc.HexToColor("649B00");
        private static Color OneDashColor = Calc.HexToColor("FFE100");
        private static Color TwoDashColor = Calc.HexToColor("9B6A15");
        private static Color ThreeDashColor = Calc.HexToColor("593B08");
        private static Color FourDashColor = Calc.HexToColor("261805");
        private static Color FiveDashColor = Calc.HexToColor("020100");

        // recolored dash particles (A = 0-dash, B = 1-dash, C = 2-dash)
        private static ParticleType P_DashA;
        private static ParticleType P_DashB;
        private static ParticleType P_DashC;

        public ProBananaSkinModule() {
            Instance = this;
        }

        public override void Load() {
            On.Celeste.PlayerSprite.ctor += onPlayerSpriteConstructor;
            On.Celeste.Player.Render += onPlayerRender;
            On.Celeste.Player.GetCurrentTrailColor += onPlayerGetTrailColor;
            On.Celeste.Player.UpdateHair += onPlayerUpdateHair;
            On.Celeste.Player.DashUpdate += onPlayerDashUpdate;
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);

            // build the dash particles.
            P_DashA = new ParticleType(Player.P_DashA)
            {
                Color = ZeroDashColor,
                Color2 = Calc.HexToColor("89D300")
            };
            P_DashB = new ParticleType(Player.P_DashA)
            {
                Color = OneDashColor,
                Color2 = Calc.HexToColor("C4861B")
            };
            P_DashC = new ParticleType(Player.P_DashA)
            {
                Color = TwoDashColor,
                Color2 = Calc.HexToColor("B77D19")
            };
        }

        public override void Unload() {
            On.Celeste.PlayerSprite.ctor -= onPlayerSpriteConstructor;
            On.Celeste.Player.Render -= onPlayerRender;
            On.Celeste.Player.GetCurrentTrailColor -= onPlayerGetTrailColor;
            On.Celeste.Player.UpdateHair -= onPlayerUpdateHair;
            On.Celeste.Player.DashUpdate -= onPlayerDashUpdate;
        }

        private void onPlayerSpriteConstructor(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode) {
            orig(self, mode);

            if (Settings.Enabled && (mode == PlayerSpriteMode.Madeline || mode == PlayerSpriteMode.MadelineAsBadeline || mode == PlayerSpriteMode.MadelineNoBackpack)) {
                // replace the sprite with the Pro Banana sprite (defined in Graphics/Sprites.xml).
                GFX.SpriteBank.CreateOn(self, "probanana_player");
            }
        }

        private void onPlayerRender(On.Celeste.Player.orig_Render orig, Player self) {
            if (Settings.Enabled) {
                // we are going to apply a color grade to Maddy to indicate dash count (replacing yellow with another color).
                string id;
                if (self.Dashes == 0) {
                    id = "probanana_0dash";
                } else if (self.Dashes == 2) {
                    id = "probanana_2dash";
                } else if (self.Dashes == 3) {
                    id = "probanana_3dash";
                } else if (self.Dashes == 4) {
                    id = "probanana_4dash";
                } else if (self.Dashes > 4) {
                    id = "probanana_5dash";
                } else {
                    // no replacement to do, original sprites are the right color.
                    orig(self);
                    return;
                }

                // initialize color grade...
                Effect fxColorGrading = GFX.FxColorGrading;
                fxColorGrading.CurrentTechnique = fxColorGrading.Techniques["ColorGradeSingle"];
                Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[id].Texture.Texture_Safe;
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, fxColorGrading, (self.Scene as Level).GameplayRenderer.Camera.Matrix);

                // render Maddy...
                orig(self);

                // ... and reset rendering to stop using the color grade.
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
            } else {
                // skin disabled, just render Maddy
                orig(self);
            }
        }

        private Color onPlayerGetTrailColor(On.Celeste.Player.orig_GetCurrentTrailColor orig, Player self) {
            if (Settings.Enabled) {
                // replace trail colors with banana colors
                int dashCount = self.Dashes;
                if (dashCount == 0) {
                    return ZeroDashColor;
                } else if (dashCount == 1) {
                    return OneDashColor;
                } else if (dashCount == 2) {
                    return TwoDashColor;
                } else if (dashCount == 3) {
                    return ThreeDashColor;
                } else if (dashCount == 4) {
                    return FourDashColor;
                } else {
                    return FiveDashColor;
                }
            }

            // skin disabled, keep original colors
            return orig(self);
        }

        private void onPlayerUpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity) {
            orig(self, applyGravity);

            if (Settings.Enabled) {
                // change player hair color to match dash colors.
                // (hair is invisible, but that influences other things like the orbs when Maddy dies and respawns)
                int dashCount = self.Dashes;
                if (dashCount == 0) {
                    self.Hair.Color = ZeroDashColor;
                } else if (dashCount == 1) {
                    self.Hair.Color = OneDashColor;
                } else if (dashCount == 2) {
                    self.Hair.Color = TwoDashColor;
                } else if (dashCount == 3) {
                    self.Hair.Color = ThreeDashColor;
                } else if (dashCount == 4) {
                    self.Hair.Color = FourDashColor;
                } else {
                    self.Hair.Color = FiveDashColor;
                }
            }
        }

        private int onPlayerDashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self) {
            if (!Settings.Enabled) {
                // skin disabled: just run vanilla code
                return orig(self);
            }

            // back up vanilla particles
            ParticleType bakDashA = Player.P_DashA;
            ParticleType bakDashB = Player.P_DashB;
            ParticleType bakDashBadB = Player.P_DashBadB;

            // replace them with our recolored ones
            Player.P_DashA = P_DashA;
            Player.P_DashB = P_DashB;
            Player.P_DashBadB = P_DashB;

            // run vanilla code: if it emits particles, it will use our recolored ones.
            int result = orig(self);

            // restore vanilla particles
            Player.P_DashA = bakDashA;
            Player.P_DashB = bakDashB;
            Player.P_DashBadB = bakDashBadB;

            return result;
        }
    }
}
