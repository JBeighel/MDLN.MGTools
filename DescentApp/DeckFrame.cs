using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

namespace DescentApp {
	class DeckFrame : Container {
		private const float DEFAULT_CARDRATIO = 0.714f;
		private const float DEFAULT_MARGINPERCENT = 0.02f;
		private const float DEFAULT_CARDWIDTHPERCENT = 0.40f;
		private const float DEFAULT_FONTPERCENT = 0.07f;
		private const int DEFAULT_ZOOMTIME = 750;

		/// <summary>
		/// Ratio of the card with / card height
		/// Used to scale the cards
		/// </summary>
		public float CardRatio { get; set; }

		/// <summary>
		/// Percent of container size that should be used in margins
		/// </summary>
		public float MarginPercent { get; set; }

		/// <summary>
		/// Percent of the container width that should be used for each card
		/// </summary>
		public float CardWidthPercent { get; set; }

		/// <summary>
		/// Amount of time in milliseconds that the zoom animation should take
		/// </summary>
		public int ZoomDuration { get; set; }

		/// <summary>
		/// List of textures to use as card backs
		/// </summary>
		public List<Texture2D> CardBackList;

		/// <summary>
		/// List of textures to use as card faces
		/// </summary>
		public List<Texture2D> CardFaceList;

		/// <summary>
		/// True if the selected card is zoomed in, false for zoomed out
		/// </summary>
		public bool SelectedCardIsZoomed;

		/// <summary>
		/// Index of the currently selected card.  Negative number means no selected card
		/// </summary>
		public int SelectedCardIndex;

		/// <summary>
		/// Index of the card back selected to display.  Negative number means none chosen
		/// </summary>
		public int SelectedBackIndex {
			get {
				return cSelectedBackIndex;
			}

			set {
				if (value >= CardBackList.Count) {
					throw new IndexOutOfRangeException("Index exceeds number of card backs loaded.");
				}

				cSelectedBackIndex = value;

				if (cSelectedBackIndex > 0) {
					cCardBackCtrl.Background = CardBackList[cSelectedBackIndex];
				}
				cCardBackCtrl.Text = "";
			}
		}

		/// <summary>
		/// Maximum number of cards that can be drawn and shown
		/// Additional cards drawn will begin to replace the currently draw ones
		/// Set to -1 for no draw limit
		/// </summary>
		public int MaxCardsShown { get; set; }

		public TextureFont Font {
			get {
				return cFont;
			}

			set {
				cFont = value;

				cCardBackCtrl.Font = Font;
				
				foreach (Button ShownCard in cCardsShownCtrls) {
					ShownCard.Font = Font;
				}
			}
		}

		private Random cRandNumber;
		private List<int> cDeckShuffleList;
		private int cDeckShuffleIndex;
		private Button cCardBackCtrl;
		private List<Button> cCardsShownCtrls;
		private List<int> cCardsShownIndexes;
		private int cSelectedBackIndex;
		private TextureFont cFont;
		
		public DeckFrame(GraphicsDevice GraphDev, int Height, int Width) 
			: base(GraphDev, null, 0, 0, Height, Width) {
			Rectangle CardRegion = new Rectangle();

			CardRegion.X = (int)(ClientRegion.Width * MarginPercent);
			CardRegion.Y = (int)(ClientRegion.Height * MarginPercent);
			CardRegion.Width = (int)(ClientRegion.Width * CardWidthPercent);
			CardRegion.Height = (int)(CardRegion.Width / CardRatio);

			cCardBackCtrl = new Button(GraphDev, null, CardRegion.Y, CardRegion.X, CardRegion.Height, CardRegion.Width);
			cCardBackCtrl.BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
			cCardBackCtrl.Click += CardBackClickHandler;
			cCardBackCtrl.Visible = true;
			cCardBackCtrl.FontColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
			cCardBackCtrl.Text = "No Cards";
			cCardBackCtrl.Alignment = Justify.MiddleCenter;

			CardBackList = new List<Texture2D>();
			CardFaceList = new List<Texture2D>();
			cCardsShownIndexes = new List<int>();
			cCardsShownCtrls = new List<Button>();

			cDeckShuffleList = new List<int>();

			cRandNumber = new Random();

			MaxCardsShown = -1;
			SelectedBackIndex = -1;
			SelectedCardIndex = -1;
			SelectedCardIsZoomed = false;
			cDeckShuffleIndex = -1;

			CardRatio = DEFAULT_CARDRATIO;
			MarginPercent = DEFAULT_MARGINPERCENT;
			CardWidthPercent = DEFAULT_CARDWIDTHPERCENT;
			ZoomDuration = DEFAULT_ZOOMTIME;

			BackgroundColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);
		}

