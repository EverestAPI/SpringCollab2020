using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/FlagToggleWater")]
    [TrackedAs(typeof(Water))]
    class FlagToggleWater : Water {
        private FlagToggleComponent toggle;

        public FlagToggleWater(EntityData data, Vector2 offset) : base(data, offset) {
            // look up the displacement renderer to be able to toggle it.
            DisplacementRenderHook displacementHook = null;
            foreach (Component component in this) {
                if (component is DisplacementRenderHook displacement) {
                    displacementHook = displacement;
                    break;
                }
            }

            // when the water is toggled, toggle the displacement as well.
            Add(toggle = new FlagToggleComponent(data.Attr("flag"),
                () => Remove(displacementHook), () => Add(displacementHook)));
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
