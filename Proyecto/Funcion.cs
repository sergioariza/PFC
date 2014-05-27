using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Proyecto
{
    class Funcion
    {
        private string funcion;
        private int idFuncion;
        private Color colorFuncion;
        private bool mostrar;

        public Funcion(string str, int i, Color c, bool b)
        {
            funcion = str;
            idFuncion = i;
            colorFuncion = c;
            mostrar = b;
        }

        public string devuelve_funcion()
        {
            return funcion;
        }

        public int devuelve_id()
        {
            return idFuncion;
        }

        public bool devuelve_mostrar()
        {
            return mostrar;
        }

        public Color devuelve_color()
        {
            return colorFuncion;
        }

        public void cambiarColor(Color c)
        {
            colorFuncion = c;
        }

        public void mostrarFuncion()
        {
            mostrar = true;
        }

        public void ocultarFuncion()
        {
            mostrar = false;
        }
    }
}
