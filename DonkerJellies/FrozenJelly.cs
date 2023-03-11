using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;


namespace Celeste.Mod.DonkerJellies {
    [CustomEntity("DonkerJellies/FrozenJelly")]
    internal class FrozenJelly : Glider {
        public static void Load() {
            IL.Celeste.Glider.Update += cancelGravity;
            IL.Celeste.Player.NormalUpdate += slowDownVerticalMovement;
        }

        public static void Unload() {
            IL.Celeste.Glider.Update -= cancelGravity;
            IL.Celeste.Player.NormalUpdate -= slowDownVerticalMovement;
        }

        public FrozenJelly(EntityData data, Vector2 offset) : base(data, offset) {
            DonkerJelliesModule.RecreateJellySpritesByHand(this, data.Attr("spriteDirectory", "objects/glider"));
            if (data.Bool("glow")) {
                Add(new VertexLight(new Vector2(0f, -10f), Color.White, 1f, 16, 48));
                Add(new BloomPoint(new Vector2(0f, -10f), 0.5f, 16f));
            }
        }

        private static void cancelGravity(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(30f))) {
                Logger.Log("DonkerJellies/FrozenJelly", $"Changing terminal falling velocity of jelly at {cursor.Index} in IL for Glider.Update");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Glider, float>>((orig, self) => (self is FrozenJelly) ? 0f : orig);
            }
        }

        private static void slowDownVerticalMovement(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(120f) || instr.MatchLdcR4(-32f) || instr.MatchLdcR4(24f) || instr.MatchLdcR4(40f))) {
                Logger.Log("DonkerJellies/FrozenJelly", $"Changing terminal falling velocity of player at {cursor.Index} in IL for Player.NormalUpdate (constant = {cursor.Prev.Operand})");

                bool lastMatched = (float) cursor.Prev.Operand == 40f;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>((orig, self) => (self.Holding?.Entity is FrozenJelly) ? (orig / 2f) : orig);

                if (lastMatched) {
                    break;
                }
            }
        }
    }
}