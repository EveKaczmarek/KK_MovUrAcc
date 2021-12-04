using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using ChaCustom;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using KKAPI;
using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
#if KK
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
#else
	[BepInProcess("KoikatsuSunshine")]
#endif
	[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
	[BepInDependency("madevil.JetPack", JetPack.Core.Version)]
	[BepInPlugin(GUID, Name, Version)]
	public partial class MovUrAcc : BaseUnityPlugin
	{
		public const string GUID = "madevil.kk.MovUrAcc";
#if DEBUG
		public const string Name = "MovUrAcc (Debug Build)";
#else
		public const string Name = "MovUrAcc";
#endif
		public const string Version = "2.1.1.0";

		internal static ManualLogSource _logger;
		internal static Harmony _hooksInstance;

		internal static bool IsDark;
		internal static bool btnLock = false;

		internal static object MEpluginCtrl;
		internal static object HACpluginCtrl;
		internal static object ASSpluginCtrl;
		internal static object MRpluginCtrl;
		internal static object DBEpluginCtrl;
		internal static object APKpluginCtrl;
		internal static object BUApluginCtrl;
		internal static object CApluginCtrl;

		internal static ChaControl _chaCtrl;
		internal static int _currentCoordinateIndex => _chaCtrl?.fileStatus?.coordinateType ?? -1;

		private void Start()
		{
			_logger = base.Logger;

#if KK && !DEBUG
			if (JetPack.MoreAccessories.BuggyBootleg)
			{
				_logger.LogError($"Could not load {Name} {Version} because it is incompatible with MoreAccessories experimental build");
				return;
			}
#endif
			if (!JetPack.MoreAccessories.Installed)
			{
#if KK
				if (JetPack.MoreAccessories.BuggyBootleg)
					_logger.LogError($"Backward compatibility in BuggyBootleg MoreAccessories is disabled");
				return;
#endif
			}

			IsDark = JetPack.Game.HasDarkness;

			MoreAccessories.InitSupport();

			MaterialEditor.InitSupport();
			HairAccessoryCustomizer.InitSupport();
			AccStateSync.InitSupport();
			MaterialRouter.InitSupport();
			DynamicBoneEditor.InitSupport();
			AAAPK.InitSupport();
			BendUrAcc.InitSupport();
			CharacterAccessory.InitSupport();

			MakerAPI.RegisterCustomSubCategories += (_sender, _ev) =>
			{
				_hooksInstance = Harmony.CreateAndPatchAll(typeof(Hooks));
				MaterialEditor.HookInit();
				AccStateSync.HookInit();
				AAAPK.HookInit();

				MakerCategory _category = new MakerCategory("05_ParameterTop", "tglMovUrAcc", MakerConstants.Parameter.Attribute.Position + 1, "MovUrAcc");
				_ev.AddSubCategory(_category);

				if (MoreAccessories.BuggyBootleg)
				{
					_ev.AddControl(new MakerText("MoreAccessories experimental build detected", _category, this) { TextColor = new Color(1, 0.7f, 0, 1) });
					_ev.AddControl(new MakerText("This is not meant for production use", _category, this));
					_ev.AddControl(new MakerSeparator(_category, this));
				}

				{
					CatBatchTransfer(_ev, _category);

					_ev.AddControl(new MakerSeparator(_category, this));
					CatBatchRemove(_ev, _category);

					if (!MoreAccessories.BuggyBootleg)
					{
						_ev.AddControl(new MakerSeparator(_category, this));
						CatParentSort(_ev, _category);
					}

					_ev.AddControl(new MakerSeparator(_category, this));
					CatPacking(_ev, _category);
					
					if (!MoreAccessories.BuggyBootleg)
					{
						_ev.AddControl(new MakerSeparator(_category, this));
						CatTrimMoreacc(_ev, _category);
					}
					else
					{
						_ev.AddControl(new MakerSeparator(_category, this));
						_ev.AddControl(new MakerText("Trim down unused slots is part of the experimental MoreAccessories feature", _category, this));
					}

					if (CharacterAccessory.Installed)
					{
						_ev.AddControl(new MakerSeparator(_category, this));
						CatRemoveCA(_ev, _category);
					}
				}
				btnLock = false;
			};

			MakerAPI.MakerFinishedLoading += (_sender, _ev) =>
			{
				_chaCtrl = CustomBase.Instance.chaCtrl;
				MEpluginCtrl = MaterialEditor.GetController(_chaCtrl);
				HACpluginCtrl = HairAccessoryCustomizer.GetController(_chaCtrl);
				ASSpluginCtrl = AccStateSync.GetController(_chaCtrl);
				MRpluginCtrl = MaterialRouter.GetController(_chaCtrl);
				DBEpluginCtrl = DynamicBoneEditor.GetController(_chaCtrl);
				APKpluginCtrl = AAAPK.GetController(_chaCtrl);
				BUApluginCtrl = BendUrAcc.GetController(_chaCtrl);
				CApluginCtrl = CharacterAccessory.GetController(_chaCtrl);
			};

			MakerAPI.MakerExiting += (_sender, _ev) =>
			{
				_chaCtrl = null;
				MEpluginCtrl = null;
				HACpluginCtrl = null;
				ASSpluginCtrl = null;
				MRpluginCtrl = null;
				DBEpluginCtrl = null;
				APKpluginCtrl = null;
				BUApluginCtrl = null;
				CApluginCtrl = null;

				_hooksInstance.UnpatchAll(_hooksInstance.Id);
				_hooksInstance = null;
			};
		}

		internal void CatTrimMoreacc(RegisterSubCategoriesEvent _ev, MakerCategory _category)
		{
			_ev.AddControl(new MakerText("Trim down unused MoreAccessories slots", _category, this));

			_ev.AddControl(new MakerButton("Go", _category, this)).OnClick.AddListener(delegate
			{
				MoreAccessories.TrimUnusedSlots();
			});
		}

		internal class QueueItem
		{
			public int srcSlot { get; set; }
			public int dstSlot { get; set; }
			public QueueItem(int _src, int _dst)
			{
				srcSlot = _src;
				dstSlot = _dst;
			}
		}

		internal class Hooks
		{
			internal static bool DuringLoading_Prefix()
			{
				return !btnLock;
			}

			internal static bool DuringLoading_Co_Prefix(ref IEnumerator __result)
			{
				if (btnLock)
				{
					IEnumerator _original = __result;
					__result = new[] { _original, Postfix() }.GetEnumerator();
					return false;
				}

				return true;

				IEnumerator Postfix()
				{
					yield break;
				}
			}
		}

		internal static void ProcessQueue(List<QueueItem> _queue)
		{
			HairAccessoryCustomizer.HairAccessoryInfos = new Dictionary<int, HairAccessoryCustomizer.HairAccessoryInfo>();

			int _coordinateIndex = _currentCoordinateIndex;

			foreach (QueueItem x in _queue)
			{
				_logger.LogDebug($"{x.srcSlot} -> {x.dstSlot}");
				HairAccessoryCustomizer.StoreSetting(_chaCtrl, HACpluginCtrl, x.srcSlot); // need to do this before move PartsInfo
				MoreAccessories.ModifyPartsInfo(_chaCtrl, _coordinateIndex, x.srcSlot, x.dstSlot);
				MaterialEditor.ModifySetting(MEpluginCtrl, _coordinateIndex, x.srcSlot, x.dstSlot);
				AccStateSync.ModifySetting(ASSpluginCtrl, _coordinateIndex, x.srcSlot, x.dstSlot);
				MaterialRouter.ModifySetting(MRpluginCtrl, _coordinateIndex, x.srcSlot, x.dstSlot);
				DynamicBoneEditor.ModifySetting(DBEpluginCtrl, _coordinateIndex, x.srcSlot, x.dstSlot);
				AAAPK.ModifySetting(APKpluginCtrl, _coordinateIndex, x.srcSlot, x.dstSlot);
				BendUrAcc.ModifySetting(BUApluginCtrl, _coordinateIndex, x.srcSlot, x.dstSlot);
			}

			_chaCtrl.ChangeCoordinateTypeAndReload(false);
			CustomBase.Instance.updateCustomUI = true;

			foreach (QueueItem x in _queue)
				HairAccessoryCustomizer.ModifySetting(HACpluginCtrl, x.srcSlot, x.dstSlot); // need to do this after updateCustomUI
		}

		internal static bool IsHairAccessory(ChaControl _chaCtrl, int _slotIndex)
		{
			return JetPack.Accessory.IsHairAccessory(_chaCtrl, _slotIndex);
		}
	}
}
