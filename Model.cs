using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Media;
using System.Resources;
using System.Text;
using System.Xml;
using NUnit.Framework;
using TCore.UI;

namespace SList
{
	public partial class SListApp : System.Windows.Forms.Form
	{
		private byte[] m_rgb1;
		private byte[] m_rgb2;
		private IgnoreList m_ign;

		public const int lcbMax = 4 * 1024 * 1024;
		public static string s_sRegRoot = "Software\\Thetasoft\\SList";

		ListView LvCur
		{
			get { return SlisCur.Lv; }
		}

		SLISet SlisCur
		{
			get { return m_rgslis[m_islisCur]; }
		}

		#region Initialization

		void InitIgnoreLists()
		{
			m_ign = new IgnoreList();
			m_ign.LoadIgnoreListNames(s_sRegRoot);

			m_cbxIgnoreList.Items.Add("<Create List...>");
			m_cbxIgnoreList.Items.Add("<Copy current list...>");
			foreach (string s in m_ign.IgnoreLists)
			{
				m_cbxIgnoreList.Items.Add(s);
			}
		}

		// the designer initializes m_lv.  this will become m_rglv[s_ilvSource], and m_lv will be set to null. this allows us to create the templates
		// for all the list views in the designer and still have our switchable list views
		void InitializeListViews()
		{
			m_rgslis = new SLISet[s_clvMax];

			m_rgslis[s_ilvSource] = new SLISet();
			m_rgslis[s_ilvSource].Lv = m_lv;
			m_lv = null;

			for (int ilv = 0; ilv < s_clvMax; ilv++)
			{
				if (ilv == s_ilvSource)
					continue; // skip, this is already initialized

				ListView lv = new System.Windows.Forms.ListView();
				lv.Anchor = m_rgslis[s_ilvSource].Lv.Anchor;
				lv.CheckBoxes = m_rgslis[s_ilvSource].Lv.CheckBoxes;

				lv.ContextMenu = m_rgslis[s_ilvSource].Lv.ContextMenu;
				lv.Location = m_rgslis[s_ilvSource].Lv.Location;
				lv.Name = String.Format("m_rglv{0}", ilv);
				lv.Size = m_rgslis[s_ilvSource].Lv.Size;
				lv.TabIndex = m_rgslis[s_ilvSource].Lv.TabIndex;
				lv.UseCompatibleStateImageBehavior = m_rgslis[s_ilvSource].Lv.UseCompatibleStateImageBehavior;
				//m_rglv[ilv].AfterLabelEdit += m_rglv[s_ilvSource].AfterLabelEdit;
				lv.Visible = false;
				this.Controls.Add(lv);
				m_rgslis[ilv] = new SLISet();
				m_rgslis[ilv].Lv = lv;
			}
		}

		private void InitializeListView(int ilv)
		{
			m_rgslis[ilv].Lv.Columns.Add(new ColumnHeader());
			m_rgslis[ilv].Lv.Columns[0].Text = "    Name";
			m_rgslis[ilv].Lv.Columns[0].Width = 146;

			m_rgslis[ilv].Lv.Columns.Add(new ColumnHeader());
			m_rgslis[ilv].Lv.Columns[1].Text = "Size";
			m_rgslis[ilv].Lv.Columns[1].Width = 52;
			m_rgslis[ilv].Lv.Columns[1].TextAlign = HorizontalAlignment.Right;

			m_rgslis[ilv].Lv.Columns.Add(new ColumnHeader());
			m_rgslis[ilv].Lv.Columns[2].Text = "Location";
			m_rgslis[ilv].Lv.Columns[2].Width = 128;

			m_rgslis[ilv].Lv.FullRowSelect = true;
			m_rgslis[ilv].Lv.MultiSelect = false;
			m_rgslis[ilv].Lv.View = View.Details;
			m_rgslis[ilv].Lv.ListViewItemSorter = new ListViewItemComparer(1);
			m_rgslis[ilv].Lv.ColumnClick += new ColumnClickEventHandler(EH_ColumnClick);
			m_rgslis[ilv].Lv.LabelEdit = true;
		}

		private int m_islisCur = -1;

		void ShowListView(int ilv)
		{
			if (m_islisCur != -1)
				m_rgslis[m_islisCur].PathSpec = m_ebSearchPath.Text;

			for (int i = 0; i < s_clvMax; i++)
			{
				m_rgslis[i].Lv.Visible = (i == ilv);
			}
			m_islisCur = ilv;

			SyncSearchTargetUI(ilv);
			m_ebSearchPath.Text = m_rgslis[ilv].PathSpec;
		}

		/* S Y N C  S E A R C H  T A R G E T */
		/*----------------------------------------------------------------------------
        	%%Function: SyncSearchTargetUI
        	%%Qualified: SList.SListApp.SyncSearchTargetUI
        	%%Contact: rlittle
        	
            make the UI reflect what we want the sync target to be. Typically used
            on initialization
        ----------------------------------------------------------------------------*/
		void SyncSearchTargetUI(int ilv)
		{
			m_cbxSearchTarget.SelectedIndex = ilv;
		}

		#endregion


		#region Generic Utilites

		public class PerfTimer
		{
			Stopwatch m_sw;
			private string m_sOp;

			public PerfTimer()
			{
				m_sw = new Stopwatch();
			}

