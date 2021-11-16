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
				if (PluginInstance != null)
				{
					if (PluginInstance.Info.Metadata.Version.CompareTo(new Version("4.0.0.0")) < 0)
					{
						_logger.LogError($"AccStateSync 4.0.0.0 is required to work properly, version {PluginInfo.Metadata.Version} detected");
						return;
					}
					Installed = true;
				}
			}

			internal static void HookInit()
			{
				if (!Installed) return;

				Type AccStateSyncController = PluginInstance.GetType().Assembly.GetType("AccStateSync.AccStateSync+AccStateSyncController");
				_hooksInstance.Patch(AccStateSyncController.GetMethod("AccSlotChangedHandler", AccessTools.all), prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.DuringLoading_Prefix)));
				_hooksInstance.Patch(AccStateSyncController.GetMethod("RefreshCache", AccessTools.all), prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.DuringLoading_Prefix)));
			}

			internal static object GetController(ChaControl _chaCtrl)
			{
				if (!Installed) return null;

				return Traverse.Create(PluginInstance).Method("GetController", new object[] { _chaCtrl }).GetValue();
			}

			internal static void ModifySetting(object _pluginCtrl, int _coordinateIndex, int _srcSlot, int _dstSlot)
			{
				if (!Installed) return;

				Traverse.Create(_pluginCtrl).Method("CloneSlotTriggerProperty", new object[] { _srcSlot, _dstSlot, _coordinateIndex, _coordinateIndex }).GetValue();
				RemoveSetting(_pluginCtrl, _coordinateIndex, _srcSlot);
			}

			internal static void RemoveSetting(object _pluginCtrl, int _coordinateIndex, int _slotIndex)
			{
				if (!Installed) return;

				Traverse.Create(_pluginCtrl).Method("RemoveSlotTriggerProperty", new object[] { _coordinateIndex, _slotIndex }).GetValue();
			}
		}
	}
}
