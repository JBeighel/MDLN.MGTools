using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ISOTiles {
	class Launcher {
		static void Main(string[] Args) {
			using (ISOTiles Game = new ISOTiles()) {
				Game.Run();
			}
		}
	}

	/// <summary>
	/// Experimental project to work out how to load a tileset image and display 
	/// a map built from layered floor tiles.
	/// </summary>
	class ISOTiles : Game {
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
		private TileSetManager cTileSets;
		
		/// <summary>
		/// Constructor to initialize class variables
		/// </summary>
		public ISOTiles() {
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
				cDevConsole.CommandSent += new CommandSentEventHandler(ConsoleCommandEvent);
			} catch (Exception ExErr) {
				System.Windows.Forms.MessageBox.Show("Failed to initialize console: " + ExErr.GetType().ToString() + " - " + ExErr.Message);
				Exit();
				return;
			}

			cTileSets = new TileSetManager(Content, INTERFACECONTENTDIR + @"\Tiles.XML", "/tilesets");
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
			Rectangle Position;

			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			Position.X = 0;
			Position.Y = 0;
			Position.Width = 48;
			Position.Height = 48;
			for (int Ctr = 0; Ctr < 5; Ctr++) {
				cTileSets.DrawTile("floors", Ctr, 0, cDrawBatch, Position, Color.White);

				Position.X += 52;
				Position.Y += 52;
			}

			Position.Y = 0;
			for (int Ctr = 0; Ctr < 5; Ctr++) {
				cTileSets.DrawTile("floors small", Ctr, 0, cDrawBatch, Position, Color.White);

				Position.X += 52;
				Position.Y += 52;
			}

			//Always draw console last
			cDevConsole.Draw(cDrawBatch);

			cDrawBatch.End();

			//Use monogame draw
			base.Draw(gameTime);
		}

		/// <summary>
		/// Event handler for when the window is resized
		/// </summary>
		/// <param name="Sender">Object raising the event</param>
		/// <param name="Args">Any arguments decscribing the event</param>
		private void ResizeHandler(object Sender, EventArgs Args) {
			cDevConsole.Width = Window.ClientBounds.Width;
		}

		/// <summary>
		/// Event handler for commands entered into the developer console
		/// </summary>
		/// <param name="Sender">Object raising the event</param>
		/// <param name="CommandEvent">The text string entered</param>
		private void ConsoleCommandEvent(object Sender, string CommandEvent) {

		}
	}

	public class TileSetManager {
		private Dictionary<string, TileSetInfo> cTileSetList;

		/// <summary>
		/// Basic constructor, initializes the class but assumes all information will be loaded later
		/// </summary>
		/// <param name="ContentMgr"></param>
		public TileSetManager() : this(null, null, null) { }

		/// <summary>
		/// COnstructor that attempts to load all data during initialization
		/// </summary>
		/// <param name="ContentMgr">The content manager to use when loading image data</param>
		/// <param name="XMLFile">The path to the XML file containing the tileset information</param>
		/// <param name="RootPath">The root path in the XML file to find the tile information tags</param>
		public TileSetManager(ContentManager ContentMgr, string XMLFile, string RootPath) {
			cTileSetList = new Dictionary<string, TileSetInfo>();

			if ((ContentMgr != null) && (XMLFile != null) && (RootPath != null)) {
				LoadTileSetXML(ContentMgr, XMLFile, RootPath);
			}
		}

		/// <summary>
		/// Constructor that attempts to load all data during initialization.  This will discard any 
		/// ppreviously loaded data and replace it with this new information.
		/// </summary>
		/// <param name="ContentMgr">The content manager to use when loading image data</param>
		/// <param name="XMLFile">The path to the XML file containing the tileset information</param>
		/// <param name="RootPath">The root path in the XML file to find the tile information tags</param>
		public void LoadTileSetXML(ContentManager ContentMgr, string XMLFile, string RootPath) {
			XmlDocument TilesXML;
			XmlNodeList TilesList;
			TileSetInfo NewSet;

			cTileSetList.Clear();

			try {
				TilesXML = new XmlDocument();
				TilesXML.Load(XMLFile);
			} catch (Exception ExErr) {
				throw new Exception(String.Format("Failed to load XML File {1}{0}Exception {2}{0}Message {3}", Environment.NewLine, XMLFile, ExErr.GetType().ToString(), ExErr.Message));
			}

			TilesList = TilesXML.DocumentElement.SelectNodes(RootPath + "/tiles");
			foreach (XmlNode TileNode in TilesList) {
				NewSet = new TileSetInfo();

				//Load all tile set details from the XML file
				if (TileNode.Attributes["name"] != null) {
					NewSet.Name = TileNode.Attributes["name"].InnerText;
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag with no name attribute.", Environment.NewLine, XMLFile));
				}

				if (TileNode.Attributes["file"] != null) {
					NewSet.ImageFile = TileNode.Attributes["file"].InnerText;
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with no file attribute.", Environment.NewLine, XMLFile, NewSet.Name));
				}

				if (TileNode.Attributes["columns"] != null) {
					if (Int32.TryParse(TileNode.Attributes["columns"].InnerText, out NewSet.ColumnCnt) == false) {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with an invalid columns attribute.", Environment.NewLine, XMLFile, NewSet.Name));
					}
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with no columns attribute.", Environment.NewLine, XMLFile, NewSet.Name));
				}

				if (TileNode.Attributes["rows"] != null) {
					if (Int32.TryParse(TileNode.Attributes["columns"].InnerText, out NewSet.RowCnt) == false) {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with an invalid rows attribute.", Environment.NewLine, XMLFile, NewSet.Name));
					}
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with no rows attribute.", Environment.NewLine, XMLFile, NewSet.Name));
				}

				if (TileNode.Attributes["columnwidth"] != null) {
					if (Int32.TryParse(TileNode.Attributes["columnwidth"].InnerText, out NewSet.TileWidth) == false) {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with an invalid columnwidth attribute.", Environment.NewLine, XMLFile, NewSet.Name));
					}
				} else {
					NewSet.TileWidth = -1;
				}

				if (TileNode.Attributes["rowheight"] != null) {
					if (Int32.TryParse(TileNode.Attributes["rowheight"].InnerText, out NewSet.TileHeight) == false) {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with an invalid rowheight attribute.", Environment.NewLine, XMLFile, NewSet.Name));
					}
				} else {
					NewSet.TileHeight = -1;
				}

				//Load the image data for use
				NewSet.TileTexture = ContentMgr.Load<Texture2D>(NewSet.ImageFile);

				//Add this tile set to the list
				cTileSetList.Add(NewSet.Name, NewSet);
			}
		}

		public void DrawTile(string SetName, int ColNum, int RowNum, SpriteBatch DrawBatch, Rectangle DrawPos, Color TintColor) {
			TileSetInfo TileSet;
			Vector2 Origin; //Origin is used to rotate about the tile's center
			Rectangle TileRect = new Rectangle();
			
			if (cTileSetList.ContainsKey(SetName) == false) {
				throw new Exception("Request to draw from non-existant tile set " + SetName);
			}

			TileSet = cTileSetList[SetName];

			if ((ColNum >= TileSet.ColumnCnt) || (RowNum >= TileSet.RowCnt)) {
				throw new Exception(String.Format("Request for out of range tile [{0},{1}] from tile set {2} with boundaries [{3},{4}]", ColNum, RowNum, TileSet.Name, TileSet.ColumnCnt, TileSet.RowCnt));
			}

			TileRect.Width = TileSet.TileWidth;
			TileRect.Height = TileSet.TileHeight;
			TileRect.X = TileSet.TileWidth * ColNum;
			TileRect.Y = TileSet.TileHeight * RowNum;

			Origin.X = TileRect.Width / 2;
			Origin.Y = TileRect.Height / 2;

			DrawPos.X += (int)(Origin.X * DrawPos.Width / TileSet.TileWidth);
			DrawPos.Y += (int)(Origin.Y * DrawPos.Height / TileSet.TileHeight);

			DrawBatch.Draw(TileSet.TileTexture, DrawPos, TileRect, TintColor, 0, Origin, SpriteEffects.None, 0);
		}

		/// <summary>
		/// Structure containing all information for a tile set
		/// </summary>
		private struct TileSetInfo {
			/// <summary>
			/// The name assigned to this tile set
			/// </summary>
			public string Name;
			/// <summary>
			/// The file name containing the image data for these tiles
			/// </summary>
			public string ImageFile;
			/// <summary>
			/// The number of columns of tiles in this set
			/// </summary>
			public int ColumnCnt;
			/// <summary>
			/// The number of rows of tiles in this set
			/// </summary>
			public int RowCnt;
			/// <summary>
			/// The Width of the tiles in pixels
			/// </summary>
			public int TileWidth;
			/// <summary>
			/// The height of the tiles in pixels
			/// </summary>
			public int TileHeight;
			/// <summary>
			/// The texture data to use when rendering these tiles
			/// </summary>
			public Texture2D TileTexture;
		}
	}
}
