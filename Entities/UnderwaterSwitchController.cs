using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/UnderwaterSwitchController")]
    class UnderwaterSwitchController : Entity {
        private static FieldInfo waterFill = typeof(Water).GetField("fill", BindingFlags.NonPublic | BindingFlags.Instance);

        private string flag;
        private Water water;

        public UnderwaterSwitchController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flag = data.Attr("flag");

            Add(new Coroutine(Routine()));
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // if the flag is already set, spawn water right away
            Session session = SceneAs<Level>().Session;
            if (session.GetFlag(flag)) {
                spawnWater(session.LevelData.Bounds);
            }
        }

        public IEnumerator Routine() {
            Session session = SceneAs<Level>().Session;
            while (true) {
                // wait until the flag is set.
                while (!session.GetFlag(flag)) {
                    yield return null;
                }

                // spawn water.
                if (water == null) {
                    spawnWater(session.LevelData.Bounds);
                }

                // wait until the flag is set.
                while (session.GetFlag(flag)) {
                    yield return null;
                }

                // make water go away.
                Scene.Remove(water);
                water = null;
            }
        }

        private void spawnWater(Rectangle levelBounds) {
            // flood the room with water, make the water 10 pixels over the top to prevent a "splash" effect when going in a room above.
            water = new Water(new Vector2(levelBounds.Left, levelBounds.Top - 10),
                false, false, levelBounds.Width, levelBounds.Height + 10);

            // but we don't want the water to render off-screen, because it is visible on upwards transitions.
            Rectangle fill = (Rectangle) waterFill.GetValue(water);
            fill.Y += 10;
            fill.Height -= 10;
            waterFill.SetValue(water, fill);

            Scene.Add(water);
        }
    }
}
