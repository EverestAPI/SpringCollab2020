using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    /// <summary>
    /// Mashup of vanilla RisingLava and SandwichLava, allowing for lava coming from the sides instead of from the top / bottom.
    ///
    /// Attributes:
    /// - intro: if true, lava will be invisible until the player moves
    /// - lavaMode: allows picking the lava direction (left to right, right to left, or sandwich)
    /// - speedMultiplier: multiplies the vanilla speed for lava
    /// </summary>
    [CustomEntity("SpringCollab2020/SidewaysLava")]
    public class SidewaysLava : Entity {
        private static FieldInfo lavaBlockerTriggerEnabled = typeof(LavaBlockerTrigger).GetField("enabled", BindingFlags.NonPublic | BindingFlags.Instance);

        private enum LavaMode {
            LeftToRight, RightToLeft, Sandwich
        }

        private const float Speed = 30f;

        // if the player collides with one of those, lava should be forced into waiting.
        private List<LavaBlockerTrigger> lavaBlockerTriggers;

        // atrributes
        private bool intro;
        private LavaMode lavaMode;
        private float speedMultiplier;

        // state keeping
        private bool iceMode;
        private bool waiting;
        private float delay = 0f;
        private float lerp;

        // sandwich-specific stuff
        private bool sandwichLeaving = false;
        private float sandwichTransitionStartX = 0;
        private bool sandwichHasToSetPosition = false;
        private bool sandwichTransferred = false;

        private SidewaysLavaRect leftRect;
        private SidewaysLavaRect rightRect;

        private SoundSource loopSfx;

        public static Color[] Hot = new Color[3] {
            Calc.HexToColor("ff8933"),
            Calc.HexToColor("f25e29"),
            Calc.HexToColor("d01c01")
        };

        public static Color[] Cold = new Color[3] {
            Calc.HexToColor("33ffe7"),
            Calc.HexToColor("4ca2eb"),
            Calc.HexToColor("0151d0")
        };

        public SidewaysLava(bool intro, string lavaMode, float speedMultiplier) : this(new EntityData() {
            Values = new Dictionary<string, object>() {
                { "intro", intro }, { "lavaMode", lavaMode }, { "speedMultiplier", speedMultiplier }
            }
        }, Vector2.Zero) { }

        public SidewaysLava(EntityData data, Vector2 offset) {
            intro = data.Bool("intro", false);
            lavaMode = data.Enum("lavaMode", LavaMode.LeftToRight);
            speedMultiplier = data.Float("speedMultiplier", 1f);

            Depth = -1000000;

            if (lavaMode == LavaMode.LeftToRight) {
                // one hitbox on the left.
                Collider = new Hitbox(340f, 200f, -340f);
            } else if (lavaMode == LavaMode.RightToLeft) {
                // one hitbox on the right.
                Collider = new Hitbox(340f, 200f, 320f);
            } else {
                // hitboxes on both sides, 280px apart.
                Collider = new ColliderList(new Hitbox(340f, 200f, -340f), new Hitbox(340f, 200f, 280f));
            }

            Visible = false;
            Add(new PlayerCollider(OnPlayer));
            Add(new CoreModeListener(OnChangeMode));
            Add(loopSfx = new SoundSource());

            if (lavaMode != LavaMode.RightToLeft) {
                // add the left lava rect, just off-screen (it is 340px wide)
                Add(leftRect = new SidewaysLavaRect(340f, 200f, 4, SidewaysLavaRect.OnlyModes.OnlyLeft));
                leftRect.Position = new Vector2(-340f, 0f);
                leftRect.SmallWaveAmplitude = 2f;
            }
            if (lavaMode != LavaMode.LeftToRight) {
                // add the right lava rect, just off-screen (the screen is 320px wide)
                Add(rightRect = new SidewaysLavaRect(340f, 200f, 4, SidewaysLavaRect.OnlyModes.OnlyRight));
                rightRect.Position = new Vector2(lavaMode == LavaMode.Sandwich ? 280f : 320f, 0f);
                rightRect.SmallWaveAmplitude = 2f;
            }

            if (lavaMode == LavaMode.Sandwich) {
                // listen to transitions since we need the sandwich lava to deal smoothly with them.
                Add(new TransitionListener {
                    OnOutBegin = () => {
                        sandwichTransitionStartX = X;
                        if (!sandwichTransferred) {
                            // the next screen has no sideways sandwich lava. so, just leave.
                            AddTag(Tags.TransitionUpdate);
                            sandwichLeaving = true;
                            Collidable = false;
                            Alarm.Set(this, 2f, () => RemoveSelf());
                        } else {
                            sandwichTransferred = false;

                            // look up for all lava blocker triggers in the next room.
                            lavaBlockerTriggers = Scene.Entities.OfType<LavaBlockerTrigger>().ToList();
                        }
                    },
                    OnOut = progress => {
                        if (Scene != null) {
                            Level level = Scene as Level;

                            // make sure the sandwich lava is following the transition.
                            Y = level.Camera.Y - 10f;

                            if (!sandwichLeaving) {
                                // make the lava smoothly go back to 20px on each side.
                                X = MathHelper.Lerp(sandwichTransitionStartX, level.Camera.Left + 20f, progress);
                            }
                        }
                        if (progress > 0.95f && sandwichLeaving) {
                            // destroy the lava, since transition is almost done.
                            RemoveSelf();
                        }
                    }
                });
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            iceMode = (SceneAs<Level>().Session.CoreMode == Session.CoreModes.Cold);
            loopSfx.Play("event:/game/09_core/rising_threat", "room_state", iceMode ? 1 : 0);

            if (lavaMode == LavaMode.LeftToRight) {
                // make the lava off-screen by 16px.
                X = SceneAs<Level>().Bounds.Left - 16;
                // sound comes from the left side.
                loopSfx.Position = new Vector2(0f, Height / 2f);

            } else if (lavaMode == LavaMode.RightToLeft) {
                // same, except the lava is offset by 320px. That gives Right - 320 + 16 = Right - 304.
                X = SceneAs<Level>().Bounds.Right - 304;
                // sound comes from the right side.
                loopSfx.Position = new Vector2(320f, Height / 2f);

            } else {
                // the position should be set on the first Update call, in case the level starts with a room with lava in it
                // and the camera doesn't really exist yet.
                sandwichHasToSetPosition = true;
                // sound comes from the middle.
                loopSfx.Position = new Vector2(140f, Height / 2f);
            }

            Y = SceneAs<Level>().Bounds.Top - 10;

            if (lavaMode == LavaMode.Sandwich) {
                // check if another sandwich lava is already here.
                List<SidewaysLava> sandwichLavas = new List<SidewaysLava>(Scene.Entities.FindAll<SidewaysLava>()
                    .Where(lava => (lava as SidewaysLava).lavaMode == LavaMode.Sandwich));

                bool didRemoveSelf = false;
                if (sandwichLavas.Count >= 2) {
                    SidewaysLava otherLava = (sandwichLavas[0] == this) ? sandwichLavas[1] : sandwichLavas[0];
                    if (!otherLava.sandwichLeaving) {
                        // just let the existing lava do the job. transfer settings to it.
                        otherLava.speedMultiplier = speedMultiplier;
                        otherLava.sandwichTransferred = true;
                        RemoveSelf();
                        didRemoveSelf = true;
                    }
                }

                if (!didRemoveSelf) {
                    // we should make ourselves persistent to handle transitions smoothly.
                    Tag = Tags.Persistent;

                    if ((scene as Level).LastIntroType != Player.IntroTypes.Respawn) {
                        // both rects start from off-screen, and fade in.
                        leftRect.Position.X -= 60f;
                        rightRect.Position.X += 60f;
                    } else {
                        // start directly visible if we respawned in the room (likely from dying in it).
                        Visible = true;
                    }
                }
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (intro || (Scene.Tracker.GetEntity<Player>()?.JustRespawned ?? false)) {
                // wait for the player to move before starting.
                waiting = true;
            }

            if (intro) {
                Visible = true;
            }

            // look up for all lava blocker triggers in the room.
            lavaBlockerTriggers = scene.Entities.OfType<LavaBlockerTrigger>().ToList();
        }

        private void OnChangeMode(Session.CoreModes mode) {
            iceMode = (mode == Session.CoreModes.Cold);
            loopSfx.Param("room_state", iceMode ? 1 : 0);
        }

        private void OnPlayer(Player player) {
            int direction; // 1 if right lava was hit, -1 is left lava was hit.
            if (lavaMode == LavaMode.LeftToRight) {
                direction = -1;
            } else if (lavaMode == LavaMode.RightToLeft) {
                direction = 1;
            } else {
                // determine which side was hit depending on the player position.
                direction = (player.X > X + rightRect.Position.X - 32f) ? 1 : -1;
            }

            if (SaveData.Instance.Assists.Invincible) {
                if (delay <= 0f) {
                    float from = X;
                    float to = X + 48f * direction;
                    player.Speed.X = -200f * direction;

                    player.RefillDash();
                    Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, delegate (Tween t) {
                        X = MathHelper.Lerp(from, to, t.Eased);
                    });
                    delay = 0.5f;
                    loopSfx.Param("rising", 0f);
                    Audio.Play("event:/game/general/assist_screenbottom", player.Position);
                }
            } else {
                player.Die(Vector2.UnitX * -direction);
            }
        }

        public override void Update() {
            if (sandwichHasToSetPosition) {
                sandwichHasToSetPosition = false;

                // should be 20px to the right, so that the right rect is at 300px and both rects have the same on-screen size (20px).
                X = SceneAs<Level>().Camera.Left + 20f;
            }

            delay -= Engine.DeltaTime;
            Y = SceneAs<Level>().Camera.Y - 10f;
            base.Update();
            Visible = true;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                LavaBlockerTrigger collidedTrigger = lavaBlockerTriggers.Find(trigger => player.CollideCheck(trigger));

                if (collidedTrigger != null && (bool) lavaBlockerTriggerEnabled.GetValue(collidedTrigger)) {
                    // player is in a lava blocker trigger and it is enabled; block the lava.
                    waiting = true;
                }
            }

            if (waiting) {
                loopSfx.Param("rising", 0f);

                if (player == null || !player.JustRespawned) {
                    waiting = false;
                } else {
                    // the sandwich lava fade in animation is not handled here.
                    if (lavaMode != LavaMode.Sandwich) {
                        float target;
                        if (lavaMode == LavaMode.LeftToRight) {
                            // stop 32px to the left of the player.
                            target = player.X - 32f;
                        } else {
                            // stop 32px to the right of the player. since lava is offset by 320px, that gives 320 - 32 = 288px.
                            target = player.X - 288f;
                        }

                        if (!intro && player != null && player.JustRespawned) {
                            X = Calc.Approach(X, target, 32f * speedMultiplier * Engine.DeltaTime);
                        }
                    }
                }
            } else {
                if (lavaMode != LavaMode.Sandwich) {
                    // this is the X position around which the speed factor will be set. At this position, speedFactor = 1.
                    float positionThreshold;
                    // the current lava position.
                    float currentPosition;
                    // the direction the lava moves at (1 = right, -1 = left).
                    int direction;

                    if (lavaMode == LavaMode.LeftToRight) {
                        positionThreshold = SceneAs<Level>().Camera.Left + 21f;
                        // if lava is too far away, drag it in.
                        if (Right < positionThreshold - 96f) {
                            Right = positionThreshold - 96f;
                        }
                        currentPosition = Right;
                        direction = 1;
                    } else {
                        positionThreshold = SceneAs<Level>().Camera.Right - 21f;
                        // if lava is too far away, drag it in.
                        if (Left > positionThreshold + 96f) {
                            Left = positionThreshold + 96f;
                        }

                        // note: positionThreshold and currentPosition are negative here because the direction is inversed.
                        positionThreshold *= -1;
                        currentPosition = -Left;
                        direction = -1;
                    }

                    // those constants are just pulled from vanilla * 320 / 180, in an attempt to scale it for horizontal movement.
                    float speedFactor = (currentPosition > positionThreshold) ?
                        Calc.ClampedMap(currentPosition - positionThreshold, 0f, 56f, 1f, 0.5f) :
                        Calc.ClampedMap(currentPosition - positionThreshold, 0f, 170f, 1f, 2f);

                    if (delay <= 0f) {
                        loopSfx.Param("rising", 1f);
                        X += Speed * speedFactor * speedMultiplier * direction * Engine.DeltaTime;
                    }
                } else {
                    // sandwich lava moves at a constant speed depending on core mode.
                    int direction = iceMode ? -1 : 1;
                    loopSfx.Param("rising", 1f);
                    X += 20f * speedMultiplier * direction * Engine.DeltaTime;
                }
            }

            // lerp both lava rects when changing core mode.
            lerp = Calc.Approach(lerp, iceMode ? 1 : 0, Engine.DeltaTime * 4f);

            if (leftRect != null) {
                leftRect.SurfaceColor = Color.Lerp(Hot[0], Cold[0], lerp);
                leftRect.EdgeColor = Color.Lerp(Hot[1], Cold[1], lerp);
                leftRect.CenterColor = Color.Lerp(Hot[2], Cold[2], lerp);
                leftRect.Spikey = lerp * 5f;
                leftRect.UpdateMultiplier = (1f - lerp) * 2f;
                leftRect.Fade = (iceMode ? 128 : 32);
            }

            if (rightRect != null) {
                rightRect.SurfaceColor = Color.Lerp(Hot[0], Cold[0], lerp);
                rightRect.EdgeColor = Color.Lerp(Hot[1], Cold[1], lerp);
                rightRect.CenterColor = Color.Lerp(Hot[2], Cold[2], lerp);
                rightRect.Spikey = lerp * 5f;
                rightRect.UpdateMultiplier = (1f - lerp) * 2f;
                rightRect.Fade = (iceMode ? 128 : 32);
            }

            if (lavaMode == LavaMode.Sandwich) {
                // move lava rects towards their intended positions: -340 (0 - its width) for the left rect, 280 for the right rect.
                // if leaving, move them away quickly instead.
                leftRect.Position.X = Calc.Approach(leftRect.Position.X, -340 + (sandwichLeaving ? -512 : 0), (sandwichLeaving ? 512 : 64) * Engine.DeltaTime);
                rightRect.Position.X = Calc.Approach(rightRect.Position.X, 280 + (sandwichLeaving ? 512 : 0), (sandwichLeaving ? 512 : 64) * Engine.DeltaTime);
            }
        }
    }
}
