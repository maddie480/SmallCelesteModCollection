using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/DoNotLoadTrigger")]
    class DoNotLoadTrigger : Trigger {
        private readonly string roomName;
        private readonly int entityID;
        private const string devMessage = "This could have been done with Lua Cutscenes ngl";

        public DoNotLoadTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            roomName = data.Attr("roomName");
            entityID = data.Int("entityID");
        }

        public override void OnEnter(Player player) {
            SceneAs<Level>().Session.DoNotLoad.Add(new EntityID(roomName, entityID));
        }
    }
}
