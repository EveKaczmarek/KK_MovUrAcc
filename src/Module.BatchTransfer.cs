using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal void CatBatchTransfer(RegisterSubCategoriesEvent ev, MakerCategory category)
		{
			ev.AddControl(new MakerText("Batch transfer accessory slots", category, this));

			MakerTextbox StartTextbox = ev.AddControl(new MakerTextbox(category, "Start", "", this));
			MakerTextbox EndTextbox = ev.AddControl(new MakerTextbox(category, "End", "", this));

			MakerTextbox NewStartTextbox = ev.AddControl(new MakerTextbox(category, "Shift first slot to", "", this));

			MakerRadioButtons tglMode = ev.AddControl(new MakerRadioButtons(category, this, "Mode", "All", "Hair", "Item"));

			MakerButton btnApply = ev.AddControl(new MakerButton("Go", category, this));
			btnApply.OnClick.AddListener(delegate
			{
				if (!int.TryParse(StartTextbox.Value, out int start))
				{
					StartTextbox.Value = "";
					start = 0;
				}
				if (!int.TryParse(EndTextbox.Value, out int end))
				{
					EndTextbox.Value = "";
					end = 0;
				}
				if (!int.TryParse(NewStartTextbox.Value, out int newstart))
				{
					NewStartTextbox.Value = "";
					newstart = 0;
				}
				ActBatchTransfer(start - 1, end - 1, newstart - 1, tglMode.Value);
			});
		}

		internal static void ActBatchTransfer(int start, int end, int newstart, int mode)
		{
			if (btnLock)
				return;
			btnLock = true;

			int nowAccCount = MoreAccessories.PluginInstance._charaMakerData.nowAccessories.Count;

			if (start < 0)
				start = 0;

			if (end < 0)
				end = nowAccCount + 19;
			else if (end > nowAccCount + 19)
				end = nowAccCount + 19;

			if (newstart < 0)
				newstart = 0;

			if (mode == 0 && start == newstart)
			{
				Logger.LogMessage($"Start and new start are the same, nothing to do");
				btnLock = false;
				return;
			}

			if (start > end)
			{
				Logger.LogMessage($"End value must be greater than start value");
				btnLock = false;
				return;
			}

			int amount = newstart - start;
			Logger.LogDebug($"[mode: {mode}][start: {start + 1:00}][end: {end + 1:00}][newstart: {newstart + 1:00}][amount: {amount}]");

			List<QueueItem> Queue = new List<QueueItem>();
			int newSlot = newstart;
			if (mode == 0)
			{
				for (int i = start; i <= end; i++)
				{
					newSlot = i + amount;
					Queue.Add(new QueueItem(i, newSlot));
				}
			}
			else
			{
				ChaControl chaCtrl = MakerAPI.GetCharacterControl();

				for (int i = start; i <= end; i++)
				{
					ChaFileAccessory.PartsInfo part = AccessoriesApi.GetPartsInfo(i);
					if (part.type == 120)
						continue;
					if (mode == 1 && !IsHairAccessory(chaCtrl, i))
						continue;
					else if (mode == 2 && IsHairAccessory(chaCtrl, i))
						continue;

					Queue.Add(new QueueItem(i, newSlot));
					newSlot++;
				}
				newSlot -= 1;
			}

			if (Queue.Count == 0)
			{
				Logger.LogMessage($"Nothing to do");
				btnLock = false;
				return;
			}

			if (amount > 0)
			{
				if (newSlot > 19)
				{
					Logger.LogDebug($"Expand MoreAccessories slots from {nowAccCount} to {end + amount - 19}");

					for (int i = 0; i < (newSlot - 19 - nowAccCount); i++)
						Traverse.Create(MoreAccessories.PluginInstance).Method("AddSlot").GetValue();
				}

				Queue = Queue.OrderByDescending(x => x.srcSlot).ToList();
			}

			ProcessQueue(Queue);

			btnLock = false;
		}
	}
}
