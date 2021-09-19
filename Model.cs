using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Media;
using System.Text;
using NUnit.Framework;
using TCore.UI;

namespace SList
{
	public class SmartList
	{
		private ISmartListUi m_ui;

		public SmartList(ISmartListUi ui)
		{
			m_ui = ui;

			m_rgb1 = new byte[lcbMax];
			m_rgb2 = new byte[lcbMax];

			InitIgnoreLists();
			m_ui.ShowListView(SListApp.s_ilvSource);
		}

		private byte[] m_rgb1;
		private byte[] m_rgb2;
		private IgnoreList m_ign;

		public const int lcbMax = 4 * 1024 * 1024;
		public static string s_sRegRoot = "Software\\Thetasoft\\SList";


		#region Initialization

		void InitIgnoreLists()
		{
			m_ign = new IgnoreList();
			m_ign.LoadIgnoreListNames(s_sRegRoot);

			m_ui.AddIgnoreListItem("<Create List...>");
			m_ui.AddIgnoreListItem("<Copy current list...>");
			foreach (string s in m_ign.IgnoreLists)
			{
				m_ui.AddIgnoreListItem(s);
			}
		}

		public void CreateIgnoreList(string name, bool fCreateFromExisting)
		{
			m_ign.CreateIgnoreList(name, fCreateFromExisting);
		}

		public void EnsureIgnoreListSaved()
		{
			m_ign.EnsureListSaved();
		}

		public void AddIgnorePath(string path)
		{
			m_ign.AddIgnorePath(path);
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

			public void Report(int msecMin = 0, ISmartListUi ui = null)
			{
				if (m_sw.ElapsedMilliseconds > msecMin)
				{
					string sReport = String.Format("{0} elapsed time: {1:0.00}", m_sOp,
						m_sw.ElapsedMilliseconds / 1000.0);

					if (ui != null)
						ui.SetStatusText(sReport);
					else
						MessageBox.Show(sReport);
				}
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
		public void ChangeListViewSort(ListView lv, int iColSort)
		{
			if (lv.ListViewItemSorter == null)
				lv.ListViewItemSorter = new ListViewItemComparer(iColSort);
			else
				((ListViewItemComparer)lv.ListViewItemSorter).SetColumn(iColSort);

			lv.Sort();
		}

		public void ToggleAllListViewItems(ListView lvCur)
		{
			int i, iMac;

			for (i = 0, iMac = m_ui.LvCur.Items.Count; i < iMac; i++)
			{
				m_ui.LvCur.Items[i].Checked = !m_ui.LvCur.Items[i].Checked;
			}
		}

		internal void UncheckAllListViewItems(ListView lvCur)
		{
			int i, iMac;

			for (i = 0, iMac = m_ui.LvCur.Items.Count; i < iMac; i++)
			{
				m_ui.LvCur.Items[i].Checked = false;
			}
		}

		#endregion

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
			lvi.SubItems[2].Text = sli.Path;
			lvi.SubItems[1].Text = sli.Size.ToString("###,###,###");
			lvi.SubItems[0].Text = sli.Name;

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
				catch (Exception)
				{
					fTooLong = true;
				}
				if (fTooLong)
					plfiTooLong.Add(rgfi[i]);

				// Application.DoEvents();
			}

			if (fRecurse)
			{
				DirectoryInfo[] rgdi = di.GetDirectories();

				for (i = 0, iMac = rgdi.Length; i < iMac; i++)
					AddDirectory(rgdi[i], slis, sPattern, fRecurse, plfiTooLong);
			}
		}

		/* B U I L D  F I L E  L I S T */
		/*----------------------------------------------------------------------------
        	%%Function: BuildFileList
        	%%Qualified: SList.SListApp.BuildFileList
        	%%Contact: rlittle
        	
            Take the search path and build the file list (for the selected target)
        ----------------------------------------------------------------------------*/
		public void BuildFileList()
		{
			string sFileSpec = m_ui.GetSearchPath();
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
				sPattern = Path.GetFileName(m_ui.GetSearchPath());

				if (sPattern == "")
					sPattern = "*";
			}

			DirectoryInfo di = new DirectoryInfo(sPath);

			if (di == null)
			{
				MessageBox.Show("Path not found: " + sPath, "SList");
				return;
			}

			Cursor crsSav = m_ui.SetCursor(Cursors.WaitCursor);

			// stop redrawing
			m_ui.LvCur.BeginUpdate();

			// save off and reset the item sorter for faster adding
			IComparer lvicSav = m_ui.LvCur.ListViewItemSorter;
			m_ui.LvCur.ListViewItemSorter = null;

