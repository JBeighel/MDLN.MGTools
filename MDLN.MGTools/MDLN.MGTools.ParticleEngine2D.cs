using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	/// <summary>
	/// Class to track, update, and draw particles in 2D graphics.
	/// </summary>
	public class ParticleEngine2D {
		private readonly GraphicsDevice cGraphDev;
		private readonly SpriteBatch cDrawBatch;
		/// <summary>
		/// List of all particles this object is managing
		/// </summary>
		private readonly List<Particle2D> cParticleList;

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
		/// Gets or sets the draw blending mode.
		/// </summary>
		/// <value>The draw blending mode.</value>
		public BlendState DrawBlendingMode { get; set; }

		/// <summary>
		/// Gets or sets the shader effect to use when rendering the particles.
		/// </summary>
		/// <value>The shader effect.</value>
		public Effect ShaderEffect { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.ParticleEngine2D"/> class.
		/// </summary>
		/// <param name="GraphDev">Graph device.</param>
		public ParticleEngine2D(GraphicsDevice GraphDev) {
			cGraphDev = GraphDev;

			cDrawBatch = new SpriteBatch(cGraphDev);

			cParticleList = new List<Particle2D>();

			DrawBlendingMode = BlendState.NonPremultiplied;

			ShaderEffect = null;
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
			List<int> IndexesToRemove = new List<int>();

			for (Ctr = 0; Ctr < cParticleList.Count; Ctr++) {
				CurrBullet = cParticleList[Ctr]; //Pull out he particle to update

				if (CurrBullet.Update(CurrTime) == false) { //Particle update says this particle no longer exists
					IndexesToRemove.Add(Ctr);
					continue;
				}

				if ((CurrBullet.SpeedX <= 0) && (CurrBullet.TopLeft.X < -1 * cParticleList[Ctr].Width)) {  //Bullet moved off screen left
					if (WrapScreenEdges == false) {
						IndexesToRemove.Add(Ctr);
						continue;
					} else {
						CurrBullet.TopLeft.X = cGraphDev.Viewport.Bounds.Width;
					}
				} else if ((CurrBullet.SpeedX >= 0) && (CurrBullet.TopLeft.X > cGraphDev.Viewport.Bounds.Width)) {  //Bullet moved off screen right
					if (WrapScreenEdges == false) {
						IndexesToRemove.Add(Ctr);
						continue;
					} else {
						CurrBullet.TopLeft.X = cParticleList[Ctr].Width * -1;
					}
				} else if ((CurrBullet.SpeedY >= 0) && (CurrBullet.TopLeft.Y < -1 * cParticleList[Ctr].Height)) {  //Bullet moved off screen top
					if (WrapScreenEdges == false) {
						IndexesToRemove.Add(Ctr);
						continue;
					} else {
						CurrBullet.TopLeft.Y = cGraphDev.Viewport.Bounds.Height;
					}
				} else if ((CurrBullet.SpeedY <= 0) && (CurrBullet.TopLeft.Y > cGraphDev.Viewport.Bounds.Height)) {  //Bullet moved off screen bottom
					if (WrapScreenEdges == false) {
						IndexesToRemove.Add(Ctr);
						continue;
					} else {
						CurrBullet.TopLeft.Y = cParticleList[Ctr].Height * -1;
					}
				}

				cParticleList[Ctr] = CurrBullet; //Place the changed values into the list
			}

			for (Ctr = IndexesToRemove.Count - 1; Ctr >= 0; Ctr--) {
				cParticleList.RemoveAt(Ctr);
			}
		}

		/// <summary>
		/// Draw all of the particles to current render device
		/// </summary>
		public void Draw() {
			cDrawBatch.Begin(SpriteSortMode.Immediate, DrawBlendingMode);

			foreach (Particle2D CurrParticle in cParticleList) {
				if (ShaderEffect != null) {
					//ShaderEffect.Parameters["TintColor"].SetValue(CurrParticle.Tint.ToVector4());
					ShaderEffect.Techniques[0].Passes[0].Apply();
				}
				CurrParticle.Draw(cDrawBatch);
			}

			cDrawBatch.End();
		}

		/// <summary>
		/// Draw all of the particles to current render device using an externally defined sprite batch
		/// </summary>
		public void Draw(SpriteBatch DrawBatch) {
			foreach (Particle2D CurrParticle in cParticleList) {
				if (ShaderEffect != null) {
					//ShaderEffect.Parameters["TintColor"].SetValue(CurrParticle.Tint.ToVector4());
					ShaderEffect.Techniques[0].Passes[0].Apply();
				}
				CurrParticle.Draw(DrawBatch);
			}
		}
	}

	/// <summary>
	/// Structure that holds all information needed to manage a particle
	/// </summary>
	public class Particle2D : ICollidable, IVisible {
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
		/// Current rotation in radians of the particle
		/// </summary>
		public float Rotation;
		/// <summary>
		/// The texture to use as the image of the particle
		/// </summary>
		public Texture2D Image;
		/// <summary>
		/// The speed of the particle in the X direction each frame
		/// </summary>
		public float SpeedX;
		/// <summary>
		/// Speed of the particle  alont the Y axis each frame
		/// </summary>
		public float SpeedY;
		/// <summary>
		/// Speed at which the particle is rotating in radians each frame
		/// </summary>
		public float SpeedRotate;
		/// <summary>
		/// The amount of time in milliseconds the particle should exist
		/// </summary>
		public double TimeToLive;
		/// <summary>
		/// Number of milliseconds to delay the creation of this parrticle
		/// </summary>
		public double tCreateDelay;
		/// <summary>
		/// True to have the particle fade over time, false to not fade
		/// </summary>
		public bool AlphaFade;
		/// <summary>
		/// Set true to have the rotation occur after moving, meaning it will follow a curved path
		/// False will rotate the image as it moves
		/// </summary>
		public bool bSpiralPath;
		/// <summary>
		/// Total distance the particle will travel across its life
		/// This does not factor in the SpeedX and SpeedY variables
		/// </summary>
		public Vector2 TotalDistance;
		/// <summary>
		/// Total distance the particle will rotate across its life
		/// This does not factor in the SpeedRotate variable
		/// </summary>
		public float TotalRotate;
		/// <summary>
		/// Time that the particle was created determined by its first update
		/// </summary>
		private double cCreationTime;
		/// <summary>
		/// Time when the particle was last updated
		/// </summary>
		private double ctLastUpdate;

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.Particle2D"/> class.
		/// </summary>
		public Particle2D() {
			TimeToLive = 0;
			AlphaFade = false;
			cCreationTime = -1;
		}

		/// <summary>
		/// Retrieves a list of collision circles that represent where this object exists on screen
		/// </summary>
		/// <returns>The collision regions.</returns>
		public IEnumerable<CollisionRegion> GetCollisionRegions() {
			List<CollisionRegion> RegionList = new List<CollisionRegion>();
			CollisionRegion NewRegion = new CollisionRegion() {
				Type = CollideType.Circle,
				Origin = new Vector2(TopLeft.X + (Width / 2), TopLeft.Y + (Height / 2)),
			};

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
					if (MyRegion.Type == CollideType.Circle) {
						if (CurrRegion.Type == CollideType.Circle) {
							if (MGMath.TestCircleCollisions(CurrRegion.Origin, CurrRegion.Radius, MyRegion.Origin, MyRegion.Radius) == true) {
								return true;
							}
						} else { //CurrRegion is a rectangle (MyRegion is Circle)
							if (MGMath.TestCircleRectangleCollision(CurrRegion.Origin, CurrRegion.RectOffsets, MyRegion.Origin, MyRegion.Radius) == true) {
								return true;
							}
						}
					} else { //My Region is a rectangle
						if (CurrRegion.Type == CollideType.Circle) {
							if (MGMath.TestCircleRectangleCollision(MyRegion.Origin, MyRegion.RectOffsets, CurrRegion.Origin, CurrRegion.Radius) == true) {
								return true;
							}
						} else { //CurrRegion is a rectangle (MyRegion is Rectangle)
							if (MGMath.TestRectangleCollisions(CurrRegion.Origin, CurrRegion.RectOffsets, MyRegion.Origin, MyRegion.RectOffsets) == true) {
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Function to use to update this particle
		/// </summary>
		/// <param name="CurrTime">Curr time.</param>
		public virtual bool Update(GameTime CurrTime) {
			double nLastUpdPct, nTotLifePct;

			TopLeft.X += SpeedX;
			TopLeft.Y -= SpeedY;

			if (cCreationTime < 0) {
				cCreationTime = CurrTime.TotalGameTime.TotalMilliseconds;
				nLastUpdPct = 0;
				nTotLifePct = 0;
			} else if (TimeToLive > 0) {
				nLastUpdPct = (CurrTime.TotalGameTime.TotalMilliseconds - ctLastUpdate) / TimeToLive;
				nTotLifePct = (CurrTime.TotalGameTime.TotalMilliseconds - cCreationTime) / TimeToLive;
			} else {
				nLastUpdPct = 0;
				nTotLifePct = 0;
			}

			if (tCreateDelay > 0) { //Particle is still being delayed
				if (tCreateDelay <= CurrTime.TotalGameTime.TotalMilliseconds - cCreationTime) { //Delay is over, update creation time to allow animation to begin
					tCreateDelay = 0;
					cCreationTime = CurrTime.TotalGameTime.TotalMilliseconds;
					ctLastUpdate = cCreationTime;
				}

				return true;
			}

			if ((TimeToLive > 0) && (CurrTime.TotalGameTime.TotalMilliseconds - cCreationTime >= TimeToLive)) {
				return false;
			}

			Rotation += SpeedRotate;

			TopLeft.X += (float)(TotalDistance.X * nLastUpdPct);
			TopLeft.Y += (float)(TotalDistance.Y * nLastUpdPct);
			Rotation += (float)(TotalRotate * nLastUpdPct);

			//Make sure the rotation stays between 0 and 360 degrees in radians
			if (Rotation <= 0) {
				Rotation += 2 * (float)Math.PI;
			}

			if (Rotation > 2 * (float)Math.PI) {
				Rotation -= 2 * (float)Math.PI; ;
			}

			if (AlphaFade == true) {
				Tint.A = (byte)(255 - (nTotLifePct * 255));
			}

			ctLastUpdate = CurrTime.TotalGameTime.TotalMilliseconds;

			return true;
		}

		/// <summary>
		/// Function to draw this particle to the screen
		/// </summary>
		/// <param name="DrawBatch">Draw batch.</param>
		public virtual bool Draw(SpriteBatch DrawBatch) {
			Vector2 Origin;
			Rectangle DrawRegion;
			double nLifePct;

			if (tCreateDelay > 0) { //Particle is still being delayed
				return true;
			}

			Origin.X = Image.Bounds.Width / 2;
			Origin.Y = Image.Bounds.Height / 2;

			DrawRegion.X = (int)(TopLeft.X + (Width / 2));
			DrawRegion.Y = (int)(TopLeft.Y + (Height / 2));
			DrawRegion.Width = Width;
			DrawRegion.Height = Height;

			if (bSpiralPath == true) {
				nLifePct = (ctLastUpdate - cCreationTime) / TimeToLive;
				Origin.X += (float)(nLifePct * TotalDistance.X * Image.Bounds.Width / DrawRegion.Width / 8);
				Origin.Y += (float)(nLifePct * TotalDistance.Y * Image.Bounds.Height / DrawRegion.Height /8);
			}

			DrawBatch.Draw(Image, DrawRegion, Image.Bounds, Tint, Rotation, Origin, SpriteEffects.None, 0);

			return true;
		}

		/// <summary>
		/// Returns the approximate center point of this object
		/// </summary>
		/// <returns></returns>
		public Vector2 GetCenterCoordinates() {
			Vector2 Center;
			Center.X = TopLeft.X + (Width / 2);
			Center.Y = TopLeft.Y + (Height / 2);

			return Center;
		}
	}

	/// <summary>
	/// Holds information regarding a collision region.
	/// This region is a circle described by its origin/center point and radius
	/// </summary>
	public struct CollisionRegion {
		/// <summary>
		/// Speicifes how to test this collision region
		/// </summary>
		public CollideType Type;
		/// <summary>
		/// The center point of the circle
		/// </summary>
		public Vector2 Origin;
		/// <summary>
		/// The radius of the circle
		/// </summary>
		public float Radius;
		/// <summary>
		/// For the rectangular version of this region these are offsets to the boundaries
		/// </summary>
		public Rectangle RectOffsets;
		/// <summary>
		/// List of vertexes defining a polygon shape
		/// </summary>
		public List<Vector2> Vertexes;
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

	/// <summary>
	/// Specifies how to test this collision region
	/// </summary>
	public enum CollideType {
		/// <summary>
		/// The region is a circle
		/// </summary>
		Circle,
		/// <summary>
		/// The region is a rectangle
		/// </summary>
		Rectangle,
		/// <summary>
		/// The region is a convex polygon made from 2D Vertexes
		/// </summary>
		ConvexPolygon,
	}

	/// <summary>
	/// Interface that offers common controls for any object that might be visible on the screen
	/// </summary>
	public interface IVisible {
		/// <summary>
		/// Retrieves the coordinates of the center of the control
		/// </summary>
		/// <returns>The center coordinates.</returns>
		Vector2 GetCenterCoordinates();

		/// <summary>
		/// Allows the object to update itself, this may be visal positions or
		/// any other action it needs to perform
		/// </summary>
		/// <param name="CurrTime"></param>
		bool Update(GameTime CurrTime);

		/// <summary>
		/// Tells the object to draw itself using the specified SpriteBatch for rendering the
		/// visible components.
		/// </summary>
		/// <param name="DrawBatch"></param>
		bool Draw(SpriteBatch DrawBatch);
	}
}