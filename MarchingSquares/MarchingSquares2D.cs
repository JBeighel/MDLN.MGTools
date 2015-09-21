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
		/// States of every cell vertex/corner.  cCellVertexes[Y/Row][X/Column]
		/// </summary>
		private List<List<CellCornerState>> cCellVertexes;
		private Random cRandom;

		public MarchingSquares2D(GraphicsDevice GraphDev, int Height, int Width) : base(GraphDev, Height, Width) {
			BackgroundColor = new Color(0, 0, 0, 255);
			cCellVertexes = new List<List<CellCornerState>>();

			cRowCount = 0;
			cColCount = 0;

			CornerTexture = null;

			cRandom = new Random(DateTime.Now.Millisecond);
		}

		public Texture2D WallTexture { get; set; }

		public Texture2D CornerTexture { get; set; }

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

		public void SetCellCornerState(int CornerRow, int CornerColumn, CellCornerState State) {
			cCellVertexes[CornerRow][CornerColumn] = State;
			HasChanged = true;
		}

		public CellCornerState GetCellCornerState(int CornerRow, int CornerColumn) {
			return cCellVertexes[CornerRow][CornerColumn];
		}

		public void RandomizeAllCornerStates(float SolidChance) {
			int RowCtr;

			foreach (List<CellCornerState> Column in cCellVertexes) {
				for (RowCtr = 0; RowCtr < Column.Count; RowCtr++) {
					if (cRandom.NextDouble() < SolidChance) {
						Column[RowCtr] = CellCornerState.Solid;
					} else {
						Column[RowCtr] = CellCornerState.Empty;
					}
				}
			}

			HasChanged = true;
		}

		public void CellularAutomatonPass(CellCornerState GridEdge, int BirthLimit, int NeighborMinLmit, int NeighborMaxLimit) {
			int ColCtr, RowCtr, NeighborCount;
			List<List<CellCornerState>> NewVertexList = new List<List<CellCornerState>>();
			List<CellCornerState> NewRow;

			for (RowCtr = 0; RowCtr < cCellVertexes.Count; RowCtr++) {
				NewRow = new List<CellCornerState>();
				NewVertexList.Add(NewRow);

				for (ColCtr = 0; ColCtr < cCellVertexes[RowCtr].Count; ColCtr++) {
					NeighborCount = GetCellNeighborCount(RowCtr, ColCtr, GridEdge);

					if (cCellVertexes[RowCtr][ColCtr] == CellCornerState.Solid) {
						if ((NeighborCount < NeighborMinLmit) || (NeighborCount > NeighborMaxLimit)) {
							NewRow.Add(CellCornerState.Empty);
						} else {
							NewRow.Add(CellCornerState.Solid);
						}
					} else {
						if (NeighborCount > BirthLimit) {
							NewRow.Add(CellCornerState.Solid);
						} else {
							NewRow.Add(CellCornerState.Empty);
						}
					}
				}
			}

			cCellVertexes = NewVertexList;
			HasChanged = true;
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

		private int GetCellNeighborCount(int Row, int Col, CellCornerState GridEdge) {
			List<CellCornerState> Neighbors = new List<CellCornerState>();
			int ColCtr, RowCtr;

			//Generate a list of neighbors
			for (ColCtr = -1; ColCtr < 2; ColCtr++) {
				for (RowCtr = -1; RowCtr < 2; RowCtr++) {
					if ((ColCtr == 0) && (RowCtr == 0)) {
						continue; //Skip the center, we only want neighbors
					}

					if ((Row + RowCtr < 0) || (Row + RowCtr > RowCount) || (Col + ColCtr < 0) || (Col + ColCtr > ColumnCount)) {
						//Neighbor is outside the grid
						Neighbors.Add(GridEdge);
					} else {
						Neighbors.Add(cCellVertexes[Row + RowCtr][Col + ColCtr]);
					}
				}
			}

			//Count the solid neighbors
			ColCtr = 0;
			foreach (CellCornerState CurrVal in Neighbors) {
				if (CurrVal == CellCornerState.Solid) {
					ColCtr++;
				}
			}

			return ColCtr;
		}
	}

	public enum CellCornerState : byte {
		Solid = 1,
		Empty = 0
	}
}
	