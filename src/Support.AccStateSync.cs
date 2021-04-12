using System;

using BepInEx;
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

			internal static void HookInit()
			{
				if (!Installed) return;

				Type AccStateSyncController = PluginInstance.GetType().Assembly.GetType("AccStateSync.AccStateSync+AccStateSyncController");
				HooksInstance.Patch(AccStateSyncController.GetMethod("AccSlotChangedHandler", AccessTools.all, null, new[] { typeof(int) }, null), prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.DuringLoading_Prefix)));
				HooksInstance.Patch(AccStateSyncController.GetMethod("SyncOutfitVirtualGroupInfo", AccessTools.all, null, new[] { typeof(int) }, null), prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.DuringLoading_Prefix)));
			}

			internal static object GetController(ChaControl chaCtrl)
			{
				if (!Installed) return null;
				return Traverse.Create(PluginInstance).Method("GetController", new object[] { chaCtrl }).GetValue();
			}

			internal static void ModifySetting(object pluginCtrl, int index, int srcSlot, int dstSlot)
			{
				if (!Installed) return;

				RemoveSetting(pluginCtrl, index, dstSlot);
				object CurOutfitTriggerInfo = Traverse.Create(pluginCtrl).Field("CharaTriggerInfo").GetValue().RefTryGetValue(index);
				if (!Traverse.Create(CurOutfitTriggerInfo).Property("Parts").Method("ContainsKey", new object[] { srcSlot }).GetValue<bool>())
					return;

				Traverse.Create(pluginCtrl).Method("AccessoryTransferredHandler", new object[] { srcSlot, dstSlot, index }).GetValue();
				RemoveSetting(pluginCtrl, index, srcSlot);
			}

			internal static void RemoveSetting(object pluginCtrl, int index, int slot)
			{
				if (!Installed) return;
				object CurOutfitTriggerInfo = Traverse.Create(pluginCtrl).Field("CharaTriggerInfo").GetValue().RefTryGetValue(index);
				Traverse.Create(CurOutfitTriggerInfo).Property("Parts").Method("Remove", new object[] { slot }).GetValue();
			}

			internal static void SyncVirtualGroupInfo(object pluginCtrl, int index)
			{
				if (!Installed) return;
				Traverse.Create(pluginCtrl).Method("SyncOutfitVirtualGroupInfo", new object[] { index }).GetValue();
			}
		}
	}
}
