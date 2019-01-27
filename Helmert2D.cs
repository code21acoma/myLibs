using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaneTransform
{
    class Helmert2D
    {
        double[,] IdentSystem1;
        double[,] IdentSystem2;
        double[,] System1;
        double[,] System2;
        Matrix A, transA, invertA, l, h, x, v;
        int pocetIdent;
        

        public Helmert2D (double[,] IdentSystem1, double[,] IdentSystem2, int pocetIdent)
        {
            this.IdentSystem1 = IdentSystem1;
            this.IdentSystem2 = IdentSystem2;
            this.pocetIdent = pocetIdent;
            SolveKey(IdentSystem1, IdentSystem2, pocetIdent);
        }

        private void SolveKey(double[,] IdentSystem1, double[,] IdentSystem2, int pocetIdent)
        {
            A = new Matrix(2 * pocetIdent, 4);
            l = new Matrix(2 * pocetIdent, 1);
            for (int i = 0; i < pocetIdent; i++)
            {
                A[i, 0] = 1;
                A[i, 1] = 0;
                A[i, 2] = IdentSystem1[i,0];
                A[i, 3] = -IdentSystem1[i,1];
                A[pocetIdent + i, 0] = 0;
                A[pocetIdent + i, 1] = 1;
                A[pocetIdent + i, 2] = IdentSystem1[i,1];
                A[pocetIdent + i, 3] = IdentSystem1[i,0];

                l[i,0] = IdentSystem2[i,0];
                l[pocetIdent + i,0] = IdentSystem2[i,1];
            }

            transA = Matrix.Transpose(A);
            invertA = (transA * A).Invert();
            h = invertA * transA * l;

            double q = Math.Sqrt((h[2, 0] * h[2, 0]) + (h[3, 0] * h[3, 0]));
            h[2, 0] = h[2, 0] / q;
            h[3, 0] = h[3, 0] / q;

            v = (A * h) - l;
            //x = A * h;            
        }


        public double[,] Transform(double[,] System1, int pocet)
        {
            for (int i = 0; i < pocet; i++)
            {
                A[i, 0] = 1;
                A[i, 1] = 0;
                A[i, 2] = System1[i,0];
                A[i, 3] = -System1[i,1];
                A[pocet + i, 0] = 0;
                A[pocet + i, 1] = 1;
                A[pocet + i, 2] = System1[i,1];
                A[pocet + i, 3] = System1[i,0];
            }

            x = A * h;

            //double[] point = new double[2];
            double[,] System2 = new double[pocet + 1, 2];

            for (int i = 0; i < pocet; i++)
            {
                System2[i,0] = x[i, 0];
                System2[i,1] = x[pocet + i, 0];                
            }

            return System2;
        }
    }
}
