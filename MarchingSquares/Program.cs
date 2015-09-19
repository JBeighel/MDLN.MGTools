using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MDLN.MarchingSquares {
	class MainClass {
		public static void Main(string[] args) {
			using (var game = new MarchingSquares())
			{
				game.Run();
			}
		}
	}

	public class MarchingSquares : Game {
		private GraphicsDeviceManager cGraphDevMgr;
		private GameConsole cDevConsole;
		private Dictionary<Textures, Texture2D> cTextureDict;

		public MarchingSquares() {
			cGraphDevMgr = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize() {
			//Initializes monogame
			base.Initialize();
		}

		protected override void LoadContent() {
			cTextureDict = new Dictionary<Textures, Texture2D>();
			foreach (Textures CurrTexture in Enum.GetValues(typeof(Textures))) {
				cTextureDict.Add(CurrTexture, Content.Load<Texture2D>(Tools.Tools.GetEnumDescriptionAttribute(CurrTexture)));
			}

			cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, Tools.Tools.GetEnumDescriptionAttribute(Textures.Font), 0, 0, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2);
			cDevConsole.CommandSent += new CommandSentEventHandler(CommandSentEventHandler);
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;
			cDevConsole.AccessKey = Keys.OemTilde;
			cDevConsole.UseAccessKey = true;

			//Call monogame base
			base.LoadContent();
		}

		protected override void Update(GameTime gameTime) {
			
			//Update visual objects
			cDevConsole.Update(gameTime);

			//Call monogame base
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

			//Draw visual objects
			cDevConsole.Draw();

			//Call monogame base
			base.Draw(gameTime);
		}

		protected override void UnloadContent() {

			//Call monogame base
			base.UnloadContent();
		}

		private void CommandSentEventHandler(object Sender, string Command) {

		}

		protected enum Textures {
			[Description("MarchingSquares.png")]
			Squares,
			[Description("Font.png")]
			Font
		}
	}
}

