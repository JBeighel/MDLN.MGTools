using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ParticleTest {
	class ParticleTester : Game {
		/// <summary>
		/// Subdirectory that will contain all external content
		/// </summary>
		private const string INTERFACECONTENTDIR = @"Content";
		/// <summary>
		/// Connection to the graphics device
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
		private ParticleEngine2D cSparkles;
		private Dictionary<TextureFiles, Texture2D> cTextureDict;
		private KeyboardState cPriorKeyState;
		private Random cRand;

		public ParticleTester() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += FormResizeHandler;

			cTextureDict = new Dictionary<TextureFiles, Texture2D>();
			cRand = new Random();

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
			} else if (RegEx.LooseTest(CommandEvent, @"^add\s*([0-9]+)\s*([^ ]+)$") == true) {
				string strPart;
				bool bParse;
				UInt32 nCtr, nNumParticles;
				TextureFiles eTexture;
				Particle2D NewParticle = new Particle2D();

				strPart = RegEx.GetRegExGroup(CommandEvent, @"^add\s*([0-9]+)\s*([^ ]+)$", 1);
				nNumParticles = UInt32.Parse(strPart);

				strPart = RegEx.GetRegExGroup(CommandEvent, @"^add\s*([0-9]+)\s*([^ ]+)$", 2);
				bParse = Enum.TryParse<TextureFiles>(strPart, true, out eTexture);
				if (bParse == false) {
					cDevConsole.AddText(String.Format("No texture named '{0}' could be found", strPart));
					return;
				}

				for (nCtr = 0; nCtr < nNumParticles; nCtr++) {
					NewParticle = new Particle2D();
					NewParticle.Tint = new Color(cRand.Next(0, 255), cRand.Next(0, 255), cRand.Next(0, 255));
					
					NewParticle.Height = cTextureDict[eTexture].Height / 2;
					NewParticle.Width = cTextureDict[eTexture].Width / 2;
					NewParticle.TopLeft.X = (GraphicsDevice.Viewport.Width / 2) - (NewParticle.Width / 2);
					NewParticle.TopLeft.Y = (GraphicsDevice.Viewport.Height / 2) - (NewParticle.Height / 2);

					NewParticle.Image = cTextureDict[eTexture];
					NewParticle.TotalDistance.X = cRand.Next(GraphicsDevice.Viewport.Width / -2, GraphicsDevice.Viewport.Width / 2);
					NewParticle.TotalDistance.Y = cRand.Next(GraphicsDevice.Viewport.Height / -2, GraphicsDevice.Viewport.Height / 2);

					NewParticle.Rotation = (float)((cRand.Next(0, 360) * (2 * Math.PI)) / 360);
					NewParticle.TotalRotate = (float)(cRand.Next(-5, 5) * 2 * Math.PI);

					NewParticle.TimeToLive = cRand.Next(1000, 10000);
					NewParticle.AlphaFade = true;

					cSparkles.AddParticle(NewParticle);
				}
			} else {
				cDevConsole.AddText("Unrecognized command: " + CommandEvent);
			}
		}

		/// <summary>
		/// Place to initialize and prepare MonoGame objects
		/// </summary>
		protected override void Initialize() {
			cDrawBatch = new SpriteBatch(cGraphDevMgr.GraphicsDevice);

			//Initializes monogame
			base.Initialize();

			return;
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
			String strFileName;
			Texture2D NewTexture;

			try {
				cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, INTERFACECONTENTDIR + "\\Font.png", cGraphDevMgr.GraphicsDevice.Viewport.Width, cGraphDevMgr.GraphicsDevice.Viewport.Height / 2);
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

			foreach (TextureFiles CurrTexture in Enum.GetValues(typeof(TextureFiles))) {
				strFileName = INTERFACECONTENTDIR + "\\" + EnumTools.GetEnumDescriptionAttribute(CurrTexture);

				NewTexture = Texture2D.FromStream(cGraphDevMgr.GraphicsDevice, new FileStream(strFileName, FileMode.Open));
				cTextureDict.Add(CurrTexture, NewTexture);
			}

			cSparkles = new ParticleEngine2D(cGraphDevMgr.GraphicsDevice);

			return;
		}

		/// <summary>
		/// Update game state and all content.  Should be called once every redraw, possibly called 
		/// more often than the draws but only if it exceeds time between frame draws.
		/// </summary>
		/// <param name="gameTime">Current time information of the application</param>
		protected override void Update(GameTime gameTime) {
			cDevConsole.Update(gameTime);
			cSparkles.Update(gameTime);
			KeyboardState CurrKeys = Keyboard.GetState();

			//Handle keyboard input

			cPriorKeyState = CurrKeys;

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

			cSparkles.Draw();

			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			//Always draw console last
			cDevConsole.Draw(cDrawBatch);

			cDrawBatch.End();

			//Use monogame draw
			base.Draw(gameTime);
		}

		protected enum TextureFiles
		{
			[Description("circle_01.png")]
			circle_01,
			[Description("circle_02.png")]
			circle_02,
			[Description("circle_03.png")]
			circle_03,
			[Description("circle_04.png")]
			circle_04,
			[Description("circle_05.png")]
			circle_05,
			[Description("dirt_01.png")]
			dirt_01,
			[Description("dirt_02.png")]
			dirt_02,
			[Description("dirt_03.png")]
			dirt_03
		}
	}
}