			public void Start(string sOperation)
			{
				m_sOp = sOperation;
				m_sw.Start();
			}

			public void Stop()
			{
				m_sw.Stop();
			}

			public void Report(int msecMin = 0)
			{
				if (m_sw.ElapsedMilliseconds > msecMin)
					MessageBox.Show(String.Format("{0} elapsed time: {1:0.00}", m_sOp, m_sw.ElapsedMilliseconds / 1000.0));
			}
		}

		#endregion

		#region ListView Support

		/* C H A N G E  L I S T  V I E W  S O R T */
		/*----------------------------------------------------------------------------
        	%%Function: ChangeListViewSort
        	%%Qualified: SList.SListApp.ChangeListViewSort
        	%%Contact: rlittle
        	
            Change the sort order for the given listview to sort by the given column
        ----------------------------------------------------------------------------*/
		void ChangeListViewSort(ListView lv, int iColSort)
		{
			if (lv.ListViewItemSorter == null)
				lv.ListViewItemSorter = new ListViewItemComparer(iColSort);
			else
				((ListViewItemComparer)lv.ListViewItemSorter).SetColumn(iColSort);

			lv.Sort();
		}

		void ToggleAllListViewItems(ListView lvCur)
		{
			int i, iMac;

			for (i = 0, iMac = LvCur.Items.Count; i < iMac; i++)
			{
				lvCur.Items[i].Checked = !lvCur.Items[i].Checked;
			}
		}

		void UncheckAllListViewItems(ListView lvCur)
		{
			int i, iMac;

			for (i = 0, iMac = lvCur.Items.Count; i < iMac; i++)
			{
				lvCur.Items[i].Checked = false;
			}
		}

		#endregion

		static int s_ilvSource = 0;
		static int s_ilvDest = 1;
		static int s_clvMax = 2;

		#region BuildFileList

		/* A D D  S L I  T O  L I S T  V I E W */
		/*----------------------------------------------------------------------------
        	%%Function: AddSliToListView
        	%%Qualified: SList.SListApp.AddSliToListView
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		static public void AddSliToListView(SLItem sli, ListView lv)
		{
			AddSliToListView(sli, lv, false);
		}

		static private void AddSliToListView(SLItem sli, ListView lv, bool fChecked)
		{
			ListViewItem lvi = new ListViewItem();

			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

			lvi.Tag = sli;
			lvi.SubItems[2].Text = sli.m_sPath;
			lvi.SubItems[1].Text = sli.m_lSize.ToString("###,###,###");
			lvi.SubItems[0].Text = sli.m_sName;

			if (fChecked)
				lvi.Checked = true;
			lv.Items.Add(lvi);
		}

		private void AddDirectory(DirectoryInfo di, SLISet slis, string sPattern, bool fRecurse, List<FileInfo> plfiTooLong)
		{
			FileInfo[] rgfi;
			int cchDir = di.FullName.Length;

			try
			{
				rgfi = di.GetFiles(sPattern);
			}
			catch
			{
				return;
			}

			int i, iMac;

			for (i = 0, iMac = rgfi.Length; i < iMac; i++)
			{
				bool fTooLong = false;
				try
				{
					if (rgfi[i].Name.Length + cchDir > 256)
						fTooLong = true;
					else
					{
						SLItem sli = new SLItem(rgfi[i].Name, rgfi[i].Length, rgfi[i].DirectoryName, rgfi[i]);
						slis.Add(sli);
					}
				}
				catch (Exception exc)
				{
					fTooLong = true;
				}
				if (fTooLong)
					plfiTooLong.Add(rgfi[i]);

				// Application.DoEvents();
			}

			if (fRecurse)
			{
				DirectoryInfo[] rgdi;

				rgdi = di.GetDirectories();
				if (rgdi != null)
				{
					for (i = 0, iMac = rgdi.Length; i < iMac; i++)
					{
						AddDirectory(rgdi[i], slis, sPattern, fRecurse, plfiTooLong);
					}
				}
			}
		}

		/* B U I L D  F I L E  L I S T */
		/*----------------------------------------------------------------------------
        	%%Function: BuildFileList
        	%%Qualified: SList.SListApp.BuildFileList
        	%%Contact: rlittle
        	
            Take the search path and build the file list (for the selected target)
        ----------------------------------------------------------------------------*/
		private void BuildFileList()
		{
			string sFileSpec = m_ebSearchPath.Text;
			string sPath = null;
			string sPattern = null;
			FileAttributes fa = 0;
			bool fAttrsValid = false;
			PerfTimer pt = new PerfTimer();

			pt.Start("Search");

			// let's see what they gave us.  First, see if its a directory
			try
			{
				fa = File.GetAttributes(sFileSpec);
				fAttrsValid = true;
			}
			catch
			{
				fAttrsValid = false;
			}

			if (fAttrsValid && ((int)fa != -1) && (fa & FileAttributes.Directory) == FileAttributes.Directory)
			{
				// its a directory; use it
				sPath = sFileSpec;
				sPattern = "*";
			}
			else
			{
				sPath = Path.GetDirectoryName(sFileSpec);
				sPattern = Path.GetFileName(m_ebSearchPath.Text);

				if (sPattern == "")
					sPattern = "*";
			}

			DirectoryInfo di = new DirectoryInfo(sPath);

			if (di == null)
			{
				MessageBox.Show("Path not found: " + sPath, "SList");
				return;
			}

			Cursor crsSav = this.Cursor;

			// start a wait cursor
			this.Cursor = Cursors.WaitCursor;

			// stop redrawing
			LvCur.BeginUpdate();

			// save off and reset the item sorter for faster adding
			IComparer lvicSav = LvCur.ListViewItemSorter;
			LvCur.ListViewItemSorter = null;

			LvCur.Items.Clear();

			List<FileInfo> plfiTooLong = new List<FileInfo>();

			AddDirectory(di, SlisCur, sPattern, m_cbRecurse.Checked, plfiTooLong);
			if (plfiTooLong.Count > 0)
			{
				MessageBox.Show(String.Format("Encountered {0} paths that were too long", plfiTooLong.Count));
			}

			pt.Stop();
			pt.Report();

			LvCur.EndUpdate();
			LvCur.ListViewItemSorter = lvicSav;
			LvCur.Update();
			this.Cursor = crsSav;
		}

