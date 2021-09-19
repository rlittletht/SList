﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SList
{
	public class SLISet
	{
		private Hashtable m_ht;
		private ListView m_lv;
		private string m_sSpec;

		public string PathSpec { get { return m_sSpec; } set { m_sSpec = value; } }
		public bool Recurse { get; set; }

		public ListView Lv
		{
			get { return m_lv; }
			set { m_lv = value; }
		}

		public SLISet()
		{
			m_ht = new Hashtable();
			m_plLvComparerStack = new List<IComparer>();
		}

		public void Add(SLItem sli)
		{
			if (m_ht.Contains(sli.Hashkey))
			{
				SLIBucket slib = (SLIBucket)m_ht[sli.Hashkey];

				slib.Items.Add(sli);
			}
			else
			{
				SLIBucket slib = new SLIBucket(sli.Hashkey, sli);
				m_ht.Add(sli.Hashkey, slib);
			}
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
				if (sli != null && sli.MatchesPrefPath(sPathRoot))
				{
					SLIBucket slib = (SLIBucket)m_ht[sli.Hashkey];

					slib.Remove(sli);
					cRemove++;
					m_lv.Items[i].Tag = null;
					if (i <= iSelStart)
						diSelStart++;
				}
			}

			// now go through and find all the null tags and remove them
			SmartList.PerfTimer pt = new SmartList.PerfTimer();
			pt.Start(String.Format("remove {0} {1} times", sPathRoot, cRemove));

			if (cRemove > 500)
			{
				ListViewItem[] rglvi = new ListViewItem[m_lv.Items.Count - cRemove];

				i = 0;
				int isli = 0;
				while (i < m_lv.Items.Count)
				{
					if (m_lv.Items[i].Tag != null)
						rglvi[isli++] = m_lv.Items[i];
					i++;
				}
				int iCur = m_lv.SelectedIndices[0];
				m_lv.Items.Clear();
				m_lv.Items.AddRange(rglvi);
				iCur -= diSelStart;
				m_lv.Items[iCur].Selected = true;
				m_lv.Items[iCur].EnsureVisible();
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
			pt.Stop();
			pt.Report(10000);
			m_lv.EndUpdate();
			ui.HideProgressBar(ProgressBarType.Current);
			ResumeListViewUpdate();
		}
	}
}