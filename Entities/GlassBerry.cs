using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

/*
 * Glass Berry (Spring Collab 2020)
 * https://github.com/EverestAPI/SpringCollab2020/
 * 
 * A custom Strawberry which can be collected normally.
 * It will break and return to its home location if the player dashes while carrying it.
 */

namespace Celeste.Mod.SpringCollab2020.Entities {
    // This custom Strawberry is tracked.
    // It will appear in the pause menu tracker, as well as all places where red berries are tallied.
    // This custom Strawberry does not "block collection".
    // It has normal collection rules, so it can be part of the berry train chain.
    [RegisterStrawberry(true, false)]
    [CustomEntity("SpringCollab2020/glassBerry")]
    class GlassBerry : Entity, IStrawberry, IStrawberrySeeded {
        // Requested implementations for using IStrawberrySeeded. 
        // These are the most basic implementations thereof and there is likely no reason to change these.
        public List<GenericStrawberrySeed> Seeds { get; }
        public string gotSeedFlag => "collected_seeds_of_" + ID.ToString();
        public bool WaitingOnSeeds {
            get {
                if (Seeds != null)
                    return !SceneAs<Level>().Session.GetFlag(gotSeedFlag) && Seeds.Count > 0;
                else
                    return false;
            }
        }

        // Particle references. These are the glittery bits that trail behind you when you're carrying a Strawberry.
        // You can set these during your EverestModule.Load() or LoadContent() command.
        public static ParticleType P_Glow = Strawberry.P_Glow;
        public static ParticleType P_GhostGlow = Strawberry.P_GhostGlow;

        // Common to every strawberry, that may need to be read from other sources.
        public EntityID ID;
        public Follower Follower;

        // Internals common to every strawberry. It's not likely that you'll need these outside of your custom berry.
        private Sprite sprite;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private Tween lightTween;
        private Vector2 start;
        private float wobble = 0f;
        private float collectTimer = 0f;
        private bool collected = false;
        private bool isOwned;

        // The Glass Berry's gimmick breaks the berry and returns it to its home location when the player dashes while holding it.
        private bool wasBroken = false;

        // A Sprite Bank for the Glass Berry, initialized in Load().
        private static SpriteBank spriteBank;


        // Basic Strawberry setup. This usually won't change too much.
        public GlassBerry(EntityData data, Vector2 offset, EntityID gid) {
            ID = gid;
            Position = (start = data.Position + offset);

            isOwned = SaveData.Instance.CheckStrawberry(ID);
            Depth = -100;
            Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            Add(Follower = new Follower(ID, null, OnLoseLeader));
            Follower.FollowDelay = 0.3f;

            // It's unlikely, but there's no reason not to allow the Glass Berry to be a seeded berry.
            // We'll use standard GenericStrawberrySeeds; in the future, maybe a special fragile seed could be created.
            if (data.Nodes != null && data.Nodes.Length != 0) {
                Seeds = new List<GenericStrawberrySeed>();
                for (int i = 0; i < data.Nodes.Length; i++) {
                    Seeds.Add(new GenericStrawberrySeed(this, offset + data.Nodes[i], i, isOwned));
                }
            }
        }

        // When a new Strawberry is "added" to a scene, we need it to initialize certain features, most of them visual in nature.
        public override void Added(Scene scene) {
            base.Added(scene);

            sprite = spriteBank.Create("springCollabGlassBerry");
            Add(sprite);

            if (!isOwned)
                sprite.Play("idle");
            else
                sprite.Play("idleGhost");

            // Strawberries have certain special effects during their animation sequence.
            // This adds a handler to enable this.
            sprite.OnFrameChange = OnAnimate;

            // A Wiggler is capable of "shaking" and "pulsing" sprites.
            // This Wiggler adjusts the sprite's Scale when triggered.
            wiggler = Wiggler.Create
                (
                    0.4f,
                    4f,
                    delegate (float v) {
                        sprite.Scale = Vector2.One * (1f + v * 0.35f);
                    },
                    false,
                    false
                );
            Add(wiggler);

            // Bloom makes bright things brighter!
            // The default BloomPoint for a vanilla Strawberry is
            // alpha = (this.Golden || this.Moon || this.isGhostBerry) ? 0.5f : 1f
            // radius = 12f
            bloom = new BloomPoint(isOwned ? 0.25f : 0.5f, 12f);
            Add(bloom);

            // Strawberries give off light. This is the vanilla VertexLight.
            light = new VertexLight(Color.White, 1f, 16, 24);
            lightTween = light.CreatePulseTween();
            Add(light);
            Add(lightTween);

            // While we're here, a seeded Strawberry must be allowed to initialize its Seeds.
            if (Seeds != null && Seeds.Count > 0 && !SceneAs<Level>().Session.GetFlag(gotSeedFlag)) {
                foreach (GenericStrawberrySeed seed in Seeds)
                    scene.Add(seed);

                // If we added the seeds, we don't want to see or touch the strawberry, do we?
                Visible = false;
                Collidable = false;
                bloom.Visible = light.Visible = false;
            }

            // Let's be polite and turn down the bloom a little bit if the base level has bloom.
            if (SceneAs<Level>().Session.BloomBaseAdd > 0.1f)
                bloom.Alpha *= 0.5f;

            // The Glass Berry's gimmick is that it breaks if the player dashes while carrying it.
            // So we need a generic DashListener.
            Add(new DashListener { OnDash = OnDash });
        }

