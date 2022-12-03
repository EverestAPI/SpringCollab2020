using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/MusicLayerFadeTrigger")]
    public class MusicLayerFadeTrigger : Trigger {

        private int[] layers;
        private float fadeA;
        private float fadeB;
        private PositionModes direction;

        public MusicLayerFadeTrigger(EntityData data, Vector2 offset)
            : base(data, offset) {

            // parse the "layers" attribute (for example "1,3,4") as an int array.
            string[] layersAsStrings = data.Attr("layers").Split(',');
            layers = new int[layersAsStrings.Length];
            for (int i = 0; i < layers.Length; i++) {
                layers[i] = int.Parse(layersAsStrings[i]);
            }

            // parse the other parameters for the trigger
            fadeA = data.Float("fadeA", 0f);
            fadeB = data.Float("fadeB", 1f);
            direction = data.Enum("direction", PositionModes.LeftToRight);
        }

        public override void OnStay(Player player) {
            // compute the current fade value
            float fade = MathHelper.Lerp(fadeA, fadeB, GetPositionLerp(player, direction));

            // apply it to all the required layers
            AudioState audio = SceneAs<Level>().Session.Audio;
            foreach (int layer in layers) {
                audio.Music.Layer(layer, fade);
            }
            audio.Apply();
        }
    }
}
