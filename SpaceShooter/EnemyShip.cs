using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MDLN.MGTools;

namespace MDLN {
	class EnemyShip : PhysicalObject {
		private const Int32 ctHitFlashDuration = 500;

		private float cnMaxSpeed;
		private float cnMaxTurn;
		private float cnMaxStrafeDist, cnMinStrafeDist;
		private Vector2 cvCurrSpeed;
		private ObjectManager cObjMgr;
		private Int32 cnTargetGroupID;
		private Int32 cnAvoidGroupID;
		private double ctHitFlashUntil;
		private Color cclrNormalColor;
		private bool cbStrafeCW;
		private PhysicalObject cTarget;
		private PhysicalObject cAvoid;

		private int cnLastMove = 0;
		public GameConsole cDevConsole;

		public EnemyShip(GraphicsDevice GraphDev, TextureAtlas TxtrAtlas, ObjectManager ObjManager, Int32 nTargetGroupID, Int32 nAvoidGroupID) : base(GraphDev, TxtrAtlas) {
			cObjMgr = ObjManager;
			cnTargetGroupID = nTargetGroupID;
			cnAvoidGroupID = nAvoidGroupID;

			cnMaxTurn = (float)(5 * Math.PI / 180);
			cnMaxSpeed = 5;
			cnMaxStrafeDist = 300;
			cnMinStrafeDist = 150;

			ctHitFlashUntil = 0;
			cbStrafeCW = true;

			cclrNormalColor = Color.CornflowerBlue;
			TintColor = cclrNormalColor;

			Drawing += DrawHandle;

			return;
		}

		private bool DrawHandle(PhysicalObject CurrObj, GraphicsDevice gdDevice, SpriteBatch dbDraw) {
			if (cTarget != null) {
				DrawTools.DrawLine(gdDevice, dbDraw, Color.Red, 2, CenterPoint, cTarget.CenterPoint);
			}

			if (cAvoid != null) {
				DrawTools.DrawLine(gdDevice, dbDraw, Color.Cyan, 2, CenterPoint, cAvoid.CenterPoint);
			}

			return true;
		}

		public void RandomizeAttributes(Random Rand) {
			cnMaxTurn = (float)(5 + (4 * (0.5 - Rand.NextDouble())));
			cnMaxTurn *= (float)(Math.PI / 180); //Convert to radians
			cnMaxSpeed = (float)(5 + (3 * (0.5 - Rand.NextDouble())));
			cnMaxStrafeDist = (float)(300 + (200 * (0.5 - Rand.NextDouble())));
			cnMinStrafeDist = (float)(150 + (100 * (0.5 - Rand.NextDouble())));

			if (Rand.NextDouble() > 0.5) {
				cbStrafeCW = true;
			} else {
				cbStrafeCW = false;
			}

			return;
		}

		public void SetMovement(float nDirection, float nVelocity) {
			while (nDirection > Math.PI) {
				nDirection -= (float)(2 * Math.PI);
			}

			while (nDirection < -1 * Math.PI) {
				nDirection += (float)(2 * Math.PI);
			}

			ObjectRotation = nDirection;

			cvCurrSpeed.X = (float)(nVelocity * Math.Cos(nDirection));
			cvCurrSpeed.Y = (float)(nVelocity * Math.Sin(nDirection));
			cnMaxSpeed = nVelocity;
		}

