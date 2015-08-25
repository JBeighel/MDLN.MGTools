using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MDLN.MGTools;
using MDLN.Cards;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

namespace MGTest
{
	public static class Launcher
	{
		static void Main()
		{
			using (var game = new MGTest())
			{
				game.Run();
			}
		}
	}

	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class MGTest : Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Texture2D background, ConsoleRect, CardImage, CardBack, Assassin, Archer;
		private Texture2D cShip;
		private ShipDirection cShipDir;
		private bool cShipUseKeys, cShipShow;
		private int Transparency;
		private KeyboardState PriorKeyState;
		private MouseState PriorMouseState;
		private MDLN.MGTools.Console DevConsole;
		private Container TestCont;
		private TextureFont cFont;
		private bool cMouseOverCard;
		private Card TestCard;
		private double cMouseEnterTime;

		private int FrameNum;

		public MGTest()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Initializes the game class
		/// </summary>
		protected override void Initialize() {
			IsMouseVisible = true;

			FrameNum = 1;

			DevConsole = new MDLN.MGTools.Console(GraphicsDevice, Content, "Font.png", 0, 0, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height / 2);
			DevConsole.CommandSent += CommandSentEventHandler;
			DevConsole.OpenEffect = DisplayEffect.SlideDown;
			DevConsole.CloseEffect = DisplayEffect.SlideUp;

			cMouseEnterTime = -1;

			//Initializes monogame
			base.Initialize();
		}

		/// <summary>
		/// Called once at start to load content
		/// </summary>
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			background = Content.Load<Texture2D>("DiceTexture.png");
			CardImage = Content.Load<Texture2D>("Fighter.png");
			CardBack = Content.Load<Texture2D>("CardBack.png");
			Assassin = Content.Load<Texture2D>("Assassin.png");
			Archer = Content.Load<Texture2D>("Archer.png");
			cShip = Content.Load<Texture2D>("Ship.png");

			cShipDir = ShipDirection.Straight;
			cShipUseKeys = true;
			cShipShow = false;

			ConsoleRect = new Texture2D(graphics.GraphicsDevice, 1, 1);
			ConsoleRect.SetData(new[] { Color.DarkGray });

			cFont = new TextureFont(Content.Load<Texture2D>("Font.png"));

			TestCont = new Container(GraphicsDevice, background, 20, 250, 100, 200);
			TestCont.OpenEffect = DisplayEffect.SlideRight;
			TestCont.CloseEffect = DisplayEffect.SlideLeft;
			TestCont.EffectDuration = 500;

			TestCard = new Card(GraphicsDevice, 350, 250, cFont);
			TestCard.Top = 105;
			TestCard.Left = 325;
			TestCard.CardBase = Content.Load<Texture2D>("CardBase.png");
			TestCard.CardImage = CardImage;
			TestCard.Title = "Shield Maiden";

			List<string> Lines = new List<string>();
			Lines.Add("Health: 5");
			Lines.Add("Attack: 3");
			Lines.Add("");
			Lines.Add("Women of Rohan");
			TestCard.DescriptionLines = Lines;
			TestCard.SendMouseEvents = true;
			TestCard.MouseDown += new ContainerMouseDownEventHandler(MouseLeftDown);

