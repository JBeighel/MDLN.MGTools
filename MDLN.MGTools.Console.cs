using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection;

[assembly: AssemblyVersion("0.1.0.1")]
[assembly: AssemblyFileVersion("0.1.0.1")]

namespace MDLN.MGTools {
	public class Console {
		private Rectangle cDrawRegion;
		private Color cBackColor, cFontColor;
		private Texture2D cBackTexture, cFontTexture;
		private KeyboardState cPriorKeys;
		private GraphicsDevice cGraphicsDevice;
		private bool cShowConsole, cClosingConsole, cCursorOn;
		private SpriteBatch cDrawBatch;
		private string cCommand;
		private int cTextureColWidth, cTextureRowHeight, cMaxLines, cFinalTop, cFinalLeft, cSlideStepX, cSlideStepY;
		private List<string> cLines;
		private SlideEffect cSlideOpen, cSlideClosed;

		public Console(GraphicsDevice GraphicsDev, ContentManager ContentMgr, string FontFile, int Top, int Left, int Width, int Height) : this(GraphicsDev, ContentMgr, FontFile, new Rectangle(Left, Top, Width, Height)) { }

		public Console(GraphicsDevice GraphicsDev, ContentManager ContentMgr, string FontFile, int Width, int Height) : this(GraphicsDev, ContentMgr, FontFile, new Rectangle(0, 0, Width, Height)) { }

		public Console(GraphicsDevice GraphicsDev, ContentManager ContentMgr, string FontFile, Rectangle Region) {
			cGraphicsDevice = GraphicsDev;
			cDrawRegion = Region;
			cFinalTop = Region.Y;
			cFinalLeft = Region.X;

			cSlideStepX = Region.Width / 10;
			cSlideStepY = Region.Height / 10;

			cBackColor = new Color(Color.DarkGray, 0.75f);
			cFontColor = new Color(Color.LightBlue, 1.0f);

			CreateBackTexture();
			cFontTexture = ContentMgr.Load<Texture2D>(FontFile);
			cTextureColWidth = cFontTexture.Width / 16;
			cTextureRowHeight = cFontTexture.Height / 16;
			cMaxLines = (cDrawRegion.Height / cTextureRowHeight) - 1; //Remove 1 line for the command input

			cPriorKeys = new KeyboardState();
			cShowConsole = false;

			cDrawBatch = new SpriteBatch(cGraphicsDevice);

			cCommand = "> ";
			cLines = new List<string>();
		}

		public event CommandSentEventHandler CommandSent;

		public Color BackgroundColor {
			get {
				return cBackColor;
			}

			set {
				cBackColor = value;
				CreateBackTexture();
			}
		}

		public Color FontColor {
			get {
				return cFontColor;
			}

			set {
				cFontColor = value;
			}
		}

		public bool Visible {
			get {
				return cShowConsole;
			}

			set {
				if ((cShowConsole == true) && (value == false)) {
					cClosingConsole = true;
				} else if ((cShowConsole == false) && (value == true)) {
					RestartSlideEffect();
				}

				cShowConsole = value;
			}
		}

		public SlideEffect SlideOpen {
			get {
				return cSlideOpen;
			}

			set {
				cSlideOpen = value;
			}
		}

		public SlideEffect SlideClosed {
			get {
				return cSlideClosed;
			}

			set {
				cSlideClosed = value;
			}
		}

		public void ToggleVisible() {
			if (cShowConsole == true) {
				cShowConsole = false;
				cClosingConsole = true;
			} else {
				cShowConsole = true;
				RestartSlideEffect();
			}
		}

		public void AddText(string Text) {
			Text = Text.Replace("\r\n", "\n");
			Text = Text.Replace("\r", "\n");
			string[] Lines = Text.Split('\n');

			foreach (string Line in Lines) {
				cLines.Add(Line);
			}

			while (cLines.Count > cMaxLines) {
				cLines.RemoveAt(0);
			}
		}

		public void ClearText() {
			cLines.Clear();
		}

