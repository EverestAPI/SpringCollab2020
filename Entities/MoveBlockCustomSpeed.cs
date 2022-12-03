using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/MoveBlockCustomSpeed")]
    class MoveBlockCustomSpeed : MoveBlock {
        private static MethodInfo moveBlockController = typeof(MoveBlock).GetMethod("Controller", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo moveBlockTargetSpeed = typeof(MoveBlock).GetField("targetSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

        private float moveSpeed;

        public MoveBlockCustomSpeed(EntityData data, Vector2 offset) : base(data, offset) {
            moveSpeed = data.Float("moveSpeed");

            // remove the Controller coroutine.
            foreach (Component component in this) {
                if (component.GetType() == typeof(Coroutine)) {
                    Remove(component);
                    break;
                }
            }

            // replace it with our own "wrapped" coroutine.
            Add(new Coroutine(controllerWrapper()));
        }

        private IEnumerator controllerWrapper() {
            IEnumerator controller = (IEnumerator) moveBlockController.Invoke(this, new object[0]);

            bool checkNext = false;

            while (controller.MoveNext()) {
                // the target speed is set just AFTER a "yield return 0.2f".
                // if we encounter it, we should check its value and change it if required on the next frame.
                if (controller.Current != null && (float) controller.Current == 0.2f) {
                    checkNext = true;
                } else if (checkNext) {
                    checkNext = false;

                    // if the speed is 60 (block is moving), replace it with our custom speed.
                    if ((float) moveBlockTargetSpeed.GetValue(this) == 60f) {
                        moveBlockTargetSpeed.SetValue(this, moveSpeed);
                    }
                }

                yield return controller.Current;
            }
        }
    }
}
