using BepInEx;
using HarmonyLib;

using KKAPI.Maker;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class MaterialRouter
		{
			internal static bool Installed = false;
			internal static BaseUnityPlugin PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("madevil.kk.mr", out PluginInfo PluginInfo);
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
				Traverse.Create(pluginCtrl).Method("TransferAccSlotInfo", new object[] { index, new AccessoryTransferEventArgs(srcSlot, dstSlot) }).GetValue();
				RemoveSetting(pluginCtrl, index, srcSlot);
			}

			internal static void RemoveSetting(object pluginCtrl, int index, int slot)
			{
				if (!Installed) return;
				Traverse.Create(pluginCtrl).Method("RemoveAccSlotInfo", new object[] { index, slot }).GetValue();
			}
		}
	}
}
