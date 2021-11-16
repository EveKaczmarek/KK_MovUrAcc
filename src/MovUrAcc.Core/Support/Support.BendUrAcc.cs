using System;

using BepInEx;
using HarmonyLib;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class BendUrAcc
		{
			internal static bool Installed = false;
			internal static BaseUnityPlugin PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("madevil.kk.BendUrAcc", out PluginInfo PluginInfo);
				PluginInstance = PluginInfo?.Instance;
				if (PluginInstance != null)
				{
					if (PluginInfo.Metadata.Version.CompareTo(new Version("1.0.5.0")) < 0)
					{
						_logger.LogError($"BendUrAcc 1.0.5.0 is required to work properly, version {PluginInfo.Metadata.Version} detected");
						return;
					}
					Installed = true;
				}
			}

			internal static object GetController(ChaControl _chaCtrl)
			{
				if (!Installed) return null;

				return Traverse.Create(PluginInstance).Method("GetController", new object[] { _chaCtrl }).GetValue();
			}

			internal static void ModifySetting(object _pluginCtrl, int _coordinateIndex, int _srcSlot, int _dstSlot)
			{
				if (!Installed) return;

				Traverse.Create(_pluginCtrl).Method("CloneModifier", new object[] { _srcSlot, _dstSlot, _coordinateIndex, _coordinateIndex }).GetValue();
				Traverse.Create(_pluginCtrl).Method("RemoveSlotModifier", new object[] { _coordinateIndex, _srcSlot }).GetValue();
			}

			internal static void RemoveSetting(object _pluginCtrl, int _coordinateIndex, int _slotIndex)
			{
				if (!Installed) return;

				Traverse.Create(_pluginCtrl).Method("RemoveSlotModifier", new object[] { _coordinateIndex, _slotIndex }).GetValue();
			}
		}
	}
}
