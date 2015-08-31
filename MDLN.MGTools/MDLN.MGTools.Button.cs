using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace MDLN.MGTools {
	/// <summary>
	/// Class to act as a button or label control in monogame projects
	/// </summary>
	public class Button : Container {
		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.Button"/> class.
		/// </summary>
		/// <param name="GraphDev">Connection to Graph device.</param>
		/// <param name="Background">Background texture</param>
		/// <param name="Top">Top coordinate of control</param>
		/// <param name="Left">Left coordinate of control</param>
		/// <param name="Height">Height of control</param>
		/// <param name="Width">Width of control</param>
		public Button(GraphicsDevice GraphDev, Texture2D Background, int Top , int Left, int Height, int Width) : base(GraphDev, Background, Top, Left, Height, Width) {
			
		}

		/// <summary>
		/// Gets or sets the text being displayed on the control
		/// </summary>
		/// <value>The text.</value>
		public string Text { get; set; }

		/// <summary>
		/// Texture font object to use in rendering the text
		/// </summary>
		/// <value>The font object</value>
		public TextureFont Font { get; set; }

		/// <summary>
		/// Gets or sets the color to use when rendering the text
		/// </summary>
		/// <value>The color of the font.</value>
		public Color FontColor{ get; set; }

		/// <summary>
		/// Function to render the contents of hte control to the screen or rendering targetsd
		/// </summary>
		/// <param name="CurrTime">Current time information</param>
		public override void DrawContents(GameTime CurrTime) {
			if (Text.Length != 0) {
				Font.WriteText(cDrawBatch, Text, 0, 0, FontColor);
			}
		}
	}
}

