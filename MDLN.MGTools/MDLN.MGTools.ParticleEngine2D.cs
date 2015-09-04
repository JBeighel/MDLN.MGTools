﻿using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	/// <summary>
	/// Class to track, update, and draw particles in 2D graphics.
	/// </summary>
	public class ParticleEngine2D {
		private GraphicsDevice cGraphDev;
		private SpriteBatch cDrawBatch;
		/// <summary>
		/// List of all particles this object is managing
		/// </summary>
		private List<Particle2D> cParticleList;

		/// <summary>
		/// If a particle reaches the edge of the screen should it appear on the opposite side or be removed
		/// </summary>
		/// <value><c>true</c> if wrap screen edges; otherwise, <c>false</c>.</value>
		public bool WrapScreenEdges { get; set; }

		/// <summary>
		/// List of all particles this object is managing
		/// </summary>
		public List<Particle2D> ParticleList { 
			get {
				return cParticleList;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.Particles2D"/> class.
		/// </summary>
		/// <param name="GraphDev">Graph device.</param>
		public ParticleEngine2D(GraphicsDevice GraphDev) {
			cGraphDev = GraphDev;

			cDrawBatch = new SpriteBatch(cGraphDev);

			cParticleList = new List<Particle2D>();
		}

		/// <summary>
		/// Add a new particle to be managed by this instance of the class
		/// </summary>
		/// <param name="NewParticle">Particle object to be managed.</param>
		public void AddParticle(Particle2D NewParticle) {
			cParticleList.Add(NewParticle);
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
			Particle2D NewBullet = new Particle2D();

			NewBullet.TopLeft.X = (int)Left;
			NewBullet.TopLeft.Y = (int)Top;
			NewBullet.Width = Width;
			NewBullet.Height = Height;

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
			Particle2D CurrBullet;

			for (Ctr = 0; Ctr < cParticleList.Count; Ctr++) {
				CurrBullet = cParticleList[Ctr];

				CurrBullet.Update(CurrTime);

				if ((CurrBullet.SpeedX <= 0) && (CurrBullet.TopLeft.X < -1 * cParticleList[Ctr].Width)) {  //Bullet moved off screen left
					if (WrapScreenEdges == false) {
						cParticleList.RemoveAt(Ctr);
						continue;
					} else {
						CurrBullet.TopLeft.X = cGraphDev.Viewport.Bounds.Width;
					}
				} else if ((CurrBullet.SpeedX >= 0) && (CurrBullet.TopLeft.X > cGraphDev.Viewport.Bounds.Width)) {  //Bullet moved off screen right
					if (WrapScreenEdges == false) {
						cParticleList.RemoveAt(Ctr);
						continue;
					} else {
						CurrBullet.TopLeft.X = cParticleList[Ctr].Width * -1;
					}
				} else if ((CurrBullet.SpeedY >= 0) && (CurrBullet.TopLeft.Y < -1 * cParticleList[Ctr].Height)) {  //Bullet moved off screen top
					if (WrapScreenEdges == false) {
						cParticleList.RemoveAt(Ctr);
						continue;
					} else {
						CurrBullet.TopLeft.Y = cGraphDev.Viewport.Bounds.Height;
					}
				} else if ((CurrBullet.SpeedY <= 0) && (CurrBullet.TopLeft.Y > cGraphDev.Viewport.Bounds.Height)) {  //Bullet moved off screen bottom
					if (WrapScreenEdges == false) {
						cParticleList.RemoveAt(Ctr);
						continue;
					} else {
						CurrBullet.TopLeft.Y = cParticleList[Ctr].Height * -1;
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

			foreach (Particle2D CurrParticle in cParticleList) {
				CurrParticle.Draw(cDrawBatch);
			}

			cDrawBatch.End();
		}
	}

	/// <summary>
	/// Structure that holds all information needed to manage a particle
	/// </summary>
	public class Particle2D : ICollidable {
		/// <summary>
		/// Color to tine the particle
		/// </summary>
		public Color Tint;
		/// <summary>
		/// Vector holding the top left coordinates of this particle
		/// </summary>
		public Vector2 TopLeft;
		/// <summary>
		/// Soecifies the height of the particle
		/// </summary>
		public int Height;
		/// <summary>
		/// The width of hte particle
		/// </summary>
		public int Width;
		/// <summary>
		/// Current rotation of the particle
		/// </summary>
		public float Rotation;
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
		/// <summary>
		/// Speed at which the particle is rotating
		/// </summary>
		public float SpeedRotate;
		/// <summary>
		/// Split the particle into more when it does, assuming all other conditions are met
		/// </summary>
		public bool SplitOnDeath;

		/// <summary>
		/// Retrieves a list of collision circles that represent where this object exists on screen
		/// </summary>
		/// <returns>The collision regions.</returns>
		public IEnumerable<CollisionRegion> GetCollisionRegions() {
			List<CollisionRegion> RegionList = new List<CollisionRegion>();
			CollisionRegion NewRegion = new CollisionRegion();

			NewRegion.Origin.X = TopLeft.X + (Width / 2);
			NewRegion.Origin.Y = TopLeft.Y + (Height / 2);

			if (Height > Width) {
				NewRegion.Radius = Height / 2;
			} else {
				NewRegion.Radius = Width / 2;
			}

			//NewRegion.Radius *= 0.9f; //Shrink the region a bit to account for image border

			RegionList.Add(NewRegion);

			return RegionList;
		}

		/// <summary>
		/// Tests if this object collides with another object based on the list of collision circles
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="TestObj">Test object.</param>
		public bool TestCollision(ICollidable TestObj) {
			return TestCollision(TestObj.GetCollisionRegions());
		}

		/// <summary>
		/// Tests if this object collides with another object based on the list of collision circles
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="TestRegions">Test regions.</param>
		public bool TestCollision(IEnumerable<CollisionRegion> TestRegions) {
			List<CollisionRegion> MyRegionsList = (List<CollisionRegion>)GetCollisionRegions();

			foreach (CollisionRegion CurrRegion in TestRegions) {
				foreach (CollisionRegion MyRegion in MyRegionsList) {
					if (MGMath.TestCircleCollision(CurrRegion.Origin, CurrRegion.Radius, MyRegion.Origin, MyRegion.Radius) == true) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Function to use to update this particle
		/// </summary>
		/// <param name="CurrTime">Curr time.</param>
		public virtual void Update(GameTime CurrTime) {
			TopLeft.X += SpeedX;
			TopLeft.Y -= SpeedY;

			Rotation += SpeedRotate;

			//Make sure the rotation stays between 0 and 360 degrees in radians
			if (Rotation <= 0) {
				Rotation = 6.28318531f;
			}

			if (Rotation > 6.28318531f) {
				Rotation = 0;
			}
		}

		/// <summary>
		/// Function to draw this particle to the screen
		/// </summary>
		/// <param name="DrawBatch">Draw batch.</param>
		public virtual void Draw(SpriteBatch DrawBatch) {
			Vector2 Origin;
			Rectangle DrawRegion;

			Origin.X = Image.Bounds.Width / 2;
			Origin.Y = Image.Bounds.Height / 2;

			DrawRegion.X = (int)(TopLeft.X + (Width / 2));
			DrawRegion.Y = (int)(TopLeft.Y + (Height / 2));
			DrawRegion.Width = Width;
			DrawRegion.Height = Height;

			DrawBatch.Draw(Image, DrawRegion, Image.Bounds, Tint, Rotation, Origin, SpriteEffects.None, 0);
		}
	}

	/// <summary>
	/// Holds information regarding a collision region.
	/// This region is a circle described by its origin/center point and radius
	/// </summary>
	public struct CollisionRegion {
		/// <summary>
		/// The center point of the circle
		/// </summary>
		public Vector2 Origin;
		/// <summary>
		/// The radius of the circle
		/// </summary>
		public float Radius;
	}

	/// <summary>
	/// Interface that can be used to aid in detecting collisions between objects
	/// </summary>
	public interface ICollidable {
		/// <summary>
		/// Retrieves a list of collision circles that represent where this object exists on screen
		/// </summary>
		/// <returns>The collision regions.</returns>
		IEnumerable<CollisionRegion> GetCollisionRegions();

		/// <summary>
		/// Tests if this object collides with another object based on the list of collision circles
		/// </summary>
		/// <returns><c>true</c>, if collision was tested, <c>false</c> otherwise.</returns>
		/// <param name="TestRegions">Test regions.</param>
		bool TestCollision(IEnumerable<CollisionRegion> TestRegions);

		/// <summary>
		/// Tests if this object collides with another object using the collision sircles
		/// </summary>
		/// <returns><c>true</c>, if collision is happening, <c>false</c> otherwise.</returns>
		/// <param name="TestObj">Test object.</param>
		bool TestCollision(ICollidable TestObj);
	}

	public interface IVisible {
		Vector2 GetCenterCoordinates();
	}
}