using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CacheMode = Godot.ResourceLoader.CacheMode;
using InternalMode = Godot.Node.InternalMode;
using MouseFilterEnum = Godot.Control.MouseFilterEnum;
using SizeFlags = Godot.Control.SizeFlags;
using Range = Godot.Range;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;

namespace STS2GiftingMod;

internal static class GiftPanel
{
	private static CanvasLayer? _layer;

	private static Control? _layerRoot;

	private static Button? _openButton;

	private static ColorRect? _backdrop;

	private static CenterContainer? _center;

	private static RunState? _state;

	private static bool _attached;

	private static OptionButton? _recipientDrop;

	private static TabContainer? _tabs;

	private static SpinBox? _goldSpin;

	private static ItemList? _cardList;

	private static ItemList? _relicList;

	private static ItemList? _potionList;

	private static Label? _feeLabel;

	private static Label? _statusLabel;

	private static Button? _sendButton;

	private static Godot.Timer? _refreshTimer;

	private static Font? _fontBold;

	private static Font? _fontRegular;

	private static Theme? _baseTheme;

	private static readonly List<Player> _recipients = new List<Player>();

	private static readonly List<CardModel> _cards = new List<CardModel>();

	private static readonly List<RelicModel> _relics = new List<RelicModel>();

	private static readonly List<PotionModel> _potions = new List<PotionModel>();

	public static void Attach(NRun run, RunState state)
	{
		if (_attached && _layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			return;
		}
		_state = state;
		_attached = true;
		try
		{
			_fontBold = ResourceLoader.Load<Font>("res://themes/kreon_bold_shared.tres", (string)null, (CacheMode)1);
			_fontRegular = ResourceLoader.Load<Font>("res://themes/kreon_regular_shared.tres", (string)null, (CacheMode)1);
			_baseTheme = ResourceLoader.Load<Theme>("res://themes/main_menu_text_button.tres", (string)null, (CacheMode)1);
		}
		catch
		{
		}
		Control val = new Control();
		val.MouseFilter = (MouseFilterEnum)2;
		val.AnchorRight = 1f;
		val.AnchorBottom = 1f;
		if (run.GlobalUi != null)
		{
			((Node)run.GlobalUi).AddChild((Node)(object)val, false, (InternalMode)0);
			_layer = new CanvasLayer
			{
				Layer = 10
			};
			((Node)val).AddChild((Node)(object)_layer, false, (InternalMode)0);
			_layerRoot = new Control();
			_layerRoot.AnchorRight = 1f;
			_layerRoot.AnchorBottom = 1f;
			_layerRoot.MouseFilter = (MouseFilterEnum)2;
			if (_baseTheme != null)
			{
				_layerRoot.Theme = _baseTheme;
			}
			((Node)_layer).AddChild((Node)(object)_layerRoot, false, (InternalMode)0);
			BuildOpenButton();
			BuildPanel();
			Godot.Timer val2 = new();
			val2.WaitTime = 0.2;
			val2.Autostart = true;
			val2.Timeout += UpdateButtonVisibility;
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			_refreshTimer = new Godot.Timer();
			_refreshTimer.WaitTime = 0.5;
			_refreshTimer.Autostart = false;
			_refreshTimer.Timeout += OnRefreshTick;
			((Node)_layerRoot).AddChild((Node)(object)_refreshTimer, false, (InternalMode)0);
		}
	}

	private static bool IsOnMapScreen()
	{
		if (CombatManager.Instance != null && CombatManager.Instance.IsInProgress)
		{
			return false;
		}
		NMapScreen instance = NMapScreen.Instance;
		if (instance == null || !instance.IsOpen)
		{
			return false;
		}
		NRun instance2 = NRun.Instance;
		object obj;
		if (instance2 == null)
		{
			obj = null;
		}
		else
		{
			NGlobalUi globalUi = instance2.GlobalUi;
			obj = ((globalUi != null) ? globalUi.TopBar : null);
		}
		NTopBar val = (NTopBar)obj;
		if (val != null && ((Control)val).Position.Y < -10f)
		{
			return false;
		}
		return true;
	}

