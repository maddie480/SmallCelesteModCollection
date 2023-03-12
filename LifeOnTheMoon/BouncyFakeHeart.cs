using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.LifeOnTheMoon {
    [CustomEntity("LifeOnTheMoon/BouncyFakeHeart")]
    class BouncyFakeHeart : FakeHeart {
        public static void Load() {
            IL.Celeste.FakeHeart.OnPlayer += modFakeHeartOnPlayer;
        }

        public static void Unload() {
            IL.Celeste.FakeHeart.OnPlayer -= modFakeHeartOnPlayer;
        }

        private static void modFakeHeartOnPlayer(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_DashAttacking"))) {
                Logger.Log("LifeOnTheMoon/BouncyFakeHeart", $"Disabling bouncing on fake hearts at {cursor.Index} in IL for FakeHeart.OnPlayer");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<bool, FakeHeart, bool>>((orig, self) => {
                    if (self is BouncyFakeHeart) {
                        return false;
                    }
                    return orig;
                });
            }
        }

        private MoonGlitchBackgroundTrigger moonGlitch;

        public BouncyFakeHeart(EntityData data, Vector2 offset) : base(data, offset) {
            if (data.Bool("glitch", false)) {
                moonGlitch = new MoonGlitchBackgroundTrigger(new EntityData(), Vector2.Zero);
                moonGlitch.Collidable = false;

                Add(new PlayerCollider(glitchOnPlayer));
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (moonGlitch != null) {
                scene.Add(moonGlitch);
            }
        }

        private void glitchOnPlayer(Player player) {
            moonGlitch.Invoke();
            new DynData<MoonGlitchBackgroundTrigger>(moonGlitch)["triggered"] = false;
        }
    }
}
