using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MDLN.MGTools;

namespace MDLN
{
	class Missile : PhysicalObject, IParticle {
		private float cnMaxSpeed;
		private float cnMaxTurn;
		private ObjectManager cObjMgr;
		private Int32 cnTargetGroupID;

		public Vector2 Speed;

		public Missile(GraphicsDevice GraphDev, TextureAtlas TxtrAtlas, ObjectManager ObjManager, Int32 nTargetGroupID) : base (GraphDev, TxtrAtlas) {
			cObjMgr = ObjManager;
			cnTargetGroupID = nTargetGroupID;

			cnMaxTurn = (float)(5 * Math.PI / 180);

			return;
		}

		public void SetMovement(float nDirection, float nVelocity) {
			while (nDirection > Math.PI) {
				nDirection -= (float)(2 * Math.PI);
			}

			while (nDirection < - 1 * Math.PI) {
				nDirection += (float)(2 * Math.PI);
			}

			ObjectRotation = nDirection;

			Speed.X = (float)(nVelocity * Math.Cos(nDirection));
			Speed.Y = (float)(nVelocity * Math.Sin(nDirection));
			cnMaxSpeed = nVelocity;
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
			int nCtr, nBestTargetID = -1;
			float nBestTargetDist = 0, nCurrDist;
			List<PhysicalObject> aTargetList = cObjMgr[cnTargetGroupID];

			base.Update(CurrTime);

			//Find a target to track
			for (nCtr = 0; nCtr < aTargetList.Count; nCtr++) {
				nCurrDist = MGMath.SquaredDistanceBetweenPoints(CenterPoint, aTargetList[nCtr].CenterPoint);

				if (nBestTargetID == -1) { //No target picked
					nBestTargetID = nCtr;
					nBestTargetDist = nCurrDist;
				} else if (nCurrDist < nBestTargetDist) {
					nBestTargetID = nCtr;
					nBestTargetDist = nCurrDist;
				}
			}

			if (nBestTargetID != -1) { //Found a valid target, steer toward it
				nCurrDist = AITools.SteerTowardTarget(CenterPoint, aTargetList[nBestTargetID].CenterPoint, ObjectRotation, cnMaxTurn);

				//Set the new movement direction, but don't change the speed
				SetMovement(nCurrDist, cnMaxSpeed);
			}

			//Apply the current speed
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

			//Return True to keep this alive, false to have it removed
			return true;
		}
	}
}
