using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/RetractSpinner")]
    public class RetractSpinner : Entity {

        public bool AttachToSolid;

        private float offset;

        private Image retractedSprite;
        private Image expandedSprite;

        private bool isExpanded = false;

        public RetractSpinner(EntityData data, Vector2 offset) : base(data.Position + offset) {
            // pulled straight from vanilla spinners.
            this.offset = Calc.Random.NextFloat();
            Tag = Tags.TransitionUpdate;
            Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
            Visible = false;
            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            Add(new LedgeBlocker());
            Depth = 1; // just below Madeline, since the default state is retracted
            AttachToSolid = data.Bool("attachToSolid");
            if (AttachToSolid) {
                Add(new StaticMover {
                    OnShake = OnShake,
                    SolidChecker = IsRiding,
                    OnDestroy = RemoveSelf
                });
            }
            int randomSeed = Calc.Random.Next();

            // load the retracted and expanded sprites.
            Calc.PushRandom(randomSeed);
            List<MTexture> expandedVariations = GFX.Game.GetAtlasSubtextures("danger/SpringCollab2020/retractspinner/urchin_harm");
            List<MTexture> retractedVariations = GFX.Game.GetAtlasSubtextures("danger/SpringCollab2020/retractspinner/urchin_safe");
            MTexture expandedTexture = Calc.Random.Choose(expandedVariations);
            MTexture retractedTexture = Calc.Random.Choose(retractedVariations);
            expandedSprite = new Image(expandedTexture).SetOrigin(12f, 12f);
            retractedSprite = new Image(retractedTexture).SetOrigin(12f, 12f);
            Calc.PopRandom();

            // the default state is "retracted". add the matching sprite
            Add(retractedSprite);
        }

        public override void Update() {
            if (!Visible) {
                // check if in view, if so make the spinner visible.
                Collidable = false;
                if (InView()) {
                    Visible = true;
                }
            } else {
                base.Update();

                // hide out-of-view spinners like the vanilla "spinner cycle"
                if (Scene.OnInterval(0.25f, offset) && !InView()) {
                    Visible = false;
                }

                // check if spinners are close enough to the player to get collidable, like the vanilla "spinner cycle"
                if (Scene.OnInterval(0.05f, offset)) {
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    if (entity != null) {
                        // those spinners are expanded only when in contact with water.
                        if (isExpanded != CollideCheck<Water>()) {
                            // the state has to be changed.
                            if (isExpanded) {
                                Remove(expandedSprite);
                                Add(retractedSprite);
                                Depth = 1; // just below Madeline
                            } else {
                                Remove(retractedSprite);
                                Add(expandedSprite);
                                Depth = -8500; // vanilla spinner depth
                            }
                            isExpanded = !isExpanded;
                        }

                        // spinners are collidable if expanded + close enough to the player (same rule as vanilla).
                        Collidable = isExpanded && Math.Abs(entity.X - X) < 128f && Math.Abs(entity.Y - Y) < 128f;
                    }
                }
            }
        }

        // those are pulled straight from vanilla.
        private bool InView() {
            Camera camera = (Scene as Level).Camera;
            return X > camera.X - 16f && Y > camera.Y - 16f && X < camera.X + 320f + 16f && Y < camera.Y + 180f + 16f;
        }

        private void OnShake(Vector2 pos) {
            foreach (Component component in Components) {
                if (component is Image image) {
                    image.Position = pos;
                }
            }
        }

        private bool IsRiding(Solid solid) {
            return CollideCheck(solid);
        }

        private void OnPlayer(Player player) {
            player.Die((player.Position - Position).SafeNormalize());
        }

        private void OnHoldable(Holdable h) {
            h.HitSpinner(this);
        }
    }
}
