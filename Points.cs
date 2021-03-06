using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CooCompare
{
    public class POINT
    {
        //NYXZC
        public int i { get; set; }  // index 
        public string n { get; set; }  // number or name
        public double y { get; set; } // coordinate y
        public double x { get; set; } // coordinate x
        public double z { get; set; } // coordinate z        
        public string comment { get; set; } // note, comment
    }

    public class Points
    {
        public List<POINT> points = new List<POINT>();

        public Points() {}

        public static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.LastIndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }


        public void AddPoint(POINT point)
        {
            points.Add(point);
        }


        /// <summary>
        /// Import file with points
        /// </summary>
        /// <param name="file">file path</param>
        /// <param name="type">input type - for example NYXZC (number, y, x, z, commnet)</param>
        public void Import(string file, string type)
        {
            string radek;
            string[] prvky;

            StreamReader reader = new StreamReader(file);            
            while ((radek = reader.ReadLine()) != null)
            {
                POINT point = new POINT();
                radek = radek.Replace(",", ".");
                radek = radek.Replace(";", " ");
                radek = radek.Trim();
                prvky = radek.Split(' ');
                for (int i = 0; i < prvky.Length; i++)
                {
                    if (type[i] == 'N') point.n = prvky[i];
                    if (type[i] == 'Y') point.y = Convert.ToDouble(prvky[i]);
                    if (type[i] == 'X') point.x = Convert.ToDouble(prvky[i]);
                    if (type[i] == 'Z') point.z = Convert.ToDouble(prvky[i]);
                    if (type[i] == 'C') point.comment = prvky[i];
                }
                points.Add(point);
                
            }
            reader.Close();
        }

        /// <summary>
        /// Export file with points
        /// </summary>
        /// <param name="file">file path</param>
        /// <param name="type">ouput type - for example NYXZC (number, y, x, z, commnet)</param>
        /// <param name="separator">separator ';' or ' ',...</param>
        public void Export(string file, string type, string separator)
        {
            StreamWriter writer = new StreamWriter(file);
            string radek;
            for (int i = 0; i < points.Count; i++)
            {
                radek = "";
                for (int j = 0; j < type.Length; j++)
                {
                    if (type[j] == 'N') radek = radek + points[i].n.ToString() + separator;
                    if (type[j] == 'Y') radek = radek + points[i].y.ToString("0.000") + separator;
                    if (type[j] == 'X') radek = radek + points[i].x.ToString("0.000") + separator;
                    if (type[j] == 'Z') radek = radek + points[i].z.ToString("0.000") + separator;
                    if (type[j] == 'C') radek = radek + points[i].comment.ToString() + separator;
                }

                try
                {
                    radek = ReplaceLastOccurrence(radek, separator, "");
                    writer.WriteLine(radek);
                }
                catch { }
            }

            writer.Close();

        }

        /*
        public static double GetBearing(double[] point1, double[] point2)
        {
            double bearing = 0;

            return bearing;
        }
        */

        /// <summary>
        /// Get Bearing
        /// </summary>
        /// <param name="point1">from</param>
        /// <param name="point2">to</param>
        /// <returns></returns>
        public double GetBearing(POINT point1, POINT point2)
        {
            double bearing = bearing = Math.Atan2((point2.y - point1.y), (point2.x - point1.x));
            bearing = bearing * 200 / Math.PI;
            if (bearing >= 400) bearing = bearing - 400;
            if (bearing < 0) bearing = bearing + 400;
            return bearing;
        }

        /// <summary>
        /// Get 2D Distance
        /// </summary>
        /// <param name="point1">from</param>
        /// <param name="point2">to</param>
        /// <returns></returns>
        public double Get2dDistance(POINT point1, POINT point2)
        {
            double distance = Math.Sqrt(((point2.y - point1.y) * (point2.y - point1.y)) + ((point2.x - point1.x) * (point2.x - point1.x)));
            return distance;
        }

        /// <summary>
        /// Get 3D Distance
        /// </summary>
        /// <param name="point1">from</param>
        /// <param name="point2">to</param>
        /// <returns></returns>
        public double Get3dDistance(POINT point1, POINT point2)
        {
            double distance = Math.Sqrt(((point2.y - point1.y) * (point2.y - point1.y)) + ((point2.x - point1.x) * (point2.x - point1.x)) + ((point2.z - point1.z) * (point2.z - point1.z)));
            return distance;
        }


        /// <summary>
        /// Get rayon paramaters
        /// </summary>
        /// <param name="distance">distance in meter</param>
        /// <param name="bearing">bearing in gon</param>
        /// <returns></returns>
        public double[] GetRayonParameters(double distance, double bearing)
        {
            if (bearing < 0) bearing = bearing + 400;
            if (bearing > 400) bearing = bearing - 400;
            double fi = 200 / Math.PI;
            double[] rayon = new double[2];
            rayon[0] = distance * Math.Sin(bearing / fi);  // dy
            rayon[1] = distance * Math.Cos(bearing / fi);  // dx                        
            return rayon;
        }

        /// <summary>
        /// Get Rayon
        /// </summary>
        /// <param name="startpoint">Start point</param>
        /// <param name="distance">distance in meter</param>
        /// <param name="bearing">bearing in gon</param>
        /// <returns></returns>
        public POINT GetRayon(POINT startpoint, double distance, double bearing)
        {
            if (bearing < 0) bearing = bearing + 400;
            if (bearing > 400) bearing = bearing - 400;
            double fi = 200 / Math.PI;            
            double dy = distance * Math.Sin(bearing / fi);  // dy
            double dx = distance * Math.Cos(bearing / fi);  // dx   
            POINT point = new POINT();
            point.y = startpoint.y + dy;
            point.x = startpoint.x + dx;
            return point;
        }


        /// <summary>
        /// Určí, kde leží bod vůči přímce - hodnota > 0 - bod leží vlevo - ve 2D
        /// </summary>
        /// <param name="point1">bod1 přímky</param>
        /// <param name="point2">bod2 přímky</param>
        /// <param name="pointA">bod A</param>
        /// <returns>hodnota</returns>
        public double GetPositionOnLine(POINT point1, POINT point2, POINT pointA)
        {
            return ((point2.y - point1.y) * (pointA.x - point1.x)) - ((pointA.y - point1.y) * (point2.x - point1.x));
        }


    }
}
