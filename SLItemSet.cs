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
		public SLISetView View { get; private set; }
		private string m_sSpec;

		public SListApp.FileList FileListType { get; private set; }

		public string PathSpec { get { return m_sSpec; } set { m_sSpec = value; } }
		public bool Recurse { get; set; }

		public SLISet(SListApp.FileList fileList, ListView lv)
		{
			FileListType = fileList;
			View = new SLISetView(lv);
			m_items = new Dictionary<string, SLItem>();
			m_plLvComparerStack = new List<IComparer>();
		}

		public void Clear()
		{
			m_items = new Dictionary<string, SLItem>();
			View.Clear();
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
			SmartList.AddSliToListView(sli, View);
		}

		private List<IComparer> m_plLvComparerStack;

		public void PauseListViewUpdate(bool fClear)
		{
			if (fClear)
				View.Clear();
			View.BeginUpdate();
		}

		public void ResumeListViewUpdate(int colNew = -1)
		{
			View.EndUpdate();
			View.Refresh();
		}
		
		public void UpdateListViewFromSlis()
		{
			View.AddRange(m_items.Values);
		}
		
		public delegate bool RemoveFilterDelegate(SLItem sli);

		public void RemoveGeneric(ISmartListUi ui, RemoveFilterDelegate delRemove)
		{
			using (RaiiWaitCursor cursor = new RaiiWaitCursor(ui, Cursors.WaitCursor))
			{
				ui.SetProgressBarMac(ProgressBarType.Overall, View.Items.Count);
				ui.SetProgressBarOnDemand(ProgressBarType.Overall, 500 /*ms*/);

				PauseListViewUpdate(false);

				View.BeginUpdate();

				// walk through every list view item, find matching items, then remove them and remove them from the hash set
				int i = View.Items.Count;

				int c = i;
				int cRemove = 0;

				int iSelStart = View.SelectedIndex();
				int diSelStart = 0;

				while (--i >= 0)
				{
					ui.UpdateProgressBar(ProgressBarType.Overall, c - i, Application.DoEvents);

					SLItem sli = View.Items[i];

					if (!delRemove(sli))
						continue;

					m_items.Remove(sli.Hashkey);
					View.Remove(i);
					cRemove++;
					if (i <= iSelStart)
						diSelStart++;
				}

				iSelStart -= diSelStart;
				iSelStart++;

				if (iSelStart < 0)
					iSelStart = 0;

				if (iSelStart > View.Items.Count - 1)
					iSelStart = View.Items.Count - 1;

				if (View.Items.Count > 0)
					View.Select(iSelStart);

				View.EndUpdate();
				ui.HideProgressBar(ProgressBarType.Overall);
				ResumeListViewUpdate();
			}
		}

		public void Remove(string sPathRoot, ISmartListUi ui)
		{
			RemoveGeneric(
				ui, 
				(sli) => sli != null && sli.MatchesPathPrefix(sPathRoot));
		}

		public void RemoveType(FilePatternInfo typeInfo, ISmartListUi ui)
		{
			RemoveGeneric(
				ui,
				(sli) =>
				{
					if (sli == null || !sli.MatchesPathPrefix(typeInfo.RootPath))
						return false;

					return string.Compare(sli.Extension, typeInfo.Pattern, true) == 0;
				});
		}

		public void RemovePattern(FilePatternInfo typeInfo, ISmartListUi ui)
		{
			RemoveGeneric(
				ui,
				(sli) =>
				{
					if (sli == null || !sli.MatchesPathPrefix(typeInfo.RootPath))
						return false;

					// now the extension has to match
					return string.Compare(sli.Name, typeInfo.Pattern, true) == 0;
				});
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
