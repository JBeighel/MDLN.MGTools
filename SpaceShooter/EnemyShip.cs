using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MDLN.MGTools;

namespace MDLN {
	class EnemyShip : PhysicalObject {
		private float cnMaxSpeed;
		private float cnMaxTurn;
		private float cnMaxStrafeDist, cnMinStrafeDist;
		private Vector2 cvCurrSpeed;
		private ObjectManager cObjMgr;
		private Int32 cnTargetGroupID;

		private int cnLastMove = 0;
		public GameConsole cDevConsole;

		public EnemyShip(GraphicsDevice GraphDev, TextureAtlas TxtrAtlas, ObjectManager ObjManager, Int32 nTargetGroupID) : base(GraphDev, TxtrAtlas) {
			cObjMgr = ObjManager;
			cnTargetGroupID = nTargetGroupID;

			cnMaxTurn = (float)(5 * Math.PI / 180);
			cnMaxSpeed = 5;
			cnMaxStrafeDist = 300;
			cnMinStrafeDist = 150;

			TintColor = Color.CornflowerBlue;

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
			int nCtr, nBestTargetID = -1;
			float nBestTargetDist = 0, nCurrDist;
			Vector2 vNewPos;
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
					nCurrDist = AITools.SteerToCircleTarget(CenterPoint, aTargetList[nBestTargetID].CenterPoint, ObjectRotation, cnMaxTurn, true);
				}

				//Set the new movement direction, but don't change the speed
				SetMovement(nCurrDist, cnMaxSpeed);
			}

			//Apply current speed
			vNewPos = CenterPoint;
			vNewPos.X += cvCurrSpeed.X;
			vNewPos.Y += cvCurrSpeed.Y;

			CenterPoint = vNewPos;

			//Return True to keep this alive, false to have it removed
			return true;
		}
	}
}
