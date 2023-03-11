using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.DonkerJellies {
    public class DonkerJelliesModule : EverestModule {
        public override void Load() {
            Logger.SetLogLevel("DonkerJellies", LogLevel.Info);
            redilG.Load();
            FrozenJelly.Load();
        }

        public override void Unload() {
            redilG.Unload();
            FrozenJelly.Unload();
        }

        public static void RecreateJellySpritesByHand(Glider jelly, string path) {
            Sprite sprite = new Sprite(GFX.Game, path + "/");
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("held", "held", 0.1f);
            sprite.Add("fall", "fall", 0.06f, "fallLoop");
            sprite.AddLoop("fallLoop", "fallLoop", 0.06f);
            sprite.Add("death", "death", 0.06f);
            sprite.JustifyOrigin(new Vector2(0.5f, 0.58f));
            sprite.Play("idle");

            DynData<Glider> jellyData = new DynData<Glider>(jelly);
            jelly.Remove(jellyData.Get<Sprite>("sprite"));
            jellyData["sprite"] = sprite;
            jelly.Add(sprite);
        }
    }
}