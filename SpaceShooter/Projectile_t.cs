using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MDLN.MGTools;

namespace MDLN
{
	class Projectile_t : PhysicalObject, IParticle {
		private float cnMaxSpeed;
		private float cnMaxTurn;
		private double ctTimeToLive, ctCreated;
		private ObjectManager cObjMgr;
		private Int32 cnTargetGroupID;
		private eProjectileType_t ceProjType;
		
		public ParticleEngine2D ParticleHandler;
		public Vector2 Speed;
		public Random Rand;

		public Projectile_t(GraphicsDevice GraphDev, TextureAtlas TxtrAtlas, ObjectManager ObjManager, Int32 nTargetGroupID, eProjectileType_t eType) : base (GraphDev, TxtrAtlas) {
			cObjMgr = ObjManager;
			cnTargetGroupID = nTargetGroupID;

			cnMaxTurn = (float)(5 * Math.PI / 180);

			ctTimeToLive = 2000;
			ctCreated = -1;
			ceProjType = eType;

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
			bool bRetVal = true;

			base.Update(CurrTime);

			if (ctCreated == -1) { //Just created, mark the time
				ctCreated = CurrTime.TotalGameTime.TotalMilliseconds;
			}

			switch (ceProjType) {
			case eProjectileType_t.Tracking:
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

				if (CurrTime.TotalGameTime.TotalMilliseconds - ctCreated > ctTimeToLive) {
					//Lived its full life, time to pop
					bRetVal = false;
				}

				break;
			case eProjectileType_t.Straight:
			default:
				//No logic, just flies straight
				if (cGraphDev.Viewport.Width < CenterPoint.X + Width) { //remove it when off screen
					bRetVal = false;
				}

				if (cGraphDev.Viewport.Height< CenterPoint.Y + Height) { //remove it when off screen
					bRetVal = false;
				}

				if (CenterPoint.X < -1 * Width) {
					bRetVal = false;
				}

				if (CenterPoint.Y < -1 * Height) {
					bRetVal = false;
				}

				break;
			}

			//Look for collisions with all possible targets
			foreach (PhysicalObject CurrObj in aTargetList) {
				if (CurrObj.TestCollision(this) == true) {
					//Hit a target! (tell it that it was hit)
					CurrObj.ReportCollision(CurrTime, this);
					bRetVal = false;
					break;
				}
			}

			//Apply the current speed
			vNewPos = CenterPoint;
			vNewPos.X += Speed.X;
			vNewPos.Y += Speed.Y;
			CenterPoint = vNewPos;

			if ((ParticleHandler != null) && (bRetVal == false)) {
				//Missile is expiring, throw some particles
				for (nCtr = 0; nCtr < 10; nCtr++) {
					ParticleHandler.AddParticle(new DustParticle(CenterPoint, Rand, cGraphDev, cImgAtlas, "spaceEffects_008.png"));
				}
			}

			//Return True to keep this alive, false to have it removed
			return bRetVal;
		}
	}

	class DustParticle : Particle2D {
		private TextureAtlas cTxtrAtlas;
		private string cstrTxtrName;

		public DustParticle(Vector2 CenterPt,  Random Rand, GraphicsDevice GraphDev, TextureAtlas TxtrAtlas, string strTxtrName) : base(GraphDev) {
			int nDist;
			
			cTxtrAtlas = TxtrAtlas;
			cstrTxtrName = strTxtrName;

			Tint = Color.White;
			TopLeft.X = CenterPt.X - (Width / 2);
			TopLeft.Y = CenterPt.Y - (Height / 2);
			Height = 16;
			Width = 16;
			TimeToLive = 100 + Rand.Next(0, 250);
			AlphaFade = true;
			nDist = (int)(Height * 2.5);
			TotalDistance.X = Rand.Next(-1 * nDist, nDist);
			TotalDistance.Y = Rand.Next(-1 * nDist, nDist);
			TotalRotate = Rand.Next(0, 6);

			return;
		}

		public override bool Draw(SpriteBatch DrawBatch) {
			Rectangle rectDraw = new Rectangle() {
				X = (int)TopLeft.X,
				Y = (int)TopLeft.Y,
				Height = this.Height,
				Width = this.Width,
			};

			cTxtrAtlas.DrawTile(cstrTxtrName, DrawBatch, rectDraw, Tint, Rotation);

			return true;
		}
	}

	enum eProjectileType_t {
		Straight,
		Tracking,
	}
}
