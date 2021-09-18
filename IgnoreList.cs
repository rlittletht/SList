using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TCore.Settings;

namespace SList
{
	class IgnoreList // IGN
	{
		#region Internal types
		public class IgnoreListItem // IGNI
		{
			string m_sPathPrefix;

			public IgnoreListItem() { }

			public string PathPrefix
			{
				get { return m_sPathPrefix; }
				set { m_sPathPrefix = value; }
			}
		}

		private bool m_fDirtyListItems;
		private bool m_fDirtyList;
		List<IgnoreListItem> m_pligni;
		private List<string> m_plsIgnoreLists;
		private int m_iIgnoreListCur;
		#endregion

		#region Public methods
		public IgnoreList()
		{
			m_iIgnoreListCur = -1;
			m_pligni = new List<IgnoreListItem>();
		}

		public List<IgnoreListItem> IgnoreItems => m_pligni;

		public IEnumerable<string> IgnoreLists { get { return m_plsIgnoreLists; } }

		/* C R E A T E  I G N O R E  L I S T */
		/*----------------------------------------------------------------------------
        	%%Function: CreateIgnoreList
        	%%Qualified: SList.IgnoreList.CreateIgnoreList
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		public void CreateIgnoreList(string sIgnoreList, bool fCreateFromExisting)
		{
			EnsureListSaved();
			m_plsIgnoreLists.Add(sIgnoreList);
			if (!fCreateFromExisting)
				m_pligni = null;

			m_iIgnoreListCur = m_plsIgnoreLists.Count - 1;
			m_fDirtyList = true;
		}

		/* A D D  I G N O R E  P A T H */
		/*----------------------------------------------------------------------------
        	%%Function: AddIgnorePath
        	%%Qualified: SList.IgnoreList.AddIgnorePath
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		public void AddIgnorePath(string sPathPrefix)
		{
			m_pligni.Add(new IgnoreListItem() { PathPrefix = sPathPrefix });
			m_fDirtyListItems = true;
		}

		/* L O A D  I G N O R E  L I S T  N A M E S */
		/*----------------------------------------------------------------------------
        	%%Function: LoadIgnoreListNames
        	%%Qualified: SList.IgnoreList.LoadIgnoreListNames
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		public void LoadIgnoreListNames(string sRegRoot)
		{
			string[] rgs = Settings.RgsGetSubkeys(String.Format("{0}\\IgnoreLists", sRegRoot));

			m_plsIgnoreLists = new List<string>();
			if (rgs != null)
				m_plsIgnoreLists.AddRange(rgs);
		}

		/* E N S U R E  L I S T  S A V E D */
		/*----------------------------------------------------------------------------
        	%%Function: EnsureListSaved
        	%%Qualified: SList.IgnoreList.EnsureListSaved
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		public void EnsureListSaved()
		{
			if (m_fDirtyList)
				SaveIgnoreLists();

			if (m_fDirtyListItems && m_iIgnoreListCur != -1)
				SaveIgnoreList(m_plsIgnoreLists[m_iIgnoreListCur], m_pligni);

			m_fDirtyListItems = false;
		}
		/* L O A D  I G N O R E  L I S T */
		/*----------------------------------------------------------------------------
        	%%Function: LoadIgnoreList
        	%%Qualified: SList.IgnoreList.LoadIgnoreList
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		public void LoadIgnoreList(string sIgnoreListName)
		{
			EnsureListSaved();

			m_iIgnoreListCur = -1;
			for (int i = 0; i < m_plsIgnoreLists.Count; i++)
			{
				if (String.Compare(m_plsIgnoreLists[i], sIgnoreListName, false) == 0)
				{
					m_iIgnoreListCur = i;
					break;
				}
			}

			if (m_iIgnoreListCur == -1)
			{
				MessageBox.Show("Coulnd't find ignore list name!");
				return;
			}

			Settings ste = new Settings(_rgsteeIgnoreList, SGetListKey(sIgnoreListName), null);

			ste.Load();
			string[] rgs = ste.RgsValue("IgnorePaths");

			m_pligni = new List<IgnoreListItem>();

			foreach (string s in rgs)
			{
				AddIgnorePath(s);
			}

			m_fDirtyListItems = false;
		}

		#endregion

		#region Internal model

		/* S A V E  I G N O R E  L I S T S */
		/*----------------------------------------------------------------------------
        	%%Function: SaveIgnoreLists
        	%%Qualified: SList.IgnoreList.SaveIgnoreLists
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		void SaveIgnoreLists()
		{
			if (m_fDirtyList == false)
				return;

			for (int i = 0; i < m_plsIgnoreLists.Count; i++)
			{
				if (i == m_iIgnoreListCur && m_fDirtyListItems)
					SaveIgnoreList(m_plsIgnoreLists[i], m_pligni);
				else
					SaveIgnoreList(m_plsIgnoreLists[i], null);
			}

			m_fDirtyList = false;
		}

		/* S  G E T  L I S T  K E Y */
		/*----------------------------------------------------------------------------
        	%%Function: SGetListKey
        	%%Qualified: SList.IgnoreList.SGetListKey
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		string SGetListKey(string sIgnoreListName)
		{
			return String.Format("{0}\\IgnoreLists\\{1}", SmartList.s_sRegRoot, sIgnoreListName);
		}

		private Settings.SettingsElt[] _rgsteeIgnoreList =
			{
				new Settings.SettingsElt("IgnorePaths", Settings.Type.StrArray, new string[] {}, new string[] {}),
			};

		/* S A V E  I G N O R E  L I S T */
		/*----------------------------------------------------------------------------
        	%%Function: SaveIgnoreList
        	%%Qualified: SList.IgnoreList.SaveIgnoreList
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		void SaveIgnoreList(string sIgnoreListName, List<IgnoreListItem> pligni)
		{
			Settings ste = new Settings(_rgsteeIgnoreList, SGetListKey(sIgnoreListName), null);

			if (pligni != null)
			{
				List<string> plsPaths = new List<string>();
				foreach (IgnoreListItem igni in pligni)
					plsPaths.Add(igni.PathPrefix);

				ste.SetRgsValue("IgnorePaths", plsPaths.ToArray());
			}
			ste.Save();
		}
		#endregion
	}
}