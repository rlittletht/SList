using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Media;
using System.Resources;
using System.Text;
using System.Xml;
using NUnit.Framework;
using TCore.UI;

namespace SList
{
    public partial class SListApp : System.Windows.Forms.Form
    {
        private byte[] m_rgb1;
        private byte[] m_rgb2;
        private IgnoreList m_ign;

        public const int lcbMax = 4*1024*1024;
        public static string s_sRegRoot = "Software\\Thetasoft\\SList";

        ListView LvCur
        {
            get { return SlisCur.Lv; }
        }

        SLISet SlisCur
        {
            get { return m_rgslis[m_islisCur]; }
        }

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

        #region AppHost

        public SListApp()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            m_rgb1 = new byte[lcbMax];
            m_rgb2 = new byte[lcbMax];

            InitializeListViews();
            InitializeListView(s_ilvSource);
            InitializeListView(s_ilvDest);
            InitIgnoreLists();
            ShowListView(s_ilvSource);
            //
            // TODO: Add any constructor code after InitializeComponent call
            //
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

        #region Initialization

        void InitIgnoreLists()
        {
            m_ign = new IgnoreList();
            m_ign.LoadIgnoreListNames(s_sRegRoot);

            m_cbxIgnoreList.Items.Add("<Create List...>");
            m_cbxIgnoreList.Items.Add("<Copy current list...>");
            foreach (string s in m_ign.IgnoreLists)
                {
                m_cbxIgnoreList.Items.Add(s);
                }
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

        void ShowListView(int ilv)
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

        /* S Y N C  S E A R C H  T A R G E T */
        /*----------------------------------------------------------------------------
        	%%Function: SyncSearchTargetUI
        	%%Qualified: SList.SListApp.SyncSearchTargetUI
        	%%Contact: rlittle
        	
            make the UI reflect what we want the sync target to be. Typically used
            on initialization
        ----------------------------------------------------------------------------*/
        void SyncSearchTargetUI(int ilv)
        {
            m_cbxSearchTarget.SelectedIndex = ilv;
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
            this.m_ebSearchPath.Location = new System.Drawing.Point(107, 36);
            this.m_ebSearchPath.Name = "m_ebSearchPath";
            this.m_ebSearchPath.Size = new System.Drawing.Size(314, 20);
            this.m_ebSearchPath.TabIndex = 2;
            this.m_ebSearchPath.Text = "c:\\temp";
            // 
            // m_pbSearch
            // 
            this.m_pbSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbSearch.Location = new System.Drawing.Point(733, 32);
            this.m_pbSearch.Name = "m_pbSearch";
            this.m_pbSearch.Size = new System.Drawing.Size(72, 24);
            this.m_pbSearch.TabIndex = 4;
            this.m_pbSearch.Text = "Search";
            this.m_pbSearch.Click += new System.EventHandler(this.EH_DoSearch);
            // 
            // m_lblSearch
            // 
            this.m_lblSearch.Location = new System.Drawing.Point(16, 40);
            this.m_lblSearch.Name = "m_lblSearch";
            this.m_lblSearch.Size = new System.Drawing.Size(72, 16);
            this.m_lblSearch.TabIndex = 1;
            this.m_lblSearch.Text = "Search Spec";
            // 
            // m_cbRecurse
            // 
            this.m_cbRecurse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cbRecurse.Checked = true;
            this.m_cbRecurse.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbRecurse.Location = new System.Drawing.Point(427, 39);
            this.m_cbRecurse.Name = "m_cbRecurse";
            this.m_cbRecurse.Size = new System.Drawing.Size(72, 16);
            this.m_cbRecurse.TabIndex = 3;
            this.m_cbRecurse.Text = "Recurse";
            // 
            // m_pbDuplicates
            // 
            this.m_pbDuplicates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbDuplicates.Location = new System.Drawing.Point(738, 201);
            this.m_pbDuplicates.Name = "m_pbDuplicates";
            this.m_pbDuplicates.Size = new System.Drawing.Size(72, 24);
            this.m_pbDuplicates.TabIndex = 9;
            this.m_pbDuplicates.Text = "Uniquify";
            this.m_pbDuplicates.Click += new System.EventHandler(this.EH_Uniquify);
            // 
            // m_lblFilterBanner
            // 
            this.m_lblFilterBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblFilterBanner.Location = new System.Drawing.Point(13, 186);
            this.m_lblFilterBanner.Name = "m_lblFilterBanner";
            this.m_lblFilterBanner.Size = new System.Drawing.Size(805, 16);
            this.m_lblFilterBanner.TabIndex = 5;
            this.m_lblFilterBanner.Tag = "Filter files";
            this.m_lblFilterBanner.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
            // 
            // m_lblSearchCriteria
            // 
            this.m_lblSearchCriteria.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblSearchCriteria.Location = new System.Drawing.Point(8, 16);
            this.m_lblSearchCriteria.Name = "m_lblSearchCriteria";
            this.m_lblSearchCriteria.Size = new System.Drawing.Size(805, 16);
            this.m_lblSearchCriteria.TabIndex = 0;
            this.m_lblSearchCriteria.Tag = "Populate file lists";
            this.m_lblSearchCriteria.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
            // 
            // m_cbCompareFiles
            // 
            this.m_cbCompareFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cbCompareFiles.Location = new System.Drawing.Point(513, 209);
            this.m_cbCompareFiles.Name = "m_cbCompareFiles";
            this.m_cbCompareFiles.Size = new System.Drawing.Size(152, 16);
            this.m_cbCompareFiles.TabIndex = 8;
            this.m_cbCompareFiles.Text = "Real Dupe Checking";
            // 
            // m_stb
            // 
            this.m_stb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_stb.Dock = System.Windows.Forms.DockStyle.None;
            this.m_stb.Location = new System.Drawing.Point(0, 717);
            this.m_stb.Name = "m_stb";
            this.m_stb.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.m_stbpMainStatus,
            this.m_stbpFilterStatus,
            this.m_stbpSearch});
            this.m_stb.ShowPanels = true;
            this.m_stb.Size = new System.Drawing.Size(821, 24);
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
            this.m_prbar.Location = new System.Drawing.Point(112, 721);
            this.m_prbar.Name = "m_prbar";
            this.m_prbar.Size = new System.Drawing.Size(190, 15);
            this.m_prbar.TabIndex = 10;
            this.m_prbar.Visible = false;
            // 
            // m_lblActions
            // 
            this.m_lblActions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblActions.Location = new System.Drawing.Point(13, 266);
            this.m_lblActions.Name = "m_lblActions";
            this.m_lblActions.Size = new System.Drawing.Size(805, 16);
            this.m_lblActions.TabIndex = 15;
            this.m_lblActions.Tag = "Perform actions";
            this.m_lblActions.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
            // 
            // m_ebRegEx
            // 
            this.m_ebRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ebRegEx.Location = new System.Drawing.Point(101, 206);
            this.m_ebRegEx.Name = "m_ebRegEx";
            this.m_ebRegEx.Size = new System.Drawing.Size(406, 20);
            this.m_ebRegEx.TabIndex = 7;
            // 
            // m_lblRegEx
            // 
            this.m_lblRegEx.Location = new System.Drawing.Point(21, 210);
            this.m_lblRegEx.Name = "m_lblRegEx";
            this.m_lblRegEx.Size = new System.Drawing.Size(72, 40);
            this.m_lblRegEx.TabIndex = 6;
            this.m_lblRegEx.Text = "Regular Expressions";
            // 
            // m_pbMove
            // 
            this.m_pbMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbMove.Location = new System.Drawing.Point(658, 282);
            this.m_pbMove.Name = "m_pbMove";
            this.m_pbMove.Size = new System.Drawing.Size(72, 24);
            this.m_pbMove.TabIndex = 18;
            this.m_pbMove.Text = "Move";
            this.m_pbMove.Click += new System.EventHandler(this.EH_DoMove);
            // 
            // m_pbDelete
            // 
            this.m_pbDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbDelete.Location = new System.Drawing.Point(738, 282);
            this.m_pbDelete.Name = "m_pbDelete";
            this.m_pbDelete.Size = new System.Drawing.Size(72, 24);
            this.m_pbDelete.TabIndex = 19;
            this.m_pbDelete.Text = "Delete";
            this.m_pbDelete.Click += new System.EventHandler(this.EH_DoDelete);
            // 
            // m_pbToggle
            // 
            this.m_pbToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbToggle.Location = new System.Drawing.Point(738, 234);
            this.m_pbToggle.Name = "m_pbToggle";
            this.m_pbToggle.Size = new System.Drawing.Size(72, 24);
            this.m_pbToggle.TabIndex = 14;
            this.m_pbToggle.Text = "Toggle All";
            this.m_pbToggle.Click += new System.EventHandler(this.EH_ToggleAll);
            // 
            // m_pbClear
            // 
            this.m_pbClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbClear.Location = new System.Drawing.Point(658, 234);
            this.m_pbClear.Name = "m_pbClear";
            this.m_pbClear.Size = new System.Drawing.Size(72, 24);
            this.m_pbClear.TabIndex = 13;
            this.m_pbClear.Text = "Clear All";
            this.m_pbClear.Click += new System.EventHandler(this.EH_ClearAll);
            // 
            // m_lblMoveTo
            // 
            this.m_lblMoveTo.Location = new System.Drawing.Point(21, 290);
            this.m_lblMoveTo.Name = "m_lblMoveTo";
            this.m_lblMoveTo.Size = new System.Drawing.Size(56, 16);
            this.m_lblMoveTo.TabIndex = 16;
            this.m_lblMoveTo.Text = "Move to";
            // 
            // m_ebMovePath
            // 
            this.m_ebMovePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_ebMovePath.Location = new System.Drawing.Point(101, 286);
            this.m_ebMovePath.Name = "m_ebMovePath";
            this.m_ebMovePath.Size = new System.Drawing.Size(541, 20);
            this.m_ebMovePath.TabIndex = 17;
            // 
            // m_pbMatchRegex
            // 
            this.m_pbMatchRegex.Location = new System.Drawing.Point(101, 234);
            this.m_pbMatchRegex.Name = "m_pbMatchRegex";
            this.m_pbMatchRegex.Size = new System.Drawing.Size(80, 24);
            this.m_pbMatchRegex.TabIndex = 10;
            this.m_pbMatchRegex.Text = "Match Regex";
            this.m_pbMatchRegex.Click += new System.EventHandler(this.EH_MatchRegex);
            // 
            // m_pbRemoveRegex
            // 
            this.m_pbRemoveRegex.Location = new System.Drawing.Point(181, 234);
            this.m_pbRemoveRegex.Name = "m_pbRemoveRegex";
            this.m_pbRemoveRegex.Size = new System.Drawing.Size(80, 24);
            this.m_pbRemoveRegex.TabIndex = 11;
            this.m_pbRemoveRegex.Text = "Filter Regex";
            this.m_pbRemoveRegex.Click += new System.EventHandler(this.EH_FilterRegex);
            // 
            // m_pbCheckRegex
            // 
            this.m_pbCheckRegex.Location = new System.Drawing.Point(261, 234);
            this.m_pbCheckRegex.Name = "m_pbCheckRegex";
            this.m_pbCheckRegex.Size = new System.Drawing.Size(80, 24);
            this.m_pbCheckRegex.TabIndex = 12;
            this.m_pbCheckRegex.Text = "Check Regex";
            this.m_pbCheckRegex.Click += new System.EventHandler(this.EH_CheckRegex);
            // 
            // m_prbarOverall
            // 
            this.m_prbarOverall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_prbarOverall.Location = new System.Drawing.Point(305, 721);
            this.m_prbarOverall.Maximum = 1000;
            this.m_prbarOverall.Name = "m_prbarOverall";
            this.m_prbarOverall.Size = new System.Drawing.Size(190, 15);
            this.m_prbarOverall.TabIndex = 23;
            this.m_prbarOverall.Visible = false;
            // 
            // m_pbSmartMatch
            // 
            this.m_pbSmartMatch.Location = new System.Drawing.Point(341, 234);
            this.m_pbSmartMatch.Name = "m_pbSmartMatch";
            this.m_pbSmartMatch.Size = new System.Drawing.Size(80, 24);
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
            this.m_lbPrefPath.Location = new System.Drawing.Point(112, 96);
            this.m_lbPrefPath.Name = "m_lbPrefPath";
            this.m_lbPrefPath.Size = new System.Drawing.Size(413, 69);
            this.m_lbPrefPath.TabIndex = 25;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(21, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 17);
            this.label1.TabIndex = 26;
            this.label1.Text = "Preferred Paths";
            // 
            // m_pbRemove
            // 
            this.m_pbRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbRemove.Location = new System.Drawing.Point(738, 141);
            this.m_pbRemove.Name = "m_pbRemove";
            this.m_pbRemove.Size = new System.Drawing.Size(72, 24);
            this.m_pbRemove.TabIndex = 27;
            this.m_pbRemove.Text = "Remove";
            // 
            // m_pbAddPath
            // 
            this.m_pbAddPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbAddPath.Location = new System.Drawing.Point(738, 112);
            this.m_pbAddPath.Name = "m_pbAddPath";
            this.m_pbAddPath.Size = new System.Drawing.Size(72, 24);
            this.m_pbAddPath.TabIndex = 28;
            this.m_pbAddPath.Text = "Add Path";
            // 
            // m_cbMarkFavored
            // 
            this.m_cbMarkFavored.AutoSize = true;
            this.m_cbMarkFavored.Location = new System.Drawing.Point(14, 141);
            this.m_cbMarkFavored.Name = "m_cbMarkFavored";
            this.m_cbMarkFavored.Size = new System.Drawing.Size(92, 17);
            this.m_cbMarkFavored.TabIndex = 29;
            this.m_cbMarkFavored.Text = "Mark Favored";
            this.m_cbMarkFavored.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 30;
            this.label2.Text = "Search target";
            // 
            // m_cbxSearchTarget
            // 
            this.m_cbxSearchTarget.FormattingEnabled = true;
            this.m_cbxSearchTarget.Items.AddRange(new object[] {
            "Source",
            "Destination"});
            this.m_cbxSearchTarget.Location = new System.Drawing.Point(107, 63);
            this.m_cbxSearchTarget.Name = "m_cbxSearchTarget";
            this.m_cbxSearchTarget.Size = new System.Drawing.Size(121, 21);
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
            this.m_lv.Location = new System.Drawing.Point(16, 315);
            this.m_lv.Name = "m_lv";
            this.m_lv.Size = new System.Drawing.Size(789, 394);
            this.m_lv.TabIndex = 20;
            this.m_lv.UseCompatibleStateImageBehavior = false;
            this.m_lv.Visible = false;
            this.m_lv.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.EH_HandleEdit);
            // 
            // m_pbValidateSrc
            // 
            this.m_pbValidateSrc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbValidateSrc.Location = new System.Drawing.Point(658, 201);
            this.m_pbValidateSrc.Name = "m_pbValidateSrc";
            this.m_pbValidateSrc.Size = new System.Drawing.Size(74, 24);
            this.m_pbValidateSrc.TabIndex = 33;
            this.m_pbValidateSrc.Text = "Validate Src";
            this.m_pbValidateSrc.Click += new System.EventHandler(this.EH_ValidateSrc);
            // 
            // m_cbxIgnoreList
            // 
            this.m_cbxIgnoreList.FormattingEnabled = true;
            this.m_cbxIgnoreList.Location = new System.Drawing.Point(287, 63);
            this.m_cbxIgnoreList.Name = "m_cbxIgnoreList";
            this.m_cbxIgnoreList.Size = new System.Drawing.Size(121, 21);
            this.m_cbxIgnoreList.TabIndex = 35;
            this.m_cbxIgnoreList.SelectedIndexChanged += new System.EventHandler(this.EH_HandleIgnoreListSelect);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(234, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 16);
            this.label3.TabIndex = 34;
            this.label3.Text = "Ignore list";
            // 
            // m_cbAddToIgnoreList
            // 
            this.m_cbAddToIgnoreList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cbAddToIgnoreList.Checked = true;
            this.m_cbAddToIgnoreList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbAddToIgnoreList.Location = new System.Drawing.Point(653, 64);
            this.m_cbAddToIgnoreList.Name = "m_cbAddToIgnoreList";
            this.m_cbAddToIgnoreList.Size = new System.Drawing.Size(152, 21);
            this.m_cbAddToIgnoreList.TabIndex = 36;
            this.m_cbAddToIgnoreList.Text = "Automatically add ignore";
            // 
            // m_pbSaveList
            // 
            this.m_pbSaveList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbSaveList.Location = new System.Drawing.Point(414, 63);
            this.m_pbSaveList.Name = "m_pbSaveList";
            this.m_pbSaveList.Size = new System.Drawing.Size(72, 24);
            this.m_pbSaveList.TabIndex = 37;
            this.m_pbSaveList.Text = "Save List";
            this.m_pbSaveList.Click += new System.EventHandler(this.EH_DoSaveIgnoreList);
            // 
            // m_pbLoadFromFile
            // 
            this.m_pbLoadFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbLoadFromFile.Location = new System.Drawing.Point(505, 35);
            this.m_pbLoadFromFile.Name = "m_pbLoadFromFile";
            this.m_pbLoadFromFile.Size = new System.Drawing.Size(86, 24);
            this.m_pbLoadFromFile.TabIndex = 38;
            this.m_pbLoadFromFile.Text = "Load FileList";
            this.m_pbLoadFromFile.Click += new System.EventHandler(this.EH_LoadFileListFromFile);
            // 
            // m_pbSaveFileList
            // 
            this.m_pbSaveFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pbSaveFileList.Location = new System.Drawing.Point(597, 36);
            this.m_pbSaveFileList.Name = "m_pbSaveFileList";
            this.m_pbSaveFileList.Size = new System.Drawing.Size(86, 24);
            this.m_pbSaveFileList.TabIndex = 39;
            this.m_pbSaveFileList.Text = "Save FileList";
            this.m_pbSaveFileList.Click += new System.EventHandler(this.EH_SaveFileListToFile);
            // 
            // SListApp
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(821, 739);
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

