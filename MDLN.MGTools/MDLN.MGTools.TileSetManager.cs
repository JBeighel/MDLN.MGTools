using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Xml;

namespace MDLN.MGTools {
	/// <summary>
	/// Class the store and render tiles from a sprite sheet
	/// The class uses an XML file to specify the image data to load as well as describe how the tiles
	///	are sized and arranged.  The image is effectively cut into a grid and you can request tiles
	///	based on the X and Y coordinates within that grid (the top left picture being 0, 0).
	/// </summary>
	public class TileSetManager {
		/// <summary>
		/// Collection to store all of the tilesets availabe in this manager
		/// </summary>
		private Dictionary<string, TileSetInfo> cTileSetList;

		/// <summary>
		/// Basic constructor, initializes the class but assumes all information will be loaded later
		/// </summary>
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
			XmlNodeList TilesList, NamedTilesList;
			TileSetInfo NewSet;
			string Name;
			UInt32 MarchID;
			Point Coords;

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
				NewSet.MarchingSquaresList = new Dictionary<MarchingSquaresTiles, Point>();
				NewSet.NamedTileList = new Dictionary<string, Point>();

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

				NamedTilesList = TileNode.SelectNodes("namedtile");
				foreach(XmlNode NamedTileNode in NamedTilesList) {
					if (NamedTileNode.Attributes["column"] != null) {
						if (Int32.TryParse(NamedTileNode.Attributes["column"].InnerText, out Coords.X) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with a namedtile tag with an invalid column attribute.", Environment.NewLine, XMLFile, NewSet.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with a namedtile tag with an missing column attribute.", Environment.NewLine, XMLFile, NewSet.Name));
					}

					if (NamedTileNode.Attributes["row"] != null) {
						if (Int32.TryParse(NamedTileNode.Attributes["row"].InnerText, out Coords.Y) == false) {
							throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with a namedtile tag with an invalid row attribute.", Environment.NewLine, XMLFile, NewSet.Name));
						}
					} else {
						throw new Exception(String.Format("Failed to load XML File {1}{0}Encountered a tiles tag named {2} with a namedtile tag with an missing row attribute.", Environment.NewLine, XMLFile, NewSet.Name));
					}

					if (NamedTileNode.Attributes["name"] != null) {
						Name = NamedTileNode.Attributes["name"].InnerText;

						NewSet.NamedTileList.Add(Name, Coords);
					}

					if (NamedTileNode.Attributes["marchid"] != null) {
						Name = NamedTileNode.Attributes["marchid"].InnerText;
						MarchID = MDLN.Tools.TypeTools.BinaryStringToUInt32(Name);

						NewSet.MarchingSquaresList.Add((MarchingSquaresTiles)MarchID, Coords);
					}
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
		/// <param name="TileName">The named tile to draw</param>
		/// <param name="DrawBatch">The draw batch to use to render this image</param>
		/// <param name="DrawPos">The position on the screen to draw the tile</param>
		/// <param name="TintColor">Any color to apply to the image, use Color.White for no change</param>
		public void DrawTile(string SetName, string TileName, SpriteBatch DrawBatch, Rectangle DrawPos, Color TintColor) {
			if (cTileSetList.ContainsKey(SetName) == false) {
				throw new Exception("Request to draw from non-existant tile set " + SetName);
			}

			if (cTileSetList[SetName].NamedTileList.ContainsKey(TileName) == false) {
				throw new Exception("Request to draw non-existant named tile, " + TileName + ", in set " + SetName);
			}

			if (RegEx.QuickTest(TileName, "inside bottom") == true) {
				TileName = TileName;
			}

			DrawTile(SetName, cTileSetList[SetName].NamedTileList[TileName].X, cTileSetList[SetName].NamedTileList[TileName].Y, DrawBatch, DrawPos, TintColor);
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
			TileRect.Y = (TileSet.TileHeight * RowNum);

			Origin.X = TileSet.TileWidth / 2;
			Origin.Y = TileSet.TileHeight / 2;

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

			public Dictionary<string, Point> NamedTileList;
			public Dictionary<MarchingSquaresTiles, Point> MarchingSquaresList;
		}

		/// <summary>
		/// Enumeration of all marching square tile images.
		/// There are 4 bits in their ID numbers starting from the top left corner as the most 
		/// significant bit, go around the square clockwise with each corner being the next lower
		/// bit.  The bit is set if that corner is in the blocked space.
		/// 1000    0100
		///    O---O 
		///    |   | 
		///    O---O
		/// 0001    0010
		/// </summary>
		public enum MarchingSquaresTiles {
			[StringValue("open|Marching squares ID=0000")]
			Open= 0x00,
			[StringValue("inside top left|Marching squares ID=0001")]
			InTopLeft= 0x01,
			[StringValue("inside top right|Marching squares ID=0010")]
			InTopRight = 0x02,
			[StringValue("bottom|Marching squares ID=0011")]
			Bottom = 0x03,
			[StringValue("inside bottom right|Marching squares ID=0100")]
			InBottomRight= 0x04,
			[StringValue("right|Marching squares ID=0110")]
			Right = 0x06,
			[StringValue("outside bottom right|Marching squares ID=1110")]
			OutBottomRight = 0x07,
			[StringValue("outside bottom left|Marching squares ID=1110")]
			OutBottomLeft = 0x0B,
			[StringValue("outside top right|Marching squares ID=1110")]
			OutTopRight = 0x0E,
			[StringValue("outside top left|Marching squares ID=1101")]
			OutTopLeft = 0x0D,
			[StringValue("blocked|Marching squares ID=1111")]
			Blocked = 0x0F
		}
	}
}
