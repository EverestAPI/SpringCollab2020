using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/CustomRespawnTimeRefill")]
    class CustomRespawnTimeRefill : Refill {
        public CustomRespawnTimeRefill(EntityData data, Vector2 offset) : base(data, offset) {
            DynData<Refill> self = new DynData<Refill>(this);
            float respawnTime = data.Float("respawnTime", 2.5f);

            foreach (Component component in this) {
                if (component is PlayerCollider collider) {
                    // wrap the original OnPlayer method to modify the respawnTimer if it gets reset to 2.5f.
                    Action<Player> orig = collider.OnCollide;
                    collider.OnCollide = player => {
                        orig(player);
                        if (self.Get<float>("respawnTimer") == 2.5f) {
                            self["respawnTimer"] = respawnTime;
                        }
                    };
                }
            }
        }
    }
}
