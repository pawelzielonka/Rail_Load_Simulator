using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Vamos21
{
    [Serializable]
    class SystemObjects
    {
    }
    [Serializable]
    public class Vehicle
    {
        public string Name { get; set; }
        public float Length { get; set; }
        public int AxlesCount { get; set; }
        public int AxlesDriven { get; set; }
        public float MaxSpeed { get; set; }
        public float GrossMass { get; set; }
        public float FrontalArea { get; set; }
        public int Members { get; set; }
        public float AxleForce { get; set; }
        public float AxleForceManufacturer { get; set; }
        public float FastBreakMass { get; set; }
        public float SlowBreakMass { get; set; }
        public string AxlesConfig { get; set; }
        public float FastDecel { get; set; }
        public float SlowDecel { get; set; }
        public float CoefA { get; set; }
        public float CoefB { get; set; }
        public float CoefC { get; set; }
        public float JerkMax { get; set; }
        public float AccMax { get; set; }
        public float DecMax { get; set; }
        public Direction Direction { get; set; }
        public float[] Force { get; set; }
        [XmlIgnore]
        public float MaxForce { get; set; }
        [XmlIgnore]
        public float P { get; set; }
        [XmlIgnore]
        public float I { get; set; }
        public Vehicle()
        {
            this.Name = "";
            this.AxlesConfig = "";
        }

    }
    [Serializable]
    public class Route
    {
        public string Name { get; set; }
        public List<Node> Nodes { get; set; }
        public bool[][] Reverse { get; set; }
        public Route()
        {

        }
        public Route(string name)
        {
            this.Nodes = new List<Node>();
            this.Name = name;
        }
    }
    [Serializable]
    public class Node
    {
        public string Name { get; set; }
        public int WingIn { get; set; }
        public int WingOut { get; set; }
        public Thing ThingIn { get; set; }
        public Thing ThingOut { get; set; }
        public Node()
        {

        }
        public Node(Thing inputLeft, int supplyin, Thing outputRight, int supplyout, string name)
        {
            ThingIn = inputLeft;
            WingIn = supplyin;
            ThingOut = outputRight;
            WingOut = supplyout;
            Name = name;
        }

    }
    [Serializable]
    public class ElectricalData
    {
        public float UA { get; set; }
        public float UB { get; set; }
        public float RWewA { get; set; }
        public float RWewB { get; set; }
        public float RZA1 { get; set; }
        public float RZA2 { get; set; }
        public float RZB1 { get; set; }
        public float RZB2 { get; set; }
        public float RPA { get; set; }
        public float RPB { get; set; }
        public float RS { get; set; }
        public float RT { get; set; }
        public float RK1 { get; set; }
        public float RK2 { get; set; }
        public float RK3 { get; set; }
        public float RK4 { get; set; }
        public float[] RZ { get; set; }
        public ElectricalData()
        {
            RZ = new float[12];
        }
    }
    [Serializable]
    public enum ThingType { Supply, Station, Track, Cabin, Junction, Separator, Junction1, Junction2, Junction12, Junction22 }
    [Serializable]
    public enum Direction { Along, Opposite }
    [Serializable]
    public class Thing
    {
        public string Name { get; set; }
        public ThingType ThingType { get; set; }
        public int RailCount { get; set; }
        public int SupplyCount { get; set; }
        public int WingCount { get; set; }
        public string LineName { get; set; }
        public float KMstart { get; set; }
        [XmlIgnore]
        public List<int> Vehicles { get; set; }
        public float Length { get; set; }
        public string ProfileName { get; set; }
        [XmlIgnore]
        public Profile Profile { get; set; }
        public string[] ThingsAtWings { get; set; }
        public ElectricalData ElecData { get; set; }

        public Thing(string name, int rails, ThingType thingtype, float length)
        {
            this.Name = name;
            this.ThingType = thingtype;
            this.RailCount = rails;
            this.Length = length;
            this.Vehicles = new List<int>();
            this.ElecData = new ElectricalData();
            this.ThingsAtWings = new string[16];
        }
        public Thing()
        {
            this.Vehicles = new List<int>();
        }
    }

    [Serializable]
    public class PID
    {
        public float[,] P { get; set; }
        public float[,] I { get; set; }
        public float[,] D { get; set; }
        public float[,] Coeff { get; set; }
    }
    [Serializable]
    public class Profile
    {
        public string Name { get; set; }
        public float[,] Profile1 { get; set; }
        public float[,] Profile2 { get; set; }
        public float[,] Limits { get; set; }
        public Station[] Stations { get; set; }
        public PowerObject[] PowerObject { get; set; }
        public ObjectType[] ObjectType { get; set; }
        public Track[] Tracks { get; set; }
    }
    [Serializable]
    public class Station
    {
        public string Name { get; set; }
        public float Position { get; set; }
        public int Index { get; set; }
        public Station()
        {
            string n = "";
            Name = n;
        }
    }
    [Serializable]
    public class Track
    {
        public string Name { get; set; }
        public string ProfileName { get; set; }
        public float Position { get; set; }
        public float Length { get; set; }
        public float RT { get; set; }
        public float RS { get; set; }
        public int RailCount { get; set; }
        public int Index { get; set; }
        public Track()
        {
            string n = "";
            Name = n;
            ProfileName = n;

        }
    }
    [Serializable]
    public class PowerObject
    {
        public string Name { get; set; }
        public float Position { get; set; }
        public int Index { get; set; }
        public PowerObjectType Type { get; set; }
        public ElectricalData Elec { get; set; }
        public int RailCount { get; set; }
        public int SupplyCount { get; set; }
        public int WingCount { get; set; }
        public PowerObject()
        {
            Elec = new ElectricalData();
        }
    }
    [Serializable]
    public enum PowerObjectType { PowerStation, Junction, Separator, Cabin, Junction1, Junction2, Junction12, Junction22 }
    [Serializable]
    public enum ObjectType { Station, PowerObject }
}
