using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/invisibleLightSource")]
    class InvisibleLightSource : Entity {
        public InvisibleLightSource(EntityData data, Vector2 position) : base(data.Position) {
            Position = data.Position;

            alpha = data.Float("alpha", 1f);
            radius = data.Float("radius", 48f);
            color = ColorHelper.GetColor(data.Attr("color", "White"));

            bloom = new BloomPoint(alpha, radius);
            light = new VertexLight(color, alpha, data.Int("startFade", 24), data.Int("endFade", 48));

            bloom.Visible = true;
            light.Visible = true;

            Add(bloom);
            Add(light);
        }

        private BloomPoint bloom;

        private VertexLight light;

        private float alpha;

        private float radius;

        private Color color;
    }

    // Cruor made this
    class ColorHelper {
        public static Color GetColor(string color) {
            foreach (PropertyInfo c in colorProps) {
                if (color.Equals(c.Name, System.StringComparison.OrdinalIgnoreCase))
                    return (Color) c.GetValue(new Color(), null);
            }

            try {
                return Calc.HexToColor(color.Replace("#", ""));
            } 
            catch {
                Logger.Log("ColorHelper", "Failed to transform color " + color + ", returning Color.White");
            }

            return Color.White;
        }

        private static PropertyInfo[] colorProps = typeof(Color).GetProperties();
    }
}
