using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.CelsiusFixer {
    // This contains a hook that fixes Grow Blocks being invisible after the Everest update that added tile rendering culling.
    public static class CelsiusGrowBlockFixup {
        private static Hook celsiusHook;

        public static void Load(EverestModule module) {
            MethodInfo hookTarget = typeof(CelsiusGrowBlockFixup).GetMethod("fixupGrowBlockTileGrid", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo celsiusGrowBlockAwake = module.GetType().Assembly.GetType("Celeste.Mod.CelsiusHelper.GrowBlock").GetMethod("Awake");
            celsiusHook = new Hook(celsiusGrowBlockAwake, hookTarget);

            Logger.Log("CelsiusFixer", "Celsius grow blocks have been hooked!");
        }

        public static void Unload() {
            celsiusHook?.Dispose();
            celsiusHook = null;

            Logger.Log("CelsiusFixer", "Celsius grow blocks have been UN-hooked!");
        }

        private static void fixupGrowBlockTileGrid(Action<Solid, Scene> orig, Solid self, Scene scene) {
            orig(self, scene);

            TileGrid tileGrid = self.Get<TileGrid>();

            // do not use the level camera for rendering clipping, use the block's bounds instead to be sure we render the whole block
            tileGrid.ClipCamera = new Camera(tileGrid.TileWidth * 8, tileGrid.TileHeight * 8) {
                Position = self.Position
            };
        }
    }
}
