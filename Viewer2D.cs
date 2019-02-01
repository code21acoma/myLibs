using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using DXF;
//using System.Drawing.Drawing2D;

namespace Viewer2DTest
{
    class Viewer2D
    {
        // ---------------------------------------------------------------------------
        // Datové struktury pro práci s daty
        //----------------------------------------------------------------------------
        public class POINT
        {
            public string cislo { get; set; }
            public double x { get; set; }
            public double y { get; set; }
            public string layer { get; set; }
        }

        //POINT bod;
        List<POINT> points = new List<POINT>();

        public class POINTc
        {
            public string cislo { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }


        public class LINE
        {
            public string startPoint { get; set; }
            public string endPoint { get; set; }
            public string layer { get; set; }
        }

        //LINE line;
        List<LINE> lines = new List<LINE>();
        
        public class LAYER
        {
            public string name { get; set; }
            public Color color { get; set; }
            public Font font { get; set; }
        }

        List<LAYER> layers = new List<LAYER>();

        public double maxX = 0;
        public double maxY = 0;
        public double minX = 0;
        public double minY = 0;
        public double centerX = 0;
        public double centerY = 0;
        public double MERITKO = 0;


        // ---------------------------------------------------------------------------
        // Datové struktury pro vykreslení
        //----------------------------------------------------------------------------
        PictureBox pB;
        Graphics g;
        public Bitmap bmp;

        Pen peroRed = new Pen(Color.Red, 1);
        Pen peroBlack = new Pen(Color.Black, 1);
        Pen peroBlue = new Pen(Color.Blue, 1);
        Font FontArial = new Font("Arial", 8);
        SolidBrush blaB = new SolidBrush(Color.Black);
        Brush blackBrush = new SolidBrush(Color.FromArgb(0, 0, 0));

        //--------------myš v nějakém okénku
        public int xM = 0; //primárně pozice - proto xM, yM.. u primární myši mX,mY..
        public int yM = 0;
        //--------------/myš v v abs.souradnicich
        public double YM;
        public double XM;

        public double par;    // parametr posunu a zvetseni


        // ---------------------------------------------------------------------------
        // ***************************************************************************
        // ---------------------------------------------------------------------------
        // Konstruktor
        //----------------------------------------------------------------------------
        // ***************************************************************************
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="pictureBox"></param>
        public Viewer2D(PictureBox pictureBox)
        {
            this.pB = pictureBox;
            Initialisation();
        }


        // ---------------------------------------------------------------------------
        // Inicializace
        //----------------------------------------------------------------------------

        private void Initialisation()
        {
            pB.Image = new Bitmap(pB.Width, pB.Height);
            bmp = new Bitmap(pB.Image);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);  
        }


        // ---------------------------------------------------------------------------
        // Transformace souřadnic
        //----------------------------------------------------------------------------

        public POINTc zobrazeni(POINT bod)
        {
            POINTc bodc = new POINTc();
            bodc.cislo = bod.cislo;
            bodc.y = Convert.ToInt32((bod.x - centerX) * MERITKO);        
            bodc.x = Convert.ToInt32((centerY - bod.y) * MERITKO);
            return bodc;
        }

        public POINT zobrazeni_zpet(POINTc bodc)
        {
            POINT bod = new POINT();
            bod.cislo = bodc.cislo;
            bod.x = Convert.ToDouble((bodc.y / MERITKO) + centerX);
            bod.y = Convert.ToDouble(-(bodc.x / MERITKO) + centerY);
            return bod;
        }

        public void setMinMax()
        {
            minX = points.Min(s => s.x);
            minY = points.Min(s => s.y);
            maxX = points.Max(s => s.x);
            maxY = points.Max(s => s.y);
        }

        private POINT FindPoint(string cislo)
        {
            int index = points.FindIndex(point => point.cislo.Equals(cislo));
            return points[index];
        }

        public void Centrace()
        {
            setMinMax();

            bmp = new Bitmap(pB.Width, pB.Height);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            int image_width = pB.Size.Width;
            int image_height = pB.Size.Height;

            double image_vyska = maxX - minX;
            double image_sirka = maxY - minY;

            double meritkoY = image_width / image_sirka;
            double meritkoX = image_height / image_vyska;
            double meritko = meritkoX;

            if (meritkoX >= meritkoY) meritko = meritkoY;
            if (meritkoX < meritkoY) meritko = meritkoX;
            MERITKO = Convert.ToInt32(Math.Floor(meritko) * 0.9);
            
            double okrajX = ((maxX - minX) - (image_height / MERITKO))/2;            
            centerX = minX + okrajX;
            double okrajY = ((maxY - minY) - (image_width / MERITKO)) / 2;                       
            centerY = maxY - okrajY;

            DrawAll(true);
        }

