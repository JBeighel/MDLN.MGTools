using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace MDLN.MGTools {
	/// <summary>
	/// Class to act as a button or label control in monogame projects
	/// </summary>
	public class Button : Container, IControl {
		private bool cMouseDown;
		private MouseButton cMouseButtonDown;
		private string cCaption;
		private TextureFont cFont;

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
			Alignment = Justify.MiddleCenter;
			cCaption = "";
			FontSize = 10;
		}

		/// <summary>
		/// Gets or sets the text being displayed on the control
		/// </summary>
		/// <value>The text.</value>
		public string Text {
			get {
				return cCaption;
			}

			set {
				cCaption = value;
				HasChanged = true;
			}
		}

		/// <summary>
		/// Texture font object to use in rendering the text
		/// </summary>
		/// <value>The font object</value>
		public TextureFont Font {
			get {
				return cFont;
			}

			set {
				cFont = value;
				HasChanged = true;
			}
		}

		/// <summary>
		/// Gets or sets the alignment of the text inside the control area
		/// </summary>
		/// <value>The alignment of the text.</value>
		public Justify Alignment { get; set; }

		/// <summary>
		/// Gets or sets the color to use when rendering the text
		/// </summary>
		/// <value>The color of the font.</value>
		public Color FontColor{ get; set; }

		/// <summary>
		/// Sets or gets the size of the font in pixels
		/// </summary>
		public int FontSize { get; set; }

		/// <summary>
		/// Occurs when the mouse clicks this control.
		/// </summary>
		public event ClickEvent Click;

		/// <summary>
		/// This function is called whenever a mouse button is pressed while the mouse is in the screen space this container
		/// is displayed in
		/// </summary>
		/// <param name="CurrMouse">Current mouse state.</param>
		/// <param name="Button">Button that triggered this event.</param>
		protected override void MouseEventButtonDown(MouseState CurrMouse, MouseButton Button) {
			cMouseDown = true;
			cMouseButtonDown = Button;
		}

		/// <summary>
		/// This function is called whenever the mouse leaves the screen space this container is displayed in
		/// </summary>
		/// <param name="CurrMouse">Current mouse state.</param>
		protected override void MouseEventLeave(MouseState CurrMouse) {
			cMouseDown = false;
		}

		/// <summary>
		/// This function is called whenever a mouse button is released while the mouse is in the screen space this container
		/// is displayed in
		/// </summary>
		/// <param name="CurrMouse">Current mouse state.</param>
		/// <param name="Button">Button that triggered this event.</param>
		protected override void MouseEventButtonUp(MouseState CurrMouse, MouseButton Button) {
			if ((cMouseDown == true) && (cMouseButtonDown == Button)) { //Trigger mouse click event
				if (Click != null) {
					Click(this, Button);
				}
			}
		}

		/// <summary>
		/// Function to render the contents of hte control to the screen or rendering targetsd
		/// </summary>
		/// <param name="CurrTime">Current time information</param>
		protected override void DrawContents(GameTime CurrTime) {
			string[] Lines;
			int Ctr, LineWidth, LinesHeight, TextTop, TextLeft;

			if (cCaption.Length != 0) {
				Lines = cCaption.Split('\n');

				LinesHeight = Lines.Length * FontSize;

				for (Ctr = 0; Ctr < Lines.Length; Ctr++) {
					LineWidth = cFont.DetermineRenderWidth(Lines[Ctr], FontSize);

					switch (Alignment) {
						case Justify.TopLeft:
							TextTop = 0 + (Ctr * FontSize);
							TextLeft = 0;
							break;
						case Justify.TopCenter:
							TextTop = 0 + (Ctr * FontSize);
							TextLeft = (Width - LineWidth) / 2;
							break;
						case Justify.TopRight:
							TextTop = 0 + (Ctr * FontSize);
							TextLeft = Width - LineWidth;
							break;
						case Justify.MiddleLeft:
							TextTop = ((Height - LinesHeight) / 2) + (Ctr * FontSize);
							TextLeft = 0;
							break;
						case Justify.MiddleRight:
							TextTop = ((Height - LinesHeight) / 2) + (Ctr * FontSize);
							TextLeft = Width - LineWidth;
							break;
						case Justify.BottomLeft:
							TextTop = (Height - LinesHeight) + (Ctr * FontSize);
							TextLeft = 0;
							break;
						case Justify.BottomCenter:
							TextTop = (Height - LinesHeight) + (Ctr * FontSize);
							TextLeft = (Width - LineWidth) / 2;
							break;
						case Justify.BottomRight:
							TextTop = (Height - LinesHeight) + (Ctr * FontSize);
							TextLeft = Width - LineWidth;
							break;
						default : //Middle Center
							TextTop = ((Height - LinesHeight) / 2) + (Ctr * FontSize);
							TextLeft = (Width - LineWidth) / 2;
							break;
					}

					cFont.WriteText(cDrawBatch, Lines[Ctr], FontSize, TextTop, TextLeft, FontColor);
				}
			}
		}
	}

	/// <summary>
	/// Interface defining common attributes of user interface controls
	/// </summary>
	public interface IControl {
		/// <summary>
		/// Gets or sets the text displayed on the control
		/// </summary>
		/// <value>The text.</value>
		string Text { get; set; }

		/// <summary>
		/// Occurs when the mouse clicks the control
		/// </summary>
		event ClickEvent Click;

		/// <summary>
		/// Updates the control based on current information
		/// </summary>
		/// <param name="CurrTime">Current game time.</param>
		void Update(GameTime CurrTime);

		/// <summary>
		/// Draws the control to the screen
		/// </summary>
		void Draw();
	}

	/// <summary>
	/// Event handler for button clicks
	/// </summary>
	public delegate void ClickEvent(object Sender, MouseButton Button);

	/// <summary>
	/// Used to specify the alignment of the text within a control
	/// </summary>
	public enum Justify {
		/// <summary>
		/// Text vertically starts at the top of the control 
		/// Text horizontally starts at the left of the control
		/// </summary>
		TopLeft,
		/// <summary>
		/// Text vertically starts at the top of the control 
		/// Text horizontally starts at the center of the control
		/// </summary>
		TopCenter,
		/// <summary>
		/// Text vertically starts at the top of the control 
		/// Text horizontally starts at the right of the control
		/// </summary>
		TopRight,
		/// <summary>
		/// Text vertically starts at the middle of the control 
		/// Text horizontally starts at the left of the control
		/// </summary>
		MiddleLeft,
		/// <summary>
		/// Text vertically starts at the middle of the control 
		/// Text horizontally starts at the center of the control
		/// </summary>
		MiddleCenter,
		/// <summary>
		/// Text vertically starts at the middle of the control 
		/// Text horizontally starts at the right of the control
		/// </summary>
		MiddleRight,
		/// <summary>
		/// Text vertically starts at the bottom of the control 
		/// Text horizontally starts at the left of the control
		/// </summary>
		BottomLeft,
		/// <summary>
		/// Text vertically starts at the bottom of the control 
		/// Text horizontally starts at the center of the control
		/// </summary>
		BottomCenter,
		/// <summary>
		/// Text vertically starts at the bottom of the control 
		/// Text horizontally starts at the right of the control
		/// </summary>
		BottomRight
	}
}

