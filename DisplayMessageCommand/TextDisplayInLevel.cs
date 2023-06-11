using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.DisplayMessageCommand {
    public class TextDisplayInLevel : Entity {
        private static Dictionary<string, TextDisplayInLevel> textDisplays = new Dictionary<string, TextDisplayInLevel>();

        private readonly string id;
        private readonly string text;
        private readonly float scale;
        private readonly float textWidth;
        private readonly float verticalScale;
        private readonly bool isLeft;
        private readonly Color fillColor;

        private readonly float xIn;
        private readonly float xOut;

        private MTexture bg = GFX.Gui["DisplayMessageCommand/extendedStrawberryCountBG"];

        public TextDisplayInLevel(string id, string text, float scale, float y, bool isLeft, float duration, Color fillColor) {
            this.id = id;
            this.text = text;
            this.scale = scale;
            this.isLeft = isLeft;
            this.fillColor = fillColor;

            textDisplays[id] = this;

            textWidth = ActiveFont.Measure(text).X * scale;
            verticalScale = 1 + (ActiveFont.Measure(text).Y - 64f) / 38f;

            xOut = isLeft ? -100f - textWidth : 2020f;
            xIn = isLeft ? 0f : 1840f - textWidth;

            Tag = (Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate);
            Position = new Vector2(xOut, y);

            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 0.5f, true);
            tween.OnUpdate = t => Position.X = Calc.ClampedMap(t.Eased, 0, 1, xOut, xIn);
            Add(tween);

            if (duration > 0) {
                Add(new Coroutine(hideRoutine(duration)));
            }
        }

        private IEnumerator hideRoutine(float delay) {
            yield return delay;
            transitionOut();
        }

        public override void Render() {
            base.Render();

            bg.Draw(Position + new Vector2(
                    isLeft ? textWidth - 1897f * scale + 55f : 1891f * scale + 29f,
                    38f * (1 - scale) - 38f * (verticalScale - 1) * scale
                ),
                Vector2.Zero,
                fillColor,
                new Vector2(isLeft ? 1 : -1, verticalScale) * scale);

            ActiveFont.DrawOutline(text, Position + new Vector2(44, 38), new Vector2(0, 1), Vector2.One * scale, fillColor, 2f, Color.Black);
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            if (textDisplays.TryGetValue(id, out TextDisplayInLevel textDisplay) && textDisplay == this) {
                textDisplays.Remove(id);
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            textDisplays.Remove(id);
        }

        private void transitionOut() {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineIn, 0.5f, true);
            tween.OnUpdate = t => Position.X = Calc.ClampedMap(t.Eased, 0, 1, xIn, xOut);
            tween.OnComplete = t => RemoveSelf();
            Add(tween);
        }

        internal static void displayMessageCommand(string id, float scale, float y, bool isLeft, string text, float duration, Color fillColor) {
            if (textDisplays.TryGetValue(id, out TextDisplayInLevel existingMessage)) {
                existingMessage.transitionOut();
            }

            Engine.Scene.Add(new TextDisplayInLevel(id, text.Replace("\\n", "\n"), scale, y, isLeft, duration, fillColor));
        }

        [Command("display_message", "Displays a message on a screen border")]
        private static void displayMessageCommand(string id, float scale, float y, bool isLeft, string text) {
            displayMessageCommand(id, scale, y, isLeft, text, 0f, Color.White);
        }

        [Command("hide_message", "Hide a previously displayed message")]
        private static void hideMessageCommand(string id) {
            if (textDisplays.TryGetValue(id, out TextDisplayInLevel existingMessage)) {
                existingMessage.transitionOut();
            } else {
                Engine.Commands.Log("Text display with id '" + id + "' not found! This should match an ID previously passed to the 'display_message' command.");
            }
        }
    }
}
