using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/bubblePushField")]
    class BubblePushField : Entity {
        public enum ActivationMode {
            Always, OnlyWhenFlagActive, OnlyWhenFlagInactive
        }

        public float Strength;

        public float UpwardStrength;

        private Dictionary<WindMover, float> WindMovers = new Dictionary<WindMover, float>();

        private HitboxlessWater Water;

        private bool _water;

        // This is public so that bubble particles can use it so that their positions actually change.
        public Random Rand;

        private int FramesSinceSpawn = 0;

        private int SpawnFrame = 30;

        public PushDirection Direction;

        private ActivationMode activationMode;
        private string flag;

        public BubblePushField(EntityData data, Vector2 offset) : this(
            data.Position + offset,
            data.Width,
            data.Height,
            data.Float("strength", 1f),
            data.Float("upwardStrength", 1f),
            data.Attr("direction", "right"),
            data.Bool("water", true),
            data.Enum("activationMode", ActivationMode.Always),
            data.Attr("flag", "bubble_push_field")
            ) { }

        public BubblePushField(Vector2 position, int width, int height, float strength, float upwardStrength, string direction, bool water, ActivationMode activationMode, string flag) {
            Position = position;
            Strength = strength;
            UpwardStrength = upwardStrength;
            _water = water;
            this.activationMode = activationMode;
            this.flag = flag;

            Rand = new Random();

            Enum.TryParse(direction, out Direction);

            Collider = new Hitbox(width, height);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (_water)
                scene.Add(Water = new HitboxlessWater(Position, true, true, Width, Height));
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            if (_water)
                scene.Remove(Water);
        }

        public override void Render() {
            base.Render();
        }

        public override void Update() {
            base.Update();

            Session session = SceneAs<Level>().Session;
            if ((activationMode == ActivationMode.OnlyWhenFlagActive && !session.GetFlag(flag))
                || (activationMode == ActivationMode.OnlyWhenFlagInactive && session.GetFlag(flag))) {

                // the bubble push field is currently turned off by a session flag.
                return;
            }

            FramesSinceSpawn++;
            if (FramesSinceSpawn == SpawnFrame) {
                FramesSinceSpawn = 0;
                SpawnFrame = Rand.Next(2, 10);
                Add(new BubbleParticle(true, true));
            }

            foreach (WindMover mover in Scene.Tracker.GetComponents<WindMover>()) {
                if(mover.Entity.CollideCheck(this)) {
                    if (WindMovers.ContainsKey(mover))
                        WindMovers[mover] = Calc.Approach(WindMovers[mover], Strength, Engine.DeltaTime / .6f);
                    else
                        WindMovers.Add(mover, 0f);
                } else {
                    if (WindMovers.ContainsKey(mover)) {
                        WindMovers[mover] = Calc.Approach(WindMovers[mover], 0f, Engine.DeltaTime / 0.3f);
                        if (WindMovers[mover] == 0f)
                            WindMovers.Remove(mover);
                    }
                }
            }

            foreach (WindMover mover in WindMovers.Keys) {
                float windSpeed = Strength * 2f * Ease.CubeInOut(WindMovers[mover]);

                if (mover != null && mover.Entity != null && mover.Entity.Scene != null)
                    switch (Direction) {
                        case PushDirection.Up:
                            mover.Move(new Vector2(0, -windSpeed));
                            break;

                        case PushDirection.Down:
                            mover.Move(new Vector2(0, windSpeed));
                            break;

                        case PushDirection.Left:
                            mover.Move(new Vector2(-windSpeed, 0));
                            break;

                        case PushDirection.Right:
                            mover.Move(new Vector2(windSpeed, 0));
                            break;
                    }

                if (mover.Entity is Player && mover.Entity.CollideCheck(this) && Strength > 0 && Direction != PushDirection.Down) {
                    Player tempPlayer = (Player) mover.Entity;
                    if (tempPlayer.Holding != null || tempPlayer.Dead)
                        return;

                    mover.Move(new Vector2(0f, -UpwardStrength));
                }

                if (mover.Entity is Glider && Strength > 0 && Direction != PushDirection.Down) {
                    mover.Move(new Vector2(0f, -UpwardStrength / 20));
                }
            }
        }
    }

    class HitboxlessWater : Water {
        public HitboxlessWater(Vector2 position, bool topSurface, bool bottomSurface, float width, float height)
            : base(position, topSurface, bottomSurface, width, height) {
            Collidable = false;
        }
    }

    class BubbleParticle : Component {
        public Vector2 Position = Vector2.Zero;

        public BubblePushField BubbleField;

        public MTexture Texture;

        private Random Rand;

        private int FramesAlive = 0;

        private int FramesMaxAlive;

        private Vector2 Origin, End;

        private static readonly string[] TextureNames = new string[] { "a", "b" };

        public BubbleParticle(bool active, bool visible) : base(active, visible) { }

        public override void Added(Entity entity) {
            base.Added(entity);

            BubbleField = (BubblePushField) entity;
            Position = BubbleField.Position;

            Rand = BubbleField.Rand;
            Texture = GFX.Game["particles/SpringCollab2020/bubble_" + TextureNames[Rand.Next(0, 1)]];

            // Determine bubble spawn point
            switch (BubbleField.Direction) {
                case PushDirection.Up:
                    Origin = new Vector2(Rand.Range(BubbleField.BottomLeft.X, BubbleField.BottomRight.X), BubbleField.BottomCenter.Y);
                    End = new Vector2(Rand.Range(BubbleField.TopLeft.X, BubbleField.TopRight.X), BubbleField.TopCenter.Y);
                    FramesMaxAlive = (int) Rand.Range(20, BubbleField.Height / BubbleField.Strength * .5f);
                    break;

                case PushDirection.Down:
                    Origin = new Vector2(Rand.Range(BubbleField.TopLeft.X, BubbleField.TopRight.X), BubbleField.TopCenter.Y);
                    End = new Vector2(Rand.Range(BubbleField.BottomLeft.X, BubbleField.BottomRight.X), BubbleField.BottomCenter.Y);
                    FramesMaxAlive = (int) Rand.Range(20, BubbleField.Height / BubbleField.Strength * .5f);
                    break;

                case PushDirection.Right:
                    Origin = new Vector2(BubbleField.CenterLeft.X, Rand.Range(BubbleField.BottomLeft.Y, BubbleField.TopLeft.Y));
                    End = new Vector2(BubbleField.CenterRight.X, Rand.Range(BubbleField.BottomRight.Y, BubbleField.TopRight.Y));
                    FramesMaxAlive = (int) Rand.Range(20, BubbleField.Width / BubbleField.Strength * .5f);
                    break;

                case PushDirection.Left:
                    Origin = new Vector2(BubbleField.CenterRight.X, Rand.Range(BubbleField.BottomRight.Y, BubbleField.TopRight.Y));
                    End = new Vector2(BubbleField.CenterLeft.X, Rand.Range(BubbleField.BottomLeft.Y, BubbleField.TopLeft.Y));
                    FramesMaxAlive = (int) Rand.Range(20, BubbleField.Width / BubbleField.Strength * .5f);
                    break;
            }

            Position = Origin;
        }

        public override void Update() {
            base.Update();

            if (FramesAlive == FramesMaxAlive)
                RemoveSelf();

            FramesAlive++;

            if (BubbleField.Direction == PushDirection.Up || BubbleField.Direction == PushDirection.Down) {
                Position.X = Calc.Approach(Position.X, End.X, BubbleField.Strength / 5);
                Position.Y = Calc.Approach(Position.Y, End.Y, BubbleField.Strength * 2);
            } else {
                Position.X = Calc.Approach(Position.X, End.X, BubbleField.Strength * 2);
                Position.Y = Calc.Approach(Position.Y, End.Y, BubbleField.Strength / 5);
            }
        }

        public override void Render() {
            Texture.DrawCentered(Position);
        }
    }

    public enum PushDirection {
        Left,
        Right,
        Up,
        Down
    }
}
