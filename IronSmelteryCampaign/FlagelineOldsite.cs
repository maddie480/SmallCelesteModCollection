using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/FlagBadelineOldsite")]
    [TrackedAs(typeof(BadelineOldsite))]
    class FlagelineOldsite : BadelineOldsite {
        private readonly string flagName;

        private bool started = false;

        public FlagelineOldsite(EntityData data, Vector2 offset) : base(data, offset, data.Int("index", defaultValue: 0)) {
            flagName = data.Attr("flag");
        }

        public override void Update() {
            if (started || !SceneAs<Level>().Session.GetFlag(flagName)) {
                base.Update();
                started = true;
            }
        }
    }
}
