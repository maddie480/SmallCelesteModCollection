using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using IL.Celeste;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/DeactivateDreamBlocksCutsceneThing")]
    [Tracked(false)]
    internal class DeactivateDreamBlocksCutsceneThing : Trigger {
        private readonly float speed;
        private float position;
        private bool started;

        private List<DreamBlock> dreamBlocksRemaining;

        private Dictionary<DreamBlock, DreamBlock> dreamBlocksTransitioning = new Dictionary<DreamBlock, DreamBlock>();

        public DeactivateDreamBlocksCutsceneThing(EntityData data, Vector2 offset)
            : base(data, offset) {
            speed = data.Float("speed");

            position = data.NodesOffset(offset)[0].Y;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            dreamBlocksRemaining = (from block in base.Scene.Tracker.GetEntities<DreamBlock>()
                                    where block.Bottom < position
                                    select block).OfType<DreamBlock>().ToList();

            SceneAs<Level>().Session.Inventory.DreamDash = false;
            IL.Celeste.Player.DreamDashCheck += onDreamDashCheck;
            IL.Celeste.Player.DreamDashUpdate += onDreamDashUpdate;
            IL.Celeste.DreamBlock.Render += onDreamBlockRender;
        }

        public override void Removed(Scene scene) {
            SceneAs<Level>().Session.Inventory.DreamDash = true;
            IL.Celeste.Player.DreamDashCheck -= onDreamDashCheck;
            IL.Celeste.Player.DreamDashUpdate -= onDreamDashUpdate;
            IL.Celeste.DreamBlock.Render -= onDreamBlockRender;
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene) {
            SceneAs<Level>().Session.Inventory.DreamDash = true;
            IL.Celeste.Player.DreamDashCheck -= onDreamDashCheck;
            IL.Celeste.Player.DreamDashUpdate -= onDreamDashUpdate;
            IL.Celeste.DreamBlock.Render -= onDreamBlockRender;
            base.SceneEnd(scene);
        }

        private void onDreamBlockRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, (Instruction instr) => instr.MatchCallvirt<Camera>("get_Position"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<DreamBlock>>(delegate (DreamBlock self) {
                    if (dreamBlocksTransitioning.ContainsKey(self)) {
                        self.Collider.Height += dreamBlocksTransitioning[self].Height;
                    }
                });
            }
            if (!cursor.TryGotoNext(MoveType.After, (Instruction instr) => instr.MatchLdfld<DreamBlock>("whiteFill"))) {
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<DreamBlock>>(delegate (DreamBlock self) {
                if (dreamBlocksTransitioning.ContainsKey(self)) {
                    self.Collider.Height -= dreamBlocksTransitioning[self].Height;
                }
            });
        }

        private void onDreamDashCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, (Instruction instr) => instr.MatchLdfld<PlayerInventory>("DreamDash"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<bool, Player, Vector2, bool>>(delegate (bool orig, Player self, Vector2 dir) {
                    DeactivateDreamBlocksCutsceneThing deactivateDreamBlocksCutsceneThing = self.Scene?.Tracker.GetEntity<DeactivateDreamBlocksCutsceneThing>();
                    DreamBlock dreamBlock = self.CollideFirst<DreamBlock>(self.Position + dir);
                    if (deactivateDreamBlocksCutsceneThing == null || dreamBlock == null) {
                        return orig;
                    }
                    return orig || deactivateDreamBlocksCutsceneThing.dreamBlocksRemaining.Contains(dreamBlock) || deactivateDreamBlocksCutsceneThing.dreamBlocksTransitioning.ContainsKey(dreamBlock);
                });
            }
        }

        private void onDreamDashUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, (Instruction instr) => instr.MatchCall<Entity>("CollideFirst"))) {
                cursor.EmitDelegate<Func<DreamBlock, DreamBlock>>(delegate (DreamBlock orig) {
                    DeactivateDreamBlocksCutsceneThing deactivateDreamBlocksCutsceneThing = orig?.Scene?.Tracker.GetEntity<DeactivateDreamBlocksCutsceneThing>();
                    return (deactivateDreamBlocksCutsceneThing != null && (deactivateDreamBlocksCutsceneThing.dreamBlocksRemaining.Contains(orig) || deactivateDreamBlocksCutsceneThing.dreamBlocksTransitioning.ContainsKey(orig))) ? orig : null;
                });
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);
            started = true;
        }

        public override void DebugRender(Camera camera) {
            base.DebugRender(camera);
            Draw.Line(new Vector2(camera.Left, position), new Vector2(camera.Right, position), Color.Blue);
        }

        public override void Update() {
            base.Update();
            if (started) {
                position -= speed * Engine.DeltaTime;
            }
            foreach (DreamBlock item in dreamBlocksRemaining.ToList()) {
                if (item.Bottom > position) {
                    DreamBlock dreamBlock = new DreamBlock(new Vector2(item.Left, item.Bottom), item.Width, 0f, null, fastMoving: false, oneUse: false, item.Depth == 5000);
                    base.Scene.Add(dreamBlock);
                    dreamBlocksRemaining.Remove(item);
                    dreamBlocksTransitioning.Add(item, dreamBlock);
                }
            }
            foreach (KeyValuePair<DreamBlock, DreamBlock> item2 in dreamBlocksTransitioning.ToList()) {
                DreamBlock topBlock = item2.Key;
                DreamBlock bottomBlock = item2.Value;

                int boundaryY;
                if (topBlock.Top > position) {
                    boundaryY = (int) topBlock.Position.Y;
                    dreamBlocksTransitioning.Remove(topBlock);
                    base.Scene.Remove(topBlock);
                } else {
                    boundaryY = (int) Math.Round(position);
                }

                float bottom = bottomBlock.Bottom;
                bottomBlock.Position.Y = boundaryY;
                float deltaY = bottom - bottomBlock.Bottom;
                bottomBlock.Collider.Height += deltaY;
                topBlock.Collider.Height -= deltaY;
                if (topBlock.Collider.Height <= 0f) {
                    topBlock.Collider.Height = 8f;
                }
                if (base.Scene.OnInterval(0.1f)) {
                    for (int i = 0; i < bottomBlock.Width; i++) {
                        SceneAs<Level>().ParticlesFG.Emit(Strawberry.P_WingsBurst, new Vector2(bottomBlock.Left + (float) i, bottomBlock.Top));
                    }
                }
            }
        }
    }
}
