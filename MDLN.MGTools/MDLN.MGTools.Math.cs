using Microsoft.Xna.Framework;

using System;

namespace MDLN.MGTools {
	public static class MGMath {
		public static Vector2 CalculateXYMagnitude(float Angle, float Magnitude) {
			Vector2 Components;

			Components.X = (float)(Math.Cos(Angle) * Magnitude);
			Components.Y = (float)(Math.Sin(Angle) * Magnitude);

			return Components;
		}
	}
}

