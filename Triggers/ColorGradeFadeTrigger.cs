using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/ColorGradeFadeTrigger")]
    [Tracked]
    class ColorGradeFadeTrigger : Trigger {
        public static void Load() {
            On.Celeste.Level.Update += onLevelUpdate;
        }

        public static void Unload() {
            On.Celeste.Level.Update -= onLevelUpdate;
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            orig(self);

            // check if the player is in a color grade fade trigger
            Player player = self.Tracker.GetEntity<Player>();
            ColorGradeFadeTrigger trigger;
            if (player != null && (trigger = player.CollideFirst<ColorGradeFadeTrigger>()) != null) {
                DynData<Level> selfData = new DynData<Level>(self);

                // the game fades from lastColorGrade to Session.ColorGrade using colorGradeEase as a lerp value.
                // let's hijack that!
                float positionLerp = trigger.GetPositionLerp(player, trigger.direction);
                if (positionLerp > 0.5f) {
                    // we are closer to B. let B be the target color grade when player exits the trigger / dies in it
                    selfData["lastColorGrade"] = trigger.colorGradeA;
                    self.Session.ColorGrade = trigger.colorGradeB;
                    selfData["colorGradeEase"] = MathHelper.Clamp(positionLerp, 0.001f, 0.999f);
                } else {
                    // we are closer to A. let A be the target color grade when player exits the trigger / dies in it
                    selfData["lastColorGrade"] = trigger.colorGradeB;
                    self.Session.ColorGrade = trigger.colorGradeA;
                    selfData["colorGradeEase"] = MathHelper.Clamp(1 - positionLerp, 0.001f, 0.999f);
                }
                selfData["colorGradeEaseSpeed"] = 1f;
            }
        }


        private string colorGradeA;
        private string colorGradeB;
        private PositionModes direction;

        public ColorGradeFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            colorGradeA = data.Attr("colorGradeA");
            colorGradeB = data.Attr("colorGradeB");
            direction = data.Enum<PositionModes>("direction");
        }
    }
}
