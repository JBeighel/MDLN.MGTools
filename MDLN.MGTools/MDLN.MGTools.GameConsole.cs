using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MDLN.MGTools {
	/// <summary>
	/// Class that acts as a text console drawn in Monogame
	/// </summary>
	public class GameConsole : Container {
		private Color cFontColor;
		private TextureFont cFont;
		private bool cCursorOn;
		private string cCommand;
		private int cMaxLines, cHistCtr;
		private List<string> cLines;
		private Queue<string> cCommandHist;

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.GameConsole"/> class.
		/// </summary>
		/// <param name="GraphicsDev">Connection to the Graphics device</param>
		/// <param name="ContentMgr">Connection to the content manager</param>
		/// <param name="FontFile">Image file container the font texture</param>
		/// <param name="Top">Top screen coordinate for the console</param>
		/// <param name="Left">Left screen coordinate for the console</param>
		/// <param name="Width">Width of the console on screen</param>
		/// <param name="Height">Height of the console on screen</param>
		public GameConsole(GraphicsDevice GraphicsDev, ContentManager ContentMgr, string FontFile, int Top, int Left, int Width, int Height) : this(GraphicsDev, ContentMgr, FontFile, new Rectangle(Left, Top, Width, Height)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.GameConsole"/> class.
		/// </summary>
		/// <param name="GraphicsDev">Connection to the Graphics device</param>
		/// <param name="ContentMgr">Connection to the content manager</param>
		/// <param name="FontFile">Image file container the font texture</param>
		/// <param name="Width">Width of the console on screen</param>
		/// <param name="Height">Height of the console on screen</param>
		public GameConsole(GraphicsDevice GraphicsDev, ContentManager ContentMgr, string FontFile, int Width, int Height) : this(GraphicsDev, ContentMgr, FontFile, new Rectangle(0, 0, Width, Height)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.GameConsole"/> class.
		/// </summary>
		/// <param name="GraphicsDev">Connection to the Graphics device</param>
		/// <param name="ContentMgr">Connection to the content manager</param>
		/// <param name="FontFile">Image file container the font texture</param>
		/// <param name="Region">Region on the screen to draw the console</param>
		public GameConsole(GraphicsDevice GraphicsDev, ContentManager ContentMgr, string FontFile, Rectangle Region) : base(GraphicsDev, null, Region) {
			cFont = new TextureFont();

			cFontColor = new Color(Color.LightBlue, 1.0f);
			BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.75f);

			cFont.FontTexture = ContentMgr.Load<Texture2D>(FontFile);

			cMaxLines = (ClientRegion.Height / cFont.CharacterHeight) - 1; //Remove 1 line for the command input

			cPriorKeys = new KeyboardState();

			cCommandHist = new Queue<string>();
			cHistCtr = 0;

			cCommand = "> ";
			cLines = new List<string>();

			AccessKey = Keys.OemTilde;
			UseAccessKey = false;
		}

		/// <summary>
		/// Occurs when a command is submitted to the console
		/// </summary>
		public event CommandSentEventHandler CommandSent;

		/// <summary>
		/// Gets or sets the color of the font.
		/// </summary>
		/// <value>The color of the font.</value>
		public Color FontColor {
			get {
				return cFontColor;
			}

			set {
				cFontColor = value;
			}
		}

		/// <summary>
		/// Adds a new line of text to be displayed in the console.  A carraige return will be added to the end
		/// </summary>
		/// <param name="Text">Text to display</param>
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

		/// <summary>
		/// Clears all text currently being shown in the console
		/// </summary>
		public void ClearText() {
			cLines.Clear();
		}

		/// <summary>
		/// Processes inputs to determine if any new content should be displayed in the console
		/// </summary>
		/// <param name="CurrKeys">Current state of the keyboard.</param>
		/// <param name="CurrMouse">Current state of the mouse.</param>
		/// <param name="TotalTime">Current time information</param>
		protected override void UpdateContents(GameTime TotalTime, KeyboardState CurrKeys, MouseState CurrMouse) {
			string NewKeys = MGInput.GetTypedChars(CurrKeys, cPriorKeys);

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

			if (NewKeys.CompareTo("") != 0) {
				HasChanged = true;

				if (NewKeys.CompareTo("\b") == 0) {
					if (cCommand.Length > 2) {
						cCommand = cCommand.Substring(0, cCommand.Length - 1);
					}
				} else if (NewKeys.CompareTo("\n") == 0) {
					if (CommandSent != null) {
						CommandSent(this, cCommand.Substring(2)); //Start at two to chop off the > prompt

						//Add command to history
						cCommandHist.Enqueue(cCommand.Substring(2));

						while (cCommandHist.Count > 10) {
							cCommandHist.Dequeue();
						}

						cHistCtr = cCommandHist.Count;
					}

					cCommand = "> ";
				} else {
					cCommand += NewKeys;
				}
			}

			if ((CurrKeys.IsKeyDown(Keys.Up) == true) && (cPriorKeys.IsKeyDown(Keys.Up) == false)) {
				cHistCtr--;

				if (cHistCtr < 0) { //0 is oldest command in the history
					cHistCtr = 0;
				}

				if (cHistCtr < cCommandHist.Count) {
					cCommand = "> " + cCommandHist.ElementAt(cHistCtr);
					HasChanged = true;
				}
			}

			if ((CurrKeys.IsKeyDown(Keys.Down) == true) && (cPriorKeys.IsKeyDown(Keys.Down) == false)) {
				cHistCtr++;

				if (cHistCtr > cCommandHist.Count) {
					cHistCtr = cCommandHist.Count;
				}

				if (cHistCtr < cCommandHist.Count) {
					cCommand = "> " + cCommandHist.ElementAt(cHistCtr);
					HasChanged = true;
				}
			}

			//Update the prior keys
			cPriorKeys = CurrKeys;
		}

		/// <summary>
		/// This function renders the contents of the console
		/// </summary>
		/// <param name="CurrTime">Current time information</param>
		protected override void DrawContents(GameTime CurrTime) {
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

	/// <summary>
	/// Command sent event handler.
	/// </summary>
	public delegate void CommandSentEventHandler(object Sender, string EventCommand);
}