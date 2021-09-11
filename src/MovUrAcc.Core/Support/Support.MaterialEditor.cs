using System;

using BepInEx;
using HarmonyLib;

using KKAPI.Maker;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class MaterialEditor
		{
			internal static bool Installed = false;
			internal static BaseUnityPlugin PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.materialeditor", out PluginInfo PluginInfo);
				PluginInstance = PluginInfo?.Instance;
				if (PluginInstance != null) Installed = true;
			}

			internal static void HookInit()
			{
				Type MaterialEditorCharaController = PluginInstance.GetType().Assembly.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController");
				HooksInstance.Patch(MaterialEditorCharaController.GetMethod("LoadData", AccessTools.all, null, new[] { typeof(bool), typeof(bool), typeof(bool) }, null), prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.DuringLoading_Co_Prefix)));
			}

			internal static object GetController(ChaControl chaCtrl)
			{
				if (!Installed) return null;
				return Traverse.Create(PluginInstance).Method("GetCharaController", new object[] { chaCtrl }).GetValue();
			}

			internal static void ModifySetting(object pluginCtrl, int index, int srcSlot, int dstSlot)
			{
				if (!Installed) return;
				Traverse.Create(pluginCtrl).Method("AccessoryTransferredEvent", new object[] { null, new AccessoryTransferEventArgs(srcSlot, dstSlot) }).GetValue();
				RemoveSetting(pluginCtrl, srcSlot);
			}

			internal static void RemoveSetting(object pluginCtrl, int slot)
			{
				if (!Installed) return;
				Traverse.Create(pluginCtrl).Method("AccessoryKindChangeEvent", new object[] { null, new AccessorySlotEventArgs(slot) }).GetValue();
			}
		}
	}
}
