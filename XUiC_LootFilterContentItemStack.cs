using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using InControl;
using Platform;
using static XUiC_ItemStack;
using static LootFilter.XUiC_LootFilterContentGrid;
using System.Collections;

namespace LootFilter
{
	public class XUiC_LootFilterContentItemStack : XUiC_SelectableEntry
	{
		public static XUiC_LootFilterDragAndDropWindow dragAndDrop;
		protected LootFilterItemStack lfItemStack = LootFilterItemStack.Empty.Clone();
		public XUiEvent_LootFilterContentSlotChangedEventHandler LootFilterContentSlotChangedEvent;
		public static LootFilterItemStack movingcontent = null;
		public XUiC_LootFilterContentGrid LFGrid = null;
		public bool isOver;
		public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		public bool lastClicked;
		public Color32 selectionBorderColor;
		public int PickupSnapDistance = 4;

		public Color32 finalPressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		public Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);
		public Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);
		public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);
		public Color32 attributeLockColor = new Color32(48, 48, 48, byte.MaxValue);

		public XUiV_Sprite lockTypeIcon;

		public XUiV_Sprite itemIconSprite;

		public XUiV_Texture backgroundTexture;

		public bool flashLockTypeIcon;
		public StackLockTypes stackLockType;
		public bool attributeLock;
		public bool isDragAndDrop;
		public bool userLockedSlot;
		public int currentInterval = -1;
		public TweenScale tweenScale;
		public Vector3 startMousePos = Vector3.zero;

		public readonly CachedStringFormatterXuiRgbaColor itemiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();
		public readonly CachedStringFormatterInt itemcountFormatter = new CachedStringFormatterInt();
		public readonly CachedStringFormatterFloat durabilityFillFormatter = new CachedStringFormatterFloat();
		public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();
		public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();
		public readonly CachedStringFormatterXuiRgbaColor backgroundcolorFormatter = new CachedStringFormatterXuiRgbaColor();
		public readonly CachedStringFormatterXuiRgbaColor selectionbordercolorFormatter = new CachedStringFormatterXuiRgbaColor();

		public Color32 SelectionBorderColor
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return selectionBorderColor;
			}
			[PublicizedFrom(EAccessModifier.Protected)]
			set
			{
				if(!selectionBorderColor.ColorEquals(value))
				{
					selectionBorderColor = value;
					IsDirty = true;
				}
			}
		}
		public int SlotNumber { get; set; }
		public float HoverIconGrow;
		public bool IsDragAndDrop
		{
			get
			{
				return isDragAndDrop;
			}
			set
			{
				isDragAndDrop = value;
				if(value)
				{
					base.ViewComponent.EventOnPress = false;
					base.ViewComponent.EventOnHover = false;
				}
			}
		}
		public virtual bool CanSwap(ItemStack _stack)
		{
			return true;
		}
		public bool IsHolding { get; set; }
		public bool AllowIconGrow
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				if(lfItemStack != null)
					return lfItemStack.itemValue.ItemClass != null;
				return false;
			}
		}
		public XUiC_LootFilterContentInfoWindow InfoWindow { get; set; }
		public bool AllowDropping
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		} = true;
		public bool PrefixId
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}
		public ItemClass itemClassOrMissing
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return lfItemStack?.itemValue?.ItemClassOrMissing;
			}
		}
		public virtual string ItemIcon
		{
			get
			{
				if(lfItemStack.IsEmpty())
				{
					return "";
				}

				ItemClass itemClass = itemClassOrMissing;
				Block block = (itemClass as ItemClassBlock)?.GetBlock();
				if(block == null)
				{
					return lfItemStack.itemValue.GetPropertyOverride(ItemClass.PropCustomIcon, itemClass.GetIconName());
				}

				if(!block.SelectAlternates)
				{
					return lfItemStack.itemValue.GetPropertyOverride(ItemClass.PropCustomIcon, itemClass.GetIconName());
				}

				return block.GetAltBlockValue(lfItemStack.itemValue.Meta).Block.GetIconName();
			}
		}
		public virtual string ItemIconColor
		{
			get
			{
				ItemClass itemClass = itemClassOrMissing;
				if(itemClass == null)
				{
					return "255,255,255,0";
				}

				Color32 v = itemClass.GetIconTint(lfItemStack.itemValue);
				return itemiconcolorFormatter.Format(v);
			}
		}

		public override void Init()
		{
			base.Init();

			XUiController childById2 = GetChildById("itemIcon");
			if(childById2 != null)
			{
				itemIconSprite = childById2.ViewComponent as XUiV_Sprite;
			}

			XUiController childById3 = GetChildById("lockTypeIcon");
			if(childById3 != null)
			{
				lockTypeIcon = childById3.ViewComponent as XUiV_Sprite;
			}

			XUiController childById4 = GetChildById("backgroundTexture");
			if(childById4 != null)
			{
				backgroundTexture = childById4.ViewComponent as XUiV_Texture;
				if(backgroundTexture != null)
				{
					backgroundTexture.CreateMaterial();
				}
			}

			tweenScale = itemIconSprite.UiTransform.gameObject.AddComponent<TweenScale>();
			base.ViewComponent.UseSelectionBox = false;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if(_bindingName.Equals("backgroundcolor"))
			{
				if(lfItemStack.IsEmpty())
				{
					_value = backgroundcolorFormatter.Format(backgroundColor);
					return true;
				}
				if(lfItemStack.isDropItem)
				{
					_value = backgroundcolorFormatter.Format(new Color32(7, 197, 219, byte.MaxValue));
					return true;
				}
				if(lfItemStack.isScrapItem)
				{
					_value = backgroundcolorFormatter.Format(new Color32(191, 179, 49, byte.MaxValue));
					return true;
				}
				//_value = backgroundcolorFormatter.Format(AttributeLock ? attributeLockColor : backgroundColor);
				return false;
			}
			switch(_bindingName)
			{
				case "tooltip":
					_value = ItemNameText;
					return true;
		/*		case "isfavorite":
					_value = (ShowFavorites && IsFavorite).ToString();
					return true;*/
				case "itemicon":
					_value = ItemIcon;
					return true;
				case "iconcolor":
					_value = ItemIconColor;
					return true;
				case "itemcount":
					_value = "";
					if(!lfItemStack.IsEmpty())
					{
						if(ShowDurability)
						{
							_value = ((lfItemStack.itemValue.Quality > 0) ? itemcountFormatter.Format(lfItemStack.itemValue.Quality) : (lfItemStack.itemValue.IsMod ? "*" : ""));
						}
						else
						{
							_value = ((itemClassOrMissing.Stacknumber == 1) ? "" : itemcountFormatter.Format(lfItemStack.count));
						}
					}

					return true;
				case "hasdurability":
					_value = ShowDurability.ToString();
					return true;
				case "durabilitycolor":
					_value = durabilitycolorFormatter.Format(QualityInfo.GetQualityColor(lfItemStack?.itemValue.Quality ?? 0));
					return true;
				case "durabilityfill":
					_value = ((lfItemStack?.itemValue == null) ? "0.0" : durabilityFillFormatter.Format(lfItemStack.itemValue.PercentUsesLeft));
					return true;
				case "showicon":
					_value = (ItemIcon != "").ToString();
					return true;
				case "itemtypeicon":
					_value = "";
					if(!lfItemStack.IsEmpty())
					{
						ItemClass itemClass2 = itemClassOrMissing;
						if(itemClass2 != null)
						{
							if(itemClass2.IsBlock() && lfItemStack.itemValue.TextureAllSides == 0)
							{
								_value = Block.list[lfItemStack.itemValue.type].ItemTypeIcon;
							}
							else
							{
								if(itemClass2.AltItemTypeIcon != null && itemClass2.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, itemClass2, lfItemStack.itemValue))
								{
									_value = itemClass2.AltItemTypeIcon;
									return true;
								}

								_value = itemClass2.ItemTypeIcon;
							}
						}
					}

					return true;
				/*case "hasitemtypeicon":
					if(lfItemStack.IsEmpty() || !string.IsNullOrEmpty(lockSprite))
					{
						_value = "false";
					}
					else
					{
						ItemClass itemClass = itemClassOrMissing;
						if(itemClass == null)
						{
							_value = "false";
						}
						else
						{
							_value = (itemClass.IsBlock() ? (Block.list[lfItemStack.itemValue.type].ItemTypeIcon != "").ToString() : (itemClass.ItemTypeIcon != "").ToString());
						}
					}

					return true;*/
				case "itemtypeicontint":
					_value = "255,255,255,255";
					if(!lfItemStack.IsEmpty())
					{
						ItemClass itemClass3 = itemClassOrMissing;
						if(itemClass3 != null && itemClass3.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, itemClass3, lfItemStack.itemValue))
						{
							_value = altitemtypeiconcolorFormatter.Format(itemClass3.AltItemTypeIconColor);
						}
					}

					return true;
			/*	case "locktypeicon":
					_value = lockSprite ?? "";
					return true;*/
				case "isassemblelocked":
					_value = ((stackLockType != 0 && stackLockType != StackLockTypes.Hidden) || (attributeLock && lfItemStack.IsEmpty())).ToString();
					return true;
				case "stacklockicon":
					if(stackLockType == StackLockTypes.Quest)
					{
						_value = "ui_game_symbol_quest";
					}
					else if(attributeLock && lfItemStack.IsEmpty())
					{
						_value = "ui_game_symbol_pack_mule";
					}
					else
					{
						_value = "ui_game_symbol_lock";
					}

					return true;
				case "stacklockcolor":
					if(attributeLock && lfItemStack.IsEmpty())
					{
						_value = "200,200,200,64";
					}
					else
					{
						_value = "255,255,255,255";
					}

					return true;
				case "selectionbordercolor":
					_value = selectionbordercolorFormatter.Format(SelectionBorderColor);
					return true;
