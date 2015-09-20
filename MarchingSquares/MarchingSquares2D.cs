using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

namespace MDLN.MarchingSquares {
	public class MarchingSquares2D : Container {
		private float cCellHeight, cCellWidth;
		private uint cRowCount, cColCount;
		/// <summary>
		/// States of every cell vertex/corner.  cCellVertexes[X/Row][Y/Column]
		/// </summary>
		private List<List<CellCornerState>> cCellVertexes;

		public MarchingSquares2D(GraphicsDevice GraphDev, int Height, int Width) : base(GraphDev, Height, Width) {
			BackgroundColor = new Color(0, 0, 0, 255);
			cCellVertexes = new List<List<CellCornerState>>();

			cRowCount = 0;
			cColCount = 0;

			CornerTexture = null;
		}

		public Texture2D WallTexture { get; set; }

		public Texture2D CornerTexture { get; set; }

		public void SetCellCornerState(int CornerRow, int CornerColumn, CellCornerState State) {
			cCellVertexes[CornerRow][CornerColumn] = State;
			HasChanged = true;
		}

		public CellCornerState GetCellCornerState(int CornerRow, int CornerColumn) {
			return cCellVertexes[CornerRow][CornerColumn];
		}

		protected override void DrawContents(GameTime CurrTime) {
			int ColCtr, RowCtr, TextureID;
			Rectangle DrawRegion = new Rectangle(), TextureRegion;

			//Draw marching squares
			for (ColCtr = 0; ColCtr < cColCount; ColCtr++) {
				for (RowCtr = 0; RowCtr < cRowCount; RowCtr++) {
					TextureID = GetCellTextureID(RowCtr, ColCtr);
					TextureRegion = GetTextureRegionFromID(TextureID);

					DrawRegion.X = (int)(ColCtr * cCellWidth);
					DrawRegion.Y = (int)(RowCtr * cCellHeight);
					DrawRegion.Height = (int)cCellHeight;
					DrawRegion.Width = (int)cCellWidth;

					cDrawBatch.Draw(WallTexture, DrawRegion, TextureRegion, Color.White);
				}
			}

			//If requested draw corner markers
			if (CornerTexture != null) { //Draw markers on the cell corners
				for (ColCtr = 0; ColCtr <= cColCount; ColCtr++) {
					for (RowCtr = 0; RowCtr <= cRowCount; RowCtr++) {
						DrawRegion.X = (int)((ColCtr * cCellWidth) - 2.5);
						DrawRegion.Y = (int)((RowCtr * cCellHeight) - 2.5);
						DrawRegion.Height = 5;
						DrawRegion.Width = 5;

						if (cCellVertexes[RowCtr][ColCtr] == CellCornerState.Empty) {
							cDrawBatch.Draw(CornerTexture, DrawRegion, Color.Cyan);
						} else {
							cDrawBatch.Draw(CornerTexture, DrawRegion, Color.Olive);
						}
					}
				}
			}
		}

		public uint RowCount { 
			get {
				return  cRowCount;
			}

			set {
				cRowCount = value;
				ResizeVertexArrays();
			}
		}

		public uint ColumnCount { 
			get {
				return cColCount;
			}

			set {
				cColCount = value;
				ResizeVertexArrays();
			}
		}

		private void ResizeVertexArrays() {
			int Ctr;
			List<CellCornerState> ColList;

			//Calculate the cell dimensions
			if (cRowCount != 0) {
				cCellHeight = Height / cRowCount;
			} else {
				cCellHeight = 0;
			}

			if (cColCount != 0) {
				cCellWidth = Width / cColCount;
			} else {
				cCellWidth = 0;
			}

			//Resize all of the lists
			if (cRowCount < cCellVertexes.Count) { //Remove extra columns
				cCellVertexes.RemoveRange((int)(cRowCount - 1), (int)(cCellVertexes.Count - cRowCount));
			}

			while (cRowCount >= cCellVertexes.Count) {
				cCellVertexes.Add(new List<CellCornerState>());
			}

			for (Ctr = 0; Ctr < cCellVertexes.Count; Ctr++) {
				ColList = cCellVertexes[Ctr];

				if (cColCount < ColList.Count) {
					ColList.RemoveRange((int)(cColCount - 1), (int)(ColList.Count - cColCount));
				}

				while (cColCount >= ColList.Count) {
					ColList.Add(CellCornerState.Empty);
				}

				cCellVertexes[Ctr] = ColList;
			}
		}

		/// <summary>
		/// Gets the texture ID to use for a specified cell
		/// </summary>
		/// <returns>The cell texture ID</returns>
		/// <param name="CellRow">Cell row, zero based</param>
		/// <param name="CellCol">Cell col, zero based</param>
		private int GetCellTextureID(int CellRow, int CellCol) {
			int TextureID = 0;

			//Top left corner is bit 1
			TextureID += (int)cCellVertexes[CellRow][CellCol];

			//Top right corner is bit 2
			TextureID += 2 * (int)cCellVertexes[CellRow][CellCol + 1];

			//Bottom right corner is bit 3
			TextureID += 4 * (int)cCellVertexes[CellRow + 1][CellCol + 1];

			//Bottom left corner is bit 4
			TextureID += 8 * (int)cCellVertexes[CellRow+ 1][CellCol];

			return TextureID;
		}

		private Rectangle GetTextureRegionFromID(int TextureID) {
			int TextRow, TextCol;
			Rectangle TextRegion = new Rectangle();

			TextRow = 0x03 & TextureID;
			TextCol = TextureID >> 2;

			TextRegion.Height = (int)(WallTexture.Height / 4);
			TextRegion.Width = (int)(WallTexture.Width / 4);

			TextRegion.X = TextRegion.Width * TextRow;
			TextRegion.Y = TextRegion.Height * TextCol;

			return TextRegion;
		}
	}

	public enum CellCornerState : byte {
		Solid = 1,
		Empty = 0
	}
}
	