using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [RegisterStrawberry(true, false)]
    [CustomEntity("SpringCollab2020/returnBerry")]
    class BubbleReturnBerry : Strawberry {
        public BubbleReturnBerry(EntityData data, Vector2 position, EntityID gid) : base(data, position, gid) {
            Add(collider = new PlayerCollider(new Action<Player>(OnPlayer)));
        }

        public static void Load() {
            using (new DetourContext { After = { "*" } }) {
                // fix player specific behavior allowing them to go through upside-down jumpthrus.
                On.Celeste.Player.ctor += onPlayerConstructor;
            }
        }

        public static void Unload() {
            On.Celeste.Player.ctor -= onPlayerConstructor;
        }

        private static void onPlayerConstructor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);

            Collision originalSquishCallback = self.SquishCallback;

            Collision patchedSquishCallback = collisionData => {
                // State 21 is the state where the player is located within the Cassette Bubble.
                // They shouldn't be squished by moving blocks inside of it, so we prevent that.
                if (self.StateMachine.State == 21)
                    return;

                originalSquishCallback(collisionData);
            };

            self.SquishCallback = patchedSquishCallback;
        }

        public new void OnPlayer(Player player) {
            base.OnPlayer(player);

            if (!WaitingOnSeeds) {
                Add(new Coroutine(Return(player), true));
                Collidable = false;
            }
        }

        private IEnumerator Return(Player player) {
            yield return 0.3f;

            if (!player.Dead) {
                Vector2 targetLocation = (Vector2) SceneAs<Level>().Session.RespawnPoint;

                Audio.Play("event:/game/general/cassette_bubblereturn", SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
                player.StartCassetteFly(targetLocation, targetLocation);
            }
        }

        private static PlayerCollider collider;
    }
}
