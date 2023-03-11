namespace Celeste.Mod.SafeRespawnCrumble {
    class SafeRespawnCrumbleModule : EverestModule {
        public override void Load() {
            SafeRespawnCrumble.Load();
            CrystalBombDetonatorRenderer.Load();
        }

        public override void Unload() {
            SafeRespawnCrumble.Unload();
            CrystalBombDetonatorRenderer.Unload();
        }
    }
}
