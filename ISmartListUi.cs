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
		void SetCount(int count);
		void AddIgnoreListItem(string text);
		void ShowListView(SListApp.FileList fileList);
		SLISet GetSliSet(SListApp.FileList fileList);
		string GetSearchPath();
		bool FRecurseChecked();
		IEnumerable GetPreferredPaths();
		void AddPreferredPath(string path);
		bool FMarkFavored();
		string GetFileListDefaultName(SListApp.FileList fileList);
		void SetFileListDefaultName(SListApp.FileList fileList, string sDefault);

		ListView LvCur { get; }
		SLISet SlisCur { get; }

	}
}