namespace Proyecto
{
    partial class PuntosMenpas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PuntosMenpas));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.puntosGrid = new System.Windows.Forms.DataGridView();
            this.usuarioDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.coordenadaXDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.coordenadaYDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fechaDeAltaDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.puntosBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.estimaciondeFuncionesDataSet = new Proyecto.EstimaciondeFuncionesDataSet();
            this.botonAceptar = new System.Windows.Forms.Button();
            this.puntosTableAdapter = new Proyecto.EstimaciondeFuncionesDataSetTableAdapters.PuntosTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.puntosGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.puntosBindingSource)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.puntosGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.botonAceptar);
            // 
            // puntosGrid
            // 
            this.puntosGrid.AllowUserToAddRows = false;
            this.puntosGrid.AllowUserToDeleteRows = false;
            this.puntosGrid.AutoGenerateColumns = false;
            this.puntosGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.puntosGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.usuarioDataGridViewTextBoxColumn,
            this.coordenadaXDataGridViewTextBoxColumn,
            this.coordenadaYDataGridViewTextBoxColumn,
            this.fechaDeAltaDataGridViewTextBoxColumn});
            this.puntosGrid.DataSource = this.puntosBindingSource;
            resources.ApplyResources(this.puntosGrid, "puntosGrid");
            this.puntosGrid.Name = "puntosGrid";
            this.puntosGrid.ReadOnly = true;
            this.puntosGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // usuarioDataGridViewTextBoxColumn
            // 
            this.usuarioDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.usuarioDataGridViewTextBoxColumn.DataPropertyName = "Usuario";
            resources.ApplyResources(this.usuarioDataGridViewTextBoxColumn, "usuarioDataGridViewTextBoxColumn");
            this.usuarioDataGridViewTextBoxColumn.Name = "usuarioDataGridViewTextBoxColumn";
            this.usuarioDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // coordenadaXDataGridViewTextBoxColumn
            // 
            this.coordenadaXDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.coordenadaXDataGridViewTextBoxColumn.DataPropertyName = "Coordenada X";
            resources.ApplyResources(this.coordenadaXDataGridViewTextBoxColumn, "coordenadaXDataGridViewTextBoxColumn");
            this.coordenadaXDataGridViewTextBoxColumn.Name = "coordenadaXDataGridViewTextBoxColumn";
            this.coordenadaXDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // coordenadaYDataGridViewTextBoxColumn
            // 
            this.coordenadaYDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.coordenadaYDataGridViewTextBoxColumn.DataPropertyName = "Coordenada Y";
            resources.ApplyResources(this.coordenadaYDataGridViewTextBoxColumn, "coordenadaYDataGridViewTextBoxColumn");
            this.coordenadaYDataGridViewTextBoxColumn.Name = "coordenadaYDataGridViewTextBoxColumn";
            this.coordenadaYDataGridViewTextBoxColumn.ReadOnly = true;
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
            // puntosBindingSource
            // 
            this.puntosBindingSource.DataMember = "Puntos";
            this.puntosBindingSource.DataSource = this.estimaciondeFuncionesDataSet;
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
            this.botonAceptar.Click += new System.EventHandler(this.botonImportar_Click);
            // 
            // puntosTableAdapter
            // 
            this.puntosTableAdapter.ClearBeforeFill = true;
            // 
            // PuntosMenpas
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.Name = "PuntosMenpas";
            this.Load += new System.EventHandler(this.PuntosMenpas_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.puntosGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.puntosBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.estimaciondeFuncionesDataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button botonAceptar;
        private System.Windows.Forms.DataGridView puntosGrid;
        private EstimaciondeFuncionesDataSet estimaciondeFuncionesDataSet;
        private System.Windows.Forms.BindingSource puntosBindingSource;
        private EstimaciondeFuncionesDataSetTableAdapters.PuntosTableAdapter puntosTableAdapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn usuarioDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn coordenadaXDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn coordenadaYDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fechaDeAltaDataGridViewTextBoxColumn;
    }
}