using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [Tracked]
    [CustomEntity("SpringCollab2020/GlassBlockOriginal")]
    public class GlassBlockOriginal : Solid {
        private struct Line {
            public Vector2 A;

            public Vector2 B;

            public Line(Vector2 a, Vector2 b) {
                A = a;
                B = b;
            }
        }

        private List<Line> lines = new List<Line>();
        private Color lineColor = Color.White;

        public GlassBlockOriginal(Vector2 position, float width, float height)
            : base(position, width, height, safe: false) {

            Depth = -10000;
            Add(new LightOcclude());
            Add(new MirrorSurface());
            SurfaceSoundIndex = 32;
        }

        public GlassBlockOriginal(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height) { }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            int widthInTiles = (int) Width / 8;
            int heightInTiles = (int) Height / 8;
            AddSide(new Vector2(0f, 0f), new Vector2(0f, -1f), widthInTiles);
            AddSide(new Vector2(widthInTiles - 1, 0f), new Vector2(1f, 0f), heightInTiles);
            AddSide(new Vector2(widthInTiles - 1, heightInTiles - 1), new Vector2(0f, 1f), widthInTiles);
            AddSide(new Vector2(0f, heightInTiles - 1), new Vector2(-1f, 0f), heightInTiles);
        }

        private void AddSide(Vector2 start, Vector2 normal, int tiles) {
            Vector2 vector = new Vector2(0f - normal.Y, normal.X);
            for (int i = 0; i < tiles; i++) {
                if (Open(start + vector * i + normal)) {
                    Vector2 a = (start + vector * i) * 8f + new Vector2(4f) - vector * 4f + normal * 4f;
                    if (!Open(start + vector * (i - 1))) {
                        a -= vector;
                    }
                    for (; i < tiles && Open(start + vector * i + normal); i++) { }

                    Vector2 b = (start + vector * i) * 8f + new Vector2(4f) - vector * 4f + normal * 4f;
                    if (!Open(start + vector * i)) {
                        b += vector;
                    }

                    lines.Add(new Line(a, b));
                }
            }
        }

        private bool Open(Vector2 tile) {
            Vector2 point = new Vector2(X + tile.X * 8f + 4f, base.Y + tile.Y * 8f + 4f);
            if (!Scene.CollideCheck<SolidTiles>(point)) {
                return !Scene.CollideCheck<GlassBlockOriginal>(point);
            }
            return false;
        }

        public override void Render() {
            foreach (Line line in lines) {
                Draw.Line(Position + line.A, Position + line.B, lineColor);
            }
        }
    }
}
