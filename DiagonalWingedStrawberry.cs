using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.SpringCollab2020 {
    class DiagonalWingedStrawberry : Strawberry {
        public DiagonalWingedStrawberry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
            Component[] componentArray = Components.ToArray();

            foreach(Component comp in componentArray) {
                if (comp is DashListener)
                    Components.Remove(comp);
            }
        }

        private void OnDiagDash(Vector2 dir) {
            Console.WriteLine("Diagonal Dash OnDiagDash : Vector is "+dir.X+" "+dir.Y);
        }

        private bool flyingAway;
    }
}