	private static void UpdateButtonVisibility()
	{
		if (_openButton != null)
		{
			bool flag = IsOnMapScreen();
			((CanvasItem)_openButton).Visible = flag;
			if (!flag && _center != null && ((CanvasItem)_center).Visible)
			{
				ClosePanel();
			}
		}
	}

	private static void OnRefreshTick()
	{
		if (_center != null && ((CanvasItem)_center).Visible)
		{
			Refresh();
		}
	}

	private static void BuildOpenButton()
	{
		_openButton = new Button
		{
			Text = "Gift Menu",
			MouseFilter = (MouseFilterEnum)0,
			CustomMinimumSize = new Vector2(120f, 40f)
		};
		StyleBtn(_openButton);
		((Control)_openButton).AnchorLeft = 1f;
		((Control)_openButton).AnchorRight = 1f;
		((Control)_openButton).AnchorTop = 0f;
		((Control)_openButton).AnchorBottom = 0f;
		((Control)_openButton).OffsetLeft = -620f;
		((Control)_openButton).OffsetRight = -490f;
		((Control)_openButton).OffsetTop = 16f;
		((Control)_openButton).OffsetBottom = 56f;
		((BaseButton)_openButton).Pressed += Toggle;
		((Node)_layerRoot).AddChild((Node)(object)_openButton, false, (InternalMode)0);
	}

