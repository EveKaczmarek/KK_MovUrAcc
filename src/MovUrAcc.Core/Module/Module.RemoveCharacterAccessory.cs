using System.Collections.Generic;

using ChaCustom;

using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal void CatRemoveCA(RegisterSubCategoriesEvent _ev, MakerCategory _category)
		{
			_ev.AddControl(new MakerText("Remove Character Accessory slots", _category, this));

			MakerButton btnPackSlots = _ev.AddControl(new MakerButton("Go", _category, this));
			btnPackSlots.OnClick.AddListener(delegate
			{
				ActRemoveCA();
			});
		}

		internal static void ActRemoveCA()
		{
			if (btnLock)
				return;

			Dictionary<int, ChaFileAccessory.PartsInfo> _parts = Traverse.Create(CApluginCtrl).Field("PartsInfo").GetValue<Dictionary<int, ChaFileAccessory.PartsInfo>>();
			if (_parts?.Count == 0) return;

			btnLock = true;

			List<ChaFileAccessory.PartsInfo> _nowAccessories = JetPack.Accessory.ListNowAccessories(_chaCtrl);

			foreach (KeyValuePair<int, ChaFileAccessory.PartsInfo> x in _parts)
			{
				int i = x.Key;
				if (i >= _nowAccessories.Count) break;
				if (x.Value.type != _nowAccessories[i].type) continue;
				if (x.Value.id != _nowAccessories[i].id) continue;

				HairAccessoryCustomizer.RemoveSetting(HACpluginCtrl, i);
				MaterialEditor.RemoveSetting(MEpluginCtrl, i);
				AccStateSync.RemoveSetting(ASSpluginCtrl, _currentCoordinateIndex, i);
				MaterialRouter.RemoveSetting(MRpluginCtrl, _currentCoordinateIndex, i);
				DynamicBoneEditor.RemoveSetting(DBEpluginCtrl, _currentCoordinateIndex, i);
				AAAPK.RemoveSetting(APKpluginCtrl, _currentCoordinateIndex, i);
				BendUrAcc.RemoveSetting(BUApluginCtrl, _currentCoordinateIndex, i);
				MoreAccessories.ResetPartsInfo(_chaCtrl, _currentCoordinateIndex, i);
			}

			btnLock = false;
			_chaCtrl.ChangeCoordinateTypeAndReload(false);
			CustomBase.Instance.updateCustomUI = true;
		}
	}
}
