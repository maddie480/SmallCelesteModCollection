using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.LifeOnTheMoon {
    [CustomEntity("LifeOnTheMoon/LightningColorTrigger")]
    class LightningColorTrigger : Trigger {
        private string lightningColorSmall;
        private string lightningColorBig;
        private bool lightningHasOutline;

        public LightningColorTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            lightningColorSmall = data.Attr("lightningColorSmall", "ffffff");
            lightningColorBig = data.Attr("lightningColorBig", "dedede");
            lightningHasOutline = data.Bool("lightningHasOutline", false);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            LifeOnTheMoonModule.Session.LightningColorSmall = lightningColorSmall;
            LifeOnTheMoonModule.Session.LightningColorBig = lightningColorBig;
            LifeOnTheMoonModule.Session.LightningHasOutline = lightningHasOutline;
        }
    }
}
