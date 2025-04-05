using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootFilter
{
	public class XUiC_LootFilterContentInfoWindow : XUiC_ItemInfoWindow
	{
		private XUiC_LootFilterContentItemStack selectedLootFilterItemStack;
		public new XUiC_SelectableEntry HoverEntry
		{
			get
			{
				return hoverEntry;
			}
			set
			{
				if(hoverEntry == value)
				{
					return;
				}

				hoverEntry = value;
				if(hoverEntry != null && !hoverEntry.Selected && !itemStack.IsEmpty())
				{
					ItemStack hoverControllerItemStack = GetHoverControllerItemStack();
					if(!hoverControllerItemStack.IsEmpty() && XUiM_ItemStack.CanCompare(hoverControllerItemStack.itemValue.ItemClass, itemClass))
					{
						CompareStack = hoverControllerItemStack;
					}
					else
					{
						CompareStack = ItemStack.Empty;
					}
				}
				else
				{
					CompareStack = ItemStack.Empty;
				}
			}
		}
		public new void ShowEmptyInfo()
		{
			if(emptyInfoWindow == null)
			{
				emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("lootfilter").GetChildById("emptyInfoPanel");
			}

			emptyInfoWindow.ViewComponent.IsVisible = true;
		}
		public override void Update(float _dt)
		{
			base.Update(_dt);
			if(IsDirty && base.ViewComponent.IsVisible)
			{
				if(emptyInfoWindow == null)
				{
					emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("lootfilter").GetChildById("emptyInfoPanel");
				}
				if(selectedLootFilterItemStack != null)
				{
					SetItemStack(selectedLootFilterItemStack);
				}
				else if(selectedItemStack != null)
				{
					SetItemStack(selectedItemStack);
				}
				else if(selectedEquipmentStack != null)
				{
					SetItemStack(selectedEquipmentStack);
				}
				else if(selectedPartStack != null)
				{
					SetItemStack(selectedPartStack);
				}
				else if(selectedTraderItemStack != null)
				{
					SetItemStack(selectedTraderItemStack);
				}
				else if(selectedTurnInItemStack != null)
				{
					SetItemStack(selectedTurnInItemStack);
				}

				IsDirty = false;
			}
		}
		public void SetItemStack(XUiC_LootFilterContentItemStack stack, bool _makeVisible = true)
		{
			if(stack == null || stack.LFItemStack == null || stack.LFItemStack.IsEmpty())
			{
				ShowEmptyInfo();
				return;
			}

			makeVisible(_makeVisible);
			selectedItemStack = null;
			selectedEquipmentStack = null;
			selectedPartStack = null;
			selectedTraderItemStack = null;
			selectedTurnInItemStack = null;
			selectedLootFilterItemStack = stack;
			SetInfo(stack.LFItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.None);
		}

		public new ItemStack GetHoverControllerItemStack()
		{
			if(hoverEntry is XUiC_LootFilterContentItemStack xUiC_LootFilterItemStack)
			{
				return xUiC_LootFilterItemStack.LFItemStack;
			}

			if(hoverEntry is XUiC_ItemStack xUiC_ItemStack)
			{
				return xUiC_ItemStack.ItemStack;
			}

			if(hoverEntry is XUiC_EquipmentStack xUiC_EquipmentStack)
			{
				return xUiC_EquipmentStack.ItemStack;
			}

			if(hoverEntry is XUiC_BasePartStack xUiC_BasePartStack)
			{
				return xUiC_BasePartStack.ItemStack;
			}

			if(hoverEntry is XUiC_TraderItemEntry xUiC_TraderItemEntry)
			{
				return xUiC_TraderItemEntry.Item;
			}

			if(hoverEntry is XUiC_QuestTurnInEntry xUiC_QuestTurnInEntry)
			{
				return xUiC_QuestTurnInEntry.Item;
			}

			return null;
		}
	}
}
