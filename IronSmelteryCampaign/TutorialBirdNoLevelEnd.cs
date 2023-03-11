using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/TutorialBirdNoLevelEnd")]
    class TutorialBirdNoLevelEnd : BirdNPC {
        private bool cutsceneHappened = false;

        public TutorialBirdNoLevelEnd(EntityData data, Vector2 offset) : base(data.Position + offset, Modes.DashingTutorial) { }

        public override void Update() {
            base.Update();

            if (!cutsceneHappened && Get<Coroutine>() == null) {
                CS00_Ending vanillaEndingCutscene = Scene.Entities.FindFirst<CS00_Ending>();
                if (vanillaEndingCutscene != null) {
                    // no
                    vanillaEndingCutscene.RemoveSelf();
                    Scene.Add(new CS00_Ending_NoLevelEnd(Scene.Tracker.GetEntity<Player>(), this, Scene.Entities.FindFirst<Bridge>()));
                    cutsceneHappened = true;
                }
            }
        }
    }
}
