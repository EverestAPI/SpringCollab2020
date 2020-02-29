using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/RemoveLightSourcesTrigger")]
    [Tracked]
    class RemoveLightSourcesTrigger : Trigger {

        private static FieldInfo LightsField;

        private static FieldInfo BloomField;

        private static FieldInfo LightField;

        private static bool HooksEnabled = false;

        public RemoveLightSourcesTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            LightsField = typeof(LightingRenderer).GetField("lights", BindingFlags.NonPublic | BindingFlags.Instance);
            BloomField = typeof(Payphone).GetField("bloom", BindingFlags.NonPublic | BindingFlags.Instance);
            LightField = typeof(Payphone).GetField("light", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void Load() {
            Everest.Events.Level.OnLoadLevel += LevelLoadHandler;
            Everest.Events.Level.OnExit += OnExitHandler;
            On.Celeste.Payphone.Update += PayphoneHook;
        }

        public static void Unload() {
            Everest.Events.Level.OnLoadLevel -= LevelLoadHandler;
            Everest.Events.Level.OnExit -= OnExitHandler;
            On.Celeste.Payphone.Update -= PayphoneHook;
        }

        private static void PayphoneHook(On.Celeste.Payphone.orig_Update orig, Payphone self) {
            orig(self);

            VertexLight tempLight = (VertexLight) LightField.GetValue(self);
            BloomPoint tempBloom = (BloomPoint) BloomField.GetValue(self);
            tempBloom.Visible = tempLight.Visible = !tempLight.Visible;

            if(self.SceneAs<Level>().Session.GetFlag("lightsDisabled")) {
                BloomField.SetValue(self, tempBloom);
                LightField.SetValue(self, tempLight);
            }
        }

        private static void LevelLoadHandler(Level loadedLevel, Player.IntroTypes playerIntro, bool isFromLoader) {
            if (loadedLevel.Session.GetFlag("lightsDisabled"))
                DisableLightRender();
            else
                EnableLightRender();
        }

        private static void OnExitHandler(Level exitLevel, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            EnableLightRender();
        }

        private static void BloomRendererHook(On.Celeste.BloomRenderer.orig_Apply orig, BloomRenderer self, VirtualRenderTarget target, Scene scene) {
            foreach (BloomPoint component in scene.Tracker.GetComponents<BloomPoint>().ToArray()) {
                if (!(component.Entity is Payphone))
                    component.Visible = false;
            }

            orig(self, target, scene);
        }

        private static void LightHook(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene) {
            foreach (VertexLight component in scene.Tracker.GetComponents<VertexLight>().ToArray()) {
                if (!component.Spotlight)
                    component.RemoveSelf();
            }

            LightsField.SetValue(self, new VertexLight[64]);
            orig(self, scene);
        }

        private static void TransitionHook(On.Celeste.Level.orig_TransitionTo orig, Level transitionLevel, LevelData next, Vector2 direction) {
            transitionLevel.Tracker.GetComponents<VertexLight>().ForEach(light => {
                if (!((VertexLight)light).Spotlight)
                    light.RemoveSelf();
            });

            orig(transitionLevel, next, direction);
        }

        private static void EnableLightRender() {
            if (!HooksEnabled)
                return;

            On.Celeste.LightingRenderer.BeforeRender -= LightHook;
            On.Celeste.BloomRenderer.Apply -= BloomRendererHook;
            On.Celeste.Level.TransitionTo -= TransitionHook;
            HooksEnabled = false;
        }

        private static void DisableLightRender() {
            if (HooksEnabled)
                return;

            On.Celeste.LightingRenderer.BeforeRender += LightHook;
            On.Celeste.BloomRenderer.Apply += BloomRendererHook;
            On.Celeste.Level.TransitionTo += TransitionHook;
            HooksEnabled = true;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (SceneAs<Level>().Session.GetFlag("lightsDisabled") == false) {
                SceneAs<Level>().Session.SetFlag("lightsDisabled", true);
                DisableLightRender();
            }
        }
    }
}
