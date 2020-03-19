using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/FlagToggleWaterfall")]
    class FlagToggleWaterfall : WaterFall {
        private FlagToggleComponent toggle;
        private SoundSource loopingSfx;
        private SoundSource enteringSfx;
        private string loopingSfxEvent;
        private string enteringSfxEvent;

        public FlagToggleWaterfall(EntityData data, Vector2 offset) : base(data, offset) {
            Add(toggle = new FlagToggleComponent(data.Attr("flag"), data.Bool("inverted"), () => {
                // disable the waterfall sound.
                loopingSfx.Stop();
                enteringSfx.Stop();
            }, () => {
                // enable it again.
                loopingSfx.Play(loopingSfxEvent);
                enteringSfx.Play(enteringSfxEvent);
            }));
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // store the values for these private variables.
            DynData<WaterFall> self = new DynData<WaterFall>(this);
            loopingSfx = self.Get<SoundSource>("loopingSfx");
            enteringSfx = self.Get<SoundSource>("enteringSfx");
            loopingSfxEvent = loopingSfx.EventName;
            enteringSfxEvent = enteringSfx.EventName;
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
