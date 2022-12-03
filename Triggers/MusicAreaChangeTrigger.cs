using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/MusicAreaChangeTrigger")]
    class MusicAreaChangeTrigger : Trigger {
        private string musicParam1;
        private string musicParam2;
        private string musicParam3;
        private float enterValue;
        private float exitValue;

        // this is used specifically for the expert lobby. On enter, it sets all given  parameter values
        // to the enter value. On exit, it sets all parameter values to exit. Individual exit/enter values were not necessary so they were not added.
        // this is hastily written code that could likely be refactored into a more widely useful trigger for FMOD'ers

        public MusicAreaChangeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            musicParam1 = data.Attr("musicParam1");
            musicParam2 = data.Attr("musicParam2");
            musicParam3 = data.Attr("musicParam3");
            enterValue = data.Float("enterValue", 1f);
            exitValue = data.Float("exitValue", 0f);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (!string.IsNullOrEmpty(musicParam1)) {
                Audio.SetMusicParam(musicParam1, enterValue);
            }
            if (!string.IsNullOrEmpty(musicParam2)) {
                Audio.SetMusicParam(musicParam2, enterValue);
            }
            if (!string.IsNullOrEmpty(musicParam3)) {
                Audio.SetMusicParam(musicParam3, enterValue);
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if (!string.IsNullOrEmpty(musicParam1)) {
                Audio.SetMusicParam(musicParam1, exitValue);
            }
            if (!string.IsNullOrEmpty(musicParam2)) {
                Audio.SetMusicParam(musicParam2, exitValue);
            }
            if (!string.IsNullOrEmpty(musicParam3)) {
                Audio.SetMusicParam(musicParam3, exitValue);
            }
        }
    }
}
