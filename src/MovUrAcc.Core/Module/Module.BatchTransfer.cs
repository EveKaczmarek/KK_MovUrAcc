using System.Collections.Generic;
using System.Linq;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal void CatBatchTransfer(RegisterSubCategoriesEvent _ev, MakerCategory _category)
		{
			_ev.AddControl(new MakerText("Batch transfer accessory slots", _category, this));

			MakerTextbox StartTextbox = _ev.AddControl(new MakerTextbox(_category, "Start", "", this));
			MakerTextbox EndTextbox = _ev.AddControl(new MakerTextbox(_category, "End", "", this));

			MakerTextbox NewStartTextbox = _ev.AddControl(new MakerTextbox(_category, "Shift first slot to", "", this));

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
				if (!int.TryParse(NewStartTextbox.Value, out int _newstart))
				{
					NewStartTextbox.Value = "";
					_newstart = 0;
				}
				ActBatchTransfer(_start - 1, _end - 1, _newstart - 1, _tglMode.Value);
			});
		}

		internal static void ActBatchTransfer(int _start, int _end, int _newstart, int _mode)
		{
			if (btnLock)
				return;
			btnLock = true;

			List<ChaFileAccessory.PartsInfo> _nowAccessories = JetPack.Accessory.ListNowAccessories(_chaCtrl);

			Dictionary<int, int> _parts = new Dictionary<int, int>();

			for (int i = 0; i < _nowAccessories.Count; i++)
			{
				ChaFileAccessory.PartsInfo _part = _nowAccessories.ElementAtOrDefault(i);
				if (_part.type == 120)
					continue;
				_parts[i] = _part.type;
			}

			if (_start < 0)
			{
				if (_parts.Count > 0)
					_start = _parts.First().Key;
			}

			if (_end < 0)
			{
				if (_parts.Count > 0)
					_end = _parts.Last().Key;
			}

			if (_newstart < 0)
				_newstart = 0;

			if (_mode == 0 && _start == _newstart)
			{
				_logger.LogMessage($"Start and new start are the same, nothing to do");
				btnLock = false;
				return;
			}

			if (_start > _end)
			{
				_logger.LogMessage($"End value must be greater than start value");
				btnLock = false;
				return;
			}

			int _amount = _newstart - _start;
			_logger.LogDebug($"[mode: {_mode}][start: {_start + 1:00}][end: {_end + 1:00}][newstart: {_newstart + 1:00}][amount: {_amount}]");

			List<QueueItem> _queue = new List<QueueItem>();
			int _newSlot = _newstart;
			if (_mode == 0)
			{
				for (int i = _start; i <= _end; i++)
				{
					_newSlot = i + _amount;
					_queue.Add(new QueueItem(i, _newSlot));
				}
			}
			else
			{
				for (int i = _start; i <= _end; i++)
				{
					ChaFileAccessory.PartsInfo _part = _nowAccessories.ElementAtOrDefault(i);
					if (_part.type == 120)
						continue;
					if (_mode == 1 && !IsHairAccessory(_chaCtrl, i))
						continue;
					else if (_mode == 2 && IsHairAccessory(_chaCtrl, i))
						continue;

					_queue.Add(new QueueItem(i, _newSlot));
					_newSlot++;
				}
				_newSlot -= 1;
			}

			if (_queue.Count == 0)
			{
				_logger.LogMessage($"Nothing to do");
				btnLock = false;
				return;
			}

			if (_amount > 0)
			{
				_queue = _queue.OrderByDescending(x => x.srcSlot).ToList();
				if (_queue.Any(x => x.dstSlot >= _nowAccessories.Count))
				{
					for (int i = 0; i < (_newSlot - (_nowAccessories.Count - 1)); i++)
						MoreAccessories.AddSlot();
				}
			}

			ProcessQueue(_queue);

			btnLock = false;
			_chaCtrl.ChangeCoordinateTypeAndReload(false);
		}
	}
}
