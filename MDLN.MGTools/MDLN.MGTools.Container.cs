using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MDLN.MGTools {
	public class Container {
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

		protected GraphicsDevice cGraphicsDevice;
		protected SpriteBatch cDrawBatch;

		public Container(GraphicsDevice GraphDev, int Height, int Width) : this (GraphDev, null, new Rectangle(0, 0, Width, Height)) { }

		public Container(GraphicsDevice GraphDev, Texture2D Background, int Height, int Width) : this (GraphDev, Background, new Rectangle(0, 0, Width, Height)) { }

		public Container(GraphicsDevice GraphDev, Texture2D Background, int Top , int Left, int Height, int Width) : this (GraphDev, Background, new Rectangle(Left, Top, Width, Height)) { }

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

			CalculateEffectSteps();
		}

		public Texture2D Background {
			get {
				return cBackTexture;
			}

			set {
				cBackTexture = value;
			}
		}

		public Color BackgroundColor {
			set {
				cBackTexture = new Texture2D(cGraphicsDevice, 1, 1);
				cBackTexture.SetData (new[] { value });
			}
		}

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
			}
		}

		public DisplayEffect OpenEffect {
			get {
				return cOpenEffect;
			}

			set {
				cOpenEffect = value;
			}
		}

		public DisplayEffect CloseEffect {
			get {
				return cCloseEffect;
			}

			set {
				cCloseEffect = value;
			}
		}

		public int Top {
			get {
				return cFullDrawRegion.Y;
			}

			set {
				cFullDrawRegion.Y = value;
				cOrigin.Y = cFullDrawRegion.Y;
			}
		}

		public int Left {
			get {
				return cFullDrawRegion.X;
			}

			set {
				cFullDrawRegion.X = value;
				cOrigin.X = cFullDrawRegion.X;
			}
		}

		public Vector2 TopLeft {
			get {
				return cOrigin;
			}

			set {
				cOrigin = value;

				cFullDrawRegion.X = (int)cOrigin.X;
				cFullDrawRegion.Y = (int)cOrigin.Y;
			}
		}

		public bool SendMouseEvents {
			get {
				return cSendMouseEvents;
			}

			set {
				cSendMouseEvents = value;
			}
		}

		public event ContainerMouseEnterEventHandler MouseEnter;

		public event ContainerMouseLeaveEventHandler MouseLeave;

		public event ContainerLeftMouseDownEventHandler LeftMouseDown;

		protected Rectangle ClientRegion {
			get {
				Rectangle Region = new Rectangle();

				Region.X = 0;
				Region.Y = 0;
				Region.Width = cFullDrawRegion.Width;
				Region.Height = cFullDrawRegion.Height;

				return Region;
			}
		}

		protected bool HasChanged {
			get {
				return cHasChanges;
			}

			set {
				cHasChanges = value;
			}
		}

		public void ToggleVisible() {
			if (cIsVisible == true) {
				Visible = false;
			} else {
				Visible = true;
			}
		}

		public void Update(GameTime CurrTime) {
			Update(CurrTime, Keyboard.GetState(), Mouse.GetState());
		}

		public void Update(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
			if ((cIsVisible == false) && (cIsClosing == false)) { //Only draw if container is shown
				return;
			}

			UpdateEffect();
			UpdateContents (CurrTime, CurrKeyboard, CurrMouse);

			if (cSendMouseEvents == true) {
				if ((cFullDrawRegion.Contains(CurrMouse.Position) == true) && ((cFullDrawRegion.Contains(cPriorMouse.Position) == false))) {
					if (MouseEnter != null) {
						MouseEnter(this, CurrMouse);
					}
				}

				if ((cFullDrawRegion.Contains(CurrMouse.Position) == false) && ((cFullDrawRegion.Contains(cPriorMouse.Position) == true))) {
					if (MouseLeave != null) {
						MouseLeave(this, CurrMouse);
					}
				}

				if ((CurrMouse.LeftButton == ButtonState.Pressed) && (cPriorMouse.LeftButton == ButtonState.Released) && (cFullDrawRegion.Contains(CurrMouse.Position) == true)) {
					if (LeftMouseDown != null) {
						LeftMouseDown(this, CurrMouse);
					}
				}

				cPriorMouse = CurrMouse;
			}

			if (cHasChanges == true) { //Don't recreate the texture unless changes need drawn
				//Render container to texture
				RenderTargetBinding[] RenderTargets;

				//Save render targets to restore later
				RenderTargets = cGraphicsDevice.GetRenderTargets ();

				//Set to render to buffer
				cGraphicsDevice.SetRenderTarget (cRenderToBuffer);

				//Do drawing
				cGraphicsDevice.Clear (new Color (255, 255, 255, 0)); //Start fully transparent 
				cDrawBatch.Begin (SpriteSortMode.Deferred, BlendState.AlphaBlend);
				cDrawBatch.Draw (cBackTexture, cGraphicsDevice.Viewport.Bounds, Color.White);

				DrawContents (CurrTime);

				cDrawBatch.End ();

				//Restore render targets
				cGraphicsDevice.SetRenderTargets (RenderTargets);

				cHasChanges = false;
			}
		}

		public virtual void DrawContents(GameTime CurrTime) { }
		public virtual void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) { }

		public void Draw() {
			if ((cIsVisible == false) && (cIsClosing == false)) { //Only draw if container is shown
				return;
			}

			Rectangle DrawRegion = new Rectangle();

			DrawRegion.X = cFullDrawRegion.X + (cFullDrawRegion.Width - cCurrDrawRegion.Width);
			DrawRegion.Y = cFullDrawRegion.Y + (cFullDrawRegion.Height - cCurrDrawRegion.Height);
			DrawRegion.Width = cCurrDrawRegion.Width - cCurrDrawRegion.X;
			DrawRegion.Height = cCurrDrawRegion.Height - cCurrDrawRegion.Y;

			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			cDrawBatch.Draw(cRenderToBuffer, DrawRegion, cCurrDrawRegion, cAlphaOverlay);
			cDrawBatch.End();
		}

		private void CalculateEffectSteps() {
			cSlideStepY = cFullDrawRegion.Height / 10;
			cSlideStepX = cFullDrawRegion.Width / 10;
			cFadeStep = 255 / 10;
		}

		private void RestartOpenEffect() {
			//Default is to use final region
			cCurrDrawRegion.X = 0;
			cCurrDrawRegion.Y = 0;
			cCurrDrawRegion.Width = cFullDrawRegion.Width;
			cCurrDrawRegion.Height = cFullDrawRegion.Height;

			switch (cOpenEffect) {
				case DisplayEffect.SlideUp:
					cCurrDrawRegion.Height = 1;
					break;
				case DisplayEffect.SlideDown:
					cCurrDrawRegion.Y = cFullDrawRegion.Height - 1;
					break;
				case DisplayEffect.SlideLeft:
					cCurrDrawRegion.Width = 1;
					break;
				case DisplayEffect.SlideRight:
					cCurrDrawRegion.X = cFullDrawRegion.Width - 1;
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

		private void UpdateEffect() {
			if (cIsClosing == true) { //Container is closing
				switch (cCloseEffect) {
					case DisplayEffect.SlideDown :
						cCurrDrawRegion.Height -= cSlideStepY;

						if (cCurrDrawRegion.Height <= 0) {
							cIsClosing = false;
						}

						break;
					case DisplayEffect.SlideUp :
						cCurrDrawRegion.Y += cSlideStepY;

						if (cCurrDrawRegion.Y >= cFullDrawRegion.Height) {
							cIsClosing = false;
						}

						break;
					case DisplayEffect.SlideLeft :
						cCurrDrawRegion.X += cSlideStepX;

						if (cCurrDrawRegion.X >= cFullDrawRegion.Width) {
							cIsClosing = false;
						}

						break;
					case DisplayEffect.SlideRight :
						cCurrDrawRegion.Width -= cSlideStepX;

						if (cCurrDrawRegion.Width <= 0) {
							cIsClosing = false;
						}

						break;
					case DisplayEffect.Fade :
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
					default :
						cIsClosing = false;
						break;
				}
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

	public delegate void ContainerMouseEnterEventHandler(object Sender, MouseState CurrMouse);

	public delegate void ContainerMouseLeaveEventHandler(object Sender, MouseState CurrMouse);

	public delegate void ContainerLeftMouseDownEventHandler(object Sender, MouseState CurrMouse);

	public enum DisplayEffect {
		None,
		SlideDown,
		SlideRight,
		SlideUp,
		SlideLeft,
		Zoom,
		Fade
	}
}