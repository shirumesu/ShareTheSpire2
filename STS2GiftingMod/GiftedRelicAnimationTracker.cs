using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace STS2GiftingMod;

internal static class GiftedRelicAnimationTracker
{
	private static readonly HashSet<RelicModel> Pending = new HashSet<RelicModel>();

	public static void Mark(RelicModel relic)
	{
		if (relic != null)
		{
			Pending.Add(relic);
		}
	}

	public static bool Consume(RelicModel relic)
	{
		return relic != null && Pending.Remove(relic);
	}
}
