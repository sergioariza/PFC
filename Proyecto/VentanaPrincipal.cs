using System;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Microsoft.CSharp;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using Traducciones;

namespace Proyecto
{
    public partial class VentanaPrincipal : Form
    {
        float alturaPanelInt, anchuraPanelInt, alturaContenedor, anchuraContenedor,
                     alturaTeorica, anchuraTeorica, x_inicio, x_fin, y_inicio, y_fin;
        int idFuncion = 0, desplazamiento = 10;
        double zoom, zoom_id, zoom_anterior, zoom_posterior;
        List<Funcion> listadoFunciones = new List<Funcion>();
        List<Punto> listadoPuntos = new List<Punto>(), listadoPuntosDibujo = new List<Punto>();
        string ficheroFunciones = " ", ficheroPuntos = " ", usuario;
        ListViewItem item;
        Punto p_inicio, p_fin;
        bool pulsado = false, primera = true;

        public VentanaPrincipal()
        {
            InitializeComponent();
            splitContainer1.KeyDown += new KeyEventHandler(validarTeclas);
            panelExt.KeyDown += new KeyEventHandler(validarTeclas);
            panelInt.KeyDown += new KeyEventHandler(validarTeclas);
            historialFunciones.KeyDown += new KeyEventHandler(validarTeclas);
            historialPuntos.KeyDown += new KeyEventHandler(validarTeclas);
        }

        private void validarTeclas(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            switch (e.KeyData.ToString())
            {
                case "Up":
                    buttonArriba_Click(sender, e);
                    break;
                case "Down":
                    buttonAbajo_Click(sender, e);
                    break;
                case "Right":
                    buttonDerecha_Click(sender, e);
                    break;
                case "Left":
                    buttonIzquierda_Click(sender, e);
                    break;
                default:
                    break;
            }
        }

        private void Principal_Load(object sender, EventArgs e)
        {
            //Inicializar el zoom por defecto
            //zoom = 100;
            //zoom_anterior = 71;
            //zoom_posterior = 141;
            //zoom_id = 0;

            //Inicializar el zoom por defecto (nueva versión)
            zoom = 400;
            zoom_anterior = 283;
            zoom_posterior = 566;
            zoom_id = 4;

            //Iniciar sesión
            Login login = new Login();
            
            login.DialogResult = DialogResult.No;
           
            while (login.DialogResult == DialogResult.No)
            {
                login.ShowDialog();
                if (login.DialogResult == DialogResult.OK)
                {
                    if (login.login == false)
                    {
                        MessageBox.Show(Cadenas.inicioNok);
                        login.DialogResult = DialogResult.No;
                    }
                    else
                    {
                        traducirControles(this);
                        traducirContenidoGeneral();
                        traducirColumnasHistoriales();
                        traducirBarraDeEstado();
                        cambiarIdiomaFunciones();
                        cambiarIdiomaPuntos();
                        usuario = login.usuarioLogin;
                    }
                }
                else
                {
                    this.Close();
                }
            }

            //Inicializo los puntos frontera
            x_inicio = 0;
            x_fin = 0;
            y_inicio = 0;
            y_fin = 0;

            //Borrado de funciones previas existentes
            borrar_funciones_previas();

            //Etiquetado del tamaño actual del control de panel
            estadoLabel.Text = Cadenas.tamaño + ": " + anchuraPanelInt + ", " + alturaPanelInt;
        }

        private void borrar_funciones_previas()
        {
            //Borrado de funciones previas
            int i = 0;
            for (i = 0; i < 5000; i++)
            {
                if (File.Exists("c:\\func\\" + i + ".dll"))
                {
                    try { File.Delete("c:\\func\\" + i + ".dll"); }
                    catch (Exception) { ;}
                }

                if (File.Exists("c:\\func\\" + i + ".cs"))
                {
                    try { File.Delete("c:\\func\\" + i + ".cs"); }
                    catch (Exception) { ;}
                }
            }
        }

        private void panelExt_Paint(object sender, PaintEventArgs e)
        {
            actualizarPanel();
            pintarReglaYEje();

            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);                    
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }

