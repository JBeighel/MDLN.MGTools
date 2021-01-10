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
		private ObjectManager cObjManager;

		public SpaceShooter() {
			cGraphDevMgr = new GraphicsDeviceManager(this);

			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += FormResizeHandler;

			return;
		}

		private void FormResizeHandler(object Sender, EventArgs Args) {
			cDevConsole.Width = Window.ClientBounds.Width;
			cObjManager.UpdateGraphicsDevice(cGraphDevMgr.GraphicsDevice);

			return;
		}

		/// <summary>
		/// Handler to interpret and execute commands entered in the console
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="CommandEvent"></param>
		private void ConsoleCommandHandler(object Sender, string CommandEvent) {
			int nCtr;
			string strParam;

			if (RegEx.LooseTest(CommandEvent, @"^\s*(quit|exit|close)\s*$") == true) {
				Exit();
			} else if (RegEx.LooseTest(CommandEvent, @"edit\s*vertexes=(on|1|off|0)") == true) {
				if (RegEx.LooseTest(CommandEvent, @"edit\s*vertexes=(on|1)") == true) {
					cObjManager[0][0].AllowCollisionVertexEdits = true;
				} else {
					cObjManager[0][0].AllowCollisionVertexEdits = false;
				}
			} else if (RegEx.LooseTest(CommandEvent, @"list\s*vertexes") == true) {
				List<Vector2> VertList = new List<Vector2>(cObjManager[0][0].GetCollisionVertexes());
				nCtr = 0;
				foreach (Vector2 vVert in VertList) {
					cDevConsole.AddText(String.Format("{0}:X{1} Y{2}", nCtr, vVert.X, vVert.Y));
					nCtr += 1;
				}
			} else if (RegEx.LooseTest(CommandEvent, @"set\s*texture\s*=(.*)") == true) {
				strParam = RegEx.GetRegExGroup(CommandEvent, @"set\s*texture\s*=(.*)", 1);

				if (cTextureAtlas.ContainsImage(strParam) == false) {
					cDevConsole.AddText(String.Format("No tile exists named '{0}'", strParam));
					return;
				}

				cObjManager[0][0].TextureName = strParam;
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
			PhysicalObject NewObj;

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
			cObjManager = new ObjectManager(cGraphDevMgr.GraphicsDevice, cTextureAtlas, INTERFACECONTENTDIR + "GameObjects.xml");

			NewObj = cObjManager.SpawnGameObject("ship01", 0);

			NewObj.SetPosition(new Vector2(128, 128), 256, 256, new Vector2(1, 1), 0);
			
			return;
		}

		/// <summary>
		/// Update game state and all content.  Should be called once every redraw, possibly called 
		/// more often than the draws but only if it exceeds time between frame draws.
		/// </summary>
		/// <param name="gameTime">Current time information of the application</param>
		protected override void Update(GameTime gameTime) {
			MouseState Currmouse = Mouse.GetState();
			Vector2 vShipCenter = cObjManager[0][0].GetCenterCoordinates();
			Vector vShipToMouse = new Vector();

			if ((Currmouse.RightButton == ButtonState.Pressed) && (cPriorMouse.RightButton == ButtonState.Pressed)) {
				//User is holding the right mouse button
				vShipCenter = cObjManager[0][0].CenterPoint;

				//vShipCenter.X += Currmouse.X - cPriorMouse.X;
				//vShipCenter.Y += Currmouse.Y - cPriorMouse.Y;
				vShipCenter.X = Currmouse.X;
				vShipCenter.Y = Currmouse.Y;

				cObjManager[0][0].CenterPoint = vShipCenter;
			}

			if ((Currmouse.RightButton == ButtonState.Pressed) && (cPriorMouse.RightButton == ButtonState.Released)){
				//User just clicked the right mouse button
			}

			if ((Currmouse.MiddleButton == ButtonState.Pressed) && (cPriorMouse.MiddleButton == ButtonState.Pressed)) {
				//User is holding the middle mouse button
				
				vShipCenter.X = Currmouse.X - cObjManager[0][0].CenterPoint.X; //distance to add via scaling
				vShipCenter.Y = Currmouse.Y - cObjManager[0][0].CenterPoint.Y;

				vShipCenter.X += cObjManager[0][0].Width; //New width/height of ship
				vShipCenter.Y += cObjManager[0][0].Height;

				vShipCenter.X = vShipCenter.X / cObjManager[0][0].Width;
				vShipCenter.Y = vShipCenter.Y / cObjManager[0][0].Height;

				cObjManager[0][0].Scale = vShipCenter;
			}

			if ((Currmouse.MiddleButton == ButtonState.Pressed) && (cPriorMouse.MiddleButton == ButtonState.Released)) {
				//User jsut clicked the middle mouse button
			}

			if ((Currmouse.XButton1 == ButtonState.Pressed) && (cPriorMouse.XButton1 == ButtonState.Pressed)) {
				//User is holding the middle mouse button
				vShipToMouse.SetRectangularCoordinates(Currmouse.X - vShipCenter.X, Currmouse.Y - vShipCenter.Y);

				cObjManager[0][0].ObjectRotation = (float)(vShipToMouse.Polar.Angle * Math.PI / 180);
			}

			if ((Currmouse.XButton1 == ButtonState.Pressed) && (cPriorMouse.XButton1 == ButtonState.Released)) {
				//User jsut clicked the middle mouse button
			}

			cPriorMouse = Currmouse;

			cObjManager.UpdateObjects(gameTime);
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

			cObjManager.DrawObjects();

			DrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			
			cDevConsole.Draw(DrawBatch);

			DrawBatch.End();

			//Use monogame draw
			base.Draw(gameTime);
		}

	}
}
