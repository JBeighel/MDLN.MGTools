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

		private AutomatedOverlordFrame cAOFrame;
		private DeckFrame cOLFrame;

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
			Content.RootDirectory = INTERFACECONTENTDIR;

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

			cAOFrame = new AutomatedOverlordFrame(cGraphDevMgr.GraphicsDevice, cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width / 2);
			cDevConsole.AddText(cAOFrame.LoadContent(Content, INTERFACECONTENTDIR + Path.DirectorySeparatorChar + "AOCardsList.xml"));
			cAOFrame.Visible = true;
			cAOFrame.CloseEffect = DisplayEffect.SlideUp;
			cAOFrame.OpenEffect = DisplayEffect.SlideDown;

			cOLFrame = new DeckFrame(cGraphDevMgr.GraphicsDevice, cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width / 2);
			cOLFrame.Left = cGraphDevMgr.GraphicsDevice.Viewport.Width / 2;
			//cDevConsole.AddText(cOLFrame.LoadContent(Content, INTERFACECONTENTDIR + Path.DirectorySeparatorChar + "AOCardsList.xml"));
			cOLFrame.Visible = true;
			cOLFrame.CloseEffect = DisplayEffect.SlideUp;
			cOLFrame.OpenEffect = DisplayEffect.SlideDown;
			cOLFrame.Font = new TextureFont(Content.Load<Texture2D>("Font.png"));
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
		}
	}

	public class AutomatedOverlordFrame : Container {
		/// <summary>
		/// Standard cards are 2.5 wide and 3.5 tall, so W/H = this ratio
		/// This gives us a scaling factor when resizing cards on screen
		/// </summary>
		private const double CARDRATIO = 0.714;
		private const double MARGINPERCENT = 0.02;
		private const double CARDWIDTHPERCENT = 0.40;
		private const int CARDZOOMSTEPS = 10;

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

		private bool cZoomCurrCard;
		private byte cZoomStep;

		private Button cAOCardBackCtrl;
		private Button cAOCurrCardCtrl;

		private Random cRandomNumber;

		public AutomatedOverlordFrame(GraphicsDevice GraphDev, int Height, int Width)
			: base(GraphDev, null, 0, 0, Height, Width) {
			cAORefCards = new Dictionary<string, Texture2D>();
			cAOCardBacks = new List<Texture2D>();
			cAOCards = new List<Texture2D>();

			cRandomNumber = new Random();

			cZoomCurrCard = false;
		}

		public string LoadContent(ContentManager Content, string DeckXMLFile) {
			int Index;
			string Message;

			BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

			Message = LoadAutoOverlordDeck(Content, DeckXMLFile);

			if (cAOCardBacks.Count > 0) {
				Index = cRandomNumber.Next(0, cAOCardBacks.Count);
				cAOCardBackCtrl = new Button(cGraphicsDevice, cAOCardBacks[Index], 10, 10, 175, 125);
			} else {
				cAOCardBackCtrl = new Button(cGraphicsDevice, null, 10, 10, 175, 125);
				cAOCardBackCtrl.BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			}
			cAOCardBackCtrl.Visible = true;
			cAOCardBackCtrl.Click += AOCardBackClickHandler;

			cAOCurrCardCtrl = new Button(cGraphicsDevice, null, 10, 145, 175, 125);
			cAOCurrCardCtrl.BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			cAOCurrCardCtrl.Visible = false;
			cAOCurrCardCtrl.Click += AOCardFaceClickHandler;

			return Message;
		}

		protected override void DrawContents(GameTime gameTime) {
			cAOCardBackCtrl.Draw(cDrawBatch);
			cAOCurrCardCtrl.Draw(cDrawBatch);
		}

		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
			int CardMargin, CardHeight, CardWidth;
			Rectangle CurrCardRegion;

			CardMargin = (int)(ClientRegion.Width * MARGINPERCENT);
			CardWidth = (int)(ClientRegion.Width * CARDWIDTHPERCENT);
			CardHeight = (int)(CardWidth / CARDRATIO);
			
			if (cAOCardBackCtrl.Top != CardMargin) {
				cAOCardBackCtrl.Top = CardMargin;
				cAOCardBackCtrl.Left = CardMargin;
				HasChanged = true;
			}

			if (cAOCardBackCtrl.Width != CardWidth) {
				cAOCardBackCtrl.Height = CardHeight;
				cAOCardBackCtrl.Width = CardWidth;
				HasChanged = true;
			}

			CurrCardRegion = DetermineCurrentCardRegion(CurrTime.ElapsedGameTime.TotalMilliseconds);

			if ((cAOCurrCardCtrl.Top != CurrCardRegion.Y) || (cAOCurrCardCtrl.Width != CurrCardRegion.Width)) {
				cAOCurrCardCtrl.Top = CurrCardRegion.Y;
				cAOCurrCardCtrl.Left = CurrCardRegion.X;
				cAOCurrCardCtrl.Height = CurrCardRegion.Height;
				cAOCurrCardCtrl.Width = CurrCardRegion.Width;
				HasChanged = true;
			}

			cAOCardBackCtrl.Update(CurrTime, CurrKeyboard, CurrMouse);
			cAOCurrCardCtrl.Update(CurrTime, CurrKeyboard, CurrMouse);
		}

		private void AOCardBackClickHandler(object Sender, MouseButton Button) {
			if (cZoomCurrCard == false) { //Only draw new cards when current card isn't zoomed
				int Index = cRandomNumber.Next(0, cAOCards.Count);

				cAOCurrCardCtrl.Visible = true;
				cAOCurrCardCtrl.Background = cAOCards[Index];
				HasChanged = true;
			}
		}

		private void AOCardFaceClickHandler(object Sender, MouseButton Button) {
			cZoomCurrCard = !cZoomCurrCard;
			cZoomStep = 0;
		}

		/// <summary>
		/// Parses an XML file that contains all information regarding the automated overlord deck.  Assumes all
		/// image files are stored in the content directory.
		/// </summary>
		/// <param name="DeckXMLFile">Path and filename to the XML file containing deck information</param>
		private string LoadAutoOverlordDeck(ContentManager Content, string DeckXMLFile) {
			XmlDocument DeckXML;
			XmlNodeList CardNodeList;
			Texture2D CardImage;
			string Message = "";
			int Ctr;

			Message = "Loading card list from " + DeckXMLFile + "\n";
			cAORefCards.Clear();
			cAOCardBacks.Clear();
			cAOCards.Clear();

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
						Message +=  String.Format("Tag {0} '{1}' contains no attributes, skipping.\n", Ctr, Tag.Name);
						continue;
					}

					switch (Tag.Name) {
						case "autooverlord":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);
								cAOCards.Add(CardImage);
							} else {
								Message += String.Format("Tag {0} '{1}' contains no image attribute, skipping.\n", Ctr, Tag.Name);
							}

							break;
						case "autooverlordback":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);
								cAOCardBacks.Add(CardImage);
							} else {
								Message += String.Format("Tag {0} '{1}' contains no image attribute, skipping.\n", Ctr, Tag.Name);
							}

							break;
						case "reference":
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

							break;
						case "overlord" :
							//These need loaded, but no spot exists for that, so skipping them
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

			Message += String.Format("Automated Overlord deck loaded.  {0} Cards, {1} Backs, {2} References\n", cAOCards.Count, cAOCardBacks.Count, cAORefCards.Count);

			return Message;
		}

		private Rectangle DetermineCurrentCardRegion(double EllapsedTime) {
			Rectangle Region = new Rectangle();
			Rectangle ZoomIn = new Rectangle();
			Rectangle ZoomOut = new Rectangle();
			int MoveAmnt;

			ZoomOut.X = (int)((ClientRegion.Width * MARGINPERCENT * 2) + (ClientRegion.Width * CARDWIDTHPERCENT));
			ZoomOut.Y = (int)(ClientRegion.Width * MARGINPERCENT);
			ZoomOut.Width = (int)(ClientRegion.Width * CARDWIDTHPERCENT);
			ZoomOut.Height = (int)(ZoomOut.Width / CARDRATIO);

			if (ClientRegion.Width / CARDRATIO < ClientRegion.Height) {
				ZoomIn.Width = ClientRegion.Width;
				ZoomIn.Height = (int)(ClientRegion.Width / CARDRATIO);
			} else {
				ZoomIn.Height = ClientRegion.Height;
				ZoomIn.Width = (int)(ClientRegion.Height * CARDRATIO);
			}
			ZoomIn.X = (ClientRegion.Width - ZoomIn.Width) / 2;
			ZoomIn.Y = (ClientRegion.Height - ZoomIn.Height) / 2;

			if (cZoomStep < CARDZOOMSTEPS) {
				cZoomStep += 1;

				if (cZoomCurrCard == true) {
					MoveAmnt = ZoomIn.X - ZoomOut.X;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.X = MoveAmnt + ZoomOut.X;

					MoveAmnt = ZoomIn.Y - ZoomOut.Y;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Y = MoveAmnt + ZoomOut.Y;

					MoveAmnt = ZoomIn.Height - ZoomOut.Height;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Height = MoveAmnt + ZoomOut.Height;

					MoveAmnt = ZoomIn.Width - ZoomOut.Width;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Width = MoveAmnt + ZoomOut.Width;
				} else {
					MoveAmnt = ZoomOut.X - ZoomIn.X;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.X = MoveAmnt + ZoomIn.X;

					MoveAmnt = ZoomOut.Y - ZoomIn.Y;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Y = MoveAmnt + ZoomIn.Y;

					MoveAmnt = ZoomOut.Height - ZoomIn.Height;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Height = MoveAmnt + ZoomIn.Height;

					MoveAmnt = ZoomOut.Width - ZoomIn.Width;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Width = MoveAmnt + ZoomIn.Width;
				}
			} else {
				if (cZoomCurrCard == true) {
					return ZoomIn;
				} else {
					return ZoomOut;
				}
			}

			return Region;
		}
	}

	public class OverlordFrame : Container {
		/// <summary>
		/// Standard cards are 2.5 wide and 3.5 tall, so W/H = this ratio
		/// This gives us a scaling factor when resizing cards on screen
		/// </summary>
		private const double CARDRATIO = 0.714;
		private const double MARGINPERCENT = 0.02;
		private const double CARDWIDTHPERCENT = 0.40;
		private const int CARDZOOMSTEPS = 10;

		/// <summary>
		/// Collection of images for use as backs for the automated overlord deck
		/// </summary>
		private List<Texture2D> cCardBacks;
		/// <summary>
		/// Collection of images for all cards in the automated overlord deck
		/// </summary>
		private List<Texture2D> cCards;

		private bool cZoomCurrCard;
		private byte cZoomStep;

		private Button cCardBackCtrl;
		private Button cAOCurrCardCtrl;

		private Random cRandomNumber;

		public OverlordFrame(GraphicsDevice GraphDev, int Height, int Width)
			: base(GraphDev, null, 0, 0, Height, Width) {
			cCardBacks = new List<Texture2D>();
			cCards = new List<Texture2D>();

			cRandomNumber = new Random();

			cZoomCurrCard = false;
		}

		public string LoadContent(ContentManager Content, string DeckXMLFile) {
			int Index;
			string Message;

			BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

			Message = LoadOverlordDeck(Content, DeckXMLFile);

			if (cCardBacks.Count > 0) {
				Index = cRandomNumber.Next(0, cCardBacks.Count);
				cCardBackCtrl = new Button(cGraphicsDevice, cCardBacks[Index], 10, 10, 175, 125);
			} else {
				cCardBackCtrl = new Button(cGraphicsDevice, null, 10, 10, 175, 125);
				cCardBackCtrl.BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			}
			cCardBackCtrl.Visible = true;
			cCardBackCtrl.Click += CardBackClickHandler;

			cAOCurrCardCtrl = new Button(cGraphicsDevice, null, 10, 145, 175, 125);
			cAOCurrCardCtrl.BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			cAOCurrCardCtrl.Visible = false;
			cAOCurrCardCtrl.Click += AOCardFaceClickHandler;

			return Message;
		}

		protected override void DrawContents(GameTime gameTime) {
			cCardBackCtrl.Draw(cDrawBatch);
			cAOCurrCardCtrl.Draw(cDrawBatch);
		}

		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
			int CardMargin, CardHeight, CardWidth;
			Rectangle CurrCardRegion;

			CardMargin = (int)(ClientRegion.Width * MARGINPERCENT);
			CardWidth = (int)(ClientRegion.Width * CARDWIDTHPERCENT);
			CardHeight = (int)(CardWidth / CARDRATIO);

			if (cCardBackCtrl.Top != CardMargin) {
				cCardBackCtrl.Top = CardMargin;
				cCardBackCtrl.Left = CardMargin;
				HasChanged = true;
			}

			if (cCardBackCtrl.Width != CardWidth) {
				cCardBackCtrl.Height = CardHeight;
				cCardBackCtrl.Width = CardWidth;
				HasChanged = true;
			}

			CurrCardRegion = DetermineCurrentCardRegion(CurrTime.ElapsedGameTime.TotalMilliseconds);

			if ((cAOCurrCardCtrl.Top != CurrCardRegion.Y) || (cAOCurrCardCtrl.Width != CurrCardRegion.Width)) {
				cAOCurrCardCtrl.Top = CurrCardRegion.Y;
				cAOCurrCardCtrl.Left = CurrCardRegion.X;
				cAOCurrCardCtrl.Height = CurrCardRegion.Height;
				cAOCurrCardCtrl.Width = CurrCardRegion.Width;
				HasChanged = true;
			}

			cCardBackCtrl.Update(CurrTime, CurrKeyboard, CurrMouse);
			cAOCurrCardCtrl.Update(CurrTime, CurrKeyboard, CurrMouse);
		}

		private void CardBackClickHandler(object Sender, MouseButton Button) {
			if (cZoomCurrCard == false) { //Only draw new cards when current card isn't zoomed
				int Index = cRandomNumber.Next(0, cCards.Count);

				cAOCurrCardCtrl.Visible = true;
				cAOCurrCardCtrl.Background = cCards[Index];
				HasChanged = true;
			}
		}

		private void AOCardFaceClickHandler(object Sender, MouseButton Button) {
			cZoomCurrCard = !cZoomCurrCard;
			cZoomStep = 0;
		}

		/// <summary>
		/// Parses an XML file that contains all information regarding the automated overlord deck.  Assumes all
		/// image files are stored in the content directory.
		/// </summary>
		/// <param name="DeckXMLFile">Path and filename to the XML file containing deck information</param>
		private string LoadOverlordDeck(ContentManager Content, string DeckXMLFile) {
			XmlDocument DeckXML;
			XmlNodeList CardNodeList;
			Texture2D CardImage;
			string Message = "";
			int Ctr, CardCnt;

			Message = "Loading card list from " + DeckXMLFile + "\n";
			cCardBacks.Clear();
			cCards.Clear();

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
						case "autooverlordback":
						case "reference":
							//Valid tags, but unused
							break;
						case "overlordback":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);

								try {
									CardCnt = Int32.Parse(Tag.Attributes["count"].InnerText);
								} catch (Exception) {
									CardCnt = 1;
								}

								cCardBacks.Add(CardImage);
							} else {
								Message += String.Format("Tag {0} '{1}' contains no image attribute, skipping.\n", Ctr, Tag.Name);
							}

							break;
						case "overlord":
							if (Tag.Attributes["image"] != null) {
								CardImage = Content.Load<Texture2D>(Tag.Attributes["image"].InnerText);

								try {
									CardCnt = Int32.Parse(Tag.Attributes["count"].InnerText);
								} catch (Exception) {
									CardCnt = 1;
								}

								while (CardCnt > 0) {
									cCards.Add(CardImage);
									CardCnt -= 1;
								}
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

			Message += String.Format("Overlord deck loaded.  {0} Cards, {1} Backs\n", cCards.Count, cCardBacks.Count);

			return Message;
		}

		private Rectangle DetermineCurrentCardRegion(double EllapsedTime) {
			Rectangle Region = new Rectangle();
			Rectangle ZoomIn = new Rectangle();
			Rectangle ZoomOut = new Rectangle();
			int MoveAmnt;

			ZoomOut.X = (int)((ClientRegion.Width * MARGINPERCENT * 2) + (ClientRegion.Width * CARDWIDTHPERCENT));
			ZoomOut.Y = (int)(ClientRegion.Width * MARGINPERCENT);
			ZoomOut.Width = (int)(ClientRegion.Width * CARDWIDTHPERCENT);
			ZoomOut.Height = (int)(ZoomOut.Width / CARDRATIO);

			if (ClientRegion.Width / CARDRATIO < ClientRegion.Height) {
				ZoomIn.Width = ClientRegion.Width;
				ZoomIn.Height = (int)(ClientRegion.Width / CARDRATIO);
			} else {
				ZoomIn.Height = ClientRegion.Height;
				ZoomIn.Width = (int)(ClientRegion.Height * CARDRATIO);
			}
			ZoomIn.X = (ClientRegion.Width - ZoomIn.Width) / 2;
			ZoomIn.Y = (ClientRegion.Height - ZoomIn.Height) / 2;

			if (cZoomStep < CARDZOOMSTEPS) {
				cZoomStep += 1;

				if (cZoomCurrCard == true) {
					MoveAmnt = ZoomIn.X - ZoomOut.X;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.X = MoveAmnt + ZoomOut.X;

					MoveAmnt = ZoomIn.Y - ZoomOut.Y;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Y = MoveAmnt + ZoomOut.Y;

					MoveAmnt = ZoomIn.Height - ZoomOut.Height;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Height = MoveAmnt + ZoomOut.Height;

					MoveAmnt = ZoomIn.Width - ZoomOut.Width;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Width = MoveAmnt + ZoomOut.Width;
				} else {
					MoveAmnt = ZoomOut.X - ZoomIn.X;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.X = MoveAmnt + ZoomIn.X;

					MoveAmnt = ZoomOut.Y - ZoomIn.Y;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Y = MoveAmnt + ZoomIn.Y;

					MoveAmnt = ZoomOut.Height - ZoomIn.Height;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Height = MoveAmnt + ZoomIn.Height;

					MoveAmnt = ZoomOut.Width - ZoomIn.Width;
					MoveAmnt = (int)(((float)cZoomStep / CARDZOOMSTEPS) * MoveAmnt);
					Region.Width = MoveAmnt + ZoomIn.Width;
				}
			} else {
				if (cZoomCurrCard == true) {
					return ZoomIn;
				} else {
					return ZoomOut;
				}
			}

			return Region;
		}
	}
}
