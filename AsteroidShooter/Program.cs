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
		private MDLN.MGTools.Console cDevConsole;
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
			cDevConsole = new MDLN.MGTools.Console(cGraphDevMgr.GraphicsDevice, Content, "Font.png", 0, 0, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2);
			cDevConsole.CommandSent += CommandSentEventHandler;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;

			cPlayerShip = new Ship(cGraphDevMgr.GraphicsDevice, 150);
			cPlayerShip.BackgroundColor = new Color(100, 100, 100, 0); //Set background completely transparent
			cPlayerShip.ShipTexture = cTextureDict[Textures.Ship];
			cPlayerShip.Visible = true;
			cPlayerShip.Top = cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2;
			cPlayerShip.Left = cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width / 2;
		}

		protected override void Update(GameTime gameTime) {
			KeyboardState CurrKeys = Keyboard.GetState();
			MouseState CurrMouse = Mouse.GetState();

			if ((cPriorKeyState.IsKeyDown(Keys.OemTilde) == false) && (CurrKeys.IsKeyDown(Keys.OemTilde) == true)) {
				cDevConsole.ToggleVisible();
			}

			cPlayerShip.Update(gameTime, CurrKeys, CurrMouse);
			cDevConsole.Update(gameTime, CurrKeys, CurrMouse);

			cPriorKeyState = CurrKeys;

			//Use monogame update
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

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
