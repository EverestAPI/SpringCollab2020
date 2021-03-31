﻿using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/StaticPuffer")]
    class StaticPuffer : Puffer {
        public static void Load() {
            IL.Celeste.Puffer.ctor_Vector2_bool += onPufferConstructor;
        }

        public static void Unload() {
            IL.Celeste.Puffer.ctor_Vector2_bool -= onPufferConstructor;
        }

        private static void onPufferConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<SineWave>("Randomize"))) {
                Logger.Log("SpringCollab2020/StaticPuffer", $"Injecting call to unrandomize puffer sine wave at {cursor.Index} in IL for Puffer constructor");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(Puffer).GetField("idleSine", BindingFlags.NonPublic | BindingFlags.Instance));
                cursor.EmitDelegate<Action<Puffer, SineWave>>((self, idleSine) => {
                    if (self is StaticPuffer) {
                        // unrandomize the initial pufferfish position.
                        idleSine.Reset();
                    }
                });
            }
        }

        public StaticPuffer(EntityData data, Vector2 offset) : base(data, offset) {
            // remove the sine wave component so that it isn't updated.
            Get<SineWave>()?.RemoveSelf();

            // offset the horizontal position by a tiny bit.
            // Vanilla puffers have a non-integer position (due to the randomized offset), making it impossible to be boosted downwards,
            // so we want to do the same.
            Position.X += 0.0001f;
        }
    }
}
