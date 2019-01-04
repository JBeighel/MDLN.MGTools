using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Xml;

namespace MDLN.MGTools {
	/// <summary>
	/// Class that will manage and load individual sprites.
	/// It expects the image data to be stored in a TileSetManager class, and will load an XML that will 
	/// describe the animations and appearances of the sprite.  The class will then draw the sprites 
	/// through the animations and postions upon request.
	/// </summary>
	public class SpriteManager {
		/// <summary>
		/// Link to the tile set manager with the images for the sprites
		/// </summary>
		private TileSetManager cTilesMgr;
		/// <summary>
		/// Collection of all sprites managed by this object
		/// </summary>
		private Dictionary<string, SpriteInfo> cSpriteList;
		/// <summary>
		/// The last time the sprites were draw (this is also when the updates happen)
		/// </summary>
		private double cLastUpdate;

		/// <summary>
		/// Constructor to collect runtime information
		/// </summary>
		/// <param name="TileSetObj">The object containing the timeset image data</param>
		/// <param name="XMLFile">The XML file to load containing the sprite descriptions</param>
		/// <param name="RootPath">The path within the XML file containing the sprite tags</param>
		public SpriteManager(TileSetManager TileSetObj, string XMLFile, string RootPath) {
			cSpriteList = new Dictionary<string, SpriteInfo>();
			cTilesMgr = TileSetObj;

			LoadSpriteXML(XMLFile, RootPath);
		}

		/// <summary>
		/// Load sprite data from an XML file
		/// </summary>
		/// <param name="XMLFile">Path and name of the XML file to load</param>
		/// <param name="RootPath">Path within the XML scheme to look for the sprite tags</param>
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

		/// <summary>
		/// Checks if a sprite exists within the object using the specified name
		/// </summary>
		/// <param name="Name">Name to look for</param>
		/// <returns>True if a sprite with that name is found, false otherwise</returns>
		public bool ContainsSprite(string Name) {
			if (Name == null) {
				return false;
			}

			return cSpriteList.ContainsKey(Name);
		}

		/// <summary>
		/// Checks if a sprite in this object has an animation with the specified name
		/// </summary>
		/// <param name="SpriteName">Name of the sprite to check for the animation</param>
		/// <param name="AnimName">Name of the animation to verify</param>
		/// <returns>True if the sprite exists and has the requested animation, false if either piece can't be found</returns>
		public bool SpriteContainsAnim(string SpriteName, string AnimName) {
			if (ContainsSprite(SpriteName) == false) {
				return false;
			}

			return cSpriteList[SpriteName].AnimationList.ContainsKey(AnimName);
		}

		/// <summary>
		/// Tests if a specified sprite is currently being animated
		/// </summary>
		/// <param name="SpriteName">Name of the sprite to check on</param>
		/// <returns>True if the sprite exists and is performing an animation, false otherwise</returns>
		public bool SpriteIsAnimating(string SpriteName) {
			if (cSpriteList.ContainsKey(SpriteName) == false) {
				return false;
			}

			if (String.IsNullOrEmpty(cSpriteList[SpriteName].AnimationName) == false) {
				return true; //Animation name exists, so it's running that animation
			} else {
				return false; //no animation name, nothing being performed
			}
		}

		/// <summary>
		/// Specify the state of a managed sprite
		/// </summary>
		/// <param name="Name">Name of the sprite to modify</param>
		/// <param name="Animation">Name of an animation for the sprite to begin carrying ouy</param>
		/// <param name="Facing">Which direction the sprite should be facing on screed</param>
		/// <param name="ScreenPosition">Where on the screen the sprite should be drawn, this is in screen coordinates</param>
		/// <param name="TintColor">The color to apply to the sprite, tinting its appearance</param>
		/// <param name="Visible">True if the sprite should be draw, false if it should be left out of when rendering</param>
		public void SetSpriteState(string Name, string Animation, SpriteFacing Facing, Point ScreenPosition, Color TintColor, bool Visible) {
			SpriteInfo CurrSprite;

			CurrSprite = cSpriteList[Name];

			CurrSprite.Visible = Visible;
			CurrSprite.Facing = Facing;
			CurrSprite.ScreenRect.X = ScreenPosition.X;
			CurrSprite.ScreenRect.Y = ScreenPosition.Y;
			CurrSprite.AnimationName = Animation;
			CurrSprite.AnimationTime = 0;
			CurrSprite.TintColor = TintColor;

			cSpriteList[Name] = CurrSprite;
		}

