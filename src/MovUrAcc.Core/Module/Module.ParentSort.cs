using System;
using System.Collections.Generic;
using System.Linq;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal void CatParentSort(RegisterSubCategoriesEvent _ev, MakerCategory _category)
		{
			_ev.AddControl(new MakerText("Sort by accessory parents", _category, this));

			MakerRadioButtons _tglMode = _ev.AddControl(new MakerRadioButtons(_category, this, "Mode", "All", "Parent", "Hair"));

			MakerToggle _tglIDSort = _ev.AddControl(new MakerToggle(_category, "Sort by item ID", false, this));

			MakerButton _btnApply = _ev.AddControl(new MakerButton("Go", _category, this));
			_btnApply.OnClick.AddListener(delegate
			{
				ActParentSort(_tglMode.Value, _tglIDSort.Value);
			});
		}

		internal static void ActParentSort(int _mode, bool _idSort)
		{
			if (btnLock)
				return;
			btnLock = true;

			List<ChaFileAccessory.PartsInfo> _nowAccessories = JetPack.Accessory.ListNowAccessories(_chaCtrl);

			if (_nowAccessories.Count == 0)
			{
				_logger.LogMessage("Nothing to do");
				btnLock = false;
				return;
			}

			int _dstSlot = -1;

			Dictionary<int, ChaFileAccessory.PartsInfo> _parts = new Dictionary<int, ChaFileAccessory.PartsInfo>();
			for (int i = 0; i < _nowAccessories.Count; i++)
			{
				ChaFileAccessory.PartsInfo part = _nowAccessories.ElementAtOrDefault(i);
				if (part.type == 120)
					continue;

				_parts[i] = part;
				_dstSlot = i;
			}
			List<int> _oldOrder = new List<int>(_parts.Select(x => x.Key));

			if (_idSort)
				_parts = _parts.OrderBy(x => x.Value.type).ThenBy(x => x.Value.id).ToDictionary(x => x.Key, x => x.Value);

			if (_mode == 1 || _mode == 0)
			{
				HashSet<string> _parentsUsed = new HashSet<string>(_parts.OrderBy(x => x.Value.parentKey).Select(x => x.Value.parentKey));
				HashSet<string> _parentsDefined = new HashSet<string>(Enum.GetNames(typeof(ChaAccessoryDefine.AccessoryParentKey)).Where(x => x.StartsWith("a_n_")));
				_parentsDefined.IntersectWith(_parentsUsed);
				List<string> _parentSorted = _parentsDefined.ToList();
				_parentSorted.AddRange(_parentsUsed.Where(x => !x.StartsWith("a_n_")));

				Dictionary<int, ChaFileAccessory.PartsInfo> _pool = new Dictionary<int, ChaFileAccessory.PartsInfo>();
				foreach (string _parent in _parentSorted)
				{
					foreach (KeyValuePair<int, ChaFileAccessory.PartsInfo> x in _parts)
					{
						if (x.Value.parentKey != _parent)
							continue;
						_pool.Add(x.Key, x.Value);
					}
				}
				_parts = _pool;
				//_pool.Clear();
			}

			if (_mode == 2 || _mode == 0)
			{
				Dictionary<int, ChaFileAccessory.PartsInfo> _partsHair = new Dictionary<int, ChaFileAccessory.PartsInfo>();
				Dictionary<int, ChaFileAccessory.PartsInfo> _partsItem = new Dictionary<int, ChaFileAccessory.PartsInfo>();
				foreach (KeyValuePair<int, ChaFileAccessory.PartsInfo> x in _parts)
				{
					if (IsHairAccessory(_chaCtrl, x.Key))
						_partsHair.Add(x.Key, x.Value);
					else
						_partsItem.Add(x.Key, x.Value);
				}
				_parts = _partsHair.Concat(_partsItem).ToDictionary(x => x.Key, x => x.Value);
			}

			bool _changed = false;
			int j = 0;
			List <QueueItem> _queue = new List<QueueItem>();
			foreach (KeyValuePair<int, ChaFileAccessory.PartsInfo> x in _parts)
			{
				if (x.Key != _oldOrder[j])
					_changed = true;
				j++;

				_dstSlot++;
				_queue.Add(new QueueItem(x.Key, _dstSlot));
			}

			if (!_changed)
			{
				_logger.LogMessage("Same order, nothing to do");
				btnLock = false;
				return;
			}

			for (int i = 0; i < (_dstSlot - (_nowAccessories.Count - 1)); i++)
				MoreAccessories.AddSlot();

			ProcessQueue(_queue);

			btnLock = false;
			_chaCtrl.ChangeCoordinateTypeAndReload(false);
		}
	}
}
