using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mth;

namespace Vamos21
{
    public class StopTimes
    {
        public float[] StopTime { get; set; }
        public bool[] Alarm { get; set; }
        public float[] DelayToStart { get; set; }
        public StopTimes(int vehicles)
        {
            StopTime = new float[vehicles];
            Alarm = new bool[vehicles];
            DelayToStart = new float[vehicles];
        }
        public void Wait(int vehicle, float deltaT, float timeLimit)
        {
            StopTime[vehicle] += deltaT;
            Check(timeLimit, vehicle);
        }
        private void Check(float timeLimit, int vehicle)
        {
            if (StopTime[vehicle] > timeLimit)
            {
                StopTime[vehicle] = 0;
                Alarm[vehicle] = true;
            }
        }
    }
    public enum VehicleState { Travel, Breaking, BreakingToStop, Stop, FinalStop }
    public class KP
    {
        public float DeltaT { get; set; }
        public int MaximumTime { get; set; }
        public Time InitialTime { get; set; }
        public int VehiclesCount { get; set; }
        public PID PID { get; set; }
        public CalculationObjects CO { get; set; }
        public bool ElectricalCalculations { get; set; }
    }
    [Serializable]
    public partial class Przejazd06
    {
        //zawiera wszystkie wyniki [pojazdy][czas, wyniki]
        public float[][,] PT { get; set; }
        public string[][,] Info { get; set; }
        public Results R { get; set; }
        public List<Branches> Branches { get; set; }
        public KP KP { get; set; }
        public ConfigurationData cData { get; set; }
        public VehicleState[] VehiclesState { get; set; }
        public StopTimes StopTimer { get; set; }
        public Przejazd06(float deltaT, int maximumTime, Time initialTime, CalculationObjects co, ConfigurationData cdata, bool electricalCalc, bool isContinuation)
        {
            int vehiclesCount = co.Vehicles.Length;
            KP = new KP();
            KP.CO = co;
            KP.PID = cdata.PID;
            KP.DeltaT = deltaT;
            KP.MaximumTime = maximumTime;
            KP.InitialTime = initialTime;
            KP.VehiclesCount = vehiclesCount;
            KP.ElectricalCalculations = electricalCalc;
            VehiclesState = new VehicleState[vehiclesCount];
            StopTimer = new StopTimes(vehiclesCount);

            if (isContinuation == false)
            {
                PT = new float[KP.VehiclesCount][,];
                Info = new string[KP.VehiclesCount][,];
                for (int v = 0; v < KP.VehiclesCount; v++)
                {
                    PT[v] = new float[KP.MaximumTime, 20];
                    Info[v] = new string[KP.MaximumTime, 5];
                }
            }
            cData = cdata;
        }
        public Przejazd06()
        {

        }
        public void MakeStep(int i)
        {
            int vehiclesCount = KP.VehiclesCount;
            for (int v = 0; v < vehiclesCount; v++)
            {
                int delay = cData.DelayConfig[v] * 60;
                if (delay < KP.MaximumTime)
                {
                    //wykryc predkosc zadana
                    float breakDistance = 0;
                    float speedSetPoint = DefineSpeedSetPoint(i, v, ref breakDistance);
                    PT[v][i, 14] = breakDistance;
                    //obliczenia
                    Calculations(v, i, speedSetPoint);
                    //koniec obliczen
                }
            }
            if (KP.ElectricalCalculations == true) PowerCalculation(i);
        }
        public void MakeFirstStep(float[] distanceInit, float[] speedSPInit)
        {
            int vehiclesCount = KP.VehiclesCount;
            for (int v = 0; v < vehiclesCount; v++)
            {
                float P = 0;
                float I = 0;
                GetPID(ref P, ref I, v);
                PIDParameters pidparam = new PIDParameters();
                pidparam.P = P;
                pidparam.I = I;
                pidparam.D = 0;
                KP.CO.PIDParameters[v] = pidparam;
                PT[v][0, 5] = distanceInit[v];
                PT[v][0, 16] = speedSPInit[v];
                PT[v][0, 6] = PT[v][0, 16] / 3.6f;
                VehiclesState[v] = VehicleState.Travel;
                if (cData.DirectionConfig[v] == Direction.Along)
                    KP.CO.NextStopNumber[v]++;
                else
                    KP.CO.NextStopNumber[v] = KP.CO.StopsDist[v].Length - 2;
                if (KP.ElectricalCalculations == true)
                {
                    KP.CO.BranchesSup = new int[cData.Things.Count];
                }
            }
        }
        private void Calculations(int v, int i, float speedSetPoint)
        {
            int direction = 0;
            if (cData.DirectionConfig[v] == Direction.Along)
                direction = 1;
            if (cData.DirectionConfig[v] == Direction.Opposite)
                direction = -1;

            //predkosc zadana, dla regulatora
            PT[v][i, 15] = speedSetPoint;
            //czas
            PT[v][i, 0] = PT[v][i - 1, 0] + KP.DeltaT;
            if (WaitDelay(v, i) == false)
            {
                PT[v][i, 15] = 0;
                speedSetPoint = 0;
            }
            //zmiana predkosci w kroku
            if (VehiclesState[v] != VehicleState.Stop)
                PT[v][i, 1] = PT[v][i - 1, 3] * KP.DeltaT;
            //predkosc aktualna m/s
            PT[v][i, 6] = (float)Math.Round(PT[v][i - 1, 6] + PT[v][i, 1], 2);
            //ograniczona do dodatniej predkosci jesli jest ograniczenie mocy
            if (PT[v][i, 19] < 1f && PT[v][i, 6] < 0) PT[v][i, 6] = 0;
            //predkosc aktualna km/h
            PT[v][i, 16] = PT[v][i, 6] * 3.6f;
            //opory ruchu Fr
            PT[v][i, 7] = GetFr(PT[v][i, 16], v);
            //profil
            PT[v][i, 11] = GetProfile(v, PT[v][i - 1, 5], direction, PT[v][i, 6]);
            //opory od profilu Fi
            PT[v][i, 8] = (PT[v][i, 11] * (this.KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]) * 10);
            //sila Fa
            ForPID forpid = new ForPID(PT[v][i, 15], PT[v][i - 1, 15], PT[v][i, 16], PT[v][i - 1, 16], PT[v][i - 1, 2], PT[v][i - 1, 3],
                PT[v][i, 7], PT[v][i, 8]);
            PT[v][i, 2] = GetFa(forpid, v, i);
            //przyspieszenie
            PT[v][i, 3] = (PT[v][i, 2] - (PT[v][i, 7] + PT[v][i, 8])) / ((this.KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]) * 1.15F);
            //zmiana drogi w kroku
            PT[v][i, 4] = PT[v][i, 6] * this.KP.DeltaT;
            //droga pokonana
            PT[v][i, 5] = PT[v][i - 1, 5] + direction * PT[v][i, 4];
            KP.CO.DistanceVeh[v] = PT[v][i, 5];
            //zryw
            //PT[v][i, 14] = (PT[v][i, 3] - PT[v][i - 1, 3]) / this.KP.DeltaT;
            //obiekt i numer toru oraz kierunek dla dwutorowki
            GetObject(v, i);
            GetDirectionOnTrack(v, i);
            //obliczenia rozplywu pradów
            //prad pojazdu
            PT[v][i, 9] = GetCurrent(v, i);
            KP.CO.CurrentsVeh[v] = PT[v][i, 9];
            //zmiany stanu pojazdu
            CheckVehicleState(v, i, speedSetPoint, forpid);
            //stan pojazdu 0-jazda itd.
            PT[v][i, 12] = (int)VehiclesState[v];
            PT[v][i, 13] = StopTimer.StopTime[v];
            //czas bezwzgledny
            Time realTime = new Time();
            realTime = Time.GetTime((int)PT[v][i, 0] + KP.InitialTime.OnlySeconds);
            PT[v][i, 18] = KP.CO.NextStopNumber[v];
        }
        private void GetDirectionOnTrack(int v, int i)
        {
            try
            {
                if (Info[v][i - 1, 2] == Info[v][i, 2])
                {
                    float dist1 = float.Parse(Info[v][i, 3]);
                    float dist2 = float.Parse(Info[v][i - 1, 3]);
                    if (dist1 > dist2) KP.CO.DirectionOnTrack[v] = Direction.Along;
                    else KP.CO.DirectionOnTrack[v] = Direction.Opposite;
                }
            }
            catch
            {
                KP.CO.DirectionOnTrack[v] = Direction.Along;
            }
        }
        public void GetObject(int v, int i)
        {
            float dist = PT[v][i, 5];
            for (int n = 0; n < KP.CO.OnTrackEnter[v].Length; n++)
            {
                string name = KP.CO.ThingNames[v][n];
                Thing th = cData.Things.FirstOrDefault(thi => thi.Name == name);
                if (th.ThingType == ThingType.Track)
                {
                    if (dist >= KP.CO.OnTrackEnter[v][n])
                    {
                        Info[v][i, 0] = KP.CO.ThingNames[v][n];
                        float distanceOnThing = dist - KP.CO.OnTrackEnter[v][n];

                        if (KP.CO.ReverseNode[v][0][n] == true)
                            distanceOnThing = th.Length + KP.CO.OnTrackEnter[v][n] - dist;

                        Info[v][i, 1] = distanceOnThing.ToString();
                        KP.CO.DistVehOnThing[v] = distanceOnThing;

                        Info[v][i, 2] = th.LineName;
                        float pos = th.KMstart + distanceOnThing;
                        Info[v][i, 3] = pos.ToString();

                        Info[v][i, 4] = KP.CO.NextStopNumber[v].ToString();

                        int index = cData.Things.IndexOf(th);
                        if (cData.Things[index].Vehicles != null &&
                            cData.Things[index].Vehicles.Contains(v) == false)
                            cData.Things[index].Vehicles.Add(v);
                    }
                    if (dist > KP.CO.OnTrackLeave[v][n])
                    {
                        int index = cData.Things.IndexOf(th);
                        if (cData.Things[index].Vehicles.Contains(v) == true)
                            cData.Things[index].Vehicles.Remove(v);
                    }
                    if (dist <= KP.CO.OnTrackEnter[v][n])
                    {
                        int index = cData.Things.IndexOf(th);
                        if (cData.Things[index].Vehicles.Contains(v) == true)
                            cData.Things[index].Vehicles.Remove(v);
                    }
                }
            }
        }
        private void CheckVehicleState(int v, int i, float speedSetPoint, ForPID forpid)
        {
            if (KP.CO.CheckStopAllowance[v] == true)
            {
                if (cData.DirectionConfig[v] == Direction.Along)
                    KP.CO.NextStopNumber[v]++;
                else
                    KP.CO.NextStopNumber[v]--;
                KP.CO.CheckStopAllowance[v] = false;
            }
            if (speedSetPoint == 0 && PT[v][i, 16] > 0)
            {
                VehiclesState[v] = VehicleState.BreakingToStop;
            }
            if (speedSetPoint == 0 && PT[v][i, 16] <= 0 && VehiclesState[v] == VehicleState.BreakingToStop)
            {
                VehiclesState[v] = VehicleState.Stop;
                KP.CO.CheckSlowDownAllowance[v] = false;//
                float lastDec = -1 * PT[v][i - 1, 6];
                float lastFa = lastDec * (this.KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]) * 1.15f;
                PT[v][i, 2] = lastFa;
                PT[v][i, 6] = 0;
                PT[v][i, 16] = 0;
                PT[v][i, 3] = lastDec;
                PT[v][i, 1] = lastDec * KP.DeltaT;
                PT[v][i, 7] = 0;
                PT[v][i, 8] = 0;
            }
            if (VehiclesState[v] == VehicleState.Stop)
            {
                float timeLimit = 0;
                if (KP.CO.NextStopNumber[v] < KP.CO.StopsTimes[v].Length
                    && KP.CO.NextStopNumber[v] >= 0)
                    timeLimit = KP.CO.StopsTimes[v][KP.CO.NextStopNumber[v]];
                else timeLimit = 0;
                StopTimer.Wait(v, KP.DeltaT, timeLimit);
            }
            if (VehiclesState[v] == VehicleState.Breaking)
            {
                if (forpid.Speed <= forpid.SpeedSetPoint + 0.5f
                    && forpid.Speed >= forpid.SpeedSetPoint - 0.5f
                    && forpid.SpeedLast <= forpid.SpeedSetPointLast + 0.5f
                    && forpid.SpeedLast >= forpid.SpeedSetPointLast - 0.5f)
                {
                    VehiclesState[v] = VehicleState.Travel;
                    PT[v][i, 16] = forpid.SpeedSetPoint;
                    PT[v][i, 6] = forpid.SpeedSetPoint / 3.6f;
                    KP.CO.CheckSlowDownAllowance[v] = false;//
                }
            }
        }
        private float GetFr(float speed, int vehicle)
        {
            float fr = this.KP.CO.Vehicles[vehicle].CoefA
                + speed * this.KP.CO.Vehicles[vehicle].CoefB
                + (float)Math.Pow(speed, 2) * this.KP.CO.Vehicles[vehicle].CoefC;
            if (speed <= 0) fr = 0;
            return fr;
        }
        private float GetProfile(int v, float distance, float direction, float speed)
        {
            float p = 0;
            float p1 = 0;
            float p2 = 0;
            float ramp_length = 80;
            for (int i = 0; i < KP.CO.Profile1[v].GetLength(1) - 1; i++)
            {
                if (distance >= KP.CO.Profile1[v][0, i]
                    && distance < KP.CO.Profile1[v][0, i + 1])
                {
                    float[] sections = new float[2];
                    sections[0] = KP.CO.Profile1[v][0, i];
                    sections[1] = KP.CO.Profile1[v][0, i + 1];
                    float[] profiles = new float[3];
                    if (i > 0)
                        profiles[0] = KP.CO.Profile1[v][1, i - 1];
                    else
                        profiles[0] = 0;
                    profiles[1] = KP.CO.Profile1[v][1, i];
                    profiles[2] = KP.CO.Profile1[v][1, i + 1];
                    p1 = RampProfile(sections, profiles, distance, ramp_length);
                    break;
                }
            }
            for (int i = 0; i < KP.CO.Profile2[v].GetLength(1) - 1; i++)
            {
                if (distance >= KP.CO.Profile2[v][0, i]
                    && distance < KP.CO.Profile2[v][0, i + 1])
                {
                    /*p2 = KP.CO.Profile2[v][1, i];
                    float start_section = KP.CO.Profile2[v][0, i];
                    float end_section = KP.CO.Profile2[v][0, i + 1];
                    p2 = RampProfile(p2, start_section, end_section, distance, ramp_length);*/
                    float[] sections = new float[2];
                    sections[0] = KP.CO.Profile2[v][0, i];
                    sections[1] = KP.CO.Profile2[v][0, i + 1];
                    float[] profiles = new float[3];
                    if (i > 0)
                        profiles[0] = KP.CO.Profile2[v][1, i - 1];
                    else
                        profiles[0] = 0;
                    profiles[1] = KP.CO.Profile2[v][1, i];
                    profiles[2] = KP.CO.Profile2[v][1, i + 1];
                    p2 = RampProfile(sections, profiles, distance, ramp_length);
                    break;
                }
            }
            if (VehiclesState[v] != VehicleState.Stop &&
                VehiclesState[v] != VehicleState.FinalStop &&
                speed != 0)
                p = direction * p1 + p2;
            return p / 1000;
        }
        private float RampProfile(float[] sections, float[] profiles, float distance, float ramp_length)
        {
            float profile = profiles[1];

            if (distance >= sections[1] - ramp_length - 1)
                profile = (float)(Mth.LinearTwoPoints(0, profiles[1], ramp_length, (profiles[1] + profiles[2]) / 2, Math.Abs(sections[1] - distance - ramp_length)));
            if (distance <= sections[0] + ramp_length + 1)
                profile = (float)(Mth.LinearTwoPoints(0, (profiles[0] + profiles[1]) / 2, ramp_length, profiles[1], distance - sections[0]));

            return profile;
        }
        private float GetFa(ForPID forpid, int v, int i)
        {
            float fa = 0;
            if (VehiclesState[v] == VehicleState.Travel || VehiclesState[v] == VehicleState.Breaking)
            {
                float P = 0;
                float I = 0;
                try
                {
                    P = KP.CO.PIDParameters[v].P;
                    I = KP.CO.PIDParameters[v].I;
                }
                catch
                {
                    GetPID(ref P, ref I, v);
                    PIDParameters pidparam = new PIDParameters();
                    pidparam.P = P;
                    pidparam.I = I;
                    pidparam.D = 0;
                    KP.CO.PIDParameters[v] = pidparam;
                }
                //GetPID(ref P, ref I, v);
                float kP = 1000;
                float kI = 10000;
                float tI = 0.1F * this.KP.DeltaT;
                float pidDivCoeff = 1f;
                float e0 = forpid.SpeedSetPoint - forpid.Speed;
                float e1 = forpid.SpeedSetPointLast - forpid.SpeedLast;
                //poprawa stabilinosci przy stalej predkosci - dorobic tabelę
                int speedDevCoeff = 100;
                float actualSpeed = forpid.Speed * 3.6f;
                if (forpid.SpeedSetPoint * 0.95 >= forpid.Speed && forpid.SpeedSetPointLast * 0.95 >= forpid.Speed
                    && forpid.Speed > forpid.SpeedSetPoint * 0.95)
                {
                    speedDevCoeff = SpeedDeviationCoefficent(Math.Abs(forpid.SpeedSetPoint - forpid.Speed));
                }
                float deltaP = P * kP * (speedDevCoeff / 100) * (e0 - e1);
                float deltaI = (e0 + e1) * (I * kI * (speedDevCoeff / 100) * tI) / 2;
                fa = (forpid.FaLast + deltaP + deltaI) / pidDivCoeff;
                //limiter
                ForceLimiter(ref fa, forpid, v, i);
            }
            if (VehiclesState[v] == VehicleState.BreakingToStop)
            {
                fa = -1 * KP.CO.Vehicles[v].SlowDecel * (this.KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]);
            }
            if (VehiclesState[v] == VehicleState.Stop)
            {
                fa = 0;
            }
            return fa;
        }
        private float GetCurrent(int v, int i)
        {
            if (v == 11 && i >= 2890)
                v += 0;
            float voltage = PT[v][i - 1, 10];
            int filterLength = 4;
            if (i > filterLength)
            {
                float[] volts = new float[filterLength];
                volts[0] = voltage;
                volts[1] = PT[v][i - 2, 10];
                volts[2] = PT[v][i - 3, 10];
                volts[3] = PT[v][i - 4, 10];
                voltage = (volts[0] + volts[1] + volts[2] + volts[3]) / filterLength;
            }
            if (float.IsNaN(voltage) == true || voltage == 0) voltage = 3456;
            float current = 0;
            if (PT[v][i, 2] > 0)
                current = (PT[v][i, 2] / 0.95f) * PT[v][i, 6] / voltage;
            if (current <= 0) current = 0;
            if (current > 3500) current = 3500;
            return current;
        }
        private bool WaitDelay(int v, int i)
        {
            int minutes = cData.DelayConfig[v];
            int seconds = minutes * 60;
            StopTimer.DelayToStart[v] += KP.DeltaT;
            if (StopTimer.DelayToStart[v] >= seconds) return true;
            else return false;
        }
        private int SpeedDeviationCoefficent(float speedDeviation)
        {
            int coeff = 100;
            for (int i = 0; i < 6; i++)
            {
                if (speedDeviation > KP.PID.Coeff[0, i])
                {
                    coeff = (int)KP.PID.Coeff[1, i];
                }
            }
            return coeff;
        }
        private void GetPID(ref float p, ref float i, int v)
        {
            float masstmp = (this.KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]);
            float maxforcetmp = KP.CO.Vehicles[v].MaxForce;
            if (masstmp > KP.PID.P[KP.PID.P.GetLength(0) - 1, 0]) masstmp = KP.PID.P[KP.PID.P.GetLength(0) - 1, 0];
            if (maxforcetmp > KP.PID.P[0, KP.PID.P.GetLength(1) - 1]) maxforcetmp = KP.PID.P[0, KP.PID.P.GetLength(1) - 1];

            for (int f = 1; f < KP.PID.P.GetLength(1) - 1; f++)
            {
                for (int m = 1; m < KP.PID.P.GetLength(0) - 1; m++)
                {
                    float a = KP.PID.P[m, 0];
                    float b = KP.PID.P[m + 1, 0];
                    if (masstmp > KP.PID.P[m, 0] && masstmp <= KP.PID.P[m + 1, 0])
                        if (maxforcetmp > KP.PID.P[0, f] && maxforcetmp <= KP.PID.P[0, f + 1])
                        {
                            float y = (float)Mth.LinearTwoPoints(KP.PID.P[0, f], KP.PID.P[m, f], KP.PID.P[0, f + 1], KP.PID.P[m, f + 1], maxforcetmp);
                            float y2 = (float)Mth.LinearTwoPoints(KP.PID.P[0, f], KP.PID.P[m + 1, f], KP.PID.P[0, f + 1], KP.PID.P[m + 1, f + 1], maxforcetmp);
                            p = (float)Mth.LinearTwoPoints(KP.PID.P[m, 0], y, KP.PID.P[m + 1, 0], y2, masstmp);

                            y = (float)Mth.LinearTwoPoints(KP.PID.I[0, f], KP.PID.I[m, f], KP.PID.I[0, f + 1], KP.PID.I[m, f + 1], maxforcetmp);
                            y2 = (float)Mth.LinearTwoPoints(KP.PID.I[0, f], KP.PID.I[m + 1, f], KP.PID.I[0, f + 1], KP.PID.I[m + 1, f + 1], maxforcetmp);
                            i = (float)Mth.LinearTwoPoints(KP.PID.I[m, 0], y, KP.PID.I[m + 1, 0], y2, masstmp);
                        }
                }
            }
        }
        private void ForceLimiter(ref float fa, ForPID forpid, int v, int i)
        {
            float fMaxAtSpeed = 0;
            float maxSpeed = KP.CO.Vehicles[v].Force.Length - 1;
            //dla ograniczenia zrywu
            float accMax = forpid.AccelerationLast + KP.CO.Vehicles[v].JerkMax * KP.DeltaT;
            float accMin = forpid.AccelerationLast - KP.CO.Vehicles[v].JerkMax * KP.DeltaT;
            float faMax = accMax * (this.KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]) * 1.15f + forpid.FrictionAdditional + forpid.FrictionBase;
            float faMin = accMin * (this.KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]) * 1.15f + forpid.FrictionAdditional + forpid.FrictionBase;
            //dla ograniczenia przyspieszenia
            float faMaxAcc = KP.CO.Vehicles[v].AccMax * (this.KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]) * 1.15f + forpid.FrictionAdditional + forpid.FrictionBase;
            float faMinAcc = -1 * KP.CO.Vehicles[v].DecMax * (this.KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]) * 1.15f + forpid.FrictionAdditional + forpid.FrictionBase;
            //ograniczenie zrywu
            if (fa >= faMax && forpid.SpeedSetPoint > forpid.Speed) fa = faMax;
            if (fa < faMin && forpid.SpeedSetPoint < forpid.Speed) fa = faMin;
            //ograniczenie przyspieszenia
            if (fa >= faMaxAcc && forpid.SpeedSetPoint > forpid.Speed) fa = faMaxAcc;
            if (fa < faMinAcc && forpid.SpeedSetPoint < forpid.Speed) fa = faMinAcc;
            //ograniczenie mocy wedlug wykresu sily pociagowej
            if (forpid.Speed == 0) fMaxAtSpeed = KP.CO.Vehicles[v].Force[0];
            if (forpid.Speed > 0 && forpid.Speed < maxSpeed)
            {
                int speedLower = (int)Math.Truncate(forpid.Speed);
                int speedHigher = speedLower + 1;
                if (speedHigher >= 0 && speedLower >= 0)
                    fMaxAtSpeed = (float)Mth.LinearTwoPoints(speedLower, KP.CO.Vehicles[v].Force[speedLower], speedHigher, KP.CO.Vehicles[v].Force[speedHigher], forpid.Speed);
            }
            if (forpid.Speed >= maxSpeed) fMaxAtSpeed = KP.CO.Vehicles[v].Force[KP.CO.Vehicles[v].Force.Length - 1];
            //if (fa > fMaxAtSpeed) fa = fMaxAtSpeed;
            //ograniczenie podczas hamowania
            float faMinimum = -1 * (KP.CO.Vehicles[v].GrossMass + cData.TrainMassConfig[v]) * KP.CO.Vehicles[v].SlowDecel;
            if (fa < faMinimum) fa = faMinimum;
            //ograniczenie ze wzgledu na spadek napiecia
            float volt = PT[v][i - 1, 10];
            PT[v][i, 19] = 1;
            int filterTime = 4;
            if (i > filterTime)
            {
                float avgVoltage = 0;
                for (int av = 0; av < filterTime; av++)
                {
                    avgVoltage += PT[v][i - 1 - av, 10];
                }
                volt = avgVoltage / filterTime;
            }
            if (volt < 2700 && volt > 2200)
            {
                float maxPowerPerCent = 0;
                maxPowerPerCent = (float)Mth.LinearTwoPoints(2200, 0, 2700, 100, volt) / 100;
                fMaxAtSpeed *= maxPowerPerCent;
                PT[v][i, 19] = maxPowerPerCent;
            }
            if (volt <= 2200 && volt > 0) fMaxAtSpeed = 0;
            if (volt > 3600 && volt < 4000)
            {
                float maxPowerPerCent = 100;
                maxPowerPerCent = (float)Mth.LinearTwoPoints(3600, 100, 4000, 0, volt) / 100;
                fMaxAtSpeed *= maxPowerPerCent;
                PT[v][i, 19] = maxPowerPerCent;
            }
            if (fa > fMaxAtSpeed) fa = fMaxAtSpeed;
        }
        private float DefineSpeedSetPoint(int i, int v, ref float breakDistance)
        {
            if (StopTimer.Alarm[v] == true)
            {
                StopTimer.Alarm[v] = false;
                VehiclesState[v] = VehicleState.Travel;
                KP.CO.CheckStopAllowance[v] = true;
            }

            float setpoint = 0;
            float sp1 = 0;
            float sp2 = 1000;

            for (int n = 0; n < KP.CO.Limits[v].GetLength(1); n++)
            {
                if (PT[v][i - 1, 5] >= KP.CO.Limits[v][0, n]) sp1 = KP.CO.Limits[v][1, n];
            }
            //sprawdzenie czy trzeba hamowac
            float distInit = PT[v][i - 1, 5] + 0*KP.DeltaT * PT[v][i - 1, 6];
            float speedInit = PT[v][i - 1, 16];

            float speedSP = 0;
            float spLimits = 0;
            float nextSlowDownPointLimits = 0;
            float nextSlowDownPointStops = 0;
            float nextSlowDownPoint = 0;
            if (cData.DirectionConfig[v] == Direction.Along)
            {
                for (int n = 0; n < KP.CO.Limits[v].GetLength(1) - 1; n++)
                {
                    if (distInit >= KP.CO.Limits[v][0, n])
                    {
                        spLimits = KP.CO.Limits[v][1, n + 1];
                        nextSlowDownPointLimits = KP.CO.Limits[v][0, n + 1];
                    }
                }
            }
            else
            {
                for (int n = KP.CO.Limits[v].GetLength(1) - 1; n > 0; n--)
                {
                    if (distInit <= KP.CO.Limits[v][0, n])
                    {
                        spLimits = KP.CO.Limits[v][1, n - 1];
                        nextSlowDownPointLimits = KP.CO.Limits[v][0, n - 1];
                    }
                }
            }

            if (cData.DirectionConfig[v] == Direction.Along)
            {
                for (int n = 0; n < KP.CO.StopsDist[v].Length - 1; n++)
                {
                    if (distInit >= KP.CO.StopsDist[v][n])
                        nextSlowDownPointStops = KP.CO.StopsDist[v][n + 1];
                }
            }
            else
            {
                if (KP.CO.NextStopNumber[v] >= 0)
                    nextSlowDownPointStops = KP.CO.StopsDist[v][KP.CO.NextStopNumber[v]];
            }

            if (cData.DirectionConfig[v] == Direction.Along)
            {
                if (nextSlowDownPointLimits < nextSlowDownPointStops && distInit <= nextSlowDownPointLimits)
                {
                    speedSP = spLimits;
                    nextSlowDownPoint = nextSlowDownPointLimits;
                }
                else
                {
                    speedSP = 0;
                    nextSlowDownPoint = nextSlowDownPointStops;
                }
            }
            else
            {
                if (nextSlowDownPointLimits > nextSlowDownPointStops && distInit >= nextSlowDownPointLimits)
                {
                    speedSP = spLimits;
                    nextSlowDownPoint = nextSlowDownPointLimits;
                }
                else
                {
                    speedSP = 0;
                    nextSlowDownPoint = nextSlowDownPointStops;
                }
            }

            float breakingMassCoefficient = 1;
            if (KP.CO.Vehicles[v].GrossMass > 1000000)
            {
                breakingMassCoefficient = 3.25f;
                if (KP.CO.Vehicles[v].GrossMass > 3000000)
                    breakingMassCoefficient = 4.25f;
            }
            if (speedInit > 100)
                breakingMassCoefficient *= (float)1.2;
            if (speedInit > 120)
                breakingMassCoefficient *= (float)1.8;
            if (speedInit > 150)
                breakingMassCoefficient *= (float)2.0;
            if (speedInit > 180)
                breakingMassCoefficient *= (float)3.0;


            if (PT[v][i - 1, 6] != 0 &&
                ((cData.DirectionConfig[v] == Direction.Along && distInit + cData.BreakingDistance * breakingMassCoefficient >= nextSlowDownPoint)
                || (cData.DirectionConfig[v] == Direction.Opposite && distInit - cData.BreakingDistance * breakingMassCoefficient <= nextSlowDownPoint)
                    && VehiclesState[v] == VehicleState.Travel)
                    && KP.CO.CheckSlowDownAllowance[v] == false)//
            {
                int breakingTime = 400;
                if (breakingMassCoefficient > 1) breakingTime = 800;
                if (breakingMassCoefficient > 3) breakingTime = 1200;

                Przejazd06 breaking = new Przejazd06(KP.DeltaT, breakingTime, KP.InitialTime, KP.CO, cData, false, false);

                if (speedSP < speedInit)
                {
                    for (int j = 0; j < breakingTime; j++)
                    {
                        if (j == 0)
                        {
                            breaking.PT[v][0, 5] = distInit;
                            breaking.PT[v][0, 16] = speedInit;
                            breaking.PT[v][0, 6] = breaking.PT[v][0, 16] / 3.6f;
                            breaking.PT[v][0, 3] = PT[v][i - 1, 3];
                            breaking.VehiclesState[v] = VehicleState.Breaking;
                            if (speedSP == 0) breaking.VehiclesState[v] = VehicleState.BreakingToStop;//
                        }
                        else
                        {
                            breaking.Calculations(v, j, speedSP);
                            breakDistance = breaking.PT[v][j, 5];
                            if (breaking.PT[v][j, 16] <= speedSP)
                            {
                                break;
                            }
                        }
                    }
                }
                if ((cData.DirectionConfig[v] == Direction.Along && breakDistance >= nextSlowDownPoint)
                    || (cData.DirectionConfig[v] == Direction.Opposite && breakDistance <= nextSlowDownPoint))
                {
                    sp2 = speedSP;
                    VehiclesState[v] = VehicleState.Breaking;
                    KP.CO.CheckSlowDownAllowance[v] = true;//
                }
            }

            if (KP.CO.CheckSlowDownAllowance[v] == true
                && (VehiclesState[v] == VehicleState.Travel || VehiclesState[v] == VehicleState.BreakingToStop)) sp2 = speedSP;//
            //koniec warunku na hamowanie
            if (sp2 < sp1) setpoint = sp2;
            else setpoint = sp1;

            //warunek na zatrzymanie ostateczne
            float lastStop = 0;

            if (cData.DirectionConfig[v] == Direction.Along)
            {
                lastStop = KP.CO.StopsDist[v][KP.CO.StopsDist[v].Length - 1];
                if (VehiclesState[v] == VehicleState.Travel && lastStop <= distInit) VehiclesState[v] = VehicleState.FinalStop;
            }
            else
            {
                lastStop = KP.CO.StopsDist[v][0];
                if (VehiclesState[v] == VehicleState.Travel && lastStop > distInit) VehiclesState[v] = VehicleState.FinalStop;
            }

            if (KP.CO.Vehicles[v].MaxSpeed < setpoint)
                setpoint = KP.CO.Vehicles[v].MaxSpeed;

            if (VehiclesState[v] == VehicleState.BreakingToStop) setpoint = 0;
            if (VehiclesState[v] == VehicleState.Stop) setpoint = 0;
            if (VehiclesState[v] == VehicleState.FinalStop) setpoint = 0;

            return setpoint;
        }
    }
    public class ForPID
    {
        public float SpeedSetPoint { get; set; }
        public float Speed { get; set; }
        public float SpeedSetPointLast { get; set; }
        public float SpeedLast { get; set; }
        public float FaLast { get; set; }
        public float AccelerationLast { get; set; }
        public float FrictionBase { get; set; }
        public float FrictionAdditional { get; set; }
        public ForPID(float speedsetpoint, float speedsetpointlast, float speed, float speedlast, float falast, float accLast, float frictionBase, float frictionAdd)
        {
            Speed = speed;
            SpeedLast = speedlast;
            SpeedSetPoint = speedsetpoint;
            SpeedSetPointLast = speedsetpointlast;
            FaLast = falast;
            AccelerationLast = accLast;
            FrictionBase = frictionBase;
            FrictionAdditional = frictionAdd;
        }
    }
}
