using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace STS2GiftingMod;

[HarmonyPatch(typeof(NRelicInventory))]
internal static class GiftedRelicAnimationPatch
{
	[HarmonyPostfix]
	[HarmonyPatch("OnRelicObtained")]
	private static void Postfix(NRelicInventory __instance, RelicModel relic)
	{
		if (__instance != null && relic != null && GiftedRelicAnimationTracker.Consume(relic))
		{
			__instance.AnimateRelic(relic, (Vector2?)null, (Vector2?)null);
		}
	}
}
