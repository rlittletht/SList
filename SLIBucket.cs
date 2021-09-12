using System.Collections.Generic;

namespace SList
{
	public class SLIBucket
	{
		List<SLItem> m_plsli;
		string m_sKey;

		public SLIBucket(string sKey, SLItem sli)
		{
			m_plsli = new List<SLItem>();
			m_plsli.Add(sli);
			m_sKey = sKey;
		}

		public List<SLItem> Items
		{
			get { return m_plsli; }
		}

		public void Remove(SLItem sli)
		{
			int i = m_plsli.Count;

			while (--i >= 0)
			{
				if (m_plsli[i].CompareTo(sli) == 0)
				{
					m_plsli.RemoveAt(i);
					return;
				}
			}
		}

		public override string ToString()
		{
			return m_sKey;
		}
	}
}