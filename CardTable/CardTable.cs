using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MDNLN.CardTable {
	class Launcher {
		public static void Main(string[] args) {
			using (var game = new CardTable())
			{
				game.Run();
			}
		}
	}

	public class CardTable : Game {
		private GraphicsDeviceManager cGraphDevMgr;
		private Dictionary<Textures, Texture2D> cTextureDict;
		private TextureFont cFont;
		private GameConsole cDevConsole;
		private Button cOpenDoorButton, cSpawnMonsterButton;

		public CardTable() {
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
			foreach (Textures CurrTexture in Enum.GetValues(typeof(Textures))) {
				cTextureDict.Add(CurrTexture, Content.Load<Texture2D>(Tools.GetEnumDescriptionAttribute(CurrTexture)));
			}

			cFont = new TextureFont(cTextureDict[Textures.Font]);

			cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, "Font.png", cGraphDevMgr.GraphicsDevice.Viewport.Width, cGraphDevMgr.GraphicsDevice.Viewport.Height / 2);
			cDevConsole.AccessKey = Keys.OemTilde;
			cDevConsole.UseAccessKey = true;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;

			cOpenDoorButton = new Button(cGraphDevMgr.GraphicsDevice, null, 0, cGraphDevMgr.GraphicsDevice.Viewport.Width - 150, cGraphDevMgr.GraphicsDevice.Viewport.Height / 5, 150);
			cOpenDoorButton.Text = "Open Door";
			cOpenDoorButton.Font = cFont;
			cOpenDoorButton.Visible = true;
			cOpenDoorButton.BackgroundColor = Color.Navy;
			cOpenDoorButton.FontColor = Color.AntiqueWhite;

			cSpawnMonsterButton = new Button(cGraphDevMgr.GraphicsDevice, null, cOpenDoorButton.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width - 150, cGraphDevMgr.GraphicsDevice.Viewport.Height / 5, 150);
			cSpawnMonsterButton.Text = "Spawn\nMonster";
			cSpawnMonsterButton.Font = cFont;
			cSpawnMonsterButton.Visible = true;
			cSpawnMonsterButton.BackgroundColor = Color.Blue;
			cSpawnMonsterButton.FontColor = Color.AntiqueWhite;
		}

		protected override void Update(GameTime gameTime) {
			cOpenDoorButton.Update(gameTime);
			cSpawnMonsterButton.Update(gameTime);

			cDevConsole.Update(gameTime);

			//Use monogame update
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

			cOpenDoorButton.Draw();
			cSpawnMonsterButton.Draw();

			cDevConsole.Draw();

			//Use monogame draw
			base.Draw(gameTime);
		}

		protected enum Textures {
			[Description("Font.png")]
			Font
		}
	}
}
