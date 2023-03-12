using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MagiCustomizableChapter1Ending {
    [CustomEntity("MagiCustomizableChapter1Ending/BonfireCutsceneTrigger")]
    internal class BonfireCutsceneTrigger : Trigger {
        private string dialogId;

        public BonfireCutsceneTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            dialogId = data.Attr("dialogId");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            base.Scene.Add(new CS01_Ending(player, dialogId));
            RemoveSelf();
        }
    }
}