using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
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

            DrawAll();
        }

        /// <summary>
        /// Posun obrazu nahoru
        /// </summary>
        public void PosunNahoru(double par)
        {            
            centerX = centerX - par;
            DrawAll();
        }

        /// <summary>
        /// Posun obrazu dolů
        /// </summary>
        public void PosunDolu(double par)
        {            
            centerX = centerX + par;
            DrawAll();
        }

        /// <summary>
        /// Posun obrazu doleva
        /// </summary>
        public void PosunDoleva(double par)
        {            
            centerY = centerY + par;
            DrawAll();
        }

        /// <summary>
        /// Posun obrazu doprava
        /// </summary>
        public void PosunDoprava(double par)
        {            
            centerY = centerY - par;
            DrawAll();
        }


        /// <summary>
        /// Zmenšení obrazu
        /// </summary>
        public void ZmensiObraz(double par)
        {
            double pom;            
            pom = Convert.ToDouble(MERITKO) / (1 + (2 * Convert.ToDouble(par)));
            MERITKO = Convert.ToInt32(pom);
            DrawAll();
        }

        /// <summary>
        /// Zvětšení obrazu
        /// </summary>
        public void ZvetsiObraz(double par)
        {
            double pom;            
            pom = Convert.ToDouble(MERITKO) * (1 + (2 * Convert.ToDouble(par)));
            MERITKO = Convert.ToInt32(pom);
            DrawAll();
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
            // doplnit vrstvu - 
            POINT point = new POINT();
            point.cislo = cislo;
            point.x = x;
            point.y = y;
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
            // doplnit vrstvu
            LINE line = new LINE();
            line.startPoint = startPoint;
            line.endPoint = endPoint;
            lines.Add(line);
        }

        public void ClearAllLines()
        {
            lines.Clear();
        }



        // ---------------------------------------------------------------------------
        // Vykreslení dat
        //----------------------------------------------------------------------------
        
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


        public void DrawNumbers(Brush brush, Font font)
        {
            POINTc pointc;
            foreach (POINT point in points)
            {
                pointc = zobrazeni(point);
                g.DrawString(point.cislo.ToString(), font, brush, pointc.x, pointc.y);                
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

        public void DrawAll()
        {
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            DrawNumbers(blackBrush, FontArial);
            DrawLines(peroBlue);
            DrawPoint(peroRed);
        }
        
        public void ImportDTM(List<string> pointsX, List<string> lines, List<string> triangles)
        {
            // Import triangles from DTM
        }

    }
}

