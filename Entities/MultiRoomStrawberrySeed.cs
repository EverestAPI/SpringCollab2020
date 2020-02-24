using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/MultiRoomStrawberrySeed")]
    [Tracked]
    class MultiRoomStrawberrySeed : StrawberrySeed {
        public static void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
            On.Celeste.LightingRenderer.BeforeRender += onLightingBeforeRender;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
            On.Celeste.LightingRenderer.BeforeRender -= onLightingBeforeRender;
        }

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                Player player = self.Tracker.GetEntity<Player>();

                if (player != null) {
                    Vector2 seedPosition = player.Position;

                    // we have to restore collected strawberry seeds.
                    foreach (SpringCollab2020Session.MultiRoomStrawberrySeedInfo sessionSeedInfo in SpringCollab2020Module.Instance.Session.CollectedMultiRoomStrawberrySeeds) {
                        seedPosition += new Vector2(-12 * (int) player.Facing, -8f);

                        self.Add(new MultiRoomStrawberrySeed(player, seedPosition, sessionSeedInfo));
                    }
                }
            }
        }

        private static void onLightingBeforeRender(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene) {
            orig(self, scene);

            Draw.SpriteBatch.GraphicsDevice.SetRenderTarget(GameplayBuffers.Light);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            foreach (MultiRoomStrawberrySeed seed in scene.Tracker.GetEntities<MultiRoomStrawberrySeed>()) {
                if (seed.cutoutTexture != null) {
                    Draw.SpriteBatch.Draw(seed.cutoutTexture.Texture.Texture, seed.Position + seed.spriteObject.Position - (scene as Level).Camera.Position
                        - new Vector2(seed.cutoutTexture.Width / 2, seed.cutoutTexture.Height / 2), Color.White);
                }
            }
            Draw.SpriteBatch.End();
        }

        private DynData<StrawberrySeed> selfStrawberrySeed;

        private int index;
        public EntityID BerryID;

        private float canLoseTimerMirror;
        private Player player;
        private bool spawnedAsFollower = false;

        private string sprite;
        private bool ghost;

        private Sprite spriteObject;
        private MTexture cutoutTexture;

        public MultiRoomStrawberrySeed(Vector2 position, int index, bool ghost, string sprite, string ghostSprite, bool ignoreLighting) : base(null, position, index, ghost) {
            selfStrawberrySeed = new DynData<StrawberrySeed>(this);

            this.index = index;
            this.ghost = ghost;
            this.sprite = ghost ? ghostSprite : sprite;

            if (ignoreLighting) {
                cutoutTexture = GFX.Game["collectables/" + sprite + "_cutout"];
            }

            foreach (Component component in this) {
                if (component is PlayerCollider playerCollider) {
                    playerCollider.OnCollide = OnPlayer;
                }
            }
        }

        public MultiRoomStrawberrySeed(EntityData data, Vector2 offset) : this(data.Position + offset, data.Int("index"),
            SaveData.Instance.CheckStrawberry(new EntityID(data.Attr("berryLevel"), data.Int("berryID"))),
            data.Attr("sprite", "strawberry/seed"), data.Attr("ghostSprite", "ghostberry/seed"), data.Bool("ignoreLighting")) {

            BerryID = new EntityID(data.Attr("berryLevel"), data.Int("berryID"));
        }

        private MultiRoomStrawberrySeed(Player player, Vector2 position, SpringCollab2020Session.MultiRoomStrawberrySeedInfo sessionSeedInfo)
            : this(position, sessionSeedInfo.Index, SaveData.Instance.CheckStrawberry(sessionSeedInfo.BerryID), sessionSeedInfo.Sprite, sessionSeedInfo.Sprite, sessionSeedInfo.IgnoreLighting) {

            BerryID = sessionSeedInfo.BerryID;

            // the seed is collected right away.
            this.player = player;
            spawnedAsFollower = true;

            Add(new EffectCutout());
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (!spawnedAsFollower) {
                if (SceneAs<Level>().Session.GetFlag("collected_seeds_of_" + BerryID.ToString())) {
                    // if all seeds for this berry were already collected (the berry was already formed), commit remove self.
                    RemoveSelf();
                } else {
                    // if the seed already follows the player, commit remove self.
                    foreach (SpringCollab2020Session.MultiRoomStrawberrySeedInfo sessionSeedInfo in SpringCollab2020Module.Instance.Session.CollectedMultiRoomStrawberrySeeds) {
                        if (sessionSeedInfo.Index == index && sessionSeedInfo.BerryID.ID == BerryID.ID && sessionSeedInfo.BerryID.Level == BerryID.Level) {
                            RemoveSelf();
                            break;
                        }
                    }
                }
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if ((ghost && sprite != "ghostberry/seed") || (!ghost && sprite != "strawberry/seed")) {
                // the sprite is non-default. replace it.
                Sprite vanillaSprite = selfStrawberrySeed.Get<Sprite>("sprite");

                // build the new sprite.
                MTexture frame0 = GFX.Game["collectables/" + sprite + "00"];
                MTexture frame1 = GFX.Game["collectables/" + sprite + "01"];

                Sprite modSprite = new Sprite(GFX.Game, sprite);
                modSprite.CenterOrigin();
                modSprite.Justify = new Vector2(0.5f, 0.5f);
                modSprite.AddLoop("idle", 0.1f, new MTexture[] {
                    frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0,
                    frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame1
                });
                modSprite.AddLoop("noFlash", 0.1f, new MTexture[] { frame0 });

                // copy over the values from the vanilla sprite
                modSprite.Position = vanillaSprite.Position;
                modSprite.Color = vanillaSprite.Color;
                modSprite.OnFrameChange = vanillaSprite.OnFrameChange;
                modSprite.Play("idle");
                modSprite.SetAnimationFrame(vanillaSprite.CurrentAnimationFrame);

                // and replace it for good
                Remove(vanillaSprite);
                Add(modSprite);
                selfStrawberrySeed["sprite"] = modSprite;
            }

            if (spawnedAsFollower) {
                player.Leader.GainFollower(selfStrawberrySeed.Get<Follower>("follower"));
                canLoseTimerMirror = 0.25f;
                Collidable = false;
                Depth = -1000000;
                AddTag(Tags.Persistent);
            }

            // get a reference to the sprite. this will be used to "cut out" the lighting renderer.
            spriteObject = selfStrawberrySeed.Get<Sprite>("sprite");
        }

        private void OnPlayer(Player player) {
            Audio.Play("event:/game/general/seed_touch", Position, "count", index);
            player.Leader.GainFollower(selfStrawberrySeed.Get<Follower>("follower"));
            canLoseTimerMirror = 0.25f;
            Collidable = false;
            Depth = -1000000;
            AddTag(Tags.Persistent);

            // Add the info for this berry seed to the session.
            SpringCollab2020Session.MultiRoomStrawberrySeedInfo sessionSeedInfo = new SpringCollab2020Session.MultiRoomStrawberrySeedInfo();
            sessionSeedInfo.Index = index;
            sessionSeedInfo.BerryID = BerryID;
            sessionSeedInfo.Sprite = sprite;
            sessionSeedInfo.IgnoreLighting = (cutoutTexture != null);
            SpringCollab2020Module.Instance.Session.CollectedMultiRoomStrawberrySeeds.Add(sessionSeedInfo);
        }

        public override void Update() {
            base.Update();

            // be sure the canLoseTimer always has a positive value. we don't want the player to lose this berry seed.
            canLoseTimerMirror -= Engine.DeltaTime;
            if (canLoseTimerMirror < 1f) {
                canLoseTimerMirror = 1000f;
                selfStrawberrySeed["canLoseTimer"] = 1000f;
            }
        }
    }
}
