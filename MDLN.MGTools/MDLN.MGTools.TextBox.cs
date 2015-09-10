using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace MDLN.MGTools {
	/// <summary>
	/// A control to allow the user to enter text input
	/// </summary>
	public class TextBox : Button, IControl, IVisible {
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
		protected override void UpdateContents(GameTime CurrTime, Microsoft.Xna.Framework.Input.KeyboardState CurrKeyboard, Microsoft.Xna.Framework.Input.MouseState CurrMouse) {
			if ((CurrMouse.LeftButton == ButtonState.Pressed) || (CurrMouse.LeftButton == ButtonState.Pressed)) { //Mouse button is pressed
				if (ClientRegion.Contains(CurrMouse.Position) == false) {//Mouse is outside of the control, it has lost focus
					HasFocus = false;
				}
			}

			if (HasFocus == false) { //Only update if the control has focus

			}
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

