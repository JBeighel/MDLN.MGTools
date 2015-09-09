using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

namespace MDLN.MGTools {
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

		private void ControlClicked(object Sender, MouseButton Button) {
			HasFocus = true;
		}
	}
}

