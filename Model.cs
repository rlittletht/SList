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
using NUnit.Framework.Internal;
using TCore.UI;
using TCore.XmlSettings;

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
			m_ui.ShowListView(SListApp.FileList.Source);
		}

		public SmartList() {}

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
		public void ChangeListViewSort(SLISetView view, int iColSort)
		{
			if (view.Comparer == null)
				view.Comparer = new SLISetViewItemComparer(iColSort);
			else
				view.Comparer.SetColumn(iColSort);

			view.Sort();
		}

		public void ToggleAllListViewItems(SLISetView view)
		{
			int i, iMac;

			for (i = 0, iMac = m_ui.ViewCur.Items.Count; i < iMac; i++)
			{
				m_ui.ViewCur.Check(i, !m_ui.ViewCur.Items[i].Checked);
			}
		}

		internal void UncheckAllListViewItems(SLISetView view)
		{
			int i, iMac;

			for (i = 0, iMac = m_ui.ViewCur.Items.Count; i < iMac; i++)
			{
				m_ui.ViewCur.Check(i, false);
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
		public static void AddSliToListView(SLItem sli, SLISetView view)
		{
			AddSliToListView(sli, view, false);
		}


		private static void AddSliToListView(SLItem sli, SLISetView view, bool fChecked, ISmartListUi ui = null)
		{
			view.Add(sli, fChecked);
			if (ui != null)
				ui.SetCount(view.Count);
		}

		private void AddDirectory(DirectoryInfo di, SLISet slis, string sPattern, bool fRecurse, List<FileInfo> plfiTooLong, bool fAppend)
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
						slis.Add(sli, fAppend);
					}
				}
				catch (Exception e)
				{
					MessageBox.Show($"exception: {e.Message}");
					fTooLong = true;
				}
				if (fTooLong && !di.FullName.Contains("WindowsApps"))
					plfiTooLong.Add(rgfi[i]);

				// Application.DoEvents();
			}

			if (fRecurse)
			{
				DirectoryInfo[] rgdi = di.GetDirectories();

				for (i = 0, iMac = rgdi.Length; i < iMac; i++)
					AddDirectory(rgdi[i], slis, sPattern, fRecurse, plfiTooLong, fAppend);
			}
		}

		/* B U I L D  F I L E  L I S T */
		/*----------------------------------------------------------------------------
        	%%Function: BuildFileList
        	%%Qualified: SList.SListApp.BuildFileList
        	%%Contact: rlittle
        	
            Take the search path and build the file list (for the selected target)
        ----------------------------------------------------------------------------*/
		public void BuildFileList(bool fAppend = false)
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
			m_ui.ViewCur.BeginUpdate();

			if (!fAppend)
				m_ui.SlisCur.Clear();

			List<FileInfo> plfiTooLong = new List<FileInfo>();

			AddDirectory(di, m_ui.SlisCur, sPattern, m_ui.FRecurseChecked(), plfiTooLong, fAppend);
			if (plfiTooLong.Count > 0)
			{
				MessageBox.Show(String.Format("Encountered {0} paths that were too long", plfiTooLong.Count));
			}

			pt.Stop();
			pt.Report(0, m_ui);

			m_ui.ViewCur.EndUpdate();
			m_ui.SetCount(m_ui.SlisCur.View.Items.Count);
			m_ui.ViewCur.UpdateAfterAdd();
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

