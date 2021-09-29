using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq.Expressions;
using NUnit.Framework;
using TCore.UI;

namespace SList
{
	public partial class SListApp : System.Windows.Forms.Form, ISmartListUi
	{
		#region Designer definitions

		private System.Windows.Forms.ListView m_lv;
		private System.Windows.Forms.TextBox m_ebSearchPath;
		private System.Windows.Forms.Button m_pbSearch;
		private System.Windows.Forms.Label m_lblSearch;
		private System.Windows.Forms.CheckBox m_cbRecurse;
		private System.Windows.Forms.Label m_lblFilterBanner;
		private System.Windows.Forms.Button m_pbDuplicates;
		private System.Windows.Forms.Label m_lblSearchCriteria;
		private System.Windows.Forms.CheckBox m_cbCompareFiles;
		private System.Windows.Forms.StatusBar m_stb;
		private System.Windows.Forms.ProgressBar m_prbar;
		private System.Windows.Forms.StatusBarPanel m_stbpMainStatus;
		private System.Windows.Forms.StatusBarPanel m_stbpFilterStatus;
		private System.Windows.Forms.StatusBarPanel m_stbpSearch;
		private System.Windows.Forms.StatusBarPanel m_stbpCount;
		private System.Windows.Forms.Label m_lblActions;
		private System.Windows.Forms.Button m_pbMove;
		private System.Windows.Forms.Button m_pbDelete;
		private System.Windows.Forms.Button m_pbToggle;
		private System.Windows.Forms.Button m_pbClear;
		private System.Windows.Forms.TextBox m_ebRegEx;
		private System.Windows.Forms.Label m_lblRegEx;
		private System.Windows.Forms.Label m_lblMoveTo;
		private System.Windows.Forms.TextBox m_ebMovePath;
		private System.Windows.Forms.Button m_pbMatchRegex;
		private System.Windows.Forms.Button m_pbRemoveRegex;
		private System.Windows.Forms.Button m_pbCheckRegex;
		private System.Windows.Forms.ContextMenu m_cxtListView;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.ProgressBar m_prbarOverall;

		private System.Windows.Forms.Button m_pbSmartMatch;
		private System.Windows.Forms.Timer m_tmr;
		private ListBox m_lbPrefPath;
		private Button m_pbRemove;
		private Button m_pbAddPath;
		private MenuItem menuItem2;
		private MenuItem menuItem3;
		private CheckBox m_cbMarkFavored;
		private MenuItem menuItem4;
		private MenuItem menuItem5;
		private Button m_pbValidateSrc;
		private MenuItem menuItem6;
		private MenuItem menuItem7;
		private ComboBox m_cbxIgnoreList;
		private Label label3;
		private CheckBox m_cbAddToIgnoreList;
		private Button m_pbSaveList;
		private Button m_pbLoadFromFile;
		private Button m_pbSaveFileList;
		private System.ComponentModel.IContainer components;

		#endregion

		private SLISet[] m_rgslis;

		public enum FileList
		{
			Source = 0,
			Destination = 1
		}

		private ToolTip m_toolTip;

		public SLISetView ViewCur => SlisCur.View;
		public SLISet SlisCur => m_rgslis[m_islisCur];

		#region AppHost

		private SmartList m_model;
		private Button button1;
		private RadioButton radioButton1;
		private RadioButton radioButton2;
		private Panel panel1;
		private Label label2;
		private TextBox m_ebCopyPath;
		private Button button2;
		private CheckBox m_cbGenerateScript;
		private Label label6;
		private TextBox m_ebScript;
		private MenuItem menuItem8;
		private MenuItem menuItem9;
		private MenuItem menuItem10;
		private MenuItem menuItem11;
		private Label label1;
		private Button m_pbLoadPreferredPaths;
		private Button m_pbSavePreferredPaths;
		private Button button3;
		private Button button4;
		private Button button5;
		private SmartListSettings m_settings;

		public SListApp()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			// Clear the text for the controls that have their own paint...
			m_lblFilterBanner.Text = "";
			m_lblSearchCriteria.Text = "";
			m_lblActions.Text = "";
			
			m_progressBarStatusOverall = new ProgressBarStatus(m_prbarOverall);
			m_progressBarStatusCurrent = new ProgressBarStatus(m_prbar);

			InitializeListViews();

			m_settings = new SmartListSettings();
			m_settings.Load();
			SyncUiWithSettings();

			m_model = new SmartList(this);

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		void SyncUiWithSettings()
		{
			m_cbAddToIgnoreList.Checked = m_settings.AutomaticallyAddIgnore;
			m_rgslis[s_ilvSource].PathSpec = m_settings.SourceSearchPath;
			m_rgslis[s_ilvSource].Recurse = m_settings.RecurseSourceSearch;
			m_rgslis[s_ilvDest].PathSpec = m_settings.DestinationSearchPath;
			m_rgslis[s_ilvDest].Recurse = m_settings.RecurseDestinationSearch;
			if (m_islisCur == s_ilvSource)
			{
				m_ebSearchPath.Text = m_settings.SourceSearchPath;
				m_cbRecurse.Checked = m_settings.RecurseSourceSearch;
			}
			else if (m_islisCur == s_ilvDest)
			{
				m_ebSearchPath.Text = m_settings.DestinationSearchPath;
				m_cbRecurse.Checked = m_settings.RecurseDestinationSearch;
			}
			m_cbMarkFavored.Checked = m_settings.MarkFavored;
			m_ebCopyPath.Text = m_settings.CopyTargetPath;
			m_ebScript.Text = m_settings.ScriptPath;
			m_ebMovePath.Text = m_settings.MoveTargetPath;
			m_cbCompareFiles.Checked = m_settings.RealFileDiffing;
		}

		void SyncSettingsWithUi()
		{
			m_settings.AutomaticallyAddIgnore = m_cbAddToIgnoreList.Checked;
			m_settings.SourceSearchPath = m_rgslis[s_ilvSource].PathSpec;
			m_settings.RecurseSourceSearch = m_rgslis[s_ilvSource].Recurse;
			m_settings.DestinationSearchPath = m_rgslis[s_ilvDest].PathSpec;
			m_settings.RecurseDestinationSearch = m_rgslis[s_ilvDest].Recurse;

			if (m_islisCur == s_ilvSource)
			{
				m_settings.SourceSearchPath = m_ebSearchPath.Text;
				m_settings.RecurseSourceSearch = m_cbRecurse.Checked;
			}
			else if (m_islisCur == s_ilvDest)
			{
				m_settings.DestinationSearchPath = m_ebSearchPath.Text;
				m_settings.RecurseDestinationSearch = m_cbRecurse.Checked;
			}

			m_settings.MarkFavored = m_cbMarkFavored.Checked;
			m_settings.CopyTargetPath = m_ebCopyPath.Text;
			m_settings.ScriptPath = m_ebScript.Text;
			m_settings.MoveTargetPath = m_ebMovePath.Text;
			m_settings.RealFileDiffing = m_cbCompareFiles.Checked;
		}
		// the designer initializes m_lv.  this will become m_rglv[s_ilvSource], and m_lv will be set to null. this allows us to create the templates
		// for all the list views in the designer and still have our switchable list views
		void InitializeListViews()
		{
			m_rgslis = new SLISet[s_clvMax];

			m_rgslis[s_ilvSource] = new SLISet(FileList.Source, m_lv, this);
			ListView lvSource = m_lv;

			m_lv = null;

			for (int ilv = 0; ilv < s_clvMax; ilv++)
			{
				if (ilv == s_ilvSource)
					continue; // skip, this is already initialized

				ListView lv = new System.Windows.Forms.ListView();
				lv.VirtualMode = true;
				lv.Anchor = lvSource.Anchor;
				lv.CheckBoxes = lvSource.CheckBoxes;
				lv.Font = lvSource.Font;

				lv.ContextMenu = lvSource.ContextMenu;
				lv.Location = lvSource.Location;
				lv.Name = String.Format("m_rglv{0}", ilv);
				lv.Size = lvSource.Size;
				lv.TabIndex = lvSource.TabIndex;
				lv.UseCompatibleStateImageBehavior = lvSource.UseCompatibleStateImageBehavior;
				//m_rglv[ilv].AfterLabelEdit += m_rglv[s_ilvSource].AfterLabelEdit;
				lv.Visible = false;
				this.Controls.Add(lv);
				m_rgslis[ilv] = new SLISet((FileList) ilv, lv, this);
			}
		}

