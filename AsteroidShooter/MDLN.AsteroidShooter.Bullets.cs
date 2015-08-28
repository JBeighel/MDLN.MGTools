using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.AsteroidShooter {
	public class Bullets {
		private GraphicsDevice cGraphDev;
		private SpriteBatch cDrawBatch;
		private List<BulletInfo> cBulletList;

		public Bullets(GraphicsDevice GraphDev) {
			cGraphDev = GraphDev;

			cDrawBatch = new SpriteBatch(cGraphDev);

			cBulletList = new List<BulletInfo>();
		}

		public void AddBullet(Texture2D Image, float Top, float Left, int Height, int Width, float Direction, int Speed, Color Tint) {
			Vector2 SpeedComponents = MGMath.CalculateXYMagnitude(Direction, Speed);

			AddBullet(Image, Top, Left, Height, Width, SpeedComponents.X, SpeedComponents.Y, Tint);
		}

		public void AddBullet(Texture2D Image, float Top, float Left, int Height, int Width, float SpeedX, float SpeedY, Color Tint) {
			BulletInfo NewBullet = new BulletInfo();

			NewBullet.DrawRegion.X = (int)Left;
			NewBullet.DrawRegion.Y = (int)Top;
			NewBullet.DrawRegion.Width = Width;
			NewBullet.DrawRegion.Height = Height;

			NewBullet.Image = Image;
			NewBullet.SpeedX = SpeedX;
			NewBullet.SpeedY = SpeedY;
			NewBullet.Tint = Tint;

			cBulletList.Add(NewBullet);
		}

		public void Update(GameTime CurrTime) {
			int Ctr;
			BulletInfo CurrBullet;

			for (Ctr = 0; Ctr < cBulletList.Count; Ctr++) {
				CurrBullet = cBulletList[Ctr];

				CurrBullet.DrawRegion.X += (int)CurrBullet.SpeedX;
				CurrBullet.DrawRegion.Y -= (int)CurrBullet.SpeedY;

				if ((CurrBullet.SpeedX <= 0) && (CurrBullet.DrawRegion.X < -1 * cBulletList[Ctr].DrawRegion.Width)) {  //Bullet moved off screen left
					cBulletList.RemoveAt(Ctr);
				} else if ((CurrBullet.SpeedX >= 0) && (CurrBullet.DrawRegion.X > cGraphDev.Viewport.Bounds.Width)) {  //Bullet moved off screen right
					cBulletList.RemoveAt(Ctr);
				} else if ((CurrBullet.SpeedY <= 0) && (CurrBullet.DrawRegion.Y < -1 * cBulletList[Ctr].DrawRegion.Height)) {  //Bullet moved off screen top
					cBulletList.RemoveAt(Ctr);
				} else if ((CurrBullet.SpeedY >= 0) && (CurrBullet.DrawRegion.Y > cGraphDev.Viewport.Bounds.Height)) {  //Bullet moved off screen bottom
					cBulletList.RemoveAt(Ctr);
				} else { //Bullet is on screen, update it in list
					cBulletList[Ctr] = CurrBullet;
				}
			}
		}

		public void Draw() {
			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			foreach (BulletInfo CurrBullet in cBulletList) {
				cDrawBatch.Draw(CurrBullet.Image, CurrBullet.DrawRegion, CurrBullet.Image.Bounds, CurrBullet.Tint);
			}

			cDrawBatch.End();
		}
	}

	public struct BulletInfo {
		public Color Tint;
		public Rectangle DrawRegion;
		public Texture2D Image;
		public float SpeedX, SpeedY;
	}
}