        /// <summary>
        /// Posun obrazu nahoru
        /// </summary>
        public void PosunNahoru(double par)
        {            
            centerX = centerX - par;
            DrawAll(true);
        }

        /// <summary>
        /// Posun obrazu dolů
        /// </summary>
        public void PosunDolu(double par)
        {            
            centerX = centerX + par;
            DrawAll(true);
        }

        /// <summary>
        /// Posun obrazu doleva
        /// </summary>
        public void PosunDoleva(double par)
        {            
            centerY = centerY + par;
            DrawAll(true);
        }

        /// <summary>
        /// Posun obrazu doprava
        /// </summary>
        public void PosunDoprava(double par)
        {            
            centerY = centerY - par;
            DrawAll(true);
        }


        /// <summary>
        /// Zmenšení obrazu
        /// </summary>
        public void ZmensiObraz(double par)
        {
            double pom;            
            pom = Convert.ToDouble(MERITKO) / (1 + (2 * Convert.ToDouble(par)));
            MERITKO = Convert.ToInt32(pom);
            DrawAll(true);
        }

        /// <summary>
        /// Zvětšení obrazu
        /// </summary>
        public void ZvetsiObraz(double par)
        {
            double pom;            
            pom = Convert.ToDouble(MERITKO) * (1 + (2 * Convert.ToDouble(par)));
            MERITKO = Convert.ToInt32(pom);
            DrawAll(true);
        }

        public void ResizePictureBox(int width, int height)
        {
            pB.Size = new Size(width - 200, height - 50);            
        }

        public void MousePostition(int mouseX, int mouseY, out double XM, out double YM)
        {            
            POINTc bodc = new POINTc();
            POINT bod = new POINT();            
            bodc.x = mouseX;
            bodc.y = mouseY;
            bodc.cislo = "";
            bod = zobrazeni_zpet(bodc);
            XM = Math.Round(bod.x, 3);
            YM = Math.Round(bod.y, 3);                        
        }

        // ---------------------------------------------------------------------------
        // Práce s daty
        //----------------------------------------------------------------------------


        public void AddPoint(string cislo, double y, double x)
        {
            POINT point = new POINT();
            point.cislo = cislo;
            point.x = x;
            point.y = y;
            points.Add(point);
        }

        public void AddPoint(string layer, string cislo, double y, double x)
        {            
            POINT point = new POINT();
            point.cislo = cislo;
            point.x = x;
            point.y = y;
            point.layer = layer;
            points.Add(point);
        }

        public void ClearAllPoints()
        {
            points.Clear();
        }
        
        public void AddLine(string startPoint, string endPoint)
        {
            LINE line = new LINE();
            line.startPoint = startPoint;
            line.endPoint = endPoint;
            lines.Add(line);
        }

        public void AddLine(string layer, string startPoint, string endPoint)
        {            
            LINE line = new LINE();
            line.startPoint = startPoint;
            line.endPoint = endPoint;
            line.layer = layer;
            lines.Add(line);
        }

        public void ClearAllLines()
        {
            lines.Clear();
        }

        public void ClearAll()
        {
            ClearAllLines();
            ClearAllLines();
        }



        // ---------------------------------------------------------------------------
        // Vykreslení dat
        //----------------------------------------------------------------------------
        
        private Color GetColorByLayer(string layerName)
        {
            Color color = new Color();
            foreach (LAYER layer in layers)
            {
                if (layer.name == layerName)
                {
                    color = layer.color;
                    return color;
                }               
            }
            color = Color.Green;
            return color;
        }

        private Font GetFontByLayer(string layerName)
        {
            Font font;
            foreach (LAYER layer in layers)
            {
                if (layer.name == layerName)
                {
                    font = layer.font;
                    return font;
                }
            }
            font = new Font("Arial", 8);
            return font;
        }


        public void DrawLines(Pen pero)
        {                                            
            POINT startPoint, endPoint;
            POINTc startPointc, endPointc;
            
            foreach (LINE line in lines)
            {
                startPoint = FindPoint(line.startPoint);
                endPoint = FindPoint(line.endPoint);
                startPointc = zobrazeni(startPoint);
                endPointc = zobrazeni(endPoint);
                g.DrawLine(pero,startPointc.x, startPointc.y, endPointc.x, endPointc.y);
            }

            pB.Image = bmp;            
        }

        public void DrawLines()
        {            
            POINT startPoint, endPoint;
            POINTc startPointc, endPointc;

            foreach (LINE line in lines)
            {
                Pen pero1 = new Pen(GetColorByLayer(line.layer), 1);
                startPoint = FindPoint(line.startPoint);
                endPoint = FindPoint(line.endPoint);
                startPointc = zobrazeni(startPoint);
                endPointc = zobrazeni(endPoint);
                g.DrawLine(pero1, startPointc.x, startPointc.y, endPointc.x, endPointc.y);
            }

            pB.Image = bmp;
        }

