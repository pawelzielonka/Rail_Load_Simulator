using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;

namespace Vamos21
{
    public partial class Form1 : Form
    {
        public ProjectData projectData;
        public CalculationObjects calculationObjects;
        public Przejazd06 przejazd06;
        public List<Vehicle> vehicles;
        public Lines line;
        public Form1()
        {
            InitializeComponent();
            projectData = new ProjectData();
            calculationObjects = new CalculationObjects();
            try
            {
                string path = Operations.ReadStringFromXML(Directory.GetCurrentDirectory() + "//default.xml");
                projectData = Operations.ReadFromXML(path);
                this.Text = path;
            }
            catch (Exception ex)
            {
                richTextBox.Text = " błąd przy uruchomieniu" + ex.ToString();
            }

            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += new DoWorkEventHandler(LoadProjectDataToForm);
            backgroundWorker.RunWorkerAsync();

            backgroundWorkerVamos.DoWork += new DoWorkEventHandler(Vamos);
            startI = 0;
        }

        private void ButtonFunction_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerVamos.IsBusy) backgroundWorkerVamos.CancelAsync();

            int tabIndex = 0;
            string command = richTextBox.Text;
            if (command == "SaveConfig") tabIndex = 6;
            if (command == "CopyConfig") tabIndex = 7;
            if (command == "ClearConfig") tabIndex = 5;
            if (command == "Upload") tabIndex = 4;
            if (command == "New") tabIndex = 3;
            if (command == "R") tabIndex = 8;
            if (command == "Export") tabIndex = 9;
            if (command == "Final") tabIndex = 10;
            if (command == "ForNN") tabIndex = 11;


