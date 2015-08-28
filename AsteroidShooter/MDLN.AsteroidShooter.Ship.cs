using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace MDLN.AsteroidShooter
{
	public class Ship : Container {
		private Texture2D cShipTexture;
		public float cRotation, cSpeedX, cSpeedY;
		private Vector2 cOrigin;
		private Rectangle cDrawRegion;
		//private KeyboardState cPriorKeys;

		public Ship(GraphicsDevice GraphDev, int HeightAndWidth) : base(GraphDev, HeightAndWidth, HeightAndWidth) {
			int DrawSize;

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
		}

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

		public override void DrawContents(GameTime CurrTime) {
			cDrawBatch.Draw(cShipTexture, cDrawRegion, cShipTexture.Bounds, Color.White, -1 * cRotation + 1.570796f, cOrigin, SpriteEffects.None, 0);
		}

		public override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeys, MouseState CurrMouse) {
			Vector2 SpeedAdjust;
			//float RotatDegrees;

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
				SpeedAdjust = CalculateXYMagnitude(cRotation, 0.1f);

				cSpeedX -= SpeedAdjust.X;
				cSpeedY += SpeedAdjust.Y;
			}

			if (CurrKeys.IsKeyDown(Keys.Down) == true) {
				SpeedAdjust = CalculateXYMagnitude(cRotation, 0.1f);

				cSpeedX += SpeedAdjust.X;
				cSpeedY -= SpeedAdjust.Y;
			}

			Left -= (int)cSpeedX;
			Top -= (int)cSpeedY;

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

		private Vector2 CalculateXYMagnitude(float Angle, float Magnitude) {
			Vector2 Components;

			Components.X = (float)(Math.Cos(Angle) * Magnitude);
			Components.Y = (float)(Math.Sin(Angle) * Magnitude);

			return Components;
		}
	}
}

