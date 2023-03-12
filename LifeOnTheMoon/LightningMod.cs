using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.LifeOnTheMoon {
    static class LightningMod {
        private static ILHook hookOnLightningBoltRender;
        private static ILHook hookOnLightningBoltRoutine;

        public static void Load() {
            hookOnLightningBoltRender = new ILHook(typeof(LightningRenderer).GetNestedType("Bolt", BindingFlags.NonPublic).GetMethod("Render"), modBoltColor);
            hookOnLightningBoltRoutine =
                new ILHook(typeof(LightningRenderer).GetNestedType("Bolt", BindingFlags.NonPublic).GetMethod("Run", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modBoltDelay);
            IL.Celeste.LightningRenderer.Render += modLightningRendererRender;

            On.Celeste.LightningRenderer.DrawSimpleLightning += drawSimpleLightningOutlined;
            On.Celeste.LightningRenderer.DrawFatLightning += drawFatLightningOutlined;
        }

        public static void Unload() {
            hookOnLightningBoltRender?.Dispose();
            hookOnLightningBoltRender = null;
            hookOnLightningBoltRoutine?.Dispose();
            hookOnLightningBoltRoutine = null;
            IL.Celeste.LightningRenderer.Render -= modLightningRendererRender;

            On.Celeste.LightningRenderer.DrawSimpleLightning -= drawSimpleLightningOutlined;
            On.Celeste.LightningRenderer.DrawFatLightning -= drawFatLightningOutlined;
        }

        private static void modBoltColor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld(typeof(LightningRenderer).GetNestedType("Bolt", BindingFlags.NonPublic), "color"))) {
                Logger.Log("LifeOnTheMoon/LightningMod", $"Modding lightning bolt color at {cursor.Index} in IL for LightningRenderer.Bolt.Render()");
                cursor.EmitDelegate<Func<Color, Color>>(orig => {
                    if (orig.R == 252) {
                        return Calc.HexToColor(LifeOnTheMoonModule.Session.LightningColorBig);
                    }
                    return Calc.HexToColor(LifeOnTheMoonModule.Session.LightningColorSmall);
                });
            }
        }

        private static void modBoltDelay(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchLdcR4(4f))) {
                Logger.Log("LifeOnTheMoon/LightningMod", $"Modding lightning bolt delay at {cursor.Index} in IL for LightningRenderer.Bolt.Run()");
                cursor.Next.Operand = 0.5f;
            }
            cursor.Index = 0;
            while (cursor.TryGotoNext(instr => instr.MatchLdcR4(8f))) {
                Logger.Log("LifeOnTheMoon/LightningMod", $"Modding lightning bolt delay at {cursor.Index} in IL for LightningRenderer.Bolt.Run()");
                cursor.Next.Operand = 1.5f;
            }
        }

        private static void modLightningRendererRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<LightningRenderer>("electricityColors"),
                instr => instr.OpCode == OpCodes.Ldloc_S,
                instr => instr.OpCode == OpCodes.Ldelem_Any)) {

                Logger.Log("LifeOnTheMoon/LightningMod", $"Modding lightning edge color at {cursor.Index} in IL for LightningRenderer.Render()");
                cursor.EmitDelegate<Func<Color, Color>>(orig => {
                    if (orig.R == 252) {
                        return Calc.HexToColor(LifeOnTheMoonModule.Session.LightningColorBig);
                    }
                    return Calc.HexToColor(LifeOnTheMoonModule.Session.LightningColorSmall);
                });
            }
        }

        private static void drawSimpleLightningOutlined(On.Celeste.LightningRenderer.orig_DrawSimpleLightning orig, ref int index, ref VertexPositionColor[] verts,
            uint seed, Vector2 pos, Vector2 a, Vector2 b, Color color, float thickness) {

            if (LifeOnTheMoonModule.Session.LightningHasOutline) {
                // draw white lightning that is 2px thicker, that will appear as an outline once the regular lightning is drawn on top.
                orig(ref index, ref verts, seed, pos, a, b, Color.White, thickness + 2);
            }

            orig(ref index, ref verts, seed, pos, a, b, color, thickness);
        }

        private static void drawFatLightningOutlined(On.Celeste.LightningRenderer.orig_DrawFatLightning orig, uint seed, Vector2 a, Vector2 b, float size, float gap, Color color) {

            if (LifeOnTheMoonModule.Session.LightningHasOutline) {
                // draw white lightning that is 2px thicker, that will appear as an outline once the regular lightning is drawn on top.
                orig(seed, a, b, size + 2, gap, Color.White);
            }

            orig(seed, a, b, size, gap, color);
        }
    }
}
