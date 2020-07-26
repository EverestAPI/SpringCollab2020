using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/NonCoreModeWallBooster")]
    [TrackedAs(typeof(WallBooster))]
    class NonCoreModeWallBooster : WallBooster {
        public NonCoreModeWallBooster(EntityData data, Vector2 offset) : base(data, offset) {
            Remove(Get<CoreModeListener>());
        }
    }
}
