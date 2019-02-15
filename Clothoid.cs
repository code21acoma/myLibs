using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnalyticGeometry;
using System.IO;

namespace Clothoid
{
    class Clothoid
    {
        public double A;
        public double Lk;
        public double Rk;
        public double tauk;
        public double xk;
        public double yk;
        public double l;
        public double R;
        public double tau;
        public double x;
        public double y;
        public double R1, R2, dL;
        public double tangent;
        public double xOrigin;
        public double yOrigin;
        public double DOC;
        //public double radius1;
        //public double radius2;

        AnalyticGeometry.AnalyticGeometry AG = new AnalyticGeometry.AnalyticGeometry();

        public Clothoid() { }

        /// <summary>
        /// Vypočte parametr klotoidy A
        /// </summary>
        /// <param name="Lk">Délka přechodnice</param>
        /// <param name="Rk">Poloměr křivosti přechodnice</param>
        /// <returns></returns>
        public double CalcA(double Lk, double Rk)
        {
            this.Lk = Lk;
            this.Rk = Rk;
            A = Math.Sqrt(Lk * Rk);
            return A;
        }

        /// <summary>
        /// Vypočte parametr klotoidy A ze dvou poloměrů a délky
        /// </summary>
        /// <param name="R1"></param>
        /// <param name="R2"></param>
        /// <param name="dL"></param>
        /// <returns></returns>
        public double CalcAFrom2Radius(double R1, double R2, double dL)
        {
            A = Math.Sqrt((R1 * R2 * dL) / (Math.Abs(R1-R2)));
            return A;
        }

        private double CalcTauK(double Lk, double Rk)
        {
            tauk = Lk / (2 * Rk);
            return tauk;
        }

        private double[] CalcXkYk(double Lk, double Rk)
        {
            xk = Lk - (Math.Pow(Lk, 3) / (40 * Math.Pow(Rk, 2))) + (Math.Pow(Lk, 5) / (3456 * Math.Pow(Rk, 4))) - (Math.Pow(Lk, 7) / (599040 * Math.Pow(Rk, 6)));
            yk = (Math.Pow(Lk, 2) / (6 * Rk)) - (Math.Pow(Lk, 4) / (336 * Math.Pow(Rk, 3))) + (Math.Pow(Lk, 6) / (42240 * Math.Pow(Rk, 5)));
            double[] yx = new double[2] { yk, xk};
            return yx;
        }

        /// <summary>
        /// Vypočte souřadnice xy vůči tangentě klotoidy
        /// </summary>
        /// <param name="l">Vzdálenost bodu od začátku klotoidy</param>
        /// <param name="Lk">Délka přechodnice</param>
        /// <param name="Rk">Poloměr křivosti</param>
        /// <returns></returns>
        public double[] CalcXY(double l, double Lk, double Rk)
        {
            double[] yx = new double[2];
            x = l - (Math.Pow(l, 5) / (40 * Math.Pow(Rk, 2) * Math.Pow(Lk, 2))) + (Math.Pow(l, 9) / (3456 * Math.Pow(Rk, 4) * Math.Pow(Lk, 4))) - (Math.Pow(l, 13) / (599040 * Math.Pow(Rk, 6) * Math.Pow(Lk, 6)));
            y = (Math.Pow(l, 3) / (6 * Rk * Lk)) - (Math.Pow(l, 7) / (336 * Math.Pow(Rk, 3) * Math.Pow(Lk, 3))) + (Math.Pow(l, 11) / (42240 * Math.Pow(Rk, 5) * Math.Pow(Lk, 5)));
            yx[0] = y;
            yx[1] = x;
            return yx;
        }