		static Int64 FileSizeFromDirectoryLine(string sLine)
		{
			return Int64.Parse(sLine.Substring(20, 19),
							   NumberStyles.AllowThousands | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowParentheses);
		}

		static string FileNameFromDirectoryLine(string sLine)
		{
			return sLine.Substring(39).TrimEnd();
		}

		static void ParseFileListLine(string sLine, out string sPath, out string sFilename, out Int64 nSize)
		{
			int ich;
			int ichLast;

			ich = sLine.IndexOf('\t');
			sPath = sLine.Substring(0, ich);
			ichLast = ich + 1;
			ich = sLine.IndexOf('\t', ichLast);
			sFilename = sLine.Substring(ichLast, ich - ichLast);
			ichLast = ich + 1;
			nSize = Int64.Parse(sLine.Substring(ichLast));
		}
		#region tests

		[TestCase("04/05/2015  05:59 PM    13,704,581,120 hiberfil.sys", 13704581120)]
		[TestCase("04/05/2015  05:59 PM 1,113,704,581,120 hiberfil.sys", 1113704581120)]
		[Test]
		public static void TestFileSizeFromDirectoryLine(string sLine, Int64 nSizeExpected)
		{
			Int64 nSize = FileSizeFromDirectoryLine(sLine);
			Assert.AreEqual(nSizeExpected, nSize);
		}

		[TestCase("04/05/2015  05:59 PM    13,704,581,120 hiberfil.sys", "hiberfil.sys")]
		[TestCase("04/05/2015  05:59 PM 1,113,704,581,120 hiberfil.sys", "hiberfil.sys")]
		[Test]
		public static void TestFileNameFromDirectoryLine(string sLine, string sNameExpected)
		{
			string sFile = FileNameFromDirectoryLine(sLine);
			Assert.AreEqual(sNameExpected, sFile);
		}

		[TestCase("04/05/2015  05:59 PM    13,704,581,120 hiberfil.sys", "hiberfil.sys")]
		[TestCase("04/05/2015  05:59 PM 1,113,704,581,120 hiberfil.sys", "hiberfil.sys")]
		[Test]
		public static void TestParseFileListLine(string sLine, string sPathExpected, string sNameExpected, Int64 nSizeExpected)
		{
			string sPath, sName;
			Int64 nSize;

			ParseFileListLine(sLine, out sPath, out sName, out nSize);
			Assert.AreEqual(sPathExpected, sPath);
			Assert.AreEqual(nSizeExpected, nSize);
		}

		static string DirectoryNameFromDirectoryLine(string sLine)
		{
			return sLine.Substring(14);
		}

		[TestCase(" Directory of F:\\", "F:\\")]
		[TestCase(" Directory of F:\\$Recycle.Bin", "F:\\$Recycle.Bin")]
		[Test]
		public static void TestDirectoryNameFromDirectoryLine(string sLine, string sDirNameExpected)
		{
			string sDirName = DirectoryNameFromDirectoryLine(sLine);
			Assert.AreEqual(sDirNameExpected, sDirName);
		}

		#endregion // tests

		private void LoadFileListFromFile(SLISet slis)
		{
			string sFile;

			if (!InputBox.ShowInputBox("File list", out sFile))
				return;

			PerfTimer pt = new PerfTimer();
			pt.Start("load file list");
			// parse a directory listing and add 
			string sCurDirectory = null;
			TextReader tr = new StreamReader(new FileStream(sFile, FileMode.Open, FileAccess.Read), Encoding.Default);
			string sLine;
			slis.PauseListViewUpdate(true);

			sLine = tr.ReadLine();
			bool fInternalFormat = false;

			if (sLine == "[file.lst]")
				fInternalFormat = true;

			while ((sLine = tr.ReadLine()) != null)
			{
				if (fInternalFormat)
				{
					string sPath, sName;
					Int64 nSize;

					ParseFileListLine(sLine, out sPath, out sName, out nSize);
					SLItem sli = new SLItem(sName, nSize, sPath, String.Concat(sPath, "/", sName));
					slis.Add(sli);
					continue;
				}
				// figure out what this line is
				if (sLine.Length < 14)
					continue;

				if (sLine[2] == '/' && sLine[5] == '/')
				{
					// this is a leading date, which means this is either a directory or a file
					if (sLine[24] == '<') // this is a directory
						continue;

					// ok, from [14,39] is the size, [40, ...] is filename
					Int64 nSize = FileSizeFromDirectoryLine(sLine);
					string sFileLine = FileNameFromDirectoryLine(sLine);

					SLItem sli = new SLItem(sFileLine, nSize, sCurDirectory, String.Concat(sCurDirectory, "/", sFileLine));
					slis.Add(sli);
				}
				else if (sLine.StartsWith(" Directory of "))
				{
					sCurDirectory = DirectoryNameFromDirectoryLine(sLine);
				}
			}
			slis.ResumeListViewUpdate();

			pt.Stop();
			pt.Report();
			tr.Close();
		}