        // Every Entity needs an Update sequence. This is where we do the bulk of our checking for things.
        public override void Update() {
            // Let's not do anything if the Strawberry is waiting for seeds.
            if (WaitingOnSeeds)
                return;

            if (!collected) {
                // Subtle up-and-down movement sequence.
                wobble += Engine.DeltaTime * 4f;
                sprite.Y = bloom.Y = light.Y =
                    (float) Math.Sin(wobble) * 2f;

                // We'll check collection rules for our strawberry here. It's standard collection rules, so...
                if (Follower.Leader != null) {
                    Player player = Follower.Leader.Entity as Player;

                    // First in line of the normal-collection train?
                    if (Follower.DelayTimer <= 0f && StrawberryRegistry.IsFirstStrawberry(this)) {
                        if (player != null && player.Scene != null &&
                            !player.StrawberriesBlocked && player.OnSafeGround &&
                            player.StateMachine.State != 13) {
                            // lot of checks!
                            collectTimer += Engine.DeltaTime;
                            if (collectTimer > 0.15f)
                                OnCollect();
                        } else {
                            collectTimer = Math.Min(collectTimer, 0f);
                        }
                    }
                    // Not first in line?
                    else if (Follower.FollowIndex > 0)
                        collectTimer = -0.15f;
                }
            }

            // This spawns glittery particles if we're carrying the berry!
            if (Follower.Leader != null && Scene.OnInterval(0.08f)) {
                ParticleType type;
                if (!isOwned)
                    type = P_Glow;
                else
                    type = P_GhostGlow;

                SceneAs<Level>().ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
            }

            base.Update();
        }

        // From our OnFrameChange handler assigned in Added, this routine allows us to run additional effects based on the animation frame.
        private void OnAnimate(string id) {
            // Strawberries play a sound and a little extra "burst" on a specific animation frame.
            // Since this is a unique animation, we'll targeting the middle frame of the "sheen" animation.
            int numFrames = 35; // 4 loops of 0-6, 1 loop of 7-13.

            if (sprite.CurrentAnimationFrame == numFrames - 4) {
                lightTween.Start();

                // Use a different pulse effect if the berry is visually obstructed.
                // Don't play it if OnCollect was successfully called.
                bool visuallyObstructed = CollideCheck<FakeWall>() || CollideCheck<Solid>();
                if (!collected && visuallyObstructed) {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
                } else {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                }
            }
        }

        // We added a PlayerCollider handler in the constructor. This routine is called when it is triggered.
        public void OnPlayer(Player player) {
            // Bail if we're not ready to be picked up, or are already picked up.
            if (Follower.Leader != null || collected || WaitingOnSeeds || wasBroken)
                return;

            // Let's play a pickup sound and trigger the Wiggler to make the strawberry temporarily change size when we touch it.
            Audio.Play(isOwned ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);
            player.Leader.GainFollower(Follower);
            wiggler.Start();
            Depth = -1000000;
        }

        // We don't need the dash direction, but I'm vaguely certain the DashListener cries if it can't pass one...
        public void OnDash(Vector2 nyoom) {
            if (Follower.Leader != null) {
                if (!(Follower.Leader.Entity is Player p))
                    return;
                wasBroken = true;
                p.Leader.LoseFollower(Follower);
                Add(new Coroutine(GlassBreakReturnRoutine(), true));
            }
        }

