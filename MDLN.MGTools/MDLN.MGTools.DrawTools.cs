using MDLN.Tools;
using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	public static class DrawTools {
		public static void DrawLine(GraphicsDevice gdDevice, SpriteBatch dbDraw, Color clrColor, Int32 nWidth, Vector2 vStart, Vector2 vEnd) {
			Texture2D txrColor = new Texture2D(gdDevice, 1, 1);
			Rectangle rectLineBounds = new Rectangle();
			Vector vLineSeg = new Vector();
			Vector2 vTxrRotPt;
			float nLineRot;

			//Create the texture color
			txrColor.SetData(new Color[] { clrColor });

			//Create a vector representation of the line segment
			vLineSeg.SetRectangularCoordinates(vEnd.X - vStart.X, vEnd.Y - vStart.Y);

			//Figure out how far the rectangle needs to rotate in radians to be the line
			nLineRot = (float)(vLineSeg.Polar.Angle * Math.PI / 180);

			//Create the rectangle bounding box
			rectLineBounds.X = (int)vStart.X;
			rectLineBounds.Y = (int)vStart.Y;
			rectLineBounds.Width = (int)(vLineSeg.Polar.Length + 1);
			rectLineBounds.Height = nWidth;

			//Set the center of rotation to the top middle of the color texture
			vTxrRotPt = new Vector2(0, txrColor.Width / (float)2);

			//Finally, draw the line
			dbDraw.Draw(txrColor, rectLineBounds, txrColor.Bounds, Color.White, nLineRot, vTxrRotPt, SpriteEffects.None, 0);

			return;
		}

		public static void DrawLineSeries(GraphicsDevice gdDevice, SpriteBatch dbDraw, Color clrColor, Int32 nWidth, IEnumerable<Vector2> avLinePts) {
			Texture2D txrColor = new Texture2D(gdDevice, 1, 1);
			Rectangle rectLineBounds = new Rectangle();
			Vector vLineSeg = new Vector();
			Vector2 vTxrRotPt, vStartPt;
			bool bHaveStart = false;
			float nLineRot;

			//Create the texture color
			txrColor.SetData(new Color[] { clrColor });

			vStartPt = new Vector2();
			bHaveStart = false;
			foreach (Vector2 vCurrPt in avLinePts) {
				if (bHaveStart == true) {
					//Create a vector representation of the line segment
					vLineSeg.SetRectangularCoordinates(vCurrPt.X - vStartPt.X, vCurrPt.Y - vStartPt.Y);

					//Figure out how far the rectangle needs to rotate in radians to be the line
					nLineRot = (float)(vLineSeg.Polar.Angle * Math.PI / 180);

					//Create the rectangle bounding box
					rectLineBounds.X = (int)vStartPt.X;
					rectLineBounds.Y = (int)vStartPt.Y;
					rectLineBounds.Width = (int)(vLineSeg.Polar.Length + 1);
					rectLineBounds.Height = nWidth;

					//Set the center of rotation to the top middle of the color texture
					vTxrRotPt = new Vector2(0, txrColor.Width / (float)2);

					//Finally, draw the line
					dbDraw.Draw(txrColor, rectLineBounds, txrColor.Bounds, Color.White, nLineRot, vTxrRotPt, SpriteEffects.None, 0);
				} else { //First pass sets the start point of thefirst line
					bHaveStart = true;
				}

				//Curent point becomes start of next point
				vStartPt = vCurrPt;
			}

			return;
		}

		public static void DrawRectangle(GraphicsDevice gdDevice, SpriteBatch dbDraw, Color clrColor, Int32 nWidth, Rectangle rectBox) {
			List<Vector2> avPoints = new List<Vector2>();

			avPoints.Add(new Vector2(rectBox.X, rectBox.Y));
			avPoints.Add(new Vector2(rectBox.X + rectBox.Width, rectBox.Y));
			avPoints.Add(new Vector2(rectBox.X + rectBox.Width, rectBox.Y + rectBox.Height));
			avPoints.Add(new Vector2(rectBox.X, rectBox.Y + rectBox.Height));
			avPoints.Add(new Vector2(rectBox.X, rectBox.Y));

			DrawLineSeries(gdDevice, dbDraw, clrColor, nWidth, avPoints);

			return;
		}
	}
}
