using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Traducciones;

namespace Proyecto
{
    public partial class FuncionesMenpas : Form
    {
        public DataGridView dgv;
        public DataGridViewSelectedCellCollection dgvscc;

        public FuncionesMenpas(string actividad)
        {
            InitializeComponent();

            if (actividad == "cargar")
            {
                botonAceptar.Text = Cadenas.importarFunciones;
            }
            else
            {
                botonAceptar.Text = Cadenas.borrarFunciones;
            }
        }

        private void FuncionesMenpas_Load(object sender, EventArgs e)
        {
            Proyecto.MenPas.WS_EstimacionF WS_EstimacionF = new Proyecto.MenPas.WS_EstimacionF();
            funcionesGrid.DataSource = WS_EstimacionF.ObtenerFuncionesDS().Tables[0];
        }

        private void botonAceptar_Click(object sender, EventArgs e)
        {
            dgv = funcionesGrid;
            dgvscc = funcionesGrid.SelectedCells;
        }
    }
}
