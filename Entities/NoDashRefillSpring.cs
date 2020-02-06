using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/NoDashRefillSpring", "SpringCollab2020/NoDashRefillSpringLeft", "SpringCollab2020/NoDashRefillSpringRight")]
    class NoDashRefillSpring : Spring {
        private static MethodInfo bounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic | BindingFlags.Instance);
        private static object[] noParams = new object[0];

        public NoDashRefillSpring(EntityData data, Vector2 offset)
            : base(data.Position + offset, GetOrientationFromName(data.Name), data.Bool("playerCanUse", true)) {

            // remove the vanilla player collider. this is the one thing we want to mod here.
            foreach(Component component in this) {
                if(component.GetType() == typeof(PlayerCollider)) {
                    Remove(component);
                    break;
                }
            }

            // replace it with our own collider.
            if(data.Bool("playerCanUse", true)) {
                Add(new PlayerCollider(OnCollide));
            }
        }

        private static Orientations GetOrientationFromName(string name) {
            switch (name) {
                case "SpringCollab2020/NoDashRefillSpring":
                    return Orientations.Floor;
                case "SpringCollab2020/NoDashRefillSpringRight":
                    return Orientations.WallRight;
                case "SpringCollab2020/NoDashRefillSpringLeft":
                    return Orientations.WallLeft;
                default:
                    throw new Exception("No Dash Refill Spring name doesn't correlate to a valid Orientation!");
            }
        }


        private void OnCollide(Player player) {
            if (player.StateMachine.State == 9) {
                return;
            }

            // Save dash count. Dashes are reloaded by SideBounce and SuperBounce.
            int originalDashCount = player.Dashes;

            if (Orientation == Orientations.Floor) {
                if (player.Speed.Y >= 0f) {
                    bounceAnimate.Invoke(this, noParams);
                    player.SuperBounce(Top);
                }
            } else if (Orientation == Orientations.WallLeft) {
                if (player.SideBounce(1, Right, CenterY)) {
                    bounceAnimate.Invoke(this, noParams);
                }
            } else if (Orientation == Orientations.WallRight) {
                if (player.SideBounce(-1, Left, CenterY)) {
                    bounceAnimate.Invoke(this, noParams);
                }
            } else {
                throw new Exception("Orientation not supported!");
            }

            // Restore original dash count.
            player.Dashes = originalDashCount;
        }
    }
}
