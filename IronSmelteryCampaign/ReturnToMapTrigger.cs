using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/ReturnToMapTrigger")]
    class ReturnToMapTrigger : Trigger {
        public ReturnToMapTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            Level level = Scene as Level;
            Engine.TimeRate = 1f;
            level.Session.InArea = false;
            level.PauseLock = true;
            Audio.SetMusic(null);
            Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
            level.DoScreenWipe(wipeIn: false, delegate {
                Engine.Scene = new LevelExit(LevelExit.Mode.GiveUp, level.Session, level.HiresSnow);
            }, hiresSnow: true);
            foreach (LevelEndingHook component in level.Tracker.GetComponents<LevelEndingHook>()) {
                if (component.OnEnd != null) {
                    component.OnEnd();
                }
            }

            RemoveSelf();
        }
    }
}
