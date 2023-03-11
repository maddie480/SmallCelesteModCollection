using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/IncrementDialogCutsceneTrigger")]
    internal class IncrementDialogCutsceneTrigger : Trigger {
        private readonly string dialogIdPrefix;

        private readonly EntityID entityID;
        private readonly bool miniTextbox;

        public IncrementDialogCutsceneTrigger(EntityData data, Vector2 offset, EntityID entId)
            : base(data, offset) {
            dialogIdPrefix = data.Attr("dialogIdPrefix");
            miniTextbox = data.Bool("miniTextbox");
            entityID = entId;
        }

        public override void OnEnter(Player player) {
            (base.Scene as Level).Session.IncrementCounter(dialogIdPrefix);
            if (miniTextbox && (Scene as Level).Session.GetCounter(dialogIdPrefix) > 1) {
                Scene.Add(new MiniTextbox(dialogIdPrefix + (base.Scene as Level).Session.GetCounter(dialogIdPrefix)));
            } else {
                // dialog cutscene
                base.Scene.Add(new DialogCutscene(dialogIdPrefix + (base.Scene as Level).Session.GetCounter(dialogIdPrefix), player, endLevel: false));
            }
            (base.Scene as Level).Session.DoNotLoad.Add(new EntityID(entityID.Level, entityID.ID + 10000000));
            RemoveSelf();
        }
    }
}
