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
    public partial class GeneradorFunciones : Form
    {
        public string texto;

        public GeneradorFunciones(string texto)
        {
            InitializeComponent();
            textBox.Text = texto;            
        }

        private void button_Click(object sender, EventArgs e)
        {
            texto = textBox.Text;
        }

        private void seno_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Sin(x)";
        }

        private void coseno_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Cos(x)";
        }

        private void tangente_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Tan(x)";
        }

        private void lognp_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Log(x, 2.71828183)";
        }

        private void secante_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "(1/Math.Cos(x))";
        }

        private void cosecante_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "(1/Math.Sin(x))";
        }

        private void cotangente_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "(1/Math.Tan(x))";
        }

        private void tanghip_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Tanh(x)";
        }

        private void arcotangente_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Atan(x)";
        }

        private void cosenohip_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Cosh(x)";
        }

        private void senohip_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Sinh(x)";
        }

        private void exponencial_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Exp(x)";
        }

        private void logaritmo_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Log(x, base)";
        }

        private void arcocoseno_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Acos(x)";
        }

        private void arcoseno_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Asin(x)";
        }

        private void absoluto_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Abs(x)";
        }

        private void e_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "2.71828183";
        }

        private void pi_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "3.14159265";
        }

        private void potencia_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Pow(x, exponente)";
        }

        private void raiz2_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "Math.Sqrt(x)";
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData.ToString())
            {
                case "Return":
                    button_Click(sender, e);
                    this.DialogResult = DialogResult.OK;
                    break;
            }
        }
    }
}
