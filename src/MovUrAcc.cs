using System.Collections.Generic;
using System.Linq;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	[BepInPlugin(GUID, PluginName, Version)]
	[BepInDependency("marco.kkapi")]
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
	public partial class MovUrAcc : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.MovUrAcc";
		public const string PluginName = "MovUrAcc";
		public const string Version = "1.5.0.0";

		internal static new ManualLogSource Logger;
		internal static bool IsDark;
		internal static bool btnLock = false;

		private void Start()
		{
			Logger = base.Logger;

			IsDark = typeof(ChaControl).GetProperties(AccessTools.all).Any(x => x.Name == "exType");

			MoreAccessories.InitSupport();
			MaterialEditor.InitSupport();
			HairAccessoryCustomizer.InitSupport();
			AccStateSync.InitSupport();
			MaterialRouter.InitSupport();
			DynamicBoneEditor.InitSupport();

			MakerAPI.RegisterCustomSubCategories += (object sender, RegisterSubCategoriesEvent ev) =>
			{
				MakerCategory category = new MakerCategory("05_ParameterTop", "tglMovUrAcc", MakerConstants.Parameter.Attribute.Position + 1, "MovUrAcc");

				CatBatchTransfer(ev, category);
				ev.AddControl(new MakerSeparator(category, this));

				CatBatchRemove(ev, category);
				ev.AddControl(new MakerSeparator(category, this));

				CatPacking(ev, category);
				ev.AddControl(new MakerSeparator(category, this));

				CatTrimMoreacc(ev, category);

				ev.AddSubCategory(category);

				btnLock = false;
			};
		}

		internal void CatTrimMoreacc(RegisterSubCategoriesEvent ev, MakerCategory category)
		{
			ev.AddControl(new MakerText("Trim down unused MoreAccessories slots", category, this));

			MakerButton btnMoreAccApply = ev.AddControl(new MakerButton("Go", category, this));
			btnMoreAccApply.OnClick.AddListener(delegate
			{
				MoreAccessories.TrimUnusedSlots();
			});
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
			object MEpluginCtrl = MaterialEditor.GetController(chaCtrl);
			object HACpluginCtrl = HairAccessoryCustomizer.GetController(chaCtrl);
			object ASSpluginCtrl = AccStateSync.GetController(chaCtrl);
			object MRpluginCtrl = MaterialRouter.GetController(chaCtrl);
			object DBEpluginCtrl = DynamicBoneEditor.GetController(chaCtrl);

			HairAccessoryCustomizer.HairAccessoryInfos = new Dictionary<int, HairAccessoryCustomizer.HairAccessoryInfo>();
			int Coordinate = chaCtrl.fileStatus.coordinateType;

			foreach (QueueItem item in Queue)
			{
				Logger.LogDebug($"{item.srcSlot} -> {item.dstSlot}");
				HairAccessoryCustomizer.StoreSetting(chaCtrl, HACpluginCtrl, item.srcSlot); // need to do this before move PartsInfo
				MoreAccessories.ModifyPartsInfo(chaCtrl, Coordinate, item.srcSlot, item.dstSlot);
				MaterialEditor.ModifySetting(MEpluginCtrl, Coordinate, item.srcSlot, item.dstSlot);
				AccStateSync.ModifySetting(ASSpluginCtrl, Coordinate, item.srcSlot, item.dstSlot);
				MaterialRouter.ModifySetting(MRpluginCtrl, Coordinate, item.srcSlot, item.dstSlot);
				DynamicBoneEditor.ModifySetting(DBEpluginCtrl, Coordinate, item.srcSlot, item.dstSlot);
			}

			ChaCustom.CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);
			ChaCustom.CustomBase.Instance.updateCustomUI = true;

			foreach (QueueItem item in Queue)
				HairAccessoryCustomizer.ModifySetting(HACpluginCtrl, item.srcSlot, item.dstSlot); // need to do this after updateCustomUI
		}

		internal static bool IsHairAccessory(ChaControl chaCtrl, int slot)
		{
			try
			{
				ChaAccessoryComponent accessory = chaCtrl.GetAccessoryObject(slot)?.GetComponent<ChaAccessoryComponent>();
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
