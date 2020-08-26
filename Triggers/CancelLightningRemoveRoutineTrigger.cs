using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/CancelLightningRemoveRoutineTrigger")]
    class CancelLightningRemoveRoutineTrigger : Trigger {
        private static MethodInfo lightningSetBreakValue = typeof(Lightning).GetMethod("SetBreakValue", BindingFlags.Static | BindingFlags.NonPublic);

        public CancelLightningRemoveRoutineTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (Scene.Entities.FirstOrDefault(entity => entity is LightningBreakerBox && entity.TagCheck(Tags.Persistent))
                is LightningBreakerBox box) {

                // break routine is in progress! delete the box running it, and reset the "break value" to zero.
                box.Remove(box.Get<Coroutine>());
                box.RemoveSelf();
                lightningSetBreakValue.Invoke(null /* static method */, new object[] { Scene as Level, 0f });
            }

            RemoveSelf();
        }
    }
}
