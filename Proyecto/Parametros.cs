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
    public partial class Parametros : Form
    {
        private string operacion;
        private double parametro;
        
        public Parametros(string op)
        {
            InitializeComponent();

            operacion = op;
            switch (operacion)
            {
                case "PuntoX": 
                    puntoX();
                    break;
                case "PuntoY":
                    puntoY();
                    break;
                case "Desplazamiento":
                    desplazamiento();
                    break;
                case "Coeficiente":
                    coeficiente();
                    break;
                case "Filas":
                    filas();
                    break;
            }
        }

        private void puntoX()
        {            
            this.Text = Cadenas.introduzcaCoordenadaX;
            label1.Text = Cadenas.coordenadaX;
            label2.Text = Cadenas.advertenciaNumero;
            label3.Text = "";

            switch (System.Globalization.CultureInfo.CurrentCulture.Name)
            {
                case "pt":                    
                    textBox.Size = new Size(157, 20);
                    textBox.Location = new Point(93, 24);
                    label2.Location = new Point(93, 70);
                    break;

                case "en":                    
                    textBox.Size = new Size(150, 20);
                    textBox.Location = new Point(100, 24);
                    label2.Location = new Point(83, 70);                    
                    break;

                case "":
                case "es-ES":
                default:
                    textBox.Size = new Size(145, 20);
                    textBox.Location = new Point(105, 24);                    
                    label2.Location = new Point(50, 70);
                    break;
            }
        }

        private void puntoY()
        {
            this.Text = Cadenas.introduzcaCoordenadaY;
            label1.Text = Cadenas.coordenadaY;
            label2.Text = Cadenas.advertenciaNumero;
            label3.Text = "";

            switch (System.Globalization.CultureInfo.CurrentCulture.Name)
            {
                case "pt":                    
                    textBox.Size = new Size(157, 20);
                    textBox.Location = new Point(93, 24);
                    label2.Location = new Point(93, 70);
                    break;

                case "en":                   
                    textBox.Size = new Size(150, 20);
                    textBox.Location = new Point(100, 24);                    
                    label2.Location = new Point(83, 70);
                    break;

                case "es-ES":
                case "es":
                default:
                    textBox.Size = new Size(145, 20);
                    textBox.Location = new Point(105, 24);
                    label2.Location = new Point(50, 70);
                    break;
            }
        }

        private void desplazamiento()
        {
            this.Text = Cadenas.desplazamiento;
            label1.Text = Cadenas.factorDeDesplazamiento;
            label2.Text = Cadenas.advertenciaDesplazamiento1;
            label3.Text = Cadenas.advertenciaDesplazamiento2;

            switch (System.Globalization.CultureInfo.CurrentCulture.Name)
            {                    
                case "en":                    
                    textBox.Size = new Size(170, 20);
                    textBox.Location = new Point(80, 24);
                    label2.Location = new Point(80, 70);
                    label3.Location = new Point(60, 90);
                    break;

                case "pt":
                    textBox.Size = new Size(110, 20);
                    textBox.Location = new Point(140, 24);
                    label2.Location = new Point(50, 70);
                    label3.Location = new Point(40, 90);
                    break;

                case "es-ES":
                case "español":
                default:
                    textBox.Size = new Size(100, 20);
                    textBox.Location = new Point(150, 24);
                    label2.Location = new Point(28, 70);
                    label3.Location = new Point(35, 90);
                    break;
            }
        }

        private void coeficiente()
        {
            this.Text = Cadenas.coeficiente;
            label1.Text = Cadenas.coeficienteDeRegresion;
            label2.Text = Cadenas.advertenciaCoeficiente1;
            label3.Text = Cadenas.advertenciaCoeficiente2;

            switch (System.Globalization.CultureInfo.CurrentCulture.Name)
            {
                case "inglés":
                    textBox.Size = new Size(112, 20);
                    textBox.Location = new Point(136, 24);
                    label2.Location = new Point(70, 70);
                    label3.Location = new Point(95, 90);
                    break;

                case "portugués":
                    textBox.Size = new Size(100, 20);
                    textBox.Location = new Point(151, 24);
                    label2.Location = new Point(65, 70);
                    label3.Location = new Point(85, 90);
                    break;

                case "español":
                default:
                    textBox.Size = new Size(108, 20);
                    textBox.Location = new Point(142, 24);
                    label2.Location = new Point(50, 70);
                    label3.Location = new Point(68, 90);
                    break;
            }
        }

        private void filas()
        {
            this.Text = Cadenas.Filas;
            label1.Text = Cadenas.numeroDeFilas;
            label2.Text = Cadenas.advertenciaFilas1;
            label3.Text = Cadenas.advertenciaFilas2;

            switch (System.Globalization.CultureInfo.CurrentCulture.Name)
            {
                case "inglés":
                    textBox.Size = new Size(140, 20);
                    textBox.Text = "0";
                    textBox.Location = new Point(110, 24);
                    label2.Location = new Point(70, 70);
                    label3.Location = new Point(75, 90);
                    break;

                case "portugués":
                    textBox.Size = new Size(100, 20);
                    textBox.Text = "0";
                    textBox.Location = new Point(151, 24);
                    label2.Location = new Point(65, 70);
                    label3.Location = new Point(85, 90);
                    break;

                case "español":
                default:
                    textBox.Size = new Size(143, 20);
                    textBox.Text = "0";
                    textBox.Location = new Point(107, 24);
                    label2.Location = new Point(40, 70);
                    label3.Location = new Point(58, 90);
                    break;
            }
        }

        private void aceptar_Click(object sender, EventArgs e)
        {
            bool ok = double.TryParse(textBox.Text, out parametro);
            if (ok == false)
            {
                DialogResult = DialogResult.No;
            }
            else
            {
                if (textBox.Text.Contains('.'))
                {
                    DialogResult = DialogResult.No;
                }
                else
                {
                    DialogResult = DialogResult.OK;
                }                
            }
        }

        public double devolverParametro()
        {
            return parametro;
        }        

        private void Parametros_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData.ToString())
            {
                case "Return":
                    aceptar_Click(sender, e);
                    this.DialogResult = DialogResult.OK;
                    break;
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData.ToString())
            {
                case "Return":
                    aceptar_Click(sender, e);
                    break;
            }
        }
    }
}