            switch (tabIndex)
            {
                case 11:
                    PrepareForExtraction();
                    Operations.ExtractProfilesForNN(przejazd06);
                    break;
                case 10:
                    MeanVoltageVehicles();
                    MeanZone();
                    break;
                case 9:
                    ExportFunction();
                    break;
                case 8:
                    //update reverse arrays
                    AddNodesToReverse();
                    break;
                case 7:
                    //copy config stops from temp object to new config
                    break;
                case 6:
                    //save config stops for each vohicle in config to temporary objects
                    break;
                case 5:
                    projectData.ConfigurationData.StopConfig.Clear();
                    projectData.ConfigurationData.DelayConfig.Clear();
                    projectData.ConfigurationData.DirectionConfig.Clear();
                    projectData.ConfigurationData.InfoConfig.Clear();
                    projectData.ConfigurationData.IsCheckedConfig.Clear();
                    projectData.ConfigurationData.RoundCountConfig.Clear();
                    projectData.ConfigurationData.RouteConfig.Clear();
                    projectData.ConfigurationData.TrainMassConfig.Clear();
                    projectData.ConfigurationData.VehicleConfig.Clear();
                    dataGridViewConfiguration.Rows.Clear();
                    dataGridViewConfiguration.Refresh();
                    break;
                case 4:
                    projectData.ConfigurationData.Things.Clear();
                    projectData.ConfigurationData.Nodes.Clear();
                    projectData.ConfigurationData.Routes.Clear();
                    projectData.ConfigurationData.Things = Operations.ImportStationsTracksObjects(projectData.ConfigurationData.Profiles);
                    List<Thing> tempTh = projectData.ConfigurationData.Things;
                    List<Route> tempRt = projectData.ConfigurationData.Routes;
                    projectData.ConfigurationData.Nodes = Operations.ImportNodesAndRoutes(projectData.ConfigurationData.Profiles, ref tempTh, ref tempRt);
                    projectData.ConfigurationData.Things = tempTh;
                    projectData.ConfigurationData.Routes = tempRt;
                    UpdateThingListBoxes();
                    UpdateRoutesListBoxes();
                    UpdateNodesListBoxes();
                    break;
                case 3:
                    NewProject();
                    break;
            }
        }
        private void NewProject()
        {

        }
        private void BRefresh_Click(object sender, EventArgs e)
        {
            PopulateResults();
        }
        private void UpdateProgressBar(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
        private void LoadProjectDataToForm(object sender, DoWorkEventArgs e)
        {
            tbVehData.Text = projectData.PathVehicles;
            tbRouData.Text = projectData.PathProfiles;
            tbPowData.Text = projectData.PathPowerSystems;
            tbPIDData.Text = projectData.PathPID;

            LoadsDataFromExcel();
            //ladowanie danych z exceli
            try
            {
            }
            catch (Exception ex)
            {
                richTextBox.Text = "Błąd ładowania z Excela" + ex.ToString();
            }

            //ładowanie danych układu
            try
            {
                cbThingType.DataSource = null;
                cbThingType.DataSource = Enum.GetValues(typeof(ThingType));
            }
            catch (Exception ex)
            {
                richTextBox.Text += " Bład ładowania danych układu" + ex.ToString();
            }

            try
            {
                if (projectData.ConfigurationData.IsCheckedConfig.Count != 0)
                {
                    this.dataGridViewConfiguration.RowCount = projectData.ConfigurationData.IsCheckedConfig.Count;

                    for (int i = 0; i < projectData.ConfigurationData.IsCheckedConfig.Count; i++)
                    {
                        dataGridViewConfiguration.Rows[i].Cells[0].Value = projectData.ConfigurationData.IsCheckedConfig[i];
                        dataGridViewConfiguration.Rows[i].Cells[1].Value = projectData.ConfigurationData.RouteConfig[i];
                        dataGridViewConfiguration.Rows[i].Cells[2].Value = projectData.ConfigurationData.VehicleConfig[i];
                        dataGridViewConfiguration.Rows[i].Cells[3].Value = projectData.ConfigurationData.DirectionConfig[i];
                        dataGridViewConfiguration.Rows[i].Cells[4].Value = projectData.ConfigurationData.TrainMassConfig[i];
                        dataGridViewConfiguration.Rows[i].Cells[6].Value = projectData.ConfigurationData.InfoConfig[i];
                        dataGridViewConfiguration.Rows[i].Cells[5].Value = projectData.ConfigurationData.DelayConfig[i];
                        dataGridViewConfiguration.Rows[i].Cells[7].Value = projectData.ConfigurationData.RoundCountConfig[i];
                    }
                }

            }
            catch (Exception ex)
            {
                richTextBox.Text += " Błąd >konfiguracja, trasa, pojazd, zasilanie< " + ex.ToString();
            }

        }

        private void LoadProjectDataFromForm()
        {
            try
            {
                projectData.PathVehicles = tbVehData.Text;
                projectData.PathProfiles = tbRouData.Text;
                projectData.PathPowerSystems = tbPowData.Text;
                projectData.PathPID = tbPIDData.Text;
            }
            catch
            {
                richTextBox.Text += " Błąd załadowania ścieżek do obibektu";
            }

            try
            {
                projectData.ConfigurationData.IsCheckedConfig.Clear();
                projectData.ConfigurationData.RouteConfig.Clear();
                projectData.ConfigurationData.VehicleConfig.Clear();
                projectData.ConfigurationData.DirectionConfig.Clear();
                projectData.ConfigurationData.DelayConfig.Clear();
                projectData.ConfigurationData.InfoConfig.Clear();
                projectData.ConfigurationData.RoundCountConfig.Clear();
                projectData.ConfigurationData.TrainMassConfig.Clear();

                for (int i = 0; i < dataGridViewConfiguration.Rows.Count; i++)
                {
                    if (dataGridViewConfiguration.Rows[i].Cells[1].Value != null)
                    {
                        projectData.ConfigurationData.IsCheckedConfig.Add((bool)dataGridViewConfiguration.Rows[i].Cells[0].Value);
                        projectData.ConfigurationData.RouteConfig.Add((string)dataGridViewConfiguration.Rows[i].Cells[1].Value.ToString());
                        projectData.ConfigurationData.VehicleConfig.Add((string)dataGridViewConfiguration.Rows[i].Cells[2].Value.ToString());
                        projectData.ConfigurationData.DirectionConfig.Add((Direction)dataGridViewConfiguration.Rows[i].Cells[3].Value);
                        projectData.ConfigurationData.TrainMassConfig.Add(Convert.ToInt32(dataGridViewConfiguration.Rows[i].Cells[4].Value.ToString()));
                        projectData.ConfigurationData.InfoConfig.Add(dataGridViewConfiguration.Rows[i].Cells[6].Value.ToString());
                        projectData.ConfigurationData.DelayConfig.Add(Convert.ToInt32(dataGridViewConfiguration.Rows[i].Cells[5].Value.ToString()));
                        projectData.ConfigurationData.RoundCountConfig.Add(Convert.ToInt32(dataGridViewConfiguration.Rows[i].Cells[7].Value.ToString()));
                    }
                }
            }
            catch
            {
                richTextBox.Text += " Błąd ładowania konfiguracji do obiektu";
            }
            try
            {
                Time t = new Time();
                t = Time.GetTime(int.Parse(tbSimTimeHours.Text.ToString()), int.Parse(tbSimTimeMinutes.Text.ToString()));
                projectData.SimTime = t;
                projectData.DeltaT = float.Parse(tbDeltaT.Text.ToString());
                Time t2 = new Time();
                t2 = Time.GetTime(int.Parse(tbInitialTimeH.Text.ToString()), int.Parse(tbInitialTimeM.Text.ToString()));
                projectData.InitialTime = t2;
            }
            catch
            {
                richTextBox.Text += " Błąd ładowania stałych czasowych do obiektu";
            }
            try
            {
                projectData.BreakingDistance = float.Parse(tbBreakingDistance.Text.ToString());
            }
            catch
            {
                richTextBox.Text += " błąd ładowania innych danych do obiektu";
            }

        }

        private void ZapiszJakoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadProjectDataFromForm();
            saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Operations.SerializeToXML(projectData, saveFileDialog.FileName);
                Operations.SerializeToXML(saveFileDialog.FileName + ".xml", Directory.GetCurrentDirectory() + "//default");
            }
            this.Text = saveFileDialog.FileName + ".xml";
        }

        private void OtwórzToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                projectData = Operations.ReadFromXML(openFileDialog.FileName);
                richTextBox.Text = "Otwarto " + openFileDialog.FileName;
                this.Text = openFileDialog.FileName;
                this.tabControl.Enabled = false;

                backgroundWorker.WorkerReportsProgress = true;
                backgroundWorker.DoWork += new DoWorkEventHandler(LoadProjectDataToForm);
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void ZapiszToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadProjectDataFromForm();
            Operations.SerializeToXML(projectData, this.Text);
            Operations.SerializeToXML(this.Text, Directory.GetCurrentDirectory() + "//default");

        }

        private void TbVehData_DoubleClick(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                tbVehData.Text = openFileDialog.FileName;
            }
        }

        private void TbPIDData_DoubleClick(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                tbPIDData.Text = openFileDialog.FileName;
            }

        }

        private void TbRouData_DoubleClick(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                tbRouData.Text = openFileDialog.FileName;
            }

        }

        private void TbPowData_DoubleClick(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                tbPowData.Text = openFileDialog.FileName;
            }

        }
        private void UpdateThingListBoxes()
        {
            lbThing.DataSource = null;
            cbNodeLeft.DataSource = null;
            cbNodeRight.DataSource = null;

            lbThing.BindingContext = new BindingContext();
            cbNodeLeft.BindingContext = new BindingContext();
            cbNodeRight.BindingContext = new BindingContext();

            lbThing.DisplayMember = "Name";
            cbNodeLeft.DisplayMember = "Name";
            cbNodeRight.DisplayMember = "Name";

            lbThing.DataSource = projectData.ConfigurationData.Things;
            cbNodeLeft.DataSource = projectData.ConfigurationData.Things;
            cbNodeRight.DataSource = projectData.ConfigurationData.Things;
        }

        private void UpdateNodesListBoxes()
        {
            lbNodes.DataSource = null;
            cbNodes.DataSource = null;

            lbNodes.BindingContext = new BindingContext();
            cbNodes.BindingContext = new BindingContext();

            lbNodes.DisplayMember = "Name";
            cbNodes.DisplayMember = "Name";

            lbNodes.DataSource = projectData.ConfigurationData.Nodes;
            cbNodes.DataSource = projectData.ConfigurationData.Nodes;
        }
        private void UpdateRoutesListBoxes()
        {
            lbRoutes.DataSource = null;
            lbRoutes.BindingContext = new BindingContext();
            lbRoutes.DisplayMember = "Name";
            lbRoutes.DataSource = projectData.ConfigurationData.Routes;

            cbRouteMerge.DataSource = null;
            cbRouteMerge.BindingContext = new BindingContext();
            cbRouteMerge.DisplayMember = "Name";
            cbRouteMerge.DataSource = projectData.ConfigurationData.Routes;
        }
        private void UpdateNodesInRoutesListBoxes()
        {
            lbNodesInRoute.DataSource = null;
            lbNodesInRoute.BindingContext = new BindingContext();
            lbNodesInRoute.DisplayMember = "Name";
            if (lbRoutes.SelectedIndex >= 0 && projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes != null)
                lbNodesInRoute.DataSource = projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes;
        }
        private void UpdateReverseListBoxes()
        {
            int index = lbRoutes.SelectedIndex;

            try
            {
                if (projectData.ConfigurationData.Routes[index].Reverse == null)
                {
                    projectData.ConfigurationData.Routes[index].Reverse = new bool[2][];

                    int count = projectData.ConfigurationData.Routes[index].Nodes.Count;
                    for (int i = 0; i < 2; i++)
                    {
                        projectData.ConfigurationData.Routes[index].Reverse[i] = new bool[count];
                    }
                }
            }
            catch { }

            try
            {
                clbLeft.Items.Clear();
                clbRight.Items.Clear();
                for (int i = 0; i < projectData.ConfigurationData.Routes[index].Reverse[0].Length; i++)
                {
                    clbLeft.Items.Add(projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes[i].ThingIn.Name);
                    clbRight.Items.Add(projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes[i].ThingOut.Name);
                }
            }
            catch { }

            try
            {
                for (int i = 0; i < projectData.ConfigurationData.Routes[index].Reverse[0].Length; i++)
                {
                    clbLeft.SetItemChecked(i, projectData.ConfigurationData.Routes[index].Reverse[0][i]);
                    clbRight.SetItemChecked(i, projectData.ConfigurationData.Routes[index].Reverse[1][i]);
                }
            }
            catch { }
        }
        private void UpdateReverse(bool updateReverse)
        {
            int index = lbRoutes.SelectedIndex;

            if (updateReverse == true)
                for (int i = 0; i < clbLeft.Items.Count; i++)
                {
                    try { projectData.ConfigurationData.Routes[index].Reverse[0][i] = clbLeft.GetItemChecked(i); } catch { }
                    try { projectData.ConfigurationData.Routes[index].Reverse[1][i] = clbRight.GetItemChecked(i); } catch { }
                }
            changeNodeReverse = false;
        }
        private void AddNodesToReverse()
        {
            int index = lbRoutes.SelectedIndex;

            try
            {
                projectData.ConfigurationData.Routes[index].Reverse = new bool[2][];

                int count = projectData.ConfigurationData.Routes[index].Nodes.Count;
                for (int i = 0; i < 2; i++)
                {
                    projectData.ConfigurationData.Routes[index].Reverse[i] = new bool[count];
                }

                UpdateReverse(true);
                UpdateReverseListBoxes();
            }
            catch { }
        }
        private void UpdateVehiclesListBoxes()
        {
            lbVehicles.BindingContext = new BindingContext();
            lbVehicles.DisplayMember = "Name";
            lbVehicles.DataSource = projectData.ConfigurationData.Vehicles;
        }
        private void UpdateProfilesListBoxes()
        {
            lbProfiles.DataSource = null;
            cbProfile.DataSource = null;

            lbProfiles.BindingContext = new BindingContext();
            cbProfile.BindingContext = new BindingContext();

            lbProfiles.DisplayMember = "Name";
            cbProfile.DisplayMember = "Name";

            lbProfiles.DataSource = projectData.ConfigurationData.Profiles;
            cbProfile.DataSource = projectData.ConfigurationData.Profiles;
        }
        private void UpdateProfilesInThings()
        {
            int n = projectData.ConfigurationData.Things.Count;
            for (int i = 0; i < n; i++)
            {
                var p = projectData.ConfigurationData.Profiles.FirstOrDefault(pr => pr.Name == projectData.ConfigurationData.Things[i].ProfileName);
                projectData.ConfigurationData.Things[i].Profile = p;
            }
            n = projectData.ConfigurationData.Nodes.Count;
            for (int i = 0; i < n; i++)
            {
                var p1 = projectData.ConfigurationData.Profiles.FirstOrDefault(pr1 => pr1.Name == projectData.ConfigurationData.Nodes[i].ThingIn.ProfileName);
                var p2 = projectData.ConfigurationData.Profiles.FirstOrDefault(pr2 => pr2.Name == projectData.ConfigurationData.Nodes[i].ThingIn.ProfileName);
                projectData.ConfigurationData.Nodes[i].ThingIn.Profile = p1;
                projectData.ConfigurationData.Nodes[i].ThingOut.Profile = p2;
            }
        }
        private void LoadsPIDFromForm()
        {
            PID pid = new PID();
            int rows = projectData.ConfigurationData.PID.P.GetLength(0);
            int columns = projectData.ConfigurationData.PID.P.GetLength(1);
            pid.P = new float[rows, columns];
            pid.I = new float[rows, columns];
            pid.D = new float[rows, columns];
            pid.Coeff = projectData.ConfigurationData.PID.Coeff;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    pid.P[i, j] = float.Parse(dataGridViewP[j, i].Value.ToString());
                    pid.I[i, j] = float.Parse(dataGridViewI[j, i].Value.ToString());
                    pid.D[i, j] = float.Parse(dataGridViewD[j, i].Value.ToString());
                }
            }
            projectData.ConfigurationData.PID = pid;
        }
        private void LoadsDataFromExcel()
        {
            List<Vehicle> vehicles = new List<Vehicle>();
            Operations.LoadExcelFile(projectData.PathVehicles, ExcelType.Vehicles, ref vehicles);
            projectData.ConfigurationData.Vehicles = vehicles;
            lbVehicles.DataSource = vehicles;
            PopulateTextBoxesVehicles(0);

            PID pid = new PID();
            Operations.LoadExcelFile(projectData.PathPID, ExcelType.PID, ref pid);
            projectData.ConfigurationData.PID = pid;
            PopulatePIDGridView();

            List<Profile> prof = new List<Profile>();
            Operations.LoadExcelFile(projectData.PathProfiles, ExcelType.Profile, ref prof);
            projectData.ConfigurationData.Profiles = prof;
            PopulateProfilesGridView(0);

            UpdateThingListBoxes();
            UpdateNodesListBoxes();
            UpdateRoutesListBoxes();
            UpdateNodesInRoutesListBoxes();
            UpdateVehiclesListBoxes();
            UpdateProfilesListBoxes();

            UpdateProfilesInThings();

            (this.dataGridViewConfiguration.Columns[1] as DataGridViewComboBoxColumn).DataSource = projectData.ConfigurationData.Routes;
            (this.dataGridViewConfiguration.Columns[2] as DataGridViewComboBoxColumn).DataSource = projectData.ConfigurationData.Vehicles;
            (this.dataGridViewConfiguration.Columns[3] as DataGridViewComboBoxColumn).ValueType = typeof(Direction);
            (this.dataGridViewConfiguration.Columns[3] as DataGridViewComboBoxColumn).DataSource = Enum.GetValues(typeof(Direction));

            (this.dataGridViewConfiguration.Columns[1] as DataGridViewComboBoxColumn).DisplayMember = "Name";
            (this.dataGridViewConfiguration.Columns[2] as DataGridViewComboBoxColumn).DisplayMember = "Name";
        }
        private void PopulateTextBoxesVehicles(int n)
        {
            this.tbRodzaj.Text = (lbVehicles.Items[n] as Vehicle).Name;
            this.tbDlugosc.Text = (lbVehicles.Items[n] as Vehicle).Length.ToString();
            this.tbLiczbaOsi.Text = (lbVehicles.Items[n] as Vehicle).AxlesCount.ToString();
            this.tbLOsiNapednych.Text = (lbVehicles.Items[n] as Vehicle).AxlesDriven.ToString();
            this.tbVmax.Text = (lbVehicles.Items[n] as Vehicle).MaxSpeed.ToString();
            this.tbMasa.Text = (lbVehicles.Items[n] as Vehicle).GrossMass.ToString();
            this.tbCzolo.Text = (lbVehicles.Items[n] as Vehicle).FrontalArea.ToString();
            this.tbLczlonow.Text = (lbVehicles.Items[n] as Vehicle).Members.ToString();
            this.tbNaciskNaOs.Text = (lbVehicles.Items[n] as Vehicle).AxleForce.ToString();
            this.tbNaciskNaOsProd.Text = (lbVehicles.Items[n] as Vehicle).AxleForceManufacturer.ToString();
            this.tbMasaHSzyb.Text = (lbVehicles.Items[n] as Vehicle).FastBreakMass.ToString();
            this.tbMasaHwolny.Text = (lbVehicles.Items[n] as Vehicle).SlowBreakMass.ToString();
            this.tbUklOsi.Text = (lbVehicles.Items[n] as Vehicle).AxlesConfig;
            this.tbOpozHszybki.Text = (lbVehicles.Items[n] as Vehicle).FastDecel.ToString();
            this.tbOpozHwolny.Text = (lbVehicles.Items[n] as Vehicle).SlowDecel.ToString();
            this.tbOporyA.Text = (lbVehicles.Items[n] as Vehicle).CoefA.ToString();
            this.tbOporyB.Text = (lbVehicles.Items[n] as Vehicle).CoefB.ToString();
            this.tbOporyC.Text = (lbVehicles.Items[n] as Vehicle).CoefC.ToString();
            this.tbJerkMax.Text = (lbVehicles.Items[n] as Vehicle).JerkMax.ToString();
            this.tbAccMax.Text = (lbVehicles.Items[n] as Vehicle).AccMax.ToString();
            this.tbDecMax.Text = (lbVehicles.Items[n] as Vehicle).DecMax.ToString();

            DrawForce((lbVehicles.Items[n] as Vehicle).Force, 1, chartVehicle);
        }
        private void PopulatePIDGridView()
        {
            int rows = projectData.ConfigurationData.PID.P.GetLength(0);
            int columns = projectData.ConfigurationData.PID.P.GetLength(1);

            this.dataGridViewP.ColumnCount = columns;
            this.dataGridViewI.ColumnCount = columns;
            this.dataGridViewD.ColumnCount = columns;
            this.dataGridViewDiff.ColumnCount = 2;

            this.dataGridViewP.RowCount = rows;
            this.dataGridViewI.RowCount = rows;
            this.dataGridViewD.RowCount = rows;
            this.dataGridViewDiff.RowCount = 6;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    this.dataGridViewP[j, i].Value = projectData.ConfigurationData.PID.P[i, j].ToString();
                    this.dataGridViewI[j, i].Value = projectData.ConfigurationData.PID.I[i, j].ToString();
                    this.dataGridViewD[j, i].Value = projectData.ConfigurationData.PID.D[i, j].ToString();
                }
            }

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    this.dataGridViewDiff[i, j].Value = projectData.ConfigurationData.PID.Coeff[i, j].ToString();
                }
            }
        }
        private void PopulateProfilesGridView(int n)
        {
            int r1 = projectData.ConfigurationData.Profiles[n].Profile1.GetLength(1);
            int r2 = projectData.ConfigurationData.Profiles[n].Profile2.GetLength(1);
            int r3 = projectData.ConfigurationData.Profiles[n].Limits.GetLength(1);
            int rows = r1 + 1;
            if (r2 > r1) rows = r2 + 1;
            if (r3 > r2) rows = r3 + 1;

            this.dataGridViewProfile.ColumnCount = 6;
            this.dataGridViewProfile.RowCount = rows;

            this.dataGridViewProfile[0, 0].Value = "Metr";
            this.dataGridViewProfile[1, 0].Value = "Wzniesienie";
            this.dataGridViewProfile[2, 0].Value = "Metr";
            this.dataGridViewProfile[3, 0].Value = "Łuk";
            this.dataGridViewProfile[4, 0].Value = "Metr";
            this.dataGridViewProfile[5, 0].Value = "Ograniczenie";

            for (int i = 0; i < projectData.ConfigurationData.Profiles[n].Profile1.GetLength(1); i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    this.dataGridViewProfile[j, i + 1].Value = projectData.ConfigurationData.Profiles[n].Profile1[j, i].ToString();
                }
            }
            for (int i = 0; i < projectData.ConfigurationData.Profiles[n].Profile2.GetLength(1); i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    this.dataGridViewProfile[j + 2, i + 1].Value = projectData.ConfigurationData.Profiles[n].Profile2[j, i].ToString();
                }
            }
            for (int i = 0; i < projectData.ConfigurationData.Profiles[n].Limits.GetLength(1); i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    this.dataGridViewProfile[j + 4, i + 1].Value = projectData.ConfigurationData.Profiles[n].Limits[j, i].ToString();
                }
            }

        }
        private void BAddThing_Click(object sender, EventArgs e)
        {
            Thing t = new Thing(this.tbThingName.Text, (int)nudRailCount.Value, (ThingType)cbThingType.SelectedItem, float.Parse(tbLength.Text));
            t.LineName = tbLineName.Text;
            t.KMstart = float.Parse(tbKMstart.Text.ToString());
            t.Profile = (Profile)cbProfile.SelectedItem;
            t.ProfileName = (cbProfile.SelectedItem as Profile).Name;
            t.WingCount = (int)nudWing.Value;
            if (t.ThingType == ThingType.Cabin)
            {
                try
                {
                    t.ElecData.RK1 = float.Parse(tbrk1.Text.ToString());
                    t.ElecData.RK2 = float.Parse(tbrk2.Text.ToString());
                    t.ElecData.RK3 = float.Parse(tbrk3.Text.ToString());
                    t.ElecData.RK4 = float.Parse(tbrk4.Text.ToString());
                }
                catch { }
            }
            if (t.ThingType == ThingType.Supply)
            {
                try
                {
                    t.ElecData.RZA1 = float.Parse(tbrza1.Text.ToString());
                    t.ElecData.RZA2 = float.Parse(tbrza2.Text.ToString());
                    t.ElecData.RZB1 = float.Parse(tbrzb1.Text.ToString());
                    t.ElecData.RZB2 = float.Parse(tbrzb2.Text.ToString());
                    t.ElecData.RWewA = float.Parse(tbrwew.Text.ToString());
                    t.ElecData.RPA = float.Parse(tbRp.Text.ToString());
                    t.ElecData.UA = float.Parse(tbu.Text.ToString());
                    t.SupplyCount = (int)nudSupplyCount.Value;
                }
                catch { }
            }
            if (t.ThingType == ThingType.Track)
            {
                try
                {
                    t.ElecData.RT = float.Parse(tbrt.Text.ToString());
                    t.ElecData.RS = float.Parse(tbrs.Text.ToString());
                }
                catch { }
            }
            projectData.ConfigurationData.Things.Add(t);

            UpdateThingListBoxes();
        }
        private void LbThing_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = lbThing.SelectedIndex;
            if (this.tabControl.Enabled == true && lbThing.Items != null && n >= 0)
            {
                Thing t = projectData.ConfigurationData.Things[n];
                this.tbThingName.Text = t.Name;
                this.nudRailCount.Value = t.RailCount;
                this.cbThingType.SelectedItem = t.ThingType;
                this.tbLength.Text = t.Length.ToString();
                this.tbLineName.Text = t.LineName;
                this.tbKMstart.Text = t.KMstart.ToString();
                this.cbProfile.SelectedItem = t.Profile;
                this.nudWing.Value = t.WingCount;

                if (t.ThingType == ThingType.Track)
                {
                    tbrs.Enabled = true;
                    tbrt.Enabled = true;
                    try
                    {
                        tbrs.Text = t.ElecData.RS.ToString();
                        tbrt.Text = t.ElecData.RT.ToString();
                    }
                    catch { }
                }
                if (t.ThingType != ThingType.Cabin) groupBox11.Enabled = false;
                if (t.ThingType == ThingType.Cabin)
                {
                    groupBox11.Enabled = true;
                    tbrt.Enabled = false;
                    tbrs.Enabled = false;
                    try
                    {
                        tbrk1.Text = t.ElecData.RK1.ToString();
                        tbrk2.Text = t.ElecData.RK2.ToString();
                        tbrk3.Text = t.ElecData.RK3.ToString();
                        tbrk4.Text = t.ElecData.RK4.ToString();
                    }
                    catch { }
                }
                if (t.ThingType != ThingType.Supply) groupBox10.Enabled = false;
                if (t.ThingType == ThingType.Supply)
                {
                    groupBox10.Enabled = true;
                    tbrt.Enabled = false;
                    tbrs.Enabled = false;
                    try
                    {
                        tbrza1.Text = t.ElecData.RZA1.ToString();
                        tbrza2.Text = t.ElecData.RZA2.ToString();
                        tbrzb1.Text = t.ElecData.RZB1.ToString();
                        tbrzb2.Text = t.ElecData.RZB2.ToString();
                        tbrwew.Text = t.ElecData.RWewA.ToString();
                        tbRp.Text = t.ElecData.RPA.ToString();
                        tbu.Text = t.ElecData.UA.ToString();
                        this.nudSupplyCount.Value = t.SupplyCount;
                    }
                    catch { }
                }
            }
        }
        private void LbNodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = lbNodes.SelectedIndex;
            if (this.tabControl.Enabled == true && lbNodes.Items != null && n >= 0)
            {
                cbNodeLeft.SelectedIndex = n;
                cbNodeRight.SelectedIndex = n;

                nudLeftThing.Maximum = projectData.ConfigurationData.Nodes[n].ThingIn.WingCount;
                nudRightThing.Maximum = projectData.ConfigurationData.Nodes[n].ThingOut.WingCount;

                nudLeftThing.Value = projectData.ConfigurationData.Nodes[n].WingIn;
                nudRightThing.Value = projectData.ConfigurationData.Nodes[n].WingOut;

            }
        }
        private void BDelThing_Click(object sender, EventArgs e)
        {
            projectData.ConfigurationData.Things.RemoveAt(lbThing.SelectedIndex);
            UpdateThingListBoxes();
        }
        private void BNodeAdd_Click(object sender, EventArgs e)
        {
            Thing t1 = (Thing)cbNodeLeft.SelectedItem;
            Thing t2 = (Thing)cbNodeRight.SelectedItem;
            string name = t1.Name +
                " odg." +
                nudLeftThing.Value.ToString() +
                " " +
                t2.Name +
                " odg." +
                nudRightThing.Value.ToString();
            Node n = new Node(t1, (int)nudLeftThing.Value, t2, (int)nudRightThing.Value, name);
            projectData.ConfigurationData.Nodes.Add(n);

            int ind1 = projectData.ConfigurationData.Things.IndexOf(t1);
            int ind2 = projectData.ConfigurationData.Things.IndexOf(t2);
            if (projectData.ConfigurationData.Things[ind1].ThingsAtWings == null) projectData.ConfigurationData.Things[ind1].ThingsAtWings = new string[16];
            if (projectData.ConfigurationData.Things[ind2].ThingsAtWings == null) projectData.ConfigurationData.Things[ind2].ThingsAtWings = new string[16];
            projectData.ConfigurationData.Things[ind1].ThingsAtWings[(int)nudLeftThing.Value] = t2.Name;
            projectData.ConfigurationData.Things[ind2].ThingsAtWings[(int)nudRightThing.Value] = t1.Name;

            UpdateNodesListBoxes();
        }
        private void BNodeDel_Click(object sender, EventArgs e)
        {
            projectData.ConfigurationData.Nodes.RemoveAt(lbNodes.SelectedIndex);
            UpdateNodesListBoxes();
        }
        private void BAddToRoute_Click(object sender, EventArgs e)
        {
            projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes.Add((Node)cbNodes.SelectedItem);
            UpdateNodesInRoutesListBoxes();
        }
        private void BMerge_Click(object sender, EventArgs e)
        {
            List<Node> toAdd = new List<Node>();
            Route r = (Route)cbRouteMerge.SelectedItem;
            toAdd = r.Nodes;
            projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes.AddRange(toAdd);
            UpdateNodesInRoutesListBoxes();
        }
        private void BDeleteFromRoute_Click(object sender, EventArgs e)
        {
            int index = lbNodesInRoute.SelectedIndex;
            projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes.RemoveAt(lbNodesInRoute.SelectedIndex);
            UpdateNodesInRoutesListBoxes();
            lbNodesInRoute.SelectedIndex = index - 1;
        }
        private void BAddRoute_Click(object sender, EventArgs e)
        {
            Route r = new Route(tbRouteName.Text);
            projectData.ConfigurationData.Routes.Add(r);
            UpdateRoutesListBoxes();
        }
        private void BDelRoute_Click(object sender, EventArgs e)
        {
            projectData.ConfigurationData.Routes.RemoveAt(lbRoutes.SelectedIndex);
            UpdateRoutesListBoxes();
        }
        private void CbNodeLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                nudLeftThing.Maximum = projectData.ConfigurationData.Things[cbNodeLeft.SelectedIndex].WingCount;
            }
            catch
            {

            }
        }

        private void CbNodeRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                nudRightThing.Maximum = projectData.ConfigurationData.Things[cbNodeRight.SelectedIndex].WingCount;
            }
            catch
            {

            }
        }

        private void LbRoutes_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateNodesInRoutesListBoxes();
                UpdateReverseListBoxes();
            }
            catch
            {

            }
        }

        private void BNodeUp_Click(object sender, EventArgs e)
        {
            int index = lbNodesInRoute.SelectedIndex;
            if (index != 0)
            {
                Node n = new Node();
                n = projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes[index];
                projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes.RemoveAt(index);
                projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes.Insert(index - 1, n);
                UpdateNodesInRoutesListBoxes();
                lbNodesInRoute.SetSelected(index - 1, true);
            }
        }

        private void BNodeDown_Click(object sender, EventArgs e)
        {
            int index = lbNodesInRoute.SelectedIndex;
            if (index != lbNodesInRoute.Items.Count)
            {
                Node n = new Node();
                n = projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes[index];
                projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes.RemoveAt(index);
                projectData.ConfigurationData.Routes[lbRoutes.SelectedIndex].Nodes.Insert(index + 1, n);
                UpdateNodesInRoutesListBoxes();
                lbNodesInRoute.SetSelected(index + 1, true);
            }

        }

        private void LbVehicles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = lbVehicles.SelectedIndex;
            if (lbVehicles.Items != null)
                PopulateTextBoxesVehicles(n);
        }
        private void LbProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = lbProfiles.SelectedIndex;
            if (lbProfiles.Items != null)
                PopulateProfilesGridView(n);
        }
        private void DrawForce(float[] f, int dokladnosc, Chart chart)
        {
            DataTable dTable;           //tablica reprezentujaca baze danych
            DataView dView;             //obiekt reprezentujacy filtr danych
            dTable = new DataTable();   //
            DataColumn column;          //obiekt reprezentujacy kolumne
            DataRow row;                //obiekt reprezentujacy wiersz

            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = "F";
            dTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = "V";
            dTable.Columns.Add(column);

            //dodawanie wierszow do bazy
            for (int i = 0; i < f.GetLength(0); i += dokladnosc)
            {
                row = dTable.NewRow();
                row["V"] = i;
                row["F"] = f[i];
                dTable.Rows.Add(row);
            }
            dView = new DataView(dTable);
            //wyczyszczenie zawartosci wyrkesu
            chart.Series.Clear();
            //rysowanie
            chart.DataBindTable(dView, "V");
            chart.Series["F"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["F"].ChartArea = chart.ChartAreas[0].Name;
            //dodanie scrollbarow poziomych i pionowych
            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
        }
        private void DrawResult(float[,] f, int dokladnosc, Chart chart)
        {
            DataTable dTable;           //tablica reprezentujaca baze danych
            DataView dView;             //obiekt reprezentujacy filtr danych
            dTable = new DataTable();   //
            DataColumn column;          //obiekt reprezentujacy kolumne
            DataRow row;                //obiekt reprezentujacy wiersz

            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = "F";
            dTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = "V";
            dTable.Columns.Add(column);

            //dodawanie wierszow do bazy
            for (int i = 0; i < f.GetLength(0) - 1; i += dokladnosc)
            {
                if (f[i, (int)nudHorizont.Value] != 0)
                {
                    row = dTable.NewRow();
                    row["V"] = f[i, (int)nudHorizont.Value] / 3600;
                    row["F"] = f[i, (int)nudVertical.Value];
                    dTable.Rows.Add(row);
                }
            }
            dView = new DataView(dTable);
            //wyczyszczenie zawartosci wyrkesu
            chart.Series.Clear();
            //rysowanie
            chart.DataBindTable(dView, "V");
            chart.Series["F"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series["F"].ChartArea = chart.ChartAreas[0].Name;
            //dodanie scrollbarow poziomych i pionowych
            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;

            if (nudVertical.Value == 10)
            {
                chart.ChartAreas[0].AxisY.Title = "Napięcie [V]";
                chart.ChartAreas[0].AxisX.Title = "Czas symulacji [h]";
                chart.ChartAreas[0].AxisY.Interval = 300;
                chart.ChartAreas[0].AxisY.Minimum = 1800;

                chart.ChartAreas[0].AxisY.Maximum = 3500;
                chart.ChartAreas[0].AxisX.Interval = 0.25;
                chart.ChartAreas[0].AxisX.Minimum = 0;
                chart.ChartAreas[0].AxisX.Maximum = f.GetLength(0) / 3600f;

                Font font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
                chart.ChartAreas[0].AxisX.TitleFont = font;
                chart.ChartAreas[0].AxisY.TitleFont = font;

                chart.Series.Add("prad");
                for (int i = 0; i < f.GetLength(0) - 1; i += dokladnosc)
                {
                    if (f[i, (int)nudHorizont.Value] != 0)
                    {
                        chart.Series["prad"].Points.AddXY(f[i, 0] / 3600, f[i, 9]);
                    }
                }
                chart.Series["prad"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart.Series["prad"].YAxisType = AxisType.Secondary;
                chart.Series["prad"].Color = Color.Red;
                chart.Series["prad"].BorderWidth = 2;
                chart.ChartAreas[0].AxisY2.Minimum = 0;
                chart.ChartAreas[0].AxisY2.Maximum = 5000;
                chart.ChartAreas[0].AxisY2.Title = "Prąd [A]";
                chart.ChartAreas[0].AxisY2.TitleFont = font;
            }
            int v = (int)nudVehicle.Value;
            string name = "";
            name += projectData.ConfigurationData.InfoConfig[v];
            if (projectData.ConfigurationData.DirectionConfig[v] == Direction.Along) name += " zgodnie z km.";
            else name += " przeciwnie do km.";

            chart.Titles.Clear();
            chart.Titles.Add(name);
            chart.Legends.Clear();

        }
        private void DrawLines(float[][,] f, int dokladnosc, Chart chart, string cName, CheckedListBox clb)
        {//od maxa wersja 2
            #region maxa1
            string kierunek;
            chart.Series.Clear();
            for (int v = 0; v < f.Length; v++)
            {
                if (dataGridViewConfiguration.Rows[v].Cells[3].Value.ToString() == "Along")
                    kierunek = "zgodnie z km.";
                else
                    kierunek = "przeciwnie z km.";
                string name = " nr. poj.= " + v.ToString() + " Typ. poj: " + dataGridViewConfiguration.Rows[v].Cells[2].Value + " Relacja: " + dataGridViewConfiguration.Rows[v].Cells[1].Value +
                            " Kierunek: " + kierunek + " Godz. odj.: " + Convert.ToString(Math.Round(Convert.ToDouble(dataGridViewConfiguration.Rows[v].Cells[5].Value) / 60, 0)) + " [h] "
                            + Math.Round((Math.Abs(Math.Round(Convert.ToDouble(dataGridViewConfiguration.Rows[v].Cells[5].Value) / 60, 0) - Math.Round(Convert.ToDouble(dataGridViewConfiguration.Rows[v].Cells[5].Value) / 60, 2)) * 60), 0).ToString() + " [min.]";
                chart.Series.Add(name);
                chart.Series[name].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart.Series[name].BorderWidth = 3;

            }
            for (int i = 0; i < f[0].GetLength(0) - 1; i++)
            {
                for (int v = 0; v < f.Length; v++)
                {
                    if (dataGridViewConfiguration.Rows[v].Cells[3].Value.ToString() == "Along")
                        kierunek = "zgodnie z km.";
                    else
                        kierunek = "przeciwnie z km.";
                    string name = " nr. poj.= " + v.ToString() + " Typ. poj: " + dataGridViewConfiguration.Rows[v].Cells[2].Value + " Relacja: " + dataGridViewConfiguration.Rows[v].Cells[1].Value + " Kierunek: " + kierunek + " Godz. odj.: " + Convert.ToString(Math.Round(Convert.ToDouble(dataGridViewConfiguration.Rows[v].Cells[5].Value) / 60, 0)) + " [h] " + Math.Round((Math.Abs(Math.Round(Convert.ToDouble(dataGridViewConfiguration.Rows[v].Cells[5].Value) / 60, 0) - Math.Round(Convert.ToDouble(dataGridViewConfiguration.Rows[v].Cells[5].Value) / 60, 2)) * 60), 0).ToString() + " [min.]";
                    if (f[v][i, 1] != 0)
                    {
                        chart.Series[name].Points.AddXY(f[v][i, 0] / 3600, f[v][i, 1] / 1000);
                        chart.Series[name].BorderWidth = 3;
                    }
                }
            }
            chart.Update();
            chart.Name = cName;
            if (cName == "9")
            {
                chart.ChartAreas[0].AxisY.Minimum = 275;
                chart.ChartAreas[0].AxisY.Maximum = 320;
            }
            if (cName == "131")
            {
                chart.ChartAreas[0].AxisY.Minimum = 420;
                chart.ChartAreas[0].AxisY.Maximum = 500;
            }
            if (cName == "203")
            {
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 25;
            }
            if (cName == "728A")
            {
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 5;
            }
            if (cName == "729")
            {
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 15;
            }
            if (cName == "735")
            {
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 10;
            }
            if (cName == "260")
            {
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 20;
            }
            if (cName == "265")
            {
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 20;
            }
            //dodanie scrollbarow poziomych i pionowych
            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].AxisX.Interval = 1;
            chart.ChartAreas[0].AxisY.Interval = 5;
            chart.ChartAreas[0].AxisX.Minimum = 0;
            try
            {
                chart.ChartAreas[0].AxisX.Maximum = przejazd06.KP.MaximumTime / 3600;
            }
            catch
            {
                chart.ChartAreas[0].AxisX.Maximum = 3.0d;
            }
            chart.ChartAreas[0].AxisX.Title = "Czas symulacji [h]";
            chart.ChartAreas[0].AxisY.Title = "Droga [km]";
            Font font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
            chart.ChartAreas[0].AxisX.TitleFont = font;
            chart.ChartAreas[0].AxisY.TitleFont = font;
            for (int v = 0; v < clb.Items.Count; v++)
            {
                string name = v.ToString();
                bool state = clb.GetItemChecked(v);
                try
                {
                    if (state == false && chart.Series[name] != null)
                        chart.Series[name].Enabled = false;
                }
                catch { }
            }
            //tp check colour
            Random rnd = new Random(0);
            for (int i = 0; i < chart.Series.Count; i++)
            {
                int red = rnd.Next(255);
                int green = rnd.Next(255);
                int blue = rnd.Next(255);
                this.chartLines.Series[i].Color = Color.FromArgb(red, green, blue);
                this.dataGridViewConfiguration.Rows[i].DefaultCellStyle.BackColor = Color.White;
                this.dataGridViewConfiguration.Rows[i].Cells[7].Style.BackColor = Color.FromArgb(red, green, blue); 
            }

            chart.Legends.Clear();
            #endregion
            
            #region maxa1
            //od maxa wersja 1
            /*
            string dir;
            chart.Series.Clear();
            for (int v = 0; v < f.Length; v++)
            {
                if (projectData.ConfigurationData.DirectionConfig[v] == Direction.Along)
                    dir = "zgodnie z km.";
                else
                    dir = "przeciwnie z km.";

                string name = v.ToString() + " " + projectData.ConfigurationData.VehicleConfig[v] + " " + projectData.ConfigurationData.RouteConfig[v] + " " + dir;

                chart.Series.Add(name);
                chart.Series[name].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            }

            string[] names = new string[f[0].GetLength(0)];
            for (int i = 0; i < f[0].GetLength(0) - 1; i++)
            {
                for (int v = 0; v < f.Length; v++)
                {
                    if (projectData.ConfigurationData.DirectionConfig[v] == Direction.Along)
                        dir = "zgodnie z km.";
                    else
                        dir = "przeciwnie z km.";
                    string name = v.ToString() + " " + projectData.ConfigurationData.VehicleConfig[v] + " " + projectData.ConfigurationData.RouteConfig[v] + " " + dir;
                    names[v] = name;
                    if (f[v][i, 1] != 0)
                        chart.Series[name].Points.AddXY(f[v][i, 0] / 3600, f[v][i, 1] / 1000);
                }
            }
            chart.Update();
            chart.Name = cName;
            //dodanie scrollbarow poziomych i pionowych
            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].AxisX.Interval = 0.25;
            chart.ChartAreas[0].AxisY.Interval = 5;
            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Maximum = przejazd06.KP.MaximumTime / 3600f;
            chart.ChartAreas[0].AxisX.Title = "Czas symulacji [h]";
            chart.ChartAreas[0].AxisY.Title = "Droga [km]";
            for (int v = 0; v < clb.Items.Count; v++)
            {
                string name = names[v];
                bool state = clb.GetItemChecked(v);
                try
                {
                    if (state == false && chart.Series[name] != null)
                        chart.Series[name].Enabled = false;
                }
                catch { }
            }
            */
            #endregion
            #region moje starre
            /*
            chart.Series.Clear();
            for (int v = 0; v < f.Length; v++)
            {
                string name = v.ToString();
                chart.Series.Add(name);
                chart.Series[name].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            }

            for (int i = 0; i < f[0].GetLength(0) - 1; i++)
            {
                for (int v = 0; v < f.Length; v++)
                {
                    string name = v.ToString();
                    if (f[v][i, 1] != 0)
                        chart.Series[name].Points.AddXY(f[v][i, 0], f[v][i, 1]);
                }
            }
            chart.Update();
            chart.Name = cName;
            //dodanie scrollbarow poziomych i pionowych
            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Maximum = przejazd06.KP.MaximumTime;
            for (int v = 0; v < clb.Items.Count; v++)
            {
                string name = v.ToString();
                bool state = clb.GetItemChecked(v);
                try
                {
                    if (state == false && chart.Series[name] != null)
                        chart.Series[name].Enabled = false;
                }
                catch { }
            }*/
            #endregion
        }
        private void DrawSupply(float[,] f, int dokladnosc, Chart chart)
        {
            string name = przejazd06.R.SuppliesNames[(int)nudSupply.Value];
            DataTable dTable;           //tablica reprezentujaca baze danych
            DataView dView;             //obiekt reprezentujacy filtr danych
            dTable = new DataTable();   //
            DataColumn column;          //obiekt reprezentujacy kolumne
            DataRow row;                //obiekt reprezentujacy wiersz

            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = "t";
            dTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = name;
            dTable.Columns.Add(column);

            //dodawanie wierszow do bazy
            for (int i = 0; i < f.GetLength(0) - 1; i += dokladnosc)
            {
                row = dTable.NewRow();
                float x = f[i, 0];
                float y= f[i, 1];
                if (y > 3400) y = 3400;
                if (y < 0) y = 0;
                row["t"] = x;
                row[name] = y;
                dTable.Rows.Add(row);
            }
            dView = new DataView(dTable);
            //wyczyszczenie zawartosci wyrkesu
            chart.Series.Clear();
            //rysowanie
            chart.DataBindTable(dView, "t");
            chart.Series[name].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart.Series[name].ChartArea = chart.ChartAreas[0].Name;
            //dodanie scrollbarow poziomych i pionowych
            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
        }
        private void DrawCurrents(float[,][,] f, int supToShow, int dokladnosc, Chart chart)
        {
            chart.Series.Clear();
            for(int z = 0; z < 8; z++)
            {
                chart.Series.Add(z.ToString());
                chart.Series[z].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            }           

            for (int i = 0; i < f[0, 0].GetLength(0) - 1; i++)
            {
                for (int z = 0; z < 8; z++)
                {
                    if (f[supToShow, z][i, 1] != 0)
                    {
                        float x = f[supToShow, z][i, 0];
                        float y = f[supToShow, z][i, 1];
                        if (y > 3400) y = 3400;
                        if (y < 0) y = 0;
                        chart.Series[z.ToString()].Points.AddXY(x, y);
                    }
                }
            }
            chart.Update();
            string name = przejazd06.R.SuppliesNames[supToShow];
            chart.Name = name;
            //dodanie scrollbarow poziomych i pionowych
            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;

        }
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.tabControl.Enabled = true;
            this.buttonVamos.Enabled = true;

            try
            {
                tbSimTimeHours.Text = projectData.SimTime.Hours.ToString();
                tbSimTimeMinutes.Text = projectData.SimTime.Minutes.ToString();
                tbDeltaT.Text = projectData.DeltaT.ToString();

                tbInitialTimeH.Text = projectData.InitialTime.Hours.ToString();
                tbInitialTimeM.Text = projectData.InitialTime.Minutes.ToString();
            }
            catch
            {
                richTextBox.Text += " Błąd ładowania danych czasowych";
            }
            try
            {
                tbBreakingDistance.Text = projectData.BreakingDistance.ToString();
                projectData.ConfigurationData.BreakingDistance = projectData.BreakingDistance;
            }
            catch
            {
                richTextBox.Text += " Błąd ładowania innych danych";
            }
        }

        private void ButtonVamos_Click(object sender, EventArgs e)
        {
            try
            {
                PrepatationForVamos(true);
            }
            catch
            {
                richTextBox.Text = "Błąd w przygotowaniu do przejazdu";
            }
            try
            {
                przejazd06 = new Przejazd06(projectData.DeltaT, projectData.SimTime.OnlySeconds, projectData.InitialTime, calculationObjects, projectData.ConfigurationData, rbElectrical.Checked, false);
            }
            catch
            {
                richTextBox.Text = "Błąd przejazdu";
            }
            startI = 0;

            backgroundWorkerVamos.WorkerReportsProgress = true;
            backgroundWorkerVamos.WorkerSupportsCancellation = true;
            backgroundWorkerVamos.RunWorkerAsync();
        }
        private void PrepatationForVamos(bool ifBeforePass)
        {
            if (przejazd06 != null)
                if (przejazd06.Branches != null)
                    przejazd06.Branches.Clear();
            ConfigurationData data = new ConfigurationData();
            LoadProjectDataFromForm();
            LoadsPIDFromForm();
            //LoadsPIDFromForm();
            data = projectData.ConfigurationData;
            Lists l = new Lists();
            //aktualizuje dane czasowe symulacji
            Time t = new Time();
            t = Time.GetTime(int.Parse(tbSimTimeHours.Text.ToString()), int.Parse(tbSimTimeMinutes.Text.ToString()));
            projectData.SimTime = t;
            projectData.DeltaT = float.Parse(tbDeltaT.Text.ToString());
            //przepisuje do obiektu calcObjects liste pojazdow,tras i kierunkow ischecekd=true jako tablice
            for (int i = 0; i < data.IsCheckedConfig.Count; i++)
            {
                if (data.IsCheckedConfig[i] == true)
                {
                    string name = data.VehicleConfig[i];
                    var v = data.Vehicles.FirstOrDefault(ve => ve.Name == name);
                    l.Vl.Add(v);

                    string route = data.RouteConfig[i];
                    var r = data.Routes.FirstOrDefault(ro => ro.Name == route);
                    l.Rl.Add(r);

                    Direction d = data.DirectionConfig[i];
                    l.Dl.Add(d);
                }
            }
            //robi tablice o wymiarze takim, ile jest pojazdow w przejezdzie
            calculationObjects = new CalculationObjects(l.Vl);
            //robi tablice Thingów, profili i limitow przez ktore jechal bedzie kazdy pojazd
            try
            {
                calculationObjects.GetThings(l, data, ifBeforePass);
            }
            catch { richTextBox.Text = "Błąd ładowania Rzeczy"; }
            //okresla maksymalna sile napedowa dla kazdego z pojazdow
            for (int v = 0; v < calculationObjects.Vehicles.Length; v++)
            {
                float maxf = 0;
                if (calculationObjects.Vehicles[v] != null)
                {
                    for (int f = 0; f < calculationObjects.Vehicles[v].Force.Length; f++)
                    {
                        if (calculationObjects.Vehicles[v].Force[f] > maxf) maxf = calculationObjects.Vehicles[v].Force[f];
                    }
                    calculationObjects.Vehicles[v].MaxForce = maxf;
                }
            }
        }
        private int startI;
        private void Vamos(object sender, DoWorkEventArgs e)
        {
            float[] distInit = new float[projectData.ConfigurationData.IsCheckedConfig.Count];
            float[] spInit = new float[projectData.ConfigurationData.IsCheckedConfig.Count];
            for (int v = 0; v < projectData.ConfigurationData.IsCheckedConfig.Count; v++)
            {
                if (projectData.ConfigurationData.IsCheckedConfig[v] == true)
                {
                    if (projectData.ConfigurationData.DirectionConfig[v] == Direction.Along) distInit[v] = calculationObjects.StopsDist[v][0];
                    if (projectData.ConfigurationData.DirectionConfig[v] == Direction.Opposite) distInit[v] = calculationObjects.StopsDist[v][calculationObjects.StopsDist[v].Length - 1];
                }
            }
            int simLength = projectData.SimTime.OnlySeconds;
            for (int i = startI; i < simLength; i++)
            {
                if (i == 0)
                    przejazd06.MakeFirstStep(distInit, spInit);
                else
                    przejazd06.MakeStep(i);
                int percentage = 100 * i / projectData.SimTime.OnlySeconds;
                if (percentage % 1 == 0)
                    backgroundWorkerVamos.ReportProgress(percentage);

                if (backgroundWorkerVamos.CancellationPending)
                {
                    break;
                }

                if (rbSavePartial.Checked || true)
                {
                    for (int a = 1; a < 36; a++)
                    {
                        int d = a * 10 * 60;
                        if (i == d)
                        {
                            int minutes = a * 10;
                            string time = minutes.ToString();
                            ExportPartial(time);
                        }
                    }
                }
            }
        }
        private void PopulateResults()
        {
            try
            {
                DrawResult(przejazd06.PT[(int)nudVehicle.Value], 1, chResults);
                dataGridViewResults.ColumnCount = 25;
                dataGridViewResults.RowCount = przejazd06.PT[(int)nudVehicle.Value].GetLength(0) - 1;
                for (int i = 0; i < dataGridViewResults.RowCount - 1; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        dataGridViewResults[j, i].Value = przejazd06.PT[(int)nudVehicle.Value][i, j];
                    }
                    for (int j = 0; j < 5; j++)
                    {
                        dataGridViewResults[j + 20, i].Value = przejazd06.Info[(int)nudVehicle.Value][i, j];
                    }
                }

            }
            catch
            {
                richTextBox.Text = "Brak przejazdu do pokazania";
            }
            try
            {
                int supToShow = (int)nudSupply.Value;
                int suppliesCount = 8;
                dataGridViewSupply.Rows.Clear();
                dataGridViewSupply.RowCount = przejazd06.R.VoltSup[supToShow].GetLength(0);
                dataGridViewSupply.ColumnCount = suppliesCount + 2;
                DrawSupply(przejazd06.R.VoltSup[supToShow], 1, chSupplyVolt);
                DrawCurrents(przejazd06.R.CurrSup, supToShow, 1, chSupplyCurrents);

                for (int i = 0; i < dataGridViewSupply.RowCount - 1; i++)
                {
                    dataGridViewSupply[0, i].Value = przejazd06.R.VoltSup[supToShow][i, 0].ToString();
                    dataGridViewSupply[1, i].Value = przejazd06.R.VoltSup[supToShow][i, 1].ToString();
                    for(int z = 0; z < suppliesCount; z++)
                    {
                        dataGridViewSupply[2 + z, i].Value = przejazd06.R.CurrSup[supToShow, z][i, 1];
                    }
                }
            }
            catch { }
        }

        private void DataGridViewConfiguration_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int selectedPass = 0;
            if (dataGridViewConfiguration.CurrentCell != null)
                selectedPass = dataGridViewConfiguration.CurrentCell.RowIndex;
            if (selectedPass < projectData.ConfigurationData.StopConfig.Count)
                UpdateDGVStopsConfiguration(selectedPass);
        }
        private void RefreshStopsConfiguration(int passNumber)
        {
            StopsConfiguration sc = new StopsConfiguration(projectData.ConfigurationData.StopConfig[passNumber].StopsChecked.Length);
            for (int i = 0; i < sc.StopsChecked.Length; i++)
            {
                sc.StopsNames[i] = dataGridViewStops.Rows[i].Cells[0].Value.ToString();
                sc.StopsChecked[i] = bool.Parse(dataGridViewStops.Rows[i].Cells[1].Value.ToString());
                sc.StopsTimes[i] = int.Parse(dataGridViewStops.Rows[i].Cells[2].Value.ToString());
            }
            projectData.ConfigurationData.StopConfig[passNumber] = sc;
        }
        private void UpdateDGVStopsConfiguration(int passNumber)
        {
            if (passNumber < projectData.ConfigurationData.StopConfig.Count)
            {
                dataGridViewStops.RowCount = projectData.ConfigurationData.StopConfig[passNumber].StopsChecked.Length;

                for (int i = 0; i < projectData.ConfigurationData.StopConfig[passNumber].StopsChecked.Length; i++)
                {
                    dataGridViewStops.Rows[i].Cells[0].Value = projectData.ConfigurationData.StopConfig[passNumber].StopsNames[i];
                    dataGridViewStops.Rows[i].Cells[1].Value = projectData.ConfigurationData.StopConfig[passNumber].StopsChecked[i];
                    dataGridViewStops.Rows[i].Cells[2].Value = projectData.ConfigurationData.StopConfig[passNumber].StopsTimes[i];
                }
            }
        }
        private void BackgroundWorkerVamos_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PopulateResults();
            progressBar.Value = 0;
            richTextBox.Text = "Symulacja ukończona";
            CreateLines();
            GetBranches();
            ExportPartial("koniec");
        }
        private void GetBranches()
        {
            try
            {
                dataGridViewBranches.Rows.Clear();
                dataGridViewBranches.ColumnCount = 8;
                dataGridViewBranches.Rows.Add(przejazd06.Branches.Count);
                for (int i = 0; i < przejazd06.Branches.Count; i++)
                {
                    dataGridViewBranches[0, i].Value = przejazd06.Branches[i].BranchNumber;
                    dataGridViewBranches[1, i].Value = przejazd06.Branches[i].NodeIn;
                    dataGridViewBranches[2, i].Value = przejazd06.Branches[i].NodeOut;
                    dataGridViewBranches[3, i].Value = przejazd06.Branches[i].Resistance;
                    dataGridViewBranches[4, i].Value = przejazd06.Branches[i].Voltage;
                    dataGridViewBranches[5, i].Value = przejazd06.Branches[i].Current;
                    dataGridViewBranches[6, i].Value = przejazd06.Branches[i].CurrentResult;
                    dataGridViewBranches[7, i].Value = przejazd06.Branches[i].VoltageResult;
                }
            }
            catch { }
        }

        private void BackgroundWorkerVamos_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            int seconds = e.ProgressPercentage * projectData.SimTime.OnlySeconds / 100;
            Time t = Time.GetTime(seconds);
            if (seconds % 1 == 0)
                richTextBox.Text = "Ukończony czas symulacji... " + t.Hours.ToString() + " godzin " + t.Minutes.ToString() + " minut " + t.Seconds.ToString() + " sekund"                + "\nz " + projectData.SimTime.Hours.ToString() + " godzin " + projectData.SimTime.Minutes.ToString() + " minut " + projectData.SimTime.Seconds.ToString() + " sekund";
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = tabControl.SelectedIndex;
            switch (selectedIndex)
            {
                case 1:
                    buttonFunction.Text = "Funkcja";
                    break;
                case 2:
                    buttonFunction.Text = "Funkcja";
                    break;
                case 3:
                    buttonFunction.Text = "Funkcja";
                    break;
                case 4:
                    buttonFunction.Text = "Importuj stacje i szlaki z profili";
                    break;
                case 5:
                    buttonFunction.Text = "Czyść konfigurację";
                    int selectedPass = 0;
                    if (dataGridViewConfiguration.CurrentCell != null)
                        selectedPass = dataGridViewConfiguration.CurrentCell.RowIndex;
                    PrepatationForVamos(false);
                    UpdateDGVStopsConfiguration(selectedPass);
                    break;
                case 6:
                    buttonFunction.Text = "Funkcja";
                    break;
            }

        }
        private void DataGridViewStops_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter) && dataGridViewConfiguration.CurrentCell != null && dataGridViewConfiguration.CurrentCell.RowIndex < projectData.ConfigurationData.StopConfig.Count)
            {
                RefreshStopsConfiguration(dataGridViewConfiguration.CurrentCell.RowIndex);
            }
        }

        private void DataGridViewConfiguration_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0)
            {

                int selectedPass = dataGridViewConfiguration.CurrentCell.RowIndex;
                if (dataGridViewConfiguration.Rows[selectedPass].Cells[1].Value != null)
                {
                    PrepatationForVamos(false);
                    int n = 0;
                    n = calculationObjects.StopsDist[selectedPass].Length;
                    StopsConfiguration sc = new StopsConfiguration(n);
                    n = 0;
                    for (int t = 0; t < calculationObjects.ThingNames[selectedPass].Length; t++)
                    {
                        string name = calculationObjects.ThingNames[selectedPass][t];
                        var th = projectData.ConfigurationData.Things.FirstOrDefault(thi => thi.Name == name);
                        if (th.ThingType == ThingType.Station)
                        {
                            sc.StopsNames[n] = th.Name;
                            sc.StopsTimes[n] = 60;
                            sc.StopsChecked[n] = true;
                            n++;
                        }
                    }
                    if (e.RowIndex + 1 > projectData.ConfigurationData.StopConfig.Count) projectData.ConfigurationData.StopConfig.Add(sc);
                    else projectData.ConfigurationData.StopConfig[selectedPass] = sc;
                }
            }
        }

        bool isChecked = false;
        private void RbElectrical_Click(object sender, EventArgs e)
        {
            if (rbElectrical.Checked && !isChecked)
                rbElectrical.Checked = false;
            else
            {
                rbElectrical.Checked = true;
                isChecked = false;
            }
        }
        private void RbElectrical_CheckedChanged(object sender, EventArgs e)
        {
            isChecked = rbElectrical.Checked;
        }

        private void DataGridViewConfiguration_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            int index = e.RowIndex;
            try
            {
                if (index >= 0 && projectData.ConfigurationData.StopConfig != null && projectData.ConfigurationData.StopConfig.Count != 0)
                    projectData.ConfigurationData.StopConfig.RemoveAt(index);
            }
            catch { }
        }

        private void BAtTime_Click(object sender, EventArgs e)
        {
            Time atTime = new Time();
            int hours = int.Parse(tbAtHour.Text.ToString());
            int minutes = int.Parse(tbAtMinute.Text.ToString());
            atTime = Time.GetTime(hours, minutes);
            dataGridViewVehiclesPositions.RowCount = dataGridViewConfiguration.RowCount;
            try
            {
                for (int i = 0; i < przejazd06.PT[0].GetLength(0); i++)
                {
                    if (przejazd06.PT[0][i, 0] == atTime.OnlySeconds)
                        for (int v = 0; v < projectData.ConfigurationData.VehicleConfig.Count; v++)
                        {
                            if (projectData.ConfigurationData.IsCheckedConfig[v] == true)
                            {
                                dataGridViewVehiclesPositions[0, v].Value = v.ToString();
                                dataGridViewVehiclesPositions[1, v].Value = przejazd06.Info[v][i, 2];
                                dataGridViewVehiclesPositions[2, v].Value = przejazd06.Info[v][i, 3];
                                dataGridViewVehiclesPositions[3, v].Value = przejazd06.Info[v][i, 0];
                                dataGridViewVehiclesPositions[4, v].Value = przejazd06.Info[v][i, 1];
                                dataGridViewVehiclesPositions[5, v].Value = przejazd06.PT[v][i, 16].ToString();
                            }
                        }
                }
            }
            catch { }
        }
        private void CreateLines()
        {
            clbVehsToShow.Items.Clear();
            int vehs = przejazd06.PT.Length;
            for (int v = 0; v < vehs; v++)
            {
                clbVehsToShow.Items.Add(v);
                clbVehsToShow.SetItemChecked(v, true);
            }

            line = new Lines(cbLineToPresent.Items.Count, przejazd06.PT.Length, projectData.SimTime.OnlySeconds);
            for (int n = 0; n < cbLineToPresent.Items.Count; n++)
            {
                line.Names[n] = cbLineToPresent.Items[n].ToString();
            }

            for (int i = 0; i < projectData.SimTime.OnlySeconds; i++)
            {
                for (int l = 0; l < line.Line.Length; l++)
                {
                    string lineName = line.Names[l];
                    for (int v = 0; v < line.Line[l].Length; v++)
                    {
                        for (int n = 0; n < cbLineToPresent.Items.Count; n++)
                        {
                            if (przejazd06.Info[v][i, 2] == lineName)
                            {
                                line.Line[l][v][i, 0] = przejazd06.PT[v][i, 0];
                                line.Line[l][v][i, 1] = float.Parse(przejazd06.Info[v][i, 3].ToString());
                            }
                            if (przejazd06.Info[v][i, 2] == lineName + "wstecz")
                            {
                                line.Line[l][v][i, 0] = przejazd06.PT[v][i, 0];
                                var p = projectData.ConfigurationData.Profiles.FirstOrDefault(pr => pr.Name == lineName);
                                float dist = float.Parse(przejazd06.Info[v][i, 3].ToString());
                                float lineLength = p.Stations[p.Stations.Length - 1].Position;
                                float realDist = Math.Abs(lineLength - dist);
                                line.Line[l][v][i, 1] = realDist;
                            }
                        }
                    }
                }
            }
            try
            {
                for (int v = 0; v < vehs; v++)
                {
                    line.ManualDelays[v] = new float[przejazd06.KP.CO.StopsTimes[v].Length];
                    for (int s = 0; s < line.ManualDelays[v].Length; s++) line.ManualDelays[v][s] = 0;
                }
            }
            catch { }
        }
        private void CbLineToPresent_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string lName1;
                string lName2;
                string selectedLine = cbLineToPresent.Text.ToString();
                lName1 = selectedLine;
                lName2 = lName1 + "wstecz";
                int index = 0;
                for (int n = 0; n < line.Names.Length; n++)
                {
                    if (lName1 == line.Names[n])
                    {
                        index = n;
                        break;
                    }
                }
                DrawLines(line.Line[index], 1, chartLines, lName1, clbVehsToShow);
            }
            catch { }
        }

        private void ClbVehsToShow_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                string lName1;
                string selectedLine = cbLineToPresent.Text.ToString();
                lName1 = selectedLine;
                int index = 0;
                for (int n = 0; n < line.Names.Length; n++)
                {
                    if (lName1 == line.Names[n])
                    {
                        index = n;
                        break;
                    }
                }
                DrawLines(line.Line[index], 1, chartLines, lName1, clbVehsToShow);

            }
            catch { }
        }

        private void TbManualDelay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    int v = int.Parse(clbVehsToShow.SelectedItem.ToString());
                    float delay = 0;
                    if (tbManualDelay.Text.ToString() != null)
                        delay = 60 * float.Parse(tbManualDelay.Text.ToString());
                    for (int i = 1; i < przejazd06.KP.MaximumTime; i++)
                    {
                        for (int l = 0; l < line.Line.Length; l++)
                        {
                            line.Line[l][v][i, 0] += delay;
                            if (projectData.ConfigurationData.DirectionConfig[v] == Direction.Along)
                                for (int s = 0; s < dataGridViewManualStops.RowCount; s++)
                                {
                                    int stopDelay = int.Parse(dataGridViewManualStops[1, s].Value.ToString());
                                    if (przejazd06.Info[v][i, 4] != null)
                                    {
                                        int nextStop = int.Parse(przejazd06.Info[v][i, 4]);
                                        if (nextStop > s + 1)
                                        {
                                            line.Line[l][v][i, 0] += stopDelay;
                                        }
                                    }
                                }
                            else
                                for (int s = dataGridViewManualStops.RowCount - 1; s > 0; s--)
                                {
                                    int stopDelay = int.Parse(dataGridViewManualStops[1, s - 1].Value.ToString());
                                    if (przejazd06.Info[v][i, 4] != null)
                                    {
                                        int nextStop = int.Parse(przejazd06.Info[v][i, 4]);
                                        if (nextStop < s - 1)
                                        {
                                            line.Line[l][v][i, 0] += stopDelay;
                                        }
                                    }
                                }
                        }
                    }

                    string lName1;
                    string selectedLine = cbLineToPresent.Text.ToString();
                    lName1 = selectedLine;
                    int index = 0;
                    for (int n = 0; n < line.Names.Length; n++)
                    {
                        if (lName1 == line.Names[n])
                        {
                            index = n;
                            break;
                        }
                    }

                    for (int s = 0; s < dataGridViewManualStops.RowCount; s++)
                    {
                        int newDelay = int.Parse(dataGridViewManualStops[1, s].Value.ToString());
                        int oldDelay = int.Parse(dataGridViewManualStops[2, s].Value.ToString());
                        line.ManualDelays[v][s] = oldDelay + newDelay;
                        dataGridViewManualStops[2, s].Value = line.ManualDelays[v][s];
                        dataGridViewManualStops[1, s].Value = 0;
                    }

                    DrawLines(line.Line[index], 1, chartLines, lName1, clbVehsToShow);
                }
                catch { }                
            }
        }

        private void ClbVehsToShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int v = int.Parse(clbVehsToShow.SelectedItem.ToString());
                dataGridViewManualStops.RowCount = przejazd06.KP.CO.StopsTimes[v].Length;
                for (int s = 0; s < dataGridViewManualStops.RowCount; s++)
                {
                    dataGridViewManualStops[0, s].Value = s.ToString();
                    dataGridViewManualStops[1, s].Value = 0;
                    dataGridViewManualStops[2, s].Value = line.ManualDelays[v][s];
                }
            }
            catch { }
        }

        bool changeNodeReverse = false;
        private void LbNodesInRoute_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbNodesInRoute.SelectedIndex >= 0)
            {
                try
                {
                    if (clbLeft.Items.Count > 0) clbLeft.SelectedIndex = lbNodesInRoute.SelectedIndex;
                    if (clbRight.Items.Count > 0) clbRight.SelectedIndex = lbNodesInRoute.SelectedIndex;
                }
                catch { }
            }
        }

        private void ClbLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateReverse(changeNodeReverse);
        }
        private void ClbRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateReverse(changeNodeReverse);
        }

        private void ClbLeft_MouseClick(object sender, MouseEventArgs e)
        {
            changeNodeReverse = true;
        }

        private void ClbRight_MouseClick(object sender, MouseEventArgs e)
        {
            changeNodeReverse = true;
        }

        private void BExport_Click(object sender, EventArgs e)
        {
            ExportFunction();
        }
        public void ExportFunction()
        {
            try
            {
                for (int i = 0; i < przejazd06.PT.Length; i++)
                {
                    nudVehicle.Value = i;
                    string name = projectData.ConfigurationData.InfoConfig[i];
                    PopulateResults();
                    ExportToExcel(name, dataGridViewResults);
                }
                for (int i = 0; i < przejazd06.R.VoltSup.Length; i++)
                {
                    nudSupply.Value = i;
                    string name = przejazd06.R.SuppliesNames[(int)nudSupply.Value];
                    PopulateResults();
                    ExportToExcel(name, dataGridViewSupply);
                }
            }
            catch { }
        }
        public void ExportToExcel(string name, DataGridView dgv)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xls)|*.xls";
            sfd.FileName = name;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // Copy DataGridView results to clipboard
                copyAlltoClipboard(dgv);

                object misValue = System.Reflection.Missing.Value;
                Excel.Application xlexcel = new Excel.Application();

                xlexcel.DisplayAlerts = false; // Without this you will get two confirm overwrite prompts
                Excel.Workbook xlWorkBook = xlexcel.Workbooks.Add(misValue);
                Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                // Format column D as text before pasting results, this was required for my data
                Excel.Range rng = xlWorkSheet.get_Range("D:D").Cells;
                rng.NumberFormat = "@";

                // Paste clipboard results to worksheet range
                Excel.Range CR = (Excel.Range)xlWorkSheet.Cells[1, 1];
                CR.Select();
                xlWorkSheet.PasteSpecial(CR, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, true);

                // For some reason column A is always blank in the worksheet. ¯\_(ツ)_/¯
                // Delete blank column A and select cell A1
                Excel.Range delRng = xlWorkSheet.get_Range("A:A").Cells;
                delRng.Delete(Type.Missing);
                xlWorkSheet.get_Range("A1").Select();

                // Save the excel file under the captured location from the SaveFileDialog
                xlWorkBook.SaveAs(sfd.FileName, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                xlexcel.DisplayAlerts = true;
                xlWorkBook.Close(true, misValue, misValue);
                xlexcel.Quit();

                releaseObject(xlWorkSheet);
                releaseObject(xlWorkBook);
                releaseObject(xlexcel);

                // Clear Clipboard and DataGridView selection
                Clipboard.Clear();
                dataGridViewResults.ClearSelection();

                // Open the newly saved excel file
                if (File.Exists(sfd.FileName))
                    System.Diagnostics.Process.Start(sfd.FileName);
            }
        }
        private void copyAlltoClipboard(DataGridView dgv)
        {
            dgv.SelectAll();
            DataObject dataObj = dgv.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }
        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occurred while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        private void BBinImport_Click(object sender, EventArgs e)
        {
            string pathPT = tbBinImportPath.Text;
            string pathR = tbBinImportResults.Text;
            string pathInfo = tbBinImportInfo.Text;

            try
            {
                przejazd06 = new Przejazd06();
                this.przejazd06.PT = Operations.ReadFromBinaryFile<float[][,]>(pathPT);
            }
            catch
            {
                this.richTextBox.Text = "Błąd odczytu pliku binarnego PT";
            }
            try
            {
                przejazd06.R = new Results();
                this.przejazd06.R = Operations.ReadFromBinaryFile<Results>(pathR);
            }
            catch { this.richTextBox.Text = "Błąd odczytu pliku binarnego R"; }
            try
            {
                this.przejazd06.Info = Operations.ReadFromBinaryFile<string[][,]>(pathInfo);
            }
            catch { this.richTextBox.Text = "Błąd odczytu pliku binarnego Info"; }
            CreateLines();
        }
        private void ExportPartial(string time)
        {
            string path = "C:\\Users\\Paweł\\ + time;";
            string namePT = " przejazd";
            string nameR = " results";
            string nameI = " info";

            try
            {
                Operations.WriteToBinaryFile<float[][,]>(path + namePT + ".bin", przejazd06.PT);
            }
            catch
            {
                //this.richTextBox.Text = "Błąd zapisu pliku binarnego PT";
            }
            try
            {

                Operations.WriteToBinaryFile<Results>(path + nameR + ".bin", przejazd06.R);
            }
            catch {  }
            try
            {
                Operations.WriteToBinaryFile <string[][,] > (path + nameI + ".bin", przejazd06.Info);
            }
            catch { }
        }

        private void BContinue_Click(object sender, EventArgs e)
        {
            try
            {
                PrepatationForVamos(true);
                Przejazd06 temp = przejazd06;
                przejazd06 = new Przejazd06(projectData.DeltaT, projectData.SimTime.OnlySeconds, projectData.InitialTime, calculationObjects, projectData.ConfigurationData, rbElectrical.Checked, true);
                przejazd06.PT = temp.PT;
                przejazd06.R = temp.R;
                przejazd06.Info = temp.Info;

                startI = int.Parse(tbStartI.Text.ToString());
                for (int v = 0; v < przejazd06.PT.Length; v++)
                {
                    przejazd06.GetObject(v, startI);
                }
            }
            catch { }

            backgroundWorkerVamos.WorkerReportsProgress = true;
            backgroundWorkerVamos.WorkerSupportsCancellation = true;
            backgroundWorkerVamos.RunWorkerAsync();
        }

        private void BExportJPG_Click(object sender, EventArgs e)
        {
            int height = chResults.Height;
            int width = chResults.Width;
            for (int i = 0; i < projectData.ConfigurationData.VehicleConfig.Count; i++)
            {
                nudVehicle.Value = i;
                DrawResult(przejazd06.PT[(int)nudVehicle.Value], 1, chResults);
                string name = "";
                try
                {
                    string kierunek = "";
                    int v = (int)nudVehicle.Value;
                    if (dataGridViewConfiguration.Rows[v].Cells[3].Value.ToString() == "Along")
                        kierunek = "zgodnie z km.";
                    else
                        kierunek = "przeciwnie z km.";


                    name = " nr. poj.= " + v.ToString() + " Typ. poj " + dataGridViewConfiguration.Rows[v].Cells[2].Value + " Relacja " + dataGridViewConfiguration.Rows[v].Cells[1].Value +

                                " Kierunek " + kierunek + " Godz. odj. " + Convert.ToString(Math.Round(Convert.ToDouble(dataGridViewConfiguration.Rows[v].Cells[5].Value) / 60, 0)) + " [h] "

                                + Math.Round((Math.Abs(Math.Round(Convert.ToDouble(dataGridViewConfiguration.Rows[v].Cells[5].Value) / 60, 0) - Math.Round(Convert.ToDouble(dataGridViewConfiguration.Rows[v].Cells[5].Value) / 60, 2)) * 60), 0).ToString() + " [min.]";
                }
                catch { }
                try
                {
                    string path = "C:\\Users\\Paweł\\";
                    chResults.Width = 1920;
                    chResults.Height = 1080;
                    this.chResults.SaveImage(path + "\\" + name + ".tiff", ChartImageFormat.Tiff);
                }
                catch { }
            }
            chResults.Width = width;
            chResults.Height = height;
        }
        private void MeanVoltageVehicles()
        {
            try
            {
                dgvMeanVoltVeh.RowCount = projectData.ConfigurationData.VehicleConfig.Count;
                float[] meanVoltage = new float[projectData.ConfigurationData.VehicleConfig.Count];
                float[] meanCurrent = new float[projectData.ConfigurationData.VehicleConfig.Count];
                float[] meanPower = new float[projectData.ConfigurationData.VehicleConfig.Count];
                int[] below3000 = new int[projectData.ConfigurationData.VehicleConfig.Count];
                int[] below2700 = new int[projectData.ConfigurationData.VehicleConfig.Count];
                int[] stepsCount = new int[meanVoltage.Length];

                for (int i = 0; i < przejazd06.PT[0].GetLength(0); i++)
                    for (int v = 0; v < meanVoltage.Length; v++)
                    {
                        if (przejazd06.PT[v][i, 1] > 0) stepsCount[v]++;
                    }

                for (int i = 0; i < przejazd06.PT[0].GetLength(0); i++)
                    for (int v = 0; v < meanVoltage.Length; v++)
                    {
                        if (przejazd06.PT[v][i, 1] > 0)
                        {
                            meanPower[v] += (przejazd06.PT[v][i, 10] * przejazd06.PT[v][i, 9]) / stepsCount[v];
                            meanCurrent[v] += przejazd06.PT[v][i, 9] / stepsCount[v];
                            if (przejazd06.PT[v][i, 10] < 3000) below3000[v]++;
                            if (przejazd06.PT[v][i, 10] < 2700) below2700[v]++;
                        }
                    }
                for (int v = 0; v < meanPower.Length; v++)
                {
                    meanVoltage[v] = meanPower[v] / meanCurrent[v];
                }

                for (int v = 0; v < projectData.ConfigurationData.VehicleConfig.Count; v++)
                {
                    float b3 = (float)below3000[v] / stepsCount[v] * 100f;
                    float b27 =(float)below2700[v] / stepsCount[v] * 100f;
                    dgvMeanVoltVeh[0, v].Value = projectData.ConfigurationData.InfoConfig[v];
                    dgvMeanVoltVeh[1, v].Value = meanVoltage[v];
                    dgvMeanVoltVeh[2, v].Value = b3;
                    dgvMeanVoltVeh[3, v].Value = b27;
                }
            }
            catch { }
        }
        private void MeanZone()
        {
            float[][][] zones = new float[6][][];
            string[][] names = new string[6][];
            string[] zoneNames;
            float[][] zoneDistances = new float[2][];
            string[] zoneLines;
            float[] zoneMeanVoltage;
            float[] zoneMeanCurrent;
            float[] zoneMeanPower;
            int[] below3000;
            int[] below2700;
            int[] totalIter;
            Operations.LoadExcelFile(projectData.PathPowerSystems, ref zones, ref names);
            int count = 0;
            for (int l = 0; l < 1; l++)
                count += names[l].Length;
            zoneNames = new string[count];
            zoneMeanVoltage = new float[count];
            zoneMeanCurrent = new float[count];
            zoneMeanPower = new float[count];
            zoneDistances[0] = new float[count];
            zoneDistances[1] = new float[count];
            zoneLines = new string[count];
            below3000 = new int[count];
            below2700 = new int[count];
            totalIter = new int[count];
            int[] zoneIterNumber = new int[count];
            dgvZones.RowCount = count;
            count = 0;
            for (int l = 0; l < 1; l++)
            {
                for (int z = 0; z < names[l].Length; z++)
                {
                    zoneNames[count] = names[l][z];
                    zoneDistances[0][count] = zones[l][0][z];
                    zoneDistances[1][count] = zones[l][1][z];
                    string lineName = "";
                    switch (l)
                    {
                        case 0:
                            lineName = "10001";
                            //lineName = "98";
                            break;
                        case 1:
                            lineName = "10000";
                            break;
                    }
                    zoneLines[count] = lineName;
                    dgvZones[0, count].Value = zoneNames[count];
                    count++;
                }
            }
            for(int i = 0; i < przejazd06.PT[0].GetLength(0); i++)
            {
                for (int z = 0; z < zoneLines.Length; z++)
                {
                    for (int v = 0; v < przejazd06.PT.Length; v++)
                        if (przejazd06.Info[v][i, 2] != null && przejazd06.PT[v][i, 9] != 0)
                        {
                            if (przejazd06.Info[v][i, 2] == zoneLines[z] && przejazd06.PT[v][i, 9] > 0
                                && float.Parse(przejazd06.Info[v][i, 3]) >= zoneDistances[0][z] && float.Parse(przejazd06.Info[v][i, 3]) < zoneDistances[1][z])
                            {
                                zoneIterNumber[z]++;
                                break;
                            }
                            if (ManualZoneDetection(i, v, zoneNames, z) && przejazd06.PT[v][i, 9] > 0)
                            {
                                zoneIterNumber[z]++;
                                break;
                            }
                        }
                }
            }
            for (int i = 0; i < przejazd06.PT[0].GetLength(0); i++)
            {
                for(int z = 0; z < zoneNames.Length; z++)
                {
                    int vehsOnZone = 0;
                    for(int v = 0; v < przejazd06.PT.Length; v++)
                    {
                        if (przejazd06.Info[v][i, 2] != null && przejazd06.PT[v][i, 10] != 0)
                        {
                            if (przejazd06.PT[v][i, 9] > 0 && przejazd06.Info[v][i, 2] == zoneLines[z]
                                && float.Parse(przejazd06.Info[v][i, 3]) >= zoneDistances[0][z] && float.Parse(przejazd06.Info[v][i, 3]) < zoneDistances[1][z])
                            {
                                vehsOnZone++;
                                totalIter[z]++;
                            }
                            if (ManualZoneDetection(i, v, zoneNames, z) && przejazd06.PT[v][i, 9] > 0)
                            {
                                vehsOnZone++;
                                totalIter[z]++;
                            }
                        }
                    }
                    for (int v = 0; v < przejazd06.PT.Length; v++)
                    {
                        if (przejazd06.Info[v][i, 2] != null && przejazd06.PT[v][i, 10] != 0)
                        {
                            if (przejazd06.PT[v][i, 9] > 0 && przejazd06.Info[v][i, 2] == zoneLines[z]
                                && float.Parse(przejazd06.Info[v][i, 3]) >= zoneDistances[0][z] && float.Parse(przejazd06.Info[v][i, 3]) < zoneDistances[1][z])
                            {
                                zoneMeanPower[z] += (przejazd06.PT[v][i, 10] * przejazd06.PT[v][i, 9])/ vehsOnZone / zoneIterNumber[z];
                                zoneMeanCurrent[z] += przejazd06.PT[v][i, 9] / vehsOnZone / zoneIterNumber[z];
                                if (przejazd06.PT[v][i, 10] < 3000) below3000[z]++;
                                if (przejazd06.PT[v][i, 10] < 2700) below2700[z]++;
                            }
                            if (ManualZoneDetection(i, v, zoneNames, z) && przejazd06.PT[v][i, 9] > 0)
                            {
                                zoneMeanPower[z] += (przejazd06.PT[v][i, 10] * przejazd06.PT[v][i, 9]) / vehsOnZone / zoneIterNumber[z];
                                zoneMeanCurrent[z] += przejazd06.PT[v][i, 9] / vehsOnZone / zoneIterNumber[z];
                                if (przejazd06.PT[v][i, 10] < 3000) below3000[z]++;
                                if (przejazd06.PT[v][i, 10] < 2700) below2700[z]++;
                            }
                        }
                    }
                }
            }
            for(int z = 0; z < zoneMeanVoltage.Length; z++)
            {
                zoneMeanVoltage[z] = zoneMeanPower[z] / zoneMeanCurrent[z];
            }
            for (int z = 0; z < zoneMeanVoltage.Length; z++)
            {
                dgvZones[1, z].Value = zoneMeanVoltage[z].ToString();
                dgvZones[2, z].Value = (float)below3000[z] / totalIter[z] * 100f;
                dgvZones[3, z].Value = (float)below2700[z] / totalIter[z] * 100f;
            }
        }
        private bool ManualZoneDetection(int i, int v, string[] zoneNames, int z)
        {
            if ((przejazd06.Info[v][i, 2] == "10000" && zoneNames[z] == "przyklad"))
                return true;
            else return false;
        }

        private void bExportLines_Click(object sender, EventArgs e)
        {
            int height = chartLines.Height;
            int width = chartLines.Width;
            chartLines.Width = 1920;
            chartLines.Height = 1080;
            string directoryPath = "C:\\Users\\Paweł\\";
            for(int l = 0; l < cbLineToPresent.Items.Count; l++)
            {
                try
                {
                    cbLineToPresent.SelectedIndex = l;
                    string selectedLine = cbLineToPresent.Text.ToString();
                    string lName1 = selectedLine;
                    DrawLines(line.Line[l], 1, chartLines, lName1, clbVehsToShow);
                    //this.chartLines.ChartAreas[0].RecalculateAxesScale();
                    this.chartLines.Legends.Clear();
                    this.chartLines.SaveImage(directoryPath + "\\" + lName1 + ".tiff", ChartImageFormat.Tiff);
                }
                catch { }
            }
            chartLines.Height = height;
            chartLines.Width = width;
        }
        private void PrepareForExtraction()
        {
            PrepatationForVamos(true);
            Przejazd06 temp = przejazd06;
            przejazd06 = new Przejazd06(projectData.DeltaT, projectData.SimTime.OnlySeconds, projectData.InitialTime, calculationObjects, projectData.ConfigurationData, rbElectrical.Checked, true);
            przejazd06.PT = temp.PT;
            przejazd06.R = temp.R;
            przejazd06.Info = temp.Info;
        }
    }
}
