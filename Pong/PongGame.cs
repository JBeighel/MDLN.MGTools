using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace MDLN.Pong {
	class PongGame : Game {
		private const string CONTENT_PATH = "Content";

		private const int BALL_MINSPEED = 10;
		private const float BALL_VOLLEYSPEEDINCREASE = 0.5f;

		private const int PADDLE_MAXSPEED = 10;
		private const int PADDLE_MAXANGLECHANGE = 15;

		private GraphicsDeviceManager cGraphDevMgr;
		private SpriteBatch cDrawBatch;

		private KeyboardState cPriorKeyState;
		private MouseState cPriorMouseState;
		private GamePadState cPriorPadState;

		private Dictionary<Textures, Texture2D> cTextureDict;
		private Random cRandom;

		private TextureFont cFont;
		private GameConsole cDevConsole;
		private Button cLeftPaddleInputBtn, cRightPaddleInputBtn, cScoreLbl;

		private GameObject[] cBoundaryBars;
		private GameObject cBall;
		private GameObject cLeftPaddle, cRightPaddle;

		private Vector2 cBallSpeed;
		private PaddleInput cLeftPaddleControl, cRightPaddleControl;
		private int cScoreLeft, cScoreRight;
		private float cBallSpeedMag;

		public PongGame() {
			//Setup the graphics device
			cGraphDevMgr = new GraphicsDeviceManager(this);
			cGraphDevMgr.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

			cGraphDevMgr.PreferredBackBufferWidth = 1024;
			cGraphDevMgr.PreferredBackBufferHeight = 768;
			cGraphDevMgr.ApplyChanges();

			//Setup monogame class variables
			IsMouseVisible = true;

			//Setup class specific variables
			cTextureDict = new Dictionary<Textures, Texture2D>();
			cRandom = new Random(DateTime.Now.Millisecond);
			cBoundaryBars = new GameObject[2];

			cLeftPaddleControl = PaddleInput.Mouse;
			cRightPaddleControl = PaddleInput.Computer;

			cScoreLeft = 0;
			cScoreRight = 0;

			cBallSpeedMag = BALL_MINSPEED;
        }

		protected override void Initialize() {

			//Initialize monogame
			base.Initialize();
		}

		protected override void UnloadContent() {
			//The files are loaded directly, so they must be cleaned up manually
			foreach (Texture2D CurrTexture in cTextureDict.Values) {
				CurrTexture.Dispose();
			}
		}

		protected override void LoadContent() {
			string FileToLoad;
			Texture2D TextureImage;
			CollisionRegion NewCollide = new CollisionRegion();
			int Ctr;

			cDrawBatch = new SpriteBatch(cGraphDevMgr.GraphicsDevice);

			//Load all texture files
			foreach (Textures CurrTexture in Enum.GetValues(typeof(Textures))) {
				FileToLoad = String.Format("{0}{1}{2}", CONTENT_PATH, Path.DirectorySeparatorChar, EnumTools.GetEnumDescriptionAttribute(CurrTexture));

				TextureImage = Texture2D.FromStream(cGraphDevMgr.GraphicsDevice, File.OpenRead(FileToLoad));

				cTextureDict.Add(CurrTexture, TextureImage);
			}

			//Create drawing objects
			cFont = new TextureFont(cTextureDict[Textures.Font]);
			cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, cFont, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2);
			cDevConsole.CommandSent += ConsoleCommandHandler;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;

			//Create interface controls
			TextureImage = new Texture2D(cGraphDevMgr.GraphicsDevice, 1, 1);
			TextureImage.SetData(new[] { Color.Transparent });

			cLeftPaddleInputBtn = new Button(cGraphDevMgr.GraphicsDevice, TextureImage, 0, 0, 50, 200);
			cLeftPaddleInputBtn.Visible = true;
			cLeftPaddleInputBtn.Font = cFont;
			cLeftPaddleInputBtn.FontSize = 15;
			cLeftPaddleInputBtn.FontColor = Color.Orange;
			cLeftPaddleInputBtn.SendMouseEvents = true;
			cLeftPaddleInputBtn.MouseEnter += PaddleInputBtnMouseEnter;
			cLeftPaddleInputBtn.MouseLeave += PaddleInputBtnMouseLeave;
			cLeftPaddleInputBtn.MouseUp += LeftPaddleInputBtnCLick;

			cRightPaddleInputBtn = new Button(cGraphDevMgr.GraphicsDevice, TextureImage, 0, cGraphDevMgr.GraphicsDevice.Viewport.Width - 200, 50, 200);
			cRightPaddleInputBtn.Visible = true;
			cRightPaddleInputBtn.Font = cFont;
			cRightPaddleInputBtn.FontSize = 15;
			cRightPaddleInputBtn.FontColor = Color.LightBlue;
			cRightPaddleInputBtn.SendMouseEvents = true;
			cRightPaddleInputBtn.MouseEnter += PaddleInputBtnMouseEnter;
			cRightPaddleInputBtn.MouseLeave += PaddleInputBtnMouseLeave;
			cRightPaddleInputBtn.MouseUp += RightPaddleInputBtnCLick;

			cScoreLbl = new Button(cGraphDevMgr.GraphicsDevice, TextureImage, 0, cLeftPaddleInputBtn.Top + cLeftPaddleInputBtn.Width, 50, cGraphDevMgr.GraphicsDevice.Viewport.Width - cLeftPaddleInputBtn.Width - cRightPaddleInputBtn.Width);
			cScoreLbl.Text = "0 - 0";
			cScoreLbl.Font = cFont;
			cScoreLbl.FontSize = 20;
			cScoreLbl.FontColor = Color.AntiqueWhite;
			cScoreLbl.Visible = true;

			//Create the boundary bars (0 = top, 1 = bottom, 2&3 could be left and right?)
			for (Ctr = 0; Ctr < cBoundaryBars.Length; Ctr++) {
				cBoundaryBars[Ctr] = new GameObject(cGraphDevMgr.GraphicsDevice, 10, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width);
				cBoundaryBars[Ctr].Left = 0;
				cBoundaryBars[Ctr].Height = 20;
				cBoundaryBars[Ctr].BackgroundColor = Color.White;
				cBoundaryBars[Ctr].Visible = true;

				NewCollide.Type = CollideType.Rectangle;
				NewCollide.Origin = cBoundaryBars[Ctr].GetCenterCoordinates();
				NewCollide.RectOffsets.X = (int)(NewCollide.Origin.X * -1);
				NewCollide.RectOffsets.Y = -1 * (cBoundaryBars[Ctr].Height / 2);
				NewCollide.RectOffsets.Width = cBoundaryBars[Ctr].Width;
				NewCollide.RectOffsets.Height = cBoundaryBars[Ctr].Height;

				cBoundaryBars[Ctr].AddCollisionRegion(NewCollide);
			}
			
			cBoundaryBars[0].Top = 50;
			cBoundaryBars[1].Top = cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height - cBoundaryBars[1].Height;

			//Create the ball
			cBall = new GameObject(cGraphDevMgr.GraphicsDevice, 75);
			cBall.Top = (int)((cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2) - cBall.GetCenterCoordinates().Y);
			cBall.Left = (int)((cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width / 2) - cBall.GetCenterCoordinates().X);
			cBall.Visible = true;
			cBall.Background = cTextureDict[Textures.Circle];

			NewCollide.Type = CollideType.Circle;
			NewCollide.Origin = cBall.GetCenterCoordinates();
			NewCollide.Radius = cBall.Width / 2;

			cBall.AddCollisionRegion(NewCollide);

			//Create the paddles
			cLeftPaddle = new GameObject(cGraphDevMgr.GraphicsDevice, 150, 20);
			cLeftPaddle.Top = (cGraphDevMgr.GraphicsDevice.Viewport.Height / 2) - (cLeftPaddle.Height / 2);
			cLeftPaddle.Left = 20;
			cLeftPaddle.Visible = true;
			cLeftPaddle.BackgroundColor = Color.Orange;

			NewCollide.Type = CollideType.Rectangle;
			NewCollide.Origin = cLeftPaddle.GetCenterCoordinates();
			NewCollide.RectOffsets.X = -1 * cLeftPaddle.Width / 2;
			NewCollide.RectOffsets.Y = -1 * cLeftPaddle.Height / 2;
			NewCollide.RectOffsets.Width = cLeftPaddle.Width;
			NewCollide.RectOffsets.Height = cLeftPaddle.Height;
			cLeftPaddle.AddCollisionRegion(NewCollide);
			
			cRightPaddle = new GameObject(cGraphDevMgr.GraphicsDevice, 150, 20);
			cRightPaddle.Top = (cGraphDevMgr.GraphicsDevice.Viewport.Height / 2) - (cRightPaddle.Height / 2);
			cRightPaddle.Left = cGraphDevMgr.GraphicsDevice.Viewport.Width - cRightPaddle.Width - 20;
			cRightPaddle.Visible = true;
			cRightPaddle.BackgroundColor = Color.LightBlue;

			NewCollide.Type = CollideType.Rectangle;
			NewCollide.Origin = cRightPaddle.GetCenterCoordinates();
			NewCollide.RectOffsets.X = -1 * cRightPaddle.Width / 2;
			NewCollide.RectOffsets.Y = -1 * cRightPaddle.Height / 2;
			NewCollide.RectOffsets.Width = cRightPaddle.Width;
			NewCollide.RectOffsets.Height = cRightPaddle.Height;
			cRightPaddle.AddCollisionRegion(NewCollide);
		}

		protected override void Update(GameTime gameTime) {
			KeyboardState CurrKeys = Keyboard.GetState();
			MouseState CurrMouse = Mouse.GetState();
			GamePadState CurrPad = GamePad.GetState(PlayerIndex.One);
			int nHeightDiff;

			int Ctr;

			//Process keyboard input
			if ((CurrKeys.IsKeyDown(Keys.OemTilde) == true) && (cPriorKeyState.IsKeyDown(Keys.OemTilde) == false)) {
				cDevConsole.ToggleVisible();
			}

			if (CurrKeys.IsKeyDown(Keys.W) == true) {
				if (cLeftPaddleControl == PaddleInput.KeysWASD) {
					MoveRightPaddle(-1 * PADDLE_MAXSPEED);
				}

				if (cRightPaddleControl == PaddleInput.KeysWASD) {
					MoveRightPaddle(-1 * PADDLE_MAXSPEED);
				}
			}

			if (CurrKeys.IsKeyDown(Keys.S) == true) {
				if (cLeftPaddleControl == PaddleInput.KeysWASD) {
					MoveRightPaddle(PADDLE_MAXSPEED);
				}

				if (cRightPaddleControl == PaddleInput.KeysWASD) {
					MoveRightPaddle(PADDLE_MAXSPEED);
				}
			}

			if (CurrKeys.IsKeyDown(Keys.Up) == true) {
				if (cLeftPaddleControl == PaddleInput.KeysArrows) {
					MoveLeftPaddle(-1 * PADDLE_MAXSPEED);
				}

				if (cRightPaddleControl == PaddleInput.KeysArrows) {
					MoveRightPaddle(-1 * PADDLE_MAXSPEED);
				}
			}

			if (CurrKeys.IsKeyDown(Keys.Down) == true) {
				if (cLeftPaddleControl == PaddleInput.KeysArrows) {
					MoveLeftPaddle(PADDLE_MAXSPEED);
				}

				if (cRightPaddleControl == PaddleInput.KeysArrows) {
					MoveRightPaddle(PADDLE_MAXSPEED);
				}
			}

			//Process mouse input
			if (cLeftPaddleControl == PaddleInput.Mouse) {
				nHeightDiff = (int)(CurrMouse.Position.Y - cLeftPaddle.GetCenterCoordinates().Y);

				MoveLeftPaddle(nHeightDiff);
            }

			if (cRightPaddleControl == PaddleInput.Mouse) {
				nHeightDiff = (int)(CurrMouse.Position.Y - cRightPaddle.GetCenterCoordinates().Y);

				MoveRightPaddle(nHeightDiff);
			}

			//Process gamepad input
			if (CurrPad.IsConnected == true) {
				if (CurrPad.IsButtonDown(Buttons.DPadUp) == true) {
					if (cLeftPaddleControl == PaddleInput.PadDPad) {
						MoveLeftPaddle(-1 * PADDLE_MAXSPEED);
					}

					if (cRightPaddleControl == PaddleInput.PadDPad) {
						MoveRightPaddle(-1 * PADDLE_MAXSPEED);
					}
				}

				if (CurrPad.IsButtonDown(Buttons.DPadDown) == true) {
					if (cLeftPaddleControl == PaddleInput.PadDPad) {
						MoveLeftPaddle(PADDLE_MAXSPEED);
					}

					if (cRightPaddleControl == PaddleInput.PadDPad) {
						MoveRightPaddle(PADDLE_MAXSPEED);
					}
				}

				if (cLeftPaddleControl == PaddleInput.PadLStick) {
					nHeightDiff = (int)(PADDLE_MAXSPEED * CurrPad.ThumbSticks.Left.Y); //Thumbsticks give values of -1 to 1

					MoveLeftPaddle(nHeightDiff);
                }

				if (cRightPaddleControl == PaddleInput.PadLStick) {
					nHeightDiff = (int)(PADDLE_MAXSPEED * CurrPad.ThumbSticks.Left.Y); //Thumbsticks give values of -1 to 1

					MoveRightPaddle(nHeightDiff);
				}

				if (cLeftPaddleControl == PaddleInput.PadRStick) {
					nHeightDiff = (int)(PADDLE_MAXSPEED * CurrPad.ThumbSticks.Right.Y); //Thumbsticks give values of -1 to 1

					MoveLeftPaddle(nHeightDiff);
				}

				if (cRightPaddleControl == PaddleInput.PadRStick) {
					nHeightDiff = (int)(PADDLE_MAXSPEED * CurrPad.ThumbSticks.Right.Y); //Thumbsticks give values of -1 to 1

					MoveRightPaddle(nHeightDiff);
				}
			}

			//Computer controlled paddle
			if (cLeftPaddleControl == PaddleInput.Computer) {
				nHeightDiff = (int)(cBall.GetCenterCoordinates().Y - cLeftPaddle.GetCenterCoordinates().Y);

				MoveLeftPaddle(nHeightDiff);
			}

			if (cRightPaddleControl == PaddleInput.Computer) {
				nHeightDiff = (int)(cBall.GetCenterCoordinates().Y - cRightPaddle.GetCenterCoordinates().Y);

				MoveRightPaddle(nHeightDiff);
			}

			//Allow all interface objects to update
			cDevConsole.Update(gameTime, CurrKeys, CurrMouse);

			for (Ctr = 0; Ctr < cBoundaryBars.Length; Ctr++) {
				cBoundaryBars[Ctr].Update(gameTime);
			}

			cBall.Update(gameTime);
			MoveBall();

			cLeftPaddle.Update(gameTime);
			cRightPaddle.Update(gameTime);

			cLeftPaddleInputBtn.Text = EnumTools.GetEnumDescriptionAttribute(cLeftPaddleControl);
			cLeftPaddleInputBtn.Update(gameTime, CurrKeys, CurrMouse);

			cRightPaddleInputBtn.Text = EnumTools.GetEnumDescriptionAttribute(cRightPaddleControl);
			cRightPaddleInputBtn.Update(gameTime, CurrKeys, CurrMouse);

			cScoreLbl.Update(gameTime);

			//Update input tracking values
			cPriorKeyState = CurrKeys;
			cPriorMouseState = CurrMouse;
			cPriorPadState = CurrPad;

			//Use monogame update
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			int Ctr;

			//Clear the current screen buffer
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);
			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			//Draw all interface objects
			cDevConsole.Draw(cDrawBatch);

			for (Ctr = 0; Ctr < cBoundaryBars.Length; Ctr++) {
				cBoundaryBars[Ctr].Draw();
			}

			cBall.Draw(cDrawBatch);
			cLeftPaddle.Draw(cDrawBatch);
			cRightPaddle.Draw(cDrawBatch);

			cLeftPaddleInputBtn.Draw(cDrawBatch);
			cRightPaddleInputBtn.Draw(cDrawBatch);
			cScoreLbl.Draw(cDrawBatch);

			cDrawBatch.End();

			//Use monogame draw
			base.Draw(gameTime);
		}

		/// <summary>
		/// Function that will update the position of the ball on the screen
		/// </summary>
		private void MoveBall() {
			int Angle;
			Vector BallSpeed = new Vector();
			float nPaddlePercent;
			bool bResetBall = false;

			if (cBallSpeed.X == 0) {
				cBallSpeed.X = 5;
			}

			if (cBallSpeed.Y == 0) {
				cBallSpeed.Y = 5;
			}

			//Adjust the ball's position
			cBall.Top += (int)(cBallSpeed.Y);
			cBall.Left += (int)(cBallSpeed.X);

			if (cBall.TestCollision(cBoundaryBars[0].GetCollisionRegions()) == true)  {
				cBallSpeed.Y *= -1;
			}

			if (cBall.TestCollision(cBoundaryBars[1].GetCollisionRegions()) == true) {
				cBallSpeed.Y *= -1;
			}

			if ((cBall.TestCollision(cLeftPaddle.GetCollisionRegions()) == true) && (cBallSpeed.X < 0)) {
				cBallSpeedMag += BALL_VOLLEYSPEEDINCREASE; //Every bounce increase speed a little

				cBallSpeed.X *= -1;

				//Convert to ratio using top of paddle as the reference/zero point
				nPaddlePercent = (cBall.GetCenterCoordinates().Y - cLeftPaddle.Top);
				if (nPaddlePercent < 0) {
					nPaddlePercent = 0;
				} else if (nPaddlePercent > cLeftPaddle.Height) {
					nPaddlePercent = cLeftPaddle.Height;
				}

				nPaddlePercent /= cLeftPaddle.Height; //Now we have the percentage across the whole paddle

				nPaddlePercent *= -2 * PADDLE_MAXANGLECHANGE;
				nPaddlePercent += PADDLE_MAXANGLECHANGE;

				BallSpeed.SetRectangularCoordinates(cBallSpeed.X, cBallSpeed.Y);
				BallSpeed.MultiplyPolarVector(1, nPaddlePercent);

				if (BallSpeed.Polar.Length < cBallSpeedMag) {
					BallSpeed.SetPolarCoordinates(cBallSpeedMag, BallSpeed.Polar.Angle);
					cDevConsole.AddText("Ball Speed: " + cBallSpeedMag.ToString());
				}

				cBallSpeed.X = BallSpeed.RectangularInt.Real;
				cBallSpeed.Y = BallSpeed.RectangularInt.Imaginary;
			}

			if ((cBall.TestCollision(cRightPaddle.GetCollisionRegions()) == true) && (cBallSpeed.X > 0)) {
				cBallSpeedMag += BALL_VOLLEYSPEEDINCREASE;  //Every bounce increase speed a little

				cBallSpeed.X *= -1;

				//Convert to ratio using top of paddle as the reference/zero point
				nPaddlePercent = (cBall.GetCenterCoordinates().Y - cRightPaddle.Top);
				if (nPaddlePercent < 0) {
					nPaddlePercent = 0;
				} else if (nPaddlePercent > cRightPaddle.Height) {
					nPaddlePercent = cRightPaddle.Height;
				}

				nPaddlePercent /= cRightPaddle.Height; //Now we have the percentage across the whole paddle

				nPaddlePercent *= -2 * PADDLE_MAXANGLECHANGE;
				nPaddlePercent += PADDLE_MAXANGLECHANGE;

				BallSpeed.SetRectangularCoordinates(cBallSpeed.X, cBallSpeed.Y);
				BallSpeed.MultiplyPolarVector(1, nPaddlePercent);

				if (BallSpeed.Polar.Length < cBallSpeedMag) {
					BallSpeed.SetPolarCoordinates(cBallSpeedMag, BallSpeed.Polar.Angle);
					cDevConsole.AddText("Ball Speed: " + cBallSpeedMag.ToString());
				}

				cBallSpeed.X = BallSpeed.RectangularInt.Real;
				cBallSpeed.Y = BallSpeed.RectangularInt.Imaginary;
			}

			if (cBall.Left + cBall.Width <= 0) { //Ball has gone off the left side of the screen
				cScoreRight += 1;
				bResetBall = true;
			}

			if (cBall.Left >= cGraphDevMgr.GraphicsDevice.Viewport.Width) { //Ball has gone off right side of screen
				cScoreLeft += 1;
				bResetBall = true;
				
			}

			if (bResetBall == true) {
				Angle = cRandom.Next(-35, 55); //Choose a random starting direction (boundaries are degrees)
				if (cRandom.Next(1, 10) > 5) { //Randomly pick if it travels left or right
					Angle += 180;
				}

				//Calculate the horizontal and vertical speeds for that direction
				BallSpeed.SetPolarCoordinates(BALL_MINSPEED, Angle);
				cBallSpeed.X = BallSpeed.RectangularInt.Real;
				cBallSpeed.Y = BallSpeed.RectangularInt.Imaginary;

				//Reset the balls position to the start of the screen
				cBall.Top = cGraphDevMgr.GraphicsDevice.Viewport.Height / 2;
				cBall.Left = cGraphDevMgr.GraphicsDevice.Viewport.Width / 2;

				//Update the score
				cScoreLbl.Text = String.Format("{0} - {1}", cScoreLeft, cScoreRight);

				cBallSpeedMag = BALL_MINSPEED;
            }
		}

		private void MoveLeftPaddle(int Distance) {
			if (Distance > PADDLE_MAXSPEED) {
				Distance = PADDLE_MAXSPEED;
			}

			if (Distance < -1 * PADDLE_MAXSPEED) {
				Distance = -1 * PADDLE_MAXSPEED;
			}

			cLeftPaddle.Top += Distance;

			//Verify that the paddle isn't going out of bounds
			if (cLeftPaddle.Top < cBoundaryBars[0].Top + cBoundaryBars[0].Height) {
				cLeftPaddle.Top = cBoundaryBars[0].Top + cBoundaryBars[0].Height;
            }

			if (cLeftPaddle.Top + cLeftPaddle.Height > cBoundaryBars[1].Top) {
				cLeftPaddle.Top = cBoundaryBars[1].Top - cLeftPaddle.Height;
			}
		}

		private void MoveRightPaddle(int Distance) {
			if (Distance > PADDLE_MAXSPEED) {
				Distance = PADDLE_MAXSPEED;
			}

			if (Distance < -1 * PADDLE_MAXSPEED) {
				Distance = -1 * PADDLE_MAXSPEED;
			}

			cRightPaddle.Top += Distance;

			//Verify that the paddle isn't going out of bounds
			if (cRightPaddle.Top < cBoundaryBars[0].Top + cBoundaryBars[0].Height) {
				cRightPaddle.Top = cBoundaryBars[0].Top + cBoundaryBars[0].Height;
			}

			if (cRightPaddle.Top + cRightPaddle.Height > cBoundaryBars[1].Top) {
				cRightPaddle.Top = cBoundaryBars[1].Top - cRightPaddle.Height;
			}
		}

		private void PaddleInputBtnMouseEnter(object Sender, MouseState CurrMouse) {
			Button CurrInputBtn = (Button)Sender;

			CurrInputBtn.BackgroundColor = Color.DimGray;
		}

		private void PaddleInputBtnMouseLeave(object Sender, MouseState CurrMouse) {
			Button CurrInputBtn = (Button)Sender;

			CurrInputBtn.BackgroundColor = Color.Transparent;
		}

		private void LeftPaddleInputBtnCLick(object Sender, MouseButton ButtonDown, MouseState CurrMouse) {
			if (ButtonDown == MouseButton.Left) { //Cycle through the available control schemes
				cLeftPaddleControl += 1;
				if (cLeftPaddleControl == PaddleInput.EndOfList) {
					cLeftPaddleControl = 0;
				}
			}
		}

		private void RightPaddleInputBtnCLick(object Sender, MouseButton ButtonDown, MouseState CurrMouse) {
			if (ButtonDown == MouseButton.Left) { //Cycle through the available control schemes
				cRightPaddleControl += 1;
				if (cRightPaddleControl == PaddleInput.EndOfList) {
					cRightPaddleControl = 0;
				}
			}
		}

		/// <summary>
		/// Handler for commands entered through the console
		/// </summary>
		/// <param name="Sender">Object raising the event</param>
		/// <param name="Command">Text command entered by the user</param>
		private void ConsoleCommandHandler(object Sender, string Command) {
			if (Tools.RegEx.QuickTest(Command, "^(quit|exit)$") == true) {
				Exit();
			} else {
				cDevConsole.AddText("Unrecognized command: " + Command);
			}
		}

		protected enum Textures {
			[Description("Font.png")]
			Font,
			[Description("Circle.png")]
			Circle,
		}

		protected enum PaddleInput {
			[Description("Arrow Keys")]
			KeysArrows,
			[Description("WASD Keys")]
			KeysWASD,
			[Description("Mouse")]
			Mouse,
			[Description("D-Pad")]
			PadDPad,
			[Description("L-Stick")]
			PadLStick,
			[Description("R-Stick")]
			PadRStick,
			[Description("Computer")]
			Computer,
			[Description("Invalid!")]
			EndOfList
		}
	}
}
