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
			cSquares.ColumnCount = 11 * 3;
			cSquares.RowCount = 10 * 3;
			cSquares.SendMouseEvents = true;
			cSquares.MouseUp += new ContainerMouseButtonEventHandler(SquaresClick);

			//Call monogame base
			base.LoadContent();
		}

		protected override void Update(GameTime gameTime) {
			
			//Update visual objects
			cSquares.Update(gameTime);
			cDevConsole.Update(gameTime);

			//Call monogame base
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

			//Draw visual objects
			cSquares.Draw();
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
			}else {
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

