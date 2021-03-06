using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Media;
using System.Resources;

namespace SList
{
/// <summary>
/// Summary description for MainForm.
/// </summary>
public class MainForm : System.Windows.Forms.Form
{
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

	private byte []m_rgb1;
	private byte []m_rgb2;
	
	public const int lcbMax = 4 * 1024 * 1024;
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
	private System.ComponentModel.IContainer components;

	public MainForm()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();

		m_rgb1 = new byte[lcbMax];
		m_rgb2 = new byte[lcbMax];
		InitializeListView();
		//
		// TODO: Add any constructor code after InitializeComponent call
		//
	}

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			if (components != null) 
			{
				components.Dispose();
			}
		}
		base.Dispose( disposing );
	}

	#region Windows Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	/* I N I T I A L I Z E  C O M P O N E N T */
	/*----------------------------------------------------------------------------
		%%Function: InitializeComponent
		%%Qualified: SList.MainForm.InitializeComponent
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void InitializeComponent()
	{
        this.components = new System.ComponentModel.Container();
        this.m_lv = new System.Windows.Forms.ListView();
        this.m_cxtListView = new System.Windows.Forms.ContextMenu();
        this.menuItem1 = new System.Windows.Forms.MenuItem();
        this.menuItem2 = new System.Windows.Forms.MenuItem();
        this.menuItem3 = new System.Windows.Forms.MenuItem();
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
        this.menuItem4 = new System.Windows.Forms.MenuItem();
        this.menuItem5 = new System.Windows.Forms.MenuItem();
        ((System.ComponentModel.ISupportInitialize)(this.m_stbpMainStatus)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.m_stbpFilterStatus)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.m_stbpSearch)).BeginInit();
        this.SuspendLayout();
        // 
        // m_lv
        // 
        this.m_lv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.m_lv.CheckBoxes = true;
        this.m_lv.ContextMenu = this.m_cxtListView;
        this.m_lv.Location = new System.Drawing.Point(16, 280);
        this.m_lv.Name = "m_lv";
        this.m_lv.Size = new System.Drawing.Size(592, 496);
        this.m_lv.TabIndex = 20;
        this.m_lv.UseCompatibleStateImageBehavior = false;
        this.m_lv.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.EH_HandleEdit);
        // 
        // m_cxtListView
        // 
        this.m_cxtListView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
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
        // m_ebSearchPath
        // 
        this.m_ebSearchPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.m_ebSearchPath.Location = new System.Drawing.Point(107, 36);
        this.m_ebSearchPath.Name = "m_ebSearchPath";
        this.m_ebSearchPath.Size = new System.Drawing.Size(344, 20);
        this.m_ebSearchPath.TabIndex = 2;
        this.m_ebSearchPath.Text = "c:\\temp";
        // 
        // m_pbSearch
        // 
        this.m_pbSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.m_pbSearch.Location = new System.Drawing.Point(536, 32);
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
        this.m_cbRecurse.Location = new System.Drawing.Point(456, 39);
        this.m_cbRecurse.Name = "m_cbRecurse";
        this.m_cbRecurse.Size = new System.Drawing.Size(72, 16);
        this.m_cbRecurse.TabIndex = 3;
        this.m_cbRecurse.Text = "Recurse";
        // 
        // m_pbDuplicates
        // 
        this.m_pbDuplicates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.m_pbDuplicates.Location = new System.Drawing.Point(536, 168);
        this.m_pbDuplicates.Name = "m_pbDuplicates";
        this.m_pbDuplicates.Size = new System.Drawing.Size(72, 24);
        this.m_pbDuplicates.TabIndex = 9;
        this.m_pbDuplicates.Text = "Duplicates";
        this.m_pbDuplicates.Click += new System.EventHandler(this.EH_FindDuplicates);
        // 
        // m_lblFilterBanner
        // 
        this.m_lblFilterBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.m_lblFilterBanner.Location = new System.Drawing.Point(8, 152);
        this.m_lblFilterBanner.Name = "m_lblFilterBanner";
        this.m_lblFilterBanner.Size = new System.Drawing.Size(608, 16);
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
        this.m_lblSearchCriteria.Size = new System.Drawing.Size(608, 16);
        this.m_lblSearchCriteria.TabIndex = 0;
        this.m_lblSearchCriteria.Tag = "Perform search";
        this.m_lblSearchCriteria.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
        // 
        // m_cbCompareFiles
        // 
        this.m_cbCompareFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.m_cbCompareFiles.Location = new System.Drawing.Point(384, 176);
        this.m_cbCompareFiles.Name = "m_cbCompareFiles";
        this.m_cbCompareFiles.Size = new System.Drawing.Size(152, 16);
        this.m_cbCompareFiles.TabIndex = 8;
        this.m_cbCompareFiles.Text = "Real Duplicate Checking";
        // 
        // m_stb
        // 
        this.m_stb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.m_stb.Dock = System.Windows.Forms.DockStyle.None;
        this.m_stb.Location = new System.Drawing.Point(0, 784);
        this.m_stb.Name = "m_stb";
        this.m_stb.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.m_stbpMainStatus,
            this.m_stbpFilterStatus,
            this.m_stbpSearch});
        this.m_stb.ShowPanels = true;
        this.m_stb.Size = new System.Drawing.Size(624, 24);
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
        this.m_prbar.Location = new System.Drawing.Point(112, 788);
        this.m_prbar.Name = "m_prbar";
        this.m_prbar.Size = new System.Drawing.Size(190, 15);
        this.m_prbar.TabIndex = 10;
        this.m_prbar.Visible = false;
        // 
        // m_lblActions
        // 
        this.m_lblActions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.m_lblActions.Location = new System.Drawing.Point(8, 232);
        this.m_lblActions.Name = "m_lblActions";
        this.m_lblActions.Size = new System.Drawing.Size(608, 16);
        this.m_lblActions.TabIndex = 15;
        this.m_lblActions.Tag = "Perform actions";
        this.m_lblActions.Paint += new System.Windows.Forms.PaintEventHandler(this.EH_RenderHeadingLine);
        // 
        // m_ebRegEx
        // 
        this.m_ebRegEx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.m_ebRegEx.Location = new System.Drawing.Point(96, 172);
        this.m_ebRegEx.Name = "m_ebRegEx";
        this.m_ebRegEx.Size = new System.Drawing.Size(272, 20);
        this.m_ebRegEx.TabIndex = 7;
        // 
        // m_lblRegEx
        // 
        this.m_lblRegEx.Location = new System.Drawing.Point(16, 176);
        this.m_lblRegEx.Name = "m_lblRegEx";
        this.m_lblRegEx.Size = new System.Drawing.Size(72, 40);
        this.m_lblRegEx.TabIndex = 6;
        this.m_lblRegEx.Text = "Regular Expressions";
        // 
        // m_pbMove
        // 
        this.m_pbMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.m_pbMove.Location = new System.Drawing.Point(456, 248);
        this.m_pbMove.Name = "m_pbMove";
        this.m_pbMove.Size = new System.Drawing.Size(72, 24);
        this.m_pbMove.TabIndex = 18;
        this.m_pbMove.Text = "Move";
        this.m_pbMove.Click += new System.EventHandler(this.EH_DoMove);
        // 
        // m_pbDelete
        // 
        this.m_pbDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.m_pbDelete.Location = new System.Drawing.Point(536, 248);
        this.m_pbDelete.Name = "m_pbDelete";
        this.m_pbDelete.Size = new System.Drawing.Size(72, 24);
        this.m_pbDelete.TabIndex = 19;
        this.m_pbDelete.Text = "Delete";
        this.m_pbDelete.Click += new System.EventHandler(this.EH_DoDelete);
        // 
        // m_pbToggle
        // 
        this.m_pbToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.m_pbToggle.Location = new System.Drawing.Point(536, 200);
        this.m_pbToggle.Name = "m_pbToggle";
        this.m_pbToggle.Size = new System.Drawing.Size(72, 24);
        this.m_pbToggle.TabIndex = 14;
        this.m_pbToggle.Text = "Toggle All";
        this.m_pbToggle.Click += new System.EventHandler(this.EH_ToggleAll);
        // 
        // m_pbClear
        // 
        this.m_pbClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.m_pbClear.Location = new System.Drawing.Point(456, 200);
        this.m_pbClear.Name = "m_pbClear";
        this.m_pbClear.Size = new System.Drawing.Size(72, 24);
        this.m_pbClear.TabIndex = 13;
        this.m_pbClear.Text = "Clear All";
        this.m_pbClear.Click += new System.EventHandler(this.EH_ClearAll);
        // 
        // m_lblMoveTo
        // 
        this.m_lblMoveTo.Location = new System.Drawing.Point(16, 256);
        this.m_lblMoveTo.Name = "m_lblMoveTo";
        this.m_lblMoveTo.Size = new System.Drawing.Size(56, 16);
        this.m_lblMoveTo.TabIndex = 16;
        this.m_lblMoveTo.Text = "Move to";
        // 
        // m_ebMovePath
        // 
        this.m_ebMovePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.m_ebMovePath.Location = new System.Drawing.Point(96, 252);
        this.m_ebMovePath.Name = "m_ebMovePath";
        this.m_ebMovePath.Size = new System.Drawing.Size(344, 20);
        this.m_ebMovePath.TabIndex = 17;
        // 
        // m_pbMatchRegex
        // 
        this.m_pbMatchRegex.Location = new System.Drawing.Point(96, 200);
        this.m_pbMatchRegex.Name = "m_pbMatchRegex";
        this.m_pbMatchRegex.Size = new System.Drawing.Size(80, 24);
        this.m_pbMatchRegex.TabIndex = 10;
        this.m_pbMatchRegex.Text = "Match Regex";
        this.m_pbMatchRegex.Click += new System.EventHandler(this.EH_MatchRegex);
        // 
        // m_pbRemoveRegex
        // 
        this.m_pbRemoveRegex.Location = new System.Drawing.Point(176, 200);
        this.m_pbRemoveRegex.Name = "m_pbRemoveRegex";
        this.m_pbRemoveRegex.Size = new System.Drawing.Size(80, 24);
        this.m_pbRemoveRegex.TabIndex = 11;
        this.m_pbRemoveRegex.Text = "Filter Regex";
        this.m_pbRemoveRegex.Click += new System.EventHandler(this.EH_FilterRegex);
        // 
        // m_pbCheckRegex
        // 
        this.m_pbCheckRegex.Location = new System.Drawing.Point(256, 200);
        this.m_pbCheckRegex.Name = "m_pbCheckRegex";
        this.m_pbCheckRegex.Size = new System.Drawing.Size(80, 24);
        this.m_pbCheckRegex.TabIndex = 12;
        this.m_pbCheckRegex.Text = "Check Regex";
        this.m_pbCheckRegex.Click += new System.EventHandler(this.EH_CheckRegex);
        // 
        // m_prbarOverall
        // 
        this.m_prbarOverall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.m_prbarOverall.Location = new System.Drawing.Point(305, 788);
        this.m_prbarOverall.Maximum = 1000;
        this.m_prbarOverall.Name = "m_prbarOverall";
        this.m_prbarOverall.Size = new System.Drawing.Size(190, 15);
        this.m_prbarOverall.TabIndex = 23;
        this.m_prbarOverall.Visible = false;
        // 
        // m_pbSmartMatch
        // 
        this.m_pbSmartMatch.Location = new System.Drawing.Point(336, 200);
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
        this.m_lbPrefPath.Location = new System.Drawing.Point(107, 62);
        this.m_lbPrefPath.Name = "m_lbPrefPath";
        this.m_lbPrefPath.Size = new System.Drawing.Size(413, 69);
        this.m_lbPrefPath.TabIndex = 25;
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(16, 62);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(85, 17);
        this.label1.TabIndex = 26;
        this.label1.Text = "Preferred Paths";
        // 
        // m_pbRemove
        // 
        this.m_pbRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.m_pbRemove.Location = new System.Drawing.Point(536, 107);
        this.m_pbRemove.Name = "m_pbRemove";
        this.m_pbRemove.Size = new System.Drawing.Size(72, 24);
        this.m_pbRemove.TabIndex = 27;
        this.m_pbRemove.Text = "Remove";
        // 
        // m_pbAddPath
        // 
        this.m_pbAddPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.m_pbAddPath.Location = new System.Drawing.Point(536, 78);
        this.m_pbAddPath.Name = "m_pbAddPath";
        this.m_pbAddPath.Size = new System.Drawing.Size(72, 24);
        this.m_pbAddPath.TabIndex = 28;
        this.m_pbAddPath.Text = "Add Path";
        // 
        // m_cbMarkFavored
        // 
        this.m_cbMarkFavored.AutoSize = true;
        this.m_cbMarkFavored.Location = new System.Drawing.Point(9, 107);
        this.m_cbMarkFavored.Name = "m_cbMarkFavored";
        this.m_cbMarkFavored.Size = new System.Drawing.Size(92, 17);
        this.m_cbMarkFavored.TabIndex = 29;
        this.m_cbMarkFavored.Text = "Mark Favored";
        this.m_cbMarkFavored.UseVisualStyleBackColor = true;
        // 
        // menuItem4
        // 
        this.menuItem4.Index = 2;
        this.menuItem4.Text = "Select previous duplicate";
        this.menuItem4.Click += new System.EventHandler(this.EH_SelectPrevDupe);
        // 
        // menuItem5
        // 
        this.menuItem5.Index = 3;
        this.menuItem5.Text = "Select next duplicate";
        this.menuItem5.Click += new System.EventHandler(this.EH_SelectNextDupe);
        // 
        // MainForm
        // 
        this.AllowDrop = true;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(624, 806);
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
        this.Name = "MainForm";
        this.Text = "MainForm";
        this.DragLeave += new System.EventHandler(this.HandleDragLeave);
        this.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleDrop);
        this.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleDragEnter);
        ((System.ComponentModel.ISupportInitialize)(this.m_stbpMainStatus)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.m_stbpFilterStatus)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.m_stbpSearch)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

	}
	#endregion

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new MainForm());
	}

	private void InitializeListView()
	{
		m_lv.Columns.Add(new ColumnHeader());
		m_lv.Columns[0].Text = "    Name";
		m_lv.Columns[0].Width = 146;
		
		m_lv.Columns.Add(new ColumnHeader());
		m_lv.Columns[1].Text = "Size";
		m_lv.Columns[1].Width = 52;
		m_lv.Columns[1].TextAlign = HorizontalAlignment.Right;

		m_lv.Columns.Add(new ColumnHeader());
		m_lv.Columns[2].Text = "Location";
		m_lv.Columns[2].Width = 128;

		m_lv.FullRowSelect = true;
		m_lv.MultiSelect = false;
		m_lv.View = View.Details;
		m_lv.ListViewItemSorter = new ListViewItemComparer(1);
		m_lv.ColumnClick += new ColumnClickEventHandler(EH_ColumnClick);
		m_lv.LabelEdit = true;
	}

	/* E  H  _ C O L U M N  C L I C K */
	/*----------------------------------------------------------------------------
		%%Function: EH_ColumnClick
		%%Qualified: SList.MainForm.EH_ColumnClick
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void EH_ColumnClick(object o, ColumnClickEventArgs e)
	{
		if (((ListView)o).ListViewItemSorter == null)
			((ListView)o).ListViewItemSorter = new ListViewItemComparer(e.Column);
		else
			((ListViewItemComparer)(((ListView)o).ListViewItemSorter)).SetColumn(e.Column);

		((ListView)o).Sort();
	}

	private void AddSliToListView(SLItem sli, ListView lv)
	{
		AddSliToListView(sli, lv, false);
	}

	private void AddSliToListView(SLItem sli, ListView lv, bool fChecked)
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

	private void AddDirectory(DirectoryInfo di, ListView lv, string sPattern, bool fRecurse)
	{
		FileInfo []rgfi;

		try
		{
			rgfi = di.GetFiles(sPattern);
		} catch
		{
			return;
		}
		
		int i, iMac;

		for (i = 0, iMac = rgfi.Length; i < iMac; i++)
			{
			SLItem sli = new SLItem(rgfi[i].Name, rgfi[i].Length, rgfi[i].DirectoryName, rgfi[i]);

			Application.DoEvents();
			AddSliToListView(sli, lv);
			}

		if (fRecurse)
			{
			DirectoryInfo []rgdi;

			rgdi = di.GetDirectories();
			if (rgdi != null)
				{
				for (i = 0, iMac = rgdi.Length; i < iMac; i++)
					{
					AddDirectory(rgdi[i], lv, sPattern, fRecurse);
					}
				}
			}
	}

	private void EH_DoSearch(object sender, System.EventArgs e)
	{
		string sFileSpec = m_ebSearchPath.Text;
		string sPath = null;
		string sPattern = null;
		FileAttributes fa = 0;
		bool fAttrsValid = false;

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

		if (fAttrsValid && ((int)fa != -1) && (fa & FileAttributes.Directory) == FileAttributes.Directory)
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
		m_lv.BeginUpdate();

		// save off and reset the item sorter for faster adding
		IComparer lvicSav = m_lv.ListViewItemSorter;
		m_lv.ListViewItemSorter = null;

		m_lv.Items.Clear();

		AddDirectory(di, m_lv, sPattern, m_cbRecurse.Checked);

		m_lv.EndUpdate();
		m_lv.ListViewItemSorter = lvicSav;
		m_lv.Update();
		this.Cursor = crsSav;
	}

	private void EH_RenderHeadingLine(object sender, System.Windows.Forms.PaintEventArgs e)
	{
		Label lbl = (Label)sender;
		string s = (string)lbl.Tag;

		SizeF sf = e.Graphics.MeasureString(s, lbl.Font);
		int nWidth = (int)sf.Width;
		int nHeight = (int)sf.Height;

		e.Graphics.DrawString(s, lbl.Font, new SolidBrush(Color.SlateBlue), 0, 0);// new System.Drawing.Point(0, (lbl.Width - nWidth) / 2));
		e.Graphics.DrawLine(new Pen(new SolidBrush(Color.Gray), 1), 6 + nWidth + 1, (nHeight / 2), lbl.Width, (nHeight / 2));

	}

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
		int iIncrement = (int)sli1.m_lSize / 100;
		long lProgressLast = 0;

		if (iIncrement == 0)
			iIncrement = 1;

		if (sli1.m_lSize < 10000)
			fProgress = false;

		if (icb + lcb >= sli1.m_lSize)
			lcb = (int)(sli1.m_lSize - icb);

		m_stbpMainStatus.Text = sli1.m_sName;
		if (fProgress)
			{
			m_prbar.Value = iProgress;
			m_prbar.Show();
			}


		while (lcb > 0)
			{
			Application.DoEvents();
			if (fProgress)
				{
				if (lProgressLast + iIncrement < icb)
					{
					iCurProgress = (int)(icb / iIncrement);
					m_prbar.Value = Math.Min(iCurProgress, 100);
					iProgress = iCurProgress;
					lProgressLast = iIncrement * iProgress;
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
				if ((int)(sli1.m_lSize - icb - 1) == 0)
					break;

				lcb *= 2;
				if (lcb > lcbMax)
					lcb = lcbMax;
				}

			if (icb + lcb >= sli1.m_lSize)
				lcb = (int)(sli1.m_lSize - icb - 1);
			
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

	/* U P D A T E  S E A R C H */
	/*----------------------------------------------------------------------------
		%%Function: UpdateSearch
		%%Qualified: SList.MainForm.UpdateSearch
		%%Contact: rlittle

		Kinda like FindDuplicates, but it doesn't search for them.  It just looks
		for dupe chains, and then favors marking/unmark items that match the paths
		in the preferred paths list (uses m_cbMarkFavored)
	----------------------------------------------------------------------------*/
	void UpdateSearch()
	{
		foreach (ListViewItem lvi in m_lv.Items)
			{
			SLItem sli = (SLItem)lvi.Tag;

			foreach(String s in m_lbPrefPath.Items)
				{
				if (sli.MatchesPrefPath(s))
					{
					UpdateForPrefPath(sli, s, m_cbMarkFavored.Checked);
					break;
					}
				}
			}
	}

	/* E  H  _ F I N D  D U P L I C A T E S */
	/*----------------------------------------------------------------------------
		%%Function: EH_FindDuplicates
		%%Qualified: SList.MainForm.EH_FindDuplicates
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void EH_FindDuplicates(object sender, System.EventArgs e)
	{
		SLItem []rgsli = new SLItem[m_lv.Items.Count];
		int start, end, sum = 0;
		int min = 999999, max = 0, c = 0;
	
		start = Environment.TickCount;

		int i, iMac;

		for (i = 0, iMac = m_lv.Items.Count; i < iMac; i++)
			{
			rgsli[i] = (SLItem)m_lv.Items[i].Tag;
			rgsli[i].ClearDupeChain();
			rgsli[i].m_fMarked = false;
			}

		Array.Sort(rgsli, new SLItemComparer(SLItem.SLItemCompare.CompareSize));

		m_lv.BeginUpdate();
		m_lv.Items.Clear();

		i = 0;
		iMac = rgsli.Length; 

		int iIncrement = Math.Max(1, iMac / 1000);
		int iLast = 0;

		Cursor crsSav = this.Cursor;

		// start a wait cursor
		this.Cursor = Cursors.WaitCursor;
		m_prbarOverall.Show();
		for ( ; i < iMac; i++)
			{
			int iDupe, iDupeMac;

			if (iLast + iIncrement < i)
				{
				m_prbarOverall.Value = Math.Min(1000, (int)(i / iIncrement));
				iLast = m_prbarOverall.Value * iIncrement;
				}

			if (rgsli[i].m_fMarked)
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
								AddSliToListView(rgsli[i], m_lv, true);
								
							if (rgsli[iDupe].m_fMarked == false)
								AddSliToListView(rgsli[iDupe], m_lv);
							
							rgsli[i].m_fMarked = rgsli[iDupe].m_fMarked = true;
							rgsli[i].AddDupeToChain(rgsli[iDupe]);
							}
						}
					else
						{
						if (rgsli[i].m_sName == rgsli[iDupe].m_sName)
							{
							if (rgsli[i].m_fMarked == false)
								AddSliToListView(rgsli[i], m_lv);
							
							if (rgsli[iDupe].m_fMarked == false)
								AddSliToListView(rgsli[iDupe], m_lv);
							
							rgsli[i].m_fMarked = rgsli[iDupe].m_fMarked = true;
							rgsli[i].AddDupeToChain(rgsli[iDupe]);
							}
						}
					}
				else
					break; // no reason to continue if the lengths changed; we sorted by length
				}
			}
		m_prbar.Hide();
		m_prbarOverall.Hide();
		if (m_cbCompareFiles.Checked)
			m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by file compare.";
		else
			m_stbpMainStatus.Text = "Search complete.  Duplicates filtered by size and name.";

		m_lv.EndUpdate();
		this.Cursor = crsSav;
		end = Environment.TickCount;

		int len = end - start;
		if (c == 0)
			c = 1;

		int avg = len / c;
		int avg2 = sum / c;
		m_stbpSearch.Text = len.ToString() + "ms, (" + min.ToString() + ", " + max.ToString() + ", " + avg.ToString() + ", " + avg2.ToString() + ", " + c.ToString() + ")";
	}

	/* E  H  _ D O  M O V E */
	/*----------------------------------------------------------------------------
		%%Function: EH_DoMove
		%%Qualified: SList.MainForm.EH_DoMove
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void EH_DoMove(object sender, System.EventArgs e)
	{
		string sDir = m_ebMovePath.Text;
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

		for (i = 0, iMac = m_lv.Items.Count; i < iMac; i++)
			{
			if (!m_lv.Items[i].Checked)
				continue;

			SLItem sli = (SLItem)(m_lv.Items[i].Tag);
			string sSource = Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName));
			string sDest = Path.GetFullPath(Path.Combine(sDir, sli.m_sName));

			if (String.Compare(sSource, sDest, true/*ignoreCase*/) == 0)
				{
				m_stbpMainStatus.Text = "Skipped identity move: " + sSource;
				continue;
				}
			
			// now, see if sDest already exists.  if it does, we need to try
			// to rename the file
			int n = 0;
			string sDestClone = sDest + "";

			while (File.Exists(sDestClone) && n < 1020)
				{
				sDestClone = Path.Combine(Path.GetDirectoryName(sDest), Path.GetFileNameWithoutExtension(sDest) + "("+n.ToString()+")" + Path.GetExtension(sDest));
//				sDestClone = sDest + " (" + n.ToString() + ")";
				n++;
				}

			if (n >= 1020)
				{
				MessageBox.Show("Cannot move " + sSource + " to new location " + sDestClone + ".  There are too many duplicates in the destination.", "SList");
				continue;
				}

			// ok, let's do the move
			m_stbpMainStatus.Text = "Moving "+sSource+" -> "+sDestClone;
			File.Move(sSource, sDestClone);
			m_lv.Items[i].Checked = false;
			}
	}

	private void EH_DoDelete(object sender, System.EventArgs e)
	{
	
	}

	private void EH_ToggleAll(object sender, System.EventArgs e)
	{
		int i, iMac;

		for (i = 0, iMac = m_lv.Items.Count; i < iMac; i++)
			{
			m_lv.Items[i].Checked = !m_lv.Items[i].Checked;
			}
	
	}

	private void EH_ClearAll(object sender, System.EventArgs e)
	{
		int i, iMac;

		for (i = 0, iMac = m_lv.Items.Count; i < iMac; i++)
			{
			m_lv.Items[i].Checked = false;
			}
	
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
			MessageBox.Show("Could not compile Regular Expression '" + m_ebRegEx.Text + "':\n"+ e.ToString(), "SLList");
			return;
			}


		int i, iMac;

		for (i = 0, iMac = m_lv.Items.Count; i < iMac; i++)
			{
			SLItem sli = (SLItem)(m_lv.Items[i].Tag);
			string sPath = Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName));
			bool fMatch = false;

			fMatch = rx.IsMatch(sPath);
			
			switch (rop)
				{
				case RegexOp.Check:
					if (fMatch)
						m_lv.Items[i].Checked = true;
					break;
				case RegexOp.Filter:
					if (fMatch)
						{
						m_lv.Items[i].Remove();
						iMac--;
						i--;
						}
					break;
				case RegexOp.Match:
					if (!fMatch)
						{
						m_lv.Items[i].Remove();
						iMac--;
						i--;
						}
					break;
				}
			}
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

	string sCancelled;

	private void EH_SmartMatchClick(object sender, System.EventArgs e) 
	{
		ATMC atmc = new ATMC(m_ebRegEx.Text);
		string sMatch = String.Format("Matches for '{0}':\n\n", m_ebRegEx.Text);

		int i, iMac;
		int cMatch = 0;

		for (i = 0, iMac = m_lv.Items.Count; i < iMac; i++)
			{
			SLItem sli = (SLItem)(m_lv.Items[i].Tag);

			if (sli.m_atmc == null)
				sli.m_atmc = new ATMC(sli.m_sName);

			int nMatch = 0;
			nMatch = sli.m_atmc.NMatch(atmc);
			if (nMatch > 65)
				{
				sMatch += String.Format("{0:d3}% : '{1}'\n", nMatch, Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName)), m_ebRegEx.Text);
				cMatch++;
				}
			}
		if (cMatch == 0 || MessageBox.Show(sMatch, "Matches", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
			sCancelled += String.Format("{0}\n", m_ebRegEx.Text);
	}

	private void EH_HandleExecuteMenu(object sender, System.EventArgs e)
	{
		ListView.SelectedListViewItemCollection slvic = m_lv.SelectedItems;

		if (slvic != null && slvic.Count >= 1)
			{
            SLItem sli = (SLItem)slvic[0].Tag;
            
			Process.Start(Path.Combine(sli.m_sPath, sli.m_sName));
			}

	}

	private void EH_HandleEdit(object sender, System.Windows.Forms.LabelEditEventArgs e)
	{
		if (e.Label == null)
			return;

		SLItem sli = (SLItem)m_lv.Items[e.Item].Tag;

		string sSource = Path.GetFullPath(Path.Combine(sli.m_sPath, sli.m_sName));
		string sDest = Path.GetFullPath(Path.Combine(sli.m_sPath, e.Label));

		try
			{
			File.Move(sSource, sDest);
			}
		catch (Exception ex)
			{
			MessageBox.Show("Cannot rename '" + sli.m_sName + "' to '" + e.Label + "':\n\n" + ex.ToString(), "SList");
			e.CancelEdit = true;
			}

		if (e.CancelEdit != true)
			sli.m_sName = e.Label;

		// let's try to rename it as they have asked

	}

	/* H A N D L E  D R O P */
	/*----------------------------------------------------------------------------
		%%Function: HandleDrop
		%%Qualified: SList.MainForm.HandleDrop
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void HandleDrop(object sender, System.Windows.Forms.DragEventArgs e)
	{
		this.Activate();

		m_tmr.Enabled = false;

		string []files = (string[])e.Data.GetData(DataFormats.FileDrop);
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
		%%Qualified: SList.MainForm.HandleDragEnter
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
		%%Qualified: SList.MainForm.HandleDragLeave
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void HandleDragLeave(object sender, System.EventArgs e)
	{

	}

	/* E  H  _ I D L E */
	/*----------------------------------------------------------------------------
		%%Function: EH_Idle
		%%Qualified: SList.MainForm.EH_Idle
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
		UpdateSearch();
    }

    private void EH_AddPreferredPath(object sender, EventArgs e)
    {
        MenuItem mni = (MenuItem)sender;
        AddPreferredPath(mni.Text);
    }

    private void EH_DoContextPopup(object sender, EventArgs e)
    {
		ListView.SelectedListViewItemCollection slvic = m_lv.SelectedItems;

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
            foreach(string s in rgs)
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
		foreach(ListViewItem lvi in m_lv.Items)
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
			m_lv.Select();
			return;
			}
		SystemSounds.Beep.Play();
	}



    private void EH_SelectPrevDupe(object sender, EventArgs e)
    {
		ListView.SelectedListViewItemCollection slvic = m_lv.SelectedItems;

		if (slvic != null && slvic.Count >= 1)
			{
			SLItem sli = (SLItem)slvic[0].Tag;

			SLItem sliSel = sli.Prev;
			Select(sliSel);
			}
    }

    private void EH_SelectNextDupe(object sender, EventArgs e)
    {
		ListView.SelectedListViewItemCollection slvic = m_lv.SelectedItems;

		if (slvic != null && slvic.Count >= 1)
			{
			SLItem sli = (SLItem)slvic[0].Tag;

			SLItem sliSel = sli.Next;
			Select(sliSel);
			}
    }


}

public enum ATMT
{
	TitleWord,
	OtherWord,
	Separator
};

public class ATM
{
	ATMT m_atmt;
	string m_sWord;
	char m_chSep;

	public ATMT Atmt { get { return m_atmt; } set { m_atmt = value; } }

	public ATM(ATMT atmt, string sWord)
	{
		// strip out ' and "
		int ich = 0, ichNext = 0, ichCur = 0, ichMax = sWord.Length;
		int ichL = sWord.IndexOf('\'');
		int ichQ = sWord.IndexOf('"');
		m_sWord = "";
		while (ichCur < ichMax)
			{
			if (ichQ < ichCur)
				ichQ = sWord.IndexOf('"', ichCur);
			if (ichL < ichCur)
				ichL = sWord.IndexOf('\'', ichCur);

			if (ichQ == -1)
				ichNext = ichL;
			else if (ichL == -1)
				ichNext = ichQ;
			else
				ichNext = Math.Min(ichQ, ichL);

			if (ichNext == -1)
				break;

			m_sWord += sWord.Substring(ich, ichNext - ich);
			ich = ichNext + 1;
			ichCur = ichNext + 1;
			}

			// figure out the next segment
		m_sWord += sWord.Substring(ich);

		m_atmt = atmt;
		m_chSep = '\0';
	}

	public ATM(ATMT atmt, char chSep)
	{
		m_sWord = null;
		m_atmt = atmt;
		m_chSep = chSep;
	}

	public bool FMatch(ATM atm)
	{
		if (m_atmt != atm.m_atmt)
			return false;

		if (m_atmt == ATMT.Separator)
			return true;	// separators match, even if they're not the same

		return String.Compare(m_sWord, atm.m_sWord, true) == 0;
	}
}

public class ATMC // ATM Collection
{
	ArrayList m_platm;
	int m_cWords;

	public int CountWords 
		{ get { if (m_cWords == -1) return (m_cWords = CTitleWordsInPlatm(m_platm)); else return m_cWords; } }


	public int Count { get { return m_platm.Count; } }

	public ATMC(string sName)
	{
		m_cWords = -1;
		m_platm = PlatmBuildFromString(sName);
	}

	public ATM this[int i]
	{
		get { return (ATM)m_platm[i]; }
		set { m_platm[i] = value; }
	}

	/* P L A T M  B U I L D  F R O M  S T R I N G */
	/*----------------------------------------------------------------------------
		%%Function: PlatmBuildFromString
		%%Qualified: SList.MainForm:ATMC.PlatmBuildFromString
		%%Contact: rlittle

		Build platm from the given sName
	----------------------------------------------------------------------------*/
	ArrayList PlatmBuildFromString(string sName)
	{
		ArrayList platm = new ArrayList();
		int ich = 0;
		int ichFirst = -1;
		int ichMax;
		ATMT atmtCur = ATMT.TitleWord;
		bool fCollecting = false;
		bool fEndWord = false;

		if ((ich = sName.LastIndexOf('.')) != -1)
			sName = sName.Substring(0, ich);

		ich = 0;
		ichMax = sName.Length;

		while (ich < ichMax)
			{
			if (fEndWord)
				{
				// end the word and add it as atmtCur
				ATM atm = new ATM(atmtCur, sName.Substring(ichFirst, ich - ichFirst));
				platm.Add(atm);
				fEndWord = false;
				fCollecting = false;
				ichFirst = -1;
				}

			// ok, classify the character
			char ch = sName[ich];

			if (Char.IsWhiteSpace(ch) && fCollecting)
				{
				fEndWord = true;
				continue;	// this effectively pushes the token back on the stack
				}

			while(ich < ichMax && Char.IsWhiteSpace(sName[ich]))
				ich++;

			if (ich >= ichMax)
				break;

			ch = sName[ich];

			switch (ch)
				{
				case '(':
				case '[':
				case '{':
					if (fCollecting)
						{
						fEndWord = true;
						continue;
						}
					atmtCur = ATMT.OtherWord;
					break;
				case ')':
				case ']':
				case '}':
					if (fCollecting)
						{
						fEndWord = true;
						continue;
						}
					atmtCur = ATMT.TitleWord;	// we don't handle nested parens, we always go back to the title
					break;
				case '-':
				case '=':
				case ':':
					if (fCollecting)
						{
						fEndWord = true;
						continue;
						}
					// all collected words before this now become OtherWord
					foreach (ATM atm in platm)
						{
						if (atm.Atmt == ATMT.TitleWord)
							atm.Atmt = ATMT.OtherWord;
						}
					{
					ATM atm = new ATM(ATMT.Separator, ch);
					platm.Add(atm);
					}
					break;
				case '\'':
				case '"':
				default:
					if (!fCollecting && (ch == '\'' || ch == '"' ))
						break;	// skip if we're not collecting

					// the rest start collecting
					if (!fCollecting)
						{
						ichFirst = ich;
						fCollecting = true;
						}
					break;
				}
			ich++;
			}
		if (fCollecting)
			{
			ATM atm = new ATM(atmtCur, sName.Substring(ichFirst, ich - ichFirst));
			platm.Add(atm);
			}

		// before we're done, make sure there are *some* title words
		if (CTitleWordsInPlatm(platm) == 0)
			{
			// work backwards and make title words until we hit a separator
			int i = platm.Count - 1;

			while (i >= 0)
				{
				// skip trailing seps
				while (i >= 0 && ((ATM)platm[i]).Atmt == ATMT.Separator)
					i--;

				if (i < 0)
					break;

				while (i >= 0 && ((ATM)platm[i]).Atmt != ATMT.Separator)
					((ATM)platm[i--]).Atmt = ATMT.TitleWord;

				break;
				}
			}
		return platm;
	}


	/* C  T I T L E  W O R D S  I N  P L A T M */
	/*----------------------------------------------------------------------------
		%%Function: CTitleWordsInPlatm
		%%Qualified: SList.MainForm.CTitleWordsInPlatm
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	int CTitleWordsInPlatm(ArrayList platm)
	{
		int c = 0;

		foreach(ATM atm in platm)
			{
			if (atm.Atmt == ATMT.TitleWord)
				c++;
			}
		return c;
	}

	/* N  M A T C H  P L A T M */
	/*----------------------------------------------------------------------------
		%%Function: NMatchPlatm
		%%Qualified: SList.SLItem.NMatchPlatm
		%%Contact: rlittle

		try to match the two platms.  Return the confidence (0-100%)

		(confidence is the number of matched words / the number of words in platm1)
	----------------------------------------------------------------------------*/
	public int NMatch(ATMC atmc)
	{
		int i1 = this.Count - 1, i2 = atmc.Count - 1;
		int cMatch = 0;

		if (i1 < 0 || i2 < 0)
			return cMatch;

		while (i1 >= 0 && i2 >= 0)
			{
			// find the first titleword
			while (i2 >= 0 && atmc[i2].Atmt != ATMT.TitleWord)
				i2--;

			if (i2 < 0)
				break;

			while (i1 >= 0 && this[i1].Atmt != ATMT.TitleWord)
				i1--;

			if (i1 < 0)
				break;

			if (!this[i1].FMatch(atmc[i2]))
				break;

			cMatch++;
			i1--;
			i2--;
			}

		return (cMatch * 100) / this.CountWords;
	}
}

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

	/* C O M P A R E */
	/*----------------------------------------------------------------------------
		%%Function: Compare
		%%Qualified: SList.ListViewItemComparer.Compare
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public int Compare(object x, object y)
	{
		ListViewItem lvi1 = (ListViewItem)x;
		ListViewItem lvi2 = (ListViewItem)y;
		SLItem sli1 = (SLItem)lvi1.Tag;
		SLItem sli2 = (SLItem)lvi2.Tag;
		int n = 0;

		switch (m_col)
			{
#if NEVER
		case 0:
			if (lvi1.Checked)
				{
				if (lvi2.Checked)
					n = 0;
				else
					n = -1;
				}
			else if (!lvi2.Checked)
				n = 0;
			else
				n = 1;

			// manually handle fReverse here -- SLItem.Compare handles it for us elsewhere
			if (m_fReverse)
				n = -n;

			if (n == 0)
				n = SLItem.Compare(sli1, sli2, SLItem.SLItemCompare.CompareName, m_fReverse);
			break;
#endif

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

public class SLItemComparer : IComparer
{
	private SLItem.SLItemCompare m_slic;

	/* S  L  I T E M  C O M P A R E R */
	/*----------------------------------------------------------------------------
		%%Function: SLItemComparer
		%%Qualified: SList.SLItemComparer.SLItemComparer
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public SLItemComparer()
	{
		m_slic = SLItem.SLItemCompare.CompareName;
	}
	
	/* S  L  I T E M  C O M P A R E R */
	/*----------------------------------------------------------------------------
		%%Function: SLItemComparer
		%%Qualified: SList.SLItemComparer.SLItemComparer
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public SLItemComparer(SLItem.SLItemCompare slic)
	{
		m_slic = slic;
	}

	/* C O M P A R E */
	/*----------------------------------------------------------------------------
		%%Function: Compare
		%%Qualified: SList.ListViewItemComparer.Compare
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public int Compare(object x, object y)
	{
		SLItem sli1 = (SLItem)x;
		SLItem sli2 = (SLItem)y;

		return SLItem.Compare(sli1, sli2, m_slic, false/*fReverse*/);
	}
}

