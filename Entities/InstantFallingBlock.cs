using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/InstantFallingBlock")]
    class InstantFallingBlock : FallingBlock {
        public InstantFallingBlock(EntityData data, Vector2 offset) : base(data, offset) {
            // this block starts triggered right away. that's about it.
            Triggered = true;
        }
    }
}
