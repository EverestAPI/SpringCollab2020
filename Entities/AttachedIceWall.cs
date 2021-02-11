using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/AttachedIceWall")]
    static class AttachedIceWall {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // an attached ice wall is just like a regular "not core mode" wall booster, but with a different static mover hitbox.
            bool left = entityData.Bool("left");
            WallBooster iceWall = new WallBooster(entityData.Position + offset, entityData.Height, left, notCoreMode: true);
            StaticMover staticMover = iceWall.Get<StaticMover>();
            staticMover.SolidChecker = solid => iceWall.CollideCheck(solid, iceWall.Position + (left ? -2 : 2) * Vector2.UnitX);
            staticMover.OnAttach = platform => iceWall.Depth = platform.Depth + 1;
            iceWall.Get<CoreModeListener>().RemoveSelf();
            return iceWall;
        }
    }
}
