﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/UpsideDownJumpThru")]
    [Tracked]
    class UpsideDownJumpThru : JumpThru {

        private static FieldInfo actorMovementCounter = typeof(Actor).GetField("movementCounter", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo playerVarJumpTimer = typeof(Player).GetField("varJumpTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo playerOnCollideV = typeof(Player).GetField("onCollideV", BindingFlags.Instance | BindingFlags.NonPublic);

        private static ILHook playerOrigUpdateHook;

        private static bool hooksActive = false;

        private static readonly Hitbox normalHitbox = new Hitbox(8f, 11f, -4f, -11f);
        
        public static void Load() {
            On.Celeste.LevelLoader.ctor += onLevelLoad;
            On.Celeste.OverworldLoader.ctor += onOverworldLoad;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.ctor -= onLevelLoad;
            On.Celeste.OverworldLoader.ctor -= onOverworldLoad;
            deactivateHooks();
        }

        private static void onLevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);

            if (session.MapData?.Levels?.Any(level => level.Entities?.Any(entity => entity.Name == "SpringCollab2020/UpsideDownJumpThru") ?? false) ?? false) {
                activateHooks();
            } else {
                deactivateHooks();
            }
        }

        private static void onOverworldLoad(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startMode, HiresSnow snow) {
            orig(self, startMode, snow);

            if (startMode != (Overworld.StartMode) (-1)) { // -1 = in-game overworld from the collab utils
                deactivateHooks();
            }
        }


        public static void activateHooks() {
            if (hooksActive) {
                return;
            }
            hooksActive = true;

            Logger.Log(LogLevel.Info, "SpringCollab2020/UpsideDownJumpThru", "=== Activating upside-down jumpthru hooks");

            using (new DetourContext { Before = { "*" } }) { // these don't always call the orig methods, better apply them first.
                // fix general actor/platform behavior to make them comply with jumpthrus.
                On.Celeste.Actor.MoveVExact += onActorMoveVExact;
                On.Celeste.Platform.MoveVExactCollideSolids += onPlatformMoveVExactCollideSolids;
            }

            using (new DetourContext { After = { "*" } }) {
                // fix player specific behavior allowing them to go through upside-down jumpthrus.
                On.Celeste.Player.ctor += onPlayerConstructor;
            }


            using (new DetourContext()) {
                // block player if they try to climb past an upside-down jumpthru.
                IL.Celeste.Player.ClimbUpdate += patchPlayerClimbUpdate;

                // ignore upside-down jumpthrus in select places.
                playerOrigUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), filterOutJumpThrusFromCollideChecks);
                IL.Celeste.Player.DashUpdate += filterOutJumpThrusFromCollideChecks;
                IL.Celeste.Player.RedDashUpdate += filterOutJumpThrusFromCollideChecks;

                // listen for the player unducking, to knock the player down before they would go through upside down jumpthrus.
                On.Celeste.Player.Update += onPlayerUpdate;
            }
        }

        public static void deactivateHooks() {
            if (!hooksActive) {
                return;
            }
            hooksActive = false;

            Logger.Log(LogLevel.Info, "SpringCollab2020/UpsideDownJumpThru", "=== Deactivating upside-down jumpthru hooks");

            On.Celeste.Actor.MoveVExact -= onActorMoveVExact;
            On.Celeste.Platform.MoveVExactCollideSolids -= onPlatformMoveVExactCollideSolids;

            On.Celeste.Player.ctor -= onPlayerConstructor;
            IL.Celeste.Player.ClimbUpdate -= patchPlayerClimbUpdate;

            playerOrigUpdateHook?.Dispose();
            IL.Celeste.Player.DashUpdate -= filterOutJumpThrusFromCollideChecks;
            IL.Celeste.Player.RedDashUpdate -= filterOutJumpThrusFromCollideChecks;

            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        private static bool onActorMoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher) {
            // fall back to vanilla if no upside-down jumpthru is in the room.
            if (self.SceneAs<Level>().Tracker.CountEntities<UpsideDownJumpThru>() == 0)
                return orig(self, moveV, onCollide, pusher);

            Vector2 targetPosition = self.Position + Vector2.UnitY * moveV;
            int moveDirection = Math.Sign(moveV);
            int moveAmount = 0;
            while (moveV != 0) {
                bool didCollide = false;

                Platform platform = self.CollideFirst<Solid>(self.Position + Vector2.UnitY * moveDirection);
                CollisionData data;
                if (platform != null) {
                    // hit platform
                    didCollide = true;
                } else if (!self.IgnoreJumpThrus) {
                    if (moveV > 0) {
                        platform = self.CollideFirstOutside<JumpThru>(self.Position + Vector2.UnitY * moveDirection);
                        if (platform != null && platform.GetType() != typeof(UpsideDownJumpThru)) {
                            // hit vanilla jumpthru while going down
                            didCollide = true;
                        }
                    } else if (moveV < 0) {
                        platform = self.CollideFirstOutside<UpsideDownJumpThru>(self.Position + Vector2.UnitY * moveDirection);
                        if (platform != null) {
                            // hit upside-down jumpthru while going up
                            didCollide = true;
                        }
                    }
                }

                if (didCollide) {
                    Vector2 movementCounter = (Vector2) actorMovementCounter.GetValue(self);
                    movementCounter.Y = 0f;
                    actorMovementCounter.SetValue(self, movementCounter);
                    if (onCollide != null) {
                        data = new CollisionData {
                            Direction = Vector2.UnitY * moveDirection,
                            Moved = Vector2.UnitY * moveAmount,
                            TargetPosition = targetPosition,
                            Hit = platform,
                            Pusher = pusher
                        };
                        onCollide(data);
                    }
                    return true;
                }

                // continue moving
                moveAmount += moveDirection;
                moveV -= moveDirection;
                self.Y += moveDirection;
            }
            return false;
        }

        private static bool onPlatformMoveVExactCollideSolids(On.Celeste.Platform.orig_MoveVExactCollideSolids orig,
            Platform self, int moveV, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide) {

            // fall back to vanilla if no upside-down jumpthru is in the room.
            if (self.SceneAs<Level>().Tracker.CountEntities<UpsideDownJumpThru>() == 0)
                return orig(self, moveV, thruDashBlocks, onCollide);

            float y = self.Y;
            int moveDirection = Math.Sign(moveV);
            int moveAmount = 0;

            Platform platform = null;
            while (moveV != 0) {
                if (thruDashBlocks) {
                    foreach (DashBlock dashBlock in self.Scene.Tracker.GetEntities<DashBlock>()) {
                        if (self.CollideCheck(dashBlock, self.Position + Vector2.UnitY * moveDirection)) {
                            // break any dash block we collide with.
                            dashBlock.Break(self.Center, Vector2.UnitY * moveDirection, true, true);
                            self.SceneAs<Level>().Shake(0.2f);
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                    }
                }
                platform = self.CollideFirst<Solid>(self.Position + Vector2.UnitY * moveDirection);
                if (platform != null) {
                    // hit a solid
                    break;
                }
                if (moveV > 0) {
                    platform = self.CollideFirstOutside<JumpThru>(self.Position + Vector2.UnitY * moveDirection);
                    if (platform != null && platform.GetType() != typeof(UpsideDownJumpThru)) {
                        // hit a vanilla jumpthru while going down
                        break;
                    }
                }
                if (moveV < 0) {
                    platform = self.CollideFirstOutside<UpsideDownJumpThru>(self.Position + Vector2.UnitY * moveDirection);
                    if (platform != null) {
                        // hit an upside-down jumpthru while going up
                        break;
                    }
                }

                // continue moving
                moveAmount += moveDirection;
                moveV -= moveDirection;
                self.Y += moveDirection;
            }

            self.Y = y;
            self.MoveVExact(moveAmount);
            if (platform != null && onCollide != null) {
                onCollide(Vector2.UnitY * moveDirection, Vector2.UnitY * moveAmount, platform);
            }
            return platform != null;
        }

        private static void onPlayerConstructor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);

            Collision originalOnCollideV = (Collision) playerOnCollideV.GetValue(self);

            Collision patchedOnCollideV = collisionData => {
                // we just want to kill a piece of code that executes in these conditions (supposed to push the player left or right when hitting a wall angle).
                if (self.StateMachine.State != 19 && self.StateMachine.State != 3 && self.StateMachine.State != 9 && self.Speed.Y < 0
                    && self.CollideCheckOutside<UpsideDownJumpThru>(self.Position - Vector2.UnitY)) {

                    // kill the player's vertical speed.
                    self.Speed.Y = 0;

                    // reset varJumpTimer to prevent a weird "stuck on ceiling" effect.
                    playerVarJumpTimer.SetValue(self, 0);
                }

                originalOnCollideV(collisionData);
            };

            playerOnCollideV.SetValue(self, patchedOnCollideV);
        }

        private static void filterOutJumpThrusFromCollideChecks(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.Next != null) {
                Instruction nextInstruction = cursor.Next;
                if (nextInstruction.OpCode == OpCodes.Call || nextInstruction.OpCode == OpCodes.Callvirt) {
                    switch ((nextInstruction.Operand as MethodReference)?.FullName ?? "") {
                        case "T Monocle.Entity::CollideFirstOutside<Celeste.JumpThru>(Microsoft.Xna.Framework.Vector2)":
                            Logger.Log("SpringCollab2020/UpsideDownJumpThru", $"Patching CollideFirstOutside at {cursor.Index} in IL for {il.Method.Name}");
                            cursor.Index++;

                            // nullify if mod jumpthru.
                            cursor.EmitDelegate<Func<JumpThru, JumpThru>>(jumpThru => {
                                if (jumpThru?.GetType() == typeof(UpsideDownJumpThru))
                                    return null;
                                return jumpThru;
                            });
                            break;
                        case "System.Boolean Monocle.Entity::CollideCheckOutside<Celeste.JumpThru>(Microsoft.Xna.Framework.Vector2)":
                            Logger.Log("SpringCollab2020/UpsideDownJumpThru", $"Patching CollideCheckOutside at {cursor.Index} in IL for {il.Method.Name}");

                            cursor.Remove();

                            // check if colliding with a jumpthru but not an upside-down jumpthru.
                            cursor.EmitDelegate<Func<Entity, Vector2, bool>>((self, at) => self.CollideCheckOutside<JumpThru>(at) && !self.CollideCheckOutside<UpsideDownJumpThru>(at));
                            break;
                        case "System.Boolean Monocle.Entity::CollideCheck<Celeste.JumpThru>()":
                            Logger.Log("SpringCollab2020/UpsideDownJumpThru", $"Patching CollideCheck at {cursor.Index} in IL for {il.Method.Name}");

                            // we want stack to be: CollideCheck result, this
                            cursor.Index++;
                            cursor.Emit(OpCodes.Ldarg_0);

                            // turn check to false if colliding with an upside-down jumpthru.
                            cursor.EmitDelegate<Func<bool, Player, bool>>((vanillaCheck, self) => vanillaCheck && !self.CollideCheck<UpsideDownJumpThru>());
                            break;

                        case "System.Collections.Generic.List`1<Monocle.Entity> Monocle.Tracker::GetEntities<Celeste.JumpThru>()":
                            Logger.Log("SpringCollab2020/UpsideDownJumpThru", $"Patching GetEntities at {cursor.Index} in IL for {il.Method.Name}");

                            cursor.Index++;

                            // remove all mod jumpthrus from the returned list.
                            cursor.EmitDelegate<Func<List<Entity>, List<Entity>>>(matches => {
                                for (int i = 0; i < matches.Count; i++) {
                                    if (matches[i].GetType() == typeof(UpsideDownJumpThru)) {
                                        matches.RemoveAt(i);
                                        i--;
                                    }
                                }
                                return matches;
                            });
                            break;
                    }
                }

                cursor.Index++;
            }
        }

        private static void patchPlayerClimbUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // in decompiled code, we want to get ourselves just before the last occurrence of "if (climbNoMoveTimer <= 0f)".
            while (cursor.TryGotoNext(
                instr => instr.MatchStfld<Vector2>("Y"),
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdfld<Player>("climbNoMoveTimer"),
                instr => instr.MatchLdcR4(0f))) {

                cursor.Index += 2;

                FieldInfo f_lastClimbMove = typeof(Player).GetField("lastClimbMove", BindingFlags.NonPublic | BindingFlags.Instance);

                Logger.Log("SpringCollab2020/UpsideDownJumpThru", $"Injecting collide check to block climbing at {cursor.Index} in IL for {il.Method.Name}");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f_lastClimbMove);

                // if climbing is blocked by an upside down jumpthru, cancel the climb (lastClimbMove = 0 and Speed.Y = 0).
                // injecting that at that point in the method allows it to drain stamina as if the player was not climbing.
                cursor.EmitDelegate<Func<Player, int, int>>((self, lastClimbMove) => {
                    if (Input.MoveY.Value == -1 && self.CollideCheckOutside<UpsideDownJumpThru>(self.Position - Vector2.UnitY)) {
                        self.Speed.Y = 0;
                        return 0;
                    }
                    return lastClimbMove;
                });

                cursor.Emit(OpCodes.Stfld, f_lastClimbMove);
                cursor.Emit(OpCodes.Ldarg_0);
            }
        }

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            bool unduckWouldGoThroughPlatform = self.Ducking && !self.CollideCheck<UpsideDownJumpThru>();
            if (unduckWouldGoThroughPlatform) {
                Collider bak = self.Collider;
                self.Collider = normalHitbox;
                unduckWouldGoThroughPlatform = self.CollideCheck<UpsideDownJumpThru>();
                self.Collider = bak;
            }

            orig(self);

            if (unduckWouldGoThroughPlatform && !self.Ducking) {
                // we just unducked, and are now inside an upside-down jumpthru.
                // knock the player down if possible!
                while (self.CollideCheck<UpsideDownJumpThru>() && !self.CollideCheck<Solid>(self.Position + new Vector2(0f, 1f))) {
                    self.Position.Y++;
                }
            }
        }



        private int columns;
        private string overrideTexture;
        private float animationDelay;

        public UpsideDownJumpThru(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, false) {

            columns = data.Width / 8;
            Depth = -60;
            overrideTexture = data.Attr("texture", "default");
            animationDelay = data.Float("animationDelay", 0f);

            // shift the hitbox a bit to match the graphic
            Collider.Top += 3;
        }

        public override void Awake(Scene scene) {
            if (animationDelay > 0f) {
                for (int i = 0; i < columns; i++) {
                    Sprite jumpthruSprite = new Sprite(GFX.Game, "objects/jumpthru/" + overrideTexture);
                    jumpthruSprite.AddLoop("idle", "", animationDelay);
                    jumpthruSprite.X = i * 8;
                    jumpthruSprite.Y = 8;
                    jumpthruSprite.Scale.Y = -1;
                    jumpthruSprite.Play("idle");
                    Add(jumpthruSprite);
                }
            } else {
                AreaData areaData = AreaData.Get(scene);
                string jumpthru = areaData.Jumpthru;
                if (!string.IsNullOrEmpty(overrideTexture) && !overrideTexture.Equals("default")) {
                    jumpthru = overrideTexture;
                }

                MTexture mTexture = GFX.Game["objects/jumpthru/" + jumpthru];
                int textureWidthInTiles = mTexture.Width / 8;
                for (int i = 0; i < columns; i++) {
                    int xTilePosition;
                    int yTilePosition;
                    if (i == 0) {
                        xTilePosition = 0;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(-1f, 0f))) ? 1 : 0);
                    } else if (i == columns - 1) {
                        xTilePosition = textureWidthInTiles - 1;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(1f, 0f))) ? 1 : 0);
                    } else {
                        xTilePosition = 1 + Calc.Random.Next(textureWidthInTiles - 2);
                        yTilePosition = Calc.Random.Choose(0, 1);
                    }

                    Image image = new Image(mTexture.GetSubtexture(xTilePosition * 8, yTilePosition * 8, 8, 8));
                    image.X = i * 8;
                    image.Y = 8;
                    image.Scale.Y = -1;
                    Add(image);
                }
            }
        }
    }
}
