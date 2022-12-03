using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/AnimatedJumpthruPlatform")]
    class AnimatedJumpthruPlatform : JumpthruPlatform {
        private string animationPath;
        private float animationDelay;
        private int columns;

        public AnimatedJumpthruPlatform(EntityData data, Vector2 offset) : base(data, offset) {
            columns = data.Width / 8;
            animationPath = data.Attr("animationPath");
            animationDelay = data.Float("animationDelay");
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // clean up all visuals for the jumpthru
            List<Component> componentsToRemove = new List<Component>();
            foreach (Component component in this) {
                if (component is Image) {
                    componentsToRemove.Add(component);
                }
            }
            foreach (Component toRemove in componentsToRemove) {
                toRemove.RemoveSelf();
            }

            for (int i = 0; i < columns; i++) {
                Sprite jumpthruSprite = new Sprite(GFX.Game, "objects/jumpthru/" + animationPath);
                jumpthruSprite.AddLoop("idle", "", animationDelay);
                jumpthruSprite.X = i * 8;
                jumpthruSprite.Play("idle");
                Add(jumpthruSprite);
            }
        }
    }
}