		/// <summary>
		/// Specifies an animation that the sprite should begin doing.
		/// Movement on screen can be specified with this call, and the sprites render position will be udpated during the animation
		/// </summary>
		/// <param name="Name">Name of the sprite to modify</param>
		/// <param name="Animation">Name of the animation to carry out</param>
		/// <param name="MoveX">Distance to move in the X direction in screen coordinates</param>
		/// <param name="MoveY">Distance to move in the X direction in screen coordinates</param>
		public void SetSpriteAnimation(string Name, string Animation, int MoveX, int MoveY) {
			SpriteInfo CurrSprite;

			CurrSprite = cSpriteList[Name];

			CurrSprite.AnimationName = Animation;
			CurrSprite.AnimationTime = 0;
			CurrSprite.AnimationMoveX = MoveX;
			CurrSprite.AnimationMoveY = MoveY;

			cSpriteList[Name] = CurrSprite;
		}

		/// <summary>
		/// Sets the direction the sprite should be facing
		/// </summary>
		/// <param name="Name">Name of the sprite to modify</param>
		/// <param name="Facing">Direction the sprite should be facing</param>
		public void SetSpriteFacing(string Name, SpriteFacing Facing) {
			SpriteInfo CurrSprite;

			CurrSprite = cSpriteList[Name];

			CurrSprite.Facing = Facing;

			cSpriteList[Name] = CurrSprite;
		}

		/// <summary>
		/// Specify the position of a sprite in screen coordinates.  This will be the top left corner of the sprite
		/// </summary>
		/// <param name="Name">Name of the sprite to modify</param>
		/// <param name="X">X coordiate in screen units</param>
		/// <param name="Y">Y coordinate in screen units</param>
		public void SetSpritePosition(string Name, int X, int Y) {
			SpriteInfo CurrSprite;

			CurrSprite = cSpriteList[Name];

			CurrSprite.ScreenRect.X = X;
			CurrSprite.ScreenRect.Y = Y;

			cSpriteList[Name] = CurrSprite;
		}

		/// <summary>
		/// Retrieve the current position and size of this sprite on the screen
		/// </summary>
		/// <param name="Name">Name index of the sprite to query</param>
		/// <returns>Returns the coordinates of the top left coner of the sprite along with its height and width</returns>
		public Rectangle GetSpritePosition(string Name) {
			if (cSpriteList.ContainsKey(Name) == false) {
				throw new Exception("Requested an invalid sprite, name " + Name);
			}

			return cSpriteList[Name].ScreenRect;
		}

		/// <summary>
		/// Render all visible sprites through the specified SpriteBatch object.
		/// By passing in different SpriteBatch objects the sprites can be rendered to different image buffers
		/// or rendered with different properties.
		/// </summary>
		/// <param name="CurrTime">The current time within the game.  This is used to update timed animations</param>
		/// <param name="DrawBatch">The oobject to render the sprites through</param>
		public void DrawSprites(GameTime CurrTime, SpriteBatch DrawBatch) {
			int FrameIndex, MoveX, MoveY;
			Point TileSetLoc;
			Rectangle ScreenRect;
			SpriteInfo CurrSprite;
			double ElapsedTime = CurrTime.TotalGameTime.TotalMilliseconds - cLastUpdate;
			List<string> KeyList = new List<string>(cSpriteList.Keys);

			foreach (string SpriteIndex in KeyList) {
				CurrSprite = cSpriteList[SpriteIndex];

				if (CurrSprite.Visible == true) {
					CurrSprite.AnimationTime += ElapsedTime;

					if ((String.IsNullOrWhiteSpace(CurrSprite.AnimationName) == false) && (CurrSprite.AnimationTime < CurrSprite.AnimationList[CurrSprite.AnimationName].TimeMS)) {
						//Calculate the animation frame that should be used
						FrameIndex = (int)Math.Floor(CurrSprite.AnimationTime / (CurrSprite.AnimationList[CurrSprite.AnimationName].TimeMS / CurrSprite.AnimationList[CurrSprite.AnimationName].TilePos[CurrSprite.Facing].Count));
						//Calculate how far the sprite has moved
						MoveX = (CurrSprite.AnimationMoveX / CurrSprite.AnimationList[CurrSprite.AnimationName].TilePos[CurrSprite.Facing].Count) * (FrameIndex + 1);
						MoveY = (CurrSprite.AnimationMoveY / CurrSprite.AnimationList[CurrSprite.AnimationName].TilePos[CurrSprite.Facing].Count) * (FrameIndex + 1);

						//Save off the coordinates of the tile to use
						TileSetLoc.X = CurrSprite.AnimationList[CurrSprite.AnimationName].TilePos[CurrSprite.Facing][FrameIndex].X;
						TileSetLoc.Y = CurrSprite.AnimationList[CurrSprite.AnimationName].TilePos[CurrSprite.Facing][FrameIndex].Y;

						//Save off the screen position to draw the sprite at
						ScreenRect = CurrSprite.ScreenRect;
						ScreenRect.X += MoveX;
						ScreenRect.Y += MoveY;

						//Draw the animation frame
						cTilesMgr.DrawTile(CurrSprite.TileSet, TileSetLoc.X, TileSetLoc.Y, DrawBatch, ScreenRect, CurrSprite.TintColor);
					} else {
						//Clear out any animation values
						CurrSprite.AnimationTime = 0;
						CurrSprite.AnimationName = "";

						//At the end of an animation update the position of the sprite
						if (CurrSprite.AnimationMoveX != 0) {
							CurrSprite.ScreenRect.X += CurrSprite.AnimationMoveX;
							CurrSprite.AnimationMoveX = 0;
						}

						if (CurrSprite.AnimationMoveY != 0) {
							CurrSprite.ScreenRect.Y += CurrSprite.AnimationMoveY;
							CurrSprite.AnimationMoveY = 0;
						}

						//Draw default sprite
						cTilesMgr.DrawTile(CurrSprite.TileSet, CurrSprite.DefaultTilePos[CurrSprite.Facing].X, CurrSprite.DefaultTilePos[CurrSprite.Facing].Y, DrawBatch, CurrSprite.ScreenRect, Color.White);
					}
				}

				//Update the sprite with the latest animation changes
				cSpriteList[SpriteIndex] = CurrSprite;
			}

			//Record the last time we modified the sprite
			cLastUpdate = CurrTime.TotalGameTime.TotalMilliseconds;
		}

