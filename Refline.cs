using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using POINT;

namespace Refline
{    
    public class Refline
    {
        POINT.POINT point1, point2;
        double station, paralel, height;
        double spaceDistance, abstand;
        double[] point1array;
        double[] point2array;
        double[] tangentVector;
        double[] normalVector;
        double[] point3array;
        double[] binormalVector;
        double[] point4array;
        double[] tangentPlane;
        double[] normalPlane;
        double[] binormalPlane;
        double[] point5array;
        double[] svislaPlane;
        AnalyticGeometry.AnalyticGeometry AG = new AnalyticGeometry.AnalyticGeometry();

        //public Refline() { }

        public Refline(POINT.POINT point1, POINT.POINT point2)
        {
            this.point1 = point1;
            this.point2 = point2;
            Solve();
        }

        public Refline(double[] point1, double[] point2)
        {
            this.point1 = new POINT.POINT();
            this.point1.y = point1[0];
            this.point1.x = point1[1];
            this.point1.z = point1[2];
            this.point2 = new POINT.POINT();
            this.point2.y = point2[0];
            this.point2.x = point2[1];
            this.point2.z = point2[2];
            Solve();
        }

        private void Solve()
        {
            point1array = new double[3] { point1.y, point1.x, point1.z };
            point2array = new double[3] { point2.y, point2.x, point2.z };
            
            tangentVector = AG.getVector(point1array, point2array);
            normalVector = AG.getRightVector(tangentVector);  // kolmy vektor na refline ve smeru offset  
            normalVector = AG.getNegativeVector(normalVector);
            point3array = AG.getRajon(point1array, normalVector);
            binormalVector = AG.VectorialProduct(tangentVector, normalVector); // kolmy vektor na refline a spádovou rovinu
            //binormalVector = AG.getNegativeVector(binormalVector);
            point4array = AG.getRajon(point1array, binormalVector);
            tangentPlane = AG.getPlaneEquation(point1array, point2array, point3array);
            normalPlane = AG.getPlaneEquation(point1array, point3array, point4array);
            binormalPlane = AG.getPlaneEquation(point1array, point2array, point4array);
            point5array = new double[3] { point1array[0], point1array[1], point1array[2] - 10 };
            svislaPlane = AG.getPlaneEquation(point1array, point5array, point3array);
        }

        /// <summary>
        /// Vypočte staničení, kolmici a svislou výšku nad reflinou
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Tuple - double, double, double</returns>
        public Tuple<double, double, double> GetStationParalelHeight(double[] point)
        {            
            station = AG.getDistancePointFromPlane(point, svislaPlane);
            double[] pointDown = new double[3] { point[0], point[1], point[2] - 10};
            double[,] lineDown = AG.getLineParametricEquation(point, pointDown);
            double[] intersectionPoint = AG.getLineAndPlaneIntersection(tangentPlane, lineDown);
            double[] vodorovnaPlane = new double[4] { 0, 0, 0, intersectionPoint[2] };
            height = AG.getDistancePointFromPlane(point, vodorovnaPlane);
            paralel = AG.getDistancePointFromPlane(point, binormalPlane);
            return new Tuple<double, double, double>(station, paralel, height);
        }

        /// <summary>
        /// Vypočte šikmé staničení, kolmici a kolmou výšku nad reflinou
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Tuple - double, double, double</returns>
        public Tuple<double, double, double> GetSpaceDistanceParalelAbstand(double[] point)
        {
            spaceDistance = AG.getDistancePointFromPlane(point, normalPlane);
            paralel = AG.getDistancePointFromPlane(point, binormalPlane);
            abstand = AG.getDistancePointFromPlane(point, tangentPlane);
            return new Tuple<double, double, double>(spaceDistance, paralel, abstand);
        }


    }
}