public class SLItem
{
	public string m_sName;
	public long m_lSize;
	public string m_sPath;
	public bool m_fMarked;

	public FileInfo m_fi;
	public DirectoryInfo m_di;
	public ATMC m_atmc;

	SLItem m_sliPrev;
	SLItem m_sliNext;

	public SLItem Prev { get { return m_sliPrev; } }
	public SLItem Next { get { return m_sliNext; } }

	public enum SLItemCompare
	{
		CompareName,
		CompareSize,
		ComparePath
	}

	public SLItem(string sName, long lSize, string sPath, DirectoryInfo di)
	{
		m_sName = sName;
		m_lSize = lSize;
		m_sPath = sPath;
		m_di = di;
		m_fi = null;
		m_atmc = null;
	}

	public SLItem(string sName, long lSize, string sPath, FileInfo fi)
	{
		m_sName = sName;
		m_lSize = lSize;
		m_sPath = sPath;
		m_fi = fi;
		m_di = null;
		m_atmc = null;
	}

	CultureInfo ci = new CultureInfo("en-US");

	public void ClearDupeChain()
	{
		m_sliNext = m_sliPrev = null;
	}

	public bool MatchesPrefPath(string s)
	{
		if (m_sPath.Length < s.Length)
			return false;

		if (m_sPath.StartsWith(s, true/*ignoreCase*/, ci))
			return true;

		return false;
	}