		/// <summary>
		/// Enum containing all possible sprite facing directions
		/// </summary>
		public enum SpriteFacing {
			/// <summary>
			/// Sprite is facing downward/south
			/// </summary>
			Down,
			/// <summary>
			/// Sprite is facing upward, north
			/// </summary>
			Up,
			/// <summary>
			/// Sprite is facing left, west
			/// </summary>
			Left,
			/// <summary>
			/// Sprite is facing right, east
			/// </summary>
			Right,
		}

		/// <summary>
		/// Structure containing all information describing a sprite
		/// </summary>
		private struct SpriteInfo {
			/// <summary>
			/// Constructor to initialize all values in the structure
			/// </summary>
			/// <param name="SpriteName">Name to give this sprite</param>
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
				AnimationMoveX = 0;
				AnimationMoveY = 0;
				TintColor = Color.White;

				DefaultTilePos.Add(SpriteFacing.Down, new Point());
				DefaultTilePos.Add(SpriteFacing.Up, new Point());
				DefaultTilePos.Add(SpriteFacing.Left, new Point());
				DefaultTilePos.Add(SpriteFacing.Right, new Point());
			}

			/// <summary>
			/// Index name of this sprite
			/// </summary>
			public string Name;
			/// <summary>
			/// Index of the tileset containing this sprite's images
			/// </summary>
			public string TileSet;
			/// <summary>
			/// Direction the sprite should be facing
			/// </summary>
			public SpriteFacing Facing;
			/// <summary>
			/// Whether or not the sprite should be included when rendering
			/// </summary>
			public bool Visible;
			/// <summary>
			/// Size and position of the sprite on screen
			/// </summary>
			public Rectangle ScreenRect;
			/// <summary>
			/// Name of the current animation the sprite is performing, or an empty string if it is idle
			/// </summary>
			public string AnimationName;
			/// <summary>
			/// The duration of the animation being performed in milliseconds
			/// </summary>
			public double AnimationTime;
			/// <summary>
			/// The list and descriptions of all animations this sprite can perform
			/// </summary>
			public Dictionary<string, SpriteAnimation> AnimationList;
			/// <summary>
			/// The default images of this sprite, those used when it is idle organized by facing directions
			/// </summary>
			public Dictionary<SpriteFacing, Point> DefaultTilePos;
			/// <summary>
			/// The distance the sprite should move in along the X axis during the current animation
			/// </summary>
			public int AnimationMoveX;
			/// <summary>
			/// The distance the sprite should move in along the Y axis during the current animation
			/// </summary>
			public int AnimationMoveY;
			/// <summary>
			/// Any tinting color being applied to the sprite
			/// </summary>
			public Color TintColor;
		}

		/// <summary>
		/// Structure containing all information describing an animation
		/// </summary>
		private struct SpriteAnimation {
			/// <summary>
			/// Constructor to initialize all class members
			/// </summary>
			/// <param name="AnimationName">Name to give this animation</param>
			public SpriteAnimation(string AnimationName) {
				Name = AnimationName;
				TimeMS = 0;

				TilePos = new Dictionary<SpriteFacing, List<Point>>();

				TilePos.Add(SpriteFacing.Down, new List<Point>());
				TilePos.Add(SpriteFacing.Up, new List<Point>());
				TilePos.Add(SpriteFacing.Left, new List<Point>());
				TilePos.Add(SpriteFacing.Right, new List<Point>());
			}

			/// <summary>
			/// Index name of this animation
			/// </summary>
			public string Name;
			/// <summary>
			/// Number of milliseconds this animation should last
			/// </summary>
			public int TimeMS;
			/// <summary>
			/// All tiles used in the animation grouped by facing directions
			/// </summary>
			public Dictionary<SpriteFacing, List<Point>> TilePos;
		}
	}
}
