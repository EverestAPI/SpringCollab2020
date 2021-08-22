using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/SidewaysJumpThru")]
    [Tracked]
    class SidewaysJumpThru : Entity {

        private static ILHook hookOnUpdateSprite;

        private static FieldInfo actorMovementCounter = typeof(Actor).GetField("movementCounter", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool hooksActive = false;

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

            if (session.MapData?.Levels?.Any(level => level.Entities?.Any(entity => entity.Name == "SpringCollab2020/SidewaysJumpThru") ?? false) ?? false) {
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

            Logger.Log(LogLevel.Info, "SpringCollab2020/SidewaysJumpThru", "=== Activating sideways jumpthru hooks");

            string updateSpriteMethodToPatch = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "Everest", Version = new Version(1, 1432) }) ?
                "orig_UpdateSprite" : "UpdateSprite";

            // implement the basic collision between actors/platforms and sideways jumpthrus.
            IL.Celeste.Actor.MoveHExact += addSidewaysJumpthrusInHorizontalMoveMethods;
            IL.Celeste.Platform.MoveHExactCollideSolids += addSidewaysJumpthrusInHorizontalMoveMethods;

            // block "climb hopping" on top of sideways jumpthrus, because this just looks weird.
            On.Celeste.Player.ClimbHopBlockedCheck += onPlayerClimbHopBlockedCheck;

            using (new DetourContext()) {
                // mod collide checks to include sideways jumpthrus, so that the player behaves with them like with walls.
                IL.Celeste.Player.WallJumpCheck += modCollideChecks; // allow player to walljump off them
                IL.Celeste.Player.ClimbCheck += modCollideChecks; // allow player to climb on them
                IL.Celeste.Player.ClimbBegin += modCollideChecks; // if not applied, the player will clip through jumpthrus if trying to climb on them
                IL.Celeste.Player.ClimbUpdate += modCollideChecks; // when climbing, jumpthrus are handled like walls
                IL.Celeste.Player.SlipCheck += modCollideChecks; // make climbing on jumpthrus not slippery
                IL.Celeste.Player.NormalUpdate += modCollideChecks; // get the wall slide effect
                IL.Celeste.Player.OnCollideH += modCollideChecks; // handle dashes against jumpthrus properly, without "shifting" down

                // have the push animation when Madeline runs against a jumpthru for example
                hookOnUpdateSprite = new ILHook(typeof(Player).GetMethod(updateSpriteMethodToPatch, BindingFlags.NonPublic | BindingFlags.Instance), modCollideChecks);
            }

            // one extra hook that kills the player momentum when hitting a jumpthru so that they don't get "stuck" on them.
            On.Celeste.Player.NormalUpdate += onPlayerNormalUpdate;
        }

        public static void deactivateHooks() {
            if (!hooksActive) {
                return;
            }
            hooksActive = false;

            Logger.Log(LogLevel.Info, "SpringCollab2020/SidewaysJumpThru", "=== Deactivating sideways jumpthru hooks");
            IL.Celeste.Actor.MoveHExact -= addSidewaysJumpthrusInHorizontalMoveMethods;
            IL.Celeste.Platform.MoveHExactCollideSolids -= addSidewaysJumpthrusInHorizontalMoveMethods;

            On.Celeste.Player.ClimbHopBlockedCheck -= onPlayerClimbHopBlockedCheck;

            IL.Celeste.Player.WallJumpCheck -= modCollideChecks;
            IL.Celeste.Player.ClimbCheck -= modCollideChecks;
            IL.Celeste.Player.ClimbBegin -= modCollideChecks;
            IL.Celeste.Player.ClimbUpdate -= modCollideChecks;
            IL.Celeste.Player.SlipCheck -= modCollideChecks;
            IL.Celeste.Player.NormalUpdate -= modCollideChecks;
            IL.Celeste.Player.OnCollideH -= modCollideChecks;
            hookOnUpdateSprite?.Dispose();

            On.Celeste.Player.NormalUpdate -= onPlayerNormalUpdate;
        }

        private static void addSidewaysJumpthrusInHorizontalMoveMethods(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Entity>("CollideFirst"))
                 && cursor.TryGotoNext(instr => instr.OpCode == OpCodes.Brfalse_S || instr.OpCode == OpCodes.Brtrue_S)) {

                Logger.Log("SpringCollab2020/SidewaysJumpThru", $"Injecting sideways jumpthru check at {cursor.Index} in IL for {il.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Solid, Entity, int, Solid>>((orig, self, moveH) => {
                    if (orig != null)
                        return orig;

                    int moveDirection = Math.Sign(moveH);
                    bool movingLeftToRight = moveH > 0;
                    if (checkCollisionWithSidewaysJumpthruWhileMoving(self, moveDirection, movingLeftToRight)) {
                        return new Solid(Vector2.Zero, 0, 0, false); // what matters is that it is non null.
                    }

                    return null;
                });
            }
        }

        private static bool checkCollisionWithSidewaysJumpthruWhileMoving(Entity self, int moveDirection, bool movingLeftToRight) {
            // check if colliding with a sideways jumpthru
            SidewaysJumpThru jumpThru = self.CollideFirstOutside<SidewaysJumpThru>(self.Position + Vector2.UnitX * moveDirection);
            if (jumpThru != null && jumpThru.AllowLeftToRight != movingLeftToRight) {
                // there is a sideways jump-thru and we are moving in the opposite direction => collision
                return true;
            }

            return false;
        }

        private static bool onPlayerClimbHopBlockedCheck(On.Celeste.Player.orig_ClimbHopBlockedCheck orig, Player self) {
            bool vanillaCheck = orig(self);
            if (vanillaCheck)
                return vanillaCheck;

            // block climb hops on jumpthrus because those look weird
            return self.CollideCheckOutside<SidewaysJumpThru>(self.Position + Vector2.UnitX * (int) self.Facing);
        }

        private static void modCollideChecks(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // create a Vector2 temporary variable
            VariableDefinition checkAtPositionStore = new VariableDefinition(il.Import(typeof(Vector2)));
            il.Body.Variables.Add(checkAtPositionStore);

            bool isClimb = il.Method.Name.Contains("Climb");
            bool isWallJump = il.Method.Name.Contains("WallJump") || il.Method.Name.Contains("NormalUpdate");

            while (cursor.Next != null) {
                Instruction next = cursor.Next;

                // we want to replace all CollideChecks with solids here.
                if (next.OpCode == OpCodes.Call && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("SpringCollab2020/SidewaysJumpThru", $"Patching Entity.CollideCheck to include sideways jumpthrus at {cursor.Index} in IL for {il.Method.Name}");

                    callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: false);

                    // mod the result
                    cursor.EmitDelegate<Func<bool, Entity, Vector2, bool>>((orig, self, checkAtPosition) => {
                        // we still want to check for solids...
                        if (orig) {
                            return true;
                        }

                        // if we are not checking a side, this certainly has nothing to do with jumpthrus.
                        if (self.Position.X == checkAtPosition.X)
                            return false;

                        return entityCollideCheckWithSidewaysJumpthrus(self, checkAtPosition, isClimb, isWallJump);
                    });
                }

                if (next.OpCode == OpCodes.Callvirt && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Scene::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("SpringCollab2020/SidewaysJumpThru", $"Patching Scene.CollideCheck to include sideways jumpthrus at {cursor.Index} in IL for {il.Method.Name}");

                    callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: true);

                    cursor.EmitDelegate<Func<bool, Scene, Vector2, bool>>((orig, self, vector) => {
                        if (orig) {
                            return true;
                        }
                        return sceneCollideCheckWithSidewaysJumpthrus(self, vector, isClimb, isWallJump);
                    });
                }

                cursor.Index++;
            }
        }

        private static void callOrigMethodKeepingEverythingOnStack(ILCursor cursor, VariableDefinition checkAtPositionStore, bool isSceneCollideCheck) {
            // store the position in the local variable
            cursor.Emit(OpCodes.Stloc, checkAtPositionStore);
            cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);

            // let vanilla call CollideCheck
            cursor.Index++;

            // reload the parameters
            cursor.Emit(OpCodes.Ldarg_0);
            if (isSceneCollideCheck) {
                cursor.Emit(OpCodes.Call, typeof(Entity).GetProperty("Scene").GetGetMethod());
            }

            cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);
        }

        private static bool entityCollideCheckWithSidewaysJumpthrus(Entity self, Vector2 checkAtPosition, bool isClimb, bool isWallJump) {
            // our entity collides if this is with a jumpthru and we are colliding with the solid side of it.
            // we are in this case if the jumpthru is left to right (the "solid" side of it is the right one) 
            // and we are checking the collision on the left side of the player for example.
            bool collideOnLeftSideOfPlayer = (self.Position.X > checkAtPosition.X);
            SidewaysJumpThru jumpthru = self.CollideFirstOutside<SidewaysJumpThru>(checkAtPosition);
            return jumpthru != null && self is Player && jumpthru.AllowLeftToRight == collideOnLeftSideOfPlayer
                && jumpthru.Bottom >= self.Top + checkAtPosition.Y - self.Position.Y + 3;
        }

        private static bool sceneCollideCheckWithSidewaysJumpthrus(Scene self, Vector2 vector, bool isClimb, bool isWallJump) {
            return self.CollideCheck<SidewaysJumpThru>(vector);
        }

        private static int onPlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self) {
            int result = orig(self);

            // kill speed if player is going towards a jumpthru.
            if (self.Speed.X != 0) {
                bool movingLeftToRight = self.Speed.X > 0;
                SidewaysJumpThru jumpThru = self.CollideFirstOutside<SidewaysJumpThru>(self.Position + Vector2.UnitX * Math.Sign(self.Speed.X));
                if (jumpThru != null && jumpThru.AllowLeftToRight != movingLeftToRight) {
                    self.Speed.X = 0;
                }
            }

            return result;
        }

        // ======== Begin of entity code ========

        private int lines;
        private string overrideTexture;
        private float animationDelay;

        public bool AllowLeftToRight;

        public SidewaysJumpThru(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay)
            : base(position) {

            lines = height / 8;
            AllowLeftToRight = allowLeftToRight;
            Depth = -60;
            this.overrideTexture = overrideTexture;
            this.animationDelay = animationDelay;

            float hitboxOffset = 0f;
            if (AllowLeftToRight)
                hitboxOffset = 3f;

            Collider = new Hitbox(5f, height, hitboxOffset, 0);
        }

        public SidewaysJumpThru(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Height, !data.Bool("left"), data.Attr("texture", "default"), data.Float("animationDelay", 0f)) {
        }

        public override void Awake(Scene scene) {
            if (animationDelay > 0f) {
                for (int i = 0; i < lines; i++) {
                    Sprite jumpthruSprite = new Sprite(GFX.Game, "objects/jumpthru/" + overrideTexture);
                    jumpthruSprite.AddLoop("idle", "", animationDelay);

                    jumpthruSprite.Y = i * 8;
                    jumpthruSprite.Rotation = (float) (Math.PI / 2);
                    if (AllowLeftToRight)
                        jumpthruSprite.X = 8;
                    else
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
                int num = mTexture.Width / 8;
                for (int i = 0; i < lines; i++) {
                    int xTilePosition;
                    int yTilePosition;
                    if (i == 0) {
                        xTilePosition = 0;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, -1f))) ? 1 : 0);
                    } else if (i == lines - 1) {
                        xTilePosition = num - 1;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, 1f))) ? 1 : 0);
                    } else {
                        xTilePosition = 1 + Calc.Random.Next(num - 2);
                        yTilePosition = Calc.Random.Choose(0, 1);
                    }
                    Image image = new Image(mTexture.GetSubtexture(xTilePosition * 8, yTilePosition * 8, 8, 8));
                    image.Y = i * 8;
                    image.Rotation = (float) (Math.PI / 2);

                    if (AllowLeftToRight)
                        image.X = 8;
                    else
                        image.Scale.Y = -1;

                    Add(image);
                }
            }
        }
    }
}
