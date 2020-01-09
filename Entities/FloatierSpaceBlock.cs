using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/floatierSpaceBlock")]
    public class FloatierSpaceBlock : FloatySpaceBlock {
        public float floatinessBoost;

        private static FieldInfo sinkTimerInfo = typeof(FloatySpaceBlock).GetField("sinkTimer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo yLerpInfo = typeof(FloatySpaceBlock).GetField("yLerp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo sineWaveInfo = typeof(FloatySpaceBlock).GetField("sineWave", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo dashEaseInfo = typeof(FloatySpaceBlock).GetField("dashEase", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo dashDirectionInfo = typeof(FloatySpaceBlock).GetField("dashDirection", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        //private static MethodInfo MoveToTargetInfo = typeof(FloatySpaceBlock).GetMethod("MoveToTarget", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

        public FloatierSpaceBlock(EntityData data, Vector2 offset) : base(data, offset) {
            floatinessBoost = data.Float("floatinessMultiplier", 1);
        }

        public static void Load() {
            On.Monocle.Tracker.Initialize += InitializeTracker;
        }

        public static void Unload() {
            On.Monocle.Tracker.Initialize -= InitializeTracker;
        }

        private static void InitializeTracker(On.Monocle.Tracker.orig_Initialize orig) {
            orig();
            Tracker.TrackedEntityTypes[typeof(FloatierSpaceBlock)].Add(typeof(FloatySpaceBlock));
        }

        public override void Update() {
            float oldYLerp = (float) yLerpInfo.GetValue(this);
            base.Update();
            if (MasterOfGroup) {
                bool flag = false;
                foreach (FloatySpaceBlock item in Group) {
                    if (item.HasPlayerRider()) {
                        flag = true;
                        break;
                    }
                }
                if (!flag) {
                    foreach (JumpThru jumpthru in Jumpthrus) {
                        if (jumpthru.HasPlayerRider()) {
                            flag = true;
                            break;
                        }
                    }
                }
                float sinkTimerVal = (float) sinkTimerInfo.GetValue(this);
                if (flag) {
                    sinkTimerInfo.SetValue(this, sinkTimerVal = 0.3f * floatinessBoost);
                } else if (sinkTimerVal > 0f) {
                    sinkTimerInfo.SetValue(this, sinkTimerVal -= Engine.DeltaTime);
                }
                if (sinkTimerVal > 0f) {
                    yLerpInfo.SetValue(this, Calc.Approach(oldYLerp, 1f, (1f / floatinessBoost) * Engine.DeltaTime));
                } else {
                    yLerpInfo.SetValue(this, Calc.Approach(oldYLerp, 0f, (1f / floatinessBoost) * Engine.DeltaTime));
                }
                MoveToTarget();
            }
            LiftSpeed = Vector2.Zero;
        }

        private void MoveToTarget() {
            float num = (float) Math.Sin((float)sineWaveInfo.GetValue(this)) * 4f;
            Vector2 vector = Calc.YoYo(Ease.QuadIn((float)dashEaseInfo.GetValue(this))) * (Vector2)dashDirectionInfo.GetValue(this) * 8f;
            for (int i = 0; i < 2; i++) {
                foreach (KeyValuePair<Platform, Vector2> move in Moves) {
                    Platform key = move.Key;
                    bool flag = false;
                    Solid solid = key as Solid;
                    if ((key is JumpThru jumpThru && jumpThru.HasRider()) || (solid != null && solid.HasRider())) {
                        flag = true;
                    }
                    if ((flag || i != 0) && (!flag || i != 1)) {
                        Vector2 value = move.Value;
                        // This is the important line
                        float num2 = MathHelper.Lerp(value.Y, value.Y + 12f * floatinessBoost, Ease.SineInOut((float) yLerpInfo.GetValue(this))) + num;
                        key.MoveToY(num2 + vector.Y);
                        key.MoveToX(value.X + vector.X);
                    }
                }
            }
        }
    }
}
