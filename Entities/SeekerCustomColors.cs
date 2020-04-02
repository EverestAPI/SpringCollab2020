using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/SeekerCustomColors")]
    [TrackedAs(typeof(Seeker))]
    class SeekerCustomColors : Seeker {
        public static void Load() {
            On.Celeste.Seeker.GotBouncedOn += modGotBouncedOn;
        }

        public static void Unload() {
            On.Celeste.Seeker.GotBouncedOn -= modGotBouncedOn;
        }

        private static void modGotBouncedOn(On.Celeste.Seeker.orig_GotBouncedOn orig, Seeker self, Entity entity) {
            // if the seeker is one with custom colors, switch out the "stomp" particle that will be used by vanilla code.
            bool hasCustomColor = self is SeekerCustomColors;
            if (hasCustomColor) {
                P_Stomp = (self as SeekerCustomColors).pStomp;
            }

            orig(self, entity);

            // restore the stomp particle.
            if (hasCustomColor) {
                P_Stomp = basePStomp;
            }
        }

        private static ParticleType basePAttack;
        private static ParticleType basePHitWall;
        private static ParticleType basePStomp;
        private static ParticleType basePRegen;
        private static Color baseTrailColor;

        private ParticleType pAttack; // blinks between 99e550 and ddffbc
        private ParticleType pHitWall; // blinks between 99e550 and ddffbc (same as Attack)
        private ParticleType pStomp; // copies pAttack
        private ParticleType pRegen; // blinks between cbdbfc and 575fd9
        private Color trailColor;

        private DynData<Seeker> self;

        public SeekerCustomColors(EntityData data, Vector2 offset) : base(data, offset) {
            if (basePAttack == null) {
                basePAttack = P_Attack;
                basePHitWall = P_HitWall;
                basePStomp = P_Stomp;
                basePRegen = P_Regen;
                baseTrailColor = TrailColor;
            }

            pAttack = new ParticleType(P_Attack) {
                Color = Calc.HexToColor(data.Attr("attackParticleColor1")),
                Color2 = Calc.HexToColor(data.Attr("attackParticleColor2"))
            };
            pHitWall = new ParticleType(P_HitWall) {
                Color = Calc.HexToColor(data.Attr("attackParticleColor1")),
                Color2 = Calc.HexToColor(data.Attr("attackParticleColor2"))
            };
            pStomp = new ParticleType(P_Stomp) {
                Color = Calc.HexToColor(data.Attr("attackParticleColor1")),
                Color2 = Calc.HexToColor(data.Attr("attackParticleColor2"))
            };
            pRegen = new ParticleType(P_Regen) {
                Color = Calc.HexToColor(data.Attr("regenParticleColor1")),
                Color2 = Calc.HexToColor(data.Attr("regenParticleColor2"))
            };
            trailColor = Calc.HexToColor(data.Attr("trailColor"));

            self = new DynData<Seeker>(this);
        }

        public override void Update() {
            P_Attack = pAttack;
            P_HitWall = pHitWall;
            P_Regen = pRegen;
            self["TrailColor"] = trailColor;

            base.Update();

            P_Attack = basePAttack;
            P_HitWall = basePHitWall;
            P_Regen = basePRegen;
            self["TrailColor"] = baseTrailColor;
        }


    }
}
