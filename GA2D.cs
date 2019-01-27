using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Points;


namespace GA_test
{
    /*
     
        GEOMETRICKÉ ALGORITMY VE 2D
        v. 0.1

    */
    public class GA2D
    {
        List<POINT> points = new List<POINT>();
        List<POINT> pomPoints = new List<POINT>();

        int minX, maxX, minY, maxY;        

        public GA2D() { }

        public GA2D(List<POINT> points)
        {
            this.points.Clear();
            this.points = points;
            SetIndex();
        }

        public void SetIndex()
        {
            for (int i = 0; i < points.Count(); i++)
            {
                points[i].i = i;
            }
        }

        public void AddPoint(POINT point)
        {
            points.Add(point);
            SetIndex();
        }

        public void AddPoint(double y, double x)
        {
            POINT point = new POINT();
            point.x = x;
            point.y = y;
            points.Add(point);
            SetIndex();
        }

        public void AddPoint(int n, double y, double x)
        {
            POINT point = new POINT();
            point.n = n;
            point.x = x;
            point.y = y;
            points.Add(point);
            SetIndex();
        }

        public void ClearPoints()
        {
            points.Clear();
        }

        public void GetMinMax()
        {
            int i = 0;
            foreach (POINT point in points)
            {
                if (i == 0)
                {
                    minX = i;
                    maxX = i;
                    minY = i;
                    maxY = i;
                    minX = i;
                    maxX = i;
                    minY = i;
                    maxY = i;
                }
                else
                {
                    if (point.x < points[minX].x)
                    {
                        minX = i;
                    }
                    if (point.y < points[minY].y)
                    {
                        minY = i;
                    }
                    if (point.x > points[maxX].x)
                    {
                        maxX = i;
                    }
                    if (point.y > points[maxY].y)
                    {
                        maxY = i;
                    }
                }

                i++;
            }
        }

        /// <summary>
        /// Gift wrapping algorithm (Jarvis march)
        /// </summary>
        /// <returns></returns>
        public List<POINT> GetKonvexBoundary()
        {
            List<POINT> body = new List<POINT>();
            body = points;
            GetMinMax();
            List<POINT> konvexBoundary = new List<POINT>();
            
            konvexBoundary.Add(body[minY]);
            AnalyticGeometry AG = new AnalyticGeometry();

            int p = 1000;
            double minVectorAngle = 500;
            
            // prvni smer an druhy bod konvexniho obalu
            double[] bod1 = new double[] { 1, 0, 0};
            double[] bod2 = new double[] { 0, 0, 0 };
            double[] bod3 = new double[3];
            double[] vectorStart = new double[3];
            vectorStart = AG.getVector(bod1, bod2);
            bod1 = new double[] { body[minY].y, body[minY].x, 0 };
            
            for (int i = 0; i < body.Count(); i++)
            {
                if (i != minY)
                {
                    bod2 = new double[] { body[i].y, body[i].x, 0 };
                    double[] vector = AG.getVector(bod1, bod2);
                    double vectorAngle = AG.VectorAngle(vectorStart, vector);
                    if (vectorAngle < minVectorAngle)
                    {
                        minVectorAngle = vectorAngle;
                        p = i;
                    }
                }
            }

            konvexBoundary.Add(body[p]);

            // dalsi body konvexniho obalu
            bool findFirst = false;

            while (findFirst == false)
            {
                minVectorAngle = 500;
                p = 1000;

                bod1 = new double[] { konvexBoundary[konvexBoundary.Count - 2].y, konvexBoundary[konvexBoundary.Count - 2].x, 0 };
                bod2 = new double[] { konvexBoundary[konvexBoundary.Count - 1].y, konvexBoundary[konvexBoundary.Count - 1].x, 0 };
                vectorStart = new double[3];
                vectorStart = AG.getVector(bod1, bod2);

                for (int i = 0; i < body.Count(); i++)
                {
                    if ((bod2[0] != body[i].y) && (bod2[2] != body[i].x))
                    {
                        bod3 = new double[] { body[i].y, body[i].x, 0 };
                        double[] vector = AG.getVector(bod2, bod3);
                        double vectorAngle = AG.VectorAngle(vectorStart, vector);
                        if (vectorAngle < minVectorAngle)
                        {
                            minVectorAngle = vectorAngle;
                            p = i;
                        }
                    }
                }

                if ((konvexBoundary[0].y == body[p].y) && (konvexBoundary[0].x == body[p].x))
                {
                    findFirst = true;
                }
                else
                {
                    konvexBoundary.Add(body[p]);
                }

            }

            return konvexBoundary;
        }
      

