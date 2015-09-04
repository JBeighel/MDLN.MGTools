using MDLN.MGTools;

using Microsoft.Xna.Framework;

using System;

namespace MDLN.AsteroidShooter {
	public class HunterShip : Particle2D {
		private Random cRand;
		private int cDirection;

		public HunterShip() {
			cRand = new Random(DateTime.Now.Millisecond);
		}

		public IVisible TargetObject { get; set; }

		public float MaxDistanceFromTarget { get; set; }
		public float MinDistanceFromTarget { get; set; }

		public override void Update(GameTime CurrTime) {
			Vector2 MyCenter, TargetCenter, Speed;
			float Distance;

			MyCenter.X = TopLeft.X + (Width / 2);
			MyCenter.Y = TopLeft.Y + (Height / 2);

			TargetCenter = TargetObject.GetCenterCoordinates();

			Rotation = MGMath.GetAngleFromPoints(MyCenter, TargetCenter);

			Distance = MGMath.SquaredDistanceBetweenPoints(MyCenter, TargetCenter);

			if (Distance >= MaxDistanceFromTarget * MaxDistanceFromTarget) { //Too far away get closer
				Speed = MGMath.CalculateXYMagnitude(Rotation, 2);

				TopLeft.X += Speed.X;
				TopLeft.Y -= Speed.Y;

				cDirection = 0;
			} if (Distance <= MinDistanceFromTarget * MinDistanceFromTarget) { //Too close back away
				Speed = MGMath.CalculateXYMagnitude(Rotation + 3.14f, 2);

				TopLeft.X += Speed.X;
				TopLeft.Y -= Speed.Y;
			} else { //Close enough, circle
				if (cDirection == 0) {
					if (cRand.Next(1, 3) == 1) {
						cDirection = -1;
					} else {
						cDirection = 1;
					}
				}

				Speed = MGMath.CalculateXYMagnitude(Rotation + 1.57f, 2);

				TopLeft.X += Speed.X * cDirection;
				TopLeft.Y -= Speed.Y * cDirection;
			}
		}
	}
}
	