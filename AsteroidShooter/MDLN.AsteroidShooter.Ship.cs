using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

namespace MDLN.AsteroidShooter
{
	public class Ship : Container, ICollidable, IVisible {
		private const float SPEEDSTEP = 0.03f;
		private const float SPEEDMAX = 3.0f;

		private Texture2D cShipTexture;
		public float cRotation, cSpeedX, cSpeedY;
		private Vector2 cOrigin;
		private Rectangle cDrawRegion;
		private List<CollisionRegion> cCollisionList;
		private Color cTint;

		public Ship(GraphicsDevice GraphDev, int HeightAndWidth) : base(GraphDev, HeightAndWidth, HeightAndWidth) {
			int DrawSize;
			CollisionRegion CollideRegion = new CollisionRegion();

			//Diagonal of square = SqRt(2) * SideLength
			//That is max with of ship rotated
			DrawSize = (int)((float)HeightAndWidth / (float)Math.Sqrt(2));

			//cDrawRegion = new Rectangle();
			cDrawRegion.X = ((HeightAndWidth - DrawSize) / 2) + (DrawSize / 2);
			cDrawRegion.Y = ((HeightAndWidth - DrawSize) / 2) + (DrawSize / 2);
			cDrawRegion.Width = DrawSize;
			cDrawRegion.Height = DrawSize;

			cSpeedX = 0;
			cSpeedY = 0;

			ImageInitialAngle = 0;

			cCollisionList = new List<CollisionRegion>();
			CollideRegion.Origin.X = Left + cDrawRegion.X + (DrawSize / 2);
			CollideRegion.Origin.Y = Top + cDrawRegion.Y + (DrawSize / 2);;
			CollideRegion.Radius = (DrawSize / 2) * 0.9f;

			cCollisionList.Add(CollideRegion);

			cTint = Color.White;

			MouseRotate = false;
		}

		public bool MouseRotate { get; set; }

		public Texture2D ShipTexture {
			get {
				return cShipTexture;
			}

			set {
				cShipTexture = value;

				//Origin is based on the source image, not the destination
				cOrigin.X = cShipTexture.Width / 2;
				cOrigin.Y = cShipTexture.Height / 2;

				HasChanged = true;
			}
		}

		public override int Left {
			get {
				return base.Left;
			}

			set {
				base.Left = value;

				CollisionRegion CollideRegion = cCollisionList[0];
				CollideRegion.Origin.X = Left + value + Width / 2;
				cCollisionList[0] = CollideRegion;
			}
		}

		public override int Top {
			get {
				return base.Top;
			}

			set {
				base.Top = value;

				CollisionRegion CollideRegion = cCollisionList[0];
				CollideRegion.Origin.Y = Top + value + Height / 2;
				cCollisionList[0] = CollideRegion;
			}
		}

		public override Vector2 TopLeft {
			get {
				return base.TopLeft;
			}

			set {
				base.TopLeft = value;

				CollisionRegion CollideRegion = cCollisionList[0];
				CollideRegion.Origin.Y = value.Y + Height / 2;
				CollideRegion.Origin.X = value.X + Width / 2;
				cCollisionList[0] = CollideRegion;
			}
		}

		public float ImageInitialAngle { get; set; }

		public Color ImageTint { 
			get {
				return cTint;
			}

			set {
				cTint = value;
				HasChanged = true;
			}
		}

		public IEnumerable<CollisionRegion> GetCollisionRegions() {
			return cCollisionList;
		}

		public bool TestCollision(ICollidable TestObj) {
			return TestCollision(TestObj.GetCollisionRegions());
		}

		public bool TestCollision(IEnumerable<CollisionRegion> TestRegions) {
			foreach (CollisionRegion CurrRegion in TestRegions) {
				foreach (CollisionRegion MyRegion in cCollisionList) {
					if (MGMath.TestCircleCollision(CurrRegion.Origin, CurrRegion.Radius, MyRegion.Origin, MyRegion.Radius) == true) {
						return true;
					}
				}
			}

			return false;
		}

