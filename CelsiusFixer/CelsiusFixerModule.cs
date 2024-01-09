namespace Celeste.Mod.CelsiusFixer {
    public class CelsiusFixerModule : EverestModule {
        public override void Load() {
            CelsiusDetourSwapout.Load();
        }

        public override void Unload() {
            CelsiusDetourSwapout.Unload();
            CelsiusGrowBlockFixup.Unload();
        }
    }
}