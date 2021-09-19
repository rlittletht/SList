using System;
using System.Collections;
using System.IO;
using System.Globalization;

namespace SList
{
	public class SLItemComparer : IComparer
	{
		private SLItem.SLItemCompare m_slic;

		/* S  L  I T E M  C O M P A R E R */
		/*----------------------------------------------------------------------------
        	%%Function: SLItemComparer
        	%%Qualified: SList.SLItemComparer.SLItemComparer
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		public SLItemComparer()
		{
			m_slic = SLItem.SLItemCompare.CompareName;
		}

		/* S  L  I T E M  C O M P A R E R */
		/*----------------------------------------------------------------------------
        	%%Function: SLItemComparer
        	%%Qualified: SList.SLItemComparer.SLItemComparer
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		public SLItemComparer(SLItem.SLItemCompare slic)
		{
			m_slic = slic;
		}

		/* C O M P A R E */
		/*----------------------------------------------------------------------------
		    %%Function: Compare
		    %%Qualified: SList.ListViewItemComparer.Compare
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		public int Compare(object x, object y)
		{
			SLItem sli1 = (SLItem)x;
			SLItem sli2 = (SLItem)y;

			return SLItem.Compare(sli1, sli2, m_slic, false /*fReverse*/);
		}
	}

	// ============================================================================
	// S  L  I T E M
	// ============================================================================
	public class SLItem
	{
		public string Name { get; set; }
		public Int64 Size { get; }
		public string Path { get; }
		public bool IsMarked { get; set; }

		// some items are intended only to matched *against*, but we shouldn't treat them as unique.  
		// (i.e. drive old and drive new.  we don't care to find dupes for items on drive new on 
		// drive new; we only care about finding items from drive old that are on drive new.

		public bool IsDestOnly { get; set; }

		FileInfo m_fi;
		DirectoryInfo m_di;
		public TextAtoms Atoms { get; set; }

		SLItem m_sliPrev;
		SLItem m_sliNext;

		public int CompareTo(SLItem sli)
		{
			return String.Compare(Path, sli.Path);
		}

		public SLItem Prev => m_sliPrev;
		public SLItem Next => m_sliNext;

		public enum SLItemCompare
		{
			CompareName,
			CompareSize,
			ComparePath,
			CompareSizeDest,
			CompareHashkey
		}

		public string Hashkey => m_sFiName;

		private string m_sFiName;

		public SLItem(string sName, long lSize, string sPath, string sFiName)
		{
			Name = sName;
			Size = lSize;
			Path = sPath;
			m_sFiName = sFiName;
			m_di = null;
			m_fi = null;
			Atoms = null;
		}

		public SLItem(string sName, long lSize, string sPath, DirectoryInfo di)
		{
			Name = sName;
			Size = lSize;
			Path = sPath;
			m_di = di;
			m_fi = null;
			Atoms = null;
		}

		public SLItem(string sName, long lSize, string sPath, FileInfo fi)
		{
			Name = sName;
			Size = lSize;
			Path = sPath;
			m_sFiName = String.Format("{0}/{1}", sPath, fi.Name);
			m_fi = fi;
			m_di = null;
			Atoms = null;
		}

		CultureInfo ci = new CultureInfo("en-US");

		public void ClearDupeChain()
		{
			m_sliNext = m_sliPrev = null;
		}

		public bool MatchesPrefPath(string s)
		{
			if (Path.Length < s.Length)
				return false;

			if (Path.StartsWith(s, true /*ignoreCase*/, ci))
				return true;

			return false;
		}

		public void AddDupeToChain(SLItem sli)
		{
			if (sli.m_sliNext != null || sli.m_sliPrev != null)
				throw new Exception("Can't add an sli that already has dupes!");

			SLItem sliLast = this;

			while (sliLast.m_sliNext != null)
				sliLast = sliLast.m_sliNext;

			sliLast.m_sliNext = sli;
			sli.m_sliPrev = sliLast;
		}


		public static int Compare(SLItem sli1, SLItem sli2, SLItemCompare slic, bool fReverse)
		{
			int n = 0;

			switch (slic)
			{
				case SLItemCompare.CompareHashkey:
					n = String.Compare(sli1.Hashkey, sli2.Hashkey);
					if (n == 0)
					{
						// they are again equivalent; the difference is now file size
						n = (int)(sli1.Size - sli2.Size);

						if (n == 0)
						{
							// yeesh.  diff is now folder

							n = String.Compare(sli1.Path, sli2.Path);
						}
					}
					break;
				case SLItemCompare.CompareName:
					n = String.Compare(sli1.Name, sli2.Name);
					if (n == 0)
					{
						// they are again equivalent; the difference is now file size
						n = (int)(sli1.Size - sli2.Size);

						if (n == 0)
						{
							// yeesh.  diff is now folder

							n = String.Compare(sli1.Path, sli2.Path);
						}
					}
					break;
				case SLItemCompare.CompareSize:
					n = (int)(sli1.Size - sli2.Size);

					if (n == 0)
					{
						// they are the same; now look at the name
						n = String.Compare(sli1.Name, sli2.Name);

						if (n == 0)
						{
							// yeesh.  diff is now folder

							n = String.Compare(sli1.Path, sli2.Path);
						}
					}
					break;
				case SLItemCompare.CompareSizeDest:
					n = (int)(sli1.Size - sli2.Size);

					if (n == 0)
					{
						// they are the same; now look at the name
						n = String.Compare(sli1.Name, sli2.Name);

						if (n == 0)
						{
							if (sli1.IsDestOnly == sli2.IsDestOnly)
								n = 0;
							else if (sli1.IsDestOnly)
								n = 1;
							else
								n = -1;

							if (n == 0)
							{
								// yeesh.  diff is now folder

								n = String.Compare(sli1.Path, sli2.Path);
							}
						}
					}
					break;
				case SLItemCompare.ComparePath:
					n = String.Compare(sli1.Path, sli2.Path);

					if (n == 0)
					{
						// they are equivalent; the difference is now based on the name
						n = String.Compare(sli1.Name, sli2.Name);

						if (n == 0)
						{
							// they are again equivalent; the difference is now file size
							n = (int)(sli1.Size - sli2.Size);
						}
					}
					break;
			}

			if (fReverse)
				return -n;
			else
				return n;
		}

	};

}
