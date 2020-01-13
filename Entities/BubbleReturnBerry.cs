using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/returnBerry")]
    [RegisterStrawberry(true, false)]
    class BubbleReturnBerry : Strawberry {
        public BubbleReturnBerry(EntityData data, Vector2 position, EntityID gid) : base(data, position, gid) {
            Add(collider = new PlayerCollider(new Action<Player>(OnPlayer)));
        }

        public new void OnPlayer(Player player) {
            Add(new Coroutine(Return(player), true));

            Collidable = false;
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
