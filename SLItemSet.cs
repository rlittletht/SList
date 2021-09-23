using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TCore.XmlSettings;

namespace SList
{
	public class SLISet
	{
		private Dictionary<string, SLItem> m_items;
		private ListView m_lv;
		private string m_sSpec;

		public SListApp.FileList FileListType { get; private set; }

		public string PathSpec { get { return m_sSpec; } set { m_sSpec = value; } }
		public bool Recurse { get; set; }

		public ListView Lv
		{
			get { return m_lv; }
			set { m_lv = value; }
		}

		public SLISet(SListApp.FileList fileList)
		{
			FileListType = fileList;
			m_items = new Dictionary<string, SLItem>();
			m_plLvComparerStack = new List<IComparer>();
		}

		public void Clear()
		{
			m_items = new Dictionary<string, SLItem>();
		}

		public void AddInternal(SLItem sli)
		{
			m_items.Add(sli.Hashkey, sli);
		}

		public void Add(SLItem sli, bool duplicatesOK = false)
		{
			if (duplicatesOK && m_items.ContainsKey(sli.Hashkey))
				return;

			m_items.Add(sli.Hashkey, sli);
			SmartList.AddSliToListView(sli, m_lv);
		}

		private List<IComparer> m_plLvComparerStack;

		public void PauseListViewUpdate(bool fClear)
		{
			m_plLvComparerStack.Add(m_lv.ListViewItemSorter);
			m_lv.ListViewItemSorter = null;

			if (fClear)
				m_lv.Items.Clear();
			m_lv.BeginUpdate();
		}

		public void ResumeListViewUpdate(int colNew = -1)
		{
			m_lv.ListViewItemSorter = m_plLvComparerStack[m_plLvComparerStack.Count - 1];
			if (colNew != -1)
				((ListViewItemComparer)m_lv.ListViewItemSorter).SetColumn(colNew);

			m_plLvComparerStack.RemoveAt(m_plLvComparerStack.Count - 1);
			m_lv.EndUpdate();
			m_lv.Update();
		}
		
		public void UpdateListViewFromSlis()
		{
			ListViewItem[] rglvi = new ListViewItem[m_items.Count];

			int isli = 0;
			foreach (SLItem sli in m_items.Values)
				rglvi[isli++] = SmartList.LviCreateForSli(sli, false);

			m_lv.Items.Clear();
			m_lv.BeginUpdate();
			m_lv.Items.AddRange(rglvi);
			m_lv.EndUpdate();
		}

		void CleanupNullTags(ISmartListUi ui, int cRemove, int diSelStart)
		{
			// now go through and find all the null tags and remove them
			SmartList.PerfTimer pt = new SmartList.PerfTimer();
			pt.Start($"cleaning up null tags: {cRemove} items");
			int i = 0;
			int iSelCur = m_lv.SelectedIndices[0];

			m_lv.SelectedIndices.Clear();

			if (cRemove > 500)
			{
				ListViewItem[] rglvi = new ListViewItem[m_lv.Items.Count - cRemove];
				
				int isli = 0;
				while (i < m_lv.Items.Count)
				{
					if (m_lv.Items[i].Tag != null)
						rglvi[isli++] = m_lv.Items[i];
					i++;
				}
				m_lv.Items.Clear();
				m_lv.Items.AddRange(rglvi);
			}
			else
			{
				i = m_lv.Items.Count;
				while (--i >= 0)
				{
					if (m_lv.Items[i].Tag == null)
						m_lv.Items.RemoveAt(i);
				}
			}
			iSelCur -= diSelStart;
			if (iSelCur < 0)
				iSelCur = 0;

			if (m_lv.Items.Count > 0)
			{
				m_lv.Items[iSelCur].Selected = true;
				m_lv.Items[iSelCur].EnsureVisible();
			}

			pt.Stop();
			pt.Report(10000);
		}


