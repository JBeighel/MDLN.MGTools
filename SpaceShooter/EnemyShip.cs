using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MDLN.MGTools;
using MDLN.Tools;

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
		private Int32 cnHealth;

		private int cnLastMove = 0;
		public GameConsole cDevConsole;

		public EnemyShip(GraphicsDevice GraphDev, TextureAtlas TxtrAtlas, ObjectManager ObjManager, Int32 nTargetGroupID, Int32 nAvoidGroupID) : base(GraphDev, TxtrAtlas) {
			cObjMgr = ObjManager;
			cnTargetGroupID = nTargetGroupID;
			cnAvoidGroupID = nAvoidGroupID;

			cnHealth = 100;

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
			//Debug stuff to help with movement algorithm
			/*
			if (cTarget != null) {
				DrawTools.DrawLine(gdDevice, dbDraw, Color.Red, 2, CenterPoint, cTarget.CenterPoint);
			}

			if (cAvoid != null) {
				DrawTools.DrawLine(gdDevice, dbDraw, Color.Cyan, 2, CenterPoint, cAvoid.CenterPoint);
			}
			*/

			Vector vHealthBar = new Vector();
			Vector2 vHealthStart, vHealthEnd;

			vHealthBar.SetPolarCoordinates(50f * (cnHealth / 100f), 0);

			vHealthStart = CenterPoint + new Vector2(-25, 25);
			vHealthEnd = vHealthStart + new Vector2((float)vHealthBar.Rectangular.Real, (float)vHealthBar.Rectangular.Imaginary);
			DrawTools.DrawLine(gdDevice, dbDraw, Color.Red, 5, vHealthStart, vHealthEnd);

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
			float nBestTargetDist = 0, nBestAvoidDist = 0, nCurrDist, nAvoidDir, nTargetDir;
			Vector2 vNewPos;
			List<PhysicalObject> aTargetList = cObjMgr[cnTargetGroupID];
			List<PhysicalObject> aAvoidList = cObjMgr[cnAvoidGroupID];

			base.Update(CurrTime);

			if (cnHealth <= 0) {
				//This enemy is dead :(
				//Should fire off some explosion particles

				return false;
			}

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

			if (nBestAvoidDist > cnMaxStrafeDist * cnMaxStrafeDist) {
				//Thing to avoid is too far away to worry about
				nBestAvoidID = -1;
				cAvoid = null;
			}

			if (nBestTargetID != -1) { //Found a valid target, attack it
				if (nBestTargetDist > (cnMaxStrafeDist * cnMaxStrafeDist)) {
					if (cnLastMove != 1) {
						cDevConsole?.AddText("Move closer");
					}
					cnLastMove = 1;
					//Too far away to strafe, move closer
					nTargetDir = AITools.SteerTowardTarget(CenterPoint, aTargetList[nBestTargetID].CenterPoint, ObjectRotation, cnMaxTurn);
				} else if (nBestTargetDist < cnMinStrafeDist * cnMinStrafeDist) {
					if (cnLastMove != 2) {
						cDevConsole?.AddText("Back away");
					}
					cnLastMove = 2;
					//Too close, steer away from target
					nTargetDir = AITools.SteerAwayFromTarget(CenterPoint, aTargetList[nBestTargetID].CenterPoint, ObjectRotation, cnMaxTurn);
				}  else { //Close enough, strafe around the target
					if (cnLastMove != 3) {
						cDevConsole?.AddText("Strafe");
					}
					cnLastMove = 3;
					nTargetDir = AITools.SteerToCircleTarget(CenterPoint, aTargetList[nBestTargetID].CenterPoint, ObjectRotation, cnMaxTurn, cbStrafeCW);
				}
			} else {
				nTargetDir = 0;
			}
			
			if (nBestAvoidID != -1) {
				nAvoidDir = AITools.SteerAwayFromTarget(CenterPoint, aAvoidList[nBestAvoidID].CenterPoint, ObjectRotation, cnMaxTurn);
			} else {
				nAvoidDir = 0;
			}

			if (cnTargetGroupID != -1) {
				if ((nBestAvoidID != -1) && (nBestAvoidDist < nBestTargetDist)) {
					//Thing to avoid is closer than the target, it gets priority
					nCurrDist = (0.75f * nAvoidDir) + (0.25f * nTargetDir);
				} else {
					//Target is closer, go for the kill
					nCurrDist = nTargetDir;
				}
			} else {
				//No target so just avoid?
				nCurrDist = nAvoidDir;
			}

			//Set the new movement direction, but don't change the speed
			SetMovement(nCurrDist, cnMaxSpeed);

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
			if (oCollider is Projectile_t) {
				switch (((Projectile_t)oCollider).ProjectileType) {
				case eProjectileType_t.Straight:
					cnHealth -= 15;
					break;
				case eProjectileType_t.Tracking:
					cnHealth -= 10;
					break;
				default:
					break;
				}

				if (cnHealth < 0) {
					cnHealth = 0;
				}
			}

			return;
		}
	}
}
