namespace Proyecto
{
    partial class Calcular
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Calcular));
            this.colorCombo = new System.Windows.Forms.ComboBox();
            this.aceptarBoton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // colorCombo
            // 
            this.colorCombo.FormattingEnabled = true;
            resources.ApplyResources(this.colorCombo, "colorCombo");
            this.colorCombo.Name = "colorCombo";
            // 
            // aceptarBoton
            // 
            this.aceptarBoton.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.aceptarBoton, "aceptarBoton");
            this.aceptarBoton.Name = "aceptarBoton";
            this.aceptarBoton.UseVisualStyleBackColor = true;
            this.aceptarBoton.Click += new System.EventHandler(this.aceptarBoton_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // Calcular
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.aceptarBoton);
            this.Controls.Add(this.colorCombo);
            this.MaximizeBox = false;
            this.Name = "Calcular";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox colorCombo;
        private System.Windows.Forms.Button aceptarBoton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}