		private int m_islisCur = -1;

		/* S Y N C  S E A R C H  T A R G E T */
		/*----------------------------------------------------------------------------
			%%Function: SyncSearchTargetUI
			%%Qualified: SList.SListApp.SyncSearchTargetUI

			make the UI reflect what we want the sync target to be. Typically used
			on initialization
        ----------------------------------------------------------------------------*/
		void SyncSearchTargetUI(FileList fileList)
		{
			fSyncing = true;
			radioButton1.Checked = fileList == FileList.Source;
			radioButton2.Checked = fileList == FileList.Destination;
			fSyncing = false;
		}

		int IlvFromFileList(FileList fileList)
		{
			return (int) fileList;
		}

		public FileList CurrentFileList()
		{
			if (radioButton1.Checked)
				return FileList.Source;
			else if (radioButton2.Checked)
				return FileList.Destination;

			throw new Exception("no file list selected");
		}

		private static int s_ilvSource = 0;
		private static int s_ilvDest = 1;
		private static int s_clvMax = 2;
		private bool m_fSyncingSearchTarget = false;

		public void ShowListView(FileList fileList)
		{
			int ilv = IlvFromFileList(fileList);

			if (m_fSyncingSearchTarget)
				return;

			if (m_islisCur != -1)
			{
				m_rgslis[m_islisCur].PathSpec = m_ebSearchPath.Text;
				m_rgslis[m_islisCur].Recurse = m_cbRecurse.Checked;
			}

			for (int i = 0; i < s_clvMax; i++)
			{
				m_rgslis[i].View.Visible = (i == ilv);
			}
			m_islisCur = ilv;

			m_fSyncingSearchTarget = true;
			SyncSearchTargetUI(fileList);
			m_fSyncingSearchTarget = false;
			m_ebSearchPath.Text = m_rgslis[ilv].PathSpec;
			m_cbRecurse.Checked = m_rgslis[ilv].Recurse;
			SetCount(ViewCur.Count);
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.Run(new SListApp());
		}

		#endregion

