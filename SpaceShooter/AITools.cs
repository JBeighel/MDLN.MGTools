using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace MDLN.MGTools {
	public static class AITools {
		public static float SteerTowardTarget(Vector2 vMyPos, Vector2 vTargetPos, float nMyCurrDir, float nMaxTurn) {
			float nNewDir;
			Vector2 vMeToTarget = new Vector2() {
				X = vTargetPos.X - vMyPos.X,
				Y = vTargetPos.Y - vMyPos.Y,
			};

			if (vMeToTarget.X == 0) {
				if (((vMeToTarget.Y < 0) && (nMyCurrDir > 0)) || ((vMeToTarget.Y > 0) && (nMyCurrDir < 0))) {
					//Target is directly above/below me and I'm pointed the wrong way
					nNewDir = nMaxTurn;
				} else { //Target is directly above/blow me and I'm pointed toward it
					nNewDir = 0;
				}
			} else {
				nNewDir = (float)Math.Atan2(vMeToTarget.Y, vMeToTarget.X); //Direction to target
				nNewDir -= nMyCurrDir; //How far I must turn to face target

				if ((nNewDir > Math.PI) || (nNewDir < -1 * Math.PI)) {
					//Have to turn more than 180 degrees? The other way is shorter
					nNewDir *= -1;
				}

				//Don't exceed max turning speed
				if (nNewDir > nMaxTurn) {
					nNewDir = nMaxTurn;
				} else if (nNewDir < -1 * nMaxTurn) {
					nNewDir = -1 * nMaxTurn;
				}
			}

			return nMyCurrDir + nNewDir;
		}

		public static float SteerAwayFromTarget(Vector2 vMyPos, Vector2 vTargetPos, float nMyCurrDir, float nMaxTurn) {
			float nNewDir;
			Vector2 vMeToTarget = new Vector2() {
				X = vTargetPos.X - vMyPos.X,
				Y = vTargetPos.Y - vMyPos.Y,
			};

			if (vMeToTarget.X == 0) {
				if (((vMeToTarget.Y < 0) && (nMyCurrDir > 0)) || ((vMeToTarget.Y > 0) && (nMyCurrDir < 0))) {
					//Target is directly above/below me and I'm pointed the wrong way
					nNewDir = nMaxTurn;
				} else { //Target is directly above/blow me and I'm pointed toward it
					nNewDir = 0;
				}
			} else {
				nNewDir = (float)Math.Atan2(vMeToTarget.Y, vMeToTarget.X); //Direction to target

				//Set the desired direction the other way
				if (nNewDir > 0) {
					nNewDir -= (float)Math.PI;
				} else {
					nNewDir += (float)Math.PI;
				}

				nNewDir -= nMyCurrDir; //How far I must turn to face target

				if ((nNewDir > Math.PI) || (nNewDir < -1 * Math.PI)) {
					//Have to turn more than 180 degrees? The other way is shorter
					nNewDir *= -1;
				}

				//Don't exceed max turning speed
				if (nNewDir > nMaxTurn) {
					nNewDir = nMaxTurn;
				} else if (nNewDir < -1 * nMaxTurn) {
					nNewDir = -1 * nMaxTurn;
				}
			}

			return nMyCurrDir + nNewDir;
		}

		public static float SteerToCircleTarget(Vector2 vMyPos, Vector2 vTargetPos, float nMyCurrDir, float nMaxTurn, bool bClockWise) {
			float nNewDir;
			Vector2 vMeToTarget = new Vector2() {
				X = vTargetPos.X - vMyPos.X,
				Y = vTargetPos.Y - vMyPos.Y,
			};

			if (vMeToTarget.X == 0) {
				//Target is directly above/below me
				if (((Math.Abs(nMyCurrDir) > Math.PI) && (nMyCurrDir > 0)) || ((Math.Abs(nMyCurrDir) < Math.PI) && (nMyCurrDir < 0))) {
					//Pointed NW or SE
					nNewDir = nMaxTurn;
				} else { //Pointed NE or SW
					nNewDir = -1 * nMaxTurn;
				}
				nNewDir = nMaxTurn;
			} else {
				nNewDir = (float)Math.Atan2(vMeToTarget.Y, vMeToTarget.X); //Direction to target
				if (bClockWise == true) { //Adjust target 90 degrees to travel paralel to target
					nNewDir += (float)(Math.PI / 2);

					if (nNewDir > Math.PI) {//Crossed Y axis
						nNewDir -= (float)(2 * Math.PI);
					}
				} else {
					nNewDir -= (float)(Math.PI / 2);//this is screen clockwise

					if (nNewDir > Math.PI) { //Crossed Y axis
						nNewDir -= (float)(2 * Math.PI);
					}

					if (nNewDir < -1 * Math.PI) { //Crossed Y axis
						nNewDir += (float)(2 * Math.PI);
					}
				}

				nNewDir -= nMyCurrDir; //How far I must turn to face target

				if ((nNewDir > Math.PI) || (nNewDir < -1 * Math.PI)) {
					//Have to turn more than 180 degrees? The other way is shorter
					nNewDir *= -1;
				}

				//Don't exceed max turning speed
				if (nNewDir > nMaxTurn) {
					nNewDir = nMaxTurn;
				} else if (nNewDir < -1 * nMaxTurn) {
					nNewDir = -1 * nMaxTurn;
				}
			}

			return nMyCurrDir + nNewDir;
		}
	}
}
