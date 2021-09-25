using System;
using System.Windows.Forms;

namespace SList
{
	public class RaiiWaitCursor : IDisposable
	{
		private Cursor m_cursor;
		private ISmartListUi m_ui; 

		public RaiiWaitCursor(ISmartListUi ui,  Cursor cursorNew)
		{
			m_cursor = ui.SetCursor(cursorNew);
			m_ui = ui;
		}

		public void Dispose()
		{
			m_ui.SetCursor(m_cursor);
		}
	}
}