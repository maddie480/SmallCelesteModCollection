using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ShirbSleepTeleport {
    public class ShirbSleepTeleportModule : EverestModule {
        public static ShirbSleepTeleportModule Instance;

        public ShirbSleepTeleportModule() {
            Instance = this;
        }

        public override void Load() {
        }

        public override void Unload() {
        }

        public void SleepTeleportTo(string name, float spawnX, float spawnY) {
            Level level = Engine.Scene as Level;
            level.Session.Level = name;
            level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(spawnX, spawnY));
            level.Session.UpdateLevelStartDashes();
            Engine.Scene = new LevelLoader(level.Session, level.Session.RespawnPoint) {
                PlayerIntroTypeOverride = Player.IntroTypes.WakeUp
            };
        }
    }
}