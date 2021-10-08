using KKAPI.Maker;
using KKAPI.Maker.UI;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal void CatBatchRemove(RegisterSubCategoriesEvent ev, MakerCategory category)
		{
			ev.AddControl(new MakerText("Batch remove accessory slots", category, this));

			MakerTextbox StartTextbox = ev.AddControl(new MakerTextbox(category, "Start", "", this));
			MakerTextbox EndTextbox = ev.AddControl(new MakerTextbox(category, "End", "", this));

			MakerRadioButtons tglMode = ev.AddControl(new MakerRadioButtons(category, this, "Mode", "All", "Hair", "Item"));

			MakerButton btnRmApply = ev.AddControl(new MakerButton("Go", category, this));
			btnRmApply.OnClick.AddListener(delegate
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
				ActBatchRemove(start - 1, end - 1, tglMode.Value);
			});
		}

		internal static void ActBatchRemove(int start, int end, int mode)
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

			ChaControl chaCtrl = ChaCustom.CustomBase.Instance.chaCtrl;

#if MoreAcc
			int nowAccCount = MoreAccessories.PluginInstance._charaMakerData.nowAccessories.Count;
#else
			int nowAccCount = chaCtrl.nowCoordinate.accessory.parts.Length;
#endif
			if (start < 0)
				start = 0;
#if MoreAcc
			if (end < 0)
				end = nowAccCount + 19;
			else if (end > nowAccCount + 19)
				end = nowAccCount + 19;
#else
			if (end < 0)
				end = nowAccCount - 1;
			else if (end >= nowAccCount)
				end = nowAccCount - 1;
#endif
			if (start > end)
			{
				Logger.LogMessage($"End value must be greater than start value");
				btnLock = false;
				return;
			}

			object MEpluginCtrl = MaterialEditor.GetController(chaCtrl);
			object HACpluginCtrl = HairAccessoryCustomizer.GetController(chaCtrl);
			object ASSpluginCtrl = AccStateSync.GetController(chaCtrl);
			object MRpluginCtrl = MaterialRouter.GetController(chaCtrl);
			object DBEpluginCtrl = DynamicBoneEditor.GetController(chaCtrl);
			object APKpluginCtrl = AAAPK.GetController(chaCtrl);
			object BUApluginCtrl = BendUrAcc.GetController(chaCtrl);
			int Coordinate = chaCtrl.fileStatus.coordinateType;

			for (int i = start; i <= end; i++)
			{
				ChaFileAccessory.PartsInfo part = MoreAccessories.GetPartsInfo(i);
				if (part.type == 120)
					continue;
				if (mode == 1 && !IsHairAccessory(chaCtrl, i))
					continue;
				else if (mode == 2 && IsHairAccessory(chaCtrl, i))
					continue;

				HairAccessoryCustomizer.RemoveSetting(HACpluginCtrl, i);
				MaterialEditor.RemoveSetting(MEpluginCtrl, i);
				AccStateSync.RemoveSetting(ASSpluginCtrl, Coordinate, i);
				MaterialRouter.RemoveSetting(MRpluginCtrl, Coordinate, i);
				DynamicBoneEditor.RemoveSetting(DBEpluginCtrl, Coordinate, i);
				AAAPK.RemoveSetting(APKpluginCtrl, Coordinate, i);
				BendUrAcc.RemoveSetting(BUApluginCtrl, Coordinate, i);
				MoreAccessories.ResetPartsInfo(chaCtrl, Coordinate, i);
			}

			AccStateSync.SyncVirtualGroupInfo(ASSpluginCtrl, Coordinate);

			btnLock = false;
			ChaCustom.CustomBase.Instance.chaCtrl.ChangeCoordinateTypeAndReload(false);
			ChaCustom.CustomBase.Instance.updateCustomUI = true;
		}
	}
}
