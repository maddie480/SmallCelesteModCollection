

// Celeste.Mod.DonkerJellies.redilG
using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.DonkerJellies;
using Celeste.Mod.Entities;
using IL.Celeste;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;


namespace Celeste.Mod.DonkerJellies {
    [CustomEntity("DonkerJellies/ReverseJelly")]
    internal class redilG : Glider {
        public static void Load() {
            IL.Celeste.Glider.Update += reverseYAxis;
            IL.Celeste.Player.NormalUpdate += reverseYAxisPlayer;
            IL.Celeste.Player.Throw += reverseYAxisThrow;
        }

        public static void Unload() {
            IL.Celeste.Glider.Update -= reverseYAxis;
            IL.Celeste.Player.NormalUpdate -= reverseYAxisPlayer;
            IL.Celeste.Player.Throw -= reverseYAxisThrow;
        }

        public redilG(EntityData data, Vector2 offset) : base(data, offset) {
            DonkerJelliesModule.RecreateJellySpritesByHand(this, data.Attr("spriteDirectory", "objects/glider"));
            if (data.Bool("glow")) {
                Add(new VertexLight(new Vector2(0f, -10f), Color.White, 1f, 16, 48));
                Add(new BloomPoint(new Vector2(0f, -10f), 0.5f, 16f));
            }
        }

        private static void reverseYAxis(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdsfld(typeof(Input), "GliderMoveY"),
                instr => instr.MatchLdfld<VirtualIntegerAxis>("Value"))) {

                Logger.Log("DonkerJellies/ReverseJelly", $"Reversing jelly Y axis at {cursor.Index} in IL for Glider.Update");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, Glider, int>>((orig, self) => (self is redilG) ? (-orig) : orig);
            }
        }

        private static void reverseYAxisPlayer(ILContext il) {
            ILCursor val = new ILCursor(il);
            while (val.TryGotoNext((MoveType) 2,
                instr => instr.MatchLdsfld(typeof(Input), "GliderMoveY"),
                instr => instr.MatchCall<VirtualIntegerAxis>("op_Implicit"))) {

                object operand = val.Prev.Operand;
                bool returnsFloat = (operand as MethodReference).ReturnType.FullName == "System.Single";
                Logger.Log("DonkerJellies/ReverseJelly", $"Reversing jelly Y axis at {val.Index} in IL for Player.NormalUpdate");

                val.Emit(OpCodes.Ldarg_0);
                if (returnsFloat) {
                    val.EmitDelegate<Func<float, Player, float>>((orig, self) => (self.Holding?.Entity is redilG) ? -orig : orig);
                } else {
                    val.EmitDelegate<Func<int, Player, int>>((orig, self) => (self.Holding?.Entity is redilG) ? -orig : orig);
                }
            }
        }

        private static void reverseYAxisThrow(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdsfld(typeof(Input), "MoveY"),
                instr => instr.MatchLdfld<VirtualIntegerAxis>("Value"))) {

                Logger.Log("DonkerJellies/ReverseJelly", $"Reversing jelly Y axis at {cursor.Index} in IL for Player.Throw");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, Player, int>>((orig, self) => (self.Holding?.Entity is redilG) ? -orig : orig);
            }

            cursor.Index = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => ILPatternMatchingExt.MatchLdfld<Player>(instr, "Facing"))) {
                Logger.Log("DonkerJellies/ReverseJelly", $"Reversing facing at {cursor.Index} in IL for Player.Throw");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Facings, Player, Facings>>((orig, self) => {
                    if (!(self.Holding?.Entity is redilG)) {
                        return orig;
                    }
                    return (orig != Facings.Right) ? Facings.Right : Facings.Left;
                });
            }
        }
    }
}