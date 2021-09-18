using System.Collections;
using System.Windows.Forms;

namespace SList
{
	public enum ProgressBarType
	{
		Current,
		Overall
	}

	public delegate void OnProgressUpdateDelegate();

	public interface ISmartListUi
	{
		Cursor SetCursor(Cursor cursor);
		void ShowProgressBar(ProgressBarType barType);
		void HideProgressBar(ProgressBarType barType);
		void SetProgressBarMac(ProgressBarType barType, long iMac);
		void UpdateProgressBar(ProgressBarType barType, long i, OnProgressUpdateDelegate del);
		bool FCompareFilesChecked();
		void SetStatusText(string text);
		void AddIgnoreListItem(string text);
		void ShowListView(int iListView);
		SLISet GetSliSet(int iListView);
		string GetSearchPath();
		bool FRecurseChecked();
		IEnumerable GetPreferredPaths();
		void AddPreferredPath(string path);
		bool FMarkFavored();

		ListView LvCur { get; }
		SLISet SlisCur { get; }

	}
}