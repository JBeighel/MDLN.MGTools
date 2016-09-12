using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DescentApp {
	class OverlordDecksFrame : Container {
		private const int DECKOPTIONS_PERPAGE = 15;

		public TextureFont Font;

		private OverlordCardsFrame cCardsFrame;
		private Dictionary<string, Button> cDeckChoiceCtrlList;
		private List<OverlordCard> cOverlordCardList;

		public OverlordDecksFrame(GraphicsDevice GraphDev, int Height, int Width)
			: base(GraphDev, Height, Width) {
			cCardsFrame = new OverlordCardsFrame(GraphDev, Height, GraphDev.Viewport.Width - Width);
			cCardsFrame.Top = 0;
			cCardsFrame.Left = Width;
			cCardsFrame.Visible = false;
			cCardsFrame.OpenEffect = DisplayEffect.SlideUp;
			cCardsFrame.CloseEffect = DisplayEffect.SlideDown;

			cDeckChoiceCtrlList = new Dictionary<string, Button>();
			cOverlordCardList = new List<OverlordCard>();

			BackgroundColor = new Color(0.8f, 0.608f, 0.398f, 1.0f);
			cCardsFrame.BackgroundColor = new Color(0.8f, 0.608f, 0.398f, 1.0f);
		}

		public void AddOverlordCard(OverlordCard NewCard) {
			cOverlordCardList.Add(NewCard);

			if (cDeckChoiceCtrlList.ContainsKey(NewCard.Class) == false) {
				cDeckChoiceCtrlList.Add(NewCard.Class, new Button(cGraphicsDevice, null, 0, 0, ClientRegion.Height / DECKOPTIONS_PERPAGE, ClientRegion.Width));
				cDeckChoiceCtrlList[NewCard.Class].BackgroundColor = new Color(0.8f, 0.608f, 0.398f, 1.0f);
				cDeckChoiceCtrlList[NewCard.Class].Visible = true;
				cDeckChoiceCtrlList[NewCard.Class].Text = NewCard.Class;
				cDeckChoiceCtrlList[NewCard.Class].Font = Font;
				cDeckChoiceCtrlList[NewCard.Class].FontColor = Color.Black;
				cDeckChoiceCtrlList[NewCard.Class].FontSize = (ClientRegion.Height / DECKOPTIONS_PERPAGE) / 2;
				cDeckChoiceCtrlList[NewCard.Class].Click += DeckChoiceClickHandler;

				HasChanged = true;
			}
		}
		
		public new void Draw(SpriteBatch DrawBatch) {
			cCardsFrame.Draw();
			base.Draw(DrawBatch);
		}

		protected override void DrawContents(GameTime CurrTime) {
			cCardsFrame.Draw();

			foreach(Button DeckOption in cDeckChoiceCtrlList.Values) {
				DeckOption.Draw(cDrawBatch);
			}
		}

		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
			int OptionTop = 0;

			if (Visible == true) {
				OptionTop += 0;
			}

			cCardsFrame.Update(CurrTime, CurrKeyboard, CurrMouse);

			foreach (Button DeckOption in cDeckChoiceCtrlList.Values) {
				DeckOption.Top = OptionTop;
				DeckOption.Update(CurrTime, CurrKeyboard, CurrMouse);

				OptionTop += ClientRegion.Height / DECKOPTIONS_PERPAGE;
			}

			HasChanged = true; //Hack fix for the deck option selection, forces full redraw every frame
		}

		protected override void Resized() {
			foreach (Button DeckOption in cDeckChoiceCtrlList.Values) {
				DeckOption.Width = ClientRegion.Width;
				DeckOption.Height = ClientRegion.Height / DECKOPTIONS_PERPAGE;
				DeckOption.FontSize = (ClientRegion.Height / DECKOPTIONS_PERPAGE) / 2;
			}

			cCardsFrame.Height = Height;
			cCardsFrame.Width = cGraphicsDevice.Viewport.Width - Width;
			Repositioned();

		}

		protected override void Repositioned() {
			cCardsFrame.Top = Top;
			cCardsFrame.Left = Left + Width;
		}

		protected override void VisibleChanged() {
			cCardsFrame.Visible = Visible;
		}

		private void DeckChoiceClickHandler(object Sender, MouseButton Button) {
			Button CurrChoice = (Button)Sender;

			foreach (Button Choice in cDeckChoiceCtrlList.Values) {
				Choice.BackgroundColor = new Color(0.8f, 0.608f, 0.398f, 1.0f);
			}

			CurrChoice.BackgroundColor = new Color(0.57f, 0.169f, 0.129f, 1.0f);
			HasChanged = true;
		}
	}

	class OverlordCardsFrame : Container {
		public OverlordCardsFrame(GraphicsDevice GraphDev, int Height, int Width)
			: base(GraphDev, Height, Width) {
		}
	}

	public struct OverlordCard {
		//Filter points
		public string Class;
		public string Set;
		//Other values
		public Texture2D Image;
		public int Count;
		public int Include;
	}
}

