using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AnalyticGeometry
{
    class AnalyticGeometry
    {
        //double[] vector1 = new double[3];

        public AnalyticGeometry()
        {

        }

        /// <summary>
        /// Vypočítá vektor ze dvou bodů
        /// </summary>
        /// <param name="point1">bod 1</param>
        /// <param name="point2">bod 2</param>
        /// <returns>vektor</returns>
        public double[] GetVector(double[] point1, double[] point2)
        {
            double[] vector = new double[3];
            vector[0] = point2[0] - point1[0];
            vector[1] = point2[1] - point1[1];
            vector[2] = point2[2] - point1[2];
            return vector;
        }

        /// <summary>
        /// Vypočítá vektor ze směrníku a vzdálenosti
        /// </summary>        
        /// <param name="bearing">směrník v gonech</param>
        /// <param name="distance">délka</param>
        /// <returns></returns>
        public double[] GetVector(double bearing, double distance)
        {
            double[] vector = new double[3];
            double dy = distance * Math.Sin(bearing * Math.PI / 200);
            double dx = distance * Math.Cos(bearing * Math.PI / 200);
            vector[0] = dy;
            vector[1] = dx;
            vector[2] = 0;
            return vector;
        }

        /// <summary>
        /// Vrátí součet dvou vektorů
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public double[] GetSumVector(double[] vector1, double[] vector2)
        {
            double[] sum = new double[3];
            sum[0] = vector1[0] + vector2[0];
            sum[1] = vector1[1] + vector2[1];
            sum[2] = vector1[2] + vector2[2];
            return sum;
        }


        /// <summary>
        /// Vypočítá opačný vektor k danému vektoru
        /// </summary>
        /// <param name="vector">vektor</param>
        /// <returns></returns>
        public double[] GetNegativeVector(double[] vector)
        {
            double[] vectorN = new double[3];
            vectorN[0] = -vector[0];
            vectorN[1] = -vector[1];
            vectorN[2] = -vector[2];
            return vectorN;
        }

        public double[] GetRightVector(double[] vector)
        {
            double[] vectorN = new double[3];
            vectorN[0] = vector[1];
            vectorN[1] = -vector[0];
            vectorN[2] = 0;
            return vectorN;
        }

        /// <summary>
        /// Vypočítá jednotkový (normovaný vektor)
        /// </summary>
        /// <param name="vector">vektor</param>
        /// <returns></returns>
        public double[] GetNormVector(double[] vector)
        {
            double[] vectorN = new double[3];
            vectorN[0] = vector[0] / GetVectorLength(vector);
            vectorN[1] = vector[1] / GetVectorLength(vector);
            vectorN[2] = vector[2] / GetVectorLength(vector);
            return vectorN;
        }

        /// <summary>
        /// Zvětšení vektor o n-násobek
        /// </summary>
        /// <param name="vector">vektor</param>
        /// <param name="multiple">zvětšení</param>
        /// <returns></returns>
        public double[] GetExpandVector(double[] vector, double multiple)
        {
            double[] vectorN = new double[3];
            vectorN[0] = vector[0] * multiple;
            vectorN[1] = vector[1] * multiple;
            vectorN[2] = vector[2] * multiple;
            return vectorN;
        }

        /// <summary>
        /// Vrátí dimensi vektoru (2 = 2d, 3 = 3d)
        /// </summary>
        /// <param name="vector">vstupní vektor</param>
        /// <returns>dimense vektoru</returns>
        public int GetVectorDimension(double[] vector)
        {
            return vector.Length;
        }


        /// <summary>
        /// Vypočítá délku vektoru
        /// </summary>
        /// <param name="vector">vstupní vektor</param>
        /// <returns>délka vektoru</returns>
        public double GetVectorLength(double[] vector)
        {
            return Math.Sqrt((vector[0] * vector[0]) + (vector[1] * vector[1]) + (vector[2] * vector[2]));
        }

        /// <summary>
        /// Vypočítá délku mezi dvěma body
        /// </summary>
        /// <param name="point1">bod 1</param>
        /// <param name="point2">bod 2</param>
        /// <returns>délka mezi body</returns>
        public double GetVectorLength(double[] point1, double[] point2)
        {
            return Math.Sqrt(((point2[0] - point1[0]) * (point2[0] - point1[0])) + ((point2[1] - point1[1]) * (point2[1] - point1[1])) + ((point2[2] - point1[2]) * (point2[2] - point1[2])));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bearing">směrník v radianech</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public double[] GetRajon(double bearing, double length)
        {
            double[] dx = new double[3];
            dx[0] = length * Math.Sin(bearing);
            dx[1] = length * Math.Cos(bearing);
            dx[2] = 0;
            return dx;
        }

        public double[] GetRajon(double[] point, double[] vector)
        {
            double[] coo = new double[3];
            coo[0] = point[0] + vector[0];
            coo[1] = point[1] + vector[1];
            coo[2] = point[2] + vector[2];
            return coo;
        }

        /// <summary>
        /// Vypočítá skalární součin
        /// </summary>
        /// <param name="vector1">vektor 1</param>
        /// <param name="vector2">vektor 2</param>
        /// <returns>skalární součin</returns>
        public double ScalarProduct(double[] vector1, double[] vector2)
        {
            return ((vector1[0] * vector2[0]) + (vector1[1] * vector2[1]) + (vector1[2] * vector2[2]));
        }

        /// <summary>
        /// Vypočítá vektorový součin
        /// </summary>
        /// <param name="vector1">vektor 1</param>
        /// <param name="vector2">vektor 2</param>
        /// <returns>vektor 3 = vektorový součin = vektor kolmý na oba vektory</returns>
        public double[] VectorialProduct(double[] vector1, double[] vector2)
        {
            double[] vector3 = new double[3];
            vector3[0] = (vector1[1] * vector2[2]) - (vector2[1] * vector1[2]);
            vector3[1] = (vector1[2] * vector2[0]) - (vector2[2] * vector1[0]);
            vector3[2] = (vector1[0] * vector2[1]) - (vector2[0] * vector1[1]);
            return vector3;
        }

        /// <summary>
        /// Vypočítá úhel mezi dvěma vektory
        /// </summary>
        /// <param name="vector1">vektor 1</param>
        /// <param name="vector2">vektor 2</param>
        /// <returns>úhel ve stupních</returns>
        public double GetVectorAngle(double[] vector1, double[] vector2)
        {
            double p = ScalarProduct(vector1, vector2) / (GetVectorLength(vector1) * GetVectorLength(vector2));
            if (p > 1) p = 1;  // osetreni zaokrouhleni, nelze delat Math.Acos(1.00000000001)  !!!!!
            if (p < -1) p = -1;
            return Math.Acos(p) * 180 / Math.PI;
        }

        /// <summary>
        /// Vypočítá směrník vektoru v gonech
        /// </summary>
        /// <param name="vector">vektor</param>
        /// <returns>Směrník v gonech</returns>
        public double GetVectorBearing(double[] vector)
        {
            double bearing = Math.Atan2(vector[0], vector[1]) * 200 / Math.PI;
            while (bearing < 0) bearing = bearing + 400;
            while (bearing > 400) bearing = bearing - 400;
            return bearing;
        }


        /// <summary>
        /// Bod a vektor => paramtrická rovnice přímky
        /// </summary>
        /// <param name="point1">bod 1</param>
        /// <param name="point2">bod 2</param>
        /// <returns>parametrická rovnice přímky</returns>
        public double[,] GetLineParametricEquation(double[] point1, double[] point2)
        {
            // x = A1 + a1*t
            // y = A2 + a2*t
            // z = A3 + a3*t
            double[] vector = new double[3];
            vector = GetVector(point1, point2);
            double[,] lineParametric = new double[3,2];
            lineParametric = new double[,] { { point1[0], vector[0] }, { point1[1], vector[1] }, { point1[2], vector[2] } };
            return lineParametric;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public double[,] GetLineParametricEquationFromPointAndVector(double[] point, double[] vector)
        {
            // x = A1 + a1*t
            // y = A2 + a2*t
            // z = A3 + a3*t            
            double[,] lineParametric = new double[3, 2];
            lineParametric = new double[,] { { point[0], vector[0] }, { point[1], vector[1] }, { point[2], vector[2] } };
            return lineParametric;
        }




        /// <summary>
        /// 3 body => parametrická rovnice roviny
        /// </summary>
        /// <param name="point1">bod1</param>
        /// <param name="point2">bod2</param>
        /// <param name="point3">bod3</param>
        /// <returns></returns>
        public double[,] GetPlaneParametricEquation(double[] point1, double[] point2, double[] point3)
        {
            // x = A1 + u1*t + v1*s
            // y = A2 + u2*t + v2*s
            // z = A3 + u3*t + v3*s
            double[,] planeParametric = new double[3, 3];
            planeParametric = new double[,] { { point1[0], point2[0] - point1[0], point3[0] - point1[0] }, { point1[1], point2[1] - point1[1], point3[1] - point1[1] }, { point1[2], point2[2] - point1[2], point3[2] - point1[2] } };
            return planeParametric;
        }

        /// <summary>
        /// 3 body => obecná rovnice roviny
        /// </summary>
        /// <param name="point1">bod1</param>
        /// <param name="point2">bod2</param>
        /// <param name="point3">bod3</param>
        /// <returns></returns>
        public double[] GetPlaneEquation(double[] point1, double[] point2, double[] point3)
        {            
            // ax + by + cz + d = 0    - parametry a,b,c,d
            double[] vector1 = new double[3]; // u = B - A
            vector1 = GetVector(point1, point2);            
            double[] vector2 = new double[3]; // v = C - A
            vector2 = GetVector(point1, point3);            
            double[] vector3 = new double[3];
            vector3 = VectorialProduct(vector1, vector2);            

            double[] plane = new double[4];
            plane[0] = vector3[0];
            plane[1] = vector3[1];
            plane[2] = vector3[2];
            plane[3] = -(plane[0] * point1[0]) - (plane[1] * point1[1]) - (plane[2] * point1[2]);
            
            return plane;
        }


        /// <summary>
        /// Vzdálenost bodu od přímky
        /// </summary>
        /// <param name="pointA">bod A</param>
        /// <param name="lineParametric">parametrická rovnice přímky</param>
        /// <returns>vzdálenost bodu od přímky</returns>
        public double GetDistancePointFromLine(double[] pointA, double[,] lineParametric)
        {
            double[] vectorP = new double[3]; // směrový vektor přímky p - z parametrické rovnice přímky
            vectorP[0] = lineParametric[0, 1];
            vectorP[1] = lineParametric[1, 1];
            vectorP[2] = lineParametric[2, 1];
            double[] pointP = new double[3]; // bod P procházející přímkou p - z parametrické rovnice přímky
            pointP[0] = lineParametric[0, 0];
            pointP[1] = lineParametric[1, 0];
            pointP[2] = lineParametric[2, 0];
            double[] vectorA = new double[3]; // vektor a = A - P
            vectorA[0] = pointA[0] - pointP[0];
            vectorA[1] = pointA[1] - pointP[1];
            vectorA[2] = pointA[2] - pointP[2];
            return (GetVectorLength(VectorialProduct(vectorP, vectorA)) / GetVectorLength(vectorP));
        }


        /// <summary>
        /// Vzdálenost bodu od roviny
        /// </summary>
        /// <param name="pointA">bod A</param>
        /// <param name="plane">obecná rovnice roviny</param>
        /// <returns></returns>
        public double GetDistancePointFromPlane(double[] pointA, double[] plane)
        {
            double up = (plane[0] * pointA[0]) + (plane[1] * pointA[1]) + (plane[2] * pointA[2]) + plane[3];
            double down = Math.Sqrt((plane[0] * plane[0]) + (plane[1] * plane[1]) + (plane[2] * plane[2]));  
            if (down == 0) // vodorovna rovina
            {
                return pointA[2] - plane[3];
            }
            return up/down;
        }

        /// <summary>
        /// Pravoúhlý průmět bodu na rovinu
        /// </summary>
        /// <param name="pointA">bod A</param>
        /// <param name="plane">obecná rovnice roviny</param>
        /// <returns></returns>
        public double[] PlaneRectangularProjection(double[] pointA, double[] plane)
        {
            double[] pointP = new double[3]; // point of projection
            double up = -((plane[0] * pointA[0]) + (plane[1] * pointA[1]) + (plane[2] * pointA[2]) + plane[3]);
            double down = ((plane[0] * plane[0]) + (plane[1] * plane[1]) + (plane[2] * plane[2]));
            double t = up / down;
            pointP[0] = pointA[0] + (plane[0] * t);
            pointP[1] = pointA[1] + (plane[1] * t);
            pointP[2] = pointA[2] + (plane[2] * t);
            return pointP;
        }

        /// <summary>
        /// Kolmý průmět bodu na přímku
        /// </summary>
        /// <param name="point1">1. bod přímky</param>
        /// <param name="point2">2. bod přímky</param>
        /// <param name="pointP">testovaný bod</param>
        /// <returns></returns>
        public double[] LineRectangularProjection(double[] point1, double[] point2, double[] pointP)
        {
            double[] pointL = new double[3]; // point of projection

            double[] vector1 = new double[3];
            double[] vector2 = new double[3];
            double[] vector3 = new double[3];
            double[] vectorP = new double[3];
            vector1 = GetVector(point1, point2);
            vector2 = GetVector(point1, pointP);
            vector3 = VectorialProduct(vector1, vector2);
            vectorP = VectorialProduct(vector1, vector3);
            double[,] lineParametric = new double[3, 2];
            lineParametric = GetLineParametricEquation(point1, point2);
            double kolmice = GetDistancePointFromLine(pointP, lineParametric);
            pointL = GetRajon(pointP, GetExpandVector(GetNormVector(vectorP), kolmice));
            
            return pointL;
        }
      
        /// <summary>
        /// Určí, kde leží bod vůči přímce - hodnota > 0 -> bod leží vlevo - ve 2D
        /// </summary>
        /// <param name="point1">bod1 přímky</param>
        /// <param name="point2">bod2 přímky</param>
        /// <param name="pointA">bod A</param>
        /// <returns>hodnota</returns>
        public double GetPositionOnLine(double[] point1, double[] point2, double[] pointA)
        {
            return ((point2[0] - point1[0]) * (pointA[1] - point1[1])) - ((pointA[0] - point1[0]) * (point2[1] - point1[1]));
        }

        public double GetPositionOnLineWithVector(double[] pointOnLine, double[] vectorLine, double[] pointA)
        {            
            double[] point2 = GetRajon(pointOnLine, vectorLine);
            return ((point2[0] - pointOnLine[0]) * (pointA[1] - pointOnLine[1])) - ((pointA[0] - pointOnLine[0]) * (point2[1] - pointOnLine[1]));
        }


        /// <summary>
        /// Vypočítá průsečík dvou přímek ve 2D
        /// </summary>
        /// <param name="lineParametric1">parametrická rovnice přímky 1</param>
        /// <param name="lineParametric2">parametrická rovnice přímky 2</param>
        /// <returns>průsečík dvou přímek ve 2D</returns>
        public double[] GetLinesIntersectionPoint(double[,] lineParametric1, double[,] lineParametric2)
        {            
            double[,] A = new double[2,2];
            A[0, 0] = lineParametric1[0, 1];
            A[0, 1] = -lineParametric2[0, 1];
            A[1, 0] = lineParametric1[1, 1];
            A[1, 1] = -lineParametric2[1, 1];
            double[] b = new double[2];
            b[0] = lineParametric2[0, 0] - lineParametric1[0, 0];
            b[1] = lineParametric2[1, 0] - lineParametric1[1, 0];
            double detA = (A[0,0]*A[1,1]) - (A[0,1]*A[1,0]);
            double[,] invA = new double[2, 2];
            invA[0, 0] = A[1, 1] / detA;
            invA[0, 1] = -A[0, 1] / detA;
            invA[1, 0] = -A[1, 0] / detA;
            invA[1, 1] = A[0, 0] / detA;
            double[] x = new double[2];
            x[0] = (invA[0, 0] * b[0]) + (invA[0, 1] * b[1]);
            x[1] = (invA[1, 0] * b[0]) + (invA[1, 1] * b[1]);

            double[] coo = new double[3];
            coo[0] = lineParametric1[0, 0] + lineParametric1[0, 1] * x[0];
            coo[1] = lineParametric1[1, 0] + lineParametric1[1, 1] * x[0];
                                                       
            return coo;
        }

        /// <summary>
        /// Vypočte průsečík přímky a roviny
        /// </summary>
        /// <param name="plane">obecná rovnice roviny</param>
        /// <param name="line">parametrická rovnice přímky</param>
        /// <returns>intersection point</returns>
        public double[] GetLineAndPlaneIntersection(double[] plane, double[,] line)
        {
            double up = -(plane[0] * line[0, 0]) - (plane[1] * line[1, 0]) - (plane[2] * line[2, 0]) - plane[3];
            double down = (plane[0] * line[0, 1]) + (plane[1] * line[1, 1]) + (plane[2] * line[2, 1]);
            double t = up / down;
            double[] point = new double[3] { (line[0, 0] + (line[0, 1] * t)), (line[1, 0] + (line[1, 1] * t)), (line[2, 0] + (line[2, 1] * t)) };
            return point;
        }

        /// <summary>
        /// Zjistí, zda je průsečík dvou přímek na vyšetřované úsečce A1-B1
        /// </summary>
        /// <param name="pointA1"></param>
        /// <param name="pointB1"></param>
        /// <param name="pointA2"></param>
        /// <param name="pointB2"></param>
        /// <returns>true or false</returns>
        public bool IsOnLineSegment(double[] pointA1, double[] pointB1, double[] pointA2, double[] pointB2)
        {
            bool isOnLine = false;
            double lineSegmentDistance = GetVectorLength(pointA1, pointB1);
            double[] vector1 = GetVector(pointA1, pointB1);
            double[] vector2 = GetVector(pointA2, pointB2);
            double[,] line1 = GetLineParametricEquation(pointA1, pointB1);
            double[,] line2 = GetLineParametricEquation(pointA2, pointB2);
            double[] intersectionPoint = GetLinesIntersectionPoint(line1, line2);
            double[] vector = GetVector(intersectionPoint, pointB1);
            double distance = GetVectorLength(intersectionPoint, pointB1);
            if (distance == 0) return true;
            double vectorAngle = GetVectorAngle(vector1, vector);
            if ((vectorAngle < 1) && (distance <= lineSegmentDistance)) return true;

            return isOnLine;
        }


    }
}