		public void Remove(string sPathRoot, ISmartListUi ui)
		{
			PauseListViewUpdate(false);

			m_lv.BeginUpdate();
			// walk through every list view item, find matching items, then remove them and remove them from the hash set
			int i = m_lv.Items.Count;

			ui.SetProgressBarMac(ProgressBarType.Current, i);
			ui.ShowProgressBar(ProgressBarType.Current);
			int c = i;
			int cRemove = 0;

			//int iRemoveStart = -1;
			//int iRemovePrev = -1;
			int iSelStart = m_lv.SelectedIndices[0];
			int diSelStart = 0;

			while (--i >= 0)
			{
				ui.UpdateProgressBar(ProgressBarType.Current, c - i, Application.DoEvents);

				SLItem sli = (SLItem)m_lv.Items[i].Tag;
				if (sli != null && sli.MatchesPathPrefix(sPathRoot))
				{
					m_items.Remove(sli.Hashkey);
					cRemove++;
					m_lv.Items[i].Tag = null;
					if (i <= iSelStart)
						diSelStart++;
				}
			}

			CleanupNullTags(ui, cRemove, diSelStart);
			m_lv.EndUpdate();
			ui.HideProgressBar(ProgressBarType.Current);
			ResumeListViewUpdate();
		}

		public void RemoveType(FilePatternInfo typeInfo, ISmartListUi ui)
		{
			PauseListViewUpdate(false);

			m_lv.BeginUpdate();
			// walk through every list view item, find matching items, then remove them and remove them from the hash set
			int i = m_lv.Items.Count;

			ui.SetProgressBarMac(ProgressBarType.Current, i);
			ui.ShowProgressBar(ProgressBarType.Current);
			int c = i;
			int cRemove = 0;

			//int iRemoveStart = -1;
			//int iRemovePrev = -1;
			int iSelStart = m_lv.SelectedIndices[0];
			int diSelStart = 0;

			while (--i >= 0)
			{
				ui.UpdateProgressBar(ProgressBarType.Current, c - i, Application.DoEvents);

				SLItem sli = (SLItem)m_lv.Items[i].Tag;
				if (sli != null && sli.MatchesPathPrefix(typeInfo.RootPath))
				{
					// now the extension has to match
					if (string.Compare(sli.Extension, typeInfo.Pattern, true) == 0)
					{
						m_items.Remove(sli.Hashkey);
						cRemove++;
						m_lv.Items[i].Tag = null;
						if (i <= iSelStart)
							diSelStart++;
					}
				}
			}

			CleanupNullTags(ui, cRemove, diSelStart);
			m_lv.EndUpdate();
			ui.HideProgressBar(ProgressBarType.Current);
			ResumeListViewUpdate();
		}

		public void RemovePattern(FilePatternInfo typeInfo, ISmartListUi ui)
		{
			PauseListViewUpdate(false);

			m_lv.BeginUpdate();
			// walk through every list view item, find matching items, then remove them and remove them from the hash set
			int i = m_lv.Items.Count;

			ui.SetProgressBarMac(ProgressBarType.Current, i);
			ui.ShowProgressBar(ProgressBarType.Current);
			int c = i;
			int cRemove = 0;

			//int iRemoveStart = -1;
			//int iRemovePrev = -1;
			int iSelStart = m_lv.SelectedIndices[0];
			int diSelStart = 0;

			while (--i >= 0)
			{
				ui.UpdateProgressBar(ProgressBarType.Current, c - i, Application.DoEvents);

				SLItem sli = (SLItem)m_lv.Items[i].Tag;
				if (sli != null && sli.MatchesPathPrefix(typeInfo.RootPath))
				{
					// now the extension has to match
					if (string.Compare(sli.Name, typeInfo.Pattern, true) == 0)
					{
						m_items.Remove(sli.Hashkey);
						cRemove++;
						m_lv.Items[i].Tag = null;
						if (i <= iSelStart)
							diSelStart++;
					}
				}
			}

			CleanupNullTags(ui, cRemove, diSelStart);
			m_lv.EndUpdate();
			ui.HideProgressBar(ProgressBarType.Current);
			ResumeListViewUpdate();
		}

		public IEnumerator<string> ItemEnumerator { get; set; }

		static RepeatContext<SLISet>.RepeatItemContext CreateFileRepeatItemContext(
			SLISet slis,
			Element<SLISet> element,
			RepeatContext<SLISet>.RepeatItemContext parent)
		{
			// for write...
			if (slis.m_items != null && slis.ItemEnumerator != null)
			{
				return new RepeatContext<SLISet>.RepeatItemContext(
					element,
					parent,
					slis.m_items[slis.ItemEnumerator.Current]);
			}

			// for read
			return new RepeatContext<SLISet>.RepeatItemContext(element, parent, new SLItem());
		}

