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
    public partial class PuntosMenpas : Form
    {
        public DataGridView dgv;
        public DataGridViewSelectedCellCollection dgvscc;

        public PuntosMenpas(string actividad)
        {
            InitializeComponent();

            if (actividad == "cargar")
            {
                botonAceptar.Text = Cadenas.importarPuntos;
            }
            else
            {
                botonAceptar.Text = Cadenas.borrarPuntos;
            }
        }

        private void PuntosMenpas_Load(object sender, EventArgs e)
        {
            Proyecto.MenPas.WS_EstimacionF WS_EstimacionF = new Proyecto.MenPas.WS_EstimacionF();
            puntosGrid.DataSource = WS_EstimacionF.ObtenerPuntosDS().Tables[0];
        }

        private void botonImportar_Click(object sender, EventArgs e)
        {
            dgv = puntosGrid;
            dgvscc = puntosGrid.SelectedCells;
        }
    }
}
