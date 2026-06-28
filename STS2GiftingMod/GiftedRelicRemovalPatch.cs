using System;
using System.Collections;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace STS2GiftingMod;

[HarmonyPatch(typeof(NRelicInventory))]
internal static class GiftedRelicRemovalPatch
{
	private static readonly FieldInfo RelicNodesField = AccessTools.Field(typeof(NRelicInventory), "_relicNodes");

	private static readonly MethodInfo? EmitChanged = AccessTools.Method(typeof(NRelicInventory), "EmitSignalRelicsChanged", (Type[])null, (Type[])null);

	private static readonly MethodInfo? UpdateNav = AccessTools.Method(typeof(NRelicInventory), "UpdateNavigation", (Type[])null, (Type[])null);

	[HarmonyPostfix]
	[HarmonyPatch("OnRelicRemoved")]
	private static void Postfix(NRelicInventory __instance, RelicModel relic)
	{
		if (__instance == null || relic == null || !(RelicNodesField.GetValue(__instance) is IList list))
		{
			return;
		}
		NRelicInventoryHolder val = null;
		foreach (object item in list)
		{
			NRelicInventoryHolder val2 = (NRelicInventoryHolder)((item is NRelicInventoryHolder) ? item : null);
			if (val2 != null)
			{
				NRelic relic2 = val2.Relic;
				if (((relic2 != null) ? relic2.Model : null) == relic)
				{
					val = val2;
					break;
				}
			}
		}
		if (val != null)
		{
			list.Remove(val);
			if ((object)((Node)val).GetParent() == __instance)
			{
				((Node)__instance).RemoveChild((Node)(object)val);
			}
			((Node)val).QueueFree();
			EmitChanged?.Invoke(__instance, null);
			UpdateNav?.Invoke(__instance, null);
		}
	}
}
