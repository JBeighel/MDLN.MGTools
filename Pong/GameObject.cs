using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

namespace MDLN {
	class GameObject : Container, ICollidable, IVisible {
		/// <summary>
		/// Collection of all regions that define when this object has been collided with
		/// </summary>
		protected List<CollisionRegion> cCollisionList;

		/// <summary>
		/// Constructor that will prepare this object for use
		/// </summary>
		/// <param name="GraphDev">Graphics device to use when drawing this object</param>
		/// <param name="HeightAndWidth">Height and width of the object on screen</param>
		public GameObject(GraphicsDevice GraphDev, int HeightAndWidth) : this(GraphDev, HeightAndWidth, HeightAndWidth) { }

		/// <summary>
		/// Constructor that will prepare this object for use
		/// </summary>
		/// <param name="GraphDev">Graphics device to use when drawing this object</param>
		/// <param name="Height">Height of the object on screen</param>
		/// <param name="Width">Width of the object on screen</param>
		public GameObject(GraphicsDevice GraphDev, int Height, int Width) : base (GraphDev, Height, Width) {
			cCollisionList = new List<CollisionRegion>();
		}

		/// <summary>
		/// Set the top or y coordinate of the top left corner of this object
		/// </summary>
		public override int Top {
			get {
				return base.Top;
			}

			set {
				Vector2 ObjTopLeft;

				ObjTopLeft.X = this.Left;
				ObjTopLeft.Y = value;

				this.TopLeft = ObjTopLeft;
			}
		}

		/// <summary>
		/// Set the left or X coordinate of the top left corner of this object
		/// </summary>
		public override int Left {
			get {
				return base.Left;
			}

			set {
				Vector2 ObjTopLeft;

				ObjTopLeft.X = value;
				ObjTopLeft.Y = this.Top;

				this.TopLeft = ObjTopLeft;
			}
		}

		/// <summary>
		/// Specify the screen coordinates of the top left corner of this object
		/// </summary>
		public override Vector2 TopLeft {
			get {
				return base.TopLeft;
			}

			set {
				int Ctr;
				Vector2 CenterPt;
				CollisionRegion CurrRegion;

				base.TopLeft = value;

				CenterPt = GetCenterCoordinates();

				//Update all the collision regions for this object to use an updated origin
				for (Ctr = 0; Ctr < cCollisionList.Count; Ctr++) {
					CurrRegion = cCollisionList[Ctr];

					CurrRegion.Origin = CenterPt;

					cCollisionList[Ctr] = CurrRegion;
				}
			}
		}

		/// <summary>
		/// Retrieve a lost of all collision detection ranges
		/// </summary>
		/// <returns></returns>
		public IEnumerable<CollisionRegion> GetCollisionRegions() {
			return cCollisionList;
		}

		/// <summary>
		/// Test that this object is colliding with a specific collidable object
		/// </summary>
		/// <param name="TestObj">Object to test collision with</param>
		/// <returns>Trut if this object is collidign with the specified object</returns>
		public bool TestCollision(ICollidable TestObj) {
			return TestCollision(TestObj.GetCollisionRegions());
		}

		/// <summary>
		/// Test if this object is colliding with any object in a collection of regions
		/// </summary>
		/// <param name="TestRegions">Collection of regions to test collisions with</param>
		/// <returns>True if this object collides with any of the collision regions</returns>
		public bool TestCollision(IEnumerable<CollisionRegion> TestRegions) {
			foreach (CollisionRegion CurrRegion in TestRegions) { //Loop through all specified regions
				foreach (CollisionRegion MyRegion in cCollisionList) { //Loop through all of this objects regions
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

		public void AddCollisionRegion(CollisionRegion CollideReg) {
			cCollisionList.Add(CollideReg);
		}
	}
}
