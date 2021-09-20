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
				if (PluginInstance != null) Installed = true;

				if (PluginInfo.Metadata.Version.CompareTo(new Version("1.0.5.0")) < 0)
				{
					Logger.LogError($"BendUrAcc 1.0.5.0 is required to work properly, version {PluginInfo.Metadata.Version} detected");
					Installed = false;
				}
			}

			internal static object GetController(ChaControl chaCtrl)
			{
				if (!Installed) return null;

				return Traverse.Create(PluginInstance).Method("GetController", new object[] { chaCtrl }).GetValue();
			}

			internal static void ModifySetting(object pluginCtrl, int index, int srcSlot, int dstSlot)
			{
				if (!Installed) return;

				Traverse.Create(pluginCtrl).Method("CloneModifier", new object[] { srcSlot, dstSlot, index, index }).GetValue();
				Traverse.Create(pluginCtrl).Method("RemoveSlotModifier", new object[] { index, srcSlot }).GetValue();
			}

			internal static void RemoveSetting(object pluginCtrl, int index, int slot)
			{
				if (!Installed) return;

				Traverse.Create(pluginCtrl).Method("RemoveSlotModifier", new object[] { index, slot }).GetValue();
			}
		}
	}
}
