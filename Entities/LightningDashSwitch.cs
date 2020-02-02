using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/LightningDashSwitch")]

    public class LDashSwitch : Solid {
        public LDashSwitch(EntityData data, Vector2 offset) : this(data.Position + offset, data.Enum("side", Sides.Up), data.Bool("persistent", false), new EntityID(data.Level.Name, data.ID), data.Attr("sprite", "default")) { }

        public LDashSwitch(Vector2 position, Sides side, bool persistent, EntityID id, string spriteName) : base(position, 0f, 0f, true) {
            this.side = side;
            this.persistent = persistent;
            this.id = id;
            mirrorMode = (spriteName != "default");
            Add(sprite = GFX.SpriteBank.Create("DashSwitch_" + spriteName));
            sprite.Play("idle", false, false);
            if (side == Sides.Up || side == Sides.Down) {
                Collider.Width = 16f;
                Collider.Height = 8f;
            } else {
                Collider.Width = 8f;
                Collider.Height = 16f;
            }
            switch (side) {
                case Sides.Up:
                    sprite.Position = new Vector2(8f, 0f);
                    sprite.Rotation = -1.5707964f;
                    pressedTarget = Position + Vector2.UnitY * -8f;
                    pressDirection = -Vector2.UnitY;
                    break;
                case Sides.Down:
                    sprite.Position = new Vector2(8f, 8f);
                    sprite.Rotation = 1.5707964f;
                    pressedTarget = Position + Vector2.UnitY * 8f;
                    pressDirection = Vector2.UnitY;
                    startY = Y;
                    break;
                case Sides.Left:
                    sprite.Position = new Vector2(0f, 8f);
                    sprite.Rotation = 3.1415927f;
                    pressedTarget = Position + Vector2.UnitX * -8f;
                    pressDirection = -Vector2.UnitX;
                    break;
                case Sides.Right:
                    sprite.Position = new Vector2(8f, 8f);
                    sprite.Rotation = 0f;
                    pressedTarget = Position + Vector2.UnitX * 8f;
                    pressDirection = Vector2.UnitX;
                    break;
            }
            OnDashCollide = new DashCollision(OnDashed);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (persistent && SceneAs<Level>().Session.GetFlag(FlagName)) {
                sprite.Play("pushed", false, false);
                Position = pressedTarget - pressDirection * 2f;
                pressed = true;
                Collidable = false;
                Add(new Coroutine(Lightning.RemoveRoutine(SceneAs<Level>(), new Action(RemoveSelf)), true));
            }
        }

        public override void Update() {
            base.Update();
            if (!pressed && side == Sides.Down) {
                Player playerOnTop = GetPlayerOnTop();
                if (playerOnTop != null) {
                    if (playerOnTop.Holding != null) {
                        OnDashed(playerOnTop, Vector2.UnitY);
                    } else {
                        if (speedY < 0f) {
                            speedY = 0f;
                        }
                        speedY = Calc.Approach(speedY, 70f, 200f * Engine.DeltaTime);
                        MoveTowardsY(startY + 2f, speedY * Engine.DeltaTime);
                        if (!playerWasOn) {
                            Audio.Play("event:/game/05_mirror_temple/button_depress", Position);
                        }
                    }
                } else {
                    if (speedY > 0f) {
                        speedY = 0f;
                    }
                    speedY = Calc.Approach(speedY, -150f, 200f * Engine.DeltaTime);
                    MoveTowardsY(startY, -speedY * Engine.DeltaTime);
                    if (playerWasOn) {
                        Audio.Play("event:/game/05_mirror_temple/button_return", Position);
                    }
                }
                playerWasOn = (playerOnTop != null);
            }
        }

        public DashCollisionResults OnDashed(Player player, Vector2 direction) {
            if (!pressed && direction == pressDirection) {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
                sprite.Play("push", false, false);
                pressed = true;
                MoveTo(pressedTarget);
                Collidable = false;
                Position -= pressDirection * 2f;
                Add(new Coroutine(Lightning.RemoveRoutine(SceneAs<Level>(), new Action(RemoveSelf)), true));
            }
            return DashCollisionResults.NormalCollision;
        }

        private string FlagName {
            get {
                return "LDashSwitch_" + id.Key;
            }
        }

        private Sides side;
        private Vector2 pressedTarget;
        private bool pressed;
        private Vector2 pressDirection;
        private float speedY;
        private float startY;
        private bool persistent;
        private EntityID id;
        private bool mirrorMode;
        private bool playerWasOn;
        private Sprite sprite;
        public enum Sides {
            Up,
            Down,
            Left,
            Right
        }
    }
}
