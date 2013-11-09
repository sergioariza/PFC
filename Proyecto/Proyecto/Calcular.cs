using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Proyecto
{
    public partial class Calcular : Form
    {
        private Dictionary<string, int> dic;
        public string color;

        public Calcular(Dictionary<string, int> d)
        {
            InitializeComponent();
            dic = d;

            for (int i = 0; i < dic.Keys.Count; i++)
            {
                colorCombo.Items.Add(dic.Keys.ElementAt(i));
            }

            color = "Ninguno";
        }

        private void aceptarBoton_Click(object sender, EventArgs e)
        {
            if (colorCombo.SelectedIndex != -1)
            {
                color = (string)colorCombo.SelectedItem;
            }
        }
    }
}