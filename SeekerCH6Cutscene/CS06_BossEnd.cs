using System.Collections;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.SeekerCH6Cutscene {
    [CustomEvent("seekerinthedark_ch6_ending")]
    public class CS06_BossEnd : CutsceneEntity {
        public const string Flag = "badeline_connection";

        private Player player;
        private NPC06_Badeline_Crying badeline;

        public CS06_BossEnd(EventTrigger trigger, Player player, string eventID) {
            Tag = Tags.HUD;
            this.player = player;
        }

        public override void OnBegin(Level level) {
            if (level.Session.GetFlag(Flag)) {
                RemoveSelf();
                return;
            }
            badeline = level.Entities.FindFirst<NPC06_Badeline_Crying>();
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level) {
            new DynData<NPC06_Badeline_Crying>(badeline)["started"] = true;
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            while (!player.OnGround()) {
                yield return null;
            }
            player.Facing = Facings.Right;

            yield return 1f;

            Level level2 = SceneAs<Level>();
            level2.Session.Audio.Music.Event = "";
            level2.Session.Audio.Apply(forceSixteenthNoteHack: false);

            yield return Textbox.Say("seekerinthedark_custom_ch6_boss_ending", PlayerHug, BadelineCalmDown);
            yield return 0.5f;

            player.Sprite.Play("idle");

            yield return 0.25f;
            yield return player.DummyWalkToExact((int) player.X - 8, walkBackwards: true);

            Add(new Coroutine(CenterCameraOnPlayer()));

            yield return 0.5f;

            (base.Scene as Level).Session.SetFlag(Flag);
            badeline.RemoveSelf();
            Audio.Play("event:/char/badeline/disappear", badeline.Position);
            level.Displacement.AddBurst(badeline.Center, 0.5f, 24f, 96f, 0.4f);
            level.Particles.Emit(BadelineOldsite.P_Vanish, 12, badeline.Center, Vector2.One * 6f);

            yield return 0.1f;

            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);

            yield return 1f;
            yield return level.ZoomBack(0.5f);

            EndCutscene(level);
        }

        private IEnumerator PlayerHug() {
            Add(new Coroutine(Level.ZoomTo(badeline.Center + new Vector2(0f, -24f) - Level.Camera.Position, 2f, 0.5f)));

            yield return 0.6f;
            yield return player.DummyWalkToExact((int) badeline.X - 10);

            player.Facing = Facings.Right;

            yield return 0.25f;

            player.DummyAutoAnimate = false;
            player.Sprite.Play("hug");

            yield return 0.5f;
        }

        private IEnumerator BadelineCalmDown() {
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 0f);
            badeline.LoopingSfx.Param("end", 1f);

            yield return 0.5f;

            badeline.Sprite.Play("scaredTransition");
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            FinalBossStarfield bossBg = Level.Background.Get<FinalBossStarfield>();
            if (bossBg != null) {
                while (bossBg.Alpha > 0f) {
                    bossBg.Alpha -= Engine.DeltaTime;
                    yield return null;
                }
            }

            yield return 1.5f;
        }

        private IEnumerator CenterCameraOnPlayer() {
            yield return 0.5f;

            Vector2 from = Level.ZoomFocusPoint;
            Vector2 to = player.Center + new Vector2(0f, -24f) - Level.Camera.Position;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime) {
                Level.ZoomFocusPoint = from + (to - from) * Ease.SineInOut(p);
                yield return null;
            }
        }

        public override void OnEnd(Level level) {
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 0f);
            level.ResetZoom();
            if (WasSkipped) {
                level.Session.Audio.Music.Event = "";
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            }
            player.DummyAutoAnimate = true;
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            FinalBossStarfield finalBossStarfield = Level.Background.Get<FinalBossStarfield>();
            if (finalBossStarfield != null) {
                finalBossStarfield.Alpha = 0f;
            }
            badeline.RemoveSelf();
            level.Session.SetFlag(Flag);
        }
    }
}