#if broken
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
#endif // broken

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
			using (new RaiiWaitCursor(m_ui, Cursors.WaitCursor))
			{
				string sDefault = m_ui.GetFileListDefaultName(slis.FileListType);
				if (!InputBox.ShowInputBox("File list", sDefault, out string sFile))
					return;

				m_ui.SetFileListDefaultName(slis.FileListType, sFile);
				PerfTimer pt = new PerfTimer();
				pt.Start("load file list");
				slis.PauseListViewUpdate(true);

				if (sFile.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
				{
					SLISet.LoadFileListXml(slis, sFile);
					slis.View.UpdateAfterAdd();
				}
				else
				{
					using (FileStream fs = new FileStream(sFile, FileMode.Open, FileAccess.Read))
					{
						m_ui.SetProgressBarMac(ProgressBarType.Overall, fs.Length);
						m_ui.ShowProgressBar(ProgressBarType.Overall);
						// parse a directory listing and add 
						string sCurDirectory = null;

						using (StreamReader sr = new StreamReader(fs, Encoding.Default))
						{
							using (TextReader tr = sr)
							{
								string sLine = tr.ReadLine();
								bool fInternalFormat = sLine == "[file.lst]";

								while ((sLine = tr.ReadLine()) != null)
								{
									m_ui.UpdateProgressBar(ProgressBarType.Overall, sr.BaseStream.Position,
										Application.DoEvents);
									if (fInternalFormat)
									{
										ParseFileListLine(sLine, out string sPath, out string sName, out long nSize);
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
										if (sLine.Substring(24, 5) == "<DIR>"
										    || sLine.Substring(24, 10) == "<SYMLINKD>"
										    || sLine.Substring(24, 10) == "<JUNCTION>") // this is a directory
											continue;

										// ok, from [14,39] is the size, [40, ...] is filename
										Int64 nSize;
										bool fReparsePoint = false;

										if (sLine.Substring(24, 9) == "<SYMLINK>")
										{
											nSize = 0;
											fReparsePoint = true;
										}
										else
										{
											nSize = FileSizeFromDirectoryLine(sLine);
										}

										string sFileLine = FileNameFromDirectoryLine(sLine);

										if (fReparsePoint)
											sFileLine = sFileLine.Substring(0, sFileLine.LastIndexOf('[') - 1);

										SLItem sli = new SLItem(sFileLine, nSize, sCurDirectory,
											String.Concat(sCurDirectory, "/", sFileLine));

										if (fReparsePoint)
											SLItem.SetIsReparsePoint(sli, "true");

										slis.Add(sli);
									}
									else if (sLine.StartsWith(" Directory of "))
									{
										sCurDirectory = DirectoryNameFromDirectoryLine(sLine);
									}
									else if (sLine.Contains("Volume")
									         || sLine.Contains("File(s)")
									         || sLine.Contains("Total Files")
									         || sLine.Contains("bytes free"))
										continue;
									else
										throw new Exception($"cannot parse {sLine}");
								}

								tr.Close();
							}
						}
					}
				}

				slis.ResumeListViewUpdate();

				pt.Stop();
				pt.Report(0, m_ui);
			}
		}

		internal void SaveFileListToFile(SLISet slis)
		{
			using (new RaiiWaitCursor(m_ui, Cursors.WaitCursor))
			{
				string sFile;
				string sDefault = m_ui.GetFileListDefaultName(slis.FileListType);

				if (!InputBox.ShowInputBox("File list", sDefault, out sFile))
					return;

				m_ui.SetFileListDefaultName(slis.FileListType, sFile);

				SLISet.SaveFileListXml(slis, sFile);
			}
		}

#endregion

