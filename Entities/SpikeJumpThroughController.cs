using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.SpringCollab2020;
using Celeste.Mod.Entities;
using System.Linq;

// Based on RainbowSpinnerColorController
// from SpringCollab2020 / MaxHelpingHand

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/SpikeJumpThroughController")]
    class SpikeJumpThroughController : Entity {
        private static bool SpikeHooked;
        private static SpikeJumpThroughController CurrentController;
        private static SpikeJumpThroughController NextController;
        private static float TransitionProgress = -1f;

        public SpikeJumpThroughController(EntityData data, Vector2 offset) : this(data.Bool("persistent", false), offset) { }

        public SpikeJumpThroughController(bool persistent, Vector2 offset) : base(offset) {
            SpringCollab2020Module.Instance.Session.SpikeJumpThroughHooked = persistent;

            Add(new TransitionListener {
                OnIn = progress => TransitionProgress = progress,
                OnOut = progress => TransitionProgress = progress,
                OnInBegin = () => TransitionProgress = 0f,
                OnInEnd = () => TransitionProgress = -1f
            });

            Logger.Log("s", SpringCollab2020Module.Instance.Session.SpikeJumpThroughHooked.ToString());

        }

        public static void Load() {
            On.Celeste.Level.LoadLevel += OnLoadLevelHook;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= OnLoadLevelHook;
        }

        public override void Update() {
            base.Update();
            if (TransitionProgress == -1f && CurrentController == null) {
                CurrentController = this;
                NextController = null;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            NextController = this;
            if (!SpikeHooked) {
                On.Celeste.Spikes.OnCollide += OnCollideHook;
                SpikeHooked = true;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            CurrentController = NextController;
            NextController = null;

            TransitionProgress = -1f;

            if (SpikeHooked && CurrentController == null) {
                On.Celeste.Spikes.OnCollide -= OnCollideHook;
                SpikeHooked = false;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            CurrentController = NextController = null;
            if (SpikeHooked) {
                On.Celeste.Spikes.OnCollide -= OnCollideHook;
                SpikeHooked = false;
            }
        }

        private static void OnLoadLevelHook(On.Celeste.Level.orig_LoadLevel orig, Level level, Player.IntroTypes introType, bool isFromLoader) {
            orig(level, introType, isFromLoader);

            if (SpringCollab2020Module.Instance.Session.SpikeJumpThroughHooked && !level.Session.LevelData.Entities.Any(entity => entity.Name == "SpringCollab2020/SpikeJumpThroughController")) {
                level.Add(new SpikeJumpThroughController(SpringCollab2020Module.Instance.Session.SpikeJumpThroughHooked, Vector2.Zero));
                level.Entities.UpdateLists();
            }
        }

        private static void OnCollideHook(On.Celeste.Spikes.orig_OnCollide orig, Spikes spikes, Player player) {
            // If the player is picking up a holdable, don't kill them.
            if (player.StateMachine.State == 8) {
                return;
            }

            orig(spikes, player);
        }
    }
}