using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Reflection;
using System;

/*
 * Safe Respawn Crumble (Spring Collab 2020)
 * https://github.com/EverestAPI/SpringCollab2020/
 * 
 * A Crumble Platform which is always "crumbled",
 * unless the player respawns in its vicinity
 * or is bubbled to it via a CassetteReturn routine.
 * 
 * It performs a similar role to Mario Maker's pink block platform,
 * which only spawns when a user playtests their level from an aerial position
 * and disappears once the user jumps off of it.
 */
namespace Celeste.Mod.SpringCollab2020.Entities {
    [Tracked(false)]
    [CustomEntity("SpringCollab2020/safeRespawnCrumble")]
    class SafeRespawnCrumble : Solid {
        // We'll want to declare the SafeRespawnCrumble as "safe ground" so that Return Strawberries can be collected on it.
        public SafeRespawnCrumble(EntityData data, Vector2 offset) : base(data.Position + offset, (float) data.Width, 8f, true) {
            EnableAssistModeChecks = false;

            // make sure the platform is not collidable by default.
            Collidable = false;

            Visible = !data.Bool("invisible");
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // Set up the outline and block tiles and first-initialize the fader coroutines
            MTexture outlineTexture = GFX.Game["objects/crumbleBlock/outline"];
            MTexture tileTexture = GFX.Game["objects/SpringCollab2020/safeRespawnCrumble/tile"];

            tiles = new List<Image>();
            outlineTiles = new List<Image>();

            if (Width <= 8f) {
                Image toDraw = new Image(outlineTexture.GetSubtexture(24, 0, 8, 8, null));
                toDraw.Color = Color.White;
                outlineTiles.Add(toDraw);
            } else {
                // Select left, middle, or right
                for (int tile = 0; (float) tile < base.Width; tile += 8) {
                    int tileTex;
                    if (tile == 0)
                        tileTex = 0;
                    else if (tile > 0 && (float) tile < base.Width - 8f)
                        tileTex = 1;
                    else
                        tileTex = 2;

                    Image toDraw = new Image(outlineTexture.GetSubtexture(tileTex * 8, 0, 8, 8));
                    toDraw.Position = new Vector2((float) tile, 0f);
                    outlineTiles.Add(toDraw);
                    Add(toDraw);
                }
            }

            // Add pink-block tiles
            for (int tile = 0; (float) tile < base.Width; tile += 8) {
                Image toDraw = new Image(tileTexture);
                toDraw.CenterOrigin();
                toDraw.Position = new Vector2((float) tile, 0f) + new Vector2(toDraw.Width / 2f, toDraw.Height / 2f);
                toDraw.Color = Color.White * 0f;
                tiles.Add(toDraw);
                Add(toDraw);
            }

            Add(outlineFader = new Coroutine(false));
            Add(tileFader = new Coroutine(false));

            Add(new Coroutine(Sequence(), true));

            // Let's try adding a wiggler.
            wiggler = Wiggler.Create(
                0.5f,
                4f,
                delegate (float v) {
                    foreach (Image tile in tiles) {
                        tile.Scale = Vector2.One * (1f + v * 0.35f);
                    }
                },
                false,
                false
                );
            Add(wiggler);
        }

        private IEnumerator Sequence() {
            for (; ; ) {
                // Wait until activated
                Collidable = false;
                while (!activated)
                    yield return null;

                // Fade out the outline, fade in the tiles. Oh, and make ourselves collidable.
                if (isRespawn)
                    wiggler.Start(1.5f, 1.5f);
                else
                    wiggler.Start(0.5f, 4f);
                tileFader.Replace(TileFade(1f, tiles, !isRespawn));
                outlineFader.Replace(TileFade(0f, outlineTiles, !isRespawn));
                Collidable = true;

                // Wait until player is found
                while (GetPlayerOnTop() == null)
                    yield return null;

                // Wait until player leaves
                while (CheckToStayEnabled())
                    yield return null;

                // Fade out tiles, fade in outline.
                tileFader.Replace(TileFade(0f, tiles, true));
                outlineFader.Replace(TileFade(1f, outlineTiles, true));

                // Do nothing until next activation
                activated = false;
                isRespawn = false;
            }
        }

        private bool CheckToStayEnabled() {
            Player p = GetPlayerOnTop();
            if (p != null) {
                if (p.StartedDashing) {
                    typeof(Player).GetField("jumpGraceTimer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, 0f);
                    return false;
                }
                if (p.Ducking)
                    return false;
                return true;
            }

            return false;
        }

        // Fade the passed tiles in or out.
        private IEnumerator TileFade(float to, List<Image> targetTiles, bool fast = false) {
            float from = 1f - to;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * (fast ? 5f : 1.5f)) {
                foreach (Image img in targetTiles)
                    img.Color = Color.White * (from + (to - from) * Ease.CubeInOut(t));
                yield return null;
            }
            yield break;
        }

        // Bubble and player detection hooks
        public static void Load() {
            On.Celeste.Player.CassetteFlyCoroutine += SafeActivatorCassetteFly;
            On.Celeste.Player.IntroRespawnBegin += SafeActivatorRespawn;
        }

        public static void Unload() {
            On.Celeste.Player.CassetteFlyCoroutine -= SafeActivatorCassetteFly;
            On.Celeste.Player.IntroRespawnBegin -= SafeActivatorRespawn;
        }

        private static IEnumerator SafeActivatorCassetteFly(On.Celeste.Player.orig_CassetteFlyCoroutine orig, Player self) {
            IEnumerator origEnum = orig(self);
            while (origEnum.MoveNext())
                yield return origEnum.Current;

            SafeActivate(self, false);
        }

        private static void SafeActivatorRespawn(On.Celeste.Player.orig_IntroRespawnBegin orig, Player self) {
            orig(self);
            SafeActivate(self, true);
        }

        private static void SafeActivate(Player player, bool respawn) {
            SafeRespawnCrumble target = player.Scene.Tracker.GetNearestEntity<SafeRespawnCrumble>(player.Position);
            if (target == null)
                return;

            if (target.Left < player.X &&
                target.Right > player.X &&
                target.Top - 12f <= player.Y &&
                target.Bottom > player.Y) {
                target.activated = true;
                target.isRespawn = respawn;
            }
        }

        private List<Image> tiles;
        private List<Image> outlineTiles;
        private Coroutine tileFader;
        private Coroutine outlineFader;
        private Wiggler wiggler;
        private bool activated = false;
        private bool isRespawn = false;
    }
}
