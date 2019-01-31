using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AFunc;


namespace DTM
{
    public class DTM
    {
        public class POINT
        {
            public string cislo { get; set; }
            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }
        }
        
        List<POINT> points = new List<POINT>();

        public class LINE
        {
            public string startPoint { get; set; }
            public string endPoint { get; set; }
        }
        
        List<LINE> lines = new List<LINE>();

        public class TRIANGLE
        {            
            public string point1 { get; set; }
            public string point2 { get; set; }
            public string point3 { get; set; }
        }

        List<TRIANGLE> triangles = new List<TRIANGLE>();

        /// <summary>
        /// Constructors
        /// </summary>
        public DTM(string name) { }

        public DTM(string name, List<string> pointsX, List<string> linesX)
        {
            for (int i = 0; i < pointsX.Count; i++)
            {
                POINT point = new POINT();
                string[] prvky;
                prvky = pointsX[i].Split(' ');
                point.cislo = prvky[0];
                point.y = Convert.ToDouble(prvky[1]);
                point.x = Convert.ToDouble(prvky[2]);
                point.z = Convert.ToDouble(prvky[3]);
                points.Add(point);
            }

            for (int i = 0; i < linesX.Count; i++)
            {
                LINE line = new LINE();
                string[] prvky1;
                prvky1 = linesX[i].Split(' ');
                double y1 = Convert.ToDouble(prvky1[0]);
                double x1 = Convert.ToDouble(prvky1[1]);
                double z1 = Convert.ToDouble(prvky1[2]);
                double y2 = Convert.ToDouble(prvky1[3]);
                double x2 = Convert.ToDouble(prvky1[4]);
                double z2 = Convert.ToDouble(prvky1[5]);
                string pointName1 = FindPoint(y1, x1);
                string pointName2 = FindPoint(y2, x2);
                line.startPoint = pointName1;
                line.endPoint = pointName2;
                lines.Add(line);
            }
        }

        private string FindPoint(double y, double x)
        {
            double precision = 0.01;
            string pointName = "";
            foreach (POINT point in points)
            {
                if ((Math.Abs(point.x - x) < precision) && (Math.Abs(point.y - y) < precision)) pointName = point.cislo;
            }
            return pointName;
        }

        private int GetIndex(string number)
        {
            int c = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (number == points[i].cislo) c = i;
            }

            return c;
        }

        private string GetPointNumber(int index)
        {
            return points[index].cislo;
        }
       

        public string CheckConsistenceOfInputData()
        {
            //int consist = false;
            string consist = "OK";

            // Check Point name duplicity
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = 1; j < points.Count; j++)
                {
                    if (points[i].cislo == points[j].cislo)
                    {
                        consist = "Duplicitní číslo bodu " + points[i].cislo;
                        return consist;
                    }
                }
            }
            // Check near points (cca 5 cm)
            double minDistance = 0.05;
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = 1; j < points.Count; j++)
                {
                    if  (((points[i].x - points[j].x) * (points[i].x - points[j].x)) + ((points[i].y - points[j].y) * (points[i].y - points[j].y)) < minDistance)                    
                    {
                        consist = "Minimální odstup bodů " + minDistance + " m byl překročení mezi body " + points[i].cislo + " a " + points[j].cislo;
                        return consist;
                    }
                }
            }

            // Check lines - from to same point lines
            for (int j = 0; j < lines.Count; j++)
            {
                if (lines[j].startPoint == lines[j].endPoint)
                {
                    consist = "Linie č. " + j + " se skládá ze dvou stejných bodů " + lines[j].startPoint + " = " + lines[j].endPoint;
                    return consist;
                }
            }


            // Check lines - empty point of line
            for (int j = 0; j < lines.Count; j++)
            {
                if ((lines[j].startPoint == "") || (lines[j].endPoint == ""))
                {
                    consist = "Linie č. " + j + " ovbsahuje nedefinovaný bod.";
                    return consist;
                }
            }

            return consist;
        }

        public void SavePolyFile(string file)
        {                        
            string fileName = System.IO.Path.GetFileName(file);
            StreamWriter writer = new StreamWriter(file);
            writer.WriteLine("# " + fileName);
            writer.WriteLine("#");
            writer.WriteLine("# Generated: " + DateTime.Now);
            writer.WriteLine("#");
            writer.WriteLine("#  Declare vertices, 2 dimensions, 0 attribute, no boundary markers.");
            writer.WriteLine("#");
            writer.WriteLine(points.Count + " 2 0 0");
            writer.WriteLine("#");
            writer.WriteLine("#  List the vertices.");
            writer.WriteLine("#");
            for (int i = 0; i < points.Count; i++)
            {
                writer.WriteLine((i + 1).ToString() + " " + (points[i].y).ToString() + " " + (points[i].x).ToString());
            }
            writer.WriteLine("#");
            writer.WriteLine("# Declare the number of segments.");
            writer.WriteLine("#");
            int j = 0;
            List<string> brlines = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                brlines.Add((j + 1).ToString() + " " + ((GetIndex(lines[i].startPoint) + 1).ToString()) + " " + ((GetIndex(lines[i].endPoint) + 1).ToString()));
                j++;
            }
            writer.WriteLine(brlines.Count + " 0");
            writer.WriteLine("#");
            writer.WriteLine("#  List the segments.");
            writer.WriteLine("#");
            foreach (string s in brlines)
            {
                writer.WriteLine(s);
            }
            writer.WriteLine("#");
            writer.WriteLine("# Declare the number of holes.");
            writer.WriteLine("#");
            writer.WriteLine("0");
            writer.WriteLine("#");
            writer.WriteLine("#  Specify the hole.");
            writer.WriteLine("#");
            writer.Close();
            

        }
      
        public void ImportTrianglesFromPoly(string file)
        {
            string[] parts;
            StreamReader sR = new StreamReader(file);
            string radek = sR.ReadLine();
            radek = AFunc.AFunc.RemoveWhiteSpaces(radek);
            parts = radek.Split(' ');
            int count = Convert.ToInt32(parts[0]);
            for (int i = 0; i < count; i++)
            {
                radek = sR.ReadLine();
                radek = AFunc.AFunc.RemoveWhiteSpaces(radek);
                parts = radek.Split(' ');
                TRIANGLE triangle = new TRIANGLE();
                triangle.point1 = points[Convert.ToInt32(parts[1]) - 1].cislo;
                triangle.point2 = points[Convert.ToInt32(parts[2]) - 1].cislo;
                triangle.point3 = points[Convert.ToInt32(parts[3]) - 1].cislo;
                triangles.Add(triangle);
            }
            sR.Close();

        }

        public void Triangulate(string outputFile)
        {
            // SavePolyFile -> Triangle64.exe -> ELE file -> Triangles
            string polyFile = outputFile.Replace(".dmt", ".poly");
            SavePolyFile(polyFile);
            //Process.Start("triangle64 -PN " + polyFile);   
            string exeFile = "triangle64.exe";
            string parameter = "-PN " + polyFile;
            AFunc.AFunc.LaunchCommandLineApp(exeFile, parameter);            
            string eleFile = outputFile.Replace(".dmt", "") + ".1.ele";
            ImportTrianglesFromPoly(eleFile);            
        }

        public void ExportTriangles()
        {
            // Export triangles, then send to Viewer2D
        }


        




    }
}
