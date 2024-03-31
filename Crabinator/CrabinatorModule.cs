using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.Crabinator {
    public class CrabinatorModule : EverestModule {
        private Hook getTextureHook;

        public override void Load() {
            getTextureHook = new Hook(
                typeof(Atlas).GetMethod("get_Item"),
                typeof(CrabinatorModule).GetMethod("modTexture", BindingFlags.NonPublic | BindingFlags.Static));

            IL.Monocle.Atlas.GetAtlasSubtextureFromAtlasAt += hookGetAtlasSubtexture;
        }

        public override void Unload() {
            getTextureHook?.Dispose();
            getTextureHook = null;

            IL.Monocle.Atlas.GetAtlasSubtextureFromAtlasAt -= hookGetAtlasSubtexture;
        }

        private static MTexture modTexture(Func<Atlas, string, MTexture> orig, Atlas self, string id) {
            if (GFX.Gui != null && GFX.Gui.Textures.TryGetValue("Crabinator/crab", out MTexture crab)) {
                return crab;
            }

            return orig(self, id);
        }

        private delegate bool OutReplacer(bool orig, MTexture origTexture, out MTexture output);

        private static void hookGetAtlasSubtexture(ILContext context) {
            ILCursor cursor = new ILCursor(context);

            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Dictionary<string, MTexture>>("get_Item"));
            cursor.EmitDelegate<Func<MTexture, MTexture>>(orig => {
                if (GFX.Gui != null && GFX.Gui.Textures.TryGetValue("Crabinator/crab", out MTexture crab)) {
                    return crab;
                }
                return orig;
            });

            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Dictionary<string, MTexture>>("TryGetValue"));
            cursor.Emit(OpCodes.Ldloc_2);
            cursor.Emit(OpCodes.Ldloca_S, (byte) 2);
            cursor.EmitDelegate<OutReplacer>((bool orig, MTexture origTexture, out MTexture output) => {
                output = origTexture;

                // do nothing if the original texture does not exist
                if (!orig) return orig;

                // if it does exist, replace it with crab if it is loaded
                if (GFX.Gui != null && GFX.Gui.Textures.TryGetValue("Crabinator/crab", out MTexture crab)) {
                    output = crab;
                }
                return true;
            });
        }
    }
}