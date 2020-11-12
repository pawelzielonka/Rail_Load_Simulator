using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mth;

namespace Vamos21
{
    [Serializable]
    public class FirstLastSupply
    {
        public bool[] FirstSupply { get; set; }
        public bool[] LastSupply { get; set; }
        public bool[] SeparatesOnLeft { get; set; }
        public bool[] SeparatesOnRight { get; set; }
    }
    [Serializable]
    public class PowerInfos
    {
        public ConfigurationData CD { get; set; }
        public CalculationObjects CO { get; set; }
        public NodeInput NI { get; set; }
        public int[] StartNodes { get; set; }
        public int[] StartBranches { get; set; }
        public bool[] Connected { get; set; }
        public FirstLastSupply FLS { get; set; }
        public int NodesCount { get; set; }
        public int BranchesCount { get; set; }
        public List<Branches> Branches { get; set; }
        public PowerInfos(ConfigurationData cData, CalculationObjects cObj)
        {
            CD = cData;
            CO = cObj;
            StartBranches = new int[CD.Things.Count];
            StartNodes = new int[CD.Things.Count];
            Connected = new bool[CD.Things.Count];
            FLS = new FirstLastSupply();
            FLS.FirstSupply = new bool[CD.Things.Count];
            FLS.LastSupply = new bool[CD.Things.Count];
            FLS.SeparatesOnLeft = new bool[CD.Things.Count];
            FLS.SeparatesOnRight = new bool[CD.Things.Count];
            for (int i = 0; i < CD.Things.Count; i++)
            {
                if (CD.Things[i].ThingType == ThingType.Supply)
                {
                    FLS.FirstSupply[i] = true;
                    FLS.LastSupply[i] = true;
                }
            }
        }
    }
    [Serializable]
    class PowerCalculations
    {
        public static void AddPowerThings(ref PowerInfos powerInfos, int i, float[][,] PT)
        {
            for (int t = 0; t < powerInfos.Connected.Length; t++) powerInfos.Connected = new bool[powerInfos.Connected.Length];

            CheckFirstLastSeparator(ref powerInfos);

            for (int n = 0; n < powerInfos.CD.Nodes.Count; n++)
            {
                Thing th = powerInfos.CD.Nodes[n].ThingIn;
                int t = IndexOfThing(th, powerInfos);
                th = powerInfos.CD.Things[t];
                if (t >= 0) DetermineBasic(ref powerInfos, th, t, n, true);

                th = powerInfos.CD.Nodes[n].ThingOut;
                t = IndexOfThing(th, powerInfos);
                th = powerInfos.CD.Things[t];
                if (t >= 0) DetermineBasic(ref powerInfos, th, t, n, false);
            }

            powerInfos.NI = new NodeInput(powerInfos.NodesCount - 1, powerInfos.BranchesCount);

            for (int t = 0; t < powerInfos.Connected.Length; t++) powerInfos.Connected = new bool[powerInfos.Connected.Length];

            for (int n = 0; n < powerInfos.CD.Nodes.Count; n++)
            {
                if (n == 11)
                    n += 0;
                Thing th = new Thing();
                int t = 0;
                string name = powerInfos.CD.Nodes[n].ThingIn.Name;
                for (int a = 0; a < powerInfos.CD.Things.Count; a++)
                {
                    if (powerInfos.CD.Things[a].Name == name)
                    {
                        th = powerInfos.CD.Things[a];
                        t = a;
                    }
                }
                if (t >= 0) CreatePowerCircuit(ref powerInfos, th, t, n, true);

                name = powerInfos.CD.Nodes[n].ThingOut.Name;
                for (int a = 0; a < powerInfos.CD.Things.Count; a++)
                {
                    if (powerInfos.CD.Things[a].Name == name)
                    {
                        th = powerInfos.CD.Things[a];
                        t = a;
                    }
                }
                if (t >= 0) CreatePowerCircuit(ref powerInfos, th, t, n, false);
            }

        }
        private static void CheckFirstLastSeparator(ref PowerInfos powerInfos)
        {
            for (int n = 0; n < powerInfos.CD.Nodes.Count; n++)
            {
                for (int m = 0; m < powerInfos.CD.Nodes.Count; m++)
                {
                    if (n != m)
                    {
                        Thing t1 = powerInfos.CD.Nodes[n].ThingIn;
                        Thing t2 = powerInfos.CD.Nodes[n].ThingOut;
                        int index1 = IndexOfThing(t1, powerInfos);
                        int index2 = IndexOfThing(t2, powerInfos);

                        Thing t1bis = powerInfos.CD.Nodes[m].ThingIn;
                        Thing t2bis = powerInfos.CD.Nodes[m].ThingOut;
                        if (t1.ThingType == ThingType.Supply)
                            index1 += 0;

                        if (t1.ThingType == ThingType.Supply)
                        {
                            if (t1bis.ThingType == ThingType.Cabin ||
                                t1bis.ThingType == ThingType.Track ||
                                t1bis.ThingType == ThingType.Separator ||
                                t1bis.ThingType == ThingType.Station)
                                powerInfos.FLS.LastSupply[index1] = false;
                        }
                        if (t2.ThingType == ThingType.Supply)
                        {
                            if (t2bis.ThingType == ThingType.Cabin ||
                                t2bis.ThingType == ThingType.Track ||
                                t1bis.ThingType == ThingType.Separator ||
                                t1bis.ThingType == ThingType.Station)
                                powerInfos.FLS.FirstSupply[index2] = false;
                        }
                        if (t1.ThingType == ThingType.Separator) powerInfos.FLS.SeparatesOnLeft[index1] = true;
                        if (t2.ThingType == ThingType.Separator) powerInfos.FLS.SeparatesOnRight[index2] = true;

                    }
                }
            }
        }
        private static void DetermineBasic(ref PowerInfos powerInfos, Thing th, int t, int n, bool leftInThing)
        {
            int nodes = powerInfos.NodesCount;
            int branches = powerInfos.BranchesCount;

            if (powerInfos.Connected[t] == false)
            {
                Thing thNext = new Thing();
                Thing thPrev = new Thing();
                int tPrev = -1;
                int tNext = -1;
                if (leftInThing == true)
                {
                    thNext = powerInfos.CD.Nodes[n].ThingOut;
                    tNext = IndexOfThing(thNext, powerInfos);
                    for (int a = 0; a < powerInfos.CD.Nodes.Count; a++)
                    {
                        if (th.Name == powerInfos.CD.Nodes[a].ThingOut.Name) tPrev = IndexOfThing(powerInfos.CD.Nodes[a].ThingIn, powerInfos);
                    }
                }
                else
                {
                    thPrev = powerInfos.CD.Nodes[n].ThingIn;
                    tPrev = IndexOfThing(thPrev, powerInfos);
                    for (int a = 0; a < powerInfos.CD.Nodes.Count; a++)
                    {
                        if (th.Name == powerInfos.CD.Nodes[a].ThingIn.Name) tNext = IndexOfThing(powerInfos.CD.Nodes[a].ThingOut, powerInfos);
                    }
                }
                if (tPrev >= 0) thPrev = powerInfos.CD.Things[tPrev];
                if (tNext >= 0) thNext = powerInfos.CD.Things[tNext];

                if (t == 4 || t == 96)
                    t += 0;
                if (tNext >= 0 && false)
                    if (powerInfos.StartBranches[tNext] == 0 && powerInfos.StartNodes[tNext] == 0 && t != 0)
                    {
                        powerInfos.StartBranches[tNext] = powerInfos.BranchesCount;
                        powerInfos.StartNodes[tNext] = powerInfos.NodesCount;
                    }

                if (th.ThingType == ThingType.Cabin ||
                    th.ThingType == ThingType.Track ||
                    th.ThingType == ThingType.Junction ||
                    th.ThingType == ThingType.Junction1 ||
                    th.ThingType == ThingType.Junction2 ||
                    th.ThingType == ThingType.Junction12 ||
                    th.ThingType == ThingType.Junction22 ||
                    th.ThingType == ThingType.Separator)
                {
                    if (t == 4 || t == 96)
                        t += 0;
                    if (powerInfos.StartBranches[t] == 0)
                        powerInfos.StartBranches[t] = powerInfos.BranchesCount;
                    if (powerInfos.StartNodes[t] == 0)
                        powerInfos.StartNodes[t] = powerInfos.NodesCount;
                }
                else
                {
                    if (th.ThingType == ThingType.Supply)
                    {
                        powerInfos.StartBranches[t] = powerInfos.BranchesCount;
                        powerInfos.StartNodes[t] = powerInfos.NodesCount;
                        //if (powerInfos.FLS.FirstSupply[t] != true)
                        //    powerInfos.StartNodes[t] = powerInfos.StartNodes[tPrev];
                        //else
                        //    powerInfos.StartNodes[t] = powerInfos.NodesCount;
                    }
                    else
                    {
                        powerInfos.StartBranches[t] = powerInfos.StartBranches[tPrev];
                        powerInfos.StartNodes[t] = powerInfos.StartNodes[tPrev];
                    }
                }

                if (th.ThingType == ThingType.Supply)
                {
                    branches += 1 + th.SupplyCount;
                    nodes += 2 + th.SupplyCount;
                    for (int w = 0; w < powerInfos.CD.Nodes.Count; w++)
                        if (powerInfos.CD.Nodes[w].ThingOut.Name == th.Name)
                        {
                            branches += 1;
                            nodes -= powerInfos.CD.Nodes[w].ThingIn.RailCount;
                        }
                    //if (powerInfos.FLS.FirstSupply[t] == true)
                    //    branches -= 1;
                }
                if (th.ThingType == ThingType.Cabin)
                {
                    if (th.RailCount == 2)
                    {
                        branches += 5;
                        nodes += 4;
                    }
                    if (th.RailCount == 1)
                    {
                        branches += 2;
                        nodes += 2;
                    }
                }
                if (th.ThingType == ThingType.Track)
                {
                    if (th.Name == "Szlak st. Kowalewo Pomorskie PT Kowalewo")
                        t += 0;
                    branches += 1 + th.RailCount;
                    nodes += 1 + th.RailCount;
                    if (th.Vehicles != null && th.Vehicles.Count != 0)
                        for (int v = 0; v < th.Vehicles.Count; v++)
                        {
                            if (powerInfos.CO.DistVehOnThing[v] >= 0)
                            {
                                branches += 2;
                                nodes += 1;
                                for (int r = 0; r < th.RailCount; r++)
                                {
                                    branches += 1;
                                    nodes += 1;
                                }
                            }
                        }
                }
                if (th.ThingType == ThingType.Junction || th.ThingType == ThingType.Junction12 || th.ThingType == ThingType.Junction22)
                {
                    nodes += 1 + th.RailCount;
                    for (int w = 0; w < powerInfos.CD.Nodes.Count; w++)
                    {
                        string name = powerInfos.CD.Nodes[w].ThingOut.Name;
                        if (name == th.Name && name != thPrev.Name)
                            branches += 1 + thPrev.RailCount;
                    }
                }
                if (th.ThingType == ThingType.Junction1 || th.ThingType == ThingType.Junction2)
                {
                    nodes += 1 + th.RailCount;
                    for (int w = 0; w < powerInfos.CD.Nodes.Count; w++)
                    {
                        string name = powerInfos.CD.Nodes[w].ThingOut.Name;
                        if (name == th.Name && name != thPrev.Name)
                            branches += 1 + thPrev.RailCount;
                    }
                }
                if (th.ThingType == ThingType.Separator)
                {
                    if ((powerInfos.FLS.SeparatesOnLeft[t] == true && powerInfos.FLS.SeparatesOnRight[t] == false) ||
                        (powerInfos.FLS.SeparatesOnRight[t] == true && powerInfos.FLS.SeparatesOnLeft[t] == false))
                    {
                        branches += thNext.RailCount + thPrev.RailCount;
                        //if (powerInfos.FLS.SeparatesOnRight[t] == true)
                        //    nodes += thPrev.RailCount + 1;
                        if (powerInfos.FLS.SeparatesOnLeft[t] == true)
                            nodes += thNext.RailCount + 1;
                    }
                    if (powerInfos.FLS.SeparatesOnLeft[t] == true &&
                        powerInfos.FLS.SeparatesOnRight[t] == true)
                    {
                        branches += thNext.RailCount + thPrev.RailCount;
                        nodes += thNext.RailCount + 1;
                    }
                    if(th.RailCount > 1 && th.ElecData.RPA < 1)
                    {
                        if (thPrev.Name != null)
                            branches += 1;
                        if (thNext.Name != null)
                            branches += 1;
                    }
                }

                powerInfos.Connected[t] = true;
            }

            powerInfos.BranchesCount = branches;
            powerInfos.NodesCount = nodes;
        }
        private static void CreatePowerCircuit(ref PowerInfos powerInfos, Thing th, int t, int n, bool leftInThing)
        {
            float rMin = 0.000001f;
            float rHigh = 100000000000f;
            int thCount = powerInfos.CD.Things.Count;
            if (powerInfos.CO.BranchesSup == null) powerInfos.CO.BranchesSup = new int[thCount];

            if (powerInfos.Connected[t] == false)
            {
                Thing thNext = new Thing();
                Thing thPrev = new Thing();
                int tPrev = -1;
                int tNext = -1;
                if (leftInThing == true)
                {
                    thNext = powerInfos.CD.Nodes[n].ThingOut;
                    tNext = IndexOfThing(thNext, powerInfos);
                    for (int a = 0; a < powerInfos.CD.Nodes.Count; a++)
                    {
                        if (th.Name == powerInfos.CD.Nodes[a].ThingOut.Name) tPrev = IndexOfThing(powerInfos.CD.Nodes[a].ThingIn, powerInfos);
                    }
                }
                else
                {
                    thPrev = powerInfos.CD.Nodes[n].ThingIn;
                    tPrev = IndexOfThing(thPrev, powerInfos);
                    for (int a = 0; a < powerInfos.CD.Nodes.Count; a++)
                    {
                        if (th.Name == powerInfos.CD.Nodes[a].ThingIn.Name) tNext = IndexOfThing(powerInfos.CD.Nodes[a].ThingOut, powerInfos);
                    }
                }
                if (tPrev >= 0) thPrev = powerInfos.CD.Things[tPrev];
                if (tNext >= 0) thNext = powerInfos.CD.Things[tNext];

                int startB = powerInfos.StartBranches[t];
                int startN = powerInfos.StartNodes[t];
                int startNPrev = 0;
                int startNNext = 0;
                if (tPrev >= 0) startNPrev = powerInfos.StartNodes[tPrev];
                if (tNext >= 0) startNNext = powerInfos.StartNodes[tNext];

                if (th.ThingType == ThingType.Junction1 || th.ThingType == ThingType.Junction2)
                    t += 0;

                int startNodeWingPrev = 0;
                int startNodeWingNext = 0;//
                if (thPrev.ThingsAtWings != null && thPrev.ThingType == ThingType.Supply)
                {
                    bool enough = false;
                    startNodeWingPrev += 1;
                    for (int i = 0; i < thPrev.ThingsAtWings.Length; i++)
                    {
                        if (thPrev.ThingsAtWings[i] != null)
                        {
                            Thing thing = new Thing();
                            for (int a = 0; a < powerInfos.CD.Things.Count; a++)
                            {
                                if (powerInfos.CD.Things[a].Name == th.Name) enough = true;
                                if (powerInfos.CD.Things[a].Name == thPrev.ThingsAtWings[i] && enough == false)
                                {
                                    thing = powerInfos.CD.Things[a];
                                    startNodeWingPrev += thing.RailCount;
                                    break;
                                }
                            }
                        }
                    }
                }                    
                //
                if (thNext.ThingsAtWings != null && thNext.ThingType == ThingType.Supply)
                {
                    startNodeWingNext += 1;
                    bool enough = false;
                    for (int i = 0; i < thNext.ThingsAtWings.Length; i++)
                    {
                        if (thNext.ThingsAtWings[i] != null)
                        {
                            Thing thing = new Thing();
                            for (int a = 0; a < powerInfos.CD.Things.Count; a++)
                            {
                                if (powerInfos.CD.Things[a].Name == th.Name) enough = true;
                                if (powerInfos.CD.Things[a].Name == thNext.ThingsAtWings[i] && enough == false)
                                {
                                    thing = powerInfos.CD.Things[a];
                                    startNodeWingNext += thing.RailCount;
                                    enough = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                //

                int[] vehiclesOrder = new int[0];
                float[] vehiclesDistance = new float[0];
                if (th.Vehicles != null && th.Vehicles.Count != 0)
                {
                    vehiclesOrder = new int[th.Vehicles.Count];
                    vehiclesDistance = new float[vehiclesOrder.Length];
                    for (int v = 0; v < th.Vehicles.Count; v++)
                    {
                        int vehInd = th.Vehicles[v];
                        vehiclesOrder[v] = vehInd;
                        float dst = powerInfos.CO.DistVehOnThing[vehInd];
                        vehiclesDistance[v] = dst;
                    }
                    for (int a = 0; a < th.Vehicles.Count; a++)
                    {
                        for (int v = 0; v < th.Vehicles.Count - 1; v++)
                        {
                            if (vehiclesDistance[v + 1] < vehiclesDistance[v])
                            {
                                float dstTmp = vehiclesDistance[v + 1];
                                int indTmp = vehiclesOrder[v + 1];
                                vehiclesDistance[v + 1] = vehiclesDistance[v];
                                vehiclesOrder[v + 1] = vehiclesOrder[v];
                                vehiclesDistance[v] = dstTmp;
                                vehiclesOrder[v] = indTmp;
                            }
                        }
                    }
                }

                if (th.ThingType == ThingType.Supply)
                {
                    powerInfos.CO.BranchesSup[t] = startB;
                    int brStart = startB;
                    int z = 0;
                    float rZ = th.ElecData.RZA1;
                    float[] rz = th.ElecData.RZ;
                    float u = th.ElecData.UA;

                    Mth.AddBranch(brStart + 0, startN + 1, startN + 0, th.ElecData.RWewA + th.ElecData.RPA, u, 0, powerInfos.NI);
                    AddBranch(brStart + 0, startN + 1, startN + 0, th.ElecData.RWewA + th.ElecData.RPA, u, 0, powerInfos.NI);
                    brStart++;

                    for(int w = 0; w < powerInfos.CD.Nodes.Count; w++)
                    {
                        if (powerInfos.CD.Nodes[w].ThingOut.Name == th.Name)
                        {
                            int ind = IndexOfThing(powerInfos.CD.Nodes[w].ThingIn, powerInfos);
                            int stNBefore = powerInfos.StartNodes[ind];

                            Mth.AddBranch(brStart, startN + 0, stNBefore + 0, rMin, 0, 0, powerInfos.NI);
                            AddBranch(brStart, startN + 0, stNBefore + 0, rMin, 0, 0, powerInfos.NI);
                            brStart++;

                            for(int i = 1; i <= powerInfos.CD.Nodes[w].ThingIn.RailCount; i++)
                            {
                                int wingNumber = 0;
                                for (int wing = 0; wing < th.ThingsAtWings.Length; wing++) if (th.ThingsAtWings[wing] == powerInfos.CD.Nodes[w].ThingIn.Name) wingNumber = wing;

                                Mth.AddBranch(brStart, stNBefore + i, startN + 1, rz[z], 0, 0, powerInfos.NI);
                                AddBranch(brStart, stNBefore + i, startN + 1, rz[z], 0, 0, powerInfos.NI);
                                brStart++;
                                z++;
                            }
                        }
                    }
                    if (powerInfos.FLS.FirstSupply[t] == true)
                    {
                        for(int i = 1; i <= thNext.RailCount; i++)
                        {
                            Mth.AddBranch(brStart, startN + i + 1, startN + 1, rz[z], 0, 0, powerInfos.NI);
                            AddBranch(brStart, startN + i + 1, startN + 1, rz[z], 0, 0, powerInfos.NI);
                            brStart++;
                            z++;
                        }
                    }
                    int zSoFar = z;
                    if (powerInfos.FLS.FirstSupply[t] != true)
                        for (int i = 1; i <= th.SupplyCount - zSoFar; i++)
                        {
                            Mth.AddBranch(brStart, startN + i + 1, startN + 1, rz[z], 0, 0, powerInfos.NI);
                            AddBranch(brStart, startN + i + 1, startN + 1, rz[z], 0, 0, powerInfos.NI);
                            brStart++;
                            z++;
                        }
                }
                if (th.ThingType == ThingType.Cabin)
                {
                    if (th.RailCount == 2)
                    {
                        Mth.AddBranch(startB + 1, startN + 3, startN + 1, th.ElecData.RK3, 0, 0, powerInfos.NI);
                        Mth.AddBranch(startB + 2, startN + 3, startN + 2, th.ElecData.RK4, 0, 0, powerInfos.NI);
                        Mth.AddBranch(startB + 3, startN + 3, startNPrev + 1, th.ElecData.RK1, 0, 0, powerInfos.NI);
                        Mth.AddBranch(startB + 4, startN + 3, startNPrev + 2, th.ElecData.RK2, 0, 0, powerInfos.NI);
                        Mth.AddBranch(startB + 0, startN + 0, startNPrev + 0, rMin, 0, 0, powerInfos.NI);

                        AddBranch(startB + 1, startN + 3, startN + 1, th.ElecData.RK3, 0, 0, powerInfos.NI);
                        AddBranch(startB + 2, startN + 3, startN + 2, th.ElecData.RK4, 0, 0, powerInfos.NI);
                        AddBranch(startB + 3, startN + 3, startNPrev + 1, th.ElecData.RK1, 0, 0, powerInfos.NI);
                        AddBranch(startB + 4, startN + 3, startNPrev + 2, th.ElecData.RK2, 0, 0, powerInfos.NI);
                        AddBranch(startB + 0, startN + 0, startNPrev + 0, rMin, 0, 0, powerInfos.NI);
                    }
                    if (th.RailCount == 1)
                    {
                        Mth.AddBranch(startB + 0, startNPrev + 0, startN + 0, rMin, 0, 0, powerInfos.NI);
                        Mth.AddBranch(startB + 1, startN + 1, startNPrev + 1, rMin, 0, 0, powerInfos.NI);

                        AddBranch(startB + 0, startNPrev + 0, startN + 0, rMin, 0, 0, powerInfos.NI);
                        AddBranch(startB + 1, startN + 1, startNPrev + 1, rMin, 0, 0, powerInfos.NI);
                    }
                }
                if (th.ThingType == ThingType.Track)
                {
                    int railCount = th.RailCount;

                    int zComp = 0;
                    if (thPrev.ThingType == ThingType.Supply)
                    {
                        zComp++;
                        for(int w = 0; w < thPrev.ThingsAtWings.Length; w++)
                        {
                            if (thPrev.ThingsAtWings[w] != null)
                            {
                                string wingName = thPrev.ThingsAtWings[w];
                                if (wingName == th.Name)
                                    break;
                                for (int nodes = 0; nodes < powerInfos.CD.Nodes.Count; nodes++)
                                {
                                    if (wingName == powerInfos.CD.Nodes[nodes].ThingOut.Name &&
                                        powerInfos.CD.Nodes[nodes].ThingIn.Name == thPrev.Name)
                                    {
                                        zComp += powerInfos.CD.Nodes[nodes].ThingOut.RailCount;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (th.Vehicles == null || th.Vehicles.Count == 0)
                    {
                        float distance = th.Length / 1000;
                        Mth.AddBranch(startB + 0, startNPrev + 0, startN + 0, th.ElecData.RT * distance, 0, 0, powerInfos.NI);
                        AddBranch(startB + 0, startNPrev + 0, startN + 0, th.ElecData.RT * distance, 0, 0, powerInfos.NI);
                        for (int i = 1; i <= railCount; i++)
                        {
                            float resistance = 0;
                            resistance += th.ElecData.RS * distance;

                            Mth.AddBranch(startB + i, startN + i, startNPrev + i + zComp, resistance, 0, 0, powerInfos.NI);
                            AddBranch(startB + i, startN + i, startNPrev + i + zComp, resistance, 0, 0, powerInfos.NI);
                        }
                    }
                    else
                    {

                        int br = 0;
                        int nod = 0;
                        float sectionLength = 0;
                        for (int v = 0; v < vehiclesDistance.Length; v++)
                        {
                            for (int v_comp = v + 1; v_comp < vehiclesDistance.Length; v_comp++)
                            {
                                if (vehiclesDistance[v] == vehiclesDistance[v_comp])
                                    vehiclesDistance[v_comp] += 1f;
                            }
                        }
                        for (int v = vehiclesOrder.Length - 1; v >= 0; v--)
                        {
                            if (vehiclesDistance[v] == 0) vehiclesDistance[v] = (v + 1) / 100f;
                            if (vehiclesDistance[v] == th.Length) vehiclesDistance[v] = th.Length - (v + 1) / 100f;
                            int counter = 0;
                            int vehInd = vehiclesOrder[v];
                            if (v == vehiclesOrder.Length - 1)
                            {
                                sectionLength = th.Length - vehiclesDistance[v];
                            }
                            else
                            {
                                sectionLength = vehiclesDistance[v + 1] - vehiclesDistance[v];
                            }
                            if (sectionLength == 0) sectionLength = v / 100;
                            sectionLength /= 1000;
                            Mth.AddBranch(startB + br + 0, startN + nod + railCount + 1, startN + nod + 0, sectionLength * th.ElecData.RT, 0, 0, powerInfos.NI);
                            AddBranch(startB + br + 0, startN + nod + railCount + 1, startN + nod + 0, sectionLength * th.ElecData.RT, 0, 0, powerInfos.NI);
                            counter++;

                            for (int i = 0; i < th.RailCount; i++)
                            {
                                float resistance = 0;
                                resistance += sectionLength * th.ElecData.RS;

                                Mth.AddBranch(startB + br + 1 + i, startN + nod + railCount + 2 + i, startN + nod + 1 + i, resistance, 0, 0, powerInfos.NI);
                                AddBranch(startB + br + 1 + i, startN + nod + railCount + 2 + i, startN + nod + 1 + i, resistance, 0, 0, powerInfos.NI);
                                counter++;
                            }

                            float current = -1 * powerInfos.CO.CurrentsVeh[vehInd];
                            if (float.IsNaN(current))
                                current = 0;
                            int drc = 1;
                            if (th.RailCount == 2 && powerInfos.CO.DirectionOnTrack[v] == Direction.Opposite)
                                drc = 2;

                            int bn = startB + br + th.RailCount + 1;
                            Mth.AddBranch(bn, startN + nod + th.RailCount + 1, startN + nod + railCount + 1 + drc, 0, 0, current, powerInfos.NI);
                            AddBranch(bn, startN + nod + th.RailCount + 1, startN + nod + railCount + 1 + drc, 0, 0, current, powerInfos.NI);
                            counter++;

                            powerInfos.CO.BranchesVeh[vehInd] = bn;

                            br += counter;
                            nod += th.RailCount + 1;
                        }

                        sectionLength = vehiclesDistance[0] / 1000;
                        if (sectionLength == 0) sectionLength = (1) / 100f;
                        Mth.AddBranch(startB + br, startNPrev + 0, startN + nod + 0, sectionLength * th.ElecData.RT, 0, 0, powerInfos.NI);
                        AddBranch(startB + br, startNPrev + 0, startN + nod + 0, sectionLength * th.ElecData.RT, 0, 0, powerInfos.NI);

                        for (int i = 0; i < railCount; i++)
                        {
                            float resistance = 0;
                            resistance += sectionLength * th.ElecData.RS;

                            Mth.AddBranch(startB + br + 1 + i, startN + nod + 1 + i, startNPrev + i + 1 + zComp, resistance, 0, 0, powerInfos.NI);
                            AddBranch(startB + br + 1 + i, startN + nod + 1 + i, startNPrev + startNodeWingPrev + i + 1 + zComp, resistance, 0, 0, powerInfos.NI);
                        }
                    }
                }
                if (th.ThingType == ThingType.Junction)
                {
                    powerInfos.CO.BranchesSup[t] = startB;
                    int brStart = startB;

                    Mth.AddBranch(brStart, startN + 0, startNPrev + 0, rMin, 0, 0, powerInfos.NI);
                    AddBranch(brStart, startN + 0, startNPrev + 0, rMin, 0, 0, powerInfos.NI);
                    brStart++;

                    for (int i = 1; i <= thPrev.RailCount; i++)
                    {
                        Mth.AddBranch(brStart, startN + i, startNPrev + i, rMin, 0, 0, powerInfos.NI);
                        AddBranch(brStart, startN + i, startNPrev + i, rMin, 0, 0, powerInfos.NI);
                        brStart++;
                    }

                    for (int nodes = 0; nodes < powerInfos.CD.Nodes.Count; nodes++)
                    {
                        if (th.Name == powerInfos.CD.Nodes[nodes].ThingOut.Name &&
                            thPrev.Name != powerInfos.CD.Nodes[nodes].ThingIn.Name)
                        {
                            int index = IndexOfThing(powerInfos.CD.Nodes[nodes].ThingIn, powerInfos);
                            int stNbefore = powerInfos.StartNodes[index];

                            Mth.AddBranch(brStart, startN + 0, stNbefore + 0, rMin, 0, 0, powerInfos.NI);
                            AddBranch(brStart, startN + 0, stNbefore + 0, rMin, 0, 0, powerInfos.NI);
                            brStart++;

                            for (int i = 1; i <= powerInfos.CD.Things[nodes].RailCount; i++)
                            {
                                Mth.AddBranch(brStart, startN + i, stNbefore + i, rMin, 0, 0, powerInfos.NI);
                                AddBranch(brStart, startN + i, stNbefore + i, rMin, 0, 0, powerInfos.NI);
                                brStart++;
                            }
                        }
                    }
                }
                if (th.ThingType == ThingType.Junction1 || th.ThingType == ThingType.Junction2)
                {
                    int trackToConnectTo = 1;
                    if (th.ThingType == ThingType.Junction2)
                        trackToConnectTo = 2;

                    powerInfos.CO.BranchesSup[t] = startB;
                    int brStart = startB;

                    Mth.AddBranch(brStart, startN + 0, startNPrev + 0, rMin, 0, 0, powerInfos.NI);
                    AddBranch(brStart, startN + 0, startNPrev + 0, rMin, 0, 0, powerInfos.NI);
                    brStart++;

                    for (int i = 1; i <= thPrev.RailCount; i++)
                    {
                        Mth.AddBranch(brStart, startN + i, startNPrev + i, rMin, 0, 0, powerInfos.NI);
                        AddBranch(brStart, startN + i, startNPrev + i, rMin, 0, 0, powerInfos.NI);
                        brStart++;
                    }

                    for (int nodes = 0; nodes < powerInfos.CD.Nodes.Count; nodes++)
                    {
                        if (th.Name == powerInfos.CD.Nodes[nodes].ThingOut.Name &&
                            thPrev.Name != powerInfos.CD.Nodes[nodes].ThingIn.Name)
                        {
                            int index = IndexOfThing(powerInfos.CD.Nodes[nodes].ThingIn, powerInfos);
                            int stNbefore = powerInfos.StartNodes[index];

                            Mth.AddBranch(brStart, startN + 0, stNbefore + 0, rMin, 0, 0, powerInfos.NI);
                            AddBranch(brStart, startN + 0, stNbefore + 0, rMin, 0, 0, powerInfos.NI);
                            brStart++;

                            Mth.AddBranch(brStart, startN + trackToConnectTo, stNbefore + 1, rMin, 0, 0, powerInfos.NI);
                            AddBranch(brStart, startN + trackToConnectTo, stNbefore + 1, rMin, 0, 0, powerInfos.NI);
                            brStart++;
                        }
                    }
                }
                if (th.ThingType == ThingType.Junction12 || th.ThingType == ThingType.Junction22)
                {
                    int trackToConnectTo = 1;
                    if (th.ThingType == ThingType.Junction22)
                        trackToConnectTo = 2;

                    powerInfos.CO.BranchesSup[t] = startB;
                    int brStart = startB;

                    Mth.AddBranch(brStart, startN + 0, startNPrev + 0, rMin, 0, 0, powerInfos.NI);
                    AddBranch(brStart, startN + 0, startNPrev + 0, rMin, 0, 0, powerInfos.NI);
                    brStart++;

                    for (int i = 1; i <= thPrev.RailCount; i++)
                    {
                        Mth.AddBranch(brStart, startN + i, startNPrev + i, rMin, 0, 0, powerInfos.NI);
                        AddBranch(brStart, startN + i, startNPrev + i, rMin, 0, 0, powerInfos.NI);
                        brStart++;
                    }

                    for (int nodes = 0; nodes < powerInfos.CD.Nodes.Count; nodes++)
                    {
                        if (th.Name == powerInfos.CD.Nodes[nodes].ThingOut.Name &&
                            thPrev.Name != powerInfos.CD.Nodes[nodes].ThingIn.Name)
                        {
                            int index = IndexOfThing(powerInfos.CD.Nodes[nodes].ThingIn, powerInfos);
                            int stNbefore = powerInfos.StartNodes[index];

                            Mth.AddBranch(brStart, startN + 0, stNbefore + 0, rMin, 0, 0, powerInfos.NI);
                            AddBranch(brStart, startN + 0, stNbefore + 0, rMin, 0, 0, powerInfos.NI);
                            brStart++;

                            for (int i = 1; i <= powerInfos.CD.Things[nodes].RailCount; i++)
                            {
                                Mth.AddBranch(brStart, startN + trackToConnectTo, stNbefore + i, rMin, 0, 0, powerInfos.NI);
                                AddBranch(brStart, startN + trackToConnectTo, stNbefore + i, rMin, 0, 0, powerInfos.NI);
                                brStart++;
                            }
                        }
                    }
                }
                if (th.ThingType == ThingType.Separator)
                {
                    if (tPrev != -1)
                    {
                        for (int i = 0; i < thPrev.RailCount; i++)
                        {
                            Mth.AddBranch(startB + i, startNPrev + 1 + i, startNPrev + 0, rHigh, 0, 0, powerInfos.NI);
                            AddBranch(startB + i, startNPrev + 1 + i, startNPrev + 0, rHigh, 0, 0, powerInfos.NI);
                        }

                        if (th.RailCount > 1 && th.ElecData.RPA < 1)
                        {
                            Mth.AddBranch(startB + thPrev.RailCount, startNPrev + 1, startNPrev + 2, rMin, 0, 0, powerInfos.NI);
                            AddBranch(startB + thPrev.RailCount, startNPrev + 1, startNPrev + 2, rMin, 0, 0, powerInfos.NI);
                        }
                    }
                    if (tNext != -1)
                    {
                        for (int i = 0; i < thNext.RailCount; i++)
                        {
                            Mth.AddBranch(startB + thPrev.RailCount + i, startN + 1 + i, startN + 0, rHigh, 0, 0, powerInfos.NI);
                            AddBranch(startB + thPrev.RailCount + i, startN + 1 + i, startN + 0, rHigh, 0, 0, powerInfos.NI);
                        }

                        if (th.RailCount > 1 && th.ElecData.RPA < 1)
                        {
                            Mth.AddBranch(startB + thPrev.RailCount + thNext.RailCount, startN + 1, startN + 2, rMin, 0, 0, powerInfos.NI);
                            AddBranch(startB + thPrev.RailCount + thNext.RailCount, startN + 1, startN + 2, rMin, 0, 0, powerInfos.NI);
                        }
                    }
                }

                powerInfos.Connected[t] = true;
            }
            powerInfos.Branches = Branches;
        }
        public static int IndexOfThing(Thing t, PowerInfos powerInfos)
        {
            for (int i = 0; i < powerInfos.CD.Things.Count; i++)
            {
                if (t.Name == powerInfos.CD.Things[i].Name)
                {
                    return i;
                }
            }
            return -1;
        }
        public static List<Branches> Branches { get; set; }
        private static void AddBranch(int bNumber, int nodeIn, int nodeOut, float r, float v, float c, NodeInput nI)
        {
            if (Branches == null) Branches = new List<Branches>();
            Branches b = new Branches();
            b.BranchNumber = bNumber;
            b.NodeIn = nodeIn;
            b.NodeOut = nodeOut;
            b.Resistance = r;
            b.Voltage = v;
            b.Current = c;
            Branches.Add(b);
        }
    }
    [Serializable]
    public class Branches
    {
        public int BranchNumber { get; set; }
        public int NodeIn { get; set; }
        public int NodeOut { get; set; }
        public float Resistance { get; set; }
        public float Voltage { get; set; }
        public float Current { get; set; }
        public float CurrentResult { get; set; }
        public float VoltageResult { get; set; }
    }
    [Serializable]
    public class Results
    {
        public float[][,] VoltVeh { get; set; }
        public float[][,] CurrVeh { get; set; }
        public float[][,] VoltSup { get; set; }
        public float[,][,] CurrSup { get; set; }
        public string[] SuppliesNames { get; set; }
        public int[] SuppliesBranches { get; set; }
        public Results() { }
        public Results(int vehicles, int supplies, int length)
        {
            VoltVeh = new float[vehicles][,];
            CurrVeh = new float[vehicles][,];
            for(int v = 0; v < vehicles; v++)
            {
                VoltVeh[v] = new float[length, 2];
                CurrVeh[v] = new float[length, 2];
            }

            VoltSup = new float[supplies][,];
            CurrSup = new float[supplies, 8][,];
            SuppliesNames = new string[supplies];
            SuppliesBranches = new int[supplies];
            for(int s = 0; s < supplies; s++)
            {
                VoltSup[s] = new float[length, 2];
                for (int i = 0; i < 8; i++) CurrSup[s, i] = new float[length, 2];
            }
        }
    }
    public partial class Przejazd06
    {
        public void PowerCalculation(int i)
        {
            NodeInput nodeInput = new NodeInput(0, 0);
            PowerInfos powerInfos = new PowerInfos(cData, KP.CO);
            powerInfos.NI = nodeInput;
            PowerCalculations.AddPowerThings(ref powerInfos, i, PT);

            Mth.AddCircuit(powerInfos.NI);
            Mth.NodeAnalysis(powerInfos.NI);

            SaveCV(i, ref powerInfos);
            int[,] sum = new int[2, powerInfos.NI.wezlyDodatnie.GetLength(0)];
            string[] nodesDesc = new string[sum.GetLength(1)];
            for (int w = 0; w < powerInfos.NI.wezlyDodatnie.GetLength(0); w++)
            {
                for(int g = 0; g < powerInfos.NI.wezlyDodatnie.GetLength(1); g++)
                {
                    if (g == 277 || true)
                    {
                        if (powerInfos.NI.wezlyDodatnie[w, g] == true)
                        {
                            sum[0, w]++;
                            nodesDesc[w] += " +" + g.ToString();
                        }
                        if (powerInfos.NI.wezlyUjemne[w, g] == true)
                        {
                            sum[1, w]++;
                            nodesDesc[w] += " -" + g.ToString();
                        }
                    }
                }
            }
            for(int b = 0; b < powerInfos.Branches.Count; b++)
            {
                if (powerInfos.Branches[b].BranchNumber != b)
                    b += 0;
            }

            List<int> galezie = new List<int>();
            List<int> numery = new List<int>();
            List<int> roznice = new List<int>();
            for (int g = 0; g < powerInfos.Branches.Count; g++)
            {
                if (powerInfos.Branches[g].BranchNumber != g)
                {
                    numery.Add(g);
                    roznice.Add(g - powerInfos.Branches[g].BranchNumber);

                }
            }
            for (int g = 0; g < powerInfos.NI.Ig.GetLength(0); g++)
            {
                //if (powerInfos.NI.Ig[g, 0] > 2500) galezie.Add(g);
                //galezie.Add((int)powerInfos.NI.Ig[g, 0]);
            }
            for (int g = 0; g < powerInfos.NI.R.GetLength(0); g++)
            {
                if (powerInfos.NI.R[g, 0] == 0) galezie.Add(g);
            }
            for (int g = 0; g < KP.CO.NextStopNumber.Length; g++)
            {
                //if (Info[g][i - 2, 4] != null)
                //    try
                //    {
                //        KP.CO.NextStopNumber[g] = int.Parse(Info[g][i - 2, 4]);
                //    }
                //    catch { }
            }

            if (i >= 1) powerInfos.Branches.Clear();
        }
        private void SaveCV(int i, ref PowerInfos powerInfos)
        {
            for (int v = 0; v < powerInfos.CO.Vehicles.Length; v++)
            {
                int bn = powerInfos.CO.BranchesVeh[v];
                try
                {
                    PT[v][i, 10] = (float)powerInfos.NI.U[bn, 0];
                    if (PT[v][i, 10] < 0) PT[v][i, 10] *= -1;
                    if (PT[v][i, 10] > 4000) PT[v][i, 10] = 4000;
                    powerInfos.CO.VoltageVeh[v] = PT[v][i, 10];
                }
                catch { }
            }
            if (R == null)
            {
                int supplies = GetSuppliesCount(cData.Things);
                R = new Results(KP.VehiclesCount, supplies, KP.MaximumTime);
            }
            if (R != null)
            {
                for(int v = 0; v < KP.VehiclesCount; v++)
                {
                    R.CurrVeh[v][i, 0] = i;
                    R.CurrVeh[v][i, 1] = powerInfos.CO.CurrentsVeh[v];
                    R.VoltVeh[v][i, 0] = i;
                    R.CurrVeh[v][i, 1] = powerInfos.CO.VoltageVeh[v];
                }
                for(int s = 0; s < R.VoltSup.Length; s++)
                {
                    GetSuppliesNamesAndBranches(cData.Things, powerInfos);
                    int b = R.SuppliesBranches[s];
                    R.VoltSup[s][i, 0] = i;
                    R.VoltSup[s][i, 1] = -1 * (float)powerInfos.NI.U[b, 0];
                    GetSuppliesCurrent(powerInfos, s, i);
                }
            }
        }
        private int GetSuppliesCount(List<Thing> things)
        {
            int s = 0;
            for(int t = 0; t < things.Count; t++)
            {
                if (things[t].ThingType == ThingType.Supply) s++;
            }
            return s;
        }
        private void GetSuppliesNamesAndBranches(List<Thing> things, PowerInfos powerInfos)
        {
            int s = 0;
            for (int t = 0; t < things.Count; t++)
            {
                if (things[t].ThingType == ThingType.Supply)
                {
                    this.R.SuppliesNames[s] = things[t].Name;
                    this.R.SuppliesBranches[s] = powerInfos.CO.BranchesSup[t];
                    s++;
                }
            }
        }
        private void GetSuppliesCurrent(PowerInfos powerInfos, int s, int i)
        {
            string supplyName = R.SuppliesNames[s];
            Thing th = new Thing();
            for(int t = 0; t < powerInfos.CD.Things.Count; t++)
            {
                if (powerInfos.CD.Things[t].Name == supplyName) th = powerInfos.CD.Things[t];
            }

            for (int z = 1; z <= th.SupplyCount; z++)
            {
                int b = R.SuppliesBranches[s];
                R.CurrSup[s, z - 1][i, 0] = i;
                R.CurrSup[s, z - 1][i, 1] = (float)powerInfos.NI.Ig[b + 1 + z, 0];
            }
        }
    }
}
