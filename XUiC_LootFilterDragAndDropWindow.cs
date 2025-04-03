using UnityEngine;

namespace LootFilter
{
	public class XUiC_LootFilterDragAndDropWindow : XUiController
	{
		public XUiC_LootFilterContentItemStack ItemStackControl;
		public LootFilterItemStack itemStack = LootFilterItemStack.Empty.Clone();
		public bool InMenu;
		public LootFilterItemStack CurrentStack
		{
			get
			{
				return itemStack;
			}
			set
			{
				itemStack = value;
				ItemStackControl.LFItemStack = value;
			}
		}
		public override void Init()
		{
			base.Init();
			ItemStackControl = GetChildByType<XUiC_LootFilterContentItemStack>();
			ItemStackControl.IsDragAndDrop = true;
			ItemStackControl.LFItemStack = LootFilterItemStack.Empty.Clone();
			base.ViewComponent.IsSnappable = false;
		}

		public override void Update(float _dt)
		{
			/*if(!InMenu)
			{
				//PlaceItemBackInInventory();
			}*/
			if(itemStack != null && !itemStack.IsEmpty())
			{
				((XUiV_Window)base.ViewComponent).Panel.alpha = 1f;
				Vector2 screenPosition = base.xui.playerUI.CursorController.GetScreenPosition();
				Vector3 position = base.xui.playerUI.camera.ScreenToWorldPoint(screenPosition);
				Transform transform = base.xui.transform;
				position.z = transform.position.z - 3f * transform.lossyScale.z;
				base.ViewComponent.UiTransform.position = position;
			}
			else
			{
				((XUiV_Window)base.ViewComponent).Panel.alpha = 0f;
			}

			base.Update(_dt);
		}

		public override void OnOpen()
		{
			base.OnOpen();
			/*if(entityPlayer is EntityPlayerLocal entityPlayerLocal && entityPlayerLocal.DragAndDropItem != ItemStack.Empty)
			{
				CurrentStack = entityPlayerLocal.DragAndDropItem;
				PlaceItemBackInInventory();
			}*/
		}

		public override void OnClose()
		{
			base.OnClose();
			//PlaceItemBackInInventory();
		}

		public void PlaceItemBackInInventory()
		{
			
		}

		public override bool ParseAttribute(string name, string value, XUiController _parent)
		{

			return base.ParseAttribute(name, value, _parent);
		}
	}
}
