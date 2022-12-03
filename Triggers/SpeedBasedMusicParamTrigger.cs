using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    /**
     * Port of the speed-based music param trigger from max480's Helping Hand:
     * https://github.com/max4805/MaxHelpingHand/blob/master/Triggers/SpeedBasedMusicParamTrigger.cs
     */
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

            if (SpringCollab2020Module.Instance.Session.ActiveSpeedBasedMusicParams.Count > 0) {
                AudioState audio = self.SceneAs<Level>().Session.Audio;
                float playerSpeed = self.Speed.Length();

                // set all the speed-based music params to their corresponding values.
                foreach (KeyValuePair<string, SpringCollab2020Session.SpeedBasedMusicParamInfo> musicParam
                    in SpringCollab2020Module.Instance.Session.ActiveSpeedBasedMusicParams) {

                    audio.Music.Param(musicParam.Key, Calc.ClampedMap(playerSpeed,
                        musicParam.Value.MinimumSpeed, musicParam.Value.MaximumSpeed,
                        musicParam.Value.MinimumParamValue, musicParam.Value.MaximumParamValue));
                }

                audio.Apply();
            }
        }

        private string paramName;
        private float minSpeed;
        private float maxSpeed;
        private float minParamValue;
        private float maxParamValue;
        private bool activate;

        public SpeedBasedMusicParamTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            paramName = data.Attr("paramName");
            minSpeed = data.Float("minSpeed");
            maxSpeed = data.Float("maxSpeed");
            minParamValue = data.Float("minParamValue");
            maxParamValue = data.Float("maxParamValue");
            activate = data.Bool("activate");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (activate) {
                // register the speed-based music param as active to keep it updated.
                SpringCollab2020Module.Instance.Session.ActiveSpeedBasedMusicParams[paramName] = new SpringCollab2020Session.SpeedBasedMusicParamInfo() {
                    MinimumSpeed = minSpeed,
                    MaximumSpeed = maxSpeed,
                    MinimumParamValue = minParamValue,
                    MaximumParamValue = maxParamValue
                };
            } else {
                // unregister the param, and set the music param to the minimum value.
                SpringCollab2020Module.Instance.Session.ActiveSpeedBasedMusicParams.Remove(paramName);

                AudioState audio = SceneAs<Level>().Session.Audio;
                audio.Music.Param(paramName, minParamValue);
                audio.Apply();
            }
        }
    }
}