		public void ShuffleCompleteDeck(bool ClearShownCards, bool OmitShownCardsFromDeck) {
			List<int> CardList = new List<int>();
			int Index;

			for (Index = 0; Index < CardFaceList.Count; Index++) {
				if ((OmitShownCardsFromDeck == false) || (cCardsShownIndexes.Contains(Index) == false)) {
					CardList.Add(Index);
				}
			}

			while (CardList.Count > 0) {
				Index = cRandNumber.Next(0, CardList.Count);

				cDeckShuffleList.Add(CardList[Index]);
				
				CardList.RemoveAt(Index);
			}

			if (cDeckShuffleList.Count == 0) {//No cards in deck, set index to invalid
				cDeckShuffleIndex = -1;
			} else { //Deck contains cards, put index at beginning
				cDeckShuffleIndex = 0;
			}

			if (ClearShownCards == true) {
				cCardsShownCtrls.Clear();
				HasChanged = true;
			}
		}

		protected override void DrawContents(GameTime CurrTime) {
			cCardBackCtrl.Draw(cDrawBatch);

			foreach (Button ShownCard in cCardsShownCtrls) {
				ShownCard.Draw(cDrawBatch);
			}
		}

		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
			int MarginVert, MarginHoriz, CardHeight, CardWidth, FontHeight;
			Rectangle CurrCardRegion;

			MarginVert = (int)(ClientRegion.Width * MarginPercent);
			MarginHoriz = (int)(ClientRegion.Height * MarginPercent);
			CardWidth = (int)(ClientRegion.Width * CardWidthPercent);
			CardHeight = (int)(CardWidth / CardRatio);
			FontHeight = (int)(CardHeight * DEFAULT_FONTPERCENT);

			//Update the card back, draw deck, control
			if (cCardBackCtrl.Top != MarginVert) {
				cCardBackCtrl.Top = MarginVert;
				cCardBackCtrl.Left = MarginHoriz;
				HasChanged = true;
			}

			if (cCardBackCtrl.Width != CardWidth) {
				cCardBackCtrl.Width = CardWidth;
				cCardBackCtrl.Height = CardHeight;
				HasChanged = true;
			}

			if (cCardBackCtrl.Font.CharacterHeight != FontHeight) {
				cCardBackCtrl.FontSize = FontHeight;
			}

			cCardBackCtrl.Update(CurrTime, CurrKeyboard, CurrMouse);

			//Update all of the face card, shown cards, controls
			
		}
		
		private void CardBackClickHandler(object Sender, MouseButton Button) {
			if ((cDeckShuffleIndex >= cDeckShuffleList.Count) || (cDeckShuffleIndex < 0)) {
				ShuffleCompleteDeck(false, true);
			} else {
				cCardsShownCtrls.Add(new MDLN.MGTools.Button(cGraphicsDevice, CardFaceList[cDeckShuffleList[cDeckShuffleIndex]], 0, 0, 100, 100));
				cCardsShownCtrls[cCardsShownCtrls.Count - 1].Visible = true;
				cCardsShownIndexes.Add(cDeckShuffleList[cDeckShuffleIndex]);
				cDeckShuffleIndex += 1;
			}

			if (cDeckShuffleIndex >= cDeckShuffleList.Count) {
				cCardBackCtrl.Text = "Shuffle";
			} else if (cDeckShuffleIndex == -1) {
				cCardBackCtrl.Text = "No Cards";
			} else {
				cCardBackCtrl.Text = "";
			}

			HasChanged = true;
		}
	}
}
