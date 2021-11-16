using System.Collections.Generic;
using System.Linq;

using ChaCustom;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal void CatBatchRemove(RegisterSubCategoriesEvent _ev, MakerCategory _category)
		{
			_ev.AddControl(new MakerText("Batch remove accessory slots", _category, this));

			MakerTextbox StartTextbox = _ev.AddControl(new MakerTextbox(_category, "Start", "", this));
			MakerTextbox EndTextbox = _ev.AddControl(new MakerTextbox(_category, "End", "", this));

			MakerRadioButtons _tglMode = _ev.AddControl(new MakerRadioButtons(_category, this, "Mode", "All", "Hair", "Item"));

			MakerButton _btnApply = _ev.AddControl(new MakerButton("Go", _category, this));
			_btnApply.OnClick.AddListener(delegate
			{
				if (!int.TryParse(StartTextbox.Value, out int _start))
				{
					StartTextbox.Value = "";
					_start = 0;
				}
				if (!int.TryParse(EndTextbox.Value, out int _end))
				{
					EndTextbox.Value = "";
					_end = 0;
				}
				ActBatchRemove(_start - 1, _end - 1, _tglMode.Value);
			});
		}

		internal static void ActBatchRemove(int _start, int _end, int _mode)
		{
			if (btnLock)
				return;
			btnLock = true;

			List<ChaFileAccessory.PartsInfo> _nowAccessories = JetPack.Accessory.ListNowAccessories(_chaCtrl);

			if (_start < 0)
				_start = 0;

			if (_end < 0 || _end >= _nowAccessories.Count)
				_end = _nowAccessories.Count - 1;

			if (_start > _end)
			{
				_logger.LogMessage($"End value must be greater than start value");
				btnLock = false;
				return;
			}

			for (int i = _start; i <= _end; i++)
			{
				ChaFileAccessory.PartsInfo _part = _nowAccessories.ElementAtOrDefault(i);
				if (_part.type == 120)
					continue;
				if (_mode == 1 && !IsHairAccessory(_chaCtrl, i))
					continue;
				else if (_mode == 2 && IsHairAccessory(_chaCtrl, i))
					continue;

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
