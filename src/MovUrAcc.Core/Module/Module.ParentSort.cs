using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal void CatParentSort(RegisterSubCategoriesEvent ev, MakerCategory category)
		{
			ev.AddControl(new MakerText("Sort by accessory parents", category, this));

			MakerButton btnApply = ev.AddControl(new MakerButton("Go", category, this));
			btnApply.OnClick.AddListener(delegate
			{
				ActParentSort();
			});
		}

		internal static void ActParentSort()
		{
#if !DEBUG && MoreAcc
			if (MoreAccessories.BuggyBootlegCheck())
			{
				Logger.LogMessage($"The card is not supported because its accessory data has been altered by a buggy plugin");
				return;
			}
#endif
			if (btnLock)
				return;
			btnLock = true;
#if MoreAcc
			int nowAccCount = MoreAccessories.PluginInstance._charaMakerData.nowAccessories.Count;
#else
			int nowAccCount = 0;
#endif
			int dstSlot = -1;

			Dictionary<int, string> parts = new Dictionary<int, string>();

			for (int i = 0; i < (20 + nowAccCount); i++)
			{
				ChaFileAccessory.PartsInfo part = MoreAccessories.GetPartsInfo(i);
				if (part.type == 120)
					continue;

				parts[i] = part.parentKey;
				dstSlot = i;
			}

			if (parts.Count == 0)
			{
				Logger.LogMessage("Nothing to do");
				btnLock = false;
				return;
			}

			dstSlot++;

			HashSet<string> parentsUsed = new HashSet<string>(parts.OrderBy(x => x.Value).Select(x => x.Value));
			HashSet<string> parentsDefined = new HashSet<string>(Enum.GetNames(typeof(ChaAccessoryDefine.AccessoryParentKey)).Where(x => x.StartsWith("a_n_")));
			parentsDefined.IntersectWith(parentsUsed);
			List<string> parentSorted = parentsDefined.ToList();
			parentSorted.AddRange(parentsUsed.Where(x => !x.StartsWith("a_n_")));

			List<QueueItem> Queue = new List<QueueItem>();
			bool changed = false;
			int max = -1;

			foreach (string parent in parentSorted)
			{
				foreach (KeyValuePair<int, string> part in parts)
				{
					if (part.Value != parent)
						continue;

					if (max > part.Key)
						changed = true;
					max = part.Key;

					Queue.Add(new QueueItem(part.Key, dstSlot));
					dstSlot++;
				}
			}

			if (!changed)
			{
				Logger.LogMessage("Same order, nothing to do");
				btnLock = false;
				return;
			}

			if (dstSlot - 19 > nowAccCount)
			{
				for (int i = 1; i < (dstSlot - 19 - nowAccCount); i++)
					Traverse.Create(MoreAccessories.PluginInstance).Method("AddSlot").GetValue();
			}

			ProcessQueue(Queue);

			btnLock = false;
			ChaCustom.CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);
		}
	}
}
