using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace MDLN.CardTable {
	public class FullCardPanel : Container {
		private const float MARGINTOPBOTTOMPERCENT = 0.017f;
		private const float MARGINLEFTRIGHTPERCENT = 0.03f;
		private const float CARDHEIGHTRATIO = (350f/250f);

		private CardInfo cCard;
		private Button cCloseBtn;

		public FullCardPanel(GraphicsDevice GraphDev, int Height, int Width) : base(GraphDev, Height, Width) {
			Card = null;
			cCloseBtn = new Button(GraphDev, null, 0 , 0, 75, 100);
			cCloseBtn.Text = "Close";
			cCloseBtn.BackgroundColor = Color.Navy;
			cCloseBtn.FontColor = Color.AntiqueWhite;
			cCloseBtn.Visible = true;
			cCloseBtn.Click += new ClickEvent(CloseBtnCLick);
		}

		public CardInfo Card { 
			get {
				return cCard;
			}

			set {
				cCard = value;
				HasChanged = true;
			}
		}

		public TextureFont Font { get; set; }

		protected override void DrawContents(GameTime CurrTime) {
			Rectangle CardRegion;

			CardRegion.X = (int)(Width * MARGINLEFTRIGHTPERCENT);
			CardRegion.Y = (int)(Height * MARGINTOPBOTTOMPERCENT);
			CardRegion.Width = Width - (int)(Width * MARGINLEFTRIGHTPERCENT * 2);
			CardRegion.Height = (int)(Width * CARDHEIGHTRATIO);

			cCard.RenderFullCard(CardRegion, Color.White, Color.DarkSlateGray, cDrawBatch, Font);
			cCard.HasChanged = false;

			cCloseBtn.Top = CardRegion.Height + (int)(Height * MARGINTOPBOTTOMPERCENT * 2);
			cCloseBtn.Left = CardRegion.X;
			cCloseBtn.Draw(cDrawBatch);
		}

		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse, bool ProcessMOuseEvent) {
			if (cCard.HasChanged == true) { //Trigger a redraw if the card information changes
				HasChanged = true;
			}
			HasChanged = true;
			cCloseBtn.Font = Font;
			cCloseBtn.Update(CurrTime, CurrKeyboard, CurrMouse);
		}

		private void CloseBtnCLick(object Sender, MouseButton MouseBtn) {
			if (MouseBtn == MouseButton.Left) {
				Visible = false;
			}
		}
	}
}

