using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MDLN.MarchingSquares {
	class MainClass {
		public static void Main(string[] args) {
			using (var game = new MarchingSquares())
			{
				game.Run();
			}
		}
	}

	public class MarchingSquares : Game {
		private GraphicsDeviceManager cGraphDevMgr;
		private GameConsole cDevConsole;
		private Dictionary<Textures, Texture2D> cTextureDict;
		private MarchingSquares2D cSquares;
		private Button cNewMap, cFlyMap;

		public MarchingSquares() {
			cGraphDevMgr = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize() {
			//Initializes monogame
			base.Initialize();
		}

		protected override void LoadContent() {
			cTextureDict = new Dictionary<Textures, Texture2D>();
			foreach (Textures CurrTexture in Enum.GetValues(typeof(Textures))) {
				cTextureDict.Add(CurrTexture, Content.Load<Texture2D>(Tools.Tools.GetEnumDescriptionAttribute(CurrTexture)));
			}

			cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, Tools.Tools.GetEnumDescriptionAttribute(Textures.Font), 0, 0, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Width, cGraphDevMgr.GraphicsDevice.Viewport.Bounds.Height / 2);
			cDevConsole.CommandSent += new CommandSentEventHandler(CommandSentEventHandler);
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;
			cDevConsole.AccessKey = Keys.OemTilde;
			cDevConsole.UseAccessKey = true;

			cSquares = new MarchingSquares2D(cGraphDevMgr.GraphicsDevice, cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Height + (cGraphDevMgr.GraphicsDevice.Viewport.Height / 10));
			cSquares.CornerTexture = cTextureDict[Textures.Circle];
			cSquares.WallTexture = cTextureDict[Textures.Squares];
			cSquares.Top = 0;
			cSquares.Left = 0;
			cSquares.Visible = true;
			cSquares.ColumnCount = 11 * 4;
			cSquares.RowCount = 10 * 4;
			cSquares.SendMouseEvents = true;
			cSquares.CellDisplayHeight = 150;
			cSquares.CellDisplayWidth = 150;
			cSquares.DisplayCenterCoords = new Vector2(0, 0);
			cSquares.MouseUp += new ContainerMouseButtonEventHandler(SquaresClick);

			cNewMap = new Button(cGraphDevMgr.GraphicsDevice, null, 10, cSquares.Width + 40, 50, 150);
			cNewMap.BackgroundColor = Color.BlueViolet;
			cNewMap.Text = "New Map";
			cNewMap.Font = new TextureFont(cTextureDict[Textures.Font]);
			cNewMap.FontColor = Color.White;
			cNewMap.Visible = true;
			cNewMap.Click += NewMapClicked;

			cFlyMap = new Button(cGraphDevMgr.GraphicsDevice, null, 70, cSquares.Width + 40, 50, 150);
			cFlyMap.BackgroundColor = Color.BlueViolet;
			cFlyMap.Text = "Fly Map";
			cFlyMap.Font = new TextureFont(cTextureDict[Textures.Font]);
			cFlyMap.FontColor = Color.White;
			cFlyMap.Visible = true;
			cFlyMap.Click += FlyMapClicked;

			//Call monogame base
			base.LoadContent();
		}

		void NewMapClicked (object Sender, MouseButton Button) {
			cSquares.RandomizeAllCornerStates(0.6f);
			cSquares.CellularAutomatonPass(CellCornerState.Empty, 4, 2, 4);
			cSquares.CellularAutomatonPass(CellCornerState.Empty, 4, 2, 4);
			cSquares.CellularAutomatonPass(CellCornerState.Empty, 4, 2, 4);
			cSquares.SetAllEdges(CellCornerState.Solid);
			cSquares.FindCave(200);
		}

		void FlyMapClicked (object Sender, MouseButton Button) {
			if (cSquares.FitMapInDisplay == false) {
				cSquares.FitMapInDisplay = true;
			} else {
				cSquares.FitMapInDisplay = false;
			}
		}

		protected override void Update(GameTime gameTime) {
			KeyboardState CurrKeys = Keyboard.GetState();
			Vector2 WorldPos = cSquares.DisplayCenterCoords;

			if (CurrKeys.IsKeyDown(Keys.Left) == true) {
				WorldPos.X -= 1;
			}

			if (CurrKeys.IsKeyDown(Keys.Right) == true) {
				WorldPos.X += 1;
			}

			if (CurrKeys.IsKeyDown(Keys.Up) == true) {
				WorldPos.Y -= 1;
			}

			if (CurrKeys.IsKeyDown(Keys.Down) == true) {
				WorldPos.Y += 1;
			}

			cSquares.DisplayCenterCoords = WorldPos;

			//Update visual objects
			cSquares.Update(gameTime);
			cNewMap.Update(gameTime);
			cFlyMap.Update(gameTime);
			cDevConsole.Update(gameTime);

			//Call monogame base
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

			//Draw visual objects
			cSquares.Draw();
			cNewMap.Draw();
			cFlyMap.Draw();
			cDevConsole.Draw();

			//Call monogame base
			base.Draw(gameTime);
		}

		protected override void UnloadContent() {

			//Call monogame base
			base.UnloadContent();
		}

		private void CommandSentEventHandler(object Sender, string Command) {
			if (RegEx.QuickTest(Command, @"^(quit|exit)$") == true) {
				Exit();
			} else if (RegEx.QuickTest(Command, @"^Randomize\s*[=:]\s*[0-9]{1,2}$") == true) {
				int Chance = Int32.Parse(RegEx.GetRegExGroup(Command, @"^Randomize\s*[=:]\s*([0-9]{1,2})$", 1));
				cSquares.RandomizeAllCornerStates((float)Chance / 100);
				cDevConsole.AddText("Randomizing with " + (float)Chance / 100 + " chance of solids.");
			} else if (RegEx.QuickTest(Command, @"^cell\s*step$") == true) {
				cSquares.CellularAutomatonPass(CellCornerState.Empty, 4, 2, 4);
				cDevConsole.AddText("Cellular Automaton step.");
			} else if (RegEx.QuickTest(Command, @"^new\s*(map|caves?)$") == true) {
				cSquares.RandomizeAllCornerStates(0.6f);
				cSquares.CellularAutomatonPass(CellCornerState.Empty, 4, 2, 4);
				cSquares.CellularAutomatonPass(CellCornerState.Empty, 4, 2, 4);
				cSquares.CellularAutomatonPass(CellCornerState.Empty, 4, 2, 4);
				cSquares.SetAllEdges(CellCornerState.Solid);
				cSquares.FindCave(200);
				cDevConsole.AddText("Generating new cave map");
			} else if (RegEx.QuickTest(Command, @"^flood\s*([0-9]+)\s([0-9]+)$") == true) {
				int Row = int.Parse(RegEx.GetRegExGroup(Command, @"^flood\s*([0-9]+)\s([0-9]+)$", 1));
				int Col = int.Parse(RegEx.GetRegExGroup(Command, @"^flood\s*([0-9]+)\s([0-9]+)$", 2));
				cDevConsole.AddText("Flooding map from cell row " + Row + " col " + Col);
				cSquares.FloodFill(Row, Col);
			} else {
				cDevConsole.AddText("Unrecognized command: " + Command);
			}
		}

		private void SquaresClick(object Sender, MouseButton MBtn, MouseState CurrMouse) {
			Rectangle CornerRegion = new Rectangle();
			int RowCtr, ColCtr;

			for (RowCtr = 0; RowCtr <= cSquares.RowCount; RowCtr++) {
				for (ColCtr = 0; ColCtr <= cSquares.ColumnCount; ColCtr++) {
					CornerRegion.X = (int)((ColCtr * cSquares.Width / cSquares.ColumnCount) - 5);
					CornerRegion.Y = (int)((RowCtr * cSquares.Height / cSquares.RowCount) - 5);
					CornerRegion.Height = 10;
					CornerRegion.Width = 10;

					if (CornerRegion.Contains(CurrMouse.Position) == true) {
						if (cSquares.GetCellCornerState(RowCtr, ColCtr) == CellCornerState.Empty) {
							cSquares.SetCellCornerState(RowCtr, ColCtr, CellCornerState.Solid);
						} else {
							cSquares.SetCellCornerState(RowCtr, ColCtr, CellCornerState.Empty);
						}
						return;
					}
				}
			}
		}

		protected enum Textures {
			[Description("MarchingSquares.png")]
			Squares,
			[Description("Circle.png")]
			Circle,
			[Description("Font.png")]
			Font
		}
	}
}

