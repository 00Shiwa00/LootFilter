using GUI_2;
using Microsoft.Analytics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using UnityEngine;
using static ParticleEffect;

namespace LootFilter
{
	public class XUiC_LootFilterContentGrid : XUiController
	{
		public bool wasHoldingItem;
		public XUiC_LootFilterContentItemStack[] itemControllers;
		
		public LootFilter currentLootFilter = null;
		public XUiC_Paging pager = null;
		public int page;
		public static int PageLength = 45;
		public LootFilterItemStack[] itemStacks = LootFilterItemStack.CreateArray(PageLength);
		public delegate void PageNumberChangedHandler();
		public event PageNumberChangedHandler PageNumberChanged;

		public delegate void XUiEvent_LootFilterContentSlotChangedEventHandler(int slotNumber, LootFilterItemStack newLootFilterItemStack);
		public event XUiEvent_LootFilterContentSlotChangedEventHandler LootFilterUIContentChanged;
		public bool bAwakeCalled;

		public int Page
		{
			get
			{
				return page;
			}
			set
			{
				int num = Math.Max(0, Math.Min(value, LastPage));
				if(num != page)
				{
					page = num;
					pager?.SetPage(page);
					IsDirty = true;
				//	SelectedEntry = null;
					this.PageNumberChanged?.Invoke();
				}
			}
		}
		
