using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/ForegroundReflectionTentacles")]
    [TrackedAs(typeof(ReflectionTentacles))]
    class ForegroundReflectionTentacles : ReflectionTentacles {
        public ForegroundReflectionTentacles() : base() { }
        public ForegroundReflectionTentacles(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void Added(Scene scene) {
            // turn off createdFromLevel to prevent vanilla from spawning ReflectionTentacles.
            DynData<ReflectionTentacles> self = new DynData<ReflectionTentacles>(this);
            bool createdFromLevel = self.Get<bool>("createdFromLevel");
            self["createdFromLevel"] = false;

            // run vanilla code.
            base.Added(scene);

            // restore the createdFromLevel value.
            self["createdFromLevel"] = createdFromLevel;

            // add tentacles like vanilla would, but make them ForegroundReflectionTentacles.
            if (createdFromLevel) {
                for (int i = 1; i < 4; i++) {
                    ForegroundReflectionTentacles reflectionTentacles = new ForegroundReflectionTentacles();
                    reflectionTentacles.Create(self.Get<float>("fearDistance"), self.Get<int>("slideUntilIndex"), i, Nodes);
                    scene.Add(reflectionTentacles);
                }
            }

            // bring all tentacles to the foreground.
            Depth = -1000000 + self.Get<int>("layer");
        }
    }
}
