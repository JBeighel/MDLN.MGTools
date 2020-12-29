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
		private MDLN.MGTools.Container cSettingsCont;
		private MDLN.MGTools.TextBox cNumParticlesTxt;
		private Button cAddParitclesBtn, cNumParticlesLbl, cShowingLbl;
		private Dictionary<TextureFiles, Texture2D> cTextureDict;
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
			cSettingsCont.Height = GraphicsDevice.Viewport.Height;

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

				//Randomly generate the number of particles requested
				for (nCtr = 0; nCtr < nNumParticles; nCtr++) {
					NewParticle = new Particle2D();
					//Particle images should be grayscale, allowing this tint value to color them
					NewParticle.Tint = new Color(cRand.Next(0, 255), cRand.Next(0, 255), cRand.Next(0, 255));
					NewParticle.Image = cTextureDict[eTexture];

					//Set the dimensions of the image on screen
					NewParticle.Height = cTextureDict[eTexture].Height / 2;
					NewParticle.Width = cTextureDict[eTexture].Width / 2;
					//Set the position of the image
					NewParticle.TopLeft.X = (GraphicsDevice.Viewport.Width / 2) - (NewParticle.Width / 2);
					NewParticle.TopLeft.Y = (GraphicsDevice.Viewport.Height / 2) - (NewParticle.Height / 2);
					NewParticle.Rotation = (float)((cRand.Next(0, 360) * (2 * Math.PI)) / 360);

					//Set the total movement the particle will travel
					NewParticle.TotalDistance.X = cRand.Next(GraphicsDevice.Viewport.Width / -2, GraphicsDevice.Viewport.Width / 2);
					NewParticle.TotalDistance.Y = cRand.Next(GraphicsDevice.Viewport.Height / -2, GraphicsDevice.Viewport.Height / 2);
					NewParticle.TotalRotate = (float)(cRand.Next(-5, 5) * 2 * Math.PI);

					//Set how long the particle will live in milliseconds
					NewParticle.TimeToLive = cRand.Next(1000, 10000);
					NewParticle.AlphaFade = true;

					cSparkles.AddParticle(NewParticle);
				}
			} else {
				cDevConsole.AddText("Unrecognized command: " + CommandEvent);
			}
		}

		private void AddParticlesClickHandler(object Sender, MouseButton eButton) {
			Particle2D NewParticle;
			TextureFiles eTexture = TextureFiles.dirt_01;
			int nCtr, nParticleCnt;

			if (Int32.TryParse(cNumParticlesTxt.Text, out nParticleCnt) == false) {
				nParticleCnt = 1;
			}

			if (eButton == MouseButton.Left) {
				for (nCtr = 0; nCtr < nParticleCnt; nCtr++) {
					NewParticle = new Particle2D();
					//Particle images should be grayscale, allowing this tint value to color them
					NewParticle.Tint = new Color(cRand.Next(0, 255), cRand.Next(0, 255), cRand.Next(0, 255));
					NewParticle.Image = cTextureDict[eTexture];

					//Set the dimensions of the image on screen
					NewParticle.Height = cTextureDict[eTexture].Height / 2;
					NewParticle.Width = cTextureDict[eTexture].Width / 2;
					//Set the position of the image
					NewParticle.TopLeft.X = (GraphicsDevice.Viewport.Width / 2) - (NewParticle.Width / 2);
					NewParticle.TopLeft.Y = (GraphicsDevice.Viewport.Height / 2) - (NewParticle.Height / 2);
					NewParticle.Rotation = (float)((cRand.Next(0, 360) * (2 * Math.PI)) / 360);

					//Set the total movement the particle will travel
					NewParticle.TotalDistance.X = cRand.Next(GraphicsDevice.Viewport.Width / -2, GraphicsDevice.Viewport.Width / 2);
					NewParticle.TotalDistance.Y = cRand.Next(GraphicsDevice.Viewport.Height / -2, GraphicsDevice.Viewport.Height / 2);
					NewParticle.TotalRotate = (float)(cRand.Next(-5, 5) * 2 * Math.PI);

					//Set how long the particle will live in milliseconds
					NewParticle.TimeToLive = cRand.Next(1000, 10000);
					NewParticle.AlphaFade = true;

					cSparkles.AddParticle(NewParticle);
				}
			}

			return;
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
			TextureFont Font = new TextureFont();
			FileStream FileLoad;

			strFileName = INTERFACECONTENTDIR + "\\Font.png";
			FileLoad = new FileStream(strFileName, FileMode.Open);
			NewTexture = Texture2D.FromStream(cGraphDevMgr.GraphicsDevice, FileLoad);
			Font.FontTexture = NewTexture;
			FileLoad.Close();

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

			cSettingsCont = new MDLN.MGTools.Container(GraphicsDevice, GraphicsDevice.Viewport.Height, 300);
			cSettingsCont.Width = 300;
			cSettingsCont.Height = GraphicsDevice.Viewport.Height;
			cSettingsCont.Visible = true;
			cSettingsCont.BackgroundColor = new Color(Color.White, 0.5f);
			cSettingsCont.Top = 0;
			cSettingsCont.Left = 0;

			cShowingLbl = new Button(GraphicsDevice, null, 10, 10, 20, 280);
			cShowingLbl.Text = "";
			cShowingLbl.Visible = true;
			cShowingLbl.Font = Font;
			cShowingLbl.BackgroundColor = Color.Transparent;
			cShowingLbl.FontColor = Color.White;

			cAddParitclesBtn = new Button(GraphicsDevice, null, 40, 10, 20, 280);
			cAddParitclesBtn.Text = "Add Particles";
			cAddParitclesBtn.Visible = true;
			cAddParitclesBtn.Font = Font;
			cAddParitclesBtn.BackgroundColor = Color.LightGray;
			cAddParitclesBtn.FontColor = Color.Black;
			cAddParitclesBtn.Click += AddParticlesClickHandler;

			cNumParticlesLbl = new Button(GraphicsDevice, null, 70, 10, 20, 105);
			cNumParticlesLbl.Text = "Add Amount";
			cNumParticlesLbl.Visible = true;
			cNumParticlesLbl.Font = Font;
			cNumParticlesLbl.BackgroundColor = Color.Transparent;
			cNumParticlesLbl.FontColor = Color.Black;

			cNumParticlesTxt = new TextBox(GraphicsDevice, null, 70, 125, 20, 165);
			cNumParticlesTxt.Text = "1";
			cNumParticlesTxt.Visible = true;
			cNumParticlesTxt.Font = Font;
			cNumParticlesTxt.BackgroundColor = Color.Black;
			cNumParticlesTxt.FontColor = Color.White;
			cNumParticlesTxt.Alignment = Justify.MiddleCenter;

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

			cShowingLbl.Text = String.Format("Showing {0} Particles", cSparkles.ParticleList.Count);
			cShowingLbl.Update(gameTime);
			cSettingsCont.Update(gameTime);
			cAddParitclesBtn.Update(gameTime);
			cNumParticlesLbl.Update(gameTime);
			cNumParticlesTxt.Update(gameTime);

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

			cShowingLbl.Draw(cDrawBatch);
			cSettingsCont.Draw(cDrawBatch);
			cAddParitclesBtn.Draw(cDrawBatch);
			cNumParticlesLbl.Draw(cDrawBatch);
			cNumParticlesTxt.Draw(cDrawBatch);

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
