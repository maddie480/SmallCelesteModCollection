using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.LifeOnTheMoon {
    [CustomEntity("LifeOnTheMoon/SpaceBooster")]
    internal class SpaceBooster : Booster {
        // Static Fields

        private static ILHook hookOnPlayerOrigUpdate;

        private static readonly List<Color> rainbow = new List<Color> {
            Color.Red,
            Color.OrangeRed,
            Color.Orange,
            Color.Gold,
            Color.Yellow,
            Color.YellowGreen,
            Color.LightGreen,
            Color.Teal,
            Color.SkyBlue,
            Color.BlueViolet,
            Color.Purple,
            Color.Magenta,
            Color.HotPink
        };

        // Static Methods

        private static IEnumerable<Vector2> ComputeCurves(IReadOnlyList<Vector2> positions) {
            var curvesToFollow = new SimpleCurve[positions.Count - 2];

            for (var i = 0; i < positions.Count - 2; i++) {
                curvesToFollow[i] = new SimpleCurve(positions[i], positions[i + 2], positions[i + 1]);
                curvesToFollow[i].DoubleControl();
            }

            var pathPoints = new Vector2[(positions.Count - 1) * 50 + 1];

            if (curvesToFollow.Length == 0) // edge case: there are 2 points, so there is no curve, just a straight line between start and end.
                for (var i = 0; i < pathPoints.Length; i++)
                    pathPoints[i] = Vector2.Lerp(positions[0], positions[1], i / 50f);
            else // follow the curves and merge them into one big curve.
                for (var i = 0; i < pathPoints.Length; i++)
                    if (i < 50) // first curve only
                        pathPoints[i] = curvesToFollow[0].GetPoint(i / 100f);
                    else if (i >= pathPoints.Length - 51) // last curve only
                        pathPoints[i] = curvesToFollow[curvesToFollow.Length - 1].GetPoint((i - (curvesToFollow.Length - 1) * 50) / 100f);
                    else // mix of 2 curves
                        pathPoints[i] =
                            curvesToFollow[i / 50 - 1].GetPoint((i - (i / 50 - 1) * 50) / 100f) * (1 - i % 50 / 50f)
                            + curvesToFollow[i / 50].GetPoint((i - i / 50 * 50) / 100f) * (i % 50 / 50f);

            return pathPoints.ToList();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // a space booster is mostly like a red booster.
            entityData.Values["red"] = true;
            return new SpaceBooster(entityData, offset);
        }

        public static void Load() {
            On.Celeste.Player.RedDashBegin += OnPlayerRedDashBegin;
            On.Celeste.Player.RedDashUpdate += OnPlayerRedDashUpdate;
            On.Celeste.Player.RedDashEnd += OnPlayerRedDashEnd;
            On.Celeste.Player.RedBoost += OnPlayerRedBoost;
            On.Celeste.Player.BoostUpdate += OnPlayerBoostUpdate;

            On.Celeste.Player.OnCollideH += ModBoosterCollideSolidH;
            On.Celeste.Player.OnCollideV += ModBoosterCollideSolidV;

            On.Celeste.Booster.PlayerReleased += OnPlayerReleased;
            On.Celeste.Booster.Respawn += OnBoosterRespawn;
            On.Celeste.Booster.OnPlayerDashed += OnBoosterPlayerDashed;

            IL.Celeste.Booster.Appear += ModSounds;
            IL.Celeste.Booster.OnPlayer += ModSounds;
            IL.Celeste.Booster.PlayerBoosted += ModSounds;
            IL.Celeste.Booster.PlayerReleased += ModSounds;
            IL.Celeste.Booster.Respawn += ModSounds;
            IL.Celeste.Booster.PlayerBoosted += ModLoopingSfxSound;

            hookOnPlayerOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update"), ModCameraTarget);
        }

        public static void Unload() {
            On.Celeste.Player.RedDashBegin -= OnPlayerRedDashBegin;
            On.Celeste.Player.RedDashUpdate -= OnPlayerRedDashUpdate;
            On.Celeste.Player.RedDashEnd -= OnPlayerRedDashEnd;
            On.Celeste.Player.RedBoost -= OnPlayerRedBoost;
            On.Celeste.Player.BoostUpdate -= OnPlayerBoostUpdate;

            On.Celeste.Player.OnCollideH -= ModBoosterCollideSolidH;
            On.Celeste.Player.OnCollideV -= ModBoosterCollideSolidV;

            On.Celeste.Booster.PlayerReleased -= OnPlayerReleased;
            On.Celeste.Booster.Respawn -= OnBoosterRespawn;
            On.Celeste.Booster.OnPlayerDashed -= OnBoosterPlayerDashed;

            IL.Celeste.Booster.Appear -= ModSounds;
            IL.Celeste.Booster.OnPlayer -= ModSounds;
            IL.Celeste.Booster.PlayerBoosted -= ModSounds;
            IL.Celeste.Booster.PlayerReleased -= ModSounds;
            IL.Celeste.Booster.Respawn -= ModSounds;
            IL.Celeste.Booster.PlayerBoosted -= ModLoopingSfxSound;

            hookOnPlayerOrigUpdate?.Dispose();
            hookOnPlayerOrigUpdate = null;
        }

        private static int OnPlayerBoostUpdate(On.Celeste.Player.orig_BoostUpdate orig, Player self) {
            if (self.CurrentBooster is SpaceBooster spaceBooster && spaceBooster.instant) {
                // switch to Red Dash state immediately.
                return 5;
            }
            return orig(self);
        }

        private static void OnPlayerRedDashBegin(On.Celeste.Player.orig_RedDashBegin orig, Player self) {
            orig(self);

            // reset wind displacement when space booster starts from its starting point.
            if (self.CurrentBooster is SpaceBooster spaceBooster)
                if (spaceBooster.currentIndex == 0)
                    spaceBooster.windDisplacement = Vector2.Zero;
                else
                    spaceBooster.windDisplacement = spaceBooster.windDisplacementAtBoostBegin;
        }

        private static int OnPlayerRedDashUpdate(On.Celeste.Player.orig_RedDashUpdate orig, Player self) {
            if (self.CurrentBooster is SpaceBooster aSpaceBooster) // store the space booster to be able to read it here
                new DynData<Player>(self)["spaceBooster"] = aSpaceBooster;

            var spaceBooster = new DynData<Player>(self).Get<SpaceBooster>("spaceBooster");
            if (spaceBooster == null)
                return orig(self);

            if (spaceBooster.leapPositions.Contains(spaceBooster.currentIndex - 1)) {
                self.Center = spaceBooster.pathPointOffset(spaceBooster.currentIndex);
                self.Position = new Vector2((int) Math.Round(self.Position.X), (int) Math.Round(self.Position.Y)); // make sure Position stays an integer
            }

            // move towards the next point that is farther of 2px from the current position.
            while (spaceBooster.currentIndex + 1 < spaceBooster.pathPoints.Count
                   && (spaceBooster.pathPointOffset(spaceBooster.currentIndex) - self.Center).LengthSquared() < 16f)
                spaceBooster.currentIndex++;

            float distanceToPathPointSquared = (spaceBooster.pathPointOffset(spaceBooster.currentIndex) - self.Center).LengthSquared();

            if (spaceBooster.currentIndex == spaceBooster.pathPoints.Count - 1) {
                // we reached the end, get off the bubble!
                spaceBooster.railEnded = true;
                return 0;
            }

            var target = spaceBooster.pathPointOffset(spaceBooster.currentIndex);
            var direction = target - self.Center;
            direction.Normalize();
            self.Speed = direction * spaceBooster.speed;

            Vector2 oldPosition = self.Position;

            spaceBooster.Ch9HubTransition = !spaceBooster.canDashOut;
            var state = orig(self);
            spaceBooster.Ch9HubTransition = false;

            if ((self.Position - oldPosition).LengthSquared() > distanceToPathPointSquared) {
                // we overshot the path point! we should target the next one.
                spaceBooster.currentIndex++;
            }

            return state;
        }

        private static void OnPlayerRedDashEnd(On.Celeste.Player.orig_RedDashEnd orig, Player self) {
            new DynData<Player>(self)["spaceBooster"] = null;
            orig(self);
        }

        private static void OnPlayerReleased(On.Celeste.Booster.orig_PlayerReleased orig, Booster self) {
            if (self is SpaceBooster spaceBooster) {
                if (((Level) Engine.Scene).Tracker.GetEntity<Player>().CollideCheck<Solid>()) {
                    orig(self);
                    return;
                }

                if (spaceBooster.stayTime > 0f || spaceBooster.continuesWithoutPlayer) {
                    if (spaceBooster.railEnded) {
                        // don't stay at the end of the rail.
                        spaceBooster.railEnded = false;
                        spaceBooster.atLast = !spaceBooster.atLast;
                        self.Collider.Position = new Vector2(0f, 2f);
                        orig(self);

                        if (spaceBooster.respawnTime != 1f)
                            new DynData<Booster>(self)["respawnTimer"] = spaceBooster.respawnTime;

                        return;
                    }

                    var selfData = new DynData<Booster>(self);
                    self.Collider.Position = selfData.Get<Sprite>("sprite").Position + new Vector2(0f, 2f);
                    selfData.Get<Sprite>("sprite").Play("loop", true);
                    selfData["cannotUseTimer"] = 0.25f;
                    selfData["BoostingPlayer"] = false;

                    if (spaceBooster.continuesWithoutPlayer) spaceBooster.continuingWithoutPlayer = true;
                    else {
                        // same as vanilla, but don't set the respawnTimer and move the hitbox of the booster to the current position
                        // + set the stayTimer to reset the position after it times out.
                        Audio.Play("event:/muntheory/lifeonthemoon/booster_end", selfData.Get<Sprite>("sprite").RenderPosition, "muffle", spaceBooster.muffle);
                        selfData.Get<Wiggler>("wiggler").Stop();
                        selfData.Get<SoundSource>("loopingSfx").Stop();

                        spaceBooster.staying = true;
                        spaceBooster.stayTimer = spaceBooster.stayTime;
                    }
                } else {
                    orig(self);

                    if (spaceBooster.respawnTime != 1f) new DynData<Booster>(self)["respawnTimer"] = spaceBooster.respawnTime;
                }
            } else {
                orig(self);
            }
        }

        private static void OnPlayerRedBoost(On.Celeste.Player.orig_RedBoost orig, Player self, Booster booster) {
            if (booster is SpaceBooster spaceBooster) {
                spaceBooster.staying = false;
                spaceBooster.continuingWithoutPlayer = false;
                spaceBooster.windDisplacementAtBoostBegin = spaceBooster.windDisplacement;
                new DynData<Booster>(spaceBooster).Get<SoundSource>("loopingSfx").Stop();
            }

            orig(self, booster);
        }

        private static void OnBoosterRespawn(On.Celeste.Booster.orig_Respawn orig, Booster self) {
            orig(self);

            if (self is SpaceBooster space) space.currentIndex = 0;
        }

        private static void OnBoosterPlayerDashed(On.Celeste.Booster.orig_OnPlayerDashed orig, Booster self, Vector2 direction) {
            if (self is SpaceBooster && self.BoostingPlayer) new DynData<Booster>(self)["cannotUseTimer"] = 0.05f;

            orig(self, direction);
        }

        private static void ModBoosterCollideSolidH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            var initialState = self.StateMachine.State;
            orig(self, data);

            if (initialState != 5 || self.StateMachine.State != 6 || !(self.LastBooster is SpaceBooster spaceBooster))
                return;

            spaceBooster.railEnded = true;
        }

        private static void ModBoosterCollideSolidV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            var initialState = self.StateMachine.State;
            orig(self, data);

            if (initialState != 5 || self.StateMachine.State != 6 || !(self.LastBooster is SpaceBooster spaceBooster))
                return;

            spaceBooster.railEnded = true;
        }

        private static void ModCameraTarget(ILContext il) {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchLdarg(0), instr => instr.MatchCallvirt<Player>("get_CameraTarget"))) {
                if (cursor.Next.Next.Next.OpCode == OpCodes.Callvirt) // this is not the instance we want! this one is in an if(State == 18) so there is no way we are currently in the RedDash state.
                    continue;

                Logger.Log("LifeOnTheMoon/SpaceBooster", $"Modding camera target at {cursor.Index} in IL for Player.orig_Update");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Player>>(self => {
                    if (self.StateMachine.State == 5) {
                        var selfData = new DynData<Player>(self);
                        if (selfData.Data.TryGetValue("spaceBooster", out var spaceBoosterObj) && spaceBoosterObj != null) {
                            var spaceBooster = (SpaceBooster) spaceBoosterObj;
                            if (!spaceBooster.controlCamera) {
                                new DynData<StateMachine>(self.StateMachine)["state"] = 0;
                                selfData["stateForcedTo0"] = true;
                            }
                        }
                    }
                });

                cursor.Index += 2;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Player>>(self => {
                    var selfData = new DynData<Player>(self);
                    if (selfData.Data.TryGetValue("stateForcedTo0", out var stateForcedTo0) && (bool) stateForcedTo0) {
                        new DynData<StateMachine>(self.StateMachine)["state"] = 5;
                        selfData["stateForcedTo0"] = false;
                    }
                });
            }
        }

        private static void ModSounds(ILContext il) {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchCall(typeof(Audio).GetMethod("Play", new[] { typeof(string), typeof(Vector2) })))) {
                Logger.Log("LifeOnTheMoon/SpaceBooster", $"Modding booster sound at {cursor.Index} in IL for {cursor.Method.FullName}");

                cursor.Remove();
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, Vector2, Booster, EventInstance>>((path, position, self) => {
                    if (self is SpaceBooster spaceBooster) // play the modded sound with the modified path.
                        return Audio.Play(path.Replace("event:/game/05_mirror_temple/redbooster_", "event:/muntheory/lifeonthemoon/booster_"), position, "muffle", spaceBooster.muffle, "instant", spaceBooster.instant ? 1 : 0);

                    // play the sound like vanilla.
                    return Audio.Play(path, position);
                });
            }
        }

        private static void ModLoopingSfxSound(ILContext il) {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchCallvirt<SoundSource>("Play"))) {
                Logger.Log("LifeOnTheMoon/SpaceBooster", $"Modding looping SFX sound at {cursor.Index} in IL for {cursor.Method.FullName}");

                cursor.Remove();
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<SoundSource, string, string, float, Booster, SoundSource>>((loopingSfx, path, param, value, self) => {
                    if (self is SpaceBooster spaceBooster) // play the modded sound with the modified path.
                        return loopingSfx.Play(path.Replace("event:/game/05_mirror_temple/redbooster_", "event:/muntheory/lifeonthemoon/booster_"), "muffle", spaceBooster.muffle);

                    // play the sound like vanilla.
                    return loopingSfx.Play(path, param, value);
                });
            }
        }

        // Fields

        private readonly bool canDashOut;
        private readonly bool continuesWithoutPlayer;
        private readonly bool controlCamera;
        private readonly List<Vector2> dotsPosition;
        private readonly float windMultiplier;

        private readonly List<int> leapPositions;
        private readonly float muffle;
        private readonly Color pathColor;
        private readonly List<Vector2> pathPoints;
        private readonly float respawnTime;
        private readonly float speed;
        private readonly float stayTime;

        private bool continuingWithoutPlayer;

        private int currentIndex;

        private readonly new ParticleType P_RedAppear;
        private readonly ParticleType P_SpaceAppear;
        private bool railEnded;
        private bool staying;
        private float stayTimer;
        private ParticleType flameParticle;

        private readonly bool instant;
        private readonly bool recycle;

        private bool atLast;

        private Vector2 windDisplacement = Vector2.Zero;
        private Vector2 windDisplacementAtBoostBegin = Vector2.Zero;

        // Constructors

        public SpaceBooster(EntityData data, Vector2 offset) : base(data, offset) {
            var self = new DynData<Booster>(this);
            stayTime = data.Float("stayTime");
            canDashOut = data.Bool("canDashOut", true);
            pathColor = Calc.HexToColor(data.Attr("pathColor", "8f8f8f"));
            continuesWithoutPlayer = data.Bool("continuesWithoutPlayer");
            respawnTime = data.Float("respawnTime", 1f);
            controlCamera = data.Bool("controlCamera", true);
            speed = data.Float("speed", 240f);
            muffle = data.Float("muffle", 0.75f);
            instant = data.Bool(nameof(instant));
            recycle = data.Bool(nameof(recycle));
            windMultiplier = data.Float("windMultiplier", 1f);

            if (data.Bool("hasFlame")) {
                flameParticle = new ParticleType(TouchSwitch.P_Fire) { Color = pathColor, Color2 = pathColor };
            }

            LifeOnTheMoonModule.SpriteBank.CreateOn(self.Get<Sprite>("sprite"), continuesWithoutPlayer ? "spaceBoosterContinuous" :
                stayTime > 0f ? "spaceBoosterStaying" : "spaceBooster");

            var particleType = new ParticleType(self.Get<ParticleType>("particleType")) {
                Color = Calc.HexToColor("868686")
            };
            self["particleType"] = particleType;

            P_RedAppear = Booster.P_RedAppear;
            P_SpaceAppear = new ParticleType(Booster.P_RedAppear) {
                Color = Calc.HexToColor("C5C5C5")
            };

            var nodes = data.NodesOffset(offset);
            var positions = new Vector2[nodes.Length + 1];
            positions[0] = Position;
            for (var i = 0; i < nodes.Length; i++) positions[i + 1] = nodes[i];

            // parse the teleportAtNodes option.
            var teleportAtNodes = new int[0];
            if (!string.IsNullOrEmpty(data.Attr("teleportAtNodes"))) {
                var split = data.Attr("teleportAtNodes").Split(',');
                teleportAtNodes = new int[split.Length];
                for (var i = 0; i < split.Length; i++) teleportAtNodes[i] = int.Parse(split[i]);
            }

            pathPoints = new List<Vector2>();
            leapPositions = new List<int>();
            var positionsToProcess = new List<Vector2>();
            var index = 0;
            var indexInTeleportAtNodes = 0;
            foreach (var position in positions) {
                positionsToProcess.Add(position);
                if (teleportAtNodes.Length > indexInTeleportAtNodes && teleportAtNodes[indexInTeleportAtNodes] == index) {
                    pathPoints.AddRange(ComputeCurves(positionsToProcess));
                    leapPositions.Add(pathPoints.Count - 1);
                    positionsToProcess.Clear();
                    indexInTeleportAtNodes++;
                }

                index++;
            }

            pathPoints.AddRange(ComputeCurves(positionsToProcess));

            // the displayed dots have to be visually regular (every 2px) whereas path points are irregular depending on distance between nodes.
            dotsPosition = new List<Vector2>();
            var lastDot = pathPoints[0];
            dotsPosition.Add(lastDot);
            index = 0;
            foreach (var waypoint in pathPoints) {
                if (leapPositions.Contains(index - 1)) {
                    lastDot = waypoint;
                    dotsPosition.Add(lastDot);
                }

                while ((waypoint - lastDot).LengthSquared() > 16f) {
                    // the waypoint is at > 2px distance: move towards it by 2px and add a dot.
                    var diff = waypoint - lastDot;
                    diff.Normalize();
                    diff *= 4f;
                    lastDot += diff;
                    dotsPosition.Add(lastDot);
                }

                index++;
            }

            Add(new WindMover(windMove));
        }

        // Methods

        private void windMove(Vector2 move) {
            if (!staying) {
                // wind just makes the bubble path offset grow over time.
                windDisplacement += move * windMultiplier;
            }
        }

        private Vector2 pathPointOffset(int index) {
            // the path is offset by the wind.
            return pathPoints[index] + windDisplacement;
        }

        public override void Update() {
            Booster.P_RedAppear = P_SpaceAppear;
            base.Update();
            Booster.P_RedAppear = P_RedAppear;

            var selfData = new DynData<Booster>(this);
            if (flameParticle != null && Scene.OnInterval(0.01f) && selfData.Get<float>("respawnTimer") <= 0f) {
                Vector2 position = Position + selfData.Get<Sprite>("sprite").Position + Calc.AngleToVector(Calc.Random.NextAngle(), 9f);
                SceneAs<Level>().ParticlesBG.Emit(flameParticle, position);
            }

            if (staying) {
                // force the sprite to stay still, vanilla wants to drag it real bad
                selfData.Get<Sprite>("sprite").Position = Collider.Position - new Vector2(0f, 2f);

                stayTimer -= Engine.DeltaTime;
                if (stayTimer <= 0f) {
                    staying = false;

                    // pop the bubble
                    Audio.Play("event:/muntheory/lifeonthemoon/booster_end", selfData.Get<Sprite>("sprite").RenderPosition, "muffle", muffle);
                    selfData.Get<Sprite>("sprite").Play("pop");
                    selfData["respawnTimer"] = respawnTime;
                    Collider.Position = new Vector2(0f, 2f);
                }
            }

            if (!continuingWithoutPlayer)
                return;

            {
                if (leapPositions.Contains(currentIndex - 1)) Collider.Center = pathPointOffset(currentIndex) - Position;

                // move towards the next point that is farther of 2px from the current position.
                while (currentIndex + 1 < pathPoints.Count
                       && (pathPointOffset(currentIndex) - Center).LengthSquared() < 16f)
                    currentIndex++;

                if (currentIndex == pathPoints.Count - 1) {
                    // we reached the end, the bubble should pop and return to its original position.
                    Audio.Play("event:/muntheory/lifeonthemoon/booster_end", selfData.Get<Sprite>("sprite").RenderPosition, "muffle", muffle);
                    selfData.Get<Wiggler>("wiggler").Stop();
                    selfData.Get<SoundSource>("loopingSfx").Stop();
                    selfData.Get<Sprite>("sprite").Play("pop");
                    selfData["respawnTimer"] = respawnTime;
                    Collider.Position = new Vector2(0f, 2f);

                    continuingWithoutPlayer = false;
                } else {
                    var target = pathPointOffset(currentIndex);
                    var direction = target - Center;
                    direction.Normalize();
                    var dirSpeed = direction * speed;
                    Collider.Position += dirSpeed * Engine.DeltaTime;
                    selfData.Get<Sprite>("sprite").Position = Collider.Position - new Vector2(0f, 2f);
                }
            }
        }

        public override void Render() {
            var offset = 0;

            /*
            foreach (Vector2 dot in dotsPosition) {
                Draw.Point(dot, Color.Lerp(pathColor * 0.8f, Color.White * 0.5f, Ease.SineInOut(Calc.YoYo((Scene.TimeActive / 2f - offset * 0.01f) % 1f))));
                offset++;Ease
            }
             */

            foreach (var dot in dotsPosition) {
                var firstOrLast = dot == dotsPosition[dotsPosition.Count - 1] || dot == dotsPosition[0];
                var amount = Ease.SineInOut(Calc.YoYo((Scene.TimeActive * (speed * (speed >= 320f || speed <= 190f ? 2f : 1f) / 240f) / (instant ? 1.5f : 2.5f) - offset * 0.05f) % 1f));

                var size = firstOrLast
                    ? Calc.LerpClamp(14f, (float) Math.Abs(Math.Sin(Scene.TimeActive / 2f)), amount) + 5f
                    : Calc.LerpClamp(8f, (float) Math.Abs(Math.Sin(Scene.TimeActive / 2f)), amount);

                var color = Color.Lerp((recycle ? rainbow[offset % rainbow.Count] : pathColor) * 0.8f, (recycle ? rainbow[offset % rainbow.Count] : Color.Black) * 0.4f, Ease.SineInOut(Calc.YoYo((Scene.TimeActive / 2f - offset * 0.1f) % 1f)));

                // Draw.Rect(dot - new Vector2(size / 2f), size, size, color);
                Draw.Circle(dot, size, firstOrLast ? pathColor * 0.6f : color, 4);

                offset++;
            }

            base.Render();
        }
    }
}