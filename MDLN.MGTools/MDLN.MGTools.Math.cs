using Microsoft.Xna.Framework;using System;

namespace MDLN.MGTools {
	/// <summary>
	/// Functions for common mathmatecal calculations that are useful in mono game projects
	/// </summary>
	public static class MGMath { 
		/// <summary>
		/// Calculates the X and Y components of a line that has an angle and magnitude.
		/// </summary>
		/// <returns>The X and Y component magnitudes in a Vector2 object.</returns>
		/// <param name="Angle">Angle the line segment is at.</param>
		/// <param name="Magnitude">Magnitude or length of the line segment.</param>
		public static Vector2 CalculateXYMagnitude(float Angle, float Magnitude) {
			Vector2 Components;

			Components.X = (float)(Math.Cos(Angle) * Magnitude);
			Components.Y = (float)(Math.Sin(Angle) * Magnitude);

			return Components;
		}

		/// <summary>
		/// Calculates the distance between two points squared.  
		/// Caclulating square roots costs more than squaring a value.  To get the distance between two points you need
		/// to calculate a square root, instead of doing that it's more efficient to square the distance you are comparing 
		/// to.  That's why this function gives the squared distance, simple Math.Sqrt() the result to get the true distance
		/// </summary>
		/// <returns>The squared distance between the two points.</returns>
		/// <param name="Point1">Point1.</param>
		/// <param name="Point2">Point2.</param>
		public static float SquaredDistanceBetweenPoints(Vector2 Point1, Vector2 Point2) {
			//Distance between 2 points = SquareRoot ( (X1 - X2)^2 + (Y1-Y2)^2 )
			return ((Point1.X - Point2.X) * (Point1.X - Point2.X)) + ((Point1.Y - Point2.Y) * (Point1.Y - Point2.Y));
		}

