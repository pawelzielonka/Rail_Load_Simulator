using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace Vamos21
{
    public enum ExcelType { Vehicles, PID, Profile }
    class Operations
    {
        //zapis do pliku xml
        public static void SerializeToXML(object obj, string path)
        {
            StreamWriter wr = new StreamWriter(path + ".xml"); //To służy do zapisu danych
            XmlSerializer serializer = new XmlSerializer(obj.GetType()); //To będzie je formatowało :)
            serializer.Serialize(wr, obj); //Serializujemy
            wr.Flush();
            wr.Close(); //Sprzątamy
        }
        //Odczyt z pliku xml
        public static ProjectData ReadFromXML(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ProjectData));
            StreamReader reader = new StreamReader(path);
            ProjectData obj = new ProjectData();
            obj = (ProjectData)serializer.Deserialize(reader);
            // close the stream            
            reader.Close();
            return obj;
        }
        public static string ReadStringFromXML(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(string));
            StreamReader reader = new StreamReader(path);
            string obj;
            obj = (string)serializer.Deserialize(reader);
            // close the stream            
            reader.Close();
            return obj;
        }
        //odczyt pliku excelowego
        public static bool LoadExcelFile(string path, ExcelType type, ref List<Vehicle> veh)
        {
            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(path);
            Excel._Worksheet xlWorksheet2 = xlWorkbook.Sheets[2];
            Excel._Worksheet xlWorksheet4 = xlWorkbook.Sheets[4];
            Excel.Range xlRange2 = xlWorksheet2.UsedRange;
            Excel.Range xlRange4 = xlWorksheet4.UsedRange;

            //Operacje tutaj zaczynać

            if (type == ExcelType.Vehicles)
            {
                GetVehicles(xlRange2, xlRange4, ref veh);
            }
            //Koniec operacji

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange2);
            Marshal.ReleaseComObject(xlWorksheet2);
            Marshal.ReleaseComObject(xlRange4);
            Marshal.ReleaseComObject(xlWorksheet4);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return true;
        }
        public static bool LoadExcelFile(string path, ExcelType type, ref PID pid)
        {
            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(path);
            Excel._Worksheet xlWorksheet1 = xlWorkbook.Sheets[1];
            Excel._Worksheet xlWorksheet2 = xlWorkbook.Sheets[2];
            Excel._Worksheet xlWorksheet3 = xlWorkbook.Sheets[3];
            Excel.Range xlRange1 = xlWorksheet1.UsedRange;
            Excel.Range xlRange2 = xlWorksheet2.UsedRange;
            Excel.Range xlRange3 = xlWorksheet3.UsedRange;

            //Operacje tutaj zaczynać
            if (type == ExcelType.PID)
            {
                GetPID(xlRange1, xlRange2, xlRange3, ref pid);
            }
            //Koniec operacji

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange1);
            Marshal.ReleaseComObject(xlWorksheet1);
            Marshal.ReleaseComObject(xlRange2);
            Marshal.ReleaseComObject(xlWorksheet2);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return true;
        }
        public static bool LoadExcelFile(string path, ExcelType type, ref List<Profile> prof)
        {
            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(path);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            //Operacje tutaj zaczynać
            if (type == ExcelType.Profile)
            {
                int n = xlWorkbook.Worksheets.Count;
                for (int i = 1; i <= n; i++)
                {
                    xlWorksheet = xlWorkbook.Sheets[i];
                    Excel.Range range = xlWorksheet.UsedRange;

                    string name = xlWorksheet.Name;
                    GetProfiles(range, ref prof, name);
                }
            }
            //Koniec operacji

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return true;
        }
        public static bool LoadExcelFile(string path, ref float[][][] zones, ref string[][] names)
        {
            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(path);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            //Operacje tutaj zaczynać
            int n = xlWorkbook.Worksheets.Count;
            for (int i = 1; i <= n; i++)
            {
                xlWorksheet = xlWorkbook.Sheets[i];
                Excel.Range range = xlWorksheet.UsedRange;

                string name = xlWorksheet.Name;
                GetZones(range, ref zones, ref names);
            }
            
            //Koniec operacji

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return true;
        }
        private static bool GetZones(Excel.Range range, ref float[][][] zones, ref string[][] names)
        {
            for(int l = 0; l < 1; l++)
            {
                int n = 0;
                while (range.Cells[n + 2, 2 * l + 1].Value != null)
                {
                    n++;
                }
                zones[l] = new float[2][];
                zones[l][0] = new float[n - 1];
                zones[l][1] = new float[n - 1];
                names[l] = new string[n - 1];
            }
            for(int l = 0; l < 1; l++)
            {
                for(int i = 0; i < zones[l][0].Length; i++)
                {
                    string name = range.Cells[i + 2, 2 * l + 1].Value + " " + range.Cells[i + 3, 2 * l + 1].Value;
                    names[l][i] = name;
                    try
                    {
                        float start = float.Parse(range.Cells[i + 2, 2 * l + 2].Value.ToString());
                        float end = float.Parse(range.Cells[i + 3, 2 * l + 2].Value.ToString());
                        zones[l][0][i] = start;
                        zones[l][1][i] = end;
                    }
                    catch { }
                }
            }
            return true;
        }
        private static bool GetVehicles(Excel.Range range2, Excel.Range range4, ref List<Vehicle> veh)
        {
            //okresla nazwy pojazdow
            for (int i = 56; i <= 63; i++)
            {
                if (range2.Cells[1, i + 1].Value != null)
                {
                    var v = new Vehicle();
                    v.Name = range2.Cells[1, i + 1].Value.ToString();
                    v.Length = float.Parse(range4.Cells[3, i + 1].Value2.ToString());
                    v.AxlesCount = int.Parse(range4.Cells[4, i + 1].Value2.ToString());
                    v.AxlesDriven = int.Parse(range4.Cells[5, i + 1].Value2.ToString());
                    v.MaxSpeed = float.Parse(range4.Cells[6, i + 1].Value2.ToString());
                    v.GrossMass = float.Parse(range4.Cells[7, i + 1].Value.ToString()) * 1000;
                    v.FrontalArea = float.Parse(range4.Cells[8, i + 1].Value.ToString());
                    v.Members = int.Parse(range4.Cells[9, i + 1].Value.ToString());
                    v.AxleForce = float.Parse(range4.Cells[10, i + 1].Value.ToString());
                    v.AxleForceManufacturer = float.Parse(range4.Cells[11, i + 1].Value.ToString());
                    v.FastBreakMass = float.Parse(range4.Cells[12, i + 1].Value.ToString());
                    v.SlowBreakMass = float.Parse(range4.Cells[13, i + 1].Value.ToString());
                    v.AxlesConfig = range4.Cells[14, i + 1].Value.ToString();
                    v.FastDecel = float.Parse(range4.Cells[15, i + 1].Value.ToString());
                    v.SlowDecel = float.Parse(range4.Cells[16, i + 1].Value.ToString());
                    v.CoefA = float.Parse(range4.Cells[17, i + 1].Value.ToString());
                    v.CoefB = float.Parse(range4.Cells[18, i + 1].Value.ToString());
                    v.CoefC = float.Parse(range4.Cells[19, i + 1].Value.ToString());
                    v.JerkMax = float.Parse(range4.Cells[20, i + 1].Value.ToString());
                    v.AccMax = float.Parse(range4.Cells[21, i + 1].Value.ToString());
                    v.DecMax = float.Parse(range4.Cells[22, i + 1].Value.ToString());

                    int velocityMax = 0;
                    for (int vel = 0; vel < 300; vel++)
                    {
                        if (range2.Cells[vel + 3, i + 1].Value != null)
                        {
                            velocityMax++;
                        }
                        else break;
                    }
                    v.Force = new float[velocityMax];
                    for (int vel = 0; vel < velocityMax; vel++)
                    {
                        v.Force[vel] = float.Parse(range2.Cells[vel + 3, i + 1].Value.ToString()) * 1000;
                    }

                    veh.Add(v);
                }
                else break;
            }

            return true;
        }
        private static bool GetPID(Excel.Range range1, Excel.Range range2, Excel.Range range3, ref PID pid)
        {
            int n = 0;
            int m = 0;
            while (range1.Cells[n + 1, 1].Value != null)
            {
                n++;
            }
            while (range1.Cells[1, m + 1].Value != null)
            {
                m++;
            }

            pid.P = new float[n, m];
            pid.I = new float[n, m];
            pid.D = new float[n, m];
            pid.Coeff = new float[2, 6];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (true)
                    {
                        pid.P[i, j] = float.Parse(range1.Cells[i + 1, j + 1].Value.ToString());
                        pid.I[i, j] = float.Parse(range2.Cells[i + 1, j + 1].Value.ToString());
                    }
                }
            }
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    pid.Coeff[i, j] = float.Parse(range3.Cells[j + 1, i + 1].Value.ToString());
                }
            }
            return true;
        }
        private static bool GetProfiles(Excel.Range range, ref List<Profile> prof, string name)
        {
            Profile p = new Profile();
            p.Name = name;
            int ind = 0;

            int n = 0;
            int m = 0;
            int o = 0;

            int s = 0;
            int pt = 0;
            int j = 0;
            int sector = 0;
            int c = 0;

            while (range.Cells[n + 1, 1].Value != null) { n++; }
            while (range.Cells[m + 1, 3].Value != null) { m++; }
            while (range.Cells[o + 1, 5].Value != null) { o++; }
            while (range.Cells[ind + 3, 10].Value != null) { ind++; }

            for (int i = 0; i < ind; i++)
            {
                if (IsStop(range, i)) s++;
                if (IsPowerStation(range, i)) pt++;
                if (IsJunction(range, i)) j++;
                if (IsJunction1(range, i)) j++;
                if (IsJunction2(range, i)) j++;
                if (IsJunction12(range, i)) j++;
                if (IsJunction22(range, i)) j++;
                if (IsSeparator(range, i)) sector++;
                if (IsCabin(range, i)) c++;
            }

            int objectsCount = s + pt + j + sector + c;

            p.Profile1 = new float[2, n - 1];
            p.Profile2 = new float[2, m - 1];
            p.Limits = new float[2, o - 1];
            p.ObjectType = new ObjectType[objectsCount];
            if (s != 0)
            {
                p.Stations = new Station[s];
                p.PowerObject = new PowerObject[objectsCount - s];
                p.Tracks = new Track[objectsCount - 1];
                for (int i = 0; i < p.PowerObject.Length; i++) p.PowerObject[i] = new PowerObject();

                for (int i = 0; i < p.Stations.Length; i++) p.Stations[i] = new Station();
            }

            for (int i = 0; i < n - 1; i++)
            {
                p.Profile1[0, i] = float.Parse(range.Cells[i + 2, 1].Value.ToString());
                p.Profile1[1, i] = float.Parse(range.Cells[i + 2, 2].Value.ToString());
            }
            for (int i = 0; i < m - 1; i++)
            {
                p.Profile2[0, i] = float.Parse(range.Cells[i + 2, 3].Value.ToString());
                p.Profile2[1, i] = float.Parse(range.Cells[i + 2, 4].Value.ToString());
            }
            for (int i = 0; i < o - 1; i++)
            {
                p.Limits[0, i] = float.Parse(range.Cells[i + 2, 5].Value.ToString());
                p.Limits[1, i] = float.Parse(range.Cells[i + 2, 6].Value.ToString());
            }

            int sInd = 0;
            int pObjInd = 0;
            for (int i = 0; i < objectsCount; i++)
            {
                if (range.Cells[i + 3, 9].Value != null)
                {
                    if (IsStop(range, i))
                    {
                        p.Stations[sInd] = new Station();

                        string nameStation = range.Cells[i + 3, 10].Value.ToString();
                        float positionStation = float.Parse(range.Cells[i + 3, 9].Value.ToString());

                        p.Stations[sInd].Name = nameStation;
                        p.Stations[sInd].Position = positionStation;

                        sInd++;
                    }

                    PowerObjectType pOType = new PowerObjectType();
                    if (IsPowerObject(range, i, ref pOType))
                    {
                        p.PowerObject[pObjInd] = new PowerObject();
                        p.PowerObject[pObjInd].Type = pOType;

                        string nameObj = range.Cells[i + 3, 10].Value.ToString();
                        float position = float.Parse(range.Cells[i + 3, 9].Value.ToString());
                        int rCount = int.Parse(range.Cells[i + 3, 14].Value.ToString());
                        int wCount = int.Parse(range.Cells[i + 3, 29].Value.ToString());

                        p.PowerObject[pObjInd].Name = nameObj;
                        p.PowerObject[pObjInd].Position = position;
                        p.PowerObject[pObjInd].RailCount = rCount;
                        p.PowerObject[pObjInd].WingCount = wCount;

                        //elektryka
                        if (pOType == PowerObjectType.PowerStation)
                        {
                            p.PowerObject[pObjInd].Elec.RWewA = float.Parse(range.Cells[i + 3, 17].Value.ToString());
                            p.PowerObject[pObjInd].Elec.RPA = float.Parse(range.Cells[i + 3, 18].Value.ToString());
                            p.PowerObject[pObjInd].Elec.RZA1 = float.Parse(range.Cells[i + 3, 19].Value.ToString());
                            p.PowerObject[pObjInd].Elec.RZA2 = float.Parse(range.Cells[i + 3, 20].Value.ToString());
                            //p.PowerObject[pObjInd].Elec.RZB1 = float.Parse(range.Cells[i + 3, 21].Value.ToString());
                            //p.PowerObject[pObjInd].Elec.RZB2 = float.Parse(range.Cells[i + 3, 22].Value.ToString());
                            p.PowerObject[pObjInd].Elec.UA = float.Parse(range.Cells[i + 3, 23].Value.ToString());
                            p.PowerObject[pObjInd].SupplyCount = int.Parse(range.Cells[i + 3, 28].Value.ToString());
                            for (int rz = 0; rz< 4; rz++)
                            {
                                try
                                {
                                    p.PowerObject[pObjInd].Elec.RZ[rz] = float.Parse(range.Cells[i + 3, 19 + rz].Value.ToString());
                                }
                                catch
                                {
                                    p.PowerObject[pObjInd].Elec.RZ[rz] = 0;
                                }
                            }
                            for (int rz = 0; rz < 8; rz++)
                            {
                                try
                                {
                                    p.PowerObject[pObjInd].Elec.RZ[rz + 4] = float.Parse(range.Cells[i + 3, 30 + rz].Value.ToString());
                                }
                                catch
                                {
                                    p.PowerObject[pObjInd].Elec.RZ[rz + 4] = 0;
                                }
                            }
                        }
                        if (pOType == PowerObjectType.Junction)
                        {

                        }
                        if (pOType == PowerObjectType.Separator)
                        {
                            float rParallel = 0;
                            if (float.Parse(range.Cells[i + 3, 18].Value.ToString()) != null)
                                rParallel = float.Parse(range.Cells[i + 3, 18].Value.ToString());
                            p.PowerObject[pObjInd].Elec.RPA = rParallel;
                        }
                        if (pOType == PowerObjectType.Cabin)
                        {
                            if (p.PowerObject[pObjInd].RailCount == 2)
                            {
                                p.PowerObject[pObjInd].Elec.RK1 = float.Parse(range.Cells[i + 3, 24].Value.ToString());
                                p.PowerObject[pObjInd].Elec.RK2 = float.Parse(range.Cells[i + 3, 25].Value.ToString());
                                p.PowerObject[pObjInd].Elec.RK3 = float.Parse(range.Cells[i + 3, 26].Value.ToString());
                                p.PowerObject[pObjInd].Elec.RK4 = float.Parse(range.Cells[i + 3, 27].Value.ToString());
                            }
                        }
                        pObjInd++;
                    }

                    if (i != 0)
                    {
                        p.Tracks[i - 1] = new Track();

                        string nameTrack = range.Cells[i + 3, 12].Value.ToString();
                        float lengthTrack = float.Parse(range.Cells[i + 3, 13].Value.ToString());
                        int railCount = int.Parse(range.Cells[i + 3, 14].Value.ToString());
                        float rS = float.Parse(range.Cells[i + 3, 15].Value.ToString());
                        float rT = float.Parse(range.Cells[i + 3, 16].Value.ToString());

                        p.Tracks[i - 1].Name = nameTrack;
                        p.Tracks[i - 1].Length = lengthTrack;
                        p.Tracks[i - 1].Position = float.Parse(range.Cells[i - 1 + 3, 9].Value.ToString());
                        p.Tracks[i - 1].ProfileName = name;
                        p.Tracks[i - 1].RailCount = railCount;

                        p.Tracks[i - 1].RS = rS;
                        p.Tracks[i - 1].RT = rT;
                    }
                }

                if (range.Cells[i + 3, 11].Value.ToString() == "st"
                    || range.Cells[i + 3, 11].Value.ToString() == "po"
                    || range.Cells[i + 3, 11].Value.ToString() == "podg") p.ObjectType[i] = ObjectType.Station;
                else p.ObjectType[i] = ObjectType.PowerObject;
            }

            prof.Add(p);
            return true;
        }
        public static List<Thing> ImportStationsTracksObjects(List<Profile> prof)
        {
            List<Thing> Things = new List<Thing>();

            for (int p = 0; p < prof.Count; p++)
            {
                for (int s = 0; s < prof[p].Stations.Length; s++)
                {
                    if (prof[p].Stations[s] != null)
                    {
                        Thing t = new Thing(prof[p].Stations[s].Name, 1, ThingType.Station, 0);
                        t.LineName = prof[p].Name;
                        t.KMstart = prof[p].Stations[s].Position;
                        t.Profile = prof[p];
                        t.ProfileName = prof[p].Name;
                        t.WingCount = 2;

                        Things.Add(t);
                        prof[p].Stations[s].Index = Things.IndexOf(t);
                    }
                }
                for (int o = 0; o < prof[p].PowerObject.Length; o++)
                {
                    if (prof[p].PowerObject[o] != null)
                    {
                        Thing t = new Thing(prof[p].PowerObject[o].Name, prof[p].PowerObject[o].RailCount, ThingType.Supply, 0);

                        t.KMstart = prof[p].PowerObject[o].Position;
                        t.Profile = prof[p];
                        t.ProfileName = prof[p].Name;
                        t.WingCount = prof[p].PowerObject[o].WingCount;
                        t.RailCount = prof[p].PowerObject[o].RailCount;

                        if (prof[p].PowerObject[o].Type == PowerObjectType.PowerStation)
                        {
                            t.ThingType = ThingType.Supply;
                            t.SupplyCount = prof[p].PowerObject[o].SupplyCount;
                            t.ElecData.RWewA = prof[p].PowerObject[o].Elec.RWewA;
                            t.ElecData.RPA = prof[p].PowerObject[o].Elec.RPA;
                            t.ElecData.RZA1 = prof[p].PowerObject[o].Elec.RZA1;
                            t.ElecData.UA = prof[p].PowerObject[o].Elec.UA;
                            t.ElecData.RZ = prof[p].PowerObject[o].Elec.RZ;
                        }
                        if (prof[p].PowerObject[o].Type == PowerObjectType.Junction)
                        {
                            t.ThingType = ThingType.Junction;
                        }
                        if (prof[p].PowerObject[o].Type == PowerObjectType.Junction1)
                        {
                            t.ThingType = ThingType.Junction1;
                        }
                        if (prof[p].PowerObject[o].Type == PowerObjectType.Junction2)
                        {
                            t.ThingType = ThingType.Junction2;
                        }
                        if (prof[p].PowerObject[o].Type == PowerObjectType.Junction12)
                        {
                            t.ThingType = ThingType.Junction12;
                        }
                        if (prof[p].PowerObject[o].Type == PowerObjectType.Junction22)
                        {
                            t.ThingType = ThingType.Junction22;
                        }
                        if (prof[p].PowerObject[o].Type == PowerObjectType.Separator)
                        {
                            t.ThingType = ThingType.Separator;
                            t.ElecData.RPA = prof[p].PowerObject[o].Elec.RPA;
                        }
                        if (prof[p].PowerObject[o].Type == PowerObjectType.Cabin)
                        {
                            t.ThingType = ThingType.Cabin;

                            if (prof[p].PowerObject[o].RailCount == 2)
                            {
                                t.ElecData.RK1 = prof[p].PowerObject[o].Elec.RK1;
                                t.ElecData.RK2 = prof[p].PowerObject[o].Elec.RK2;
                                t.ElecData.RK3 = prof[p].PowerObject[o].Elec.RK3;
                                t.ElecData.RK4 = prof[p].PowerObject[o].Elec.RK4;
                            }

                            if (prof[p].PowerObject[o].RailCount == 1)
                            {
                                t.ElecData.RK1 = prof[p].PowerObject[o].Elec.RK1;
                                t.ElecData.RK2 = prof[p].PowerObject[o].Elec.RK2;
                                t.ElecData.RK3 = prof[p].PowerObject[o].Elec.RK3;
                                t.ElecData.RK4 = prof[p].PowerObject[o].Elec.RK4;
                            }
                        }

                        Things.Add(t);
                        prof[p].PowerObject[o].Index = Things.IndexOf(t);
                    }
                }
            }
            for (int p = 0; p < prof.Count; p++)
            {
                for (int tr = 0; tr < prof[p].Tracks.Length; tr++)
                {
                    if (prof[p].Tracks[tr] != null)
                    {
                        Thing t = new Thing(prof[p].Tracks[tr].Name, prof[p].Tracks[tr].RailCount, ThingType.Track, prof[p].Tracks[tr].Length);
                        t.LineName = prof[p].Name;
                        t.KMstart = prof[p].Tracks[tr].Position;
                        t.Profile = prof[p];
                        t.ProfileName = prof[p].Name;
                        t.WingCount = 2;

                        t.ElecData.RT = prof[p].Tracks[tr].RT;
                        t.ElecData.RS = prof[p].Tracks[tr].RS;

                        Things.Add(t);
                        prof[p].Tracks[tr].Index = Things.IndexOf(t);
                    }
                }
            }

            return Things;
        }
        public static List<Node> ImportNodesAndRoutes(List<Profile> prof, ref List<Thing> things, ref List<Route> routes)
        {
            List<Node> Nodes = new List<Node>();

            for (int p = 0; p < prof.Count; p++)
            {
                Route route = new Route(prof[p].Name);

                int count = prof[p].Stations.Length + prof[p].PowerObject.Length;
                int s = 0;
                int po = 0;
                int tr = 0;

                for (int i = 0; i < count - 1; i++)
                {
                    Thing left = new Thing();
                    if (prof[p].ObjectType[i] == ObjectType.Station)
                        left = things[prof[p].Stations[s].Index];
                    else
                        left = things[prof[p].PowerObject[po].Index];
                    Thing right = things[prof[p].Tracks[i].Index];

                    int l = 1;
                    int r = 0;
                    if (i == 0) l = 0;
                    string nameNode = left.Name +
                        " odg." +
                        l.ToString() +
                        "| " +
                        right.Name +
                        " odg." +
                        r.ToString();
                    Node n = new Node(left, l, right, r, nameNode);
                    Nodes.Add(n);
                    route.Nodes.Add(n);

                    int ind1 = things.IndexOf(left);
                    int ind2 = things.IndexOf(right);
                    if (things[ind1].ThingsAtWings == null) things[ind1].ThingsAtWings = new string[16];
                    if (things[ind2].ThingsAtWings == null) things[ind2].ThingsAtWings = new string[16];
                    if (i == 0) things[ind1].ThingsAtWings[0] = right.Name;
                    else things[ind1].ThingsAtWings[1] = right.Name;
                    things[ind2].ThingsAtWings[0] = left.Name;

                    if (prof[p].ObjectType[i] == ObjectType.Station) s++;
                    else po++;

                    if (i < prof[p].Tracks.Length - 1)
                    {
                        left = things[prof[p].Tracks[tr].Index];
                        if (prof[p].ObjectType[i + 1] == ObjectType.Station)
                            right = things[prof[p].Stations[s].Index];
                        else
                            right = things[prof[p].PowerObject[po].Index];

                        l = 1;
                        r = 0;
                        nameNode = left.Name +
                            " odg." +
                            l.ToString() +
                            "| " +
                            right.Name +
                            " odg." +
                            r.ToString();
                        n = new Node(left, l, right, r, nameNode);
                        Nodes.Add(n);
                        route.Nodes.Add(n);

                        ind1 = things.IndexOf(left);
                        ind2 = things.IndexOf(right);
                        if (things[ind1].ThingsAtWings == null) things[ind1].ThingsAtWings = new string[16];
                        if (things[ind2].ThingsAtWings == null) things[ind2].ThingsAtWings = new string[16];
                        things[ind1].ThingsAtWings[1] = right.Name;
                        things[ind2].ThingsAtWings[0] = left.Name;

                        tr++;

                        /*if (prof[p].ObjectType[i] == ObjectType.Station) s++;
                        else po++;*/
                    }
                }
                routes.Add(route);
            }

            return Nodes;
        }
        private static bool IsStop(Excel.Range range, int s)
        {
            string cell = range.Cells[s + 3, 11].Value.ToString();
            if (cell == "st"
                   || cell == "po"
                   || cell == "podg")
                return true;
            else
                return false;
        }
        private static bool IsPowerStation(Excel.Range range, int pt)
        {
            if (range.Cells[pt + 3, 11].Value.ToString() == "pt")
                return true;
            else
                return false;
        }
        private static bool IsJunction(Excel.Range range, int j)
        {
            if (range.Cells[j + 3, 11].Value.ToString() == "r")
                return true;
            else
                return false;
        }
        private static bool IsJunction1(Excel.Range range, int j)
        {
            if (range.Cells[j + 3, 11].Value.ToString() == "r1")
                return true;
            else
                return false;
        }
        private static bool IsJunction2(Excel.Range range, int j)
        {
            if (range.Cells[j + 3, 11].Value.ToString() == "r2")
                return true;
            else
                return false;
        }
        private static bool IsJunction12(Excel.Range range, int j)
        {
            if (range.Cells[j + 3, 11].Value.ToString() == "r12")
                return true;
            else
                return false;
        }
        private static bool IsJunction22(Excel.Range range, int j)
        {
            if (range.Cells[j + 3, 11].Value.ToString() == "r22")
                return true;
            else
                return false;
        }
        private static bool IsSeparator(Excel.Range range, int separator)
        {
            if (range.Cells[separator + 3, 11].Value.ToString() == "s")
                return true;
            else
                return false;
        }
        private static bool IsCabin(Excel.Range range, int c)
        {
            if (range.Cells[c + 3, 11].Value.ToString() == "ks")
                return true;
            else
                return false;
        }
        private static bool IsPowerObject(Excel.Range range, int pO, ref PowerObjectType pOType)
        {
            if (IsPowerStation(range, pO)) { pOType = PowerObjectType.PowerStation; return true; }
            if (IsJunction(range, pO)) { pOType = PowerObjectType.Junction; return true; }
            if (IsJunction1(range, pO)) { pOType = PowerObjectType.Junction1; return true; }
            if (IsJunction2(range, pO)) { pOType = PowerObjectType.Junction2; return true; }
            if (IsJunction12(range, pO)) { pOType = PowerObjectType.Junction12; return true; }
            if (IsJunction22(range, pO)) { pOType = PowerObjectType.Junction22; return true; }
            if (IsSeparator(range, pO)) { pOType = PowerObjectType.Separator; return true; }
            if (IsCabin(range, pO)) { pOType = PowerObjectType.Cabin; return true; }
            return false;
        }

        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }

        public static void ExtractProfilesForNN(Przejazd06 przejazd06)
        {
            int upcomingProfilesCount = 10;
            float[,,] upcomingProfiles1 = new float[przejazd06.KP.VehiclesCount, przejazd06.KP.MaximumTime, upcomingProfilesCount];
            float[,,] upcomingDistances1 = new float[przejazd06.KP.VehiclesCount, przejazd06.KP.MaximumTime, upcomingProfilesCount];
            float[,,] upcomingProfiles2 = new float[przejazd06.KP.VehiclesCount, przejazd06.KP.MaximumTime, upcomingProfilesCount];
            float[,,] upcomingDistances2 = new float[przejazd06.KP.VehiclesCount, przejazd06.KP.MaximumTime, upcomingProfilesCount];
            float[,,] upcomingLimits = new float[przejazd06.KP.VehiclesCount, przejazd06.KP.MaximumTime, 1];
            float[,,] upcomingDistanceLimits = new float[przejazd06.KP.VehiclesCount, przejazd06.KP.MaximumTime, 1];
            float[,] upcomingStop = new float[przejazd06.KP.VehiclesCount, przejazd06.KP.MaximumTime];

            for (int i = 0; i < przejazd06.KP.MaximumTime; i++)
            {
                Parallel.For(0, przejazd06.KP.VehiclesCount, v =>
                {
                    float position = przejazd06.PT[v][i, 5];
                    CheckProfileForNN(przejazd06.KP.CO.Profile1, v, upcomingProfilesCount, ref upcomingProfiles1, ref upcomingDistances1, i, position);
                    CheckProfileForNN(przejazd06.KP.CO.Profile2, v, upcomingProfilesCount, ref upcomingProfiles2, ref upcomingDistances2, i, position);
                    CheckProfileForNN(przejazd06.KP.CO.Limits, v, 1, ref upcomingLimits, ref upcomingDistanceLimits, i, position);
                    //CheckUpcomingStopForNN(v, i, przejazd06)
                });
            }
        }

        private static void CheckProfileForNN(float[][,] profile, int v, int upcomingProfilesCount, ref float[,,] upcomingProfiles1,
            ref float[,,] upcomingDistances1, int i, float position)
        {
            int profileArrayLength = profile[v].GetLength(1);
            for (int p = 0; p < profileArrayLength - 1; p++)
            {
                if (position > profile[v][0, p] && position < profile[v][0, p + 1])
                {
                    float upcomingProfile = 0;
                    float upcomingDistance = 0;
                    for (int up = 1; up <= upcomingProfilesCount; up++)
                    {
                        try
                        {
                            upcomingProfile = profile[v][1, p + up];
                            upcomingDistance = profile[v][0, p + up];
                        }
                        catch
                        {
                            upcomingProfile = profile[v][1, profileArrayLength - 1];
                            upcomingDistance = profile[v][0, profileArrayLength - 1];
                        }
                        upcomingProfiles1[v, i, up - 1] = upcomingProfile;
                        upcomingDistances1[v, i, up - 1] = upcomingDistance;
                    }
                    break;
                }
            }
        }

        private static void CheckUpcomingStopForNN(int v, int i, Przejazd06 przejazd06)
        {

        }
    }
}
