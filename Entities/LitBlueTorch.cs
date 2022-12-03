using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/LitBlueTorch")]
    class LitBlueTorch : Torch {
        public LitBlueTorch(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id) { }

        public override void Added(Scene scene) {
            // by turning on startLit in Added but not in the constructor, we make the torch blue instead of yellow.
            new DynData<Torch>(this)["startLit"] = true;

            base.Added(scene);
        }
    }
}
