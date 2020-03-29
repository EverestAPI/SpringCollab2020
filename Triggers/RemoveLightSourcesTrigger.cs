using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/RemoveLightSourcesTrigger")]
    [Tracked]
    class RemoveLightSourcesTrigger : Trigger {

        private static float alphaFade = 1f;

        public static void Load() {
            On.Celeste.LightingRenderer.BeforeRender += LightHook;
            On.Celeste.BloomRenderer.Apply += BloomRendererHook;
            On.Celeste.Level.LoadLevel += OnLoadLevel;
            On.Celeste.Level.Update += OnLevelUpdate;
        }

        public static void Unload() {
            On.Celeste.LightingRenderer.BeforeRender -= LightHook;
            On.Celeste.BloomRenderer.Apply -= BloomRendererHook;
            On.Celeste.Level.LoadLevel -= OnLoadLevel;
            On.Celeste.Level.Update -= OnLevelUpdate;
        }

        private static void OnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                // do not fade and set the alpha right away when spawning into the level.
                alphaFade = SpringCollab2020Module.Instance.Session.LightSourcesDisabled ? 0f : 1f;
            }
        }

        private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            orig(self);

            if (!self.Paused) {
                // progressively fade in or out.
                alphaFade = Calc.Approach(alphaFade, SpringCollab2020Module.Instance.Session.LightSourcesDisabled ? 0f : 1f, Engine.DeltaTime * 3f);
            }
        }

        private static void BloomRendererHook(On.Celeste.BloomRenderer.orig_Apply orig, BloomRenderer self, VirtualRenderTarget target, Scene scene) {
            if (alphaFade < 1f) {
                // set all alphas to 0, and back up original values.
                List<BloomPoint> affectedBloomPoints = new List<BloomPoint>();
                List<float> originalAlpha = new List<float>();
                foreach (BloomPoint bloomPoint in scene.Tracker.GetComponents<BloomPoint>().ToArray()) {
                    if (bloomPoint.Visible && !(bloomPoint.Entity is Payphone)) {
                        affectedBloomPoints.Add(bloomPoint);
                        originalAlpha.Add(bloomPoint.Alpha);
                        bloomPoint.Alpha *= alphaFade;
                    }
                }

                // render the bloom.
                orig(self, target, scene);

                // restore original alphas.
                int index = 0;
                foreach (BloomPoint bloomPoint in affectedBloomPoints) {
                    bloomPoint.Alpha = originalAlpha[index++];
                }
            } else {
                // alpha multiplier is 1: nothing to modify, go on with vanilla.
                orig(self, target, scene);
            }
        }

        private static void LightHook(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene) {
            if (alphaFade < 1f) {
                // set all alphas to 0, and back up original values.
                List<VertexLight> affectedVertexLights = new List<VertexLight>();
                List<float> originalAlpha = new List<float>();
                foreach (VertexLight vertexLight in scene.Tracker.GetComponents<VertexLight>().ToArray()) {
                    if (vertexLight.Visible && !vertexLight.Spotlight) {
                        affectedVertexLights.Add(vertexLight);
                        originalAlpha.Add(vertexLight.Alpha);
                        vertexLight.Alpha *= alphaFade;
                    }
                }

                // render the lighting.
                orig(self, scene);

                // restore original alphas.
                int index = 0;
                foreach (VertexLight vertexLight in affectedVertexLights) {
                    vertexLight.Alpha = originalAlpha[index++];
                }
            } else {
                // alpha multiplier is 1: nothing to modify, go on with vanilla.
                orig(self, scene);
            }
        }

        private bool enableLightSources;
        private bool fade;

        public RemoveLightSourcesTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            enableLightSources = data.Bool("enableLightSources", false);
            fade = data.Bool("fade", false);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            SpringCollab2020Module.Instance.Session.LightSourcesDisabled = !enableLightSources;

            if (!fade) {
                // don't fade; set the fade to its final value right away.
                alphaFade = SpringCollab2020Module.Instance.Session.LightSourcesDisabled ? 0f : 1f;
            }
        }
    }
}
