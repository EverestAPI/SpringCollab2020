using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/dashSpring", "SpringCollab2020/wallDashSpringRight", "SpringCollab2020/wallDashSpringLeft")]
    public class DashSpring : Spring {

        private static FieldInfo playerCanUseInfo = typeof(Spring).GetField("playerCanUse", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo spriteInfo = typeof(Spring).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        private static MethodInfo BounceAnimateInfo = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
        
        public DashSpring(Vector2 position, Orientations orientation, bool playerCanUse)
            : base(position, orientation, playerCanUse) {
            // Only one other player collider is added so it can easily be removed
            Remove(Get<PlayerCollider>());
            Add(new PlayerCollider(OnCollide));
            Sprite sprite = (Sprite) spriteInfo.GetValue(this);
            sprite.Reset(GFX.Game, "objects/SpringCollab2020/dashSpring/");
            sprite.Add("idle", "", 0f, default(int));
            sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
            sprite.Add("disabled", "white", 0.07f);
            sprite.Play("idle");
            sprite.Origin.X = sprite.Width / 2f;
            sprite.Origin.Y = sprite.Height;

        }

        public DashSpring(EntityData data, Vector2 offset)
            : this(data.Position + offset, GetOrientationFromName(data.Name), data.Bool("playerCanUse", true)) {

        }

        public static Orientations GetOrientationFromName(string name) {
            switch (name) {
                case "SpringCollab2020/dashSpring":
                    return Orientations.Floor;
                case "SpringCollab2020/wallDashSpringRight":
                    return Orientations.WallRight;
                case "SpringCollab2020/wallDashSpringLeft":
                    return Orientations.WallLeft;
                default:
                    throw new Exception("Dash Spring name doesn't correlate to a valid Orientation!");
            }
        }

        protected void OnCollide(Player player) {
            if (player.StateMachine.State == 9 || !(bool)playerCanUseInfo.GetValue(this) || !player.DashAttacking) {
                return;
            }
            if (Orientation == Orientations.Floor) {
                if (player.Speed.Y >= 0f) {
                    BounceAnimateInfo.Invoke(this, null);
                    player.SuperBounce(base.Top);
                }
                return;
            }
            if (Orientation == Orientations.WallLeft) {
                if (player.SideBounce(1, base.Right, base.CenterY)) {
                    BounceAnimateInfo.Invoke(this, null);
                }
                return;
            }
            if (Orientation == Orientations.WallRight) {
                if (player.SideBounce(-1, base.Left, base.CenterY)) {
                    BounceAnimateInfo.Invoke(this, null);
                }
                return;
            }
            throw new Exception("Orientation not supported!");
        }
    }
}