		public Vector2 GetCenterCoordinates() {
			Vector2 CenterCoords;

			CenterCoords.X = Left + (Width / 2);
			CenterCoords.Y = Top + (Height / 2);

			return CenterCoords;
		}

		protected override void DrawContents(GameTime CurrTime) {
			cDrawBatch.Draw(cShipTexture, cDrawRegion, cShipTexture.Bounds, cTint, -1 * cRotation + ImageInitialAngle, cOrigin, SpriteEffects.None, 0);
		}

		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeys, MouseState CurrMouse) {
			Vector2 SpeedAdjust, NewPos, ShipCenter;
			//float RotatDegrees;

			if (MouseRotate == false) {
				if (CurrKeys.IsKeyDown(Keys.Left) == true) {
					cRotation += 0.03f;

					if (cRotation > 6.28318531f) {
						cRotation = 0;
					}

					HasChanged = true;
				}

				if (CurrKeys.IsKeyDown(Keys.Right) == true) {
					cRotation -= 0.03f;

					if (cRotation < 0) {
						cRotation = 6.28318531f;
					}

					HasChanged = true;
				}

				if (CurrKeys.IsKeyDown(Keys.Up) == true) {
					SpeedAdjust = MGMath.CalculateXYMagnitude(cRotation, 0.1f);

					cSpeedX -= SpeedAdjust.X;
					cSpeedY += SpeedAdjust.Y;
				}

				if (CurrKeys.IsKeyDown(Keys.Down) == true) {
					SpeedAdjust = MGMath.CalculateXYMagnitude(cRotation, 0.1f);

					cSpeedX += SpeedAdjust.X;
					cSpeedY -= SpeedAdjust.Y;
				}

			} else {
				ShipCenter.Y = Height / 2;
				ShipCenter.X = Width / 2;

				cRotation = MGMath.GetAngleFromPoints(ShipCenter, CurrMouse.Position.ToVector2()) + ImageInitialAngle;

				if (CurrKeys.IsKeyDown(Keys.W) == true) {
					cSpeedY -= SPEEDSTEP;
				} else if (cSpeedY < 0) {
					cSpeedY += SPEEDSTEP / 2;
				}

				if (CurrKeys.IsKeyDown(Keys.S) == true) {
					cSpeedY += SPEEDSTEP;
				} else if (cSpeedY > 0) {
					cSpeedY -= SPEEDSTEP / 2;
				}

				if (cSpeedY > SPEEDMAX) {
					cSpeedY = SPEEDMAX;
				} else if (cSpeedY < SPEEDMAX * -1) {
					cSpeedY = SPEEDMAX * -1;
				}

				if (CurrKeys.IsKeyDown(Keys.A) == true) {
					cSpeedX -= SPEEDSTEP;
				} else if (cSpeedX < 0) {
					cSpeedX += SPEEDSTEP / 2;
				} 

				if (CurrKeys.IsKeyDown(Keys.D) == true) {
					cSpeedX += SPEEDSTEP;
				} else if (cSpeedX > 0) {
					cSpeedX -= SPEEDSTEP / 2;
				}

				if (cSpeedX < SPEEDMAX * -1) {
					cSpeedX = SPEEDMAX * -1;
				} else if (cSpeedX > SPEEDMAX) {
					cSpeedX = SPEEDMAX;
				}
			}

			NewPos = TopLeft;
			NewPos.X += cSpeedX;
			NewPos.Y += cSpeedY;

			TopLeft = NewPos;

			if (Top < -1 * (cDrawRegion.Height  + cDrawRegion.X)) {
				Top = cGraphicsDevice.Viewport.Bounds.Height;
			}

			if (Top > cGraphicsDevice.Viewport.Bounds.Height) {
				Top = -1 * cDrawRegion.Height;
			}

			if (Left < -1 * (cDrawRegion.Width + cDrawRegion.X)) {
				Left = cGraphicsDevice.Viewport.Bounds.Width;
			}

			if (Left > cGraphicsDevice.Viewport.Bounds.Width) {
				Left = -1 * cDrawRegion.Width;
			}
		}
	}
}