using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SpringCollab2020.Entities {
    /// <summary>
    /// This class adds a custom attribute to vanilla strawberries. This is not a new entity.
    /// </summary>
    class StrawberryIgnoringLighting {
        private static MTexture strawberryCutoutTexture;

        public static void Load() {
            On.Celeste.Strawberry.ctor += onStrawberryConstructor;
            On.Celeste.LightingRenderer.BeforeRender += onLightingBeforeRender;
        }

        public static void LoadContent() {
            strawberryCutoutTexture = GFX.Game["collectables/SpringCollab2020/strawberry/cutout"];
        }

        public static void Unload() {
            On.Celeste.Strawberry.ctor -= onStrawberryConstructor;
            On.Celeste.LightingRenderer.BeforeRender -= onLightingBeforeRender;
        }

        private static void onStrawberryConstructor(On.Celeste.Strawberry.orig_ctor orig, Strawberry self, EntityData data, Vector2 offset, EntityID gid) {
            orig(self, data, offset, gid);

            // save the value for SpringCollab2020_ignoreLighting to the DynData for the strawberry.
            new DynData<Strawberry>(self)["SpringCollab2020_ignoreLighting"] = data.Bool("SpringCollab2020_ignoreLighting");
        }

        private static void onLightingBeforeRender(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene) {
            orig(self, scene);

            Draw.SpriteBatch.GraphicsDevice.SetRenderTarget(GameplayBuffers.Light);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            foreach (Entity entity in scene.Entities) {
                if (entity is Strawberry berry) {
                    DynData<Strawberry> berryData = new DynData<Strawberry>(berry);
                    if (berryData.Get<bool>("SpringCollab2020_ignoreLighting")) {
                        Draw.SpriteBatch.Draw(strawberryCutoutTexture.Texture.Texture, berry.Position + berryData.Get<Sprite>("sprite").Position - (scene as Level).Camera.Position
                            - new Vector2(9, 8), Color.White);
                    }
                }
            }
            Draw.SpriteBatch.End();
        }
    }
}
