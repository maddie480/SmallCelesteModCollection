using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LifeOnTheMoon {
    public class LifeOnTheMoonModule : EverestModule {
        internal const string Map = "muntheory/lifeonthemoon/0";

        internal const string BoosterPrefix = "event:/muntheory_lifeonthemoon_booster_";
        internal const string BoosterEnd = "event:/muntheory_lifeonthemoon_booster_end";
        internal const string SecretKeySound = "event:/muntheory_lifeonthemoon_sessionkey";

        internal const string MuffleParameter = "muffle";
        internal const string InstantParameter = "instant";

        internal static LifeOnTheMoonModule Instance;

        public override Type SessionType => typeof(LifeOnTheMoonSession);
        internal static LifeOnTheMoonSession Session => (LifeOnTheMoonSession) Instance._Session;

        internal static SpriteBank SpriteBank;
        internal static SpriteBank GuiSpriteBank;

        private static bool active;

        public LifeOnTheMoonModule() => Instance = this;

        public override void Load() {
            CustomGuiManager.Load();

            On.Celeste.LevelLoader.ctor += OnLevelLoaderConstructor;
        }

        public override void Unload() {
            CustomGuiManager.Unload();

            On.Celeste.LevelLoader.ctor -= OnLevelLoaderConstructor;

            SpaceBooster.Unload();
            LightningMod.Unload();
            BouncyFakeHeart.Unload();
            ReskinnableBillboard.Unload();
            CustomMoonCreature.Unload();
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);

            SpriteBank = new SpriteBank(GFX.Game, "Graphics/muntheory/lifeonthemoon/CustomSprites.xml");
            GuiSpriteBank = new SpriteBank(GFX.Gui, "Graphics/muntheory/lifeonthemoon/CustomSpritesGui.xml");
        }

        private static void OnLevelLoaderConstructor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            if (session.Area.GetSID() == Map) {
                if (!active) {
                    Logger.Log("LifeOnTheMoon", "Loading Life On The Moon hooks");
                    active = true;

                    SpaceBooster.Load();
                    LightningMod.Load();
                    BouncyFakeHeart.Load();
                    ReskinnableBillboard.Load();
                    CustomMoonCreature.Load();
                }
            } else if (active) {
                Logger.Log("LifeOnTheMoon", "Unloading Life On The Moon hooks");
                active = false;

                SpaceBooster.Unload();
                LightningMod.Unload();
                BouncyFakeHeart.Unload();
                ReskinnableBillboard.Unload();
                CustomMoonCreature.Unload();
            }

            orig(self, session, startPosition);
        }
    }
}