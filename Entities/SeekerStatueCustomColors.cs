using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/SeekerStatueCustomColors")]
    class SeekerStatueCustomColors : SeekerStatue {
        private static ParticleType basePBreakOut;
        private ParticleType pBreakOut; // chooses between 643e73 and 3e2854

        public SeekerStatueCustomColors(EntityData data, Vector2 offset) : base(data, offset) {
            if (basePBreakOut == null) {
                basePBreakOut = Seeker.P_BreakOut;
            }

            pBreakOut = new ParticleType(Seeker.P_BreakOut) {
                Color = Calc.HexToColor(data.Attr("breakOutParticleColor1")),
                Color2 = Calc.HexToColor(data.Attr("breakOutParticleColor2"))
            };

            new DynData<SeekerStatue>(this).Get<Sprite>("sprite").OnLastFrame = anim => {
                if (anim == "hatch") {
                    Scene.Add(new SeekerCustomColors(data, offset) { Light = { Alpha = 0f } });
                    RemoveSelf();
                }
            };
        }

        public override void Update() {
            Seeker.P_BreakOut = pBreakOut;
            base.Update();
            Seeker.P_BreakOut = basePBreakOut;
        }
    }
}