	private static void BuildPanel()
	{
		_backdrop = new ColorRect
		{
			Color = new Color(0f, 0f, 0f, 0.55f),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0
		};
		((Control)_backdrop).AnchorRight = 1f;
		((Control)_backdrop).AnchorBottom = 1f;
		_backdrop.GuiInput += ev =>
		{
			if (ev is InputEventMouseButton { Pressed: true })
			{
				ClosePanel();
			}
		};
		((Node)_layerRoot).AddChild((Node)(object)_backdrop, false, (InternalMode)0);
		_center = new CenterContainer
		{
			Visible = false,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)_center).AnchorRight = 1f;
		((Control)_center).AnchorBottom = 1f;
		((Node)_layerRoot).AddChild((Node)(object)_center, false, (InternalMode)0);
		PanelContainer val2 = new PanelContainer
		{
			MouseFilter = (MouseFilterEnum)0,
			CustomMinimumSize = new Vector2(880f, 640f)
		};
		StylePanel(val2);
		((Node)_center).AddChild((Node)(object)val2, false, (InternalMode)0);
		MarginContainer val3 = new MarginContainer();
		((Control)val3).AddThemeConstantOverride("margin_left", 24);
		((Control)val3).AddThemeConstantOverride("margin_top", 24);
		((Control)val3).AddThemeConstantOverride("margin_right", 24);
		((Control)val3).AddThemeConstantOverride("margin_bottom", 24);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		VBoxContainer val4 = new VBoxContainer();
		((Control)val4).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val4).SizeFlagsVertical = (SizeFlags)3;
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		HBoxContainer val5 = new HBoxContainer();
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		Label val6 = new Label
		{
			Text = "Send a gift",
			SizeFlagsHorizontal = (SizeFlags)3
		};
		StyleLbl(val6, 26);
		((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
		Button val7 = new Button
		{
			Text = "Close"
		};
		StyleBtn(val7);
		((BaseButton)val7).Pressed += ClosePanel;
		((Node)val5).AddChild((Node)(object)val7, false, (InternalMode)0);
		HBoxContainer val8 = new HBoxContainer();
		((Node)val4).AddChild((Node)(object)val8, false, (InternalMode)0);
		Label val9 = new Label
		{
			Text = "To: "
		};
		StyleLbl(val9);
		((Node)val8).AddChild((Node)(object)val9, false, (InternalMode)0);
		_recipientDrop = new OptionButton
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Node)val8).AddChild((Node)(object)_recipientDrop, false, (InternalMode)0);
		_tabs = new TabContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		((Node)val4).AddChild((Node)(object)_tabs, false, (InternalMode)0);
		MarginContainer val10 = new MarginContainer();
		((Control)val10).AddThemeConstantOverride("margin_top", 12);
		VBoxContainer val11 = new VBoxContainer();
		Label val12 = new Label
		{
			Text = "Send gold to another player."
		};
		StyleLbl(val12, 18);
		((Node)val11).AddChild((Node)(object)val12, false, (InternalMode)0);
		_goldSpin = new SpinBox
		{
			MinValue = 1.0,
			MaxValue = 9999.0,
			Step = 1.0,
			Value = 25.0
		};
		_goldSpin.ValueChanged += _ => UpdateFee();
		LineEdit lineEdit = _goldSpin.GetLineEdit();
		lineEdit.TextChanged += _ => UpdateFee();
		((Node)val11).AddChild((Node)(object)_goldSpin, false, (InternalMode)0);
		((Node)val10).AddChild((Node)(object)val11, false, (InternalMode)0);
		((Node)_tabs).AddChild((Node)(object)val10, false, (InternalMode)0);
		_tabs.SetTabTitle(0, "Gold");
		MarginContainer val15 = new MarginContainer();
		((Control)val15).AddThemeConstantOverride("margin_top", 12);
		VBoxContainer val16 = new VBoxContainer();
		Label val17 = new Label
		{
			Text = "Send a card from your deck."
		};
		StyleLbl(val17, 18);
		((Node)val16).AddChild((Node)(object)val17, false, (InternalMode)0);
		_cardList = new ItemList
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		_cardList.ItemSelected += _ => UpdateFee();
		((Node)val16).AddChild((Node)(object)_cardList, false, (InternalMode)0);
		((Node)val15).AddChild((Node)(object)val16, false, (InternalMode)0);
		((Node)_tabs).AddChild((Node)(object)val15, false, (InternalMode)0);
		_tabs.SetTabTitle(1, "Cards");
		MarginContainer val19 = new MarginContainer();
		((Control)val19).AddThemeConstantOverride("margin_top", 12);
		VBoxContainer val20 = new VBoxContainer();
		Label val21 = new Label
		{
			Text = "Send a relic to another player. Certain relics are unable to be gifted."
		};
		StyleLbl(val21, 18);
		((Node)val20).AddChild((Node)(object)val21, false, (InternalMode)0);
		_relicList = new ItemList
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		_relicList.ItemSelected += _ => UpdateFee();
		((Node)val20).AddChild((Node)(object)_relicList, false, (InternalMode)0);
		((Node)val19).AddChild((Node)(object)val20, false, (InternalMode)0);
		((Node)_tabs).AddChild((Node)(object)val19, false, (InternalMode)0);
		_tabs.SetTabTitle(2, "Relics");
		MarginContainer val23 = new MarginContainer();
		((Control)val23).AddThemeConstantOverride("margin_top", 12);
		VBoxContainer val24 = new VBoxContainer();
		Label val25 = new Label
		{
			Text = "Send a potion (recipient needs an open slot)."
		};
		StyleLbl(val25, 18);
		((Node)val24).AddChild((Node)(object)val25, false, (InternalMode)0);
		_potionList = new ItemList
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		_potionList.ItemSelected += _ => UpdateFee();
		((Node)val24).AddChild((Node)(object)_potionList, false, (InternalMode)0);
		((Node)val23).AddChild((Node)(object)val24, false, (InternalMode)0);
		((Node)_tabs).AddChild((Node)(object)val23, false, (InternalMode)0);
		_tabs.SetTabTitle(3, "Potions");
		_tabs.TabChanged += _ => UpdateFee();
		_feeLabel = new Label
		{
			Text = ""
		};
		StyleLbl(_feeLabel, 19);
		((Node)val4).AddChild((Node)(object)_feeLabel, false, (InternalMode)0);
		_statusLabel = new Label
		{
			Text = ""
		};
		StyleLbl(_statusLabel, 17);
		((Node)val4).AddChild((Node)(object)_statusLabel, false, (InternalMode)0);
		HBoxContainer val28 = new HBoxContainer();
		((Node)val4).AddChild((Node)(object)val28, false, (InternalMode)0);
		((Node)val28).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		_sendButton = new Button
		{
			Text = "Send"
		};
		StyleBtn(_sendButton);
		((BaseButton)_sendButton).Pressed += OnSend;
		((Node)val28).AddChild((Node)(object)_sendButton, false, (InternalMode)0);
	}

	private static void Toggle()
	{
		if (_center == null)
		{
			return;
		}
		if (((CanvasItem)_center).Visible)
		{
			ClosePanel();
		}
		else if (IsOnMapScreen() && _state != null && _state.Players.Count >= 2)
		{
			((CanvasItem)_backdrop).Visible = true;
			((CanvasItem)_center).Visible = true;
			Refresh();
			Godot.Timer? refreshTimer = _refreshTimer;
			if (refreshTimer != null)
			{
				refreshTimer.Start(-1.0);
			}
		}
	}

	private static void ClosePanel()
	{
		if (_backdrop != null)
		{
			((CanvasItem)_backdrop).Visible = false;
		}
		if (_center != null)
		{
			((CanvasItem)_center).Visible = false;
		}
		Godot.Timer? refreshTimer = _refreshTimer;
		if (refreshTimer != null)
		{
			refreshTimer.Stop();
		}
	}

	private static void Refresh()
	{
		if (_state == null)
		{
			return;
		}
		Player me = LocalContext.GetMe((IPlayerCollection)(object)_state);
		if (me == null)
		{
			return;
		}
		int selected = _recipientDrop.Selected;
		int[] selectedItems = _cardList.GetSelectedItems();
		int[] selectedItems2 = _relicList.GetSelectedItems();
		int[] selectedItems3 = _potionList.GetSelectedItems();
		_recipients.Clear();
		_recipientDrop.Clear();
		foreach (Player otherPlayer in GiftHelper.GetOtherPlayers(me))
		{
			_recipients.Add(otherPlayer);
			_recipientDrop.AddItem(GiftHelper.GetDisplayName(otherPlayer), -1);
		}
		if (selected >= 0 && selected < _recipients.Count)
		{
			_recipientDrop.Select(selected);
		}
		else if (_recipients.Count > 0)
		{
			_recipientDrop.Select(0);
		}
		((BaseButton)_sendButton).Disabled = _recipients.Count == 0;
		((Range)_goldSpin).MaxValue = Math.Max(1, me.Gold);
		_cards.Clear();
		_cardList.Clear();
		foreach (CardModel card in me.Deck.Cards)
		{
			if (card != null && GiftHelper.IsCardGiftable(card))
			{
				_cards.Add(card);
				string title = card.Title;
				_cardList.AddItem(title ?? "", (Texture2D)null, true);
				_cardList.SetItemCustomFgColor(_cardList.ItemCount - 1, GiftHelper.GetCardColor(card));
			}
		}
		if (selectedItems.Length != 0 && selectedItems[0] < _cardList.ItemCount)
		{
			_cardList.Select(selectedItems[0], true);
		}
		_relics.Clear();
		_relicList.Clear();
		foreach (RelicModel relic in me.Relics)
		{
			if (relic != null && GiftHelper.IsRelicGiftable(relic))
			{
				_relics.Add(relic);
				string formattedText = relic.Title.GetFormattedText();
				_relicList.AddItem(formattedText ?? "", (Texture2D)null, true);
				_relicList.SetItemCustomFgColor(_relicList.ItemCount - 1, GiftHelper.GetRelicColor(relic));
			}
		}
		if (selectedItems2.Length != 0 && selectedItems2[0] < _relicList.ItemCount)
		{
			_relicList.Select(selectedItems2[0], true);
		}
		_potions.Clear();
		_potionList.Clear();
		foreach (PotionModel potion in me.Potions)
		{
			if (potion != null && GiftHelper.IsPotionGiftable(potion))
			{
				_potions.Add(potion);
				string formattedText2 = potion.Title.GetFormattedText();
				_potionList.AddItem(formattedText2 ?? "", (Texture2D)null, true);
				_potionList.SetItemCustomFgColor(_potionList.ItemCount - 1, GiftHelper.GetPotionColor(potion));
			}
		}
		if (selectedItems3.Length != 0 && selectedItems3[0] < _potionList.ItemCount)
		{
			_potionList.Select(selectedItems3[0], true);
		}
		UpdateFee();
	}

	private static void UpdateFee()
	{
		if (_tabs == null || _feeLabel == null || _state == null)
		{
			return;
		}
		Player me = LocalContext.GetMe((IPlayerCollection)(object)_state);
		if (me == null)
		{
			_feeLabel.Text = "";
			return;
		}
		switch (_tabs.CurrentTab)
		{
		case 0:
		{
			int value = (int)((Range)_goldSpin).Value;
			if (int.TryParse(_goldSpin.GetLineEdit().Text, out var result) && result > 0)
			{
				value = result;
			}
			_feeLabel.Text = $"You send: {value}g     (You have {me.Gold}g)";
			break;
		}
		case 1:
			_feeLabel.Text = $"You have {me.Gold}g";
			break;
		case 2:
			_feeLabel.Text = "";
			break;
		case 3:
			_feeLabel.Text = "";
			break;
		}
	}

	private static void OnSend()
	{
		if (_state == null || _tabs == null)
		{
			return;
		}
		Player me = LocalContext.GetMe((IPlayerCollection)(object)_state);
		if (me == null)
		{
			return;
		}
		int selected = _recipientDrop.Selected;
		if (selected < 0 || selected >= _recipients.Count)
		{
			Status("Choose a player.");
			return;
		}
		Player val = _recipients[selected];
		int playerIndex = GiftHelper.GetPlayerIndex(val);
		switch (_tabs.CurrentTab)
		{
		case 0:
		{
			int num5 = (int)((Range)_goldSpin).Value;
			int goldFee = GiftHelper.GetGoldFee(num5);
			if (me.Gold < num5 + goldFee)
			{
				Status("Not enough gold.");
				return;
			}
			Enqueue(me, GiftKind.Gold, playerIndex, num5, goldFee);
			Status($"Queued {num5}g for {GiftHelper.GetDisplayName(val)}.");
			break;
		}
		case 1:
		{
			int[] selectedItems2 = _cardList.GetSelectedItems();
			if (selectedItems2.Length == 0)
			{
				Status("Select a card first.");
				return;
			}
			int num3 = selectedItems2[0];
			if (num3 >= _cards.Count)
			{
				return;
			}
			CardModel val3 = _cards[num3];
			int cardFee = GiftHelper.GetCardFee(val3);
			if (me.Gold < cardFee)
			{
				Status("Not enough gold.");
				return;
			}
			int srcIdx = me.Deck.Cards.ToList().IndexOf(val3);
			Enqueue(me, GiftKind.Card, playerIndex, srcIdx, cardFee);
			Status($"Queued {val3.Title} for {GiftHelper.GetDisplayName(val)}.");
			break;
		}
		case 2:
		{
			int[] selectedItems3 = _relicList.GetSelectedItems();
			if (selectedItems3.Length == 0)
			{
				Status("Select a relic first.");
				return;
			}
			int num4 = selectedItems3[0];
			if (num4 >= _relics.Count)
			{
				return;
			}
			RelicModel val4 = _relics[num4];
			int relicFee = GiftHelper.GetRelicFee(val4);
			if (me.Gold < relicFee)
			{
				Status("Not enough gold.");
				return;
			}
			int srcIdx2 = me.Relics.ToList().IndexOf(val4);
			Enqueue(me, GiftKind.Relic, playerIndex, srcIdx2, relicFee);
			Status($"Queued {val4.Title.GetFormattedText()} for {GiftHelper.GetDisplayName(val)}.");
			break;
		}
		case 3:
		{
			int[] selectedItems = _potionList.GetSelectedItems();
			if (selectedItems.Length == 0)
			{
				Status("Select a potion first.");
				return;
			}
			int num = selectedItems[0];
			if (num >= _potions.Count)
			{
				return;
			}
			PotionModel val2 = _potions[num];
			if (!val.HasOpenPotionSlots)
			{
				Status("Player has no open potion slots.");
				return;
			}
			int potionFee = GiftHelper.GetPotionFee(val2);
			if (me.Gold < potionFee)
			{
				Status("Not enough gold.");
				return;
			}
			int num2 = -1;
			for (int i = 0; i < me.PotionSlots.Count; i++)
			{
				if (me.PotionSlots[i] == val2)
				{
					num2 = i;
					break;
				}
			}
			if (num2 < 0)
			{
				return;
			}
			Enqueue(me, GiftKind.Potion, playerIndex, num2, potionFee);
			Status($"Queued {val2.Title.GetFormattedText()} for {GiftHelper.GetDisplayName(val)}.");
			break;
		}
		}
		Refresh();
	}

	private static void Enqueue(Player giver, GiftKind kind, int targetIdx, int srcIdx, int fee)
	{
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue((GameAction)(object)new GiveGiftAction(giver, kind, targetIdx, srcIdx, fee));
	}

	private static void Status(string msg)
	{
		if (_statusLabel != null)
		{
			_statusLabel.Text = msg;
		}
	}

	private static StyleBoxFlat MakeSB(Color bg, Color? border = null, int bw = 0, int cr = 4)
	{
		StyleBoxFlat val = new StyleBoxFlat
		{
			BgColor = bg
		};
		val.CornerRadiusBottomLeft = cr;
		val.CornerRadiusBottomRight = cr;
		val.CornerRadiusTopLeft = cr;
		val.CornerRadiusTopRight = cr;
		if (border.HasValue && bw > 0)
		{
			val.BorderColor = border.Value;
			val.BorderWidthBottom = bw;
			val.BorderWidthTop = bw;
			val.BorderWidthLeft = bw;
			val.BorderWidthRight = bw;
		}
		((StyleBox)val).ContentMarginLeft = 8f;
		((StyleBox)val).ContentMarginRight = 8f;
		((StyleBox)val).ContentMarginTop = 4f;
		((StyleBox)val).ContentMarginBottom = 4f;
		return val;
	}

	private static void StyleBtn(Button b)
	{
		Color bg = new(0.22f, 0.2f, 0.18f, 1f);
		Color bg2 = new(0.32f, 0.28f, 0.24f, 1f);
		Color bg3 = new(0.16f, 0.14f, 0.12f, 1f);
		Color value = new(0.75f, 0.65f, 0.45f, 1f);
		((Control)b).AddThemeStyleboxOverride("normal", (StyleBox)(object)MakeSB(bg, value, 2));
		((Control)b).AddThemeStyleboxOverride("hover", (StyleBox)(object)MakeSB(bg2, value, 2));
		((Control)b).AddThemeStyleboxOverride("pressed", (StyleBox)(object)MakeSB(bg3, value, 2));
		((Control)b).AddThemeStyleboxOverride("focus", (StyleBox)(object)MakeSB(bg, value, 2));
		((Control)b).AddThemeColorOverride("font_color", new Color(0.9f, 0.85f, 0.7f, 1f));
		((Control)b).AddThemeColorOverride("font_hover_color", new Color(1f, 0.95f, 0.8f, 1f));
		((Control)b).AddThemeFontSizeOverride("font_size", 20);
		if (_fontBold != null)
		{
			((Control)b).AddThemeFontOverride("font", _fontBold);
		}
	}

	private static void StylePanel(PanelContainer p)
	{
		((Control)p).AddThemeStyleboxOverride("panel", (StyleBox)(object)MakeSB(new Color(0.12f, 0.11f, 0.1f, 0.95f), (Color?)new Color(0.55f, 0.45f, 0.3f, 1f), 2, 6));
	}

	private static void StyleLbl(Label l, int sz = 20)
	{
		((Control)l).AddThemeFontSizeOverride("font_size", sz);
		if (_fontRegular != null)
		{
			((Control)l).AddThemeFontOverride("font", _fontRegular);
		}
	}
}
