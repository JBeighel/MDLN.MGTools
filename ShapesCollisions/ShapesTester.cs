using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ShapesCollisions
{
	class ShapesTester : Game
	{
		const int CIRCLERADIUS = 8;

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
		private Texture2D cCircleTexture;
		private MouseState cPriorMouse;

		private ConvexPolygon cMousePoly;
		private ConvexPolygon cMovePoly;
		private Vector2 cMoveStart;
		private List<ConvexPolygon> cPolyList;
		private int cMouseVertIdx;

		public ShapesTester() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += FormResizeHandler;

			return;
		}

		private void FormResizeHandler(object Sender, EventArgs Args) {
			cDevConsole.Width = Window.ClientBounds.Width;

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
			} else if (RegEx.QuickTest(CommandEvent, @"linewidth\s*=\s*[0-9]+") == true) {
				int nNewWidth, nCtr;
				string Param = RegEx.GetRegExGroup(CommandEvent, @"linewidth\s*=\s*([0-9]+)", 1);

				if (int.TryParse(Param, out nNewWidth) == false) {
					cDevConsole.AddText(String.Format("Invalid parameter '{0}' in command 'LINEWIDTH'", Param));
				} else if (nNewWidth <= 0) {
					cDevConsole.AddText(String.Format("Invalid parameter '{0}' in command 'LINEWIDTH'", Param));
				} else {
					cDevConsole.AddText(String.Format("Polygon line width set to {0}", nNewWidth));

					for (nCtr = 0; nCtr < cPolyList.Count; nCtr++) {
						cPolyList[nCtr].LineWidth = nNewWidth;
					}
				}

			} else {
				cDevConsole.AddText("Unrecognized command: " + CommandEvent);
			}
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
			FileStream FileLoad;
			Vector2 Vert;

			strFileName = INTERFACECONTENTDIR + "\\Circle.png";
			FileLoad = new FileStream(strFileName, FileMode.Open);
			cCircleTexture = Texture2D.FromStream(cGraphDevMgr.GraphicsDevice, FileLoad);
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

			cPolyList = new List<ConvexPolygon>();
			cPolyList.Add(new ConvexPolygon(cGraphDevMgr.GraphicsDevice));
			cPolyList[0].LineColor = Color.Blue;
			cPolyList[0].FillColor = Color.DarkOliveGreen;
			cPolyList[0].FillShape = true;

			Vert.X = 100;
			Vert.Y = 100;
			cPolyList[0].AddVertex(Vert);

			Vert.X = 200;
			Vert.Y = 100;
			cPolyList[0].AddVertex(Vert);

			Vert.X = 200;
			Vert.Y = 200;
			cPolyList[0].AddVertex(Vert);

			cPolyList.Add(new ConvexPolygon(cGraphDevMgr.GraphicsDevice));
			cPolyList[1].LineColor = Color.Green;

			strFileName = INTERFACECONTENTDIR + "\\Ship.png";
			FileLoad = new FileStream(strFileName, FileMode.Open);
			Texture2D tmpTexture = Texture2D.FromStream(cGraphDevMgr.GraphicsDevice, FileLoad);
			FileLoad.Close();

			cPolyList[1].FillTexture = tmpTexture;
			cPolyList[1].FillShape = true;

			Vert.X = 300;
			Vert.Y = 300;
			cPolyList[1].AddVertex(Vert, new Vector2(0, 1));

			Vert.X = 200;
			Vert.Y = 300;
			cPolyList[1].AddVertex(Vert, new Vector2(0, 0));

			Vert.X = 200;
			Vert.Y = 200;
			cPolyList[1].AddVertex(Vert, new Vector2(1, 0));

			Vert.X = 300;
			Vert.Y = 200;
			cPolyList[1].AddVertex(Vert, new Vector2(1, 1));

			cPolyList.Add(new ConvexPolygon(cGraphDevMgr.GraphicsDevice));
			cPolyList[2].LineColor = Color.Gray;
			cPolyList[2].FillColor = Color.BlueViolet;

			//tmpTexture = new Texture2D(cGraphDevMgr.GraphicsDevice, 2, 2);
			//tmpTexture.SetData(new Color[] { Color.RosyBrown, Color.Aqua, Color.DarkOrchid, Color.PaleGreen });
			cPolyList[2].FillTexture = tmpTexture;

			Vert.X = 450;
			Vert.Y = 150;
			cPolyList[2].AddVertex(Vert, new Vector2(0.5f, 0));

			Vert.X = 500;
			Vert.Y = 200;
			cPolyList[2].AddVertex(Vert, new Vector2(1f, 0));

			Vert.X = 500;
			Vert.Y = 300;
			cPolyList[2].AddVertex(Vert, new Vector2(1f, 1));

			Vert.X = 400;
			Vert.Y = 300;
			cPolyList[2].AddVertex(Vert, new Vector2(0f, 1));

			Vert.X = 400;
			Vert.Y = 200;
			cPolyList[2].AddVertex(Vert, new Vector2(0f, 0));

			return;
		}

		/// <summary>
		/// Update game state and all content.  Should be called once every redraw, possibly called 
		/// more often than the draws but only if it exceeds time between frame draws.
		/// </summary>
		/// <param name="gameTime">Current time information of the application</param>
		protected override void Update(GameTime gameTime) {
			int nVertCtr, nPolyCtr, nTestCtr;
			List<Vector2> VertList = new List<Vector2>();
			MouseState CurrMouse = Mouse.GetState();
			Vector2 MousePt;
			Color NewColor;

			MousePt.X = CurrMouse.X;
			MousePt.Y = CurrMouse.Y;

			if ((CurrMouse.LeftButton == ButtonState.Pressed) && (cPriorMouse.LeftButton == ButtonState.Released)) {
				//Mouse was just clicked, see if its in range of a vertex
				for (nPolyCtr = 0; nPolyCtr < cPolyList.Count; nPolyCtr++) {
					VertList.Clear();
					VertList.AddRange(cPolyList[nPolyCtr].GetVertexes());

					for (nVertCtr = 0; nVertCtr < VertList.Count; nVertCtr++) {
						if (MGMath.IsPointInCircle(MousePt, VertList[nVertCtr], CIRCLERADIUS) == true) {
							cMousePoly = cPolyList[nPolyCtr];
							cMouseVertIdx = nVertCtr;

							break;
						}
					}
				}

				for (nPolyCtr = 0; cMousePoly == null && nPolyCtr < cPolyList.Count; nPolyCtr++) {
					if (MGMath.PointInConvexPolygon(MousePt, cPolyList[nPolyCtr].GetVertexes()) == true) {
						cMovePoly = cPolyList[nPolyCtr];
						cMoveStart = MousePt;

						break;
					}
				}
			}

			if ((CurrMouse.LeftButton == ButtonState.Released) && (cPriorMouse.LeftButton == ButtonState.Pressed)) {
				//Mouse was just released
				cMousePoly = null;
				cMovePoly = null;
			}

			if ((CurrMouse.LeftButton == ButtonState.Pressed) && (cMousePoly != null)) {
				//Update the position of the selected vetex
				cMousePoly.UpdateVertex(cMouseVertIdx, MousePt);
			}

			if ((CurrMouse.LeftButton == ButtonState.Pressed) && (cMovePoly != null)) {
				//Update the position of the selected polygon
				cMovePoly.MoveShape(MousePt - cMoveStart);
				cMoveStart = MousePt;
			}

			cPriorMouse = CurrMouse;

			//Check if the polygons are colliding and make them red
			for (nPolyCtr = 0; nPolyCtr < cPolyList.Count; nPolyCtr++) {
				//Reset the background color
				NewColor = cPolyList[nPolyCtr].LineColor;
				NewColor.R = 0;

				cPolyList[nPolyCtr].LineColor = NewColor;

				for (nTestCtr = 0; nTestCtr < cPolyList.Count; nTestCtr++) {
					if (nPolyCtr == nTestCtr) { //Can't collide with itself
						continue;
					}

					if (cPolyList[nPolyCtr].TestCollision(cPolyList[nTestCtr]) == true) {
						NewColor = cPolyList[nPolyCtr].LineColor;
						NewColor.R = 255;

						cPolyList[nPolyCtr].LineColor = NewColor;
					}
				}
			}

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
			Rectangle CircleDest;
			int nVertCtr, nPolyCtr;
			List<Vector2> VertList = new List<Vector2>();

			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);
			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			for (nPolyCtr = 0; nPolyCtr < cPolyList.Count; nPolyCtr++) {
				VertList.Clear();
				VertList.AddRange(cPolyList[nPolyCtr].GetVertexes());

				//Draw circles at each vertex
				for (nVertCtr = 0; nVertCtr < VertList.Count; nVertCtr++) {
					CircleDest.X = (int)(VertList[nVertCtr].X - CIRCLERADIUS);
					CircleDest.Y = (int)(VertList[nVertCtr].Y - CIRCLERADIUS);
					CircleDest.Height = CIRCLERADIUS * 2;
					CircleDest.Width = CIRCLERADIUS * 2;

					cDrawBatch.Draw(cCircleTexture, CircleDest, Color.Gray);
				}

				//Draw thenCtrpolygons
				cPolyList[nPolyCtr].Draw(cDrawBatch);
			}

			//Always draw console last
			cDevConsole.Draw(cDrawBatch);

			cDrawBatch.End();

			//Use monogame draw
			base.Draw(gameTime);
		}
	}
}
