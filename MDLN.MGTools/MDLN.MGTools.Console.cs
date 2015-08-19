using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	public class Console : Container {
		private Color cFontColor;
		private TextureFont cFont;
		private KeyboardState cPriorKeys;
		private bool cCursorOn;
		private string cCommand;
		private int cMaxLines;
		private List<string> cLines;

		public Console(GraphicsDevice GraphicsDev, ContentManager ContentMgr, string FontFile, int Top, int Left, int Width, int Height) : this(GraphicsDev, ContentMgr, FontFile, new Rectangle(Left, Top, Width, Height)) { }

		public Console(GraphicsDevice GraphicsDev, ContentManager ContentMgr, string FontFile, int Width, int Height) : this(GraphicsDev, ContentMgr, FontFile, new Rectangle(0, 0, Width, Height)) { }

		public Console(GraphicsDevice GraphicsDev, ContentManager ContentMgr, string FontFile, Rectangle Region) : base(GraphicsDev, null, Region) {
			cFont = new TextureFont();

			cFontColor = new Color(Color.LightBlue, 1.0f);
			BackgroundColor = new Color(Color.DarkGray, 0.75f);

			cFont.FontTexture = ContentMgr.Load<Texture2D>(FontFile);

			cMaxLines = (ClientRegion.Height / cFont.CharacterHeight) - 1; //Remove 1 line for the command input

			cPriorKeys = new KeyboardState();

			cCommand = "> ";
			cLines = new List<string>();
		}

		public event CommandSentEventHandler CommandSent;

		public Color FontColor {
			get {
				return cFontColor;
			}

			set {
				cFontColor = value;
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

			HasChanged = true;
		}

		public void ClearText() {
			cLines.Clear();
		}

		public override void UpdateContents(GameTime TotalTime, KeyboardState CurrKeys, MouseState CurrMouse) {
			Keys[] PressedList = CurrKeys.GetPressedKeys();
			string NewKeys = "";
			bool ShiftDown = false;

			if ((CurrKeys.IsKeyDown(Keys.LeftShift) == true) || (CurrKeys.IsKeyDown(Keys.RightShift) == true)) {
				ShiftDown = true;
			} else {
				ShiftDown = false;
			}

			if (TotalTime.TotalGameTime.Milliseconds <= 500) {
				if (cCursorOn == false) {
					HasChanged = true;
				}

				cCursorOn = true;
			} else {
				if (cCursorOn == true) {
					HasChanged = true;
				}

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
				HasChanged = true;

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

		public override void DrawContents(GameTime CurrTime) {
			Rectangle LetterPos = new Rectangle(0, 0, cFont.CharacterHeight, cFont.CharacterWidth);

			//Draw the current command
			LetterPos.Y = ClientRegion.Height - cFont.CharacterHeight;
			cFont.WriteText(cDrawBatch, cCommand, LetterPos.Y, LetterPos.X, cFontColor);

			if (cCursorOn == true) {
				cFont.WriteAsciiCharacter(cDrawBatch, new byte[] { 220 }, cFont.CharacterHeight, LetterPos.Y, cFont.DetermineRenderWidth(cCommand), cFontColor);
			}

			//Draw all text lines in reverse order (bottom to top)
			if (cLines.Count > 0) {
				LetterPos.Y -= cFont.CharacterHeight;
				for (int Ctr = cLines.Count - 1; Ctr >= 0; Ctr--) {
					cFont.WriteText(cDrawBatch, cLines[Ctr], LetterPos.Y, LetterPos.X, cFontColor);

					LetterPos.Y -= cFont.CharacterHeight;
				}
			}
		}
	}

	public delegate void CommandSentEventHandler(object Sender, string EventCommand);
}