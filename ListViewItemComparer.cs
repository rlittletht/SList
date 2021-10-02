using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SList
{
	public class SLISetViewItemComparer : IComparer<SLItem>
	{
		private int m_col;
		private bool m_fReverse;

		/* L I S T  V I E W  I T E M  C O M P A R E R */
		/*----------------------------------------------------------------------------
		%%Function: SLISetViewItemComparer
		%%Qualified: SList.SLISetViewItemComparer.SLISetViewItemComparer
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
		public SLISetViewItemComparer()
		{
			m_col = 0;
			m_fReverse = false;
		}

		/* L I S T  V I E W  I T E M  C O M P A R E R */
		/*----------------------------------------------------------------------------
		%%Function: SLISetViewItemComparer
		%%Qualified: SList.SLISetViewItemComparer.SLISetViewItemComparer
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
		public SLISetViewItemComparer(int col)
		{
			m_col = col;
			m_fReverse = false;
		}

		/* S E T  C O L U M N */
		/*----------------------------------------------------------------------------
		%%Function: SetColumn
		%%Qualified: SList.SLISetViewItemComparer.SetColumn
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
		public void SetColumn(int col)
		{
			if (m_col == col)
				m_fReverse = !m_fReverse;
			else
				m_fReverse = false;

			m_col = col;
		}

		public int GetColumn()
		{
			return m_col;
		}

		/* C O M P A R E */
		/*----------------------------------------------------------------------------
		%%Function: Compare
		%%Qualified: SList.SLISetViewItemComparer.Compare
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
		public int Compare(SLItem sli1, SLItem sli2)
		{
			int n = 0;

			switch (m_col)
			{
				case -1:
					n = SLItem.Compare(sli1, sli2, SLItem.SLItemCompare.CompareHashkey, m_fReverse);
					break;
				case 0:
					n = SLItem.Compare(sli1, sli2, SLItem.SLItemCompare.CompareName, m_fReverse);
					break;
				case 1:
					n = SLItem.Compare(sli1, sli2, SLItem.SLItemCompare.CompareType, m_fReverse);
					break;
				case 2:
					n = SLItem.Compare(sli1, sli2, SLItem.SLItemCompare.CompareSize, m_fReverse);
					break;
				case 3:
					n = SLItem.Compare(sli1, sli2, SLItem.SLItemCompare.ComparePath, m_fReverse);
					break;
			}

			return n;
		}
	}
}