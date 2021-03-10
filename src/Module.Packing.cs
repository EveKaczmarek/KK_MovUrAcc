using System.Collections.Generic;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal void CatPacking(RegisterSubCategoriesEvent ev, MakerCategory category)
		{
			ev.AddControl(new MakerText("Pack acc list by removing unused slots", category, this));

			MakerButton btnPackSlots = ev.AddControl(new MakerButton("Go", category, this));
			btnPackSlots.OnClick.AddListener(delegate
			{
				ActPacking();
			});
		}

		internal static void ActPacking()
		{
			if (btnLock)
				return;
			btnLock = true;

			List<QueueItem> Queue = new List<QueueItem>();
			int nowAccCount = MoreAccessories.PluginInstance._charaMakerData.nowAccessories.Count;
			int dstSlot = 0;

			for (int srcSlot = 0; srcSlot < (20 + nowAccCount); srcSlot++)
			{
				ChaFileAccessory.PartsInfo part = MoreAccessories.GetPartsInfo(srcSlot);
				if (part.type == 120)
					continue;

				if (srcSlot != dstSlot)
					Queue.Add(new QueueItem(srcSlot, dstSlot));

				dstSlot++;
			}

			if (Queue.Count == 0)
			{
				Logger.LogMessage("Nothing to do");
				btnLock = false;
				return;
			}

			ProcessQueue(Queue);

			btnLock = false;
		}
	}
}
