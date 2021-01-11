using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MDLN.MGTools;

namespace MDLN
{
	class Missile : PhysicalObject, IParticle {
		public Vector2 Speed;

		public Missile(GraphicsDevice GraphDev, TextureAtlas TxtrAtlas) : base (GraphDev, TxtrAtlas) {

		}

		public int GetWidth() {
			return crectExtents.Width;
		}

		public int GetHeight() {
			return crectExtents.Height;
		}

		public Vector2 GetTopLeft() {
			return new Vector2(crectExtents.X, crectExtents.Y);
		}

		public float GetSpeedX() {
			return Speed.X;
		}

		public float GetSpeedY() {
			return Speed.Y;
		}

		public void SetTopLeft(float Left, float Top) {
			Vector2 vCenter;

			vCenter.X = crectExtents.X + (crectExtents.Width / 2);
			vCenter.Y = crectExtents.Y + (crectExtents.Height / 2);

			CenterPoint = vCenter;

			return;
		}

		public override bool Update(GameTime CurrTime) {
			Vector2 vNewPos;
			
			base.Update(CurrTime);

			//Return True to keep this alive, false to have it removed
			vNewPos = CenterPoint;
			vNewPos.X += Speed.X;
			vNewPos.Y += Speed.Y;

			if (vNewPos.X < -1 * Width * Scale.X) { //Off the left of the screen
				return false;
			}

			if (vNewPos.X > (Width * Scale.X) + cGraphDev.Viewport.Width) { //Off the right of the screen
				return false;
			}

			if (vNewPos.Y < -1 * Height * Scale.Y) { //Off the top of the screen
				return false;
			}

			if (vNewPos.Y > (Width * Scale.Y) + cGraphDev.Viewport.Height) { //Off the bottom of the screen
				return false;
			}

			CenterPoint = vNewPos;

			return true;
		}
	}
}
