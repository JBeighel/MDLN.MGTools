using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace MDLN.MGTools {
	/// <summary>
	/// A control to allow the user to enter text input
	/// </summary>
	public class TextBox : Button, IControl, IVisible {
		private bool cCursorOn;

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.TextBox"/> class.
		/// </summary>
		/// <param name="GraphDev">Connection to Graph device.</param>
		/// <param name="Background">Background texture</param>
		/// <param name="Top">Top coordinate of control</param>
		/// <param name="Left">Left coordinate of control</param>
		/// <param name="Height">Height of control</param>
		/// <param name="Width">Width of control</param>
		public TextBox(GraphicsDevice GraphDev, Texture2D Background, int Top , int Left, int Height, int Width) : base(GraphDev, Background, Top, Left, Height, Width) {
			Click += new ClickEvent(ControlClicked);
			cCursorOn = false;
		}

		/// <summary>
		/// True if the control will accept and handle keyboard input, false to ignore keyboard input
		/// </summary>
		/// <value><c>true</c> if this instance has focus; otherwise, <c>false</c>.</value>
		public bool HasFocus { get; set; }

		/// <summary>
		/// This function is used to update the contents of the text box. 
		/// </summary>
		/// <param name="CurrTime">Currend time information</param>
		/// <param name="CurrKeyboard">Current state of the keyboard.</param>
		/// <param name="CurrMouse">Current state of the mouse.</param>
		/// <param name="ProcessMouseEvent">Set true to allow this object to process mouse events, false to ignore them</param>
		protected override void UpdateContents(GameTime CurrTime, Microsoft.Xna.Framework.Input.KeyboardState CurrKeyboard, Microsoft.Xna.Framework.Input.MouseState CurrMouse, bool ProcessMouseEvent) {
			if ((ProcessMouseEvent == true) && ((CurrMouse.LeftButton == ButtonState.Pressed) || (CurrMouse.LeftButton == ButtonState.Pressed))) { //Mouse button is pressed
				if (ClientRegion.Contains(CurrMouse.Position) == false) {//Mouse is outside of the control, it has lost focus
					if (HasFocus != false) {
						HasChanged = true;	
					}

					HasFocus = false;
				}
			}

			if (HasFocus != false) { //Only update if the control has focus
				string Input = MGInput.GetTypedChars(CurrKeyboard, cPriorKeys);

				if (CurrTime.TotalGameTime.Milliseconds <= 500) {
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

				if (Input.CompareTo("\b") == 0) {
					if (Text.Length > 0) {
						Text = Text.Substring(0, Text.Length - 1);
						HasChanged = true;
					}
				} else {
					Text += Input;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Ensures that the cursor is drawn, all other rendering is done by the base class
		/// </summary>
		/// <param name="CurrTime">Current time information</param>
		protected override void DrawContents(GameTime CurrTime) {
			string[] Lines;
			int CursorTop, CursorLeft;

			if ((cCursorOn == true) && (HasFocus == true)) {
				Lines = Text.Split('\n');

				switch (Alignment) {
					case Justify.TopLeft:
						CursorTop = (Lines.Length - 1) * FontSize;
						CursorLeft = Font.DetermineRenderWidth(Lines[Lines.Length - 1], FontSize);
						break;
					case Justify.TopCenter:
						CursorTop = (Lines.Length - 1) * FontSize;
						CursorLeft = (Width + Font.DetermineRenderWidth(Lines[Lines.Length - 1], FontSize)) / 2;
						break;
					case Justify.TopRight:
						CursorTop = (Lines.Length - 1) * FontSize;
						CursorLeft = Width - FontSize;
						break;
					case Justify.MiddleLeft:
						CursorTop = (Height + (FontSize * Lines.Length)) / 2 - FontSize;
						CursorLeft = Font.DetermineRenderWidth(Lines[Lines.Length - 1], FontSize);
						break;
					case Justify.MiddleRight:
						CursorTop = (Height + (FontSize * Lines.Length)) / 2 - FontSize;
						CursorLeft = Width - FontSize;
						break;
					case Justify.BottomLeft:
						CursorTop = Height - FontSize;
						CursorLeft = Font.DetermineRenderWidth(Lines[Lines.Length - 1], FontSize);
						break;
					case Justify.BottomCenter:
						CursorTop = Height - FontSize;
						CursorLeft = (Width + Font.DetermineRenderWidth(Lines[Lines.Length - 1], FontSize)) / 2;
						break;
					case Justify.BottomRight:
						CursorTop = Height - FontSize;
						CursorLeft = Width - FontSize;
						break;
					default : //Middle Center
						CursorTop = (Height + (FontSize * Lines.Length)) / 2 - FontSize;
						CursorLeft = (Width + Font.DetermineRenderWidth(Lines[Lines.Length - 1], FontSize)) / 2;
						break;
				}

				Font.WriteAsciiCharacter(cDrawBatch, new byte[] { 220 }, FontSize, CursorTop , CursorLeft, FontColor);
			}

			base.DrawContents(CurrTime);
		}

		/// <summary>
		/// Retrieves teh coordinates of the center of the control
		/// </summary>
		/// <returns>The center coordinates.</returns>
		public Vector2 GetCenterCoordinates() {
			return new Vector2(Left + (Height / 2), Top + (Width / 2));
		}

		private void ControlClicked(object Sender, MouseButton Button) {
			HasFocus = true;
		}
	}
}