		private void SaveFileListToFile(SLISet slis)
		{
			string sFile;

			if (!InputBox.ShowInputBox("File list", out sFile))
				return;

			TextWriter tr = new StreamWriter(new FileStream(sFile, FileMode.CreateNew, FileAccess.Write), Encoding.Default);

			tr.WriteLine("[file.lst]"); // write something out so we know this is one of our files (we will parse it faster)
			foreach (ListViewItem lvi in slis.Lv.Items)
			{
				SLItem sli = (SLItem)lvi.Tag;
				tr.WriteLine("{0}\t{1}\t{2}", sli.m_sPath, sli.m_sName, sli.m_lSize);
			}
			tr.Flush();
			tr.Close();
		}

		#endregion

		#region Core Model (Compare Files, etc)

		private bool FCompareFiles(SLItem sli1, SLItem sli2, ref int min, ref int max, ref int sum)
		{
			int nStart = Environment.TickCount;
			int nEnd;

			FileStream bs1 = new FileStream(Path.Combine(sli1.m_sPath, sli1.m_sName), FileMode.Open, FileAccess.Read, FileShare.Read, 8, false);
			FileStream bs2 = new FileStream(Path.Combine(sli2.m_sPath, sli2.m_sName), FileMode.Open, FileAccess.Read, FileShare.Read, 8, false);

			int lcb = 16;

			long icb = 0;
			int i;
			int iProgress = 0;
			int iCurProgress = 0;
			bool fProgress = true;
			int iIncrement = (int)sli1.m_lSize / 100;
			long lProgressLast = 0;

			if (iIncrement == 0)
				iIncrement = 1;

			if (sli1.m_lSize < 10000)
				fProgress = false;

			if (icb + lcb >= sli1.m_lSize)
				lcb = (int)(sli1.m_lSize - icb);

			m_stbpMainStatus.Text = sli1.m_sName;
			if (fProgress)
			{
				m_prbar.Value = iProgress;
				m_prbar.Show();
			}


			while (lcb > 0)
			{
				// Application.DoEvents();
				if (fProgress)
				{
					if (lProgressLast + iIncrement < icb)
					{
						iCurProgress = (int)(icb / iIncrement);
						m_prbar.Value = Math.Min(iCurProgress, 100);
						iProgress = iCurProgress;
						lProgressLast = iIncrement * iProgress;
					}
				}

				bs1.Read(m_rgb1, 0, lcb);
				bs2.Read(m_rgb2, 0, lcb);

				icb += lcb;
				i = 0;
				while (i < lcb)
				{
					if (m_rgb1[i] != m_rgb2[i])
					{
						//					br1.Close();
						//					br2.Close();
						bs1.Close();
						bs2.Close();

						m_prbar.Value = 100;
						nEnd = Environment.TickCount;

						if ((nEnd - nStart) < min)
							min = nEnd - nStart;

						if ((nEnd - nStart) > max)
							max = (nEnd - nStart);

						sum += (nEnd - nStart);
						return false;
					}
					i++;
				}

				if (lcb < lcbMax)
				{
					if ((int)(sli1.m_lSize - icb - 1) == 0)
						break;

					lcb *= 2;
					if (lcb > lcbMax)
						lcb = lcbMax;
				}

				if (icb + lcb >= sli1.m_lSize)
					lcb = (int)(sli1.m_lSize - icb - 1);

			}
			//		br1.Close();
			//		br2.Close();
			bs1.Close();
			bs2.Close();
			m_prbar.Value = 100;
			nEnd = Environment.TickCount;

			if ((nEnd - nStart) < min)
				min = nEnd - nStart;

			if ((nEnd - nStart) > max)
				max = (nEnd - nStart);

			sum += (nEnd - nStart);
			return true;

		}

		void AddSlisToRgsli(SLISet slis, SLItem[] rgsli, int iFirst, bool fDestOnly)
		{
			int i, iMac;

			for (i = 0, iMac = slis.Lv.Items.Count; i < iMac; i++)
			{
				rgsli[iFirst + i] = (SLItem)slis.Lv.Items[i].Tag;
				rgsli[iFirst + i].ClearDupeChain();
				rgsli[iFirst + i].m_fMarked = false;
				rgsli[iFirst + i].DestOnly = fDestOnly;
			}
		}

