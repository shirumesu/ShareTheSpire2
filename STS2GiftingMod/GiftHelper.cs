using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace STS2GiftingMod;

internal static class GiftHelper
{
	public static List<Player> GetOtherPlayers(Player self)
	{
		if (self == null || self.RunState == null)
		{
			return new List<Player>();
		}
		return ((IPlayerCollection)self.RunState).Players.Where((Player p) => p != null && p != self).ToList();
	}

	public static int GetPlayerIndex(Player player)
	{
		if (((player != null) ? player.RunState : null) == null)
		{
			return -1;
		}
		return ((IPlayerCollection)player.RunState).Players.ToList().IndexOf(player);
	}

	public static string GetDisplayName(Player player)
	{
		return PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId);
	}

	public static bool IsCardGiftable(CardModel card)
	{
		if (card == null || card.HasBeenRemovedFromState)
		{
			return false;
		}
		if (card.Pile == null || (int)card.Pile.Type != 6)
		{
			return false;
		}
		return (int)card.Rarity - 1 <= 3;
	}

	public static bool IsRelicGiftable(RelicModel relic)
	{
		if (relic == null)
		{
			return false;
		}
		return relic.IsTradable;
	}

	public static bool IsPotionGiftable(PotionModel potion)
	{
		if (potion == null || potion.HasBeenRemovedFromState)
		{
			return false;
		}
		return (int)potion.Rarity - 1 <= 2;
	}

	public static int GetGoldFee(int amount)
	{
		return 0;
	}

	public static int GetCardFee(CardModel card)
	{
		return 0;
	}

	public static int GetRelicFee(RelicModel relic)
	{
		return 0;
	}

	public static int GetPotionFee(PotionModel potion)
	{
		return 0;
	}

	public static Color GetCardColor(CardModel card)
	{
		Color result = ((int)card.Rarity - 1) switch
		{
			0 => new Color("B0B0B0"), 
			1 => new Color("D4D4D4"), 
			2 => new Color("5BBFDE"), 
			3 => new Color("FFD454"), 
			_ => new Color("AAAAAA"), 
		};
		return result;
	}

	public static Color GetRelicColor(RelicModel relic)
	{
		Color result = ((int)relic.Rarity - 1) switch
		{
			0 => new Color("B0B0B0"), 
			1 => new Color("D4D4D4"), 
			2 => new Color("5BBFDE"), 
			3 => new Color("FFD454"), 
			4 => new Color("3A6BD4"), 
			5 => new Color("CF8BF3"), 
			_ => new Color("AAAAAA"), 
		};
		return result;
	}

	public static Color GetPotionColor(PotionModel potion)
	{
		Color result = ((int)potion.Rarity - 1) switch
		{
			0 => new Color("D4D4D4"), 
			1 => new Color("5BBFDE"), 
			2 => new Color("FFD454"), 
			_ => new Color("AAAAAA"), 
		};
		return result;
	}
}
