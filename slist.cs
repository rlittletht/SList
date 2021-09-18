using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.IO;
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
		private Label label1;
		private Button m_pbRemove;
		private Button m_pbAddPath;
		private MenuItem menuItem2;
		private MenuItem menuItem3;
		private CheckBox m_cbMarkFavored;
		private MenuItem menuItem4;
		private MenuItem menuItem5;
		private Label label2;
		private ComboBox m_cbxSearchTarget;
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
		public static int s_ilvSource = 0;
		public static int s_ilvDest = 1;
		public static int s_clvMax = 2;
		private ToolTip m_toolTip;

		public ListView LvCur
		{
			get { return SlisCur.Lv; }
		}

		public SLISet SlisCur
		{
			get { return m_rgslis[m_islisCur]; }
		}

		#region AppHost

		private SmartList m_model;

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
			InitializeListView(s_ilvSource);
			InitializeListView(s_ilvDest);

			m_model = new SmartList(this);

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		// the designer initializes m_lv.  this will become m_rglv[s_ilvSource], and m_lv will be set to null. this allows us to create the templates
		// for all the list views in the designer and still have our switchable list views
		void InitializeListViews()
		{
			m_rgslis = new SLISet[s_clvMax];

			m_rgslis[s_ilvSource] = new SLISet();
			m_rgslis[s_ilvSource].Lv = m_lv;
			m_lv = null;

			for (int ilv = 0; ilv < s_clvMax; ilv++)
			{
				if (ilv == s_ilvSource)
					continue; // skip, this is already initialized

				ListView lv = new System.Windows.Forms.ListView();
				lv.Anchor = m_rgslis[s_ilvSource].Lv.Anchor;
				lv.CheckBoxes = m_rgslis[s_ilvSource].Lv.CheckBoxes;

				lv.ContextMenu = m_rgslis[s_ilvSource].Lv.ContextMenu;
				lv.Location = m_rgslis[s_ilvSource].Lv.Location;
				lv.Name = String.Format("m_rglv{0}", ilv);
				lv.Size = m_rgslis[s_ilvSource].Lv.Size;
				lv.TabIndex = m_rgslis[s_ilvSource].Lv.TabIndex;
				lv.UseCompatibleStateImageBehavior = m_rgslis[s_ilvSource].Lv.UseCompatibleStateImageBehavior;
				//m_rglv[ilv].AfterLabelEdit += m_rglv[s_ilvSource].AfterLabelEdit;
				lv.Visible = false;
				this.Controls.Add(lv);
				m_rgslis[ilv] = new SLISet();
				m_rgslis[ilv].Lv = lv;
			}
		}

		private void InitializeListView(int ilv)
		{
			m_rgslis[ilv].Lv.Columns.Add(new ColumnHeader());
			m_rgslis[ilv].Lv.Columns[0].Text = "    Name";
			m_rgslis[ilv].Lv.Columns[0].Width = 146;

			m_rgslis[ilv].Lv.Columns.Add(new ColumnHeader());
			m_rgslis[ilv].Lv.Columns[1].Text = "Size";
			m_rgslis[ilv].Lv.Columns[1].Width = 52;
			m_rgslis[ilv].Lv.Columns[1].TextAlign = HorizontalAlignment.Right;

			m_rgslis[ilv].Lv.Columns.Add(new ColumnHeader());
			m_rgslis[ilv].Lv.Columns[2].Text = "Location";
			m_rgslis[ilv].Lv.Columns[2].Width = 128;

			m_rgslis[ilv].Lv.FullRowSelect = true;
			m_rgslis[ilv].Lv.MultiSelect = false;
			m_rgslis[ilv].Lv.View = View.Details;
			m_rgslis[ilv].Lv.ListViewItemSorter = new ListViewItemComparer(1);
			m_rgslis[ilv].Lv.ColumnClick += new ColumnClickEventHandler(EH_ColumnClick);
			m_rgslis[ilv].Lv.LabelEdit = true;
		}

		private int m_islisCur = -1;

		/* S Y N C  S E A R C H  T A R G E T */
		/*----------------------------------------------------------------------------
			%%Function: SyncSearchTargetUI
			%%Qualified: SList.SListApp.SyncSearchTargetUI

			make the UI reflect what we want the sync target to be. Typically used
			on initialization
        ----------------------------------------------------------------------------*/
		void SyncSearchTargetUI(int ilv)
		{
			m_cbxSearchTarget.SelectedIndex = ilv;
		}

		public void ShowListView(int ilv)
		{
			if (m_islisCur != -1)
				m_rgslis[m_islisCur].PathSpec = m_ebSearchPath.Text;

			for (int i = 0; i < s_clvMax; i++)
			{
				m_rgslis[i].Lv.Visible = (i == ilv);
			}
			m_islisCur = ilv;

			SyncSearchTargetUI(ilv);
			m_ebSearchPath.Text = m_rgslis[ilv].PathSpec;
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
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
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
			this.label1 = new System.Windows.Forms.Label();
			this.m_pbRemove = new System.Windows.Forms.Button();
			this.m_pbAddPath = new System.Windows.Forms.Button();
			this.m_cbMarkFavored = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.m_cbxSearchTarget = new System.Windows.Forms.ComboBox();
			this.m_lv = new System.Windows.Forms.ListView();
			this.m_pbValidateSrc = new System.Windows.Forms.Button();
			this.m_cbxIgnoreList = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.m_cbAddToIgnoreList = new System.Windows.Forms.CheckBox();
			this.m_pbSaveList = new System.Windows.Forms.Button();
			this.m_pbLoadFromFile = new System.Windows.Forms.Button();
			this.m_pbSaveFileList = new System.Windows.Forms.Button();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.m_stbpMainStatus)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpFilterStatus)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpSearch)).BeginInit();
			this.SuspendLayout();
			// 
			// m_cxtListView
			// 
			this.m_cxtListView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2,
            this.menuItem6,
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
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem3});
			this.menuItem2.Text = "Add Preferred Path";
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 0;
			this.menuItem3.Text = "Placeholder";
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 2;
			this.menuItem6.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem7});
			this.menuItem6.Text = "Remove Path";
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 0;
			this.menuItem7.Text = "Placeholder";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 3;
			this.menuItem4.Text = "Select previous duplicate";
			this.menuItem4.Click += new System.EventHandler(this.EH_SelectPrevDupe);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 4;
			this.menuItem5.Text = "Select next duplicate";
			this.menuItem5.Click += new System.EventHandler(this.EH_SelectNextDupe);
			// 
			// m_ebSearchPath
			// 
			this.m_ebSearchPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_ebSearchPath.Location = new System.Drawing.Point(171, 53);
			this.m_ebSearchPath.Name = "m_ebSearchPath";
			this.m_ebSearchPath.Size = new System.Drawing.Size(520, 26);
			this.m_ebSearchPath.TabIndex = 2;
			this.m_ebSearchPath.Text = "c:\\temp";
			// 
			// m_pbSearch
			// 
			this.m_pbSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbSearch.Location = new System.Drawing.Point(1190, 47);
			this.m_pbSearch.Name = "m_pbSearch";
			this.m_pbSearch.Size = new System.Drawing.Size(115, 35);
			this.m_pbSearch.TabIndex = 4;
			this.m_pbSearch.Text = "Search";
			this.m_pbSearch.Click += new System.EventHandler(this.EH_DoSearch);
			// 
			// m_lblSearch
			// 
			this.m_lblSearch.Location = new System.Drawing.Point(26, 58);
			this.m_lblSearch.Name = "m_lblSearch";
			this.m_lblSearch.Size = new System.Drawing.Size(115, 24);
			this.m_lblSearch.TabIndex = 1;
			this.m_lblSearch.Text = "Search Spec";
			// 
			// m_cbRecurse
			// 
			this.m_cbRecurse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbRecurse.Checked = true;
			this.m_cbRecurse.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbRecurse.Location = new System.Drawing.Point(700, 57);
			this.m_cbRecurse.Name = "m_cbRecurse";
			this.m_cbRecurse.Size = new System.Drawing.Size(115, 23);
			this.m_cbRecurse.TabIndex = 3;
			this.m_cbRecurse.Text = "Recurse";
			// 
			// m_pbDuplicates
			// 
			this.m_pbDuplicates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbDuplicates.Location = new System.Drawing.Point(1198, 294);
			this.m_pbDuplicates.Name = "m_pbDuplicates";
			this.m_pbDuplicates.Size = new System.Drawing.Size(115, 35);
			this.m_pbDuplicates.TabIndex = 9;
			this.m_pbDuplicates.Text = "Uniquify";
			this.m_pbDuplicates.Click += new System.EventHandler(this.EH_Uniquify);
			// 
			// m_lblFilterBanner
			// 
			this.m_lblFilterBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblFilterBanner.Location = new System.Drawing.Point(21, 272);
			this.m_lblFilterBanner.Name = "m_lblFilterBanner";
			this.m_lblFilterBanner.Size = new System.Drawing.Size(1305, 23);
			this.m_lblFilterBanner.TabIndex = 5;
			this.m_lblFilterBanner.Tag = "Filter files";
			this.m_lblFilterBanner.Text = "Filter files ----";
			this.m_lblFilterBanner.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
			// 
			// m_lblSearchCriteria
			// 
			this.m_lblSearchCriteria.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblSearchCriteria.Location = new System.Drawing.Point(13, 23);
			this.m_lblSearchCriteria.Name = "m_lblSearchCriteria";
			this.m_lblSearchCriteria.Size = new System.Drawing.Size(1305, 24);
			this.m_lblSearchCriteria.TabIndex = 0;
			this.m_lblSearchCriteria.Tag = "Populate file lists";
			this.m_lblSearchCriteria.Text = "Populate file lists ----";
			this.m_lblSearchCriteria.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
			// 
			// m_cbCompareFiles
			// 
			this.m_cbCompareFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbCompareFiles.Location = new System.Drawing.Point(838, 305);
			this.m_cbCompareFiles.Name = "m_cbCompareFiles";
			this.m_cbCompareFiles.Size = new System.Drawing.Size(243, 24);
			this.m_cbCompareFiles.TabIndex = 8;
			this.m_cbCompareFiles.Text = "Real Dupe Checking";
			// 
			// m_stb
			// 
			this.m_stb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_stb.Dock = System.Windows.Forms.DockStyle.None;
			this.m_stb.Location = new System.Drawing.Point(0, 707);
			this.m_stb.Name = "m_stb";
			this.m_stb.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.m_stbpMainStatus,
            this.m_stbpFilterStatus,
            this.m_stbpSearch});
			this.m_stb.ShowPanels = true;
			this.m_stb.Size = new System.Drawing.Size(1331, 35);
			this.m_stb.TabIndex = 9;
			// 
			// m_stbpMainStatus
			// 
			this.m_stbpMainStatus.Name = "m_stbpMainStatus";
			this.m_stbpMainStatus.Width = 300;
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
			// m_prbar
			// 
			this.m_prbar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_prbar.Location = new System.Drawing.Point(179, 713);
			this.m_prbar.Name = "m_prbar";
			this.m_prbar.Size = new System.Drawing.Size(304, 22);
			this.m_prbar.TabIndex = 10;
			this.m_prbar.Visible = false;
			// 
			// m_lblActions
			// 
			this.m_lblActions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblActions.Location = new System.Drawing.Point(21, 389);
			this.m_lblActions.Name = "m_lblActions";
			this.m_lblActions.Size = new System.Drawing.Size(1305, 23);
			this.m_lblActions.TabIndex = 15;
			this.m_lblActions.Tag = "Perform actions";
			this.m_lblActions.Text = "Perform actions ----";
			this.m_lblActions.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
			// 
			// m_ebRegEx
			// 
			this.m_ebRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_ebRegEx.Location = new System.Drawing.Point(162, 301);
			this.m_ebRegEx.Name = "m_ebRegEx";
			this.m_ebRegEx.Size = new System.Drawing.Size(666, 26);
			this.m_ebRegEx.TabIndex = 7;
			// 
			// m_lblRegEx
			// 
			this.m_lblRegEx.Location = new System.Drawing.Point(34, 307);
			this.m_lblRegEx.Name = "m_lblRegEx";
			this.m_lblRegEx.Size = new System.Drawing.Size(115, 58);
			this.m_lblRegEx.TabIndex = 6;
			this.m_lblRegEx.Text = "Regular Expressions";
			// 
			// m_pbMove
			// 
			this.m_pbMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbMove.Location = new System.Drawing.Point(1070, 412);
			this.m_pbMove.Name = "m_pbMove";
			this.m_pbMove.Size = new System.Drawing.Size(115, 35);
			this.m_pbMove.TabIndex = 18;
			this.m_pbMove.Text = "Move";
			this.m_pbMove.Click += new System.EventHandler(this.EH_DoMove);
			// 
			// m_pbDelete
			// 
			this.m_pbDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbDelete.Location = new System.Drawing.Point(1198, 412);
			this.m_pbDelete.Name = "m_pbDelete";
			this.m_pbDelete.Size = new System.Drawing.Size(115, 35);
			this.m_pbDelete.TabIndex = 19;
			this.m_pbDelete.Text = "Delete";
			this.m_pbDelete.Click += new System.EventHandler(this.EH_DoDelete);
			// 
			// m_pbToggle
			// 
			this.m_pbToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbToggle.Location = new System.Drawing.Point(1198, 342);
			this.m_pbToggle.Name = "m_pbToggle";
			this.m_pbToggle.Size = new System.Drawing.Size(115, 35);
			this.m_pbToggle.TabIndex = 14;
			this.m_pbToggle.Text = "Toggle All";
			this.m_pbToggle.Click += new System.EventHandler(this.EH_ToggleAll);
			// 
			// m_pbClear
			// 
			this.m_pbClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbClear.Location = new System.Drawing.Point(1070, 342);
			this.m_pbClear.Name = "m_pbClear";
			this.m_pbClear.Size = new System.Drawing.Size(115, 35);
			this.m_pbClear.TabIndex = 13;
			this.m_pbClear.Text = "Clear All";
			this.m_pbClear.Click += new System.EventHandler(this.EH_ClearAll);
			// 
			// m_lblMoveTo
			// 
			this.m_lblMoveTo.Location = new System.Drawing.Point(34, 424);
			this.m_lblMoveTo.Name = "m_lblMoveTo";
			this.m_lblMoveTo.Size = new System.Drawing.Size(89, 23);
			this.m_lblMoveTo.TabIndex = 16;
			this.m_lblMoveTo.Text = "Move to";
			// 
			// m_ebMovePath
			// 
			this.m_ebMovePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_ebMovePath.Location = new System.Drawing.Point(162, 418);
			this.m_ebMovePath.Name = "m_ebMovePath";
			this.m_ebMovePath.Size = new System.Drawing.Size(882, 26);
			this.m_ebMovePath.TabIndex = 17;
			// 
			// m_pbMatchRegex
			// 
			this.m_pbMatchRegex.Location = new System.Drawing.Point(162, 342);
			this.m_pbMatchRegex.Name = "m_pbMatchRegex";
			this.m_pbMatchRegex.Size = new System.Drawing.Size(128, 35);
			this.m_pbMatchRegex.TabIndex = 10;
			this.m_pbMatchRegex.Text = "Match Regex";
			this.m_pbMatchRegex.Click += new System.EventHandler(this.EH_MatchRegex);
			// 
			// m_pbRemoveRegex
			// 
			this.m_pbRemoveRegex.Location = new System.Drawing.Point(290, 342);
			this.m_pbRemoveRegex.Name = "m_pbRemoveRegex";
			this.m_pbRemoveRegex.Size = new System.Drawing.Size(128, 35);
			this.m_pbRemoveRegex.TabIndex = 11;
			this.m_pbRemoveRegex.Text = "Filter Regex";
			this.m_pbRemoveRegex.Click += new System.EventHandler(this.EH_FilterRegex);
			// 
			// m_pbCheckRegex
			// 
			this.m_pbCheckRegex.Location = new System.Drawing.Point(418, 342);
			this.m_pbCheckRegex.Name = "m_pbCheckRegex";
			this.m_pbCheckRegex.Size = new System.Drawing.Size(128, 35);
			this.m_pbCheckRegex.TabIndex = 12;
			this.m_pbCheckRegex.Text = "Check Regex";
			this.m_pbCheckRegex.Click += new System.EventHandler(this.EH_CheckRegex);
			// 
			// m_prbarOverall
			// 
			this.m_prbarOverall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_prbarOverall.Location = new System.Drawing.Point(488, 713);
			this.m_prbarOverall.Maximum = 1000;
			this.m_prbarOverall.Name = "m_prbarOverall";
			this.m_prbarOverall.Size = new System.Drawing.Size(304, 22);
			this.m_prbarOverall.TabIndex = 23;
			this.m_prbarOverall.Visible = false;
			// 
			// m_pbSmartMatch
			// 
			this.m_pbSmartMatch.Location = new System.Drawing.Point(546, 342);
			this.m_pbSmartMatch.Name = "m_pbSmartMatch";
			this.m_pbSmartMatch.Size = new System.Drawing.Size(128, 35);
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
			this.m_lbPrefPath.FormattingEnabled = true;
			this.m_lbPrefPath.ItemHeight = 20;
			this.m_lbPrefPath.Location = new System.Drawing.Point(179, 140);
			this.m_lbPrefPath.Name = "m_lbPrefPath";
			this.m_lbPrefPath.Size = new System.Drawing.Size(661, 84);
			this.m_lbPrefPath.TabIndex = 25;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(34, 140);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(136, 25);
			this.label1.TabIndex = 26;
			this.label1.Text = "Preferred Paths";
			// 
			// m_pbRemove
			// 
			this.m_pbRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbRemove.Location = new System.Drawing.Point(1198, 206);
			this.m_pbRemove.Name = "m_pbRemove";
			this.m_pbRemove.Size = new System.Drawing.Size(115, 35);
			this.m_pbRemove.TabIndex = 27;
			this.m_pbRemove.Text = "Remove";
			// 
			// m_pbAddPath
			// 
			this.m_pbAddPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbAddPath.Location = new System.Drawing.Point(1198, 164);
			this.m_pbAddPath.Name = "m_pbAddPath";
			this.m_pbAddPath.Size = new System.Drawing.Size(115, 35);
			this.m_pbAddPath.TabIndex = 28;
			this.m_pbAddPath.Text = "Add Path";
			// 
			// m_cbMarkFavored
			// 
			this.m_cbMarkFavored.AutoSize = true;
			this.m_cbMarkFavored.Location = new System.Drawing.Point(22, 206);
			this.m_cbMarkFavored.Name = "m_cbMarkFavored";
			this.m_cbMarkFavored.Size = new System.Drawing.Size(132, 24);
			this.m_cbMarkFavored.TabIndex = 29;
			this.m_cbMarkFavored.Text = "Mark Favored";
			this.m_cbMarkFavored.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(26, 95);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(115, 23);
			this.label2.TabIndex = 30;
			this.label2.Text = "File list";
			// 
			// m_cbxSearchTarget
			// 
			this.m_cbxSearchTarget.FormattingEnabled = true;
			this.m_cbxSearchTarget.Items.AddRange(new object[] {
            "Source",
            "Destination"});
			this.m_cbxSearchTarget.Location = new System.Drawing.Point(171, 92);
			this.m_cbxSearchTarget.Name = "m_cbxSearchTarget";
			this.m_cbxSearchTarget.Size = new System.Drawing.Size(194, 28);
			this.m_cbxSearchTarget.TabIndex = 31;
			this.m_cbxSearchTarget.SelectedIndexChanged += new System.EventHandler(this.DoSearchTargetChange);
			// 
			// m_lv
			// 
			this.m_lv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lv.CheckBoxes = true;
			this.m_lv.ContextMenu = this.m_cxtListView;
			this.m_lv.HideSelection = false;
			this.m_lv.Location = new System.Drawing.Point(26, 460);
			this.m_lv.Name = "m_lv";
			this.m_lv.Size = new System.Drawing.Size(1279, 235);
			this.m_lv.TabIndex = 20;
			this.m_lv.UseCompatibleStateImageBehavior = false;
			this.m_lv.Visible = false;
			this.m_lv.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.EH_HandleEdit);
			// 
			// m_pbValidateSrc
			// 
			this.m_pbValidateSrc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbValidateSrc.Location = new System.Drawing.Point(1070, 294);
			this.m_pbValidateSrc.Name = "m_pbValidateSrc";
			this.m_pbValidateSrc.Size = new System.Drawing.Size(118, 35);
			this.m_pbValidateSrc.TabIndex = 33;
			this.m_pbValidateSrc.Text = "Validate Src";
			this.m_pbValidateSrc.Click += new System.EventHandler(this.EH_ValidateSrc);
			// 
			// m_cbxIgnoreList
			// 
			this.m_cbxIgnoreList.FormattingEnabled = true;
			this.m_cbxIgnoreList.Location = new System.Drawing.Point(459, 92);
			this.m_cbxIgnoreList.Name = "m_cbxIgnoreList";
			this.m_cbxIgnoreList.Size = new System.Drawing.Size(194, 28);
			this.m_cbxIgnoreList.TabIndex = 35;
			this.m_cbxIgnoreList.SelectedIndexChanged += new System.EventHandler(this.EH_HandleIgnoreListSelect);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(374, 95);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(116, 23);
			this.label3.TabIndex = 34;
			this.label3.Text = "Ignore list";
			// 
			// m_cbAddToIgnoreList
			// 
			this.m_cbAddToIgnoreList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cbAddToIgnoreList.Checked = true;
			this.m_cbAddToIgnoreList.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbAddToIgnoreList.Location = new System.Drawing.Point(1062, 94);
			this.m_cbAddToIgnoreList.Name = "m_cbAddToIgnoreList";
			this.m_cbAddToIgnoreList.Size = new System.Drawing.Size(243, 30);
			this.m_cbAddToIgnoreList.TabIndex = 36;
			this.m_cbAddToIgnoreList.Text = "Automatically add ignore";
			this.m_toolTip.SetToolTip(this.m_cbAddToIgnoreList, "Add path to ignore list when \"Remove...\" is selected from ListView context menu.");
			// 
			// m_pbSaveList
			// 
			this.m_pbSaveList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbSaveList.Location = new System.Drawing.Point(679, 92);
			this.m_pbSaveList.Name = "m_pbSaveList";
			this.m_pbSaveList.Size = new System.Drawing.Size(116, 35);
			this.m_pbSaveList.TabIndex = 37;
			this.m_pbSaveList.Text = "Save List";
			this.m_pbSaveList.Click += new System.EventHandler(this.EH_DoSaveIgnoreList);
			// 
			// m_pbLoadFromFile
			// 
			this.m_pbLoadFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbLoadFromFile.Location = new System.Drawing.Point(825, 51);
			this.m_pbLoadFromFile.Name = "m_pbLoadFromFile";
			this.m_pbLoadFromFile.Size = new System.Drawing.Size(138, 35);
			this.m_pbLoadFromFile.TabIndex = 38;
			this.m_pbLoadFromFile.Text = "Load FileList";
			this.m_pbLoadFromFile.Click += new System.EventHandler(this.EH_LoadFileListFromFile);
			// 
			// m_pbSaveFileList
			// 
			this.m_pbSaveFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pbSaveFileList.Location = new System.Drawing.Point(972, 53);
			this.m_pbSaveFileList.Name = "m_pbSaveFileList";
			this.m_pbSaveFileList.Size = new System.Drawing.Size(138, 35);
			this.m_pbSaveFileList.TabIndex = 39;
			this.m_pbSaveFileList.Text = "Save FileList";
			this.m_pbSaveFileList.Click += new System.EventHandler(this.EH_SaveFileListToFile);
			// 
			// SListApp
			// 
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
			this.ClientSize = new System.Drawing.Size(1331, 739);
			this.Controls.Add(this.m_pbSaveFileList);
			this.Controls.Add(this.m_pbLoadFromFile);
			this.Controls.Add(this.m_pbSaveList);
			this.Controls.Add(this.m_cbAddToIgnoreList);
			this.Controls.Add(this.m_cbxIgnoreList);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.m_pbValidateSrc);
			this.Controls.Add(this.m_cbxSearchTarget);
			this.Controls.Add(this.label2);
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
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleDragEnter);
			this.DragLeave += new System.EventHandler(this.HandleDragLeave);
			((System.ComponentModel.ISupportInitialize)(this.m_stbpMainStatus)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpFilterStatus)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_stbpSearch)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region EventHandlers

		private void DoSearchTargetChange(object sender, EventArgs e)
		{
			ShowListView(m_cbxSearchTarget.SelectedIndex);
		}

		private void EH_ColumnClick(object o, ColumnClickEventArgs e)
		{
			m_model.ChangeListViewSort((ListView)o, e.Column);
		}

		private void EH_Uniquify(object sender, System.EventArgs e)
		{
			m_model.BuildUniqueFileList();
		}

		private void EH_DoSearch(object sender, System.EventArgs e)
		{
			m_model.BuildFileList();
		}

		private void EH_RenderHeadingLine(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			RenderSupp.RenderHeadingLine(sender, e);
		}

		private void EH_DoMove(object sender, System.EventArgs e)
		{
			SmartList.MoveSelectedFiles(LvCur, m_ebMovePath.Text, m_stbpMainStatus);
		}

		private void EH_DoDelete(object sender, System.EventArgs e) { }

		private void EH_ToggleAll(object sender, System.EventArgs e)
		{
			m_model.ToggleAllListViewItems(LvCur);
		}

		private void EH_ClearAll(object sender, System.EventArgs e)
		{
			m_model.UncheckAllListViewItems(LvCur);
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
			ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

			if (slvic != null && slvic.Count >= 1)
			{
				m_model.LaunchSli((SLItem)slvic[0].Tag);
			}
		}

		private void EH_SmartMatchClick(object sender, System.EventArgs e)
		{
			sCancelled = SmartList.SCalcMatchingListViewItems(LvCur, m_ebRegEx.Text, sCancelled);
		}

		private void EH_HandleEdit(object sender, System.Windows.Forms.LabelEditEventArgs e)
		{
			SLItem sli = (SLItem)LvCur.Items[e.Item].Tag;

			if (!SmartList.FRenameFile(sli.m_sPath, sli.m_sName, sli.m_sPath, e.Label))
			{
				e.CancelEdit = true;
			}
			else
			{
				sli.m_sName = e.Label;
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
				if (TCore.UI.InputBox.ShowInputBox("New ignore list name", "Ignore list name", "", out sName))
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
		}

		private void EH_SaveFileListToFile(object sender, EventArgs e)
		{
			m_model.SaveFileListToFile(SlisCur);
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
			MenuItem mni = (MenuItem)sender;
			m_model.AddPreferredPath(mni.Text);
		}

		private void EH_DoContextPopup(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

			if (slvic != null && slvic.Count >= 1)
			{
				SLItem sli = (SLItem)slvic[0].Tag;

				ContextMenu cm = (ContextMenu)sender;

				MenuItem mni = cm.MenuItems[1];

				mni.MenuItems.Clear();
				// break the path into pieces and add an item for each piece
				Path.GetDirectoryName(sli.m_sPath);
				string[] rgs = sli.m_sPath.Split('\\');

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

				mni = cm.MenuItems[2];
				mni.MenuItems.Clear();

				sSub = "";
				foreach (string s in rgs)
				{
					MenuItem mniNew = new MenuItem();

					if (sSub != "")
						sSub += "\\" + s;
					else
						sSub = s;

					mniNew.Text = sSub;
					mniNew.Click += new EventHandler(EH_RemovePath);

					mni.MenuItems.Add(mniNew);
				}
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
			ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

			if (slvic != null && slvic.Count >= 1)
			{
				SLItem sli = (SLItem)slvic[0].Tag;

				SLItem sliSel = sli.Prev;
				m_model.Select(sliSel);
			}
		}

		private void EH_SelectNextDupe(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

			if (slvic != null && slvic.Count >= 1)
			{
				SLItem sli = (SLItem)slvic[0].Tag;

				SLItem sliSel = sli.Next;
				m_model.Select(sliSel);
			}
		}
		#endregion

		#region ISmartListUi

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

		public void AddIgnoreListItem(string text)
		{
			m_cbxIgnoreList.Items.Add(text);
		}

		public SLISet GetSliSet(int iListView)
		{
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

		class ProgressBarStatus
		{
			private ProgressBar m_bar;

			public ProgressBarStatus(ProgressBar bar)
			{
				m_bar = bar;
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
				MacProgressBarOverall = mac;
				IncrementProgressBarOverall = Math.Max(1, mac / m_bar.Maximum);
				m_bar.Value = 0;
			}

			public void Update(long i, OnProgressUpdateDelegate del)
			{
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

