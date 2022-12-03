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
        // Duration of flashing anim
        public float Flash = 0f;

        // Used in the shape of the "wave" on the renderer
        public float Solidify = 0f;
        private float solidifyDelay = 0f;

        // Used to propagate state
        private List<CrystalBombDetonator> adjacent = new List<CrystalBombDetonator>();

        // If the field is currently in its flashing state (used to propagate Flashing state to adjacent fields)
        public bool Flashing = false;

        // Particle array and a list of speeds
        private List<Vector2> particles = new List<Vector2>();
        private readonly static float[] speeds = new float[] {
            12f,
            20f,
            40f
        };

        // Cached method reference, first calculated in Update
        private static MethodInfo bombExplosionMethod;

        public CrystalBombDetonator(Vector2 position, float width, float height) : base(position, width, height, false) {
            Collidable = false;
            for (int num = 0; (float) num < Width * Height / 16f; num++)
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
        }

        public CrystalBombDetonator(EntityData data, Vector2 offset) : this(data.Position + offset, (float) data.Width, (float) data.Height) { }

        public override void Added(Scene scene) {
            base.Added(scene);
            scene.Tracker.GetEntity<CrystalBombDetonatorRenderer>().Track(this);
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            scene.Tracker.GetEntity<CrystalBombDetonatorRenderer>().Untrack(this);
        }

        public override void Update() {
            if (Flashing) {
                Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
                if (Flash <= 0f) {
                    Flashing = false;
                }
            } else {
                if (solidifyDelay > 0f)
                    solidifyDelay -= Engine.DeltaTime;
                else if (Solidify > 0f)
                    Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
            }

            for (int i = 0; i < particles.Count; i++) {
                Vector2 newPosition = particles[i] + Vector2.UnitY * speeds[i % speeds.Length] * Engine.DeltaTime;
                newPosition.Y %= Height - 1f;
                particles[i] = newPosition;
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
    }
}
