using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/DiagonalWingedStrawberry")]
    class DiagonalWingedStrawberry : Strawberry {
        public DiagonalWingedStrawberry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
            Component[] componentArray = Components.ToArray();
            OriginalOnDash = typeof(Strawberry).GetMethod("OnDash", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

            foreach (Component comp in componentArray) {
                if (comp is DashListener)
                    Components.Remove(comp);
            }

            base.Add(new DashListener {
                OnDash = new Action<Vector2>(OnDiagDash)
            });
        }

        public static void Load() {
            MethodInfo mapDataLoad = typeof(MapData).GetMethod("orig_Load", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo mapGetStrawberries = typeof(MapData).GetMethod("GetStrawberries", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo levelDataCtor = typeof(LevelData).GetMethod("orig_ctor", BindingFlags.Instance | BindingFlags.Public);

            MapDataLoadHook = new ILHook(mapDataLoad, SearchModdedStrawberries);
            StrawberryDetectHook = new ILHook(levelDataCtor, SearchModdedStrawberries);
        }

        public static void Unload() {
            MapDataLoadHook.Dispose();
            StrawberryDetectHook.Dispose();
        }

        private static void SearchModdedStrawberries(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdstr("strawberry"))) {
                cursor.Remove();
                cursor.Goto(cursor.Next);
                cursor.Remove();
                cursor.EmitDelegate<Func<string, bool>>(CountModdedBerries);
            }
        }

        private static bool CountModdedBerries(string berryName) {
            if (berryName == "strawberry" || berryName == "SpringCollab2020/DiagonalWingedStrawberry")
                return true;

            return false;
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

        private MethodInfo OriginalOnDash;

        static ILHook MapDataLoadHook;

        static ILHook StrawberryDetectHook;
    }
}
