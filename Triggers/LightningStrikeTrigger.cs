using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Linq;
using Monocle;
using System.Collections;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/LightningStrikeTrigger")]
    class LightningStrikeTrigger : Trigger {
        public LightningStrikeTrigger(EntityData data, Vector2 offset) : this(data, offset, data.Float("playerOffset", 0f), data.Int("seed", 0), data.Float("delay", 0f), data.Bool("rain", true), data.Bool("flash", true)) { }

        public LightningStrikeTrigger(EntityData data, Vector2 offset, float playerOffset, int seed, float delay, bool raining, bool flash) : base (data, offset) {
            PlayerOffset = playerOffset;
            Seed = seed;
            Delay = delay;
            Raining = raining;
            Flash = flash;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            Level level = player.SceneAs<Level>();

            if(!Activated) {
                Activated = true;
                level.Add(new LightningStrike(new Vector2(player.X + PlayerOffset, level.Bounds.Top), Seed, level.Bounds.Height, Delay));
                Add(new Coroutine(ThunderEffect(level), true));

                if(Raining && !level.Background.Backdrops.OfType<RainFG>().Any()) {
                    level.Background.Backdrops.Add(new RainFG());
                }
            }
        }

        private IEnumerator ThunderEffect(Level level) {
            yield return Delay;
            Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
            level.Shake(0.3f);

            if(Flash)
                level.Flash(Color.White, false);

            yield break;
        }

        public bool Activated { get; private set; }

        private float PlayerOffset;

        private int Seed;

        private float Delay;

        private bool Raining;

        private bool Flash;
    }
}
