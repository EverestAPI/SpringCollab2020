using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    // a very hardcoded trigger that is here to persist theme choices between play sessions.
    [CustomEntity("SpringCollab2020/ChangeThemeTrigger")]
    public class ChangeThemeTrigger : Trigger {
        private readonly bool enable;

        public ChangeThemeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            enable = data.Bool("enable", false);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // let's restore the saved theme...
            Level level = Scene as Level;
            string sid = level.Session.Area.GetSID();
            bool enabled = SpringCollab2020Module.Instance.SaveData.ModifiedThemeMaps.Contains(sid);
            if (enabled) {
                switch (sid) {
                    case "SpringCollab2020/3-Advanced/LinjKarma":
                        level.Session.SetFlag("skylinesoff");
                        break;
                    case "SpringCollab2020/3-Advanced/NeoKat":
                        setBloom(level, -0.12f);
                        break;
                    case "SpringCollab2020/3-Advanced/RealVet":
                        level.Session.SetFlag("ignore_darkness_Room1");
                        level.Session.SetFlag("ignore_darkness_Room2");
                        level.Session.SetFlag("ignore_darkness_Room3");
                        level.Session.SetFlag("ignore_darkness_Room4");
                        level.Session.SetFlag("ignore_darkness_Room5");
                        level.Session.SetFlag("ignore_darkness_Strawberry1");
                        level.Session.SetFlag("ignore_darkness_Strawberry2");
                        level.Session.SetFlag("ignore_darkness_Strawberry3");
                        level.Session.SetFlag("ignore_darkness_BeginningRoom");
                        level.Session.SetFlag("ignore_darkness_DebugRoom");
                        level.Session.SetFlag("ignore_darkness_EndRoom");
                        level.Session.LightingAlphaAdd = 0.3f;

                        // and no, the current room is not dark.
                        level.Lighting.Alpha = level.BaseLightingAlpha + level.Session.LightingAlphaAdd;
                        level.DarkRoom = false;
                        break;
                    case "SpringCollab2020/4-Expert/Mun":
                        level.Session.SetFlag("darkmode");
                        level.SnapColorGrade("panicattack");
                        setBloom(level, 0.3f);
                        level.Session.LightingAlphaAdd = 0.25f;
                        level.Lighting.Alpha = level.BaseLightingAlpha + level.Session.LightingAlphaAdd;
                        break;
                    case "SpringCollab2020/4-Expert/Zerex":
                        level.Session.SetFlag("darker");
                        setBloom(level, 0.65f);
                        break;
                    case "SpringCollab2020/5-Grandmaster/BobDole":
                        level.Session.SetFlag("boblight");
                        level.SnapColorGrade("bobgrade");
                        break;
                }
            } else {
                switch (sid) {
                    case "SpringCollab2020/3-Advanced/NeoKat":
                        level.Session.SetFlag("sc2020_nyoom_normalmode");
                        break;
                    case "SpringCollab2020/4-Expert/Zerex":
                        setBloom(level, 2.5f);
                        break;
                }
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // let's restore the saved theme...
            Level level = Scene as Level;
            string sid = level.Session.Area.GetSID();
            bool enabled = SpringCollab2020Module.Instance.SaveData.ModifiedThemeMaps.Contains(sid);
            if (enabled) {
                switch (sid) {
                    case "SpringCollab2020/1-Beginner/DanTKO":
                        triggerTrigger("LightningColorTrigger", -456, 32);
                        triggerTrigger("ExtendedVariantTrigger", -456, 32);

                        level.Session.LightingAlphaAdd = 0.095f;
                        level.Lighting.Alpha = level.BaseLightingAlpha + level.Session.LightingAlphaAdd;
                        break;
                }
            } else {
                switch (sid) {
                    case "SpringCollab2020/1-Beginner/DanTKO":
                        triggerTrigger("ExtendedVariantTrigger", -448, 136);
                        break;
                }
            }
        }

        private void triggerTrigger(string triggerName, int x, int y) {
            foreach (Trigger trigger in Scene.Tracker.GetEntities<Trigger>()) {
                if (trigger.GetType().Name.Contains(triggerName) && trigger.X == x && trigger.Y == y) {
                    trigger.OnEnter(Scene.Tracker.GetEntity<Player>());
                    trigger.OnLeave(Scene.Tracker.GetEntity<Player>());
                    break;
                }
            }
        }

        private void setBloom(Level level, float bloomAdd) {
            level.Session.BloomBaseAdd = bloomAdd;
            level.Bloom.Base = AreaData.Get(level).BloomBase + bloomAdd;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (enable) {
                SpringCollab2020Module.Instance.SaveData.ModifiedThemeMaps.Add((Scene as Level).Session.Area.GetSID());
            } else {
                SpringCollab2020Module.Instance.SaveData.ModifiedThemeMaps.Remove((Scene as Level).Session.Area.GetSID());
            }
        }
    }
}
