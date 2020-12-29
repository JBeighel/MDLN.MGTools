using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

namespace MDLN.CardTable {
	class Launcher {
		public static void Main(string[] args) {
			using (var game = new CardTable())
			{
				game.Run();
			}
		}
	}

	public class CardTable : Game {
		private const float BUTTONWIDTHPERCENT = 0.20f;
		private const string INTERFACECONTENTDIR = "Content";

		private const string DECKXMLFILE = "Content\\deck.xml";

		private GraphicsDeviceManager cGraphDevMgr;
		private SpriteBatch cDrawBatch;
		private Dictionary<Textures, Texture2D> cTextureDict;
		private TextureFont cFont;
		private FullCardPanel cFullCardFrame;
		private GameConsole cDevConsole;
		private Dictionary<MenuButtons, Button> cMenuBtns;
		private List<CardInfo> cMonsterDeck;
		private List<CardMini> cCardsInPlay;

		public CardTable() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			IsMouseVisible = true;
			cTextureDict = new Dictionary<Textures, Texture2D>();
			cMenuBtns = new Dictionary<MenuButtons, Button>();
			cMonsterDeck = new List<CardInfo>();
			cCardsInPlay = new List<CardMini>();
		}

		protected override void Initialize() {
			cDrawBatch = new SpriteBatch(cGraphDevMgr.GraphicsDevice);

			//Initializes monogame
			base.Initialize();
		}

		protected override void UnloadContent() {
			
		}

		protected override void LoadContent() {
			Rectangle ButtonArea = new Rectangle();
			int MiniCardHeight, MiniCardWidth;

			Content.RootDirectory = INTERFACECONTENTDIR;

			foreach (Textures CurrTexture in Enum.GetValues(typeof(Textures))) {
				cTextureDict.Add(CurrTexture, Content.Load<Texture2D>(Tools.EnumTools.GetEnumDescriptionAttribute(CurrTexture)));
			}

			cFont = new TextureFont(cTextureDict[Textures.Font]);

			ButtonArea.Width = (int)(cGraphDevMgr.GraphicsDevice.Viewport.Width * BUTTONWIDTHPERCENT);
			ButtonArea.Height = (cGraphDevMgr.GraphicsDevice.Viewport.Height - Enum.GetValues(typeof(MenuButtons)).Length) / Enum.GetValues(typeof(MenuButtons)).Length;
			ButtonArea.X = cGraphDevMgr.GraphicsDevice.Viewport.Width - ButtonArea.Width;
			ButtonArea.Y = 0;
			foreach (MenuButtons CurrBtn in Enum.GetValues(typeof(MenuButtons))) {
				cMenuBtns.Add(CurrBtn, new Button(cGraphDevMgr.GraphicsDevice, null, ButtonArea.Y, ButtonArea.X, ButtonArea.Height, ButtonArea.Width));
				cMenuBtns[CurrBtn].Text = Tools.EnumTools.GetEnumDescriptionAttribute(CurrBtn);
				cMenuBtns[CurrBtn].Font = cFont;
				cMenuBtns[CurrBtn].Visible = true;
				cMenuBtns[CurrBtn].BackgroundColor = Color.Navy;
				cMenuBtns[CurrBtn].FontColor = Color.AntiqueWhite;

				ButtonArea.Y += ButtonArea.Height + 1;
			}

			cMenuBtns[MenuButtons.Menu].Click += new ClickEvent(MenuClick);
			cMenuBtns[MenuButtons.OpenDoor].Click += new ClickEvent(OpenDoorClick);
			cMenuBtns[MenuButtons.DrawMonster].Click += new ClickEvent(SpawnMonsterClick);
			cMenuBtns[MenuButtons.Abilities].Click += new ClickEvent(AbilitiesClick);
			cMenuBtns[MenuButtons.Treasure].Click += new ClickEvent(TreasureClick);

			cFullCardFrame = new FullCardPanel(cGraphDevMgr.GraphicsDevice, cGraphDevMgr.GraphicsDevice.Viewport.Height, (int)(cGraphDevMgr.GraphicsDevice.Viewport.Width / 3));
			cFullCardFrame.BackgroundColor = Color.DarkViolet;
			cFullCardFrame.Font = cFont;

			cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, "Font.png", cGraphDevMgr.GraphicsDevice.Viewport.Width, cGraphDevMgr.GraphicsDevice.Viewport.Height / 2);
			cDevConsole.AccessKey = Keys.OemTilde;
			cDevConsole.UseAccessKey = true;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;
			cDevConsole.CommandSent += new CommandSentEventHandler(CommandEvent);

