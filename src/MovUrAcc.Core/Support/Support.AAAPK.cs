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

			internal static void HookInit()
			{
				if (!Installed) return;

				_hooksInstance.Patch(PluginInstance.GetType().Assembly.GetType("AAAPK.AAAPK+Hooks").GetMethod("ChaControl_ChangeShakeAccessory_Prefix", AccessTools.all), prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.DuringLoading_Prefix)));
			}

			internal static object GetController(ChaControl _chaCtrl)
			{
				if (!Installed) return null;

				return Traverse.Create(PluginInstance).Method("GetController", new object[] { _chaCtrl }).GetValue();
			}

			internal static void ModifySetting(object _pluginCtrl, int _coordinateIndex, int _srcSlot, int _dstSlot)
			{
				if (!Installed) return;

				Traverse.Create(_pluginCtrl).Method("MoveRule", new object[] { _srcSlot, _dstSlot, _coordinateIndex }).GetValue();
			}

			internal static void RemoveSetting(object _pluginCtrl, int _coordinateIndex, int _slotIndex)
			{
				if (!Installed) return;

				Traverse.Create(_pluginCtrl).Method("RemoveRule", new object[] { _coordinateIndex, _slotIndex }).GetValue();
			}
		}
	}
}
