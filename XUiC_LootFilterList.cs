using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParticleEffect;

namespace LootFilter
{
	public class XUiC_LootFilterList : XUiC_List<LootFilter>
	{
		//public LootFilterManager LootFilterManager;
		

		public override void Init()
		{
			if(viewComponent != null)
			{
				viewComponent.InitView();
			}

			for(int i = 0; i < children.Count; i++)
			{
				children[i].Init();
			}

			curInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
			//windowGroup.Controller.GetChildByType<XUiC_CategoryList>().CategoryChanged += HandleCategoryChanged;
			pager = base.Parent.GetChildByType<XUiC_Paging>();
			if(pager != null)
			{
				pager.OnPageChanged += () =>
				{
					Page = pager.CurrentPageNumber;
				};
			}

			XUiController childById = GetChildById("lootfilters");
			if(childById != null)
			{
				Log.Out("found child");
				listEntryControllers = new XUiC_ListEntry<LootFilter>[childById.Children.Count];
				for(int i = 0; i < childById.Children.Count; i++)
				{
					listEntryControllers[i] = childById.Children[i] as XUiC_ListEntry<LootFilter>;
					if(listEntryControllers[i] != null)
					{
						listEntryControllers[i].OnScroll += HandleOnScroll;
						listEntryControllers[i].List = this;
						listEntryControllers[i].OnPress += HandleLootFilterOnPress;
					}
					else
					{
						Log.Warning("[XUi] List elements do not have the correct controller set (should be \"XUiC_ListEntry<" + typeof(LootFilter).FullName + ">\")");
					}
				}

				if(CursorControllable)
				{
					if(childById.ViewComponent is XUiV_Grid xUiV_Grid)
					{
						if(xUiV_Grid.Arrangement == UIGrid.Arrangement.Horizontal)
						{
							columns = xUiV_Grid.Columns;
							rows = PageLength / columns;
						}
						else
						{
							rows = xUiV_Grid.Rows;
							columns = PageLength / rows;
						}
					}

					if(childById.ViewComponent is XUiV_Table xUiV_Table)
					{
						columns = xUiV_Table.Columns;
						rows = PageLength / columns;
					}
				}
			}
			searchBox = windowGroup.Controller.GetChildById("searchInput") as XUiC_TextInput;
			if(searchBox != null)
			{
				searchBox.OnChangeHandler += OnSearchInputChanged;
				searchBox.OnSubmitHandler += OnSearchInputSubmit;
			}
			IsDirty = true;
			Page = 0;
			childById = base.Parent.Parent.GetChildById("btnAddLootFilter");
			if(childById != null) 
				childById.OnPress += HandleaddFilterToLootFilterPanel;
			LootFilterManager.LootFilterRemoved += OnLootFilterRemove;
			LootFilterManager.LootFilterChanged += updateSelectedEntry;
			//LootFilterManager = ((XUiC_LootFilterWindowGroup)parent.Parent.Parent.GetChildByType<XUiC_LootFilterWindowGroup>()).LootFilterManager;
			RebuildList(_resetFilter: true);
		}

		private void updateSelectedEntry(LootFilter lootFilter)
		{
			if(selectedEntry == null)
				return;
			if(this.SelectedEntry.entryData == null)
				return;
			if(this.SelectedEntry.entryData.getName() != lootFilter.getName())
				return;

			base.windowGroup.Controller.GetChildByType<XUiC_LootFilterContentGrid>().SetStacks(lootFilter.getFilteredItems());
		}
		private void HandleLootFilterOnPress(XUiController _sender, int _mouseButton)
		{

		}

		private void HandleaddFilterToLootFilterPanel(XUiController _sender, int _mouseButton)
		{
			LootFilterManager.CreateLootFilter();
			GetData();
			Update(0);
			RefreshView();
		}

		public void GetData()
		{
			allEntries.Clear();
			allEntries.AddRange(LootFilterManager.LootFilters);
			Log.Warning(allEntries.Count.ToString());
			filteredEntries = allEntries;
			Log.Warning(filteredEntries.Count.ToString());
			IsDirty = true;
			Page = 0;
		}

		public override void RefreshView(bool _resetFilter = false, bool _resetPage = true)
		{
			
			if(_resetFilter && searchBox != null)
			{
				searchBox.Text = "";
			}
			if(searchBox != null)
				OnSearchInputChanged(this, searchBox?.Text, _changeFromCode: true);
			if(_resetPage)
			{
				Page = 0;
			}
		}

		public override void OnOpen()
		{
			base.OnOpen();
			if(!allEntries.Equals(LootFilterManager.LootFilters)) { GetData(); }


		}
		public void OnLootFilterRemove(object sender, EventArgs e)
		{
			GetData();
		}
	}
}
