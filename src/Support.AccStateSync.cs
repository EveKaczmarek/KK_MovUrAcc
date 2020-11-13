﻿using BepInEx;
using HarmonyLib;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class AccStateSync
		{
			internal static bool Installed = false;
			internal static BaseUnityPlugin PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("madevil.kk.ass", out PluginInfo PluginInfo);
				PluginInstance = PluginInfo?.Instance;
				if (PluginInstance != null) Installed = true;
			}

			internal static object GetController(ChaControl chaCtrl)
			{
				if (!Installed) return null;
				return Traverse.Create(PluginInstance).Method("GetController", new object[] { chaCtrl }).GetValue();
			}

			internal static void ModifySetting(object pluginCtrl, int index, int srcSlot, int dstSlot)
			{
				if (!Installed) return;

				RemoveSetting(pluginCtrl, dstSlot);
				if (!Traverse.Create(pluginCtrl).Field("CurOutfitTriggerInfo").Property("Parts").Method("ContainsKey", new object[] { srcSlot }).GetValue<bool>())
					return;

				Traverse.Create(pluginCtrl).Method("AccessoryTransferredHandler", new object[] { srcSlot, dstSlot, index }).GetValue();
				RemoveSetting(pluginCtrl, srcSlot);
			}

			internal static void RemoveSetting(object pluginCtrl, int slot)
			{
				if (!Installed) return;
				Traverse.Create(pluginCtrl).Field("CurOutfitTriggerInfo").Property("Parts").Method("Remove", new object[] { slot }).GetValue();
			}

			internal static void SyncVirtualGroupInfo(object pluginCtrl, int index)
			{
				if (!Installed) return;
				Traverse.Create(pluginCtrl).Method("SyncOutfitVirtualGroupInfo", new object[] { index }).GetValue();
			}
		}
	}
}
