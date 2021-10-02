using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace SList
{
	public class SLISetView
	{
		public List<SLItem> Items { get; private set; }
		private ListView LvControl { get; set; }
		public SLISetViewItemComparer Comparer { get; set; }
		private SLISet m_parent;
		private ISmartListUi m_ui;

		public int Count => Items.Count;

		public SLISetView(ListView lvControl, SLISet parent, ISmartListUi ui)
		{
			m_parent = parent;
			m_ui = ui;
			LvControl = lvControl;
			LvControl.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(RetrieveVirtualItem);
			Items = new List<SLItem>();
			Comparer = new SLISetViewItemComparer(2);
			InitListView();
		}

		void InitListView()
		{
			LvControl.Columns.Add(new ColumnHeader());
			LvControl.Columns[0].Text = "    Name";
			LvControl.Columns[0].Width = 212;

			LvControl.Columns.Add(new ColumnHeader());
			LvControl.Columns[1].Text = "Type";
			LvControl.Columns[1].Width = 98;
			LvControl.Columns[1].TextAlign = HorizontalAlignment.Center;

			LvControl.Columns.Add(new ColumnHeader());
			LvControl.Columns[2].Text = "Size";
			LvControl.Columns[2].Width = 92;
			LvControl.Columns[2].TextAlign = HorizontalAlignment.Right;

			LvControl.Columns.Add(new ColumnHeader());
			LvControl.Columns[3].Text = "Location";
			LvControl.Columns[3].Width = 512;

			LvControl.FullRowSelect = true;
			LvControl.MultiSelect = true;
			LvControl.View = System.Windows.Forms.View.Details;
			LvControl.ColumnClick += new ColumnClickEventHandler(EH_ColumnClick);
			LvControl.KeyUp += new KeyEventHandler(EH_OnKeyUp);
			LvControl.LabelEdit = true;
		}

		void EH_OnKeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Delete)
				return;

			if (LvControl.SelectedIndices.Count == 0)
			{
				SystemSounds.Beep.Play();
				return;
			}

			List<int> deleting = new List<int>();

			foreach (int i in LvControl.SelectedIndices)
				Items[i].PendingRemove = true;

			m_parent.RemovePendingItems(m_ui);
			m_ui.SetCount(Items.Count);
		}

		private void EH_ColumnClick(object o, ColumnClickEventArgs e)
		{
			if (Comparer == null)
				Comparer = new SLISetViewItemComparer(e.Column);
			else
				Comparer.SetColumn(e.Column);

			Sort();
		}

		static ListViewItem LviCreateForSli(SLItem sli)
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

			if (sli.Checked)
				lvi.Checked = true;

			return lvi;
		}

		public void Clear()
		{
			Items.Clear();
			LvControl.VirtualListSize = 0;
		}

		public void UpdateAfterAdd()
		{
			Items.Sort(Comparer);
			Refresh();
		}

		public void Refresh()
		{
			if (Items.Count == 0)
				LvControl.Items.Clear();
			else
				LvControl.RedrawItems(0, Items.Count - 1, true);
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

		public void ClearMarks()
		{
			foreach (SLItem sli in Items)
				sli.IsMarked = false;
		}

		public void UpdateChecked(SLItem sli)
		{
			int i = GetItemIndex(sli);
			if (i == -1)
				return; // its possible this sli is in a different view...
			Check(i, sli.Checked);
		}

		public void Select(int i)
		{
			LvControl.SelectedIndices.Clear();
			LvControl.Items[i].Selected = true;
			LvControl.Select();
			LvControl.Items[i].EnsureVisible();
		}

		public void Select(SLItem sli)
		{
			Select(GetItemIndex(sli));
		}

		public int SelectedIndex()
		{
			if (LvControl.SelectedIndices.Count == 0)
				return -1;

			return LvControl.SelectedIndices[0];
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
			sli.Checked = fChecked;
			Items.Add(sli);
			LvControl.VirtualListSize = Items.Count;
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
			Items[i].Checked = fChecked;
			LvControl.Items[i].Checked = fChecked;
			LvControl.RedrawItems(i, i, true);
		}

		void RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			e.Item = LviCreateForSli(Items[e.ItemIndex]);
		}

		void listView1_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
		{
		}

		void listView1_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
		{
		}
	}
}