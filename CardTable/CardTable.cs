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
		private GameConsole cDevConsole;
		private Dictionary<MenuButtons, Button> cMenuBtns;
		private List<CardInfo> cMonsterDeck;

		public CardTable() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			IsMouseVisible = true;
			cTextureDict = new Dictionary<Textures, Texture2D>();
			cMenuBtns = new Dictionary<MenuButtons, Button>();
			cMonsterDeck = new List<CardInfo>();
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

			Content.RootDirectory = INTERFACECONTENTDIR;

			foreach (Textures CurrTexture in Enum.GetValues(typeof(Textures))) {
				cTextureDict.Add(CurrTexture, Content.Load<Texture2D>(Tools.Tools.GetEnumDescriptionAttribute(CurrTexture)));
			}

			cFont = new TextureFont(cTextureDict[Textures.Font]);

			ButtonArea.Width = (int)(cGraphDevMgr.GraphicsDevice.Viewport.Width * BUTTONWIDTHPERCENT);
			ButtonArea.Height = (cGraphDevMgr.GraphicsDevice.Viewport.Height - Enum.GetValues(typeof(MenuButtons)).Length) / Enum.GetValues(typeof(MenuButtons)).Length;
			ButtonArea.X = cGraphDevMgr.GraphicsDevice.Viewport.Width - ButtonArea.Width;
			ButtonArea.Y = 0;
			foreach (MenuButtons CurrBtn in Enum.GetValues(typeof(MenuButtons))) {
				cMenuBtns.Add(CurrBtn, new Button(cGraphDevMgr.GraphicsDevice, null, ButtonArea.Y, ButtonArea.X, ButtonArea.Height, ButtonArea.Width));
				cMenuBtns[CurrBtn].Text = Tools.Tools.GetEnumDescriptionAttribute(CurrBtn);
				cMenuBtns[CurrBtn].Font = cFont;
				cMenuBtns[CurrBtn].Visible = true;
				cMenuBtns[CurrBtn].BackgroundColor = Color.Navy;
				cMenuBtns[CurrBtn].FontColor = Color.AntiqueWhite;

				ButtonArea.Y += ButtonArea.Height + 1;
			}

			cMenuBtns[MenuButtons.Menu].Click += new ButtonClickEvent(MenuClick);
			cMenuBtns[MenuButtons.OpenDoor].Click += new ButtonClickEvent(OpenDoorClick);
			cMenuBtns[MenuButtons.DrawMonster].Click += new ButtonClickEvent(SpawnMonsterClick);
			cMenuBtns[MenuButtons.Abilities].Click += new ButtonClickEvent(AbilitiesClick);
			cMenuBtns[MenuButtons.Treasure].Click += new ButtonClickEvent(TreasureClick);

			cDevConsole = new GameConsole(cGraphDevMgr.GraphicsDevice, Content, "Font.png", cGraphDevMgr.GraphicsDevice.Viewport.Width, cGraphDevMgr.GraphicsDevice.Viewport.Height / 2);
			cDevConsole.AccessKey = Keys.OemTilde;
			cDevConsole.UseAccessKey = true;
			cDevConsole.OpenEffect = DisplayEffect.SlideDown;
			cDevConsole.CloseEffect = DisplayEffect.SlideUp;
			cDevConsole.CommandSent += new CommandSentEventHandler(CommandEvent);

			cDevConsole.AddText(String.Format("Viewport Height={0} Width={1}", cGraphDevMgr.GraphicsDevice.Viewport.Height, cGraphDevMgr.GraphicsDevice.Viewport.Width));

			LoadMonsterDeck(DECKXMLFILE);
		}

		protected override void Update(GameTime gameTime) {
			foreach (KeyValuePair<MenuButtons, Button> KeyPair in cMenuBtns) {
				KeyPair.Value.Update(gameTime);
			}

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

			Rectangle FullCardRegion, MiniCardRegion;
			FullCardRegion.X = 10;
			FullCardRegion.Y = 10;
			FullCardRegion.Width = 250;
			FullCardRegion.Height = 350;
			cMonsterDeck[0].RenderFullCard(FullCardRegion, Color.White, Color.DarkSlateGray, cDrawBatch, cFont);

			MiniCardRegion.X = 270;
			MiniCardRegion.Y = 10;
			MiniCardRegion.Height = (int)(((cGraphDevMgr.GraphicsDevice.Viewport.Height - 10) / 3) - 10);
			MiniCardRegion.Width = (int)(200f * ((float)MiniCardRegion.Height / 175f));
			cMonsterDeck[0].RenderMiniCard(MiniCardRegion, Color.White, Color.DarkSlateGray, cDrawBatch, cFont);

			MiniCardRegion.Y += MiniCardRegion.Height + 10;
			cMonsterDeck[1].RenderMiniCard(MiniCardRegion, Color.White, Color.DarkSlateGray, cDrawBatch, cFont);

			MiniCardRegion.Y += MiniCardRegion.Height + 10;
			cMonsterDeck[2].RenderMiniCard(MiniCardRegion, Color.White, Color.DarkSlateGray, cDrawBatch, cFont);

			cDrawBatch.End();

			cDevConsole.Draw();

			//Use monogame draw
			base.Draw(gameTime);
		}

		private void CommandEvent(object Sender, string CommandEvent) {
			if (RegEx.LooseTest(CommandEvent, @"^\s*(quit|exit|close)\s*$") == true) {
				Exit();
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
