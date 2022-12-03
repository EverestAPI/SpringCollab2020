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
        public float dashEaseMultiplier;
        public float dashOffsetMultiplier;

        private static FieldInfo sinkTimerInfo = typeof(FloatySpaceBlock).GetField("sinkTimer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo yLerpInfo = typeof(FloatySpaceBlock).GetField("yLerp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo sineWaveInfo = typeof(FloatySpaceBlock).GetField("sineWave", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo dashEaseInfo = typeof(FloatySpaceBlock).GetField("dashEase", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo dashDirectionInfo = typeof(FloatySpaceBlock).GetField("dashDirection", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        private static PropertyInfo MasterOfGroupInfo = typeof(FloatySpaceBlock).GetProperty("MasterOfGroup", BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

        public FloatierSpaceBlock(EntityData data, Vector2 offset) : base(data, offset) {
            floatinessBoost = data.Float("floatinessMultiplier", 1);
            dashEaseMultiplier = data.Float("bounceBackMultiplier", 1);
            dashOffsetMultiplier = data.Float("dashOffsetMultiplier", floatinessBoost);
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
            bool masterOfGroup = MasterOfGroup;
            // If MasterOfGroup is false, FloatySpaceBlock does almost nothing in Update()
            MasterOfGroupInfo.SetValue(this, false);
            base.Update();
            MasterOfGroupInfo.SetValue(this, masterOfGroup);

            if (MasterOfGroup) {
                bool playerRiding = false;
                foreach (FloatySpaceBlock item in Group) {
                    if (item.HasPlayerRider()) {
                        playerRiding = true;
                        break;
                    }
                }
                if (!playerRiding) {
                    foreach (JumpThru jumpthru in Jumpthrus) {
                        if (jumpthru.HasPlayerRider()) {
                            playerRiding = true;
                            break;
                        }
                    }
                }
                float sinkTimerVal = (float) sinkTimerInfo.GetValue(this);
                if (playerRiding) {
                    sinkTimerInfo.SetValue(this, sinkTimerVal = 0.3f * floatinessBoost);
                } else if (sinkTimerVal > 0f) {
                    sinkTimerInfo.SetValue(this, sinkTimerVal -= Engine.DeltaTime);
                }
                float yLerpVal = (float)yLerpInfo.GetValue(this);
                if (sinkTimerVal > 0f) {
                    yLerpInfo.SetValue(this, Calc.Approach(yLerpVal, 1f, 1f * Engine.DeltaTime));
                } else {
                    yLerpInfo.SetValue(this, Calc.Approach(yLerpVal, 0f, 1f * Engine.DeltaTime));
                }
                sineWaveInfo.SetValue(this, (float)sineWaveInfo.GetValue(this) + Engine.DeltaTime);
                dashEaseInfo.SetValue(this, Calc.Approach((float)dashEaseInfo.GetValue(this), 0f, Engine.DeltaTime * 1.5f * dashEaseMultiplier));
                MoveToTarget();
            }
            LiftSpeed = Vector2.Zero;
        }

        private void MoveToTarget() {
            float sineWavePos = (float) Math.Sin((float)sineWaveInfo.GetValue(this)) * 4f;
            Vector2 dashOffset = Calc.YoYo(Ease.QuadIn((float)dashEaseInfo.GetValue(this))) * (Vector2)dashDirectionInfo.GetValue(this) * 8f * dashOffsetMultiplier;
            for (int i = 0; i < 2; i++) {
                foreach (KeyValuePair<Platform, Vector2> move in Moves) {
                    Platform key = move.Key;
                    bool flag = (key is JumpThru jumpThru && jumpThru.HasRider()) || (key is Solid solid && solid.HasRider());
                    if ((flag || i != 0) && (!flag || i != 1)) {
                        Vector2 intialPos = move.Value;
                        // This is the important line
                        float bobbingOffset = MathHelper.Lerp(intialPos.Y, intialPos.Y + 12f * floatinessBoost, Ease.SineInOut((float) yLerpInfo.GetValue(this))) + sineWavePos * floatinessBoost;
                        key.MoveToY(bobbingOffset + dashOffset.Y);
                        key.MoveToX(intialPos.X + dashOffset.X);
                    }
                }
            }
        }
    }
}
