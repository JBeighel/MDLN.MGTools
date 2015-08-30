using Microsoft.Xna.Framework;

using System;

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
	}
}

