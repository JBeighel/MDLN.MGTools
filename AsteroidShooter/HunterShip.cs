using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

namespace MDLN.AsteroidShooter {
	public class HunterShip : Particle2D {
		private readonly Random cRand;
		private int cStrafeDirection;
		private double cLastShot; 

		public HunterShip(GraphicsDevice GraphDev) : base(GraphDev) {
			cRand = new Random(DateTime.Now.Millisecond);
			cStrafeDirection = 0;
		}

		public IVisible TargetObject { get; set; }

		public float MaxDistanceFromTarget { get; set; }
		public float MinDistanceFromTarget { get; set; }
		public ParticleEngine2D BulletManager { get; set; }
		public Texture2D BulletTexture { get; set; }

		public override bool Update(GameTime CurrTime) {
			Vector2 MyCenter, TargetCenter, Speed;
			float Distance;

			MyCenter.X = TopLeft.X + (Width / 2);
			MyCenter.Y = TopLeft.Y + (Height / 2);

			TargetCenter = TargetObject.GetCenterCoordinates();

			Rotation = MGMath.GetAngleFromPoints(MyCenter, TargetCenter, true);

			Distance = MGMath.SquaredDistanceBetweenPoints(MyCenter, TargetCenter);

			if ((Distance >= MaxDistanceFromTarget * MaxDistanceFromTarget) && (cStrafeDirection != 0)) { //Too far away get closer
				Speed = MGMath.CalculateXYMagnitude(Rotation, 3);
				cStrafeDirection = 0;
			} else if ((Distance >= (MaxDistanceFromTarget * MaxDistanceFromTarget * 0.8f * 0.8f)) && (cStrafeDirection == 0)) { //If moving forward overshoot a bit
				Speed = MGMath.CalculateXYMagnitude(Rotation, 3);
			} else if (Distance <= MinDistanceFromTarget * MinDistanceFromTarget) { //Too close back away
				Speed = MGMath.CalculateXYMagnitude(Rotation + 3.14f, 3);
				cStrafeDirection = 0;
			} else if ((Distance <= MinDistanceFromTarget * MinDistanceFromTarget * 1.2f * 1.2f) && (cStrafeDirection == 0)) { //If backing away overshoot a bit
				Speed = MGMath.CalculateXYMagnitude(Rotation + 3.14f, 3);
			} else { //Close enough, circle
				if (cStrafeDirection == 0) {
					if (cRand.Next(1, 3) == 1) {
						cStrafeDirection = -1;
					} else {
						cStrafeDirection = 1;
					}
				}

				Speed = MGMath.CalculateXYMagnitude(Rotation + 1.57f, 2);

				Speed.X *= cStrafeDirection;
				Speed.Y *= cStrafeDirection;
			}

			SpeedX = (SpeedX + Speed.X) / 2;
			SpeedY = (SpeedY + Speed.Y) / 2;

			TopLeft.X += SpeedX;
			TopLeft.Y -= SpeedY;

			if (cLastShot == 0) {
				cLastShot = CurrTime.TotalGameTime.TotalMilliseconds + 1500 + cRand.Next(1000);
			}

			if (cLastShot < CurrTime.TotalGameTime.TotalMilliseconds) {
				Vector2 BulletOrigin;

				BulletOrigin.Y = TopLeft.Y + (Height / 2);
				BulletOrigin.X = TopLeft.X + (Height / 2);

				Rotation += (float)(cRand.Next(-10, 10) * (Math.PI / 180.0)); //Randmize the direction so that it's not perfect (+/- 10 degrees)
				BulletManager.AddParticle(BulletTexture, BulletOrigin.Y - 10, BulletOrigin.X - 10, 20, 20, Rotation, 8, new Color(255, 75, 75, 255));

				cLastShot = CurrTime.TotalGameTime.TotalMilliseconds + 1500 + cRand.Next(1000);
			}

			return true;
		}
	}
}
	