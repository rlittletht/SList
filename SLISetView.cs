using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SList
{
	public class SLISetView
	{
		public List<SLItem> Items { get; private set; }
		public ListView LvControl { get; private set; }
		public SLISetViewItemComparer Comparer { get; set; }

		public int Count => Items.Count;

		public SLISetView(ListView lvControl)
		{
			LvControl = lvControl;
			LvControl.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(RetrieveVirtualItem);
			Items = new List<SLItem>();
			Comparer = new SLISetViewItemComparer(2);
		}

		static ListViewItem LviCreateForSli(SLItem sli, bool fChecked)
		{
			ListViewItem lvi = new ListViewItem();

			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

			lvi.Tag = sli;
			lvi.SubItems[3].Text = sli.Path;
			lvi.SubItems[2].Text = sli.Size.ToString("###,###,###");
			lvi.SubItems[1].Text = Path.GetExtension(sli.Name);
			lvi.SubItems[0].Text = sli.Name;

			if (fChecked)
				lvi.Checked = true;

			return lvi;
		}

		public void Clear()
		{
			Items.Clear();
			LvControl.VirtualListSize = 0;
		}

		public void Update()
		{
			LvControl.Update();
		}

		public void Sort()
		{
			// get the sorter
			Items.Sort(Comparer);
			LvControl.RedrawItems(0, Items.Count - 1, true);
		}

		public bool Visible
		{
			get => LvControl.Visible;
			set => LvControl.Visible = value;
		}

		int GetItemIndex(SLItem sli)
		{
			for (int i = 0; i < Items.Count; i++)
				if (Items[i] == sli)
					return i;

			return -1;
		}

		public void UpdateMark(SLItem sli)
		{
			int i = GetItemIndex(sli);

			Check(i, sli.IsMarked);
		}

		public void Select(SLItem sli)
		{
			int i = GetItemIndex(sli);

			LvControl.Items[i].Selected = true;
			LvControl.Select();
		}

		public SLItem SelectedItem()
		{
			if (LvControl.SelectedIndices.Count == 0)
				return null;

			return Items[LvControl.SelectedIndices[0]];
		}

		public IComparer ItemSorter
		{
			get => LvControl.ListViewItemSorter;
			set => LvControl.ListViewItemSorter = value;
		}

		public void Remove(int i)
		{
			Items.RemoveAt(i);
			LvControl.VirtualListSize--;
		}

		public void BeginUpdate()
		{
			LvControl.BeginUpdate();
		}

		public void EndUpdate()
		{
			LvControl.EndUpdate();
		}

		public void Add(SLItem sli, bool fChecked)
		{
			Items.Add(sli);
			LvControl.VirtualListSize++;
		}

		public void AddRange(IEnumerable<SLItem> items)
		{
			LvControl.BeginUpdate();
			Items.Clear();
			Items.AddRange(items);
			LvControl.VirtualListSize = Items.Count;
			LvControl.EndUpdate();
			LvControl.RedrawItems(0, Count - 1, true);
		}

		public void Check(int i, bool fChecked)
		{
		}

		void RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			e.Item = LviCreateForSli(Items[e.ItemIndex], false);
		}

		void listView1_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
		{
		}

		void listView1_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
		{
		}
	}
}