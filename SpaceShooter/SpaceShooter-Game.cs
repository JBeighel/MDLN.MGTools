using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MDLN.MGTools;
using MDLN.Tools;

namespace MDLN.SpaceShooter
{
	/// <summary>
	/// This game is for experimenting with AI and movement.  Seeing how to have ships
	/// moving around reacting to the player as well as other ships.
	/// 
	/// Collision detection will need adapted to discover obstructions then some decision 
	/// making algorithm will need to move around them.
	/// </summary>
	class SpaceShooter : Game {
		/// <summary>
		/// Subdirectory that will contain all external content
		/// </summary>
		private const string INTERFACECONTENTDIR = @"Content\";

		/// <summary>
		/// Graphics Device Manager used to initialize the graphics device
		/// </summary>
		private GraphicsDeviceManager cGraphDevMgr;
		/// <summary>
		/// Object to display and manage the game console
		/// </summary>
		private GameConsole cDevConsole;
		private TextureAtlas cTextureAtlas;
		private MouseState cPriorMouse;
		private float cnRotation;

		private PhysicalObject cShip;

		public SpaceShooter() {
			cGraphDevMgr = new GraphicsDeviceManager(this);

			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += FormResizeHandler;

			cnRotation = 0;

			return;
		}

		private void FormResizeHandler(object Sender, EventArgs Args) {
			cDevConsole.Width = Window.ClientBounds.Width;

			return;
		}

		/// <summary>
		/// Handler to interpret and execute commands entered in the console
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="CommandEvent"></param>
		private void ConsoleCommandHandler(object Sender, string CommandEvent) {
			if (RegEx.LooseTest(CommandEvent, @"^\s*(quit|exit|close)\s*$") == true) {
				Exit();
			} else {
				cDevConsole.AddText("Unrecognized command: " + CommandEvent);
			}
		}

		/// <summary>
		/// Place to initialize and prepare MonoGame objects
		/// </summary>
		protected override void Initialize() {
			
			//Initializes monogame
			base.Initialize();

			return;
		}

		/// <summary>
		/// Called when application is shutting down so that unmanaged resources can be released.
		/// </summary>
		protected override void UnloadContent() {

			//Let monogame unload its stuff
			base.UnloadContent();
		}

		/// <summary>
		/// Load all external content files that are needed
		/// </summary>
		protected override void LoadContent() {
			//The font texture isn't needed, but this is an example of loading a PNG
			string strFileName = INTERFACECONTENTDIR + "Font.png";
			FileStream FileLoad = new FileStream(strFileName, FileMode.Open);
			Texture2D FontTexture = Texture2D.FromStream(GraphicsDevice, FileLoad);
			FileLoad.Close();

			try {
				cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, INTERFACECONTENTDIR + "Font.png", GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height / 2);
				cDevConsole.AccessKey = Keys.OemTilde;
				cDevConsole.UseAccessKey = true;
				cDevConsole.OpenEffect = DisplayEffect.SlideDown;
				cDevConsole.CloseEffect = DisplayEffect.SlideUp;
				cDevConsole.CommandSent += new CommandSentEventHandler(ConsoleCommandHandler);
			} catch (Exception ExErr) {
				System.Windows.Forms.MessageBox.Show("Failed to initialize console: " + ExErr.GetType().ToString() + " - " + ExErr.Message);
				Exit();
				return;
			}

			cTextureAtlas = new TextureAtlas(cGraphDevMgr.GraphicsDevice, INTERFACECONTENTDIR + "spaceShooter2_spritesheet.png", INTERFACECONTENTDIR + "spaceShooter2_spritesheet.xml");

			cShip = new PhysicalObject(cGraphDevMgr.GraphicsDevice, cTextureAtlas) {
				TextureName = "spaceShips_001.png",
				TextureRotation = (float)(90 * Math.PI / 180),
				DrawRegion = new Rectangle() {
					X = 0,
					Y = 0,
					Width = 256,
					Height = 256,
				},
			};

			List<Vector2> VertList = new List<Vector2>();
			VertList.Add(new Vector2(100, 100));
			VertList.Add(new Vector2(100, 110));
			VertList.Add(new Vector2(100, 120));
			VertList.Add(new Vector2(100, 130));
			VertList.Add(new Vector2(100, 140));

			cShip.SetCollisionVertexes(VertList);

			return;
		}

		/// <summary>
		/// Update game state and all content.  Should be called once every redraw, possibly called 
		/// more often than the draws but only if it exceeds time between frame draws.
		/// </summary>
		/// <param name="gameTime">Current time information of the application</param>
		protected override void Update(GameTime gameTime) {
			MouseState Currmouse = Mouse.GetState();
			Vector vShipToMouse = new Vector();

			if (Currmouse.LeftButton == ButtonState.Pressed) {
				vShipToMouse.SetRectangularCoordinates(Currmouse.Position.X - 128, Currmouse.Position.Y - 128);
				cnRotation = (float)(vShipToMouse.Polar.Angle * Math.PI / 180);
			}

			cPriorMouse = Currmouse;

			cShip.Update(gameTime);
			cDevConsole.Update(gameTime);

			//Use monogame update
			base.Update(gameTime);
		}

		/// <summary>
		/// Called to actually render the game content to the backbuffer, which will be flipped with
		/// the currently displayed buffer to show the next frame.
		/// </summary>
		/// <param name="gameTime">Current time information of the application</param>
		protected override void Draw(GameTime gameTime) {
			SpriteBatch DrawBatch = new SpriteBatch(GraphicsDevice);

			GraphicsDevice.Clear(Color.Black);
			DrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			//cTextureAtlas.DrawTile("spaceShips_001.png", DrawBatch, new Rectangle(0, 0, 256, 256), Color.White, cnRotation);

			//cTextureAtlas.DrawTile("spaceShips_002.png", DrawBatch, new Rectangle(256, 0, 256, 256), Color.White);
			//cTextureAtlas.DrawTile("spaceShips_003.png", DrawBatch, new Rectangle(0, 256, 256, 256), Color.White);
			//cTextureAtlas.DrawTile("spaceShips_004.png", DrawBatch, new Rectangle(256, 256, 256, 256), Color.White);

			cShip.Draw(DrawBatch);

			//Always draw console last
			cDevConsole.Draw(DrawBatch);

			DrawBatch.End();

			//Use monogame draw
			base.Draw(gameTime);
		}

	}
}
