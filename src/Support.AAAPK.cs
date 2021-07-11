using BepInEx;
using HarmonyLib;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class AAAPK
		{
			internal static bool Installed = false;
			internal static BaseUnityPlugin PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("madevil.kk.AAAPK", out PluginInfo PluginInfo);
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

				Traverse.Create(pluginCtrl).Method("MoveRule", new object[] { srcSlot, dstSlot, index }).GetValue();
			}

			internal static void RemoveSetting(object pluginCtrl, int index, int slot)
			{
				if (!Installed) return;

				Traverse.Create(pluginCtrl).Method("RemoveRule", new object[] { index, slot }).GetValue();
			}
		}
	}
}
