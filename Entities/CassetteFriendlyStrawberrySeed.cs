using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    /// <summary>
    /// Strawberry seeds that deal nicer with being hidden in cassette blocks.
    /// They appear when the cassette block disappears, and keep a normal hitbox.
    /// </summary>
    class CassetteFriendlyStrawberrySeed : StrawberrySeed {

        bool isInCassetteBlock;

        public CassetteFriendlyStrawberrySeed(Strawberry strawberry, Vector2 position, int index, bool ghost)
            : base(strawberry, position, index, ghost) { }

        public override void Added(Scene scene) {
            if (scene.Entities.OfType<CassetteBlock>().Any(block => block.Collider.Bounds.Contains(Collider.Bounds))) {
                // our seed is entirely inside a cassette block: look for the static mover.
                foreach (Component component in this) {
                    if (component is StaticMover mover) {
                        Remove(mover);
                        Depth = 11; // just below active cassette blocks
                        isInCassetteBlock = true;
                        break;
                    }
                }
            }

            base.Added(scene);
        }

        public override void Update() {
            base.Update();

            if (isInCassetteBlock && !Visible) {
                Depth = 11; // reset depth when losing the seed
            }
        }
    }
}
