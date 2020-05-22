using Celeste.Mod.Entities;
using Celeste.Mod.SpringCollab2020.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/FlagToggleCameraTargetTrigger")]
    class FlagToggleCameraTargetTrigger : CameraTargetTrigger {
        public FlagToggleCameraTargetTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            Add(new FlagToggleComponent(data.Attr("flag"), data.Bool("inverted")));
        }
    }
}
