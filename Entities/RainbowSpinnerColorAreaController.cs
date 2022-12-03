using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/RainbowSpinnerColorAreaController")]
    [Tracked]
    class RainbowSpinnerColorAreaController : Entity {
        private static bool rainbowSpinnerHueHooked = false;

        // the parameters for this spinner controller.
        private Color[] colors;
        private float gradientSize;

        public RainbowSpinnerColorAreaController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            // convert the color list to Color objects
            string[] colorsAsStrings = data.Attr("colors", "89E5AE,88E0E0,87A9DD,9887DB,D088E2").Split(',');
            colors = new Color[colorsAsStrings.Length];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Calc.HexToColor(colorsAsStrings[i]);
            }

            gradientSize = data.Float("gradientSize", 280);

            // make this controller collidable.
            Collider = new Hitbox(data.Width, data.Height);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // enable the hook on rainbow spinner hue.
            if (!rainbowSpinnerHueHooked) {
                On.Celeste.CrystalStaticSpinner.GetHue += getRainbowSpinnerHue;
                rainbowSpinnerHueHooked = true;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // if this controller was the last in the scene, disable the hook on rainbow spinner hue.
            if (rainbowSpinnerHueHooked && scene.Tracker.CountEntities<RainbowSpinnerColorAreaController>() <= 1) {
                On.Celeste.CrystalStaticSpinner.GetHue -= getRainbowSpinnerHue;
                rainbowSpinnerHueHooked = false;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            // leaving level; disable the hook on rainbow spinner hue.
            if (rainbowSpinnerHueHooked) {
                On.Celeste.CrystalStaticSpinner.GetHue -= getRainbowSpinnerHue;
                rainbowSpinnerHueHooked = false;
            };
        }

        private static Color getRainbowSpinnerHue(On.Celeste.CrystalStaticSpinner.orig_GetHue orig, CrystalStaticSpinner self, Vector2 position) {
            RainbowSpinnerColorAreaController controller = self.CollideFirst<RainbowSpinnerColorAreaController>();
            if (controller != null) {
                // apply the color from the controller we are in.
                return RainbowSpinnerColorController.getModHue(controller.colors, controller.gradientSize, self.Scene, position);
            } else {
                // we are not in a controller; apply the vanilla color.
                return orig(self, position);
            }
        }
    }
}
