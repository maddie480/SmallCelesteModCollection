using Monocle;

namespace Celeste.Mod.BloomCommand {
    public class BloomCommandModule : EverestModule {
        public override void Load() { }

        public override void Unload() { }

        [Command("bloom", "checks bloom values")]
        private static void CmdBloom() {
            Level level = Engine.Scene as Level;
            if (level != null) {
                Engine.Commands.Log("level base(" + AreaData.Get(level).BloomBase + "), session base add(" + level.Session.BloomBaseAdd + "), current base(" + level.Bloom.Base + "), current strength(" + level.Bloom.Strength + ")");
            }
        }
    }
}