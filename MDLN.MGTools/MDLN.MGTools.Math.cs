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
		public static bool TestCircleCollision(Vector2 Circle1Origin, float Circle1Radius, Vector2 Circle2Origin, float Circle2Radius) {
			if (SquaredDistanceBetweenPoints(Circle1Origin, Circle2Origin) <= (Circle1Radius + Circle2Radius) * (Circle1Radius + Circle2Radius)) {
				return true;
			} else {
				return false;
			}
		}

		public static float GetAngleFromPoints(Vector2 PointOrigin, Vector2 PointOffset) {
			return GetAngleFromPoints(PointOrigin, PointOffset, false);
		}

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
	}
}