        /// <summary>
        /// Výpočet plochy mnohoúhelníku vypočtená L'Hullierovým vzorcem
        /// </summary>
        /// <returns></returns>
        public double GetArea()
        {
            double s = 0;
            int j, k;

            for (int i = 0; i < points.Count; i++)
            {
                j = i + 1;
                k = i - 1;
                if (i == 0) k = points.Count - 1;
                if (i == points.Count - 1) j = 0;

                s = s + points[i].x * (points[j].y - points[k].y);
            }

            return Math.Abs(s / 2);
        }

        /// <summary>
        /// Výpočet těžiště mnohoúhelníku https://en.wikipedia.org/wiki/Polygon
        /// </summary>
        /// <returns></returns>
        public POINT GetCenter()
        {
            POINT point = new POINT();

            double A = GetArea();

            int j;
            double Cx = 0;
            double Cy = 0;

            for (int i = 0; i < points.Count; i++)
            {
                j = i + 1;
                if (i == points.Count - 1) j = 0;
                Cx = Cx + (points[i].x + points[j].x) * ((points[i].x * points[j].y) - (points[j].x * points[i].y));
                Cy = Cy + (points[i].y + points[j].y) * ((points[i].x * points[j].y) - (points[j].x * points[i].y));
            }

            point.x = Cx / (6 * A);
            point.y = Cy / (6 * A);            
            return point;
        }

        /// <summary>
        /// Get Bool if point is inside convex boundary ()
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsInside(POINT point)
        {
            bool inside = true;
            int j;
            // určení orientace mnohoúhelníka (konvexního obalu) - levotičivost x pravotočivost

            Matrix A = new Matrix(3, 3);
            A[0, 0] = points[0].x;
            A[0, 1] = points[0].y;
            A[0, 2] = 1;
            A[1, 0] = points[1].x;
            A[1, 1] = points[1].y;
            A[1, 2] = 1;
            A[2, 0] = points[2].x;
            A[2, 1] = points[2].y;
            A[2, 2] = 1;
            double det0 = A.Det();

            for (int i = 0; i < points.Count; i++)
            {
                j = i + 1;
                if (i == points.Count - 1) j = 0;
                Matrix A1 = new Matrix(3, 3);
                A1[0, 0] = points[i].x;
                A1[0, 1] = points[i].y;
                A1[0, 2] = 1;
                A1[1, 0] = points[j].x;
                A1[1, 1] = points[j].y;
                A1[1, 2] = 1;
                A1[2, 0] = point.x;
                A1[2, 1] = point.y;
                A1[2, 2] = 1;
                double det1 = A1.Det();

                if ((det0 < 0) && (det1 > 0))
                {
                    inside = false;
                }
                if ((det0 > 0) && (det1 < 0))
                {
                    inside = false;
                }
            }
            return inside;
        }

        /// <summary>
        /// TEST - SMAZAT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public string IsInside1(POINT point)
        {
            bool inside = true;
            int j;
            // určení orientace mnohoúhelníka (konvexního obalu) - levotičivost x pravotočivost
            string s = "";
            Matrix A = new Matrix(3, 3);
            A[0, 0] = points[0].x;
            A[0, 1] = points[0].y;
            A[0, 2] = 1;
            A[1, 0] = points[1].x;
            A[1, 1] = points[1].y;
            A[1, 2] = 1;
            A[2, 0] = points[2].x;
            A[2, 1] = points[2].y;
            A[2, 2] = 1;
            double det0 = A.Det();
            s = s + "det0 = " + det0;

            for (int i = 0; i < points.Count; i++)
            {
                j = i + 1;
                if (i == points.Count - 1) j = 0;
                Matrix A1 = new Matrix(3, 3);
                A1[0, 0] = points[i].x;
                A1[0, 1] = points[i].y;
                A1[0, 2] = 1;
                A1[1, 0] = points[j].x;
                A1[1, 1] = points[j].y;
                A1[1, 2] = 1;
                A1[2, 0] = point.x;
                A1[2, 1] = point.y;
                A1[2, 2] = 1;
                double det1 = A1.Det();
                s = s + ", det1 = " + det1;

                if ((det0 < 0) && (det1 > 0))
                {
                    inside = false;
                }
                if ((det0 > 0) && (det1 < 0))
                {
                    inside = false;
                }
            }
            return s;
        }





    }
}
