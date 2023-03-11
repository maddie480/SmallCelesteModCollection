using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/FlagInventoryTrigger")]
    class FlagInventoryTrigger : ChangeInventoryTrigger {
        private readonly string flagName;

        public FlagInventoryTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            flagName = data.Attr("flag");
        }

        public override void OnEnter(Player player) {
            if (SceneAs<Level>().Session.GetFlag(flagName)) {
                base.OnEnter(player);
            }
        }
    }
}
