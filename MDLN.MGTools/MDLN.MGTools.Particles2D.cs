using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	/// <summary>
	/// Class to track, update, and draw particles in 2D graphics.
	/// </summary>
	public class Particles2D {
		private GraphicsDevice cGraphDev;
		private SpriteBatch cDrawBatch;
		/// <summary>
		/// List of all particles this object is managing
		/// </summary>
		public List<ParticleInfo> cParticleList;

		/// <summary>
		/// If a particle reaches the edge of the screen should it appear on the opposite side or be removed
		/// </summary>
		/// <value><c>true</c> if wrap screen edges; otherwise, <c>false</c>.</value>
		public bool WrapScreenEdges { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.Particles2D"/> class.
		/// </summary>
		/// <param name="GraphDev">Graph device.</param>
		public Particles2D(GraphicsDevice GraphDev) {
			cGraphDev = GraphDev;

			cDrawBatch = new SpriteBatch(cGraphDev);

			cParticleList = new List<ParticleInfo>();
		}

		/// <summary>
		/// Add a new particle to be managed by this instance of the class
		/// </summary>
		/// <param name="Image">Image to display for the particle.</param>
		/// <param name="Top">Top coordinate for the particle's initial position.</param>
		/// <param name="Left">Left coordinate of hte particle's initial position.</param>
		/// <param name="Height">Initial Height in pixes of the particle.</param>
		/// <param name="Width">Initial Width of the particle.</param>
		/// <param name="Direction">Angle in radians of the Direction the particle is travelling.</param>
		/// <param name="Speed">Initial Speed of the particle.</param>
		/// <param name="Tint">Tint or color overlay for the particle.</param>
		public void AddParticle(Texture2D Image, float Top, float Left, int Height, int Width, float Direction, int Speed, Color Tint) {
			Vector2 SpeedComponents = MGMath.CalculateXYMagnitude(Direction, Speed);

			AddParticle(Image, Top, Left, Height, Width, SpeedComponents.X, SpeedComponents.Y, Tint);
		}

		/// <summary>
		/// Add a new particle to be managed by this instance of the class
		/// </summary>
		/// <param name="Image">Image to display for the particle.</param>
		/// <param name="Top">Top coordinate for the particle's initial position.</param>
		/// <param name="Left">Left coordinate of hte particle's initial position.</param>
		/// <param name="Height">Initial Height in pixes of the particle.</param>
		/// <param name="Width">Initial Width of the particle.</param>
		/// <param name="SpeedX">Speed of the particle along the X axis.</param>
		/// <param name="SpeedY">Speed of the particle along the Y axis.</param>
		/// <param name="Tint">Tint or color overlay for the particle.</param>
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

		/// <summary>
		/// Calculate position and other changes in each particle
		/// </summary>
		/// <param name="CurrTime">Current time in the game.</param>
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

		/// <summary>
		/// Draw all of the particles to current render device
		/// </summary>
		public void Draw() {
			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			foreach (ParticleInfo CurrBullet in cParticleList) {
				cDrawBatch.Draw(CurrBullet.Image, CurrBullet.DrawRegion, CurrBullet.Image.Bounds, CurrBullet.Tint);
			}

			cDrawBatch.End();
		}
	}

	/// <summary>
	/// Structure that holds all information needed to manage a particle
	/// </summary>
	public struct ParticleInfo {
		/// <summary>
		/// Color to tine the particle
		/// </summary>
		public Color Tint;
		/// <summary>
		/// Rectangular region the particle will be drawn in
		/// </summary>
		public Rectangle DrawRegion;
		/// <summary>
		/// The texture to use as the image of the particle
		/// </summary>
		public Texture2D Image;
		/// <summary>
		/// The speed of the particle in the X direction
		/// </summary>
		public float SpeedX;
		/// <summary>
		/// Speed of the particle  alont the Y axis
		/// </summary>
		public float SpeedY;
	}
}