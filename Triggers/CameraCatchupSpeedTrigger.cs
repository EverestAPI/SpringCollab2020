using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/CameraCatchupSpeedTrigger")]
    [Tracked]
    class CameraCatchupSpeedTrigger : Trigger {
        private static ILHook playerOrigUpdateHook;

        public static void Load() {
            playerOrigUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), modPlayerOrigUpdate);
        }

        public static void Unload() {
            playerOrigUpdateHook?.Dispose();
        }

        private static void modPlayerOrigUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're looking for: 1f - (float)Math.Pow(0.01f / num2, Engine.DeltaTime)
            if (cursor.TryGotoNext(
                instr => instr.MatchLdcR4(0.01f),
                instr => instr.OpCode == OpCodes.Ldloc_S,
                instr => instr.MatchDiv())) {

                // and we want to position the cursor just after loading num2
                cursor.Index += 2;

                Logger.Log("SpringCollab2020/CameraCatchupSpeedTrigger", $"Inserting code to mod camera catchup speed at {cursor.Index} in IL for Player.orig_Update()");

                // this delegate will allow us to turn num2 into something else.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>((orig, self) => {
                    CameraCatchupSpeedTrigger trigger = self.CollideFirst<CameraCatchupSpeedTrigger>();
                    if (trigger != null) {
                        return trigger.catchupSpeed;
                    }
                    return orig;
                });
            }
        }


        private float catchupSpeed;

        public CameraCatchupSpeedTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            catchupSpeed = data.Float("catchupSpeed", 1f);
        }
    }
}
