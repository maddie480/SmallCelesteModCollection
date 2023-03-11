using System;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.AroxonBerry {
    public class AroxonBerryModule : EverestModule {
        [CustomEntity("AroxonBerry/AroxonBerry")]
        [RegisterStrawberry(false, true)]
        [Tracked]
        private class AroxonBerry : Strawberry {
            private Action flyAway;

            public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
                entityData.Name = "memorialTextController";
                AroxonBerry result = new AroxonBerry(entityData, offset, new EntityID(levelData.Name, entityData.ID));
                entityData.Name = "AroxonBerry/AroxonBerry";
                return result;
            }

            public AroxonBerry(EntityData data, Vector2 offset, EntityID id)
                : base(data, offset, id) {
                DashListener dashListener = Get<DashListener>();
                if (dashListener != null) {
                    Remove(dashListener);
                    flyAway = () => {
                        dashListener.OnDash(Vector2.Zero);
                    };
                }
            }

            public override void Added(Scene scene) {
                base.Added(scene);
                if (!SceneAs<Level>().Session.StartedFromBeginning || Session.MidairJumpedInSession) {
                    RemoveSelf();
                }
                Sprite sprite = Get<Sprite>();
                if (SaveData.Instance.CheckStrawberry(ID)) {
                    GFX.SpriteBank.CreateOn(sprite, "ghost_aroxon_berry");
                } else {
                    GFX.SpriteBank.CreateOn(sprite, "aroxon_berry");
                }
                sprite.Play("flap");
            }

            public void FlyAway() {
                flyAway?.Invoke();
            }
        }

        public class AroxonBerrySession : EverestModuleSession {
            public bool MidairJumpedInSession { get; set; }

            public bool MidairJumpedInScreen { get; set; }
        }

        private delegate float orig_canJump(object selfJumpCount, float initialJumpGraceTimer, Player self, bool canWallJumpRight, bool canWallJumpLeft);

        public static AroxonBerryModule Instance;

        private static Hook extendedVariantsHook;

        public override Type SessionType => typeof(AroxonBerrySession);

        public static AroxonBerrySession Session => (AroxonBerrySession) Instance._Session;

        public AroxonBerryModule() {
            Instance = this;
        }

        public override void Load() {
            On.Celeste.Session.UpdateLevelStartDashes += onUpdateLevelStartDashes;
            On.Celeste.Level.Reload += onLevelReload;
        }

        public override void Initialize() {
            base.Initialize();
            EverestModule everestModule = Everest.Modules.Where((EverestModule mod) => mod.Metadata?.Name == "ExtendedVariantMode").FirstOrDefault();
            if (everestModule != null) {
                extendedVariantsHook = new Hook(
                    everestModule.GetType().Assembly.GetType("ExtendedVariants.Variants.JumpCount").GetMethod("canJump", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(AroxonBerryModule).GetMethod("onExtendedVariantJumpCheck", BindingFlags.Static | BindingFlags.NonPublic));
            }
        }

        public override void Unload() {
            On.Celeste.Session.UpdateLevelStartDashes -= onUpdateLevelStartDashes;
            On.Celeste.Level.Reload -= onLevelReload;

            extendedVariantsHook?.Dispose();
            extendedVariantsHook = null;
        }

        private void onUpdateLevelStartDashes(On.Celeste.Session.orig_UpdateLevelStartDashes orig, Session self) {
            orig.Invoke(self);
            Session.MidairJumpedInSession = Session.MidairJumpedInScreen;
        }

        private void onLevelReload(On.Celeste.Level.orig_Reload orig, Level self) {
            if (!self.Completed) {
                Session.MidairJumpedInScreen = Session.MidairJumpedInSession;
            }
            orig.Invoke(self);
        }

        private static float onExtendedVariantJumpCheck(orig_canJump orig, object selfJumpCount, float initialJumpGraceTimer, Player self, bool canWallJumpRight, bool canWallJumpLeft) {
            float num = orig(selfJumpCount, initialJumpGraceTimer, self, canWallJumpLeft, canWallJumpRight);
            if (self.SceneAs<Level>() != null && !self.SceneAs<Level>().Session.GetFlag("allow_midair_jumping") && initialJumpGraceTimer != 1f && num == 1f) {
                Session.MidairJumpedInScreen = true;
                self.Scene.Tracker.GetEntity<AroxonBerry>()?.FlyAway();
            }
            return num;
        }
    }
}