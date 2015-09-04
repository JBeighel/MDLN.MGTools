using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MDLN.AsteroidShooter {
	class MainClass {
		public static void Main(string[] args) {
			using (var game = new AsteroidShooter())
			{
				game.Run();
			}
		}
	}

	public class AsteroidShooter : Game {
		private GraphicsDeviceManager cGraphDevMgr;
		private Ship cPlayerShip;
		private ParticleEngine2D cPlayerBullets;
		private ParticleEngine2D cAsteroids;
		private double cLastShot, cLastAsteroid;
		private MDLN.MGTools.GameConsole cDevConsole;
		private KeyboardState cPriorKeyState;
		private MouseState cPriorMouseState;
		private Dictionary<Textures, Texture2D> cTextureDict;
		private Random cRandom;
		private uint cAsteroidKills, cAliveSince;

		public AsteroidShooter() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			cTextureDict = new Dictionary<Textures, Texture2D>();

			cRandom = new Random(DateTime.Now.Millisecond);

			cAsteroidKills = 0;
			cAliveSince = 0;
		}

		protected override void Initialize() {

			//Initializes monogame
			base.Initialize();
		}

		protected override void UnloadContent() {
		}

		protected override void LoadContent() {
			foreach (Textures CurrTexture in Enum.GetValues(typeof(Textures))) {
				cTextureDict.Add(CurrTexture, Content.Load<Texture2D>(Tools.Tools.GetEnumDescriptionAttribute(CurrTexture)));
			}

			//cFont = new TextureFont(cTextureDict[Textures.Font]);
			cDevConsole = new MDLN.MGTools.GameConsole(cGraphDevMgr.GraphicsDevice, Content, "Font.png", 0, 0, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2);
			cDevConsole.CommandSent += CommandSentEventHandler;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;

			cPlayerShip = new Ship(cGraphDevMgr.GraphicsDevice, 50);
			cPlayerShip.BackgroundColor = new Color(100, 100, 100, 0); //Set background completely transparent
			cPlayerShip.ShipTexture = cTextureDict[Textures.Ship];
			cPlayerShip.Visible = true;
			cPlayerShip.Top = cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2;
			cPlayerShip.Left = cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width / 2;
			//cPlayerShip.ImageInitialAngle = 1.570796f; //Offset for image pointing up instead of right
			cPlayerShip.MouseRotate = true;

			cPlayerBullets = new ParticleEngine2D(cGraphDevMgr.GraphicsDevice);

			cAsteroids = new ParticleEngine2D(cGraphDevMgr.GraphicsDevice);
			cAsteroids.WrapScreenEdges = true;
		}

		protected override void Update(GameTime gameTime) {
			KeyboardState CurrKeys = Keyboard.GetState();
			MouseState CurrMouse = Mouse.GetState();
			Vector2 BulletOrigin;
			Particle2D BulletInfo, AstInfo;

			if (cAliveSince == 0) {
				cAliveSince = (uint)gameTime.TotalGameTime.TotalSeconds;
			}

			if (cLastAsteroid < gameTime.TotalGameTime.TotalMilliseconds) { //Create new asteroid
				if (cLastAsteroid % 3 == 0) {
					CreateNewAsteroid(100, new Vector2(-1, -1));	
				} else {
					CreateNewHunter(60, new Vector2(-1, -1));
				}

				cLastAsteroid = gameTime.TotalGameTime.TotalMilliseconds + 3000;
			}

			//Check for player input
			if (((CurrKeys.IsKeyDown(Keys.Space) == true) || (CurrMouse.LeftButton == ButtonState.Pressed)) && (cLastShot < gameTime.TotalGameTime.TotalMilliseconds - 250)) {
				//Calculate coordinates of ship tip relative to its center
				BulletOrigin = MDLN.MGTools.MGMath.CalculateXYMagnitude(-1 * cPlayerShip.cRotation, cPlayerShip.Width / 4);

				//Adjust it so that it's relative to the top left screen corner
				BulletOrigin.Y += cPlayerShip.Top + (cPlayerShip.Height / 2);
				BulletOrigin.X += cPlayerShip.Left + (cPlayerShip.Height / 2);

				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - 10, BulletOrigin.X - 10, 20, 20, cPlayerShip.cRotation, 15, Color.White);
				cLastShot = gameTime.TotalGameTime.TotalMilliseconds;
			}

			if ((CurrKeys.IsKeyDown(Keys.OemTilde) == true) && (cPriorKeyState.IsKeyDown(Keys.OemTilde) == false)) {
				cDevConsole.ToggleVisible();
			}

			cAsteroids.Update(gameTime);
			cPlayerBullets.Update(gameTime);
			cPlayerShip.Update(gameTime, CurrKeys, CurrMouse);
			cDevConsole.Update(gameTime, CurrKeys, CurrMouse);

			//Collision detection
			cPlayerShip.ImageTint = Color.White;
			for (int Cnt = 0; Cnt < cAsteroids.ParticleList.Count; Cnt++) {
				AstInfo = cAsteroids.ParticleList[Cnt];

				//Are bullts hitting the asteroid?
				for (int Ctr = 0; Ctr < cPlayerBullets.ParticleList.Count; Ctr++) {
					BulletInfo = cPlayerBullets.ParticleList[Ctr];

					if (BulletInfo.TestCollision(AstInfo.GetCollisionRegions()) == true) {
						//Spawn little asteroids
						if (AstInfo.Height > 50) {
							Vector2 TopLeft;

							TopLeft.X = AstInfo.TopLeft.X + (AstInfo.Width / 2) - (AstInfo.Width * 0.35f);
							TopLeft.Y = AstInfo.TopLeft.Y + (AstInfo.Height / 2) - (AstInfo.Height * 0.35f);
								
							if (AstInfo.SplitOnDeath == true) { //Only split asteroids, not UFOs
								CreateNewAsteroid((int)(AstInfo.Width * 0.7f), TopLeft);
								CreateNewAsteroid((int)(AstInfo.Width * 0.7f), TopLeft);
							}
						}

						//Destroy shot and large asteroid
						cPlayerBullets.ParticleList.RemoveAt(Ctr);
						cAsteroids.ParticleList.RemoveAt(Cnt);
						cAsteroidKills++;

						break; //Exit inner loop so each bullet ony gets 1 asteroid
					}
				}

				//Is the asteroid hitting the player?
				if (AstInfo.TestCollision(cPlayerShip) == true) {
					cPlayerShip.ImageTint = Color.Red;
					if ((gameTime.TotalGameTime.TotalSeconds - cAliveSince >= 1) && (cAliveSince != 0)) {
						cDevConsole.AddText(String.Format("Survived {0:0.0} seconds and shot {1} asteroids.", gameTime.TotalGameTime.TotalSeconds - cAliveSince, cAsteroidKills));
					}

					cAsteroidKills = 0;
					cAliveSince = 0;
				}
			}

			cPriorMouseState = CurrMouse;
			cPriorKeyState = CurrKeys;

			//Use monogame update
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

			cAsteroids.Draw();
			cPlayerBullets.Draw();
			cPlayerShip.Draw();
			cDevConsole.Draw();

			//Use monogame draw
			base.Draw(gameTime);
		}

		private void CommandSentEventHandler(object Sender, string Command) {
			if (Tools.RegEx.QuickTest(Command, "^(quit|exit)$") == true) {
				Exit();
			} else if (Tools.RegEx.QuickTest(Command, "^ship *stop$") == true) {
				cPlayerShip.cSpeedX = 0;
				cPlayerShip.cSpeedY = 0;
				cDevConsole.AddText("Ship speed set to 0");
			} else if (Tools.RegEx.QuickTest(Command, "^ship *center$") == true) {
				cPlayerShip.Top = (cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height) / 2 - (cPlayerShip.Height / 2);
				cPlayerShip.Left = (cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width) / 2 - (cPlayerShip.Width / 2);
				cDevConsole.AddText("Ship position set to [X=" + cPlayerShip.Left + ", Y=" + cPlayerShip.Width + "]");
			} else if (Tools.RegEx.QuickTest(Command, "^ship *state$") == true) {
				cDevConsole.AddText("Ship Position [X=" + cPlayerShip.Top + ", Y=" + cPlayerShip.Left + "]");
				cDevConsole.AddText("     Speed X=" + cPlayerShip.cSpeedX + " Y=" + cPlayerShip.cSpeedY);
				cDevConsole.AddText("     Size Width=" + cPlayerShip.Width + " Height=" + cPlayerShip.Height);
			} else if (Tools.RegEx.QuickTest(Command, "^fire *spread$") == true) {
				cDevConsole.AddText("Firing Spread");
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation - 0.628f, 10, Color.White);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation - 0.314f, 10, Color.White);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation, 10, Color.White);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation + 0.314f, 10, Color.White);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation + 0.628f, 10, Color.White);
			} else if (Tools.RegEx.QuickTest(Command, "^fire *multi-?(ple|shot)$") == true) {
				cDevConsole.AddText("Firing Multi-Shot");
				Vector2 BulletOrigin, BulletOffset;
				BulletOrigin = MGMath.CalculateXYMagnitude(-1 * cPlayerShip.cRotation, cPlayerShip.Width / 4);
				BulletOffset = MGMath.CalculateXYMagnitude(-1 * cPlayerShip.cRotation + 1.570796f, cPlayerShip.Width / 5);

				//Adjust it so that it's relative to the top left screen corner
				BulletOrigin.Y += cPlayerShip.Top + (cPlayerShip.Height / 2);
				BulletOrigin.X += cPlayerShip.Left + (cPlayerShip.Height / 2);

				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y + (BulletOffset.Y * 2) - 10, BulletOrigin.X + (BulletOffset.X * 2) - 10, 20, 20, cPlayerShip.cRotation, 15, Color.White);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y + BulletOffset.Y - 10, BulletOrigin.X + BulletOffset.X - 10, 20, 20, cPlayerShip.cRotation, 15, Color.White);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - 10, BulletOrigin.X - 10, 20, 20, cPlayerShip.cRotation, 15, Color.White);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - BulletOffset.Y - 10, BulletOrigin.X - BulletOffset.X - 10, 20, 20, cPlayerShip.cRotation, 15, Color.White);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - (BulletOffset.Y * 2) - 10, BulletOrigin.X - (BulletOffset.X * 2) - 10, 20, 20, cPlayerShip.cRotation, 15, Color.White);
			} else if (Tools.RegEx.QuickTest(Command, "^new *asteroid$") == true) {
				cDevConsole.AddText("Spawning Asteroid");
				cAsteroids.AddParticle(cTextureDict[Textures.Asteroid], cPlayerShip.Top + (cPlayerShip.Height / 2) - 50, cPlayerShip.Left + (cPlayerShip.Height / 2) - 50, 100, 100, cPlayerShip.cRotation - 0.628f, 2, Color.White);
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*ast(eroid)?\s*count\s*?$") == true) {
				cDevConsole.AddText("Current Asteroid Count: " + cAsteroids.ParticleList.Count);
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*ast(eroid)?\s*clear$") == true) {
				cDevConsole.AddText("Destroying all asteroids");
				cAsteroids.ParticleList.Clear();
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*mouse\s*turn\s*=\s*(on|true|enable|1)\s*$") == true) {
				cDevConsole.AddText("Using mouse position to rotate ship");
				cPlayerShip.MouseRotate = true;
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*mouse\s*turn\s*=\s*(off|false|disable|0)\s*$") == true) {
				cDevConsole.AddText("Using arrow keys to rotate ship");
				cPlayerShip.MouseRotate = false;
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*(spawn|new)\s*hunter\s*$") == true) {
				cDevConsole.AddText("Spawning new hunter UFO");
				Vector2 StartPos = new Vector2(-50, -50);
				CreateNewHunter(100, StartPos);
			}else {
				cDevConsole.AddText("Unrecognized command: " + Command);
			}
		}

		private void CreateNewAsteroid(int Size, Vector2 Position) {
			Particle2D AstInfo;
			Vector2 AstSpeed;

			AstInfo = new Particle2D();

			AstInfo.Width = Size;
			AstInfo.Height = Size;

			if ((Position.X == -1) && (Position.Y == -1)) {
				AstInfo.TopLeft.X = AstInfo.Width * cRandom.Next(-1, 1);
				AstInfo.TopLeft.Y = AstInfo.Height * cRandom.Next(-1, 1);
				if ((AstInfo.TopLeft.X == 0) && (AstInfo.TopLeft.Y == 0)) {
					AstInfo.TopLeft.X = -1 * AstInfo.Width;
				}
			} else {
				AstInfo.TopLeft.X = Position.X;
				AstInfo.TopLeft.Y = Position.Y;
			}

			AstInfo.Image = cTextureDict[Textures.Asteroid];

			AstSpeed = MGMath.CalculateXYMagnitude((float)cRandom.NextDouble() * 6.28318531f, 2);
			AstInfo.SpeedX = AstSpeed.X;
			AstInfo.SpeedY = AstSpeed.Y;
			AstInfo.Tint = new Color(150 + cRandom.Next(0, 105), 150 + cRandom.Next(0, 105), 150 + cRandom.Next(0, 105), 255);

			AstInfo.SpeedRotate = ((float)cRandom.NextDouble() * 0.2f) - 0.1f;

			AstInfo.SplitOnDeath = true;

			cAsteroids.AddParticle(AstInfo);
		}

		private void CreateNewHunter(int Size, Vector2 Position) {
			HunterShip NewShip = new HunterShip();

			if ((Position.X == -1) && (Position.Y == -1)) {
				NewShip.TopLeft.X = NewShip.Width * cRandom.Next(-1, 1);
				NewShip.TopLeft.Y = NewShip.Height * cRandom.Next(-1, 1);
				if ((NewShip.TopLeft.X == 0) && (NewShip.TopLeft.Y == 0)) {
					NewShip.TopLeft.X = -1 * NewShip.Width;
				}
			} else {
				NewShip.TopLeft.X = Position.X;
				NewShip.TopLeft.Y = Position.Y;
			}
				
			NewShip.TargetObject = cPlayerShip;
			NewShip.MaxDistanceFromTarget = 150 + (75 * cRandom.Next(1, 5));
			NewShip.MinDistanceFromTarget = (NewShip.MaxDistanceFromTarget / 2) + 25;
			NewShip.Image = cTextureDict[Textures.Hunter];
			NewShip.Height = Size;
			NewShip.Width = Size;
			NewShip.Tint = Color.White;
			NewShip.SplitOnDeath = false;

			cAsteroids.AddParticle(NewShip);
		}

		protected enum Textures {
			[Description("UFO.png")]
			Ship,
			[Description("Asteroid.png")]
			Asteroid,
			[Description("Font.png")]
			Font,
			[Description("Bullet-Blue.png")]
			Bullet,
			[Description("UFO-Red.png")]
			Hunter
		}
	}
}
