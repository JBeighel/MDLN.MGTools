using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace MDLN.CardTable {
	public class CardMini : Button {
		private CardInfo cCard;

		public CardMini(GraphicsDevice GraphDev, int Height, int Width) : base(GraphDev, null, 0, 0, Height, Width) {
			cCard = null;
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

		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
			if (cCard.HasChanged == true) { //Trigger a redraw if the card information changes
				HasChanged = true;
			}

			base.UpdateContents(CurrTime, CurrKeyboard, CurrMouse);
		}

		protected override void DrawContents(GameTime CurrTime) {
			cCard.RenderMiniCard(ClientRegion, Color.White, FontColor, cDrawBatch, Font);
			cCard.HasChanged = false;

			base.DrawContents(CurrTime);
		}
	}
}

