using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Entities {
    class GlassBlockBgOriginal : Entity {

        public static void Load() {
            On.Celeste.LevelLoader.LoadingThread += onLevelLoaderLoadingThread;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.LoadingThread -= onLevelLoaderLoadingThread;
        }

        private static void onLevelLoaderLoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            self.Level.Add(new GlassBlockBgOriginal());
            orig(self);
        }

        private struct Star {
			public Vector2 Position;

			public MTexture Texture;

			public Color Color;

			public Vector2 Scroll;
		}

		private struct Ray {
			public Vector2 Position;

			public float Width;

			public float Length;

			public Color Color;
		}

		private static readonly Color[] starColors = new Color[6] {
				Calc.HexToColor("ff7777"),
				Calc.HexToColor("77ff77"),
				Calc.HexToColor("7777ff"),
				Calc.HexToColor("ff77ff"),
				Calc.HexToColor("77ffff"),
				Calc.HexToColor("ffff77"),
		};

		private const int StarCount = 100;

		private const int RayCount = 50;

		private Star[] stars = new Star[StarCount];

		private Ray[] rays = new Ray[RayCount];

		private VertexPositionColor[] verts = new VertexPositionColor[2700];

		private Vector2 rayNormal = new Vector2(-5f, -8f).SafeNormalize();

		private Color bgColor = Calc.HexToColor("202020");

		private VirtualRenderTarget beamsTarget;

		private VirtualRenderTarget starsTarget;

		private bool hasBlocks;

		public GlassBlockBgOriginal() {
			Tag = Tags.Global;
			Add(new BeforeRenderHook(BeforeRender));
			Depth = -9990;
			List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("particles/stars/");
			for (int i = 0; i < stars.Length; i++) {
				stars[i].Position.X = Calc.Random.Next(320);
				stars[i].Position.Y = Calc.Random.Next(180);
				stars[i].Texture = Calc.Random.Choose(atlasSubtextures);
				stars[i].Color = Calc.Random.Choose(starColors);
				stars[i].Scroll = Vector2.One * Calc.Random.NextFloat(0.05f);
			}

			for (int j = 0; j < rays.Length; j++) {
				rays[j].Position.X = Calc.Random.Next(320);
				rays[j].Position.Y = Calc.Random.Next(180);
				rays[j].Width = Calc.Random.Range(4f, 16f);
				rays[j].Length = Calc.Random.Choose(48, 96, 128);
				rays[j].Color = Color.White * Calc.Random.Range(0.2f, 0.4f);
			}
		}

		private void BeforeRender() {
			List<Entity> glassBlocks = Scene.Tracker.GetEntities<GlassBlockOriginal>();
			hasBlocks = (glassBlocks.Count > 0);
			if (!hasBlocks) {
				return;
			}

			Camera camera = (Scene as Level).Camera;
			int screenWidth = 320;
			int screenHeight = 180;
			if (starsTarget == null) {
				starsTarget = VirtualContent.CreateRenderTarget("glass-block-original-surfaces", screenWidth, screenHeight);
			}

			Engine.Graphics.GraphicsDevice.SetRenderTarget(starsTarget);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			Vector2 origin = new Vector2(8f, 8f);
			for (int i = 0; i < stars.Length; i++) {
				MTexture texture = stars[i].Texture;
				Color color = stars[i].Color;
				Vector2 scroll = stars[i].Scroll;
				Vector2 vector = default;
				vector.X = Mod(stars[i].Position.X - camera.X * (1f - scroll.X), screenWidth);
				vector.Y = Mod(stars[i].Position.Y - camera.Y * (1f - scroll.Y), screenHeight);
				texture.Draw(vector, origin, color);

				if (vector.X < origin.X) {
					texture.Draw(vector + new Vector2(screenWidth, 0f), origin, color);
				} else if (vector.X > screenWidth - origin.X) {
					texture.Draw(vector - new Vector2(screenWidth, 0f), origin, color);
				}

				if (vector.Y < origin.Y) {
					texture.Draw(vector + new Vector2(0f, screenHeight), origin, color);
				} else if (vector.Y > screenHeight - origin.Y) {
					texture.Draw(vector - new Vector2(0f, screenHeight), origin, color);
				}
			}
			Draw.SpriteBatch.End();
			int vertex = 0;
			for (int j = 0; j < rays.Length; j++) {
				Vector2 vector2 = default;
				vector2.X = Mod(rays[j].Position.X - camera.X * 0.9f, screenWidth);
				vector2.Y = Mod(rays[j].Position.Y - camera.Y * 0.9f, screenHeight);
				DrawRay(vector2, ref vertex, ref rays[j]);
				if (vector2.X < 64f) {
					DrawRay(vector2 + new Vector2(screenWidth, 0f), ref vertex, ref rays[j]);
				} else if (vector2.X > (screenWidth - 64)) {
					DrawRay(vector2 - new Vector2(screenWidth, 0f), ref vertex, ref rays[j]);
				}
				if (vector2.Y < 64f)
				{
					DrawRay(vector2 + new Vector2(0f, screenHeight), ref vertex, ref rays[j]);
				}
				else if (vector2.Y > (screenHeight - 64))
				{
					DrawRay(vector2 - new Vector2(0f, screenHeight), ref vertex, ref rays[j]);
				}
			}

			if (beamsTarget == null) {
				beamsTarget = VirtualContent.CreateRenderTarget("glass-block-original-beams", screenWidth, screenHeight);
			}

			Engine.Graphics.GraphicsDevice.SetRenderTarget(beamsTarget);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            GFX.DrawVertices(Matrix.Identity, verts, vertex);
		}

		private void DrawRay(Vector2 position, ref int vertex, ref Ray ray) {
			Vector2 value = new Vector2(0f - rayNormal.Y, rayNormal.X);
			Vector2 value2 = rayNormal * ray.Width * 0.5f;
			Vector2 value3 = value * ray.Length * 0.25f * 0.5f;
			Vector2 value4 = value * ray.Length * 0.5f * 0.5f;
			Vector2 v = position + value2 - value3 - value4;
			Vector2 v2 = position - value2 - value3 - value4;
			Vector2 vector = position + value2 - value3;
			Vector2 vector2 = position - value2 - value3;
			Vector2 vector3 = position + value2 + value3;
			Vector2 vector4 = position - value2 + value3;
			Vector2 v3 = position + value2 + value3 + value4;
			Vector2 v4 = position - value2 + value3 + value4;
			Color transparent = Color.Transparent;
			Color color = ray.Color;
			Quad(ref vertex, v, vector, vector2, v2, transparent, color, color, transparent);
			Quad(ref vertex, vector, vector3, vector4, vector2, color, color, color, color);
			Quad(ref vertex, vector3, v3, v4, vector4, color, transparent, transparent, color);
		}

		private void Quad(ref int vertex, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, Color c0, Color c1, Color c2, Color c3) {
			verts[vertex].Position.X = v0.X;
			verts[vertex].Position.Y = v0.Y;
			verts[vertex++].Color = c0;
			verts[vertex].Position.X = v1.X;
			verts[vertex].Position.Y = v1.Y;
			verts[vertex++].Color = c1;
			verts[vertex].Position.X = v2.X;
			verts[vertex].Position.Y = v2.Y;
			verts[vertex++].Color = c2;
			verts[vertex].Position.X = v0.X;
			verts[vertex].Position.Y = v0.Y;
			verts[vertex++].Color = c0;
			verts[vertex].Position.X = v2.X;
			verts[vertex].Position.Y = v2.Y;
			verts[vertex++].Color = c2;
			verts[vertex].Position.X = v3.X;
			verts[vertex].Position.Y = v3.Y;
			verts[vertex++].Color = c3;
		}

		public override void Render() {
			if (hasBlocks) {
				Vector2 position = (Scene as Level).Camera.Position;
				List<Entity> entities = Scene.Tracker.GetEntities<GlassBlockOriginal>();

				foreach (Entity item in entities) {
					Draw.Rect(item.X, item.Y, item.Width, item.Height, bgColor);
				}

				if (starsTarget != null && !starsTarget.IsDisposed) {
					foreach (Entity item2 in entities) {
						Rectangle value = new Rectangle((int)(item2.X - position.X), (int)(item2.Y - position.Y), (int)item2.Width, (int)item2.Height);
						Draw.SpriteBatch.Draw((RenderTarget2D)starsTarget, item2.Position, value, Color.White);
					}
				}

				if (beamsTarget != null && !beamsTarget.IsDisposed) {
					foreach (Entity item3 in entities) {
						Rectangle value2 = new Rectangle((int)(item3.X - position.X), (int)(item3.Y - position.Y), (int)item3.Width, (int)item3.Height);
						Draw.SpriteBatch.Draw((RenderTarget2D)beamsTarget, item3.Position, value2, Color.White);
					}
				}
			}
		}

		public override void Removed(Scene scene) {
			base.Removed(scene);
			Dispose();
		}

		public override void SceneEnd(Scene scene) {
			base.SceneEnd(scene);
			Dispose();
		}

		public void Dispose() {
			if (starsTarget != null && !starsTarget.IsDisposed) {
				starsTarget.Dispose();
			}

			if (beamsTarget != null && !beamsTarget.IsDisposed) {
				beamsTarget.Dispose();
			}
			starsTarget = null;
			beamsTarget = null;
		}

		private float Mod(float x, float m) {
			return (x % m + m) % m;
		}
	}
}
