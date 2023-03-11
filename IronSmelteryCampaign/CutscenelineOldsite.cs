using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/BadelineOldsiteCutscene")]
    [TrackedAs(typeof(BadelineOldsite))]
    class CutscenelineOldsite : BadelineOldsite {
        public CutscenelineOldsite(EntityData data, Vector2 offset) : base(data, offset, 0) { }

        // In regular C# code we can't just call the parent's base method...
        // but with MonoMod magic we can do it anyway.
        [MonoModLinkTo("Monocle.Entity", "System.Void Added(Monocle.Scene)")]
        public void base_Added(Scene scene) {
            base.Added(scene);
        }

        public override void Added(Scene scene) {
            base_Added(scene);

            Session session = SceneAs<Level>().Session;
            if (!session.GetFlag("evil_maddy_intro")) {
                Hovering = false;
                Visible = true;
                Hair.Visible = false;
                Sprite.Play("pretendDead");
                if (session.Area.Mode == AreaMode.Normal) {
                    session.Audio.Music.Event = null;
                    session.Audio.Apply(forceSixteenthNoteHack: false);
                }
                Scene.Add(new CS02_BadelineIntro(this));
            } else {
                Add(new Coroutine(StartChasingRoutine(Scene as Level)));
            }
        }
    }
}
