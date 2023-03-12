using System.Linq;
using MonoMod.Utils;

namespace Celeste.Mod.SeekerCH6Cutscene {
    public class SeekerCH6CutsceneModule : EverestModule {
        public override void Load() {
            Everest.Events.Level.OnLoadLevel += onLoadLevel;
        }

        public override void Unload() {
            Everest.Events.Level.OnLoadLevel -= onLoadLevel;
        }

        private void onLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) {
            if ((from t in level.Entities.OfType<EventTrigger>()
                 where t.Event == "seekerinthedark_ch6_ending"
                 select t).Count() > 0) {

                NPC06_Badeline_Crying badeline = level.Entities.FindFirst<NPC06_Badeline_Crying>();
                if (badeline != null) {
                    new DynData<NPC06_Badeline_Crying>(badeline)["started"] = true;
                }
            }
        }
    }
}