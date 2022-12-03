using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/BadelineBounceDirectionTrigger")]
    [Tracked]
    class BadelineBounceDirectionTrigger : Trigger {
        public static void Load() {
            On.Celeste.Player.FinalBossPushLaunch += onPlayerBadelinePushLaunch;
        }

        public static void Unload() {
            On.Celeste.Player.FinalBossPushLaunch -= onPlayerBadelinePushLaunch;
        }

        private static void onPlayerBadelinePushLaunch(On.Celeste.Player.orig_FinalBossPushLaunch orig, Player self, int dir) {
            // if the player is inside a Badeline Bounce Direction Trigger, mod the bounce direction to be the one we want.
            BadelineBounceDirectionTrigger trigger = self.CollideFirst<BadelineBounceDirectionTrigger>();
            if (trigger != null) {
                dir = trigger.bounceLeft ? -1 : 1;
            }

            orig(self, dir);
        }

        private bool bounceLeft;

        public BadelineBounceDirectionTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            bounceLeft = data.Bool("bounceLeft");
        }
    }
}
