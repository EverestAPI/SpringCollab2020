using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/SpeedBasedMusicParamTrigger")]
    class SpeedBasedMusicParamTrigger : Trigger {
        public static void Load() {
            On.Celeste.Player.Update += onPlayerUpdate;
        }

        public static void Unload() {
            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            AudioState audio = self.SceneAs<Level>().Session.Audio;
            float playerSpeed = self.Speed.Length();

            // set all the speed-based music params to their corresponding values.
            foreach (KeyValuePair<string, SpringCollab2020Session.SpeedBasedMusicParamInfo> musicParam
                in SpringCollab2020Module.Instance.Session.ActiveSpeedBasedMusicParams) {

                audio.Music.Param(musicParam.Key, MathHelper.Clamp(playerSpeed, musicParam.Value.MinimumSpeed, musicParam.Value.MaximumSpeed));
            }

            audio.Apply();
        }

        private string paramName;
        private float minSpeed;
        private float maxSpeed;
        private bool activate;

        public SpeedBasedMusicParamTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            paramName = data.Attr("paramName");
            minSpeed = data.Float("minSpeed");
            maxSpeed = data.Float("maxSpeed");
            activate = data.Bool("activate");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (activate) {
                // register the speed-based music param as active to keep it updated.
                SpringCollab2020Module.Instance.Session.ActiveSpeedBasedMusicParams[paramName] = new SpringCollab2020Session.SpeedBasedMusicParamInfo() {
                    MinimumSpeed = minSpeed,
                    MaximumSpeed = maxSpeed
                };
            } else {
                // unregister the param, and set the music param to the minimum value.
                SpringCollab2020Module.Instance.Session.ActiveSpeedBasedMusicParams.Remove(paramName);

                AudioState audio = SceneAs<Level>().Session.Audio;
                audio.Music.Param(paramName, minSpeed);
                audio.Apply();
            }
        }
    }
}
