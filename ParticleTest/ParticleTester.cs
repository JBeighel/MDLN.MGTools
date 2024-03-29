﻿using MDLN.MGTools;
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
		readonly Color LBLTEXTCOLOR = Color.White;

		/// <summary>
		/// Subdirectory that will contain all external content
		/// </summary>
		private const string INTERFACECONTENTDIR = @"Content";
		/// <summary>
		/// Connection to the graphics device
		/// </summary>
		private readonly GraphicsDeviceManager cGraphDevMgr;
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
		private MDLN.MGTools.TextBox cNumParticlesTxt, cHeightMinTxt, cHeightMaxTxt, cWidthMinTxt, cWidthMaxTxt;
		private MDLN.MGTools.TextBox cRedMinTxt, cRedMaxTxt, cGreenMinTxt, cGreenMaxTxt, cBlueMinTxt, cBlueMaxTxt;
		private MDLN.MGTools.TextBox cLifeMinTxt, cLifeMaxTxt, cDelayMinTxt, cDelayMaxTxt, cXDistMinTxt, cXDistMaxTxt;
		private MDLN.MGTools.TextBox cYDistMinTxt, cYDistMaxTxt, cRotateMinTxt, cRotateMaxTxt;
		private Button cAddParitclesBtn, cRotateAfterBtn, cAlphaFadeBtn; 
		private Button cNumParticlesLbl, cShowingLbl, cHeightLbl, cWidthLbl, cRedLbl, cGreenLbl, cBlueLbl, cLifeLbl, cDelayLbl, cXDistLbl, cYDistLbl, cRotateLbl;
		private bool cbRotateAfter, cbAlphaFade;

		private readonly Dictionary<TextureFiles, Texture2D> cTextureDict;
		private readonly Random cRand;

		public ParticleTester() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += FormResizeHandler;

			cTextureDict = new Dictionary<TextureFiles, Texture2D>();
			cRand = new Random();
			cbRotateAfter = false;
			cbAlphaFade = false;

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
				Particle2D NewParticle;

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
					NewParticle = new Particle2D(GraphicsDevice);
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
			Vector2 nHeight, nWidth, nRed, nGreen, nBlue, nLife, nDelay, nXDist, nYDist, nRotate;

			if (Int32.TryParse(cNumParticlesTxt.Text, out nParticleCnt) == false) {
				nParticleCnt = 1;
			}

			if ((float.TryParse(cHeightMinTxt.Text, out nHeight.X) == false) || (nHeight.X <= 0)) {
				nHeight.X = cTextureDict[eTexture].Height / 2;
			}

			if ((float.TryParse(cHeightMaxTxt.Text, out nHeight.Y) == false) || (nHeight.X > nHeight.Y)) {
				nHeight.Y = nHeight.X;
			}

			if ((float.TryParse(cHeightMinTxt.Text, out nWidth.X) == false) || (nWidth.X <= 0)) {
				nWidth.X = cTextureDict[eTexture].Height / 2;
			}

			if ((float.TryParse(cHeightMaxTxt.Text, out nWidth.Y) == false) || (nWidth.X > nWidth.Y)) {
				nWidth.Y = nWidth.X;
			}

			if ((float.TryParse(cRedMinTxt.Text, out nRed.X) == false) || (nRed.X < 0) || (nRed.X > 255)) {
				nRed.X = 0;
			}

			if ((float.TryParse(cRedMaxTxt.Text, out nRed.Y) == false) || (nRed.X > nRed.Y) || (nRed.Y > 255)) {
				nRed.Y = nRed.X;
			}

			if ((float.TryParse(cGreenMinTxt.Text, out nGreen.X) == false) || (nGreen.X < 0) || (nGreen.X > 255)) {
				nGreen.X = 0;
			}

			if ((float.TryParse(cGreenMaxTxt.Text, out nGreen.Y) == false) || (nGreen.X > nGreen.Y) || (nGreen.Y > 255)) {
				nGreen.Y = nGreen.X;
			}

			if ((float.TryParse(cBlueMinTxt.Text, out nBlue.X) == false) || (nBlue.X < 0) || (nBlue.X > 255)) {
				nBlue.X = 0;
			}

			if ((float.TryParse(cBlueMaxTxt.Text, out nBlue.Y) == false) || (nBlue.X > nBlue.Y) || (nBlue.Y > 255)) {
				nBlue.Y = nBlue.X;
			}

			if ((float.TryParse(cLifeMinTxt.Text, out nLife.X) == false) || (nLife.X < 0)) {
				nLife.X = 1;
			}

			if ((float.TryParse(cLifeMaxTxt.Text, out nLife.Y) == false) || (nLife.X > nLife.Y)) {
				nLife.Y = nLife.X;
			}

			if ((float.TryParse(cDelayMinTxt.Text, out nDelay.X) == false) || (nDelay.X < 0)) {
				nDelay.X = 0;
			}

			if ((float.TryParse(cDelayMaxTxt.Text, out nDelay.Y) == false) || (nDelay.X > nDelay.Y)) {
				nDelay.Y = nDelay.X;
			}

			if (float.TryParse(cXDistMinTxt.Text, out nXDist.X) == false) {
				nXDist.X = GraphicsDevice.Viewport.Width / -2;
			}

			if ((float.TryParse(cXDistMaxTxt.Text, out nXDist.Y) == false) || (nXDist.X > nXDist.Y)) {
				nXDist.Y = nXDist.X;
			}

			if (float.TryParse(cYDistMinTxt.Text, out nYDist.X) == false) {
				nYDist.X = GraphicsDevice.Viewport.Height / -2;
			}

			if ((float.TryParse(cYDistMaxTxt.Text, out nYDist.Y) == false) || (nYDist.X > nYDist.Y)) {
				nYDist.Y = nYDist.X;
			}

			if (float.TryParse(cRotateMinTxt.Text, out nRotate.X) == false) {
				nRotate.X = GraphicsDevice.Viewport.Height / -2;
			}

			if ((float.TryParse(cRotateMaxTxt.Text, out nRotate.Y) == false) || (nRotate.X > nRotate.Y)) {
				nRotate.Y = nRotate.X;
			}

			if (eButton == MouseButton.Left) {
				for (nCtr = 0; nCtr < nParticleCnt; nCtr++) {
					NewParticle = new Particle2D(GraphicsDevice);
					//Particle images should be grayscale, allowing this tint value to color them
					NewParticle.Tint = new Color(cRand.Next((int)nRed.X, (int)nRed.Y), cRand.Next((int)nGreen.X, (int)nGreen.Y), cRand.Next((int)nBlue.X, (int)nBlue.Y));
					NewParticle.Image = cTextureDict[eTexture];

					//Set the dimensions of the image on screen
					NewParticle.Height = cRand.Next((int)nHeight.X, (int)nHeight.Y);
					NewParticle.Width = cRand.Next((int)nWidth.X, (int)nWidth.Y);
					//Set the position of the image
					NewParticle.TopLeft.X = (GraphicsDevice.Viewport.Width / 2) - (NewParticle.Width / 2);
					NewParticle.TopLeft.Y = (GraphicsDevice.Viewport.Height / 2) - (NewParticle.Height / 2);
					NewParticle.Rotation = (float)((cRand.Next(0, 360) * (2 * Math.PI)) / 360);

					//Set the total movement the particle will travel
					NewParticle.TotalDistance.X = cRand.Next((int)nXDist.X, (int)nXDist.Y);
					NewParticle.TotalDistance.Y = cRand.Next((int)nYDist.X, (int)nYDist.Y);
					NewParticle.TotalRotate = (float)(cRand.Next((int)nRotate.X, (int)nRotate.Y) * 2 * Math.PI);

					//Set how long the particle will live in milliseconds
					NewParticle.TimeToLive = cRand.Next((int)nLife.X, (int)nLife.Y);
					NewParticle.tCreateDelay = cRand.Next((int)nDelay.X, (int)nDelay.Y);

					NewParticle.AlphaFade = cbAlphaFade;
					NewParticle.bSpiralPath = cbRotateAfter;

					cSparkles.AddParticle(NewParticle);
				}
			}

			return;
		}

		private void RotateAfterClickHandler(object Sender, MouseButton eButton) {
			if (cbRotateAfter == true) {
				cbRotateAfter = false;
				cRotateAfterBtn.BackgroundColor = Color.Red;
			} else {
				cbRotateAfter = true;
				cRotateAfterBtn.BackgroundColor = Color.Green;
			}

			return;
		}
		

		private void AlphaFadeClickHandler(object Sender, MouseButton eButton) {
			if (cbAlphaFade == true) {
				cbAlphaFade = false;
				cAlphaFadeBtn.BackgroundColor = Color.Red;
			} else {
				cbAlphaFade = true;
				cAlphaFadeBtn.BackgroundColor = Color.Green;
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
			cShowingLbl.FontColor = LBLTEXTCOLOR;

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
			cNumParticlesLbl.FontColor = LBLTEXTCOLOR;

			cNumParticlesTxt = new TextBox(GraphicsDevice, null, 70, 125, 20, 165);
			cNumParticlesTxt.Text = "1";
			cNumParticlesTxt.Visible = true;
			cNumParticlesTxt.Font = Font;
			cNumParticlesTxt.BackgroundColor = Color.Black;
			cNumParticlesTxt.FontColor = Color.White;
			cNumParticlesTxt.Alignment = Justify.MiddleCenter;

			cHeightLbl = new Button(GraphicsDevice, null, 100, 10, 20, 90);
			cHeightLbl.Text = "Height";
			cHeightLbl.Visible = true;
			cHeightLbl.Font = Font;
			cHeightLbl.BackgroundColor = Color.Transparent;
			cHeightLbl.FontColor = LBLTEXTCOLOR;

			cHeightMinTxt = new TextBox(GraphicsDevice, null, 100, 100, 20, 90);
			cHeightMinTxt.Text = "64";
			cHeightMinTxt.Visible = true;
			cHeightMinTxt.Font = Font;
			cHeightMinTxt.BackgroundColor = Color.Black;
			cHeightMinTxt.FontColor = Color.White;
			cHeightMinTxt.Alignment = Justify.MiddleCenter;

			cHeightMaxTxt = new TextBox(GraphicsDevice, null, 100, 200, 20, 90);
			cHeightMaxTxt.Text = "64";
			cHeightMaxTxt.Visible = true;
			cHeightMaxTxt.Font = Font;
			cHeightMaxTxt.BackgroundColor = Color.Black;
			cHeightMaxTxt.FontColor = Color.White;
			cHeightMaxTxt.Alignment = Justify.MiddleCenter;

			cWidthLbl = new Button(GraphicsDevice, null, 130, 10, 20, 90);
			cWidthLbl.Text = "Width";
			cWidthLbl.Visible = true;
			cWidthLbl.Font = Font;
			cWidthLbl.BackgroundColor = Color.Transparent;
			cWidthLbl.FontColor = LBLTEXTCOLOR;

			cWidthMinTxt = new TextBox(GraphicsDevice, null, 130, 100, 20, 90);
			cWidthMinTxt.Text = "64";
			cWidthMinTxt.Visible = true;
			cWidthMinTxt.Font = Font;
			cWidthMinTxt.BackgroundColor = Color.Black;
			cWidthMinTxt.FontColor = Color.White;
			cWidthMinTxt.Alignment = Justify.MiddleCenter;

			cWidthMaxTxt = new TextBox(GraphicsDevice, null, 130, 200, 20, 90);
			cWidthMaxTxt.Text = "64";
			cWidthMaxTxt.Visible = true;
			cWidthMaxTxt.Font = Font;
			cWidthMaxTxt.BackgroundColor = Color.Black;
			cWidthMaxTxt.FontColor = Color.White;
			cWidthMaxTxt.Alignment = Justify.MiddleCenter;

			cRedLbl = new Button(GraphicsDevice, null, 160, 10, 20, 90);
			cRedLbl.Text = "Red";
			cRedLbl.Visible = true;
			cRedLbl.Font = Font;
			cRedLbl.BackgroundColor = Color.Transparent;
			cRedLbl.FontColor = LBLTEXTCOLOR;

			cRedMinTxt = new TextBox(GraphicsDevice, null, 160, 100, 20, 90);
			cRedMinTxt.Text = "0";
			cRedMinTxt.Visible = true;
			cRedMinTxt.Font = Font;
			cRedMinTxt.BackgroundColor = Color.Black;
			cRedMinTxt.FontColor = Color.White;
			cRedMinTxt.Alignment = Justify.MiddleCenter;

			cRedMaxTxt = new TextBox(GraphicsDevice, null, 160, 200, 20, 90);
			cRedMaxTxt.Text = "255";
			cRedMaxTxt.Visible = true;
			cRedMaxTxt.Font = Font;
			cRedMaxTxt.BackgroundColor = Color.Black;
			cRedMaxTxt.FontColor = Color.White;
			cRedMaxTxt.Alignment = Justify.MiddleCenter;

			cGreenLbl = new Button(GraphicsDevice, null, 190, 10, 20, 90);
			cGreenLbl.Text = "Green";
			cGreenLbl.Visible = true;
			cGreenLbl.Font = Font;
			cGreenLbl.BackgroundColor = Color.Transparent;
			cGreenLbl.FontColor = LBLTEXTCOLOR;

			cGreenMinTxt = new TextBox(GraphicsDevice, null, 190, 100, 20, 90);
			cGreenMinTxt.Text = "0";
			cGreenMinTxt.Visible = true;
			cGreenMinTxt.Font = Font;
			cGreenMinTxt.BackgroundColor = Color.Black;
			cGreenMinTxt.FontColor = Color.White;
			cGreenMinTxt.Alignment = Justify.MiddleCenter;

			cGreenMaxTxt = new TextBox(GraphicsDevice, null, 190, 200, 20, 90);
			cGreenMaxTxt.Text = "255";
			cGreenMaxTxt.Visible = true;
			cGreenMaxTxt.Font = Font;
			cGreenMaxTxt.BackgroundColor = Color.Black;
			cGreenMaxTxt.FontColor = Color.White;
			cGreenMaxTxt.Alignment = Justify.MiddleCenter;

			cBlueLbl = new Button(GraphicsDevice, null, 220, 10, 20, 90);
			cBlueLbl.Text = "Blue";
			cBlueLbl.Visible = true;
			cBlueLbl.Font = Font;
			cBlueLbl.BackgroundColor = Color.Transparent;
			cBlueLbl.FontColor = LBLTEXTCOLOR;

			cBlueMinTxt = new TextBox(GraphicsDevice, null, 220, 100, 20, 90);
			cBlueMinTxt.Text = "0";
			cBlueMinTxt.Visible = true;
			cBlueMinTxt.Font = Font;
			cBlueMinTxt.BackgroundColor = Color.Black;
			cBlueMinTxt.FontColor = Color.White;
			cBlueMinTxt.Alignment = Justify.MiddleCenter;

			cBlueMaxTxt = new TextBox(GraphicsDevice, null, 220, 200, 20, 90);
			cBlueMaxTxt.Text = "255";
			cBlueMaxTxt.Visible = true;
			cBlueMaxTxt.Font = Font;
			cBlueMaxTxt.BackgroundColor = Color.Black;
			cBlueMaxTxt.FontColor = Color.White;
			cBlueMaxTxt.Alignment = Justify.MiddleCenter;

			cLifeLbl = new Button(GraphicsDevice, null, 250, 10, 20, 90);
			cLifeLbl.Text = "Dur mSec";
			cLifeLbl.Visible = true;
			cLifeLbl.Font = Font;
			cLifeLbl.BackgroundColor = Color.Transparent;
			cLifeLbl.FontColor = LBLTEXTCOLOR;

			cLifeMinTxt	= new TextBox(GraphicsDevice, null, 250, 100, 20, 90);
			cLifeMinTxt.Text = "1000";
			cLifeMinTxt.Visible = true;
			cLifeMinTxt.Font = Font;
			cLifeMinTxt.BackgroundColor = Color.Black;
			cLifeMinTxt.FontColor = Color.White;
			cLifeMinTxt.Alignment = Justify.MiddleCenter;

			cLifeMaxTxt = new TextBox(GraphicsDevice, null, 250, 200, 20, 90);
			cLifeMaxTxt.Text = "5000";
			cLifeMaxTxt.Visible = true;
			cLifeMaxTxt.Font = Font;
			cLifeMaxTxt.BackgroundColor = Color.Black;
			cLifeMaxTxt.FontColor = Color.White;
			cLifeMaxTxt.Alignment = Justify.MiddleCenter;

			cDelayLbl = new Button(GraphicsDevice, null, 280, 10, 20, 90);
			cDelayLbl.Text = "Delay mS";
			cDelayLbl.Visible = true;
			cDelayLbl.Font = Font;
			cDelayLbl.BackgroundColor = Color.Transparent;
			cDelayLbl.FontColor = LBLTEXTCOLOR;

			cDelayMinTxt = new TextBox(GraphicsDevice, null, 280, 100, 20, 90);
			cDelayMinTxt.Text = "0";
			cDelayMinTxt.Visible = true;
			cDelayMinTxt.Font = Font;
			cDelayMinTxt.BackgroundColor = Color.Black;
			cDelayMinTxt.FontColor = Color.White;
			cDelayMinTxt.Alignment = Justify.MiddleCenter;

			cDelayMaxTxt = new TextBox(GraphicsDevice, null, 280, 200, 20, 90);
			cDelayMaxTxt.Text = "50";
			cDelayMaxTxt.Visible = true;
			cDelayMaxTxt.Font = Font;
			cDelayMaxTxt.BackgroundColor = Color.Black;
			cDelayMaxTxt.FontColor = Color.White;
			cDelayMaxTxt.Alignment = Justify.MiddleCenter;

			cXDistLbl = new Button(GraphicsDevice, null, 310, 10, 20, 90);
			cXDistLbl.Text = "X Dist";
			cXDistLbl.Visible = true;
			cXDistLbl.Font = Font;
			cXDistLbl.BackgroundColor = Color.Transparent;
			cXDistLbl.FontColor = LBLTEXTCOLOR;

			cXDistMinTxt = new TextBox(GraphicsDevice, null, 310, 100, 20, 90);
			cXDistMinTxt.Text = "-300";
			cXDistMinTxt.Visible = true;
			cXDistMinTxt.Font = Font;
			cXDistMinTxt.BackgroundColor = Color.Black;
			cXDistMinTxt.FontColor = Color.White;
			cXDistMinTxt.Alignment = Justify.MiddleCenter;

			cXDistMaxTxt = new TextBox(GraphicsDevice, null, 310, 200, 20, 90);
			cXDistMaxTxt.Text = "300";
			cXDistMaxTxt.Visible = true;
			cXDistMaxTxt.Font = Font;
			cXDistMaxTxt.BackgroundColor = Color.Black;
			cXDistMaxTxt.FontColor = Color.White;
			cXDistMaxTxt.Alignment = Justify.MiddleCenter;

			cYDistLbl = new Button(GraphicsDevice, null, 340, 10, 20, 90);
			cYDistLbl.Text = "Y Dist";
			cYDistLbl.Visible = true;
			cYDistLbl.Font = Font;
			cYDistLbl.BackgroundColor = Color.Transparent;
			cYDistLbl.FontColor = LBLTEXTCOLOR;

			cYDistMinTxt = new TextBox(GraphicsDevice, null, 340, 100, 20, 90);
			cYDistMinTxt.Text = "-300";
			cYDistMinTxt.Visible = true;
			cYDistMinTxt.Font = Font;
			cYDistMinTxt.BackgroundColor = Color.Black;
			cYDistMinTxt.FontColor = Color.White;
			cYDistMinTxt.Alignment = Justify.MiddleCenter;

			cYDistMaxTxt = new TextBox(GraphicsDevice, null, 340, 200, 20, 90);
			cYDistMaxTxt.Text = "300";
			cYDistMaxTxt.Visible = true;
			cYDistMaxTxt.Font = Font;
			cYDistMaxTxt.BackgroundColor = Color.Black;
			cYDistMaxTxt.FontColor = Color.White;
			cYDistMaxTxt.Alignment = Justify.MiddleCenter;

			cRotateLbl = new Button(GraphicsDevice, null, 370, 10, 20, 90);
			cRotateLbl.Text = "Rotations";
			cRotateLbl.Visible = true;
			cRotateLbl.Font = Font;
			cRotateLbl.BackgroundColor = Color.Transparent;
			cRotateLbl.FontColor = LBLTEXTCOLOR;

			cRotateMinTxt = new TextBox(GraphicsDevice, null, 370, 100, 20, 90);
			cRotateMinTxt.Text = "-5";
			cRotateMinTxt.Visible = true;
			cRotateMinTxt.Font = Font;
			cRotateMinTxt.BackgroundColor = Color.Black;
			cRotateMinTxt.FontColor = Color.White;
			cRotateMinTxt.Alignment = Justify.MiddleCenter;

			cRotateMaxTxt = new TextBox(GraphicsDevice, null, 370, 200, 20, 90);
			cRotateMaxTxt.Text = "5";
			cRotateMaxTxt.Visible = true;
			cRotateMaxTxt.Font = Font;
			cRotateMaxTxt.BackgroundColor = Color.Black;
			cRotateMaxTxt.FontColor = Color.White;
			cRotateMaxTxt.Alignment = Justify.MiddleCenter;

			cRotateAfterBtn = new Button(GraphicsDevice, null, 400, 10, 20, 280);
			cRotateAfterBtn.Text = "Spiral Path";
			cRotateAfterBtn.Visible = true;
			cRotateAfterBtn.Font = Font;
			cRotateAfterBtn.BackgroundColor = Color.Red;
			cRotateAfterBtn.FontColor = Color.Black;
			cRotateAfterBtn.Click += RotateAfterClickHandler;

			cAlphaFadeBtn = new Button(GraphicsDevice, null, 430, 10, 20, 280);
			cAlphaFadeBtn.Text = "Alpha Fade";
			cAlphaFadeBtn.Visible = true;
			cAlphaFadeBtn.Font = Font;
			cAlphaFadeBtn.BackgroundColor = Color.Red;
			cAlphaFadeBtn.FontColor = Color.Black;
			cAlphaFadeBtn.Click += AlphaFadeClickHandler;

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
			cHeightLbl.Update(gameTime);
			cHeightMinTxt.Update(gameTime);
			cHeightMaxTxt.Update(gameTime);
			cWidthLbl.Update(gameTime);
			cWidthMinTxt.Update(gameTime);
			cWidthMaxTxt.Update(gameTime);
			cRedLbl.Update(gameTime);
			cRedMinTxt.Update(gameTime);
			cRedMaxTxt.Update(gameTime);
			cGreenLbl.Update(gameTime);
			cGreenMinTxt.Update(gameTime);
			cGreenMaxTxt.Update(gameTime);
			cBlueLbl.Update(gameTime);
			cBlueMinTxt.Update(gameTime);
			cBlueMaxTxt.Update(gameTime);
			cLifeLbl.Update(gameTime);
			cLifeMinTxt.Update(gameTime);
			cLifeMaxTxt.Update(gameTime);
			cDelayLbl.Update(gameTime);
			cDelayMinTxt.Update(gameTime);
			cDelayMaxTxt.Update(gameTime);
			cXDistLbl.Update(gameTime);
			cXDistMinTxt.Update(gameTime);
			cXDistMaxTxt.Update(gameTime);
			cYDistLbl.Update(gameTime);
			cYDistMinTxt.Update(gameTime);
			cYDistMaxTxt.Update(gameTime);
			cRotateLbl.Update(gameTime);
			cRotateMinTxt.Update(gameTime);
			cRotateMaxTxt.Update(gameTime);
			cRotateAfterBtn.Update(gameTime);
			cAlphaFadeBtn.Update(gameTime);

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

			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			cShowingLbl.Draw(cDrawBatch);
			cSettingsCont.Draw(cDrawBatch);
			cAddParitclesBtn.Draw(cDrawBatch);
			cNumParticlesLbl.Draw(cDrawBatch);
			cNumParticlesTxt.Draw(cDrawBatch);
			cHeightLbl.Draw(cDrawBatch);
			cHeightMinTxt.Draw(cDrawBatch);
			cHeightMaxTxt.Draw(cDrawBatch);
			cWidthLbl.Draw(cDrawBatch);
			cWidthMinTxt.Draw(cDrawBatch);
			cWidthMaxTxt.Draw(cDrawBatch);
			cRedLbl.Draw(cDrawBatch);
			cRedMinTxt.Draw(cDrawBatch);
			cRedMaxTxt.Draw(cDrawBatch);
			cGreenLbl.Draw(cDrawBatch);
			cGreenMinTxt.Draw(cDrawBatch);
			cGreenMaxTxt.Draw(cDrawBatch);
			cBlueLbl.Draw(cDrawBatch);
			cBlueMinTxt.Draw(cDrawBatch);
			cBlueMaxTxt.Draw(cDrawBatch);
			cLifeLbl.Draw(cDrawBatch);
			cLifeMinTxt.Draw(cDrawBatch);
			cLifeMaxTxt.Draw(cDrawBatch);
			cDelayLbl.Draw(cDrawBatch);
			cDelayMinTxt.Draw(cDrawBatch);
			cDelayMaxTxt.Draw(cDrawBatch);
			cXDistLbl.Draw(cDrawBatch);
			cXDistMinTxt.Draw(cDrawBatch);
			cXDistMaxTxt.Draw(cDrawBatch);
			cYDistLbl.Draw(cDrawBatch);
			cYDistMinTxt.Draw(cDrawBatch);
			cYDistMaxTxt.Draw(cDrawBatch);
			cRotateLbl.Draw(cDrawBatch);
			cRotateMinTxt.Draw(cDrawBatch);
			cRotateMaxTxt.Draw(cDrawBatch);
			cRotateAfterBtn.Draw(cDrawBatch);
			cAlphaFadeBtn.Draw(cDrawBatch);

			cSparkles.Draw(cDrawBatch);

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
