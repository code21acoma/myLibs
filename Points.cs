using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Points
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
        /// Find type of import file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string GetTypeOfImportFile(string file)
        {            
            StreamReader reader = new StreamReader(file);
            string radek = reader.ReadLine();
            reader.Close();
            if (radek[0] == '*') return "gsi";
            if (radek.Contains(";")) return "csv";
            else return "txt";                     
        }
        
        
        
        /// <summary>
        /// Import defined txt and csv file with points
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
        /// Import defined txt and csv file with points
        /// </summary>
        /// <param name="file">file path</param>        
        public void Import(string file)
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
                if (radek[0] == '#') continue;
                if ((radek == "") || (radek == " ")) continue;

                point.n = prvky[0];
                point.y = Convert.ToDouble(prvky[1]);
                point.x = Convert.ToDouble(prvky[2]);
                point.z = double.NaN;
                if (prvky.Length > 3)
                {
                    double z;
                    if (double.TryParse(prvky[3], out z)) point.z = z;
                    else point.comment = prvky[3];
                }

                if (prvky.Length > 4) point.comment = prvky[4];                                                        

                points.Add(point);
             
            }
            reader.Close();
        }

        /// <summary>
        /// Import gsi file with points
        /// </summary>
        /// <param name="file">file path</param>        
        public void ImportGSI(string file)
        {
            string radek;
            string[] prvky;
            char separatorIn = ' ';
            string pn = "";
            string y = "";
            string x = "";
            string z = "";
            string poznamka = "";
            bool test = false;
            double parameterMM = 1000;
            string ciferCount = "0.000";

            StreamReader reader = new StreamReader(file);
            while ((radek = reader.ReadLine()) != null)
            {
                if ((radek == "") || (radek == " ")) continue;
                if (!radek.Contains("81..")) continue;
               
                prvky = radek.Split(separatorIn);

                POINT point = new POINT();

                for (int i = 0; i < prvky.Length; i++)
                {
                    if (prvky[i].Contains("*"))
                    {
                        pn = prvky[i].Substring(8, 16);
                        while (pn[0] == '0') pn = pn.Substring(1, pn.Length - 1);
                    }

                    if (prvky[i].Contains("81.."))
                    {
                        if (test == false)
                        {
                            string testY = prvky[i].Substring(7, 16);
                            while (testY[0] == '0') testY = testY.Substring(1, testY.Length - 1);
                            if (testY.Length == 10)
                            {
                                parameterMM = 10000;
                                ciferCount = "0.0000";
                            }
                            test = true;
                        }
                        y = prvky[i].Substring(7, 16);
                        while (y[0] == '0') y = y.Substring(1, y.Length - 1);
                        y = (Convert.ToDouble(y) / parameterMM).ToString(ciferCount);
                    }

                    if (prvky[i].Contains("82.."))
                    {
                        x = prvky[i].Substring(7, 16);
                        while (x[0] == '0') x = x.Substring(1, x.Length - 1);
                        x = (Convert.ToDouble(x) / parameterMM).ToString(ciferCount);
                    }

                    if (prvky[i].Contains("71.."))
                    {
                        poznamka = prvky[i].Substring(7, 16);
                        if (poznamka == "0000000000000000")
                        {
                            poznamka = "";
                            continue;
                        }
                        while (poznamka[0] == '0') poznamka = poznamka.Substring(1, poznamka.Length - 1);
                    }

                    if (prvky[i].Contains("83.."))
                    {
                        z = prvky[i].Substring(7, 16);
                        if (z == "0000000000000000") z = "0";
                        else
                        {
                            while (z[0] == '0') z = z.Substring(1, z.Length - 1);
                            z = (Convert.ToDouble(z) / parameterMM).ToString(ciferCount);
                        }
                    }
                }

                point.n = pn;
                point.y = Convert.ToDouble(y);
                point.x = Convert.ToDouble(x);
                point.z = Convert.ToDouble(z);
                point.comment = poznamka;

                points.Add(point);

            }
            reader.Close();
        }


        /// <summary>
        /// Export defined file with points
        /// </summary>
        /// <param name="file">file path</param>
        /// <param name="type">ouput type - for example NYXZC (number, y, x, z, commnet)</param>
        /// <param name="separator">separator ';' or ' ',...</param>
        public void ExportTxt(string file, string type, string separator)
        {
            StreamWriter writer = new StreamWriter(file);
            string radek;
            for (int i = 0; i < points.Count; i++)
            {
                radek = "";
                for (int j = 0; j < type.Length; j++)
                {
                    if (type[j] == 'N') radek = radek + points[i].n.ToString() + separator;
                    if (type[j] == 'Y') radek = radek + points[i].y.ToString() + separator;
                    if (type[j] == 'X') radek = radek + points[i].x.ToString() + separator;
                    if (type[j] == 'Z') radek = radek + points[i].z.ToString() + separator;
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


        /// <summary>
        /// Export txt file with points
        /// </summary>
        /// <param name="file">file path</param>        
        public void ExportTxt(string file)
        {
            StreamWriter writer = new StreamWriter(file);            
            for (int i = 0; i < points.Count; i++)
            {
                writer.WriteLine(points[i].n + " " + points[i].y + " " + points[i].x + " " + points[i].z + " " + points[i].comment);
            }
            writer.Close();
        }

        /// <summary>
        /// Export csv file with points
        /// </summary>
        /// <param name="file">file path</param>        
        public void ExportCsv(string file)
        {
            StreamWriter writer = new StreamWriter(file);           
            for (int i = 0; i < points.Count; i++)
            {                
                writer.WriteLine(points[i].n + ";" + points[i].y.ToString().Replace(".",",") + ";" + points[i].x.ToString().Replace(".", ",") + ";" + points[i].z.ToString().Replace(".", ",") + ";" + points[i].comment);
            }
            writer.Close();
        }

        /// <summary>
        /// Export gsi file with points
        /// </summary>
        /// <param name="file">file path</param>        
        public void ExportGsi(string file)
        {
            StreamWriter writer = new StreamWriter(file);            
            for (int i = 0; i < points.Count; i++)
            {
                string pn = points[i].n;
                
                while (pn.Length != 16) pn = "0" + pn;
                string y = (Convert.ToDouble(points[i].y) * 1000).ToString("0");
                while (y.Length != 16) y = "0" + y;
                string x = (Convert.ToDouble(points[i].x) * 1000).ToString("0");
                while (x.Length != 16) x = "0" + x;
                string z = " ";
                if (points[i].z != Double.NaN)
                {
                    string signum = "+";
                    z = points[i].z.ToString("0.000");
                    if (Convert.ToDouble(z) >= 0) signum = "+";
                    else
                    {
                        signum = "-";
                        z = (-Convert.ToDouble(z)).ToString("0.000");
                    }
                    z = (Convert.ToDouble(z) * 1000).ToString("0");
                    while (z.Length != 16) z = "0" + z;
                    z = " 83..40" + signum + z + " ";
                }

                writer.WriteLine("*110001+" + pn + " 81..40+" + y + " 82..40+" + x + z);
            }
            writer.Close();
        }




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
