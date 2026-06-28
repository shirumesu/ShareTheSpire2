using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace STS2GiftingMod;

[HarmonyPatch(typeof(NRun), "_Ready")]
internal static class NRunGiftPanelPatch
{
	private static void Postfix(NRun __instance, RunState ____state)
	{
		if (((__instance != null) ? __instance.GlobalUi : null) != null && ____state != null && ____state.Players != null && ____state.Players.Count >= 2)
		{
			GiftPanel.Attach(__instance, ____state);
		}
	}
}
