using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Windows.Input;

using System.IO;

namespace MDLN.MGTools {
	/// <summary>
	/// Class that acts as a visual container for content drawn in Monogame
	/// </summary>
	public class Container : IVisible {
		private Rectangle cFullDrawRegion, cCurrDrawRegion;
		private Color cAlphaOverlay;
		private Texture2D cBackTexture;
		private int cSlideStepX, cSlideStepY;
		private byte cFadeStep;
		private bool cIsVisible, cIsClosing, cHasChanges, cSendMouseEvents;
		private DisplayEffect cCloseEffect, cOpenEffect;
		private RenderTarget2D cRenderToBuffer;
		private Vector2 cOrigin;
		private MouseState cPriorMouse;
		private int cAnimationTime;

		/// <summary>
		/// Connection to the graphics rendering device
		/// </summary>
		protected GraphicsDevice cGraphicsDevice;
		/// <summary>
		/// Variable used to draw batches of 2D images
		/// </summary>
		protected SpriteBatch cDrawBatch;
		/// <summary>
		/// State of the keyboard the last time this class was updated
		/// </summary>
		protected KeyboardState cPriorKeys;

		/// <summary>
		/// Constructor that sets minimum information for the <see cref="MDLN.MGTools.Container"/> class
		/// </summary>
		/// <param name="GraphDev">Connection to graphics device</param>
		/// <param name="Height">Screen height of the container</param>
		/// <param name="Width">Screen width of the container</param>
		public Container(GraphicsDevice GraphDev, int Height, int Width) : this (GraphDev, null, new Rectangle(0, 0, Width, Height)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.Container"/> class.
		/// </summary>
		/// <param name="GraphDev">Connection to graphics device</param>
		/// <param name="Background">Texture to use as the background of the container</param>
		/// <param name="Height">Screen height of the container</param>
		/// <param name="Width">Screen width of the container</param>
		public Container(GraphicsDevice GraphDev, Texture2D Background, int Height, int Width) : this (GraphDev, Background, new Rectangle(0, 0, Width, Height)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.Container"/> class.
		/// </summary>
		/// <param name="GraphDev">Connection to graphics device</param>
		/// <param name="Background">Texture to use as the background of the container</param>
		/// <param name="Top">Top screen coordinate to draw the container</param>
		/// <param name="Left">Left screen coordinate to draw the container</param>
		/// <param name="Height">Screen height of the container</param>
		/// <param name="Width">Screen width of the container</param>
		public Container(GraphicsDevice GraphDev, Texture2D Background, int Top , int Left, int Height, int Width) : this (GraphDev, Background, new Rectangle(Left, Top, Width, Height)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MDLN.MGTools.Container"/> class.
		/// </summary>
		/// <param name="GraphDev">Connection to graphics device</param>
		/// <param name="Background">Texture to use as the background of the container</param>
		/// <param name="DrawArea">Rectangular region on screen to draw the container</param>
		public Container(GraphicsDevice GraphDev, Texture2D Background, Rectangle DrawArea) {
			cGraphicsDevice = GraphDev;
			cBackTexture = Background;
			cFullDrawRegion = DrawArea;

			cCurrDrawRegion = new Rectangle();
			cAlphaOverlay = new Color(255, 255, 255, 255);
			cRenderToBuffer = new RenderTarget2D(cGraphicsDevice, cFullDrawRegion.Width, cFullDrawRegion.Height);
			cDrawBatch = new SpriteBatch(cGraphicsDevice);
			cIsVisible = false;
			cIsClosing = false;
			cHasChanges = true;
			cSendMouseEvents = false;

			cOrigin.X = cFullDrawRegion.X;
			cOrigin.Y = cFullDrawRegion.Y;

			cAnimationTime = 250;

			if (cBackTexture == null) { //No background texture was specified, use transparent
				cBackTexture = new Texture2D(cGraphicsDevice, 1, 1);
				cBackTexture.SetData(new[] { Color.Transparent });
			}
		}

		/// <summary>
		/// Gets or sets the background texture for the container
		/// </summary>
		/// <value>The background texture</value>
		public Texture2D Background {
			get {
				return cBackTexture;
			}

			set {
				cBackTexture = value;
				cHasChanges = true;
			}
		}

		/// <summary>
		/// Sets the color of the background, this flat color will be used in place of a texture
		/// </summary>
		/// <value>The color of the background.</value>
		public Color BackgroundColor {
			set {
				cBackTexture = new Texture2D(cGraphicsDevice, 1, 1);
				cBackTexture.SetData (new[] { value });
				cHasChanges = true;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MDLN.MGTools.Container"/> is visible.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		public bool Visible {
			get {
				return cIsVisible;
			}

			set {
				if ((cIsVisible == true) && (value == false)) { //Request to close container
					cIsClosing = true;
				} else if ((cIsVisible == false) && (value == true)) { //Request to open container
					cIsClosing = false;
					RestartOpenEffect();
				}

				cIsVisible = value;
				cHasChanges = true;
				VisibleChanged();
			}
		}

		/// <summary>
		/// Gets or sets the effect to use to animate the container when it is shown
		/// </summary>
		/// <value>The open effect.</value>
		public DisplayEffect OpenEffect {
			get {
				return cOpenEffect;
			}

			set {
				cOpenEffect = value;
			}
		}

		/// <summary>
		/// Gets or sets the effect to use to animate the container when it is closed
		/// </summary>
		/// <value>The close effect.</value>
		public DisplayEffect CloseEffect {
			get {
				return cCloseEffect;
			}

			set {
				cCloseEffect = value;
			}
		}

		/// <summary>
		/// Gets or sets the duration of the open and close effects in milliseconds
		/// </summary>
		/// <value>The duration of the effect.</value>
		public int EffectDuration {
			get {
				return cAnimationTime;
			}

			set {
				cAnimationTime = value;
			}
		}

		/// <summary>
		/// Gets or sets the top screen coordinate of the container.
		/// </summary>
		/// <value>The top.</value>
		public virtual int Top {
			get {
				return cFullDrawRegion.Y;
			}

			set {
				cFullDrawRegion.Y = value;
				cOrigin.Y = cFullDrawRegion.Y;
				HasChanged = true;

				Repositioned();
			}
		}

		/// <summary>
		/// Gets or sets the left coordinate of the container.
		/// </summary>
		/// <value>The left.</value>
		public virtual int Left {
			get {
				return cFullDrawRegion.X;
			}

			set {
				cFullDrawRegion.X = value;
				cOrigin.X = cFullDrawRegion.X;
				HasChanged = true;

				Repositioned();
			}
		}

		/// <summary>
		/// Gets the height of the container in pixels
		/// </summary>
		/// <value>The height in pixesls</value>
		public int Height {
			get {
				return cFullDrawRegion.Height;
			}

			set {
				if (cFullDrawRegion.Height != 0) {
					cCurrDrawRegion.Height = (cCurrDrawRegion.Height / cFullDrawRegion.Height) * value;
				}

				cFullDrawRegion.Height = value;
				cRenderToBuffer = new RenderTarget2D(cGraphicsDevice, cFullDrawRegion.Width, cFullDrawRegion.Height);
				cHasChanges = true;

				Resized();
			}
		}

		/// <summary>
		/// Gets the width of the container in pixels
		/// </summary>
		/// <value>The width in pixels</value>
		public int Width {
			get {
				return cFullDrawRegion.Width;
			}

			set {
				if (cFullDrawRegion.Width != 0) {
					cCurrDrawRegion.Width = (cCurrDrawRegion.Width / cFullDrawRegion.Width) * value;
				} else if (cFullDrawRegion.Width == cCurrDrawRegion.Width) {
					cCurrDrawRegion.Width = value;
				}

				cFullDrawRegion.Width = value;
				cRenderToBuffer = new RenderTarget2D(cGraphicsDevice, cFullDrawRegion.Width, cFullDrawRegion.Height);
				cHasChanges = true;

				Resized();
			}
		}

		/// <summary>
		/// Gets or sets the top left screen coordinates of the container.
		/// </summary>
		/// <value>The top left screen coordinates.</value>
		public virtual Vector2 TopLeft {
			get {
				return cOrigin;
			}

			set {
				cOrigin = value;

				cFullDrawRegion.X = (int)cOrigin.X;
				cFullDrawRegion.Y = (int)cOrigin.Y;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MDLN.MGTools.Container"/> should send mouse events.
		/// </summary>
		/// <value><c>true</c> if mouse events should be sent; otherwise, <c>false</c>.</value>
		public bool SendMouseEvents {
			get {
				return cSendMouseEvents;
			}

			set {
				cSendMouseEvents = value;
			}
		}

		/// <summary>
		/// Occurs when the mouse enters the containers screen space
		/// </summary>
		public event ContainerMouseEnterEventHandler MouseEnter;

		/// <summary>
		/// Occurs when the mouse leave the containers screen space
		/// </summary>
		public event ContainerMouseLeaveEventHandler MouseLeave;

		/// <summary>
		/// Occurs when the left mouse buttone goes down over the container's screen space
		/// </summary>
		public event ContainerMouseButtonEventHandler MouseDown;

		/// <summary>
		/// Occurs when the left mouse button goes up over the container's screen space
		/// </summary>
		public event ContainerMouseButtonEventHandler MouseUp;

		/// <summary>
		/// Gets the client region of the container.  Specifies where the content should be drawn within this container.
		/// </summary>
		/// <value>The client region.</value>
		protected Rectangle ClientRegion {
			get {
				Rectangle Region = new Rectangle {
					X = 0,
					Y = 0,
					Width = cFullDrawRegion.Width,
					Height = cFullDrawRegion.Height
				};

				return Region;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether visible content has changed.  This will trigger a redraw of the buffer used to display
		/// the containers
		/// </summary> 
		/// <value><c>true</c> if the visible data has changed; otherwise, <c>false</c>.</value>
		protected bool HasChanged {
			get {
				return cHasChanges;
			}

			set {
				cHasChanges = value;
			}
		}

		/// <summary>
		/// Get or Set a key to use to open or close the conainer if UseAccessKey is true.
		/// </summary>
		/// <value>The access key.</value>
		public Keys AccessKey { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MDLN.MGTools.Container"/> should use access key.
		/// </summary>
		/// <value><c>true</c> if use access key; otherwise, <c>false</c>.</value>
		public bool UseAccessKey { get; set; }

		/// <summary>
		/// Toggles the visible state of the container.  Hides it if it's shown, or shows it if its hidden
		/// </summary>
		public void ToggleVisible() {
			if (cIsVisible == true) {
				Visible = false;
			} else {
				Visible = true;
			}
		}

		/// <summary>
		/// Update function to be called during the game update routine.  Will attempt to collect currect keyboard
		/// and mouse input for the update.
		/// </summary>
		/// <param name="CurrTime">Current time information in the game</param>
		public bool Update(GameTime CurrTime) {
			return Update(CurrTime, Keyboard.GetState(), Mouse.GetState(), true);
		}

		/// <summary>
		/// Update function to be called during the game update routine.  Will use the provided keybaord and mouse
		/// state for determining effects from user input.
		/// </summary>
		/// <param name="CurrTime">Current time information in the game</param>
		/// <param name="CurrKeyboard">Current state of the keyboard.</param>
		/// <param name="CurrMouse">Current state of the mouse.</param>
		public bool Update(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
			return Update(CurrTime, CurrKeyboard, CurrMouse, true);
		}

		/// <summary>
		/// Update function to be called during the game update routine.  Will use the provided keybaord and mouse
		/// state for determining effects from user input.
		/// </summary>
		/// <param name="CurrTime">Current time information in the game</param>
		/// <param name="CurrKeyboard">Current state of the keyboard.</param>
		/// <param name="CurrMouse">Current state of the mouse.</param>
		/// <param name="ProcessMouseEvent">True to have this container check it should check for mouse events.  False to ignore mouse input.</param>
		/// <returns>True if this container recived a mouse input event.  False if no mouse input was accepted.</returns>
		public bool Update(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse, bool ProcessMouseEvent) {
			bool TookMouseEvent = false;

			if ((UseAccessKey == true) && (CurrKeyboard.IsKeyDown(AccessKey) == true) && (cPriorKeys.IsKeyDown(AccessKey) == false)) {
				ToggleVisible();
			}

			if ((cIsVisible == false) && (cIsClosing == false)) { //Only draw if container is shown
				cPriorKeys = CurrKeyboard;
				return false;
			}

			//Create a MouseState based on the top left of the container for inheriting classes to use
			MouseState ContMouse = new MouseState(CurrMouse.X - Left, CurrMouse.Y - Top, CurrMouse.ScrollWheelValue, CurrMouse.LeftButton, CurrMouse.MiddleButton, CurrMouse.RightButton, CurrMouse.XButton1, CurrMouse.XButton2);

			//Update the container
			UpdateEffect(CurrTime.ElapsedGameTime.TotalMilliseconds);

			//Update the contents
			UpdateContents (CurrTime, CurrKeyboard, ContMouse, ProcessMouseEvent);

			if (ProcessMouseEvent == false) {
				cPriorMouse = CurrMouse;
			}

			//Trigger events
			if ((cFullDrawRegion.Contains(CurrMouse.Position) == true) && ((cFullDrawRegion.Contains(cPriorMouse.Position) == false))) {
				if ((MouseEnter != null) && (cSendMouseEvents == true)) {
					MouseEnter(this, CurrMouse);
				}

				MouseEventEnter(ContMouse);
				TookMouseEvent = true;
			}

			if ((cFullDrawRegion.Contains(CurrMouse.Position) == false) && ((cFullDrawRegion.Contains(cPriorMouse.Position) == true))) {
				if ((MouseLeave != null) && (cSendMouseEvents == true)) {
					MouseLeave(this, CurrMouse);
				}

				MouseEventLeave(ContMouse);
				TookMouseEvent = true;
			}

			if ((CurrMouse.LeftButton == ButtonState.Pressed) && (cPriorMouse.LeftButton == ButtonState.Released) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseDown != null) && (cSendMouseEvents == true)) {
					MouseDown(this, MouseButton.Left, CurrMouse);
				}

				MouseEventButtonDown(ContMouse, MouseButton.Left);
				TookMouseEvent = true;
			}

			if ((CurrMouse.RightButton == ButtonState.Pressed) && (cPriorMouse.RightButton == ButtonState.Released) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseDown != null) && (cSendMouseEvents == true)) {
					MouseDown(this, MouseButton.Right, CurrMouse);
				}

				MouseEventButtonDown(ContMouse, MouseButton.Right);
				TookMouseEvent = true;
			}

			if ((CurrMouse.MiddleButton == ButtonState.Pressed) && (cPriorMouse.MiddleButton == ButtonState.Released) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseDown != null) && (cSendMouseEvents == true)) {
					MouseDown(this, MouseButton.Middle, CurrMouse);
				}

				MouseEventButtonDown(ContMouse, MouseButton.Middle);
				TookMouseEvent = true;
			}

			if ((CurrMouse.XButton1 == ButtonState.Pressed) && (cPriorMouse.XButton1 == ButtonState.Released) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseDown != null) && (cSendMouseEvents == true)) {
					MouseDown(this, MouseButton.XButton1, CurrMouse);
				}

				MouseEventButtonDown(ContMouse, MouseButton.XButton1);
				TookMouseEvent = true;
			}

			if ((CurrMouse.XButton2 == ButtonState.Pressed) && (cPriorMouse.XButton2 == ButtonState.Released) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseDown != null) && (cSendMouseEvents == true)) {
					MouseDown(this, MouseButton.XButton2, CurrMouse);
				}

				MouseEventButtonDown(ContMouse, MouseButton.XButton2);
				TookMouseEvent = true;
			}

