using System;

namespace Proyecto
{
    public class RegresionEstandar
    {
        private double[] x;
        private double[] y;
        private int n; 
        private string tipoRegresion;
        public double a, b;
        
        public RegresionEstandar(double[] x, double[] y, string tR)
        {
            this.x = x;
            this.y = y;
            tipoRegresion = tR;
            n = x.Length;
        }

        public void resolver()
        {
            int i;
            double temp;
            RegresionEstandar regresionLineal;

            switch(tipoRegresion)
            {
                case "Lineal":
                    double pxy, sx, sy, sx2, sy2;
                    pxy = sx = sy = sx2 = sy2 = 0.0;
                    for (i = 0; i < n; i++)
                    {
                        sx += x[i];
                        sy += y[i];
                        sx2 += x[i] * x[i];
                        sy2 += y[i] * y[i];
                        pxy += x[i] * y[i];
                    }
                    b = (n * pxy - sx * sy) / (n * sx2 - sx * sx);
                    a = (sy - b * sx) / n;
                    break;

                case "Exponencial":
                    for (i = 0; i < n; i++)
                    {
                        temp = Math.Log10(y[i]);
                        y[i] = temp;
                    }

                    regresionLineal = new RegresionEstandar(x, y, "Lineal");
                    regresionLineal.resolver();

                    a = Math.Pow(10, regresionLineal.a);
                    b = Math.Pow(10, regresionLineal.b);
                    break;

                case "Logaritmica":
                    for (i = 0; i < n; i++)
                    {
                        temp = Math.Log(x[i]);
                        x[i] = temp;
                    }

                    regresionLineal = new RegresionEstandar(x, y, "Lineal");
                    regresionLineal.resolver();

                    a = regresionLineal.a;
                    b = regresionLineal.b;
                    break;
            }
        }
    }
}
