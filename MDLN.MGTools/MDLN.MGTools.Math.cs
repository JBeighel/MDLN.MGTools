using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

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

		/// <summary>
		/// The determinant of a triangle is equal to 2 * its area, its this matrix math
		///              | X1, Y1, 1 |
		/// Area = 1/2 * | X2, Y2, 1 |
		///              | X3, Y3, 1 |
		/// The result of this may be negative depending on the order of the points, an 
		/// absolute value will give the area.  Counter-clockwise for positive and clockwise
		/// giving negative determinants.
		/// 
		/// If a point is inside the triangle then its determinant with any combination of
		/// two vertexes should all result in determinants with the same sign.  We can use the
		/// sign of the determinant to tell if a point is inside or outside of a triangle, or
		/// get its area.
		/// </summary>
		/// <param name="Pt1">First vertex</param>
		/// <param name="Pt2">Second vertex</param>
		/// <param name="Pt3">Third vertex</param>
		/// <returns>Determinant of the tringle whose vertexes are the given points</returns>
		public static float GetTriangleDeterminant(Vector2 Pt1, Vector2 Pt2, Vector2 Pt3) {
			return (Pt1.X - Pt3.X) * (Pt2.Y - Pt3.Y) - (Pt2.X - Pt3.X) * (Pt1.Y - Pt3.Y);
		}

		/// <summary>
		/// Checks if a point is inside of a specified triangle
		/// </summary>
		/// <param name="Pt">Point to test</param>
		/// <param name="Vert1">First vertex of the triangle</param>
		/// <param name="Vert2">Second vertex of the triangle</param>
		/// <param name="Vert3">Third vertex of the triangle</param>
		/// <returns>True if the point is inside the triangle, false otherwise</returns>
		public static bool PointInTriangle(Vector2 Pt, Vector2 Vert1, Vector2 Vert2, Vector2 Vert3) {
			float nDet1, nDet2, nDet3;

			nDet1 = GetTriangleDeterminant(Pt, Vert2, Vert3);

			nDet2 = GetTriangleDeterminant(Vert1, Pt, Vert3);

			if (((nDet1 < 0) && (nDet2 > 0)) || ((nDet1 > 0) && (nDet2 < 0))) {
				//Mismatched signs mean point is outside the triangle
				return false;
			}

			nDet1 += nDet2; //In case 1 or 2 is zero make sure the sign isn't lost
							//The signs of the first 2 determinants match, so we only need to check against 1 of them
			nDet3 = GetTriangleDeterminant(Vert1, Vert2, Pt);
			if (((nDet1 < 0) && (nDet3 > 0)) || ((nDet1 > 0) && (nDet3 < 0))) {
				//Mismatched signs mean point is outside the triangle
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if a given point is inside of a convex polygon
		/// </summary>
		/// <param name="Pt">Point to test</param>
		/// <param name="aPolygonVertexes">List of all vertexes of the convex polygon</param>
		/// <returns>True if the point is inside the polygon, false otherwise</returns>
		public static bool PointInConvexPolygon(Vector2 Pt, IEnumerable<Vector2> aPolygonVertexes) {
			List<Vector2> aVerts = new List<Vector2>(aPolygonVertexes);
			Vector2 CmnVert = aVerts[0];
			int nCtr;
			bool bInTriangle;

			if (aVerts.Count < 3) { //Not really a polygon
				return false;
			}

			//Check our point against a triangle made by a common point and every other 
			//pair of points in the polygon.  These combinations only work of the polygon
			//is entirely convex.
			for (nCtr = 2; nCtr < aVerts.Count; nCtr++) {
				bInTriangle = PointInTriangle(Pt, CmnVert, aVerts[nCtr - 1], aVerts[nCtr]);

				if (bInTriangle == true) { //If its in any of the triangles, its in the polygon
					return true;
				}
			}

			//The point wasn't in any of the triangles, outside the polygon
			return false;
		}

		/// <summary>
		/// Checks if two line segments intersect between the endpoints provided
		/// </summary>
		/// <param name="Line1End1">First endpoint of line 1</param>
		/// <param name="Line1End2">Second end point of line 1</param>
		/// <param name="Line2End1">First endpoint of line 2</param>
		/// <param name="Line2End2">Second endpoint of line 2</param>
		/// <returns>True if the lines intesect within the endpoints provided, false if not</returns>
		public static bool LineSegmentIntesection(Vector2 Line1End1, Vector2 Line1End2, Vector2 Line2End1, Vector2 Line2End2) {
			float Line1M, Line1B, Line2M, Line2B;
			Vector2 vIntersect;

			//Start with general equation for bothe lines, aY = mX + b
			//Calculate line slopes
			if (Line1End1.X == Line1End2.X) {
				//Line is vertical, infinite slope
				Line1M = float.MaxValue;
			} else {
				Line1M = (Line1End1.Y - Line1End2.Y) / (Line1End1.X - Line1End2.X);
			}

			if (Line2End1.X == Line2End2.X) {
				//Line is vertical, infinite slope
				Line2M = float.MaxValue;
			} else {
				Line2M = (Line2End1.Y - Line2End2.Y) / (Line2End1.X - Line2End2.X);
			}

			if (Line1M == Line2M) {
				//Lines are parallel and can't instersect
				return false;
			}

			//Calculate Y intercepts
			Line1B = Line1End1.Y - (Line1M * Line1End1.X);
			Line2B = Line2End1.Y - (Line2M * Line2End1.X);

			if (Line1M == float.MaxValue) {
				//Veritical Line 1 handling
				//look for Y value in line 2 at line 1's X
				vIntersect.X = Line1End1.X;
				vIntersect.Y = (Line2M * vIntersect.X) + Line2B;
			} else if (Line2M == float.MaxValue) {
				//Veritical Line 2 handling
				//look for Y value in line 1 at line 2's X
				vIntersect.X = Line2End1.X;
				vIntersect.Y = (Line1M * vIntersect.X) + Line1B;
			} else {
				//Find intersection X value
				vIntersect.X = Line2B - Line1B;
				vIntersect.X /= Line1M - Line2M;

				//Find intersection Y value
				//Sub X into either equation
				vIntersect.Y = (Line1M * vIntersect.X) + Line1B;
			}

			//See if the X coordinate is inside both line segments
			if (Line1End1.X < Line1End2.X) {
				if ((Line1End1.X > vIntersect.X) || (vIntersect.X > Line1End2.X)) {
					return false;
				}
			} else {
				if ((Line1End2.X > vIntersect.X) || (vIntersect.X > Line1End1.X)) {
					return false;
				}
			}

			if (Line2End1.X < Line2End2.X) {
				if ((Line2End1.X > vIntersect.X) || (vIntersect.X > Line2End2.X)) {
					return false;
				}
			} else {
				if ((Line2End2.X > vIntersect.X) || (vIntersect.X > Line2End1.X)) {
					return false;
				}
			}

			//See if the Y coordinate is inside both line segments
			if (Line1End1.Y < Line1End2.Y) {
				if ((Line1End1.Y > vIntersect.Y) || (vIntersect.Y > Line1End2.Y)) {
					return false;
				}
			} else {
				if ((Line1End2.Y > vIntersect.Y) || (vIntersect.Y > Line1End1.Y)) {
					return false;
				}
			}

			if (Line2End1.Y < Line2End2.Y) {
				if ((Line2End1.Y > vIntersect.Y) || (vIntersect.Y > Line2End2.Y)) {
					return false;
				}
			} else {
				if ((Line2End2.Y > vIntersect.Y) || (vIntersect.Y > Line2End1.Y)) {
					return false;
				}
			}

			//The line segments do intersect
			return true;
		}

		/// <summary>
		/// Calculates a set of points that will create a smooth curve.  The 
		/// start and end points will be expressly defined.  The curve shape 
		/// will be controlled by two other points.
		/// 
		/// The first control point will define the angle the curve leaves the 
		/// starting point as well as how sharp the curve begins.
		/// 
		/// The second control point will define the angle the curve approaches
		/// the ending point as well as how sharp the curve ends.
		/// </summary>
		/// <param name="vCurveStart">Starting point of the curve</param>
		/// <param name="vCurveEnd">Ending point of the curve</param>
		/// <param name="vPt1">First control point</param>
		/// <param name="vPt2">Second control point</param>
		/// <param name="nNumPoints">Number of line segments to calculate that
		/// comprise the curve</param>
		/// <param name="avCurvePts">Points defining the line segments.  Each 
		/// point will end the previous line segment and start the next line
		/// segment</param>
		/// <returns>True on success, false on any error</returns>
		public static bool CubicBezierCurvePoints(Vector2 vCurveStart, Vector2 vCurveEnd, Vector2 vPt1, Vector2 vPt2, UInt32 nNumPoints, out List<Vector2> avCurvePts) {
			//vCurveStart = X0, Y0
			//vPt1 = X1, Y1
			//vPt2 = X2, Y2
			//vCurveEnd = X3, Y3
			//X(n) = [(3n)^0 * (1 - n)^3 * X0] + [(3n)^1 * (1 - n)^2 * X1] + [(3n)^2 * (1 - n)^1 X2] + [(3n)^3 * (1 - n)^0 X3]
			//Y(n) = [(3n)^0 * (1 - n)^3 * Y0] + [(3n)^1 * (1 - n)^2 * Y1] + [(3n)^2 * (1 - n)^1 Y2] + [(3n)^3 * (1 - n)^0 Y3]
			//n must be a value from 0 to 1

			UInt32 nCtr;
			double nStepSize, nCurrStep;
			Vector2 vPoint, vOffset;

			avCurvePts = new List<Vector2>();

			vOffset = vCurveEnd;

			avCurvePts.Add(vCurveStart);//Always start at the origin
			vCurveStart -= vOffset;

			vCurveEnd -= vOffset;

			vPt1 -= vOffset;

			vPt2 -= vOffset;

			//Make the steps even, this will not evenly space the points on the curve
			nStepSize = 1 / (double)nNumPoints;

			nCurrStep = nStepSize;
			for (nCtr = 1; nCtr < nNumPoints; nCtr += 1) {
				vPoint = new Vector2(0, 0);

				//Calcualte the X coordinate
				vPoint.X = (float)Math.Pow(1 - nCurrStep, 3) * vCurveStart.X;
				vPoint.X += (float)(3 * nCurrStep * Math.Pow(1 - nCurrStep, 2) * vPt1.X);
				vPoint.X += (float)(3 * Math.Pow(nCurrStep, 2) * (1 - nCurrStep) * vPt2.X);
				vPoint.X += (float)(3 * Math.Pow(nCurrStep, 3) * vCurveEnd.X);

				//Calculate the Y coordinate
				vPoint.Y = (float)Math.Pow(1 - nCurrStep, 3) * vCurveStart.Y;
				vPoint.Y += (float)(3 * nCurrStep * Math.Pow(1 - nCurrStep, 2) * vPt1.Y);
				vPoint.Y += (float)(3 * Math.Pow(nCurrStep, 2) * (1 - nCurrStep) * vPt2.Y);
				vPoint.Y += (float)(3 * Math.Pow(nCurrStep, 3) * vCurveEnd.Y);

				//Put the point into the list
				avCurvePts.Add(vPoint + vOffset);

				//Move to the next step
				nCurrStep += nStepSize;
			}

			vCurveEnd += vOffset;
			avCurvePts.Add(vCurveEnd);

			return true;
		}

		/// <summary>
		/// Calculates a bounding box that encloses a smooth curve.  The 
		/// start and end points will be expressly defined.  The curve shape 
		/// will be controlled by two other points.
		/// 
		/// The first control point will define the angle the curve leaves the 
		/// starting point as well as how sharp the curve begins.
		/// 
		/// The second control point will define the angle the curve approaches
		/// the ending point as well as how sharp the curve ends.
		/// </summary>
		/// <param name="vCurveStart">Starting point of the curve</param>
		/// <param name="vCurveEnd">Ending point of the curve</param>
		/// <param name="vPt1">First control point</param>
		/// <param name="vPt2">Second control point</param>
		/// <param name="rectBoundary">Rectangle that contains the entire curve</param>
		/// <returns>True on success, false on any error</returns>
		public static bool CubicBezierCurveBoundaries(Vector2 vCurveStart, Vector2 vCurveEnd, Vector2 vPt1, Vector2 vPt2, out Rectangle rectBoundary) {
			//First derivitave equation
			//vCurveStart = P0
			//vPt1 = P1
			//vPt2 = P2
			//vCurveEnd = P3
			//d(Pt) = 3 * (1-n)^2 * (P1 - P0)  + 6 * (1-n)n * (P2 - P1) + 3n^2 * (P3 - P2)
			//Find values of n where the result is zero
			//Set equation to zero and solve for n
			//0 = (3 - 6n + n^2)(P1-P0) + (6n - n^2)(P2 - P1) + 3n^2(P3 - P2)
			//Algebra that to...
			//0 = (-3P0 + 9P1 - 9P2 + 3P3)n^2 + (6P0 - 12P1 + 6P2)n + (-3P0 + 3P1)
			//Into Quadratic formula: An^2 + Bn + C = 0
			//(-B +/- sqrt(B^2 - 4AC)) / 2A
			//Those inflection points and the ends will set ranges for the bounding box

			List<double> aTestPts = new List<double>();
			double nA, nB, nC, nValue;
			Vector2 vOffset;

			//Always check the endpoints of the curve
			aTestPts.Add(0);
			aTestPts.Add(1);

			//Curve equation requires the ending to be the origin
			vOffset = vCurveEnd;

			//Correct all the points so the ending is the origin
			vCurveStart -= vOffset;
			vPt1 -= vOffset;
			vPt2 -= vOffset;
			vCurveEnd = new Vector2(0, 0);

			//Find X inflection points
			nA = (-3 * vCurveStart.X) + (9 * vPt1.X) + (-9 * vPt2.X) + (3 * vCurveEnd.X);
			nB = (6 * vCurveStart.X) + (-12 * vPt1.X) + (6 * vPt2.X);
			nC = (-3 * vCurveStart.X) + (3 * vPt1.X);

			if (nA != 0) {
				nValue = ((-1 * nB) + Math.Sqrt((nB * nB) + (-4 * nA * nC))) / (2 * nA);
				if ((nValue > 0) && (nValue < 1)) {
					aTestPts.Add(nValue);
				}

				nValue = ((-1 * nB) - Math.Sqrt((nB * nB) + (-4 * nA * nC))) / (2 * nA);
				if ((nValue > 0) && (nValue < 1)) {
					aTestPts.Add(nValue);
				}
			} else {
				//X starts and ends at same point, try the middle?
				aTestPts.Add(0.5);
			}

			nA = (-3 * vCurveStart.Y) + (9 * vPt1.Y) + (-9 * vPt2.Y) + (3 * vCurveEnd.Y);
			nB = (6 * vCurveStart.Y) + (-12 * vPt1.Y) + (6 * vPt2.Y);
			nC = (-3 * vCurveStart.Y) + (3 * vPt1.Y);

			if (nA != 0) {
				nValue = ((-1 * nB) + Math.Sqrt((nB * nB) + (-4 * nA * nC))) / (2 * nA);
				if ((nValue > 0) && (nValue < 1)) {
					aTestPts.Add(nValue);
				}

				nValue = ((-1 * nB) - Math.Sqrt((nB * nB) + (-4 * nA * nC))) / (2 * nA);
				if ((nValue > 0) && (nValue < 1)) {
					aTestPts.Add(nValue);
				}
			} else {
				//Y starts and ends at same point, try the middle?
				aTestPts.Add(0.5);
			}

			//Loop through all the test points
			rectBoundary = new Rectangle(0, 0, 0, 0);

			List<double> anX = new List<double>(); ;
			List<double> anY = new List<double>();
			foreach (double nCurr in aTestPts) {
				//Calcualte the X coordinate
				nValue = Math.Pow(1 - nCurr, 3) * vCurveStart.X;
				nValue += (3 * nCurr * Math.Pow(1 - nCurr, 2) * vPt1.X);
				nValue += (3 * Math.Pow(nCurr, 2) * (1 - nCurr) * vPt2.X);
				nValue += (3 * Math.Pow(nCurr, 3) * vCurveEnd.X);

				anX.Add(nValue);

				if ((nValue < 0) && (nValue < rectBoundary.X)) { //Rectangle starts left of offset point
					rectBoundary.X = (int)(nValue - 1);
				} else if (nValue > rectBoundary.Width) {
					rectBoundary.Width = 1 + (int)nValue;
				}

				//Calculate the Y coordinate
				nValue = Math.Pow(1 - nCurr, 3) * vCurveStart.Y;
				nValue += (3 * nCurr * Math.Pow(1 - nCurr, 2) * vPt1.Y);
				nValue += (3 * Math.Pow(nCurr, 2) * (1 - nCurr) * vPt2.Y);
				nValue += (3 * Math.Pow(nCurr, 3) * vCurveEnd.Y);

				anY.Add(nValue);

				if ((nValue < 0) && (nValue < rectBoundary.Y)) { //Rectangle starts above offset point
					rectBoundary.Y = (int)(nValue - 1);
				} else if (nValue > rectBoundary.Height) {
					rectBoundary.Height = 1 + (int)nValue;
				}
			}

			//If the start isn't the offset, adjust dimensions to account for it
			rectBoundary.Width -= rectBoundary.X;
			rectBoundary.Height -= rectBoundary.Y;

			//Reposition the rectangle to the offset
			rectBoundary.X += (int)vOffset.X;
			rectBoundary.Y += (int)vOffset.Y;


			return true;
		}

		/// <summary>
		/// Applies a scale transformation to a set of 2D vertexes
		/// Maxrix math
		/// ---    ---   --- ---   ---                 ---
		/// | A1, B1 |   | A'1 |   | A1 * A'1 + B1 * A12 |
		/// | A2, B2 | * | A'2 | = | A2 * A'1 + B2 * A12 |
		/// ---    ---   --- ---   ---                 ---
		///
		/// Scale transformation matrix, Xs is X axis scale, Ys is Y axis scale
		/// ---    ---   --- ---   ---                ---   ---      ---
		/// | Xs, 0  |   | PtX |   | Xs * PtX + 0 * PtY |   | Xs * PtX |
		/// | 0,  Ys | * | PtY | = | 0 * PtX + Ys * PtY | = | Ys * PtY |
		/// ---    ---   --- ---   ---                ---   ---      ---
		/// </summary>
		/// <param name="avOriginal">Original vertex coordinates</param>
		/// <param name="vScaleCenter">Coordinate of where the scaling should be centered
		/// Equal growth/shrinking from this point</param>
		/// <param name="nScaleX">Amount to scale the shape along the X axis</param>
		/// <param name="nScaleY">Amount to scale the shape along the Y axis</param>
		/// <returns>A set of vertexes corresponding to the original set scaled
		/// around the specified center point</returns>
		public static List<Vector2> Transform2DScale(IEnumerable<Vector2> avOriginal, Vector2 vScaleCenter, float nScaleX, float nScaleY) {
			List<Vector2> avScaled = new List<Vector2>();
			Vector2 vPt;

			foreach (Vector2 vVertex in avOriginal) {
				vPt = vVertex - vScaleCenter; //Adjust coordinates so scaling is from center point

				//Apply scale factors
				vPt.X *= nScaleX;
				vPt.Y *= nScaleY;

				//Remove center adjustment
				vPt += vScaleCenter;

				avScaled.Add(vPt);
			}

			//Return the list of calculated points
			return avScaled;
		}

		public static List<Vector2> Transform2D(IEnumerable<Vector2> avOriginal, Vector2 vXFormCenter, float nScaleX, float nScaleY, float nRotCCWRadians) {
			float[,] anTransform, anVertex, anResult;
			List<Vector2> avScaled = new List<Vector2>();
			Vector2 vPt;

			anVertex = new float[2, 1];
			anTransform = new float[2, 2];

			//Create the transformation matrix
			anTransform[0, 0] = nScaleX * (float)Math.Cos(nRotCCWRadians);
			anTransform[0, 1] = nScaleY * -1 * (float)Math.Sin(nRotCCWRadians);
			anTransform[1, 0] = nScaleX * (float)Math.Sin(nRotCCWRadians);
			anTransform[1, 1] = nScaleY * (float)Math.Cos(nRotCCWRadians);

			foreach (Vector2 vVertex in avOriginal) {
				//Adjust coordinates so scaling is from center point
				anVertex[0, 0] = vVertex.X - vXFormCenter.X;
				anVertex[1, 0] = vVertex.Y - vXFormCenter.Y;

				//Apply transformation
				anResult = MatrixMult(anTransform, anVertex);

				//Remove center adjustment
				vPt.X = anResult[0, 0] + vXFormCenter.X;
				vPt.Y = anResult[1, 0] + vXFormCenter.Y;

				avScaled.Add(vPt);
			}

			//Return the list of calculated points
			return avScaled;
		}

		/// <summary>
		/// Multiplies two matrices together and returns the result.
		/// 
		/// THe first index of the multidmensional array is the Row.  The 
		/// second index is the column.
		/// { { 00, 01, 02 },
		///   { 10, 11, 12 },
		///   { 20, 21, 22 } }
		/// </summary>
		/// <param name="anMatrix1">First matrix to multiply</param>
		/// <param name="anMatrix2">Second matrix to multiply</param>
		/// <returns>Matrix result of the multiplication</returns>
		public static float[,] MatrixMult (float[,] anMatrix1, float[,] anMatrix2) {
			float[,] anResult;
			float nValue;
			Int32 nRowCtr, nColCtr, nCalcCtr;

			//Ensure the matrix sizes are compatible
			if ((anMatrix1.GetLength(0) == 0) || (anMatrix1.GetLength(1) == 0)) {
				throw new Exception("Unable to multiply matrices with a zero dimension");
			} else if ((anMatrix2.GetLength(0) == 0) || (anMatrix2.GetLength(1) == 0)) {
				throw new Exception("Unable to multiply matrices with a zero dimension");
			}

			if (anMatrix1.GetLength(1) != anMatrix2.GetLength(0)) {
				throw new Exception("Unable to multiply matrices where first matrix columns (second index) doesn't match second matrix rows (first index)");
			}

			//Set the size of the results array
			anResult = new float[anMatrix1.GetLength(0), anMatrix2.GetLength(1)];

			//Loop through all rows in the resulting matrix
			for (nRowCtr = 0; nRowCtr < anMatrix1.GetLength(0); nRowCtr += 1) {
				//Loop through all columns in th eresulting matrix
				for (nColCtr = 0; nColCtr < anMatrix2.GetLength(1); nColCtr += 1) {
					nValue = 0;

					//Loop through all calculations needed for the cell
					for (nCalcCtr = 0; nCalcCtr < anMatrix1.GetLength(1); nCalcCtr += 1) {
						//Multiply every value in matrix 1 row with matrix 2 column, sum the products
						nValue += anMatrix1[nRowCtr, nCalcCtr] * anMatrix2[nCalcCtr, nColCtr];
					}

					//Put the calculated value into the results array
					anResult[nRowCtr, nColCtr] = nValue;
				}
			}

			return anResult;
		}
	}
}