			if ((CurrMouse.LeftButton == ButtonState.Released) && (cPriorMouse.LeftButton == ButtonState.Pressed) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseUp != null) && (cSendMouseEvents == true)) {
					MouseUp(this, MouseButton.Left, CurrMouse);
				}

				MouseEventButtonUp(ContMouse, MouseButton.Left);
				TookMouseEvent = true;
			}

			if ((CurrMouse.RightButton == ButtonState.Released) && (cPriorMouse.RightButton == ButtonState.Pressed) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseUp != null) && (cSendMouseEvents == true)) {
					MouseUp(this, MouseButton.Right, CurrMouse);
				}

				MouseEventButtonUp(ContMouse, MouseButton.Right);
				TookMouseEvent = true;
			}

			if ((CurrMouse.MiddleButton == ButtonState.Released) && (cPriorMouse.MiddleButton == ButtonState.Pressed) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseUp != null) && (cSendMouseEvents == true)) {
					MouseUp(this, MouseButton.Middle, CurrMouse);
				}

				MouseEventButtonUp(ContMouse, MouseButton.Middle);
				TookMouseEvent = true;
			}

			if ((CurrMouse.XButton1 == ButtonState.Released) && (cPriorMouse.XButton1 == ButtonState.Pressed) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseUp != null) && (cSendMouseEvents == true)) {
					MouseUp(this, MouseButton.XButton1, CurrMouse);
				}

				MouseEventButtonUp(ContMouse, MouseButton.XButton1);
				TookMouseEvent = true;
			}

			if ((CurrMouse.XButton2 == ButtonState.Released) && (cPriorMouse.XButton2 == ButtonState.Pressed) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
				if ((MouseUp != null) && (cSendMouseEvents == true)) {
					MouseUp(this, MouseButton.XButton2, CurrMouse);
				}

				MouseEventButtonUp(ContMouse, MouseButton.XButton2);
				TookMouseEvent = true;
			}

			cPriorMouse = CurrMouse;

			if (cHasChanges == true) { //Don't recreate the texture unless changes need drawn
				//Render container to texture
				RenderTargetBinding[] RenderTargets;

				//Save render targets to restore later
				RenderTargets = cGraphicsDevice.GetRenderTargets();

				//Set to render to buffer
				cGraphicsDevice.SetRenderTarget(cRenderToBuffer);

				//Do drawing
				cGraphicsDevice.Clear(new Color (0, 0, 0, 0)); //Start fully transparent 

				cDrawBatch.Begin (SpriteSortMode.Deferred, BlendState.NonPremultiplied);
				cDrawBatch.Draw (cBackTexture, cGraphicsDevice.Viewport.Bounds, Color.White);

				DrawContents (CurrTime);

				cDrawBatch.End ();

				//Restore render targets
				cGraphicsDevice.SetRenderTargets (RenderTargets);

				cHasChanges = false;
			}

			cPriorKeys = CurrKeyboard;

			return TookMouseEvent;
		}

		/// <summary>
		/// Saves the appearance of this container to a PNG file
		/// </summary>
		/// <param name="PNGFileName">Path and file name of the PNG image file to write to</param>
		public void SaveAsPNG(string PNGFileName) {
			Stream ImageFile = File.Create(PNGFileName);

			cRenderToBuffer.SaveAsPng(ImageFile, cRenderToBuffer.Width, cRenderToBuffer.Height);
			ImageFile.Close();
			ImageFile.Dispose();
		}

		/// <summary>
		/// This function should be overridden with code to render the contents of this container.
		/// It will be called during the update routine.
		/// </summary>
		/// <param name="CurrTime">Current time information</param>
		protected virtual void DrawContents(GameTime CurrTime) { }

		/// <summary>
		/// This function is used to render the contents of the container.  It will render them to a buffer instead of direct to the screen.
		/// It may be called during the update routine to avoid clearing a buffer when a frame is being drawn as the render object
		/// will need to be changed.
		/// </summary>
		/// <param name="CurrTime">Currend time information</param>
		/// <param name="CurrKeyboard">Current state of the keyboard.</param>
		/// <param name="CurrMouse">Current state of the mouse.</param>
		/// <param name="ProcessMouseEvent">Set true to allow this containing to process mouse events, false to ignore them</param>
		protected virtual void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse, bool ProcessMouseEvent) { }

		/// <summary>
		/// This function is called whenever the mouse enters the screen space this container is displayed in
		/// </summary>
		/// <param name="CurrMouse">Curr mouse.</param>
		protected virtual void MouseEventEnter(MouseState CurrMouse) { }

		/// <summary>
		/// This function is called whenever the mouse leaves the screen space this container is displayed in
		/// </summary>
		protected virtual void MouseEventLeave(MouseState CurrMouse) { }

		/// <summary>
		/// This function is called whenever a mouse button is pressed while the mouse is in the screen space this container is displayed in
		/// </summary>
		protected virtual void MouseEventButtonDown(MouseState CurrMouse, MouseButton Button) { }

		/// <summary>
		/// This function is called whenever a mouse button is released while the mouse is in the screen space this container is displayed in
		/// </summary>
		protected virtual void MouseEventButtonUp(MouseState CurrMouse, MouseButton Button) { }

		/// <summary>
		/// Called when the container is resized, height or width changed
		/// </summary>
		protected virtual void Resized() { }

		/// <summary>
		/// Called when the container is moved, top or left changed
		/// </summary>
		protected virtual void Repositioned() { }

		/// <summary>
		/// Called when the container is shown or hidden, visible changed
		/// </summary>
		protected virtual void VisibleChanged() { }

		/// <summary>
		/// Draw function to be called when a frame is rendered for the game.  The class will create it's own SpriteBatch to draw with.
		/// </summary>
		public bool Draw() {
			bool bRetVal;

			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			bRetVal = Draw(cDrawBatch);
			cDrawBatch.End();

			return bRetVal;
		}

		/// <summary>
		/// Draw function to be called when a frame is rendered for the game
		/// </summary>
		/// <param name="DrawBatch">Specify the sprite object to use for the drawing</param>
		public bool Draw(SpriteBatch DrawBatch) {
			if ((cIsVisible == false) && (cIsClosing == false)) { //Only draw if container is shown
				return true;
			}

			Rectangle ScreenRegion = new Rectangle();
			Rectangle TextureRegion = new Rectangle();

			ScreenRegion.X = cFullDrawRegion.X + cCurrDrawRegion.X;
			ScreenRegion.Y = cFullDrawRegion.Y + cCurrDrawRegion.Y;
			ScreenRegion.Width = cCurrDrawRegion.Width - cCurrDrawRegion.X;
			ScreenRegion.Height = cCurrDrawRegion.Height - cCurrDrawRegion.Y;

			TextureRegion.X = cFullDrawRegion.Width - cCurrDrawRegion.Width;
			TextureRegion.Y = cFullDrawRegion.Height - cCurrDrawRegion.Height;
			TextureRegion.Width = cCurrDrawRegion.Width - cCurrDrawRegion.X;
			TextureRegion.Height = cCurrDrawRegion.Height - cCurrDrawRegion.Y;

			DrawBatch.Draw(cRenderToBuffer, ScreenRegion, TextureRegion, cAlphaOverlay);

			return true;
		}

		/// <summary>
		/// Returns the center point of this container
		/// </summary>
		/// <returns>Center coordinates of the container</returns>
		public Vector2 GetCenterCoordinates() {
			Vector2 Center;

			Center.X = TopLeft.X + (Width / 2);
			Center.Y = TopLeft.Y + (Height / 2);

			return Center;
		}

		private void CalculateEffectStep(double ElapsedTime) {
			cSlideStepY = (int)((float)(ElapsedTime / cAnimationTime) * (float)cFullDrawRegion.Height);
			cSlideStepX = (int)((float)(ElapsedTime / cAnimationTime) * (float)cFullDrawRegion.Width);
			cFadeStep = (byte)((float)(ElapsedTime / cAnimationTime) * 255f);
		}

		private void RestartOpenEffect() {
			//Default is to use final region
			cCurrDrawRegion.X = 0;
			cCurrDrawRegion.Y = 0;
			cCurrDrawRegion.Width = cFullDrawRegion.Width;
			cCurrDrawRegion.Height = cFullDrawRegion.Height;

			switch (cOpenEffect) {
				case DisplayEffect.SlideUp:
					cCurrDrawRegion.Y = cFullDrawRegion.Height - 1;
					break;
				case DisplayEffect.SlideDown:
					cCurrDrawRegion.Height = 1;
					break;
				case DisplayEffect.SlideLeft:
					cCurrDrawRegion.X = cFullDrawRegion.Width - 1;
					break;
				case DisplayEffect.SlideRight:
					cCurrDrawRegion.Width = 1;
					break;
				case DisplayEffect.Fade:
					cAlphaOverlay.A = 0;
					break;
				case DisplayEffect.Zoom:
					cCurrDrawRegion.X = (cFullDrawRegion.Width / 2) - 1;
					cCurrDrawRegion.Width = (cFullDrawRegion.Width / 2) + 1;

					cCurrDrawRegion.Y = (cFullDrawRegion.Height / 2) - 1;
					cCurrDrawRegion.Height = (cFullDrawRegion.Height / 2) + 1;
					break;
				default:
					break;
			}
		}

		private void UpdateEffect(double ElapsedTime) {
			CalculateEffectStep(ElapsedTime);

			if (cIsClosing == true) { //Container is closing
				switch (cCloseEffect) {
				case DisplayEffect.SlideDown:
					cCurrDrawRegion.Y += cSlideStepY;

					if (cCurrDrawRegion.Y >= cFullDrawRegion.Height) {
						cIsClosing = false;
					}

					break;
				case DisplayEffect.SlideUp:
					cCurrDrawRegion.Height -= cSlideStepY;

					if (cCurrDrawRegion.Height <= 0) {
						cIsClosing = false;
					}

					break;
				case DisplayEffect.SlideRight:
					cCurrDrawRegion.X += cSlideStepX;

					if (cCurrDrawRegion.X >= cFullDrawRegion.Width) {
						cIsClosing = false;
					}

					break;
				case DisplayEffect.SlideLeft:
					cCurrDrawRegion.Width -= cSlideStepX;

					if (cCurrDrawRegion.Width <= 0) {
						cIsClosing = false;
					}

					break;
				case DisplayEffect.Fade:
					if (cAlphaOverlay.A - cFadeStep <= 0) {
						cIsClosing = false;
					} else {
						cAlphaOverlay.A -= cFadeStep;
					}
					break;
				case DisplayEffect.Zoom:
					cCurrDrawRegion.Height -= cSlideStepY;
					cCurrDrawRegion.Y += cSlideStepY;
					cCurrDrawRegion.X += cSlideStepX;
					cCurrDrawRegion.Width -= cSlideStepX;

					if (cCurrDrawRegion.Height <= cFullDrawRegion.Height / 2) {
						cIsClosing = false;
					}

					break;
				default:
					cIsClosing = false;
					break;
				}
			} else if (OpenEffect == DisplayEffect.None) {
				cCurrDrawRegion.X = 0;
				cCurrDrawRegion.Y = 0;
				cCurrDrawRegion.Width = cFullDrawRegion.Width;
				cCurrDrawRegion.Height = cFullDrawRegion.Height;
			} else { //Container is opening 
				if (cCurrDrawRegion.X > 0) {
					cCurrDrawRegion.X -= cSlideStepX;

					if (cCurrDrawRegion.X <= 0) {
						cCurrDrawRegion.X = 0;
					}
				}

				if (cCurrDrawRegion.Width < cFullDrawRegion.Width) {
					cCurrDrawRegion.Width += cSlideStepX;

					if (cCurrDrawRegion.Width >= cFullDrawRegion.Width) {
						cCurrDrawRegion.Width = cFullDrawRegion.Width;
					}
				}

				if (cCurrDrawRegion.Y > 0) {
					cCurrDrawRegion.Y -= cSlideStepY;

					if (cCurrDrawRegion.Y <= 0) {
						cCurrDrawRegion.Y = 0;
					}
				}

				if (cCurrDrawRegion.Height < cFullDrawRegion.Height) {
					cCurrDrawRegion.Height += cSlideStepY;

					if (cCurrDrawRegion.Height >= cFullDrawRegion.Height) {
						cCurrDrawRegion.Height = cFullDrawRegion.Height;
					}
				}

				if (cAlphaOverlay.A < 255) {
					if (cAlphaOverlay.A + cFadeStep >= 255) {
						cAlphaOverlay.A = 255;
					} else {
						cAlphaOverlay.A += cFadeStep;
					}
				}
			}
		}
	}

	/// <summary>
	/// Container mouse enter event handler.
	/// </summary>
	public delegate void ContainerMouseEnterEventHandler(object Sender, MouseState CurrMouse);

	/// <summary>
	/// Container mouse leave event handler.
	/// </summary>
	public delegate void ContainerMouseLeaveEventHandler(object Sender, MouseState CurrMouse);

	/// <summary>
	/// Container left mouse down event handler.
	/// </summary>
	public delegate void ContainerMouseButtonEventHandler(object Sender, MouseButton ButtonDown, MouseState CurrMouse);

	/// <summary>
	/// List of effects that can be used to animate the container beign opened or closed
	/// </summary>
	public enum DisplayEffect {
		/// <summary>
		/// No effect, container will appear all at once
		/// </summary>
		None,
		/// <summary>
		/// Container will slide ito view from the top edge down
		/// </summary>
		SlideDown,
		/// <summary>
		/// Container will slide into view from the left edge right
		/// </summary>
		SlideRight,
		/// <summary>
		/// Container will slide into view from the bottom edge upwards
		/// </summary>
		SlideUp,
		/// <summary>
		/// COntainer will slide into view from the right edge left
		/// </summary>
		SlideLeft,
		/// <summary>
		/// Container will begin at the center and grow outwards
		/// </summary>
		Zoom,
		/// <summary>
		/// Container will change transpareny over time
		/// </summary>
		Fade
	}

	/// <summary>
	/// Enumeration of all mouse buttons
	/// </summary>
	public enum MouseButton {
		/// <summary>
		/// Left mouse button
		/// </summary>
		Left,
		/// <summary>
		/// Middle mouse button
		/// </summary>
		Middle,
		/// <summary>
		/// Rigth mouse button
		/// </summary>
		Right,
		/// <summary>
		/// First extended button
		/// </summary>
		XButton1,
		/// <summary>
		/// Second extended button
		/// </summary>
		XButton2
	}
}
