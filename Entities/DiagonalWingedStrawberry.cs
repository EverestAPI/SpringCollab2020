using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/diagonalWingedStrawberry")]
    [RegisterStrawberry(true, false)]
    class DiagonalWingedStrawberry : Strawberry {
        public DiagonalWingedStrawberry(EntityData data, Vector2 offset, EntityID gid) : base(FixData(data), offset, gid) {
            OriginalOnDash = typeof(Strawberry).GetMethod("OnDash", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

            // Components.ToArray() is used because the map fails to load when using merely "Components" for some reason.
            foreach (Component comp in Components.ToArray()) {
                if (comp is DashListener)
                    Components.Remove(comp);
            }

            Add(new DashListener {
                OnDash = new Action<Vector2>(OnDiagDash)
            });
        }

        private void OnDiagDash(Vector2 dir) {
            if (CheckDirection(dir))
                OriginalOnDash.Invoke(this, new object[] { dir });
        }

        private bool CheckDirection(Vector2 dir) {
            bool xOk = false;
            bool yOk = false;

            if (Math.Abs(dir.X) - .707 <= .01)
                xOk = true;

            if (Math.Abs(dir.Y) - .707 <= .01 && dir.Y < 0)
                yOk = true;

            if (xOk && yOk)
                return true;

            return false;
        }

        private static EntityData FixData(EntityData data) {
            data.Values["winged"] = true;
            return data;
        }

        private MethodInfo OriginalOnDash;
    }
}
