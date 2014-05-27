using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proyecto
{
    public class InterpolacionPolinomica
    {
        private double[] x;
        private double[] y;
        private int n;

        public InterpolacionPolinomica(double[] x, double[] y)
        {
            this.x = x;
            this.y = y;
            n = x.Length;
        }

        public String resolver()
        {
            String ecuacionFinal;
            double[,] T;
            int i = 0, m, j;
            
            //Obtención de los términos de las diferencias divididas de orden 0
            m = n - 1;
            T = new double[n - 1, n - 1];
            //Debug.WriteLine("Primeras diferencias divididas");
            while (i <= m - 1)
            {
                T[i, 0] = (y[i + 1] - y[i]) / (x[i + 1] - x[i]);
                //Debug.WriteLine("{0}", T[i, 0]);
                i = i + 1;
            }

            //Obtención de los términos de las diferencias divididas de orden 1 a (n - 1) puntos
            j = 1;
            while (j <= m - 1)
            {
                i = j;
                //Debug.WriteLine("Diferencias divididas de orden {0}", j + 1);
                while (i <= m - 1)
                {
                    T[i, j] = (T[i, j - 1] - T[i - 1, j - 1]) / (x[i + 1] - x[i - j]);
                    //Debug.WriteLine("{0}", T[i, j]);
                    i = i + 1;
                }
                j = j + 1;
            }

            //Elaboración de la ecuación final
            ecuacionFinal = y[0].ToString();
            for (i = 0; i < n - 1; i++)
            {
                if (T[i, i] != 0)
                {
                    if (T[i, i] < 0)
                    {
                        ecuacionFinal = ecuacionFinal + " - " + T[i, i] * (-1);
                    }
                    else
                    {
                        ecuacionFinal = ecuacionFinal + " + " + T[i, i];
                    }
                    j = 0;
                    while (j <= i)
                    {
                        if (x[j] == 0)
                        {
                            ecuacionFinal = ecuacionFinal + " * " + "(x)";
                        }
                        else if (x[j] < 0)
                        {
                            ecuacionFinal = ecuacionFinal + " * " + "(x + " + x[j] * (-1) + ")";
                        }
                        else
                        {
                            ecuacionFinal = ecuacionFinal + " * " + "(x - " + x[j] + ")";
                        }
                        j++;
                    }
                }
            }

            return ecuacionFinal;
        }
    }
}
