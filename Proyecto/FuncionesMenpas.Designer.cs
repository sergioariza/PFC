namespace Proyecto
{
    partial class FuncionesMenpas
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FuncionesMenpas));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.funcionesGrid = new System.Windows.Forms.DataGridView();
            this.usuarioDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.funcionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fechaDeAltaDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.funcionesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.estimaciondeFuncionesDataSet = new Proyecto.EstimaciondeFuncionesDataSet();
            this.botonAceptar = new System.Windows.Forms.Button();
            this.funcionesTableAdapter = new Proyecto.EstimaciondeFuncionesDataSetTableAdapters.FuncionesTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.funcionesGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.funcionesBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.estimaciondeFuncionesDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.funcionesGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.botonAceptar);
            // 
            // funcionesGrid
            // 
            this.funcionesGrid.AllowUserToAddRows = false;
            this.funcionesGrid.AllowUserToDeleteRows = false;
            this.funcionesGrid.AutoGenerateColumns = false;
            this.funcionesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.funcionesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.usuarioDataGridViewTextBoxColumn,
            this.funcionDataGridViewTextBoxColumn,
            this.fechaDeAltaDataGridViewTextBoxColumn});
            this.funcionesGrid.DataSource = this.funcionesBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.NullValue = null;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.funcionesGrid.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.funcionesGrid, "funcionesGrid");
            this.funcionesGrid.Name = "funcionesGrid";
            this.funcionesGrid.ReadOnly = true;
            this.funcionesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.funcionesGrid.ShowCellErrors = false;
            this.funcionesGrid.ShowRowErrors = false;
            // 
            // usuarioDataGridViewTextBoxColumn
            // 
            this.usuarioDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.usuarioDataGridViewTextBoxColumn.DataPropertyName = "Usuario";
            resources.ApplyResources(this.usuarioDataGridViewTextBoxColumn, "usuarioDataGridViewTextBoxColumn");
            this.usuarioDataGridViewTextBoxColumn.Name = "usuarioDataGridViewTextBoxColumn";
            this.usuarioDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // funcionDataGridViewTextBoxColumn
            // 
            this.funcionDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.funcionDataGridViewTextBoxColumn.DataPropertyName = "Funcion";
            resources.ApplyResources(this.funcionDataGridViewTextBoxColumn, "funcionDataGridViewTextBoxColumn");
            this.funcionDataGridViewTextBoxColumn.Name = "funcionDataGridViewTextBoxColumn";
            this.funcionDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // fechaDeAltaDataGridViewTextBoxColumn
            // 
            this.fechaDeAltaDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.fechaDeAltaDataGridViewTextBoxColumn.DataPropertyName = "Fecha de alta";
            dataGridViewCellStyle1.Format = "G";
            dataGridViewCellStyle1.NullValue = null;
            this.fechaDeAltaDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.fechaDeAltaDataGridViewTextBoxColumn, "fechaDeAltaDataGridViewTextBoxColumn");
            this.fechaDeAltaDataGridViewTextBoxColumn.Name = "fechaDeAltaDataGridViewTextBoxColumn";
            this.fechaDeAltaDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // funcionesBindingSource
            // 
            this.funcionesBindingSource.DataMember = "Funciones";
            this.funcionesBindingSource.DataSource = this.estimaciondeFuncionesDataSet;
            // 
            // estimaciondeFuncionesDataSet
            // 
            this.estimaciondeFuncionesDataSet.DataSetName = "EstimaciondeFuncionesDataSet";
            this.estimaciondeFuncionesDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // botonAceptar
            // 
            resources.ApplyResources(this.botonAceptar, "botonAceptar");
            this.botonAceptar.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.botonAceptar.MaximumSize = new System.Drawing.Size(179, 23);
            this.botonAceptar.MinimumSize = new System.Drawing.Size(179, 23);
            this.botonAceptar.Name = "botonAceptar";
            this.botonAceptar.UseVisualStyleBackColor = true;
            this.botonAceptar.Click += new System.EventHandler(this.botonAceptar_Click);
            // 
            // funcionesTableAdapter
            // 
            this.funcionesTableAdapter.ClearBeforeFill = true;
            // 
            // FuncionesMenpas
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.Name = "FuncionesMenpas";
            this.Load += new System.EventHandler(this.FuncionesMenpas_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.funcionesGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.funcionesBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.estimaciondeFuncionesDataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView funcionesGrid;
        private EstimaciondeFuncionesDataSet estimaciondeFuncionesDataSet;
        private System.Windows.Forms.BindingSource funcionesBindingSource;
        private EstimaciondeFuncionesDataSetTableAdapters.FuncionesTableAdapter funcionesTableAdapter;
        private System.Windows.Forms.Button botonAceptar;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridViewTextBoxColumn usuarioDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn funcionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fechaDeAltaDataGridViewTextBoxColumn;
    }
}