#region Core Model (Compare Files, etc)

		private bool FCompareFiles(SLItem sli1, SLItem sli2, ref int min, ref int max, ref int sum)
		{
			if (sli1.Size == 0 && !sli1.IsReparsePoint
			                   && sli2.Size == 0 && !sli2.IsReparsePoint)
			{
				return true;
			}

			if (sli1.CannotOpen || sli2.CannotOpen)
				return false;

			if (sli1.FCanCompareSha256(sli2))
				return sli1.IsEqualSha256(sli2);

			int nStart = Environment.TickCount;
			int nEnd;

			FileStream bs1 = null;
			FileStream bs2 = null;

			try
			{
				using (bs1 = new FileStream(Path.Combine(sli1.Path, sli1.Name), FileMode.Open, FileAccess.Read,
					FileShare.Read,
					8, false))
				{
					using (bs2 = new FileStream(Path.Combine(sli2.Path, sli2.Name), FileMode.Open, FileAccess.Read,
						FileShare.Read,
						8, false))
					{
						int lcb = 16;

						long icb = 0;
						int i;
						bool fProgress = true;
						m_ui.SetProgressBarMac(ProgressBarType.Current, sli1.Size);

						if (sli1.Size < 10000)
							fProgress = false;

						if (icb + lcb >= sli1.Size)
							lcb = (int) (sli1.Size - icb);

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
								if ((int) (sli1.Size - icb - 1) == 0)
									break;

								lcb *= 2;
								if (lcb > lcbMax)
									lcb = lcbMax;
							}

							if (icb + lcb >= sli1.Size)
								lcb = (int) (sli1.Size - icb - 1);

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
				}
			}
			catch (Exception e)
			{
				if (bs1 == null)
					sli1.CannotOpen = true;
				if (bs2 == null && bs1 != null)
					sli2.CannotOpen = true;

				if (bs1 != null)
					bs1.Close();
				if (bs2 != null)
					bs2.Close();

				return false;
				MessageBox.Show(
					$"failed to compare {sli1.Path}\\{sli1.Name} with {sli2.Path}\\{sli2.Name}. If this matters, write the name down so you can deal with it manually. Otherwise, we're going to assume they match\n\n{e.Message}",
					"FAILED TO COMPARE");
				return true;
			}
		}

		void AddSlisToRgsli(SLISet slis, SLItem[] rgsli, int iFirst, bool fDestOnly)
		{
			int i, iMac;

			for (i = 0, iMac = slis.View.Items.Count; i < iMac; i++)
			{
				rgsli[iFirst + i] = slis.View.Items[i];
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
			using (new RaiiWaitCursor(m_ui, Cursors.WaitCursor))
			{
				int start, end, sum = 0;
				int min = 999999, max = 0, c = 0;
				SLISet slisSrc = m_ui.GetSliSet(SListApp.FileList.Source);
				int cItems = slisSrc.View.Items.Count + m_ui.GetSliSet(SListApp.FileList.Destination).View.Items.Count;
				SLItem[] rgsli = new SLItem[cItems];

				start = Environment.TickCount;

				AddSlisToRgsli(slisSrc, rgsli, 0, false);

				if (m_ui.GetSliSet(SListApp.FileList.Destination).View.Items.Count > 0)
				{
					AddSlisToRgsli(m_ui.GetSliSet(SListApp.FileList.Destination), rgsli, slisSrc.View.Items.Count,
						true);
				}

				Array.Sort(rgsli, new SLItemComparer(SLItem.SLItemCompare.CompareSize));

				slisSrc.View.BeginUpdate();
				slisSrc.View.Items.Clear();

				int i = 0;
				int iMac = rgsli.Length;

				m_ui.SetProgressBarMac(ProgressBarType.Overall, iMac);

				Cursor crsSav = m_ui.SetCursor(Cursors.WaitCursor);

				m_ui.ShowProgressBar(ProgressBarType.Overall);
				for (; i < iMac; i++)
				{
					int iDupe, iDupeMac;

					m_ui.UpdateProgressBar(ProgressBarType.Overall, i, Application.DoEvents);

					if (rgsli[i].IsMarked)
						continue;

					// search forward for dupes
					for (iDupe = i + 1, iDupeMac = rgsli.Length; iDupe < iDupeMac; iDupe++)
					{
						if (rgsli[iDupe].IsMarked == true)
							continue;

						if (rgsli[i].Size == rgsli[iDupe].Size)
						{
							if (m_ui.FCompareFilesChecked())
							{
								c++;
								if (FCompareFiles(rgsli[i], rgsli[iDupe], ref min, ref max, ref sum))
								{
									if (rgsli[i].IsMarked == false && !rgsli[i].IsDestOnly)
										AddSliToListView(rgsli[i], slisSrc.View, true);

									if (rgsli[iDupe].IsMarked == false && !rgsli[iDupe].IsDestOnly)
										AddSliToListView(rgsli[iDupe], slisSrc.View);

									rgsli[i].IsMarked = rgsli[iDupe].IsMarked = true;
									rgsli[i].AddDupeToChain(rgsli[iDupe]);
								}
							}
							else
							{
								if (rgsli[i].Name == rgsli[iDupe].Name)
								{
									if (rgsli[i].IsMarked == false && !rgsli[i].IsDestOnly)
										AddSliToListView(rgsli[i], slisSrc.View);

									if (rgsli[iDupe].IsMarked == false && !rgsli[iDupe].IsDestOnly)
										AddSliToListView(rgsli[iDupe], slisSrc.View);

									rgsli[i].IsMarked = rgsli[iDupe].IsMarked = true;
									rgsli[i].AddDupeToChain(rgsli[iDupe]);
								}
							}
						}
						else
						{
							if (rgsli[i].IsMarked == false && !rgsli[i].IsDestOnly)
								// this was unique...
								AddSliToListView(rgsli[i], slisSrc.View, true);

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

				slisSrc.View.EndUpdate();
				m_ui.SetCount(m_ui.SlisCur.View.Items.Count);
				m_ui.SetCursor(crsSav);
				end = Environment.TickCount;

				int len = end - start;
				if (c == 0)
					c = 1;

				int avg = len / c;
				int avg2 = sum / c;
				m_ui.SetStatusText(len.ToString() + "ms, (" + min.ToString() + ", " + max.ToString() + ", " +
				                   avg.ToString() + ", " + avg2.ToString() + ", " + c.ToString() + ")");
			}
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
			foreach (SLItem sli in m_ui.ViewCur.Items)
			{
				IEnumerator<string> e = (IEnumerator<string>)m_ui.GetPreferredPaths();

				foreach (String s in m_ui.GetPreferredPaths())
				{
					if (sli.MatchesPathPrefix(s))
					{
						UpdateForPrefPath(sli, s, m_ui.FMarkFavored());
						break;
					}
				}
			}
		}

		public static string BuildRobocopyTargetPathFromPaths(string sourcePath, string destPath)
		{
			if (sourcePath == "")
				return destPath;

			string fullPath = sourcePath;

			if (fullPath[1] == ':')
				fullPath = fullPath.Substring(2);

			if (fullPath[0] == '\\')
				fullPath = fullPath.Substring(1);

			return Path.Combine(destPath, fullPath);
		}

		[TestCase("", "d:\\target", "d:\\target")]
		[TestCase("source", "d:\\target", "d:\\target\\source")]
		[TestCase("foo\\source", "d:\\target", "d:\\target\\foo\\source")]
		[TestCase("\\foo\\source", "d:\\target", "d:\\target\\foo\\source")]
		[TestCase("c:\\foo\\source", "d:\\target with a space", "d:\\target with a space\\foo\\source")]
		[TestCase("c:\\foo\\source with a space", "d:\\target", "d:\\target\\foo\\source with a space")]
		[TestCase("c:\\foo\\source", "d:\\target", "d:\\target\\foo\\source")]
		[Test]
		public static void TestBuildRobocopyTargetPathFromPaths(string source, string dest, string expected)
		{
			Assert.AreEqual(expected, BuildRobocopyTargetPathFromPaths(source, dest));
		}

		public static void DoMoveFileOperation_Script(TextWriter writer, string sourcePath, string sourceFile, string destPath)
		{
			writer.WriteLine($"robocopy \"{sourcePath}\" \"{destPath}\" \"sourceFile\"");
		}

		public static void CopySelectedFiles(SLISetView view, string sDir, string sScript, StatusBarPanel stbp)
		{
			if (sScript == null)
				throw new Exception("nyi");

			using (TextWriter tw = new StreamWriter(sScript))
			{
				IterateOverSelectedFiles(view, sDir, stbp,
					(string sourcePath, string destPath, string filename) =>
					{
						destPath = BuildRobocopyTargetPathFromPaths(sourcePath, destPath);
						tw.WriteLine($"robocopy \"{sourcePath}\" \"{destPath}\" \"{filename}\"");
						return IterateReturnVal.Succeed;
					});
			}
		}

		enum IterateReturnVal
		{
			Retry,
			Fail,
			Skipped,
			Succeed
		}

		delegate IterateReturnVal IterateDoAction(string sourcePath, string destPath, string filename);

		static void IterateOverSelectedFiles(SLISetView view, string sDestPath, StatusBarPanel stbp, IterateDoAction del)
		{
			// ok, iterate through all the items and find the ones that are checked
			int i, iMac;

			for (i = 0, iMac = view.Items.Count; i < iMac; i++)
			{
				if (!view.Items[i].Checked)
					continue;

				SLItem sli = view.Items[i];

				IterateReturnVal result;
				int n = 0;
				string sDest = sli.Name;

				stbp.Text = $"Processing {sli.Path} -> {sDest}";

				// now, see if sDest already exists.  if it does, we need to try
				// to rename the file
				while ((result = del(sli.Path, sDestPath, sDest)) == IterateReturnVal.Retry)
				{
					sDest = Path.GetFileNameWithoutExtension(sli.Name) + $"({n})" +
					        Path.GetExtension(sli.Name);
					n++;

					if (n >= 1020)
					{
						MessageBox.Show(
							"Cannot process " + sli.Name +
							".  There are too many duplicates in the destination.", "SList");
						break; // stop trying...
					}
				}

				if (result == IterateReturnVal.Skipped)
				{
					stbp.Text = $"Skipped identity move: {sli.Path} {sli.Name}";
					continue;
				}

				view.Items[i].Checked = false;
			}
		}

		public static void MoveSelectedFiles(SLISetView view, string sDir, StatusBarPanel stbp)
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

			IterateOverSelectedFiles(view, sDir, stbp,
				(string sourcePath, string destPath, string filename) =>
				{
					string sSource = Path.GetFullPath(Path.Combine(sourcePath, filename));
					string sDest = Path.GetFullPath(Path.Combine(destPath, filename));

					if (String.Compare(sSource, sDest, true /*ignoreCase*/) == 0)
						return IterateReturnVal.Skipped;

					if (File.Exists(sDest))
						return IterateReturnVal.Retry;

					File.Move(sSource, sDest);
					return IterateReturnVal.Succeed;
				});
		}

		public static void MoveSelectedFiles_Old(ListView lvCur, string sDir, StatusBarPanel stbp)
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

			int colSav = ((SLISetViewItemComparer)slis.View.ItemSorter).GetColumn();
			((SLISetViewItemComparer)slis.View.ItemSorter).SetColumn(-1);
			slis.View.Sort();

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

			for (i = 0, iMac = m_ui.ViewCur.Items.Count; i < iMac; i++)
			{
				SLItem sli = (SLItem)(m_ui.ViewCur.Items[i]);
				string sPath = Path.GetFullPath(Path.Combine(sli.Path, sli.Name));
				bool fMatch = false;

				fMatch = rx.IsMatch(sPath);

				switch (rop)
				{
					case RegexOp.Check:
						if (fMatch)
							m_ui.ViewCur.Items[i].Checked = true;
						break;
					case RegexOp.Filter:
						if (fMatch)
						{
							m_ui.ViewCur.Remove(i);
							iMac--;
							i--;
						}
						break;
					case RegexOp.Match:
						if (!fMatch)
						{
							m_ui.ViewCur.Remove(i);
							iMac--;
							i--;
						}
						break;
				}
			}
		}

		public static string SCalcMatchingListViewItems(SLISetView view, string sRegEx, string sCounts)
		{
			TextAtoms textAtoms = new TextAtoms(sRegEx);
			string sMatch = String.Format("Matches for '{0}':\n\n", sRegEx);

			int i, iMac;
			int cMatch = 0;

			for (i = 0, iMac = view.Items.Count; i < iMac; i++)
			{
				SLItem sli = view.Items[i];

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
			m_ui.SetCount(m_ui.ViewCur.Items.Count);
		}

		internal void RemoveType(SLISet slis, string sMenuText, FilePatternInfo typeInfo)
		{
			slis.RemoveType(typeInfo, m_ui);
			m_ui.SetCount(m_ui.ViewCur.Items.Count);
		}

		internal void RemovePattern(SLISet slis, string sMenuText, FilePatternInfo typeInfo)
		{
			slis.RemovePattern(typeInfo, m_ui);
			m_ui.SetCount(m_ui.ViewCur.Items.Count);
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
				if (sli.MatchesPathPrefix(s))
					sli.IsMarked = fMark;
				else
					sli.IsMarked = !fMark;

				UpdateMark(sli);
			}

			sli = sliMaster;

			while ((sli = sli.Next) != null)
			{
				if (sli.MatchesPathPrefix(s))
					sli.IsMarked = fMark;
				else
					sli.IsMarked = !fMark;
				UpdateMark(sli);
			}
		}

		void UpdateMark(SLItem sli)
		{
			m_ui.ViewCur.UpdateMark(sli);
		}

		public void Select(SLItem sli)
		{
			if (sli == null)
			{
				SystemSounds.Beep.Play();
				return;
			}

			m_ui.ViewCur.Select(sli);
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
			SLISet slisSrc = m_ui.GetSliSet(SListApp.FileList.Source);

			start = Environment.TickCount;

			int cItems = slisSrc.View.Items.Count + m_ui.GetSliSet(SListApp.FileList.Destination).View.Items.Count;

			rgsli = new SLItem[cItems];

			AddSlisToRgsli(slisSrc, rgsli, 0, false);

			if (m_ui.GetSliSet(SListApp.FileList.Destination).View.Items.Count > 0)
			{
				AddSlisToRgsli(m_ui.GetSliSet(SListApp.FileList.Destination), rgsli, slisSrc.View.Items.Count, true);
			}
			Array.Sort(rgsli, new SLItemComparer(SLItem.SLItemCompare.CompareSizeDest));

			slisSrc.View.BeginUpdate();
			slisSrc.View.Clear();

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
					AddSliToListView(rgsli[i], slisSrc.View, true);


			}
			m_ui.HideProgressBar(ProgressBarType.Current);
			m_ui.HideProgressBar(ProgressBarType.Overall);
			if (m_ui.FCompareFilesChecked())
				m_ui.SetStatusText("Search complete.  Duplicates filtered by file compare.");
			else
				m_ui.SetStatusText("Search complete.  Duplicates filtered by size and name.");

			slisSrc.View.EndUpdate();
			m_ui.SetCount(m_ui.SlisCur.View.Items.Count);
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