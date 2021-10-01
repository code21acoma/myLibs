using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;



/*
72
Horizontal text justification type (optional, default = 0) integer codes (not bit-coded)
0 = Left; 1= Center; 2 = Right
3 = Aligned (if vertical alignment = 0)
4 = Middle (if vertical alignment = 0)
5 = Fit (if vertical alignment = 0)
See the Group 72 and 73 integer codes table for clarification.
  
73
Vertical text justification type (optional, default = 0): integer codes (not bit- coded):
0 = Baseline; 1 = Bottom; 2 = Middle; 3 = Top
See the Group 72 and 73 integer codes table for clarification.  

*/

/*
  
POLYLINE

0        <<< POLYLINE entity
POLYLINE
8        <<< layer group code required
0        <<< layer '0'
62       <<< color group code not required
1        <<< Color number
66       <<< entities follow flag
1        <<< always 1, POLYLINE without vertices is pointless
70       <<< flags, see reference
8        <<< this is a 3D polyline
0        <<< 1. VERTEX entity
VERTEX
8        <<< layer group code required
0        <<< ALWAYS the same as POLYLINE layer!
70       <<< flags
32       <<< 3D polyline vertex
10       <<< x-coord group code
4.0      <<< x-coord value
20       <<< y-coord group code
3.0      <<< y-coord value
30       <<< y-coord group code
2.0      <<< z-coord value
0        <<< 2. VERTEX entity
VERTEX
8
0
70
32
10
8.0
20
5.0
30
0.0
0        <<< 3. VERTEX entity
VERTEX
8
0
70
32
10
2.0
20
4.0
30
9.0
0        <<< required SEQEND entity
SEQEND











*/
namespace DXF
{
    class DXF
    {
        public class ELEMENT
        {
            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }
            public string layer { get; set; }
            public int color { get; set; }
            public double thick { get; set; }
        }
        
        public class LINE : ELEMENT
        {
            public double x1 { get; set; }
            public double y1 { get; set; }
            public double x2 { get; set; }
            public double y2 { get; set; }
            public double z1 { get; set; }
            public double z2 { get; set; }            
        }
        
        public List<LINE> LINElist = new List<LINE>();

        public class TEXT : ELEMENT
        {
            public string write { get; set; }            
            public double hJust { get; set; }
            /*
             Horizontal text justification type (optional, default = 0) integer codes (not bit-coded)
             0 = Left; 1= Center; 2 = Right
             3 = Aligned (if vertical alignment = 0)
             4 = Middle (if vertical alignment = 0)
             5 = Fit (if vertical alignment = 0)
             */
            public double vJust { get; set; }
            /*
             Vertical text justification type (optional, default = 0): integer codes (not bit- coded):
             0 = Baseline; 1 = Bottom; 2 = Middle; 3 = Top
             See the Group 72 and 73 integer codes table for clarification.              
             */          
            public double height { get; set; } // výška textu
            public double angle { get; set; }  // uhel otočení
            public double widthFactor { get; set; }  // faktor šířky textu
            public double angle2 { get; set; }  // druhy uhel ???
            public string type { get; set; }  // typ pisma ??? STANDARD
        }

        public List<TEXT> TEXTlist = new List<TEXT>();

        public class ARC : ELEMENT
        {            
            public double startAngle { get; set; }
            public double endAngle { get; set; }
            public double radius { get; set; }
            public double ext_x { get; set; }
            public double ext_y { get; set; }
            public double ext_z { get; set; }
        }

        public List<ARC> ARClist = new List<ARC>();

        public class CIRCLE : ELEMENT
        {            
            public double radius { get; set; }
        }

        public List<CIRCLE> CIRCLElist = new List<CIRCLE>();

        public class POINT : ELEMENT
        {
            public string name;
        }

        public List<POINT> POINTlist = new List<POINT>();

        public class TRIANGLE : ELEMENT
        {
            public POINT point1 { get; set; }
            public POINT point2 { get; set; }
            public POINT point3 { get; set; }            
        }

        public List<TRIANGLE> TRIANGLElist = new List<TRIANGLE>();

        public class POLYLINE : ELEMENT
        {
            public List<POINT> points { get; set; }
        }

        public List<POLYLINE> POLYLINElist = new List<POLYLINE>();


        static string RemoveWhiteSpaces(string value)
        {
            value = value.Trim();
            return Regex.Replace(value, @"\s+", " ");
        }

