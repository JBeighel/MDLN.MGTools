using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
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
		/// Collection of card images for all of the reference cards in the deck
		/// Index is the card description, value is the card image/texture
		/// </summary>
		private Dictionary<string, Texture2D> cAORefCards;
		/// <summary>
		/// Collection of images for use as backs for the automated overlord deck
		/// </summary>
		private List<Texture2D> cAOCardBacks;
		/// <summary>
		/// Collection of images for all cards in the automated overlord deck
		/// </summary>
		private List<Texture2D> cAOCards;

		private Button cAOCardBackCtrl;
		private Button cAOCurrCardCtrl;

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
			cAORefCards = new Dictionary<string, Texture2D>();
			cAOCardBacks = new List<Texture2D>();
			cAOCards = new List<Texture2D>();

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
			Content.RootDirectory = INTERFACECONTENTDIR;

			cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, "Font.png", cGraphDevMgr.GraphicsDevice.Viewport.Width, cGraphDevMgr.GraphicsDevice.Viewport.Height / 2);
			cDevConsole.AccessKey = Keys.OemTilde;
			cDevConsole.UseAccessKey = true;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;
			cDevConsole.CommandSent += new CommandSentEventHandler(CommandEvent);

			cDevConsole.AddText(String.Format("Viewport Height={0} Width={1}", cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width));

			LoadAutoOverlordDeck(INTERFACECONTENTDIR + Path.DirectorySeparatorChar + "AOCardsList.xml");
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

		/// <summary>
		/// Handler for the resize window event.  Ensures all controls are properly adjusted and scaled
		/// </summary>
		/// <param name="Sender">Objct triggering the event, main window</param>
		/// <param name="Args">Arguments passed with the event</param>
		private void ResizeHandler(object Sender, EventArgs Args) {
			cDevConsole.Width = Window.ClientBounds.Width;
		}

		/// <summary>
		/// Parses an XML file that contains all information regarding the automated overlord deck.  Assumes all
		/// image files are stored in the content directory.
		/// </summary>
		/// <param name="DeckXMLFile">Path and filename to the XML file containing deck information</param>
		private void LoadAutoOverlordDeck(string DeckXMLFile) {
			XmlDocument DeckXML;
			XmlNodeList CardNodeList;
			Texture2D CardImage;
			int Ctr;

			cDevConsole.AddText("Loading monster cards from " + DeckXMLFile);
			cAORefCards.Clear();
			cAOCardBacks.Clear();
			cAOCards.Clear();

			try {
				DeckXML = new XmlDocument();
				DeckXML.Load(DeckXMLFile);
			} catch (Exception ExErr) {
				cDevConsole.AddText(String.Format("Failed to load XML file: {0} - {1}", ExErr.GetType().ToString(), ExErr.Message));
				return;
			}

			CardNodeList = DeckXML.DocumentElement.SelectNodes("//aocards");
			Ctr = 0;
			foreach (XmlNode CardNode in CardNodeList) {
				Ctr += 1;
				foreach (XmlNode Tag in CardNode.ChildNodes) {
					if (Tag.Attributes == null) {
						cDevConsole.AddText(String.Format("Tag {0} '{1}' contains no attributes, skipping.", Ctr, Tag.Name));
						continue;
					}

					switch (Tag.Name) {
						case "card":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);
								cAOCards.Add(CardImage);
							} else {
								cDevConsole.AddText(String.Format("Tag {0} '{1}' contains no image attribute, skipping.", Ctr, Tag.Name));
							}
							
							break;
						case "back":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);
								cAOCardBacks.Add(CardImage);
							} else {
								cDevConsole.AddText(String.Format("Tag {0} '{1}' contains no image attribute, skipping.", Ctr, Tag.Name));
							}
							
							break;
						case "reference":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);
							} else {
								cDevConsole.AddText(String.Format("Tag {0} '{1}' contains no image attribute, skipping.", Ctr, Tag.Name));
								continue;
							}

							if (Tag.Attributes["description"] != null) {
								cAORefCards.Add(Tag.Attributes["description"].InnerText, CardImage);
							} else {
								cDevConsole.AddText(String.Format("Tag {0} '{1}' contains no description attribute, skipping.", Ctr, Tag.Name));
								continue;
							}

							break;
						default:
							if (Tag.Name.CompareTo("#comment") == 0) {
								cDevConsole.AddText("Found text '" + Tag.InnerText + "' outisde any tag.");
							} else {
								cDevConsole.AddText("Unrecognized tag '" + Tag.Name + "'.");
							}
							break;
					}
				}
			}

			cDevConsole.AddText(String.Format("Automated Overlord deck loaded.  {0} Cards, {1} Backs, {2} References", cAOCards.Count, cAOCardBacks.Count, cAORefCards.Count));
		}
	}
}