        /// <summary>
        /// Vypočte začátek přechodnice (souřadnice yx) u přechodnice z oblouku1 na oblouk2
        /// </summary>
        /// <param name="onPoint">Point on clothoid by radius R1</param>
        /// <param name="endPoint">Point on clothoid by radius R2</param>
        /// <param name="R1">radius R1</param>
        /// <param name="R2">radius R2</param>
        /// <param name="length">length of clothoid (R1->R2</param>
        /// <param name="onPointBearing">Bearing of clothoid by radius R1</param>
        /// <returns>start point, startClothoidBearing, Lk</returns>
        public Tuple<double[], double, double> CalcStartPoint(double[] onPoint, double[] endPoint, double R1, double R2, double length, double onPointBearing)            
        {
            StreamWriter sW = new StreamWriter("clothtoid_log.txt");
            double[] startPoint = new double[3];
            A = Math.Sqrt((R1 * R2 * length) / Math.Abs(R1 - R2));
            Lk = CalcLk(A, Math.Abs(R1));
            tauk = CalcTauK(Lk, Math.Abs(R1)) * 200 / Math.PI;
            double startBearing = 0;
            if (R1 >= 0) startBearing = onPointBearing - tauk;
            else startBearing = onPointBearing + tauk;
            while (startBearing < 0) startBearing = startBearing + 400;
            while (startBearing >= 400) startBearing = startBearing - 400;
            double[] yx = CalcXkYk(Lk, Math.Abs(R1));
            double bearing1 = 0;
            double bearing2 = 0;
            if (R1 >= 0)
            {
                bearing1 = startBearing - 100;
                bearing2 = startBearing - 200;
                while (bearing1 < 0) bearing1 = bearing1 + 400;
                while (bearing1 >= 400) bearing1 = bearing1 - 400;
                while (bearing2 < 0) bearing2 = bearing2 + 400;
                while (bearing2 >= 400) bearing2 = bearing2 - 400;
            }
            else
            {
                bearing1 = startBearing + 100;
                bearing2 = startBearing - 200;
                while (bearing1 < 0) bearing1 = bearing1 + 400;
                while (bearing1 >= 400) bearing1 = bearing1 - 400;
                while (bearing2 < 0) bearing2 = bearing2 + 400;
                while (bearing2 >= 400) bearing2 = bearing2 - 400;
            }

            double[] vector1 = new double[3];
            double[] vector2 = new double[3];
            vector1 = AG.GetRajon(bearing1 * Math.PI / 200, yx[0]);
            vector2 = AG.GetRajon(bearing2 * Math.PI / 200, yx[1]);
            startPoint[0] = onPoint[0] + vector1[0] + vector2[0];
            startPoint[1] = onPoint[1] + vector1[1] + vector2[1];
            startPoint[2] = 0;

            double Lposun = 0;
            //if (Math.Abs(R1) < Math.Abs(R2)) Lposun = CalcLk(A, Math.Abs(R1)) - CalcLk(A, Math.Abs(R2));
            //else Lposun = CalcLk(A, Math.Abs(R2)) - CalcLk(A, Math.Abs(R1));
            if (Math.Abs(R1) < Math.Abs(R2)) Lposun =  CalcLk(A, Math.Abs(R2));
            else Lposun = CalcLk(A, Math.Abs(R1));

            sW.WriteLine("A: " + A);
            sW.WriteLine("Lk: " + Lk);
            sW.WriteLine("tauk: " + tauk);
            sW.WriteLine("onPointBearing: " + onPointBearing);
            sW.WriteLine("startBearing: " + startBearing);
            sW.WriteLine("bearing1: " + bearing1);
            sW.WriteLine("bearing2: " + bearing2);
            sW.WriteLine("startPoint: " + startPoint[0] + " " + startPoint[1]);
            sW.WriteLine("dy: " + yx[0] + " dx: " + yx[1]);
            sW.WriteLine("R1: " + R1 + " R2: " + R2);
            sW.WriteLine("Lposun: " + Lposun);
            sW.Close();

            return new Tuple<double[], double, double>(startPoint, startBearing, Lposun);
        }


        /// <summary>
        /// Vypočte úhel subtangenty a tangenty (subtangenta v bodě Pk a tangenta v počátku klotoidy)
        /// </summary>
        /// <param name="l">Vzdálenost bodu Pk od počátku</param>
        /// <param name="Lk">Délka přechodnice</param>
        /// <param name="Rk">Poloměr křivosti v bodě Pk</param>
        /// <returns></returns>
        public double CalcTau(double l, double Lk, double Rk)
        {
            tau = (l*l) / (2 * Rk * Lk);
            return tau;
        }

        /// <summary>
        /// Vypočte délku přechodnice z parametru klotoidy A a poloměru křivosti
        /// </summary>
        /// <param name="A">parametr klotoidy</param>
        /// <param name="R">poloměr křivosti</param>
        /// <returns>délka přechodnice</returns>
        public double CalcLk(double A, double R)
        {
            return A * A / R;
        }

        /// <summary>
        /// Vypočítá poloměr zakrivení pro danou přechodnici s parametrem A a délkou přechodnice l
        /// </summary>
        /// <param name="A">parametr klotoidy</param>
        /// <param name="l">délka přechodnice</param>
        /// <returns></returns>
        public double CalcR(double A, double l)
        {
            return A * A / l;
        }
       

        /// <summary>
        /// Calculate Degree od Curvature (DOC)
        /// </summary>
        /// <param name="Rk">Radius</param>
        /// <returns>DOC</returns>
        public double CalcDOC(double Rk)
        {
            DOC = 18000 / (Math.PI * Rk);
            return DOC;
        }

        


    }
}
