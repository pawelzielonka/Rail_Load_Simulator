using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vamos21
{
    [Serializable]
    public class CalculationObjects
    {
        public Vehicle[] Vehicles { get; set; }
        public float[] CurrentsVeh { get; set; }
        public float[] VoltageVeh { get; set; }
        public float[] DistanceVeh { get; set; }
        public int[] BranchesVeh { get; set; }
        public int[] BranchesSup { get; set; }
        public float[] DistVehOnThing { get; set; }
        public PIDParameters[] PIDParameters { get; set; }
        public float[][,] Limits { get; set; }
        public float[][,] Profile1 { get; set; }
        public float[][,] Profile2 { get; set; }
        public float[][] StopsDist { get; set; }
        public bool[][] StopsChecked { get; set; }
        public int[][] StopsTimes { get; set; }
        public int[] NextStopNumber { get; set; }
        public bool[] CheckStopAllowance { get; set; }
        public bool[] CheckSlowDownAllowance { get; set; }
        public string[][] ThingNames { get; set; }
        public float[][] OnTrackEnter { get; set; }
        public float[][] OnTrackLeave { get; set; }
        public bool[][][] ReverseNode { get; set; }
        public bool[][] ThingIsReversed { get; set; }
        public Direction[] DirectionOnTrack { get; set; }
        public int[][] Rails { get; set; }
        public int[][] RailsOpposite { get; set; }
        public string[][] ThingNamesOpposite { get; set; }
        public CalculationObjects()
        {

        }
        public CalculationObjects(List<Vehicle> vl)
        {
            int size = vl.Count;
            Vehicles = new Vehicle[size];
            CurrentsVeh = new float[size];
            VoltageVeh = new float[size];
            DistanceVeh = new float[size];
            BranchesVeh = new int[size];
            DistVehOnThing = new float[size];
            PIDParameters = new PIDParameters[size];
            Limits = new float[size][,];
            Profile1 = new float[size][,];
            Profile2 = new float[size][,];
            StopsDist = new float[size][];
            StopsChecked = new bool[size][];
            StopsTimes = new int[size][];
            CheckStopAllowance = new bool[size];
            CheckSlowDownAllowance = new bool[size];
            NextStopNumber = new int[size];
            ThingNames = new string[size][];
            OnTrackEnter = new float[size][];
            OnTrackLeave = new float[size][];
            ReverseNode = new bool[size][][];
            ThingIsReversed = new bool[size][];
            DirectionOnTrack = new Direction[size];
            Rails = new int[size][];
            RailsOpposite = new int[size][];
            ThingNamesOpposite = new string[size][];

            for (int i = 0; i < size; i++)
            {
                Vehicles[i] = vl[i];
            }
        }
        public void GetThings(Lists lists, ConfigurationData cData, bool ifBeforePass)
        {
            //tworzy tablice thingow przez ktore jedzie pojazd
            for (int i = 0; i < Vehicles.Length; i++)
            {
                int thingsCount = lists.Rl[i].Nodes.Count + 1;
                this.ThingNames[i] = new string[thingsCount];
                this.OnTrackEnter[i] = new float[thingsCount];
                this.OnTrackLeave[i] = new float[thingsCount];
                this.Rails[i] = new int[thingsCount];
                this.ThingIsReversed[i] = new bool[thingsCount];

                this.ReverseNode[i] = new bool[2][];
                for (int n = 0; n < 2; n++)
                {
                    this.ReverseNode[i][0] = new bool[thingsCount];
                    this.ReverseNode[i][1] = new bool[thingsCount];
                }

                if (lists.Dl[i] == Direction.Along || lists.Dl[i] == Direction.Opposite)
                {
                    for (int n = 0; n < thingsCount - 1; n++)
                    {
                        this.ThingNames[i][n] = lists.Rl[i].Nodes[n].ThingIn.Name;
                        this.Rails[i][n] = lists.Rl[i].Nodes[n].WingIn;
                        try
                        {
                            this.ReverseNode[i][0][n] = lists.Rl[i].Reverse[0][n];
                        }
                        catch
                        {
                        }
                        this.ReverseNode[i][0][this.ReverseNode[i][0].GetLength(0) - 1] = this.ReverseNode[i][0][this.ReverseNode[i][0].GetLength(0) - 2];
                    }
                    this.ThingNames[i][thingsCount - 1] = lists.Rl[i].Nodes[lists.Rl[i].Nodes.Count - 1].ThingOut.Name;
                    this.Rails[i][thingsCount - 1] = lists.Rl[i].Nodes[lists.Rl[i].Nodes.Count - 1].WingOut;
                    try
                    {
                        //this.ReverseNode[i][0][thingsCount - 1] = lists.Rl[i].Reverse[1][lists.Rl[i].Nodes.Count - 1];
                    }
                    catch { }
                }

                if (lists.Dl[i] == Direction.Opposite)
                {
                    this.ThingNamesOpposite[i] = new string[thingsCount];
                    this.RailsOpposite[i] = new int[thingsCount];
                    for (int n = 0; n < thingsCount - 1; n++)
                    {
                        this.ThingNamesOpposite[i][n] = lists.Rl[i].Nodes[thingsCount - 2 - n].ThingOut.Name;
                        this.RailsOpposite[i][n] = lists.Rl[i].Nodes[thingsCount - 2 - n].WingOut;
                        try
                        {
                            this.ReverseNode[i][1][n] = lists.Rl[i].Reverse[1][thingsCount - 2 - n];
                        }
                        catch { }
                    }
                    this.ReverseNode[i][1][this.ReverseNode[i][0].GetLength(0) - 1] = this.ReverseNode[i][0][this.ReverseNode[i][1].GetLength(0) - 2];
                    this.ThingNamesOpposite[i][thingsCount - 1] = lists.Rl[i].Nodes[0].ThingIn.Name;
                    this.RailsOpposite[i][thingsCount - 1] = lists.Rl[i].Nodes[0].WingIn;
                    try
                    {
                        //this.ReverseNode[i][1][thingsCount - 1] = lists.Rl[i].Reverse[0][0];
                    }
                    catch { }
                }

                for (int t = 0; t < ThingIsReversed[i].GetLength(0); t++)
                {
                    if (this.ReverseNode[i][0][t] == true/* || this.ReverseNode[i][1][t] == true*/) ThingIsReversed[i][t] = true;
                }

                ReverseThings(cData, i, lists.Dl[i]);
            }

            //tworzy tablice profili, limitow i stopow przez ktore jedzie pojazd
            for (int i = 0; i < Vehicles.Length; i++)
            {
                try { GetProfile1(cData, i); }
                catch { }
                try { GetProfile2(cData, i); }
                catch { }
                try { GetLimits(cData, i); }
                catch { }
                try { GetStops(cData, i, ifBeforePass); }
                catch { }
                try { GetEnterLeave(cData, i); }
                catch { }
            }
        }
        private void ReverseThings(ConfigurationData cData, int i, Direction direction)
        {
            if (direction == Direction.Along)
            {
                for(int tir = 0; tir < this.ThingIsReversed[i].GetLength(0); tir++)
                {
                    if (this.ThingIsReversed[i][tir] == true)
                    {
                        int revBoxStart = tir;
                        int revBoxStop = tir;
                        for(int tirStill = tir; tirStill < this.ThingIsReversed[i].GetLength(0); tirStill++)
                        {
                            if (this.ThingIsReversed[i][tirStill] == true)
                                revBoxStop = tirStill;
                            else
                                break;
                        }
                        string[] thingsNamesBox = new string[revBoxStop + 1 - revBoxStart];
                        for(int t = 0; t <= revBoxStop; t++)
                        {
                            thingsNamesBox[t] = this.ThingNames[i][revBoxStop - t];
                        }
                        for (int t = revBoxStart; t <= revBoxStop; t++)
                            this.ThingNames[i][t] = thingsNamesBox[t];
                        tir = revBoxStop;
                    }
                }
            }
            else
            {
                for (int tir = 0; tir < this.ThingIsReversed[i].GetLength(0); tir++)
                {
                    if (this.ThingIsReversed[i][tir] == true)
                    {
                        int revBoxStart = tir;
                        int revBoxStop = tir;
                        for (int tirStill = tir; tirStill < this.ThingIsReversed[i].GetLength(0); tirStill++)
                        {
                            if (this.ThingIsReversed[i][tirStill] == true)
                                revBoxStop = tirStill;
                            else
                                break;
                        }
                        string[] thingsNamesBox = new string[revBoxStop + 1 - revBoxStart];
                        for (int t = 0; t <= revBoxStop; t++)
                        {
                            thingsNamesBox[t] = this.ThingNames[i][revBoxStop - t];
                        }
                        for (int t = revBoxStart; t <= revBoxStop; t++)
                            this.ThingNames[i][t] = thingsNamesBox[t];
                        tir = revBoxStop;
                    }
                }

                int thingNamesLength = this.ThingNames[i].GetLength(0) - 1;
                try
                {
                    for (int tn = 0; tn <= thingNamesLength; tn++)
                        this.ThingNamesOpposite[i][tn] = this.ThingNames[i][thingNamesLength - tn];
                }
                catch { }
            }
        }
        private void GetProfile1(ConfigurationData cData, int i)
        {
            int n = 0;
            float start = 0;
            //okresla wymiar tablicy
            for (int t = 0; t < this.ThingNames[i].Length; t++)
            {
                string name = this.ThingNames[i][t];
                var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                start = th.KMstart;
                if (th.Length != 0)
                    for (int a = 0; a < th.Profile.Profile1.GetLength(1); a++)
                    {
                        if (th.Profile.Profile1[0, a] >= start && th.Profile.Profile1[0, a] <= start + th.Length)
                            n++;
                    }
            }
            this.Profile1[i] = new float[2, n];
            //wpisuje do tablicy
            n = 0;
            start = 0;
            float previousLength = 0;
            for (int t = 0; t < this.ThingNames[i].Length; t++)
            {
                bool ifReversed = false;
                if (this.Vehicles[i].Direction == Direction.Along) ifReversed = this.ReverseNode[i][0][t];
                else ifReversed = this.ReverseNode[i][1][t];

                string name = this.ThingNames[i][t];
                var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                if (ifReversed == false)
                    start = th.KMstart;
                else
                {
                    float profileLength = th.Profile.Stations[th.Profile.Stations.Length - 1].Position;
                    start = profileLength - (th.KMstart + th.Length);
                }

                if (th.Length != 0)
                    if (ifReversed == false)
                        for (int a = 0; a < th.Profile.Profile1.GetLength(1); a++)
                        {
                            if (th.Profile.Profile1[0, a] >= start && th.Profile.Profile1[0, a] <= start + th.Length)
                            {
                                this.Profile1[i][0, n] = th.Profile.Profile1[0, a] - start + previousLength;
                                this.Profile1[i][1, n] = th.Profile.Profile1[1, a];
                                n++;
                            }
                        }
                    else
                        for (int a = th.Profile.Profile1.GetLength(1) - 1; a >= 0; a--)
                            if (th.Profile.Profile1[0, a] < start && th.Profile.Profile1[0, a] >= start - th.Length)
                            {
                                this.Profile1[i][0, n] = th.Profile.Profile1[0, a] - start + previousLength;
                                this.Profile1[i][1, n] = th.Profile.Profile1[1, a];
                                n++;
                            }

                previousLength += th.Length;
            }
        }
        private void GetProfile2(ConfigurationData cData, int i)
        {
            int n = 0;
            float start = 0;
            //okresla wymiar tablicy
            for (int t = 0; t < this.ThingNames[i].Length; t++)
            {
                string name = this.ThingNames[i][t];
                var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                start = th.KMstart;
                if (th.Length != 0)
                    for (int a = 0; a < th.Profile.Profile2.GetLength(1); a++)
                    {
                        if (th.Profile.Profile2[0, a] >= start && th.Profile.Profile2[0, a] <= start + th.Length)
                            n++;
                    }
            }
            this.Profile2[i] = new float[2, n];
            //wpisuje do tablicy
            n = 0;
            start = 0;
            float previousLength = 0;
            for (int t = 0; t < this.ThingNames[i].Length; t++)
            {
                bool ifReversed = false;
                if (cData.Vehicles[i].Direction == Direction.Along) ifReversed = this.ReverseNode[i][0][t];
                else ifReversed = this.ReverseNode[i][1][t];

                string name = this.ThingNames[i][t];
                var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                if (ifReversed == false)
                    start = th.KMstart;
                else
                {
                    float profileLength = th.Profile.Stations[th.Profile.Stations.Length - 1].Position;
                    start = profileLength - (th.KMstart + th.Length);
                }

                if (th.Length != 0)
                    if (ifReversed == false)
                        for (int a = 0; a < th.Profile.Profile2.GetLength(1); a++)
                        {
                            if (th.Profile.Profile2[0, a] >= start && th.Profile.Profile2[0, a] <= start + th.Length)
                            {
                                this.Profile2[i][0, n] = th.Profile.Profile2[0, a] - start + previousLength;
                                this.Profile2[i][1, n] = th.Profile.Profile2[1, a];
                                n++;
                            }
                        }
                    else
                        for (int a = th.Profile.Profile2.GetLength(1) - 1; a >= 0; a--)
                            if (th.Profile.Profile2[0, a] < start && th.Profile.Profile2[0, a] >= start - th.Length)
                            {
                                this.Profile2[i][0, n] = th.Profile.Profile2[0, a] - start + previousLength;
                                this.Profile2[i][1, n] = th.Profile.Profile2[1, a];
                                n++;
                            }

                previousLength += th.Length;
            }
        }
        private void GetLimits(ConfigurationData cData, int i)
        {
            int n = 0;
            float start = 0;

            //okresla wymiar tablicy
            for (int t = 0; t < this.ThingNames[i].Length; t++)
            {
                string name = this.ThingNames[i][t];
                var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                start = th.KMstart;

                bool ifReversed = false;
                if (this.ThingIsReversed[i][t] == true) ifReversed = true;

                if (ifReversed == false)
                    start = th.KMstart;
                else
                {
                    float profileLength = th.Profile.Stations[th.Profile.Stations.Length - 1].Position;
                    start = th.KMstart;// profileLength - (th.KMstart + th.Length);
                }

                if (th.Length != 0)
                    if(ifReversed==false)
                    for (int a = 0; a < th.Profile.Limits.GetLength(1); a++)
                    {
                        if (th.Profile.Limits[0, a] >= start && th.Profile.Limits[0, a] <= start + th.Length)
                            n++;
                    }
                    else
                        for (int a = th.Profile.Limits.GetLength(1) - 1; a > 0; a--)
                        {
                            float actual = th.Profile.Limits[0, a];
                            if (actual >= start
                                && actual <= start + th.Length)
                                n++;
                        }
            }
            this.Limits[i] = new float[2, n];
            //wpisuje do tablicy
            n = 0;
            start = 0;
            float previousLength = 0;
            for (int t = 0; t < this.ThingNames[i].Length; t++)
            {
                bool ifReversed = false;
                if (this.ThingIsReversed[i][t] == true) ifReversed = true;

                string name = this.ThingNames[i][t];
                var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                float profileLength = 0;
                float restOfProfile = 0;
                float lastOfProfile = 0;
                if (ifReversed == false)
                    start = th.KMstart;
                else
                {
                    profileLength = th.Profile.Stations[th.Profile.Stations.Length - 1].Position;
                    start = th.KMstart;// (th.KMstart + th.Length);
                    restOfProfile = th.Profile.Limits[0, th.Profile.Limits.GetLength(1) - 1] - profileLength;
                    lastOfProfile = th.Profile.Limits[0, th.Profile.Limits.GetLength(1) - 1];
                }

                if (th.Length != 0)
                    if (ifReversed == false)
                        for (int a = 0; a < th.Profile.Limits.GetLength(1); a++)
                        {
                            if (th.Profile.Limits[0, a] >= start && th.Profile.Limits[0, a] <= start + th.Length)
                            {
                                this.Limits[i][0, n] = th.Profile.Limits[0, a] - start + previousLength;
                                this.Limits[i][1, n] = th.Profile.Limits[1, a];
                                n++;
                            }
                        }
                    else
                    {
                        for (int a = th.Profile.Limits.GetLength(1) - 1; a > 0 ; a--)
                        {
                            float actual = th.Profile.Limits[0, a];
                            if (actual >= start
                                && actual <= start + th.Length)
                            {
                                this.Limits[i][0, n] = Math.Abs(actual - start - th.Length - previousLength);
                                this.Limits[i][1, n] = th.Profile.Limits[1, a - 1];
                                n++;
                            }
                        }
                    }

                previousLength += th.Length;
            }
        }
        private void GetStops(ConfigurationData cData, int i, bool ifBeforePass)
        {
            int n = 0;
            for (int t = 0; t < this.ThingNames[i].Length; t++)
            {
                string name = this.ThingNames[i][t];
                var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                if (th.ThingType == ThingType.Station)
                    n++;
            }
            this.StopsDist[i] = new float[n];
            this.StopsChecked[i] = new bool[ThingNames[i].Length];

            n = 0;
            float previousLength = 0;
            for (int t = 0; t < this.ThingNames[i].Length; t++)
            {
                string name = this.ThingNames[i][t];
                var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                if (th.ThingType == ThingType.Station)
                {
                    this.StopsDist[i][n] = previousLength;
                    n++;
                }
                previousLength += th.Length;
            }

            if (ifBeforePass == true)
            {
                n = 0;
                List<string> acceptedNames = new List<string>();
                for (int t = 0; t < this.StopsDist[i].Length; t++)
                {
                    if (cData.StopConfig[i].StopsChecked[t] == true)
                    {
                        acceptedNames.Add(cData.StopConfig[i].StopsNames[t]);
                        n++;
                    }
                }
                float[] tmpDist = new float[n];
                int[] tmpTimes = new int[n];
                n = 0;

                previousLength = 0;
                for (int t = 0; t < this.ThingNames[i].Length; t++)
                {
                    try
                    {
                        string name = this.ThingNames[i][t];
                        var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                        for (int nm = 0; nm < acceptedNames.Count; nm++)
                        {
                            string nameAccepted = acceptedNames[nm];
                            if (name == nameAccepted)
                            {
                                tmpDist[n] = previousLength;
                                n++;
                            }
                        }
                        previousLength += th.Length;
                    }
                    catch
                    {
                    }
                }

                n = 0;
                for (int t = 0; t < cData.StopConfig[i].StopsNames.Length; t++)
                {
                    try
                    {
                        string name = cData.StopConfig[i].StopsNames[t];
                        for (int nm = 0; nm < acceptedNames.Count; nm++)
                        {
                            string nameAccepted = acceptedNames[nm];
                            if (name == nameAccepted)
                            {
                                tmpTimes[n] = cData.StopConfig[i].StopsTimes[t];
                                n++;
                            }
                        }
                    }
                    catch { }
                }
                this.StopsDist[i] = tmpDist;
                this.StopsTimes[i] = tmpTimes;
            }
        }
        private void GetEnterLeave(ConfigurationData cData, int i)
        {
            float previousLength = 0;
            for (int t = 0; t < ThingNames[i].Length; t++)
            {
                string name = this.ThingNames[i][t];
                var th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                OnTrackEnter[i][t] = previousLength;
                previousLength += th.Length;
                OnTrackLeave[i][t] = previousLength;
            }
        }
    }
    public class PIDParameters
    {
        public float P { get; set; }
        public float I { get; set; }
        public float D { get; set; }
    }
    public class Lists
    {
        public List<Vehicle> Vl { get; set; }
        public List<Route> Rl { get; set; }
        public List<Direction> Dl { get; set; }
        public Lists()
        {
            Vl = new List<Vehicle>();
            Rl = new List<Route>();
            Dl = new List<Direction>();
        }
    }
    public class Time
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Millies { get; set; }
        public int OnlySeconds { get; set; }
        public Time()
        {

        }
        public static Time GetTime(int seconds)
        {
            Time t = new Time();
            int h = seconds / 3600;
            int m = seconds / 60 - h * 60;
            t.Hours = h;
            t.Minutes = m;
            t.Seconds = seconds - m * 60 - h * 3600;
            t.OnlySeconds = seconds;
            return t;
        }
        public static Time GetTime(int hours, int minutes)
        {
            Time t = new Time();
            t.Hours = hours;
            t.Minutes = minutes;
            t.OnlySeconds = t.Hours * 3600 + t.Minutes * 60;
            return t;
        }
    }
    public class Lines
    {
        public float[][][,] Line { get; set; }
        public string[] Names { get; set; }
        public float[][] ManualDelays { get; set; }
        public Lines() { }
        public Lines(int lineCount, int vehiclescount, int simLength)
        {
            Names = new string[lineCount];
            Line = new float[lineCount][][,];
            for (int l = 0; l < lineCount; l++)
            {
                Line[l] = new float[vehiclescount][,];
                for (int v = 0; v < vehiclescount; v++) Line[l][v] = new float[simLength, 2];
            }
            ManualDelays = new float[vehiclescount][];
        }
    }

}
