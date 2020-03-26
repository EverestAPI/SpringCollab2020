using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/FlagToggleStarTrackSpinner")]
    class FlagToggleStarTrackSpinner : StarTrackSpinner {
        private FlagToggleComponent toggle;

        public FlagToggleStarTrackSpinner(EntityData data, Vector2 offset) : base(data, offset) {
            Add(toggle = new FlagToggleComponent(data.Attr("flag"), data.Bool("inverted")));
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            toggle.UpdateFlag();
        }

        public override void Update() {
            if (toggle.Enabled) {
                // update the entity as usual
                base.Update();
            } else {
                // freeze the entity, but continue updating the component
                toggle.Update();
            }
        }
    }
}
