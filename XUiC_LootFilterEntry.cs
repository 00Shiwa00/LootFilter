using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ParticleEffect;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace LootFilter
{

	public class XUiC_LootFilterEntry : XUiC_ListEntry<LootFilter>
	{
		public Boolean isDirty;
		public XUiV_Label lblName;
		public XUiV_Sprite icoRecipe;
		public XUiV_Sprite icoFavorite;
		public override void Init()
		{
			base.Init();
			for(int i = 0; i < children.Count; i++)
			{
				XUiView xUiView = children[i].ViewComponent;
				if(xUiView.ID.EqualsCaseInsensitive("name"))
				{
					lblName = xUiView as XUiV_Label;
				}
				else if(xUiView.ID.EqualsCaseInsensitive("icon"))
				{
					icoRecipe = xUiView as XUiV_Sprite;
				}
				else if(xUiView.ID.EqualsCaseInsensitive("favorite"))
				{
					icoFavorite = xUiView as XUiV_Sprite;
				}
				else if(xUiView.ID.EqualsCaseInsensitive("background"))
				{
					background = xUiView as XUiV_Sprite;
				}
			}
			isDirty = true;
			base.OnPress += OnPress_Act;
		}

		public override bool GetBindingValue(ref string value, string bindingName)
		{
			switch(bindingName)
			{
				case "lootFilterName":
					value = ((entryData != null) ? entryData.getName() : "");
					return true;
				case "isfavorite":
					value = ((entryData != null) ? entryData.isFavorite.ToString() : "false");
					return true;
				case "backgroundcolor":
					value = (entryData != null) ? entryData.isActive.ToString() : new CachedStringFormatterXuiRgbaColor().formatter(new Color32(Byte.MaxValue, Byte.MaxValue, Byte.MaxValue, Byte.MaxValue));
					return true;
				default:
					return false;
			}
		}
		public void OnPress_Act(XUiController _sender, int _mouseButton)
		{
			if(base.ViewComponent.Enabled)
			{
				entryData.isActive = !entryData.isActive;

				Log.Out(entryData.getName() + "  is active "+entryData.isActive.ToString());
				base.windowGroup.Controller.GetChildByType<XUiC_LootFilterContentGrid>().SetStacks(entryData.getFilteredItems());
				LootFilterItemStack DragAndDropItem = entryData.getDragAndDropItem();
				if(DragAndDropItem != null) 
					base.windowGroup.Controller.GetChildByType<XUiC_LootFilterContentGrid>().SetDragAndDropItem(DragAndDropItem);
				base.windowGroup.Controller.GetChildByType<XUiC_LootFilterContentGrid>().currentLootFilter = entryData;
				base.windowGroup.Controller.GetChildByType<XUiC_LootFilterContentInfoWindow>().SetLootFilter(this);
			}
		}
	}
}
