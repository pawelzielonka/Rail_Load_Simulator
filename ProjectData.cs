using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Vamos21
{
    [Serializable]
    public class ProjectData
    {
        public string Name { get; set; }
        public string PathVehicles { get; set; }
        public string PathPID { get; set; }
        public string PathProfiles { get; set; }
        public string PathPowerSystems { get; set; }
        public float DeltaT { get; set; }
        public Time SimTime { get; set; }
        public Time InitialTime { get; set; }
        public float BreakingDistance { get; set; }
        public ConfigurationData ConfigurationData { get; set; }
        public ProjectData()
        {
            ConfigurationData = new ConfigurationData();
            SimTime = new Time();
        }
    }
    [Serializable]
    public class ConfigurationData
    {
        public List<bool> IsCheckedConfig { get; set; }
        public List<string> RouteConfig { get; set; }
        public List<string> VehicleConfig { get; set; }
        public List<Direction> DirectionConfig { get; set; }
        public List<float> TrainMassConfig { get; set; }
        public List<int> DelayConfig { get; set; }
        public List<string> InfoConfig { get; set; }
        public List<int> RoundCountConfig { get; set; }
        public List<StopsConfiguration> StopConfig { get; set; }
        public List<Thing> Things { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Route> Routes { get; set; }
        [XmlIgnore]
        public List<Vehicle> Vehicles { get; set; }
        [XmlIgnore]
        public PID PID { get; set; }
        [XmlIgnore]
        public List<Profile> Profiles { get; set; }
        [XmlIgnore]
        public float BreakingDistance { get; set; }


        public ConfigurationData()
        {
            IsCheckedConfig = new List<bool>();
            RouteConfig = new List<string>();
            VehicleConfig = new List<string>();
            DirectionConfig = new List<Direction>();
            DelayConfig = new List<int>();
            InfoConfig = new List<string>();
            RoundCountConfig = new List<int>();
            StopConfig = new List<StopsConfiguration>();

            Things = new List<Thing>();
            Nodes = new List<Node>();
            Routes = new List<Route>();
            Vehicles = new List<Vehicle>();
            PID = new PID();
            Profiles = new List<Profile>();
        }
    }
    [Serializable]
    public class StopsConfiguration
    {
        public string[] StopsNames { get; set; }
        public bool[] StopsChecked { get; set; }
        public int[] StopsTimes { get; set; }
        public StopsConfiguration()
        {

        }
        public StopsConfiguration(int stopCount)
        {
            StopsNames = new string[stopCount];
            StopsChecked = new bool[stopCount];
            StopsTimes = new int[stopCount];
        }
    }
}
