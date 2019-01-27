using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaneTransform
{
    class Plane
    {
        AnalyticGeometry g = new AnalyticGeometry();
        double[] point1;
        double[] point2;
        double[] point3;  

        double[] lineVector = new double[3];
        double[] planeVector = new double[3];
        double[] thirdVector = new double[3];

        double[] plane = new double[4];
        double[,] planeParametric = new double[3, 3];
        double[,] lineParametric = new double[3, 2];
        double[] projectionPoint;
        double[] linePoint;
        double deviation;
        double offset;
        double station;
        int parita;

        /// <summary>
        /// Konstruktor - rovina ze tří bodů
        /// </summary>
        /// <param name="point1">bod 1</param>
        /// <param name="point2">bod 2</param>
        /// <param name="point3">bod 3</param>
        public Plane(double[] point1, double[] point2, double[] point3)
        {
            this.point1 = point1;
            this.point2 = point2;
            this.point3 = point3;
            this.parita = 1;
            SolvePlane();
        }

        /// <summary>
        /// Konstruktor - rovina ze tří bodů + parita (možnost otočení směru třetího vektoru)
        /// </summary>
        /// <param name="point1">bod1</param>
        /// <param name="point2">bod2</param>
        /// <param name="point3">bod3</param>
        /// <param name="parita">parita = 1 = třetí vektor je dopočten, parite = -1 = třetí vektor je dopočten a negován</param>
        public Plane(double[] point1, double[] point2, double[] point3, int parita)
        {
            this.point1 = point1;
            this.point2 = point2;
            this.point3 = point3;
            this.parita = parita;
            SolvePlane();
        }

        /// <summary>
        /// Konstruktor - rovina je spádovou rovinou ze dvou bodů
        /// </summary>
        /// <param name="point1">bod1</param>
        /// <param name="point2">bod2</param>
        public Plane(double[] point1, double[] point2)
        {
            this.point1 = point1;
            this.point2 = point2;
            SolveGradientPlane();
        }

        /// <summary>
        /// Vypočte rovnici roviny (první vektor tvoří 2 první body, druhý vektor je kolmý na rovinu, třetí vektor je na ně kolmý), rovnici linie dvou prvních bodů
        /// </summary>
        private void SolvePlane()
        {
            if (parita == -1)
            {
                thirdVector = g.getNegativeVector(g.getVector(point1, point3));
                point3 = g.getRajon(point1, thirdVector);
            }

            lineVector = g.getVector(point1, point2);
            plane = g.getPlaneEquation(point1, point2, point3);
            planeVector[0] = plane[0];
            planeVector[1] = plane[1];
            planeVector[2] = plane[2];
            thirdVector = g.VectorialProduct(lineVector, planeVector);
            planeParametric = g.getPlaneParametricEquation(point1, point2, point3);
            lineParametric = g.getLineParametricEquation(point1, point2);
        }

        /// <summary>
        /// Vypočte rovnici spádové roviny (první vektor tvoří 2 první body, druhý vektor je kolmý na rovinu, třetí vektor je na ně kolmý), rovnici linie dvou prvních bodů
        /// </summary>
        private void SolveGradientPlane()
        {
            lineVector = g.getVector(point1, point2);
            thirdVector = g.getRightVector(lineVector);
            point3 = g.getRajon(point1, thirdVector);
            plane = g.getPlaneEquation(point1, point2, point3);
            planeVector[0] = plane[0];
            planeVector[1] = plane[1];
            planeVector[2] = plane[2];
            planeParametric = g.getPlaneParametricEquation(point1, point2, point3);
            lineParametric = g.getLineParametricEquation(point1, point2);
        }

        /// <summary>
        /// Vypočte staničení, kolmici a vzdálenost od roviny
        /// </summary>
        /// <param name="point">Vyšetřovaný bod</param>
        /// <returns></returns>
        public double[] getStationOffsetDeviation(double[] point)
        {
            double[] StationOffsetDeviation = new double[3];
            deviation = g.getDistancePointFromPlane(point, plane);
            projectionPoint = g.PlaneRectangularProjection(point, plane);
            offset = g.getDistancePointFromLine(projectionPoint, lineParametric);            
            linePoint = g.LineRectangularProjection(point1, point2, projectionPoint);            
            station = g.VectorLength(point1, linePoint);
            double[] offsetVector = new double[3];
            double[] stationVector = new double[3];
            offsetVector = g.getVector(linePoint, projectionPoint);
            stationVector = g.getVector(point1, linePoint);

            if (g.ScalarProduct(offsetVector, thirdVector) < 0) offset = -offset;
            if (g.ScalarProduct(stationVector, lineVector) < 0) station = -station;

            StationOffsetDeviation[0] = station;
            StationOffsetDeviation[1] = offset;
            StationOffsetDeviation[2] = -deviation;
            return StationOffsetDeviation;
        }

        /// <summary>
        /// Vypočte souřadnice yxz z hodnot refline
        /// </summary>
        /// <param name="lineCoo"></param>
        /// <returns></returns>
        public double[] getYXZ(double[] lineCoo)
        {            
            double[] point = new double[3];
            double[] vector1 = lineVector;
            double[] vector2 = thirdVector;
            double[] vector3 = planeVector;
            if (lineCoo[0] < 0) vector1 = g.getNegativeVector(vector1);
            if (lineCoo[1] < 0) vector2 = g.getNegativeVector(vector2);
            if (lineCoo[2] > 0) vector3 = g.getNegativeVector(vector3);

            point = g.getRajon(point1, g.getExpandVector(g.getNormVector(vector1), Math.Abs(lineCoo[0])));
            point = g.getRajon(point, g.getExpandVector(g.getNormVector(vector2), Math.Abs(lineCoo[1])));
            point = g.getRajon(point, g.getExpandVector(g.getNormVector(vector3), Math.Abs(lineCoo[2])));

            return point;
        }



        /// <summary>
        /// Projekce bodu do roviny
        /// </summary>
        /// <param name="point">Vyšetřovaný bod</param>
        /// <returns>Kolmá projekce bodu do roviny</returns>
        public double[] getProjectionPoint(double[] point)
        {
            return projectionPoint = g.PlaneRectangularProjection(point, plane);
        }

        /// <summary>
        /// Kolmý průmět bodu na přímku v rámci vyšetřované roviny
        /// </summary>
        /// <param name="point">Vyšetřovaný bod</param>
        /// <returns>Kolmý průmět bodu na přímku</returns>
        public double[] getLinePoint(double[] point)
        {
            projectionPoint = g.PlaneRectangularProjection(point, plane);
            return linePoint = g.LineRectangularProjection(point1, point2, projectionPoint);
        }

        /// <summary>
        /// Vypočte vzdálenost bodu od roviny
        /// </summary>
        /// <param name="point">Vyšetřovaný bod</param>
        /// <returns>Vzdálenost bodu od roviny</returns>
        public double getDistanceFromPlane(double[] point)
        {
            return g.getDistancePointFromPlane(point, plane);            
        }
    }
}
