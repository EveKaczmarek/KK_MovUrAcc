using System.Collections.Generic;

using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class HairAccessoryCustomizer
		{
			internal static bool Installed = false;
			internal static BaseUnityPlugin PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.hairaccessorycustomizer", out PluginInfo PluginInfo);
				PluginInstance = PluginInfo?.Instance;
				if (PluginInstance != null) Installed = true;
			}

			internal static object GetController(ChaControl chaCtrl)
			{
				if (!Installed) return null;
				return Traverse.Create(PluginInstance).Method("GetController", new object[] { chaCtrl }).GetValue();;
			}

			internal static void StoreSetting(ChaControl chaCtrl, object pluginCtrl, int slot)
			{
				if (!Installed)
					return;

				if (IsHairAccessory(chaCtrl, slot))
				{
					HairAccessoryInfos[slot] = new HairAccessoryInfo()
					{
						HairGloss = Traverse.Create(pluginCtrl).Method("GetHairGloss", new object[] { slot }).GetValue<bool>(),
						ColorMatch = Traverse.Create(pluginCtrl).Method("GetColorMatch", new object[] { slot }).GetValue<bool>(),
						OutlineColor = Traverse.Create(pluginCtrl).Method("GetOutlineColor", new object[] { slot }).GetValue<Color>(),
						AccessoryColor = Traverse.Create(pluginCtrl).Method("GetAccessoryColor", new object[] { slot }).GetValue<Color>(),
						HairLength = Traverse.Create(pluginCtrl).Method("GetHairLength", new object[] { slot }).GetValue<float>()
					};
				}

				RemoveSetting(pluginCtrl, slot);
			}

			internal static void ModifySetting(object pluginCtrl, int srcSlot, int dstSlot)
			{
				if (!Installed) return;

				RemoveSetting(pluginCtrl, dstSlot);

				if (!HairAccessoryInfos.ContainsKey(srcSlot))
					return;

				if (!Traverse.Create(pluginCtrl).Method("InitHairAccessoryInfo", new object[] { dstSlot }).GetValue<bool>())
					return;

				Traverse.Create(pluginCtrl).Method("SetHairGloss", new object[] { HairAccessoryInfos[srcSlot].HairGloss, dstSlot }).GetValue();
				Traverse.Create(pluginCtrl).Method("SetColorMatch", new object[] { HairAccessoryInfos[srcSlot].ColorMatch, dstSlot }).GetValue();
				Traverse.Create(pluginCtrl).Method("SetOutlineColor", new object[] { HairAccessoryInfos[srcSlot].OutlineColor, dstSlot }).GetValue();
				Traverse.Create(pluginCtrl).Method("SetAccessoryColor", new object[] { HairAccessoryInfos[srcSlot].AccessoryColor, dstSlot }).GetValue();
				Traverse.Create(pluginCtrl).Method("SetHairLength", new object[] { HairAccessoryInfos[srcSlot].HairLength, dstSlot }).GetValue();
			}

			internal static void RemoveSetting(object pluginCtrl, int slot)
			{
				if (!Installed) return;
				Traverse.Create(pluginCtrl).Method("RemoveHairAccessoryInfo", new object[] { slot }).GetValue();
			}

			internal class HairAccessoryInfo
			{
				public bool HairGloss;
				public bool ColorMatch;
				public Color OutlineColor;
				public Color AccessoryColor;
				public float HairLength;
			}

			internal static Dictionary<int, HairAccessoryInfo> HairAccessoryInfos;
		}
	}
}
