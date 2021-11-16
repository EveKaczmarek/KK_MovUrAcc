using System.Collections.Generic;
using System.Linq;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal void CatPacking(RegisterSubCategoriesEvent _ev, MakerCategory _category)
		{
			_ev.AddControl(new MakerText("Pack acc list by removing unused slots", _category, this));

			MakerButton _btnApply = _ev.AddControl(new MakerButton("Go", _category, this));
			_btnApply.OnClick.AddListener(delegate
			{
				ActPacking();
			});
		}

		internal static void ActPacking()
		{
			if (btnLock)
				return;
			btnLock = true;

			List<QueueItem> _queue = new List<QueueItem>();

			List<ChaFileAccessory.PartsInfo> _nowAccessories = JetPack.Accessory.ListNowAccessories(_chaCtrl);

			int _dstSlot = 0;

			for (int _srcSlot = 0; _srcSlot < _nowAccessories.Count; _srcSlot++)
			{
				ChaFileAccessory.PartsInfo part = _nowAccessories.ElementAtOrDefault(_srcSlot);
				if (part.type == 120)
					continue;

				if (_srcSlot != _dstSlot)
					_queue.Add(new QueueItem(_srcSlot, _dstSlot));

				_dstSlot++;
			}

			if (_queue.Count == 0)
			{
				_logger.LogMessage("Nothing to do");
				btnLock = false;
				return;
			}

			ProcessQueue(_queue);

			btnLock = false;
			_chaCtrl.ChangeCoordinateTypeAndReload(false);
		}
	}
}