		public override bool Update(GameTime CurrTime) {
			int nCtr, nBestTargetID = -1, nBestAvoidID = -1;
			float nBestTargetDist = 0, nBestAvoidDist = 0, nCurrDist;
			Vector2 vNewPos;
			List<PhysicalObject> aTargetList = cObjMgr[cnTargetGroupID];
			List<PhysicalObject> aAvoidList = cObjMgr[cnAvoidGroupID];

			base.Update(CurrTime);

			cTarget = null;
			cAvoid = null;

			//Find a target to track
			for (nCtr = 0; nCtr < aTargetList.Count; nCtr++) {
				nCurrDist = MGMath.SquaredDistanceBetweenPoints(CenterPoint, aTargetList[nCtr].CenterPoint);

				if (nBestTargetID == -1) { //No target picked
					nBestTargetID = nCtr;
					nBestTargetDist = nCurrDist;
					cTarget = aTargetList[nCtr];
				} else if (nCurrDist < nBestTargetDist) {
					nBestTargetID = nCtr;
					nBestTargetDist = nCurrDist;
					cTarget = aTargetList[nCtr];
				}
			}

			for (nCtr = 0; nCtr < aAvoidList.Count; nCtr += 1) {
				nCurrDist = MGMath.SquaredDistanceBetweenPoints(CenterPoint, aAvoidList[nCtr].CenterPoint);

				if (nBestAvoidID == -1) { //No target picked
					nBestAvoidID = nCtr;
					nBestAvoidDist = nCurrDist;
					cAvoid = aAvoidList[nCtr];
				} else if (nCurrDist < nBestAvoidDist) {
					nBestAvoidID = nCtr;
					nBestAvoidDist = nCurrDist;
					cAvoid = aAvoidList[nCtr];
				}
			}

			if ((nBestAvoidID != -1) && (nBestAvoidDist < nBestTargetDist) && (nBestAvoidDist < cnMaxStrafeDist * cnMaxStrafeDist)) {
				//Thing to avoid is closer than the target, it gets priority
				nBestTargetID = -1;
			}

			if (nBestTargetID != -1) { //Found a valid target, attack it
				if (nBestTargetDist > (cnMaxStrafeDist * cnMaxStrafeDist)) {
					if (cnLastMove != 1) {
						cDevConsole?.AddText("Move closer");
					}
					cnLastMove = 1;
					//Too far away to strafe, move closer
					nCurrDist = AITools.SteerTowardTarget(CenterPoint, aTargetList[nBestTargetID].CenterPoint, ObjectRotation, cnMaxTurn);
				} else if (nBestTargetDist < cnMinStrafeDist * cnMinStrafeDist) {
					if (cnLastMove != 2) {
						cDevConsole?.AddText("Back away");
					}
					cnLastMove = 2;
					//Too close, steer away from target
					nCurrDist = AITools.SteerAwayFromTarget(CenterPoint, aTargetList[nBestTargetID].CenterPoint, ObjectRotation, cnMaxTurn);
				}  else { //Close enough, strafe around the target
					if (cnLastMove != 3) {
						cDevConsole?.AddText("Strafe");
					}
					cnLastMove = 3;
					nCurrDist = AITools.SteerToCircleTarget(CenterPoint, aTargetList[nBestTargetID].CenterPoint, ObjectRotation, cnMaxTurn, cbStrafeCW);
				}

				//Set the new movement direction, but don't change the speed
				SetMovement(nCurrDist, cnMaxSpeed);
			} else if (nBestAvoidID != -1) {
				nCurrDist = AITools.SteerAwayFromTarget(CenterPoint, aAvoidList[nBestAvoidID].CenterPoint, ObjectRotation, cnMaxTurn);

				SetMovement(nCurrDist, cnMaxSpeed);
			}

			//Apply current speed
			vNewPos = CenterPoint;
			vNewPos.X += cvCurrSpeed.X;
			vNewPos.Y += cvCurrSpeed.Y;

			CenterPoint = vNewPos;

			if (ctHitFlashUntil > CurrTime.TotalGameTime.TotalMilliseconds) {
				//Flash red when being hit
				this.TintColor = Color.Red;
			} else {
				//Normal condition is white, for no tint
				this.TintColor = cclrNormalColor;
			}

			//Return True to keep this alive, false to have it removed
			return true;
		}
		
		public override void ReportCollision(GameTime CurrTime, PhysicalObject oCollider) {
			ctHitFlashUntil = CurrTime.TotalGameTime.TotalMilliseconds + ctHitFlashDuration;

			//Figure out what hit this and apply damage?

			return;
		}
	}
}
