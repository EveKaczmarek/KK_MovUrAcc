using System;
using System.Collections.Generic;
using System.Linq;

using BepInEx;
using HarmonyLib;

using MessagePack;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class MoreAccessories
		{
			internal static bool Installed = false;
			internal static bool BuggyBootleg = false;
			internal static BaseUnityPlugin PluginInstance = null;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.joan6694.illusionplugins.moreaccessories", out PluginInfo PluginInfo);
				PluginInstance = PluginInfo?.Instance;

				if (PluginInstance != null)
				{
					Installed = true;
					BuggyBootleg = PluginInfo.Metadata.Version.CompareTo(new Version("2.0")) > -1;
				}
			}

			internal static bool BuggyBootlegCheck()
			{
				return _chaCtrl.chaFile.coordinate.Any(x => x.accessory.parts.Length > 20);
			}

			internal static void ModifyPartsInfo(ChaControl _chaCtrl, int _coordinateIndex, int _srcSlot, int _dstSlot)
			{
				bool _noShake = false;

				ChaFileAccessory.PartsInfo _part = JetPack.Accessory.GetPartsInfo(_chaCtrl, _srcSlot);
				byte[] _bytes = MessagePackSerializer.Serialize(_part);

				if (IsDark && Traverse.Create(_part).Property("noShake").GetValue<bool>())
					_noShake = true;

				_logger.LogDebug($"[srcSlot: {_srcSlot + 1:00}] -> [dstSlot: {_dstSlot + 1:00}][noShake: {_noShake}]");

				ResetPartsInfo(_chaCtrl, _coordinateIndex, _srcSlot);

				JetPack.Accessory.SetPartsInfo(_chaCtrl, _coordinateIndex, _dstSlot, MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(_bytes));

				if (_noShake)
					Traverse.Create(JetPack.CharaMaker.GetCvsAccessory(_dstSlot)).Field("tglNoShake").Property("isOn").SetValue(_noShake);
			}

			internal static void ResetPartsInfo(ChaControl _chaCtrl, int _coordinateIndex, int _slotIndex)
			{
				JetPack.Accessory.SetPartsInfo(_chaCtrl, _coordinateIndex, _slotIndex, new ChaFileAccessory.PartsInfo());
			}

			internal static void TrimUnusedSlots()
			{
				if (BuggyBootleg)
					return;

				if (btnLock)
					return;
				btnLock = true;

				List<ChaFileAccessory.PartsInfo> _nowAccessories = JetPack.MoreAccessories.ListNowAccessories(_chaCtrl);

				int n = _nowAccessories.Count;

				if (n <= 0)
				{
					_logger.LogMessage("No MoreAccessories slot, nothing to do");
					btnLock = false;
					return;
				}

				int i = n - 1;
				for (; i >= 0; i--)
				{
					if (_nowAccessories[i].type > 120)
						break;
				}

				if (i == n - 1)
				{
					_logger.LogMessage("Last slot is being used, nothing to do");
					btnLock = false;
					return;
				}

				_nowAccessories.RemoveRange(i + 1, n - 1 - i);

				btnLock = false;
				_chaCtrl.ChangeCoordinateTypeAndReload(false);
			}

			internal static void AddSlot()
			{
				if (!BuggyBootleg)
					Traverse.Create(PluginInstance).Method("AddSlot").GetValue();
				else
				{
					Type Accessories = PluginInstance.GetType().Assembly.GetType("MoreAccessoriesKOI.Accessories");
					Traverse.Create(Accessories).Method("AddSlot", new object[] { 1 }).GetValue();
				}
			}
		}
	}
}
