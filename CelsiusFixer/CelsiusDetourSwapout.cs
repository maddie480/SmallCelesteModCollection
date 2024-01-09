using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.CelsiusFixer {
    public static class CelsiusDetourSwapout {
        private static Hook everestHook;
        private static ILHook celsiusLoadHook;
        private static ILHook celsiusUnloadHook;
        private static Hook celsiusTransitionRoutineHook;

        private static MethodInfo levelTransitionRoutineMethod = typeof(Level).GetMethod("TransitionRoutine", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Load() {
            everestHook = new Hook(typeof(Everest).GetMethod("Register"), typeof(CelsiusDetourSwapout).GetMethod("onRegister", BindingFlags.NonPublic | BindingFlags.Static));
            Logger.Log("CelsiusFixer", "Everest mod registering has been hooked!");
        }

        public static void Unload() {
            everestHook?.Dispose();
            everestHook = null;

            celsiusLoadHook?.Dispose();
            celsiusLoadHook = null;

            celsiusUnloadHook?.Dispose();
            celsiusUnloadHook = null;

            celsiusTransitionRoutineHook?.Dispose();
            celsiusTransitionRoutineHook = null;

            Logger.Log("CelsiusFixer", "Celsius load/unload methods have been UN-hooked!");
        }

        private static void onRegister(Action<EverestModule> orig, EverestModule module) {
            if (module.Metadata.Name != "Celsius") {
                orig(module);
                return;
            }

            // Celsius has a Detour that aims to redirect CelsiusHooks.LevelTransitionRoutine to Level.TransitionRoutine, with a stunt (static => non-static)
            // that makes it break under Everest Core's compatibility layer.
            // So patch the detour out, and hook the method to make it do a simple reflection call instead!
            Type celsiusHooks = module.GetType().Assembly.GetType("Celeste.Mod.CelsiusHelper.CelsiusHooks");
            celsiusLoadHook = new ILHook(celsiusHooks.GetMethod("Load"), modCelsiusLoad);
            celsiusUnloadHook = new ILHook(celsiusHooks.GetMethod("Unload"), modCelsiusUnload);
            celsiusTransitionRoutineHook = new Hook(celsiusHooks.GetMethod("LevelTransitionRoutine"),
                typeof(CelsiusDetourSwapout).GetMethod("levelTransitionRoutine", BindingFlags.NonPublic | BindingFlags.Static));

            Logger.Log("CelsiusFixer", "Celsius load/unload methods have been hooked!");

            CelsiusGrowBlockFixup.Load(module);
            orig(module);
        }

        private static void modCelsiusLoad(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // remove the call to LegacyDetour constructor
            cursor.GotoNext(instr => instr.MatchNewobj("Celeste.Mod.Helpers.LegacyMonoMod.LegacyDetour"));
            cursor.RemoveRange(2);
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Pop);
        }

        private static void modCelsiusUnload(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // remove the unload of the detour since we didn't load it in the first place
            cursor.GotoNext(instr => instr.MatchLdsfld("Celeste.Mod.CelsiusHelper.CelsiusHooks", "d_LevelTransitionRoutine"));
            cursor.RemoveRange(2);
        }

        private static IEnumerator levelTransitionRoutine(Func<Level, LevelData, Vector2, IEnumerator> orig, Level level, LevelData next, Vector2 direction) {
            return (IEnumerator) levelTransitionRoutineMethod.Invoke(level, new object[] { next, direction });
        }
    }
}