            estadoLabel.Text = Cadenas.tamaño + ": " + anchuraPanelInt + ", " + alturaPanelInt;
            zoomLabel.Text = "Zoom: " + zoom + "%";
            desplazamientoLabel.Text = Cadenas.desplazamiento + ": " + desplazamiento + " pts";
        }
        
        private void actualizarPanel()
        {
            //Asignación de tamaños de panel1
            alturaContenedor = panelExt.Size.Height;
            anchuraContenedor = panelExt.Size.Width;

            //Asignación de tamaños de panel2
            alturaPanelInt = panelInt.Size.Height - 3;
            anchuraPanelInt = panelInt.Size.Width - 3;

            //Asignación teórica de tamaños de la gráfica
            anchuraTeorica = (float)Math.Round((anchuraPanelInt / (float)zoom) * 100);
            alturaTeorica = (float)Math.Round((alturaPanelInt / (float)zoom) * 100);

            //Asignación de puntos frontera
            float x_inicio_temp, x_fin_temp, y_inicio_temp, y_fin_temp;

            x_inicio_temp = x_inicio;
            x_fin_temp = x_fin;
            y_inicio_temp = y_inicio;
            y_fin_temp = y_fin;

            x_inicio = ((anchuraTeorica / 2) * (-1)) + ((x_fin_temp + x_inicio_temp) / 2);
            x_fin = (anchuraTeorica / 2) + ((x_fin_temp + x_inicio_temp) / 2);
            y_inicio = (alturaTeorica / 2) + ((y_fin_temp + y_inicio_temp) / 2);
            y_fin = ((alturaTeorica / 2) * (-1)) + ((y_fin_temp + y_inicio_temp) / 2);
        }

        private void pintarRegla(Graphics gr)
        {
            Pen blackPen = new Pen(Color.Black, 1);
            double y0, y10, x0, x10, ampliacion;
            Font fuente = new Font("Arial", 6);
            SolidBrush letra = new SolidBrush(Color.Black);
            int indice = 0;

            //Reinicio de regla
            gr.Clear(SystemColors.Control);

            y0 = (int)pasar_coordenada_realY(0);
            y10 = (int)pasar_coordenada_realY(10);
            x0 = (int)pasar_coordenada_realX(0);
            x10 = (int)pasar_coordenada_realX(10);

            //Pintado del reglado izquierdo
            ampliacion = y0 - y10;

            //Pintado del centro
            int y = (int)pasar_coordenada_realY(0) + 22;

            if ((y > 20) && (y < alturaContenedor - 20))
            {
                try
                {
                    gr.DrawLine(blackPen, 10, y, 20, y);
                }
                catch (Exception msg)
                {

                }

                gr.DrawString(indice.ToString(), fuente, letra, 0, y - 5);
            }

            //Pintado de la parte inferior
            y = y + (int)ampliacion;
            indice = -10;

            while (y < (alturaContenedor - 20))
            {
                if ((y > 20) && (y < alturaContenedor - 20))
                {
                    try{
                        gr.DrawLine(blackPen, 15, y, 20, y);
                    }
                    catch (Exception msg)
                    {

                    }

                    if ((zoom_id <= 0) && (Math.Abs(indice) % 50 == 0))
                    {
                        gr.DrawString(indice.ToString(), fuente, letra, -1, y - 5);
                    }
                    else if (zoom_id > 0)
                    {
                        gr.DrawString(indice.ToString(), fuente, letra, -1, y - 5);
                    }
                }

                y = y + (int)ampliacion;
                indice = indice - 10;
            }

            //Pintado de parte superior
            y = (int)pasar_coordenada_realY(0) + 22;
            y = y - (int)ampliacion;
            indice = 10;
            while (y > 20)
            {
                if ((y > 20) && (y < alturaContenedor - 20))
                {
                    try{
                        gr.DrawLine(blackPen, 15, y, 20, y);
                    }
                    catch (Exception msg)
                    {

                    }

                    if ((zoom_id <= 0) && (indice % 50 == 0))
                    {
                        gr.DrawString(indice.ToString(), fuente, letra, -1, y - 5);
                    }
                    else if (zoom_id > 0)
                    {
                        gr.DrawString(indice.ToString(), fuente, letra, -1, y - 5);
                    }
                }

                y = y - (int)ampliacion;
                indice = indice + 10;
            }

            //Pintado del reglado superior
            //Pintado del centro
            indice = 0;
            int x = (int)pasar_coordenada_realX(0) + 22;
            if ((x > 20) && (x < anchuraContenedor - 20))
            {
                try{
                    gr.DrawLine(blackPen, x, 10, x, 20);
                    gr.DrawString(indice.ToString(), fuente, letra, x - 3, 1);
                }
                catch (Exception msg)
                {

                }
            }

            //Pintado de la parte derecha
            indice = 10;
            ampliacion = x10 - x0;
            x = x + (int)ampliacion;

            while (x < (anchuraContenedor - 20))
            {
                if ((x > 20) && (x < anchuraContenedor - 20))
                {
                    try{
                        gr.DrawLine(blackPen, x, 15, x, 20);

                        if ((zoom_id <= 0) && (indice % 50 == 0))
                        {
                            gr.DrawString(indice.ToString(), fuente, letra, x - 6, 3);
                        }
                        else if (zoom_id > 0)
                        {
                            gr.DrawString(indice.ToString(), fuente, letra, x - 6, 3);
                        }
                    }
                    catch (Exception msg)
                    {

                    }
                }

                indice = indice + 10;
                x = x + (int)ampliacion;
            }

            //Pintado de parte izquierda
            indice = -10;
            x = (int)pasar_coordenada_realX(0) + 22;
            x = x - (int)ampliacion;
            while (x > 20)
            {
                if ((x > 20) && (x < anchuraContenedor - 20))
                {
                    try{
                        gr.DrawLine(blackPen, x, 15, x, 20);

                        if ((zoom_id <= 0) && (Math.Abs(indice) % 50 == 0))
                        {
                            gr.DrawString(indice.ToString(), fuente, letra, x - 6, 3);
                        }
                        else if (zoom_id > 0)
                        {
                            gr.DrawString(indice.ToString(), fuente, letra, x - 10, 3);
                        }
                    }
                    catch (Exception msg)
                    {

                    }
                }

                indice = indice - 10;
                x = x - (int)ampliacion;
            }
        }

        private void pintarEje(Graphics gr)
        {
            //Reinicio del panel interior
            int x = 0, y = 0;
            Pen blackPen = new Pen(Color.Black, 2);

            gr.Clear(Color.White);
            x = (int)pasar_coordenada_realX(0);
            y = (int)pasar_coordenada_realY(0);
            
            try{
                gr.DrawLine(blackPen, 0, y, anchuraPanelInt, y);
                gr.DrawLine(blackPen, x, 0, x, alturaPanelInt);
            }
            catch (Exception msg)
            {

            }

            if (chequeoCeldas.Checked)
            {
                pintarCeldas(gr);
            }
        }

        private void pintarReglaYEje()
        {
            //Variables para pintar eje
            int x = 0, y = 0;
            Graphics lineaPanelInt = panelInt.CreateGraphics();
            Pen blackPenPanelInt = new Pen(Color.Black, 2);

            //Variables para pintar regla
            Graphics lineaPanelExt = panelExt.CreateGraphics();
            Pen blackPenPanelExt = new Pen(Color.Black, 1);
            double y0, y10, x0, x10, ampliacion;
            Font fuente = new Font("Arial", 6);
            SolidBrush letra = new SolidBrush(Color.Black);
            int indice = 0;

            //Comienzo del procedimiento para pintar eje            
            lineaPanelInt.Clear(Color.White);
            x = (int)pasar_coordenada_realX(0);
            y = (int)pasar_coordenada_realY(0);

            try{
                lineaPanelInt.DrawLine(blackPenPanelInt, 0, y, anchuraPanelInt, y);
                lineaPanelInt.DrawLine(blackPenPanelInt, x, 0, x, alturaPanelInt);
            }
            catch (Exception msg)
            {

            }

            blackPenPanelInt = new Pen(Color.Black, 1);

            //Comienzo del procedimiento para pintar regla
            //Reinicio de regla
            lineaPanelExt.Clear(SystemColors.Control);

            y0 = (int)pasar_coordenada_realY(0);
            y10 = (int)pasar_coordenada_realY(10);
            x0 = (int)pasar_coordenada_realX(0);
            x10 = (int)pasar_coordenada_realX(10);

            //Pintado del reglado izquierdo
            //Pintado del centro
            ampliacion = y0 - y10;
            y = (int)pasar_coordenada_realY(0) + 22;

            if ((y > 20) && (y < alturaContenedor - 20))
            {
                try{
                    lineaPanelExt.DrawLine(blackPenPanelExt, 10, y, 20, y);
                    lineaPanelExt.DrawString(indice.ToString(), fuente, letra, 0, y - 5);
                }
                catch (Exception msg)
                {

                }
            }

            //Pintado de la parte inferior
            y = y + (int)ampliacion;
            indice = -10;

            while (y < (alturaContenedor - 20))
            {
                if ((y > 20) && (y < alturaContenedor - 20))
                {
                    try{
                        lineaPanelExt.DrawLine(blackPenPanelExt, 15, y, 20, y);

                        if ((zoom_id <= 0) && (Math.Abs(indice) % 50 == 0))
                        {
                            lineaPanelExt.DrawString(indice.ToString(), fuente, letra, -1, y - 5);
                        }
                        else if (zoom_id > 0)
                        {
                            lineaPanelExt.DrawString(indice.ToString(), fuente, letra, -1, y - 5);
                        }
                    }
                    catch (Exception msg)
                    {

                    }
                }

                y = y + (int)ampliacion;
                indice = indice - 10;
            }

            //Pintado de parte superior
            y = (int)pasar_coordenada_realY(0) + 22;
            y = y - (int)ampliacion;
            indice = 10;
            while (y > 20)
            {
                if ((y > 20) && (y < alturaContenedor - 20))
                {
                    try{
                        lineaPanelExt.DrawLine(blackPenPanelExt, 15, y, 20, y);

                        if ((zoom_id <= 0) && (indice % 50 == 0))
                        {
                            lineaPanelExt.DrawString(indice.ToString(), fuente, letra, -1, y - 5);
                        }
                        else if (zoom_id > 0)
                        {
                            lineaPanelExt.DrawString(indice.ToString(), fuente, letra, -1, y - 5);
                        }
                    }
                    catch (Exception msg)
                    {

                    }
                }

                y = y - (int)ampliacion;
                indice = indice + 10;
            }

            //Pintado del reglado superior
            //Pintado del centro
            indice = 0;
            x = (int)pasar_coordenada_realX(0) + 22;
            if ((x > 20) && (x < anchuraContenedor - 20))
            {
                try{
                    lineaPanelExt.DrawLine(blackPenPanelExt, x, 10, x, 20);
                    lineaPanelExt.DrawString(indice.ToString(), fuente, letra, x - 3, 1);
                }
                catch (Exception msg)
                {

                }
            }

            //Pintado de la parte derecha
            indice = 10;
            ampliacion = x10 - x0;
            x = x + (int)ampliacion;

            while (x < (anchuraContenedor - 20))
            {
                if ((x > 20) && (x < anchuraContenedor - 20))
                {
                    try{
                        lineaPanelExt.DrawLine(blackPenPanelExt, x, 15, x, 20);

                        if ((zoom_id <= 0) && (indice % 50 == 0))
                        {
                            lineaPanelExt.DrawString(indice.ToString(), fuente, letra, x - 6, 3);
                        }
                        else if (zoom_id > 0)
                        {
                            lineaPanelExt.DrawString(indice.ToString(), fuente, letra, x - 6, 3);
                        }
                    }
                    catch (Exception msg)
                    {

                    }
                }

                indice = indice + 10;
                x = x + (int)ampliacion;
            }

            //Pintado de parte izquierda
            indice = -10;
            x = (int)pasar_coordenada_realX(0) + 22;
            x = x - (int)ampliacion;
            while (x > 20)
            {
                if ((x > 20) && (x < anchuraContenedor - 20))
                {
                    try{
                        lineaPanelExt.DrawLine(blackPenPanelExt, x, 15, x, 20);

                        if ((zoom_id <= 0) && (Math.Abs(indice) % 50 == 0))
                        {
                            lineaPanelExt.DrawString(indice.ToString(), fuente, letra, x - 6, 3);
                        }
                        else if (zoom_id > 0)
                        {
                            lineaPanelExt.DrawString(indice.ToString(), fuente, letra, x - 10, 3);
                        }
                    }
                    catch (Exception msg)
                    {

                    }
                }

                indice = indice - 10;
                x = x - (int)ampliacion;
            }


            if (chequeoCeldas.Checked)
            {
                if (zoom_id == -2)
                {
                    estadoLabel.Text = Cadenas.advertenciaCeldasZoom;
                    chequeoCeldas.Checked = false;
                }
                else
                {
                    pintarCeldas(lineaPanelInt);
                }
            }

            //Se liberan ambos recursos
            lineaPanelInt.Dispose();
            lineaPanelExt.Dispose();
        }

        private void pintarCeldas(Graphics gr)
        {
            double x0, x10, ampliacion;
            int x, y;
            Pen blackPenPanelInt = new Pen(Color.Black, 1);

            x0 = (int)pasar_coordenada_realX(0);
            x10 = (int)pasar_coordenada_realX(10);
            ampliacion = x10 - x0;


            y = (int)alturaPanelInt / 2;
            while (y <= (int)alturaPanelInt)
            {
                x = (int)anchuraPanelInt / 2;
                while (x <= (int)anchuraPanelInt)
                {
                    try{
                        gr.DrawLine(blackPenPanelInt, x, y - 2, x, y + 2);
                    }
                    catch (Exception msg)
                    {

                    }

                    x = x + (int)ampliacion;
                }

                x = (int)anchuraPanelInt / 2;
                while (x >= 0)
                {
                    try{
                        gr.DrawLine(blackPenPanelInt, x, y - 2, x, y + 2);
                    }
                    catch (Exception msg)
                    {

                    }

                    x = x - (int)ampliacion;
                }

                y = y + (int)ampliacion;
            }

            y = (int)alturaPanelInt / 2;
            while (y >= 0)
            {
                x = (int)anchuraPanelInt / 2;
                while (x <= (int)anchuraPanelInt)
                {
                    try{
                        gr.DrawLine(blackPenPanelInt, x, y - 2, x, y + 2);
                    }
                    catch (Exception msg)
                    {

                    }

                    x = x + (int)ampliacion;
                }

                x = (int)anchuraPanelInt / 2;
                while (x >= 0)
                {
                    try{
                        gr.DrawLine(blackPenPanelInt, x, y - 2, x, y + 2);
                    }
                    catch (Exception msg)
                    {

                    }

                    x = x - (int)ampliacion;
                }

                y = y - (int)ampliacion;
            }

            x = (int)anchuraPanelInt / 2;
            while (x <= (int)anchuraPanelInt)
            {
                y = (int)alturaPanelInt / 2;
                while (y <= (int)alturaPanelInt)
                {
                    try{
                        gr.DrawLine(blackPenPanelInt, x - 2, y, x + 2, y);
                    }
                    catch (Exception msg)
                    {

                    }

                    y = y + (int)ampliacion;
                }

                y = (int)alturaPanelInt / 2;
                while (y >= 0)
                {
                    try{
                        gr.DrawLine(blackPenPanelInt, x - 2, y, x + 2, y);
                    }
                    catch (Exception msg)
                    {

                    }

                    y = y - (int)ampliacion;
                }

                x = x + (int)ampliacion;
            }

            x = (int)anchuraPanelInt / 2;
            while (x >= 0)
            {
                y = (int)alturaPanelInt / 2;
                while (y <= (int)alturaPanelInt)
                {
                    try
                    {
                        gr.DrawLine(blackPenPanelInt, x - 2, y, x + 2, y);
                    }
                    catch (Exception msg)
                    {

                    }

                    y = y + (int)ampliacion;
                }

                y = (int)alturaPanelInt / 2;
                while (y >= 0)
                {
                    try{
                        gr.DrawLine(blackPenPanelInt, x - 2, y, x + 2, y);
                    }
                    catch (Exception msg)
                    {

                    }

                    y = y - (int)ampliacion;
                }

                x = x - (int)ampliacion;
            }
        }

        private void pintarPunto(Punto p)
        {
            Graphics linea = panelInt.CreateGraphics();
            Pen pen = new Pen(p.devuelve_Color(), 2);
            int x = (int)pasar_coordenada_realX(p.coordenadaX());
            int y = (int)pasar_coordenada_realY(p.coordenadaY());

            try
            {
                linea.DrawLine(pen, x - 3, y - 3, x + 3, y + 3);
                linea.DrawLine(pen, x - 3, y + 3, x + 3, y - 3);
                linea.Dispose();
            }
            catch (Exception msg)
            {

            }
        }

        private void pintarPunto(Punto p, MouseEventArgs e)
        {
            Graphics linea = panelInt.CreateGraphics();
            Pen pen = new Pen(p.devuelve_Color(), 2);
            int x = e.X;
            int y = e.Y;

            try
            {
                linea.DrawLine(pen, x - 3, y - 3, x + 3, y + 3);
                linea.DrawLine(pen, x - 3, y + 3, x + 3, y - 3);
                linea.Dispose();
            }
            catch (Exception msg)
            {

            }
        }

        private void pintarFuncion(Funcion f, Graphics gr)
        {
            bool inicio = true, dentro = false;
            Pen pen;
            double x_real, x_teorica, y_real, y_teorica;
            double x_ant = int.MinValue, y_ant = int.MaxValue;

            Color color = f.devuelve_color();
            pen = new Pen(color, 2);

            for (x_real = 0; x_real <= anchuraPanelInt; x_real++)
            {
                x_teorica = pasar_coordenada_teoricaX(x_real);
                y_teorica = evalua(x_teorica, f.devuelve_id());

                if ((y_teorica <= y_inicio) && (y_fin <= y_teorica))
                {
                    y_real = pasar_coordenada_realY(y_teorica);

                    if ((y_real >= 0) && (y_real <= alturaPanelInt))
                    {
                        if (inicio == true)
                        {
                            inicio = false;
                            x_ant = x_real;
                            y_ant = y_real;
                        }
                        else
                        {
                            try
                            {
                                gr.DrawLine(pen, (int)x_ant, (int)y_ant, (int)x_real, (int)y_real);
                            }
                            catch (Exception msg)
                            {
                                continue;
                            }

                            x_ant = x_real;
                            y_ant = y_real;
                        }

                        dentro = true;
                    }
                }
                else
                {
                    if (dentro == true)
                    {
                        if (y_teorica > y_inicio)
                        {
                            y_real = 0;
                        }
                        else
                        {
                            y_real = alturaPanelInt;
                        }

                        try
                        {
                            gr.DrawLine(pen, (int)x_ant, (int)y_ant, (int)x_real, (int)y_real);
                        }
                        catch (Exception msg)
                        {
                        }

                        x_ant = x_real;
                        y_ant = y_real;
                        dentro = false;
                    }
                    else
                    {
                        y_real = pasar_coordenada_realY(y_teorica);

                        if ((y_ant <= 0) && (y_real >= alturaPanelInt) && (inicio == false))
                        {
                            try{
                                gr.DrawLine(pen, (int)x_ant, 0, (int)x_real, alturaPanelInt);
                            }
                            catch (Exception msg)
                            {

                            }
                        }
                        else if (((y_ant >= alturaPanelInt) && (y_real <= 0)) && (inicio == false))
                        {
                            try{
                                gr.DrawLine(pen, (int)x_ant, alturaPanelInt, (int)x_real, 0);
                            }
                            catch (Exception msg)
                            {

                            }
                        }

                        x_ant = x_real;

                        if (y_teorica > y_inicio)
                        {
                            y_ant = 0;
                        }
                        else
                        {
                            y_ant = alturaPanelInt;
                        }
                                                
                        if (inicio == true)
                        {
                            inicio = false;
                        }                        
                    }
                }
            }
        }

        private Color obtenerColor()
        {
            if (color.SelectedItem == null)
            {
                return Color.Black;
            }
            else
            {
                switch (color.SelectedItem.ToString())
                {
                    case "Negro":
                    case "Black":
                    case "Preto":
                        return Color.Black;
                    case "Azul":
                    case "Blue":
                        return Color.Blue;
                    case "Rojo":
                    case "Red":
                    case "Vermelho":
                        return Color.Red;
                    case "Verde":
                    case "Green":
                        return Color.Green;
                    case "Marrón":
                    case "Brown":
                    case "Marrom":
                        return Color.Brown;
                    case "Violeta":
                    case "Violet":
                        return Color.Violet;
                    case "Naranja":
                    case "Orange":
                    case "Laranja":
                        return Color.Orange;
                    case "Gris":
                    case "Gray":
                    case "Cinza":
                        return Color.Gray;
                    case "Azul oscuro":
                    case "Dark blue":
                    case "Azul escuro":
                        return Color.DarkBlue;
                    case "Magenta":
                        return Color.Magenta;
                    default:
                        return Color.Black;
                }
            }
        }

        private void panelInt_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //Calculamos sus coordenadas
                double x_revisado = (e.X / zoom) * 100;
                double x = x_revisado - ((anchuraTeorica / 2)) + ((x_inicio + x_fin) / 2);
                x = Math.Round(x, 2);

                double y_revisado = (e.Y / zoom) * 100;
                double y = (y_revisado * (-1)) + ((alturaTeorica / 2)) + ((y_inicio + y_fin) / 2);
                y = Math.Round(y, 2);

                Punto p = new Punto(x, y, true, obtenerColor());
                p_fin = new Punto(x, y, true, Color.Black);

                
                if ((p_inicio.coordenadaX() != p_fin.coordenadaX()) || (p_inicio.coordenadaY() != p_fin.coordenadaY()))
                {
                    return;
                }
                
                if (existe_punto(p))
                {
                    MessageBox.Show(Cadenas.error001);
                }
                else
                {
                    //Agregamos el punto en la lista, en el historial y en el menu
                    listadoPuntos.Add(p);

                    ListViewItem lvi = new ListViewItem("(" + String.Format("{0:0.00}", x) + "; " + String.Format("{0:0.00}", y) + ")");

                    lvi.SubItems.Add(devuelveColorStr(obtenerColor().Name));
                    historialPuntos.Items.Add(lvi);

                    string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                    if (puntosMenu.DropDownItems[3].ToString().CompareTo(noHayPuntos) == 0)
                    {
                        puntosMenu.DropDownItems.RemoveAt(3);
                    }

                    puntosMenu.DropDownItems.Add("(" + String.Format("{0:0.00}", x) + "; " + String.Format("{0:0.00}", y) + ")");

                    //Y finalmente pintamos el punto
                    pintarPunto(p);

                    //Habilitamos las opciones de quitar puntos en el menú
                    quitarPrimerPuntoAñadido.Enabled = true;
                    quitarUltimoPuntoAñadido.Enabled = true;

                    //Informamos en la barra de estado que se ha insertado el punto correctamente
                    estadoLabel.Text = Cadenas.puntoInsertadoEnCoordenada + " => (" + String.Format("{0:0.00}", p.coordenadaX()) + "; " + String.Format("{0:0.00}", p.coordenadaY()) + ")";
                }
            }
        }

        private void insertarFuncion_Click(object sender, EventArgs e)
        {
            Funcion aux;
            bool ok;

            if (textoFuncion.Text == "")
            {
                MessageBox.Show(Cadenas.error002);
                return;
            }

            if (existe_funcion(textoFuncion.Text))
            {
                MessageBox.Show(Cadenas.error003);
                return;
            }

            ok = genera_funcion(textoFuncion.Text);
            if (ok)
            {
                if (historialFunciones.Items.Count == 0)
                {
                    funcionesMenu.DropDownItems.Remove(funcionesMenu.DropDownItems[3]);
                }

                ListViewItem lvi = new ListViewItem("f(x) = " + textoFuncion.Text);
                string traduccion = traducirFuncion(textoFuncion.Text);
                lvi.SubItems.Add("f(x) = " + traduccion);
                lvi.SubItems.Add(devuelveColorStr(obtenerColor().Name));
                historialFunciones.Items.Add(lvi);
                funcionesMenu.DropDownItems.Add(textoFuncion.Text);
                borrarPrimeraFuncion.Enabled = true;
                borrarUltimaFuncion.Enabled = true;
                Color color = obtenerColor();
                aux = new Funcion(textoFuncion.Text, idFuncion, color, true);
                listadoFunciones.Add(aux);
                Graphics gr = panelInt.CreateGraphics();
                pintarFuncion(aux, gr);
                idFuncion++;
                estadoLabel.Text = Cadenas.evaluacionCorrectaDeFuncion;
                textoFuncion.Text = "";
            }
            else
            {
                string textoErroneo = textoFuncion.Text;
                MessageBox.Show(Cadenas.evaluacionIncorrectaDeFuncion + " => " + textoErroneo);
                estadoLabel.Text = Cadenas.evaluacionIncorrectaDeFuncion + " => " + textoErroneo;
            }
        }

        private bool genera_funcion(string f)
        {
            // Genero el código
            if (File.Exists("c:\\func\\" + idFuncion + ".dll")) return false;

            if (System.IO.Directory.Exists("c:\\func\\") == false)
            {
                System.IO.Directory.CreateDirectory("c:\\func\\");
            }

            StreamWriter sw;
            string destPath = "c:\\func\\" + idFuncion + ".cs";

            sw = new StreamWriter(destPath);
            sw.WriteLine("using System;");
            sw.WriteLine("");
            sw.WriteLine("namespace GeneracionCargaCodigo");
            sw.WriteLine("{");
            sw.WriteLine("    class funcion");
            sw.WriteLine("    {");
            sw.WriteLine("        public static double evalua(double x)");
            sw.WriteLine("        {");
            sw.WriteLine("            return {0};", f);
            sw.WriteLine("        }");
            sw.WriteLine("    }");
            sw.WriteLine("}");
            sw.Close();

            // Compilo el código
            // Obtain an ICodeCompiler from a CodeDomProvider class.       
            CSharpCodeProvider provider = new CSharpCodeProvider();
            ICodeCompiler compiler = provider.CreateCompiler();

            // Build the parameters for source compilation.
            CompilerParameters cp = new CompilerParameters();
            cp.CompilerOptions = "/target:library";
            // Add an assembly reference.
            cp.ReferencedAssemblies.Add("System.dll");

            // Generate a class library.
            cp.GenerateExecutable = false;

            // Set the assembly file name to generate.
            cp.OutputAssembly = "c:\\func\\" + idFuncion + ".dll";

            // Save the assembly as a physical file.
            cp.GenerateInMemory = false;

            // Invoke compilation.
            //try { File.Delete(cp.OutputAssembly); } // Lo borro si estaba
            //catch (Exception) { ;} //Ignoro la excepción si no existiera el archivo

            CompilerResults cr = compiler.CompileAssemblyFromFile(cp, destPath);

            if (cr.Errors.Count > 0)
            {
                /*string errores = "";
                foreach (CompilerError e in cr.Errors) errores += (e.ToString() + ".\n ");
                switch (idioma)
                {
                    case "español":
                        MessageBox.Show("Error de sintaxis en la función: " + errores + ".");
                        break;
                    case "inglés":
                        MessageBox.Show("Syntax error in function: " + errores + ".");
                        break;
                    case "portugués":
                        MessageBox.Show("Error de sintaxis en la función: " + errores + ".");
                        break;
                    default:
                        MessageBox.Show("Erro de sintaxe na função: " + errores + ".");
                        break;
                }*/
                return false;
            }
            else
            {
                return true;
            }
        }

        private string traducirFuncion(string funcion)
        {
            string str = funcion;

            str = str.Replace("Math.", "");
            str = str.Replace("Sin", "sen");
            str = str.Replace("Cos", "cos");
            str = str.Replace("Tan", "tan");
            str = str.Replace("Log", "Log");
            str = str.Replace("Tanh", "tanh");
            str = str.Replace("Atan", "arcotan");
            str = str.Replace("Cosh", "cosh");
            str = str.Replace("Sinh", "senh");
            str = str.Replace("Acos", "arccos");
            str = str.Replace("Asin", "arcsen");
            str = str.Replace("Abs", "abs");
            str = str.Replace("Sqrt", "√");

            if (str.Contains("Pow"))
            {
                str = exponenteBase(str);
            }

            return str;
        }

        private string exponenteBase(string str)
        {
            int principio = str.IndexOf("Pow");
            str = str.Remove(principio, 3);

            if (!str.Contains("Pow"))
            {
                int fin = principio;
                int coma = 0;
                int nivel = 0;
                bool cerrado = false;

                while (!cerrado)
                {
                    char c = str[fin];
                    switch (c)
                    {
                        case '(': nivel++; break;
                        case ')':
                            nivel--;
                            if (nivel == 0)
                            {
                                cerrado = true;
                                fin--;
                            }
                            break;
                        case ',':
                            if (nivel == 1)
                            {
                                coma = fin;
                            }
                            break;
                        default: break;
                    }
                    fin++;
                }

                string sub = str.Substring(principio, fin - principio + 1);
                string basePotencia = sub.Substring(1, coma - principio - 1);
                string exponentePotencia = sub.Substring(coma - principio + 1, sub.Length - coma + principio - 2);
                string res = "(" + basePotencia + "^" + exponentePotencia + ")";
                res = res.Replace(" ", "");
                str = str.Remove(principio, fin - principio + 1);
                str = str.Insert(principio, res);
            }
            else
            {
                str = exponenteBase(str);

                int fin = principio;
                int nivel = 0;
                int coma = 0;
                bool cerrado = false;

                while (!cerrado)
                {
                    char c = str[fin];
                    switch (c)
                    {
                        case '(': nivel++; break;
                        case ')':
                            nivel--;
                            if (nivel == 0)
                            {
                                cerrado = true;
                                fin--;
                            }
                            break;
                        case ',':
                            if (nivel == 1)
                            {
                                coma = fin;
                            }
                            break;
                        default: break;
                    }
                    fin++;
                }

                string sub = str.Substring(principio, fin - principio + 1);
                string basePotencia = sub.Substring(1, coma - principio - 1);
                string exponentePotencia = sub.Substring(coma + 1 - principio, sub.Length - (coma - principio) - 2);
                string res = "(" + basePotencia + "^" + exponentePotencia + ")";
                res = res.Replace(" ", "");
                str = str.Remove(principio, fin - principio + 1);
                str = str.Insert(principio, res);
            }

            return str;
        }

        private double evalua(double x, int id)
        {

            Assembly asm = Assembly.LoadFrom("c:\\func\\" + id + ".dll");

            MethodInfo eval = asm.GetTypes()[0].GetMethod("evalua");
            object[] par = new object[1];
            par[0] = x;
            double res = (double)eval.Invoke(null, par);

            return res;
        }

        private void generar_Click(object sender, EventArgs e)
        {
            GeneradorFunciones gF = new GeneradorFunciones(textoFuncion.Text);
            
            if (gF.ShowDialog(this) == DialogResult.OK)
            {
                textoFuncion.Text = gF.texto;
            }
        }

        private void insertarFuncionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GeneradorFunciones gF = new GeneradorFunciones(textoFuncion.Text);
            
            if (gF.ShowDialog(this) == DialogResult.OK)
            {
                textoFuncion.Text = gF.texto;
            }
            panelExt_Paint(null, null);
        }

        private void calcular_Click(object sender, EventArgs e)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            Color colorPunto;
            double[] x;
            double[] y;
            int index = 0, i;
            string ecuacionFinal = "", colorStr = "";

            //Primero se comprueba que se haya seleccionado un tipo de aproximación
            if (listadoPuntos.Count <= 1)
            {
                MessageBox.Show(Cadenas.error004);
                return;
            }

            //Luego se comprueba que se haya seleccionado un tipo de aproximación
            if (tipoAprox.SelectedIndex == -1)
            {
                MessageBox.Show(Cadenas.error005);
                return;
            }

            //Después de las verificaciones previas, se comprueba cuántos colores
            //diferentes hay en el historial
            for (i = 0; i < listadoPuntos.Count; i++)
            {
                ListViewItem lvi = historialPuntos.Items[i];
                colorStr = lvi.SubItems[1].Text;

                if (dic.ContainsKey(colorStr))
                {
                    int valor;
                    dic.TryGetValue(colorStr, out valor);
                    dic.Remove(colorStr);
                    dic.Add(colorStr, valor + 1);
                }
                else
                {
                    dic.Add(colorStr, 1);
                }
            }

            //A continuación se inicia el proceso de selección de color de nube sobre la cual
            //se va a calcular la funciónd de aproximación. Si sólo existe una nube y un color,
            //el programa se saltará este paso
            if (dic.Keys.Count > 1)
            {
                Calcular cal = new Calcular(dic);
                cal.DialogResult = DialogResult.No;

                while (cal.DialogResult == DialogResult.No)
                {
                    cal.ShowDialog(this);
                    if (cal.DialogResult == DialogResult.OK)
                    {
                        if (cal.color == "Ninguno")
                        {
                            MessageBox.Show(Cadenas.error006);
                            cal.DialogResult = DialogResult.No;
                        }
                        else
                        {
                            colorStr = cal.color;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }

            switch (colorStr)
            {
                case "Negro":
                case "Black":
                case "Preto":
                    colorPunto = Color.Black;
                    break;
                case "Azul":
                case "Blue":
                    colorPunto = Color.Blue;
                    break;
                case "Rojo":
                case "Red":
                case "Vermelho":
                    colorPunto = Color.Red;
                    break;
                case "Verde":
                case "Green":
                    colorPunto = Color.Green;
                    break;
                case "Marrón":
                case "Brown":
                case "Marrom":
                    colorPunto = Color.Brown;
                    break;
                case "Violeta":
                case "Violet":
                    colorPunto = Color.Violet;
                    break;
                case "Naranja":
                case "Orange":
                case "Laranja":
                    colorPunto = Color.Orange;
                    break;
                case "Gris":
                case "Gray":
                case "Cinza":
                    colorPunto = Color.Gray;
                    break;
                case "Azul oscuro":
                case "Dark blue":
                case "Azul escuro":
                    colorPunto = Color.DarkBlue;
                    break;
                case "Magenta":
                    colorPunto = Color.Magenta;
                    break;
                default:
                    colorPunto = Color.Black;
                    break;
            }

            //Preparación de los array's ordenados de coordenadas X y coordenadas Y
            //para la implementación de la aproximación
            SortedList<double, double> listaPuntosOrdenada = new SortedList<double, double>();
            foreach (Punto punto in listadoPuntos)
            {
                try
                {
                    if ((punto.devuelve_mostrar() == true) && (punto.devuelve_Color() == colorPunto))
                    {
                        listaPuntosOrdenada.Add(punto.coordenadaX(), punto.coordenadaY());
                    }
                }
                catch (ArgumentException)
                {
                    MessageBox.Show(Cadenas.error007);
                    return;
                }
            }

            //Se comprueba que hayan dos o más puntos del color seleccionado
            if (listaPuntosOrdenada.Count <= 1)
            {
                MessageBox.Show(Cadenas.error008);
                return;
            }

            x = new double[listaPuntosOrdenada.Count];
            IList<double> listadoX = listaPuntosOrdenada.Keys;
            foreach (double temp in listadoX)
            {
                x[index] = temp;
                index++;
            }

            index = 0;
            y = new double[listaPuntosOrdenada.Count];
            IList<double> listadoY = listaPuntosOrdenada.Values;
            foreach (double temp in listadoY)
            {
                y[index] = temp;
                index++;
            }

            //Implementación de la aproximación dependiendo del tipo
            switch (tipoAprox.SelectedItem.ToString())
            {
                case "Regresión lineal":
                case "Lineal regression":
                case "Regressão linear":
                    RegresionEstandar regresionLineal = new RegresionEstandar(x, y, "Lineal");
                    regresionLineal.resolver();
                    ecuacionFinal = regresionLineal.a + " + " + "x * " + regresionLineal.b;
                    ecuacionFinal = ecuacionFinal.Replace(',', '.');
                    break;

                case "Regresión exponencial":
                case "Exponential regression":
                case "Regressão exponencial":
                    for (i = 0; i < x.Count(); i++)
                    {
                        if (y[i] <= 0)
                        {
                            MessageBox.Show(Cadenas.error009);
                            return;
                        }
                    }
                    RegresionEstandar regresionExponencial = new RegresionEstandar(x, y, "Exponencial");
                    regresionExponencial.resolver();
                    String aExp, bExp;
                    aExp = regresionExponencial.a.ToString();
                    aExp = aExp.Replace(',', '.');
                    bExp = regresionExponencial.b.ToString();
                    bExp = bExp.Replace(',', '.');
                    ecuacionFinal = aExp + " * " + "Math.Pow(" + bExp + ", x)";
                    break;

                case "Regresión logarítmica":
                case "Logarithmic regression":
                case "Regressão logarítmica":
                    for (i = 0; i < x.Count(); i++)
                    {
                        if (x[i] <= 0)
                        {
                            MessageBox.Show(Cadenas.error010);
                            return;
                        }
                    }

                    RegresionEstandar regresionLogaritmica = new RegresionEstandar(x, y, "Logaritmica");
                    regresionLogaritmica.resolver();
                    String aLog, bLog;
                    aLog = regresionLogaritmica.a.ToString();
                    aLog = aLog.Replace(',', '.');
                    bLog = regresionLogaritmica.b.ToString();
                    bLog = bLog.Replace(',', '.');
                    ecuacionFinal = bLog + " * " + "Math.Log(x) + (" + aLog + ")";
                    break;

                case "Regresión polinómica":
                case "Polynomial regression":
                case "Regressão polinomial":
                    Parametros coeficiente = new Parametros("Coeficiente");
                    coeficiente.DialogResult = DialogResult.No;
                    
                    while (coeficiente.DialogResult == DialogResult.No)
                    {
                        if (coeficiente.ShowDialog(this) == DialogResult.OK)
                        {
                            if (coeficiente.devolverParametro() <= 1)
                            {
                                MessageBox.Show(Cadenas.error011);
                                coeficiente.DialogResult = DialogResult.No;
                            }
                            else
                            {
                                if (coeficiente.devolverParametro().ToString().Contains(','))
                                {
                                    MessageBox.Show(Cadenas.error011);
                                    coeficiente.DialogResult = DialogResult.No;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(Cadenas.error011);
                            coeficiente.DialogResult = DialogResult.No;                            
                        }
                    }

                    MathNet.Numerics.LinearAlgebra.Vector vectorX = new MathNet.Numerics.LinearAlgebra.Vector(x);
                    MathNet.Numerics.LinearAlgebra.Vector vectorY = new MathNet.Numerics.LinearAlgebra.Vector(y);
                    RegresionPolinomica regresionPolinomial = new RegresionPolinomica(vectorX, vectorY, (int)Math.Truncate(coeficiente.devolverParametro()));
                    MathNet.Numerics.LinearAlgebra.Vector coeficientes = regresionPolinomial.Coefficients;
                    String[] coef = new String[coeficientes.Length];
                    for (i = 0; i < coeficientes.Length; i++)
                    {
                        coef[i] = coeficientes[i].ToString().Replace(',', '.');
                    }

                    for (i = coeficientes.Length - 1; i >= 0; i--)
                    {
                        if (i == 0)
                        {
                            ecuacionFinal = ecuacionFinal + "(" + coef[0] + ")";
                        }
                        else
                        {
                            ecuacionFinal = ecuacionFinal + "(" + coef[i] + " * Math.Pow(x, " + i + ")) + ";
                        }
                    }
                    break;

                case "Interpolación polinómica":
                case "Polynomial interpolation":
                case "Interpolação polinomial":
                    InterpolacionPolinomica diferenciasDivididas = new InterpolacionPolinomica(x, y);
                    ecuacionFinal = diferenciasDivididas.resolver();
                    ecuacionFinal = ecuacionFinal.Replace(',', '.');
                    break;
            }

            //Se devuelve el resultado de la ecuación final a la caja de texto
            textoFuncion.Text = ecuacionFinal;

            if (ecuacionFinal.Length > textoFuncion.MaxLength)
            {
                insertarFuncion_Click(sender, e);
            }
        }

        private void colorDeFuncion_Click(object sender, ToolStripItemClickedEventArgs e)
        {
            String strFuncion = item.Text.Substring(7);
            int indice = -1;

            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_funcion().CompareTo(strFuncion) == 0)
                {
                    indice = listadoFunciones.IndexOf(f);
                }
            }

            if (indice != -1)
            {
                String colorStr = e.ClickedItem.ToString();
                Color c;

                if (colorStr.CompareTo("Negro") == 0)
                {
                    c = Color.Black;
                }
                else if (colorStr.CompareTo("Azul") == 0)
                {
                    c = Color.Blue;
                }
                else if (colorStr.CompareTo("Rojo") == 0)
                {
                    c = Color.Red;
                }
                else if (colorStr.CompareTo("Verde") == 0)
                {
                    c = Color.Green;
                }
                else if (colorStr.CompareTo("Marrón") == 0)
                {
                    c = Color.Brown;
                }
                else if (colorStr.CompareTo("Violeta") == 0)
                {
                    c = Color.Violet;
                }
                else if (colorStr.CompareTo("Naranja") == 0)
                {
                    c = Color.Orange;
                }
                else if (colorStr.CompareTo("Gris") == 0)
                {
                    c = Color.Gray;
                }
                else if (colorStr.CompareTo("Azul oscuro") == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (colorStr.CompareTo("Magenta") == 0)
                {
                    c = Color.Magenta;
                }
                else if (colorStr.CompareTo("Black") == 0)
                {
                    c = Color.Black;
                }
                else if (colorStr.CompareTo("Blue") == 0)
                {
                    c = Color.Blue;
                }
                else if (colorStr.CompareTo("Red") == 0)
                {
                    c = Color.Red;
                }
                else if (colorStr.CompareTo("Green") == 0)
                {
                    c = Color.Green;
                }
                else if (colorStr.CompareTo("Brown") == 0)
                {
                    c = Color.Brown;
                }
                else if (colorStr.CompareTo("Violet") == 0)
                {
                    c = Color.Violet;
                }
                else if (colorStr.CompareTo("Orange") == 0)
                {
                    c = Color.Orange;
                }
                else if (colorStr.CompareTo("Gray") == 0)
                {
                    c = Color.Gray;
                }
                else if (colorStr.CompareTo("Dark blue") == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (colorStr.CompareTo("Magenta") == 0)
                {
                    c = Color.Magenta;
                }
                else if (colorStr.CompareTo("Preto") == 0)
                {
                    c = Color.Black;
                }
                else if (colorStr.CompareTo("Azul") == 0)
                {
                    c = Color.Blue;
                }
                else if (colorStr.CompareTo("Vermelho") == 0)
                {
                    c = Color.Red;
                }
                else if (colorStr.CompareTo("Verde") == 0)
                {
                    c = Color.Green;
                }
                else if (colorStr.CompareTo("Marrom") == 0)
                {
                    c = Color.Brown;
                }
                else if (colorStr.CompareTo("Violeta") == 0)
                {
                    c = Color.Violet;
                }
                else if (colorStr.CompareTo("Laranja") == 0)
                {
                    c = Color.Orange;
                }
                else if (colorStr.CompareTo("Cinza") == 0)
                {
                    c = Color.Gray;
                }
                else if (colorStr.CompareTo("Azul escuro") == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (colorStr.CompareTo("Magenta") == 0)
                {
                    c = Color.Magenta;
                }
                else
                {
                    c = Color.Black;
                }

                listadoFunciones[indice].cambiarColor(c);
                int i = historialFunciones.Items.IndexOf(item);
                historialFunciones.Items[i].SubItems[2].Text = devuelveColorStr(c.Name);
            }

            Graphics gr = panelInt.CreateGraphics();
            pintarEje(gr);
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    pintarFuncion(f, gr);
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }
        }

        private void colorDePunto_Click(object sender, ToolStripItemClickedEventArgs e)
        {
            int i = historialPuntos.Items.IndexOf(item);
            String colorStr = e.ClickedItem.ToString();
            Color c = Color.Black;

            if (i != -1)
            {
                if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Negro") == 0)
                {
                    c = Color.Black;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Azul") == 0)
                {
                    c = Color.Blue;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Rojo") == 0)
                {
                    c = Color.Red;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Verde") == 0)
                {
                    c = Color.Green;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Marrón") == 0)
                {
                    c = Color.Brown;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Violeta") == 0)
                {
                    c = Color.Violet;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Naranja") == 0)
                {
                    c = Color.Orange;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Gris") == 0)
                {
                    c = Color.Gray;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Azul oscuro") == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Magenta") == 0)
                {
                    c = Color.Magenta;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Black") == 0)
                {
                    c = Color.Black;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Blue") == 0)
                {
                    c = Color.Blue;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Red") == 0)
                {
                    c = Color.Red;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Green") == 0)
                {
                    c = Color.Green;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Brown") == 0)
                {
                    c = Color.Brown;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Violet") == 0)
                {
                    c = Color.Violet;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Orange") == 0)
                {
                    c = Color.Orange;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Gray") == 0)
                {
                    c = Color.Gray;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Dark blue") == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Magenta") == 0)
                {
                    c = Color.Magenta;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Preto") == 0)
                {
                    c = Color.Black;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Azul") == 0)
                {
                    c = Color.Blue;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Vermelho") == 0)
                {
                    c = Color.Red;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Verde") == 0)
                {
                    c = Color.Green;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Brown") == 0)
                {
                    c = Color.Brown;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Violeta") == 0)
                {
                    c = Color.Violet;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Laranja") == 0)
                {
                    c = Color.Orange;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Cinza") == 0)
                {
                    c = Color.Gray;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Azul escuro") == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo("Magenta") == 0)
                {
                    c = Color.Magenta;
                }
                else
                {
                    c = Color.Black;
                }
            }
                        
            int indice = -1;
            String[] strPunto = item.ToString().Split(' ', '(', ')', ';');
            Punto punto = new Punto(double.Parse(strPunto[2]), double.Parse(strPunto[4]), true, c);

            foreach (Punto p in listadoPuntos)
            {
                if ((p.coordenadaX() == punto.coordenadaX()) && (p.coordenadaY() == punto.coordenadaY()) && (p.devuelve_Color() == punto.devuelve_Color()))
                {
                    indice = listadoPuntos.IndexOf(p);
                }
            }

            if (indice != -1)
            {
                colorStr = e.ClickedItem.ToString();

                if (colorStr.CompareTo("Negro") == 0)
                {
                    c = Color.Black;
                }
                else if (colorStr.CompareTo("Azul") == 0)
                {
                    c = Color.Blue;
                }
                else if (colorStr.CompareTo("Rojo") == 0)
                {
                    c = Color.Red;
                }
                else if (colorStr.CompareTo("Verde") == 0)
                {
                    c = Color.Green;
                }
                else if (colorStr.CompareTo("Marrón") == 0)
                {
                    c = Color.Brown;
                }
                else if (colorStr.CompareTo("Violeta") == 0)
                {
                    c = Color.Violet;
                }
                else if (colorStr.CompareTo("Naranja") == 0)
                {
                    c = Color.Orange;
                }
                else if (colorStr.CompareTo("Gris") == 0)
                {
                    c = Color.Gray;
                }
                else if (colorStr.CompareTo("Azul oscuro") == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (colorStr.CompareTo("Magenta") == 0)
                {
                    c = Color.Magenta;
                }
                else if (colorStr.CompareTo("Black") == 0)
                {
                    c = Color.Black;
                }
                else if (colorStr.CompareTo("Blue") == 0)
                {
                    c = Color.Blue;
                }
                else if (colorStr.CompareTo("Red") == 0)
                {
                    c = Color.Red;
                }
                else if (colorStr.CompareTo("Green") == 0)
                {
                    c = Color.Green;
                }
                else if (colorStr.CompareTo("Brown") == 0)
                {
                    c = Color.Brown;
                }
                else if (colorStr.CompareTo("Violet") == 0)
                {
                    c = Color.Violet;
                }
                else if (colorStr.CompareTo("Orange") == 0)
                {
                    c = Color.Orange;
                }
                else if (colorStr.CompareTo("Gray") == 0)
                {
                    c = Color.Gray;
                }
                else if (colorStr.CompareTo("Dark blue") == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (colorStr.CompareTo("Magenta") == 0)
                {
                    c = Color.Magenta;
                }
                else if (colorStr.CompareTo("Preto") == 0)
                {
                    c = Color.Black;
                }
                else if (colorStr.CompareTo("Azul") == 0)
                {
                    c = Color.Blue;
                }
                else if (colorStr.CompareTo("Vermelho") == 0)
                {
                    c = Color.Red;
                }
                else if (colorStr.CompareTo("Verde") == 0)
                {
                    c = Color.Green;
                }
                else if (colorStr.CompareTo("Brown") == 0)
                {
                    c = Color.Brown;
                }
                else if (colorStr.CompareTo("Violeta") == 0)
                {
                    c = Color.Violet;
                }
                else if (colorStr.CompareTo("Laranja") == 0)
                {
                    c = Color.Orange;
                }
                else if (colorStr.CompareTo("Cinza") == 0)
                {
                    c = Color.Gray;
                }
                else if (colorStr.CompareTo("Azul escuro") == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (colorStr.CompareTo("Magenta") == 0)
                {
                    c = Color.Magenta;
                }
                else
                {
                    c = Color.Black;
                }

                Punto aux = new Punto(listadoPuntos[indice].coordenadaX(), listadoPuntos[indice].coordenadaY(), true, c);
                
                if (existe_punto(aux))
                {
                    MessageBox.Show(Cadenas.error012);
                    return;
                }

                listadoPuntos[indice].cambiarColor(c);
                historialPuntos.Items[i].SubItems[1].Text = devuelveColorStr(c.Name);
            }

            Graphics gr = panelInt.CreateGraphics();
            pintarEje(gr);
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    pintarFuncion(f, gr);
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }
        }

        private double pasar_coordenada_realX(double i)
        {
            return (((((anchuraTeorica / 2) - ((x_fin + x_inicio) / 2)) + i) / anchuraTeorica) * anchuraPanelInt);
        }

        private double pasar_coordenada_realY(double j)
        {
            return (((((alturaTeorica / 2) + ((y_fin + y_inicio) / 2)) - j) / alturaTeorica) * alturaPanelInt);
        }

        private double pasar_coordenada_teoricaX(double i)
        {
            return (((i / anchuraPanelInt) * anchuraTeorica) - (anchuraTeorica / 2) + ((x_inicio + x_fin) / 2));
        }

        private double pasar_coordenada_teoricaY(double j)
        {
            return (((j / alturaPanelInt) * alturaTeorica) - (alturaTeorica / 2) - ((y_inicio + y_fin) / 2));
        }

        private bool existe_funcion(String func)
        {
            bool esta = false;
            foreach (Funcion f in listadoFunciones)
            {
                if ((String.Compare(f.devuelve_funcion(), func) == 0))
                {
                    esta = true;
                }
            }
            return esta;
        }

        private bool existe_punto(Punto p)
        {
            bool esta = false;
            foreach (Punto aux in listadoPuntos)
            {
                if ((p.coordenadaX() == aux.coordenadaX()) && (p.coordenadaY() == aux.coordenadaY()) && (p.devuelve_Color() == aux.devuelve_Color()))
                {
                    esta = true;
                }
            }
            return esta;
        }

        private void buttonDerecha_Click(object sender, EventArgs e)
        {
            x_inicio = x_inicio + desplazamiento;
            x_fin = x_fin + desplazamiento;

            if (primera && listadoFunciones.Count > 0)
            {
                primera = false;
                return;
            }

            pintarReglaYEje();
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);
                }
            }
            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }
        }

        private void buttonIzquierda_Click(object sender, EventArgs e)
        {
            x_inicio = x_inicio - desplazamiento;
            x_fin = x_fin - desplazamiento;

            if (primera && listadoFunciones.Count > 0)
            {
                primera = false;
                return;
            }

            pintarReglaYEje();
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);
                }
            }
            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }
        }

        private void buttonArriba_Click(object sender, EventArgs e)
        {
            y_inicio = y_inicio + desplazamiento;
            y_fin = y_fin + desplazamiento;

            if (primera && listadoFunciones.Count > 0)
            {
                primera = false;
                return;
            }

            pintarReglaYEje();
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);
                }
            }
            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }
        }

        private void buttonAbajo_Click(object sender, EventArgs e)
        {
            y_inicio = y_inicio - desplazamiento;
            y_fin = y_fin - desplazamiento;

            if (primera && listadoFunciones.Count > 0)
            {
                primera = false;
                return;
            }

            pintarReglaYEje();
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);
                }
            }
            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }
        }

        private void borrarFunciones_Click(object sender, EventArgs e)
        {
            ficheroFunciones = " ";
            historialFunciones.Items.Clear();
            listadoFunciones = new List<Funcion>();
            Graphics gr = panelInt.CreateGraphics();
            pintarEje(gr);
            textoFuncion.Text = "";

            foreach (Punto p in listadoPuntos)
            {
                pintarPunto(p);
            }

            borrarPrimeraFuncion.Enabled = false;
            borrarUltimaFuncion.Enabled = false;

            int i = 0;
            while (i < funcionesMenu.DropDownItems.Count)
            {
                if (i == 3)
                {
                    funcionesMenu.DropDownItems.RemoveAt(3);
                }
                else
                {
                    i++;
                }
            }

            string noHayFunciones = Cadenas.noHayFuncionesEnElHistorial;
            funcionesMenu.DropDownItems.Add(noHayFunciones);
            funcionesMenu.DropDownItems[3].Enabled = false;
        }

        private void borrarPuntos_Click(object sender, EventArgs e)
        {
            ficheroPuntos = " ";
            historialPuntos.Items.Clear();
            listadoPuntos = new List<Punto>();
            Graphics gr = panelInt.CreateGraphics();
            pintarEje(gr);

            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    pintarFuncion(f, gr);
                }
            }

            quitarPrimerPuntoAñadido.Enabled = false;
            quitarUltimoPuntoAñadido.Enabled = false;

            int i = 0;
            while (i < puntosMenu.DropDownItems.Count)
            {
                if (i == 3)
                {
                    puntosMenu.DropDownItems.RemoveAt(3);
                }
                else
                {
                    i++;
                }
            }

            string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;
            puntosMenu.DropDownItems.Add(noHayPuntos);
            puntosMenu.DropDownItems[3].Enabled = false;
        }

        private void abrirHistorialFunciones_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = Cadenas.historialFunciones;
            openFileDialog.Title = Cadenas.seleccionaUnHistorialDeFunciones;
            
            string titulo = Cadenas.cargarHistorialDeFunciones;
            string advertencia = Cadenas.advertenciaCargarHistorialDeFunciones;
            
            if (historialFunciones.Items.Count > 0)
            {
                if (MessageBox.Show(advertencia, titulo, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            bool ok;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ficheroFunciones = openFileDialog.FileName;
                System.IO.StreamReader sr = new System.IO.StreamReader(ficheroFunciones, System.Text.Encoding.Default, true);
                string s;

                //Si se trata de un fichero no vacio, inicializo antes los listados de funciones
                if (sr.Peek() != -1)
                {
                    // Leer una línea del ficheroFunciones
                    s = sr.ReadLine();
                    Graphics gr = panelInt.CreateGraphics();

                    // En caso de tratarse de un fichero de funciones, se borran las funciones previas
                    historialFunciones.Items.Clear();
                    ficheroFunciones = " ";
                    historialFunciones.Items.Clear();
                    listadoFunciones = new List<Funcion>();
                    textoFuncion.Text = "";
                    
                    pintarEje(gr);
                    
                    foreach (Punto p in listadoPuntos)
                    {
                        if (p.devuelve_mostrar() == true)
                        {
                            pintarPunto(p);
                        }
                    }

                    borrarPrimeraFuncion.Enabled = true;
                    borrarUltimaFuncion.Enabled = true;

                    int i = 0;
                    while (i < funcionesMenu.DropDownItems.Count)
                    {
                        string noHayFunciones = Cadenas.noHayFuncionesEnElHistorial;

                        if ((i == 3) && (funcionesMenu.DropDownItems[3].ToString().CompareTo(noHayFunciones) == 0))
                        {
                            funcionesMenu.DropDownItems.RemoveAt(3);
                        }
                        else
                        {
                            i++;
                        }
                    }

                    borrar_funciones_previas();

                    int total = 0, linea = 1;
                    while (String.IsNullOrEmpty(s) == false)
                    {
                        // Si no está vacía, añadirla al control
                        // Si está vacía, continuar el bucle
                        if (String.IsNullOrEmpty(s))
                        {
                            continue;
                        }

                        // Comprobamos si corresponde con un fichero de funciones
                        if (s.Substring(0, 7).CompareTo("f(x) = ") != 0)
                        {
                            MessageBox.Show(Cadenas.error013a + " " + linea + " " + Cadenas.error013b);
                        }
                        else
                        {
                            try
                            {
                                if (existe_funcion(s.Substring(7)))
                                {
                                    MessageBox.Show(Cadenas.error022a + s.Substring(7) + Cadenas.error022b);
                                }
                                else
                                {
                                    ok = genera_funcion(s.Substring(7));
                                    if (ok)
                                    {
                                        funcionesMenu.DropDownItems.Add(s.Substring(7));
                                        ListViewItem lvi = new ListViewItem("f(x) = " + s.Substring(7));
                                        string traduccion = traducirFuncion(s.Substring(7));
                                        lvi.SubItems.Add("f(x) = " + traduccion);
                                        lvi.SubItems.Add(devuelveColorStr(obtenerColor().Name));
                                        historialFunciones.Items.Add(lvi);
                                        borrarPrimeraFuncion.Enabled = true;
                                        borrarUltimaFuncion.Enabled = true;
                                        Color color = obtenerColor();
                                        Funcion f = new Funcion(s.Substring(7), idFuncion, color, true);
                                        listadoFunciones.Add(f);
                                        pintarFuncion(f, gr);
                                        idFuncion++;
                                        total++;
                                    }
                                    else
                                    {
                                        MessageBox.Show(Cadenas.error013a + " " + linea + " " + Cadenas.error013b);
                                    }
                                }                                
                            }
                            catch (Exception msg)
                            {
                                MessageBox.Show(Cadenas.error013a + " " + linea + " " + Cadenas.error013b);
                            }
                        }

                        // Leer una línea del ficheroFunciones
                        s = sr.ReadLine();
                        linea++;
                    }

                    //Si no se ha cargado ninguna funcion, se vuelve a añadir el mensaje
                    //en el menú de funciones "No hay funciones en el historial"
                    if (funcionesMenu.DropDownItems.Count == 3)
                    {
                        funcionesMenu.DropDownItems.Add(Cadenas.noHayFuncionesEnElHistorial);
                        funcionesMenu.DropDownItems[0].Enabled = false;
                        funcionesMenu.DropDownItems[1].Enabled = false;
                        funcionesMenu.DropDownItems[3].Enabled = false;
                    }

                    estadoLabel.Text = Cadenas.seHanCargadoConExito + " " + total + " " + Cadenas.funciones;
                }
                sr.Close();
            }
        }

        private void abrirHistorialPuntos_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = Cadenas.historialPuntos;
            openFileDialog.Title = Cadenas.seleccionaUnHistorialDePuntos;

            string titulo = Cadenas.cargarHistorialDePuntos;
            string advertencia = Cadenas.advertenciaCargarHistorialDePuntos;

            if (historialPuntos.Items.Count > 0)
            {
                if (MessageBox.Show(advertencia, titulo, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ficheroPuntos = openFileDialog.FileName;
                System.IO.StreamReader sr = new System.IO.StreamReader(ficheroPuntos, System.Text.Encoding.Default, true);
                string s;

                //Si se trata de un fichero no vacio, inicializo antes los listados de puntos
                if (sr.Peek() != -1)
                {
                    // Leer una línea del ficheroFunciones
                    s = sr.ReadLine();
                    Graphics gr = panelInt.CreateGraphics();

                    // En caso de tratarse de un fichero de puntos, se borran los puntos previos
                    ficheroPuntos = " ";
                    historialPuntos.Items.Clear();
                    listadoPuntos = new List<Punto>();
                    pintarEje(gr);
                    textoFuncion.Text = "";
                    cajaX.Text = "";
                    cajaY.Text = "";

                    foreach (Funcion f in listadoFunciones)
                    {
                        if (f.devuelve_mostrar() == true)
                        {
                            pintarFuncion(f, gr);
                        }
                    }

                    quitarPrimerPuntoAñadido.Enabled = true;
                    quitarUltimoPuntoAñadido.Enabled = true;

                    int i = 0;
                    while (i < puntosMenu.DropDownItems.Count)
                    {
                        string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                        if ((i == 3) && (puntosMenu.DropDownItems[3].ToString().CompareTo(noHayPuntos) == 0))
                        {
                            puntosMenu.DropDownItems.RemoveAt(3);
                        }
                        else
                        {
                            i++;
                        }
                    }

                    // Leer el contenido mientras no se llegue al final
                    int total = 0, linea = 1;
                    while(String.IsNullOrEmpty(s) == false)
                    {
                        // Comprobamos si corresponde con un fichero de puntos
                        if (s.Substring(0, 1).CompareTo("(") != 0)
                        {
                            MessageBox.Show(Cadenas.error013a + " " + linea + " " + Cadenas.error014);
                        }
                        else
                        {
                            try
                            {
                                String[] strPuntoArray = s.Split('(', ')', ';');
                                Punto punto = new Punto(double.Parse(strPuntoArray[1]), double.Parse(strPuntoArray[2]), true, obtenerColor());
                                ListViewItem lvi = new ListViewItem("(" + String.Format("{0:0.00}", punto.coordenadaX()) + "; " + String.Format("{0:0.00}", punto.coordenadaY()) + ")");
                                lvi.SubItems.Add(devuelveColorStr(obtenerColor().Name));
                                historialPuntos.Items.Add(lvi);
                                puntosMenu.DropDownItems.Add("(" + String.Format("{0:0.00}", punto.coordenadaX()) + "; " + String.Format("{0:0.00}", punto.coordenadaY()) + ")");
                                listadoPuntos.Add(punto);
                                pintarPunto(punto);
                                total++;
                            }
                            catch (Exception msg)
                            {
                                MessageBox.Show(Cadenas.error013a + " " + linea + " " + Cadenas.error014);
                            }
                        }

                        // Leer una línea del fichero puntos
                        s = sr.ReadLine();
                        linea++;
                    }

                    estadoLabel.Text = Cadenas.seHanCargadoConExito + " " + total + " " + Cadenas.puntosMinuscula;

                    //Si no se ha cargado ningún punto, se vuelve a añadir el mensaje
                    //en el menú de puntos "No hay puntos en el historial"
                    if (puntosMenu.DropDownItems.Count == 3)
                    {
                        puntosMenu.DropDownItems.Add(Cadenas.noHayPuntosEnElHistorial);
                        puntosMenu.DropDownItems[0].Enabled = false;
                        puntosMenu.DropDownItems[1].Enabled = false;
                        puntosMenu.DropDownItems[3].Enabled = false;
                    }
                }
                sr.Close();
            }
        }

        private void guardarFunciones_Click(object sender, EventArgs e)
        {
            if (listadoFunciones.Count == 0)
            {
                MessageBox.Show(Cadenas.error015);
                return;
            }

            if (ficheroFunciones == " ")
            {
                SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
                SaveFileDialog1.Filter = Cadenas.historialFunciones;
                SaveFileDialog1.Title = Cadenas.seleccionaUnHistorialDeFunciones;
                
                if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string s;
                    ficheroFunciones = SaveFileDialog1.FileName;

                    System.IO.StreamWriter sw =
                        new System.IO.StreamWriter(ficheroFunciones, true, System.Text.Encoding.Default);

                    for (int i = 0; i <= historialFunciones.Items.Count - 1; i++)
                    {
                        s = this.historialFunciones.Items[i].Text;
                        sw.WriteLine(s);
                    }
                    sw.Close();
                    estadoLabel.Text = Cadenas.historialDeFuncionesGuardadoCorrectamente;
                }
            }
            else
            {
                System.IO.File.Delete(ficheroFunciones);

                string s;
                System.IO.StreamWriter sw =
                    new System.IO.StreamWriter(ficheroFunciones, true, System.Text.Encoding.Default);

                for (int i = 0; i <= historialFunciones.Items.Count - 1; i++)
                {
                    s = this.historialFunciones.Items[i].Text;
                    sw.WriteLine(s);
                }
                sw.Close();
                estadoLabel.Text = Cadenas.historialDeFuncionesGuardadoCorrectamente;
            }
        }

        private void guardarFuncionesComo_Click(object sender, EventArgs e)
        {
            if (listadoFunciones.Count == 0)
            {
                MessageBox.Show(Cadenas.error015);
                return;
            }

            SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
            SaveFileDialog1.Filter = Cadenas.historialFunciones;
            SaveFileDialog1.Title = Cadenas.seleccionaUnHistorialDeFunciones;

            if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string s;
                ficheroFunciones = SaveFileDialog1.FileName;

                System.IO.StreamWriter sw =
                    new System.IO.StreamWriter(ficheroFunciones, true, System.Text.Encoding.Default);

                for (int i = 0; i <= historialFunciones.Items.Count - 1; i++)
                {
                    s = this.historialFunciones.Items[i].Text;
                    sw.WriteLine(s);
                }
                sw.Close();
                estadoLabel.Text = Cadenas.historialDeFuncionesGuardadoCorrectamente;
            }
        }

        private void guardarFuncionesEnMenpas_Click(object sender, EventArgs e)
        {
            if (listadoFunciones.Count == 0)
            {
                MessageBox.Show(Cadenas.error016);
                return;
            }

            int error = 0;
            string ahora = DateTime.Now.ToString();
            Proyecto.MenPas.WS_EstimacionF WS_EstimacionF = new Proyecto.MenPas.WS_EstimacionF();


            for (int i = 0; i <= historialFunciones.Items.Count - 1; i++)
            {
                string funcion = this.historialFunciones.Items[i].Text.Substring(7);

                try
                {
                    if (WS_EstimacionF.InsertarFuncion(usuario, funcion, ahora) == false)
                    {
                        error = i + 1;
                        i = historialFunciones.Items.Count;
                    }
                }
                catch (System.Net.WebException msg)
                {
                    MessageBox.Show(Cadenas.error017);
                    return;
                }
            }

            if (error == 0)
            {
                MessageBox.Show(Cadenas.funcionesGrabadasCorrectamente);
            }
            else
            {
                MessageBox.Show(Cadenas.error018 + ": " + error);
            }
        }

        private void guardarPuntosEnMenPas_Click(object sender, EventArgs e)
        {
            if (listadoPuntos.Count == 0)
            {
                MessageBox.Show(Cadenas.error019);
                return;
            }

            int error = 0;
            string ahora = DateTime.Now.ToString();
            Proyecto.MenPas.WS_EstimacionF WS_EstimacionF = new Proyecto.MenPas.WS_EstimacionF();
            
            for (int i = 0; i <= historialPuntos.Items.Count - 1; i++)
            {
                String[] strPunto = historialPuntos.Items[i].Text.Split(' ', '(', ')', ';');
                Punto punto = new Punto(double.Parse(strPunto[1]), double.Parse(strPunto[3]), true, Color.Black);

                try
                {
                    if (WS_EstimacionF.InsertarPunto(usuario, punto.coordenadaX(), punto.coordenadaY(), ahora) == false)
                    {
                        error = i + 1;
                        i = historialPuntos.Items.Count;
                    }
                }
                catch (System.Net.WebException msg)
                {
                    MessageBox.Show(Cadenas.error017);
                    return;
                }
            }

            if (error == 0)
            {
                MessageBox.Show(Cadenas.puntosGrabadosCorrectamente);
            }
            else
            {
                MessageBox.Show(Cadenas.error020 + ": " + error);
            }
        }

        private void guardarPuntos_Click(object sender, EventArgs e)
        {
            if (listadoPuntos.Count == 0)
            {
                MessageBox.Show(Cadenas.error021);
                return;
            }

            if (ficheroPuntos == " ")
            {
                SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
                SaveFileDialog1.Filter = Cadenas.historialPuntos;
                SaveFileDialog1.Title = Cadenas.seleccionaUnHistorialDePuntos;
                
                if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    ficheroPuntos = SaveFileDialog1.FileName;

                    System.IO.StreamWriter sw =
                        new System.IO.StreamWriter(ficheroPuntos, true, System.Text.Encoding.Default);

                    for (int i = 0; i <= historialPuntos.Items.Count - 1; i++)
                    {
                        sw.WriteLine(this.historialPuntos.Items[i].Text);
                    }
                    sw.Close();

                    MessageBox.Show(Cadenas.historialDePuntosGuardadoCorrectamente);
                }
            }
            else
            {
                System.IO.File.Delete(ficheroPuntos);
                System.IO.StreamWriter sw =
                    new System.IO.StreamWriter(ficheroPuntos, true, System.Text.Encoding.Default);

                for (int i = 0; i <= historialPuntos.Items.Count - 1; i++)
                {
                    sw.WriteLine(this.historialPuntos.Items[i].Text);
                }
                sw.Close();
                MessageBox.Show(Cadenas.historialDePuntosGuardadoCorrectamente);
            }
        }

        private void guardarPuntosComo_Click(object sender, EventArgs e)
        {
            if (listadoPuntos.Count == 0)
            {
                MessageBox.Show(Cadenas.error021);
                return;
            }

            SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
            SaveFileDialog1.Filter = Cadenas.historialPuntos;
            SaveFileDialog1.Title = Cadenas.seleccionaUnHistorialDePuntos;

            if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ficheroPuntos = SaveFileDialog1.FileName;

                System.IO.StreamWriter sw =
                    new System.IO.StreamWriter(ficheroPuntos, true, System.Text.Encoding.Default);

                for (int i = 0; i <= historialPuntos.Items.Count - 1; i++)
                {
                    sw.WriteLine(this.historialPuntos.Items[i].Text);
                }
                sw.Close();
                MessageBox.Show(Cadenas.historialDePuntosGuardadoCorrectamente);
            }
        }

        private void cargarFuncionesEnMenPas_Click(object sender, EventArgs e)
        {
            FuncionesMenpas cfem = new FuncionesMenpas("cargar");
            cfem.DialogResult = DialogResult.No;
            cfem.ShowDialog();

            if (cfem.DialogResult == DialogResult.OK)
            {
                DataGridView dgv = cfem.dgv;
                int total = 0;

                for (int i = 0; i < dgv.RowCount; i++)
                {
                    if (dgv.Rows[i].Selected == true)
                    {
                        string strFuncion = dgv.Rows[i].Cells[1].FormattedValue.ToString();

                        if (existe_funcion(strFuncion))
                        {
                            MessageBox.Show(Cadenas.error022a + " " + strFuncion + " " + Cadenas.error022b);
                        }
                        else
                        {
                            try
                            {
                                bool ok = genera_funcion(strFuncion);
                                if (ok)
                                {
                                    if (historialFunciones.Items.Count == 0)
                                    {
                                        funcionesMenu.DropDownItems.Remove(funcionesMenu.DropDownItems[3]);
                                    }

                                    ListViewItem lvi = new ListViewItem("f(x) = " + dgv.Rows[i].Cells[1].FormattedValue.ToString());
                                    string traduccion = traducirFuncion(dgv.Rows[i].Cells[1].FormattedValue.ToString());
                                    lvi.SubItems.Add("f(x) = " + traduccion);
                                    lvi.SubItems.Add(devuelveColorStr(obtenerColor().Name));
                                    historialFunciones.Items.Add(lvi);
                                    funcionesMenu.DropDownItems.Add(dgv.Rows[i].Cells[1].FormattedValue.ToString());
                                    borrarPrimeraFuncion.Enabled = true;
                                    borrarUltimaFuncion.Enabled = true;
                                    Color color = obtenerColor();
                                    Funcion aux = new Funcion(dgv.Rows[i].Cells[1].FormattedValue.ToString(), idFuncion, color, true);
                                    listadoFunciones.Add(aux);
                                    Graphics gr = panelInt.CreateGraphics();
                                    pintarFuncion(aux, gr);
                                    idFuncion++;
                                    total++;
                                }
                                else
                                {
                                    MessageBox.Show(Cadenas.evaluacionIncorrectaDeFuncion + " => " + strFuncion);
                                }
                            }
                            catch (Exception msg)
                            {
                                MessageBox.Show(Cadenas.evaluacionIncorrectaDeFuncion + " => " + strFuncion);
                            }
                        }
                    }
                }

                estadoLabel.Text = Cadenas.seHanCargadoConExito + " " + total + " " + Cadenas.funciones;
            }
        }

        private void cargarPuntosEnMenPas_Click(object sender, EventArgs e)
        {
            PuntosMenpas cpem = new PuntosMenpas("cargar");
            cpem.DialogResult = DialogResult.No;
            cpem.ShowDialog();

            if (cpem.DialogResult == DialogResult.OK)
            {
                DataGridView dgv = cpem.dgv;
                int total = 0;

                for (int i = 0; i < dgv.RowCount; i++)
                {
                    if (dgv.Rows[i].Selected == true)
                    {
                        Punto p = new Punto(Convert.ToDouble(dgv.Rows[i].Cells[1].FormattedValue.ToString()), Convert.ToDouble(dgv.Rows[i].Cells[2].FormattedValue.ToString()), true, obtenerColor());

                        if (existe_punto(p))
                        {
                            MessageBox.Show(Cadenas.error023a + " (" + String.Format("{0:0.00}", p.coordenadaX()) + "; " + String.Format("{0:0.00}", p.coordenadaY()) + ") " + Cadenas.error023b);
                        }
                        else
                        {
                            //Agregamos el punto en la lista, en el historial y en el menu
                            listadoPuntos.Add(p);

                            ListViewItem lvi = new ListViewItem("(" + String.Format("{0:0.00}", p.coordenadaX()) + "; " + String.Format("{0:0.00}", p.coordenadaY()) + ")");
                            lvi.SubItems.Add(devuelveColorStr(p.devuelve_Color().Name));
                            historialPuntos.Items.Add(lvi);

                            string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                            if (puntosMenu.DropDownItems[3].ToString().CompareTo(noHayPuntos) == 0)
                            {
                                puntosMenu.DropDownItems.RemoveAt(3);
                            }

                            puntosMenu.DropDownItems.Add("(" + p.coordenadaX() + ", " + p.coordenadaY() + ")");

                            //Y finalmente pintamos el punto
                            pintarPunto(p);

                            //Habilitamos las opciones de quitar puntos en el menú
                            quitarPrimerPuntoAñadido.Enabled = true;
                            quitarUltimoPuntoAñadido.Enabled = true;

                            total++;
                        }
                    }
                }

                estadoLabel.Text = Cadenas.seHanCargadoConExito + " " + total + " " + Cadenas.puntosMinuscula;               
            }
        }

        private void borrarFuncionesEnMenPas_Click(object sender, EventArgs e)
        {
            int error = 0;
            bool borrar;
            DataGridView dgv = new DataGridView();
            string usuarioBorrar, strFuncion, hora, tipoPerfil, grupoUsuario;
            Proyecto.MenPas.WS_EstimacionF WS_EstimacionF = new Proyecto.MenPas.WS_EstimacionF();
            FuncionesMenpas cfem = new FuncionesMenpas("borrar");
            cfem.DialogResult = DialogResult.No;
            cfem.ShowDialog();
            
            if (cfem.DialogResult == DialogResult.OK)
            {
                tipoPerfil = WS_EstimacionF.dame_perfil(usuario);
                grupoUsuario = WS_EstimacionF.Obtener_grupo(usuario);
                dgv = cfem.dgv;

                for (int i = 0; i < dgv.RowCount; i++)
                {
                    if (dgv.Rows[i].Selected == true)
                    {
                        borrar = true;
                        usuarioBorrar = dgv.Rows[i].Cells[0].FormattedValue.ToString();
                        strFuncion = dgv.Rows[i].Cells[1].FormattedValue.ToString();
                        hora = dgv.Rows[i].Cells[2].FormattedValue.ToString();
                        
                        if (usuario != usuarioBorrar)
                        {
                            switch (tipoPerfil)
                            {
                                case "Administrador":
                                    break;
                                case "AD_Cuestionarios":
                                case "Ad_Restringido":
                                case "AD_Paises":
                                    if (grupoUsuario.CompareTo(WS_EstimacionF.Obtener_grupo(usuarioBorrar)) != 0)
                                    {
                                        error = i + 1;
                                        MessageBox.Show(Cadenas.error024);
                                        borrar = false;
                                    }
                                    break;
                                default:
                                    error = i + 1;
                                    MessageBox.Show(Cadenas.error024);
                                    borrar = false;
                                    break;
                            }
                        }

                        try
                        {
                            if (borrar)
                            {
                                borrar = WS_EstimacionF.BorrarFuncion(usuarioBorrar, strFuncion, hora);

                                if (!borrar)
                                {
                                    error = i + 1;
                                    MessageBox.Show(Cadenas.error025 + ": " + error);
                                }
                            }
                        }
                        catch (System.Net.WebException msg)
                        {
                            MessageBox.Show(Cadenas.error017);
                            return;
                        }
                    }
                }
            }
            else
            {
                return;
            }

            if (dgv.RowCount == 0)
            {
                MessageBox.Show(Cadenas.error026);
                return;
            }

            if (error == 0)
            {
                MessageBox.Show(Cadenas.funcionesBorradasCorrectamente);
            }
        }

        private void borrarPuntosEnMenPas_Click(object sender, EventArgs e)
        {
            int error = 0;
            bool borrar;
            string usuarioBorrar, hora, tipoPerfil, grupoUsuario;
            int x, y;
            DataGridView dgv = new DataGridView();
            Proyecto.MenPas.WS_EstimacionF WS_EstimacionF = new Proyecto.MenPas.WS_EstimacionF();
            PuntosMenpas cpem = new PuntosMenpas("borrar");
            cpem.DialogResult = DialogResult.No;
            cpem.ShowDialog();

            if (cpem.DialogResult == DialogResult.OK)
            {
                dgv = cpem.dgv;
                tipoPerfil = WS_EstimacionF.dame_perfil(usuario);
                grupoUsuario = WS_EstimacionF.Obtener_grupo(usuario);

                for (int i = 0; i < dgv.RowCount; i++)
                {
                    if (dgv.Rows[i].Selected == true)
                    {
                        borrar = true;
                        usuarioBorrar = dgv.Rows[i].Cells[0].FormattedValue.ToString();
                        x = int.Parse(dgv.Rows[i].Cells[1].FormattedValue.ToString());
                        y = int.Parse(dgv.Rows[i].Cells[2].FormattedValue.ToString());
                        hora = dgv.Rows[i].Cells[3].FormattedValue.ToString();

                        if (usuario != usuarioBorrar)
                        {
                            switch (tipoPerfil)
                            {
                                case "Administrador":
                                    break;
                                case "AD_Cuestionarios":
                                case "Ad_Restringido":
                                case "AD_Paises":
                                    if (grupoUsuario.CompareTo(WS_EstimacionF.Obtener_grupo(usuarioBorrar)) != 0)
                                    {
                                        error = i + 1;
                                        MessageBox.Show(Cadenas.error027);
                                        borrar = false;
                                    }
                                    break;
                                default:
                                    error = i + 1;
                                    MessageBox.Show(Cadenas.error027);
                                    borrar = false;
                                    break;
                            }
                        }

                        try
                        {
                            if (borrar)
                            {
                                borrar = WS_EstimacionF.BorrarPunto(usuarioBorrar, x, y, hora);

                                if (!borrar)
                                {
                                    error = i + 1;
                                    MessageBox.Show(Cadenas.error028);
                                }
                            }
                        }
                        catch (System.Net.WebException msg)
                        {
                            MessageBox.Show(Cadenas.error017);
                            return;
                        }
                    }
                }
            }
            else
            {
                return;
            }

            if (dgv.RowCount == 0)
            {
                MessageBox.Show(Cadenas.error029);                
            }

            if (error == 0)
            {
                MessageBox.Show(Cadenas.puntosBorradosCorrectamente);
            }
        }

        private void salir_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonAumentar_Click(object sender, EventArgs e)
        {
            zoom_id++;
            double zoom_id_temp, zoom_temp;

            if (buttonDisminuir.Enabled == false)
            {
                buttonDisminuir.Enabled = true;
                disminuirZoom.Enabled = true;
            }

            if (zoom_id >= 0)
            {
                if (zoom_id == 11)
                {
                    MessageBox.Show(Cadenas.error030);
                    zoom_id = 10;
                }
                else
                {
                    zoom = Math.Round(Math.Pow(2, zoom_id / 2) * 100);
                }
            }
            else
            {
                zoom = Math.Round((1 / (Math.Pow(2, (zoom_id * (-1)) / 2))) * 100);
            }

            actualizarPanel();
            pintarReglaYEje();

            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }

            estadoLabel.Text = Cadenas.tamaño + ": " + anchuraPanelInt + ", " + alturaPanelInt;
            zoomLabel.Text = "Zoom: " + zoom + "%";
            
            //
            //Cambiar el nombre a las etiquetas del menú de opciones
            //
            zoom_id_temp = zoom_id;
            zoom_id_temp++;
            zoom_temp = zoom;

            if (zoom_id_temp >= 0)
            {
                if (zoom_id_temp == 11)
                {
                    aumentarZoom.Text = Cadenas.noSePuedeAumentarMasElZoom;
                    buttonAumentar.Enabled = false;
                    aumentarZoom.Enabled = false;
                }
                else
                {
                    zoom_temp = Math.Round(Math.Pow(2, zoom_id_temp / 2) * 100);
                    aumentarZoom.Text = Cadenas.aumentarZoom + " (" + zoom_temp + "%)";
                    zoom_posterior = zoom_temp;
                }
            }
            else
            {
                zoom_temp = Math.Round((1 / (Math.Pow(2, (zoom_id_temp * (-1)) / 2))) * 100);
                aumentarZoom.Text = Cadenas.aumentarZoom + " (" + zoom_temp + "%)";
                zoom_posterior = zoom_temp;
            }

            zoom_id_temp = zoom_id_temp - 2;
            if (zoom_id_temp >= 0)
            {
                zoom_temp = Math.Round(Math.Pow(2, zoom_id_temp / 2) * 100);                
            }
            else
            {
                zoom_temp = Math.Round((1 / (Math.Pow(2, (zoom_id_temp * (-1)) / 2))) * 100);                
            }
            disminuirZoom.Text = Cadenas.disminuirZoom + " (" + zoom_temp + "%)";
            zoom_anterior = zoom_temp;
        }

        private void buttonDisminuir_Click(object sender, EventArgs e)
        {
            zoom_id--;
            double zoom_id_temp, zoom_temp;

            if (buttonAumentar.Enabled == false)
            {
                buttonAumentar.Enabled = true;
                aumentarZoom.Enabled = true;
            }

            if (zoom_id >= 0)
            {
                zoom = Math.Round(Math.Pow(2, zoom_id / 2) * 100);
            }
            else
            {
                if (zoom_id == -3)
                {
                    MessageBox.Show(Cadenas.error031);                    
                    zoom_id = -2;
                }
                else
                {
                    zoom = Math.Round((1 / (Math.Pow(2, (zoom_id * (-1)) / 2))) * 100);
                }
            }

            actualizarPanel();
            pintarReglaYEje();

            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }

            estadoLabel.Text = Cadenas.tamaño + ": " + anchuraPanelInt + ", " + alturaPanelInt;
            zoomLabel.Text = "Zoom: " + zoom + "%";

            //
            //Cambiar el nombre a las etiquetas del menú de opciones
            //
            zoom_id_temp = zoom_id;
            zoom_id_temp--;
            zoom_temp = zoom;

            if (zoom_id_temp >= 0)
            {
                zoom_temp = Math.Round(Math.Pow(2, zoom_id_temp / 2) * 100);
                disminuirZoom.Text = Cadenas.disminuirZoom + " (" + zoom_temp + "%)";
                zoom_anterior = zoom_temp;
            }
            else
            {
                if (zoom_id_temp == -3)
                {
                    disminuirZoom.Text = Cadenas.noSePuedeDisminuirMasElZoom;
                    buttonDisminuir.Enabled = false;
                    disminuirZoom.Enabled = false;
                }
                else
                {
                    zoom_temp = Math.Round((1 / (Math.Pow(2, (zoom_id_temp * (-1)) / 2))) * 100);
                    disminuirZoom.Text = Cadenas.disminuirZoom + " (" + zoom_temp + "%)";
                    zoom_anterior = zoom_temp;
                }
            }

            zoom_id_temp = zoom_id_temp + 2;
            if (zoom_id_temp >= 0)
            {
                zoom_temp = Math.Round(Math.Pow(2, zoom_id_temp / 2) * 100);
            }
            else
            {
                zoom_temp = Math.Round((1 / (Math.Pow(2, (zoom_id_temp * (-1)) / 2))) * 100);                
            }
            aumentarZoom.Text = Cadenas.aumentarZoom + " (" + zoom_temp + "%)";
            zoom_posterior = zoom_temp;
        }

        private void fijarDesplazamiento_Click(object sender, EventArgs e)
        {
            Parametros desp = new Parametros("Desplazamiento");
            desp.DialogResult = DialogResult.No;

            while (desp.DialogResult == DialogResult.No)
            {
                if (desp.ShowDialog(this) == DialogResult.OK)
                {
                    if ((desp.devolverParametro() <= 0) || (desp.devolverParametro() % 10 != 0))
                    {
                        MessageBox.Show(Cadenas.error032);
                        desp.DialogResult = DialogResult.No;
                    }
                    else
                    {
                        desplazamiento = (int)desp.devolverParametro();
                        desplazamientoLabel.Text = Cadenas.desplazamiento + ": " + desplazamiento + " pts";                        
                    }
                }
                else
                {
                    if (desp.DialogResult == DialogResult.No)
                    {
                        MessageBox.Show(Cadenas.error032);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private void historialFunciones_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            mostrarFuncion.Enabled = false;
            ocultarFuncion.Enabled = false;
            colorDeFuncion.Enabled = false;
            borrarFuncionDelHistorial.Enabled = false;
            exportarAWord.Enabled = false;
        }

        private void historialFunciones_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                String funcion;

                item = historialFunciones.GetItemAt(e.X, e.Y);
                if (item == null)
                {
                    mostrarFuncion.Enabled = false;
                    ocultarFuncion.Enabled = false;
                    colorDeFuncion.Enabled = false;
                    borrarFuncionDelHistorial.Enabled = false;
                    exportarAWord.Enabled = false;
                }
                else
                {
                    funcion = item.Text.Substring(7);

                    int indice = -1;

                    foreach (Funcion f in listadoFunciones)
                    {
                        if (f.devuelve_funcion().CompareTo(funcion) == 0)
                        {
                            indice = listadoFunciones.IndexOf(f);
                        }
                    }

                    if (indice != -1)
                    {
                        if (listadoFunciones[indice].devuelve_mostrar())
                        {
                            mostrarFuncion.Enabled = false;
                            ocultarFuncion.Enabled = true;
                            colorDeFuncion.Enabled = true;
                        }
                        else
                        {
                            mostrarFuncion.Enabled = true;
                            ocultarFuncion.Enabled = false;
                            colorDeFuncion.Enabled = false;
                        }
                        exportarAWord.Enabled = true;
                        borrarFuncionDelHistorial.Enabled = true;
                    }
                }
            }
        }

        private void mostrarFuncion_Click(object sender, EventArgs e)
        {
            String strFuncion = item.Text.Substring(7);

            if (item != null)
            {
                int indice = -1;

                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_funcion().CompareTo(strFuncion) == 0)
                    {
                        indice = listadoFunciones.IndexOf(f);
                    }
                }

                if (indice != -1)
                {
                    listadoFunciones[indice].mostrarFuncion();
                }

                Graphics gr = panelInt.CreateGraphics();
                pintarEje(gr);
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        if (f.devuelve_mostrar() == true)
                        {
                            pintarFuncion(f, gr);
                        }
                    }
                }

                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }
            }

            mostrarFuncion.Enabled = false;
            ocultarFuncion.Enabled = true;
            colorDeFuncion.Enabled = true;
            borrarFuncionDelHistorial.Enabled = true;
        }

        private void ocultarFuncion_Click(object sender, EventArgs e)
        {
            String strFuncion = item.Text.Substring(7);
            int indice = -1;

            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_funcion().CompareTo(strFuncion) == 0)
                {
                    indice = listadoFunciones.IndexOf(f);
                }
            }

            if (indice != -1)
            {
                listadoFunciones[indice].ocultarFuncion();
            }

            Graphics gr = panelInt.CreateGraphics();
            pintarEje(gr);
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    pintarFuncion(f, gr);
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }

            mostrarFuncion.Enabled = true;
            ocultarFuncion.Enabled = false;
            colorDeFuncion.Enabled = false;
            borrarFuncionDelHistorial.Enabled = false;
        }

        private void eliminarFuncionDelHistorial_Click(object sender, EventArgs e)
        {
            String strFuncion = item.Text.Substring(7);
            int indice = -1, i = 0;
            bool encontrado = false;

            //Primero borramos la función del historial
            historialFunciones.Items.Remove(item);

            //Luego lo borramos de la pantalla
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_funcion().CompareTo(strFuncion) == 0)
                {
                    indice = listadoFunciones.IndexOf(f);
                }
            }

            if (indice != -1)
            {
                listadoFunciones.RemoveAt(indice);
            }

            Graphics gr = panelInt.CreateGraphics();
            pintarEje(gr);
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    pintarFuncion(f, gr);
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }

            //Luego lo borramos del listado que hay en el menú de funciones
            i = 3;
            while (!encontrado)
            {
                if (funcionesMenu.DropDownItems[i].ToString().CompareTo(strFuncion) == 0)
                {
                    funcionesMenu.DropDownItems.RemoveAt(i);
                    encontrado = true;
                }
                else
                {
                    i++;
                }
            }

            //Comprobamos si hay funciones en el historial para activar o desactivas las opciones
            if (historialFunciones.Items.Count > 0)
            {
                borrarPrimeraFuncion.Enabled = true;
                borrarUltimaFuncion.Enabled = true;
            }
            else
            {
                borrarPrimeraFuncion.Enabled = false;
                borrarUltimaFuncion.Enabled = false;

                string noHayFunciones = Cadenas.noHayFuncionesEnElHistorial;

                funcionesMenu.DropDownItems.Add(noHayFunciones);
                funcionesMenu.DropDownItems[3].Enabled = false;
            }

            mostrarFuncion.Enabled = false;
            ocultarFuncion.Enabled = false;
            colorDeFuncion.Enabled = false;
            borrarFuncionDelHistorial.Enabled = false;
            exportarAWord.Enabled = false;
        }

        private void historialPuntos_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            mostrarPunto.Enabled = false;
            colorDePunto.Enabled = false;
            ocultarPunto.Enabled = false;
            borrarPuntoDelHistorial.Enabled = false;
        }

        private void historialPuntos_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                item = historialPuntos.GetItemAt(e.X, e.Y);

                if (item == null)
                {
                    mostrarPunto.Enabled = false;
                    ocultarPunto.Enabled = false;
                    colorDePunto.Enabled = false;
                    borrarPuntoDelHistorial.Enabled = false;
                }
                else
                {
                    int i = historialPuntos.Items.IndexOf(item);
                    Color c = Color.Black;

                    if (i != -1)
                    {
                        if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.negro) == 0)
                        {
                            c = Color.Black;
                        }
                        else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.azul) == 0)
                        {
                            c = Color.Blue;
                        }
                        else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.rojo) == 0)
                        {
                            c = Color.Red;
                        }
                        else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.verde) == 0)
                        {
                            c = Color.Green;
                        }
                        else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.marron) == 0)
                        {
                            c = Color.Brown;
                        }
                        else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.violeta) == 0)
                        {
                            c = Color.Violet;
                        }
                        else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.naranja) == 0)
                        {
                            c = Color.Orange;
                        }
                        else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.gris) == 0)
                        {
                            c = Color.Gray;
                        }
                        else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.azulOscuro) == 0)
                        {
                            c = Color.DarkBlue;
                        }
                        else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.magenta) == 0)
                        {
                            c = Color.Magenta;
                        }                        
                        else
                        {
                            c = Color.Black;
                        }
                    }

                    String[] strPunto = item.ToString().Split(' ', '(', ')', ';');
                    Punto punto = new Punto(double.Parse(strPunto[2]), double.Parse(strPunto[4]), true, c);

                    int indice = -1;

                    foreach (Punto p in listadoPuntos)
                    {
                        if ((p.coordenadaX() == punto.coordenadaX()) && (p.coordenadaY() == punto.coordenadaY()) && (p.devuelve_Color() == punto.devuelve_Color()))
                        {
                            indice = listadoPuntos.IndexOf(p);
                        }
                    }

                    if (indice != -1)
                    {
                        if (listadoPuntos[indice].devuelve_mostrar() == true)
                        {
                            mostrarPunto.Enabled = false;
                            ocultarPunto.Enabled = true;
                            colorDePunto.Enabled = true;
                        }
                        else
                        {
                            mostrarPunto.Enabled = true;
                            ocultarPunto.Enabled = false;
                            colorDePunto.Enabled = false;
                        }
                        borrarPuntoDelHistorial.Enabled = true;
                    }
                }
            }
        }

        private void mostrarPunto_Click(object sender, EventArgs e)
        {
            String[] strPunto;
            Punto punto = null;
            int indice = -1;

            if (item != null)
            {
                int i = historialPuntos.Items.IndexOf(item);
                Color c = Color.Black;

                if (i != -1)
                {
                    if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.negro) == 0)
                    {
                        c = Color.Black;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.azul) == 0)
                    {
                        c = Color.Blue;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.rojo) == 0)
                    {
                        c = Color.Red;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.verde) == 0)
                    {
                        c = Color.Green;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.marron) == 0)
                    {
                        c = Color.Brown;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.violeta) == 0)
                    {
                        c = Color.Violet;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.violeta) == 0)
                    {
                        c = Color.Orange;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.gris) == 0)
                    {
                        c = Color.Gray;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.azulOscuro) == 0)
                    {
                        c = Color.DarkBlue;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.magenta) == 0)
                    {
                        c = Color.Magenta;
                    }                    
                    else
                    {
                        c = Color.Black;
                    }
                }

                strPunto = item.ToString().Split(' ', '(', ')', ';');
                punto = new Punto(double.Parse(strPunto[2]), double.Parse(strPunto[4]), true, c);
                
                foreach (Punto p in listadoPuntos)
                {
                    if ((p.coordenadaX() == punto.coordenadaX()) && (p.coordenadaY() == punto.coordenadaY()) && (p.devuelve_Color() == punto.devuelve_Color()))
                    {
                        indice = listadoPuntos.IndexOf(p);
                    }
                }

                if (indice != -1)
                {
                    listadoPuntos[indice].mostrarPunto();
                }

                Graphics gr = panelInt.CreateGraphics();
                pintarEje(gr);

                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        pintarFuncion(f, gr);
                    }
                }

                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }

                mostrarPunto.Enabled = false;
                colorDePunto.Enabled = true;
                ocultarPunto.Enabled = true;
                borrarPuntoDelHistorial.Enabled = true;
            }            
        }

        private void ocultarPunto_Click(object sender, EventArgs e)
        {
            String[] strPunto;
            Punto punto = null;
            int indice = -1;
            

            if (item != null)
            {
                int i = historialPuntos.Items.IndexOf(item);
                Color c = Color.Black;

                if (i != -1)
                {
                    if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.negro) == 0)
                    {
                        c = Color.Black;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.azul) == 0)
                    {
                        c = Color.Blue;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.rojo) == 0)
                    {
                        c = Color.Red;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.verde) == 0)
                    {
                        c = Color.Green;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.marron) == 0)
                    {
                        c = Color.Brown;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.violeta) == 0)
                    {
                        c = Color.Violet;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.violeta) == 0)
                    {
                        c = Color.Orange;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.gris) == 0)
                    {
                        c = Color.Gray;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.azulOscuro) == 0)
                    {
                        c = Color.DarkBlue;
                    }
                    else if (historialPuntos.Items[i].SubItems[1].Text.CompareTo(Cadenas.magenta) == 0)
                    {
                        c = Color.Magenta;
                    }
                    else
                    {
                        c = Color.Black;
                    }
                }

                strPunto = item.ToString().Split(' ', '(', ')', ';');
                punto = new Punto(double.Parse(strPunto[2]), double.Parse(strPunto[4]), true, c);

                foreach (Punto p in listadoPuntos)
                {
                    if ((p.coordenadaX() == punto.coordenadaX()) && (p.coordenadaY() == punto.coordenadaY()) && (p.devuelve_Color() == punto.devuelve_Color()))
                    {
                        indice = listadoPuntos.IndexOf(p);
                    }
                }

                if (indice != -1)
                {
                    listadoPuntos[indice].ocultarPunto();
                }

                Graphics gr = panelInt.CreateGraphics();
                pintarEje(gr);

                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        pintarFuncion(f, gr);
                    }
                }

                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }

                mostrarPunto.Enabled = true;
                colorDePunto.Enabled = false;
                ocultarPunto.Enabled = false;
                borrarPuntoDelHistorial.Enabled = true;
            }            
        }

        private void eliminarPuntoDelHistorial_Click(object sender, EventArgs e)
        {
            String[] strPuntoArray;
            string strPunto;
            Punto punto = null;
            int indice = -1;
            int i = 0;
            bool encontrado = false;

            indice = historialPuntos.Items.IndexOf(item);
            Color c = Color.Black;

            if (indice != -1)
            {
                if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.negro) == 0)
                {
                    c = Color.Black;
                }
                else if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.azul) == 0)
                {
                    c = Color.Blue;
                }
                else if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.rojo) == 0)
                {
                    c = Color.Red;
                }
                else if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.verde) == 0)
                {
                    c = Color.Green;
                }
                else if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.marron) == 0)
                {
                    c = Color.Brown;
                }
                else if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.violeta) == 0)
                {
                    c = Color.Violet;
                }
                else if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.naranja) == 0)
                {
                    c = Color.Orange;
                }
                else if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.gris) == 0)
                {
                    c = Color.Gray;
                }
                else if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.azulOscuro) == 0)
                {
                    c = Color.DarkBlue;
                }
                else if (historialPuntos.Items[indice].SubItems[1].Text.CompareTo(Cadenas.magenta) == 0)
                {
                    c = Color.Magenta;
                }
                else
                {
                    c = Color.Black;
                }
            }

            strPunto = item.Text;
            strPuntoArray = item.ToString().Split(' ', '(', ')', ';');
            punto = new Punto(double.Parse(strPuntoArray[2]), double.Parse(strPuntoArray[4]), true, c);

            //Primero borramos el punto del historial
            historialPuntos.Items.Remove(item);

            //Luego lo borramos de la pantalla
            indice = -1;
            foreach (Punto p in listadoPuntos)
            {
                if ((p.coordenadaX() == punto.coordenadaX()) && (p.coordenadaY() == punto.coordenadaY()) && (p.devuelve_Color() == punto.devuelve_Color()))
                {
                    indice = listadoPuntos.IndexOf(p);
                }
            }

            if (indice != -1)
            {
                listadoPuntos.RemoveAt(indice);
            }

            Graphics gr = panelInt.CreateGraphics();
            pintarEje(gr);

            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    pintarFuncion(f, gr);
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }

            //Luego lo borramos del listado que hay en el menú de funciones
            i = 3;
            while (!encontrado)
            {
                if (puntosMenu.DropDownItems[i].ToString().CompareTo(strPunto) == 0)
                {
                    puntosMenu.DropDownItems.RemoveAt(i);
                    encontrado = true;
                }
                else
                {
                    i++;
                }
            }

            //Comprobamos si hay puntos en el historial para activar o desactivas las opciones
            if (historialPuntos.Items.Count > 0)
            {
                quitarPrimerPuntoAñadido.Enabled = true;
                quitarUltimoPuntoAñadido.Enabled = true;
            }
            else
            {
                quitarPrimerPuntoAñadido.Enabled = false;
                quitarUltimoPuntoAñadido.Enabled = false;

                string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                puntosMenu.DropDownItems.Add(noHayPuntos);
                puntosMenu.DropDownItems[3].Enabled = false;
            }

            mostrarPunto.Enabled = false;
            colorDePunto.Enabled = false;
            ocultarPunto.Enabled = false;
            borrarPuntoDelHistorial.Enabled = false;
        }

        private void insertarPunto_Click(object sender, EventArgs e)
        {
            Parametros pX = new Parametros("PuntoX");
            Parametros pY = new Parametros("PuntoY");

            pX.DialogResult = DialogResult.No;

            while (pX.DialogResult == DialogResult.No)
            {
                pX.ShowDialog(this);
                if (pX.DialogResult == DialogResult.OK)
                {
                    pY.DialogResult = DialogResult.No;
                    while (pY.DialogResult == DialogResult.No)
                    {
                        pY.ShowDialog(this);
                        if (pY.DialogResult == DialogResult.OK)
                        {
                            Punto p = new Punto(pX.devolverParametro(), pY.devolverParametro(), true, obtenerColor());
                            if (existe_punto(p))
                            {
                                MessageBox.Show(Cadenas.error001);                                
                            }
                            else
                            {
                                //Agregamos el punto en la lista, en el historial y en el menu
                                listadoPuntos.Add(p);

                                ListViewItem lvi = new ListViewItem("(" + String.Format("{0:0.00}", pX.devolverParametro()) + "; " + String.Format("{0:0.00}", pY.devolverParametro()) + ")");
                                lvi.SubItems.Add(devuelveColorStr(p.devuelve_Color().Name));
                                historialPuntos.Items.Add(lvi);

                                string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                                if (puntosMenu.DropDownItems[3].ToString().CompareTo(noHayPuntos) == 0)
                                {
                                    puntosMenu.DropDownItems.RemoveAt(3);
                                }

                                puntosMenu.DropDownItems.Add("(" + String.Format("{0:0.00}", pX.devolverParametro()) + "; " + String.Format("{0:0.00}", pY.devolverParametro()) + ")");

                                //Y finalmente pintamos el punto
                                pintarPunto(p);

                                //Habilitamos las opciones de quitar puntos en el menú
                                quitarPrimerPuntoAñadido.Enabled = true;
                                quitarUltimoPuntoAñadido.Enabled = true;
                            }
                        }
                        else
                        {
                            if (pY.DialogResult == DialogResult.No)
                            {
                                MessageBox.Show(Cadenas.error037b);
                                pY.DialogResult = DialogResult.No;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (pX.DialogResult == DialogResult.No)
                    {
                        MessageBox.Show(Cadenas.error037a);
                        pX.DialogResult = DialogResult.No;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            panelExt_Paint(null, null);
        }

        private void situarPanelEnCoordenadas_Click(object sender, EventArgs e)
        {
            Parametros pX = new Parametros("PuntoX");
            Parametros pY = new Parametros("PuntoY");

            pX.DialogResult = DialogResult.No;

            while (pX.DialogResult == DialogResult.No)
            {
                pX.ShowDialog(this);
                if (pX.DialogResult == DialogResult.OK)
                {
                    pY.DialogResult = DialogResult.No;
                    while (pY.DialogResult == DialogResult.No)
                    {
                        pY.ShowDialog(this);
                        if (pY.DialogResult == DialogResult.OK)
                        {
                            int desp_x = (int)(pasar_coordenada_teoricaX(anchuraPanelInt / 2) - pasar_coordenada_teoricaX(0));
                            x_inicio = Convert.ToSingle(pX.devolverParametro()) - desp_x;
                            x_fin = Convert.ToSingle(pX.devolverParametro()) + desp_x;
                            int desp_y = (int)(pasar_coordenada_teoricaY(alturaPanelInt / 2) - pasar_coordenada_teoricaY(0));
                            y_inicio = Convert.ToSingle(pY.devolverParametro()) + desp_y;
                            y_fin = Convert.ToSingle(pY.devolverParametro()) - desp_y;

                            pintarReglaYEje();
                            foreach (Funcion f in listadoFunciones)
                            {
                                if (f.devuelve_mostrar() == true)
                                {
                                    Graphics gr = panelInt.CreateGraphics();
                                    pintarFuncion(f, gr);
                                }
                            }
                            foreach (Punto p in listadoPuntos)
                            {
                                if (p.devuelve_mostrar() == true)
                                {
                                    pintarPunto(p);
                                }
                            }
                        }
                        else
                        {
                            if (pY.DialogResult == DialogResult.No)
                            {
                                MessageBox.Show(Cadenas.error033);
                                pY.DialogResult = DialogResult.No;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (pX.DialogResult == DialogResult.No)
                    {
                        MessageBox.Show(Cadenas.error034);
                        pX.DialogResult = DialogResult.No;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private void chequeoCeldas_Click(object sender, EventArgs e)
        {
            pintarReglaYEje();

            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }
        }

        private void insertarPuntoBoton_Click(object sender, EventArgs e)
        {
            double x, y;

            bool xEsEntero = double.TryParse(cajaX.Text, out x);
            if (!xEsEntero)
            {
                MessageBox.Show(Cadenas.error035);
                return;
            }
            else
            {
                if (cajaX.Text.Contains('.'))
                {
                    MessageBox.Show(Cadenas.error035);
                    return;
                }
            }

            x = Math.Round(x, 2);

            bool yEsEntero = double.TryParse(cajaY.Text, out y);
            if (!yEsEntero)
            {
                if (cajaY.Text.Contains('.'))
                {
                    MessageBox.Show(Cadenas.error036);
                    return;
                }
            }
            else
            {
                if (cajaY.Text.Contains('.'))
                {
                    MessageBox.Show(Cadenas.error036);
                    return;
                }
            }

            y = Math.Round(y, 2);

            Punto p = new Punto(x, y, true, obtenerColor());
            
            if (existe_punto(p))
            {
                MessageBox.Show(Cadenas.error001);
            }
            else
            {
                //Agregamos el punto en la lista, en el historial y en el menu
                listadoPuntos.Add(p);

                ListViewItem lvi = new ListViewItem("(" + String.Format("{0:0.00}", x) + "; " + String.Format("{0:0.00}", y) + ")");
                lvi.SubItems.Add(devuelveColorStr(p.devuelve_Color().Name));
                historialPuntos.Items.Add(lvi);

                string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                if (puntosMenu.DropDownItems[3].ToString().CompareTo(noHayPuntos) == 0)
                {
                    puntosMenu.DropDownItems.RemoveAt(3);
                }

                puntosMenu.DropDownItems.Add("(" + String.Format("{0:0.00}", x) + "; " + String.Format("{0:0.00}", y) + ")");

                //Y finalmente pintamos el punto
                pintarPunto(p);

                //Habilitamos las opciones de quitar puntos en el menú
                quitarPrimerPuntoAñadido.Enabled = true;
                quitarUltimoPuntoAñadido.Enabled = true;

                //Inicializamos las cajas de texto de los puntos en caso de insertar
                cajaX.Text = "";
                cajaY.Text = "";
            }
        }

        private void situarCursorBoton_Click(object sender, EventArgs e)
        {
            int x, y;

            bool xEsEntero = int.TryParse(cajaX.Text, out x);
            if (!xEsEntero)
            {
                MessageBox.Show(Cadenas.error035);
                return;
            }

            bool yEsEntero = int.TryParse(cajaY.Text, out y);
            if (!yEsEntero)
            {
                MessageBox.Show(Cadenas.error036);
                return;
            }

            int desp_x = (int)(pasar_coordenada_teoricaX(anchuraPanelInt / 2) - pasar_coordenada_teoricaX(0));
            x_inicio = x - desp_x;
            x_fin = x + desp_x;
            int desp_y = (int)(pasar_coordenada_teoricaY(alturaPanelInt / 2) - pasar_coordenada_teoricaY(0));
            y_inicio = y + desp_y;
            y_fin = y - desp_y;

            pintarReglaYEje();
            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);
                }
            }
            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }
        }

        private void funcionesMenu_Clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string quitarPrimeraFuncion = Cadenas.borrarPrimeraFuncionDelHistorial;
            string quitarUltimaFuncion = Cadenas.borrarUltimaFuncionDelHistorial;

            if (e.ClickedItem.ToString().CompareTo(quitarPrimeraFuncion) == 0)
            {
                borrarPrimeraFuncionDelHistorial();
            }
            else if (e.ClickedItem.ToString().CompareTo(quitarUltimaFuncion) == 0)
            {
                borrarUltimaFuncionDelHistorial();
            }
            else if (e.ClickedItem.Text.CompareTo("") == 0)
            {
                return;
            }
            else
            {
                bool encontrado = false;
                string funcionStr;
                int i = 0;
                ListViewItem lvi = new ListViewItem();

                while ((i < this.historialFunciones.Items.Count) && (!encontrado))
                {
                    funcionStr = this.historialFunciones.Items[i].Text.Substring(7);
                    if (e.ClickedItem.ToString().CompareTo(funcionStr) == 0)
                    {
                        lvi = this.historialFunciones.Items[i];
                        historialFunciones.Items.Remove(lvi);
                        encontrado = true;
                    }
                    else
                    {
                        i++;
                    }
                }

                int indice = -1;

                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_funcion().CompareTo(e.ClickedItem.ToString()) == 0)
                    {
                        indice = listadoFunciones.IndexOf(f);
                    }
                }

                if (indice != -1)
                {
                    listadoFunciones.RemoveAt(indice);
                }

                Graphics gr = panelInt.CreateGraphics();
                pintarEje(gr);
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        pintarFuncion(f, gr);
                    }
                }

                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }

                funcionesMenu.DropDownItems.Remove(e.ClickedItem);

                if (historialFunciones.Items.Count > 0)
                {
                    borrarPrimeraFuncion.Enabled = true;
                    borrarUltimaFuncion.Enabled = true;
                }
                else
                {
                    borrarPrimeraFuncion.Enabled = false;
                    borrarUltimaFuncion.Enabled = false;

                    string noHayFunciones = Cadenas.noHayFuncionesEnElHistorial;

                    funcionesMenu.DropDownItems.Add(noHayFunciones);
                    funcionesMenu.DropDownItems[3].Enabled = false;
                }

                mostrarFuncion.Enabled = false;
                ocultarFuncion.Enabled = false;
                colorDeFuncion.Enabled = false;
                borrarFuncionDelHistorial.Enabled = false;
                exportarAWord.Enabled = false;
            }
        }

        private void borrarPrimeraFuncionDelHistorial()
        {
            string funcionStr = this.historialFunciones.Items[0].Text.Substring(7);
            Funcion funcion = new Funcion(funcionStr, 0, Color.Black, true);
            int indice = 0;

            if (existe_funcion(funcionStr))
            {
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_funcion().CompareTo(funcion.devuelve_funcion()) == 0)
                    {
                        indice = listadoFunciones.IndexOf(f);
                    }
                }
                listadoFunciones.RemoveAt(indice);

                Graphics gr = panelInt.CreateGraphics();
                pintarEje(gr);
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        pintarFuncion(f, gr);
                    }
                }

                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }
            }

            historialFunciones.Items.RemoveAt(0);
            funcionesMenu.DropDownItems.RemoveAt(3);

            if (historialFunciones.Items.Count == 0)
            {
                borrarPrimeraFuncion.Enabled = false;
                borrarUltimaFuncion.Enabled = false;

                string noHayFunciones = Cadenas.noHayFuncionesEnElHistorial;

                funcionesMenu.DropDownItems.Add(noHayFunciones);
                funcionesMenu.DropDownItems[3].Enabled = false;
            }
        }

        private void borrarUltimaFuncionDelHistorial()
        {
            string funcionStr = this.historialFunciones.Items[historialFunciones.Items.Count - 1].Text.Substring(7);
            Funcion funcion = new Funcion(funcionStr, 0, Color.Black, true);
            int indice = 0;

            if (existe_funcion(funcionStr))
            {
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_funcion().CompareTo(funcion.devuelve_funcion()) == 0)
                    {
                        indice = listadoFunciones.IndexOf(f);
                    }
                }
                listadoFunciones.RemoveAt(indice);

                Graphics gr = panelInt.CreateGraphics();
                pintarEje(gr);
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        pintarFuncion(f, gr);
                    }
                }

                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }
            }

            historialFunciones.Items.RemoveAt(historialFunciones.Items.Count - 1);
            funcionesMenu.DropDownItems.RemoveAt(funcionesMenu.DropDownItems.Count - 1);

            if (historialFunciones.Items.Count == 0)
            {
                borrarPrimeraFuncion.Enabled = false;
                borrarUltimaFuncion.Enabled = false;

                string noHayFunciones = Cadenas.noHayFuncionesEnElHistorial;

                funcionesMenu.DropDownItems.Add(noHayFunciones);
                funcionesMenu.DropDownItems[3].Enabled = false;
            }
        }

        private void puntosMenu_Clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string quitarPrimerPunto = Cadenas.borrarPrimerPuntoDelHistorial;
            string quitarUltimoPunto = Cadenas.borrarUltimoPuntoDelHistorial;

            if (e.ClickedItem.ToString().CompareTo(quitarPrimerPunto) == 0)
            {
                quitarPrimerPuntoInsertado();
            }
            else if (e.ClickedItem.ToString().CompareTo(quitarUltimoPunto) == 0)
            {
                quitarUltimoPuntoInsertado();
            }
            else if (e.ClickedItem.Text.CompareTo("") == 0)
            {
                return;
            }
            else
            {
                bool encontrado = false;
                string puntoStrHistorial;
                int i = 0;
                ListViewItem lvi = new ListViewItem();
                Punto punto = null;
                String[] strPuntoSeleccionado = e.ClickedItem.ToString().Split(' ', '(', ')', ';');
                
                try
                {
                    punto = new Punto(double.Parse(strPuntoSeleccionado[1]), double.Parse(strPuntoSeleccionado[3]), true, obtenerColor());
                }
                catch (Exception msg)
                {
                    return;
                }

                //Eliminamos el punto del historial
                while ((i < this.historialPuntos.Items.Count) && (!encontrado))
                {
                    puntoStrHistorial = this.historialPuntos.Items[i].Text;
                    if (e.ClickedItem.ToString().CompareTo(puntoStrHistorial) == 0)
                    {
                        lvi = this.historialPuntos.Items[i];
                        historialPuntos.Items.Remove(lvi);
                        encontrado = true;
                    }
                    else
                    {
                        i++;
                    }
                }

                //Luego eliminamos el punto de la pantalla
                int indice = -1;
                encontrado = false;
                foreach (Punto p in listadoPuntos)
                {
                    if ((p.coordenadaX() == punto.coordenadaX()) && (p.coordenadaY() == punto.coordenadaY()) && (!encontrado))
                    {
                        indice = listadoPuntos.IndexOf(p);
                        encontrado = true;
                    }
                }

                if (indice != -1)
                {
                    listadoPuntos.RemoveAt(indice);
                }

                Graphics gr = panelInt.CreateGraphics();
                pintarEje(gr);

                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        pintarFuncion(f, gr);
                    }
                }

                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }

                //Luego eliminamos el punto del menu de puntos
                puntosMenu.DropDownItems.Remove(e.ClickedItem);

                if (historialPuntos.Items.Count > 0)
                {
                    quitarPrimerPuntoAñadido.Enabled = true;
                    quitarUltimoPuntoAñadido.Enabled = true;
                }
                else
                {
                    quitarPrimerPuntoAñadido.Enabled = false;
                    quitarUltimoPuntoAñadido.Enabled = false;

                    string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                    puntosMenu.DropDownItems.Add(noHayPuntos);
                    puntosMenu.DropDownItems[0].Enabled = false;
                    puntosMenu.DropDownItems[1].Enabled = false;
                    puntosMenu.DropDownItems[2].Enabled = false;
                    puntosMenu.DropDownItems[3].Enabled = false;
                }

                mostrarPunto.Enabled = false;
                colorDePunto.Enabled = false;
                ocultarPunto.Enabled = false;
                borrarPuntoDelHistorial.Enabled = false;
            }

        }

        private void quitarPrimerPuntoInsertado()
        {
            String[] strPuntoArray = historialPuntos.Items[0].ToString().Split(' ', '(', ')', ';');
            string strPunto = historialPuntos.Items[0].Text;
            Punto punto = new Punto(double.Parse(strPuntoArray[2]), double.Parse(strPuntoArray[4]), true, obtenerColor());
            bool encontrado = false;
            int i = 0;

            //Si existe el punto por pantalla, lo borramos
            if (existe_punto(punto))
            {
                listadoPuntos.RemoveAt(0);
                Graphics gr = panelInt.CreateGraphics();
                pintarEje(gr);
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        pintarFuncion(f, gr);
                    }
                }
                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }
            }

            //Borramos el punto del historial
            historialPuntos.Items.RemoveAt(0);

            //Luego lo borramos del listado que hay en el menú de puntos
            i = 3;
            while (!encontrado)
            {
                if (puntosMenu.DropDownItems[i].ToString().CompareTo(strPunto) == 0)
                {
                    puntosMenu.DropDownItems.RemoveAt(i);
                    encontrado = true;
                }
                else
                {
                    i++;
                }
            }

            //Comprobamos si hay puntos en el historial para activar o desactivas las opciones
            if (historialPuntos.Items.Count > 0)
            {
                quitarPrimerPuntoAñadido.Enabled = true;
                quitarUltimoPuntoAñadido.Enabled = true;
            }
            else
            {
                quitarPrimerPuntoAñadido.Enabled = false;
                quitarUltimoPuntoAñadido.Enabled = false;

                string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                puntosMenu.DropDownItems.Add(noHayPuntos);
                puntosMenu.DropDownItems[0].Enabled = false;
                puntosMenu.DropDownItems[1].Enabled = false;
                puntosMenu.DropDownItems[2].Enabled = false;
                puntosMenu.DropDownItems[3].Enabled = false;
            }
        }

        private void quitarUltimoPuntoInsertado()
        {
            String[] strPuntoArray = historialPuntos.Items[historialPuntos.Items.Count - 1].ToString().Split(' ', '(', ')', ';');
            string strPunto = historialPuntos.Items[historialPuntos.Items.Count - 1].Text;
            Punto punto = new Punto(double.Parse(strPuntoArray[2]), double.Parse(strPuntoArray[4]), true, obtenerColor());
            bool encontrado = false;
            int i = 0;

            //Si existe el punto por pantalla, lo borramos
            if (existe_punto(punto))
            {
                listadoPuntos.RemoveAt(listadoPuntos.Count - 1);
                Graphics gr = panelInt.CreateGraphics();
                pintarEje(gr);
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        pintarFuncion(f, gr);
                    }
                }
                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }
            }

            //Borramos el punto del historial
            historialPuntos.Items.RemoveAt(historialPuntos.Items.Count - 1);

            //Luego lo borramos del listado que hay en el menú de puntos
            i = 3;
            while (!encontrado)
            {
                if (puntosMenu.DropDownItems[i].ToString().CompareTo(strPunto) == 0)
                {
                    puntosMenu.DropDownItems.RemoveAt(i);
                    encontrado = true;
                }
                else
                {
                    i++;
                }
            }

            //Comprobamos si hay puntos en el historial para activar o desactivas las opciones
            if (historialPuntos.Items.Count > 0)
            {
                quitarPrimerPuntoAñadido.Enabled = true;
                quitarUltimoPuntoAñadido.Enabled = true;
            }
            else
            {
                quitarPrimerPuntoAñadido.Enabled = false;
                quitarUltimoPuntoAñadido.Enabled = false;

                string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                puntosMenu.DropDownItems.Add(noHayPuntos);
                puntosMenu.DropDownItems[0].Enabled = false;
                puntosMenu.DropDownItems[1].Enabled = false;
                puntosMenu.DropDownItems[2].Enabled = false;
                puntosMenu.DropDownItems[3].Enabled = false;
            }
        }

        private void guardarPanel_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "jpg";
            saveFileDialog.Title = Cadenas.guardarPanel;
            saveFileDialog.Filter = Cadenas.archivoJPG;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmpPanelInt = new Bitmap((int)anchuraPanelInt, (int)alturaPanelInt);
                Bitmap bmpPanelExt = new Bitmap((int)anchuraContenedor, (int)alturaContenedor);
                Bitmap bmp = new Bitmap((int)anchuraContenedor, (int)alturaContenedor);
                Graphics gr = Graphics.FromImage(bmpPanelInt);

                //Pintamos ejes y funciones
                pintarEje(gr);
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        pintarFuncion(f, gr);
                    }
                }

                //Pintamos regla
                gr = Graphics.FromImage(bmpPanelExt);
                pintarRegla(gr);

                //Juntamos ambas imágenes
                Rectangle rec = new Rectangle(0, 0, (int)anchuraContenedor, (int)alturaContenedor);
                gr = Graphics.FromImage(bmp);
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                gr.DrawImage(bmpPanelExt, rec, rec, GraphicsUnit.Pixel);
                gr.DrawImage(bmpPanelInt, new Rectangle(22, 22, (int)anchuraPanelInt, (int)alturaPanelInt), new Rectangle(0, 0, (int)anchuraPanelInt, (int)alturaPanelInt), GraphicsUnit.Pixel);

                //Grabamos la imagen
                bmp.Save(saveFileDialog.FileName);
                gr.Dispose();
            }

        }

        private void acercaDe_Click(object sender, EventArgs e)
        {
            AcercaDe aD = new AcercaDe();
            aD.ShowDialog();
        }

        private void exportarAWord_Click(object sender, EventArgs e)
        {
            String strFuncion = item.Text, pathWord;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = Cadenas.funcionDoc;
            saveFileDialog1.Title = Cadenas.seleccionaUnFicheroDoc;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pathWord = saveFileDialog1.FileName;
                Word.Application objWordApplication;
                Word.Document objWordDocument;
                Object oMissing = System.Reflection.Missing.Value;

                objWordApplication = new Word.Application();
                objWordDocument = objWordApplication.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                objWordDocument.Activate();

                //Empezamos a escribir
                objWordApplication.Selection.TypeText(strFuncion);

                //Indicamos que el texto anterior es parte de un párrafo.
                //objWordApplication.Selection.TypeParagraph();

                //Ahora veamos como cambiar el tipo y tamaño de la letra
                //objWordApplication.Selection.Font.Name="Arial"; //Cambiamos el nombre
                //objWordApplication.Selection.Font.Size= 19; //Cambiamos el tamaño

                //Alinearemos el texto que vamos a escribir al centro
                //objWordApplication.Selection.ParagraphFormat.Alignment=
                //Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

                //Indicamos que el texto anterior es un párrafo
                //objWordApplication.Selection.TypeParagraph();

                //Hace visible la Aplicacion para que veas lo que se ha escrito
                objWordApplication.Visible = true;
                objWordDocument.SaveAs(pathWord);
                
                //MySaveAs(objWordDocument,pathWord);

                /*
                object[] param = { pathWord };
                Type clase = objWordDocument.GetType();
                foreach (MethodInfo m in clase.GetMethods())
                {
                    if (m.Name.Contains("SaveAs") && m.GetParameters().Length == 1)
                    {
                        //m.Invoke(objWordDocument, param);
                        continue;
                    }
                }
                */

                estadoLabel.Text = Cadenas.exportacionAWordCorrecta;                
            }
        }

        private void importarUnaFuncionDeExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = Cadenas.hojaDeFunciones;
            openFileDialog.Title = Cadenas.seleccionaUnaHojaDeCalculo;

            string pathExcel;
            int nFilas = 0, filasVacias = 0;
            bool cambioDeFila = false;

            Parametros filas = new Parametros("Filas");
            filas.DialogResult = DialogResult.No;

            while (filas.DialogResult == DialogResult.No)
            {
                if (filas.ShowDialog(this) == DialogResult.OK)
                {
                    if (filas.devolverParametro() < 0)
                    {
                        MessageBox.Show(Cadenas.error041);
                        filas.DialogResult = DialogResult.No;
                    }
                    else
                    {
                        if (filas.devolverParametro().ToString().Contains(','))
                        {
                            MessageBox.Show(Cadenas.error041);
                            filas.DialogResult = DialogResult.No;
                        }
                        else
                        {
                            nFilas = (int)filas.devolverParametro();
                        }                                  
                    }
                }
                else
                {
                    if (filas.DialogResult == DialogResult.No)
                    {
                        MessageBox.Show(Cadenas.error041);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                double[] x;
                double[] y;
                int index;
                string ecuacionFinal;
                SortedList<double, double> listaPuntosOrdenada;

                pathExcel = openFileDialog.FileName;
                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                object misValue = System.Reflection.Missing.Value;

                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(pathExcel, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                char charA = 'A';
                char charB = 'B';
                int ind = 1;
                listaPuntosOrdenada = new SortedList<double, double>();

                while (!cambioDeFila)
                {
                    if ((xlWorkSheet.get_Range(charA.ToString() + ind.ToString(), charA.ToString() + ind.ToString()).Value2 == null) || (xlWorkSheet.get_Range(charB.ToString() + ind.ToString(), charB.ToString() + ind.ToString()).Value2 == null))
                    {
                        filasVacias++;
                    }
                    else
                    {
                        filasVacias = 0;
                        double coordenadaX, coordenadaY;

                        try
                        {
                            coordenadaX = (double)xlWorkSheet.get_Range(charA.ToString() + ind.ToString(), charA.ToString() + ind.ToString()).Value2;
                        }                        
                        catch (Exception msg)
                        {
                            MessageBox.Show(Cadenas.error038a + " " + charA + ", " + Cadenas.fila + " " + ind + " " + Cadenas.error038b);
                            return;
                        }

                        try
                        {
                            coordenadaY = (double)xlWorkSheet.get_Range(charB.ToString() + ind.ToString(), charB.ToString() + ind.ToString()).Value2;
                        }
                        catch (Exception msg)
                        {
                            MessageBox.Show(Cadenas.error038a + " " + charB + ", " + Cadenas.fila + " " + ind + " " + Cadenas.error038b);
                            return;
                        }

                        try
                        {
                            listaPuntosOrdenada.Add(coordenadaX, coordenadaY);
                        }
                        catch (ArgumentException msg)
                        {
                            MessageBox.Show(Cadenas.error039 + " " + charA + ", " + Cadenas.fila + " " + ind);
                            return;
                        }
                        catch (IndexOutOfRangeException msg)
                        {
                            MessageBox.Show(Cadenas.error039 + " " + charA + ", " + Cadenas.fila + " " + ind);
                            return;
                        }
                    }

                    if (nFilas == 0)
                    {
                        if (filasVacias >= 10)
                        {
                            cambioDeFila = true;
                        }
                    }
                    else
                    {
                        if (ind >= nFilas)
                        {
                            cambioDeFila = true;
                        }
                    }

                    ind++;
                }

                if (listaPuntosOrdenada.Count > 1)
                {
                    index = 0;
                    x = new double[listaPuntosOrdenada.Count];
                    IList<double> listadoX = listaPuntosOrdenada.Keys;
                    foreach (double temp in listadoX)
                    {
                        x[index] = temp;
                        index++;
                    }

                    index = 0;
                    y = new double[listaPuntosOrdenada.Count];
                    IList<double> listadoY = listaPuntosOrdenada.Values;
                    foreach (double temp in listadoY)
                    {
                        y[index] = temp;
                        index++;
                    }

                    InterpolacionPolinomica diferenciasDivididas = new InterpolacionPolinomica(x, y);
                    ecuacionFinal = diferenciasDivididas.resolver();
                    ecuacionFinal = ecuacionFinal.Replace(',', '.');

                    if (existe_funcion(ecuacionFinal))
                    {
                        MessageBox.Show(Cadenas.error022a + "f(x) = " + ecuacionFinal + Cadenas.error022b);
                        return;
                    }
                    else
                    {
                        bool ok = genera_funcion(ecuacionFinal);
                        if (ok)
                        {
                            if (historialFunciones.Items.Count == 0)
                            {
                                funcionesMenu.DropDownItems.Remove(funcionesMenu.DropDownItems[3]);
                            }

                            ListViewItem lvi = new ListViewItem("f(x) = " + ecuacionFinal);
                            string traduccion = traducirFuncion(ecuacionFinal);
                            lvi.SubItems.Add("f(x) = " + traduccion);
                            lvi.SubItems.Add(devuelveColorStr(obtenerColor().Name));
                            historialFunciones.Items.Add(lvi);
                            funcionesMenu.DropDownItems.Add(ecuacionFinal);
                            borrarPrimeraFuncion.Enabled = true;
                            borrarUltimaFuncion.Enabled = true;
                            Color color = obtenerColor();
                            Funcion aux = new Funcion(ecuacionFinal, idFuncion, color, false);
                            listadoFunciones.Add(aux);
                            idFuncion++;
                            textoFuncion.Text = "";
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Cadenas.error040);
                    return;
                }

                estadoLabel.Text = Cadenas.importacionFuncionOK;

                xlWorkBook.Close(false, misValue, misValue);
                xlApp.Quit();

                releaseObject(xlWorkSheet);
                releaseObject(xlWorkBook);
                releaseObject(xlApp);
            }
        }

        private void importarFuncionesDeExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = Cadenas.hojaDeFunciones;
            openFileDialog.Title = Cadenas.seleccionaUnaHojaDeCalculo;

            string pathExcel;
            int nFilas = 0, filasVacias = 0;
            bool cambioDeColumna, procesadoPuntosOK;

            Parametros filas = new Parametros("Filas");
            filas.DialogResult = DialogResult.No;

            while (filas.DialogResult == DialogResult.No)
            {
                if (filas.ShowDialog(this) == DialogResult.OK)
                {
                    if (filas.devolverParametro() < 0)
                    {
                        MessageBox.Show(Cadenas.error041);
                        filas.DialogResult = DialogResult.No;
                    }
                    else
                    {
                        if (filas.devolverParametro().ToString().Contains(','))
                        {
                            MessageBox.Show(Cadenas.error041);
                            filas.DialogResult = DialogResult.No;
                        }
                        else
                        {
                            nFilas = (int)filas.devolverParametro();
                        } 
                    }
                }
                else
                {
                    if (filas.DialogResult == DialogResult.No)
                    {
                        MessageBox.Show(Cadenas.error041);
                        filas.DialogResult = DialogResult.No;                        
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                double[] x;
                double[] y;
                int index, total = 0;
                string ecuacionFinal;
                SortedList<double, double> listaPuntosOrdenada;

                pathExcel = openFileDialog.FileName;
                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                object misValue = System.Reflection.Missing.Value;

                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(pathExcel, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                for (int i = 65; i <= 90; i++)
                {
                    char c = (char)i;
                    cambioDeColumna = false;
                    procesadoPuntosOK = true;
                    int ind = 1;
                    listaPuntosOrdenada = new SortedList<double, double>();

                    while (!cambioDeColumna && procesadoPuntosOK)
                    {
                        if (xlWorkSheet.get_Range(c.ToString() + ind.ToString(), c.ToString() + ind.ToString()).Value2 == null)
                        {
                            filasVacias++;
                        }
                        else
                        {
                            filasVacias = 0;
                            String puntoStr = xlWorkSheet.get_Range(c.ToString() + ind.ToString(), c.ToString() + ind.ToString()).Value2.ToString();
                            String[] puntoStrArray = puntoStr.Split('(', ')', ';');

                            double coordenadaX = 0, coordenadaY = 0;
                            
                            try
                            {
                                bool ok = double.TryParse(puntoStrArray[1], out coordenadaX);
                                
                                if (!ok)
                                {
                                    MessageBox.Show(Cadenas.error038a + " " + c + ", " + Cadenas.fila + " " + ind + " " + Cadenas.error042);
                                    procesadoPuntosOK = false;
                                }
                            }
                            catch(Exception msg)
                            {
                                MessageBox.Show(Cadenas.error038a + " " + c + ", " + Cadenas.fila + " " + ind + " " + Cadenas.error042);
                                procesadoPuntosOK = false;
                            }

                            
                            if (procesadoPuntosOK)
                            {
                                try
                                {
                                    bool ok = double.TryParse(puntoStrArray[2], out coordenadaY);
                                    
                                    if (!ok)
                                    {
                                        MessageBox.Show(Cadenas.error038a + " " + c + ", " + Cadenas.fila + " " + ind + " " + Cadenas.error042);
                                        procesadoPuntosOK = false;
                                    }
                                }
                                catch (Exception msg)
                                {
                                    MessageBox.Show(Cadenas.error038a + " " + c + ", " + Cadenas.fila + " " + ind + " " + Cadenas.error042);
                                    procesadoPuntosOK = false;
                                }
                            }
                            
                            if (procesadoPuntosOK)
                            {
                                try
                                {
                                    listaPuntosOrdenada.Add(coordenadaX, coordenadaY);
                                }
                                catch (Exception msg)
                                {
                                    MessageBox.Show(Cadenas.error039 + " " + c + ", " + Cadenas.fila + " " + ind + ". " + Cadenas.error043);
                                    procesadoPuntosOK = false;
                                }
                            }                            
                        }

                        if (nFilas == 0)
                        {
                            if (filasVacias >= 10)
                            {
                                cambioDeColumna = true;
                            }
                        }
                        else
                        {
                            if (ind >= nFilas)
                            {
                                cambioDeColumna = true;
                            }
                        }

                        ind++;
                    }

                    if ((listaPuntosOrdenada.Count > 0) && procesadoPuntosOK)
                    {
                        index = 0;
                        x = new double[listaPuntosOrdenada.Count];
                        IList<double> listadoX = listaPuntosOrdenada.Keys;
                        foreach (double temp in listadoX)
                        {
                            x[index] = temp;
                            index++;
                        }

                        index = 0;
                        y = new double[listaPuntosOrdenada.Count];
                        IList<double> listadoY = listaPuntosOrdenada.Values;
                        foreach (double temp in listadoY)
                        {
                            y[index] = temp;
                            index++;
                        }

                        InterpolacionPolinomica diferenciasDivididas = new InterpolacionPolinomica(x, y);
                        ecuacionFinal = diferenciasDivididas.resolver();
                        ecuacionFinal = ecuacionFinal.Replace(',', '.');

                        if (existe_funcion(ecuacionFinal))
                        {
                            MessageBox.Show(Cadenas.error022a + " " + ecuacionFinal + " " + Cadenas.error022b);
                        }
                        else
                        {
                            bool ok = genera_funcion(ecuacionFinal);
                            if (ok)
                            {
                                if (historialFunciones.Items.Count == 0)
                                {

                                    funcionesMenu.DropDownItems.Remove(funcionesMenu.DropDownItems[3]);
                                }

                                ListViewItem lvi = new ListViewItem("f(x) = " + ecuacionFinal);
                                string traduccion = traducirFuncion(ecuacionFinal);
                                lvi.SubItems.Add("f(x) = " + traduccion);
                                lvi.SubItems.Add(devuelveColorStr(obtenerColor().Name));
                                historialFunciones.Items.Add(lvi);
                                funcionesMenu.DropDownItems.Add(ecuacionFinal);
                                borrarPrimeraFuncion.Enabled = true;
                                borrarUltimaFuncion.Enabled = true;
                                Color color = obtenerColor();
                                Funcion aux = new Funcion(ecuacionFinal, idFuncion, color, false);
                                listadoFunciones.Add(aux);
                                idFuncion++;
                                total++;
                                textoFuncion.Text = "";
                            }
                        }
                    }
                }

                estadoLabel.Text = Cadenas.seHanImportado + " " + total + " " + Cadenas.funciones;

                xlWorkBook.Close(false, misValue, misValue);
                xlApp.Quit();

                releaseObject(xlWorkSheet);
                releaseObject(xlWorkBook);
                releaseObject(xlApp);
            }
        }

        private void importarNubesDePuntosDeExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = Cadenas.hojaDeFunciones;
            openFileDialog.Title = Cadenas.seleccionaUnaHojaDeCalculo;

            string titulo = Cadenas.importarNubeDePuntos;
            string advertencia = Cadenas.advertenciaCargarHistorialDePuntos;
                        
            if (historialPuntos.Items.Count > 0)
            {
                if (MessageBox.Show(advertencia, titulo, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            string pathExcel, colorStr;
            int colorInd = 1, nFilas = 0, filasVacias = 0, total = 0, nubesProcesadasOK = 0;
            bool cambioDeColumna, procesadoPuntosOK;

            Parametros filas = new Parametros("Filas");
            filas.DialogResult = DialogResult.No;

            while (filas.DialogResult == DialogResult.No)
            {
                if (filas.ShowDialog(this) == DialogResult.OK)
                {
                    if (filas.devolverParametro() < 0)
                    {
                        MessageBox.Show(Cadenas.error041);
                        filas.DialogResult = DialogResult.No;
                    }
                    else
                    {
                        if (filas.devolverParametro().ToString().Contains(','))
                        {
                            MessageBox.Show(Cadenas.error041);
                            filas.DialogResult = DialogResult.No;
                        }
                        else
                        {
                            nFilas = (int)filas.devolverParametro();
                        } 
                    }
                }
                else
                {
                    if (filas.DialogResult == DialogResult.No)
                    {
                        MessageBox.Show(Cadenas.error041);
                        filas.DialogResult = DialogResult.No;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SortedList<double, double> listaPuntosOrdenada;
                pathExcel = openFileDialog.FileName;
                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                object misValue = System.Reflection.Missing.Value;

                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(pathExcel, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                borrarPuntos_Click(new object(), new EventArgs());
                
                for (int i = 65; i <= 74; i++)
                {
                    cambioDeColumna = false;
                    procesadoPuntosOK = true;
                    char c = (char)i;
                    double ind = 1;
                    Color color;

                    listaPuntosOrdenada = new SortedList<double, double>();

                    while (!cambioDeColumna && procesadoPuntosOK)
                    {
                        if (xlWorkSheet.get_Range(c.ToString() + ind.ToString(), c.ToString() + ind.ToString()).Value2 == null)
                        {
                            filasVacias++;
                        }
                        else
                        {
                            filasVacias = 0;
                            double coordenadaY = 0;

                            try
                            {
                                coordenadaY = (double)xlWorkSheet.get_Range(c.ToString() + ind.ToString(), c.ToString() + ind.ToString()).Value2;
                                coordenadaY = Math.Round(coordenadaY, 2);
                            }
                            catch(Exception msg)
                            {
                                MessageBox.Show(Cadenas.error038a + " " + c + ", " + Cadenas.fila + " " + ind + " " + Cadenas.error044);
                                procesadoPuntosOK = false;
                            }

                            try
                            {
                                if (procesadoPuntosOK)
                                {
                                    listaPuntosOrdenada.Add(ind, coordenadaY);
                                }
                            }
                            catch (Exception msg)
                            {
                                MessageBox.Show(Cadenas.error045 + " " + c + ", " + Cadenas.fila + " " + ind + ". " + Cadenas.error043);
                                procesadoPuntosOK = false;
                            }
                        }

                        if (nFilas == 0)
                        {
                            if (filasVacias >= 10)
                            {
                                cambioDeColumna = true;
                            }
                        }
                        else
                        {
                            if (ind >= nFilas)
                            {
                                cambioDeColumna = true;
                            }
                        }

                        ind++;
                    }

                    if (listaPuntosOrdenada.Count > 0 && procesadoPuntosOK)
                    {
                        for (int j = 0; j < listaPuntosOrdenada.Count; j++)
                        {
                            switch (colorInd)
                            {
                                case 1:
                                    color = Color.Black;
                                    colorStr = Cadenas.negro;
                                    break;
                                case 2:
                                    color = Color.Blue;
                                    colorStr = Cadenas.azul;
                                    break;
                                case 3:
                                    color = Color.Red;
                                    colorStr = Cadenas.rojo;    
                                    break;
                                case 4:
                                    color = Color.Green;
                                    colorStr = Cadenas.verde;
                                    break;
                                case 5:
                                    color = Color.Brown;
                                    colorStr = Cadenas.marron;
                                    break;
                                case 6:
                                    color = Color.Violet;
                                    colorStr = Cadenas.violeta;    
                                    break;
                                case 7:
                                    color = Color.Orange;
                                    colorStr = Cadenas.naranja;
                                    break;
                                case 8:
                                    color = Color.Gray;
                                    colorStr = Cadenas.gris;
                                    break;
                                case 9:
                                    color = Color.DarkBlue;
                                    colorStr = Cadenas.azulOscuro;
                                    break;
                                case 10:
                                    color = Color.Magenta;
                                    colorStr = Cadenas.magenta;
                                    break;
                                default:
                                    color = Color.Black;
                                    colorStr = Cadenas.negro;
                                    break;
                            }

                            Punto p = new Punto(listaPuntosOrdenada.Keys[j], listaPuntosOrdenada.Values[j], true, color);

                            if (existe_punto(p))
                            {
                                MessageBox.Show(Cadenas.error052a + " " + c + " " + Cadenas.error052b + " (" + p.coordenadaX() + ", " + p.coordenadaY() + "). " + Cadenas.seVaACancelarLaImportacion);
                                return;
                            }
                            else
                            {
                                //Agregamos el punto en la lista, en el historial y en el menu
                                listadoPuntos.Add(p);

                                ListViewItem lvi = new ListViewItem("(" + String.Format("{0:0.00}", p.coordenadaX()) + "; " + String.Format("{0:0.00}", p.coordenadaY()) + ")");
                                lvi.SubItems.Add(colorStr);

                                historialPuntos.Items.Add(lvi);

                                string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                                if (puntosMenu.DropDownItems[3].ToString().CompareTo(noHayPuntos) == 0)
                                {
                                    quitarPrimerPuntoAñadido.Enabled = true;
                                    quitarUltimoPuntoAñadido.Enabled = true;
                                    puntosMenu.DropDownItems.RemoveAt(3);
                                }

                                puntosMenu.DropDownItems.Add("(" + String.Format("{0:0.00}", p.coordenadaX()) + "; " + String.Format("{0:0.00}", p.coordenadaY()) + ")");

                                //Y finalmente pintamos el punto
                                pintarPunto(p);
                                total++;
                            }
                        }

                        colorInd++;

                        if (procesadoPuntosOK)
                        {
                            nubesProcesadasOK++;
                        }
                    }
                }

                estadoLabel.Text = Cadenas.seHanImportado + " " + nubesProcesadasOK + " " + Cadenas.nubesDePuntos;

                xlWorkBook.Close(false, misValue, misValue);
                xlApp.Quit();

                releaseObject(xlWorkSheet);
                releaseObject(xlWorkBook);
                releaseObject(xlApp);
            }
        }

        private void importacionDeBlandYAltman_Click(object sender, EventArgs e)
        {
            string titulo = Cadenas.importarPuntosBlandYAltman;
            string advertencia = Cadenas.advertenciaCargarHistorialDePuntos;

            if (historialPuntos.Items.Count > 0)
            {
                if (MessageBox.Show(advertencia, titulo, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            importarPuntosBlandAltmanHoisan();
        }

        private void importacionDeHoisan_Click(object sender, EventArgs e)
        {
            string titulo = Cadenas.importarPuntosHoisan;
            string advertencia = Cadenas.advertenciaCargarHistorialDePuntos;

            if (historialPuntos.Items.Count > 0)
            {
                if (MessageBox.Show(advertencia, titulo, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            importarPuntosBlandAltmanHoisan();
        }

        private void importarPuntosBlandAltmanHoisan()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = Cadenas.hojaDeFunciones;
            openFileDialog.Title = Cadenas.seleccionaUnaHojaDeCalculo;

            string pathExcel;
            int nFilas = 0, filasVacias = 0;
            bool cambioDeFila = false;

            Parametros filas = new Parametros("Filas");
            filas.DialogResult = DialogResult.No;

            while (filas.DialogResult == DialogResult.No)
            {
                if (filas.ShowDialog(this) == DialogResult.OK)
                {
                    if (filas.devolverParametro() < 0)
                    {
                        MessageBox.Show(Cadenas.error041);
                        filas.DialogResult = DialogResult.No;
                    }
                    else
                    {
                        if (filas.devolverParametro().ToString().Contains(','))
                        {
                            MessageBox.Show(Cadenas.error041);
                            filas.DialogResult = DialogResult.No;
                        }
                        else
                        {
                            nFilas = (int)filas.devolverParametro();
                        }
                    }
                }
                else
                {
                    if (filas.DialogResult == DialogResult.No)
                    {
                        MessageBox.Show(Cadenas.error041);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SortedList<double, double> listaPuntosOrdenada;
                pathExcel = openFileDialog.FileName;
                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                object misValue = System.Reflection.Missing.Value;

                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(pathExcel, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                char charC = 'C';
                char charD = 'D';
                int ind = 2;
                listaPuntosOrdenada = new SortedList<double, double>();

                while (!cambioDeFila)
                {
                    if ((xlWorkSheet.get_Range(charC.ToString() + ind.ToString(), charC.ToString() + ind.ToString()).Value2 == null) || (xlWorkSheet.get_Range(charD.ToString() + ind.ToString(), charD.ToString() + ind.ToString()).Value2 == null))
                    {
                        filasVacias++;
                    }
                    else
                    {
                        filasVacias = 0;
                        double coordenadaX, coordenadaY;

                        try
                        {
                            coordenadaX = (double)xlWorkSheet.get_Range(charC.ToString() + ind.ToString(), charC.ToString() + ind.ToString()).Value2;
                            coordenadaX = Math.Round(coordenadaX, 2);
                        }
                        catch (Exception msg)
                        {
                            MessageBox.Show(Cadenas.error038a + " " + charC + ", " + Cadenas.fila + " " + ind + " " + Cadenas.error038b);
                            return;
                        }

                        try
                        {
                            coordenadaY = (double)xlWorkSheet.get_Range(charD.ToString() + ind.ToString(), charD.ToString() + ind.ToString()).Value2;
                            coordenadaY = Math.Round(coordenadaY, 2);
                        }
                        catch (Exception msg)
                        {
                            MessageBox.Show(Cadenas.error038a + " " + charD + ", " + Cadenas.fila + " " + ind + " " + Cadenas.error038b);
                            return;
                        }

                        if (!listaPuntosOrdenada.ContainsKey(coordenadaX))
                        {
                            try
                            {
                                listaPuntosOrdenada.Add(coordenadaX, coordenadaY);
                            }
                            catch (ArgumentException msg)
                            {
                                MessageBox.Show(Cadenas.error039 + " " + charC + ", " + Cadenas.fila + " " + ind);
                                return;
                            }
                            catch (IndexOutOfRangeException msg)
                            {
                                MessageBox.Show(Cadenas.error039 + " " + charC + ", " + Cadenas.fila + " " + ind);
                                return;
                            }
                        }
                    }

                    if (nFilas == 0)
                    {
                        if (filasVacias >= 10)
                        {
                            cambioDeFila = true;
                        }
                    }
                    else
                    {
                        if (ind >= nFilas + 1)
                        {
                            cambioDeFila = true;
                        }
                    }

                    ind++;
                }

                if (listaPuntosOrdenada.Count > 1)
                {
                    // En caso de haber importado puntos, se borran los puntos previos tal y como se advirtió
                    Graphics gr = panelInt.CreateGraphics();
                    ficheroPuntos = " ";
                    historialPuntos.Items.Clear();
                    listadoPuntos = new List<Punto>();
                    pintarEje(gr);
                    textoFuncion.Text = "";
                    cajaX.Text = "";
                    cajaY.Text = "";

                    foreach (Funcion f in listadoFunciones)
                    {
                        if (f.devuelve_mostrar() == true)
                        {
                            pintarFuncion(f, gr);
                        }
                    }

                    quitarPrimerPuntoAñadido.Enabled = true;
                    quitarUltimoPuntoAñadido.Enabled = true;

                    int i = 0, total = 0;
                    while (i < puntosMenu.DropDownItems.Count)
                    {
                        string noHayPuntos = Cadenas.noHayPuntosEnElHistorial;

                        if ((i == 3) && (funcionesMenu.DropDownItems[3].ToString().CompareTo(noHayPuntos) != 0))
                        {
                            puntosMenu.DropDownItems.RemoveAt(3);
                        }
                        else
                        {
                            i++;
                        }
                    }

                    //A continuación comienza la inserción de puntos importados en el programa
                    for (int j = 0; j < listaPuntosOrdenada.Count; j++)
                    {
                        Punto p = new Punto(listaPuntosOrdenada.Keys[j], listaPuntosOrdenada.Values[j], true, obtenerColor());

                        if (existe_punto(p))
                        {
                            MessageBox.Show(Cadenas.error052a + " " + devuelveColorStr(obtenerColor().Name) + " " + Cadenas.error052b + " (" + p.coordenadaX() + ", " + p.coordenadaY() + "). " + Cadenas.seVaACancelarLaImportacion);
                            return;
                        }
                        else
                        {
                            //Agregamos el punto en la lista, en el historial y en el menu
                            listadoPuntos.Add(p);
                            ListViewItem lvi = new ListViewItem("(" + String.Format("{0:0.00}", p.coordenadaX()) + "; " + String.Format("{0:0.00}", p.coordenadaY()) + ")");
                            lvi.SubItems.Add(devuelveColorStr(obtenerColor().Name));
                            historialPuntos.Items.Add(lvi);
                            puntosMenu.DropDownItems.Add("(" + String.Format("{0:0.00}", p.coordenadaX()) + "; " + String.Format("{0:0.00}", p.coordenadaY()) + ")");

                            //Y finalmente pintamos el punto
                            pintarPunto(p);
                            total++;
                        }
                    }

                    //Se informa de cuántos puntos se ha insertado en el programa
                    estadoLabel.Text = Cadenas.seHanCargadoConExito + " " + total + " " + Cadenas.puntosMinuscula;

                    //Si no se ha cargado ningún punto, se vuelve a añadir el mensaje
                    //en el menú de puntos "No hay puntos en el historial"
                    if (puntosMenu.DropDownItems.Count == 3)
                    {
                        puntosMenu.DropDownItems.Add(Cadenas.noHayPuntosEnElHistorial);
                        puntosMenu.DropDownItems[0].Enabled = false;
                        puntosMenu.DropDownItems[1].Enabled = false;
                        puntosMenu.DropDownItems[3].Enabled = false;
                    }
                }
                else
                {
                    MessageBox.Show(Cadenas.error040);
                    return;
                }

                xlWorkBook.Close(false, misValue, misValue);
                xlApp.Quit();

                releaseObject(xlWorkSheet);
                releaseObject(xlWorkBook);
                releaseObject(xlApp);
            }
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show(Cadenas.error046 + " " + ex.ToString());                
            }
            finally
            {
                GC.Collect();
            }
        }

        private void español_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("");
            traducirControles(this);
            traducirContenidoGeneral();
            traducirColumnasHistoriales();
            traducirBarraDeEstado();
            cambiarIdiomaFunciones();
            cambiarIdiomaPuntos();
            textoFuncion_TextChanged(sender, e);
        }
        
        private void ingles_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
            traducirControles(this);
            traducirContenidoGeneral();
            traducirColumnasHistoriales();
            traducirBarraDeEstado();
            cambiarIdiomaFunciones();
            cambiarIdiomaPuntos();
            textoFuncion_TextChanged(sender, e);
        }

        private void portugues_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pt");
            traducirControles(this);
            traducirContenidoGeneral();
            traducirColumnasHistoriales();
            traducirBarraDeEstado();
            cambiarIdiomaFunciones();
            cambiarIdiomaPuntos();
            textoFuncion_TextChanged(sender, e);
        }

        public void traducirControles(Form form)
        {
            ComponentResourceManager resources = new ComponentResourceManager(form.GetType());
            resources.ApplyResources(this, "$this");

            List<Control> availControls = obtenerControles(form);

            foreach (Control c in availControls)
            {
                resources.ApplyResources(c, c.Name);
            }
        }

        public static List<Control> obtenerControles(Control form)
        {
            var controlList = new List<Control>();

            foreach (Control childControl in form.Controls)
            {
                //Recurse child controls
                controlList.AddRange(obtenerControles(childControl));
                controlList.Add(childControl);
            }
            return controlList;
        }

        public void traducirContenidoGeneral()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(VentanaPrincipal));

            //Traducción de formulario en general
            foreach (ToolStripItem i in menuGeneral.Items)
            {
                resources.ApplyResources(i, i.Name);
            }

            //Traducción de menú Archivo
            for (int i = 0; i < 15; i++)
            {
                switch (i)
                {
                    case 4:
                    case 9:
                    case 11:
                    case 13:
                        break;
                    case 10:
                        foreach (ToolStripItem j in menpas.DropDownItems)
                        {
                            resources.ApplyResources(j, j.Name);
                        }
                        resources.ApplyResources(archivo.DropDownItems[i], archivo.DropDownItems[i].Name);
                        break;
                    default:
                        resources.ApplyResources(archivo.DropDownItems[i], archivo.DropDownItems[i].Name);
                        break;
                }
            }
            
            //Traducción de menú Opciones
            for (int i = 0; i < 13; i++)
            {
                switch (i)
                {
                    case 1:
                        resources.ApplyResources(funcionesMenu.DropDownItems[0], funcionesMenu.DropDownItems[0].Name);
                        resources.ApplyResources(funcionesMenu.DropDownItems[1], funcionesMenu.DropDownItems[1].Name);
                        
                        switch (funcionesMenu.DropDownItems[3].Text)
                        {                            
                            case "(No hay funciones en el historial)":
                            case "(No functions in the history)":
                            case "(Nenhuma das funções de história)":
                                resources.ApplyResources(funcionesMenu.DropDownItems[3], funcionesMenu.DropDownItems[3].Name);
                                break;
                            default:
                                break;
                        }
                        resources.ApplyResources(opciones.DropDownItems[i], opciones.DropDownItems[i].Name);
                        break;
                    case 4:
                        resources.ApplyResources(puntosMenu.DropDownItems[0], puntosMenu.DropDownItems[0].Name);
                        resources.ApplyResources(puntosMenu.DropDownItems[1], puntosMenu.DropDownItems[1].Name);
                        switch (puntosMenu.DropDownItems[3].Text)
                        {
                            case "(No hay puntos en el historial)":
                            case "(No points in the history)":
                            case "(Nenhuma das pontos de história)":
                                resources.ApplyResources(puntosMenu.DropDownItems[3], puntosMenu.DropDownItems[3].Name);
                                break;
                            default:
                                break;
                        }
                        resources.ApplyResources(opciones.DropDownItems[i], opciones.DropDownItems[i].Name);
                        break;
                    case 6:
                        foreach (ToolStripMenuItem j in importarMenu.DropDownItems)
                        {
                            resources.ApplyResources(j, j.Name);
                        }
                        resources.ApplyResources(opciones.DropDownItems[i], opciones.DropDownItems[i].Name);
                        break;
                    case 8:
                        foreach (ToolStripMenuItem j in idiomaMenu.DropDownItems)
                        {
                            resources.ApplyResources(j, j.Name);
                        }
                        resources.ApplyResources(opciones.DropDownItems[i], opciones.DropDownItems[i].Name);
                        break;
                    case 10:
                        aumentarZoom.Text = Cadenas.aumentarZoom + " (" + zoom_posterior + "%)";
                        disminuirZoom.Text = Cadenas.disminuirZoom + " (" + zoom_anterior + "%)";
                        break;
                    case 12:
                        resources.ApplyResources(fijarDesplazamiento, fijarDesplazamiento.Name);
                        resources.ApplyResources(situarPanelEnCoordenadas, situarPanelEnCoordenadas.Name);
                        resources.ApplyResources(desplazamientoMenu, desplazamientoMenu.Name);
                        break;
                    case 2:
                    case 5:
                    case 7:
                    case 9:
                    case 11:
                        break;
                    default:
                        resources.ApplyResources(opciones.DropDownItems[i], opciones.DropDownItems[i].Name);
                        break;
                }               
            }

            //Traducción de menú Ayuda
            for (int i = 0; i < 2; i++)
            {
                resources.ApplyResources(ayuda.DropDownItems[i], ayuda.DropDownItems[i].Name);
            }

            //Traducción de menú contextual de historial de funciones
            foreach (ToolStripItem tsi in menuHistorialFunciones.Items)
            {
                resources.ApplyResources(tsi, tsi.Name);
            }

            resources.ApplyResources(funcionNegro, funcionNegro.Name);
            resources.ApplyResources(funcionAzul, funcionAzul.Name);
            resources.ApplyResources(funcionRojo, funcionRojo.Name);
            resources.ApplyResources(funcionVerde, funcionVerde.Name);
            resources.ApplyResources(funcionMarron, funcionMarron.Name);
            resources.ApplyResources(funcionVioleta, funcionVioleta.Name);
            resources.ApplyResources(funcionNaranja, funcionNaranja.Name);
            resources.ApplyResources(funcionGris, funcionGris.Name);
            resources.ApplyResources(funcionAzulOscuro, funcionAzulOscuro.Name);
            resources.ApplyResources(funcionMagenta, funcionMagenta.Name);

            colorDeFuncion.DropDownItems.RemoveAt(0);
            colorDeFuncion.DropDownItems.RemoveAt(0);
            colorDeFuncion.DropDownItems.RemoveAt(0);
            colorDeFuncion.DropDownItems.RemoveAt(0);
            colorDeFuncion.DropDownItems.RemoveAt(0);
            colorDeFuncion.DropDownItems.RemoveAt(0);
            colorDeFuncion.DropDownItems.RemoveAt(0);
            colorDeFuncion.DropDownItems.RemoveAt(0);
            colorDeFuncion.DropDownItems.RemoveAt(0);
            colorDeFuncion.DropDownItems.RemoveAt(0);

            colorDeFuncion.DropDownItems.Add(funcionNegro.Text);
            colorDeFuncion.DropDownItems.Add(funcionAzul.Text);
            colorDeFuncion.DropDownItems.Add(funcionRojo.Text);
            colorDeFuncion.DropDownItems.Add(funcionVerde.Text);
            colorDeFuncion.DropDownItems.Add(funcionMarron.Text);
            colorDeFuncion.DropDownItems.Add(funcionVioleta.Text);
            colorDeFuncion.DropDownItems.Add(funcionNaranja.Text);
            colorDeFuncion.DropDownItems.Add(funcionGris.Text);
            colorDeFuncion.DropDownItems.Add(funcionAzulOscuro.Text);
            colorDeFuncion.DropDownItems.Add(funcionMagenta.Text);
            
            //Traducción de menú contextual de historial de puntos
            foreach (ToolStripItem tsi in menuHistorialPuntos.Items)
            {
                resources.ApplyResources(tsi, tsi.Name);
            }

            resources.ApplyResources(puntoNegro, puntoNegro.Name);
            resources.ApplyResources(puntoAzul, puntoAzul.Name);
            resources.ApplyResources(puntoRojo, puntoRojo.Name);
            resources.ApplyResources(puntoVerde, puntoVerde.Name);
            resources.ApplyResources(puntoMarron, puntoMarron.Name);
            resources.ApplyResources(puntoVioleta, puntoVioleta.Name);
            resources.ApplyResources(puntoNaranja, puntoNaranja.Name);
            resources.ApplyResources(puntoGris, puntoGris.Name);
            resources.ApplyResources(puntoAzulOscuro, puntoAzulOscuro.Name);
            resources.ApplyResources(puntoMagenta, puntoMagenta.Name);

            colorDePunto.DropDownItems.RemoveAt(0);
            colorDePunto.DropDownItems.RemoveAt(0);
            colorDePunto.DropDownItems.RemoveAt(0);
            colorDePunto.DropDownItems.RemoveAt(0);
            colorDePunto.DropDownItems.RemoveAt(0);
            colorDePunto.DropDownItems.RemoveAt(0);
            colorDePunto.DropDownItems.RemoveAt(0);
            colorDePunto.DropDownItems.RemoveAt(0);
            colorDePunto.DropDownItems.RemoveAt(0);
            colorDePunto.DropDownItems.RemoveAt(0);

            colorDePunto.DropDownItems.Add(puntoNegro.Text);
            colorDePunto.DropDownItems.Add(puntoAzul.Text);
            colorDePunto.DropDownItems.Add(puntoRojo.Text);
            colorDePunto.DropDownItems.Add(puntoVerde.Text);
            colorDePunto.DropDownItems.Add(puntoMarron.Text);
            colorDePunto.DropDownItems.Add(puntoVioleta.Text);
            colorDePunto.DropDownItems.Add(puntoNaranja.Text);
            colorDePunto.DropDownItems.Add(puntoGris.Text);
            colorDePunto.DropDownItems.Add(puntoAzulOscuro.Text);
            colorDePunto.DropDownItems.Add(puntoMagenta.Text);

            //Traducción de tipos de funciones
            tipoAprox.Items.RemoveAt(0);
            tipoAprox.Items.RemoveAt(0);
            tipoAprox.Items.RemoveAt(0);
            tipoAprox.Items.RemoveAt(0);
            tipoAprox.Items.RemoveAt(0);
            tipoAprox.Items.Add(Cadenas.regresionLineal);
            tipoAprox.Items.Add(Cadenas.regresionPolinomica);
            tipoAprox.Items.Add(Cadenas.regresionExponencial);
            tipoAprox.Items.Add(Cadenas.regresionLogaritmica);
            tipoAprox.Items.Add(Cadenas.interpolacionPolinomica);
            tipoAprox.Text = Cadenas.tipoDeFuncion;

            //Traducción de tipos de puntos
            color.Items.RemoveAt(0);
            color.Items.RemoveAt(0);
            color.Items.RemoveAt(0);
            color.Items.RemoveAt(0);
            color.Items.RemoveAt(0);
            color.Items.RemoveAt(0);
            color.Items.RemoveAt(0);
            color.Items.RemoveAt(0);
            color.Items.RemoveAt(0);
            color.Items.RemoveAt(0);
            color.Items.Add(Cadenas.negro);
            color.Items.Add(Cadenas.rojo);
            color.Items.Add(Cadenas.azul);
            color.Items.Add(Cadenas.verde);
            color.Items.Add(Cadenas.marron);
            color.Items.Add(Cadenas.violeta);
            color.Items.Add(Cadenas.naranja);
            color.Items.Add(Cadenas.gris);
            color.Items.Add(Cadenas.azulOscuro);
            color.Items.Add(Cadenas.magenta);
            color.Text = Cadenas.color;
        }

        public void traducirColumnasHistoriales()
        {
            //Columnas historial de funciones
            historialFunciones.Columns[0].Text = Cadenas.definicionFormal;
            historialFunciones.Columns[1].Text = Cadenas.definicionEstandard;
            historialFunciones.Columns[2].Text = Cadenas.color;

            //Columnas historial de puntos
            historialPuntos.Columns[0].Text = Cadenas.puntosMayuscula;
            historialPuntos.Columns[1].Text = Cadenas.color;
        }

        public void traducirBarraDeEstado()
        {
            estadoLabel.Text = Cadenas.estadoDePrograma;
            coordenadaLabel.Text = Cadenas.coordenadas + ":";
            desplazamientoLabel.Text = Cadenas.desplazamiento + ":" + desplazamiento;
            zoomLabel.Text = "Zoom: " + zoom + "%";
        }

        private void cambiarIdiomaFunciones()
        {
            for (int i = 0; i < historialFunciones.Items.Count; i++)
            {
                ListViewItem lvi = historialFunciones.Items[i];
                System.Windows.Forms.ListViewItem.ListViewSubItem lvsic = lvi.SubItems[2];
                lvsic.Text = devuelveColorStr(lvsic.Text);
                historialFunciones.Items[i].SubItems[2].Text = lvsic.Text;
            }
        }

        private void cambiarIdiomaPuntos()
        {
            for (int i = 0; i < historialPuntos.Items.Count; i++)
            {
                ListViewItem lvi = historialPuntos.Items[i];
                System.Windows.Forms.ListViewItem.ListViewSubItem lvsic = lvi.SubItems[1];
                lvsic.Text = devuelveColorStr(lvsic.Text);
                historialPuntos.Items[i].SubItems[1].Text = lvsic.Text;
            }
        }

        private string devuelveColorStr(string c)
        {
            switch (c)
            {
                case "Negro":
                case "Black":
                case "Preto":
                    return Cadenas.negro;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Negro";
                        case "inglés":
                            return "Black";
                        case "portugués":
                            return "Preto";
                    }*/
                case "Azul":
                case "Blue":
                    return Cadenas.azul;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Azul";
                        case "inglés":
                            return "Blue";
                        case "portugués":
                            return "Azul";
                    }*/
                case "Rojo":
                case "Red":
                case "Vermelho":
                    return Cadenas.rojo;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Rojo";
                        case "inglés":
                            return "Red";
                        case "portugués":
                            return "Vermelho";
                    }*/
                case "Verde":
                case "Green":
                    return Cadenas.verde;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Verde";
                        case "inglés":
                            return "Green";
                        case "portugués":
                            return "Verde";
                    }*/                    
                case "Marrón":
                case "Brown":
                case "Marrom":
                    return Cadenas.marron;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Marrón";
                        case "inglés":
                            return "Brown";
                        case "portugués":
                            return "Marrom";
                    }*/
                case "Violeta":
                case "Violet":
                    return Cadenas.violeta;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Violeta";
                        case "inglés":
                            return "Violet";
                        case "portugués":
                            return "Violeta";
                    }*/
                case "Naranja":
                case "Orange":
                case "Laranja":
                    return Cadenas.naranja;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Naranja";
                        case "inglés":
                            return "Orange";
                        case "portugués":
                            return "Laranja";
                    }*/
                case "Gris":
                case "Gray":
                case "Cinza":
                    return Cadenas.gris;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Gris";
                        case "inglés":
                            return "Gray";
                        case "portugués":
                            return "Cinza";
                    }*/
                case "Azul oscuro":
                case "DarkBlue":
                case "Azul escuro":
                    return Cadenas.azulOscuro;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Azul oscuro";
                        case "inglés":
                            return "Dark blue";
                        case "portugués":
                            return "Azul escuro";
                    }*/
                case "Magenta":
                    return Cadenas.magenta;
                    /*switch (idioma)
                    {
                        case "español":
                            return "Magenta";
                        case "inglés":
                            return "Magenta";
                        case "portugués":
                            return "Magenta";
                    }*/
                /*case "Pink":
                    switch (idioma)
                    {
                        case "español":
                            return "Rosa";
                        case "inglés":
                            return "Pink";
                        case "portugués":
                            return "Rosa";
                    }
                    break;
                case "Purple":
                    switch (idioma)
                    {
                        case "español":
                            return "Morado";
                        case "inglés":
                            return "Purple";
                        case "portugués":
                            return "Roxo";
                    }
                    break;
                case "DarkRed":
                    switch (idioma)
                    {
                        case "español":
                            return "Rojo oscuro";
                        case "inglés":
                            return "Dark red";
                        case "portugués":
                            return "Vermelho escuro";
                    }
                    break;
                case "Turquoise":
                    switch (idioma)
                    {
                        case "español":
                            return "Turquesa";
                        case "inglés":
                            return "Turquoise";
                        case "portugués":
                            return "Turquesa";
                    }
                    break;
                case "GreenYellow":
                    switch (idioma)
                    {
                        case "español":
                            return "Verde claro";
                        case "inglés":
                            return "Green yellow";
                        case "portugués":
                            return "Luz verde";
                    }
                    break;
                case "Wheat":
                    switch (idioma)
                    {
                        case "español":
                            return "Trigo";
                        case "inglés":
                            return "Wheat";
                        case "portugués":
                            return "Trigo";
                    }
                    break;
                case "Gold":
                    switch (idioma)
                    {
                        case "español":
                            return "Oro";
                        case "inglés":
                            return "Gold";
                        case "portugués":
                            return "Ouro";
                    }
                    break;
                case "PaleTurquoise":
                    switch (idioma)
                    {
                        case "español":
                            return "Celeste";
                        case "inglés":
                            return "Pale turquoise";
                        case "portugués":
                            return "Celestial";
                    }
                    break;
                case "Tomato":
                    switch (idioma)
                    {
                        case "español":
                            return "Tomate";
                        case "inglés":
                            return "Tomato";
                        case "portugués":
                            return "Tomate";
                    }
                    break;
                case "Khaki":
                    switch (idioma)
                    {
                        case "español":
                            return "Caqui";
                        case "inglés":
                            return "Khaki";
                        case "portugués":
                            return "Caqui";
                    }
                    break;
                case "Yellow":
                    switch (idioma)
                    {
                        case "español":
                            return "Amarillo";
                        case "inglés":
                            return "Yellow";
                        case "portugués":
                            return "Amarelo";
                    }
                    break;
                 */
                default: return Cadenas.negro;
            }
        }

        private double distanciaPuntos(Punto p1, Punto p2)
        {
            return Math.Sqrt((Math.Pow(p2.coordenadaX() - p1.coordenadaX(), 2) + Math.Pow(p2.coordenadaY() - p1.coordenadaY(), 2)));
        }

        private void panelInt_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //Calculamos sus coordenadas
                double x_revisado = (e.X / zoom) * 100;
                double x = x_revisado - ((anchuraTeorica / 2)) + ((x_inicio + x_fin) / 2);
                x = Math.Round(x, 2);

                double y_revisado = (e.Y / zoom) * 100;
                double y = (y_revisado * (-1)) + ((alturaTeorica / 2)) + ((y_inicio + y_fin) / 2);
                y = Math.Round(y, 2);

                listadoPuntosDibujo = new List<Punto>();
                p_inicio = new Punto(x, y, true, Color.Black);
                p_fin = new Punto(x, y, true, Color.Black);
                listadoPuntosDibujo.Add(p_inicio);
                pulsado = true;
            }
        }

        private void panelInt_MouseMove(object sender, MouseEventArgs e)
        {
            double x_revisado = (e.X / zoom) * 100;
            double x = x_revisado - ((anchuraTeorica / 2)) + ((x_inicio + x_fin) / 2);
            x = Math.Round(x, 2);

            double y_revisado = (e.Y / zoom) * 100;
            double y = (y_revisado * (-1)) + ((alturaTeorica / 2)) + ((y_inicio + y_fin) / 2);
            y = Math.Round(y, 2);

            coordenadaLabel.Text = Cadenas.coordenadas + ": (" + String.Format("{0:0.00}", x) + "; " + String.Format("{0:0.00}", y) + ")";
            
            if ((pulsado == true) && (e.Button == MouseButtons.Left))
            {
                Pen blackPen = new Pen(obtenerColor(), 2);
                Graphics gr;
                gr = panelInt.CreateGraphics();
                try
                {
                    gr.DrawLine(blackPen, e.X, e.Y, e.X + 1, e.Y + 1);
                }
                catch (Exception msg)
                {

                }

                Punto aux = new Punto(x, y, true, Color.Black);
                double dis = distanciaPuntos(p_fin, aux);

                if (dis > 5)
                {
                    listadoPuntosDibujo.Add(aux);
                    p_fin = aux;
                }

                gr.Dispose();
            }
        }

        private void panelInt_MouseUp(object sender, MouseEventArgs e)
        {
            //Si se trata del botón derecho del ratón, se ignora el evento
            if (e.Button == MouseButtons.Right)
            {
                return;
            }

            //Preparación de los array's ordenados de coordenadas X y coordenadas Y
            //para la implementación de la aproximación
            bool ok;

            double x_revisado = (e.X / zoom) * 100;
            double x_temp = x_revisado - ((anchuraTeorica / 2)) + ((x_inicio + x_fin) / 2);
            x_temp = Math.Round(x_temp, 2);

            double y_revisado = (e.Y / zoom) * 100;
            double y_temp = (y_revisado * (-1)) + ((alturaTeorica / 2)) + ((y_inicio + y_fin) / 2);
            y_temp = Math.Round(y_temp, 2);

            p_fin = new Punto(x_temp, y_temp, true, Color.Black);

            if (distanciaPuntos(p_inicio, p_fin) < 20)
            {
                return;
            }

            /*if ((p_inicio.coordenadaX() != x_temp) || (p_inicio.coordenadaY() != y_temp))
            {
                if (!listadoPuntosDibujo.Contains(new Punto(x_temp, y_temp, true, Color.Black)))
                {
                    listadoPuntosDibujo.Add(new Punto(x_temp, y_temp, true, Color.Black));
                }
            }
            else
            {
                return;
            }*/

            SortedList<double, double> listaPuntosOrdenada = new SortedList<double, double>();
            string ecuacionFinal = "";
            double[] x;
            double[] y;
            int index = 0;

            foreach (Punto punto in listadoPuntosDibujo)
            {
                try
                {
                    listaPuntosOrdenada.Add(punto.coordenadaX(), punto.coordenadaY());
                }
                catch (ArgumentException)
                {
                    MessageBox.Show(Cadenas.error047);
                    
                    pintarReglaYEje();
                    foreach (Funcion f in listadoFunciones)
                    {
                        if (f.devuelve_mostrar() == true)
                        {
                            Graphics gr = panelInt.CreateGraphics();
                            pintarFuncion(f, gr);
                        }
                    }
                    foreach (Punto p in listadoPuntos)
                    {
                        if (p.devuelve_mostrar() == true)
                        {
                            pintarPunto(p);
                        }
                    }

                    return;
                }
            }

            //Se comprueba que hayan dos o más puntos del color seleccionado
            if (listaPuntosOrdenada.Count <= 1)
            {
                MessageBox.Show(Cadenas.error048);
                return;
            }

            x = new double[listaPuntosOrdenada.Count];
            IList<double> listadoX = listaPuntosOrdenada.Keys;
            foreach (int temp in listadoX)
            {
                x[index] = temp;
                index++;
            }

            index = 0;
            y = new double[listaPuntosOrdenada.Count];
            IList<double> listadoY = listaPuntosOrdenada.Values;
            foreach (int temp in listadoY)
            {
                y[index] = temp;
                index++;
            }

            //Implementación de la aproximación dependiendo del tipo
            if (tipoAprox.SelectedIndex == -1)
            {
                MessageBox.Show(Cadenas.error049);
                
                pintarReglaYEje();
                foreach (Funcion f in listadoFunciones)
                {
                    if (f.devuelve_mostrar() == true)
                    {
                        Graphics gr = panelInt.CreateGraphics();
                        pintarFuncion(f, gr);
                    }
                }
                foreach (Punto p in listadoPuntos)
                {
                    if (p.devuelve_mostrar() == true)
                    {
                        pintarPunto(p);
                    }
                }

                return;
            }

            int i;
            switch (tipoAprox.SelectedItem.ToString())
            {
                case "Regresión lineal":
                case "Lineal regression":
                case "Regressão linear":
                    RegresionEstandar regresionLineal = new RegresionEstandar(x, y, "Lineal");
                    regresionLineal.resolver();
                    ecuacionFinal = regresionLineal.a + " + " + "x * " + regresionLineal.b;
                    ecuacionFinal = ecuacionFinal.Replace(',', '.');
                    break;

                case "Regresión exponencial":
                case "Exponential regression":
                case "Regressão exponencial":
                    for (i = 0; i < x.Count(); i++)
                    {
                        if (y[i] <= 0)
                        {
                            MessageBox.Show(Cadenas.error050);
                            return;
                        }
                    }

                    RegresionEstandar regresionExponencial = new RegresionEstandar(x, y, "Exponencial");
                    regresionExponencial.resolver();
                    String aExp, bExp;
                    aExp = regresionExponencial.a.ToString();
                    aExp = aExp.Replace(',', '.');
                    bExp = regresionExponencial.b.ToString();
                    bExp = bExp.Replace(',', '.');
                    ecuacionFinal = aExp + " * " + "Math.Pow(" + bExp + ", x)";
                    break;

                case "Regresión logarítmica":
                case "Logarithmic regression":
                case "Regressão logarítmica":
                    for (i = 0; i < x.Count(); i++)
                    {
                        if (x[i] <= 0)
                        {
                            MessageBox.Show(Cadenas.error051);
                            return;
                        }
                    }

                    RegresionEstandar regresionLogaritmica = new RegresionEstandar(x, y, "Logaritmica");
                    regresionLogaritmica.resolver();
                    String aLog, bLog;
                    aLog = regresionLogaritmica.a.ToString();
                    aLog = aLog.Replace(',', '.');
                    bLog = regresionLogaritmica.b.ToString();
                    bLog = bLog.Replace(',', '.');
                    ecuacionFinal = bLog + " * " + "Math.Log(x) + (" + aLog + ")";
                    break;

                case "Regresión polinómica":
                case "Polynomial regression":
                case "Regressão polinomial":
                    Parametros coeficiente = new Parametros("Coeficiente");
                    coeficiente.DialogResult = DialogResult.No;
                    
                    while (coeficiente.DialogResult == DialogResult.No)
                    {
                        if (coeficiente.ShowDialog(this) == DialogResult.OK)
                        {
                            if (coeficiente.devolverParametro() <= 1)
                            {
                                MessageBox.Show(Cadenas.error011);
                                coeficiente.DialogResult = DialogResult.No;
                            }
                            else
                            {
                                if (coeficiente.devolverParametro().ToString().Contains(','))
                                {
                                    MessageBox.Show(Cadenas.error011);
                                    coeficiente.DialogResult = DialogResult.No;
                                }
                            }
                        }
                        else
                        {
                            if (coeficiente.DialogResult == DialogResult.No)
                            {
                                MessageBox.Show(Cadenas.error011);                                
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    MathNet.Numerics.LinearAlgebra.Vector vectorX = new MathNet.Numerics.LinearAlgebra.Vector(x);
                    MathNet.Numerics.LinearAlgebra.Vector vectorY = new MathNet.Numerics.LinearAlgebra.Vector(y);
                    RegresionPolinomica regresionPolinomial = new RegresionPolinomica(vectorX, vectorY, (int)Math.Truncate(coeficiente.devolverParametro()));
                    MathNet.Numerics.LinearAlgebra.Vector coeficientes = regresionPolinomial.Coefficients;
                    String[] coef = new String[coeficientes.Length];
                    for (i = 0; i < coeficientes.Length; i++)
                    {
                        coef[i] = coeficientes[i].ToString().Replace(',', '.');
                    }

                    for (i = coeficientes.Length - 1; i >= 0; i--)
                    {
                        if (i == 0)
                        {
                            ecuacionFinal = ecuacionFinal + "(" + coef[0] + ")";
                        }
                        else
                        {
                            ecuacionFinal = ecuacionFinal + "(" + coef[i] + " * Math.Pow(x, " + i + ")) + ";
                        }
                    }
                    break;

                case "Interpolación polinómica":
                case "Polynomial interpolation":
                case "Interpolação polinomial":
                    InterpolacionPolinomica diferenciasDivididas = new InterpolacionPolinomica(x, y);
                    ecuacionFinal = diferenciasDivididas.resolver();
                    ecuacionFinal = ecuacionFinal.Replace(',', '.');
                    break;
            }
            
            //Se devuelve el resultado de la ecuación final a la caja de texto
            ok = genera_funcion(ecuacionFinal);
            if (ok)
            {
                if (historialFunciones.Items.Count == 0)
                {
                    funcionesMenu.DropDownItems.Remove(funcionesMenu.DropDownItems[3]);
                }

                Funcion aux;
                ListViewItem lvi = new ListViewItem("f(x) = " + ecuacionFinal);
                string traduccion = traducirFuncion(ecuacionFinal);
                lvi.SubItems.Add("f(x) = " + traduccion);
                lvi.SubItems.Add(devuelveColorStr(obtenerColor().Name));
                historialFunciones.Items.Add(lvi);
                funcionesMenu.DropDownItems.Add(ecuacionFinal);
                borrarPrimeraFuncion.Enabled = true;
                borrarUltimaFuncion.Enabled = true;
                Color color = obtenerColor();
                aux = new Funcion(ecuacionFinal, idFuncion, color, true);
                listadoFunciones.Add(aux);
                Graphics gr = panelInt.CreateGraphics();
                pintarFuncion(aux, gr);
                idFuncion++;
                estadoLabel.Text = Cadenas.evaluacionCorrectaDeFuncion + " => " + traduccion;
                
                //Se termina el proceso desactivando el interruptor
                pulsado = false;
            }
        }

        private void ayuda_Click(object sender, EventArgs e)
        {
            try
            {
                String PathCarpeta = AppDomain.CurrentDomain.BaseDirectory;
                Process pr = new Process();
                pr.StartInfo.FileName = PathCarpeta + "Ayuda.chm";
                pr.Start();
            }
            catch (System.ComponentModel.Win32Exception msg)
            {
                MessageBox.Show("Error de acceso al fichero de ayuda. Pruebe a desbloquear el fichero \"Ayuda.chm\" dentro del fichero de instalación.");
            }
        }

        private void panelInt_Paint(object sender, PaintEventArgs e)
        {
            pintarReglaYEje();

            foreach (Funcion f in listadoFunciones)
            {
                if (f.devuelve_mostrar() == true)
                {
                    Graphics gr = panelInt.CreateGraphics();
                    pintarFuncion(f, gr);
                }
            }

            foreach (Punto p in listadoPuntos)
            {
                if (p.devuelve_mostrar() == true)
                {
                    pintarPunto(p);
                }
            }
        }

        private void textoFuncion_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData.ToString())
            {
                case "Return":
                    insertarFuncion_Click(sender, e);                    
                    break;
                default:
                    break;
            }
        }

        private void textoFuncion_TextChanged(object sender, EventArgs e)
        {
            if (textoFuncion.Text == "")
            {
                insertar.Enabled = false;
            }
            else
            {
                if(textoFuncion.Text.Contains("\r\n"))
                {
                    insertar.Enabled = false;
                    textoFuncion.Text = "";
                }
                else
                {
                    insertar.Enabled = true;
                }
            }
        }

    }
}