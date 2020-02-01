using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [Tracked(false)]
    public class CrystalBombDetonatorRenderer : Entity {
        public CrystalBombDetonatorRenderer() {
            list = new List<CrystalBombDetonator>();
            edges = new List<CrystalBombDetonatorRenderer.Edge>();
            Tag = (Tags.Global | Tags.TransitionUpdate);
            Depth = 0;
            Add(new CustomBloom(new Action(OnRenderBloom)));
        }

        public static void Load() {
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
        }

        static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            self.Level.Add(new CrystalBombDetonatorRenderer());
            orig(self);
        }

        public void Track(CrystalBombDetonator block) {
            list.Add(block);
            bool flag = tiles == null;
            if (flag) {
                levelTileBounds = (Scene as Level).TileBounds;
                tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height, false);
            }
            int num = (int) block.X / 8;
            while ((float) num < block.Right / 8f) {
                int num2 = (int) block.Y / 8;
                while ((float) num2 < block.Bottom / 8f) {
                    tiles[num - levelTileBounds.X, num2 - levelTileBounds.Y] = true;
                    num2++;
                }
                num++;
            }
            dirty = true;
        }

        public void Untrack(CrystalBombDetonator block) {
            list.Remove(block);
            bool flag = list.Count <= 0;
            if (flag) {
                tiles = null;
            } else {
                int num = (int) block.X / 8;
                while ((float) num < block.Right / 8f) {
                    int num2 = (int) block.Y / 8;
                    while ((float) num2 < block.Bottom / 8f) {
                        tiles[num - levelTileBounds.X, num2 - levelTileBounds.Y] = false;
                        num2++;
                    }
                    num++;
                }
            }
            dirty = true;
        }

        public override void Update() {
            bool flag = dirty;
            if (flag) {
                RebuildEdges();
            }
            UpdateEdges();
        }

        public void UpdateEdges() {
            Camera camera = (Scene as Level).Camera;
            Rectangle rectangle = new Rectangle((int) camera.Left - 4, (int) camera.Top - 4, (int) (camera.Right - camera.Left) + 8, (int) (camera.Bottom - camera.Top) + 8);
            for (int i = 0; i < edges.Count; i++) {
                bool visible = edges[i].Visible;
                if (visible) {
                    bool flag = Scene.OnInterval(0.25f, (float) i * 0.01f) && !edges[i].InView(ref rectangle);
                    if (flag) {
                        edges[i].Visible = false;
                    }
                } else {
                    bool flag2 = Scene.OnInterval(0.05f, (float) i * 0.01f) && edges[i].InView(ref rectangle);
                    if (flag2) {
                        edges[i].Visible = true;
                    }
                }
                bool flag3 = edges[i].Visible && (Scene.OnInterval(0.05f, (float) i * 0.01f) || edges[i].Wave == null);
                if (flag3) {
                    edges[i].UpdateWave(Scene.TimeActive * 3f);
                }
            }
        }

        private void RebuildEdges() {
            dirty = false;
            edges.Clear();
            bool flag = list.Count > 0;
            if (flag) {
                Level level = Scene as Level;
                int left = level.TileBounds.Left;
                int top = level.TileBounds.Top;
                int right = level.TileBounds.Right;
                int bottom = level.TileBounds.Bottom;
                Point[] array = new Point[]
                {
                    new Point(0, -1),
                    new Point(0, 1),
                    new Point(-1, 0),
                    new Point(1, 0)
                };
                foreach (CrystalBombDetonator crystalBombDetonator in list) {
                    int num = (int) crystalBombDetonator.X / 8;
                    while ((float) num < crystalBombDetonator.Right / 8f) {
                        int num2 = (int) crystalBombDetonator.Y / 8;
                        while ((float) num2 < crystalBombDetonator.Bottom / 8f) {
                            foreach (Point point in array) {
                                Point point2 = new Point(-point.Y, point.X);
                                bool flag2 = !Inside(num + point.X, num2 + point.Y) && (!Inside(num - point2.X, num2 - point2.Y) || Inside(num + point.X - point2.X, num2 + point.Y - point2.Y));
                                if (flag2) {
                                    Point point3 = new Point(num, num2);
                                    Point point4 = new Point(num + point2.X, num2 + point2.Y);
                                    Vector2 value = new Vector2(4f) + new Vector2((float) (point.X - point2.X), (float) (point.Y - point2.Y)) * 4f;
                                    while (Inside(point4.X, point4.Y) && !Inside(point4.X + point.X, point4.Y + point.Y)) {
                                        point4.X += point2.X;
                                        point4.Y += point2.Y;
                                    }
                                    Vector2 a = new Vector2((float) point3.X, (float) point3.Y) * 8f + value - crystalBombDetonator.Position;
                                    Vector2 b = new Vector2((float) point4.X, (float) point4.Y) * 8f + value - crystalBombDetonator.Position;
                                    edges.Add(new CrystalBombDetonatorRenderer.Edge(crystalBombDetonator, a, b));
                                }
                            }
                            num2++;
                        }
                        num++;
                    }
                }
            }
        }

        private bool Inside(int tx, int ty) {
            return tiles[tx - levelTileBounds.X, ty - levelTileBounds.Y];
        }

        private void OnRenderBloom() {
            Camera camera = (Scene as Level).Camera;
            Rectangle rectangle = new Rectangle((int) camera.Left, (int) camera.Top, (int) (camera.Right - camera.Left), (int) (camera.Bottom - camera.Top));
            foreach (CrystalBombDetonator crystalBombDetonator in list) {
                bool flag = !crystalBombDetonator.Visible;
                if (!flag) {
                    Draw.Rect(crystalBombDetonator.X, crystalBombDetonator.Y, crystalBombDetonator.Width, crystalBombDetonator.Height, Color.Purple);
                }
            }
            foreach (CrystalBombDetonatorRenderer.Edge edge in edges) {
                bool flag2 = !edge.Visible;
                if (!flag2) {
                    Vector2 value = edge.Parent.Position + edge.A;
                    Vector2 vector = edge.Parent.Position + edge.B;
                    int num = 0;
                    while ((float) num <= edge.Length) {
                        Vector2 vector2 = value + edge.Normal * (float) num;
                        Draw.Line(vector2, vector2 + edge.Perpendicular * edge.Wave[num], Color.Purple);
                        num++;
                    }
                }
            }
        }

        public override void Render() {
            bool flag = list.Count <= 0;
            if (!flag) {
                Color color = Color.Purple * 0.45f;
                Color value = Color.Purple * 0.55f;
                foreach (CrystalBombDetonator crystalBombDetonator in list) {
                    bool flag2 = !crystalBombDetonator.Visible;
                    if (!flag2) {
                        Draw.Rect(crystalBombDetonator.Collider, color);
                    }
                }
                bool flag3 = edges.Count > 0;
                if (flag3) {
                    foreach (CrystalBombDetonatorRenderer.Edge edge in edges) {
                        bool flag4 = !edge.Visible;
                        if (!flag4) {
                            Vector2 value2 = edge.Parent.Position + edge.A;
                            Vector2 vector = edge.Parent.Position + edge.B;
                            Color color2 = Color.Lerp(value, Color.Purple, edge.Parent.Flash);
                            int num = 0;
                            while ((float) num <= edge.Length) {
                                Vector2 vector2 = value2 + edge.Normal * (float) num;
                                Draw.Line(vector2, vector2 + edge.Perpendicular * edge.Wave[num], color);
                                num++;
                            }
                        }
                    }
                }
            }
        }

        private List<CrystalBombDetonator> list;
        private List<CrystalBombDetonatorRenderer.Edge> edges;
        private VirtualMap<bool> tiles;
        private Rectangle levelTileBounds;
        private bool dirty;

        private class Edge {
            public Edge(CrystalBombDetonator parent, Vector2 a, Vector2 b) {
                Parent = parent;
                Visible = true;
                A = a;
                B = b;
                Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
                Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
                Normal = (b - a).SafeNormalize();
                Perpendicular = -Normal.Perpendicular();
                Length = (a - b).Length();
            }

            public void UpdateWave(float time) {
                bool flag = Wave == null || (float) Wave.Length <= Length;
                if (flag) {
                    Wave = new float[(int) Length + 2];
                }
                int num = 0;
                while ((float) num <= Length) {
                    Wave[num] = GetWaveAt(time, (float) num, Length);
                    num++;
                }
            }

            private float GetWaveAt(float offset, float along, float length) {
                bool flag = along <= 1f || along >= length - 1f;
                float result;
                if (flag) {
                    result = 0f;
                } else {
                    bool flag2 = Parent.Solidify >= 1f;
                    if (flag2) {
                        result = 0f;
                    } else {
                        float num = offset + along * 0.25f;
                        float num2 = (float) (Math.Sin((double) num) * 2.0 + Math.Sin((double) (num * 0.25f)));
                        result = (1f + num2 * Ease.SineInOut(Calc.YoYo(along / length))) * (1f - Parent.Solidify);
                    }
                }
                return result;
            }

            public bool InView(ref Rectangle view) {
                return (float) view.Left < Parent.X + Max.X && (float) view.Right > Parent.X + Min.X && (float) view.Top < Parent.Y + Max.Y && (float) view.Bottom > Parent.Y + Min.Y;
            }

            public CrystalBombDetonator Parent;
            public bool Visible;
            public Vector2 A;
            public Vector2 B;
            public Vector2 Min;
            public Vector2 Max;
            public Vector2 Normal;
            public Vector2 Perpendicular;
            public float[] Wave;
            public float Length;
        }
    }
}
