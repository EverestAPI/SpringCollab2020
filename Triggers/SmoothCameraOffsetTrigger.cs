using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    // A Camera Offset Trigger lerping the offset depending on the player position.
    [CustomEntity("SpringCollab2020/SmoothCameraOffsetTrigger")]
    class SmoothCameraOffsetTrigger : Trigger {

        private Vector2 offsetFrom;
        private Vector2 offsetTo;
        private PositionModes positionMode;
        private bool onlyOnce;

        public SmoothCameraOffsetTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            // parse the trigger attributes. Multiplying X dimensions by 48 and Y ones by 32 replicates the vanilla offset trigger behavior.
            offsetFrom = new Vector2(data.Float("offsetXFrom") * 48f, data.Float("offsetYFrom") * 32f);
            offsetTo = new Vector2(data.Float("offsetXTo") * 48f, data.Float("offsetYTo") * 32f);
            positionMode = data.Enum<PositionModes>("positionMode");
            onlyOnce = data.Bool("onlyOnce");
        }

        public override void OnStay(Player player) {
            base.OnStay(player);
            SceneAs<Level>().CameraOffset = Vector2.Lerp(offsetFrom, offsetTo, GetPositionLerp(player, positionMode));
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if(onlyOnce) {
                RemoveSelf();
            }
        }
    }
}