		/* E  H  _ F I N D  D U P L I C A T E S */
		/*----------------------------------------------------------------------------
		%%Function: EH_Uniquify
		%%Qualified: SList.SListApp.EH_Uniquify
		%%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		private void BuildUniqueFileList()
		{
			int start, end, sum = 0;
			int min = 999999, max = 0, c = 0;
			SLISet slisSrc = m_rgslis[s_ilvSource];
			int cItems = slisSrc.Lv.Items.Count + m_rgslis[s_ilvDest].Lv.Items.Count;
			SLItem[] rgsli = new SLItem[cItems];

			start = Environment.TickCount;

			AddSlisToRgsli(slisSrc, rgsli, 0, false);

			if (m_rgslis[s_ilvDest].Lv.Items.Count > 0)
			{
				AddSlisToRgsli(m_rgslis[s_ilvDest], rgsli, slisSrc.Lv.Items.Count, true);
			}
			Array.Sort(rgsli, new SLItemComparer(SLItem.SLItemCompare.CompareSize));

			slisSrc.Lv.BeginUpdate();
			slisSrc.Lv.Items.Clear();

			int i = 0;
			int iMac = rgsli.Length;

			int iIncrement = Math.Max(1, iMac / 1000);
			int iLast = 0;

			Cursor crsSav = this.Cursor;

			// start a wait cursor
			this.Cursor = Cursors.WaitCursor;
			m_prbarOverall.Show();
			for (; i < iMac; i++)
			{
				int iDupe, iDupeMac;

				if (iLast + iIncrement < i)
				{
					m_prbarOverall.Value = Math.Min(1000, (int)(i / iIncrement));
					iLast = m_prbarOverall.Value * iIncrement;
				}

				if (rgsli[i].m_fMarked)
					continue;

				if (rgsli[i].DestOnly)
					continue;

				// search forward for dupes
				for (iDupe = i + 1, iDupeMac = rgsli.Length; iDupe < iDupeMac; iDupe++)
				{
					if (rgsli[iDupe].m_fMarked == true)
						continue;

					if (rgsli[i].m_lSize == rgsli[iDupe].m_lSize)
					{
						// do more extensive check here...for now, the size and the name is enough
						if (m_cbCompareFiles.Checked)
						{
							c++;
							if (FCompareFiles(rgsli[i], rgsli[iDupe], ref min, ref max, ref sum))
							{
								if (rgsli[i].m_fMarked == false)
									AddSliToListView(rgsli[i], slisSrc.Lv, true);

								if (rgsli[iDupe].m_fMarked == false)
									AddSliToListView(rgsli[iDupe], slisSrc.Lv);

								rgsli[i].m_fMarked = rgsli[iDupe].m_fMarked = true;
								rgsli[i].AddDupeToChain(rgsli[iDupe]);
							}
						}
						else
						{
							if (rgsli[i].m_sName == rgsli[iDupe].m_sName)
							{
								if (rgsli[i].m_fMarked == false)
									AddSliToListView(rgsli[i], slisSrc.Lv);

								if (rgsli[iDupe].m_fMarked == false)
									AddSliToListView(rgsli[iDupe], slisSrc.Lv);

								rgsli[i].m_fMarked = rgsli[iDupe].m_fMarked = true;
								rgsli[i].AddDupeToChain(rgsli[iDupe]);
							}
						}
					}
					else
					{
						if (rgsli[i].m_fMarked == false)
							// this was unique...
							AddSliToListView(rgsli[i], slisSrc.Lv, true);

						break; // no reason to continue if the lengths changed; we sorted by length
					}
				}
			}
			m_prbar.Hide();
			m_prbarOverall.Hide();
			if (m_cbCompareFiles.Checked)
				m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by file compare.";
			else
				m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by size and name.";

			slisSrc.Lv.EndUpdate();
			this.Cursor = crsSav;
			end = Environment.TickCount;

			int len = end - start;
			if (c == 0)
				c = 1;

			int avg = len / c;
			int avg2 = sum / c;
			m_stbpSearch.Text = len.ToString() + "ms, (" + min.ToString() + ", " + max.ToString() + ", " + avg.ToString() + ", " + avg2.ToString() + ", " + c.ToString() + ")";
		}

		/* A D J U S T  L I S T  V I E W  F O R  F A V O R E D  P A T H S */
		/*----------------------------------------------------------------------------
		    %%Function: AdjustListViewForFavoredPaths
		    %%Qualified: SList.SListApp.AdjustListViewForFavoredPaths
		    %%Contact: rlittle

		    Kinda like FindDuplicates, but it doesn't search for them.  It just looks
		    for dupe chains, and then favors marking/unmark items that match the paths
		    in the preferred paths list (uses m_cbMarkFavored)
	    ----------------------------------------------------------------------------*/
		void AdjustListViewForFavoredPaths()
		{
			foreach (ListViewItem lvi in LvCur.Items)
			{
				SLItem sli = (SLItem)lvi.Tag;

				foreach (String s in m_lbPrefPath.Items)
				{
					if (sli.MatchesPrefPath(s))
					{
						UpdateForPrefPath(sli, s, m_cbMarkFavored.Checked);
						break;
					}
				}
			}
		}

