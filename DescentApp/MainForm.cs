using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DescentApp {
	class Launcher {
		static void Main(string[] args) {
			using (var game = new DescentApp()) {
				game.Run();
			}
		}
	}

	/// <summary>
	/// Class to represent the main screen of the application.  Should always be visible and available to manage all content.
	/// </summary>
	class DescentApp : Game {
		/// <summary>
		/// Subdirectory that will contain all external content
		/// </summary>
		private const string INTERFACECONTENTDIR = "Content";

		/// <summary>
		/// Connection to th egraphics device
		/// </summary>
		private GraphicsDeviceManager cGraphDevMgr;
		/// <summary>
		/// Object used to draw sprites to the buffer in batches
		/// </summary>
		private SpriteBatch cDrawBatch;
		/// <summary>
		/// Object to display and manage the game console
		/// </summary>
		private GameConsole cDevConsole;

		/// <summary>
		/// Constructor to establish all graphical and interface settings as well as
		/// prepare class variables.
		/// </summary>
		public DescentApp() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			IsMouseVisible = true;
		}

		/// <summary>
		/// Place to initialize and prepare MonoGame objects
		/// </summary>
		protected override void Initialize() {
			cDrawBatch = new SpriteBatch(cGraphDevMgr.GraphicsDevice);

			//Initializes monogame
			base.Initialize();
		}

		/// <summary>
		/// Called when application is shutting down so that unmanaged resources can be released.
		/// </summary>
		protected override void UnloadContent() {

		}

		/// <summary>
		/// Load all external content files that are needed
		/// </summary>
		protected override void LoadContent() {
			Content.RootDirectory = INTERFACECONTENTDIR;

			cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, "Font.png", cGraphDevMgr.GraphicsDevice.Viewport.Width, cGraphDevMgr.GraphicsDevice.Viewport.Height / 2);
			cDevConsole.AccessKey = Keys.OemTilde;
			cDevConsole.UseAccessKey = true;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;
			cDevConsole.CommandSent += new CommandSentEventHandler(CommandEvent);

			cDevConsole.AddText(String.Format("Viewport Height={0} Width={1}", cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width));
		}

		/// <summary>
		/// Update game state and all content.  Should be called once every redraw, possibly called 
		/// more often than the draws but only if it exceeds time between frame draws.
		/// </summary>
		/// <param name="gameTime">Current time information of the application</param>
		protected override void Update(GameTime gameTime) {
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
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);
			
			cDevConsole.Draw();

			//Use monogame draw
			base.Draw(gameTime);
		}

		/// <summary>
		/// Handler to interpret and execute commands entered in the console
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="CommandEvent"></param>
		private void CommandEvent(object Sender, string CommandEvent) {
			if (RegEx.LooseTest(CommandEvent, @"^\s*(quit|exit|close)\s*$") == true) {
				Exit();
			} else {
				cDevConsole.AddText("Unrecognized command: " + CommandEvent);
			}
		}
	}
}
