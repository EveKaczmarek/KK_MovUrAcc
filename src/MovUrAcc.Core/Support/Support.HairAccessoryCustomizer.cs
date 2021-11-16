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

			internal static object GetController(ChaControl _chaCtrl)
			{
				if (!Installed) return null;

				return Traverse.Create(PluginInstance).Method("GetController", new object[] { _chaCtrl }).GetValue();;
			}

			internal static void StoreSetting(ChaControl _chaCtrl, object _pluginCtrl, int _slotIndex)
			{
				if (!Installed) return;

				if (IsHairAccessory(_chaCtrl, _slotIndex))
				{
					Traverse _traverse = Traverse.Create(_pluginCtrl);

					HairAccessoryInfos[_slotIndex] = new HairAccessoryInfo()
					{
						HairGloss = _traverse.Method("GetHairGloss", new object[] { _slotIndex }).GetValue<bool>(),
						ColorMatch = _traverse.Method("GetColorMatch", new object[] { _slotIndex }).GetValue<bool>(),
						OutlineColor = _traverse.Method("GetOutlineColor", new object[] { _slotIndex }).GetValue<Color>(),
						AccessoryColor = _traverse.Method("GetAccessoryColor", new object[] { _slotIndex }).GetValue<Color>(),
						HairLength = _traverse.Method("GetHairLength", new object[] { _slotIndex }).GetValue<float>()
					};
				}

				RemoveSetting(_pluginCtrl, _slotIndex);
			}

			internal static void ModifySetting(object _pluginCtrl, int _srcSlot, int _dstSlot)
			{
				if (!Installed) return;

				RemoveSetting(_pluginCtrl, _dstSlot);

				if (!HairAccessoryInfos.ContainsKey(_srcSlot))
					return;

				Traverse _traverse = Traverse.Create(_pluginCtrl);

				if (!_traverse.Method("InitHairAccessoryInfo", new object[] { _dstSlot }).GetValue<bool>())
					return;

				_traverse.Method("SetHairGloss", new object[] { HairAccessoryInfos[_srcSlot].HairGloss, _dstSlot }).GetValue();
				_traverse.Method("SetColorMatch", new object[] { HairAccessoryInfos[_srcSlot].ColorMatch, _dstSlot }).GetValue();
				_traverse.Method("SetOutlineColor", new object[] { HairAccessoryInfos[_srcSlot].OutlineColor, _dstSlot }).GetValue();
				_traverse.Method("SetAccessoryColor", new object[] { HairAccessoryInfos[_srcSlot].AccessoryColor, _dstSlot }).GetValue();
				_traverse.Method("SetHairLength", new object[] { HairAccessoryInfos[_srcSlot].HairLength, _dstSlot }).GetValue();
			}

			internal static void RemoveSetting(object _pluginCtrl, int _slotIndex)
			{
				if (!Installed) return;

				Traverse.Create(_pluginCtrl).Method("RemoveHairAccessoryInfo", new object[] { _slotIndex }).GetValue();
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