		static void MoveSelectedFiles(ListView lvCur, string sDir, StatusBarPanel stbp)
		{
			FileAttributes fa = 0;
			bool fDirExists = false;
			// let's see what they gave us.  First, see if its a directory
			try
			{
				fa = File.GetAttributes(sDir);
				if ((fa & FileAttributes.Directory) == FileAttributes.Directory)
					fDirExists = true;
				else
				{
					MessageBox.Show(sDir + " exists, but is not a directory.  Please choose a different location", "SList");
					return;
				}
			}
			catch
			{
				fDirExists = false;
			}

			if (fDirExists == false)
			{
				try
				{
					DirectoryInfo di = Directory.CreateDirectory(sDir);
					sDir = di.FullName;
				}
				catch
				{
					MessageBox.Show("Cannot create directory " + sDir + ".  Please choose a different location", "SList");
					return;
				}
			}

			// if we got here, then sDir exists
			if (MessageBox.Show("Move selected files to " + sDir + "?", "SList", MessageBoxButtons.YesNo) == DialogResult.No)
				return;

			// ok, iterate through all the items and find the ones that are checked
			int i, iMac;

			for (i = 0, iMac = lvCur.Items.Count; i < iMac; i++)
			{
				if (!lvCur.Items[i].Checked)
					continue;

				SLItem sli = (SLItem)(lvCur.Items[i].Tag);
				string sSource = Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName));
				string sDest = Path.GetFullPath(Path.Combine(sDir, sli.m_sName));

				if (String.Compare(sSource, sDest, true /*ignoreCase*/) == 0)
				{
					stbp.Text = "Skipped identity move: " + sSource;
					continue;
				}

				// now, see if sDest already exists.  if it does, we need to try
				// to rename the file
				int n = 0;
				string sDestClone = sDest + "";

				while (File.Exists(sDestClone) && n < 1020)
				{
					sDestClone = Path.Combine(Path.GetDirectoryName(sDest), Path.GetFileNameWithoutExtension(sDest) + "(" + n.ToString() + ")" + Path.GetExtension(sDest));
					//				sDestClone = sDest + " (" + n.ToString() + ")";
					n++;
				}

				if (n >= 1020)
				{
					MessageBox.Show("Cannot move " + sSource + " to new location " + sDestClone + ".  There are too many duplicates in the destination.", "SList");
					continue;
				}

