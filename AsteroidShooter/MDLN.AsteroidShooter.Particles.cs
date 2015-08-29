using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.AsteroidShooter {
	public class Particles {
		private GraphicsDevice cGraphDev;
		private SpriteBatch cDrawBatch;
		public List<ParticleInfo> cParticleList;

		public bool WrapScreenEdges { get; set; }

		public Particles(GraphicsDevice GraphDev) {
			cGraphDev = GraphDev;

			cDrawBatch = new SpriteBatch(cGraphDev);

			cParticleList = new List<ParticleInfo>();
		}

		public void AddParticle(Texture2D Image, float Top, float Left, int Height, int Width, float Direction, int Speed, Color Tint) {
			Vector2 SpeedComponents = MGMath.CalculateXYMagnitude(Direction, Speed);

			AddParticle(Image, Top, Left, Height, Width, SpeedComponents.X, SpeedComponents.Y, Tint);
		}

		public void AddParticle(Texture2D Image, float Top, float Left, int Height, int Width, float SpeedX, float SpeedY, Color Tint) {
			ParticleInfo NewBullet = new ParticleInfo();

			NewBullet.DrawRegion.X = (int)Left;
			NewBullet.DrawRegion.Y = (int)Top;
			NewBullet.DrawRegion.Width = Width;
			NewBullet.DrawRegion.Height = Height;

			NewBullet.Image = Image;
			NewBullet.SpeedX = SpeedX;
			NewBullet.SpeedY = SpeedY;
			NewBullet.Tint = Tint;

			cParticleList.Add(NewBullet);
		}

		public void Update(GameTime CurrTime) {
			int Ctr;
			ParticleInfo CurrBullet;

			for (Ctr = 0; Ctr < cParticleList.Count; Ctr++) {
				CurrBullet = cParticleList[Ctr];

				CurrBullet.DrawRegion.X += (int)CurrBullet.SpeedX;
				CurrBullet.DrawRegion.Y -= (int)CurrBullet.SpeedY;

				if ((CurrBullet.SpeedX <= 0) && (CurrBullet.DrawRegion.X < -1 * cParticleList[Ctr].DrawRegion.Width)) {  //Bullet moved off screen left
					if (WrapScreenEdges == false) {
						cParticleList.RemoveAt(Ctr);
						continue;
					} else {
						CurrBullet.DrawRegion.X = cGraphDev.Viewport.Bounds.Width;
					}
				} else if ((CurrBullet.SpeedX >= 0) && (CurrBullet.DrawRegion.X > cGraphDev.Viewport.Bounds.Width)) {  //Bullet moved off screen right
					if (WrapScreenEdges == false) {
						cParticleList.RemoveAt(Ctr);
						continue;
					} else {
						CurrBullet.DrawRegion.X = cParticleList[Ctr].DrawRegion.Width * -1;
					}
				} else if ((CurrBullet.SpeedY >= 0) && (CurrBullet.DrawRegion.Y < -1 * cParticleList[Ctr].DrawRegion.Height)) {  //Bullet moved off screen top
					if (WrapScreenEdges == false) {
						cParticleList.RemoveAt(Ctr);
						continue;
					} else {
						CurrBullet.DrawRegion.Y = cGraphDev.Viewport.Bounds.Height;
					}
				} else if ((CurrBullet.SpeedY <= 0) && (CurrBullet.DrawRegion.Y > cGraphDev.Viewport.Bounds.Height)) {  //Bullet moved off screen bottom
					if (WrapScreenEdges == false) {
						cParticleList.RemoveAt(Ctr);
						continue;
					} else {
						CurrBullet.DrawRegion.Y = cParticleList[Ctr].DrawRegion.Height * -1;
					}
				}

				cParticleList[Ctr] = CurrBullet;
			}
		}

		public void Draw() {
			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			foreach (ParticleInfo CurrBullet in cParticleList) {
				cDrawBatch.Draw(CurrBullet.Image, CurrBullet.DrawRegion, CurrBullet.Image.Bounds, CurrBullet.Tint);
			}

			cDrawBatch.End();
		}
	}

	public struct ParticleInfo {
		public Color Tint;
		public Rectangle DrawRegion;
		public Texture2D Image;
		public float SpeedX, SpeedY;
	}
}