			cDevConsole.AddText(String.Format("Viewport Height={0} Width={1}", cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width));

			MiniCardHeight = (int)(((cGraphDevMgr.GraphicsDevice.Viewport.Height - 10) / 3) - 10);
			MiniCardWidth = (int)(200f * ((float)MiniCardHeight / 175f));

			for (int Ctr = 0; Ctr < 3; Ctr++) {
				cCardsInPlay.Add(new CardMini(cGraphDevMgr.GraphicsDevice, MiniCardHeight, MiniCardWidth));
				cCardsInPlay[Ctr].Font = cFont;
				cCardsInPlay[Ctr].FontColor = Color.DarkSlateGray;
				cCardsInPlay[Ctr].Visible = true;
				cCardsInPlay[Ctr].BackgroundColor = new Color(0, 0, 0, 0);
				cCardsInPlay[Ctr].Top = 3;
				cCardsInPlay[Ctr].Left = 3 + (Ctr * (MiniCardWidth + 3));
				cCardsInPlay[Ctr].Click += new ClickEvent(CardInPlayClick);
			}

			LoadMonsterDeck(DECKXMLFILE);
		}

		protected override void Update(GameTime gameTime) {
			foreach (KeyValuePair<MenuButtons, Button> KeyPair in cMenuBtns) {
				KeyPair.Value.Update(gameTime);
			}

			cCardsInPlay[0].Card = cMonsterDeck[0];
			cCardsInPlay[1].Card = cMonsterDeck[1];
			cCardsInPlay[2].Card = cMonsterDeck[2];

			foreach (CardMini CurrCard in cCardsInPlay) {
				CurrCard.Update(gameTime);
			}

			cFullCardFrame.Update(gameTime);
			cDevConsole.Update(gameTime);

			//Use monogame update
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			cGraphDevMgr.GraphicsDevice.Clear(Color.Black);

			foreach (KeyValuePair<MenuButtons, Button> KeyPair in cMenuBtns) {
				KeyPair.Value.Draw();
			}

			cDrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			cDrawBatch.End();

			foreach (CardMini CurrCard in cCardsInPlay) {
				CurrCard.Draw();
			}

			cFullCardFrame.Draw();
			cDevConsole.Draw();

			//Use monogame draw
			base.Draw(gameTime);
		}

		private void CommandEvent(object Sender, string CommandEvent) {
			if (RegEx.LooseTest(CommandEvent, @"^\s*(quit|exit|close)\s*$") == true) {
				Exit();
			} else if (RegEx.LooseTest(CommandEvent, @"^\s*card\s*left\s*$") == true) {
				if (cFullCardFrame.Visible == false) {
					cDevConsole.AddText("Opening Full Card panel from left");
					cFullCardFrame.Card = cMonsterDeck[0];
					cFullCardFrame.Left = 0;
					cFullCardFrame.OpenEffect = DisplayEffect.SlideRight;
					cFullCardFrame.CloseEffect = DisplayEffect.SlideLeft;
					cFullCardFrame.Visible = true;
				} else {
					cDevConsole.AddText("Closing Full Card panel");
					cFullCardFrame.Visible = false;
				}
			} else if (RegEx.LooseTest(CommandEvent, @"^\s*card\s*right\s*$") == true) {
				if (cFullCardFrame.Visible == false) {
					cDevConsole.AddText("Opening Full Card panel from right");
					cFullCardFrame.Card = cMonsterDeck[1];
					cFullCardFrame.Left = cGraphDevMgr.GraphicsDevice.Viewport.Width - cFullCardFrame.Width - cMenuBtns[MenuButtons.Menu].Width;
					cFullCardFrame.OpenEffect = DisplayEffect.SlideLeft;
					cFullCardFrame.CloseEffect = DisplayEffect.SlideRight;
					cFullCardFrame.Visible = true;
				} else {
					cDevConsole.AddText("Closing Full Card panel");
					cFullCardFrame.Visible = false;
				}
			} else if (RegEx.LooseTest(CommandEvent, @"^\s*card\s*close\s*$") == true) {
				cDevConsole.AddText("Closing Full Card panel");
				cFullCardFrame.Visible = false;
			} else {
				cDevConsole.AddText("Unrecognized command: " + CommandEvent);
			}
		}

		private void MenuClick(object Sender, MouseButton Button) {
			if (Button == MouseButton.Left) {
				cDevConsole.AddText("Menu button clicked");
			}
		}

		private void OpenDoorClick(object Sender, MouseButton Button) {
			if (Button == MouseButton.Left) {
				cDevConsole.AddText("Open Door button clicked");
			}
		}

		private void SpawnMonsterClick(object Sender, MouseButton Button) {
			if (Button == MouseButton.Left) {
				cDevConsole.AddText("Spawn Monster button clicked");
			}
		}

		private void AbilitiesClick(object Sender, MouseButton Button) {
			if (Button == MouseButton.Left) {
				cDevConsole.AddText("Abilities button clicked");
			}
		}

		private void TreasureClick(object Sender, MouseButton Button) {
			if (Button == MouseButton.Left) {
				cDevConsole.AddText("Treasure button clicked");
			}
		}

		private void LoadMonsterDeck(string DeckXMLFile) {
			XmlDocument DeckXML;
			XmlNodeList CardNodeList;
			CardInfo NewCard;

			cDevConsole.AddText("Loading monster cards from " + DeckXMLFile);

			DeckXML = new XmlDocument();
			DeckXML.Load(DeckXMLFile);

			CardNodeList = DeckXML.DocumentElement.SelectNodes("//monster");
			foreach (XmlNode CardNode in CardNodeList) {
				NewCard = new CardInfo(CardType.Monster);
				NewCard.Background = cTextureDict[Textures.CardBack];
				NewCard.HeartIcon = cTextureDict[Textures.HeartIcon];
				NewCard.SwordIcon = cTextureDict[Textures.SwordIcon];

				foreach (XmlNode Tag in CardNode.ChildNodes) {
					switch (Tag.Name) {
						case "title":
							NewCard.Title = Tag.InnerText;
							break;
						case "image":
							NewCard.Image = Content.Load<Texture2D>(Tag.InnerText);
							break;
						case "attack":
							NewCard.AttackStrength = Int32.Parse(Tag.InnerText);
							break;
						case "health":
							NewCard.CurrentHealth = Int32.Parse(Tag.InnerText);
							NewCard.MaxHealth = NewCard.CurrentHealth;
							break;
						case "description":
							NewCard.Description = Tag.InnerText.Replace("\t", "").Replace("\r", "");
							break;
						default:
							if (Tag.Name.CompareTo("#comment") == 0) {
								cDevConsole.AddText("Found text '" + Tag.InnerText + "' outisde any tag in a 'monster' tag.");
							} else {
								cDevConsole.AddText("Unrecognized tag '" + Tag.Name + "' inside 'monster' tag.");
							}
							break;
					}
				}

				cMonsterDeck.Add(NewCard);
			}

			cDevConsole.AddText("Loaded " + cMonsterDeck.Count + " cards.");
		}

		private void CardInPlayClick(object Sender, MouseButton Button) {
			CardMini CurrCard = (CardMini)Sender;
			int MidPoint;

			if (Button == MouseButton.Left) {
				cFullCardFrame.Card = CurrCard.Card;
				MidPoint = (cGraphDevMgr.GraphicsDevice.Viewport.Width - cMenuBtns[MenuButtons.Menu].Width) / 2;

				if (CurrCard.Left > MidPoint) { //Card is on left half
					cFullCardFrame.Left = 0;
					cFullCardFrame.OpenEffect = DisplayEffect.SlideRight;
					cFullCardFrame.CloseEffect = DisplayEffect.SlideLeft;
				} else {
					cFullCardFrame.Left = cGraphDevMgr.GraphicsDevice.Viewport.Width - cFullCardFrame.Width - cMenuBtns[MenuButtons.Menu].Width;
					cFullCardFrame.OpenEffect = DisplayEffect.SlideLeft;
					cFullCardFrame.CloseEffect = DisplayEffect.SlideRight;
				}

				cFullCardFrame.Visible = true;
			}
		}

		protected enum Textures {
			[Description("Font.png")]
			Font,
			[Description("Heart.png")]
			HeartIcon,
			[Description("Sword.png")]
			SwordIcon,
			[Description("CardBase.png")]
			CardBack
		}

		protected enum MenuButtons {
			[Description("Menu")]
			Menu,
			[Description("Open Door")]
			OpenDoor,
			[Description("Draw\nMonster")]
			DrawMonster,
			[Description("Treasure")]
			Treasure,
			[Description("Abilities")]
			Abilities
		}
	}
}
