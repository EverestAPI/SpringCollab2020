using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/variableCrumbleBlock")]
    public class VariableCrumblePlatform : Solid {
        public static ParticleType P_Crumble = CrumblePlatform.P_Crumble;

        private List<Image> images;

        private List<Image> outline;

        private List<Coroutine> falls;

        private List<int> fallOrder;

        private ShakerList shaker;

        private LightOcclude occluder;

        private Coroutine outlineFader;

        public float crumbleTime = 0.4f;

        public float respawnTime = 2f;

        private string overrideTexture;

        public VariableCrumblePlatform(Vector2 position, float width, string overrideTexture, float timer, float respawnTimer)
            : base(position, width, 8f, false) {
            EnableAssistModeChecks = false;
            this.overrideTexture = overrideTexture;
            crumbleTime = timer;
            respawnTime = respawnTimer;
        }

        public VariableCrumblePlatform(EntityData data, Vector2 offset)
            : this(data.Position + offset, (float) data.Width, data.Attr("texture"), data.Float("timer", 0.4f), data.Float("respawnTimer", 2f)) {
        }

        public override void Added(Scene scene) {
            AreaData areaData = AreaData.Get(scene);
            string crumbleBlock = areaData.CrumbleBlock;
            if (overrideTexture != null) {
                areaData.CrumbleBlock = overrideTexture;
            }
            base.Added(scene);
            MTexture mTexture = GFX.Game["objects/crumbleBlock/outline"];
            outline = new List<Image>();
            if (base.Width <= 8f) {
                Image image = new Image(mTexture.GetSubtexture(24, 0, 8, 8));
                image.Color = Color.White * 0f;
                Add(image);
                outline.Add(image);
            } else {
                for (int i = 0; (float) i < base.Width; i += 8) {
                    int num = (i != 0) ? ((i > 0 && (float) i < base.Width - 8f) ? 1 : 2) : 0;
                    Image image2 = new Image(mTexture.GetSubtexture(num * 8, 0, 8, 8));
                    image2.Position = new Vector2(i, 0f);
                    image2.Color = Color.White * 0f;
                    Add(image2);
                    outline.Add(image2);
                }
            }
            Add(outlineFader = new Coroutine());
            outlineFader.RemoveOnComplete = false;
            images = new List<Image>();
            falls = new List<Coroutine>();
            fallOrder = new List<int>();
            MTexture mTexture2 = GFX.Game["objects/crumbleBlock/" + AreaData.Get(scene).CrumbleBlock];
            for (int j = 0; (float) j < base.Width; j += 8) {
                int num2 = (int) ((Math.Abs(base.X) + (float) j) / 8f) % 4;
                Image image3 = new Image(mTexture2.GetSubtexture(num2 * 8, 0, 8, 8));
                image3.Position = new Vector2(4 + j, 4f);
                image3.CenterOrigin();
                Add(image3);
                images.Add(image3);
                Coroutine coroutine = new Coroutine();
                coroutine.RemoveOnComplete = false;
                falls.Add(coroutine);
                Add(coroutine);
                fallOrder.Add(j / 8);
            }
            fallOrder.Shuffle();
            Add(new Coroutine(Sequence()));
            Add(shaker = new ShakerList(images.Count, false, delegate (Vector2[] v) {
                for (int k = 0; k < images.Count; k++) {
                    images[k].Position = new Vector2(4 + k * 8, 4f) + v[k];
                }
            }));
            Add(occluder = new LightOcclude(0.2f));
            areaData.CrumbleBlock = crumbleBlock;
        }

        private IEnumerator Sequence() {
            while (true) {
                Player player = GetPlayerOnTop();
                bool onTop;
                if (player != null) {
                    onTop = true;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                } else {
                    player = GetPlayerClimbing();
                    if (player == null) {
                        yield return null;
                        continue;
                    }
                    onTop = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                Audio.Play("event:/game/general/platform_disintegrate", Center);
                shaker.ShakeFor(onTop ? crumbleTime + 0.4f : crumbleTime + 0.8f, removeOnFinish: false);
                foreach (Image img2 in images) {
                    SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + img2.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                }
                for (int l = 0; l < (onTop ? 1 : 3); l++) {
                    yield return 0.2f;
                    foreach (Image img in images) {
                        SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + img.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                    }
                }
                float timer = crumbleTime;
                if (onTop) {
                    while (timer > 0f && GetPlayerOnTop() != null) {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                } else {
                    while (timer > 0f) {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                }
                outlineFader.Replace(OutlineFade(1f));
                occluder.Visible = false;
                Collidable = false;
                float delay = 0.05f;
                for (int m = 0; m < 4; m++) {
                    for (int i = 0; i < images.Count; i++) {
                        if (i % 4 - m == 0) {
                            falls[i].Replace(TileOut(images[fallOrder[i]], delay * (float) m));
                        }
                    }
                }
                yield return respawnTime;
                while (CollideCheck<Actor>() || CollideCheck<Solid>()) {
                    yield return null;
                }
                outlineFader.Replace(OutlineFade(0f));
                occluder.Visible = true;
                Collidable = true;
                for (int k = 0; k < 4; k++) {
                    for (int j = 0; j < images.Count; j++) {
                        if (j % 4 - k == 0) {
                            falls[j].Replace(TileIn(j, images[fallOrder[j]], 0.05f * (float) k));
                        }
                    }
                }
            }
        }

        private IEnumerator OutlineFade(float to) {
            float from = 1f - to;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f) {
                Color color = Color.White * (from + (to - from) * Ease.CubeInOut(t));
                foreach (Image img in outline) {
                    img.Color = color;
                }
                yield return null;
            }
        }

        private IEnumerator TileOut(Image img, float delay) {
            img.Color = Color.Gray;
            yield return delay;
            float distance = (img.X * 7f % 3f + 1f) * 12f;
            Vector2 from = img.Position;
            for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.4f) {
                yield return null;
                img.Position = from + Vector2.UnitY * Ease.CubeIn(time) * distance;
                img.Color = Color.Gray * (1f - time);
                img.Scale = Vector2.One * (1f - time * 0.5f);
            }
            img.Visible = false;
        }

        private IEnumerator TileIn(int index, Image img, float delay) {
            yield return delay;
            Audio.Play("event:/game/general/platform_return", Center);
            img.Visible = true;
            img.Color = Color.White;
            img.Position = new Vector2(index * 8 + 4, 4f);
            for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.25f) {
                yield return null;
                img.Scale = Vector2.One * (1f + Ease.BounceOut(1f - time) * 0.2f);
            }
            img.Scale = Vector2.One;
        }
    }
}