		static bool AreRemainingFiles(SLISet t, RepeatContext<SLISet>.RepeatItemContext itemcontext)
		{
			if (t.m_items == null || t.m_items.Count == 0)
				return false;

			if (t.ItemEnumerator == null)
				t.ItemEnumerator = t.m_items.Keys.GetEnumerator();

			return t.ItemEnumerator.MoveNext();
		}

		private static void CommitFileRepeatItemContext(SLISet t, RepeatContext<SLISet>.RepeatItemContext itemcontext)
		{
			SLItem item = (SLItem) itemcontext.RepeatKey;
			if (t.m_items == null)
				t.m_items = new Dictionary<string, SLItem>();

			t.AddInternal(item); // don't add to the list view yet...
		}

		static XmlDescription<SLISet> CreateXmlDescription()
		{
			return XmlDescriptionBuilder<SLISet>
				.Build("http://www.thetasoft.com/scehmas/SList/filelist/2020", "FileList")
				.DiscardAttributesWithNoSetter()
				.DiscardUnknownAttributes()
				.AddChildElement("File", null, null)
				.SetRepeating(
					CreateFileRepeatItemContext,
					AreRemainingFiles,
					CommitFileRepeatItemContext)
				.AddAttribute("hashKey", GetItemHashKey, SetItemHashKey)
				.AddAttribute("size", GetItemSize, SetItemSize)
				.AddAttribute("isReparsePoint", GetIsReparsePoint, SetIsReparsePoint)
				.AddChildElement("sha256", GetSha256, SetSha256)
				.AddElement("name", GetName, SetName)
				.AddElement("path", GetPath, SetPath);
		}

		public static void SaveFileListXml(SLISet slis, string outfile)
		{
			XmlDescription<SLISet> xml = CreateXmlDescription();

			slis.ItemEnumerator = null;				
			using (WriteFile<SLISet> writeFile = WriteFile<SLISet>.CreateSettingsFile(xml, outfile, slis))
			{
				writeFile.SerializeSettings(xml, slis);
			}
		}

		public static void LoadFileListXml(SLISet slis, string infile)
		{
			XmlDescription<SLISet> xml = CreateXmlDescription();

			using (ReadFile<SLISet> readFile = ReadFile<SLISet>.CreateSettingsFile(infile))
			{
				readFile.DeSerialize(xml, slis);
			}

			slis.UpdateListViewFromSlis();
		}

		private static void SetSha256(SLISet t, string value, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)
			=> SLItem.SetSha256((SLItem)repeatitemcontext.RepeatKey, value);

		private static string GetSha256(SLISet t, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)
			=> SLItem.GetSha256((SLItem) repeatitemcontext.RepeatKey);

		private static void SetItemSize(SLISet t, string value, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)
			=> SLItem.SetSize(((SLItem) repeatitemcontext.RepeatKey), value);

		private static string GetItemSize(SLISet t, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)
			=> SLItem.GetSize((SLItem) repeatitemcontext.RepeatKey);

		private static void SetItemHashKey(SLISet t, string value, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)	=> SLItem.SetItemHashKey((SLItem)repeatitemcontext.RepeatKey, value);
		private static string GetItemHashKey(SLISet t, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)				=> SLItem.GetItemHashKey((SLItem) repeatitemcontext.RepeatKey);

		private static void   SetName(SLISet t, string value, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)	=> SLItem.SetName((SLItem)repeatitemcontext.RepeatKey, value);
		private static string GetName(SLISet t, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)				=> SLItem.GetName((SLItem) repeatitemcontext.RepeatKey);

		private static void   SetPath(SLISet t, string value, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)	=> SLItem.SetPath((SLItem)repeatitemcontext.RepeatKey, value);
		private static string GetPath(SLISet t, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)				=> SLItem.GetPath((SLItem) repeatitemcontext.RepeatKey);

		private static void SetIsReparsePoint(SLISet t, string value, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)
			=> SLItem.SetIsReparsePoint((SLItem)repeatitemcontext.RepeatKey, value);

		private static string GetIsReparsePoint(SLISet t, RepeatContext<SLISet>.RepeatItemContext repeatitemcontext)
			=> SLItem.GetIsReparsePoint((SLItem)repeatitemcontext.RepeatKey);

	}
}
