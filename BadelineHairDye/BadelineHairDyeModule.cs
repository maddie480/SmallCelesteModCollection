using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.BadelineHairDye {
    public class BadelineHairDyeModule : EverestModule {
        public override void Load() {
            new DynData<BadelineOldsite>()["HairColor"] = Calc.HexToColor("AC3232");
            On.Celeste.BadelineDummy.ctor += changeLightTint;
        }

        public override void Unload() {
            new DynData<BadelineOldsite>()["HairColor"] = Calc.HexToColor("9B3FB5");
            On.Celeste.BadelineDummy.ctor -= changeLightTint;
        }

        private void changeLightTint(On.Celeste.BadelineDummy.orig_ctor orig, BadelineDummy self, Vector2 position) {
            orig.Invoke(self, position);
            self.Light.Color = Color.White;
        }
    }

}