using MDLN.Tools;
using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	/// <summary>
	/// Set of functions to handle some basic drawing methods
	/// </summary>
	public static class DrawTools {
		/// <summary>
		/// Draw a line between two points
		/// </summary>
		/// <param name="gdDevice">Graphics device that will do the drawing</param>
		/// <param name="dbDraw">SpriteBatch object to draw through</param>
		/// <param name="clrColor">Color to make the line</param>
		/// <param name="nWidth">Width of the line in pixels</param>
		/// <param name="vStart">Starting point of the line</param>
		/// <param name="vEnd">Ending point of the line</param>
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

		/// <summary>
		/// Draw a series of line segments from a collection of points.
		/// The first element in the collection will be the start of the first
		/// line segment.  The second point in the collection will end the 
		/// first line segment and be the start of the next line segment.  Each
		/// subsequent point will also end the previous line segment and start
		/// the next line segment.
		/// </summary>
		/// <param name="gdDevice">Graphics device that will do the drawing</param>
		/// <param name="dbDraw">SpriteBatch object to draw through</param>
		/// <param name="clrColor">Color to make the line</param>
		/// <param name="nWidth">Width of the line in pixels</param>
		/// <param name="avLinePts">Collection of points to use as line endpoitns</param>
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

		/// <summary>
		/// Draw a rectangle outline
		/// </summary>
		/// <param name="gdDevice">Graphics device that will do the drawing</param>
		/// <param name="dbDraw">SpriteBatch object to draw through</param>
		/// <param name="clrColor">Color to make the line</param>
		/// <param name="nWidth">Width of the line in pixels</param>
		/// <param name="rectBox">Structure describing the vertex points of the
		/// rectangle</param>
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
