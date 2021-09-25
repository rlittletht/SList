using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SList
{
	public class ProgressBarStatus
	{
		private ProgressBar m_bar;
		private int m_msecBeforeShow;
		private Stopwatch m_watch;
		
		public ProgressBarStatus(ProgressBar bar)
		{
			m_bar = bar;
		}

		public void SetOnDemandStatusBar(int msecBeforeShow)
		{
			m_msecBeforeShow = msecBeforeShow;
		}

		public void Show()
		{
			m_bar.Show();
		}

		public void Hide()
		{
			m_bar.Hide();
		}
		public long MacProgressBarOverall { get; set; }
		public long IncrementProgressBarOverall { get; set; }
		private long LastProgressBarOverall { get; set; }

		public void SetMacProgress(long mac)
		{
			m_msecBeforeShow = 0; // we always start as manual
			MacProgressBarOverall = mac;
			IncrementProgressBarOverall = Math.Max(1, mac / m_bar.Maximum);
			LastProgressBarOverall = 0;
			m_bar.Value = 0;
			m_watch = new Stopwatch();
			m_watch.Start();
		}

		public void Update(long i, OnProgressUpdateDelegate del)
		{
			if (m_msecBeforeShow != 0 && m_msecBeforeShow > m_watch.ElapsedMilliseconds)
				return;

			if (m_msecBeforeShow != 0)
			{
				m_bar.Show();
				m_msecBeforeShow = 0;
			}

			if (LastProgressBarOverall + IncrementProgressBarOverall < i)
			{
				m_bar.Value = Math.Min(m_bar.Maximum, (int)(i / IncrementProgressBarOverall));
				LastProgressBarOverall = m_bar.Value * IncrementProgressBarOverall;
				m_bar.Update();
				if (del != null)
					del();
			}
		}
	}
}