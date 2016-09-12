using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

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
		/// Visual object displaying the Automated overlord deck of cards
		/// </summary>
		private DeckFrame cAOFrame;
		/// <summary>
		/// Visual object displaying the overlord deck of cards
		/// </summary>
		private DeckFrame cOLFrame;

		private OverlordDecksFrame cOLConfigFrame;
		private Button cOpenConfig;

		/// <summary>
		/// Constructor to establish all graphical and interface settings as well as
		/// prepare class variables.
		/// </summary>
		public DescentApp() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += ResizeHandler;
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
			TextureFont Font;

			Content.RootDirectory = INTERFACECONTENTDIR;

			Font = new TextureFont(Content.Load<Texture2D>("Font.png"));

			try {
				cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, "Font.png", cGraphDevMgr.GraphicsDevice.Viewport.Width, cGraphDevMgr.GraphicsDevice.Viewport.Height / 2);
				cDevConsole.AccessKey = Keys.OemTilde;
				cDevConsole.UseAccessKey = true;
				cDevConsole.OpenEffect = DisplayEffect.SlideDown;
				cDevConsole.CloseEffect = DisplayEffect.SlideUp;
				cDevConsole.CommandSent += new CommandSentEventHandler(CommandEvent);
			} catch (Exception ExErr) {
				System.Windows.Forms.MessageBox.Show("Failed to initialize console: " + ExErr.GetType().ToString() + " - " + ExErr.Message);
				Exit();
				return;
			}

			cDevConsole.AddText(String.Format("Viewport Height={0} Width={1}", cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width));

			//Build card deck frames
			cAOFrame = new DeckFrame(cGraphDevMgr.GraphicsDevice, cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width / 2);
			cAOFrame.Visible = true;
			cAOFrame.CloseEffect = DisplayEffect.SlideUp;
			cAOFrame.OpenEffect = DisplayEffect.SlideDown;
			cAOFrame.Font = Font;
			cAOFrame.MaxCardsShown = 1;

			cOLFrame = new DeckFrame(cGraphDevMgr.GraphicsDevice, cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width / 2);
			cOLFrame.Left = cGraphDevMgr.GraphicsDevice.Viewport.Width / 2;
			cOLFrame.Visible = true;
			cOLFrame.CloseEffect = DisplayEffect.SlideUp;
			cOLFrame.OpenEffect = DisplayEffect.SlideDown;
			cOLFrame.Font = Font;
			cOLFrame.CardClick += OLCardClickedHandler;
			
			//build config frame
			cOpenConfig = new Button(cGraphDevMgr.GraphicsDevice, null, 0, 0, 100, 100);
			cOpenConfig.Font = Font;
			cOpenConfig.FontColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
			cOpenConfig.BackgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
			cOpenConfig.Text = "Config";
			cOpenConfig.Visible = true;
			cOpenConfig.Click += ConfigClickHandler;

			cOLConfigFrame = new OverlordDecksFrame(cGraphDevMgr.GraphicsDevice, cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width);
			cOLConfigFrame.Visible = false;
			cOLConfigFrame.OpenEffect = DisplayEffect.SlideUp;
			cOLConfigFrame.CloseEffect = DisplayEffect.SlideDown;
			cOLConfigFrame.Font = Font;

			//Load config files
			cDevConsole.AddText(LoadGameDecks(Content, INTERFACECONTENTDIR + Path.DirectorySeparatorChar + "AOCardsList.xml"));

			cAOFrame.ShuffleCompleteDeck(true, false);
			cAOFrame.SelectRandomCardBack();
			cOLFrame.ShuffleCompleteDeck(true, false);
			cOLFrame.SelectRandomCardBack();
		}

		/// <summary>
		/// Update game state and all content.  Should be called once every redraw, possibly called 
		/// more often than the draws but only if it exceeds time between frame draws.
		/// </summary>
		/// <param name="gameTime">Current time information of the application</param>
		protected override void Update(GameTime gameTime) {
			cDevConsole.Update(gameTime);
			cAOFrame.Update(gameTime);
			cOLFrame.Update(gameTime);

			cOLConfigFrame.Update(gameTime);
			cOpenConfig.Update(gameTime);

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

			cAOFrame.Draw(cDrawBatch);
			cOLFrame.Draw(cDrawBatch);

			cOLConfigFrame.Draw(cDrawBatch);
			cOpenConfig.Draw(cDrawBatch);

			//Always draw console last
			cDevConsole.Draw(cDrawBatch);

			cDrawBatch.End();

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
			} else if (RegEx.LooseTest(CommandEvent, @"^\s*(ao|overlord)( *frame)? *hide\s*$") == true) {
				cAOFrame.Visible = false;
			} else if (RegEx.LooseTest(CommandEvent, @"^\s*(ao|overlord)( *frame)? *show\s*$") == true) {
				cAOFrame.Visible = true;
			}else {
				cDevConsole.AddText("Unrecognized command: " + CommandEvent);
			}
		}

		/// <summary>
		/// Handler for the resize window event.  Ensures all controls are properly adjusted and scaled
		/// </summary>
		/// <param name="Sender">Objct triggering the event, main window</param>
		/// <param name="Args">Arguments passed with the event</param>
		private void ResizeHandler(object Sender, EventArgs Args) {
			if (cDevConsole == null) { //The game failed to load and should be quitting
				return;
			}
			cDevConsole.Width = Window.ClientBounds.Width;

			cAOFrame.Width = Window.ClientBounds.Width / 2;
			cAOFrame.Height = Window.ClientBounds.Height;

			cOLFrame.Left = Window.ClientBounds.Width / 2;
			cOLFrame.Width = Window.ClientBounds.Width / 2;
			cOLFrame.Height = Window.ClientBounds.Height;

			cOpenConfig.Height = Window.ClientBounds.Height / 10;
			cOpenConfig.Width = Window.ClientBounds.Width / 10;

			if (cOpenConfig.Width < cOpenConfig.Height) {
				cOpenConfig.Height = cOpenConfig.Width;
			} else {
				cOpenConfig.Width = cOpenConfig.Height;
			}

			cOpenConfig.Top = Window.ClientBounds.Height - cOpenConfig.Height;
			cOpenConfig.Left = Window.ClientBounds.Width - cOpenConfig.Width;
			cOpenConfig.FontSize = cOpenConfig.Height / 6;

			if (cOLConfigFrame.Visible == true) {
				cOLConfigFrame.Height = Window.ClientBounds.Height;
				cOLConfigFrame.Width = Window.ClientBounds.Width / 4;
			}
		}

		private void ConfigClickHandler(object Sender, MouseButton Button) {
			cAOFrame.Visible = !cAOFrame.Visible;
			cOLFrame.Visible = !cOLFrame.Visible;
			cOLConfigFrame.Visible = !cOLConfigFrame.Visible;

			if (cOLConfigFrame.Visible == true) {
				cOLConfigFrame.Height = Window.ClientBounds.Height;
				cOLConfigFrame.Width = Window.ClientBounds.Width / 4;
			}
		}

		private void OLCardClickedHandler(object Sender, int CardIndex, MouseButton Button) {
			if (Button == MouseButton.Right) {
				cOLFrame.RemoveShownCard(CardIndex);
			}
		}

		/// <summary>
		/// Loads all cards and decks from a common XML file
		/// </summary>
		/// <param name="Content">COntent manager object that will load the image files</param>
		/// <param name="DeckXMLFile">Path and file name of the XML file containing the card information</param>
		/// <returns>Text messages reporting any information related to loaded the deck information and images</returns>
		private string LoadGameDecks(ContentManager Content, string DeckXMLFile) {
			XmlDocument DeckXML;
			XmlNodeList CardNodeList;
			Texture2D CardImage;
			string Message = "";
			int Ctr, CardCnt;
			OverlordCard NewOLCard;

			Message = "Loading card list from " + DeckXMLFile + "\n";
			cOLFrame.CardBackList.Clear();
			cOLFrame.CardFaceList.Clear();

			try {
				DeckXML = new XmlDocument();
				DeckXML.Load(DeckXMLFile);
			} catch (Exception ExErr) {
				Message += String.Format("Failed to load XML file: {0} - {1}\n", ExErr.GetType().ToString(), ExErr.Message);
				return Message;
			}

			CardNodeList = DeckXML.DocumentElement.SelectNodes("//cardlist");
			Ctr = 0;
			foreach (XmlNode CardNode in CardNodeList) {
				Ctr += 1;
				foreach (XmlNode Tag in CardNode.ChildNodes) {
					if (Tag.Attributes == null) {
						Message += String.Format("Tag {0} '{1}' contains no attributes, skipping.\n", Ctr, Tag.Name);
						continue;
					}

					switch (Tag.Name) {
						case "autooverlord":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);
								cAOFrame.CardFaceList.Add(CardImage);
							} else {
								Message += String.Format("Tag {0} '{1}' contains no image attribute, skipping.\n", Ctr, Tag.Name);
							}

							break;
						case "autooverlordback":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);
								cAOFrame.CardBackList.Add(CardImage);
							} else {
								Message += String.Format("Tag {0} '{1}' contains no image attribute, skipping.\n", Ctr, Tag.Name);
							}

							break;
						case "reference":
							/*
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);
							} else {
								Message += String.Format("Tag {0} '{1}' contains no image attribute, skipping.\n", Ctr, Tag.Name);
								continue;
							}

							if (Tag.Attributes["description"] != null) {
								cAORefCards.Add(Tag.Attributes["description"].InnerText, CardImage);
							} else {
								Message += String.Format("Tag {0} '{1}' contains no description attribute, skipping.\n", Ctr, Tag.Name);
								continue;
							}
							*/
							break;
						case "overlordback":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);

								try {
									CardCnt = Int32.Parse(Tag.Attributes["count"].InnerText);
								} catch (Exception) {
									CardCnt = 1;
								}

								cOLFrame.CardBackList.Add(CardImage);
							} else {
								Message += String.Format("Tag {0} '{1}' contains no image attribute, skipping.\n", Ctr, Tag.Name);
							}

							break;
						case "overlord":
							if (Tag.Attributes["image"] != null) {
								/*
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);

								try {
									CardCnt = Int32.Parse(Tag.Attributes["count"].InnerText);
								} catch (Exception) {
									CardCnt = 1;
								}

								while (CardCnt > 0) {
									cOLFrame.CardFaceList.Add(CardImage);
									CardCnt -= 1;
								}
								 */

								NewOLCard = new OverlordCard();
								NewOLCard.Image = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);
								NewOLCard.Count = Int32.Parse(Tag.Attributes["count"].InnerText);
								NewOLCard.Include = 0;
								NewOLCard.Class = Tag.Attributes["class"].InnerText;
								NewOLCard.Set = Tag.Attributes["set"].InnerText;

								cOLConfigFrame.AddOverlordCard(NewOLCard);
							} else {
								Message += String.Format("Tag {0} '{1}' contains no image attribute, skipping.\n", Ctr, Tag.Name);
							}

							break;
						default:
							if (Tag.Name.CompareTo("#comment") == 0) {
								Message += "Found text '" + Tag.InnerText + "' outisde any tag.\n";
							} else {
								Message += "Unrecognized tag '" + Tag.Name + "'.\n";
							}
							break;
					}
				}
			}

			Message += String.Format("Card decks loading complete.\n");

			return Message;
		}
	}
}
