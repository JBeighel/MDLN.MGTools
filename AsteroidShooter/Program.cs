using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

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
		private readonly GraphicsDeviceManager cGraphDevMgr;
		private Ship cPlayerShip;
		private ParticleEngine2D cPlayerBullets, cEnemyBullets;
		private ParticleEngine2D cAsteroids, cUFOs;
		private ParticleEngine2D cSparkles;
		private double cLastShot, cLastAsteroid;
		private MDLN.MGTools.GameConsole cDevConsole;
		private KeyboardState cPriorKeyState;
		private readonly Dictionary<Textures, Texture2D> cTextureDict;
		private readonly Random cRandom;
		private uint cEnemyKills, cAliveSince, cSpawnNum, cEnemyKillsMax;
		private SpriteBatch cDrawBatch;
		private Texture2D cSolidTexture;
		private TextureFont cFont;
		private bool cHeadlightMode, cShowStats;
		//private Effect cShader, cShipShader;

		public AsteroidShooter() {
			cGraphDevMgr = new GraphicsDeviceManager(this) {
				PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,

				PreferredBackBufferWidth = 1024,//Set the window size
				PreferredBackBufferHeight = 768
			};
			cGraphDevMgr.ApplyChanges();

			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			cTextureDict = new Dictionary<Textures, Texture2D>();

			cRandom = new Random(DateTime.Now.Millisecond);

			cEnemyKills = 0;
			cAliveSince = 0;
			cSpawnNum = 0;

			cHeadlightMode = false;
		}

		protected override void Initialize() {

			//Initializes monogame
			base.Initialize();
		}

		protected override void UnloadContent() {
		}

		protected override void LoadContent() {
			cDrawBatch = new SpriteBatch(cGraphDevMgr.GraphicsDevice);

			cSolidTexture = new Texture2D(cGraphDevMgr.GraphicsDevice, 1, 1);
			//cSolidTexture.SetData (new[] { new Color(255, 255, 255, 100) });
			//cSolidTexture.SetData (new[] { new Color(0, 0, 0, 100) });
			cSolidTexture.SetData (new[] { Color.White });

			foreach (Textures CurrTexture in Enum.GetValues(typeof(Textures))) {
				cTextureDict.Add(CurrTexture, Content.Load<Texture2D>(Tools.EnumTools.GetEnumDescriptionAttribute(CurrTexture)));
			}

			//cShader = Content.Load<Effect>("ShaderEffect");
			//cShipShader = Content.Load<Effect>("BumpMap");
			//cShipShader.Parameters["NormalMap"].SetValue(cTextureDict[Textures.ShipNormal]);

			cFont = new TextureFont(cTextureDict[Textures.Font]);
			cDevConsole = new MDLN.MGTools.GameConsole(cGraphDevMgr.GraphicsDevice, Content, Content.RootDirectory + "\\Font.png", 0, 0, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2);
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
			cPlayerBullets.DrawBlendingMode = BlendState.Additive;
			//cPlayerBullets.ShaderEffect = cShader;

			cEnemyBullets = new ParticleEngine2D(cGraphDevMgr.GraphicsDevice);
			cEnemyBullets.DrawBlendingMode = BlendState.Additive;

			cAsteroids = new ParticleEngine2D(cGraphDevMgr.GraphicsDevice);
			cAsteroids.WrapScreenEdges = true;

			cUFOs = new ParticleEngine2D(cGraphDevMgr.GraphicsDevice);
			//cUFOs.ShaderEffect = cShipShader;
			cUFOs.WrapScreenEdges = true;

			cSparkles = new ParticleEngine2D(cGraphDevMgr.GraphicsDevice);
			cSparkles.DrawBlendingMode = BlendState.Additive;
		}

		protected override void Update(GameTime gameTime) {
			KeyboardState CurrKeys = Keyboard.GetState();
			MouseState CurrMouse = Mouse.GetState();
			Particle2D BulletInfo, EnemyInfo;
			List<int> ParticlesToRemove = new List<int>();
			int Ctr, Cnt;

			if (cAliveSince == 0) {
				cAliveSince = (uint)gameTime.TotalGameTime.TotalSeconds;
			}

			if (cLastAsteroid < gameTime.TotalGameTime.TotalMilliseconds) { //Create new asteroid
				if ((cSpawnNum % 3 != 0) && (cEnemyKills >= cAsteroids.ParticleList.Count)) {
					for (Ctr = 0; Ctr < 1 + (cEnemyKills / 20); Ctr++) {
						CreateNewAsteroid(100, new Vector2(-1, -1));
					}
				} else {
					Vector2 StartPos;

					StartPos.X = (float)(cRandom.NextDouble() * cGraphDevMgr.GraphicsDevice.Viewport.Width);
					StartPos.Y = cRandom.Next(-2, 1) * 60;

					if (StartPos.Y == 0) {
						StartPos.Y = cGraphDevMgr.GraphicsDevice.Viewport.Height;
					}

					CreateNewHunter(60, StartPos);
				}

				cSpawnNum++;
				cLastAsteroid = gameTime.TotalGameTime.TotalMilliseconds + 2000;
			}

			//Check for player input
			Ctr = (int)(500 - cEnemyKillsMax); //Calculate the minimum time between shots
			if (Ctr > 400) {
				Ctr = 400;
			} else if (Ctr < 100) {
				Ctr = 50;
			}

			if (((CurrKeys.IsKeyDown(Keys.Space) == true) || (CurrMouse.LeftButton == ButtonState.Pressed)) && (cLastShot < gameTime.TotalGameTime.TotalMilliseconds - Ctr)) {
				PlayerFireBullet();

				cLastShot = gameTime.TotalGameTime.TotalMilliseconds;
			}

			if ((CurrKeys.IsKeyDown(Keys.OemTilde) == true) && (cPriorKeyState.IsKeyDown(Keys.OemTilde) == false)) {
				cDevConsole.ToggleVisible();
			}

			cUFOs.Update(gameTime);
			cAsteroids.Update(gameTime);
			cEnemyBullets.Update(gameTime);
			cPlayerBullets.Update(gameTime);
			cPlayerShip.Update(gameTime, CurrKeys, CurrMouse);
			cSparkles.Update(gameTime);
			cDevConsole.Update(gameTime, CurrKeys, CurrMouse);

			//Collision detection
			cPlayerShip.ImageTint = Color.White;
			for (Cnt = 0; Cnt < cAsteroids.ParticleList.Count; Cnt++) {
				EnemyInfo = cAsteroids.ParticleList[Cnt];

				//Are bullts hitting the asteroid?
				for (Ctr = 0; Ctr < cPlayerBullets.ParticleList.Count; Ctr++) {
					BulletInfo = cPlayerBullets.ParticleList[Ctr];

					if (BulletInfo.TestCollision(EnemyInfo.GetCollisionRegions()) == true) {
						CreateParticleBurst(new Vector2(EnemyInfo.TopLeft.X + (EnemyInfo.Width / 2), EnemyInfo.TopLeft.Y + (EnemyInfo.Height / 2)), 25 * EnemyInfo.Height / 6, EnemyInfo.Height / 3, Color.SaddleBrown, cTextureDict[Textures.Dust]);

						//Spawn little asteroids
						if (EnemyInfo.Height > 50) {
							Vector2 TopLeft;

							TopLeft.X = EnemyInfo.TopLeft.X + (EnemyInfo.Width / 2) - (EnemyInfo.Width * 0.35f);
							TopLeft.Y = EnemyInfo.TopLeft.Y + (EnemyInfo.Height / 2) - (EnemyInfo.Height * 0.35f);
								
							CreateNewAsteroid((int)(EnemyInfo.Width * 0.7f), TopLeft);
							CreateNewAsteroid((int)(EnemyInfo.Width * 0.7f), TopLeft);
						}

						//Destroy shot and large asteroid
						ParticlesToRemove.Add(Cnt);
						cPlayerBullets.ParticleList.RemoveAt(Ctr);
						cEnemyKills++;

						break; //Exit inner loop so each bullet ony gets 1 asteroid
					}
				}

				//Is the asteroid hitting the player?
				if (EnemyInfo.TestCollision(cPlayerShip) == true) {
					cPlayerShip.ImageTint = Color.Red;

					CreateParticleBurst(new Vector2(EnemyInfo.TopLeft.X + (EnemyInfo.Width / 2), EnemyInfo.TopLeft.Y + (EnemyInfo.Height / 2)), 25 * EnemyInfo.Height / 6, EnemyInfo.Height / 3, Color.SaddleBrown, cTextureDict[Textures.Dust]);

					//Spawn little asteroids
					if (EnemyInfo.Height > 50) {
						Vector2 TopLeft;

						TopLeft.X = EnemyInfo.TopLeft.X + (EnemyInfo.Width / 2) - (EnemyInfo.Width * 0.35f);
						TopLeft.Y = EnemyInfo.TopLeft.Y + (EnemyInfo.Height / 2) - (EnemyInfo.Height * 0.35f);

						CreateNewAsteroid((int)(EnemyInfo.Width * 0.7f), TopLeft);
						CreateNewAsteroid((int)(EnemyInfo.Width * 0.7f), TopLeft);
					}

					//Destroy asteroid
					ParticlesToRemove.Add(Cnt);

					PlayerHit(gameTime);
				}
			}

			for (Cnt = ParticlesToRemove.Count - 1; Cnt >= 0; Cnt--) {
				cAsteroids.ParticleList.RemoveAt(ParticlesToRemove[Cnt]);
			}
			ParticlesToRemove.Clear();

			for (Cnt = 0; Cnt < cUFOs.ParticleList.Count; Cnt++) {
				EnemyInfo = cUFOs.ParticleList[Cnt];

				//Are bullts hitting the UFO?
				for (Ctr = 0; Ctr < cPlayerBullets.ParticleList.Count; Ctr++) {
					BulletInfo = cPlayerBullets.ParticleList[Ctr];

					if (BulletInfo.TestCollision(EnemyInfo.GetCollisionRegions()) == true) {
						//Destroy shot and UFO
						cPlayerBullets.ParticleList.RemoveAt(Ctr);
						ParticlesToRemove.Add(Cnt);
						cEnemyKills++;

						CreateParticleBurst(new Vector2(EnemyInfo.TopLeft.X + (EnemyInfo.Width / 2), EnemyInfo.TopLeft.Y + (EnemyInfo.Height / 2)), 200, Color.OrangeRed);

						break; //Exit inner loop so each bullet ony gets 1 enemy
					}
				}

				//Is the UFO hitting the player?
				if (EnemyInfo.TestCollision(cPlayerShip) == true) {
					cPlayerShip.ImageTint = Color.Red;

					PlayerHit(gameTime);
				}
			}

			for (Cnt = ParticlesToRemove.Count - 1; Cnt >= 0; Cnt--) {
				cUFOs.ParticleList.RemoveAt(ParticlesToRemove[Cnt]);
			}

			for (Cnt = 0; Cnt < cEnemyBullets.ParticleList.Count; Cnt++) {
				BulletInfo = cEnemyBullets.ParticleList[Cnt];

				//Is the bullet hitting the player?
				if (BulletInfo.TestCollision(cPlayerShip) == true) {
					cEnemyBullets.ParticleList.RemoveAt(Cnt);
					
					PlayerHit(gameTime);
				}
			}

			cPriorKeyState = CurrKeys;

			//Use monogame update
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			Rectangle OverlayRect;
			Vector2 OverlayOrigin;

			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

			cUFOs.Draw();
			cAsteroids.Draw();
			cEnemyBullets.Draw();
			cPlayerBullets.Draw();

			if (cHeadlightMode == true) {//Dimming overlay for the flashlight
				cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

				OverlayRect.X = cPlayerShip.Left + (cPlayerShip.Width / 2);
				OverlayRect.Y = cPlayerShip.Top + (cPlayerShip.Height / 2);
				OverlayRect.Width = cTextureDict[Textures.LightFilter].Width * 4 + cGraphDevMgr.GraphicsDevice.Viewport.Width;
				OverlayRect.Height = cTextureDict[Textures.LightFilter].Height * 4 + cGraphDevMgr.GraphicsDevice.Viewport.Height;

				OverlayOrigin.X = cTextureDict[Textures.LightFilter].Width / 2;
				OverlayOrigin.Y = cTextureDict[Textures.LightFilter].Height;

				//cDrawBatch.Draw(cSolidTexture, cGraphDevMgr.GraphicsDevice.Viewport.Bounds, cSolidTexture.Bounds, new Color(255, 0, 0, 255));
				cDrawBatch.Draw(cTextureDict[Textures.LightFilter], OverlayRect, cTextureDict[Textures.LightFilter].Bounds, new Color(255, 200, 255, 250), (float)((Math.PI * 2) - (cPlayerShip.cRotation)), OverlayOrigin, SpriteEffects.None, 0);
				cDrawBatch.Draw(cTextureDict[Textures.LightFilter], OverlayRect, cTextureDict[Textures.LightFilter].Bounds, new Color(255, 200, 255, 250), (float)((Math.PI * 2) - (cPlayerShip.cRotation + Math.PI)), OverlayOrigin, SpriteEffects.FlipHorizontally, 0);

				cDrawBatch.End();
			}

			cPlayerShip.Draw();
			cSparkles.Draw();

			cDevConsole.Draw();

			if (cShowStats == true) {
				cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
				cFont.WriteText(cDrawBatch, string.Format("Asteroids={0} UFOs={1} Bullets={2} Sparkles={3} Kills={4} Max={5} Alive={6:0.00}s", cAsteroids.ParticleList.Count, cUFOs.ParticleList.Count, cPlayerBullets.ParticleList.Count + cEnemyBullets.ParticleList.Count, cSparkles.ParticleList.Count, cEnemyKills, cEnemyKillsMax, gameTime.TotalGameTime.TotalSeconds - cAliveSince), 10, cGraphDevMgr.GraphicsDevice.Viewport.Height - cFont.CharacterHeight, 10, Color.Azure);
				cDrawBatch.End();
			}

			/*
			Vector2 ShadowOrigin = MGTools.MGMath.CalculateXYMagnitude(cPlayerShip.cRotation, cPlayerShip.Height);
			Matrix StencilMatrix = Matrix.CreateOrthographicOffCenter(0, cGraphDevMgr.GraphicsDevice.PresentationParameters.BackBufferWidth, cGraphDevMgr.GraphicsDevice.PresentationParameters.BackBufferHeight, 0, 0, 1);

			var StencilAlphaTest = new AlphaTestEffect(cGraphDevMgr.GraphicsDevice);
			StencilAlphaTest.Projection = StencilMatrix;

			DepthStencilState DepthStencilMask = new DepthStencilState();
			DepthStencilMask.StencilEnable = true;
			DepthStencilMask.StencilFunction = CompareFunction.Always;
			DepthStencilMask.StencilPass = StencilOperation.Replace;
			DepthStencilMask.ReferenceStencil = 1;
			DepthStencilMask.DepthBufferEnable = false;

			DepthStencilState DepthStencilDraw = new DepthStencilState {
				StencilEnable = true,
				StencilFunction = CompareFunction.LessEqual,
				StencilPass = StencilOperation.Keep,
				ReferenceStencil = 1,
				DepthBufferEnable = false,
			};

			cDrawBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, DepthStencilMask, null, StencilAlphaTest);
			cDrawBatch.Draw(cSolidTexture, new Rectangle((int)(cPlayerShip.Left + (cPlayerShip.Width / 2) - ShadowOrigin.X), (int)(cPlayerShip.Top + (cPlayerShip.Height / 2) + ShadowOrigin.Y), cGraphDevMgr.GraphicsDevice.Viewport.Width * 2, cGraphDevMgr.GraphicsDevice.Viewport.Height * 2), cSolidTexture.Bounds, new Color(0,0,0,0), (float)(2 * Math.PI - cPlayerShip.cRotation + 1.0472), Vector2.Zero, SpriteEffects.None, 0);
			cDrawBatch.Draw(cSolidTexture, new Rectangle((int)(cPlayerShip.Left + (cPlayerShip.Width / 2) - ShadowOrigin.X), (int)(cPlayerShip.Top + (cPlayerShip.Height / 2) + ShadowOrigin.Y), cGraphDevMgr.GraphicsDevice.Viewport.Width * 2, cGraphDevMgr.GraphicsDevice.Viewport.Height * 2), cSolidTexture.Bounds, new Color(0,0,0,0), (float)(2 * Math.PI - cPlayerShip.cRotation - 2.618), Vector2.Zero, SpriteEffects.None, 0);
			cDrawBatch.Draw(cSolidTexture, new Rectangle((int)(cPlayerShip.Left + (cPlayerShip.Width / 2) - ShadowOrigin.X), (int)(cPlayerShip.Top + (cPlayerShip.Height / 2) + ShadowOrigin.Y), cGraphDevMgr.GraphicsDevice.Viewport.Width * 2, cGraphDevMgr.GraphicsDevice.Viewport.Height * 2), cSolidTexture.Bounds, new Color(0,0,0,0), (float)(2 * Math.PI - cPlayerShip.cRotation + (Math.PI / 2)), Vector2.Zero, SpriteEffects.None, 0);
			cDrawBatch.Draw(cSolidTexture, new Rectangle((int)(cPlayerShip.Left + (cPlayerShip.Width / 2) - ShadowOrigin.X), (int)(cPlayerShip.Top + (cPlayerShip.Height / 2) + ShadowOrigin.Y), cGraphDevMgr.GraphicsDevice.Viewport.Width * 2, cGraphDevMgr.GraphicsDevice.Viewport.Height * 2), cSolidTexture.Bounds, new Color(0,0,0,0), (float)(2 * Math.PI - cPlayerShip.cRotation + Math.PI), Vector2.Zero, SpriteEffects.None, 0);
			cDrawBatch.End();

			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilDraw, null, StencilAlphaTest); //Draw stuff to be masked
			cDrawBatch.Draw(cTextureDict[Textures.Asteroid], cGraphDevMgr.GraphicsDevice.Viewport.Bounds, new Color(0, 0, 0, 100)); //Uses 
			cDrawBatch.End();
			*/
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
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*(sparkles|particles)\s*$") == true) {
				Particle2D NewSparkle;
				Vector2 Speed;

				cDevConsole.AddText("Particle burst on player ship");

				for (int Ctr = 0; Ctr < 25; Ctr++) {
					NewSparkle = new Particle2D(cGraphDevMgr.GraphicsDevice);

					NewSparkle.AlphaFade = true;
					NewSparkle.TimeToLive = 100 + (cRandom.NextDouble() * 1000);
					NewSparkle.Height = 10;
					NewSparkle.Width = 10;
					NewSparkle.TopLeft.X = cPlayerShip.Left + (cPlayerShip.Width / 2);
					NewSparkle.TopLeft.Y = cPlayerShip.Top + (cPlayerShip.Height / 2);
					NewSparkle.Image = cTextureDict[Textures.Bullet];

					NewSparkle.Rotation = (float)(cRandom.NextDouble() * 6.2f);
					Speed = MGMath.CalculateXYMagnitude(NewSparkle.Rotation, (float)(cRandom.NextDouble() * 5));
					NewSparkle.SpeedX = Speed.X;
					NewSparkle.SpeedY = Speed.Y;

					NewSparkle.Tint = Color.White;

					cSparkles.AddParticle(NewSparkle);
				}
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*(headlight|flashlight|dark)\s*=\s*(1|true|enable)\s*$") == true) {
				cDevConsole.AddText("Headlight mode enabled.");
				cHeadlightMode = true;
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*(headlight|flashlight|dark)\s*=\s*(0|false|disable)\s*$") == true) {
				cDevConsole.AddText("Headlight mode disabled.");
				cHeadlightMode = false;
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*stats\s*=\s*(1|true|enable|on)\s*$") == true) {
				cDevConsole.AddText("Stats enabled.");
				cShowStats = true;
			} else if (Tools.RegEx.QuickTest(Command, @"^\s*stats\s*=\s*(0|false|disable|off)\s*$") == true) {
				cDevConsole.AddText("Stats disabled.");
				cShowStats = false;
			}  else {
				cDevConsole.AddText("Unrecognized command: " + Command);
			}
		}

		private void CreateNewAsteroid(int Size, Vector2 Position) {
			Particle2D AstInfo;
			Vector2 AstSpeed;

			AstInfo = new Particle2D(GraphicsDevice);

			AstInfo.Width = Size;
			AstInfo.Height = Size;

			if ((Position.X == -1) && (Position.Y == -1)) {
				AstInfo.TopLeft.X = AstInfo.Width * -1;
				AstInfo.TopLeft.Y = AstInfo.Height * -1;
			} else {
				AstInfo.TopLeft.X = Position.X;
				AstInfo.TopLeft.Y = Position.Y;
			}

			AstInfo.Image = cTextureDict[Textures.Asteroid];
			AstInfo.TimeToLive = -1;

			AstSpeed = MGMath.CalculateXYMagnitude((float)cRandom.NextDouble() * 6.28318531f, 2);
			AstInfo.SpeedX = AstSpeed.X;
			AstInfo.SpeedY = AstSpeed.Y;
			AstInfo.Tint = new Color(150 + cRandom.Next(0, 105), 150 + cRandom.Next(0, 105), 150 + cRandom.Next(0, 105), 255);

			AstInfo.SpeedRotate = ((float)cRandom.NextDouble() * 0.2f) - 0.1f;

			cAsteroids.AddParticle(AstInfo);
		}

		private void CreateNewHunter(int Size, Vector2 Position) {
			HunterShip NewShip = new HunterShip(GraphicsDevice);

			NewShip.Height = Size;
			NewShip.Width = Size;

			if ((Position.X == -1) && (Position.Y == -1)) {
				NewShip.TopLeft.X = -1 * NewShip.Width;
				NewShip.TopLeft.Y = -1 * NewShip.Height;
			} else {
				NewShip.TopLeft.X = Position.X;
				NewShip.TopLeft.Y = Position.Y;
			}
				
			NewShip.TargetObject = cPlayerShip;
			NewShip.MaxDistanceFromTarget = 150 + (75 * cRandom.Next(1, 5));
			NewShip.MinDistanceFromTarget = (NewShip.MaxDistanceFromTarget / 2) + 25;
			NewShip.Image = cTextureDict[Textures.Hunter];
			NewShip.Tint = Color.White;

			NewShip.BulletManager = cEnemyBullets;
			NewShip.BulletTexture = cTextureDict[Textures.Bullet];

			cUFOs.AddParticle(NewShip);
		}

		private void CreateParticleBurst(Vector2 Position, int Count) {
			CreateParticleBurst(Position, Count, Color.White);
		}

		private void CreateParticleBurst(Vector2 Position, int Count, Color Tint) {
			CreateParticleBurst(Position, Count, 10, Tint, cTextureDict[Textures.Bullet]);
		}

		private void CreateParticleBurst(Vector2 Position, int Count, int Size, Color Tint, Texture2D Image) {
			Particle2D NewSparkle;
			Vector2 Speed;

			for (int Ctr = 0; Ctr < Count; Ctr++) {
				NewSparkle = new Particle2D(GraphicsDevice);

				NewSparkle.AlphaFade = true;
				NewSparkle.TimeToLive = 100 + (cRandom.NextDouble() * 1000);
				NewSparkle.Height = Size;
				NewSparkle.Width = Size;
				NewSparkle.TopLeft = Position;
				NewSparkle.Image = Image;

				NewSparkle.Rotation = (float)(cRandom.NextDouble() * 6.2f);
				Speed = MGMath.CalculateXYMagnitude(NewSparkle.Rotation, (float)(cRandom.NextDouble() * (70 / Size)));
				NewSparkle.SpeedX = Speed.X;
				NewSparkle.SpeedY = Speed.Y;
				NewSparkle.SpeedRotate = (float)cRandom.NextDouble() * 0.25f;

				NewSparkle.Tint = Tint;

				cSparkles.AddParticle(NewSparkle);
			}
		}

		private void PlayerFireBullet() {
			Vector2 BulletOrigin, BulletOffset;
			int Ctr;
			Color BulletColor = new Color(75, 75, 255, 255);

			if (cEnemyKills >= 50) { //Spread shot
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation - 0.628f, 10, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation - 0.314f, 10, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation, 10, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation + 0.314f, 10, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], cPlayerShip.Top + (cPlayerShip.Height / 2) - 10, cPlayerShip.Left + (cPlayerShip.Height / 2) - 10, 20, 20, cPlayerShip.cRotation + 0.628f, 10, BulletColor);
			} else if (cEnemyKills >= 30) { //Multi-shot
				BulletOrigin = MGMath.CalculateXYMagnitude(-1 * cPlayerShip.cRotation, cPlayerShip.Width / 4);
				BulletOffset = MGMath.CalculateXYMagnitude(-1 * cPlayerShip.cRotation + 1.570796f, cPlayerShip.Width / 5);

				//Adjust it so that it's relative to the top left screen corner
				BulletOrigin.Y += cPlayerShip.Top + (cPlayerShip.Height / 2);
				BulletOrigin.X += cPlayerShip.Left + (cPlayerShip.Height / 2);

				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y + (BulletOffset.Y * 2) - 10, BulletOrigin.X + (BulletOffset.X * 2) - 10, 20, 20, cPlayerShip.cRotation, 15, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y + BulletOffset.Y - 10, BulletOrigin.X + BulletOffset.X - 10, 20, 20, cPlayerShip.cRotation, 15, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - 10, BulletOrigin.X - 10, 20, 20, cPlayerShip.cRotation, 15, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - BulletOffset.Y - 10, BulletOrigin.X - BulletOffset.X - 10, 20, 20, cPlayerShip.cRotation, 15, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - (BulletOffset.Y * 2) - 10, BulletOrigin.X - (BulletOffset.X * 2) - 10, 20, 20, cPlayerShip.cRotation, 15, BulletColor);
			} else if (cEnemyKills >= 10) {
				BulletOrigin = MGMath.CalculateXYMagnitude(-1 * cPlayerShip.cRotation, cPlayerShip.Width / 4);
				BulletOffset = MGMath.CalculateXYMagnitude(-1 * cPlayerShip.cRotation + 1.570796f, cPlayerShip.Width / 5);

				//Adjust it so that it's relative to the top left screen corner
				BulletOrigin.Y += cPlayerShip.Top + (cPlayerShip.Height / 2);
				BulletOrigin.X += cPlayerShip.Left + (cPlayerShip.Height / 2);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y + BulletOffset.Y - 10, BulletOrigin.X + BulletOffset.X - 10, 20, 20, cPlayerShip.cRotation, 15, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - 10, BulletOrigin.X - 10, 20, 20, cPlayerShip.cRotation, 15, BulletColor);
				cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - BulletOffset.Y - 10, BulletOrigin.X - BulletOffset.X - 10, 20, 20, cPlayerShip.cRotation, 15, BulletColor);
			} else { //Single shot
				//Calculate coordinates of ship tip relative to its center
				BulletOrigin = MDLN.MGTools.MGMath.CalculateXYMagnitude(-1 * cPlayerShip.cRotation, cPlayerShip.Width / 4);

				//Adjust it so that it's relative to the top left screen corner
				BulletOrigin.Y += cPlayerShip.Top + (cPlayerShip.Height / 2);
				BulletOrigin.X += cPlayerShip.Left + (cPlayerShip.Height / 2);

				for (Ctr = 0; Ctr <= cEnemyKillsMax; Ctr += 100) {
					cPlayerBullets.AddParticle(cTextureDict[Textures.Bullet], BulletOrigin.Y - 10, BulletOrigin.X - 10, 20, 20, cPlayerShip.cRotation, 15, BulletColor);
				}
			}
		}

		private void PlayerHit(GameTime gameTime) {
			CreateParticleBurst(new Vector2(cPlayerShip.Left + (cPlayerShip.Width / 2), cPlayerShip.Top + (cPlayerShip.Height / 2)), 25);

			cPlayerShip.ImageTint = Color.Red;
			if ((gameTime.TotalGameTime.TotalSeconds - cAliveSince >= 1) && (cAliveSince != 0)) {
				cDevConsole.AddText(String.Format("Survived {0:0.0} seconds and shot {1} enemies.", gameTime.TotalGameTime.TotalSeconds - cAliveSince, cEnemyKills));
			}

			if (cEnemyKills > cEnemyKillsMax) {
				cEnemyKillsMax = cEnemyKills;
			}

			cEnemyKills = 0;
			cAliveSince = 0;
		}

		protected enum Textures {
			[Description("UFO-NormalMap.png")]
			ShipNormal,
			[Description("UFO.png")]
			Ship,
			[Description("Asteroid.png")]
			Asteroid,
			[Description("Font.png")]
			Font,
			[Description("Bullet-Gray.png")]
			Bullet,
			[Description("UFO-Red.png")]
			Hunter,
			[Description("Flashlight.png")]
			LightFilter,
			[Description("Dust.png")]
			Dust
		}
	}
}
	