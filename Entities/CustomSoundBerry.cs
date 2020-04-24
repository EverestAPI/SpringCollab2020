using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/CustomSoundBerry")]
    [RegisterStrawberry(true, false)]
    public class CustomSoundBerry : Entity, IStrawberry, IStrawberrySeeded {

        public EntityID ID;

        private Sprite sprite;

        public Follower Follower;

        private Wiggler wiggler;

        private Wiggler rotateWiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Tween lightTween;

        private float wobble = 0f;

        private Vector2 start;

        private float collectTimer = 0f;

        private bool collected = false;

        private bool isGhostBerry;

        private bool flyingAway;

        private float flapSpeed;

        public bool ReturnHomeWhenLost = true;

        private string strawberryWingFlapSound;
        private string strawberryPulseSound;
        private string strawberryBlueTouchSound;
        private string strawberryTouchSound;
        private string strawberryLaughSound;
        private string strawberryFlyAwaySound;
        private string strawberryGetSound;

        public bool WaitingOnSeeds { get; private set; }

        public List<GenericStrawberrySeed> Seeds { get; private set; }

        public bool Winged {
            get;
            private set;
        }

        public bool Golden {
            get;
            private set;
        }

        public bool Moon {
            get;
            private set;
        }

        public string gotSeedFlag => "collected_seeds_of_" + ID.ToString();

        public CustomSoundBerry(EntityData data, Vector2 offset, EntityID gid) {
            ID = gid;
            Position = (start = data.Position + offset);
            Winged = (data.Bool("winged") || data.Name == "memorialTextController");
            Golden = (data.Name == "memorialTextController" || data.Name == "goldenBerry");
            Moon = data.Bool("moon");
            isGhostBerry = SaveData.Instance.CheckStrawberry(ID);
            base.Depth = -100;
            base.Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            Add(Follower = new Follower(ID, null, OnLoseLeader));
            Follower.FollowDelay = 0.3f;
            if (Winged) {
                Add(new DashListener {
                    OnDash = OnDash
                });
            }
            if (data.Nodes != null && data.Nodes.Length != 0) {
                Seeds = new List<GenericStrawberrySeed>();
                for (int i = 0; i < data.Nodes.Length; i++) {
                    Seeds.Add(new GenericStrawberrySeed(this, offset + data.Nodes[i], i, isGhostBerry));
                }
            }

            strawberryWingFlapSound = data.Attr("strawberryWingFlapSound", "event:/game/general/strawberry_wingflap");
            strawberryPulseSound = data.Attr("strawberryPulseSound", "event:/game/general/strawberry_pulse");
            strawberryBlueTouchSound = data.Attr("strawberryBlueTouchSound", "event:/game/general/strawberry_blue_touch");
            strawberryTouchSound = data.Attr("strawberryTouchSound", "event:/game/general/strawberry_touch");
            strawberryLaughSound = data.Attr("strawberryLaughSound", "event:/game/general/strawberry_laugh");
            strawberryFlyAwaySound = data.Attr("strawberryFlyAwaySound", "event:/game/general/strawberry_flyaway");
            strawberryGetSound = data.Attr("strawberryGetSound", "event:/game/general/strawberry_get");
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (SaveData.Instance.CheckStrawberry(ID)) {
                if (Moon) {
                    sprite = GFX.SpriteBank.Create("moonghostberry");
                } else if (Golden) {
                    sprite = GFX.SpriteBank.Create("goldghostberry");
                } else {
                    sprite = GFX.SpriteBank.Create("ghostberry");
                }
                sprite.Color = Color.White * 0.8f;
            } else if (Moon) {
                sprite = GFX.SpriteBank.Create("moonberry");
            } else if (Golden) {
                sprite = GFX.SpriteBank.Create("goldberry");
            } else {
                sprite = GFX.SpriteBank.Create("strawberry");
            }
            Add(sprite);
            if (Winged) {
                sprite.Play("flap");
            }
            sprite.OnFrameChange = OnAnimate;
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate (float v) {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(rotateWiggler = Wiggler.Create(0.5f, 4f, delegate (float v) {
                sprite.Rotation = v * 30f * ((float) Math.PI / 180f);
            }));
            Add(bloom = new BloomPoint((Golden || Moon || isGhostBerry) ? 0.5f : 1f, 12f));
            Add(light = new VertexLight(Color.White, 1f, 16, 24));
            Add(lightTween = light.CreatePulseTween());
            if (Seeds != null && Seeds.Count > 0) {
                Session session = (scene as Level).Session;
                if (!session.GetFlag(gotSeedFlag)) {
                    foreach (GenericStrawberrySeed seed in Seeds) {
                        scene.Add(seed);
                    }
                    Visible = false;
                    Collidable = false;
                    WaitingOnSeeds = true;
                    bloom.Visible = (light.Visible = false);
                }
            }
            if ((scene as Level).Session.BloomBaseAdd > 0.1f) {
                bloom.Alpha *= 0.5f;
            }
        }

        public override void Update() {
            orig_Update();
        }

        private void OnDash(Vector2 dir) {
            if (!flyingAway && Winged && !WaitingOnSeeds) {
                Depth = -1000000;
                Add(new Coroutine(FlyAwayRoutine()));
                flyingAway = true;
            }
        }

        private void OnAnimate(string id) {
            if (!flyingAway && id == "flap" && sprite.CurrentAnimationFrame % 9 == 4) {
                Audio.Play(strawberryWingFlapSound, Position);
                flapSpeed = -50f;
            }
            int num = (id == "flap") ? 25 : (Golden ? 30 : ((!Moon) ? 35 : 30));
            if (sprite.CurrentAnimationFrame == num) {
                lightTween.Start();
                if (!collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>())) {
                    Audio.Play(strawberryPulseSound, Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
                } else {
                    Audio.Play(strawberryPulseSound, Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                }
            }
        }

        public void OnPlayer(Player player) {
            if (Follower.Leader == null && !collected && !WaitingOnSeeds) {
                ReturnHomeWhenLost = true;
                if (Winged) {
                    Level level = SceneAs<Level>();
                    Winged = false;
                    sprite.Rate = 0f;
                    Alarm.Set(this, Follower.FollowDelay, delegate {
                        sprite.Rate = 1f;
                        sprite.Play("idle");
                        level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position + new Vector2(8f, 0f), new Vector2(4f, 2f));
                        level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position - new Vector2(8f, 0f), new Vector2(4f, 2f));
                    });
                }
                if (Golden) {
                    (Scene as Level).Session.GrabbedGolden = true;
                }
                Audio.Play(isGhostBerry ? strawberryBlueTouchSound : strawberryTouchSound, Position);
                player.Leader.GainFollower(Follower);
                wiggler.Start();
                Depth = -1000000;
            }
        }

        public void OnCollect() {
            if (!collected) {
                int collectIndex = 0;
                collected = true;
                if (Follower.Leader != null) {
                    Player player = Follower.Leader.Entity as Player;
                    collectIndex = player.StrawberryCollectIndex;
                    player.StrawberryCollectIndex++;
                    player.StrawberryCollectResetTimer = 2.5f;
                    Follower.Leader.LoseFollower(Follower);
                }
                if (Moon) {
                    Achievements.Register(Achievement.WOW);
                }
                SaveData.Instance.AddStrawberry(ID, Golden);
                Session session = (Scene as Level).Session;
                session.DoNotLoad.Add(ID);
                session.Strawberries.Add(ID);
                session.UpdateLevelStartDashes();
                Add(new Coroutine(CollectRoutine(collectIndex)));
            }
        }

        private IEnumerator FlyAwayRoutine() {
            rotateWiggler.Start();
            flapSpeed = -200f;
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.25f, start: true);
            tween2.OnUpdate = delegate (Tween t) {
                flapSpeed = MathHelper.Lerp(-200f, 0f, t.Eased);
            };
            Add(tween2);
            yield return 0.1f;
            Audio.Play(strawberryLaughSound, Position);
            yield return 0.2f;
            if (!Follower.HasLeader) {
                Audio.Play(strawberryFlyAwaySound, Position);
            }
            tween2 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.5f, start: true);
            tween2.OnUpdate = delegate (Tween t) {
                flapSpeed = MathHelper.Lerp(0f, -200f, t.Eased);
            };
            Add(tween2);
        }

        private IEnumerator CollectRoutine(int collectIndex) {
            _ = (Scene is Level);
            Tag = Tags.TransitionUpdate;
            Depth = -2000010;
            int color = 0;
            if (Moon) {
                color = 3;
            } else if (isGhostBerry) {
                color = 1;
            } else if (Golden) {
                color = 2;
            }
            Audio.Play(strawberryGetSound, Position, "colour", color, "count", collectIndex);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            sprite.Play("collect");
            while (sprite.Animating) {
                yield return null;
            }
            Scene.Add(new StrawberryPoints(Position, isGhostBerry, collectIndex, Moon));
            RemoveSelf();
        }

        private void OnLoseLeader() {
            if (!collected && ReturnHomeWhenLost) {
                Alarm.Set(this, 0.15f, delegate {
                    CustomSoundBerry strawberry = this;
                    Vector2 vector = (start - Position).SafeNormalize();
                    float num = Vector2.Distance(Position, start);
                    float scaleFactor = Calc.ClampedMap(num, 16f, 120f, 16f, 96f);
                    Vector2 control = start + vector * 16f + vector.Perpendicular() * scaleFactor * Calc.Random.Choose(1, -1);
                    SimpleCurve curve = new SimpleCurve(Position, start, control);
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(num / 100f, 0.4f), start: true);
                    tween.OnUpdate = delegate (Tween f) {
                        strawberry.Position = curve.GetPoint(f.Eased);
                    };
                    tween.OnComplete = delegate {
                        Depth = 0;
                    };
                    Add(tween);
                });
            }
        }

        public void CollectedSeeds() {
            WaitingOnSeeds = false;
            Visible = true;
            Collidable = true;
            bloom.Visible = (light.Visible = true);
            (Scene as Level).Session.SetFlag(gotSeedFlag);
        }

        public void orig_Update() {
            if (WaitingOnSeeds) {
                return;
            }
            if (!collected) {
                if (!Winged) {
                    wobble += Engine.DeltaTime * 4f;
                    Sprite obj = sprite;
                    BloomPoint bloomPoint = bloom;
                    float num2 = light.Y = (float) Math.Sin(wobble) * 2f;
                    obj.Y = (bloomPoint.Y = num2);
                }
                int followIndex = Follower.FollowIndex;
                if (Follower.Leader != null && Follower.DelayTimer <= 0f && StrawberryRegistry.IsFirstStrawberry(this)) {
                    bool flag = false;
                    if (Follower.Leader.Entity is Player player && player.Scene != null && !player.StrawberriesBlocked) {
                        if (Golden) {
                            if (player.CollideCheck<GoldBerryCollectTrigger>() || (Scene as Level).Completed) {
                                flag = true;
                            }
                        } else if (player.OnSafeGround && (!Moon || player.StateMachine.State != 13)) {
                            flag = true;
                        }
                    }
                    if (flag) {
                        collectTimer += Engine.DeltaTime;
                        if (collectTimer > 0.15f) {
                            OnCollect();
                        }
                    } else {
                        collectTimer = Math.Min(collectTimer, 0f);
                    }
                } else {
                    if (followIndex > 0) {
                        collectTimer = -0.15f;
                    }
                    if (Winged) {
                        Y += flapSpeed * Engine.DeltaTime;
                        if (flyingAway) {
                            if (Y < (SceneAs<Level>().Bounds.Top - 16)) {
                                RemoveSelf();
                            }
                        } else {
                            flapSpeed = Calc.Approach(flapSpeed, 20f, 170f * Engine.DeltaTime);
                            if (Y < start.Y - 5f) {
                                Y = start.Y - 5f;
                            } else if (Y > start.Y + 5f) {
                                Y = start.Y + 5f;
                            }
                        }
                    }
                }
            }
            base.Update();
            if (Follower.Leader != null && Scene.OnInterval(0.08f)) {
                ParticleType type = isGhostBerry ? Strawberry.P_GhostGlow : (Golden ? Strawberry.P_GoldGlow : ((!Moon) ? Strawberry.P_Glow : Strawberry.P_MoonGlow));
                SceneAs<Level>().ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
            }
        }
    }
}