        private void EH_ColumnClick(object o, ColumnClickEventArgs e)
        {
            ChangeListViewSort((ListView) o, e.Column);
        }

        private void EH_Uniquify(object sender, System.EventArgs e)
        {
            BuildUniqueFileList();
        }

        private void EH_DoSearch(object sender, System.EventArgs e)
        {
            BuildFileList();
        }

        private void EH_RenderHeadingLine(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Label lbl = (Label) sender;
            string s = (string) lbl.Tag;

            SizeF sf = e.Graphics.MeasureString(s, lbl.Font);
            int nWidth = (int) sf.Width;
            int nHeight = (int) sf.Height;

            e.Graphics.DrawString(s, lbl.Font, new SolidBrush(Color.SlateBlue), 0, 0); // new System.Drawing.Point(0, (lbl.Width - nWidth) / 2));
            e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Gray), 1), 6 + nWidth + 1, (nHeight/2), lbl.Width, (nHeight/2));
        }

        private void EH_DoMove(object sender, System.EventArgs e)
        {
            MoveSelectedFiles(LvCur, m_ebMovePath.Text, m_stbpMainStatus);
        }

        private void EH_DoDelete(object sender, System.EventArgs e) {}

        private void EH_ToggleAll(object sender, System.EventArgs e)
        {
            ToggleAllListViewItems(LvCur);
        }

        private void EH_ClearAll(object sender, System.EventArgs e)
        {
            UncheckAllListViewItems(LvCur);
        }

        private void EH_MatchRegex(object sender, System.EventArgs e)
        {
            DoRegex(RegexOp.Match);
        }

        private void EH_FilterRegex(object sender, System.EventArgs e)
        {
            DoRegex(RegexOp.Filter);
        }

        private void EH_CheckRegex(object sender, System.EventArgs e)
        {
            DoRegex(RegexOp.Check);
        }

        private void EH_HandleExecuteMenu(object sender, System.EventArgs e)
        {
            ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

            if (slvic != null && slvic.Count >= 1)
                {
                LaunchSli((SLItem) slvic[0].Tag);
                }
        }

        private void EH_SmartMatchClick(object sender, System.EventArgs e)
        {
            sCancelled = SCalcMatchingListViewItems(LvCur, m_ebRegEx.Text, sCancelled);
        }

        private void EH_HandleEdit(object sender, System.Windows.Forms.LabelEditEventArgs e)
        {
            SLItem sli = (SLItem) LvCur.Items[e.Item].Tag;

            if (!FRenameFile(sli.m_sPath, sli.m_sName, sli.m_sPath, e.Label))
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
            BuildMissingFileList();
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
                    m_ign.CreateIgnoreList(sName, m_cbxIgnoreList.SelectedIndex == 1);
                    m_cbxIgnoreList.Items.Add(sName);
                    m_cbxIgnoreList.SelectedIndex = m_cbxIgnoreList.Items.Count - 1;
                    }
                return;
                }
            ApplyIgnoreList((string) m_cbxIgnoreList.SelectedItem);
        }

        private void EH_DoSaveIgnoreList(object sender, EventArgs e)
        {
            m_ign.EnsureListSaved();
        }

        private void EH_LoadFileListFromFile(object sender, EventArgs e)
        {
            LoadFileListFromFile(SlisCur);
        }

        private void EH_SaveFileListToFile(object sender, EventArgs e)
        {
            SaveFileListToFile(SlisCur);
        }

        #endregion

        #region Generic Utilites

        public class PerfTimer
        {
            Stopwatch m_sw;
            private string m_sOp;

            public PerfTimer()
            {
                m_sw = new Stopwatch();
            }

            public void Start(string sOperation)
            {
                m_sOp = sOperation;
                m_sw.Start();
            }

            public void Stop()
            {
                m_sw.Stop();
            }

            public void Report(int msecMin = 0)
            {
                if (m_sw.ElapsedMilliseconds > msecMin)
                    MessageBox.Show(String.Format("{0} elapsed time: {1:0.00}", m_sOp, m_sw.ElapsedMilliseconds/1000.0));
            }
        }

        #endregion

        #region ListView Support

        /* C H A N G E  L I S T  V I E W  S O R T */
        /*----------------------------------------------------------------------------
        	%%Function: ChangeListViewSort
        	%%Qualified: SList.SListApp.ChangeListViewSort
        	%%Contact: rlittle
        	
            Change the sort order for the given listview to sort by the given column
        ----------------------------------------------------------------------------*/
        void ChangeListViewSort(ListView lv, int iColSort)
        {
            if (lv.ListViewItemSorter == null)
                lv.ListViewItemSorter = new ListViewItemComparer(iColSort);
            else
                ((ListViewItemComparer) lv.ListViewItemSorter).SetColumn(iColSort);

            lv.Sort();
        }

        void ToggleAllListViewItems(ListView lvCur)
        {
            int i, iMac;

            for (i = 0, iMac = LvCur.Items.Count; i < iMac; i++)
                {
                lvCur.Items[i].Checked = !lvCur.Items[i].Checked;
                }
        }

        void UncheckAllListViewItems(ListView lvCur)
        {
            int i, iMac;

            for (i = 0, iMac = lvCur.Items.Count; i < iMac; i++)
                {
                lvCur.Items[i].Checked = false;
                }
        }

        #endregion

        static int s_ilvSource = 0;
        static int s_ilvDest = 1;
        static int s_clvMax = 2;

        #region BuildFileList

        /* A D D  S L I  T O  L I S T  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: AddSliToListView
        	%%Qualified: SList.SListApp.AddSliToListView
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        static public void AddSliToListView(SLItem sli, ListView lv)
        {
            AddSliToListView(sli, lv, false);
        }

        static private void AddSliToListView(SLItem sli, ListView lv, bool fChecked)
        {
            ListViewItem lvi = new ListViewItem();

            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

            lvi.Tag = sli;
            lvi.SubItems[2].Text = sli.m_sPath;
            lvi.SubItems[1].Text = sli.m_lSize.ToString("###,###,###");
            lvi.SubItems[0].Text = sli.m_sName;

            if (fChecked)
                lvi.Checked = true;
            lv.Items.Add(lvi);
        }

        private void AddDirectory(DirectoryInfo di, SLISet slis, string sPattern, bool fRecurse, List<FileInfo> plfiTooLong)
        {
            FileInfo[] rgfi;
            int cchDir = di.FullName.Length;

            try
                {
                rgfi = di.GetFiles(sPattern);
                }
            catch
                {
                return;
                }

            int i, iMac;

            for (i = 0, iMac = rgfi.Length; i < iMac; i++)
                {
                bool fTooLong = false;
                try
                    {
                    if (rgfi[i].Name.Length + cchDir > 256)
                        fTooLong = true;
                    else
                        {
                        SLItem sli = new SLItem(rgfi[i].Name, rgfi[i].Length, rgfi[i].DirectoryName, rgfi[i]);
                        slis.Add(sli);
                        }
                    }
                catch (Exception exc)
                    {
                    fTooLong = true;
                    }
                if (fTooLong)
                    plfiTooLong.Add(rgfi[i]);

                // Application.DoEvents();
                }

            if (fRecurse)
                {
                DirectoryInfo[] rgdi;

                rgdi = di.GetDirectories();
                if (rgdi != null)
                    {
                    for (i = 0, iMac = rgdi.Length; i < iMac; i++)
                        {
                        AddDirectory(rgdi[i], slis, sPattern, fRecurse, plfiTooLong);
                        }
                    }
                }
        }

        /* B U I L D  F I L E  L I S T */
        /*----------------------------------------------------------------------------
        	%%Function: BuildFileList
        	%%Qualified: SList.SListApp.BuildFileList
        	%%Contact: rlittle
        	
            Take the search path and build the file list (for the selected target)
        ----------------------------------------------------------------------------*/
        private void BuildFileList()
        {
            string sFileSpec = m_ebSearchPath.Text;
            string sPath = null;
            string sPattern = null;
            FileAttributes fa = 0;
            bool fAttrsValid = false;
            PerfTimer pt = new PerfTimer();

            pt.Start("Search");

            // let's see what they gave us.  First, see if its a directory
            try
                {
                fa = File.GetAttributes(sFileSpec);
                fAttrsValid = true;
                }
            catch
                {
                fAttrsValid = false;
                }

            if (fAttrsValid && ((int) fa != -1) && (fa & FileAttributes.Directory) == FileAttributes.Directory)
                {
                // its a directory; use it
                sPath = sFileSpec;
                sPattern = "*";
                }
            else
                {
                sPath = Path.GetDirectoryName(sFileSpec);
                sPattern = Path.GetFileName(m_ebSearchPath.Text);

                if (sPattern == "")
                    sPattern = "*";
                }

            DirectoryInfo di = new DirectoryInfo(sPath);

            if (di == null)
                {
                MessageBox.Show("Path not found: " + sPath, "SList");
                return;
                }

            Cursor crsSav = this.Cursor;

            // start a wait cursor
            this.Cursor = Cursors.WaitCursor;

            // stop redrawing
            LvCur.BeginUpdate();

            // save off and reset the item sorter for faster adding
            IComparer lvicSav = LvCur.ListViewItemSorter;
            LvCur.ListViewItemSorter = null;

            LvCur.Items.Clear();

            List<FileInfo> plfiTooLong = new List<FileInfo>();

            AddDirectory(di, SlisCur, sPattern, m_cbRecurse.Checked, plfiTooLong);
            if (plfiTooLong.Count > 0)
                {
                MessageBox.Show(String.Format("Encountered {0} paths that were too long", plfiTooLong.Count));
                }

            pt.Stop();
            pt.Report();

            LvCur.EndUpdate();
            LvCur.ListViewItemSorter = lvicSav;
            LvCur.Update();
            this.Cursor = crsSav;
        }

        static Int64 FileSizeFromDirectoryLine(string sLine)
        {
            return Int64.Parse(sLine.Substring(20, 19),
                               NumberStyles.AllowThousands | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowParentheses);
        }

        static string FileNameFromDirectoryLine(string sLine)
        {
            return sLine.Substring(39).TrimEnd();
        }

        static void ParseFileListLine(string sLine, out string sPath, out string sFilename, out Int64 nSize)
        {
            int ich;
            int ichLast;

            ich = sLine.IndexOf('\t');
            sPath = sLine.Substring(0, ich);
            ichLast = ich + 1;
            ich = sLine.IndexOf('\t', ichLast);
            sFilename = sLine.Substring(ichLast, ich - ichLast);
            ichLast = ich + 1;
            nSize = Int64.Parse(sLine.Substring(ichLast));
        }
        #region tests

        [TestCase("04/05/2015  05:59 PM    13,704,581,120 hiberfil.sys", 13704581120)]
        [TestCase("04/05/2015  05:59 PM 1,113,704,581,120 hiberfil.sys", 1113704581120)]
        [Test]
        public static void TestFileSizeFromDirectoryLine(string sLine, Int64 nSizeExpected)
        {
            Int64 nSize = FileSizeFromDirectoryLine(sLine);
            Assert.AreEqual(nSizeExpected, nSize);
        }

        [TestCase("04/05/2015  05:59 PM    13,704,581,120 hiberfil.sys", "hiberfil.sys")]
        [TestCase("04/05/2015  05:59 PM 1,113,704,581,120 hiberfil.sys", "hiberfil.sys")]
        [Test]
        public static void TestFileNameFromDirectoryLine(string sLine, string sNameExpected)
        {
            string sFile = FileNameFromDirectoryLine(sLine);
            Assert.AreEqual(sNameExpected, sFile);
        }

        [TestCase("04/05/2015  05:59 PM    13,704,581,120 hiberfil.sys", "hiberfil.sys")]
        [TestCase("04/05/2015  05:59 PM 1,113,704,581,120 hiberfil.sys", "hiberfil.sys")]
        [Test]
        public static void TestParseFileListLine(string sLine, string sPathExpected, string sNameExpected, Int64 nSizeExpected)
        {
            string sPath, sName;
            Int64 nSize;

            ParseFileListLine(sLine, out sPath, out sName, out nSize);
            Assert.AreEqual(sPathExpected, sPath);
            Assert.AreEqual(nSizeExpected, nSize);
        }

        static string DirectoryNameFromDirectoryLine(string sLine)
        {
            return sLine.Substring(14);
        }

        [TestCase(" Directory of F:\\", "F:\\")]
        [TestCase(" Directory of F:\\$Recycle.Bin", "F:\\$Recycle.Bin")]
        [Test]
        public static void TestDirectoryNameFromDirectoryLine(string sLine, string sDirNameExpected)
        {
            string sDirName = DirectoryNameFromDirectoryLine(sLine);
            Assert.AreEqual(sDirNameExpected, sDirName);
        }

        #endregion // tests

        private void LoadFileListFromFile(SLISet slis)
        {
            string sFile;

            if (!InputBox.ShowInputBox("File list", out sFile))
                return;

            PerfTimer pt = new PerfTimer();
            pt.Start("load file list");
            // parse a directory listing and add 
            string sCurDirectory = null;
            TextReader tr = new StreamReader(new FileStream(sFile, FileMode.Open, FileAccess.Read), Encoding.Default);
            string sLine;
            slis.PauseListViewUpdate(true);

            sLine = tr.ReadLine();
            bool fInternalFormat = false;

            if (sLine == "[file.lst]")
                fInternalFormat = true;

            while ((sLine = tr.ReadLine()) != null)
                {
                if (fInternalFormat)
                    {
                    string sPath, sName;
                    Int64 nSize;

                    ParseFileListLine(sLine, out sPath, out sName, out nSize);
                    SLItem sli = new SLItem(sName, nSize, sPath, String.Concat(sPath, "/", sName));
                    slis.Add(sli);
                    continue;
                    }
                // figure out what this line is
                if (sLine.Length < 14)
                    continue;

                if (sLine[2] == '/' && sLine[5] == '/')
                    {
                    // this is a leading date, which means this is either a directory or a file
                    if (sLine[24] == '<') // this is a directory
                        continue;

                    // ok, from [14,39] is the size, [40, ...] is filename
                    Int64 nSize = FileSizeFromDirectoryLine(sLine);
                    string sFileLine = FileNameFromDirectoryLine(sLine);

                    SLItem sli = new SLItem(sFileLine, nSize, sCurDirectory, String.Concat(sCurDirectory, "/", sFileLine));
                    slis.Add(sli);
                    }
                else if (sLine.StartsWith(" Directory of "))
                    {
                    sCurDirectory = DirectoryNameFromDirectoryLine(sLine);
                    }
                }
            slis.ResumeListViewUpdate();

            pt.Stop();
            pt.Report();
            tr.Close();
        }

        private void SaveFileListToFile(SLISet slis)
        {
            string sFile;

            if (!InputBox.ShowInputBox("File list", out sFile))
                return;

            TextWriter tr = new StreamWriter(new FileStream(sFile, FileMode.CreateNew, FileAccess.Write), Encoding.Default);

            tr.WriteLine("[file.lst]"); // write something out so we know this is one of our files (we will parse it faster)
            foreach (ListViewItem lvi in slis.Lv.Items)
                {
                SLItem sli = (SLItem) lvi.Tag;
                tr.WriteLine("{0}\t{1}\t{2}", sli.m_sPath, sli.m_sName, sli.m_lSize);
                }
            tr.Flush();
            tr.Close();
        }

        #endregion

        #region Core Model (Compare Files, etc)

        private bool FCompareFiles(SLItem sli1, SLItem sli2, ref int min, ref int max, ref int sum)
        {
            int nStart = Environment.TickCount;
            int nEnd;

            FileStream bs1 = new FileStream(Path.Combine(sli1.m_sPath, sli1.m_sName), FileMode.Open, FileAccess.Read, FileShare.Read, 8, false);
            FileStream bs2 = new FileStream(Path.Combine(sli2.m_sPath, sli2.m_sName), FileMode.Open, FileAccess.Read, FileShare.Read, 8, false);

            int lcb = 16;

            long icb = 0;
            int i;
            int iProgress = 0;
            int iCurProgress = 0;
            bool fProgress = true;
            int iIncrement = (int) sli1.m_lSize/100;
            long lProgressLast = 0;

            if (iIncrement == 0)
                iIncrement = 1;

            if (sli1.m_lSize < 10000)
                fProgress = false;

            if (icb + lcb >= sli1.m_lSize)
                lcb = (int) (sli1.m_lSize - icb);

            m_stbpMainStatus.Text = sli1.m_sName;
            if (fProgress)
                {
                m_prbar.Value = iProgress;
                m_prbar.Show();
                }


            while (lcb > 0)
                {
                // Application.DoEvents();
                if (fProgress)
                    {
                    if (lProgressLast + iIncrement < icb)
                        {
                        iCurProgress = (int) (icb/iIncrement);
                        m_prbar.Value = Math.Min(iCurProgress, 100);
                        iProgress = iCurProgress;
                        lProgressLast = iIncrement*iProgress;
                        }
                    }

                bs1.Read(m_rgb1, 0, lcb);
                bs2.Read(m_rgb2, 0, lcb);

                icb += lcb;
                i = 0;
                while (i < lcb)
                    {
                    if (m_rgb1[i] != m_rgb2[i])
                        {
//					br1.Close();
//					br2.Close();
                        bs1.Close();
                        bs2.Close();

                        m_prbar.Value = 100;
                        nEnd = Environment.TickCount;

                        if ((nEnd - nStart) < min)
                            min = nEnd - nStart;

                        if ((nEnd - nStart) > max)
                            max = (nEnd - nStart);

                        sum += (nEnd - nStart);
                        return false;
                        }
                    i++;
                    }

                if (lcb < lcbMax)
                    {
                    if ((int) (sli1.m_lSize - icb - 1) == 0)
                        break;

                    lcb *= 2;
                    if (lcb > lcbMax)
                        lcb = lcbMax;
                    }

                if (icb + lcb >= sli1.m_lSize)
                    lcb = (int) (sli1.m_lSize - icb - 1);

                }
//		br1.Close();
//		br2.Close();
            bs1.Close();
            bs2.Close();
            m_prbar.Value = 100;
            nEnd = Environment.TickCount;

            if ((nEnd - nStart) < min)
                min = nEnd - nStart;

            if ((nEnd - nStart) > max)
                max = (nEnd - nStart);

            sum += (nEnd - nStart);
            return true;

        }

        void AddSlisToRgsli(SLISet slis, SLItem[] rgsli, int iFirst, bool fDestOnly)
        {
            int i, iMac;

            for (i = 0, iMac = slis.Lv.Items.Count; i < iMac; i++)
                {
                rgsli[iFirst + i] = (SLItem) slis.Lv.Items[i].Tag;
                rgsli[iFirst + i].ClearDupeChain();
                rgsli[iFirst + i].m_fMarked = false;
                rgsli[iFirst + i].DestOnly = fDestOnly;
                }
        }

        /* E  H  _ F I N D  D U P L I C A T E S */
        /*----------------------------------------------------------------------------
		%%Function: EH_Uniquify
		%%Qualified: SList.SListApp.EH_Uniquify
		%%Contact: rlittle

	    ----------------------------------------------------------------------------*/
        private void BuildUniqueFileList()
        {
            int start, end, sum = 0;
            int min = 999999, max = 0, c = 0;
            SLISet slisSrc = m_rgslis[s_ilvSource];
            int cItems = slisSrc.Lv.Items.Count + m_rgslis[s_ilvDest].Lv.Items.Count;
            SLItem[] rgsli = new SLItem[cItems];

            start = Environment.TickCount;

            AddSlisToRgsli(slisSrc, rgsli, 0, false);

            if (m_rgslis[s_ilvDest].Lv.Items.Count > 0)
                {
                AddSlisToRgsli(m_rgslis[s_ilvDest], rgsli, slisSrc.Lv.Items.Count, true);
                }
            Array.Sort(rgsli, new SLItemComparer(SLItem.SLItemCompare.CompareSize));

            slisSrc.Lv.BeginUpdate();
            slisSrc.Lv.Items.Clear();

            int i = 0;
            int iMac = rgsli.Length;

            int iIncrement = Math.Max(1, iMac/1000);
            int iLast = 0;

            Cursor crsSav = this.Cursor;

            // start a wait cursor
            this.Cursor = Cursors.WaitCursor;
            m_prbarOverall.Show();
            for (; i < iMac; i++)
                {
                int iDupe, iDupeMac;

                if (iLast + iIncrement < i)
                    {
                    m_prbarOverall.Value = Math.Min(1000, (int) (i/iIncrement));
                    iLast = m_prbarOverall.Value*iIncrement;
                    }

                if (rgsli[i].m_fMarked)
                    continue;

                if (rgsli[i].DestOnly)
                    continue;

                // search forward for dupes
                for (iDupe = i + 1, iDupeMac = rgsli.Length; iDupe < iDupeMac; iDupe++)
                    {
                    if (rgsli[iDupe].m_fMarked == true)
                        continue;

                    if (rgsli[i].m_lSize == rgsli[iDupe].m_lSize)
                        {
                        // do more extensive check here...for now, the size and the name is enough
                        if (m_cbCompareFiles.Checked)
                            {
                            c++;
                            if (FCompareFiles(rgsli[i], rgsli[iDupe], ref min, ref max, ref sum))
                                {
                                if (rgsli[i].m_fMarked == false)
                                    AddSliToListView(rgsli[i], slisSrc.Lv, true);

                                if (rgsli[iDupe].m_fMarked == false)
                                    AddSliToListView(rgsli[iDupe], slisSrc.Lv);

                                rgsli[i].m_fMarked = rgsli[iDupe].m_fMarked = true;
                                rgsli[i].AddDupeToChain(rgsli[iDupe]);
                                }
                            }
                        else
                            {
                            if (rgsli[i].m_sName == rgsli[iDupe].m_sName)
                                {
                                if (rgsli[i].m_fMarked == false)
                                    AddSliToListView(rgsli[i], slisSrc.Lv);

                                if (rgsli[iDupe].m_fMarked == false)
                                    AddSliToListView(rgsli[iDupe], slisSrc.Lv);

                                rgsli[i].m_fMarked = rgsli[iDupe].m_fMarked = true;
                                rgsli[i].AddDupeToChain(rgsli[iDupe]);
                                }
                            }
                        }
                    else
                        {
                        if (rgsli[i].m_fMarked == false)
                            // this was unique...
                            AddSliToListView(rgsli[i], slisSrc.Lv, true);

                        break; // no reason to continue if the lengths changed; we sorted by length
                        }
                    }
                }
            m_prbar.Hide();
            m_prbarOverall.Hide();
            if (m_cbCompareFiles.Checked)
                m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by file compare.";
            else
                m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by size and name.";

            slisSrc.Lv.EndUpdate();
            this.Cursor = crsSav;
            end = Environment.TickCount;

            int len = end - start;
            if (c == 0)
                c = 1;

            int avg = len/c;
            int avg2 = sum/c;
            m_stbpSearch.Text = len.ToString() + "ms, (" + min.ToString() + ", " + max.ToString() + ", " + avg.ToString() + ", " + avg2.ToString() + ", " + c.ToString() + ")";
        }

        /* A D J U S T  L I S T  V I E W  F O R  F A V O R E D  P A T H S */
        /*----------------------------------------------------------------------------
		    %%Function: AdjustListViewForFavoredPaths
		    %%Qualified: SList.SListApp.AdjustListViewForFavoredPaths
		    %%Contact: rlittle

		    Kinda like FindDuplicates, but it doesn't search for them.  It just looks
		    for dupe chains, and then favors marking/unmark items that match the paths
		    in the preferred paths list (uses m_cbMarkFavored)
	    ----------------------------------------------------------------------------*/
        void AdjustListViewForFavoredPaths()
        {
            foreach (ListViewItem lvi in LvCur.Items)
                {
                SLItem sli = (SLItem) lvi.Tag;

                foreach (String s in m_lbPrefPath.Items)
                    {
                    if (sli.MatchesPrefPath(s))
                        {
                        UpdateForPrefPath(sli, s, m_cbMarkFavored.Checked);
                        break;
                        }
                    }
                }
        }

        static void MoveSelectedFiles(ListView lvCur, string sDir, StatusBarPanel stbp)
        {
            FileAttributes fa = 0;
            bool fDirExists = false;
            // let's see what they gave us.  First, see if its a directory
            try
                {
                fa = File.GetAttributes(sDir);
                if ((fa & FileAttributes.Directory) == FileAttributes.Directory)
                    fDirExists = true;
                else
                    {
                    MessageBox.Show(sDir + " exists, but is not a directory.  Please choose a different location", "SList");
                    return;
                    }
                }
            catch
                {
                fDirExists = false;
                }

            if (fDirExists == false)
                {
                try
                    {
                    DirectoryInfo di = Directory.CreateDirectory(sDir);
                    sDir = di.FullName;
                    }
                catch
                    {
                    MessageBox.Show("Cannot create directory " + sDir + ".  Please choose a different location", "SList");
                    return;
                    }
                }

            // if we got here, then sDir exists
            if (MessageBox.Show("Move selected files to " + sDir + "?", "SList", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            // ok, iterate through all the items and find the ones that are checked
            int i, iMac;

            for (i = 0, iMac = lvCur.Items.Count; i < iMac; i++)
                {
                if (!lvCur.Items[i].Checked)
                    continue;

                SLItem sli = (SLItem) (lvCur.Items[i].Tag);
                string sSource = Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName));
                string sDest = Path.GetFullPath(Path.Combine(sDir, sli.m_sName));

                if (String.Compare(sSource, sDest, true /*ignoreCase*/) == 0)
                    {
                    stbp.Text = "Skipped identity move: " + sSource;
                    continue;
                    }

                // now, see if sDest already exists.  if it does, we need to try
                // to rename the file
                int n = 0;
                string sDestClone = sDest + "";

                while (File.Exists(sDestClone) && n < 1020)
                    {
                    sDestClone = Path.Combine(Path.GetDirectoryName(sDest), Path.GetFileNameWithoutExtension(sDest) + "(" + n.ToString() + ")" + Path.GetExtension(sDest));
                    //				sDestClone = sDest + " (" + n.ToString() + ")";
                    n++;
                    }

                if (n >= 1020)
                    {
                    MessageBox.Show("Cannot move " + sSource + " to new location " + sDestClone + ".  There are too many duplicates in the destination.", "SList");
                    continue;
                    }

                // ok, let's do the move
                stbp.Text = "Moving " + sSource + " -> " + sDestClone;
                File.Move(sSource, sDestClone);
                lvCur.Items[i].Checked = false;
                }
        }

        static bool FRenameFile(string sPathOrig, string sFileOrig, string sPathNew, string sFileNew)
        {
            if (sFileNew == null)
                return false;

            string sSource = Path.GetFullPath(Path.Combine(sPathOrig, sFileOrig));
            string sDest = Path.GetFullPath(Path.Combine(sPathNew, sFileNew));

            try
                {
                File.Move(sSource, sDest);
                }
            catch (Exception ex)
                {
                MessageBox.Show("Cannot rename '" + sFileOrig + "' to '" + sFileNew + "':\n\n" + ex.ToString(), "SList");
                return false;
                }

            return true;
        }

        void ApplyIgnoreList(string sIgnoreList)
        {
            SLISet slis = SlisCur;

            int colSav = ((ListViewItemComparer) slis.Lv.ListViewItemSorter).GetColumn();
            ((ListViewItemComparer) slis.Lv.ListViewItemSorter).SetColumn(-1);
            slis.Lv.Sort();

            // otherwise, we're loading a new list
            m_ign.LoadIgnoreList(sIgnoreList);
            int iProgress = 0;
            m_prbarOverall.Value = iProgress;
            m_prbarOverall.Show();

            // and apply the ignore list
            Application.DoEvents();
            int iMac = m_ign.IgnoreItems.Count;

            slis.PauseListViewUpdate(false);
            for (int i = 0; i < iMac; i++)
                {
                if (iProgress != (1000*i)/iMac)
                    {
                    iProgress = (1000*i)/iMac;
                    m_prbarOverall.Value = iProgress;
                    m_prbarOverall.Update();
                    Application.DoEvents();
                    }
                RemovePath(slis, m_ign.IgnoreItems[i].PathPrefix);
                }
            m_prbarOverall.Hide();
            Application.DoEvents();
            slis.ResumeListViewUpdate(colSav);
        }

        public enum RegexOp
        {
            Match,
            Filter,
            Check
        };

        private void DoRegex(RegexOp rop)
        {
            Regex rx = null;

            try
                {
                rx = new Regex(m_ebRegEx.Text);
                }
            catch (Exception e)
                {
                MessageBox.Show("Could not compile Regular Expression '" + m_ebRegEx.Text + "':\n" + e.ToString(), "SLList");
                return;
                }


            int i, iMac;

            for (i = 0, iMac = LvCur.Items.Count; i < iMac; i++)
                {
                SLItem sli = (SLItem) (LvCur.Items[i].Tag);
                string sPath = Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName));
                bool fMatch = false;

                fMatch = rx.IsMatch(sPath);

                switch (rop)
                    {
                    case RegexOp.Check:
                        if (fMatch)
                            LvCur.Items[i].Checked = true;
                        break;
                    case RegexOp.Filter:
                        if (fMatch)
                            {
                            LvCur.Items[i].Remove();
                            iMac--;
                            i--;
                            }
                        break;
                    case RegexOp.Match:
                        if (!fMatch)
                            {
                            LvCur.Items[i].Remove();
                            iMac--;
                            i--;
                            }
                        break;
                    }
                }
        }

        string sCancelled;

        static string SCalcMatchingListViewItems(ListView lvCur, string sRegEx, string sCounts)
        {
            ATMC atmc = new ATMC(sRegEx);
            string sMatch = String.Format("Matches for '{0}':\n\n", sRegEx);

            int i, iMac;
            int cMatch = 0;

            for (i = 0, iMac = lvCur.Items.Count; i < iMac; i++)
                {
                SLItem sli = (SLItem) (lvCur.Items[i].Tag);

                if (sli.m_atmc == null)
                    sli.m_atmc = new ATMC(sli.m_sName);

                int nMatch = 0;
                nMatch = sli.m_atmc.NMatch(atmc);
                if (nMatch > 65)
                    {
                    sMatch += String.Format("{0:d3}% : '{1}'\n", nMatch, Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName)), sRegEx);
                    cMatch++;
                    }
                }
            if (cMatch == 0 || MessageBox.Show(sMatch, "Matches", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                sCounts += String.Format("{0}\n", sRegEx);

            return sCounts;
        }


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

        void AddPreferredPath(string s)
        {
            m_lbPrefPath.Items.Add(s);
            AdjustListViewForFavoredPaths();
        }

        void RemovePath(SLISet slis, string sPathRoot)
        {
            slis.Remove(sPathRoot, m_prbar);
        }

        void EH_RemovePath(object sender, EventArgs e)
        {
            MenuItem mni = (MenuItem) sender;
            RemovePath(m_rgslis[m_islisCur], mni.Text);
            if (m_cbAddToIgnoreList.Checked)
                {
                m_ign.AddIgnorePath(mni.Text);
                }
        }

        private void EH_AddPreferredPath(object sender, EventArgs e)
        {
            MenuItem mni = (MenuItem) sender;
            AddPreferredPath(mni.Text);
        }

        #endregion // Core Model (Compare Files, etc)

        #region List View Commands

        void LaunchSli(SLItem sli)
        {
            Process.Start(Path.Combine(sli.m_sPath, sli.m_sName));
        }

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

            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
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
        private void HandleDragLeave(object sender, System.EventArgs e) {}

        private void EH_SelectPrevDupe(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

            if (slvic != null && slvic.Count >= 1)
                {
                SLItem sli = (SLItem) slvic[0].Tag;

                SLItem sliSel = sli.Prev;
                Select(sliSel);
                }
        }

        private void EH_SelectNextDupe(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

            if (slvic != null && slvic.Count >= 1)
                {
                SLItem sli = (SLItem) slvic[0].Tag;

                SLItem sliSel = sli.Next;
                Select(sliSel);
                }
        }

        #endregion // List View Commands

        private void EH_DoContextPopup(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection slvic = LvCur.SelectedItems;

            if (slvic != null && slvic.Count >= 1)
                {
                SLItem sli = (SLItem) slvic[0].Tag;

                ContextMenu cm = (ContextMenu) sender;

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

        public void UpdateForPrefPath(SLItem sliMaster, string s, bool fMark)
        {
            SLItem sli;

            sliMaster.m_fMarked = fMark;
            UpdateMark(sliMaster);

            sli = sliMaster;

            while ((sli = sli.Prev) != null)
                {
                if (sli.MatchesPrefPath(s))
                    sli.m_fMarked = fMark;
                else
                    sli.m_fMarked = !fMark;

                UpdateMark(sli);
                }

            sli = sliMaster;

            while ((sli = sli.Next) != null)
                {
                if (sli.MatchesPrefPath(s))
                    sli.m_fMarked = fMark;
                else
                    sli.m_fMarked = !fMark;
                UpdateMark(sli);
                }
        }

        void UpdateMark(SLItem sli)
        {
            ListViewItem lvi = LviFromSli(sli);

            lvi.Checked = sli.m_fMarked;
        }

        ListViewItem LviFromSli(SLItem sli)
        {
            foreach (ListViewItem lvi in LvCur.Items)
                {
                if (lvi.Tag == sli)
                    return lvi;
                }
            return null;
        }

        void Select(SLItem sli)
        {
            if (sli == null)
                {
                SystemSounds.Beep.Play();
                return;
                }
            ListViewItem lvi = LviFromSli(sli);

            if (lvi != null)
                {
                lvi.Selected = true;
                LvCur.Select();
                return;
                }
            SystemSounds.Beep.Play();
        }

        private void DoSearchTargetChange(object sender, EventArgs e)
        {
            ShowListView(m_cbxSearchTarget.SelectedIndex);
        }

        #region SLI Bucket

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

        #endregion

        private SLISet[] m_rgslis;

        // we might not be at the beginning of the dupe list for this item -- we might
        // have skipped over some DestOnly items, and those might be the dupes we
        // are looking for
        int FindFirstDupeCandidate(SLItem[] rgsli, int iCurrent)
        {
            // walk backwards until we change sizes or hit the beginning
            int i = iCurrent - 1;

            while (i >= 0 && rgsli[i].m_lSize == rgsli[iCurrent].m_lSize)
                i--;

            // we break on the first item that doesn't match...return
            // the next item
            return i + 1;
        }

        void BuildMissingFileList()
        {
            SLItem[] rgsli;
            int start, end, sum = 0;
            int min = 999999, max = 0, c = 0;
            SLISet slisSrc = m_rgslis[s_ilvSource];

            start = Environment.TickCount;

            int cItems = slisSrc.Lv.Items.Count + m_rgslis[s_ilvDest].Lv.Items.Count;

            rgsli = new SLItem[cItems];

            AddSlisToRgsli(slisSrc, rgsli, 0, false);

            if (m_rgslis[s_ilvDest].Lv.Items.Count > 0)
                {
                AddSlisToRgsli(m_rgslis[s_ilvDest], rgsli, slisSrc.Lv.Items.Count, true);
                }
            Array.Sort(rgsli, new SLItemComparer(SLItem.SLItemCompare.CompareSizeDest));

            slisSrc.Lv.BeginUpdate();
            slisSrc.Lv.Items.Clear();

            int i = 0;
            int iMac = rgsli.Length;

            int iIncrement = Math.Max(1, iMac/1000);
            int iLast = 0;

            Cursor crsSav = this.Cursor;

            // start a wait cursor
            this.Cursor = Cursors.WaitCursor;
            m_prbarOverall.Show();
            for (; i < iMac; i++)
                {
                int iDupe, iDupeMac;

                if (iLast + iIncrement < i)
                    {
                    m_prbarOverall.Value = Math.Min(1000, (int) (i/iIncrement));
                    iLast = m_prbarOverall.Value*iIncrement;
                    }

                if (rgsli[i].m_fMarked)
                    continue;

                if (rgsli[i].DestOnly)
                    continue;

                iDupe = FindFirstDupeCandidate(rgsli, i);

                // search forward for dupes
                for (iDupeMac = rgsli.Length; iDupe < iDupeMac; iDupe++)
                {
                    // don't compare against ourself
                    if (iDupe == i)
                        continue;

                    // we are explicitly looking ONLY at fDestOnly files to see if there's a dupe
                    // (used to include rgsli[iDupe].m_fMarked == true  -- but why exclude
                    // destonly files that were already duped against? a destonly file can be 
                    // a dupe for multiple source files...
                    if (rgsli[iDupe].m_fDestOnly == false)
                        continue;

                    if (rgsli[i].m_lSize == rgsli[iDupe].m_lSize)
                        {
                        // do more extensive check here...for now, the size and the name is enough
                        if (m_cbCompareFiles.Checked)
                            {
                            c++;
                            if (FCompareFiles(rgsli[i], rgsli[iDupe], ref min, ref max, ref sum))
                                {
                                // we found a dupe in the target. yay, don't add it anywhere
                                rgsli[i].m_fMarked = rgsli[iDupe].m_fMarked = true;
                                rgsli[iDupe].AddDupeToChain(rgsli[i]);
                                break;
                                }
                            }
                        else
                            {
                            if (rgsli[i].m_sName == rgsli[iDupe].m_sName)
                                {
                                // we found a dupe in the target.. nothing to add
                                rgsli[i].m_fMarked = true; //  rgsli[iDupe].m_fMarked = true; // don't mark the dupe
                                // rgsli[i].AddDupeToChain(rgsli[iDupe]); // don't add to the dupe chain
                                break;
                                }
                            else
                                {
                                break; // no sense continuing if the name changed -- we sorted by size by name, and we aren't doing a deep compare, so name mismatch means we'll never match.
                                }
                            }
                        }
                    else
                        {
                        break; // no reason to continue if the lengths changed; we sorted by length
                        }
                    }
                // we have left the loop.  either we broke out because we know we don't have a match,
                // or we exhausted all the dupes and we know we found at least one match.
                // in either case, if we found a dupe in the target, we will have marked m_fMarked to be true...
                // if its not set, then we didn't find this file in the destination.
                if (rgsli[i].m_fMarked == false)
                    // this was unique...
                    AddSliToListView(rgsli[i], slisSrc.Lv, true);


                }
            m_prbar.Hide();
            m_prbarOverall.Hide();
            if (m_cbCompareFiles.Checked)
                m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by file compare.";
            else
                m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by size and name.";

            slisSrc.Lv.EndUpdate();
            this.Cursor = crsSav;
            end = Environment.TickCount;

            int len = end - start;
            if (c == 0)
                c = 1;

            int avg = len/c;
            int avg2 = sum/c;
            m_stbpSearch.Text = len.ToString() + "ms, (" + min.ToString() + ", " + max.ToString() + ", " + avg.ToString() + ", " + avg2.ToString() + ", " + c.ToString() + ")";
        }

    }

    #region ListViewItem Comparer
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
            ListViewItem lvi1 = (ListViewItem) x;
            ListViewItem lvi2 = (ListViewItem) y;
            SLItem sli1 = (SLItem) lvi1.Tag;
            SLItem sli2 = (SLItem) lvi2.Tag;
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
                    n = SLItem.Compare(sli1, sli2, SLItem.SLItemCompare.CompareSize, m_fReverse);
                    break;
                case 2:
                    n = SLItem.Compare(sli1, sli2, SLItem.SLItemCompare.ComparePath, m_fReverse);
                    break;
                }

            return n;
        }
    }
    #endregion
}

