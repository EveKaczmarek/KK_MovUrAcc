using System.Collections.Generic;
using System.Linq;
using UniRx;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	[BepInPlugin(GUID, PluginName, Version)]
	[BepInDependency("marco.kkapi")]
	[BepInDependency("com.deathweasel.bepinex.materialeditor")]
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
	public partial class MovUrAcc : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.MovUrAcc";
		public const string PluginName = "MovUrAcc";
		public const string Version = "1.1.0.0";

		internal static new ManualLogSource Logger;
		internal static bool IsDark;

		private void Start()
		{
			Logger = base.Logger;

			IsDark = typeof(ChaControl).GetProperties(AccessTools.all).Any(x => x.Name == "exType");

			MoreAccessories.InitSupport();
			MaterialEditor.InitSupport();
			HairAccessoryCustomizer.InitSupport();
			AccStateSync.InitSupport();

			MakerAPI.RegisterCustomSubCategories += (object sender, RegisterSubCategoriesEvent ev) =>
			{
				MakerCategory category = new MakerCategory("05_ParameterTop", "tglMovUrAcc", MakerConstants.Parameter.Attribute.Position + 1, "MovUrAcc");

				ev.AddControl(new MakerText("Batch transfer accessory slots", category, this));

				MakerTextbox StartTextbox = ev.AddControl(new MakerTextbox(category, "Start", "", this));
				MakerTextbox EndTextbox = ev.AddControl(new MakerTextbox(category, "End", "", this));

				MakerTextbox NewStartTextbox = ev.AddControl(new MakerTextbox(category, "Shift first slot to", "", this));

				MakerButton btnApply = new MakerButton("Go", category, this);
				ev.AddControl(btnApply);
				btnApply.OnClick.AddListener(delegate
				{
					int start = 0, end = 0, newstart = 0;
					if (!int.TryParse(StartTextbox.Value, out start))
					{
						StartTextbox.Value = "";
						start = 0;
					}
					if (!int.TryParse(EndTextbox.Value, out end))
					{
						EndTextbox.Value = "";
						end = 0;
					}
					if (!int.TryParse(NewStartTextbox.Value, out newstart))
					{
						NewStartTextbox.Value = "";
						newstart = 0;
					}
					ApplyShifting(start - 1, end - 1, newstart - 1);
				});

				ev.AddControl(new MakerSeparator(category, this));

				ev.AddControl(new MakerText("Pack acc list by removing unused slots", category, this));

				MakerButton btnPackSlots = new MakerButton("Go", category, this);
				ev.AddControl(btnPackSlots);
				btnPackSlots.OnClick.AddListener(delegate
				{
					ApplyPacking();
				});

				ev.AddControl(new MakerSeparator(category, this));

				ev.AddControl(new MakerText("Remove hair accessories", category, this));

				var tglRemoveHairAccInverse = new MakerToggle(category, "Inverse selection", false, this);
				ev.AddControl(tglRemoveHairAccInverse);

				MakerButton btnRemoveHairAcc = new MakerButton("Go", category, this);
				ev.AddControl(btnRemoveHairAcc);
				btnRemoveHairAcc.OnClick.AddListener(delegate
				{
					RemoveHairAcc(tglRemoveHairAccInverse.Value);
				});

				ev.AddControl(new MakerSeparator(category, this));

				ev.AddControl(new MakerText("Trim down unused MoreAccessories slots", category, this));

				MakerButton btnMoreAccApply = new MakerButton("Go", category, this);
				ev.AddControl(btnMoreAccApply);
				btnMoreAccApply.OnClick.AddListener(delegate
				{
					MoreAccessories.TrimUnusedSlots();
				});

				ev.AddSubCategory(category);
			};
		}

		internal static void ApplyShifting(int start, int end, int newstart)
		{
			int nowAccCount = MoreAccessories.PluginInstance._charaMakerData.nowAccessories.Count;

			if (start < 0)
				start = 0;

			if (end < 0)
				end = nowAccCount + 19;
			else if (end > nowAccCount + 19)
				end = nowAccCount + 19;

			if (newstart < 0)
				newstart = 0;

			if (start == newstart)
			{
				Logger.LogMessage($"Start and new start are the same, nothing to do");
				return;
			}

			if (start > end)
			{
				Logger.LogMessage($"End value must be greater than start value");
				return;
			}

			int amount = newstart - start;
			Logger.LogDebug($"[start: {start + 1:00}][end: {end + 1:00}][newstart: {newstart + 1:00}][amount: {amount}]");

			List<QueueItem> Queue = new List<QueueItem>();
			for (int i = start; i <= end; i++)
				Queue.Add(new QueueItem(i, i + amount));

			if (amount > 0)
			{
				if (end + amount > 19)
				{
					Logger.LogDebug($"Expand MoreAccessories slots from {nowAccCount} to {end + amount - 19}");

					for (int i = 0; i < (end + amount - 19 - nowAccCount); i++)
						Traverse.Create(MoreAccessories.PluginInstance).Method("AddSlot").GetValue();
				}

				Queue = Queue.OrderByDescending(x => x.srcSlot).ToList();
			}

			ProcessQueue(Queue);
		}

		internal static void ApplyPacking()
		{
			List<QueueItem> Queue = new List<QueueItem>();
			int nowAccCount = MoreAccessories.PluginInstance._charaMakerData.nowAccessories.Count;
			int dstSlot = 0;

			for (int srcSlot = 0; srcSlot < (20 + nowAccCount); srcSlot++)
			{
				ChaFileAccessory.PartsInfo part = AccessoriesApi.GetPartsInfo(srcSlot);
				if (part.type == 120)
					continue;

				if (srcSlot != dstSlot)
					Queue.Add(new QueueItem(srcSlot, dstSlot));

				dstSlot++;
			}

			if (Queue.Count == 0)
			{
				Logger.LogMessage("Nothing to do");
				return;
			}

			ProcessQueue(Queue);
		}

		internal static void RemoveHairAcc(bool inverse)
		{
			ChaControl chaCtrl = MakerAPI.GetCharacterControl();
			var MEpluginCtrl = MaterialEditor.GetController(chaCtrl);
			var HACpluginCtrl = HairAccessoryCustomizer.GetController(chaCtrl);
			var ASSpluginCtrl = AccStateSync.GetController(chaCtrl);
			int nowAccCount = MoreAccessories.PluginInstance._charaMakerData.nowAccessories.Count;
			int Coordinate = chaCtrl.fileStatus.coordinateType;

			for (int srcSlot = 0; srcSlot < (20 + nowAccCount); srcSlot++)
			{
				ChaFileAccessory.PartsInfo part = AccessoriesApi.GetPartsInfo(srcSlot);
				if (part.type == 120)
					continue;
				if (!inverse && !IsHairAccessory(chaCtrl, srcSlot))
					continue;
				else if (inverse && IsHairAccessory(chaCtrl, srcSlot))
					continue;

				HairAccessoryCustomizer.RemoveSetting(HACpluginCtrl, srcSlot);
				MaterialEditor.RemoveSetting(MEpluginCtrl, Coordinate, srcSlot);
				AccStateSync.RemoveSetting(ASSpluginCtrl, srcSlot);
				MoreAccessories.ResetPartsInfo(chaCtrl, Coordinate, srcSlot);
			}

			ChaCustom.CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);
			Singleton<ChaCustom.CustomBase>.Instance.updateCustomUI = true;
		}

		internal class QueueItem
		{
			public int srcSlot { get; set; }
			public int dstSlot { get; set; }
			public QueueItem(int src, int dst)
			{
				srcSlot = src;
				dstSlot = dst;
			}
		}

		internal static void ProcessQueue(List<QueueItem> Queue)
		{
			ChaControl chaCtrl = MakerAPI.GetCharacterControl();
			var MEpluginCtrl = MaterialEditor.GetController(chaCtrl);
			var HACpluginCtrl = HairAccessoryCustomizer.GetController(chaCtrl);
			var ASSpluginCtrl = AccStateSync.GetController(chaCtrl);

			HairAccessoryCustomizer.HairAccessoryInfos = new Dictionary<int, HairAccessoryCustomizer.HairAccessoryInfo>();
			int Coordinate = chaCtrl.fileStatus.coordinateType;

			foreach (QueueItem item in Queue)
			{
				HairAccessoryCustomizer.StoreSetting(chaCtrl, HACpluginCtrl, item.srcSlot); // need to do this before move PartsInfo
				MoreAccessories.ModifyPartsInfo(chaCtrl, Coordinate, item.srcSlot, item.dstSlot);
				MaterialEditor.ModifySetting(MEpluginCtrl, Coordinate, item.srcSlot, item.dstSlot);
				AccStateSync.ModifySetting(ASSpluginCtrl, Coordinate, item.srcSlot, item.dstSlot);
			}

			ChaCustom.CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);
			Singleton<ChaCustom.CustomBase>.Instance.updateCustomUI = true;

			foreach (QueueItem item in Queue)
				HairAccessoryCustomizer.ModifySetting(HACpluginCtrl, item.srcSlot, item.dstSlot); // need to do this after updateCustomUI
		}

		internal static bool IsHairAccessory(ChaControl chaCtrl, int slot)
		{
			try
			{
				var accessory = AccessoriesApi.GetAccessory(chaCtrl, slot);
				if (accessory == null)
					return false;
				return accessory.gameObject?.GetComponent<ChaCustomHairComponent>() != null;
			}
			catch
			{
				return false;
			}
		}
	}
}
