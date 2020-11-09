using System.Collections.Generic;
using System.Linq;

using BepInEx;
using HarmonyLib;

using KK_Plugins.MaterialEditor;
using static KK_Plugins.MaterialEditor.MaterialEditorCharaController;

namespace MovUrAcc
{
	public partial class MovUrAcc
	{
		internal static class MaterialEditor
		{
			internal static BaseUnityPlugin PluginInstance;

			internal static void InitSupport()
			{
				BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.materialeditor", out PluginInfo PluginInfo);
				PluginInstance = PluginInfo.Instance;
			}

			internal static MaterialEditorCharaController GetController(ChaControl chaCtrl)
			{
				return Traverse.Create(PluginInstance).Method("GetCharaController", new object[] { chaCtrl }).GetValue<MaterialEditorCharaController>();
			}

			internal static void ModifySetting(MaterialEditorCharaController pluginCtrl, int index, int srcSlot, int dstSlot)
			{
				List<RendererProperty> RendererPropertyList = Traverse.Create(pluginCtrl).Field("RendererPropertyList").GetValue<List<RendererProperty>>();
				RendererPropertyList.Where(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == srcSlot).ToList().ForEach(x => x.Slot = dstSlot);
				List<MaterialFloatProperty> MaterialFloatPropertyList = Traverse.Create(pluginCtrl).Field("MaterialFloatPropertyList").GetValue<List<MaterialFloatProperty>>();
				MaterialFloatPropertyList.Where(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == srcSlot).ToList().ForEach(x => x.Slot = dstSlot);
				List<MaterialColorProperty> MaterialColorPropertyList = Traverse.Create(pluginCtrl).Field("MaterialColorPropertyList").GetValue<List<MaterialColorProperty>>();
				MaterialColorPropertyList.Where(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == srcSlot).ToList().ForEach(x => x.Slot = dstSlot);
				List<MaterialTextureProperty> MaterialTexturePropertyList = Traverse.Create(pluginCtrl).Field("MaterialTexturePropertyList").GetValue<List<MaterialTextureProperty>>();
				MaterialTexturePropertyList.Where(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == srcSlot).ToList().ForEach(x => x.Slot = dstSlot);
				List<MaterialShader> MaterialShaderList = Traverse.Create(pluginCtrl).Field("MaterialShaderList").GetValue<List<MaterialShader>>();
				MaterialShaderList.Where(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == srcSlot).ToList().ForEach(x => x.Slot = dstSlot);
			}

			internal static void RemoveSetting(MaterialEditorCharaController pluginCtrl, int index, int slot)
			{
				List<RendererProperty> RendererPropertyList = Traverse.Create(pluginCtrl).Field("RendererPropertyList").GetValue<List<RendererProperty>>();
				RendererPropertyList.RemoveAll(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == slot);
				List<MaterialFloatProperty> MaterialFloatPropertyList = Traverse.Create(pluginCtrl).Field("MaterialFloatPropertyList").GetValue<List<MaterialFloatProperty>>();
				MaterialFloatPropertyList.RemoveAll(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == slot);
				List<MaterialColorProperty> MaterialColorPropertyList = Traverse.Create(pluginCtrl).Field("MaterialColorPropertyList").GetValue<List<MaterialColorProperty>>();
				MaterialColorPropertyList.RemoveAll(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == slot);
				List<MaterialTextureProperty> MaterialTexturePropertyList = Traverse.Create(pluginCtrl).Field("MaterialTexturePropertyList").GetValue<List<MaterialTextureProperty>>();
				MaterialTexturePropertyList.RemoveAll(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == slot);
				List<MaterialShader> MaterialShaderList = Traverse.Create(pluginCtrl).Field("MaterialShaderList").GetValue<List<MaterialShader>>();
				MaterialShaderList.RemoveAll(x => x.CoordinateIndex == index && x.ObjectType == ObjectType.Accessory && x.Slot == slot);
			}
		}
	}
}