        /// <summary>
        /// Vrátí název souboru podle požadovaného typu
        /// </summary>
        /// <param name="file">vstupní soubor</param>
        /// <param name="type">typ: 1 = soubor bez koncovky, 2 = soubor s koncovkou, 3 = cesta k souboru</param>
        /// <returns>výstupní název souboru</returns>
        public string getFile(string file, int type)
        {
            if (type == 1) file = System.IO.Path.GetFileNameWithoutExtension(file);
            if (type == 2) file = System.IO.Path.GetFileName(file);
            if (type == 3) file = System.IO.Path.GetDirectoryName(file);
            return file;
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public DXF()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Clear all entities
        /// </summary>
        public void clearAll()
        {
            LINElist.Clear();
            TEXTlist.Clear();
            ARClist.Clear();
            CIRCLElist.Clear();
            POINTlist.Clear();
            POLYLINElist.Clear();
            TRIANGLElist.Clear();
        }

        
        /// <summary>
        /// Add Line
        /// </summary>
        /// <param name="x1">x-coordinate of first point</param>
        /// <param name="y1">y-coordinate of first point</param>
        /// <param name="z1">z-coordinate of first point</param>
        /// <param name="x2">x-coordinate of second point</param>
        /// <param name="y2">y-coordinate of second point</param>
        /// <param name="z2">z-coordinate of second point</param>
        /// <param name="layer">name of layer</param>
        /// <param name="color">number of color</param>
        /// <param name="thick">thickness in mm</param>
        public void addLine(double x1, double y1, double z1, double x2, double y2, double z2, string layer, int color, double thick)
        {
            LINE line = new LINE();
            line.x1 = x1;
            line.x2 = x2;            
            line.y1 = y1;
            line.y2 = y2;
            line.z1 = z1;
            line.z2 = z2;
            line.layer = layer;
            line.color = color;
            line.thick = thick;
            LINElist.Add(line);
        }

        /// <summary>
        /// Add Line (reduced elements)
        /// </summary>
        /// <param name="x1">x-coordinate of first point</param>
        /// <param name="y1">y-coordinate of first point</param>
        /// <param name="z1">z-coordinate of first point</param>
        /// <param name="x2">x-coordinate of second point</param>
        /// <param name="y2">y-coordinate of second point</param>
        /// <param name="z2">z-coordinate of second point</param>
        /// <param name="layer">name of layer</param>
        /// <param name="color">number of color</param>
        public void addLine(double x1, double y1, double z1, double x2, double y2, double z2, string layer, int color, int thick)
        {
            LINE line = new LINE();
            line.x1 = x1;
            line.x2 = x2;
            line.y1 = y1;
            line.y2 = y2;
            line.z1 = z1;
            line.z2 = z2;
            line.layer = layer;
            line.color = color;
            line.thick = thick;
            LINElist.Add(line);
        }


        /// <summary>
        /// Add Line (reduced elements)
        /// </summary>
        /// <param name="x1">x-coordinate of first point</param>
        /// <param name="y1">y-coordinate of first point</param>
        /// <param name="z1">z-coordinate of first point</param>
        /// <param name="x2">x-coordinate of second point</param>
        /// <param name="y2">y-coordinate of second point</param>
        /// <param name="z2">z-coordinate of second point</param>
        public void addLine(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            LINE line = new LINE();
            line.x1 = x1;
            line.x2 = x2;
            line.y1 = y1;
            line.y2 = y2;
            line.z1 = z1;
            line.z2 = z2;
            line.layer = "NONAME";
            line.color = 0;
            line.thick = 0.2;
            LINElist.Add(line);
        }

        /// <summary>
        /// Add Text
        /// </summary>
        /// <param name="write">text</param>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <param name="z">z-coordinate</param>
        /// <param name="hJust">horizontal text justification (default = 0)</param>
        /// <param name="vJust">vertikal text justification (default = 0)</param>
        /// <param name="layer">name of layer</param>
        /// <param name="color">number of color</param>
        /// <param name="thick">thickness in mm</param>
        /// <param name="height">height of text in mm</param>
        /// <param name="angle">angle of rotation (0 = horizontal)</param>
        public void addText(string write, double x, double y, double z, int hJust, int vJust, string layer, int color, double thick, double height, double angle)
        {
            TEXT text = new TEXT();
            text.write = write;
            text.x = x;
            text.y = y;
            text.z = z;
            text.hJust = hJust;
            text.vJust = vJust;
            text.layer = layer;
            text.color = color;
            text.thick = thick;
            text.height = height;
            text.angle = angle;
            text.angle2 = 0;
            text.widthFactor = 0;
            text.type = "STANDARD";
            TEXTlist.Add(text);
        }

        /// <summary>
        /// Add Arc
        /// </summary>
        /// <param name="x">x-coordinate of center point</param>
        /// <param name="y">y-coordinate of center point</param>
        /// <param name="z">z-coordinate of center point</param>
        /// <param name="ext_x">extrution x</param>
        /// <param name="ext_y">extrution y</param>
        /// <param name="ext_z">extrution z</param>
        /// <param name="startAngle">start angle in DEG</param>
        /// <param name="endAngle">end angle in DEG</param>
        /// <param name="radius">radius</param>
        /// <param name="layer">name of layer</param>
        /// <param name="color">number of color</param>
        /// <param name="thick">thickness in mm</param>
        public void addArc(double x, double y, double z, double ext_x, double ext_y, double ext_z, double startAngle, double endAngle, double radius, string layer, int color, double thick)
        {
            ARC arc = new ARC();
            arc.x = x;
            arc.y = y;
            arc.z = z;
            arc.ext_x = ext_x;
            arc.ext_y = ext_y;
            arc.ext_z = ext_z;
            arc.startAngle = startAngle; // 0 = vodorovna osa, uhel stoupa proti smeru hodinovych rucicek
            arc.endAngle = endAngle;
            arc.radius = radius;
            arc.layer = layer;
            arc.color = color;
            arc.thick = thick;
            ARClist.Add(arc);
        }


        /// <summary>
        /// Add Circle
        /// </summary>
        /// <param name="x">x-coordinate of center point</param>
        /// <param name="y">y-coordinate of center point</param>
        /// <param name="z">z-coordinate of center point</param>
        /// <param name="radius">radius</param>
        /// <param name="layer">name of layer</param>
        /// <param name="color">number of color</param>
        /// <param name="thick">thickness in mm</param>
        public void addCircle(double x, double y, double z, double radius, string layer, int color, double thick)
        {
            CIRCLE circle = new CIRCLE();
            circle.x = x;
            circle.y = y;
            circle.z = z;
            circle.radius = radius;
            circle.layer = layer;
            circle.color = color;
            circle.thick = thick;
            CIRCLElist.Add(circle);
        }

        /// <summary>
        /// Add Point
        /// </summary>
        /// <param name="x">x-coordinate of center point</param>
        /// <param name="y">y-coordinate of center point</param>
        /// <param name="z">z-coordinate of center point</param>
        /// <param name="layer">name of layer</param>
        /// <param name="color">number of color</param>
        /// <param name="thick">thickness in mm</param>
        public void addPoint(double x, double y, double z, string layer, int color, double thick)
        {
            POINT point = new POINT();
            point.x = x;
            point.y = y;
            point.z = z;            
            point.layer = layer;
            point.color = color;
            point.thick = thick;
            POINTlist.Add(point);
        }

        public void addTriangle(POINT point1, POINT point2, POINT point3, string layer)
        {
            TRIANGLE triangle = new TRIANGLE();
            triangle.point1 = point1;
            triangle.point2 = point2;
            triangle.point3 = point3;
            triangle.layer = layer;
            TRIANGLElist.Add(triangle);
        }

        
        public void addPolyline(double[,] points, int count, string layer, int color, int thick)
        {
            POLYLINE polyline = new POLYLINE();

            for (int i = 0; i < count; i++)
            {
                POINT point = new POINT();
                point.x = points[i, 0];
                point.y = points[i, 1];
                point.z = points[i, 2];
                point.layer = layer;
                point.color = color;
                point.thick = thick;
                polyline.points.Add(point);
            }

            polyline.layer = layer;
            polyline.color = color;
            polyline.thick = thick;

            POLYLINElist.Add(polyline);            
        }


        private void hlavicka(StreamWriter sw)
        {
            sw.WriteLine("0");
            sw.WriteLine("SECTION");
            sw.WriteLine("2");
            sw.WriteLine("HEADER");
            //sw.WriteLine("9");
            //sw.WriteLine("$EXTMIN");
            //sw.WriteLine("10");
            //sw.WriteLine("5425898.292300");
            //sw.WriteLine("20");
            //sw.WriteLine("5647803.312600");
            //sw.WriteLine("9");
            //sw.WriteLine("$EXTMAX");
            //sw.WriteLine("10");
            //sw.WriteLine("5426129.608599");
            //sw.WriteLine("20");
            //sw.WriteLine("5647985.921204");            
            sw.WriteLine("0");
            sw.WriteLine("ENDSEC");
            sw.WriteLine("0");
            sw.WriteLine("SECTION");
            sw.WriteLine("2");
            sw.WriteLine("TABLES");
            sw.WriteLine("0");
            sw.WriteLine("ENDSEC");
            sw.WriteLine("0");
            sw.WriteLine("SECTION");
            sw.WriteLine("2");
            sw.WriteLine("ENTITIES");
            sw.WriteLine("0");
        }

        private void spodek(StreamWriter sw)
        {
            sw.WriteLine("ENDSEC");
            sw.WriteLine("0");
            sw.WriteLine("EOF");
        }

        private void saveLine(StreamWriter sw)
        {
            for (int i = 0; i < LINElist.Count; i++)
            {
                sw.WriteLine("LINE");
                sw.WriteLine("8");
                sw.WriteLine(LINElist[i].layer);
                sw.WriteLine("62");
                sw.WriteLine(LINElist[i].color);
                sw.WriteLine("39");
                sw.WriteLine(LINElist[i].thick);
                sw.WriteLine("10");
                sw.WriteLine(LINElist[i].x1);
                sw.WriteLine("20");
                sw.WriteLine(LINElist[i].y1);
                sw.WriteLine("30");
                sw.WriteLine(LINElist[i].z1);
                sw.WriteLine("11");
                sw.WriteLine(LINElist[i].x2);
                sw.WriteLine("21");
                sw.WriteLine(LINElist[i].y2);
                sw.WriteLine("31");
                sw.WriteLine(LINElist[i].z2);
                sw.WriteLine("0");
            }
        }


        private void saveText(StreamWriter sw)
        {
            for (int i = 0; i < TEXTlist.Count; i++)
            {
                sw.WriteLine("TEXT");
                sw.WriteLine("8");
                sw.WriteLine(TEXTlist[i].layer);
                sw.WriteLine("62");
                sw.WriteLine(TEXTlist[i].color);
                sw.WriteLine("39");
                sw.WriteLine(TEXTlist[i].thick);                
                sw.WriteLine("10");
                sw.WriteLine(TEXTlist[i].x);
                sw.WriteLine("11");
                sw.WriteLine(TEXTlist[i].x);
                sw.WriteLine("20");
                sw.WriteLine(TEXTlist[i].y);
                sw.WriteLine("21");
                sw.WriteLine(TEXTlist[i].y);
                sw.WriteLine("30");
                sw.WriteLine(TEXTlist[i].z);
                sw.WriteLine("40");
                sw.WriteLine(TEXTlist[i].height);
                sw.WriteLine("50");
                sw.WriteLine(TEXTlist[i].angle);
                sw.WriteLine("72");                
                sw.WriteLine(TEXTlist[i].hJust);
                sw.WriteLine("73");
                sw.WriteLine(TEXTlist[i].vJust);
                sw.WriteLine("1");
                sw.WriteLine(TEXTlist[i].write);
                sw.WriteLine("0");
            }

        }

        private void saveArc(StreamWriter sw)
        {
            for (int i = 0; i < ARClist.Count; i++)
            {
                sw.WriteLine("ARC");
                sw.WriteLine("8");
                sw.WriteLine(ARClist[i].layer);
                sw.WriteLine("62");
                sw.WriteLine(ARClist[i].color);
                sw.WriteLine("39");
                sw.WriteLine(ARClist[i].thick);
                sw.WriteLine("10");
                sw.WriteLine(ARClist[i].x);
                sw.WriteLine("20");
                sw.WriteLine(ARClist[i].y);
                sw.WriteLine("30");
                sw.WriteLine(ARClist[i].z);
                sw.WriteLine("40");
                sw.WriteLine(ARClist[i].radius);
                sw.WriteLine("50");
                sw.WriteLine(ARClist[i].startAngle);
                sw.WriteLine("51");
                sw.WriteLine(ARClist[i].endAngle);
                sw.WriteLine("210");
                sw.WriteLine(ARClist[i].ext_x);
                sw.WriteLine("220");
                sw.WriteLine(ARClist[i].ext_y);
                sw.WriteLine("230");
                sw.WriteLine(ARClist[i].ext_z);
                sw.WriteLine("0");
            }

        }

        private void saveCircle(StreamWriter sw)
        {
            for (int i = 0; i < CIRCLElist.Count; i++)
            {
                sw.WriteLine("CIRCLE");
                sw.WriteLine("8");
                sw.WriteLine(CIRCLElist[i].layer);
                sw.WriteLine("62");
                sw.WriteLine(CIRCLElist[i].color);
                sw.WriteLine("39");
                sw.WriteLine(CIRCLElist[i].thick);
                sw.WriteLine("10");
                sw.WriteLine(CIRCLElist[i].x);
                sw.WriteLine("20");
                sw.WriteLine(CIRCLElist[i].y);
                sw.WriteLine("30");
                sw.WriteLine(CIRCLElist[i].z);
                sw.WriteLine("40");
                sw.WriteLine(CIRCLElist[i].radius);                
                sw.WriteLine("0");
            }

        }


        private void savePoint(StreamWriter sw)
        {
            for (int i = 0; i < POINTlist.Count; i++)
            {
                sw.WriteLine("POINT");
                sw.WriteLine("8");
                sw.WriteLine(POINTlist[i].layer);
                sw.WriteLine("62");
                sw.WriteLine(POINTlist[i].color);
                sw.WriteLine("39");
                sw.WriteLine(POINTlist[i].thick);
                sw.WriteLine("10");
                sw.WriteLine(POINTlist[i].x);
                sw.WriteLine("20");
                sw.WriteLine(POINTlist[i].y);
                sw.WriteLine("30");
                sw.WriteLine(POINTlist[i].z);
                sw.WriteLine("0");
            }
        }

        private void saveTriangle(StreamWriter sw)
        {
            for (int i = 0; i < TRIANGLElist.Count; i++)
            {
                sw.WriteLine("3DFACE");
                sw.WriteLine("8");
                sw.WriteLine(TRIANGLElist[i].layer);
                sw.WriteLine("62");
                sw.WriteLine(TRIANGLElist[i].color);
                sw.WriteLine("39");
                sw.WriteLine(TRIANGLElist[i].thick);
                sw.WriteLine("10");
                sw.WriteLine(TRIANGLElist[i].point1.x);
                sw.WriteLine("20");
                sw.WriteLine(TRIANGLElist[i].point1.y);
                sw.WriteLine("30");
                sw.WriteLine(TRIANGLElist[i].point1.z);
                sw.WriteLine("11");
                sw.WriteLine(TRIANGLElist[i].point2.x);
                sw.WriteLine("21");
                sw.WriteLine(TRIANGLElist[i].point2.y);
                sw.WriteLine("31");
                sw.WriteLine(TRIANGLElist[i].point2.z);
                sw.WriteLine("12");
                sw.WriteLine(TRIANGLElist[i].point3.x);
                sw.WriteLine("22");
                sw.WriteLine(TRIANGLElist[i].point3.y);
                sw.WriteLine("32");
                sw.WriteLine(TRIANGLElist[i].point3.z);
                sw.WriteLine("13");
                sw.WriteLine(TRIANGLElist[i].point3.x);
                sw.WriteLine("23");
                sw.WriteLine(TRIANGLElist[i].point3.y);
                sw.WriteLine("33");
                sw.WriteLine(TRIANGLElist[i].point3.z);
                sw.WriteLine("0");
            }
        }

        private void savePolyline(StreamWriter sw)
        {
            for (int i = 0; i < POLYLINElist.Count; i++)
            {
                sw.WriteLine("POLYLINE");
                sw.WriteLine("8");
                sw.WriteLine(POLYLINElist[i].layer);
                sw.WriteLine("62");
                sw.WriteLine(POLYLINElist[i].color);
                sw.WriteLine("39");
                sw.WriteLine(POLYLINElist[i].thick);
                sw.WriteLine("66");
                sw.WriteLine("1");
                sw.WriteLine("70");
                sw.WriteLine("8");                
                for (int j = 0; j < POLYLINElist[i].points.Count; j++)
                {
                    sw.WriteLine("0");
                    sw.WriteLine("VERTEX");
                    sw.WriteLine("8");
                    sw.WriteLine(POLYLINElist[i].layer);
                    sw.WriteLine("70");
                    sw.WriteLine("32");
                    sw.WriteLine("10");
                    sw.WriteLine(POLYLINElist[i].points[j].x);
                    sw.WriteLine("20");
                    sw.WriteLine(POLYLINElist[i].points[j].y);
                    sw.WriteLine("30");
                    sw.WriteLine(POLYLINElist[i].points[j].z);
                }

                sw.WriteLine("0");
                sw.WriteLine("SEQEND");
                sw.WriteLine("0");                
            }
        }


        /// <summary>
        /// Save all entities in file
        /// </summary>
        public void saveAll(string file)
        {
            StreamWriter dxf = new StreamWriter(file);   // vstupni proud                

            hlavicka(dxf);
            if (LINElist.Count > 0) saveLine(dxf);
            if (TEXTlist.Count > 0) saveText(dxf);
            if (ARClist.Count > 0) saveArc(dxf);
            if (CIRCLElist.Count > 0) saveCircle(dxf);
            if (POINTlist.Count > 0) savePoint(dxf);
            if (TRIANGLElist.Count > 0) saveTriangle(dxf);
            spodek(dxf);
            dxf.Close();
        }

        public void importDXFfile(string file)
        {
            string radek;
            StreamReader dxf = new StreamReader(file);


            while ((radek = dxf.ReadLine()) != null)
            {
                radek = RemoveWhiteSpaces(radek);
                if (radek == "POINT") importPoint(dxf);
                if (radek == "LINE") importLine(dxf);
                if (radek == "CIRCLE") importCircle(dxf);                
                if (radek == "ARC") importArc(dxf);
                if (radek == "3DFACE") importTriangle(dxf);
                if ((radek == "TEXT") || (radek == "MTEXT")) importText(dxf);
                if (radek == "POLYLINE") importPolyline(dxf);
            }

        }

        private void importPoint(StreamReader dxf)
        {
            POINT point = new POINT();
            
            string radek1, radek2;
            bool end = false;

            while (end == false)
            {
                radek1 = dxf.ReadLine();
                radek1 = RemoveWhiteSpaces(radek1);
                if (radek1 == "0")
                {
                    end = true;
                    continue;
                }
                radek2 = dxf.ReadLine();
                radek2 = RemoveWhiteSpaces(radek2);

                if (radek1 == "8") point.layer = radek2;
                if (radek1 == "62") point.color = Convert.ToInt32(radek2);
                if (radek1 == "39") point.thick = Convert.ToDouble(radek2);
                if (radek1 == "10") point.x = Convert.ToDouble(radek2);
                if (radek1 == "20") point.y = Convert.ToDouble(radek2);
                if (radek1 == "30") point.z = Convert.ToDouble(radek2);

            }

            POINTlist.Add(point);

        }

        private void importTriangle(StreamReader dxf)
        {
            TRIANGLE triangle = new TRIANGLE();
            triangle.point1 = new POINT();
            triangle.point2 = new POINT();
            triangle.point3 = new POINT();

            string radek1, radek2;
            bool end = false;

            while (end == false)
            {
                radek1 = dxf.ReadLine();
                radek1 = RemoveWhiteSpaces(radek1);
                if (radek1 == "0")
                {
                    end = true;
                    continue;
                }
                radek2 = dxf.ReadLine();
                radek2 = RemoveWhiteSpaces(radek2);

                if (radek1 == "8") triangle.layer = radek2;
                if (radek1 == "62") triangle.color = Convert.ToInt32(radek2);
                if (radek1 == "39") triangle.thick = Convert.ToDouble(radek2);
                if (radek1 == "10") triangle.point1.x = Convert.ToDouble(radek2);
                if (radek1 == "20") triangle.point1.y = Convert.ToDouble(radek2);
                if (radek1 == "30") triangle.point1.z = Convert.ToDouble(radek2);
                if (radek1 == "11") triangle.point2.x = Convert.ToDouble(radek2);
                if (radek1 == "21") triangle.point2.y = Convert.ToDouble(radek2);
                if (radek1 == "31") triangle.point2.z = Convert.ToDouble(radek2);
                if (radek1 == "12") triangle.point3.x = Convert.ToDouble(radek2);
                if (radek1 == "22") triangle.point3.y = Convert.ToDouble(radek2);
                if (radek1 == "32") triangle.point3.z = Convert.ToDouble(radek2);

            }

            TRIANGLElist.Add(triangle);

        }

        private void importLine(StreamReader dxf)
        {            
            LINE line = new LINE();

            string radek1, radek2;
            bool end = false;

            while (end == false)
            {
                radek1 = dxf.ReadLine();
                radek1 = RemoveWhiteSpaces(radek1);
                if (radek1 == "0")
                {
                    end = true;
                    continue;
                }
                radek2 = dxf.ReadLine();
                radek2 = RemoveWhiteSpaces(radek2);

                if (radek1 == "8") line.layer = radek2;
                if (radek1 == "62") line.color = Convert.ToInt32(radek2);
                if (radek1 == "39") line.thick = Convert.ToDouble(radek2);
                if (radek1 == "10") line.x1 = Convert.ToDouble(radek2);
                if (radek1 == "20") line.y1 = Convert.ToDouble(radek2);
                if (radek1 == "30") line.z1 = Convert.ToDouble(radek2);
                if (radek1 == "11") line.x2 = Convert.ToDouble(radek2);
                if (radek1 == "21") line.y2 = Convert.ToDouble(radek2);
                if (radek1 == "33") line.z2 = Convert.ToDouble(radek2);

            }

            LINElist.Add(line);
        }


        private void importPolyline(StreamReader dxf)
        {
            // TODO
            
            LINE line = new LINE();

            string radek1, radek2;
            bool end = false;

            while (end == false)
            {
                radek1 = dxf.ReadLine();
                radek1 = RemoveWhiteSpaces(radek1);
                if (radek1 == "0")
                {
                    end = true;
                    continue;
                }
                radek2 = dxf.ReadLine();
                radek2 = RemoveWhiteSpaces(radek2);

                if (radek1 == "8") line.layer = radek2;
                if (radek1 == "62") line.color = Convert.ToInt32(radek2);
                if (radek1 == "39") line.thick = Convert.ToDouble(radek2);
                if (radek1 == "10") line.x1 = Convert.ToDouble(radek2);
                if (radek1 == "20") line.y1 = Convert.ToDouble(radek2);
                if (radek1 == "30") line.z1 = Convert.ToDouble(radek2);
                if (radek1 == "11") line.x2 = Convert.ToDouble(radek2);
                if (radek1 == "21") line.y2 = Convert.ToDouble(radek2);
                if (radek1 == "33") line.z2 = Convert.ToDouble(radek2);

            }

            LINElist.Add(line);
        }

        private void importCircle(StreamReader dxf)
        {
            CIRCLE circle = new CIRCLE();

            string radek1, radek2;
            bool end = false;

            while (end == false)
            {
                radek1 = dxf.ReadLine();
                radek1 = RemoveWhiteSpaces(radek1);
                if (radek1 == "0")
                {
                    end = true;
                    continue;
                }
                radek2 = dxf.ReadLine();
                radek2 = RemoveWhiteSpaces(radek2);

                if (radek1 == "8") circle.layer = radek2;
                if (radek1 == "62") circle.color = Convert.ToInt32(radek2);
                if (radek1 == "39") circle.thick = Convert.ToDouble(radek2);
                if (radek1 == "10") circle.x = Convert.ToDouble(radek2);
                if (radek1 == "20") circle.y = Convert.ToDouble(radek2);
                if (radek1 == "30") circle.z = Convert.ToDouble(radek2);
                if (radek1 == "40") circle.radius = Convert.ToDouble(radek2);

            }

            CIRCLElist.Add(circle);
        }

        private void importArc(StreamReader dxf)
        {
            ARC arc = new ARC();

            string radek1, radek2;
            bool end = false;

            while (end == false)
            {
                radek1 = dxf.ReadLine();
                radek1 = RemoveWhiteSpaces(radek1);
                if (radek1 == "0")
                {
                    end = true;
                    continue;
                }
                radek2 = dxf.ReadLine();
                radek2 = RemoveWhiteSpaces(radek2);

                if (radek1 == "8") arc.layer = radek2;
                if (radek1 == "62") arc.color = Convert.ToInt32(radek2);
                if (radek1 == "39") arc.thick = Convert.ToDouble(radek2);
                if (radek1 == "10") arc.x = Convert.ToDouble(radek2);
                if (radek1 == "20") arc.y = Convert.ToDouble(radek2);
                if (radek1 == "30") arc.z = Convert.ToDouble(radek2);
                if (radek1 == "40") arc.radius = Convert.ToDouble(radek2);
                if (radek1 == "50") arc.startAngle = Convert.ToDouble(radek2);
                if (radek1 == "51") arc.endAngle = Convert.ToDouble(radek2);
                if (radek1 == "210") arc.ext_x = Convert.ToDouble(radek2);
                if (radek1 == "220") arc.ext_y = Convert.ToDouble(radek2);
                if (radek1 == "230") arc.ext_z = Convert.ToDouble(radek2);

            }

            ARClist.Add(arc);

        }

        private void importText(StreamReader dxf)
        {   
            TEXT text = new TEXT();

            string radek1, radek2;
            bool end = false;

            while (end == false)
            {
                radek1 = dxf.ReadLine();
                radek1 = RemoveWhiteSpaces(radek1);
                if (radek1 == "0")
                {
                    end = true;
                    continue;
                }
                radek2 = dxf.ReadLine();
                radek2 = RemoveWhiteSpaces(radek2);

                if (radek1 == "8") text.layer = radek2;
                if (radek1 == "62") text.color = Convert.ToInt32(radek2);
                if (radek1 == "39") text.thick = Convert.ToDouble(radek2);
                if (radek1 == "10") text.x = Convert.ToDouble(radek2);
                if (radek1 == "20") text.y = Convert.ToDouble(radek2);
                if (radek1 == "30") text.z = Convert.ToDouble(radek2);
                if (radek1 == "40") text.height = Convert.ToDouble(radek2);
                if (radek1 == "50") text.angle = Convert.ToDouble(radek2);
                if (radek1 == "72") text.hJust = Convert.ToDouble(radek2);
                if (radek1 == "73") text.vJust = Convert.ToDouble(radek2);
                if (radek1 == "1") text.write = radek2;

            }

            TEXTlist.Add(text);
        }

        public List<string> getLayers()
        {
            List<string> layers = new List<string>();            

            foreach (POINT point in POINTlist)
            {
                if (layers.Contains(point.layer) == false) layers.Add(point.layer);
            }

            foreach (LINE line in LINElist)
            {
                if (layers.Contains(line.layer) == false) layers.Add(line.layer);
            }

            foreach (CIRCLE circle in CIRCLElist)
            {
                if (layers.Contains(circle.layer) == false) layers.Add(circle.layer);
            }

            foreach (ARC arc in ARClist)
            {
                if (layers.Contains(arc.layer) == false) layers.Add(arc.layer);
            }

            foreach (TEXT text in TEXTlist)
            {
                if (layers.Contains(text.layer) == false) layers.Add(text.layer);
            }

            foreach (TRIANGLE triangle in TRIANGLElist)
            {
                if (layers.Contains(triangle.layer) == false) layers.Add(triangle.layer);
            }

            foreach (POLYLINE polyline in POLYLINElist)
            {
                if (layers.Contains(polyline.layer) == false) layers.Add(polyline.layer);
            }

            return layers;
        }

        
        public List<string> ExportPoints(string layer)
        {
            List<string> list = new List<string>();
            if (layer == null)
            {
                foreach (TEXT text in TEXTlist)
                {
                    list.Add(text.write + " " + (-text.x).ToString() + " " + (-text.y).ToString() + " " + (text.z).ToString());
                }
            }
            else
            {
                var pl = TEXTlist.Where(p => p.layer == layer);
                List<TEXT> myPoints = new List<TEXT>(pl);
                foreach (TEXT text in myPoints)
                {
                    list.Add(text.write + " " + (-text.x).ToString() + " " + (-text.y).ToString() + " " + (text.z).ToString());
                }
            }

            return list;
        }

        public List<string> ExportLines(string layer)
        {
            List<string> list = new List<string>();
            if (layer == null)
            {
                foreach (LINE line in LINElist)
                {
                    list.Add((-line.x1).ToString() + " " + (-line.y1).ToString() + " " + (line.z1).ToString() + " " + (-line.x2).ToString() + " " + (-line.y2).ToString() + " " + (line.z2).ToString());
                }
            }
            else
            {
                var pl = LINElist.Where(p => p.layer == layer);
                List<LINE> myLines = new List<LINE>(pl);
                foreach (LINE line in myLines)
                {
                    list.Add((-line.x1).ToString() + " " + (-line.y1).ToString() + " " + (line.z1).ToString() + " " + (-line.x2).ToString() + " " + (-line.y2).ToString() + " " + (line.z2).ToString());
                }
            }

            return list;
        }

        public List<string> ExportTriangles(string layer)
        {
            List<string> list = new List<string>();
            if (layer == null)
            {
                foreach (TRIANGLE triangle in TRIANGLElist)
                {
                    string name = "";
                    if ((triangle.point1.name != null) || (triangle.point1.name != "")) name = triangle.point1.name;                    
                    list.Add(name + " " + (-triangle.point1.x).ToString() + " " + (-triangle.point1.y).ToString() + " " + (triangle.point1.z).ToString());
                    name = "";
                    if ((triangle.point2.name != null) || (triangle.point2.name != "")) name = triangle.point2.name;
                    list.Add(name + " " + (-triangle.point2.x).ToString() + " " + (-triangle.point2.y).ToString() + " " + (triangle.point2.z).ToString());
                    name = "";
                    if ((triangle.point3.name != null) || (triangle.point3.name != "")) name = triangle.point3.name;
                    list.Add(name + " " + (-triangle.point3.x).ToString() + " " + (-triangle.point3.y).ToString() + " " + (triangle.point3.z).ToString());
                }
            }
            else
            {
                var pl = TRIANGLElist.Where(p => p.layer == layer);
                List<TRIANGLE> myTriangles = new List<TRIANGLE>(pl);
                int x = 1;
                foreach (TRIANGLE triangle in myTriangles)
                {                    
                    string name = x.ToString();
                    x++;
                    list.Add(name + " " + (-triangle.point1.x).ToString() + " " + (-triangle.point1.y).ToString() + " " + (triangle.point1.z).ToString());
                    name = x.ToString();
                    x++;                    
                    list.Add(name + " " + (-triangle.point2.x).ToString() + " " + (-triangle.point2.y).ToString() + " " + (triangle.point2.z).ToString());
                    name = x.ToString();
                    x++;
                    list.Add(name + " " + (-triangle.point3.x).ToString() + " " + (-triangle.point3.y).ToString() + " " + (triangle.point3.z).ToString());
                }
            }

            return list;
        }

        public void FindPointNumberToTriangles()
        {
            for (int i = 0; i < TRIANGLElist.Count; i++)            
            {
                for (int j = 0; j < TEXTlist.Count; j++)
                {
                    if ((TRIANGLElist[i].point1.x == TEXTlist[j].x) && (TRIANGLElist[i].point1.y == TEXTlist[j].y) && (TRIANGLElist[i].point1.y == TEXTlist[j].y))
                    {
                        TRIANGLElist[i].point1.name = TEXTlist[j].write;
                    }

                    if ((TRIANGLElist[i].point2.x == TEXTlist[j].x) && (TRIANGLElist[i].point2.y == TEXTlist[j].y) && (TRIANGLElist[i].point2.y == TEXTlist[j].y))
                    {
                        TRIANGLElist[i].point2.name = TEXTlist[j].write;
                    }

                    if ((TRIANGLElist[i].point3.x == TEXTlist[j].x) && (TRIANGLElist[i].point3.y == TEXTlist[j].y) && (TRIANGLElist[i].point3.y == TEXTlist[j].y))
                    {
                        TRIANGLElist[i].point3.name = TEXTlist[j].write;
                    }
                }                                
            }
        }

        public TEXT GetCoordinates(string pointName)
        {
            TEXT returnText = new TEXT();
            string coordinates = "";
            foreach (TEXT text in TEXTlist)
            {
                if (pointName == text.write) return text;                
            }
            return returnText;
        }


        public void ImportDtmFile(string file)
        {
            string pointsName = "points";
            string breaklineName = "breaklines";
            string triangleName = "triangles";            

            string radek;
            string[] parts;

            StreamReader sR = new StreamReader(file);
            int count = Convert.ToInt32(sR.ReadLine());
            for (int i = 0; i < count; i++)
            {
                radek = sR.ReadLine();
                parts = radek.Split(' ');
                addText(parts[0], Convert.ToDouble(parts[1]), Convert.ToDouble(parts[2]), Convert.ToDouble(parts[3]), 0, 0, pointsName, 0, 0, 1, 0);                
            }
            count = Convert.ToInt32(sR.ReadLine());
            for (int i = 0; i < count; i++)
            {
                radek = sR.ReadLine();
                parts = radek.Split(' ');
                TEXT text1 = GetCoordinates(parts[0]);
                TEXT text2 = GetCoordinates(parts[1]);
                addLine(text1.x, text1.y, text1.z, text2.x, text2.y, text2.z, breaklineName, 5, 0);                
            }
            count = Convert.ToInt32(sR.ReadLine());
            for (int i = 0; i < count; i++)
            {
                radek = sR.ReadLine();
                parts = radek.Split(' ');
                TEXT text1 = GetCoordinates(parts[0]);
                TEXT text2 = GetCoordinates(parts[1]);
                TEXT text3 = GetCoordinates(parts[2]);
                addLine(text1.x, text1.y, text1.z, text2.x, text2.y, text2.z, triangleName, 1, 0);
                addLine(text2.x, text2.y, text2.z, text3.x, text3.y, text3.z, triangleName, 1, 0);
                addLine(text3.x, text3.y, text3.z, text1.x, text1.y, text1.z, triangleName, 1, 0);
            }
        }


    }
}
