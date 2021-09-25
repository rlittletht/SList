using System.Collections;

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
		    %%Qualified: SList.SLISetViewItemComparer.Compare
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		public int Compare(object x, object y)
		{
			SLItem sli1 = (SLItem)x;
			SLItem sli2 = (SLItem)y;

			return SLItem.Compare(sli1, sli2, m_slic, false /*fReverse*/);
		}
	}
}