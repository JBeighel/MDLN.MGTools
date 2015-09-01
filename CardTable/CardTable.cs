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
		private const float BUTTONWIDTHPERCENT = 0.20f;
		private const string INTERFACECONTENTDIR = "Content";

		private GraphicsDeviceManager cGraphDevMgr;
		private Dictionary<Textures, Texture2D> cTextureDict;
		private TextureFont cFont;
		private GameConsole cDevConsole;
		private Dictionary<MenuButtons, Button> cMenuBtns;

		public CardTable() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			IsMouseVisible = true;
			cTextureDict = new Dictionary<Textures, Texture2D>();
			cMenuBtns = new Dictionary<MenuButtons, Button>();
		}

		protected override void Initialize() {
			//Initializes monogame
			base.Initialize();
		}

		protected override void UnloadContent() {
			
		}

		protected override void LoadContent() {
			Rectangle ButtonArea = new Rectangle();

			Content.RootDirectory = INTERFACECONTENTDIR;

			foreach (Textures CurrTexture in Enum.GetValues(typeof(Textures))) {
				cTextureDict.Add(CurrTexture, Content.Load<Texture2D>(Tools.GetEnumDescriptionAttribute(CurrTexture)));
			}

			cFont = new TextureFont(cTextureDict[Textures.Font]);

			ButtonArea.Width = (int)(cGraphDevMgr.GraphicsDevice.Viewport.Width * BUTTONWIDTHPERCENT);
			ButtonArea.Height = (cGraphDevMgr.GraphicsDevice.Viewport.Height - Enum.GetValues(typeof(MenuButtons)).Length) / Enum.GetValues(typeof(MenuButtons)).Length;
			ButtonArea.X = cGraphDevMgr.GraphicsDevice.Viewport.Width - ButtonArea.Width;
			ButtonArea.Y = 0;
			foreach (MenuButtons CurrBtn in Enum.GetValues(typeof(MenuButtons))) {
				cMenuBtns.Add(CurrBtn, new Button(cGraphDevMgr.GraphicsDevice, null, ButtonArea.Y, ButtonArea.X, ButtonArea.Height, ButtonArea.Width));
				cMenuBtns[CurrBtn].Text = Tools.GetEnumDescriptionAttribute(CurrBtn);
				cMenuBtns[CurrBtn].Font = cFont;
				cMenuBtns[CurrBtn].Visible = true;
				cMenuBtns[CurrBtn].BackgroundColor = Color.Navy;
				cMenuBtns[CurrBtn].FontColor = Color.AntiqueWhite;

				ButtonArea.Y += ButtonArea.Height + 1;
			}

			cMenuBtns[MenuButtons.Menu].Click += new ButtonClickEvent(MenuClick);
			cMenuBtns[MenuButtons.OpenDoor].Click += new ButtonClickEvent(OpenDoorClick);

			cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, "Font.png", cGraphDevMgr.GraphicsDevice.Viewport.Width, cGraphDevMgr.GraphicsDevice.Viewport.Height / 2);
			cDevConsole.AccessKey = Keys.OemTilde;
			cDevConsole.UseAccessKey = true;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;
			cDevConsole.CommandSent += new CommandSentEventHandler(CommandEvent);

			cDevConsole.AddText(String.Format("Viewport Height={0} Width={1}", cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width));
		}

		protected override void Update(GameTime gameTime) {
			foreach (KeyValuePair<MenuButtons, Button> KeyPair in cMenuBtns) {
				KeyPair.Value.Update(gameTime);
			}

			cDevConsole.Update(gameTime);

			//Use monogame update
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

			foreach (KeyValuePair<MenuButtons, Button> KeyPair in cMenuBtns) {
				KeyPair.Value.Draw();
			}

			cDevConsole.Draw();

			//Use monogame draw
			base.Draw(gameTime);
		}

		private void CommandEvent(object Sender, string CommandEvent) {
			if (RegEx.LooseTest(CommandEvent, @"^\s*(quit|exit|close)\s*$") == true) {
				Exit();
			} else {
				cDevConsole.AddText("Unrecognized command: " + CommandEvent);
			}
		}

		private void MenuClick(object Sender, MouseButton Button) {
			if (Button == MouseButton.Left) {
				cDevConsole.AddText("Menu button clicked");
			}
		}

		private void OpenDoorClick(object Sender, MouseButton Button) {
			if (Button == MouseButton.Left) {
				cDevConsole.AddText("Menu button clicked");
			}
		}

		protected enum Textures {
			[Description("Font.png")]
			Font
		}

		protected enum MenuButtons {
			[Description("Menu")]
			Menu,
			[Description("Open Door")]
			OpenDoor,
			[Description("Draw\nMonster")]
			DrawMonster,
			[Description("Treasure")]
			Treasure,
			[Description("Abilities")]
			Abilities
		}
	}
}
