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
		private SpriteManager cSpriteMgr;
		private KeyboardState cPriorKeyState;

		private string cSceneName;
		private string cSpriteName;
		private Point cSpriteTilePos;
		
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
			cSpriteName = "bluehat";
			cSceneName = "small";

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

			cSceneMgr = new TileSceneManager(cTileSets, INTERFACECONTENTDIR + @"\Tiles.XML", "/tilescene");

			cSpriteMgr = new SpriteManager(cTileSets, INTERFACECONTENTDIR + @"\Tiles.XML", "/tilescene");
		}

		/// <summary>
		/// Update game state and all content.  Should be called once every redraw, possibly called 
		/// more often than the draws but only if it exceeds time between frame draws.
		/// </summary>
		/// <param name="gameTime">Current time information of the application</param>
		protected override void Update(GameTime gameTime) {
			cDevConsole.Update(gameTime);
			KeyboardState CurrKeys = Keyboard.GetState();

			if (CurrKeys.IsKeyDown(Keys.Up) == true)  {
				if ((cSpriteMgr.ContainsSprite(cSpriteName) == true) && (cSpriteMgr.SpriteIsAnimating(cSpriteName) == false)) {
					cSpriteMgr.SetSpriteFacing(cSpriteName, SpriteManager.SpriteFacing.Up);
					cSpriteMgr.SetSpriteAnimation(cSpriteName, "walk", 0, -1 * cSpriteMgr.GetSpritePosition(cSpriteName).Height);

					cSpriteTilePos.Y -= 1;
				}
			}

			if (CurrKeys.IsKeyDown(Keys.Down) == true) {
				if ((cSpriteMgr.ContainsSprite(cSpriteName) == true) && (cSpriteMgr.SpriteIsAnimating(cSpriteName) == false)) {
					cSpriteMgr.SetSpriteFacing(cSpriteName, SpriteManager.SpriteFacing.Down);
					cSpriteMgr.SetSpriteAnimation(cSpriteName, "walk", 0, cSpriteMgr.GetSpritePosition(cSpriteName).Height);
					
					cSpriteTilePos.Y += 1;
				}
			}

			if (CurrKeys.IsKeyDown(Keys.Left) == true) {
				if ((cSpriteMgr.ContainsSprite(cSpriteName) == true) && (cSpriteMgr.SpriteIsAnimating(cSpriteName) == false)) {
					cSpriteMgr.SetSpriteFacing(cSpriteName, SpriteManager.SpriteFacing.Left);
					cSpriteMgr.SetSpriteAnimation(cSpriteName, "walk", -1 * cSpriteMgr.GetSpritePosition(cSpriteName).Width, 0);

					cSpriteTilePos.X -= 1;
				}
			}

			if (CurrKeys.IsKeyDown(Keys.Right) == true) {
				if ((cSpriteMgr.ContainsSprite(cSpriteName) == true) && (cSpriteMgr.SpriteIsAnimating(cSpriteName) == false)) {
					cSpriteMgr.SetSpriteFacing(cSpriteName, SpriteManager.SpriteFacing.Right);
					cSpriteMgr.SetSpriteAnimation(cSpriteName, "walk", cSpriteMgr.GetSpritePosition(cSpriteName).Width, 0);

					cSpriteTilePos.X += 1;
				}
			}

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

			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			if (String.IsNullOrWhiteSpace(cSceneName) == false) {
				cSceneMgr.DrawScene(cSceneName, cDrawBatch, new Point(0, 0), Color.White);
			}

			if (String.IsNullOrWhiteSpace(cSpriteName) == false) {
				cSpriteMgr.DrawSprites(gameTime, cDrawBatch);
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
		private void ConsoleCommandEvent(object Sender, string CommandStr) {
			string Param;

			if (MDLN.Tools.RegEx.QuickTest(CommandStr, "scene=([A-Z]+)") == true) {
				Param = MDLN.Tools.RegEx.GetRegExGroup(CommandStr, "scene=([A-Z]+)", 1);

				if (cSceneMgr.ContainsScene(Param) == true) {
					cSceneName = Param;
					cDevConsole.AddText("Current scene set to: " + Param);
				} else {
					cDevConsole.AddText("Unable to find a scene named: " + Param);
				}
			} else if (MDLN.Tools.RegEx.QuickTest(CommandStr, "sprite=([A-Z]+)") == true) {
				Param = MDLN.Tools.RegEx.GetRegExGroup(CommandStr, "sprite=([A-Z]+)", 1);

				if (cSpriteMgr.ContainsSprite(Param) == true) {
					if (cSpriteMgr.ContainsSprite(cSpriteName) == true) {
						cSpriteMgr.SetSpriteState(cSpriteName, "", SpriteManager.SpriteFacing.Down, new Point(0, 0), Color.White, false);
					}

					cDevConsole.AddText("Current sprite set to: " + Param);

					cSpriteTilePos.X = 0;
					cSpriteTilePos.Y = 0;
					cSpriteName = Param;
					cSpriteMgr.SetSpriteState(cSpriteName, "", SpriteManager.SpriteFacing.Down, new Point(0, 0), Color.White, true);
				} else {
					cDevConsole.AddText("Unable to find a sprite named: " + Param);
				}
			} else if (MDLN.Tools.RegEx.QuickTest(CommandStr, "anim=([A-Z]+)") == true) {
				Param = MDLN.Tools.RegEx.GetRegExGroup(CommandStr, "anim=([A-Z]+)", 1);

				if (cSpriteMgr.SpriteContainsAnim(cSpriteName, Param) == true) {
					cDevConsole.AddText("Current sprite animation set to: " + Param);

					cSpriteMgr.SetSpriteState(cSpriteName, Param, SpriteManager.SpriteFacing.Down, new Point(0, 0), Color.White, true);
				} else {
					cDevConsole.AddText("Unable to find a sprite named: " + Param);
				}
			} else if (MDLN.Tools.RegEx.QuickTest(CommandStr, "load xml") == true) {
				//string LoadFile = @"C:\Users\Jason\Dropbox\Code\MDLN.MGTools\ISO Tiles" + @"\Tiles.XML";
				string LoadFile = @"C:\Users\jbeighel\Dropbox\Code\MDLN.MGTools\ISO Tiles" + @"\Tiles.XML";

				cDevConsole.AddText("Reloading scene XML file");
				cSceneMgr.LoadSceneXML(LoadFile, "/tilescene");
				cSpriteMgr.LoadSpriteXML(LoadFile, "/tilescene");
				cTileSets.LoadTileSetXML(Content, LoadFile, "/tilescene/tilesets");
			} else if (RegEx.QuickTest(CommandStr, "(quit|exit)") == true) {
				this.Exit();
			} else {
				cDevConsole.AddText("Unrecognized command: " + CommandStr);
			}
		}
	}

	public class TileSceneManager {
		private Dictionary<string, SceneInfo> cSceneList;
		private TileSetManager cTileSetMgr;

		public TileSceneManager(TileSetManager TileSetObj, string XMLFile, string RootPath) {
			cSceneList = new Dictionary<string, SceneInfo>();
			cTileSetMgr = TileSetObj;

			LoadSceneXML(XMLFile, RootPath);
		}

		public void LoadSceneXML(string XMLFile, string RootPath) {
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
				TileList = SceneNode.SelectNodes("tile");
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
						NewTile.TileCoords.X = -1;
					}

					if (TileNode.Attributes["tilerow"] != null) {
						if (Int32.TryParse(TileNode.Attributes["tilerow"].InnerText, out NewTile.TileCoords.Y) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with an invalid tilerow attribute.", Environment.NewLine, XMLFile, NewScene.Name));
						}
					} else {
						NewTile.TileCoords.Y = -1;
					}

					if (TileNode.Attributes["scenecol"] != null) {
						if (Int32.TryParse(TileNode.Attributes["scenecol"].InnerText, out NewTile.SceneCoords.X) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with an invalid scenecol attribute.", Environment.NewLine, XMLFile, NewScene.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with no scenecol attribute.", Environment.NewLine, XMLFile, NewScene.Name));
					}

					if (TileNode.Attributes["scenerow"] != null) {
						if (Int32.TryParse(TileNode.Attributes["scenerow"].InnerText, out NewTile.SceneCoords.Y) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with an invalid scenerow attribute.", Environment.NewLine, XMLFile, NewScene.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with no scenerow attribute.", Environment.NewLine, XMLFile, NewScene.Name));
					}

					if (TileNode.Attributes["tilename"] != null) {
						NewTile.TileNameInSet = TileNode.Attributes["tilename"].InnerText;
					} else {
						NewTile.TileNameInSet = "";
					}

					NewScene.TileList.Add(NewTile);
				}

				cSceneList.Add(NewScene.Name, NewScene);
			}
		}

		public void DrawScene(string SceneName, SpriteBatch DrawBatch, Point Origin, Color TintColor) {
			SceneInfo CurrScene;
			Rectangle TileRect;
			int Ctr;

			if (cSceneList.ContainsKey(SceneName) == false) {
				throw new Exception("Request to draw an unknown scened, name: " + SceneName);
			}

			CurrScene = cSceneList[SceneName];

			for (Ctr = 0; Ctr < CurrScene.TileList.Count; Ctr++) {
				TileRect.X = CurrScene.TileList[Ctr].SceneCoords.X * CurrScene.TileWidth;
				TileRect.X += Origin.X;
				TileRect.Y = CurrScene.TileList[Ctr].SceneCoords.Y * CurrScene.TileHeight;
				TileRect.Y += Origin.Y;
				TileRect.Width = CurrScene.TileWidth;
				TileRect.Height = CurrScene.TileHeight;

				if (CurrScene.TileList[Ctr].TileCoords.X >= 0) { //Coordinates are set, use those
					cTileSetMgr.DrawTile(CurrScene.TileList[Ctr].TileSetName, CurrScene.TileList[Ctr].TileCoords.X, CurrScene.TileList[Ctr].TileCoords.Y, DrawBatch, TileRect, TintColor);
				} else { //Coordinates are not set, use named tile
					cTileSetMgr.DrawTile(CurrScene.TileList[Ctr].TileSetName, CurrScene.TileList[Ctr].TileNameInSet, DrawBatch, TileRect, TintColor);
				}
			}
		}

		public bool ContainsScene(string SceneName) {
			return cSceneList.ContainsKey(SceneName);
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
			public string TileNameInSet;
		}
	}
}
