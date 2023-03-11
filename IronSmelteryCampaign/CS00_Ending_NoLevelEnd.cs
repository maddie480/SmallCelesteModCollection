using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.IronSmelteryCampaign {
    class CS00_Ending_NoLevelEnd : CutsceneEntity {
        private Player player;
        private BirdNPC bird;
        private Bridge bridge;

        private bool keyOffed;

        public CS00_Ending_NoLevelEnd(Player player, BirdNPC bird, Bridge bridge) : base() {
            this.player = player;
            this.bird = bird;
            this.bridge = bridge;
        }

        public override void OnBegin(Level level) {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level) {
            while (Engine.TimeRate > 0f) {
                yield return null;
                if (Engine.TimeRate < 0.5f && bridge != null) {
                    bridge.StopCollapseLoop();
                }
                level.StopShake();
                MInput.GamePads[Input.Gamepad].StopRumble();
                Engine.TimeRate -= Engine.RawDeltaTime * 2f;
            }
            Engine.TimeRate = 0f;
            player.StateMachine.State = 11;
            player.Facing = Facings.Right;
            yield return WaitFor(1f);
            EventInstance instance = Audio.Play("event:/game/general/bird_in", bird.Position);
            bird.Facing = Facings.Left;
            bird.Sprite.Play("fall");
            float percent = 0f;
            Vector2 from = bird.Position;
            Vector2 to = bird.StartPosition;
            while (percent < 1f) {
                bird.Position = from + (to - from) * Ease.QuadOut(percent);
                Audio.Position(instance, bird.Position);
                if (percent > 0.5f) {
                    bird.Sprite.Play("fly");
                }
                percent += Engine.RawDeltaTime * 0.5f;
                yield return null;
            }
            bird.Position = to;
            Audio.Play("event:/game/general/bird_land_dirt", bird.Position);
            Dust.Burst(bird.Position, -(float) Math.PI / 2f, 12, null);
            bird.Sprite.Play("idle");
            yield return WaitFor(0.5f);
            bird.Sprite.Play("peck");
            yield return WaitFor(1.1f);
            yield return bird.ShowTutorial(new BirdTutorialGui(bird, new Vector2(0f, -16f), Dialog.Clean("tutorial_dash"), new Vector2(1f, -1f), "+", Input.Dash), caw: true);
            while (true) {
                Vector2 aim = Input.GetAimVector();
                if (aim.X > 0f && aim.Y < 0f && Input.Dash.Pressed) {
                    break;
                }
                yield return null;
            }
            player.StateMachine.State = 16;
            player.Dashes = 0;
            level.Session.Inventory.Dashes = 1;
            Engine.TimeRate = 1f;
            keyOffed = true;
            Audio.CurrentMusicEventInstance.triggerCue();
            bird.Add(new Coroutine(bird.HideTutorial()));
            yield return 0.25f;
            bird.Add(new Coroutine(bird.StartleAndFlyAway()));
            while (!player.Dead && !player.OnGround()) {
                yield return null;
            }
            yield return 2f;
            Audio.SetMusic("event:/music/lvl0/title_ping");
            yield return 2f;
            EndCutscene(level);
        }

        private IEnumerator WaitFor(float time) {
            for (float t = 0f; t < time; t += Engine.RawDeltaTime) {
                yield return null;
            }
        }

        public override void OnEnd(Level level) {
            if (WasSkipped) {
                if (bird != null) {
                    bird.RemoveSelf();
                }
                if (player != null) {
                    player.Position = bird.StartPosition;
                    player.Speed = Vector2.Zero;
                }
                if (!keyOffed) {
                    Audio.CurrentMusicEventInstance.triggerCue();
                }
            }
            Engine.TimeRate = 1f;
            player.StateMachine.State = 0;

            level.Session.DoNotLoad.Add(new EntityID(level.Session.LevelData.Name, level.Session.LevelData.Entities.Find(e => e.Name == "IronSmelteryCampaign/TutorialBirdNoLevelEnd").ID));
            level.Session.DoNotLoad.Add(new EntityID(level.Session.LevelData.Name, level.Session.LevelData.Entities.Find(e => e.Name == "bridge").ID));
        }
    }
}
