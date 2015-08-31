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
		private Particles2D cPlayerBullets;
		private Particles2D cAsteroids;
		private double cLastShot, cLastAsteroid;
		private MDLN.MGTools.GameConsole cDevConsole;
		//private TextureFont cFont;
		private KeyboardState cPriorKeyState;
		private Dictionary<Textures, Texture2D> cTextureDict;

		public AsteroidShooter() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			cTextureDict = new Dictionary<Textures, Texture2D>();
		}

		protected override void Initialize() {

			//Initializes monogame
			base.Initialize();
		}

		protected override void UnloadContent() {
		}

		protected override void LoadContent() {
			cTextureDict.Add(Textures.Asteroid, Content.Load<Texture2D>(Tools.Tools.GetEnumDescriptionAttribute(Textures.Asteroid)));
			cTextureDict.Add(Textures.Ship, Content.Load<Texture2D>(Tools.Tools.GetEnumDescriptionAttribute(Textures.Ship)));
			cTextureDict.Add(Textures.Font, Content.Load<Texture2D>(Tools.Tools.GetEnumDescriptionAttribute(Textures.Font)));
			cTextureDict.Add(Textures.Bullet, Content.Load<Texture2D>(Tools.Tools.GetEnumDescriptionAttribute(Textures.Bullet)));

			//cFont = new TextureFont(cTextureDict[Textures.Font]);
			cDevConsole = new MDLN.MGTools.GameConsole(cGraphDevMgr.GraphicsDevice, Content, "Font.png", 0, 0, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2);
			cDevConsole.CommandSent += CommandSentEventHandler;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;

			cPlayerShip = new Ship(cGraphDevMgr.GraphicsDevice, 150);
			cPlayerShip.BackgroundColor = new Color(100, 100, 100, 0); //Set background completely transparent
			cPlayerShip.ShipTexture = cTextureDict[Textures.Ship];
			cPlayerShip.Visible = true;
			cPlayerShip.Top = cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2;
			cPlayerShip.Left = cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width / 2;

			cPlayerBullets = new Particles2D(cGraphDevMgr.GraphicsDevice);

			cAsteroids = new Particles2D(cGraphDevMgr.GraphicsDevice);
			cAsteroids.WrapScreenEdges = true;
		}

		protected override void Update(GameTime gameTime) {
			KeyboardState CurrKeys = Keyboard.GetState();
			MouseState CurrMouse = Mouse.GetState();
			Vector2 BulletOrigin, BulletCenter, AsteroidCenter;
			float ParticleDist, CollisionDist;
			ParticleInfo BulletInfo, AstInfo;

			if (cLastAsteroid < gameTime.TotalGameTime.TotalMilliseconds) {
				cAsteroids.AddParticle(cTextureDict[Textures.Asteroid], -100, -100, 100, 100, cPlayerShip.cRotation - 0.628f, 2, Color.White);

				cLastAsteroid = gameTime.TotalGameTime.TotalMilliseconds + 1000;
			}

			if ((CurrKeys.IsKeyDown(Keys.Space) == true) && (cLastShot < gameTime.TotalGameTime.TotalMilliseconds - 250)) {
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
			for (int Ctr = 0; Ctr < cPlayerBullets.cParticleList.Count; Ctr++) {
				BulletInfo = cPlayerBullets.cParticleList[Ctr];
				BulletCenter.X = BulletInfo.DrawRegion.X + (BulletInfo.DrawRegion.Width / 2);
				BulletCenter.Y = BulletInfo.DrawRegion.Y + (BulletInfo.DrawRegion.Height / 2);

				for (int Cnt = 0; Cnt < cAsteroids.cParticleList.Count; Cnt++) {
					AstInfo = cAsteroids.cParticleList[Cnt];

					AsteroidCenter.X = AstInfo.DrawRegion.X + (AstInfo.DrawRegion.Width / 2);
					AsteroidCenter.Y = AstInfo.DrawRegion.Y + (AstInfo.DrawRegion.Height / 2);

					CollisionDist = (float)Math.Pow((AstInfo.DrawRegion.Width / 2) + (BulletInfo.DrawRegion.Width / 2), 2);
					ParticleDist = (float)(Math.Pow(AsteroidCenter.X - BulletCenter.X, 2) + Math.Pow(AsteroidCenter.Y - BulletCenter.Y, 2));

					if (ParticleDist < CollisionDist) {
						cPlayerBullets.cParticleList.RemoveAt(Ctr);
						cAsteroids.cParticleList.RemoveAt(Cnt);
						break; //Exit inner loop so each bullet ony gets 1 asteroid
					}
				}
			}

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
				cDevConsole.AddText("Ship position set to [X=" + cPlayerShip.Top + ", Y=" + cPlayerShip.Left + "]");
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
			} else {
				cDevConsole.AddText("Unrecognized command: " + Command);
			}
		}

		protected enum Textures {
			[Description("Ship.png")]
			Ship,
			[Description("Asteroid.png")]
			Asteroid,
			[Description("Font.png")]
			Font,
			[Description("Bullet-Blue.png")]
			Bullet
		}
	}
}
