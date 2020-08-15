using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace Celeste.Mod.SpringCollab2020.Entities {
    /// <summary>
    /// This is a replica of the "water zipping fix" from Isa's Grab Bag.
    /// This has side effects so this is technically a bug, but these side effects are used in ProXas's entry
    /// to give insane amounts of speed to the player with small spots of water.
    /// So this should stay in the collab where needed, even if fixed on Isa's side.
    /// </summary>
    public class WaterRocketLaunchingComponent : Component {
        public static void Load() {
            On.Celeste.Player.Added += onPlayerAdded;
        }

        public static void Unload() {
            On.Celeste.Player.Added -= onPlayerAdded;
        }

        private static void onPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig(self, scene);

            string mapSID = self.SceneAs<Level>().Session.Area.GetSID();
            if (!self.Components.Any(component => component.GetType().ToString() == "Celeste.Mod.IsaGrabBag.WaterFix") &&
                (mapSID == "SpringCollab2020/2-Intermediate/ProXas" || mapSID == "SpringCollab2020/2-Intermediate/ZZ-HeartSide")) {

                // Isa's Grab Bag didn't add the "water fix" and the current map needs it, so add it ourselves.
                self.Add(new WaterRocketLaunchingComponent(true, false));
            }
        }

        public WaterRocketLaunchingComponent(bool active, bool visible) : base(active, visible) { }

        private Player player;

        public override void Update() {
            player = Entity as Player;
            if (player == null || !player.Collidable) {
                return;
            }

            Vector2 posOffset = player.Position + player.Speed * Engine.DeltaTime * 2;

            bool isInWater = player.CollideCheck<Water>(posOffset) || player.CollideCheck<Water>(posOffset + Vector2.UnitY * -8f);

            if (!isInWater && player.StateMachine.State == 3 && (player.Speed.Y < 0 || Input.MoveY.Value == -1 || Input.Jump.Check)) {
                player.Speed.Y = (Input.MoveY.Value == -1 || Input.Jump.Check) ? -110 : 0;
                if (player.Speed.Y < -1) {
                    player.Speed.X *= 1.1f;
                }
            }
        }
    }
}
