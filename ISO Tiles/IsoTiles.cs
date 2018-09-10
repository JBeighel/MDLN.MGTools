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
		KeyboardState cPriorKeyState;

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

			if ((CurrKeys.IsKeyDown(Keys.Up) == true) && (cPriorKeyState.IsKeyDown(Keys.Up) == false)) {
				if (cSpriteMgr.ContainsSprite(cSpriteName) == true) {
					cSpriteMgr.SetSpriteFacing(cSpriteName, SpriteManager.SpriteFacing.Up);
					cSpriteMgr.SetSpriteAnimation(cSpriteName, "walk");

					cSpriteTilePos.Y -= 1;
					cSpriteMgr.SetSpritePosition(cSpriteName, cSpriteTilePos.X * 48, cSpriteTilePos.Y * 48);
				}
			}

			if ((CurrKeys.IsKeyDown(Keys.Down) == true) && (cPriorKeyState.IsKeyDown(Keys.Down) == false)) {
				if (cSpriteMgr.ContainsSprite(cSpriteName) == true) {
					cSpriteMgr.SetSpriteFacing(cSpriteName, SpriteManager.SpriteFacing.Down);
					cSpriteMgr.SetSpriteAnimation(cSpriteName, "walk");
					
					cSpriteTilePos.Y += 1;
					cSpriteMgr.SetSpritePosition(cSpriteName, cSpriteTilePos.X * 48, cSpriteTilePos.Y * 48);
				}
			}

			if ((CurrKeys.IsKeyDown(Keys.Left) == true) && (cPriorKeyState.IsKeyDown(Keys.Left) == false)) {
				if (cSpriteMgr.ContainsSprite(cSpriteName) == true) {
					cSpriteMgr.SetSpriteFacing(cSpriteName, SpriteManager.SpriteFacing.Left);
					cSpriteMgr.SetSpriteAnimation(cSpriteName, "walk");

					cSpriteTilePos.X -= 1;
					cSpriteMgr.SetSpritePosition(cSpriteName, cSpriteTilePos.X * 48, cSpriteTilePos.Y * 48);
				}
			}

			if ((CurrKeys.IsKeyDown(Keys.Right) == true) && (cPriorKeyState.IsKeyDown(Keys.Right) == false)) {
				if (cSpriteMgr.ContainsSprite(cSpriteName) == true) {
					cSpriteMgr.SetSpriteFacing(cSpriteName, SpriteManager.SpriteFacing.Right);
					cSpriteMgr.SetSpriteAnimation(cSpriteName, "walk");

					cSpriteTilePos.X += 1;
					cSpriteMgr.SetSpritePosition(cSpriteName, cSpriteTilePos.X * 48, cSpriteTilePos.Y * 48);
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
						cSpriteMgr.SetSpriteState(cSpriteName, "", SpriteManager.SpriteFacing.Down, new Rectangle(0, 0, 48, 48), false);
					}

					cDevConsole.AddText("Current sprite set to: " + Param);

					cSpriteTilePos.X = 0;
					cSpriteTilePos.Y = 0;
					cSpriteName = Param;
					cSpriteMgr.SetSpriteState(cSpriteName, "", SpriteManager.SpriteFacing.Down, new Rectangle(0, 0, 48, 48), true);
				} else {
					cDevConsole.AddText("Unable to find a sprite named: " + Param);
				}
			} else if (MDLN.Tools.RegEx.QuickTest(CommandStr, "anim=([A-Z]+)") == true) {
				Param = MDLN.Tools.RegEx.GetRegExGroup(CommandStr, "anim=([A-Z]+)", 1);

				if (cSpriteMgr.SpriteContainsAnim(cSpriteName, Param) == true) {
					cDevConsole.AddText("Current sprite animation set to: " + Param);

					cSpriteMgr.SetSpriteState(cSpriteName, Param, SpriteManager.SpriteFacing.Down, new Rectangle(0, 0, 48, 48), true);
				} else {
					cDevConsole.AddText("Unable to find a sprite named: " + Param);
				}
			} else if (MDLN.Tools.RegEx.QuickTest(CommandStr, "load scene xml") == true) {
				//string LoadFile = @"C:\Users\Jason\Dropbox\Code\MDLN.MGTools\ISO Tiles" + @"\Tiles.XML";
				string LoadFile = @"C:\Users\jbeighel\Dropbox\Code\MDLN.MGTools\ISO Tiles" + @"\Tiles.XML";

				cDevConsole.AddText("Reloading scene XML file");
				cSceneMgr.LoadSceneXML(LoadFile, "/tilescene");
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
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with no tilecol attribute.", Environment.NewLine, XMLFile, NewScene.Name));
					}

					if (TileNode.Attributes["tilerow"] != null) {
						if (Int32.TryParse(TileNode.Attributes["tilerow"].InnerText, out NewTile.TileCoords.Y) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with an invalid tilerow attribute.", Environment.NewLine, XMLFile, NewScene.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In scene named {2} Encountered a tile tag with no tilerow attribute.", Environment.NewLine, XMLFile, NewScene.Name));
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

				cTileSetMgr.DrawTile(CurrScene.TileList[Ctr].TileSetName, CurrScene.TileList[Ctr].TileCoords.X, CurrScene.TileList[Ctr].TileCoords.Y, DrawBatch, TileRect, TintColor);
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
		}
	}

	public class SpriteManager {
		private TileSetManager cTilesMgr;
		private Dictionary<string, SpriteInfo> cSpriteList;
		private double cLastUpdate;

		public SpriteManager(TileSetManager TileSetObj, string XMLFile, string RootPath) {
			cSpriteList = new Dictionary<string, SpriteInfo>();
			cTilesMgr = TileSetObj;

			LoadSpriteXML(XMLFile, RootPath);
		}

		public void LoadSpriteXML(string XMLFile, string RootPath) {
			XmlDocument SpriteXML;
			XmlNodeList SpriteList, AnimList, FrameList;
			SpriteInfo NewSprite;
			SpriteAnimation NewAnim;
			Point Pos = new Point();

			cSpriteList.Clear();

			try {
				SpriteXML = new XmlDocument();
				SpriteXML.Load(XMLFile);
			} catch (Exception ExErr) {
				throw new Exception(String.Format("Failed to load XML File {1}{0}Exception {2}{0}Message {3}", Environment.NewLine, XMLFile, ExErr.GetType().ToString(), ExErr.Message));
			}

			SpriteList = SpriteXML.DocumentElement.SelectNodes(RootPath + "/sprite");
			foreach (XmlNode SpriteNode in SpriteList) {
				//Load all details regarding this scene
				if (SpriteNode.Attributes["name"] != null) {
					NewSprite = new SpriteInfo(SpriteNode.Attributes["name"].InnerText);
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a sprite tag with no name attribute.", Environment.NewLine, XMLFile));
				}

				if (SpriteNode.Attributes["tileset"] != null) {
					NewSprite.TileSet = SpriteNode.Attributes["tileset"].InnerText;
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a sprite tag with no tileset attribute.", Environment.NewLine, XMLFile, NewSprite.Name));
				}

				if (SpriteNode.Attributes["height"] != null) {
					if (Int32.TryParse(SpriteNode.Attributes["height"].InnerText, out NewSprite.ScreenRect.Height) == false) {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a sprite tag named {2} with an invalid height attribute.", Environment.NewLine, XMLFile, NewSprite.Name));
					}
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a sprite tag named {2} with no height attribute.", Environment.NewLine, XMLFile, NewSprite.Name));
				}

				if (SpriteNode.Attributes["width"] != null) {
					if (Int32.TryParse(SpriteNode.Attributes["width"].InnerText, out NewSprite.ScreenRect.Width) == false) {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a sprite tag named {2} with an invalid width attribute.", Environment.NewLine, XMLFile, NewSprite.Name));
					}
				} else {
					throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a sprite tag named {2} with no width attribute.", Environment.NewLine, XMLFile, NewSprite.Name));
				}

				FrameList = SpriteNode.SelectNodes("default");
				if (FrameList.Count != 1) {
					throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a sprite tag with no incorrect number of default tags.", Environment.NewLine, XMLFile, NewSprite.Name));
				}

				if (FrameList[0].Attributes["down"] != null) {
					if (RegEx.QuickTest(FrameList[0].Attributes["down"].InnerText, "^([0-9+]),([0-9])+$") == true) {
						Pos.X = Int32.Parse(RegEx.GetRegExGroup(FrameList[0].Attributes["down"].InnerText, "^([0-9+]),([0-9])+$", 1));
						Pos.Y = Int32.Parse(RegEx.GetRegExGroup(FrameList[0].Attributes["down"].InnerText, "^([0-9+]),([0-9])+$", 2));

						NewSprite.DefaultTilePos[SpriteFacing.Down] = Pos;
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a default tag with invalid down attribute.", Environment.NewLine, XMLFile, NewSprite.Name));
					}
				}

				if (FrameList[0].Attributes["up"] != null) {
					if (RegEx.QuickTest(FrameList[0].Attributes["up"].InnerText, "^([0-9+]),([0-9])+$") == true) {
						Pos.X = Int32.Parse(RegEx.GetRegExGroup(FrameList[0].Attributes["up"].InnerText, "^([0-9+]),([0-9])+$", 1));
						Pos.Y = Int32.Parse(RegEx.GetRegExGroup(FrameList[0].Attributes["up"].InnerText, "^([0-9+]),([0-9])+$", 2));

						NewSprite.DefaultTilePos[SpriteFacing.Up] = Pos;
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a default tag with invalid up attribute.", Environment.NewLine, XMLFile, NewSprite.Name));
					}
				}

				if (FrameList[0].Attributes["left"] != null) {
					if (RegEx.QuickTest(FrameList[0].Attributes["left"].InnerText, "^([0-9+]),([0-9])+$") == true) {
						Pos.X = Int32.Parse(RegEx.GetRegExGroup(FrameList[0].Attributes["left"].InnerText, "^([0-9+]),([0-9])+$", 1));
						Pos.Y = Int32.Parse(RegEx.GetRegExGroup(FrameList[0].Attributes["left"].InnerText, "^([0-9+]),([0-9])+$", 2));

						NewSprite.DefaultTilePos[SpriteFacing.Left] = Pos;
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a default tag with invalid left attribute.", Environment.NewLine, XMLFile, NewSprite.Name));
					}
				}

				if (FrameList[0].Attributes["right"] != null) {
					if (RegEx.QuickTest(FrameList[0].Attributes["right"].InnerText, "^([0-9+]),([0-9])+$") == true) {
						Pos.X = Int32.Parse(RegEx.GetRegExGroup(FrameList[0].Attributes["right"].InnerText, "^([0-9+]),([0-9])+$", 1));
						Pos.Y = Int32.Parse(RegEx.GetRegExGroup(FrameList[0].Attributes["right"].InnerText, "^([0-9+]),([0-9])+$", 2));

						NewSprite.DefaultTilePos[SpriteFacing.Right] = Pos;
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a default tag with invalid right attribute.", Environment.NewLine, XMLFile, NewSprite.Name));
					}
				}

				AnimList = SpriteNode.SelectNodes("animation");
				foreach (XmlNode AnimNode in AnimList) {
					if (AnimNode.Attributes["name"] != null) {
						NewAnim = new SpriteAnimation(AnimNode.Attributes["name"].InnerText);
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered an animation tag with no name attribute.", Environment.NewLine, XMLFile));
					}

					if (AnimNode.Attributes["duration"] != null) {
						if (Int32.TryParse(AnimNode.Attributes["duration"].InnerText, out NewAnim.TimeMS) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a animation tag named {2} with an invalid duration attribute.", Environment.NewLine, XMLFile, NewAnim.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a animation tag named {2} with no duration attribute.", Environment.NewLine, XMLFile, NewAnim.Name));
					}

					FrameList = AnimNode.SelectNodes("frame");
					foreach (XmlNode FrameNode in FrameList) {
						if (FrameNode.Attributes["down"] != null) {
							if (RegEx.QuickTest(FrameNode.Attributes["down"].InnerText, "^([0-9+]),([0-9])+$") == true) {
								Pos.X = Int32.Parse(RegEx.GetRegExGroup(FrameNode.Attributes["down"].InnerText, "^([0-9+]),([0-9])+$", 1));
								Pos.Y = Int32.Parse(RegEx.GetRegExGroup(FrameNode.Attributes["down"].InnerText, "^([0-9+]),([0-9])+$", 2));

								NewAnim.TilePos[SpriteFacing.Down].Add(Pos);
							} else {
								throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a default tag with invalid down attribute.", Environment.NewLine, XMLFile, NewAnim.Name));
							}
						}

						if (FrameNode.Attributes["up"] != null) {
							if (RegEx.QuickTest(FrameNode.Attributes["up"].InnerText, "^([0-9+]),([0-9])+$") == true) {
								Pos.X = Int32.Parse(RegEx.GetRegExGroup(FrameNode.Attributes["up"].InnerText, "^([0-9+]),([0-9])+$", 1));
								Pos.Y = Int32.Parse(RegEx.GetRegExGroup(FrameNode.Attributes["up"].InnerText, "^([0-9+]),([0-9])+$", 2));

								NewAnim.TilePos[SpriteFacing.Up].Add(Pos);
							} else {
								throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a default tag with invalid up attribute.", Environment.NewLine, XMLFile, NewAnim.Name));
							}
						}

						if (FrameNode.Attributes["left"] != null) {
							if (RegEx.QuickTest(FrameNode.Attributes["left"].InnerText, "^([0-9+]),([0-9])+$") == true) {
								Pos.X = Int32.Parse(RegEx.GetRegExGroup(FrameNode.Attributes["left"].InnerText, "^([0-9+]),([0-9])+$", 1));
								Pos.Y = Int32.Parse(RegEx.GetRegExGroup(FrameNode.Attributes["left"].InnerText, "^([0-9+]),([0-9])+$", 2));

								NewAnim.TilePos[SpriteFacing.Left].Add(Pos);
							} else {
								throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a default tag with invalid left attribute.", Environment.NewLine, XMLFile, NewAnim.Name));
							}
						}

						if (FrameNode.Attributes["right"] != null) {
							if (RegEx.QuickTest(FrameNode.Attributes["right"].InnerText, "^([0-9+]),([0-9])+$") == true) {
								Pos.X = Int32.Parse(RegEx.GetRegExGroup(FrameNode.Attributes["right"].InnerText, "^([0-9+]),([0-9])+$", 1));
								Pos.Y = Int32.Parse(RegEx.GetRegExGroup(FrameNode.Attributes["right"].InnerText, "^([0-9+]),([0-9])+$", 2));

								NewAnim.TilePos[SpriteFacing.Right].Add(Pos);
							} else {
								throw new Exception(String.Format("Failed to load XML File {1}{0}In sprite named {2} Encountered a default tag with invalid right attribute.", Environment.NewLine, XMLFile, NewAnim.Name));
							}
						}
					}

					NewSprite.AnimationList.Add(NewAnim.Name, NewAnim);
				}

				cSpriteList.Add(NewSprite.Name, NewSprite);
			}
		}

		public bool ContainsSprite(string Name) {
			if (Name == null) {
				return false;
			}

			return cSpriteList.ContainsKey(Name);
		}

		public bool SpriteContainsAnim(string SpriteName, string AnimName) {
			if (ContainsSprite(SpriteName) == false) {
				return false;
			}

			return cSpriteList[SpriteName].AnimationList.ContainsKey(AnimName);
		}

		public void SetSpriteState(string Name, string Animation, SpriteFacing Facing, Rectangle ScreenPosition, bool Visible) {
			SpriteInfo CurrSprite;

			CurrSprite = cSpriteList[Name];

			CurrSprite.Visible = Visible;
			CurrSprite.Facing = Facing;
			CurrSprite.ScreenRect = ScreenPosition;
			CurrSprite.AnimationName = Animation;
			CurrSprite.AnimationTime = 0;

			cSpriteList[Name] = CurrSprite;
		}

		public void SetSpriteAnimation(string Name, string Animation) {
			SpriteInfo CurrSprite;

			CurrSprite = cSpriteList[Name];

			CurrSprite.AnimationName = Animation;
			CurrSprite.AnimationTime = 0;

			cSpriteList[Name] = CurrSprite;
		}

		public void SetSpriteFacing(string Name, SpriteFacing Facing) {
			SpriteInfo CurrSprite;

			CurrSprite = cSpriteList[Name];

			CurrSprite.Facing = Facing;

			cSpriteList[Name] = CurrSprite;
		}

		public void SetSpritePosition(string Name, int X, int Y) {
			SpriteInfo CurrSprite;

			CurrSprite = cSpriteList[Name];

			CurrSprite.ScreenRect.X = X;
			CurrSprite.ScreenRect.Y = Y;

			cSpriteList[Name] = CurrSprite;
		}

		public void DrawSprites(GameTime CurrTime, SpriteBatch DrawBatch) {
			int FrameIndex;
			SpriteInfo CurrSprite;
			double ElapsedTime = CurrTime.TotalGameTime.TotalMilliseconds - cLastUpdate;
			List<string> KeyList = new List<string>(cSpriteList.Keys);

			foreach (string SpriteIndex in KeyList) {
				CurrSprite = cSpriteList[SpriteIndex];

				if (CurrSprite.Visible == true) {
					CurrSprite.AnimationTime += ElapsedTime;

					if ((String.IsNullOrWhiteSpace(CurrSprite.AnimationName) == false) && (CurrSprite.AnimationTime < CurrSprite.AnimationList[CurrSprite.AnimationName].TimeMS)) {
						FrameIndex = (int)Math.Floor(CurrSprite.AnimationTime / (CurrSprite.AnimationList[CurrSprite.AnimationName].TimeMS / CurrSprite.AnimationList[CurrSprite.AnimationName].TilePos[CurrSprite.Facing].Count));

						//Draw the animation frame
						cTilesMgr.DrawTile(CurrSprite.TileSet, CurrSprite.AnimationList[CurrSprite.AnimationName].TilePos[CurrSprite.Facing][FrameIndex].X, CurrSprite.AnimationList[CurrSprite.AnimationName].TilePos[CurrSprite.Facing][FrameIndex].Y, DrawBatch, CurrSprite.ScreenRect, Color.White);
					} else {
						CurrSprite.AnimationTime = 0;
						CurrSprite.AnimationName = "";

						//Draw default sprite
						cTilesMgr.DrawTile(CurrSprite.TileSet, CurrSprite.DefaultTilePos[CurrSprite.Facing].X, CurrSprite.DefaultTilePos[CurrSprite.Facing].Y, DrawBatch, CurrSprite.ScreenRect, Color.White);
					}
				}

				//Update the sprite with the latest animation changes
				cSpriteList[SpriteIndex] = CurrSprite;
			}

			cLastUpdate = CurrTime.TotalGameTime.TotalMilliseconds;
		}

		public enum SpriteFacing {
			Down,
			Up,
			Left,
			Right,
		}

		private struct SpriteInfo {
			public SpriteInfo(string SpriteName) {
				Name = SpriteName;
				TileSet = "";
				Facing = SpriteFacing.Down;
				ScreenRect.X = 0;
				ScreenRect.Y = 0;
				ScreenRect.Height = 0;
				ScreenRect.Width = 0;
				Visible = false;
				AnimationList = new Dictionary<string, SpriteAnimation>();
				DefaultTilePos = new Dictionary<SpriteFacing, Point>();
				AnimationName = "";
				AnimationTime = 0;

				DefaultTilePos.Add(SpriteFacing.Down, new Point());
				DefaultTilePos.Add(SpriteFacing.Up, new Point());
				DefaultTilePos.Add(SpriteFacing.Left, new Point());
				DefaultTilePos.Add(SpriteFacing.Right, new Point());
			}

			public string Name;
			public string TileSet;
			public SpriteFacing Facing;
			public bool Visible;
			public Rectangle ScreenRect;
			public string AnimationName;
			public double AnimationTime;
			public Dictionary<string, SpriteAnimation> AnimationList;
			public Dictionary<SpriteFacing, Point> DefaultTilePos;
		}

		private struct SpriteAnimation {
			public SpriteAnimation(string AnimationName) {
				Name = AnimationName;
				TimeMS = 0;

				TilePos = new Dictionary<SpriteFacing, List<Point>>();

				TilePos.Add(SpriteFacing.Down, new List<Point>());
				TilePos.Add(SpriteFacing.Up, new List<Point>());
				TilePos.Add(SpriteFacing.Left, new List<Point>());
				TilePos.Add(SpriteFacing.Right, new List<Point>());
			}

			public string Name;
			public int TimeMS;
			public Dictionary<SpriteFacing, List<Point>> TilePos;
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
