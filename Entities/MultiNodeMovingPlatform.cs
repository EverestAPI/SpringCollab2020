using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/MultiNodeMovingPlatform")]
    class MultiNodeMovingPlatform : JumpThru {
        private enum Mode {
            BackAndForth, BackAndForthNoPause, Loop, TeleportBack
        }

        // settings
        private Vector2[] nodes;
        private float moveTime;
        private float pauseTime;
        private string overrideTexture;
        private Mode mode;

        private MTexture[] textures;

        private string lastSfx;
        private SoundSource sfx;

        // status tracking
        private float pauseTimer;
        private int prevNodeIndex = 0;
        private int nextNodeIndex = 1;
        private float percent;
        private int direction = 1;

        // sinking effect status tracking
        private float addY;
        private float sinkTimer;

        public MultiNodeMovingPlatform(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, false) {

            // read attributes
            moveTime = data.Float("moveTime", 2f);
            pauseTime = data.Float("pauseTime");
            overrideTexture = data.Attr("texture");
            mode = data.Enum<Mode>("mode");

            // read nodes
            nodes = new Vector2[data.Nodes.Length + 1];
            nodes[0] = data.Position + offset;
            for (int i = 0; i < data.Nodes.Length; i++) {
                nodes[i + 1] = data.Nodes[i] + offset;
            }

            // set up sounds and lighting
            Add(sfx = new SoundSource());
            SurfaceSoundIndex = 5;
            lastSfx = (Math.Sign(nodes[0].X - nodes[1].X) > 0 || Math.Sign(nodes[0].Y - nodes[1].Y) > 0) ?
                "event:/game/03_resort/platform_horiz_left" : "event:/game/03_resort/platform_horiz_right";

            Add(new LightOcclude(0.2f));
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // read the matching texture
            if (overrideTexture == "default") {
                overrideTexture = AreaData.Get(scene).WoodPlatform;
            }
            MTexture platformTexture = GFX.Game["objects/woodPlatform/" + overrideTexture];
            textures = new MTexture[platformTexture.Width / 8];
            for (int i = 0; i < textures.Length; i++) {
                textures[i] = platformTexture.GetSubtexture(i * 8, 0, 8, 8);
            }

            // draw lines between all nodes
            Vector2 lineOffset = new Vector2(Width, Height + 4f) / 2f;
            scene.Add(new MovingPlatformLine(nodes[0] + lineOffset, nodes[1] + lineOffset));
            if (nodes.Length > 2) {
                for (int i = 1; i < nodes.Length - 1; i++) {
                    scene.Add(new MovingPlatformLine(nodes[i] + lineOffset, nodes[i + 1] + lineOffset));
                }

                if (mode == Mode.Loop) {
                    scene.Add(new MovingPlatformLine(nodes[nodes.Length - 1] + lineOffset, nodes[0] + lineOffset));
                }
            }
        }

        public override void Render() {
            textures[0].Draw(Position);
            for (int i = 8; i < Width - 8f; i += 8) {
                textures[1].Draw(Position + new Vector2(i, 0f));
            }
            textures[3].Draw(Position + new Vector2(Width - 8f, 0f));
            textures[2].Draw(Position + new Vector2(Width / 2f - 4f, 0f));
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            sinkTimer = 0.4f;
        }

        public override void Update() {
            base.Update();

            // manage the "sinking" effect when the player is on the platform
            if (HasPlayerRider()) {
                sinkTimer = 0.2f;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            } else if (sinkTimer > 0f) {
                sinkTimer -= Engine.DeltaTime;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            } else {
                addY = Calc.Approach(addY, 0f, 20f * Engine.DeltaTime);
            }

            if (pauseTimer > 0f) {
                // the platform is currently paused at a node.
                pauseTimer -= Engine.DeltaTime;

                // still update the position to be sure to apply addY.
                MoveTo(nodes[prevNodeIndex] + new Vector2(0f, addY));
                return;
            } else {
                if (percent == 0) {
                    // the platform started moving. play sound
                    if (lastSfx == "event:/game/03_resort/platform_horiz_left") {
                        sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_right");
                    } else {
                        sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_left");
                    }
                }

                // move forward...
                percent = Calc.Approach(percent, 1f, Engine.DeltaTime / moveTime);

                if (mode == Mode.BackAndForthNoPause) {
                    if (percent == 1f) {
                        // we reached the last node.
                        prevNodeIndex = direction == 1 ? nodes.Length - 1 : 0;
                        MoveTo(nodes[prevNodeIndex] + new Vector2(0f, addY));

                        // go the other way round now.
                        direction = -direction;
                        percent = 0f;
                        pauseTimer = pauseTime;
                    } else {
                        // compute global progress among all nodes, then lerp between the two right nodes.
                        float progress = MathHelper.Lerp(0, nodes.Length - 1, Ease.SineInOut(direction == 1 ? percent : 1 - percent));
                        int nodeIndex = (int) progress;
                        MoveTo(Vector2.Lerp(nodes[nodeIndex], nodes[nodeIndex + 1], progress - nodeIndex) + new Vector2(0f, addY));
                    }
                } else {
                    // lerp between the previous node and the next one.
                    MoveTo(Vector2.Lerp(nodes[prevNodeIndex], nodes[nextNodeIndex], Ease.SineInOut(percent)) + new Vector2(0f, addY));

                    if (percent == 1f) {
                        // reached the end. start waiting before moving again, and switch the target to the next node.
                        prevNodeIndex = nextNodeIndex;
                        nextNodeIndex = prevNodeIndex + direction;
                        if (nextNodeIndex < 0) {
                            // done moving back, let's move forth again
                            nextNodeIndex = 1;
                            direction = 1;
                        } else if (nextNodeIndex >= nodes.Length) {
                            // reached the last node
                            if (mode == Mode.Loop) {
                                // go to the first node
                                nextNodeIndex = 0;
                            } else if (mode == Mode.TeleportBack) {
                                // go back to the first node instantly.
                                prevNodeIndex = 0;
                                nextNodeIndex = 1;
                            } else if (mode == Mode.BackAndForth) {
                                // start going back
                                nextNodeIndex -= 2;
                                direction = -1;
                            }
                        }
                        percent = 0;
                        pauseTimer = pauseTime;
                    }
                }
            }
        }
    }
}
