using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalyticGeometry;
using POINT;

namespace Refline
{    
    public class Refline
    {
        double[] startPoint, endPoint;
        double station, paralel, height;
        double spaceDistance, abstand;
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

        public Refline(double[] point1, double[] point2)
        {
            startPoint = point1;
            endPoint = point2;
            Solve();
        }

        private void Solve()
        {
            tangentVector = AG.GetVector(startPoint, endPoint);
            normalVector = AG.GetRightVector(tangentVector);  // kolmy vektor na refline ve smeru offset  
            normalVector = AG.GetNegativeVector(normalVector);
            point3array = AG.GetRajon(startPoint, normalVector);
            binormalVector = AG.VectorialProduct(tangentVector, normalVector); // kolmy vektor na refline a spádovou rovinu            
            point4array = AG.GetRajon(startPoint, binormalVector);
            tangentPlane = AG.GetPlaneEquation(startPoint, endPoint, point3array);
            normalPlane = AG.GetPlaneEquation(startPoint, point3array, point4array);
            binormalPlane = AG.GetPlaneEquation(startPoint, endPoint, point4array);
            point5array = new double[3] { startPoint[0], startPoint[1], startPoint[2] - 10 };
            svislaPlane = AG.GetPlaneEquation(startPoint, point5array, point3array);
        }

        /// <summary>
        /// Vypočte staničení, kolmici a svislou výšku nad reflinou
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Tuple - double, double, double</returns>
        public Tuple<double, double, double> GetStationParalelHeight(double[] point)
        {            
            station = AG.GetDistancePointFromPlane(point, svislaPlane);
            double[] pointDown = new double[3] { point[0], point[1], point[2] - 10};
            double[,] lineDown = AG.GetLineParametricEquation(point, pointDown);
            double[] intersectionPoint = AG.GetLineAndPlaneIntersection(tangentPlane, lineDown);
            double[] vodorovnaPlane = new double[4] { 0, 0, 0, intersectionPoint[2] };
            height = AG.GetDistancePointFromPlane(point, vodorovnaPlane);
            paralel = AG.GetDistancePointFromPlane(point, binormalPlane);
            return new Tuple<double, double, double>(station, paralel, height);
        }

        /// <summary>
        /// Vypočte šikmé staničení, kolmici a kolmou výšku nad reflinou
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Tuple - double, double, double</returns>
        public Tuple<double, double, double> GetSpaceDistanceParalelAbstand(double[] point)
        {
            spaceDistance = AG.GetDistancePointFromPlane(point, normalPlane);
            paralel = AG.GetDistancePointFromPlane(point, binormalPlane);
            abstand = AG.GetDistancePointFromPlane(point, tangentPlane);
            return new Tuple<double, double, double>(spaceDistance, paralel, abstand);
        }

        /// <summary>
        /// Vypočítá souřadnice yxz z parametrů refline - staničení (vodorovné), kolmice a svislá výška nad refline
        /// </summary>
        /// <param name="station">staničení (vodorovné)</param>
        /// <param name="paralel">kolmice</param>
        /// <param name="height">svislá výška nad refline</param>
        /// <returns></returns>
        public double[] GetYXZfromStationParalelHeight(double station, double paralel, double height)
        {            
            double[] horizontalVector = tangentVector;
            horizontalVector[2] = 0;
            double[] vector1 = AG.GetExpandVectorToLength(horizontalVector, station);
            double[] vector2 = AG.GetExpandVectorToLength(AG.GetNegativeVector(normalVector), paralel);
            double[] vector1a2 = AG.GetSumVector(vector1, vector2);
            double[] pointHorizontal = AG.GetRajon(startPoint, vector1a2);
            double[] verticalVector = new double[3] { 0, 0, station };
            double[,] verticalLine = AG.GetLineParametricEquationFromPointAndVector(pointHorizontal, verticalVector);
            double[] intersectionPoint = AG.GetLineAndPlaneIntersection(tangentPlane, verticalLine);
            double[] heightVector = new double[3] { 0, 0, height };
            double[] pointOut = AG.GetRajon(intersectionPoint, heightVector);
            return pointOut;
        }


        /// <summary>
        /// Vypočítá souřadnice yxz z parametrů refline - šikmé staničení (po refline), kolmice a kolmá vzdálenost od spádové roviny refline
        /// </summary>
        /// <param name="spaceDistance">šikmé staničení (po refline)</param>
        /// <param name="paralel">kolmice</param>
        /// <param name="abstand">kolmá vzdálenost od spádové roviny refline</param>
        /// <returns></returns>
        public double[] GetYXZfromSpaceDistanceParalelAbstand(double spaceDistance, double paralel, double abstand)
        {
            double[] vector1 = AG.GetExpandVectorToLength(tangentVector, spaceDistance);
            double[] vector2 = AG.GetExpandVectorToLength(AG.GetNegativeVector(normalVector), paralel);
            double[] vector3 = AG.GetExpandVectorToLength(binormalVector, abstand);
            double[] vector1a2 = AG.GetSumVector(vector1, vector2);
            double[] sumVector = AG.GetSumVector(vector1a2, vector3);
            double[] pointOut = AG.GetRajon(startPoint, sumVector);
            return pointOut;            
        }


    }
}