/*				case "backgroundcolor":
					_value = backgroundcolorFormatter.Format(AttributeLock ? attributeLockColor : backgroundColor);
					return true;*/
/*				case "userlockedslot":
					_value = UserLockedSlot.ToString();
					return true;*/
				default:
					return false;
			}
		}
		public string ItemNameText
		{
			get
			{
				if(lfItemStack.IsEmpty())
				{
					return "";
				}

				ItemClass itemClass = itemClassOrMissing;
				string text = itemClass.GetLocalizedItemName();
				if(itemClass.IsBlock())
				{
					text = Block.list[lfItemStack.itemValue.type].GetLocalizedBlockName(lfItemStack.itemValue);
				}

				if(!PrefixId)
				{
					return text;
				}

				int itemOrBlockId = lfItemStack.itemValue.GetItemOrBlockId();
				return $"{text}\n({itemOrBlockId}) {itemClass.Name}";
			}
		}
		public ItemClass itemClass
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return lfItemStack?.itemValue?.ItemClass;
			}
		}
		public bool ShowDurability
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return itemClass?.ShowQualityBar ?? false;
			}
		}

		public override void Update(float _dt)
		{
			//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e8: Invalid comparison between Unknown and I4
			//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fe: Invalid comparison between Unknown and I4
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Invalid comparison between Unknown and I4
			if(viewComponent != null && windowGroup != null && windowGroup.isShowing && viewComponent.IsVisible)
			{
				viewComponent.Update(_dt);
			}

			if(curInputStyle != lastInputStyle)
			{
				PlayerInputManager.InputStyle oldStyle = lastInputStyle;
				lastInputStyle = curInputStyle;
				RefreshBindings();
				InputStyleChanged(oldStyle, lastInputStyle);
			}

			for(int i = 0; i < children.Count; i++)
			{
				XUiController xUiController = children[i];
				if(!xUiController.IsDormant)
				{
					xUiController.Update(_dt);
				}
			}
			if(base.WindowGroup.isShowing)
			{
				PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
				CursorControllerAbs cursorController = base.xui.playerUI.CursorController;
				Vector3 vector = cursorController.GetScreenPosition();
				bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
				bool mouseButtonDown = cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
				bool mouseButton = cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
				bool mouseButtonUp2 = cursorController.GetMouseButtonUp(UICamera.MouseButton.RightButton);
				bool mouseButtonDown2 = cursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
				bool mouseButton2 = cursorController.GetMouseButton(UICamera.MouseButton.RightButton);
				if(!isDragAndDrop)
				{
					if(isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
					{
						if((int)((PlayerActionSet)gUIActions).LastInputType == 1)
						{
							Log.Out("PlayerActionSet");
							bool wasReleased = ((OneAxisInputControl)gUIActions.Submit).WasReleased;
							bool wasReleased2 = ((OneAxisInputControl)gUIActions.HalfStack).WasReleased;
							bool wasPressed = ((OneAxisInputControl)gUIActions.Inspect).WasPressed;
							bool wasReleased3 = ((OneAxisInputControl)gUIActions.RightStick).WasReleased;
							if(false)
							{
								Log.Out("SimpleClick");
								if(wasReleased)
								{
									HandleMoveToPreferredLocation();
								}
								else if(wasPressed)
								{
									Log.Out("HandleItemInspect");
									HandleItemInspect();
								}
							}
							else if(dragAndDrop.CurrentStack.IsEmpty() && !LFItemStack.IsEmpty())
							{
								if(wasReleased)
								{
									Log.Out("SwapItem1");
									SwapItem();
								}
								else if(wasPressed)
								{
									Log.Out("HandleItemInspect1");
									HandleItemInspect();
								}

							}
							else if(true)
							{
								if(wasReleased)
								{
									Log.Out("HandleStackSwap2");
									HandleStackSwap();
								}
								else if(wasReleased2 && AllowDropping)
								{
									Log.Out("HandleDropOne2");
									//HandleDropOne();
								}
							}
						}
						/*else if(SimpleClick && !StackLock)
						{
							if(mouseButtonUp)
							{
								//HandleMoveToPreferredLocation();
							}
						}*/
						else if(mouseButton || mouseButton2)
						{
							if(dragAndDrop.CurrentStack.IsEmpty() && !LFItemStack.IsEmpty())
							{
								if(!lastClicked)
								{
									startMousePos = vector;
								}
								else if(Mathf.Abs((vector - startMousePos).magnitude) > (float)PickupSnapDistance)
								{
									if(mouseButton)
									{
										Log.Out("SwapItem3");
										SwapItem();
									}
									else
									{
										Log.Out("HandlePartialStackPickup3");
										//HandlePartialStackPickup();
									}
								}
							}

							if(mouseButtonDown || mouseButtonDown2)
							{
								lastClicked = true;
							}
						}
						else if(mouseButtonUp)
						{
							if(dragAndDrop.CurrentStack.IsEmpty())
							{
								//this will trigger then item is clicked
								Log.Out("HandleItemInspect4");
								HandleItemInspect();
							}
							else if(lastClicked)
							{
								Log.Out("HandleStackSwap4");
								HandleStackSwap();
							}
						}
						else if(mouseButtonUp2)
						{
							if(lastClicked && AllowDropping)
							{
								//HandleDropOne();
							}
						}
						else
						{
							lastClicked = false;
						}
					}
					else
					{
						lastClicked = false;
						if((isOver || itemIconSprite.UiTransform.localScale != Vector3.one) && tweenScale.value != Vector3.one && !lfItemStack.IsEmpty())
						{
							tweenScale.from = Vector3.one * 1.5f;
							tweenScale.to = Vector3.one;
							tweenScale.enabled = true;
							tweenScale.duration = 0.5f;
						}
					}
				}
			}

			updateBorderColor();
			if(flashLockTypeIcon)
			{
				Color green = Color.green;
				float num = Mathf.PingPong(Time.time, 0.5f);
				/*setLockTypeIconColor(Color.Lerp(Color.grey, green, num * 4f));*/
			}

			if(IsDirty)
			{
				IsDirty = false;
				/*updateLockTypeIcon();*/
				RefreshBindings();
			}

			/*if(IsLocked && lockType != 0)
			{
				UpdateTimer(_dt);
			}*/
		}
		public LootFilterItemStack LFItemStack
		{
			get
			{
				return lfItemStack;
			}
			set
			{
				if(!lfItemStack.Equals(value))
				{
					setItemStack(value);
					if(lfItemStack.IsEmpty())
					{
						lfItemStack.Clear();
					}

					if(base.Selected)
					{
						updateItemInfoWindow(this);
					}

					HandleSlotChangeEvent();
					ItemClass itemClass = lfItemStack.itemValue.ItemClass;

					if(value.IsEmpty())
					{
						stackLockType = StackLockTypes.None;
					}

					if(backgroundTexture != null)
					{
						backgroundTexture.IsVisible = false;
						if(itemClass is ItemClassBlock itemClassBlock)
						{
							Block block = itemClassBlock.GetBlock();
							if(block.GetAutoShapeType() != 0)
							{
								int uiBackgroundTextureId = block.GetUiBackgroundTextureId(lfItemStack.itemValue.ToBlockValue(), BlockFace.Top);
								if(uiBackgroundTextureId != 0)
								{
									backgroundTexture.IsVisible = true;
									MeshDescription meshDescription = MeshDescription.meshes[0];
									UVRectTiling uVRectTiling = meshDescription.textureAtlas.uvMapping[uiBackgroundTextureId];
									Rect uv = uVRectTiling.uv;
									backgroundTexture.Texture = meshDescription.textureAtlas.diffuseTexture;
									if(meshDescription.bTextureArray)
									{
										backgroundTexture.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
										backgroundTexture.Material.SetFloat("_Index", uVRectTiling.index);
										backgroundTexture.Material.SetFloat("_Size", uVRectTiling.blockW);
									}
									else
									{
										backgroundTexture.UVRect = uv;
									}
								}
							}
						}

						if(((itemClass != null) ? itemClass.Actions[0] : null) is ItemActionTextureBlock itemActionTextureBlock)
						{
							if(lfItemStack.itemValue.Meta == 0)
							{
								lfItemStack.itemValue.Meta = itemActionTextureBlock.DefaultTextureID;
							}

							backgroundTexture.IsVisible = true;
							MeshDescription meshDescription2 = MeshDescription.meshes[0];
							int textureID = BlockTextureData.list[lfItemStack.itemValue.Meta].TextureID;
							Rect uVRect = ((textureID == 0) ? WorldConstants.uvRectZero : meshDescription2.textureAtlas.uvMapping[textureID].uv);
							backgroundTexture.Texture = meshDescription2.textureAtlas.diffuseTexture;
							if(meshDescription2.bTextureArray)
							{
								backgroundTexture.Material.SetTexture("_BumpMap", meshDescription2.textureAtlas.normalTexture);
								backgroundTexture.Material.SetFloat("_Index", meshDescription2.textureAtlas.uvMapping[textureID].index);
								backgroundTexture.Material.SetFloat("_Size", meshDescription2.textureAtlas.uvMapping[textureID].blockW);
							}
							else
							{
								backgroundTexture.UVRect = uVRect;
							}
						}
					}

					RefreshBindings();
					ResetTweenScale();
				}
				else
				{
					if(LFItemStack.IsEmpty() && backgroundTexture != null)
					{
						backgroundTexture.Texture = null;
					}

					if(base.Selected)
					{
						updateItemInfoWindow(this);
					}

					base.xui.playerUI.CursorController.HoverTarget = null;
				}

				viewComponent.IsSnappable = !lfItemStack.IsEmpty();
				IsDirty = true;
			}
		}
		public virtual void setItemStack(LootFilterItemStack _stack)
		{
			lfItemStack = _stack.Clone();
		}
		public virtual void updateItemInfoWindow(XUiC_LootFilterContentItemStack _itemStack)
		{
			InfoWindow.SetItemStack(_itemStack, _makeVisible: true);
		}
		public void HandleMoveToPreferredLocation()
		{
			base.xui.currentPopupMenu.ClearItems();
			if(LFItemStack.IsEmpty())
			{
				return;
			}

		}
		public void ResetTweenScale()
		{
			if(tweenScale != null && tweenScale.value != Vector3.one)
			{
				tweenScale.from = Vector3.one * 1.5f;
				tweenScale.to = Vector3.one;
				tweenScale.enabled = true;
				tweenScale.duration = 0.1f;
			}
		}
		public void HandleItemInspect()
		{
			if(!lfItemStack.IsEmpty() && InfoWindow != null)
			{
				base.Selected = true;
				//InfoWindow.SetMaxCountOnDirty = true;
				updateItemInfoWindow(this);
			}

			HandleClickComplete();
		}
		public virtual void HandleClickComplete()
		{
			lastClicked = false;
			if(itemIconSprite.UiTransform.localScale.x > 1f && !lfItemStack.IsEmpty())
			{
				tweenScale.from = Vector3.one * 1.5f;
				tweenScale.to = Vector3.one;
				tweenScale.enabled = true;
				tweenScale.duration = 0.5f;
			}
		}
		public override void OnHovered(bool _isOver)
		{
			isOver = _isOver;
			if(true)
			{
				if(_isOver)
				{
					if(InfoWindow != null && InfoWindow.ViewComponent.IsVisible)
					{
						InfoWindow.HoverEntry = this;
					}

					if(AllowIconGrow)
					{
						tweenScale.from = Vector3.one;
						tweenScale.to = Vector3.one * 1.5f;
						tweenScale.enabled = true;
						tweenScale.duration = 0.5f;
					}
				}
				else
				{
					if(InfoWindow != null && InfoWindow.ViewComponent.IsVisible)
					{
						InfoWindow.HoverEntry = null;
					}

					if(AllowIconGrow)
					{
						tweenScale.from = Vector3.one * 1.5f;
						tweenScale.to = Vector3.one;
						tweenScale.enabled = true;
						tweenScale.duration = 0.5f;
					}
				}
			}
			base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, LFItemStack, isOver, _canSwap: true);
			base.OnHovered(_isOver);
		}
		public virtual void SwapItem()
		{
			Log.Out("called SwapItem");
			base.xui.currentPopupMenu.ClearItems();			
			// CASE: PLACE
			if(lfItemStack.IsEmpty() && !dragAndDrop.CurrentStack.IsEmpty())
			{
				//just set the currentStack to the slot
				//PlayPlaceSound(currentStack);
				bool flag = base.Selected;
				LFItemStack = dragAndDrop.CurrentStack.Clone();
				if(!LFItemStack.IsEmpty())
				{
					base.Selected = flag;
				}				
				dragAndDrop.CurrentStack = LootFilterItemStack.Empty.Clone();
				LFItemStack.Index = this.LFGrid.Page * XUiC_LootFilterContentGrid.PageLength + SlotNumber;
				LFGrid.currentLootFilter.updateIndexOfItem(LFItemStack);
				LootFilterContentSlotChangedEvent.Invoke(LFItemStack.Index,LFItemStack);
			}
			else
			// CASE: PickUP
				if(!LFItemStack.IsEmpty() && dragAndDrop.CurrentStack.IsEmpty())
				{
					//PlayPickupSound();
					dragAndDrop.CurrentStack = lfItemStack.Clone();
					if(!dragAndDrop.CurrentStack.IsEmpty())
					{
						dragAndDrop.CurrentStack.Index = -1;
						LFGrid.currentLootFilter.updateIndexOfItem(dragAndDrop.CurrentStack);
						
					}
					bool flag = base.Selected;
					LFItemStack = LootFilterItemStack.Empty.Clone();
					base.Selected = flag;
					LFItemStack.Index = this.LFGrid.Page * XUiC_LootFilterContentGrid.PageLength + SlotNumber;
					LootFilterContentSlotChangedEvent.Invoke(LFItemStack.Index, LFItemStack);
				}
			else
			// CASE: SWAP
				if(!LFItemStack.IsEmpty() && !dragAndDrop.CurrentStack.IsEmpty())
				{
				//PlayPickupSound();
				LootFilterItemStack pickedUpItemStack = dragAndDrop.CurrentStack.Clone();
				dragAndDrop.CurrentStack = lfItemStack.Clone();
				if(!dragAndDrop.CurrentStack.IsEmpty())
				{
					dragAndDrop.CurrentStack.Index = -1;
					LFGrid.currentLootFilter.updateIndexOfItem(dragAndDrop.CurrentStack);
					//set the slot empty in the current loaded FilterContentList used by UI
					LootFilterContentSlotChangedEvent.Invoke(this.LFGrid.Page * XUiC_LootFilterContentGrid.PageLength + SlotNumber, LootFilterItemStack.Empty.Clone());
				}
				bool flag = base.Selected;
				LFItemStack = pickedUpItemStack.Clone();
				if(!pickedUpItemStack.IsEmpty())
				{
					base.Selected = flag;
					LFItemStack.Index = this.LFGrid.Page * XUiC_LootFilterContentGrid.PageLength + SlotNumber;
					LFGrid.currentLootFilter.updateIndexOfItem(LFItemStack);
					LootFilterContentSlotChangedEvent.Invoke(LFItemStack.Index, LFItemStack);
				}
			}
		}
		public void HandleSlotChangeEvent()
		{
			if(lfItemStack.IsEmpty() && base.Selected)
			{
				base.Selected = false;
			}

			//this.SlotChangedEvent?.Invoke(SlotNumber, itemStack);
		}
		public void updateBorderColor()
		{
			if(IsDragAndDrop)
			{
				SelectionBorderColor = Color.clear;
			}
			else if(base.Selected)
			{
				SelectionBorderColor = selectColor;
			}
			else if(isOver)
			{
				SelectionBorderColor = highlightColor;
			}
			else if(IsHolding)
			{
				SelectionBorderColor = holdingColor;
			}
			else
			{
				SelectionBorderColor = backgroundColor;
			}
		}
		public void HandleStackSwap()
		{
			base.xui.currentPopupMenu.ClearItems();
			if(!AllowDropping)
			{
				dragAndDrop.CurrentStack = LootFilterItemStack.Empty.Clone();
				//dragAndDrop.PickUpType = StackLocation;
			}

			bool flag = false;
			LootFilterItemStack currentStack = dragAndDrop.CurrentStack;
			ItemClass itemClass = currentStack.itemValue.ItemClassOrMissing;
			int num = 0;
			if(itemClass != null)
			/*{
				num = ((OverrideStackCount == -1) ? itemClass.Stacknumber.Value : Mathf.Min(itemClass.Stacknumber.Value, OverrideStackCount));
				if(!currentStack.IsEmpty() && this.itemStack.IsEmpty() && num < currentStack.count)
				{
					flag = true;
				}
			}*/

			if(!flag && (this.LFItemStack.IsEmpty() || currentStack.IsEmpty()))
			{
				SwapItem();
				base.Selected = false;
			}
			else if(!flag && (!this.LFItemStack.itemValue.ItemClassOrMissing.CanStack() || !itemClass.CanStack()))
			{
				SwapItem();
				base.Selected = false;
			}
			else if(currentStack.itemValue.type == this.LFItemStack.itemValue.type && !currentStack.itemValue.HasQuality && !this.LFItemStack.itemValue.HasQuality)
			{
				SwapItem();
				base.Selected = false;
					return;
				if(currentStack.count + this.LFItemStack.count > num)
				{
					
						int count = currentStack.count + this.LFItemStack.count - num;
					LootFilterItemStack itemStack = this.LFItemStack.Clone();
					itemStack.count = num;
					currentStack.count = count;
					dragAndDrop.CurrentStack = currentStack;
					//dragAndDrop.PickUpType = StackLocation;					
					LFItemStack = itemStack;
					//PlayPickupSound();
				}
				else
				{
					//No addition of stacks --> removed code:
					//LootFilterItemStack itemStack2 = this.LFItemStack.Clone();
					//itemStack2.count += currentStack.count;
					//this.LFItemStack = itemStack2;
					//base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
					//PlayPlaceSound();
				}

				if(base.Selected)
				{
					updateItemInfoWindow(this);
				}
			}
			else if(flag)
			{
					/*
				int count2 = currentStack.count - num;
				ItemStack itemStack3 = currentStack.Clone();
				itemStack3.count = num;
				currentStack.count = count2;
				base.xui.dragAndDrop.CurrentStack = currentStack;
				base.xui.dragAndDrop.PickUpType = StackLocation;
				ItemStack = itemStack3;
				PlayPickupSound();*/
			}
			else
			{
				SwapItem();
				base.Selected = false;
			}

			HandleClickComplete();
			//base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, ItemStack, isOver);
		}
	}
}