		public void Update(TimeSpan TotalTime, KeyboardState CurrKeys) {
			if ((cShowConsole == false) && (cClosingConsole == false)) { //If console isn't shown, do nothing
				cPriorKeys = CurrKeys;
				return;
			}

			Keys[] PressedList = CurrKeys.GetPressedKeys();
			string NewKeys = "";
			bool ShiftDown = false;

			UpdateSlideEffect();
			
			if ((CurrKeys.IsKeyDown(Keys.LeftShift) == true) || (CurrKeys.IsKeyDown(Keys.RightShift) == true)) {
				ShiftDown = true;
			} else {
				ShiftDown = false;
			}

			if (TotalTime.Milliseconds <= 500) {
				cCursorOn = true;
			} else {
				cCursorOn = false;
			}

			foreach (Keys CurrKey in PressedList) {
				if (cPriorKeys.IsKeyDown(CurrKey) == false) {
					if ((CurrKey >= Keys.A) && (CurrKey <= Keys.Z)) {
						if (ShiftDown == true) {
							NewKeys += CurrKey.ToString();
						} else {
							NewKeys += CurrKey.ToString().ToLower();
						}
					} else if ((CurrKey >= Keys.D0) && (CurrKey <= Keys.D9)) {
						string Num = ((int)(CurrKey - Keys.D0)).ToString();

						if (ShiftDown == true) {
							switch (Num)
							{
								case "1":
									NewKeys += "!";
									break;
								case "2":
									NewKeys += "@";
									break;
								case "3":
									NewKeys += "#";
									break;
								case "4":
									NewKeys += "$";
									break;
								case "5":
									NewKeys += "%";
									break;
								case "6":
									NewKeys += "^";
									break;
								case "7":
									NewKeys += "&";
									break;
								case "8":
									NewKeys += "*";
									break;
								case "9":
									NewKeys += "(";
									break;
								case "0":
									NewKeys += ")";
									break;
								default:
									//wtf?
									break;
							}
						} else {
							NewKeys += ((int)(CurrKey - Keys.D0)).ToString();
						}
					} else if ((CurrKey >= Keys.NumPad0) && (CurrKey <= Keys.NumPad9)) {
						NewKeys += ((int)(CurrKey - Keys.NumPad0)).ToString();
					} else if (CurrKey == Keys.Space) {
						NewKeys += " ";
					} else if (CurrKey == Keys.Enter) {
						NewKeys += "\n";
					} else if (CurrKey == Keys.Back) {
						NewKeys += "\b";
					}
				}
			}

			if (NewKeys.CompareTo("") != 0) {
				if (NewKeys.CompareTo("\b") == 0) {
					if (cCommand.Length > 2) {
						cCommand = cCommand.Substring(0, cCommand.Length - 1);
					}
				} else if (NewKeys.CompareTo("\n") == 0) {
					if (CommandSent != null) {
						CommandSent(this, cCommand.Substring(2)); //Start at two to chop off the > prompt
					}

					cCommand = "> ";
				} else {
					cCommand += NewKeys;
				}
			}

			//Update the prior keys
			cPriorKeys = CurrKeys;
		}

		public void Draw() {
			if ((cShowConsole == false) && (cClosingConsole == false)) { //If console isn't shown, do nothing
				return;
			}

			Rectangle LetterPos = new Rectangle(0, 0, cTextureColWidth, cTextureRowHeight);
			Color AlphaOverlay = new Color(255, 255, 255, cBackColor.A);

			cDrawBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

			cDrawBatch.Draw(cBackTexture, cDrawRegion, AlphaOverlay);

			//Draw the current command
			LetterPos.X = cDrawRegion.X;
			LetterPos.Y = (cDrawRegion.Height + cDrawRegion.Top) - cTextureRowHeight;
			foreach(byte CurrChar in System.Text.Encoding.UTF8.GetBytes(cCommand)) {
				if (LetterPos.X + cTextureColWidth > cDrawRegion.X + cDrawRegion.Width) {
					cCursorOn = false;
					break; //Stop drawing to avoid writing outside the draw region
				}

				cDrawBatch.Draw(cFontTexture, LetterPos, GetCharacterTextureRegion(CurrChar), cFontColor);
				LetterPos.X += cTextureColWidth;
			}

			if (cCursorOn == true) {
				cDrawBatch.Draw(cFontTexture, LetterPos, GetCharacterTextureRegion(220), cFontColor);
			}

			//Draw all text lines in reverse order (bottom to top)
			if (cLines.Count > 0) {
				LetterPos.X = cDrawRegion.X;
				LetterPos.Y -= cTextureRowHeight;
				for (int Ctr = cLines.Count - 1; Ctr >= 0; Ctr--) {
					foreach(byte CurrChar in System.Text.Encoding.UTF8.GetBytes(cLines[Ctr])) {
						if (LetterPos.X + cTextureColWidth > cDrawRegion.X + cDrawRegion.Width) {
							break; //Stop drawing to avoid writing outside the draw region
						}

						cDrawBatch.Draw(cFontTexture, LetterPos, GetCharacterTextureRegion(CurrChar), cFontColor);
						LetterPos.X += cTextureColWidth;
					}

					LetterPos.X = cDrawRegion.X;
					LetterPos.Y -= cTextureRowHeight;
				}
			}

			cDrawBatch.End();
		}

