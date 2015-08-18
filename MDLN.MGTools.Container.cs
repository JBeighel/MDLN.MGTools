using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;

[assembly: AssemblyVersion("0.1.0.1")]
[assembly: AssemblyFileVersion("0.1.0.1")]

namespace MDLN.MGTools {
	public class Container {
		private Rectangle cFullDrawRegion, cCurrDrawRegion;
		private Color cAlphaOverlay;
		private Texture2D cBackTexture;
		private int cSlideStepX, cSlideStepY;
		private byte cFadeStep;
		private bool cIsVisible, cIsClosing;
		private DisplayEffect cCloseEffect, cOpenEffect;
		private RenderTarget2D cRenderToBuffer;

		protected GraphicsDevice cGraphicsDevice;
		protected SpriteBatch cDrawBatch;

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

		public void ToggleVisible() {
			if (cIsVisible == true) {
				Visible = false;
			} else {
				Visible = true;
			}
		}

		public void Update(GameTime CurrTime) {
			if ((cIsVisible == false) && (cIsClosing == false)) { //Only draw if container is shown
				return;
			}

			UpdateEffect();

			//Render container to texture
			RenderTargetBinding[] RenderTargets;

			//Save render targets to restore later
			RenderTargets = cGraphicsDevice.GetRenderTargets();

			//Set to render to buffer
			cGraphicsDevice.SetRenderTarget(cRenderToBuffer);

			//Do drawing
			cGraphicsDevice.Clear(new Color(255, 255, 255, 0)); //Start fully transparent 
			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			cDrawBatch.Draw(cBackTexture, cGraphicsDevice.Viewport.Bounds, Color.White);

			DrawContents(CurrTime);

			cDrawBatch.End();

			//Restore render targets
			cGraphicsDevice.SetRenderTargets(RenderTargets);
		}

		public virtual void DrawContents(GameTime CurrTime) { }

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

	public enum DisplayEffect {
		None,
		SlideDown,
		SlideRight,
		SlideUp,
		SlideLeft,
		Fade
	}
}