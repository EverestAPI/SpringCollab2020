using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/RemoveLightSourcesTrigger")]
    [Tracked]
    class RemoveLightSourcesTrigger : Trigger {
        public RemoveLightSourcesTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            IsPersistent = data.Bool("isPersistent", true);
            level = SceneAs<Level>();
        }

        public static void Load() {
            Everest.Events.Level.OnLoadLevel += LevelLoadHandler;
            Everest.Events.Level.OnExit += OnExitHandler;
        }

        public static void Unload() {
            Everest.Events.Level.OnLoadLevel -= LevelLoadHandler;
            Everest.Events.Level.OnExit -= OnExitHandler;
        }

        private static void LevelLoadHandler(Level loadedLevel, Player.IntroTypes playerIntro, bool isFromLoader) {
            if(loadedLevel.Session.GetFlag("lightsDisabled") == true) {
                DisableAllLights(loadedLevel);
                On.Celeste.Level.TransitionTo += TransitionLightSources;
            }
        }

        private static void OnExitHandler(Level exitLevel, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            On.Celeste.Level.TransitionTo -= TransitionLightSources;
        }

        private static void TransitionLightSources(On.Celeste.Level.orig_TransitionTo orig, Level transitionLevel, LevelData next, Vector2 direction) {
            lightSources = new List<Component>();
            bloomSources = new List<Component>();

            DisableAllLights(transitionLevel);
            orig(transitionLevel, next, direction);
        }

        private static void DisableAllLights(Level disableLevel) {
            EntityList entities = disableLevel.Entities;

            foreach (Entity entity in entities) {
                foreach (Component component in entity.Components.ToArray()) {
                    if (component is VertexLight) {
                        lightSources.Add(component);
                        component.Visible = false;
                    }

                    if (component is BloomPoint) {
                        bloomSources.Add(component);
                        component.Visible = false;
                    }
                }
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            level = SceneAs<Level>();

            if (IsPersistent && level.Session.GetFlag("lightsDisabled") == false)
                On.Celeste.Level.TransitionTo += TransitionLightSources;

            if (IsPersistent)
                level.Session.SetFlag("lightsDisabled", true);

            DisableAllLights(level);
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if (IsPersistent || level.Session.GetFlag("lightsDisabled") == true)
                return;

            foreach (Component component in lightSources)
                component.Visible = true;

            foreach (Component component in bloomSources)
                component.Visible = true;
        }

        private static List<Component> lightSources = new List<Component>();

        private static List<Component> bloomSources = new List<Component>();

        private Level level;

        private bool IsPersistent = true;
    }
}