		protected void CreateBackTexture() {
			cBackTexture = new Texture2D(cGraphicsDevice, 1, 1);
			cBackTexture.SetData(new[] { Color.DarkGray });
		}

		protected Rectangle GetCharacterTextureRegion(byte Ascii) {
			int TexRow, TexCol;

			TexRow = Ascii / 16;
			TexCol = Ascii % 16;

			return new Rectangle(TexCol * cTextureColWidth, TexRow * cTextureRowHeight, cTextureColWidth, cTextureRowHeight);
		}

		private void RestartSlideEffect() {
			cDrawRegion.X = cFinalLeft;
			cDrawRegion.Y = cFinalTop;

			switch (cSlideOpen) {
				case SlideEffect.SlideUp:
					cDrawRegion.Y = cFinalLeft + cDrawRegion.Height;
					break;
				case SlideEffect.SlideDown:
					cDrawRegion.Y = cFinalLeft - cDrawRegion.Height;
					break;
				case SlideEffect.SlideLeft:
					cDrawRegion.X = cFinalLeft + cDrawRegion.Width;
					break;
				case SlideEffect.SlideRight:
					cDrawRegion.X = cFinalLeft - cDrawRegion.Width;
					break;
				default:
					break;
			}
		}

		private void UpdateSlideEffect() {
			if (cClosingConsole == false) { //Slide console open
				if (cDrawRegion.X < cFinalLeft) {
					cDrawRegion.X += cSlideStepX;

					if (cDrawRegion.X > cFinalLeft) {
						cDrawRegion.X = cFinalLeft;
					}
				} else if (cDrawRegion.X > cFinalLeft) {
					cDrawRegion.X -= cSlideStepX;

					if (cDrawRegion.X < cFinalLeft) {
						cDrawRegion.X = cFinalLeft;
					}
				}

				if (cDrawRegion.Y < cFinalTop) {
					cDrawRegion.Y += cSlideStepY;

					if (cDrawRegion.Y > cFinalTop) {
						cDrawRegion.Y = cFinalTop;
					}
				} else if (cDrawRegion.Y > cFinalTop) {
					cDrawRegion.X -= cSlideStepY;

					if (cDrawRegion.Y < cFinalTop) {
						cDrawRegion.Y = cFinalTop;
					}
				}
			} else { //Slide console closed
				switch (cSlideClosed) {
					case SlideEffect.SlideUp:
						cDrawRegion.Y -= cSlideStepY;

						if (cDrawRegion.Y <= cFinalTop - cDrawRegion.Height) {
							cClosingConsole = false;
						}
						break;
					case SlideEffect.SlideDown:
						cDrawRegion.Y += cSlideStepY;

						if (cDrawRegion.Y >= cFinalTop + cDrawRegion.Height) {
							cClosingConsole = false;
						}
						break;
					case SlideEffect.SlideLeft:
						cDrawRegion.X -= cSlideStepX;

						if (cDrawRegion.X <= cFinalLeft - cDrawRegion.Width) {
							cClosingConsole = false;
						}
						break;
					case SlideEffect.SlideRight:
						cDrawRegion.X += cSlideStepX;

						if (cDrawRegion.X >= cFinalLeft + cDrawRegion.Width) {
							cClosingConsole = false;
						}
						break;
					default:
						cClosingConsole = false;
						break;
				}
			}
		}
	}

	public delegate void CommandSentEventHandler(object Sender, string EventCommand);

	public enum SlideEffect {
		None,
		SlideDown,
		SlideRight,
		SlideUp,
		SlideLeft
	}
}