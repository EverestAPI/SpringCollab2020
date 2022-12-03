using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/BlockJellySpawnTrigger")]
    [Tracked]
    class BlockJellySpawnTrigger : Trigger {
        public static void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            // if the player spawned in a Block Jelly Spawn Trigger...
            if (playerIntro == Player.IntroTypes.Respawn && (self.Tracker.GetEntity<Player>()?.CollideCheck<BlockJellySpawnTrigger>() ?? false)) {
                // remove all jellyfish from the room.
                foreach (Glider jelly in self.Entities.OfType<Glider>()) {
                    jelly.RemoveSelf();
                }
                self.Entities.UpdateLists();
            }
        }

        public BlockJellySpawnTrigger(EntityData data, Vector2 offset) : base(data, offset) { }
    }
}