        public void DrawLines(string layerName)
        {
            POINT startPoint, endPoint;
            POINTc startPointc, endPointc;

            foreach (LINE line in lines)
            {
                if (line.layer == layerName)
                {
                    Pen pero1 = new Pen(GetColorByLayer(line.layer), 1);
                    startPoint = FindPoint(line.startPoint);
                    endPoint = FindPoint(line.endPoint);
                    startPointc = zobrazeni(startPoint);
                    endPointc = zobrazeni(endPoint);
                    g.DrawLine(pero1, startPointc.x, startPointc.y, endPointc.x, endPointc.y);
                }
            }

            pB.Image = bmp;
        }


        public void DrawNumbers(Brush brush, Font font)
        {
            POINTc pointc;
            foreach (POINT point in points)
            {
                pointc = zobrazeni(point);
                SolidBrush brush1 = new SolidBrush(GetColorByLayer(point.layer));
                g.DrawString(point.cislo.ToString(), font, brush1, pointc.x, pointc.y);                
            }

            pB.Image = bmp;
        }

        public void DrawNumbers()
        {
            POINTc pointc;
            foreach (POINT point in points)
            {
                pointc = zobrazeni(point);
                SolidBrush brush1 = new SolidBrush(GetColorByLayer(point.layer));
                Font font1 = GetFontByLayer(point.layer);
                g.DrawString(point.cislo.ToString(), font1, brush1, pointc.x, pointc.y);
            }

            pB.Image = bmp;
        }


        public void DrawPoint(Pen pero)
        {
            POINTc pointc;
            foreach (POINT point in points)
            {
                pointc = zobrazeni(point);                
                g.DrawEllipse(pero, pointc.x - 2, pointc.y - 2, 4, 4);                
            }

            pB.Image = bmp;
        }

        public void DrawPoint()
        {
            POINTc pointc;
            foreach (POINT point in points)
            {
                pointc = zobrazeni(point);
                Pen pero1 = new Pen(GetColorByLayer(point.layer), 1);
                g.DrawEllipse(pero1, pointc.x - 2, pointc.y - 2, 4, 4);
            }

            pB.Image = bmp;
        }

        public void DrawAll()
        {
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            DrawNumbers(blackBrush, FontArial);
            DrawLines(peroBlue);
            DrawPoint(peroRed);
        }

        public void DrawAll(bool layerOK)
        {
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            DrawNumbers();
            DrawLines();
            DrawPoint();
        }



        /// <summary>
        /// Import triangles from DTM
        /// </summary>
        /// <param name="file">Input file *.dtm</param>
        public void ImportDTM(string file)
        {
            string pointsName = "points";
            string breaklineName = "breaklines";
            string triangleName = "triangles";
            Font drawFont8 = new Font("Arial", 8);
            LAYER layer = new LAYER();
            layer.name = pointsName;
            layer.font = drawFont8;
            layer.color = Color.Black;
            layers.Add(layer);
            layer = new LAYER();
            layer.name = breaklineName;
            layer.font = drawFont8;
            layer.color = Color.Blue;
            layers.Add(layer);
            layer = new LAYER();
            layer.name = triangleName;
            layer.font = drawFont8;
            layer.color = Color.Red;
            layers.Add(layer);

            string radek;
            string[] parts;
            
            StreamReader sR = new StreamReader(file);
            int count = Convert.ToInt32(sR.ReadLine());
            for (int i = 0; i < count; i++)
            {
                radek = sR.ReadLine();
                parts = radek.Split(' ');
                AddPoint(pointsName, parts[0], Convert.ToDouble(parts[1]), Convert.ToDouble(parts[2]));                
            }
            count = Convert.ToInt32(sR.ReadLine());
            for (int i = 0; i < count; i++)
            {
                radek = sR.ReadLine();
                parts = radek.Split(' ');
                AddLine(breaklineName, parts[0], parts[1]);
            }
            count = Convert.ToInt32(sR.ReadLine());
            for (int i = 0; i < count; i++)
            {
                radek = sR.ReadLine();
                parts = radek.Split(' ');
                AddLine(triangleName, parts[0], parts[1]);
                AddLine(triangleName, parts[1], parts[2]);
                AddLine(triangleName, parts[2], parts[0]);
            }
        }


        public void ExportLines(string file)
        {
            StreamWriter sW = new StreamWriter(file);
            foreach (LINE line in lines)
            {
                sW.WriteLine(line.startPoint + " " + line.endPoint);
            }
            sW.Close();
        }

    }
}

