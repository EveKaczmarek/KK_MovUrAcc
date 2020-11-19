using System.Collections.Generic;

using BepInEx;
using HarmonyLib;
using MessagePack;

using KKAPI.Maker;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class MoreAccessories
		{
			internal static MoreAccessoriesKOI.MoreAccessories PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.joan6694.illusionplugins.moreaccessories", out PluginInfo PluginInfo);
				PluginInstance = (MoreAccessoriesKOI.MoreAccessories) PluginInfo.Instance;
			}

			internal static void ModifyPartsInfo(ChaControl chaCtrl, int index, int srcSlot, int dstSlot)
			{
				bool noShake = false;

				ChaFileAccessory.PartsInfo[] parts = chaCtrl.chaFile.coordinate[index].accessory.parts;
				List<ChaFileAccessory.PartsInfo> nowAccessories = PluginInstance._charaMakerData.nowAccessories;

				ChaFileAccessory.PartsInfo part = AccessoriesApi.GetPartsInfo(srcSlot);
				byte[] bytes = MessagePackSerializer.Serialize(part);

				if (IsDark && part.noShake)
					noShake = true;

				Logger.LogDebug($"[srcSlot: {srcSlot + 1:00}] -> [dstSlot: {dstSlot + 1:00}][noShake: {noShake}]");

				ResetPartsInfo(chaCtrl, index, srcSlot);

				if (dstSlot < 20)
					parts[dstSlot] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(bytes);
				else
					nowAccessories[dstSlot - 20] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(bytes);

				if (noShake)
					Traverse.Create(AccessoriesApi.GetCvsAccessory(dstSlot)).Field("tglNoShake").Property("isOn").SetValue(noShake);
			}

			internal static void ResetPartsInfo(ChaControl chaCtrl, int index, int slot)
			{
				if (slot < 20)
					chaCtrl.chaFile.coordinate[index].accessory.parts[slot] = new ChaFileAccessory.PartsInfo();
				else
					PluginInstance._charaMakerData.nowAccessories[slot - 20] = new ChaFileAccessory.PartsInfo();
			}

			internal static void TrimUnusedSlots()
			{
				if (btnLock)
					return;
				btnLock = true;

				int n = PluginInstance._charaMakerData.nowAccessories.Count;

				if (n == 0)
				{
					Logger.LogMessage("No MoreAccessories slot, nothing to do");
					btnLock = false;
					return;
				}

				int i = n - 1;
				for (; i >= 0; i--)
				{
					if (PluginInstance._charaMakerData.nowAccessories[i].type > 120)
						break;
				}

				if (i == n - 1)
				{
					Logger.LogMessage("Last slot is being used, nothing to do");
					btnLock = false;
					return;
				}

				PluginInstance._charaMakerData.nowAccessories.RemoveRange(i + 1, n - 1 - i);
				ChaCustom.CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);

				btnLock = false;
			}
		}
	}
}