		public void updatePageLabel()
		{
			if(pager != null)
			{
				pager.CurrentPageNumber = page;
				pager.LastPageNumber = LastPage;
			}
		}
		public int LastPage => Math.Max(0, Mathf.CeilToInt((float)itemStacks.Length / (float)(PageLength))-1);
		XUiC_LootFilterDragAndDropWindow dragAndDrop;
		public override void Init()
		{
			base.Init();
			
			dragAndDrop = base.xui.WindowGroups.Find(x => x.ID == "lootfilterdraganddrop").Controller.GetChildByType<XUiC_LootFilterDragAndDropWindow>();
			Log.Out((dragAndDrop == null).ToString());
			XUiC_LootFilterContentItemStack.dragAndDrop = dragAndDrop;
			/*wasHoldingItem = !dragAndDrop.CurrentStack.IsEmpty();
			UpdateCallouts(wasHoldingItem);
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);*/
			itemControllers = GetChildrenByType<XUiC_LootFilterContentItemStack>();
			bAwakeCalled = true;
			IsDirty = false;
			IsDormant = true;
			pager = base.Parent.GetChildByType<XUiC_Paging>();
			if(pager != null)
			{
				pager.OnPageChanged += () =>
				{
					Page = pager.CurrentPageNumber;
				};
			}
			this.PageNumberChanged += updateCurrentPageContents;
			this.LootFilterUIContentChanged += HandleSlotChangedEvent;
			//pager.OnPageChanged += updateCurrentPageContents;

			var infoWindow = this.WindowGroup.Controller.GetChildByType<XUiC_LootFilterContentInfoWindow>();

			itemControllers = GetChildrenByType<XUiC_LootFilterContentItemStack>();
			foreach(XUiC_LootFilterContentItemStack c in itemControllers)
			{
				c.OnScroll += HandleOnScroll;
				c.LootFilterContentSlotChangedEvent += HandleSlotChangedEvent;
				c.LFGrid = this;
				c.InfoWindow = infoWindow;
			}
			base.OnScroll += HandleOnScroll;
			XUiController childById = GetChildById("lootFilterContents");
			if(childById.ViewComponent is XUiV_Grid xUiV_Grid)
			{
				if(xUiV_Grid.Arrangement == UIGrid.Arrangement.Horizontal)
				{
					PageLength = xUiV_Grid.Columns * xUiV_Grid.Rows;
				}
				else
				{
					PageLength = xUiV_Grid.Columns * xUiV_Grid.Rows;
				}
			}

		}
		public virtual void HandleSlotChangedEvent(int slotNumber, LootFilterItemStack newLootFilterItemStack)
		{
			Log.Warning("HandleSlotChangedEvent");
			if(slotNumber > -1)
				this.itemStacks[slotNumber] = newLootFilterItemStack;

			//iterate through all UI Content on current Page
			/*int num = PageLength * Page;
			for(int i = 0; i < num; i++)
			{
				XUiC_LootFilterContentItemStack xUiC_ListEntry = itemControllers[i];
				if(xUiC_ListEntry == null)
				{
					Log.Error("listEntry is null! {0} items in listEntryControllers", itemControllers.Length);
					break;
				}
				xUiC_ListEntry.itemClass
			}*/
		}
		public virtual LootFilterItemStack[] getUISlots()
		{
			LootFilterItemStack[] array = new LootFilterItemStack[itemControllers.Length];
			for(int i = 0; i < itemControllers.Length; i++)
			{
				array[i] = itemControllers[i].LFItemStack.Clone();
			}

			return array;
		}
		public void RefreshBackpackSlots()
		{
			SetStacks(GetSlots());
			IsDirty = true;
		}
		public LootFilterItemStack[] GetSlots()
		{
			return this.itemStacks;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void UpdateBackend(ItemStack[] stackList)
		{
			//change in LootFilter?
		}

		public override void Update(float _dt)
		{			
			base.xui.playerUI.entityPlayer.AimingGun = false;
			if(base.ViewComponent.IsVisible)
			{
				bool flag = !dragAndDrop.CurrentStack.IsEmpty();
				if(flag != wasHoldingItem)
				{
					wasHoldingItem = flag;
					UpdateCallouts(flag);
				}
			}
			if(page > LastPage)
			{
				Page = LastPage;
			}
			if(IsDirty)
			{
				updateCurrentPageContents();
				updatePageLabel();
			}
			//this.PageContentsChanged?.Invoke();
			
			base.Update(_dt);
			RefreshBindings();
			IsDirty = false;
		}
		private void updateCurrentPageContents()
		{
			XUiC_LootFilterContentInfoWindow childByType = windowGroup.xui.GetChildByType<XUiC_LootFilterContentInfoWindow>();
			for(int i = 0; i < PageLength; i++)
			{
				int num = i + PageLength * page;
				XUiC_LootFilterContentItemStack xUiC_ListEntry = itemControllers[i];
				if(xUiC_ListEntry == null)
				{
					Log.Error("listEntry is null! {0} items in listEntryControllers", itemControllers.Length);
					break;
				}

				if(num < itemStacks.Count())
				{
					if(!itemStacks[num].IsEmpty())
						Log.Out("set: " + itemStacks[num].itemValue.ItemClass.Name);
					XUiC_LootFilterContentItemStack xUiC_ItemStack = itemControllers[i];
				//	xUiC_ItemStack.SlotChangedEvent -= handleSlotChangedDelegate;
				//	xUiC_ItemStack.SlotChangedEvent += handleSlotChangedDelegate;
					xUiC_ItemStack.SlotNumber = i;
					//xUiC_ItemStack.InfoWindow = childByType;
					xUiC_ItemStack.LFItemStack = itemStacks[num].Clone();
				}
				else
				{

					XUiC_LootFilterContentItemStack xUiC_ItemStack = itemControllers[i];
					//xUiC_ItemStack.SlotChangedEvent -= handleSlotChangedDelegate;
					xUiC_ItemStack.SlotNumber = i;
					//xUiC_ItemStack.InfoWindow = childByType;
					xUiC_ItemStack.LFItemStack = LootFilterItemStack.Empty;
					if(xUiC_ListEntry.Selected)
					{
						xUiC_ListEntry.Selected = false;
					}
				}

				/*if(!updateSelectedItemByIndex && CurrentSelectedEntry != null && CurrentSelectedEntry == xUiC_ListEntry.GetEntry() && SelectedEntry != xUiC_ListEntry)
				{
					SelectedEntry = xUiC_ListEntry;
				}*/
			}
		}


		public void UpdateCallouts(bool _holdingItem)
		{
			/*string action = string.Format(Localization.Get("igcoItemActions"), InControlExtensions.GetBlankDPadSourceString());
			XUiC_GamepadCalloutWindow calloutWindow = base.xui.calloutWindow;
			if(calloutWindow != null)
			{
				calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
				if(!_holdingItem)
				{
					calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoInspect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
					calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, action, XUiC_GamepadCalloutWindow.CalloutType.Menu);
					calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
				}

				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			}*/
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void SetStacks(LootFilterItemStack[] stackList)
		{
			Log.Out("set stacks");
			if(stackList == null)
			{
				return;
			}
			this.itemStacks = stackList;

			//maxSlotCount = base.xui.playerUI.entityPlayer.bag.MaxItemCount;
			XUiC_LootFilterContentInfoWindow childByType = base.xui.GetChildByType<XUiC_LootFilterContentInfoWindow>();
			int num = 0;
			Log.Out(itemControllers.Length.ToString());
			for(int i = 0; i < stackList.Length && itemControllers.Length > i && stackList.Length > i; i++)
			{
				if(!stackList[i].IsEmpty())
					Log.Out("set: " + stackList[i].itemValue.ItemClass.Name);
				num = i+1;
				XUiC_LootFilterContentItemStack xUiC_ItemStack = itemControllers[i];
				//xUiC_ItemStack.SlotChangedEvent -= handleSlotChangedDelegate;
				//xUiC_ItemStack.SlotChangedEvent += handleSlotChangedDelegate;
				xUiC_ItemStack.SlotNumber = i;
				xUiC_ItemStack.InfoWindow = childByType;
				xUiC_ItemStack.LFItemStack = stackList[i].Clone();
			}
			for(int i = num; i < itemControllers.Length; i++)
			{
				XUiC_LootFilterContentItemStack xUiC_ItemStack = itemControllers[i];
				//xUiC_ItemStack.SlotChangedEvent -= handleSlotChangedDelegate;
				xUiC_ItemStack.SlotNumber = i;
				xUiC_ItemStack.InfoWindow = childByType;
				xUiC_ItemStack.LFItemStack = LootFilterItemStack.Empty;
			}
			IsDirty = true;
			updatePageLabel();
		}
		internal void SetDragAndDropItem(LootFilterItemStack dragAndDropItem)
		{
			base.xui.currentPopupMenu.ClearItems();
			this.dragAndDrop.CurrentStack = dragAndDropItem;
		}
		public void HandleOnScroll(XUiController _sender, float _delta)
		{
			if(_delta > 0f)
			{
				pager?.PageDown();
			}
			else
			{
				pager?.PageUp();
			}
		}
		public virtual void ClearHoveredItems()
		{
			for(int i = 0; i < itemControllers.Length; i++)
			{
				itemControllers[i].Hovered(_isOver: false);
			}
		}
		public override void OnOpen()
		{
			SetStacks(GetSlots());
			if(base.ViewComponent != null && !base.ViewComponent.IsVisible)
			{
				base.ViewComponent.IsVisible = true;
			}

			IsDirty = true;
			IsDormant = false;
		}

		public override void OnClose()
		{
			ClearHoveredItems();
			if(base.ViewComponent != null && base.ViewComponent.IsVisible)
			{
				base.ViewComponent.IsVisible = false;
			}

			IsDormant = true;
		}


	}
}
