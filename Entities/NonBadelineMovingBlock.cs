using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/nonBadelineMovingBlock")]
    [Tracked(false)]
    public class NonBadelineMovingBlock : Solid {

        private float startDelay;

        private int nodeIndex;

        private Vector2[] nodes;

        private TileGrid sprite;

        private TileGrid highlight;

        private Coroutine moveCoroutine;

        private bool isHighlighted;

        public NonBadelineMovingBlock(Vector2[] nodes, float width, float height, char tiletype, char highlightTiletype)
            : base(nodes[0], width, height, safe: false) {
            this.nodes = nodes;
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            sprite = GFX.FGAutotiler.GenerateBox(tiletype, (int) base.Width / 8, (int) base.Height / 8).TileGrid;
            Add(sprite);
            Calc.PopRandom();
            Calc.PushRandom(newSeed);
            highlight = GFX.FGAutotiler.GenerateBox(highlightTiletype, (int) (base.Width / 8f), (int) base.Height / 8).TileGrid;
            highlight.Alpha = 0f;
            Add(highlight);
            Calc.PopRandom();
            Add(new TileInterceptor(sprite, highPriority: false));
            Add(new LightOcclude());
        }

        public NonBadelineMovingBlock(EntityData data, Vector2 offset)
            : this(data.NodesWithPosition(offset), data.Width, data.Height, data.Char("tiletype", 'g'), data.Char("highlightTiletype", 'G')) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            StartMoving(0);
        }

        public override void OnShake(Vector2 amount) {
            base.OnShake(amount);
            sprite.Position = amount;
        }

        public void StartMoving(float delay) {
            startDelay = delay;
            Add(moveCoroutine = new Coroutine(MoveSequence()));
        }

        private IEnumerator MoveSequence() {
            while (true) {
                StartShaking(0.2f + startDelay);
                if (!isHighlighted) {
                    for (float p = 0f; p < 1f; p += Engine.DeltaTime / (0.2f + startDelay + 0.2f)) {
                        highlight.Alpha = Ease.CubeIn(p);
                        sprite.Alpha = 1f - highlight.Alpha;
                        yield return null;
                    }
                    highlight.Alpha = 1f;
                    sprite.Alpha = 0f;
                    isHighlighted = true;
                } else {
                    yield return 0.2f + startDelay + 0.2f;
                }
                startDelay = 0f;
                nodeIndex++;
                nodeIndex %= nodes.Length;
                Vector2 from = Position;
                Vector2 to = nodes[nodeIndex];
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.8f, start: true);
                tween.OnUpdate = delegate (Tween t) {
                    MoveTo(Vector2.Lerp(from, to, t.Eased));
                };
                tween.OnComplete = delegate {
                    if (CollideCheck<SolidTiles>(Position + (to - from).SafeNormalize() * 2f)) {
                        Audio.Play("event:/game/06_reflection/fallblock_boss_impact", Center);
                        ImpactParticles(to - from);
                    } else {
                        StopParticles(to - from);
                    }
                };
                Add(tween);
                yield return 0.8f;
            }
        }

        private void StopParticles(Vector2 moved) {
            Level level = SceneAs<Level>();
            float direction = moved.Angle();
            if (moved.X > 0f) {
                Vector2 value = new Vector2(Right - 1f, Top);
                for (int i = 0; i < Height; i += 4) {
                    level.Particles.Emit(FinalBossMovingBlock.P_Stop, value + Vector2.UnitY * (2 + i + Calc.Random.Range(-1, 1)), direction);
                }
            } else if (moved.X < 0f) {
                Vector2 value2 = new Vector2(Left, Top);
                for (int j = 0; j < Height; j += 4) {
                    level.Particles.Emit(FinalBossMovingBlock.P_Stop, value2 + Vector2.UnitY * (2 + j + Calc.Random.Range(-1, 1)), direction);
                }
            }
            if (moved.Y > 0f) {
                Vector2 value3 = new Vector2(Left, Bottom - 1f);
                for (int k = 0; k < Width; k += 4) {
                    level.Particles.Emit(FinalBossMovingBlock.P_Stop, value3 + Vector2.UnitX * (2 + k + Calc.Random.Range(-1, 1)), direction);
                }
            } else if (moved.Y < 0f) {
                Vector2 value4 = new Vector2(Left, Top);
                for (int l = 0; l < Width; l += 4) {
                    level.Particles.Emit(FinalBossMovingBlock.P_Stop, value4 + Vector2.UnitX * (2 + l + Calc.Random.Range(-1, 1)), direction);
                }
            }
        }

        private void BreakParticles() {
            for (int i = 0; i < Width; i += 4) {
                for (int j = 0; j < Height; j += 4) {
                    SceneAs<Level>().Particles.Emit(FinalBossMovingBlock.P_Break, 1, Position + new Vector2(2 + i, 2 + j), Vector2.One * 2f, (Position + new Vector2(2 + i, 2 + j) - Center).Angle());
                }
            }
        }

        private void ImpactParticles(Vector2 moved) {
            if (moved.X < 0f) {
                Vector2 offset = new Vector2(0f, 2f);
                for (int i = 0; i < Height / 8f; i++) {
                    Vector2 collideCheckPos = new Vector2(Left - 1f, Top + 4f + (i * 8));
                    if (!Scene.CollideCheck<Water>(collideCheckPos) && Scene.CollideCheck<Solid>(collideCheckPos)) {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collideCheckPos + offset, 0f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collideCheckPos - offset, 0f);
                    }
                }
            } else if (moved.X > 0f) {
                Vector2 offset = new Vector2(0f, 2f);
                for (int j = 0; j < Height / 8f; j++) {
                    Vector2 collideCheckPos = new Vector2(Right + 1f, Top + 4f + (j * 8));
                    if (!Scene.CollideCheck<Water>(collideCheckPos) && Scene.CollideCheck<Solid>(collideCheckPos)) {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collideCheckPos + offset, (float) Math.PI);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collideCheckPos - offset, (float) Math.PI);
                    }
                }
            }
            if (moved.Y < 0f) {
                Vector2 offset = new Vector2(2f, 0f);
                for (int k = 0; k < Width / 8f; k++) {
                    Vector2 collideCheckPos = new Vector2(Left + 4f + (k * 8), Top - 1f);
                    if (!Scene.CollideCheck<Water>(collideCheckPos) && Scene.CollideCheck<Solid>(collideCheckPos)) {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collideCheckPos + offset, (float) Math.PI / 2f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collideCheckPos - offset, (float) Math.PI / 2f);
                    }
                }
            } else {
                if (!(moved.Y > 0f)) {
                    return;
                }
                Vector2 offset = new Vector2(2f, 0f);
                for (int l = 0; l < Width / 8f; l++) {
                    Vector2 collideCheckPos = new Vector2(Left + 4f + (l * 8), Bottom + 1f);
                    if (!Scene.CollideCheck<Water>(collideCheckPos) && Scene.CollideCheck<Solid>(collideCheckPos)) {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collideCheckPos + offset, -(float) Math.PI / 2f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, collideCheckPos - offset, -(float) Math.PI / 2f);
                    }
                }
            }
        }

        public override void Render() {
            Vector2 position = Position;
            Position += Shake;
            base.Render();
            if (highlight.Alpha > 0f && highlight.Alpha < 1f) {
                int num = (int) ((1f - highlight.Alpha) * 16f);
                Rectangle rect = new Rectangle((int) X, (int) Y, (int) Width, (int) Height);
                rect.Inflate(num, num);
                Draw.HollowRect(rect, Color.Lerp(Color.Purple, Color.Pink, 0.7f));
            }
            Position = position;
        }

        private void Finish() {
            Vector2 from = CenterRight + Vector2.UnitX * 10f;
            for (int i = 0; i < Width / 8f; i++) {
                for (int j = 0; j < Height / 8f; j++) {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), 'f', playSound: true).BlastFrom(from));
                }
            }
            BreakParticles();
            DestroyStaticMovers();
            RemoveSelf();
        }

        public void Destroy(float delay) {
            if (Scene != null) {
                if (moveCoroutine != null) {
                    Remove(moveCoroutine);
                }
                if (delay <= 0f) {
                    Finish();
                    return;
                }
                StartShaking(delay);
                Alarm.Set(this, delay, Finish);
            }
        }
    }
}
