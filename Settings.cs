using TCore.Settings;

namespace SList
{
	public class SmartListSettings
	{
		[Setting("SourceSearchPath", "", "")]			public string SourceSearchPath { get; set; }
		[Setting("RecurseSourceSearch", false, 0)]		public bool RecurseSourceSearch { get; set; }
		[Setting("DestinationSearchPath", "", "")]		public string DestinationSearchPath { get; set; }
		[Setting("RecurseDestinationSearch", false, 0)]	public bool RecurseDestinationSearch { get; set; }
		[Setting("AutomaticallyAddIgnore", false, 0)]	public bool AutomaticallyAddIgnore { get; set; }
		[Setting("MarkFavored", false, 0)]				public bool MarkFavored { get; set; }
		[Setting("SaveMePath", "", "")]					public string SaveMePath { get; set; }
		[Setting("RetireMePath", "", "")]				public string RetireMePath { get; set; }
		[Setting("ScriptPath", "", "")]					public string ScriptPath { get; set; }
		[Setting("CopyTargetPath", "", "")]				public string CopyTargetPath { get; set; }
		[Setting("MoveTargetPath", "", "")]				public string MoveTargetPath { get; set; }
		[Setting("SourceFilesListDefault", "", "")]		public string SourceFilesListDefault { get; set; }
		[Setting("DestFilesListDefault", "", "")]		public string DestFilesListDefault { get; set; }
		[Setting("RealFileDiffing", false, 0)]			public bool RealFileDiffing { get; set; }

		[Setting("PreferredPathListDefault", "", "")]	public string PreferredPathListDefault { get; set; }

		#region Model
		private Settings m_settings;

		public SmartListSettings()
		{
			string sRoot = $"Software\\Thetasoft\\SList";
			m_settings = new Settings(Settings.SettingsElt.CreateSettings<SmartListSettings>(), sRoot, sRoot);
		}

		public void Load()
		{
			m_settings.Load();
			m_settings.SynchronizeGetValues(this);
		}

		public void Save()
		{
			m_settings.SynchronizeSetValues(this);
			m_settings.Save();
		}
		#endregion

	}
}