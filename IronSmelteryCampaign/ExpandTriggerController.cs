using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/ExpandTriggerController")]
    internal class ExpandTriggerController : Entity {
        public ExpandTriggerController(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            base.Collider = new Hitbox(24f, 24f, -12f, -12f);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            foreach (Trigger trigger in CollideAll<Trigger>()) {
                trigger.Position = new Vector2(SceneAs<Level>().Bounds.Left, (float) SceneAs<Level>().Bounds.Top - 24f);
                trigger.Collider.Width = SceneAs<Level>().Bounds.Width;
                trigger.Collider.Height = (float) SceneAs<Level>().Bounds.Height + 24f;
            }
            RemoveSelf();
        }
    }
}