	public void AddDupeToChain(SLItem sli)
	{
		if (sli.m_sliNext != null || sli.m_sliPrev != null)
			throw new Exception("Can't add an sli that already has dupes!");

		SLItem sliLast = this;

		while (sliLast.m_sliNext != null)
			sliLast = sliLast.m_sliNext;

		sliLast.m_sliNext = sli;
		sli.m_sliPrev = sliLast;
	}


	static public int Compare(SLItem sli1, SLItem sli2, SLItemCompare slic, bool fReverse)
	{
		int n = 0;

		switch (slic)
			{
		case SLItemCompare.CompareName:
			n = String.Compare(sli1.m_sName, sli2.m_sName);
			if (n == 0)
				{
				// they are again equivalent; the difference is now file size
				n = (int)(sli1.m_lSize - sli2.m_lSize);

				if (n == 0)
					{
					// yeesh.  diff is now folder

					n = String.Compare(sli1.m_sPath, sli2.m_sPath);
					}
				}
			break;
		case SLItemCompare.CompareSize:
			n = (int)(sli1.m_lSize - sli2.m_lSize);

			if (n == 0)
				{
				// they are the same; now look at the name
				n = String.Compare(sli1.m_sName, sli2.m_sName);

				if (n == 0)
					{
					// yeesh.  diff is now folder

					n = String.Compare(sli1.m_sPath, sli2.m_sPath);
					}
				}
			break;
		case SLItemCompare.ComparePath:
			n = String.Compare(sli1.m_sPath, sli2.m_sPath);

			if (n == 0)
				{
				// they are equivalent; the difference is now based on the name
				n = String.Compare(sli1.m_sName, sli2.m_sName);

				if (n == 0)
					{
					// they are again equivalent; the difference is now file size
					n = (int)(sli1.m_lSize - sli2.m_lSize);
					}
				}
			break;
			}

		if (fReverse)
			return -n;
		else
			return n;
	}

};


}