        // Routine to "break" the berry and then respawn it
        private IEnumerator GlassBreakReturnRoutine() {
            Collidable = false;
            sprite.Scale = Vector2.One * 2f;
            Audio.Play("event:/game/general/diamond_touch", Position);
            yield return 0.05f;


            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            // Add obvious "shard" lines
            for (int i = 0; i < 12; i++) {
                float dir = Calc.Random.NextFloat(6.2831855f);
                SceneAs<Level>().ParticlesFG.Emit(StrawberrySeed.P_Burst, 1, Position + Calc.AngleToVector(dir, 4f), Vector2.Zero, dir);
            }

            SceneAs<Level>().Displacement.AddBurst(Position, 0.2f, 8f, 28f, 0.2f, null, null);
            Visible = false;
            Position = start;
            yield return 1.5f;


            Audio.Play("event:/game/general/seed_reappear", Position);
            sprite.Scale = Vector2.One;
            Visible = true;
            Collidable = true;
            wasBroken = false;
            SceneAs<Level>().Displacement.AddBurst(Position, 0.2f, 8f, 28f, 0.2f, null, null);
            yield break;
        }

        // Requested implementation for using IStrawberry.
        public void OnCollect() {
            // Bail if we're already "collected".
            if (collected)
                return;

            collected = true;

            // This is used for the ascending "score" effect and even leads to 1UPs!
            int collectIndex = 0;

            if (Follower.Leader != null) {
                Player player = Follower.Leader.Entity as Player;
                collectIndex = player.StrawberryCollectIndex;
                player.StrawberryCollectIndex++;
                player.StrawberryCollectResetTimer = 2.5f;
                Follower.Leader.LoseFollower(Follower);
            }

            // Save the Strawberry. It's not a "secret" berry, so let's not say it's "golden" for the savedata.
            SaveData.Instance.AddStrawberry(ID, false);

            // Make the Strawberry not load any more in this Session.
            Session session = SceneAs<Level>().Session;
            session.DoNotLoad.Add(ID);
            session.Strawberries.Add(ID);
            session.UpdateLevelStartDashes();

            // Coroutines allow certain processes to run independently, so to speak.
            Add(new Coroutine(CollectRoutine(collectIndex), true));
        }

        private IEnumerator CollectRoutine(int collectIndex) {
            Level level = SceneAs<Level>();
            Tag = Tags.TransitionUpdate;
            Depth = -2000010;

            // Use "yellow" text for a new berry, "blue" for an owned berry.
            // Plays the appropriate sounds, too.
            int color = !isOwned ? 0 : 1;
            Audio.Play("event:/game/general/strawberry_get", Position, "colour", (float) color, "count", (float) collectIndex);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            if (!isOwned)
                sprite.Play("collect");
            else
                sprite.Play("collectGhost");

            while (sprite.Animating) {
                yield return null;
            }
            Scene.Add(new StrawberryPoints(Position, isOwned, collectIndex, false));
            RemoveSelf();
            yield break;
        }

        // When the Strawberry's Follower loses its Leader (basically, when you die),
        // the Strawberry performs this action to smoothly return home.
        // If you detach a Strawberry and need it to be collectable again,
        // make sure to re-enable its collision in tween.OnComplete's delegate.
        private void OnLoseLeader() {
            if (collected)
                return;

            Alarm.Set(this, 0.15f, delegate {
                Vector2 vector = (start - Position).SafeNormalize();
                float num = Vector2.Distance(Position, start);
                float scaleFactor = Calc.ClampedMap(num, 16f, 120f, 16f, 96f);
                Vector2 control = start + vector * 16f + vector.Perpendicular() * scaleFactor * (float) Calc.Random.Choose(1, -1);
                SimpleCurve curve = new SimpleCurve(Position, start, control);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(num / 100f, 0.4f), true);
                tween.OnUpdate = delegate (Tween f) {
                    Position = curve.GetPoint(f.Eased);
                };
                tween.OnComplete = delegate (Tween f) {
                    Depth = 0;
                };
                Add(tween);
            }, Alarm.AlarmMode.Oneshot);
        }

        // Requested implementation for using IStrawberrySeeded.
        // You'll generally want to make a seeded berry "visible" here.
        // Additionally, set the current Session's flag for the seeds to true.
        public void CollectedSeeds() {
            SceneAs<Level>().Session.SetFlag(gotSeedFlag, true);
            Visible = true;
            Collidable = true;
            bloom.Visible = light.Visible = true;
        }

        // Initialize the spritebank.
        public static void LoadContent() {
            spriteBank = new SpriteBank(GFX.Game, "Graphics/SpringCollab2020/GlassBerry.xml");
        }
        public static void Unload() { }

    }
}