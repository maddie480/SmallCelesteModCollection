using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.LifeOnTheMoon {
    [CustomEntity("LifeOnTheMoon/CustomMoonCreature")]
    class CustomMoonCreature : MoonCreature {
        public static void Load() {
            On.Celeste.MoonCreature.ctor_EntityData_Vector2 += takeParametersIntoAccount;
            IL.Celeste.MoonCreature.ctor_Vector2 += modMoonCreatureConstructor;
        }

        public static void Unload() {
            On.Celeste.MoonCreature.ctor_EntityData_Vector2 -= takeParametersIntoAccount;
            IL.Celeste.MoonCreature.ctor_Vector2 -= modMoonCreatureConstructor;
        }

        public CustomMoonCreature(EntityData data, Vector2 offset) : base(data, offset) {
            if (!data.Bool("followPlayer", true)) {
                Remove(Components.Get<PlayerCollider>());
            }
        }

        private static void takeParametersIntoAccount(On.Celeste.MoonCreature.orig_ctor_EntityData_Vector2 orig, MoonCreature self, EntityData data, Vector2 offset) {
            if (self is CustomMoonCreature cmc) {
                DynData<CustomMoonCreature> selfData = new DynData<CustomMoonCreature>(cmc);
                selfData["centerColor"] = data.Attr("centerColor");
                selfData["tailColor"] = data.Attr("tailColor");
            }
            orig(self, data, offset);
        }

        private static void modMoonCreatureConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            int index = 0;
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("Lerp"))) {
                switch (index) {
                    case 0:
                        Logger.Log("LifeOnTheMoon/CustomMoonCreature", $"Modding moon creature center color at {cursor.Index} in IL for MoonCreature constructor");
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Func<Color, MoonCreature, Color>>((orig, self) => {
                            if (self is CustomMoonCreature cmc) {
                                return Calc.HexToColor(new DynData<CustomMoonCreature>(cmc).Get<string>("centerColor"));
                            }
                            return orig;
                        });
                        break;
                    case 1:
                        Logger.Log("LifeOnTheMoon/CustomMoonCreature", $"Modding moon creature tail color at {cursor.Index} in IL for MoonCreature constructor");
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Func<Color, MoonCreature, Color>>((orig, self) => {
                            if (self is CustomMoonCreature cmc) {
                                return Calc.HexToColor(new DynData<CustomMoonCreature>(cmc).Get<string>("tailColor"));
                            }
                            return orig;
                        });
                        break;
                }
                index++;
            }
        }
    }
}