				// ok, let's do the move
				stbp.Text = "Moving " + sSource + " -> " + sDestClone;
				File.Move(sSource, sDestClone);
				lvCur.Items[i].Checked = false;
			}
		}

		static bool FRenameFile(string sPathOrig, string sFileOrig, string sPathNew, string sFileNew)
		{
			if (sFileNew == null)
				return false;

			string sSource = Path.GetFullPath(Path.Combine(sPathOrig, sFileOrig));
			string sDest = Path.GetFullPath(Path.Combine(sPathNew, sFileNew));

			try
			{
				File.Move(sSource, sDest);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Cannot rename '" + sFileOrig + "' to '" + sFileNew + "':\n\n" + ex.ToString(), "SList");
				return false;
			}

			return true;
		}

		void ApplyIgnoreList(string sIgnoreList)
		{
			SLISet slis = SlisCur;

			int colSav = ((ListViewItemComparer)slis.Lv.ListViewItemSorter).GetColumn();
			((ListViewItemComparer)slis.Lv.ListViewItemSorter).SetColumn(-1);
			slis.Lv.Sort();

			// otherwise, we're loading a new list
			m_ign.LoadIgnoreList(sIgnoreList);
			int iProgress = 0;
			m_prbarOverall.Value = iProgress;
			m_prbarOverall.Show();

			// and apply the ignore list
			Application.DoEvents();
			int iMac = m_ign.IgnoreItems.Count;

			slis.PauseListViewUpdate(false);
			for (int i = 0; i < iMac; i++)
			{
				if (iProgress != (1000 * i) / iMac)
				{
					iProgress = (1000 * i) / iMac;
					m_prbarOverall.Value = iProgress;
					m_prbarOverall.Update();
					Application.DoEvents();
				}
				RemovePath(slis, m_ign.IgnoreItems[i].PathPrefix);
			}
			m_prbarOverall.Hide();
			Application.DoEvents();
			slis.ResumeListViewUpdate(colSav);
		}

		public enum RegexOp
		{
			Match,
			Filter,
			Check
		};

		private void DoRegex(RegexOp rop)
		{
			Regex rx = null;

			try
			{
				rx = new Regex(m_ebRegEx.Text);
			}
			catch (Exception e)
			{
				MessageBox.Show("Could not compile Regular Expression '" + m_ebRegEx.Text + "':\n" + e.ToString(), "SLList");
				return;
			}


			int i, iMac;

			for (i = 0, iMac = LvCur.Items.Count; i < iMac; i++)
			{
				SLItem sli = (SLItem)(LvCur.Items[i].Tag);
				string sPath = Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName));
				bool fMatch = false;

				fMatch = rx.IsMatch(sPath);

				switch (rop)
				{
					case RegexOp.Check:
						if (fMatch)
							LvCur.Items[i].Checked = true;
						break;
					case RegexOp.Filter:
						if (fMatch)
						{
							LvCur.Items[i].Remove();
							iMac--;
							i--;
						}
						break;
					case RegexOp.Match:
						if (!fMatch)
						{
							LvCur.Items[i].Remove();
							iMac--;
							i--;
						}
						break;
				}
			}
		}

		string sCancelled;

		static string SCalcMatchingListViewItems(ListView lvCur, string sRegEx, string sCounts)
		{
			ATMC atmc = new ATMC(sRegEx);
			string sMatch = String.Format("Matches for '{0}':\n\n", sRegEx);

			int i, iMac;
			int cMatch = 0;

			for (i = 0, iMac = lvCur.Items.Count; i < iMac; i++)
			{
				SLItem sli = (SLItem)(lvCur.Items[i].Tag);

				if (sli.m_atmc == null)
					sli.m_atmc = new ATMC(sli.m_sName);

				int nMatch = 0;
				nMatch = sli.m_atmc.NMatch(atmc);
				if (nMatch > 65)
				{
					sMatch += String.Format("{0:d3}% : '{1}'\n", nMatch, Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName)), sRegEx);
					cMatch++;
				}
			}
			if (cMatch == 0 || MessageBox.Show(sMatch, "Matches", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				sCounts += String.Format("{0}\n", sRegEx);

			return sCounts;
		}


		/* E  H  _ I D L E */
		/*----------------------------------------------------------------------------
		    %%Function: EH_Idle
		    %%Qualified: SList.SListApp.EH_Idle
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		private void EH_Idle(object sender, System.EventArgs e)
		{
			m_tmr.Enabled = false;
			if (sCancelled.Length > 0)
			{
				MessageBox.Show(sCancelled, "Not Found");
				sCancelled = "";
			}
		}

		void AddPreferredPath(string s)
		{
			m_lbPrefPath.Items.Add(s);
			AdjustListViewForFavoredPaths();
		}

		void RemovePath(SLISet slis, string sPathRoot)
		{
			slis.Remove(sPathRoot, m_prbar);
		}

		void EH_RemovePath(object sender, EventArgs e)
		{
			MenuItem mni = (MenuItem)sender;
			RemovePath(m_rgslis[m_islisCur], mni.Text);
			if (m_cbAddToIgnoreList.Checked)
			{
				m_ign.AddIgnorePath(mni.Text);
			}
		}

		private void EH_AddPreferredPath(object sender, EventArgs e)
		{
			MenuItem mni = (MenuItem)sender;
			AddPreferredPath(mni.Text);
		}

		#endregion // Core Model (Compare Files, etc)

		#region List View Commands

		void LaunchSli(SLItem sli)
		{
			Process.Start(Path.Combine(sli.m_sPath, sli.m_sName));
		}

		/* H A N D L E  D R O P */
		/*----------------------------------------------------------------------------
		    %%Function: HandleDrop
		    %%Qualified: SList.SListApp.HandleDrop
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		private void HandleDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			this.Activate();

			m_tmr.Enabled = false;

			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			sCancelled = "";
			foreach (string sFile in files)
			{
				m_ebRegEx.Text = Path.GetFileName(sFile);
				EH_SmartMatchClick(null, null);
			}
			//		if (sCancelled.Length > 0)
			//			MessageBox.Show(sCancelled, "Not Found");
			m_tmr.Interval = 500;
			m_tmr.Enabled = true;
		}

		/* H A N D L E  D R A G  E N T E R */
		/*----------------------------------------------------------------------------
		    %%Function: HandleDragEnter
		    %%Qualified: SList.SListApp.HandleDragEnter
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		private void HandleDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		/* H A N D L E  D R A G  L E A V E */
		/*----------------------------------------------------------------------------
		    %%Function: HandleDragLeave
		    %%Qualified: SList.SListApp.HandleDragLeave
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		private void HandleDragLeave(object sender, System.EventArgs e) { }

		private void EH_SelectPrevDupe(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

			if (slvic != null && slvic.Count >= 1)
			{
				SLItem sli = (SLItem)slvic[0].Tag;

				SLItem sliSel = sli.Prev;
				Select(sliSel);
			}
		}

		private void EH_SelectNextDupe(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

			if (slvic != null && slvic.Count >= 1)
			{
				SLItem sli = (SLItem)slvic[0].Tag;

				SLItem sliSel = sli.Next;
				Select(sliSel);
			}
		}

		#endregion // List View Commands

		public void UpdateForPrefPath(SLItem sliMaster, string s, bool fMark)
		{
			SLItem sli;

			sliMaster.m_fMarked = fMark;
			UpdateMark(sliMaster);

			sli = sliMaster;

			while ((sli = sli.Prev) != null)
			{
				if (sli.MatchesPrefPath(s))
					sli.m_fMarked = fMark;
				else
					sli.m_fMarked = !fMark;

				UpdateMark(sli);
			}

			sli = sliMaster;

			while ((sli = sli.Next) != null)
			{
				if (sli.MatchesPrefPath(s))
					sli.m_fMarked = fMark;
				else
					sli.m_fMarked = !fMark;
				UpdateMark(sli);
			}
		}

		void UpdateMark(SLItem sli)
		{
			ListViewItem lvi = LviFromSli(sli);

			lvi.Checked = sli.m_fMarked;
		}

		ListViewItem LviFromSli(SLItem sli)
		{
			foreach (ListViewItem lvi in LvCur.Items)
			{
				if (lvi.Tag == sli)
					return lvi;
			}
			return null;
		}

		void Select(SLItem sli)
		{
			if (sli == null)
			{
				SystemSounds.Beep.Play();
				return;
			}
			ListViewItem lvi = LviFromSli(sli);

			if (lvi != null)
			{
				lvi.Selected = true;
				LvCur.Select();
				return;
			}
			SystemSounds.Beep.Play();
		}

		private void DoSearchTargetChange(object sender, EventArgs e)
		{
			ShowListView(m_cbxSearchTarget.SelectedIndex);
		}

		private SLISet[] m_rgslis;

		// we might not be at the beginning of the dupe list for this item -- we might
		// have skipped over some DestOnly items, and those might be the dupes we
		// are looking for
		int FindFirstDupeCandidate(SLItem[] rgsli, int iCurrent)
		{
			// walk backwards until we change sizes or hit the beginning
			int i = iCurrent - 1;

			while (i >= 0 && rgsli[i].m_lSize == rgsli[iCurrent].m_lSize)
				i--;

			// we break on the first item that doesn't match...return
			// the next item
			return i + 1;
		}

		void BuildMissingFileList()
		{
			SLItem[] rgsli;
			int start, end, sum = 0;
			int min = 999999, max = 0, c = 0;
			SLISet slisSrc = m_rgslis[s_ilvSource];

			start = Environment.TickCount;

			int cItems = slisSrc.Lv.Items.Count + m_rgslis[s_ilvDest].Lv.Items.Count;

			rgsli = new SLItem[cItems];

			AddSlisToRgsli(slisSrc, rgsli, 0, false);

			if (m_rgslis[s_ilvDest].Lv.Items.Count > 0)
			{
				AddSlisToRgsli(m_rgslis[s_ilvDest], rgsli, slisSrc.Lv.Items.Count, true);
			}
			Array.Sort(rgsli, new SLItemComparer(SLItem.SLItemCompare.CompareSizeDest));

			slisSrc.Lv.BeginUpdate();
			slisSrc.Lv.Items.Clear();

			int i = 0;
			int iMac = rgsli.Length;

			int iIncrement = Math.Max(1, iMac / 1000);
			int iLast = 0;

			Cursor crsSav = this.Cursor;

			// start a wait cursor
			this.Cursor = Cursors.WaitCursor;
			m_prbarOverall.Show();
			for (; i < iMac; i++)
			{
				int iDupe, iDupeMac;

				if (iLast + iIncrement < i)
				{
					m_prbarOverall.Value = Math.Min(1000, (int)(i / iIncrement));
					iLast = m_prbarOverall.Value * iIncrement;
				}

				if (rgsli[i].m_fMarked)
					continue;

				if (rgsli[i].DestOnly)
					continue;

				iDupe = FindFirstDupeCandidate(rgsli, i);

				// search forward for dupes
				for (iDupeMac = rgsli.Length; iDupe < iDupeMac; iDupe++)
				{
					// don't compare against ourself
					if (iDupe == i)
						continue;

					// we are explicitly looking ONLY at fDestOnly files to see if there's a dupe
					// (used to include rgsli[iDupe].m_fMarked == true  -- but why exclude
					// destonly files that were already duped against? a destonly file can be 
					// a dupe for multiple source files...
					if (rgsli[iDupe].m_fDestOnly == false)
						continue;

					if (rgsli[i].m_lSize == rgsli[iDupe].m_lSize)
					{
						// do more extensive check here...for now, the size and the name is enough
						if (m_cbCompareFiles.Checked)
						{
							c++;
							if (FCompareFiles(rgsli[i], rgsli[iDupe], ref min, ref max, ref sum))
							{
								// we found a dupe in the target. yay, don't add it anywhere
								rgsli[i].m_fMarked = rgsli[iDupe].m_fMarked = true;
								rgsli[iDupe].AddDupeToChain(rgsli[i]);
								break;
							}
						}
						else
						{
							if (rgsli[i].m_sName == rgsli[iDupe].m_sName)
							{
								// we found a dupe in the target.. nothing to add
								rgsli[i].m_fMarked = true; //  rgsli[iDupe].m_fMarked = true; // don't mark the dupe
														   // rgsli[i].AddDupeToChain(rgsli[iDupe]); // don't add to the dupe chain
								break;
							}
							else
							{
								break; // no sense continuing if the name changed -- we sorted by size by name, and we aren't doing a deep compare, so name mismatch means we'll never match.
							}
						}
					}
					else
					{
						break; // no reason to continue if the lengths changed; we sorted by length
					}
				}
				// we have left the loop.  either we broke out because we know we don't have a match,
				// or we exhausted all the dupes and we know we found at least one match.
				// in either case, if we found a dupe in the target, we will have marked m_fMarked to be true...
				// if its not set, then we didn't find this file in the destination.
				if (rgsli[i].m_fMarked == false)
					// this was unique...
					AddSliToListView(rgsli[i], slisSrc.Lv, true);


			}
			m_prbar.Hide();
			m_prbarOverall.Hide();
			if (m_cbCompareFiles.Checked)
				m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by file compare.";
			else
				m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by size and name.";

			slisSrc.Lv.EndUpdate();
			this.Cursor = crsSav;
			end = Environment.TickCount;

			int len = end - start;
			if (c == 0)
				c = 1;

			int avg = len / c;
			int avg2 = sum / c;
			m_stbpSearch.Text = len.ToString() + "ms, (" + min.ToString() + ", " + max.ToString() + ", " + avg.ToString() + ", " + avg2.ToString() + ", " + c.ToString() + ")";
		}
	}
}