			m_ui.LvCur.Items.Clear();

			List<FileInfo> plfiTooLong = new List<FileInfo>();

			AddDirectory(di, m_ui.SlisCur, sPattern, m_ui.FRecurseChecked(), plfiTooLong);
			if (plfiTooLong.Count > 0)
			{
				MessageBox.Show(String.Format("Encountered {0} paths that were too long", plfiTooLong.Count));
			}

			pt.Stop();
			pt.Report(0, m_ui);

			m_ui.LvCur.EndUpdate();
			m_ui.LvCur.ListViewItemSorter = lvicSav;
			m_ui.LvCur.Update();
			m_ui.SetCursor(crsSav);
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

		internal void LoadFileListFromFile(SLISet slis)
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
			pt.Report(0, m_ui);
			tr.Close();
		}

		internal void SaveFileListToFile(SLISet slis)
		{
			string sFile;

			if (!InputBox.ShowInputBox("File list", out sFile))
				return;

			TextWriter tr = new StreamWriter(new FileStream(sFile, FileMode.CreateNew, FileAccess.Write), Encoding.Default);

			tr.WriteLine("[file.lst]"); // write something out so we know this is one of our files (we will parse it faster)
			foreach (ListViewItem lvi in slis.Lv.Items)
			{
				SLItem sli = (SLItem)lvi.Tag;
				tr.WriteLine("{0}\t{1}\t{2}", sli.Path, sli.Name, sli.Size);
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

			FileStream bs1 = new FileStream(Path.Combine(sli1.Path, sli1.Name), FileMode.Open, FileAccess.Read, FileShare.Read, 8, false);
			FileStream bs2 = new FileStream(Path.Combine(sli2.Path, sli2.Name), FileMode.Open, FileAccess.Read, FileShare.Read, 8, false);

			int lcb = 16;

			long icb = 0;
			int i;
			bool fProgress = true;
			m_ui.SetProgressBarMac(ProgressBarType.Current, sli1.Size);

			if (sli1.Size < 10000)
				fProgress = false;

			if (icb + lcb >= sli1.Size)
				lcb = (int)(sli1.Size - icb);

			m_ui.SetStatusText(sli1.Name);
			if (fProgress)
				m_ui.ShowProgressBar(ProgressBarType.Current);


			while (lcb > 0)
			{
				// Application.DoEvents();
				if (fProgress)
					m_ui.UpdateProgressBar(ProgressBarType.Current, icb, null);
				
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

						m_ui.UpdateProgressBar(ProgressBarType.Current, sli1.Size, null);
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
					if ((int)(sli1.Size - icb - 1) == 0)
						break;

					lcb *= 2;
					if (lcb > lcbMax)
						lcb = lcbMax;
				}

				if (icb + lcb >= sli1.Size)
					lcb = (int)(sli1.Size - icb - 1);

			}
			//		br1.Close();
			//		br2.Close();
			bs1.Close();
			bs2.Close();
			m_ui.UpdateProgressBar(ProgressBarType.Current, sli1.Size, null);
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
				rgsli[iFirst + i].IsMarked = false;
				rgsli[iFirst + i].IsDestOnly = fDestOnly;
			}
		}

		/*----------------------------------------------------------------------------
			%%Function: BuildUniqueFileList
			%%Qualified: SList.SListApp.BuildUniqueFileList

	    ----------------------------------------------------------------------------*/
		public void BuildUniqueFileList()
		{
			int start, end, sum = 0;
			int min = 999999, max = 0, c = 0;
			SLISet slisSrc = m_ui.GetSliSet(SListApp.s_ilvSource);
			int cItems = slisSrc.Lv.Items.Count + m_ui.GetSliSet(SListApp.s_ilvDest).Lv.Items.Count;
			SLItem[] rgsli = new SLItem[cItems];

			start = Environment.TickCount;

			AddSlisToRgsli(slisSrc, rgsli, 0, false);

			if (m_ui.GetSliSet(SListApp.s_ilvDest).Lv.Items.Count > 0)
			{
				AddSlisToRgsli(m_ui.GetSliSet(SListApp.s_ilvDest), rgsli, slisSrc.Lv.Items.Count, true);
			}
			Array.Sort(rgsli, new SLItemComparer(SLItem.SLItemCompare.CompareSize));

			slisSrc.Lv.BeginUpdate();
			slisSrc.Lv.Items.Clear();

			int i = 0;
			int iMac = rgsli.Length;

			m_ui.SetProgressBarMac(ProgressBarType.Overall, iMac);

			Cursor crsSav = m_ui.SetCursor(Cursors.WaitCursor);

			m_ui.ShowProgressBar(ProgressBarType.Overall);
			for (; i < iMac; i++)
			{
				int iDupe, iDupeMac;

				m_ui.UpdateProgressBar(ProgressBarType.Overall, i, null);

				if (rgsli[i].IsMarked)
					continue;

				if (rgsli[i].IsDestOnly)
					continue;

				// search forward for dupes
				for (iDupe = i + 1, iDupeMac = rgsli.Length; iDupe < iDupeMac; iDupe++)
				{
					if (rgsli[iDupe].IsMarked == true)
						continue;

					if (rgsli[i].Size == rgsli[iDupe].Size)
					{
						// do more extensive check here...for now, the size and the name is enough
						if (m_ui.FCompareFilesChecked())
						{
							c++;
							if (FCompareFiles(rgsli[i], rgsli[iDupe], ref min, ref max, ref sum))
							{
								if (rgsli[i].IsMarked == false)
									AddSliToListView(rgsli[i], slisSrc.Lv, true);

								if (rgsli[iDupe].IsMarked == false)
									AddSliToListView(rgsli[iDupe], slisSrc.Lv);

								rgsli[i].IsMarked = rgsli[iDupe].IsMarked = true;
								rgsli[i].AddDupeToChain(rgsli[iDupe]);
							}
						}
						else
						{
							if (rgsli[i].Name == rgsli[iDupe].Name)
							{
								if (rgsli[i].IsMarked == false)
									AddSliToListView(rgsli[i], slisSrc.Lv);

								if (rgsli[iDupe].IsMarked == false)
									AddSliToListView(rgsli[iDupe], slisSrc.Lv);

								rgsli[i].IsMarked = rgsli[iDupe].IsMarked = true;
								rgsli[i].AddDupeToChain(rgsli[iDupe]);
							}
						}
					}
					else
					{
						if (rgsli[i].IsMarked == false)
							// this was unique...
							AddSliToListView(rgsli[i], slisSrc.Lv, true);

						break; // no reason to continue if the lengths changed; we sorted by length
					}
				}
			}
			m_ui.HideProgressBar(ProgressBarType.Current);
			m_ui.HideProgressBar(ProgressBarType.Overall);
			if (m_ui.FCompareFilesChecked())
				m_ui.SetStatusText("Search complete.  Duplicates filtered by file compare.");
			else
				m_ui.SetStatusText("Search complete.  Duplicates filtered by size and name.");

			slisSrc.Lv.EndUpdate();
			m_ui.SetCursor(crsSav);
			end = Environment.TickCount;

			int len = end - start;
			if (c == 0)
				c = 1;

			int avg = len / c;
			int avg2 = sum / c;
			m_ui.SetStatusText(len.ToString() + "ms, (" + min.ToString() + ", " + max.ToString() + ", " + avg.ToString() + ", " + avg2.ToString() + ", " + c.ToString() + ")");
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
			foreach (ListViewItem lvi in m_ui.LvCur.Items)
			{
				SLItem sli = (SLItem)lvi.Tag;

				IEnumerator<string> e = (IEnumerator<string>)m_ui.GetPreferredPaths();

				foreach (String s in m_ui.GetPreferredPaths())
				{
					if (sli.MatchesPrefPath(s))
					{
						UpdateForPrefPath(sli, s, m_ui.FMarkFavored());
						break;
					}
				}
			}
		}

		public static void MoveSelectedFiles(ListView lvCur, string sDir, StatusBarPanel stbp)
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
				string sSource = Path.GetFullPath(Path.Combine(sli.Path, sli.Name));
				string sDest = Path.GetFullPath(Path.Combine(sDir, sli.Name));

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

		public static bool FRenameFile(string sPathOrig, string sFileOrig, string sPathNew, string sFileNew)
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

		internal void ApplyIgnoreList(string sIgnoreList)
		{
			SLISet slis = m_ui.SlisCur;

			int colSav = ((ListViewItemComparer)slis.Lv.ListViewItemSorter).GetColumn();
			((ListViewItemComparer)slis.Lv.ListViewItemSorter).SetColumn(-1);
			slis.Lv.Sort();

			// otherwise, we're loading a new list
			m_ign.LoadIgnoreList(sIgnoreList);

			// and apply the ignore list
			Application.DoEvents();
			int iMac = m_ign.IgnoreItems.Count;
			m_ui.SetProgressBarMac(ProgressBarType.Overall, iMac);
			m_ui.ShowProgressBar(ProgressBarType.Overall);

			slis.PauseListViewUpdate(false);
			for (int i = 0; i < iMac; i++)
			{
				m_ui.UpdateProgressBar(ProgressBarType.Overall, i, Application.DoEvents);
				RemovePath(slis, m_ign.IgnoreItems[i].PathPrefix);
			}

			m_ui.HideProgressBar(ProgressBarType.Overall);
			Application.DoEvents();
			slis.ResumeListViewUpdate(colSav);
		}

		public enum RegexOp
		{
			Match,
			Filter,
			Check
		};

		internal void DoRegex(RegexOp rop, string text)
		{
			Regex rx = null;

			try
			{
				rx = new Regex(text);
			}
			catch (Exception e)
			{
				MessageBox.Show("Could not compile Regular Expression '" + text + "':\n" + e.ToString(), "SLList");
				return;
			}


			int i, iMac;

			for (i = 0, iMac = m_ui.LvCur.Items.Count; i < iMac; i++)
			{
				SLItem sli = (SLItem)(m_ui.LvCur.Items[i].Tag);
				string sPath = Path.GetFullPath(Path.Combine(sli.Path, sli.Name));
				bool fMatch = false;

				fMatch = rx.IsMatch(sPath);

				switch (rop)
				{
					case RegexOp.Check:
						if (fMatch)
							m_ui.LvCur.Items[i].Checked = true;
						break;
					case RegexOp.Filter:
						if (fMatch)
						{
							m_ui.LvCur.Items[i].Remove();
							iMac--;
							i--;
						}
						break;
					case RegexOp.Match:
						if (!fMatch)
						{
							m_ui.LvCur.Items[i].Remove();
							iMac--;
							i--;
						}
						break;
				}
			}
		}

		public static string SCalcMatchingListViewItems(ListView lvCur, string sRegEx, string sCounts)
		{
			TextAtoms textAtoms = new TextAtoms(sRegEx);
			string sMatch = String.Format("Matches for '{0}':\n\n", sRegEx);

			int i, iMac;
			int cMatch = 0;

			for (i = 0, iMac = lvCur.Items.Count; i < iMac; i++)
			{
				SLItem sli = (SLItem)(lvCur.Items[i].Tag);

				if (sli.Atoms == null)
					sli.Atoms = new TextAtoms(sli.Name);

				int nMatch = 0;
				nMatch = sli.Atoms.NMatch(textAtoms);
				if (nMatch > 65)
				{
					sMatch += String.Format("{0:d3}% : '{1}'\n", nMatch, Path.GetFullPath(Path.Combine(sli.Path, sli.Name)), sRegEx);
					cMatch++;
				}
			}
			if (cMatch == 0 || MessageBox.Show(sMatch, "Matches", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				sCounts += String.Format("{0}\n", sRegEx);

			return sCounts;
		}

		internal void AddPreferredPath(string s)
		{
			m_ui.AddPreferredPath(s);
			AdjustListViewForFavoredPaths();
		}

		internal void RemovePath(SLISet slis, string sPathRoot)
		{
			slis.Remove(sPathRoot, m_ui);
		}

		#endregion // Core Model (Compare Files, etc)

		#region List View Commands

		internal void LaunchSli(SLItem sli)
		{
			Process.Start(Path.Combine(sli.Path, sli.Name));
		}

		#endregion // List View Commands

		public void UpdateForPrefPath(SLItem sliMaster, string s, bool fMark)
		{
			SLItem sli;

			sliMaster.IsMarked = fMark;
			UpdateMark(sliMaster);

			sli = sliMaster;

			while ((sli = sli.Prev) != null)
			{
				if (sli.MatchesPrefPath(s))
					sli.IsMarked = fMark;
				else
					sli.IsMarked = !fMark;

				UpdateMark(sli);
			}

			sli = sliMaster;

			while ((sli = sli.Next) != null)
			{
				if (sli.MatchesPrefPath(s))
					sli.IsMarked = fMark;
				else
					sli.IsMarked = !fMark;
				UpdateMark(sli);
			}
		}

		void UpdateMark(SLItem sli)
		{
			ListViewItem lvi = LviFromSli(sli);

			lvi.Checked = sli.IsMarked;
		}

		ListViewItem LviFromSli(SLItem sli)
		{
			foreach (ListViewItem lvi in m_ui.LvCur.Items)
			{
				if (lvi.Tag == sli)
					return lvi;
			}
			return null;
		}

		public void Select(SLItem sli)
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
				m_ui.LvCur.Select();
				return;
			}
			SystemSounds.Beep.Play();
		}

		// we might not be at the beginning of the dupe list for this item -- we might
		// have skipped over some IsDestOnly items, and those might be the dupes we
		// are looking for
		int FindFirstDupeCandidate(SLItem[] rgsli, int iCurrent)
		{
			// walk backwards until we change sizes or hit the beginning
			int i = iCurrent - 1;

			while (i >= 0 && rgsli[i].Size == rgsli[iCurrent].Size)
				i--;

			// we break on the first item that doesn't match...return
			// the next item
			return i + 1;
		}

		internal void BuildMissingFileList()
		{
			SLItem[] rgsli;
			int start, end, sum = 0;
			int min = 999999, max = 0, c = 0;
			SLISet slisSrc = m_ui.GetSliSet(SListApp.s_ilvSource);

			start = Environment.TickCount;

			int cItems = slisSrc.Lv.Items.Count + m_ui.GetSliSet(SListApp.s_ilvDest).Lv.Items.Count;

			rgsli = new SLItem[cItems];

			AddSlisToRgsli(slisSrc, rgsli, 0, false);

			if (m_ui.GetSliSet(SListApp.s_ilvDest).Lv.Items.Count > 0)
			{
				AddSlisToRgsli(m_ui.GetSliSet(SListApp.s_ilvDest), rgsli, slisSrc.Lv.Items.Count, true);
			}
			Array.Sort(rgsli, new SLItemComparer(SLItem.SLItemCompare.CompareSizeDest));

			slisSrc.Lv.BeginUpdate();
			slisSrc.Lv.Items.Clear();

			int i = 0;
			int iMac = rgsli.Length;

			m_ui.SetProgressBarMac(ProgressBarType.Overall, iMac);

			Cursor crsSav = m_ui.SetCursor(Cursors.WaitCursor);

			m_ui.ShowProgressBar(ProgressBarType.Overall);
			for (; i < iMac; i++)
			{
				int iDupe, iDupeMac;

				m_ui.UpdateProgressBar(ProgressBarType.Overall, i, null);

				if (rgsli[i].IsMarked)
					continue;

				if (rgsli[i].IsDestOnly)
					continue;

				iDupe = FindFirstDupeCandidate(rgsli, i);

				// search forward for dupes
				for (iDupeMac = rgsli.Length; iDupe < iDupeMac; iDupe++)
				{
					// don't compare against ourself
					if (iDupe == i)
						continue;

					// we are explicitly looking ONLY at fDestOnly files to see if there's a dupe
					// (used to include rgsli[iDupe].IsMarked == true  -- but why exclude
					// destonly files that were already duped against? a destonly file can be 
					// a dupe for multiple source files...
					if (rgsli[iDupe].IsDestOnly == false)
						continue;

					if (rgsli[i].Size == rgsli[iDupe].Size)
					{
						// do more extensive check here...for now, the size and the name is enough
						if (m_ui.FCompareFilesChecked())
						{
							c++;
							if (FCompareFiles(rgsli[i], rgsli[iDupe], ref min, ref max, ref sum))
							{
								// we found a dupe in the target. yay, don't add it anywhere
								rgsli[i].IsMarked = rgsli[iDupe].IsMarked = true;
								rgsli[iDupe].AddDupeToChain(rgsli[i]);
								break;
							}
						}
						else
						{
							if (rgsli[i].Name == rgsli[iDupe].Name)
							{
								// we found a dupe in the target.. nothing to add
								rgsli[i].IsMarked = true; //  rgsli[iDupe].IsMarked = true; // don't mark the dupe
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
				// in either case, if we found a dupe in the target, we will have marked IsMarked to be true...
				// if its not set, then we didn't find this file in the destination.
				if (rgsli[i].IsMarked == false)
					// this was unique...
					AddSliToListView(rgsli[i], slisSrc.Lv, true);


			}
			m_ui.HideProgressBar(ProgressBarType.Current);
			m_ui.HideProgressBar(ProgressBarType.Overall);
			if (m_ui.FCompareFilesChecked())
				m_ui.SetStatusText("Search complete.  Duplicates filtered by file compare.");
			else
				m_ui.SetStatusText("Search complete.  Duplicates filtered by size and name.");

			slisSrc.Lv.EndUpdate();
			m_ui.SetCursor(crsSav);
			end = Environment.TickCount;

			int len = end - start;
			if (c == 0)
				c = 1;

			int avg = len / c;
			int avg2 = sum / c;
			m_ui.SetStatusText(len.ToString() + "ms, (" + min.ToString() + ", " + max.ToString() + ", " + avg.ToString() + ", " + avg2.ToString() + ", " + c.ToString() + ")");
		}
	}
}