using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [RegisterStrawberry(true, false)]
    [CustomEntity("SpringCollab2020/returnBerry")]
    class BubbleReturnBerry : Strawberry {
        public BubbleReturnBerry(EntityData data, Vector2 position, EntityID gid) : base(data, position, gid) {
            Add(collider = new PlayerCollider(new Action<Player>(OnPlayer)));
        }

        public static void Load() {
            On.Celeste.Player.OnSquish += ModOnSquish;
        }

        public static void Unload() {
            On.Celeste.Player.OnSquish -= ModOnSquish;
        }

        public new void OnPlayer(Player player) {
            base.OnPlayer(player);

            if (!WaitingOnSeeds) {
                Add(new Coroutine(Return(player), true));
                Collidable = false;
            }
        }

        private static void ModOnSquish(On.Celeste.Player.orig_OnSquish orig, Player player, CollisionData data) {
            // State 21 is the state where the player is located within the Cassette Bubble.
            // They shouldn't be squished by moving blocks inside of it, so we prevent that.
            if (player.StateMachine.State == 21)
                return;

            orig(player, data);
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
