using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Proyecto
{
    class Punto
    {
        private double X;
        private double Y;
        private Color colorPunto;
        private bool mostrar;

        public Punto(double x, double y, bool b, Color c)
        {
            X = x;
            Y = y;
            colorPunto = c;
            mostrar = b;
        }

        public Punto()
        {
            X = 0;
            Y = 0;
            mostrar = false;
        }

        public void set_coordenadaX(int x)
        {
            X = x;
        }

        public void set_coordenadaY(int y)
        {
            Y = y;
        }

        public Color devuelve_Color()
        {
            return colorPunto;
        }

        public double coordenadaX()
        {
            return X;
        }

        public double coordenadaY()
        {
            return Y;
        }

        public bool devuelve_mostrar()
        {
            return mostrar;
        }

        public void mostrarPunto()
        {
            mostrar = true;
        }

        public void ocultarPunto()
        {
            mostrar = false;
        }

        internal void cambiarColor(Color c)
        {
            colorPunto = c;
        }
    }
}
