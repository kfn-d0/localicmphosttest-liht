namespace FakeHostLocalLab.UI;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.DataGridView dgvHosts;
    private System.Windows.Forms.DataGridView dgvPorts;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.Button btnStartStop;
    private System.Windows.Forms.Button btnCleanup;



    private System.Windows.Forms.Label lblHosts;
    private System.Windows.Forms.Label lblPorts;
    private System.Windows.Forms.Label lblLog;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.dgvHosts = new System.Windows.Forms.DataGridView();
        this.dgvPorts = new System.Windows.Forms.DataGridView();
        this.txtLog = new System.Windows.Forms.TextBox();
        this.btnStartStop = new System.Windows.Forms.Button();
        this.btnCleanup = new System.Windows.Forms.Button();



        this.lblHosts = new System.Windows.Forms.Label();
        this.lblPorts = new System.Windows.Forms.Label();
        this.lblLog = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPorts)).BeginInit();
        this.SuspendLayout();


        ((System.ComponentModel.ISupportInitialize)(this.dgvHosts)).BeginInit();
        this.dgvHosts.Location = new System.Drawing.Point(12, 30);
        this.dgvHosts.Size = new System.Drawing.Size(430, 400);
        this.dgvHosts.Columns.Add("Status", "");
        this.dgvHosts.Columns.Add("Name", "Host Name");
        this.dgvHosts.Columns.Add("Ip", "IP Address");
        var activeCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
        activeCol.Name = "Enabled";
        activeCol.HeaderText = "Active";
        this.dgvHosts.Columns.Add(activeCol);
        
        var copyCol = new System.Windows.Forms.DataGridViewButtonColumn();
        copyCol.Name = "Copy";
        copyCol.HeaderText = "";
        copyCol.Text = "Copy";
        copyCol.UseColumnTextForButtonValue = true;
        this.dgvHosts.Columns.Add(copyCol);

        this.dgvHosts.AllowUserToAddRows = false;
        this.dgvHosts.RowHeadersVisible = false;
        this.dgvHosts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvHosts.MultiSelect = false;
        this.dgvHosts.Columns[0].Width = 30;
        this.dgvHosts.Columns[1].Width = 100;
        this.dgvHosts.Columns[2].Width = 100;
        this.dgvHosts.Columns[3].Width = 50;
        this.dgvHosts.Columns[4].Width = 60;
        this.dgvHosts.SelectionChanged += new System.EventHandler(this.dgvHosts_SelectionChanged);
        this.dgvHosts.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvHosts_CellContentClick);



        this.dgvPorts.Location = new System.Drawing.Point(460, 30);
        this.dgvPorts.Size = new System.Drawing.Size(470, 250);
        this.dgvPorts.Columns.Add("Proto", "Proto");
        this.dgvPorts.Columns.Add("Port", "Port");
        this.dgvPorts.Columns.Add("Mode", "Mode");
        this.dgvPorts.Columns.Add("Response", "Response");
        this.dgvPorts.ReadOnly = true;



        this.txtLog.Location = new System.Drawing.Point(460, 310);
        this.txtLog.Size = new System.Drawing.Size(470, 120);
        this.txtLog.Multiline = true;
        this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtLog.WordWrap = true;
        this.txtLog.ReadOnly = true;


        this.btnStartStop.Location = new System.Drawing.Point(12, 440);
        this.btnStartStop.Size = new System.Drawing.Size(120, 30);
        this.btnStartStop.Text = "Start Engine";
        this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);

        this.btnCleanup.Location = new System.Drawing.Point(142, 440);
        this.btnCleanup.Size = new System.Drawing.Size(140, 30);
        this.btnCleanup.Text = "Cleanup Network";
        this.btnCleanup.Click += new System.EventHandler(this.btnCleanup_Click);








        this.lblHosts.Text = "Hosts:";
        this.lblHosts.Location = new System.Drawing.Point(12, 10);
        this.lblPorts.Text = "Port Rules (Selected Host):";
        this.lblPorts.Location = new System.Drawing.Point(460, 10);
        this.lblPorts.Size = new System.Drawing.Size(200, 20);
        this.lblLog.Text = "Errors & Critical Events:";
        this.lblLog.Location = new System.Drawing.Point(460, 290);


        this.ClientSize = new System.Drawing.Size(950, 480);
        this.Controls.Add(this.dgvHosts);
        this.Controls.Add(this.dgvPorts);
        this.Controls.Add(this.txtLog);
        this.Controls.Add(this.btnStartStop);
        this.Controls.Add(this.btnCleanup);



        this.Controls.Add(this.lblHosts);
        this.Controls.Add(this.lblPorts);
        this.Controls.Add(this.lblLog);
        this.Text = "Local ICMP Host Test - LIHT";
        ((System.ComponentModel.ISupportInitialize)(this.dgvPorts)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvHosts)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}



