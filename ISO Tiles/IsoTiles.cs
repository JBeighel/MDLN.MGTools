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
		private TileSceneManager cSceneMgr;
		
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

			cTileSets = new TileSetManager(Content, INTERFACECONTENTDIR + @"\Tiles.XML", "/tilescene/tilesets");

			cSceneMgr = new TileSceneManager(Content, cTileSets, INTERFACECONTENTDIR + @"\Tiles.XML", "/tilescene");
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

			cSceneMgr.DrawScene("test", cDrawBatch, new Point(0, 0), Color.White);

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

	public class TileSceneManager {
		private Dictionary<string, SceneInfo> cSceneList;
		private TileSetManager cTileSetMgr;

		public TileSceneManager(ContentManager ContentMgr, TileSetManager TileSetObj, string XMLFile, string RootPath) {
			cSceneList = new Dictionary<string, SceneInfo>();
			cTileSetMgr = TileSetObj;

			LoadSceneXML(ContentMgr, XMLFile, RootPath);
		}

		public void LoadSceneXML(ContentManager ContentMgr, string XMLFile, string RootPath) {
			XmlDocument SceneXML;
			XmlNodeList SceneList, TileList;
			SceneInfo NewScene;
			SceneTileInfo NewTile;
			
			cSceneList.Clear();

			try {
				SceneXML = new XmlDocument();
				SceneXML.Load(XMLFile);
			} catch (Exception ExErr) {
				throw new Exception(String.Format("Failed to load XML File {1}{0}Exception {2}{0}Message {3}", Environment.NewLine, XMLFile, ExErr.GetType().ToString(), ExErr.Message));
			}

			SceneList = SceneXML.DocumentElement.SelectNodes(RootPath + "/scene");
			foreach (XmlNode SceneNode in SceneList) {
				

				//Load all details regarding this scene
				if (SceneNode.Attributes["name"] != null) {
					NewScene = new SceneInfo(SceneNode.Attributes["name"].InnerText);
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a scene tag with no name attribute.", Environment.NewLine, XMLFile));
				}

				if (SceneNode.Attributes["tilewidth"] != null) {
					if (Int32.TryParse(SceneNode.Attributes["tilewidth"].InnerText, out NewScene.TileWidth) == false) {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a scene tag with an invalid tilewidth attribute.", Environment.NewLine, XMLFile, NewScene.Name));
					}
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a scene tag with no tilewidth attribute.", Environment.NewLine, XMLFile, NewScene.Name));
				}

				if (SceneNode.Attributes["tileheight"] != null) {
					if (Int32.TryParse(SceneNode.Attributes["tileheight"].InnerText, out NewScene.TileHeight) == false) {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a scene tag with an invalid tileheight attribute.", Environment.NewLine, XMLFile, NewScene.Name));
					}
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a scene tag with no tileheight attribute.", Environment.NewLine, XMLFile, NewScene.Name));
				}

				//Load individual tiles to draw in this scene
				TileList = SceneNode.SelectNodes("/tile");
				foreach (XmlNode TileNode in TileList) {
					NewTile = new SceneTileInfo();

					//Load all tile details from the XML file
					if (TileNode.Attributes["setname"] != null) {
						NewTile.TileSetName = TileNode.Attributes["setname"].InnerText;
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tile tag with no name attribute in scene {2}.", Environment.NewLine, XMLFile, NewScene.Name));
					}

					if (TileNode.Attributes["tilecol"] != null) {
						if (Int32.TryParse(TileNode.Attributes["tilecol"].InnerText, out NewTile.TileCoords.X) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with an invalid tilecol attribute.", Environment.NewLine, XMLFile, NewScene.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with no tilecol attribute.", Environment.NewLine, XMLFile, NewScene.Name));
					}

					if (TileNode.Attributes["tilerow"] != null) {
						if (Int32.TryParse(TileNode.Attributes["tilerow"].InnerText, out NewTile.TileCoords.X) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with an invalid tilerow attribute.", Environment.NewLine, XMLFile, NewScene.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with no tilerow attribute.", Environment.NewLine, XMLFile, NewScene.Name));
					}

					if (TileNode.Attributes["scenecol"] != null) {
						if (Int32.TryParse(TileNode.Attributes["scenecol"].InnerText, out NewTile.TileCoords.X) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with an invalid scenecol attribute.", Environment.NewLine, XMLFile, NewScene.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with no scenecol attribute.", Environment.NewLine, XMLFile, NewScene.Name));
					}

					if (TileNode.Attributes["scenerow"] != null) {
						if (Int32.TryParse(TileNode.Attributes["scenerow"].InnerText, out NewTile.TileCoords.X) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with an invalid scenerow attribute.", Environment.NewLine, XMLFile, NewScene.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with no scenerow attribute.", Environment.NewLine, XMLFile, NewScene.Name));
					}

					NewScene.TileList.Add(NewTile);
				}

				cSceneList.Add(NewScene.Name, NewScene);
			}
		}

		public void DrawScene(string SceneName, SpriteBatch DrawBatch, Point Origin, Color TintColor) {
			SceneInfo CurrScene;

			if (cSceneList.ContainsKey(SceneName) == false) {
				throw new Exception("Request to draw an unknown scened, name: " + SceneName);
			}

			CurrScene = cSceneList[SceneName];
		}

		private struct SceneInfo {
			public SceneInfo(string SceneName) {
				Name = SceneName;
				TileList = new List<SceneTileInfo>();
				TileWidth = 0;
				TileHeight = 0;
			}

			public string Name;
			public List<SceneTileInfo> TileList;
			public int TileWidth;
			public int TileHeight;
		}

		private struct SceneTileInfo {
			public Point TileCoords;
			public Point SceneCoords;
			public string TileSetName;
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

		/// <summary>
		/// Draws a single tile onto the screen
		/// </summary>
		/// <param name="SetName">The name of the tile set to get the image data from</param>
		/// <param name="ColNum">The column number in the tile set to grab the tile from</param>
		/// <param name="RowNum">The row number in the tile set to grab the tile from</param>
		/// <param name="DrawBatch">The draw batch to use to render this image</param>
		/// <param name="DrawPos">The position on the screen to draw the tile</param>
		/// <param name="TintColor">Any color to apply to the image, use Color.White for no change</param>
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