			DevConsole.AddText("Viewport Bounds: X=" + GraphicsDevice.Viewport.Bounds.X + " Y=" + GraphicsDevice.Viewport.Bounds.Y + " Width=" + GraphicsDevice.Viewport.Bounds.Width + " Height=" + GraphicsDevice.Viewport.Bounds.Height);
		}

		/// <summary>
		/// Called once to unload content not in ContentManager
		/// </summary>
		protected override void UnloadContent() {
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Game logic function.  Update world, handle input, etc
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
			int Seconds = gameTime.TotalGameTime.Seconds;
			KeyboardState KeyState = Keyboard.GetState();
			MouseState CurrMouse = Mouse.GetState();

			if ((PriorKeyState.IsKeyDown(Keys.OemTilde) == false) && (KeyState.IsKeyDown(Keys.OemTilde) == true)) {
				DevConsole.ToggleVisible();
			}

			DevConsole.Update(gameTime, Keyboard.GetState(), Mouse.GetState());

			if ((PriorMouseState.LeftButton == ButtonState.Released) && (CurrMouse.LeftButton == ButtonState.Pressed)) {
				DevConsole.AddText("Left mouse clicked, X=" + CurrMouse.X + " Y=" + CurrMouse.Y);
			}

			if (new Rectangle(10, 10, CardBack.Width, CardBack.Height).Contains(CurrMouse.X, CurrMouse.Y)) {
				if (cMouseOverCard == false) {
					DevConsole.AddText("Mouse entered card");
					cMouseEnterTime = gameTime.TotalGameTime.TotalMilliseconds;
					cMouseOverCard = true;
					TestCont.Visible = true;
				}
			} else {
				if (cMouseOverCard == true) {
					DevConsole.AddText("Mouse exited card");
					cMouseOverCard = false;
					TestCont.Visible = false;
				}
			}

			if (cMouseEnterTime != -1) {
				if (gameTime.TotalGameTime.TotalMilliseconds - cMouseEnterTime >= 500) {
					DevConsole.AddText("Mouse over 500 ms elapsed");
					cMouseEnterTime = -1;
				}
			}

			TestCont.Update(gameTime);

			PriorMouseState = CurrMouse;
			PriorKeyState = KeyState;

			FrameNum = (Seconds % 6) + 1;

			if (gameTime.TotalGameTime.Milliseconds <= 500) {
				Transparency = gameTime.TotalGameTime.Milliseconds / 2;
			} else {
				Transparency = 255 - ((gameTime.TotalGameTime.Milliseconds - 500) / 2);
			}

			if (cShipUseKeys == true) {
				if (KeyState.IsKeyDown(Keys.Left) == true) {
					cShipDir = ShipDirection.Left;
				} else if (KeyState.IsKeyDown(Keys.Right) == true) {
					cShipDir = ShipDirection.Right;
				} else {
					cShipDir = ShipDirection.Straight;
				}
			} else {
				if (CurrMouse.Position.X < 300) {
					cShipDir = ShipDirection.Left;
				} else if (CurrMouse.Position.X > 500) {
					cShipDir = ShipDirection.Right;
				} else {
					cShipDir = ShipDirection.Straight;
				}
			}

			TestCard.Update(gameTime);

			//Use monogame update
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			Color Overlay = new Color(255, 255, 255, Transparency);

			GraphicsDevice.Clear(Color.Blue);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

			switch (FrameNum) {
				case 1 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(0, 0, 114, 114), Overlay);
					break;
				case 2 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(115, 0, 114, 114), Overlay);
					break;
				case 3 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(230, 0, 114, 114), Overlay);
					break;
				case 4 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(345, 0, 114, 114), Overlay);
					break;
				case 5 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(0, 115, 114, 114), Overlay);
					break;
				case 6 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(115, 115, 114, 114), Overlay);
					break;
			}
			
			spriteBatch.Draw(CardBack, new Rectangle(10, 10, CardBack.Width, CardBack.Height), Color.White);

			spriteBatch.Draw(CardImage, new Rectangle(20, 40, 14 * 16, 195 - 40 - 10), Color.White);

			cFont.WriteText(spriteBatch, "Card Name", 20, 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Card Details 1", 195, 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Line 2", 195 + cFont.CharacterHeight, 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Line 3", 195 + (cFont.CharacterHeight * 2), 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Line 4", 195 + (cFont.CharacterHeight * 3), 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Line 5", 195 + (cFont.CharacterHeight * 4), 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Line 6", 195 + (cFont.CharacterHeight * 5), 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Line 7", 195 + (cFont.CharacterHeight * 6), 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Line 8", 195 + (cFont.CharacterHeight * 7), 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Line 9", 195 + (cFont.CharacterHeight * 8), 20, Color.DarkBlue);
			cFont.WriteText(spriteBatch, "Line 10", 195 + (cFont.CharacterHeight * 9), 20, Color.DarkBlue);

			cFont.WriteText (spriteBatch, "Small Text", 5, 50, 300, Color.OrangeRed);
			cFont.WriteText(spriteBatch, "Big Text", 40, 60, 300, Color.OrangeRed);

			if (cShipShow == true) {
				if (cShipDir == ShipDirection.Straight) {
					spriteBatch.Draw(cShip, new Vector2(300f, 200f), new Rectangle(0, 0, 200, 200), Color.White);
				} else if (cShipDir == ShipDirection.Left) {
					spriteBatch.Draw(cShip, new Vector2(300f, 200f), null, new Rectangle(200, 0, 200, 200), null, 0, null, Color.White, SpriteEffects.FlipHorizontally, 0);
				} else { //right
					spriteBatch.Draw(cShip, new Vector2(300f, 200f), new Rectangle(200, 0, 200, 200), Color.White);
				}
			}

			spriteBatch.End();

			TestCard.Draw();
			TestCont.Draw();
			DevConsole.Draw();

			base.Draw(gameTime);
		}

		protected void CommandSentEventHandler(object Sender, string Command) {
			Match rxResult;
			string Name, Value;

			if (Command.ToLower().CompareTo("exit") == 0) {
				this.Exit();
			} else if (Command.ToLower().CompareTo("clear") == 0) {
				DevConsole.ClearText();
			} else if ((Command.Length >= 4) && (Command.Substring(0, 4).ToLower().CompareTo("card") == 0)) { //Commands for the test card
				rxResult = Regex.Match(Command, @"^card\s+([A-Z0-9]+) ?= ?([A-Z0-9 ]+)$", RegexOptions.IgnoreCase);

				if (rxResult.Success == true) {
					Name = rxResult.Groups[1].Value.ToLower();
					Value = rxResult.Groups[2].Value;

					switch (Name) {
						case "visible":
							if (Regex.Match(Value, @"^[teoy1]", RegexOptions.IgnoreCase).Success == true) {
								TestCard.Visible = true;
							} else {
								TestCard.Visible = false;
							}
							DevConsole.AddText("Card visibility update");
							return;
						case "effect":
							if (Value.ToLower().CompareTo("zoom") == 0) {
								TestCard.OpenEffect = DisplayEffect.Zoom;
								TestCard.CloseEffect = DisplayEffect.Zoom;
								DevConsole.AddText("Card effect set to zoom");
								return;
							} else if (Value.ToLower().CompareTo("fade") == 0) {
								TestCard.OpenEffect = DisplayEffect.Fade;
								TestCard.CloseEffect = DisplayEffect.Fade;
								DevConsole.AddText("Card effect set to fade");
								return;
							} else if (Value.ToLower().CompareTo("none") == 0) {
								TestCard.OpenEffect = DisplayEffect.None;
								TestCard.CloseEffect = DisplayEffect.None;
								DevConsole.AddText("Card effect set to none");
								return;
							}
							break;
						case "image":
							if (Value.ToLower().CompareTo("assassin") == 0) {
								TestCard.CardImage = Assassin;
								DevConsole.AddText("Card image set");
								return;
							} else if (Value.ToLower().CompareTo("fighter") == 0) {
								TestCard.CardImage = CardImage;
								DevConsole.AddText("Card image set");
								return;
							} else if (Value.ToLower().CompareTo("archer") == 0) {
								TestCard.CardImage = Archer;
								DevConsole.AddText("Card image set");
								return;
							}
							break;
						case "title":
							TestCard.Title = Value;
							DevConsole.AddText("Card title set");
							return;
						case "anim":
							try {
								TestCard.EffectDuration = int.Parse(Value);
								DevConsole.AddText("Animation time set");
							} catch (Exception) {
								DevConsole.AddText("Invalid card animation time");
							}
							return;
						case "full":
							if (Regex.Match(Value, @"^[teoy1]", RegexOptions.IgnoreCase).Success == true) {
								TestCard.ShowFullCard = true;
							} else {
								TestCard.ShowFullCard = false;
							}
							DevConsole.AddText("Card visibility update");
							return;
					}

					DevConsole.AddText("Invalid Card value name: " + Command);
				} else {
					DevConsole.AddText("Invalid Card command format: " + Command);
				}
			} else if ((Command.Length >= 4) && (Command.Substring(0, 4).ToLower().CompareTo("ship") == 0)) { //Commands for the test ship
				rxResult = Regex.Match(Command, @"^ship\s+([A-Z0-9]+) ?= ?([A-Z0-9 ]+)$", RegexOptions.IgnoreCase);

				if (rxResult.Success == true) {
					Name = rxResult.Groups[1].Value.ToLower();
					Value = rxResult.Groups[2].Value;

					switch (Name) {
						case "visible":
							if (Regex.Match(Value, @"^[teoy1]", RegexOptions.IgnoreCase).Success == true) {
								cShipShow = true;
							} else {
								cShipShow = false;
							}
							DevConsole.AddText("Ship visibility update");
							return;
						case "control":
							if (Value.ToLower().CompareTo("mouse") == 0) {
								cShipUseKeys = false;
								DevConsole.AddText("Ship control set to mosue");
								return;
							} else if (Value.ToLower().CompareTo("keys") == 0) {
								cShipUseKeys = true;
								DevConsole.AddText("Ship control set to keyboard");
								return;
							}

							DevConsole.AddText("Invalid Ship control value name: " + Value);
							return;
					}

					DevConsole.AddText("Invalid Ship value name: " + Command);
				} else {
					DevConsole.AddText("Invalid Shpi command format: " + Command);
				}
			} else {
				DevConsole.AddText("Unrecognized Command: " + Command + "\n ");
			}
		}

		protected void MouseLeftDown(object Sender, MouseButton CurrDown, MouseState CurrMouse) {
			if (CurrDown == MouseButton.Left) {
				DevConsole.AddText("Left mouse button down on card");
			}

			if (CurrDown == MouseButton.Right) {
				DevConsole.AddText("Right mouse button down on card");
			}
		}
	}

	public enum ShipDirection {
		Left, Straight, Right
	}
}