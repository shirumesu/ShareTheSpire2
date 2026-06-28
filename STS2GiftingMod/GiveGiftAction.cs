using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace STS2GiftingMod;

public class GiveGiftAction : GameAction
{
	private readonly Player _giver;

	private readonly GiftKind _kind;

	private readonly int _targetPlayerIndex;

	private readonly int _sourceIndex;

	private readonly int _fee;

	public override ulong OwnerId => _giver.NetId;

	public override GameActionType ActionType => (GameActionType)3;

	public GiveGiftAction(Player giver, GiftKind kind, int targetPlayerIndex, int sourceIndex, int fee)
	{
		_giver = giver;
		_kind = kind;
		_targetPlayerIndex = targetPlayerIndex;
		_sourceIndex = sourceIndex;
		_fee = fee;
	}

	public override async Task ExecuteAction()
	{
		if (_giver.RunState == null)
		{
			return;
		}
		List<Player> players = ((IPlayerCollection)_giver.RunState).Players.ToList();
		if (_targetPlayerIndex < 0 || _targetPlayerIndex >= players.Count)
		{
			return;
		}
		Player target = players[_targetPlayerIndex];
		if (target != null && target != _giver)
		{
			switch (_kind)
			{
			case GiftKind.Gold:
				await ExecuteGold(target);
				break;
			case GiftKind.Card:
				await ExecuteCard(target);
				break;
			case GiftKind.Relic:
				await ExecuteRelic(target);
				break;
			case GiftKind.Potion:
				await ExecutePotion(target);
				break;
			}
		}
	}

	private Task ExecuteGold(Player target)
	{
		int num = _sourceIndex + _fee;
		if (_giver.Gold < num)
		{
			return Task.CompletedTask;
		}
		Player giver = _giver;
		giver.Gold -= num;
		target.Gold += _sourceIndex;
		return Task.CompletedTask;
	}

	private async Task ExecuteCard(Player target)
	{
		if (_sourceIndex >= 0 && _sourceIndex < _giver.Deck.Cards.Count)
		{
			CardModel card = _giver.Deck.Cards[_sourceIndex];
			if (card != null && GiftHelper.IsCardGiftable(card) && _giver.Gold >= _fee)
			{
				var serialized = card.ToSerializable();
				CardModel giftedCard = target.RunState.LoadCard(serialized, target);
				var addResult = await CardPileCmd.Add(giftedCard, PileType.Deck, skipVisuals: true);
				if (!addResult.success)
				{
					return;
				}
				await CardPileCmd.RemoveFromDeck(card, showPreview: false);
				Player giver = _giver;
				giver.Gold -= _fee;
			}
		}
	}

	private async Task ExecuteRelic(Player target)
	{
		RelicModel relic = _giver.Relics.ElementAtOrDefault(_sourceIndex);
		if (relic != null && relic.IsTradable && _giver.Gold >= _fee)
		{
			var serialized = relic.ToSerializable();
			await RelicCmd.Remove(relic);
			RelicModel cloned = RelicModel.FromSerializable(serialized);
			await RelicCmd.Obtain(cloned, target);
			if (!target.DiscoveredRelics.Contains(((AbstractModel)cloned).Id))
			{
				target.DiscoveredRelics.Add(((AbstractModel)cloned).Id);
			}
			Player giver = _giver;
			giver.Gold -= _fee;
		}
	}

	private async Task ExecutePotion(Player target)
	{
		if (_sourceIndex >= 0 && _sourceIndex < _giver.PotionSlots.Count)
		{
			PotionModel potion = _giver.PotionSlots[_sourceIndex];
			if (potion != null && GiftHelper.IsPotionGiftable(potion) && target.HasOpenPotionSlots && _giver.Gold >= _fee)
			{
				var serialized = potion.ToSerializable(_sourceIndex);
				PotionModel cloned = PotionModel.FromSerializable(serialized);
				var result = await PotionCmd.TryToProcure(cloned, target);
				if (!result.success)
				{
					return;
				}
				await PotionCmd.Discard(potion);
				Player giver = _giver;
				giver.Gold -= _fee;
			}
		}
	}

	public override INetAction ToNetAction()
	{
		return (INetAction)(object)new NetGiveGiftAction
		{
			kind = _kind,
			targetPlayerIndex = _targetPlayerIndex,
			sourceIndex = _sourceIndex,
			fee = _fee
		};
	}
}
