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

        private bool alreadyTurnedOnOnce = false;

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

            toggle.UpdateFlag();

            if (toggle.Enabled) {
                alreadyTurnedOnOnce = true;
            }
        }

        public override void Update() {
            if (toggle.Enabled) {
                // if we are turning the waterfall on for the first time, compute its height, now that other entities
                // (pools of water, underwater switch controller) have been toggled to match as well.
                if (!alreadyTurnedOnOnce) {
                    DynData<WaterFall> self = new DynData<WaterFall>(this);
                    float height = 8f;
                    Water water = null;
                    Solid solid = null;
                    while (Y + height < SceneAs<Level>().Bounds.Bottom
                        && (water = Scene.CollideFirst<Water>(new Rectangle((int) X, (int) (Y + height), 8, 8))) == null
                        && ((solid = Scene.CollideFirst<Solid>(new Rectangle((int) X, (int) (Y + height), 8, 8))) == null || !solid.BlockWaterfalls)) {

                        height += 8f;
                        solid = null;
                    }
                    if (water != null && !Scene.CollideCheck<Solid>(new Rectangle((int) X, (int) (Y + height), 8, 16))) {
                        enteringSfxEvent = "event:/env/local/waterfall_small_in_deep";
                    } else {
                        enteringSfxEvent = "event:/env/local/waterfall_small_in_shallow";
                    }
                    enteringSfx.Play(enteringSfxEvent);

                    self["height"] = height;
                    self["water"] = water;
                    self["solid"] = solid;

                    alreadyTurnedOnOnce = true;
                }

                // update the entity as usual
                base.Update();
            } else {
                // freeze the entity, but continue updating the component
                toggle.Update();
            }
        }
    }
}
