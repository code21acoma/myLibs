using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Axis
{
    public class AxisParameter
    {
        public double station;  // staničení
        public double y; // y východzí
        public double x; // x výchozí
        public double bearing; // smernik
        public int type; // typ parametru (0=přímka, 1=oblouk, 2=klotoida, 9=konec)
        public double length; // délka parametru
        public double radius1; // poloměr1 -> přímka = 0
        public double radius2; // poloměr2 (pro přechodnici) -> přímka = 0, oblouk = 0 nebo = radius1
        public double yEnd; // y koncového bodu
        public double xEnd; // x koncového bodu
    }

    public class Axis
    {
        AxisParameter axisParameter;
        List<AxisParameter> axis = new List<AxisParameter>();

        AnalyticGeometry.AnalyticGeometry AG = new AnalyticGeometry.AnalyticGeometry();

        static string RemoveSpaces(string value)
        {
            return Regex.Replace(value, @"\s+", " ").Trim();
        }

        private void ClearAxis(List<AxisParameter> axis)
        {
            axis.Clear();
        }

        public void LoadAxisFromFile(string file)
        {
            ClearAxis(axis);

            string[] prvky;
            string radek;

            StreamReader stream = new StreamReader(file);
            stream.ReadLine();

            while ((radek = stream.ReadLine()) != null)
            {
                axisParameter = new AxisParameter();
                radek = RemoveSpaces(radek);
                prvky = radek.Split(' ');

                axisParameter.station = Convert.ToDouble(prvky[0]);
                axisParameter.y = Convert.ToDouble(prvky[1]);
                axisParameter.x = Convert.ToDouble(prvky[2]);
                axisParameter.bearing = Convert.ToDouble(prvky[3]);
                axisParameter.type = Convert.ToInt32(prvky[4]);
                axisParameter.length = Convert.ToDouble(prvky[5]);
                axisParameter.radius1 = Convert.ToDouble(prvky[6]);
                axisParameter.radius2 = Convert.ToDouble(prvky[7]);
                axisParameter.yEnd = 0;
                axisParameter.xEnd = 0;

                if (axisParameter.type == 0)
                {
                    axisParameter.yEnd = axisParameter.y + axisParameter.length * Math.Sin(axisParameter.bearing * Math.PI / 200);
                    axisParameter.xEnd = axisParameter.x + axisParameter.length * Math.Cos(axisParameter.bearing * Math.PI / 200);
                }

                axis.Add(axisParameter);

                if (axisParameter.type == 9) break;
            }
        }

        public int AxisCount()
        {
            return axis.Count;
        }

        public string GetAxisParameters(int i)
        {
            string s = axis[i].station.ToString("0.000") + " " + axis[i].y.ToString("0.000") + " " + axis[i].x.ToString("0.000") + " " + axis[i].bearing.ToString("0.0000") + " " + axis[i].type.ToString() + " " + axis[i].length.ToString("0.000") + " " + axis[i].radius1.ToString("0.000") + " " + axis[i].radius2.ToString("0.000"); ;
            return s;
        }

        public Tuple<double, double, double, double, int, double, double, double> getAxisParameter(int i)
        {
            return new Tuple<double, double, double, double, int, double, double, double>(axis[i].station, axis[i].y, axis[i].x, axis[i].bearing, axis[i].type, axis[i].length, axis[i].radius1, axis[i].radius2);
        }

        private Tuple<int, double, double, double, double[]> CalcAxisCoordinates(double[] point, List<AxisParameter> axis)
        {
            StreamWriter stream = new StreamWriter("log.txt");
            Clothoid.Clothoid clot = new Clothoid.Clothoid();

            int validParameter = -99;
            double[] startPoint = new double[3];
            double[] endPoint = new double[3];
            double[] vector1 = new double[3];
            double[] vector2 = new double[3];

            double[] tangentVector = new double[3];
            double[] normalVector = new double[3];
            double[] binormalVector = new double[3];

            double[] startTangentVector = new double[3];
            double[] startNormalVector = new double[3];
            double[] startBinormalVector = new double[3];
            double[] endTangentVector = new double[3];
            double[] endNormalVector = new double[3];
            double[] endBinormalVector = new double[3];

            double[] normalPoint = new double[3];
            double[] binormalPoint = new double[3];

            double[] tangentPoint = new double[3];
            double[] centripetalVector = new double[3];
            double[] startPointVector = new double[3];
            double[] endPointVector = new double[3];
            double[] centerPoint = new double[3];
            double[] startVector = new double[3];
            double[] pointVector = new double[3];
            double[] endVector = new double[3];
            double startBearing, endBearing, pointBearing;
            double[,] lineParametric1 = new double[3, 2];
            double[,] lineParametric2 = new double[3, 2];
            double[] planeStart = new double[4];
            double[] planeEnd = new double[4];
            double[] intersectionPoint = new double[3];
            double pointAngle;
            double parameterAngle;
            double parameterLength;
            double pointStation;
            double station = 0;
            double offset = 0;
            double bearing = -9999;

            double R, R1, R2;            
            double L, A, tau, tauK;
            double l = 0;
            double Lk;
            double diff;
            double maxDistance = 0.0001;
            double distance = 1000;
            int neg = 1;
            double tangentK;
            double[] tangentKvector;
            double[] normalKvector;
            double xk, yk;
            double[] pointK;
            double[] vectorK = new double[3];
            double[] vectorKx;
            double[] vectorKy;
            double[,] normalKline;

            double minBearing = Double.NaN;
            double minOffset = 999999;
            double minStation = Double.NaN;
            double[] minIntersectionPoint = new double[3]; 


            // HLEDÁNÍ INTERVALU

            for (int i = 0; i < axis.Count - 1; i++)
            {
                if (axis[i].type == 0)   // pro přímku
                {
                    startPoint[0] = axis[i].y;
                    startPoint[1] = axis[i].x;
                    startPoint[2] = 0;
                    endPoint[0] = axis[i].yEnd;
                    endPoint[1] = axis[i].xEnd;
                    endPoint[2] = 0;
                    
                    // Řešení ve 2D
                    
                    vector1 = AG.GetVector(startPoint, endPoint); // tečný vektor
                    vector2 = new double[] { vector1[1], -vector1[0], vector1[2] }; // normálový vektor
                    lineParametric1 = AG.GetLineParametricEquationFromPointAndVector(startPoint, vector1);
                    lineParametric2 = AG.GetLineParametricEquationFromPointAndVector(point, vector2);
                    intersectionPoint = AG.GetLinesIntersectionPoint(lineParametric1, lineParametric2); // průmět bodu na osu

                    parameterLength = AG.GetVectorLength(startPoint, endPoint);       // délka parametru
                    pointStation = AG.GetVectorLength(startPoint, intersectionPoint); // vzdálenost promítnutého bodu od počátačního bodu parametru
                    pointVector = AG.GetVector(startPoint, intersectionPoint);   
                    

                    stream.WriteLine("");
                    stream.WriteLine("Parametr osy: " + i);
                    stream.WriteLine("vector1: " + vector1[0] + " " + vector1[1] + " " + vector1[2]);
                    stream.WriteLine("vector2: " + vector2[0] + " " + vector2[1] + " " + vector2[2]);
                    stream.WriteLine("lineParametric1: " + lineParametric1[0, 0] + " " + lineParametric1[0, 1]);
                    stream.WriteLine("---------------: " + lineParametric1[1, 0] + " " + lineParametric1[1, 1]);
                    stream.WriteLine("---------------: " + lineParametric1[2, 0] + " " + lineParametric1[2, 1]);
                    stream.WriteLine("lineParametric2: " + lineParametric2[0, 0] + " " + lineParametric2[0, 1]);
                    stream.WriteLine("---------------: " + lineParametric2[1, 0] + " " + lineParametric2[1, 1]);
                    stream.WriteLine("---------------: " + lineParametric2[2, 0] + " " + lineParametric2[2, 1]);
                    stream.WriteLine("Vector bearing: " + AG.GetVectorBearing(vector1));
                    stream.WriteLine("---------------: " );

                    stream.WriteLine("StartPoint: " + startPoint[0] + " " + startPoint[1] + " " + startPoint[2]);
                    stream.WriteLine("EndPoint: " + endPoint[0] + " " + endPoint[1] + " " + endPoint[2]);
                    stream.WriteLine("IntersectionPoint: " + intersectionPoint[0] + " " + intersectionPoint[1] + " " + intersectionPoint[2]);

                    stream.WriteLine("Délka úseku: " + parameterLength);
                    stream.WriteLine("Staničení bodu v rámci parametru: " + pointStation);

                    if ((parameterLength >= pointStation) && (AG.GetVectorAngle(vector1, pointVector) < 1))   // pokud je délka parametru větší než staničení promítnustého bodu && oba vektory mají stejný směr
                    {
                        validParameter = i;
                        station = axis[i].station + pointStation;
                        offset = AG.GetVectorLength(point, intersectionPoint);
                        bearing = axis[i].bearing;
                        double p = AG.GetPositionOnLine(startPoint, endPoint, point);
                        if (p > 0) offset = -offset;
                        stream.WriteLine("Splněno");
                        if (offset < minOffset)
                        { 
                            minBearing = bearing;
                            minOffset = offset;
                            minStation = station;
                            minIntersectionPoint = intersectionPoint;
                        }
                        continue;
                    }
                    else stream.WriteLine("Nesplněno");
                    
                }


                if (axis[i].type == 1)   // pro oblouk
                {
                    startPoint[0] = axis[i].y;
                    startPoint[1] = axis[i].x;
                    startPoint[2] = 0;
                    endPoint[0] = axis[i+1].y;
                    endPoint[1] = axis[i+1].x;
                    endPoint[2] = 0;

                    tangentPoint = AG.GetRajon(axis[i].bearing * Math.PI / 200, Math.Abs(axis[i].radius1));
                    tangentPoint[0] = axis[i].y + tangentPoint[0];
                    tangentPoint[1] = axis[i].x + tangentPoint[1];
                    tangentPoint[2] = 0;
                    tangentVector = AG.GetVector(startPoint, tangentPoint);
                    if (axis[i].radius1 > 0) // oblouk doprava
                    {
                        centripetalVector[0] = tangentVector[1];
                        centripetalVector[1] = -tangentVector[0];
                        centripetalVector[2] = 0;
                    }
                    else  // oblouk doleva
                    {
                        centripetalVector[0] = -tangentVector[1];
                        centripetalVector[1] = tangentVector[0];
                        centripetalVector[2] = 0;
                    }

                    centerPoint = AG.GetRajon(startPoint, centripetalVector);
                    //startVector = new double[] { -centripetalVector[0], -centripetalVector[1], 0 };

                    pointVector = AG.GetVector(centerPoint, point);
                    pointBearing = AG.GetVectorBearing(pointVector);
                    startPointVector = AG.GetVector(centerPoint, startPoint);
                    startBearing = AG.GetVectorBearing(startPointVector);
                    endPointVector = AG.GetVector(centerPoint, endPoint);
                    endBearing = AG.GetVectorBearing(endPointVector);
                    //pointAngle = AG.GetVectorAngle(startVector, pointVector);

                    stream.WriteLine("");
                    stream.WriteLine("Parametr osy: " + i);
                    stream.WriteLine("CenterPoint: " + centerPoint[0] + " " + centerPoint[1]);
                    stream.WriteLine("StartPointVector: " + startPointVector[0] + " " + startPointVector[1]);
                    stream.WriteLine("endPointVector: " + endPointVector[0] + " " + endPointVector[1]);
                    stream.WriteLine("pointVector: " + pointVector[0] + " " + pointVector[1]);

                    stream.WriteLine("startBearing: " + startBearing);
                    stream.WriteLine("endBearing: " + endBearing);
                    stream.WriteLine("pointBearing: " + pointBearing);

                    if (axis[i].radius1 > 0)
                    {
                        if (endBearing < startBearing) endBearing = endBearing + 400;
                        if (pointBearing < startBearing) pointBearing = pointBearing + 400;

                        if ((pointBearing >= startBearing) && (pointBearing <= endBearing))
                        {
                            validParameter = i;
                            pointStation = ((pointBearing - startBearing) * Math.PI * axis[i].radius1) / 200;
                            //stream.WriteLine("pointStation: " + pointStation);
                            station = axis[i].station + pointStation;
                            offset = axis[i].radius1 - AG.GetVectorLength(pointVector);
                            intersectionPoint = AG.GetRajon(centerPoint, pointVector);
                            bearing = axis[i].bearing + (pointBearing - startBearing);
                            stream.WriteLine("bearing: " + bearing);
                            while (bearing < 0) bearing = bearing + 400;
                            while (bearing > 400) bearing = bearing - 400;
                            stream.WriteLine("bearing: " + bearing);
                            stream.WriteLine("startBearing: " + startBearing);
                            stream.WriteLine("endBearing: " + endBearing);
                            stream.WriteLine("pointBearing: " + pointBearing);
                            stream.WriteLine("Splněno");
                            if (offset < minOffset)
                            {
                                minBearing = bearing;
                                minOffset = offset;
                                minStation = station;
                                minIntersectionPoint = intersectionPoint;
                            }
                            continue;
                        }
                        else stream.WriteLine("Nesplněno");
                    }
                    else
                    {
                        if (startBearing < endBearing) startBearing = startBearing + 400;
                        if (pointBearing < endBearing) pointBearing = pointBearing + 400;

                        if ((pointBearing >= endBearing) && (pointBearing <= startBearing))
                        {
                            validParameter = i;
                            pointStation = ((startBearing - pointBearing) * Math.PI * Math.Abs(axis[i].radius1)) / 200;
                            stream.WriteLine("pointStation: " + pointStation);
                            station = axis[i].station + pointStation;
                            offset = AG.GetVectorLength(pointVector) - Math.Abs(axis[i].radius1);
                            intersectionPoint = AG.GetRajon(centerPoint, pointVector);
                            bearing = axis[i].bearing - (startBearing - pointBearing);
                            stream.WriteLine("axis-bearing: " + axis[i].bearing);
                            stream.WriteLine("startBearing: " + startBearing);
                            stream.WriteLine("pointBearing: " + pointBearing);
                            stream.WriteLine("bearing: " + bearing);
                            while (bearing < 0) bearing = bearing + 400;
                            while (bearing > 400) bearing = bearing - 400;
                            stream.WriteLine("bearing: " + bearing);
                            stream.WriteLine("startBearing: " + startBearing);
                            stream.WriteLine("endBearing: " + endBearing);
                            stream.WriteLine("pointBearing: " + pointBearing);
                            stream.WriteLine("Splněno");
                            if (offset < minOffset)
                            {
                                minBearing = bearing;
                                minOffset = offset;
                                minStation = station;
                                minIntersectionPoint = intersectionPoint;
                            }
                            continue;
                        }
                        else stream.WriteLine("Nesplněno");
                    }
                    
                }

                if (axis[i].type == 2)   // přechodnice klotoida
                {
                    startPoint[0] = axis[i].y;
                    startPoint[1] = axis[i].x;
                    startPoint[2] = 0;
                    endPoint[0] = axis[i + 1].y;
                    endPoint[1] = axis[i + 1].x;
                    endPoint[2] = 0;

                    // Kontrola zda je bod leží na přechodnici - musí ležet za normálou ke směrovému vektoru počátačního směrníku a před normálou ke směrovému vektoru konečného směrníku
                    double[] tangentVector1 = AG.GetVector(axis[i].bearing, axis[i].length);
                    double[] tangentVector2 = AG.GetVector(axis[i + 1].bearing, axis[i].length);
                    double[] normalVector1 = AG.GetRightVector(tangentVector1);
                    double[] normalVector2 = AG.GetRightVector(tangentVector2);
                    double[] normalPoint1 = AG.GetRajon(startPoint, normalVector1);
                    double[] normalPoint2 = AG.GetRajon(endPoint, normalVector2);
                    double position1 = AG.GetPositionOnLine(startPoint, normalPoint1, point);
                    double position2 = AG.GetPositionOnLine(endPoint, normalPoint2, point);
                    stream.WriteLine("");
                    stream.WriteLine("Parametr osy: " + i);
                    stream.WriteLine("tangentVector1: " + tangentVector1[0] + " " + tangentVector1[1]);
                    stream.WriteLine("tangentVector2: " + tangentVector2[0] + " " + tangentVector2[1]);
                    stream.WriteLine("normalVector1: " + normalVector1[0] + " " + normalVector1[1]);
                    stream.WriteLine("normalVector2: " + normalVector2[0] + " " + normalVector2[1]);
                    stream.WriteLine("position1: " + position1);
                    stream.WriteLine("position2: " + position2);
                    if ((position1 >= 0) && (position2 <= 0))
                    {
                        // bod je na klotoide
                        stream.WriteLine("bod je na klotoide");
                    }
                    else
                    {
                        continue;
                    }

                    tangentPoint = AG.GetRajon(axis[i].bearing * Math.PI / 200, axis[i].length);
                    tangentPoint[0] = axis[i].y + tangentPoint[0];
                    tangentPoint[1] = axis[i].x + tangentPoint[1];
                    tangentPoint[2] = 0;

                    R1 = axis[i].radius1;
                    R2 = axis[i].radius2;                                        
                    Lk = Math.Abs(axis[i].length);
                    diff = 1000;
                    maxDistance = 0.00001;
                    distance = 1000;
                    neg = 1;                    

                    tangentPoint = AG.GetRajon(axis[i].bearing * Math.PI / 200, Lk);
                    tangentPoint[0] = axis[i].y + tangentPoint[0];
                    tangentPoint[1] = axis[i].x + tangentPoint[1];
                    tangentPoint[2] = 0;
                    tangentVector = AG.GetVector(startPoint, tangentPoint);



                    if ((R1 == 0) && (R2 != 0)) // z přímé do oblouku
                    {
                        stream.WriteLine("***************************************");
                        stream.WriteLine("****** Z PRIME DO OBLOUKU *************");
                        stream.WriteLine("***************************************");
                        A = Math.Sqrt(axis[i].length * Math.Abs(R2));
                        stream.WriteLine("A: " + A);
                        //tauK = clot.CalcTau(Lk, Lk, R2);

                        double[] tetivaVektor = AG.GetVector(startPoint, endPoint);
                        double[] normalaKtetiveVektor = AG.GetRightVector(tetivaVektor);
                        double[,] tetiva = AG.GetLineParametricEquationFromPointAndVector(startPoint, tetivaVektor);
                        double[,] normalaKtetive = AG.GetLineParametricEquationFromPointAndVector(point, normalaKtetiveVektor);
                        double[] tetivaPoint = AG.GetLinesIntersectionPoint(tetiva, normalaKtetive);
                        l = AG.GetVectorLength(startPoint, tetivaPoint); // přibližná délka přechodnice = délka tětivy
                        R = clot.CalcR(A, l);
                        tau = clot.CalcTau(l, Lk, R2) * 200 / Math.PI;
                        double[] yxk = clot.CalcXY(l, Lk, R2);
                        vectorKx = AG.GetRajon(axis[i].bearing * Math.PI / 200, yxk[1]);
                        vectorKy = AG.GetRajon((axis[i].bearing + 100) * Math.PI / 200, yxk[0]); // plus = doprava
                        vectorK = AG.GetSumVector(vectorKx, vectorKy);
                        pointK = AG.GetRajon(startPoint, vectorK);
                        tangentK = axis[i].bearing + tau; // plus je to u oblouku doprava
                        tangentKvector = AG.GetVector(tangentK, axis[i].length);
                        normalKvector = AG.GetRightVector(tangentKvector);
                        normalKline = AG.GetLineParametricEquationFromPointAndVector(pointK, normalKvector);
                        diff = AG.GetPositionOnLineWithVector(pointK, normalKvector, point);
                        distance = AG.GetDistancePointFromLine(point, normalKline);
                        stream.WriteLine("diff: " + diff);
                        stream.WriteLine("distanc: " + distance);
                        int j = 0;

                        while ((Math.Abs(distance) > maxDistance) || (j == 0))
                        {
                            j++;
                            stream.WriteLine("------------------------");
                            stream.WriteLine("------------------------");
                            stream.WriteLine("Cyklus..." + j);
                            if (diff >= 0) neg = 1;
                            else neg = -1;
                            l = l + (neg * distance); // přibližná délka přechodnice - iterační přiblížení
                            R = clot.CalcR(A, l);
                            tau = clot.CalcTau(l, Lk, R2) * 200 / Math.PI;
                            yxk = clot.CalcXY(l, Lk, R2);
                            vectorKx = AG.GetRajon(axis[i].bearing * Math.PI / 200, yxk[1]);
                            vectorKy = AG.GetRajon((axis[i].bearing + 100) * Math.PI / 200, yxk[0]); // plus = doprava
                            vectorK = AG.GetSumVector(vectorKx, vectorKy);
                            pointK = AG.GetRajon(startPoint, vectorK);
                            tangentK = axis[i].bearing + tau; // plus je to u oblouku doprava
                            tangentKvector = AG.GetVector(tangentK, axis[i].length);
                            normalKvector = AG.GetRightVector(tangentKvector);
                            normalKline = AG.GetLineParametricEquationFromPointAndVector(pointK, normalKvector);
                            diff = AG.GetPositionOnLineWithVector(pointK, normalKvector, point);
                            distance = AG.GetDistancePointFromLine(point, normalKline);
                            stream.WriteLine("diff: " + diff);
                            stream.WriteLine("distanc: " + distance);

                            validParameter = i;
                            station = axis[i].station + l;
                            double pos = AG.GetPositionOnLineWithVector(pointK, tangentKvector, point);
                            if (pos <= 0) neg = 1;
                            else neg = -1;
                            offset = neg * AG.GetVectorLength(pointK, point);
                            intersectionPoint = pointK;
                            bearing = tangentK;

                            stream.WriteLine("pos: " + pos);
                            stream.WriteLine("------------------------");
                            stream.WriteLine("station: " + station);
                            stream.WriteLine("offset: " + offset);
                            stream.WriteLine("bearing: " + bearing);

                            if (Math.Abs(offset) < minOffset)
                            {
                                minBearing = bearing;
                                minOffset = offset;
                                minStation = station;
                                minIntersectionPoint = intersectionPoint;
                            }

                            if (j > 20) break;
                        }

                    }

                    if ((R2 == 0) && (R1 != 0)) // z oblouku do přímé
                    {
                        stream.WriteLine("***************************************");
                        stream.WriteLine("****** Z OBLOUKU DO PRIME *************");
                        stream.WriteLine("***************************************");
                        R1 = -R1;   // klotoida jde z opačné strany, musí mít opačné znaménko, stáčí se opačným směrem
                        A = Math.Sqrt(axis[i].length * Math.Abs(R1));
                        stream.WriteLine("A: " + A);
                        double[] pomPoint = startPoint;
                        startPoint = endPoint;
                        endPoint = pomPoint;
                        
                        double[] tetivaVektor = AG.GetVector(startPoint, endPoint);
                        double[] normalaKtetiveVektor = AG.GetRightVector(tetivaVektor);
                        double[,] tetiva = AG.GetLineParametricEquationFromPointAndVector(startPoint, tetivaVektor);
                        double[,] normalaKtetive = AG.GetLineParametricEquationFromPointAndVector(point, normalaKtetiveVektor);
                        double[] tetivaPoint = AG.GetLinesIntersectionPoint(tetiva, normalaKtetive);
                        l = AG.GetVectorLength(startPoint, tetivaPoint); // přibližná délka přechodnice = délka tětivy
                        //R = clot.CalcR(A, l);
                        tau = clot.CalcTau(l, Lk, R1) * 200 / Math.PI;
                        double[] yxk = clot.CalcXY(l, Lk, R1);
                        bearing = axis[i + 1].bearing + 200;
                        while (bearing < 0) bearing = bearing + 400;
                        while (bearing > 400) bearing = bearing - 400;
                        vectorKx = AG.GetRajon(bearing * Math.PI / 200, yxk[1]);
                        vectorKy = AG.GetRajon((bearing + 100) * Math.PI / 200, yxk[0]); // plus = doprava
                        vectorK = AG.GetSumVector(vectorKx, vectorKy);
                        pointK = AG.GetRajon(startPoint, vectorK);
                        tangentK = bearing + tau; // plus je to u oblouku doprava
                        tangentKvector = AG.GetVector(tangentK, axis[i].length);
                        normalKvector = AG.GetRightVector(tangentKvector);
                        normalKline = AG.GetLineParametricEquationFromPointAndVector(pointK, normalKvector);
                        diff = AG.GetPositionOnLineWithVector(pointK, normalKvector, point);
                        distance = AG.GetDistancePointFromLine(point, normalKline);
                        stream.WriteLine("R1: " + R1);
                        stream.WriteLine("R2: " + R2);
                        stream.WriteLine("startBearing: " + bearing);
                        stream.WriteLine("startPoint: " + startPoint[0] + " " + startPoint[1] + " " + startPoint[2]);
                        stream.WriteLine("endPoint: " + endPoint[0] + " " + endPoint[1] + " " + endPoint[2]);
                        stream.WriteLine("diff: " + diff);                        
                        stream.WriteLine("distanc: " + distance);
                        stream.WriteLine("l: " + l);
                        stream.WriteLine("tau: " + tau);
                        int j = 0;

                        while ((Math.Abs(distance) > maxDistance) || (j == 0))
                        {
                            j++;
                            stream.WriteLine("------------------------");
                            stream.WriteLine("------------------------");
                            stream.WriteLine("Cyklus..." + j);
                            if (diff >= 0) neg = 1;
                            else neg = -1;
                            l = l + (neg * distance); // přibližná délka přechodnice - iterační přiblížení
                            stream.WriteLine("l: " + l);
                            //R = clot.CalcR(A, l);
                            tau = clot.CalcTau(l, Lk, R1) * 200 / Math.PI;
                            stream.WriteLine("tau: " + tau);
                            yxk = clot.CalcXY(l, Lk, R1);
                            vectorKx = AG.GetRajon(bearing * Math.PI / 200, yxk[1]);
                            vectorKy = AG.GetRajon((bearing + 100) * Math.PI / 200, yxk[0]); // plus = doprava
                            vectorK = AG.GetSumVector(vectorKx, vectorKy);
                            pointK = AG.GetRajon(startPoint, vectorK);
                            tangentK = bearing + tau; // plus je to u oblouku doprava
                            stream.WriteLine("tangentK: " + tangentK);
                            tangentKvector = AG.GetVector(tangentK, axis[i].length);
                            normalKvector = AG.GetRightVector(tangentKvector);
                            normalKline = AG.GetLineParametricEquationFromPointAndVector(pointK, normalKvector);
                            diff = AG.GetPositionOnLineWithVector(pointK, normalKvector, point);
                            distance = AG.GetDistancePointFromLine(point, normalKline);
                            stream.WriteLine("diff: " + diff);
                            stream.WriteLine("distanc: " + distance);

                            validParameter = i;
                            station = axis[i].station + Lk - l;  // zmena pro opacnz prechod
                            double pos = AG.GetPositionOnLineWithVector(pointK, tangentKvector, point);
                            if (pos <= 0) neg = 1;
                            else neg = -1;
                            offset = -neg * AG.GetVectorLength(pointK, point);
                            intersectionPoint = pointK;
                            tangentK = tangentK + 200;
                            while (tangentK < 0) tangentK = tangentK + 400;
                            while (tangentK > 400) tangentK = tangentK - 400;
                            stream.WriteLine("pos: " + pos);
                            stream.WriteLine("------------------------");
                            stream.WriteLine("station: " + station);
                            stream.WriteLine("offset: " + offset);
                            stream.WriteLine("startBearing: " + bearing);
                            stream.WriteLine("bearing: " + tangentK);

                            if (Math.Abs(offset) < minOffset)
                            {                                
                                minBearing = tangentK;
                                minOffset = offset;
                                minStation = station;
                                minIntersectionPoint = intersectionPoint;
                            }

                            if (j > 20) break;
                        }

                    }

                    if ((R2 != 0) && (R1 != 0) && (Math.Abs(R1) > Math.Abs(R2))) // z většího poloměru oblouku na menší poloměr oblouk
                    {
                        stream.WriteLine("***************************************");
                        stream.WriteLine("* Z VĚTŠÍHO POLOMĚRU OBLOUKU NA MENŠÍ *");
                        stream.WriteLine("***************************************");
                        //A = Math.Sqrt(axis[i].length * Math.Abs(R2));
                        A = Math.Sqrt((Math.Abs(R1) * Math.Abs(R2) * axis[i].length) / Math.Abs(R1 - R2));
                        stream.WriteLine("A: " + A);
                        var result1 = clot.CalcStartPoint(startPoint, endPoint, R1, R2, axis[i].length, axis[i].bearing);
                        double[] startClothoidPoint = result1.Item1;
                        startBearing = result1.Item2;
                        double Lposun = result1.Item3;
                        stream.WriteLine("startClothoidPoint: " + startClothoidPoint[0] + " " + startClothoidPoint[1]);

                        double[] tetivaVektor = AG.GetVector(startClothoidPoint, endPoint);
                        double[] normalaKtetiveVektor = AG.GetRightVector(tetivaVektor);
                        double[,] tetiva = AG.GetLineParametricEquationFromPointAndVector(startClothoidPoint, tetivaVektor);
                        double[,] normalaKtetive = AG.GetLineParametricEquationFromPointAndVector(point, normalaKtetiveVektor);
                        double[] tetivaPoint = AG.GetLinesIntersectionPoint(tetiva, normalaKtetive);
                        l = AG.GetVectorLength(startClothoidPoint, tetivaPoint); // přibližná délka přechodnice = délka tětivy
                        R = clot.CalcR(A, l);
                        Lk = clot.CalcLk(A, Math.Abs(R2));
                        tau = clot.CalcTau(l, Lk, R2) * 200 / Math.PI;
                        double[] yxk = clot.CalcXY(l, Lk, R2);
                        vectorKx = AG.GetRajon(startBearing * Math.PI / 200, yxk[1]);
                        vectorKy = AG.GetRajon((startBearing + 100) * Math.PI / 200, yxk[0]); // plus = doprava
                        vectorK = AG.GetSumVector(vectorKx, vectorKy);
                        pointK = AG.GetRajon(startClothoidPoint, vectorK);
                        tangentK = startBearing + tau; // plus je to u oblouku doprava
                        tangentKvector = AG.GetVector(tangentK, axis[i].length); /// <>>>>
                        normalKvector = AG.GetRightVector(tangentKvector);
                        normalKline = AG.GetLineParametricEquationFromPointAndVector(pointK, normalKvector);
                        diff = AG.GetPositionOnLineWithVector(pointK, normalKvector, point);
                        distance = AG.GetDistancePointFromLine(point, normalKline);
                        stream.WriteLine("diff: " + diff);
                        stream.WriteLine("distanc: " + distance);
                        stream.WriteLine("tau: " + tau);
                        int j = 0;

                        while ((Math.Abs(distance) > maxDistance) || (j == 0))
                        {
                            j++;
                            stream.WriteLine("------------------------");
                            stream.WriteLine("------------------------");
                            stream.WriteLine("Cyklus..." + j);
                            if (diff >= 0) neg = 1;
                            else neg = -1;
                            l = l + (neg * distance); // přibližná délka přechodnice - iterační přiblížení
                            R = clot.CalcR(A, l);
                            tau = clot.CalcTau(l, Lk, R2) * 200 / Math.PI;
                            yxk = clot.CalcXY(l, Lk, R2);
                            vectorKx = AG.GetRajon(startBearing * Math.PI / 200, yxk[1]);
                            vectorKy = AG.GetRajon((startBearing + 100) * Math.PI / 200, yxk[0]); // plus = doprava
                            vectorK = AG.GetSumVector(vectorKx, vectorKy);
                            pointK = AG.GetRajon(startClothoidPoint, vectorK);
                            tangentK = startBearing + tau; // plus je to u oblouku doprava
                            tangentKvector = AG.GetVector(tangentK, axis[i].length);
                            normalKvector = AG.GetRightVector(tangentKvector);
                            normalKline = AG.GetLineParametricEquationFromPointAndVector(pointK, normalKvector);
                            diff = AG.GetPositionOnLineWithVector(pointK, normalKvector, point);
                            distance = AG.GetDistancePointFromLine(point, normalKline);
                            stream.WriteLine("diff: " + diff);
                            stream.WriteLine("distanc: " + distance);
                            stream.WriteLine("l: " + l);

                            validParameter = i;
                            //station = axis[i].station + l;                            
                            station = axis[i].station - Lposun + l;
                            double pos = AG.GetPositionOnLineWithVector(pointK, tangentKvector, point);
                            if (pos <= 0) neg = 1;
                            else neg = -1;
                            offset = neg * AG.GetVectorLength(pointK, point);
                            intersectionPoint = pointK;
                            bearing = tangentK;

                            stream.WriteLine("pos: " + pos);
                            stream.WriteLine("------------------------");
                            stream.WriteLine("station: " + station);
                            stream.WriteLine("offset: " + offset);
                            stream.WriteLine("bearing: " + bearing);

                            if (Math.Abs(offset) < minOffset)
                            {
                                minBearing = bearing;
                                minOffset = offset;
                                minStation = station;
                                minIntersectionPoint = intersectionPoint;
                            }

                            if (j > 20) break;
                        }


                    }


                    if ((R2 != 0) && (R1 != 0) && (Math.Abs(R1) < Math.Abs(R2))) // z menšího poloměru oblouku na větší poloměr oblouk
                    {
                        stream.WriteLine("***************************************");
                        stream.WriteLine("* Z MENŠÍHO POLOMĚRU OBLOUKU NA VĚTŠÍ *");
                        stream.WriteLine("***************************************");
                        R1 = -R1;   // klotoida jde z opačné strany, musí mít opačné znaménko, stáčí se opačným směrem
                        R2 = -R2;
                        //A = Math.Sqrt(axis[i].length * Math.Abs(R1));
                        A = Math.Sqrt((Math.Abs(R1) * Math.Abs(R2) * axis[i].length) / Math.Abs(R1 - R2));
                        stream.WriteLine("A: " + A);
                        double[] pomPoint = startPoint;
                        startPoint = endPoint;
                        endPoint = pomPoint;

                        var result1 = clot.CalcStartPoint(startPoint, endPoint, R2, R1, axis[i].length, axis[i + 1].bearing - 200);
                        double[] startClothoidPoint = result1.Item1;
                        startBearing = result1.Item2;
                        double Lposun = result1.Item3;
                        stream.WriteLine("startClothoidPoint: " + startClothoidPoint[0] + " " + startClothoidPoint[1]);

                        double[] tetivaVektor = AG.GetVector(startClothoidPoint, endPoint);
                        double[] normalaKtetiveVektor = AG.GetRightVector(tetivaVektor);
                        double[,] tetiva = AG.GetLineParametricEquationFromPointAndVector(startClothoidPoint, tetivaVektor);
                        double[,] normalaKtetive = AG.GetLineParametricEquationFromPointAndVector(point, normalaKtetiveVektor);
                        double[] tetivaPoint = AG.GetLinesIntersectionPoint(tetiva, normalaKtetive);
                        l = AG.GetVectorLength(startClothoidPoint, tetivaPoint); // přibližná délka přechodnice = délka tětivy
                        //R = clot.CalcR(A, l);
                        tau = clot.CalcTau(l, Lk + Lposun, R1) * 200 / Math.PI;
                        double[] yxk = clot.CalcXY(l, Lk + Lposun, R1);
                        //bearing = axis[i + 1].bearing + 200;
                        while (startBearing < 0) startBearing = startBearing + 400;
                        while (startBearing > 400) startBearing = startBearing - 400;
                        vectorKx = AG.GetRajon(startBearing * Math.PI / 200, yxk[1]);
                        vectorKy = AG.GetRajon((startBearing + 100) * Math.PI / 200, yxk[0]); // plus = doprava
                        vectorK = AG.GetSumVector(vectorKx, vectorKy);
                        pointK = AG.GetRajon(startClothoidPoint, vectorK);
                        tangentK = startBearing + tau; // plus je to u oblouku doprava
                        tangentKvector = AG.GetVector(tangentK, axis[i].length);
                        normalKvector = AG.GetRightVector(tangentKvector);
                        normalKline = AG.GetLineParametricEquationFromPointAndVector(pointK, normalKvector);
                        diff = AG.GetPositionOnLineWithVector(pointK, normalKvector, point);
                        distance = AG.GetDistancePointFromLine(point, normalKline);
                        stream.WriteLine("R1: " + R1);
                        stream.WriteLine("R2: " + R2);
                        stream.WriteLine("startBearing: " + startBearing);
                        stream.WriteLine("startPoint: " + startPoint[0] + " " + startPoint[1] + " " + startPoint[2]);
                        stream.WriteLine("endPoint: " + endPoint[0] + " " + endPoint[1] + " " + endPoint[2]);
                        stream.WriteLine("diff: " + diff);
                        stream.WriteLine("distanc: " + distance);
                        stream.WriteLine("l: " + l);
                        stream.WriteLine("tau: " + tau);
                        int j = 0;

                        while ((Math.Abs(distance) > maxDistance) || (j == 0))
                        {
                            j++;
                            stream.WriteLine("------------------------");
                            stream.WriteLine("------------------------");
                            stream.WriteLine("Cyklus..." + j);
                            if (diff >= 0) neg = 1;
                            else neg = -1;
                            l = l + (neg * distance); // přibližná délka přechodnice - iterační přiblížení
                            stream.WriteLine("l: " + l);
                            //R = clot.CalcR(A, l);
                            tau = clot.CalcTau(l, Lk + Lposun, R1) * 200 / Math.PI;
                            stream.WriteLine("tau: " + tau);
                            yxk = clot.CalcXY(l, Lk + Lposun, R1);
                            vectorKx = AG.GetRajon(startBearing * Math.PI / 200, yxk[1]);
                            vectorKy = AG.GetRajon((startBearing + 100) * Math.PI / 200, yxk[0]); // plus = doprava
                            vectorK = AG.GetSumVector(vectorKx, vectorKy);
                            pointK = AG.GetRajon(startClothoidPoint, vectorK);
                            tangentK = startBearing + tau; // plus je to u oblouku doprava
                            stream.WriteLine("tangentK: " + tangentK);
                            tangentKvector = AG.GetVector(tangentK, axis[i].length);
                            normalKvector = AG.GetRightVector(tangentKvector);
                            normalKline = AG.GetLineParametricEquationFromPointAndVector(pointK, normalKvector);
                            diff = AG.GetPositionOnLineWithVector(pointK, normalKvector, point);
                            distance = AG.GetDistancePointFromLine(point, normalKline);
                            stream.WriteLine("diff: " + diff);
                            stream.WriteLine("distanc: " + distance);

                            validParameter = i;
                            //station = axis[i].station + Lk - l;  // zmena pro opacnz prechod
                            station = axis[i].station + Lposun + Lk - l;
                            double pos = AG.GetPositionOnLineWithVector(pointK, tangentKvector, point);
                            if (pos <= 0) neg = 1;
                            else neg = -1;
                            offset = -neg * AG.GetVectorLength(pointK, point);
                            intersectionPoint = pointK;
                            tangentK = tangentK + 200;
                            while (tangentK < 0) tangentK = tangentK + 400;
                            while (tangentK > 400) tangentK = tangentK - 400;
                            stream.WriteLine("pos: " + pos);
                            stream.WriteLine("------------------------");
                            stream.WriteLine("station: " + station);
                            stream.WriteLine("offset: " + offset);
                            stream.WriteLine("startBearing: " + startBearing);
                            stream.WriteLine("bearing: " + tangentK);
                            bearing = tangentK;

                            if (Math.Abs(offset) < minOffset)
                            {
                                minBearing = tangentK;
                                minOffset = offset;
                                minStation = station;
                                minIntersectionPoint = intersectionPoint;
                            }

                            if (j > 20) break;
                        }

                    }

                }


            }

          
            stream.Close();
            if (bearing == -9999)
            {
                station = Double.NaN;
                offset = Double.NaN;
                bearing = Double.NaN;
            }

            return new Tuple<int, double, double, double, double[]>(validParameter, station, offset, bearing, intersectionPoint);
        }


        /// <summary>
        /// Vrací: validní úsek, staničení, kolmice, výška nad niveletou a průmět bodu na trasu
        /// </summary>
        /// <param name="point">Bod k transformaci na trasu</param>
        /// <returns></returns>
        public Tuple<int, double, double, double, double[]> GetAxisCoordinates(double[] point)
        {
            var result = CalcAxisCoordinates(point, axis);
            return new Tuple<int, double, double, double, double[]>(result.Item1, result.Item2, result.Item3, result.Item4, result.Item5);
        }

    }
}
