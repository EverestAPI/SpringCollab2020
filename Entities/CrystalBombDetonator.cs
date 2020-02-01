using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [Tracked(false)]
    [CustomEntity("SpringCollab2020/crystalBombDetonator")]
    public class CrystalBombDetonator : Solid {
        public CrystalBombDetonator(Vector2 position, float width, float height) : base(position, width, height, false) {
            Flash = 0f;
            Solidify = 0f;
            Flashing = false;
            solidifyDelay = 0f;
            particles = new List<Vector2>();
            adjacent = new List<CrystalBombDetonator>();
            speeds = new float[]
            {
                12f,
                20f,
                40f
            };
            Collidable = false;
            int num = 0;
            while ((float) num < Width * Height / 16f) {
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
                num++;
            }
        }

        public CrystalBombDetonator(EntityData data, Vector2 offset) : this(data.Position + offset, (float) data.Width, (float) data.Height) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            scene.Tracker.GetEntity<CrystalBombDetonatorRenderer>().Track(this);
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            scene.Tracker.GetEntity<CrystalBombDetonatorRenderer>().Untrack(this);
        }

        public override void Update() {
            bool flashing = Flashing;
            if (flashing) {
                Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
                bool flag = Flash <= 0f;
                if (flag) {
                    Flashing = false;
                }
            } else {
                bool flag2 = solidifyDelay > 0f;
                if (flag2) {
                    solidifyDelay -= Engine.DeltaTime;
                } else {
                    bool flag3 = Solidify > 0f;
                    if (flag3) {
                        Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
                    }
                }
            }
            int num = speeds.Length;
            float height = Height;
            int i = 0;
            int count = particles.Count;
            while (i < count) {
                Vector2 value = particles[i] + Vector2.UnitY * speeds[i % num] * Engine.DeltaTime;
                value.Y %= height - 1f;
                particles[i] = value;
                i++;
            }
            base.Update();

            CheckForBombs();
        }

        public void OnTriggerDetonation() {
            Flash = 1f;
            Solidify = 1f;
            solidifyDelay = 1f;
            Flashing = true;
            Scene.CollideInto<CrystalBombDetonator>(new Rectangle((int) X, (int) Y - 2, (int) Width, (int) Height + 4), adjacent);
            Scene.CollideInto<CrystalBombDetonator>(new Rectangle((int) X - 2, (int) Y, (int) Width + 4, (int) Height), adjacent);
            foreach (CrystalBombDetonator crystalBombDetonator in adjacent) {
                if(!crystalBombDetonator.Flashing)
                    crystalBombDetonator.OnTriggerDetonation();
            }
            adjacent.Clear();
        }

        public override void Render() {
            Color color = Color.Yellow * 0.6f;
            foreach (Vector2 value in particles) {
                Draw.Pixel.Draw(Position + value, Vector2.Zero, color);
            }
            if (Flashing)
                Draw.Rect(Collider, Color.Purple * Flash * 0.5f);
        }

        private void CheckForBombs() {
            foreach (Entity bomb in CollideAll<Actor>()) {
                if (bomb.GetType().ToString().Contains("CrystalBomb")) {
                    if (bombExplosionMethod == null)
                        bombExplosionMethod = bomb.GetType().GetMethod("Explode", BindingFlags.Instance | BindingFlags.NonPublic);
                    bombExplosionMethod.Invoke(bomb, null);
                    if (!Flashing)
                        OnTriggerDetonation();
                }
            }
        }

        public float Flash;
        public float Solidify;
        public bool Flashing;
        private float solidifyDelay;
        private List<Vector2> particles;
        private List<CrystalBombDetonator> adjacent;
        private float[] speeds;

        private static MethodInfo bombExplosionMethod;
    }
}