		/// <summary>
		/// Tests to see if to circles defined by an origin/center point and radius intersect
		/// </summary>
		/// <returns><c>true</c>, if circle intersection exists, <c>false</c> otherwise.</returns>
		/// <param name="Circle1Origin">Circle 1 origin.</param>
		/// <param name="Circle1Radius">Circle 1 radius.</param>
		/// <param name="Circle2Origin">Circle 2 origin.</param>
		/// <param name="Circle2Radius">Circle 2 radius.</param>
		public static bool TestCircleCollisions(Vector2 Circle1Origin, float Circle1Radius, Vector2 Circle2Origin, float Circle2Radius) {
			if (SquaredDistanceBetweenPoints(Circle1Origin, Circle2Origin) <= (Circle1Radius + Circle2Radius) * (Circle1Radius + Circle2Radius)) {
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Test to see if two rectangles defined by an origin and offsets to the rectangular 
		/// boundaries.
		/// </summary>
		/// <param name="Rect1Origin">Origin point of the first rectangle</param>
		/// <param name="Rect1Bounds">Offsets from the origin to the boundaries of the first rectangle</param>
		/// <param name="Rect2Origin">Origin point of the second rectangle</param>
		/// <param name="Rect2Bounds">Offsets from the origin to the boundaries of the second rectangle</param>
		/// <returns></returns>
		public static bool TestRectangleCollisions(Vector2 Rect1Origin, Rectangle Rect1Bounds, Vector2 Rect2Origin, Rectangle Rect2Bounds) {
			if (Rect1Origin.X + Rect1Bounds.Left < Rect2Origin.X + Rect2Bounds.Left) {
				return false;
			}

			if (Rect1Origin.X + Rect1Bounds.Left + Rect1Bounds.Width > Rect2Origin.X + Rect2Bounds.Left + Rect2Bounds.Width) {
				return false;
			}

			if (Rect1Origin.Y + Rect1Bounds.Top < Rect2Origin.Y + Rect2Bounds.Top) {
				return false;
			}

			if (Rect1Origin.Y + Rect1Bounds.Top + Rect1Bounds.Height > Rect2Origin.Y + Rect2Bounds.Top + Rect2Bounds.Height) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Test to see if a rectangular and circle collide.
		/// </summary>
		/// <param name="RectOrigin">Origin of the rectangle</param>
		/// <param name="RectBounds">Offsets from the origin to all sides of the rectangle</param>
		/// <param name="CircleOrigin">Origin of the circle</param>
		/// <param name="CircleRadius">Radius of the circle</param>
		/// <returns></returns>
		public static bool TestCircleRectangleCollision(Vector2 RectOrigin, Rectangle RectBounds, Vector2 CircleOrigin, float CircleRadius) {
			Vector2 CornerPt;

			//See if circle overlaps any of the rectangle sides
			if ((RectOrigin.X + RectBounds.Left <= CircleOrigin.X) && (RectOrigin.X + RectBounds.Left + RectBounds.Width >= CircleOrigin.X)) {
				if ((RectOrigin.Y + RectBounds.Top <= CircleOrigin.Y + CircleRadius) && (RectOrigin.Y + RectBounds.Top + RectBounds.Height >= CircleOrigin.Y - CircleRadius)) {
					return true;
				}
			}

			if ((RectOrigin.Y + RectBounds.Top <= CircleOrigin.Y) && (RectOrigin.Y + RectBounds.Top + RectBounds.Height >= CircleOrigin.Y)) {
				if ((RectOrigin.X + RectBounds.Left <= CircleOrigin.X + CircleRadius) && (RectOrigin.X + RectBounds.Left + RectBounds.Width >= CircleOrigin.X - CircleRadius)) {
					return true;
				}
			}

			//See if the circle overlaps any of the rectangle corners
			CornerPt.X = RectOrigin.X + RectBounds.Left;
			CornerPt.Y = RectOrigin.Y + RectBounds.Top;
			if (SquaredDistanceBetweenPoints(CircleOrigin, CornerPt) < CircleRadius * CircleRadius) {
				return true;
			}

			CornerPt.X = RectOrigin.X + RectBounds.Left;
			CornerPt.Y = RectOrigin.Y + RectBounds.Top + RectBounds.Height;
			if (SquaredDistanceBetweenPoints(CircleOrigin, CornerPt) < CircleRadius * CircleRadius) {
				return true;
			}

			CornerPt.X = RectOrigin.X + RectBounds.Left + RectBounds.Width;
			CornerPt.Y = RectOrigin.Y + RectBounds.Top;
			if (SquaredDistanceBetweenPoints(CircleOrigin, CornerPt) < CircleRadius * CircleRadius) {
				return true;
			}

			CornerPt.X = RectOrigin.X + RectBounds.Left + RectBounds.Width;
			CornerPt.Y = RectOrigin.Y + RectBounds.Top + RectBounds.Height;
			if (SquaredDistanceBetweenPoints(CircleOrigin, CornerPt) < CircleRadius * CircleRadius) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// Calculates the angle from the X Axis going clockwise of a line segment defined by two points.
		/// </summary>
		/// <returns>The angle from points.</returns>
		/// <param name="PointOrigin">Origin point, defines the vertex of the angle a horizontal line is projected as one of the sides.</param>
		/// <param name="PointOffset">Offset point, defines the line composing the other side of the angle.</param>
		public static float GetAngleFromPoints(Vector2 PointOrigin, Vector2 PointOffset) {
			return GetAngleFromPoints(PointOrigin, PointOffset, false);
		}

		/// <summary>
		/// Calculates the angle from the X Axis going clockwise of a line segment defined by two points.
		/// </summary>
		/// <returns>The angle from points.</returns>
		/// <param name="PointOrigin">Origin point, defines the vertex of the angle a horizontal line is projected as one of the sides.</param>
		/// <param name="PointOffset">Offset point, defines the line composing the other side of the angle.</param>
		/// <param name="InvertYAxis">If set to <c>true</c> to invert Y axis, matches the results with screen coordinates.</param>
		public static float GetAngleFromPoints(Vector2 PointOrigin, Vector2 PointOffset, bool InvertYAxis) {
			float LenX, LenY;

			LenX = PointOffset.X - PointOrigin.X;

			if (InvertYAxis == true) {
				LenY = PointOrigin.Y - PointOffset.Y;
			} else {
				LenY = PointOffset.Y - PointOrigin.Y;
			}

			if ((PointOffset.X >= PointOrigin.X) && (PointOffset.Y >= PointOrigin.Y)) { //Top right quadrant
				return (float)Math.Atan(LenY / LenX);
			} else if ((PointOffset.X < PointOrigin.X) && (PointOffset.Y >= PointOrigin.Y)) { //Top left quadrant
				return (float)(Math.PI + Math.Atan(LenY / LenX));
			} else if ((PointOffset.X < PointOrigin.X) && (PointOffset.Y < PointOrigin.Y)) { //Bottom left quadrant
				return (float)(Math.Atan(LenY / LenX) + Math.PI);
			} else { //Bottom right quadrant
				return (float)((Math.PI) + Math.Atan(LenY / LenX) + Math.PI);
			}
		}

		/// <summary>
		/// Determines if a point is contained within a rectangular region
		/// </summary>
		/// <param name="Coord">Point to test</param>
		/// <param name="Rect">Rectangular region to test within.</param>
		/// <returns>True if the point is in the region, fasle if it is not.</returns>
		public static bool IsPointInRect(Point Coord, Rectangle Rect) {
			if (Coord.X < Rect.X) {
				return false;
			}

			if (Coord.Y < Rect.Y) {
				return false;
			}

			if (Coord.X > Rect.X + Rect.Width) {
				return false;
			}

			if (Coord.Y > Rect.Y + Rect.Height) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Determines if a point is inside or on the boundary of a circle
		/// </summary>
		/// <param name="Coord">X and Y coordinates of the point to test</param>
		/// <param name="CircleOrigin">Center point of the circle</param>
		/// <param name="CircleRadius">Radius of the circle</param>
		/// <returns>True if the point is within the circle, false if it is not</returns>
		public static bool IsPointInCircle(Point Coord, Vector2 CircleOrigin, float CircleRadius) {
			Vector2 Pt;

			CircleRadius *= CircleRadius;
			Pt.X = Coord.X;
			Pt.Y = Coord.Y;

			float PtDist = SquaredDistanceBetweenPoints(Pt, CircleOrigin);

			if (PtDist <= CircleRadius) {
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Determines if a point is inside or on the boundary of a circle
		/// </summary>
		/// <param name="Coord">X and Y coordinates of the point to test</param>
		/// <param name="CircleOrigin">Center point of the circle</param>
		/// <param name="CircleRadius">Radius of the circle</param>
		/// <returns>True if the point is within the circle, false if it is not</returns>
		public static bool IsPointInCircle(Vector2 Coord, Vector2 CircleOrigin, float CircleRadius) {
			float PtDist = SquaredDistanceBetweenPoints(Coord, CircleOrigin);

			CircleRadius *= CircleRadius;

			if (PtDist <= CircleRadius) {
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Finds the center point of a line segment
		/// </summary>
		/// <param name="Pt1">X and Y coordinates of the first end point</param>
		/// <param name="Pt2">X and Y coordinates of the second end point</param>
		/// <returns>X and Y coordinates of the center point</returns>
		public static Vector2 FindLineMidPoint(Vector2 Pt1, Vector2 Pt2) {
			Vector2 Mid;

			Mid.X = (Pt1.X + Pt2.X) / 2;
			Mid.Y = (Pt1.Y + Pt2.Y) / 2;

			return Mid;
		}
	}
}