		#region Destruction

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		/* I N I T I A L I Z E  C O M P O N E N T */
		/*----------------------------------------------------------------------------
		%%Function: InitializeComponent
		%%Qualified: SList.SListApp.InitializeComponent
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.m_cxtListView = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			this.menuItem11 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.m_ebSearchPath = new System.Windows.Forms.TextBox();
			this.m_pbSearch = new System.Windows.Forms.Button();
			this.m_lblSearch = new System.Windows.Forms.Label();
			this.m_cbRecurse = new System.Windows.Forms.CheckBox();
			this.m_pbDuplicates = new System.Windows.Forms.Button();
			this.m_lblFilterBanner = new System.Windows.Forms.Label();
			this.m_lblSearchCriteria = new System.Windows.Forms.Label();
			this.m_cbCompareFiles = new System.Windows.Forms.CheckBox();
			this.m_stb = new System.Windows.Forms.StatusBar();
			this.m_stbpMainStatus = new System.Windows.Forms.StatusBarPanel();
			this.m_stbpFilterStatus = new System.Windows.Forms.StatusBarPanel();
			this.m_stbpSearch = new System.Windows.Forms.StatusBarPanel();
			this.m_stbpCount = new System.Windows.Forms.StatusBarPanel();
			this.m_prbar = new System.Windows.Forms.ProgressBar();
			this.m_lblActions = new System.Windows.Forms.Label();
			this.m_ebRegEx = new System.Windows.Forms.TextBox();
			this.m_lblRegEx = new System.Windows.Forms.Label();
			this.m_pbMove = new System.Windows.Forms.Button();
			this.m_pbDelete = new System.Windows.Forms.Button();
			this.m_pbToggle = new System.Windows.Forms.Button();
			this.m_pbClear = new System.Windows.Forms.Button();
			this.m_lblMoveTo = new System.Windows.Forms.Label();
			this.m_ebMovePath = new System.Windows.Forms.TextBox();
			this.m_pbMatchRegex = new System.Windows.Forms.Button();
			this.m_pbRemoveRegex = new System.Windows.Forms.Button();
			this.m_pbCheckRegex = new System.Windows.Forms.Button();
			this.m_prbarOverall = new System.Windows.Forms.ProgressBar();
			this.m_pbSmartMatch = new System.Windows.Forms.Button();
			this.m_tmr = new System.Windows.Forms.Timer(this.components);
			this.m_lbPrefPath = new System.Windows.Forms.ListBox();
			this.m_pbRemove = new System.Windows.Forms.Button();
			this.m_pbAddPath = new System.Windows.Forms.Button();
			this.m_cbMarkFavored = new System.Windows.Forms.CheckBox();
			this.m_lv = new System.Windows.Forms.ListView();
			this.m_pbValidateSrc = new System.Windows.Forms.Button();
			this.m_cbxIgnoreList = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.m_cbAddToIgnoreList = new System.Windows.Forms.CheckBox();
			this.m_pbSaveList = new System.Windows.Forms.Button();
			this.m_pbLoadFromFile = new System.Windows.Forms.Button();
			this.m_pbSaveFileList = new System.Windows.Forms.Button();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.button1 = new System.Windows.Forms.Button();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.m_ebCopyPath = new System.Windows.Forms.TextBox();
			this.button2 = new System.Windows.Forms.Button();
			this.m_cbGenerateScript = new System.Windows.Forms.CheckBox();
			this.label6 = new System.Windows.Forms.Label();
			this.m_ebScript = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.m_pbLoadPreferredPaths = new System.Windows.Forms.Button();
			this.m_pbSavePreferredPaths = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpMainStatus)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpFilterStatus)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpSearch)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpCount)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_cxtListView
			// 
			this.m_cxtListView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem6,
            this.menuItem8,
            this.menuItem10,
            this.menuItem2,
            this.menuItem4,
            this.menuItem5});
			this.m_cxtListView.Popup += new System.EventHandler(this.EH_DoContextPopup);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "Execute";
			this.menuItem1.Click += new System.EventHandler(this.EH_HandleExecuteMenu);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 1;
			this.menuItem6.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem7});
			this.menuItem6.Text = "Remove Path";
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 0;
			this.menuItem7.Text = "Placeholder";
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 2;
			this.menuItem8.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem9});
			this.menuItem8.Text = "Remove Type";
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 0;
			this.menuItem9.Text = "Placeholder";
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 3;
			this.menuItem10.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem11});
			this.menuItem10.Text = "Remove File Pattern";
			// 
			// menuItem11
			// 
			this.menuItem11.Index = 0;
			this.menuItem11.Text = "Placeholder";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 4;
			this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem3});
			this.menuItem2.Text = "Add Preferred Path";
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 0;
			this.menuItem3.Text = "Placeholder";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 5;
			this.menuItem4.Text = "Select previous duplicate";
			this.menuItem4.Click += new System.EventHandler(this.EH_SelectPrevDupe);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 6;
			this.menuItem5.Text = "Select next duplicate";
			this.menuItem5.Click += new System.EventHandler(this.EH_SelectNextDupe);
			// 
			// m_ebSearchPath
			// 
			this.m_ebSearchPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_ebSearchPath.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_ebSearchPath.Location = new System.Drawing.Point(170, 100);
			this.m_ebSearchPath.Name = "m_ebSearchPath";
			this.m_ebSearchPath.Size = new System.Drawing.Size(588, 34);
			this.m_ebSearchPath.TabIndex = 2;
			this.m_ebSearchPath.Text = "f:\\DeDedupe";
			// 
			// m_pbSearch
			// 
			this.m_pbSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbSearch.Location = new System.Drawing.Point(1270, 98);
			this.m_pbSearch.Name = "m_pbSearch";
			this.m_pbSearch.Size = new System.Drawing.Size(114, 39);
			this.m_pbSearch.TabIndex = 4;
			this.m_pbSearch.Text = "Search";
			this.m_pbSearch.Click += new System.EventHandler(this.EH_DoSearch);
			// 
			// m_lblSearch
			// 
			this.m_lblSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_lblSearch.Location = new System.Drawing.Point(33, 103);
			this.m_lblSearch.Name = "m_lblSearch";
			this.m_lblSearch.Size = new System.Drawing.Size(115, 29);
			this.m_lblSearch.TabIndex = 1;
			this.m_lblSearch.Text = "Search Spec";
			// 
			// m_cbRecurse
			// 
			this.m_cbRecurse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbRecurse.Checked = true;
			this.m_cbRecurse.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbRecurse.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_cbRecurse.Location = new System.Drawing.Point(764, 104);
			this.m_cbRecurse.Name = "m_cbRecurse";
			this.m_cbRecurse.Size = new System.Drawing.Size(124, 30);
			this.m_cbRecurse.TabIndex = 3;
			this.m_cbRecurse.Text = "Recurse";
			// 
			// m_pbDuplicates
			// 
			this.m_pbDuplicates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbDuplicates.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbDuplicates.Location = new System.Drawing.Point(1270, 395);
			this.m_pbDuplicates.Name = "m_pbDuplicates";
			this.m_pbDuplicates.Size = new System.Drawing.Size(115, 41);
			this.m_pbDuplicates.TabIndex = 9;
			this.m_pbDuplicates.Text = "Uniquify";
			this.m_pbDuplicates.Click += new System.EventHandler(this.EH_Uniquify);
			// 
			// m_lblFilterBanner
			// 
			this.m_lblFilterBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblFilterBanner.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_lblFilterBanner.Location = new System.Drawing.Point(11, 361);
			this.m_lblFilterBanner.Name = "m_lblFilterBanner";
			this.m_lblFilterBanner.Size = new System.Drawing.Size(1373, 31);
			this.m_lblFilterBanner.TabIndex = 5;
			this.m_lblFilterBanner.Tag = "Filter files";
			this.m_lblFilterBanner.Text = "Filter files ----";
			this.m_lblFilterBanner.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
			// 
			// m_lblSearchCriteria
			// 
			this.m_lblSearchCriteria.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblSearchCriteria.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_lblSearchCriteria.Location = new System.Drawing.Point(11, 64);
			this.m_lblSearchCriteria.Name = "m_lblSearchCriteria";
			this.m_lblSearchCriteria.Size = new System.Drawing.Size(1373, 31);
			this.m_lblSearchCriteria.TabIndex = 0;
			this.m_lblSearchCriteria.Tag = "Populate file lists";
			this.m_lblSearchCriteria.Text = "Populate file lists ----";
			this.m_lblSearchCriteria.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
			// 
			// m_cbCompareFiles
			// 
			this.m_cbCompareFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbCompareFiles.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_cbCompareFiles.Location = new System.Drawing.Point(930, 398);
			this.m_cbCompareFiles.Name = "m_cbCompareFiles";
			this.m_cbCompareFiles.Size = new System.Drawing.Size(243, 38);
			this.m_cbCompareFiles.TabIndex = 8;
			this.m_cbCompareFiles.Text = "Real Dupe Checking";
			// 
			// m_stb
			// 
			this.m_stb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_stb.Dock = System.Windows.Forms.DockStyle.None;
			this.m_stb.Location = new System.Drawing.Point(0, 1137);
			this.m_stb.Name = "m_stb";
			this.m_stb.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.m_stbpMainStatus,
            this.m_stbpFilterStatus,
            this.m_stbpSearch,
            this.m_stbpCount});
			this.m_stb.ShowPanels = true;
			this.m_stb.Size = new System.Drawing.Size(1399, 35);
			this.m_stb.TabIndex = 9;
			// 
			// m_stbpMainStatus
			// 
			this.m_stbpMainStatus.Name = "m_stbpMainStatus";
			this.m_stbpMainStatus.Width = 200;
			// 
			// m_stbpFilterStatus
			// 
			this.m_stbpFilterStatus.Name = "m_stbpFilterStatus";
			this.m_stbpFilterStatus.Width = 200;
			// 
			// m_stbpSearch
			// 
			this.m_stbpSearch.Name = "m_stbpSearch";
			this.m_stbpSearch.Width = 200;
			// 
			// m_stbpCount
			// 
			this.m_stbpCount.Name = "m_stbpCount";
			// 
			// m_prbar
			// 
			this.m_prbar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_prbar.Location = new System.Drawing.Point(962, 1143);
			this.m_prbar.Name = "m_prbar";
			this.m_prbar.Size = new System.Drawing.Size(210, 22);
			this.m_prbar.TabIndex = 10;
			this.m_prbar.Visible = false;
			// 
			// m_lblActions
			// 
			this.m_lblActions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblActions.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_lblActions.Location = new System.Drawing.Point(11, 509);
			this.m_lblActions.Name = "m_lblActions";
			this.m_lblActions.Size = new System.Drawing.Size(1378, 28);
			this.m_lblActions.TabIndex = 15;
			this.m_lblActions.Tag = "Perform actions";
			this.m_lblActions.Text = "Perform actions ----";
			this.m_lblActions.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
			// 
			// m_ebRegEx
			// 
			this.m_ebRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_ebRegEx.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_ebRegEx.Location = new System.Drawing.Point(226, 399);
			this.m_ebRegEx.Name = "m_ebRegEx";
			this.m_ebRegEx.Size = new System.Drawing.Size(690, 34);
			this.m_ebRegEx.TabIndex = 7;
			// 
			// m_lblRegEx
			// 
			this.m_lblRegEx.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_lblRegEx.Location = new System.Drawing.Point(33, 402);
			this.m_lblRegEx.Name = "m_lblRegEx";
			this.m_lblRegEx.Size = new System.Drawing.Size(187, 36);
			this.m_lblRegEx.TabIndex = 6;
			this.m_lblRegEx.Text = "Regular Expressions";
			// 
			// m_pbMove
			// 
			this.m_pbMove.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbMove.Location = new System.Drawing.Point(548, 535);
			this.m_pbMove.Name = "m_pbMove";
			this.m_pbMove.Size = new System.Drawing.Size(84, 42);
			this.m_pbMove.TabIndex = 18;
			this.m_pbMove.Text = "Move";
			this.m_pbMove.Click += new System.EventHandler(this.EH_DoMove);
			// 
			// m_pbDelete
			// 
			this.m_pbDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbDelete.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbDelete.Location = new System.Drawing.Point(1274, 535);
			this.m_pbDelete.Name = "m_pbDelete";
			this.m_pbDelete.Size = new System.Drawing.Size(115, 42);
			this.m_pbDelete.TabIndex = 19;
			this.m_pbDelete.Text = "Delete";
			this.m_pbDelete.Click += new System.EventHandler(this.EH_DoDelete);
			// 
			// m_pbToggle
			// 
			this.m_pbToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbToggle.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbToggle.Location = new System.Drawing.Point(1274, 451);
			this.m_pbToggle.Name = "m_pbToggle";
			this.m_pbToggle.Size = new System.Drawing.Size(115, 41);
			this.m_pbToggle.TabIndex = 14;
			this.m_pbToggle.Text = "Toggle All";
			this.m_pbToggle.Click += new System.EventHandler(this.EH_ToggleAll);
			// 
			// m_pbClear
			// 
			this.m_pbClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbClear.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbClear.Location = new System.Drawing.Point(1146, 451);
			this.m_pbClear.Name = "m_pbClear";
			this.m_pbClear.Size = new System.Drawing.Size(115, 41);
			this.m_pbClear.TabIndex = 13;
			this.m_pbClear.Text = "Clear All";
			this.m_pbClear.Click += new System.EventHandler(this.EH_ClearAll);
			// 
			// m_lblMoveTo
			// 
			this.m_lblMoveTo.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_lblMoveTo.Location = new System.Drawing.Point(37, 543);
			this.m_lblMoveTo.Name = "m_lblMoveTo";
			this.m_lblMoveTo.Size = new System.Drawing.Size(89, 39);
			this.m_lblMoveTo.TabIndex = 16;
			this.m_lblMoveTo.Text = "Move to";
			// 
			// m_ebMovePath
			// 
			this.m_ebMovePath.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_ebMovePath.Location = new System.Drawing.Point(132, 540);
			this.m_ebMovePath.Name = "m_ebMovePath";
			this.m_ebMovePath.Size = new System.Drawing.Size(410, 34);
			this.m_ebMovePath.TabIndex = 17;
			// 
			// m_pbMatchRegex
			// 
			this.m_pbMatchRegex.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbMatchRegex.Location = new System.Drawing.Point(226, 451);
			this.m_pbMatchRegex.Name = "m_pbMatchRegex";
			this.m_pbMatchRegex.Size = new System.Drawing.Size(147, 41);
			this.m_pbMatchRegex.TabIndex = 10;
			this.m_pbMatchRegex.Text = "Match Regex";
			this.m_pbMatchRegex.Click += new System.EventHandler(this.EH_MatchRegex);
			// 
			// m_pbRemoveRegex
			// 
			this.m_pbRemoveRegex.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbRemoveRegex.Location = new System.Drawing.Point(379, 451);
			this.m_pbRemoveRegex.Name = "m_pbRemoveRegex";
			this.m_pbRemoveRegex.Size = new System.Drawing.Size(128, 41);
			this.m_pbRemoveRegex.TabIndex = 11;
			this.m_pbRemoveRegex.Text = "Filter Regex";
			this.m_pbRemoveRegex.Click += new System.EventHandler(this.EH_FilterRegex);
			// 
			// m_pbCheckRegex
			// 
			this.m_pbCheckRegex.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbCheckRegex.Location = new System.Drawing.Point(513, 451);
			this.m_pbCheckRegex.Name = "m_pbCheckRegex";
			this.m_pbCheckRegex.Size = new System.Drawing.Size(131, 41);
			this.m_pbCheckRegex.TabIndex = 12;
			this.m_pbCheckRegex.Text = "Check Regex";
			this.m_pbCheckRegex.Click += new System.EventHandler(this.EH_CheckRegex);
			// 
			// m_prbarOverall
			// 
			this.m_prbarOverall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_prbarOverall.Location = new System.Drawing.Point(1179, 1143);
			this.m_prbarOverall.Maximum = 1000;
			this.m_prbarOverall.Name = "m_prbarOverall";
			this.m_prbarOverall.Size = new System.Drawing.Size(210, 22);
			this.m_prbarOverall.TabIndex = 23;
			this.m_prbarOverall.Visible = false;
			// 
			// m_pbSmartMatch
			// 
			this.m_pbSmartMatch.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbSmartMatch.Location = new System.Drawing.Point(650, 451);
			this.m_pbSmartMatch.Name = "m_pbSmartMatch";
			this.m_pbSmartMatch.Size = new System.Drawing.Size(128, 41);
			this.m_pbSmartMatch.TabIndex = 24;
			this.m_pbSmartMatch.Text = "SmartMatch";
			this.m_pbSmartMatch.Click += new System.EventHandler(this.EH_SmartMatchClick);
			// 
			// m_tmr
			// 
			this.m_tmr.Tick += new System.EventHandler(this.EH_Idle);
			// 
			// m_lbPrefPath
			// 
			this.m_lbPrefPath.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_lbPrefPath.FormattingEnabled = true;
			this.m_lbPrefPath.ItemHeight = 28;
			this.m_lbPrefPath.Location = new System.Drawing.Point(202, 211);
			this.m_lbPrefPath.Name = "m_lbPrefPath";
			this.m_lbPrefPath.Size = new System.Drawing.Size(823, 116);
			this.m_lbPrefPath.TabIndex = 25;
			// 
			// m_pbRemove
			// 
			this.m_pbRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbRemove.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbRemove.Location = new System.Drawing.Point(1106, 246);
			this.m_pbRemove.Name = "m_pbRemove";
			this.m_pbRemove.Size = new System.Drawing.Size(115, 40);
			this.m_pbRemove.TabIndex = 27;
			this.m_pbRemove.Text = "Remove";
			this.m_pbRemove.Click += new System.EventHandler(this.RemovePreferredPath);
			// 
			// m_pbAddPath
			// 
			this.m_pbAddPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbAddPath.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbAddPath.Location = new System.Drawing.Point(1106, 201);
			this.m_pbAddPath.Name = "m_pbAddPath";
			this.m_pbAddPath.Size = new System.Drawing.Size(115, 39);
			this.m_pbAddPath.TabIndex = 28;
			this.m_pbAddPath.Text = "Add Path";
			// 
			// m_cbMarkFavored
			// 
			this.m_cbMarkFavored.AutoSize = true;
			this.m_cbMarkFavored.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_cbMarkFavored.Location = new System.Drawing.Point(38, 325);
			this.m_cbMarkFavored.Name = "m_cbMarkFavored";
			this.m_cbMarkFavored.Size = new System.Drawing.Size(158, 32);
			this.m_cbMarkFavored.TabIndex = 29;
			this.m_cbMarkFavored.Text = "Mark Favored";
			this.m_cbMarkFavored.UseVisualStyleBackColor = true;
			// 
			// m_lv
			// 
			this.m_lv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lv.CheckBoxes = true;
			this.m_lv.ContextMenu = this.m_cxtListView;
			this.m_lv.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lv.HideSelection = false;
			this.m_lv.Location = new System.Drawing.Point(25, 628);
			this.m_lv.Name = "m_lv";
			this.m_lv.Size = new System.Drawing.Size(1347, 503);
			this.m_lv.TabIndex = 20;
			this.m_lv.UseCompatibleStateImageBehavior = false;
			this.m_lv.VirtualMode = true;
			this.m_lv.Visible = false;
			// 
			// m_pbValidateSrc
			// 
			this.m_pbValidateSrc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbValidateSrc.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbValidateSrc.Location = new System.Drawing.Point(1146, 395);
			this.m_pbValidateSrc.Name = "m_pbValidateSrc";
			this.m_pbValidateSrc.Size = new System.Drawing.Size(118, 41);
			this.m_pbValidateSrc.TabIndex = 33;
			this.m_pbValidateSrc.Text = "Validate Src";
			this.m_pbValidateSrc.Click += new System.EventHandler(this.EH_ValidateSrc);
			// 
			// m_cbxIgnoreList
			// 
			this.m_cbxIgnoreList.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_cbxIgnoreList.FormattingEnabled = true;
			this.m_cbxIgnoreList.Location = new System.Drawing.Point(202, 146);
			this.m_cbxIgnoreList.Name = "m_cbxIgnoreList";
			this.m_cbxIgnoreList.Size = new System.Drawing.Size(268, 36);
			this.m_cbxIgnoreList.TabIndex = 35;
			this.m_cbxIgnoreList.SelectedIndexChanged += new System.EventHandler(this.EH_HandleIgnoreListSelect);
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.label3.Location = new System.Drawing.Point(33, 149);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(116, 33);
			this.label3.TabIndex = 34;
			this.label3.Text = "Ignore list";
			// 
			// m_cbAddToIgnoreList
			// 
			this.m_cbAddToIgnoreList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbAddToIgnoreList.Checked = true;
			this.m_cbAddToIgnoreList.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbAddToIgnoreList.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_cbAddToIgnoreList.Location = new System.Drawing.Point(201, 324);
			this.m_cbAddToIgnoreList.Name = "m_cbAddToIgnoreList";
			this.m_cbAddToIgnoreList.Size = new System.Drawing.Size(276, 34);
			this.m_cbAddToIgnoreList.TabIndex = 36;
			this.m_cbAddToIgnoreList.Text = "Automatically add ignore";
			this.m_toolTip.SetToolTip(this.m_cbAddToIgnoreList, "Add path to ignore list when \"Remove...\" is selected from ListView context menu.");
			// 
			// m_pbSaveList
			// 
			this.m_pbSaveList.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbSaveList.Location = new System.Drawing.Point(486, 143);
			this.m_pbSaveList.Name = "m_pbSaveList";
			this.m_pbSaveList.Size = new System.Drawing.Size(116, 41);
			this.m_pbSaveList.TabIndex = 37;
			this.m_pbSaveList.Text = "Save List";
			this.m_pbSaveList.Click += new System.EventHandler(this.EH_DoSaveIgnoreList);
			// 
			// m_pbLoadFromFile
			// 
			this.m_pbLoadFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbLoadFromFile.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbLoadFromFile.Location = new System.Drawing.Point(1095, 15);
			this.m_pbLoadFromFile.Name = "m_pbLoadFromFile";
			this.m_pbLoadFromFile.Size = new System.Drawing.Size(138, 39);
			this.m_pbLoadFromFile.TabIndex = 38;
			this.m_pbLoadFromFile.Text = "Load FileList";
			this.m_pbLoadFromFile.Click += new System.EventHandler(this.EH_LoadFileListFromFile);
			// 
			// m_pbSaveFileList
			// 
			this.m_pbSaveFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbSaveFileList.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbSaveFileList.Location = new System.Drawing.Point(1239, 14);
			this.m_pbSaveFileList.Name = "m_pbSaveFileList";
			this.m_pbSaveFileList.Size = new System.Drawing.Size(147, 39);
			this.m_pbSaveFileList.TabIndex = 39;
			this.m_pbSaveFileList.Text = "Save FileList";
			this.m_pbSaveFileList.Click += new System.EventHandler(this.EH_SaveFileListToFile);
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.button1.Location = new System.Drawing.Point(1146, 98);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(115, 39);
			this.button1.TabIndex = 46;
			this.button1.Text = "Append";
			this.button1.Click += new System.EventHandler(this.EH_DoAppendSearch);
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Checked = true;
			this.radioButton1.Location = new System.Drawing.Point(10, 7);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(140, 32);
			this.radioButton1.TabIndex = 47;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "Source Files";
			this.radioButton1.UseVisualStyleBackColor = true;
			this.radioButton1.CheckedChanged += new System.EventHandler(this.DoSearchTargetChange);
			// 
			// radioButton2
			// 
			this.radioButton2.AutoSize = true;
			this.radioButton2.Location = new System.Drawing.Point(156, 7);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(180, 32);
			this.radioButton2.TabIndex = 48;
			this.radioButton2.Text = "Destination Files";
			this.radioButton2.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioButton1);
			this.panel1.Controls.Add(this.radioButton2);
			this.panel1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.panel1.Location = new System.Drawing.Point(25, 12);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(346, 41);
			this.panel1.TabIndex = 49;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.label2.Location = new System.Drawing.Point(634, 543);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(93, 31);
			this.label2.TabIndex = 50;
			this.label2.Text = "Copy to";
			// 
			// m_ebCopyPath
			// 
			this.m_ebCopyPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_ebCopyPath.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_ebCopyPath.Location = new System.Drawing.Point(733, 540);
			this.m_ebCopyPath.Name = "m_ebCopyPath";
			this.m_ebCopyPath.Size = new System.Drawing.Size(440, 34);
			this.m_ebCopyPath.TabIndex = 51;
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.button2.Location = new System.Drawing.Point(1179, 535);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(82, 42);
			this.button2.TabIndex = 52;
			this.button2.Text = "Copy";
			this.button2.Click += new System.EventHandler(this.DoCopy);
			// 
			// m_cbGenerateScript
			// 
			this.m_cbGenerateScript.AutoSize = true;
			this.m_cbGenerateScript.Checked = true;
			this.m_cbGenerateScript.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbGenerateScript.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_cbGenerateScript.Location = new System.Drawing.Point(554, 582);
			this.m_cbGenerateScript.Name = "m_cbGenerateScript";
			this.m_cbGenerateScript.Size = new System.Drawing.Size(173, 32);
			this.m_cbGenerateScript.TabIndex = 53;
			this.m_cbGenerateScript.Text = "Generate Script";
			this.m_cbGenerateScript.UseVisualStyleBackColor = true;
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.label6.Location = new System.Drawing.Point(42, 582);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(76, 31);
			this.label6.TabIndex = 54;
			this.label6.Text = "Script";
			// 
			// m_ebScript
			// 
			this.m_ebScript.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_ebScript.Location = new System.Drawing.Point(132, 579);
			this.m_ebScript.Name = "m_ebScript";
			this.m_ebScript.Size = new System.Drawing.Size(410, 34);
			this.m_ebScript.TabIndex = 55;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.label1.Location = new System.Drawing.Point(33, 211);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(163, 33);
			this.label1.TabIndex = 26;
			this.label1.Text = "Preferred Paths";
			// 
			// m_pbLoadPreferredPaths
			// 
			this.m_pbLoadPreferredPaths.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbLoadPreferredPaths.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbLoadPreferredPaths.Location = new System.Drawing.Point(1237, 201);
			this.m_pbLoadPreferredPaths.Name = "m_pbLoadPreferredPaths";
			this.m_pbLoadPreferredPaths.Size = new System.Drawing.Size(147, 39);
			this.m_pbLoadPreferredPaths.TabIndex = 56;
			this.m_pbLoadPreferredPaths.Text = "Load Paths";
			this.m_pbLoadPreferredPaths.Click += new System.EventHandler(this.EH_LoadPreferredPaths);
			// 
			// m_pbSavePreferredPaths
			// 
			this.m_pbSavePreferredPaths.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbSavePreferredPaths.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.m_pbSavePreferredPaths.Location = new System.Drawing.Point(1237, 246);
			this.m_pbSavePreferredPaths.Name = "m_pbSavePreferredPaths";
			this.m_pbSavePreferredPaths.Size = new System.Drawing.Size(147, 39);
			this.m_pbSavePreferredPaths.TabIndex = 57;
			this.m_pbSavePreferredPaths.Text = "Save Paths";
			this.m_pbSavePreferredPaths.Click += new System.EventHandler(this.EH_SavePreferredPaths);
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.FlatAppearance.BorderSize = 0;
			this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button3.Font = new System.Drawing.Font("Segoe UI Symbol", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button3.Location = new System.Drawing.Point(1031, 207);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(27, 37);
			this.button3.TabIndex = 58;
			this.button3.Text = "∆";
			this.button3.Click += new System.EventHandler(this.EH_NudgePreferredPathUp);
			// 
			// button4
			// 
			this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button4.FlatAppearance.BorderSize = 0;
			this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button4.Font = new System.Drawing.Font("Segoe UI Symbol", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button4.Location = new System.Drawing.Point(1031, 273);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(27, 37);
			this.button4.TabIndex = 59;
			this.button4.Text = "∇";
			this.button4.Click += new System.EventHandler(this.EH_NudgePreferredPathDown);
			// 
			// button5
			// 
			this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button5.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.button5.Location = new System.Drawing.Point(991, 453);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(149, 39);
			this.button5.TabIndex = 60;
			this.button5.Text = "Prefer Paths";
			this.button5.Click += new System.EventHandler(this.EH_ApplyPreferredPaths);
			// 
			// SListApp
			// 
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
			this.ClientSize = new System.Drawing.Size(1399, 1169);
			this.Controls.Add(this.button5);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.m_pbSavePreferredPaths);
			this.Controls.Add(this.m_pbLoadPreferredPaths);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.m_ebScript);
			this.Controls.Add(this.m_cbGenerateScript);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.m_ebCopyPath);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.m_pbSaveFileList);
			this.Controls.Add(this.m_pbLoadFromFile);
			this.Controls.Add(this.m_pbSaveList);
			this.Controls.Add(this.m_cbAddToIgnoreList);
			this.Controls.Add(this.m_cbxIgnoreList);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.m_pbValidateSrc);
			this.Controls.Add(this.m_cbMarkFavored);
			this.Controls.Add(this.m_pbAddPath);
			this.Controls.Add(this.m_pbRemove);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.m_lbPrefPath);
			this.Controls.Add(this.m_pbSmartMatch);
			this.Controls.Add(this.m_prbarOverall);
			this.Controls.Add(this.m_pbCheckRegex);
			this.Controls.Add(this.m_pbRemoveRegex);
			this.Controls.Add(this.m_pbMatchRegex);
			this.Controls.Add(this.m_lblMoveTo);
			this.Controls.Add(this.m_ebMovePath);
			this.Controls.Add(this.m_pbClear);
			this.Controls.Add(this.m_pbToggle);
			this.Controls.Add(this.m_pbDelete);
			this.Controls.Add(this.m_pbMove);
			this.Controls.Add(this.m_lblRegEx);
			this.Controls.Add(this.m_ebRegEx);
			this.Controls.Add(this.m_lblActions);
			this.Controls.Add(this.m_prbar);
			this.Controls.Add(this.m_stb);
			this.Controls.Add(this.m_cbCompareFiles);
			this.Controls.Add(this.m_lblSearchCriteria);
			this.Controls.Add(this.m_lblFilterBanner);
			this.Controls.Add(this.m_pbDuplicates);
			this.Controls.Add(this.m_cbRecurse);
			this.Controls.Add(this.m_lblSearch);
			this.Controls.Add(this.m_pbSearch);
			this.Controls.Add(this.m_ebSearchPath);
			this.Controls.Add(this.m_lv);
			this.Name = "SListApp";
			this.Text = "SListApp";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EH_OnFormClosing);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleDragEnter);
			this.DragLeave += new System.EventHandler(this.HandleDragLeave);
			((System.ComponentModel.ISupportInitialize)(this.m_stbpMainStatus)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpFilterStatus)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpSearch)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpCount)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region EventHandlers

		void EH_ApplyPreferredPaths(object sender, EventArgs e)
		{
			m_model.AdjustListViewForFavoredPaths();
		}


		void NudgePreferredPath(int di)
		{
			if (m_lbPrefPath.SelectedIndices.Count != 1)
				return;

			int sel = m_lbPrefPath.SelectedIndex;

			if (sel + di < 0 || sel + di >= m_lbPrefPath.Items.Count)
				return;

			string s = (string)m_lbPrefPath.Items[sel];
			m_lbPrefPath.Items.RemoveAt(sel);
			PreferredPaths.PathItem item = m_model.PreferredPaths.Paths[sel];
			m_model.PreferredPaths.Paths.RemoveAt(sel);

			m_lbPrefPath.Items.Insert(sel + di, s);
			m_model.PreferredPaths.Paths.Insert(sel + di, item);
			m_lbPrefPath.SelectedIndex = sel + di;
		}

		void EH_NudgePreferredPathUp(object sender, EventArgs e)
		{
			NudgePreferredPath(-1);
		}

		void EH_NudgePreferredPathDown(object sender, EventArgs e)
		{
			NudgePreferredPath(1);
		}
		private void RemovePreferredPath(object sender, EventArgs e)
		{
			if (m_lbPrefPath.SelectedIndices.Count == 0)
				return;

			int sel = m_lbPrefPath.SelectedIndices[0];
			int di = 0;

			List<int> toRemove = new List<int>();

			foreach (int i in m_lbPrefPath.SelectedIndices)
				toRemove.Add(i);

			toRemove.Sort();

			for (int i = toRemove.Count - 1; i >= 0; --i)
			{
				if (toRemove[i] < sel)
					di--;

				m_model.RemovePreferredPath((string)m_lbPrefPath.Items[toRemove[i]]);
				m_lbPrefPath.Items.RemoveAt(toRemove[i]);
			}

			m_lbPrefPath.SelectedIndices.Clear();
			sel += di;

			if (sel >= 0 && sel < m_lbPrefPath.Items.Count)
				m_lbPrefPath.SelectedIndex = sel;
		}

		private void EH_LoadPreferredPaths(object sender, EventArgs e)
		{
			m_model.LoadPreferredPathsFromFile();
		}

		private void EH_SavePreferredPaths(object sender, EventArgs e)
		{
			m_model.SavePreferredPathsToFile();
		}

		private void EH_OnFormClosing(object sender, FormClosedEventArgs e)
		{
			SyncSettingsWithUi();
			m_settings.Save();
		}

		private bool fSyncing = false;
		private void DoSearchTargetChange(object sender, EventArgs e)
		{
			if (!fSyncing)
				ShowListView(CurrentFileList());
		}

		private void EH_ColumnClick(object o, ColumnClickEventArgs e)
		{
			m_model.ChangeListViewSort(ViewCur, e.Column);
		}

		private void EH_Uniquify(object sender, System.EventArgs e)
		{
			m_model.BuildUniqueFileList();
			m_model.AdjustListViewForFavoredPaths();
		}

		private void EH_DoSearch(object sender, System.EventArgs e)
		{
			m_model.BuildFileList();
		}

		private void EH_DoAppendSearch(object sender, System.EventArgs e)
		{
			m_model.BuildFileList(true /*fAppend*/);
		}

		private void EH_RenderHeadingLine(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			RenderSupp.RenderHeadingLine(sender, e);
		}

		private void EH_DoMove(object sender, System.EventArgs e)
		{
			SmartList.MoveSelectedFiles(ViewCur, m_ebMovePath.Text, m_stbpMainStatus);
		}

		private void DoCopy(object sender, EventArgs e)
		{
			SmartList.CopySelectedFiles(ViewCur, m_ebCopyPath.Text, m_cbGenerateScript.Checked ? m_ebScript.Text : null, m_stbpMainStatus);
		}

		private void EH_DoDelete(object sender, System.EventArgs e) { }

		private void EH_ToggleAll(object sender, System.EventArgs e)
		{
			m_model.ToggleAllListViewItems(ViewCur);
		}

		private void EH_ClearAll(object sender, System.EventArgs e)
		{
			m_model.UncheckAllListViewItems(ViewCur);
		}

		private void EH_MatchRegex(object sender, System.EventArgs e)
		{
			m_model.DoRegex(SmartList.RegexOp.Match, m_ebRegEx.Text);
		}

		private void EH_FilterRegex(object sender, System.EventArgs e)
		{
			m_model.DoRegex(SmartList.RegexOp.Filter, m_ebRegEx.Text);
		}

		private void EH_CheckRegex(object sender, System.EventArgs e)
		{
			m_model.DoRegex(SmartList.RegexOp.Check, m_ebRegEx.Text);
		}

		private void EH_HandleExecuteMenu(object sender, System.EventArgs e)
		{
			SLItem sli = ViewCur.SelectedItem();

			if (sli != null)
			{
				m_model.LaunchSli(sli);
			}
		}

		private void EH_SmartMatchClick(object sender, System.EventArgs e)
		{
			sCancelled = SmartList.SCalcMatchingListViewItems(ViewCur, m_ebRegEx.Text, sCancelled);
		}

		private void EH_HandleEdit(object sender, System.Windows.Forms.LabelEditEventArgs e)
		{
			SLItem sli = ViewCur.Items[e.Item];

			if (!SmartList.FRenameFile(sli.Path, sli.Name, sli.Path, e.Label))
			{
				e.CancelEdit = true;
			}
			else
			{
				sli.Rename(e.Label);
			}
		}

		private void EH_ValidateSrc(object sender, EventArgs e)
		{
			m_model.BuildMissingFileList();
		}

		/* E  H _  H A N D L E  I G N O R E  L I S T  S E L E C T */
		/*----------------------------------------------------------------------------
        	%%Function: EH_HandleIgnoreListSelect
        	%%Qualified: SList.SListApp.EH_HandleIgnoreListSelect
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
		private void EH_HandleIgnoreListSelect(object sender, EventArgs e)
		{
			if (m_cbxIgnoreList.SelectedIndex <= 1)
			{
				// this is "<Create...>" or "<Copy...>"
				string sName;
				if (TCore.UI.InputBox.ShowInputBox("New ignore list name", "Ignore list name", "", out sName, this))
				{
					m_model.CreateIgnoreList(sName, m_cbxIgnoreList.SelectedIndex == 1);
					m_cbxIgnoreList.Items.Add(sName);
					m_cbxIgnoreList.SelectedIndex = m_cbxIgnoreList.Items.Count - 1;
				}
				return;
			}
			m_model.ApplyIgnoreList((string)m_cbxIgnoreList.SelectedItem);
		}

		private void EH_DoSaveIgnoreList(object sender, EventArgs e)
		{
			m_model.EnsureIgnoreListSaved();
		}

		private void EH_LoadFileListFromFile(object sender, EventArgs e)
		{
			m_model.LoadFileListFromFile(SlisCur);
			m_model.AdjustListViewForFavoredPaths();
			SetCount(ViewCur.Items.Count);
		}

		private void EH_SaveFileListToFile(object sender, EventArgs e)
		{
			m_model.SaveFileListToFile(SlisCur);
		}

		void EH_RemoveType(object sender, EventArgs e)
		{
			MenuItem mni = (MenuItem)sender;
			m_model.RemoveType(m_rgslis[m_islisCur], mni.Text, (FilePatternInfo) mni.Tag);
		}

		void EH_RemovePattern(object sender, EventArgs e)
		{
			MenuItem mni = (MenuItem)sender;
			m_model.RemovePattern(m_rgslis[m_islisCur], mni.Text, (FilePatternInfo)mni.Tag);
		}

		void EH_RemovePath(object sender, EventArgs e)
		{
			MenuItem mni = (MenuItem)sender;
			m_model.RemovePath(m_rgslis[m_islisCur], mni.Text);
			if (m_cbAddToIgnoreList.Checked)
			{
				m_model.AddIgnorePath(mni.Text);
			}
		}

		private void EH_AddPreferredPath(object sender, EventArgs e)
		{
			if (SlisCur.FileListType == FileList.Destination)
			{
				MessageBox.Show("Add preferred path makes no sense with destination files. All destination paths are preferred");
				return;
			}

			MenuItem mni = (MenuItem)sender;
			m_model.AddPreferredPath(mni.Text);
		}

		void AddPreferredPathSubmenuItems(MenuItem mni, SLItem sli)
		{
			if (mni.Text != "Add Preferred Path")
				throw new Exception("context menu structure changed!");

			mni.MenuItems.Clear();
			// break the path into pieces and add an item for each piece
			Path.GetDirectoryName(sli.Path);
			string[] rgs = sli.Path.Split('\\');

			string sSub = "";
			foreach (string s in rgs)
			{
				MenuItem mniNew = new MenuItem();

				if (sSub != "")
					sSub += "\\" + s;
				else
					sSub = s;

				mniNew.Text = sSub;
				mniNew.Click += new EventHandler(EH_AddPreferredPath);

				mni.MenuItems.Add(mniNew);
			}
		}

		void AddRemovePathSubmenuItems(MenuItem mni, SLItem sli)
		{
			if (mni.Text != "Remove Path")
				throw new Exception("context menu structure changed!");

			mni.MenuItems.Clear();

			string sSub = "";
			string[] rgs = sli.Path.Split('\\');

			foreach (string s in rgs)
			{
				MenuItem mniNew = new MenuItem();

				if (sSub != "")
					sSub += "\\" + s;
				else
					sSub = s;

				mniNew.Text = sSub;
				mniNew.Click += new EventHandler(EH_RemovePath);
				mniNew.Tag = sli;

				mni.MenuItems.Add(mniNew);
			}
		}

		void AddRemoveItemPatternSubmenuItems(MenuItem mni, SLItem sli)
		{
			if (mni.Text != "Remove Type")
				throw new Exception("context menu structure changed!");

			mni.MenuItems.Clear();

			string sExt = sli.Extension;
			string sSub = "";
			string[] rgs = sli.Path.Split('\\');

			foreach (string s in rgs)
			{
				MenuItem mniNew = new MenuItem();

				if (sSub != "")
					sSub += "\\" + s;
				else
					sSub = s;

				mniNew.Text = $"{sSub}\\*.{sExt}";
				mniNew.Click += new EventHandler(EH_RemoveType);
				mniNew.Tag = new FilePatternInfo() {Pattern = sExt, RootPath = sSub};
				mni.MenuItems.Add(mniNew);
			}
		}

		void AddRemoveFilePatternSubmenuItems(MenuItem mni, SLItem sli)
		{
			if (mni.Text != "Remove File Pattern")
				throw new Exception("context menu structure changed!");

			mni.MenuItems.Clear();

			string sName = sli.Name;
			string sSub = "";
			string[] rgs = sli.Path.Split('\\');

			foreach (string s in rgs)
			{
				MenuItem mniNew = new MenuItem();

				if (sSub != "")
					sSub += "\\" + s;
				else
					sSub = s;

				mniNew.Text = $"{sSub}\\{sName}";
				mniNew.Click += new EventHandler(EH_RemovePattern);
				mniNew.Tag = new FilePatternInfo() {Pattern = sName, RootPath = sSub};
				mni.MenuItems.Add(mniNew);
			}
		}

		private void EH_DoContextPopup(object sender, EventArgs e)
		{
			SLItem sli = ViewCur.SelectedItem();

			if (sli != null)
			{
				ContextMenu cm = (ContextMenu)sender;

				AddRemovePathSubmenuItems(cm.MenuItems[1], sli);
				AddRemoveItemPatternSubmenuItems(cm.MenuItems[2], sli);
				AddRemoveFilePatternSubmenuItems(cm.MenuItems[3], sli);
				AddPreferredPathSubmenuItems(cm.MenuItems[4], sli);
			}
		}

		string sCancelled;

		/* E  H  _ I D L E */
		/*----------------------------------------------------------------------------
		    %%Function: EH_Idle
		    %%Qualified: SList.SListApp.EH_Idle
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		private void EH_Idle(object sender, System.EventArgs e)
		{
			m_tmr.Enabled = false;
			if (sCancelled.Length > 0)
			{
				MessageBox.Show(sCancelled, "Not Found");
				sCancelled = "";
			}
		}

		#endregion

		#region ListView Handlers
		/* H A N D L E  D R O P */
		/*----------------------------------------------------------------------------
		    %%Function: HandleDrop
		    %%Qualified: SList.SListApp.HandleDrop
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		private void HandleDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			this.Activate();

			m_tmr.Enabled = false;

			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			sCancelled = "";
			foreach (string sFile in files)
			{
				m_ebRegEx.Text = Path.GetFileName(sFile);
				EH_SmartMatchClick(null, null);
			}
			//		if (sCancelled.Length > 0)
			//			MessageBox.Show(sCancelled, "Not Found");
			m_tmr.Interval = 500;
			m_tmr.Enabled = true;
		}

		/* H A N D L E  D R A G  E N T E R */
		/*----------------------------------------------------------------------------
		    %%Function: HandleDragEnter
		    %%Qualified: SList.SListApp.HandleDragEnter
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		private void HandleDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		/* H A N D L E  D R A G  L E A V E */
		/*----------------------------------------------------------------------------
		    %%Function: HandleDragLeave
		    %%Qualified: SList.SListApp.HandleDragLeave
		    %%Contact: rlittle

	    ----------------------------------------------------------------------------*/
		private void HandleDragLeave(object sender, System.EventArgs e) { }

		private void EH_SelectPrevDupe(object sender, EventArgs e)
		{
			SLItem sli = ViewCur.SelectedItem();

			if (sli != null)
			{
				SLItem sliSel = sli.Prev;
				m_model.Select(sliSel);
			}
		}

		private void EH_SelectNextDupe(object sender, EventArgs e)
		{
			SLItem sli = ViewCur.SelectedItem();

			if (sli != null)
			{
				SLItem sliSel = sli.Next;
				m_model.Select(sliSel);
			}
		}
		#endregion

		#region ISmartListUi

		public Form TheForm => this;

		public string GetPreferredPathListDefaultName() => m_settings.PreferredPathListDefault;
		public void SetPreferredPathListDefaultName(string name) => m_settings.PreferredPathListDefault = name;
		public void ClearPreferredPaths() => m_lbPrefPath.Items.Clear();

		public string GetFileListDefaultName(FileList fileList)
		{
			if (fileList == FileList.Source)
				return m_settings.SourceFilesListDefault;
			else
				return m_settings.DestFilesListDefault;
		}

		public void SetFileListDefaultName(FileList fileList, string filename)
		{
			if (fileList == FileList.Source)
				m_settings.SourceFilesListDefault = filename;
			else
				m_settings.DestFilesListDefault = filename;
		}

		public Cursor SetCursor(Cursor cursor)
		{
			Cursor old = this.Cursor;

			this.Cursor = cursor;

			return old;
		}

		public bool FCompareFilesChecked() => m_cbCompareFiles.Checked;

		public void SetStatusText(string text)
		{
			m_stbpMainStatus.Text = text;
		}

		public void SetCount(int count)
		{
			m_stbpCount.Text = $"Files: {count}";
		}

		public void AddIgnoreListItem(string text)
		{
			m_cbxIgnoreList.Items.Add(text);
		}

		public SLISet GetSliSet(FileList fileList)
		{
			int iListView = IlvFromFileList(fileList);
			return m_rgslis[iListView];
		}

		public string GetSearchPath() => m_ebSearchPath.Text;

		public bool FRecurseChecked() => m_cbRecurse.Checked;
		public bool FMarkFavored() => m_cbMarkFavored.Checked;

		public void AddPreferredPath(string path)
		{
			m_lbPrefPath.Items.Add(path);
		}

		public IEnumerable GetPreferredPaths()
		{
			return m_lbPrefPath.Items;
		}

		private ProgressBarStatus m_progressBarStatusOverall;
		private ProgressBarStatus m_progressBarStatusCurrent;

		ProgressBarStatus BarFromType(ProgressBarType barType)
		{
			if (barType == ProgressBarType.Current)
				return m_progressBarStatusCurrent;
			else
				return m_progressBarStatusOverall;
		}

		public void SetProgressBarMac(ProgressBarType barType, long iMac)
		{
			BarFromType(barType).SetMacProgress(iMac);
		}

		public void SetProgressBarOnDemand(ProgressBarType barType, int msecBeforeShow)
		{
			BarFromType(barType).SetOnDemandStatusBar(msecBeforeShow);
		}

		public void ShowProgressBar(ProgressBarType barType)
		{
			BarFromType(barType).Show();
		}

		public void UpdateProgressBar(ProgressBarType barType, long i, OnProgressUpdateDelegate del)
		{
			BarFromType(barType).Update(i, del);
		}

		public void HideProgressBar(ProgressBarType barType) => BarFromType(barType).Hide();

		#endregion


	}
}

