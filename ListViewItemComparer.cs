using System.Collections;
using System.Windows.Forms;

namespace SList
{
	public class ListViewItemComparer : IComparer
	{
		private int m_col;
		private bool m_fReverse;

		/* L I S T  V I E W  I T E M  C O M P A R E R */
		/*----------------------------------------------------------------------------
		%%Function: ListViewItemComparer
		%%Qualified: SList.ListViewItemComparer.ListViewItemComparer
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
		public ListViewItemComparer()
		{
			m_col = 0;
			m_fReverse = false;
		}

		/* L I S T  V I E W  I T E M  C O M P A R E R */
		/*----------------------------------------------------------------------------
		%%Function: ListViewItemComparer
		%%Qualified: SList.ListViewItemComparer.ListViewItemComparer
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
		public ListViewItemComparer(int col)
		{
			m_col = col;
			m_fReverse = false;
		}

		/* S E T  C O L U M N */
		/*----------------------------------------------------------------------------
		%%Function: SetColumn
		%%Qualified: SList.ListViewItemComparer.SetColumn
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
		%%Qualified: SList.ListViewItemComparer.Compare
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
		public int Compare(object x, object y)
		{
			ListViewItem lvi1 = (ListViewItem)x;
			ListViewItem lvi2 = (ListViewItem)y;
			SLItem sli1 = (SLItem)lvi1.Tag;
			SLItem sli2 = (SLItem)lvi2.Tag;
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