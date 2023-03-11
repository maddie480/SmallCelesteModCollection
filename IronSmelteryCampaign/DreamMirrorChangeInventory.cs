using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using On.Celeste;

namespace Celeste.Mod.IronSmelteryCampaign {
    [CustomEntity("IronSmelteryCampaign/DreamMirrorChangeInventory")]
    internal class DreamMirrorChangeInventory : DreamMirror {
        private readonly PlayerInventory targetInventory;
        private readonly string tpTo;

        public static void Load() {
            On.Celeste.DreamMirror.Broken += broken;
        }

        public static void Unload() {
            On.Celeste.DreamMirror.Broken -= broken;
        }

        public DreamMirrorChangeInventory(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            targetInventory = (PlayerInventory) typeof(PlayerInventory).GetField(data.Attr("targetInventory", "Default")).GetValue(null);
            tpTo = data.Attr("tpTo");
        }

        private static void broken(On.Celeste.DreamMirror.orig_Broken orig, DreamMirror self, bool wasSkipped) {
            orig(self, wasSkipped);
            DreamMirrorChangeInventory mirror = self as DreamMirrorChangeInventory;
            if (mirror != null) {
                (self.Scene as Level).Session.Inventory = mirror.targetInventory;
                (self.Scene as Level).OnEndOfFrame += delegate {
                    butcheredTeleportTo(self.Scene.Tracker.GetEntity<Player>(), self.Scene as Level, mirror.tpTo);
                };
            }
        }

        private static void butcheredTeleportTo(Player player, Level level, string nextLevel) {
            Leader.StoreStrawberries(player.Leader);
            Vector2 position = player.Position;
            Vector2 roomPosition = position - new Vector2(level.Bounds.Left, level.Bounds.Top);
            Vector2 cameraPositionInRoom = level.Camera.Position - new Vector2(level.Bounds.Left, level.Bounds.Top);
            level.Remove(player);
            level.UnloadLevel();
            level.Session.Level = nextLevel;
            level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top) + roomPosition);
            player.Position = new Vector2(level.Bounds.Left, level.Bounds.Top) + roomPosition;
            player.Hair.MoveHairBy(player.Position - position);
            player.MuffleLanding = true;
            level.Add(player);
            level.LoadLevel(Player.IntroTypes.Transition);
            level.Entities.UpdateLists();
            level.Camera.Position = new Vector2(level.Bounds.Left, level.Bounds.Top) + cameraPositionInRoom;
            level.Update();
            Leader.RestoreStrawberries(player.Leader);
        }
    }
}
