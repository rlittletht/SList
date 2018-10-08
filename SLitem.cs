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
        public string m_sName;
        public Int64  m_lSize;
        public string m_sPath;
        public bool m_fMarked;

        // some items are intended only to matched *against*, but we shouldn't treat them as unique.  
        // (i.e. drive old and drive new.  we don't care to find dupes for items on drive new on 
        // drive new; we only care about finding items from drive old that are on drive new.

        public bool m_fDestOnly;

        public FileInfo m_fi;
        public DirectoryInfo m_di;
        public ATMC m_atmc;

        SLItem m_sliPrev;
        SLItem m_sliNext;

        public int CompareTo(SLItem sli)
        {
            return String.Compare(m_sPath, sli.m_sPath);
        }

        public SLItem Prev
        {
            get { return m_sliPrev; }
        }

        public SLItem Next
        {
            get { return m_sliNext; }
        }

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

        public bool DestOnly { get { return m_fDestOnly; } set { m_fDestOnly = value; } }

        public SLItem(string sName, long lSize, string sPath, string sFiName)
        {
            m_sName = sName;
            m_lSize = lSize;
            m_sPath = sPath;
            m_sFiName = sFiName;
            m_di = null;
            m_fi = null;
            m_atmc = null;
        }

        public SLItem(string sName, long lSize, string sPath, DirectoryInfo di)
        {
            m_sName = sName;
            m_lSize = lSize;
            m_sPath = sPath;
            m_di = di;
            m_fi = null;
            m_atmc = null;
        }

        public SLItem(string sName, long lSize, string sPath, FileInfo fi)
        {
            m_sName = sName;
            m_lSize = lSize;
            m_sPath = sPath;
            m_sFiName = String.Format("{0}/{1}", sPath, fi.Name);
            m_fi = fi;
            m_di = null;
            m_atmc = null;
        }

        CultureInfo ci = new CultureInfo("en-US");

        public void ClearDupeChain()
        {
            m_sliNext = m_sliPrev = null;
        }

        public bool MatchesPrefPath(string s)
        {
            if (m_sPath.Length < s.Length)
                return false;

            if (m_sPath.StartsWith(s, true /*ignoreCase*/, ci))
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


        static public int Compare(SLItem sli1, SLItem sli2, SLItemCompare slic, bool fReverse)
        {
            int n = 0;

            switch (slic)
            {
                case SLItemCompare.CompareHashkey:
                    n = String.Compare(sli1.Hashkey, sli2.Hashkey);
                    if (n == 0)
                    {
                        // they are again equivalent; the difference is now file size
                        n = (int)(sli1.m_lSize - sli2.m_lSize);

                        if (n == 0)
                        {
                            // yeesh.  diff is now folder

                            n = String.Compare(sli1.m_sPath, sli2.m_sPath);
                        }
                    }
                    break;
                case SLItemCompare.CompareName:
                    n = String.Compare(sli1.m_sName, sli2.m_sName);
                    if (n == 0)
                    {
                        // they are again equivalent; the difference is now file size
                        n = (int)(sli1.m_lSize - sli2.m_lSize);

                        if (n == 0)
                        {
                            // yeesh.  diff is now folder

                            n = String.Compare(sli1.m_sPath, sli2.m_sPath);
                        }
                    }
                    break;
                case SLItemCompare.CompareSize:
                    n = (int)(sli1.m_lSize - sli2.m_lSize);

                    if (n == 0)
                    {
                        // they are the same; now look at the name
                        n = String.Compare(sli1.m_sName, sli2.m_sName);

                        if (n == 0)
                        {
                            // yeesh.  diff is now folder

                            n = String.Compare(sli1.m_sPath, sli2.m_sPath);
                        }
                    }
                    break;
                case SLItemCompare.CompareSizeDest:
                    n = (int)(sli1.m_lSize - sli2.m_lSize);

                    if (n == 0)
                    {
                        // they are the same; now look at the name
                        n = String.Compare(sli1.m_sName, sli2.m_sName);

                        if (n == 0)
                        {
                            if (sli1.DestOnly == sli2.DestOnly)
                                n = 0;
                            else if (sli1.DestOnly)
                                n = 1;
                            else
                                n = -1;

                            if (n == 0)
                            {
                                // yeesh.  diff is now folder

                                n = String.Compare(sli1.m_sPath, sli2.m_sPath);
                            }
                        }
                    }
                    break;
                case SLItemCompare.ComparePath:
                    n = String.Compare(sli1.m_sPath, sli2.m_sPath);

                    if (n == 0)
                    {
                        // they are equivalent; the difference is now based on the name
                        n = String.Compare(sli1.m_sName, sli2.m_sName);

                        if (n == 0)
                        {
                            // they are again equivalent; the difference is now file size
                            n = (int)(sli1.m_lSize - sli2.m_lSize);
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
