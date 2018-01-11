using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;


using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Analyst3D;

namespace RDB
{
    public sealed partial class MainForm : Form
    {
        #region class private members
        private IMapControl3 m_mapControl = null;
        private string m_mapDocumentName = string.Empty;
        #endregion

        #region class constructor
        public MainForm()
        {
            InitializeComponent();
            //ɫ����ʼ��
            m_FromColor = Color.Red;  //��ʼ��ɫ����ɫ����
            m_ToColor = Color.Blue; //��ʼ��ɫ����ɫ���ң�

            RefreshColors(m_FromColor, m_ToColor);
            //��ӵ����λҶ���ǿ����
            cmb_StretchMethod.Items.Add("Ĭ������");
            cmb_StretchMethod.Items.Add("��׼������");
            cmb_StretchMethod.Items.Add("�����Сֵ����");
            cmb_StretchMethod.Items.Add("�ٷֱ������С����");
            cmb_StretchMethod.Items.Add("ֱ��ͼ����");
            cmb_StretchMethod.Items.Add("ֱ��ͼƥ��");
            cmb_StretchMethod.SelectedIndex = 0;
            //���ͼ��仯����
            cmb_TransformMethod.Items.Add("��ת");
            cmb_TransformMethod.Items.Add("����");
            cmb_TransformMethod.Items.Add("����");
            cmb_TransformMethod.Items.Add("��ת");
            cmb_TransformMethod.SelectedIndex = 0;
            //����˲�����
            cmb_FliterMethod.Items.Add("LineDetectionHorizontal");
            cmb_FliterMethod.Items.Add("LineDetectionVertical");
            cmb_FliterMethod.Items.Add("LineDetectionLeftDiagonal");
            cmb_FliterMethod.Items.Add("LineDetectionRightDiagonal");
            cmb_FliterMethod.Items.Add("GradientNorth");
            cmb_FliterMethod.Items.Add("GradientWest");
            cmb_FliterMethod.Items.Add("GradientEast");
            cmb_FliterMethod.Items.Add("GradientSouth");
            cmb_FliterMethod.Items.Add("GradientNorthEast");
            cmb_FliterMethod.Items.Add("GradientNorthWest");
            cmb_FliterMethod.Items.Add("SmoothArithmeticMean");
            cmb_FliterMethod.Items.Add("Smoothing3x3");
            cmb_FliterMethod.Items.Add("Smoothing5x5");
            cmb_FliterMethod.Items.Add("Sharpening3x3");
            cmb_FliterMethod.Items.Add("Sharpening5x5");
            cmb_FliterMethod.Items.Add("Laplacian3x3");
            cmb_FliterMethod.Items.Add("Laplacian5x5");
            cmb_FliterMethod.Items.Add("SobelHorizontal");
            cmb_FliterMethod.Items.Add("SobelVertical");
            cmb_FliterMethod.Items.Add("Sharpen");
            cmb_FliterMethod.Items.Add("Sharpen2");
            cmb_FliterMethod.Items.Add("PointSpread");
            cmb_FliterMethod.SelectedIndex = 0;
            //��������������
            cmb_NeighborhoodMethod.Items.Add("3x3LowPass");
            cmb_NeighborhoodMethod.Items.Add("3x3HighPass");
            cmb_NeighborhoodMethod.Items.Add("Majority");
            cmb_NeighborhoodMethod.Items.Add("Maximum");
            cmb_NeighborhoodMethod.Items.Add("Mean");
            cmb_NeighborhoodMethod.Items.Add("Median");
            cmb_NeighborhoodMethod.Items.Add("Minimum");
            cmb_NeighborhoodMethod.Items.Add("Minority");
            cmb_NeighborhoodMethod.Items.Add("Range");
            cmb_NeighborhoodMethod.Items.Add("Std");
            cmb_NeighborhoodMethod.Items.Add("Sum");
            cmb_NeighborhoodMethod.Items.Add("Variety");
            cmb_NeighborhoodMethod.Items.Add("Length");
            cmb_NeighborhoodMethod.SelectedIndex = 0;
        }
        #endregion

  
        private IWorkspace workspace = null;//�����ռ䣬��GeoDataBase
        private ILayer TOCRightLayer;//���ڴ洢TOC�Ҽ�ѡ��ͼ��
        private Color m_FromColor = Color.Red;  //��ʼ��ɫ����ɫ����
        private Color m_ToColor = Color.Blue; //��ʼ��ɫ����ɫ���ң�
        private ColorDialog cd_FromColor=new ColorDialog();
        private ColorDialog cd_ToColor=new ColorDialog();
        bool fClip = false;//���μ���
        bool fExtraction = false;//����μ���
        bool fLineOfSight = false;//ͨ�ӷ���
        bool fVisibility = false;//�������
        bool fTIN = false;//�ֻ湹��TIN
        ITinEdit TinEdit=new TinClass();//TIN
        private bool bCanDrag;              //ӥ�۵�ͼ�ϵľ��ο���ƶ��ı�־  
        private IPoint pMoveRectPoint;      //��¼���ƶ�ӥ�۵�ͼ�ϵľ��ο�ʱ����λ��  
        private IEnvelope pEnv;             //��¼������ͼ��Extent 

        private void MainForm_Load(object sender, EventArgs e)
        {
            //get the MapControl
            m_mapControl = (IMapControl3)axMapControl1.Object;

            //disable the Save menu (since there is no document yet)
            menuSaveDoc.Enabled = false;
            this.axTOCControl1.SetBuddyControl(this.axMapControl1);
        }

        #region Main Menu event handlers
        private void menuNewDoc_Click(object sender, EventArgs e)
        {
            //execute New Document command
            ICommand command = new CreateNewDocument();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuOpenDoc_Click(object sender, EventArgs e)
        {
            //execute Open Document command
            ICommand command = new ControlsOpenDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuSaveDoc_Click(object sender, EventArgs e)
        {
            //execute Save Document command
            if (m_mapControl.CheckMxFile(m_mapDocumentName))
            {
                //create a new instance of a MapDocument
                IMapDocument mapDoc = new MapDocumentClass();
                mapDoc.Open(m_mapDocumentName, string.Empty);

                //Make sure that the MapDocument is not readonly
                if (mapDoc.get_IsReadOnly(m_mapDocumentName))
                {
                    MessageBox.Show("Map document is read only!");
                    mapDoc.Close();
                    return;
                }

                //Replace its contents with the current map
                mapDoc.ReplaceContents((IMxdContents)m_mapControl.Map);

                //save the MapDocument in order to persist it
                mapDoc.Save(mapDoc.UsesRelativePaths, false);

                //close the MapDocument
                mapDoc.Close();
            }
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            //execute SaveAs Document command
            ICommand command = new ControlsSaveAsDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuExitApp_Click(object sender, EventArgs e)
        {
            //exit the application
            Application.Exit();
        }
        #endregion

        //listen to MapReplaced evant in order to update the statusbar and the Save menu
       

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            statusBarXY.Text = string.Format("{0}, {1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
        }

        //����SDE���ݿ�ͳ�ʼ����������������
        private void Btn_ConnectDB_Click(object sender, EventArgs e)
        {
            try
            {
                //SDE�������ݿ��������
                IPropertySet propertySet = new PropertySet();
                propertySet.SetProperty("SERVER", "localhost");
                propertySet.SetProperty("INSTANCE", "sde:oracle11g:localhost/orcl");
                propertySet.SetProperty("DATABASE", "sde1363");
                propertySet.SetProperty("USER", "sde");
                propertySet.SetProperty("PASSWORD", "sde");
                propertySet.SetProperty("VSESION", "sde.DEFAULT");
                propertySet.SetProperty("AUTHENTICATION_MODE", "DBMS");

                //ָ��SDE�����ռ�factory
                Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                workspace = workspaceFactory.Open(propertySet, 0);

                //if (workspace != null) MessageBox.Show(workspace.Type.ToString());

                //���դ��Ŀ¼�����������ѡ��
                cmb_LoadRstCatalog.Items.Clear();
                cmb_LoadRstCatalog.Items.Add("��դ��Ŀ¼�������ռ䣩");
                cmb_ImportRstCatalog.Items.Clear();
                cmb_ImportRstCatalog.Items.Add("��դ��Ŀ¼�������ռ䣩");
                //��ȡ���ݿ��е�դ��Ŀ¼����ȥSDEǰ׺
                IEnumDatasetName enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTRasterCatalog);
                IDatasetName datasetName = enumDatasetName.Next();
                while (datasetName != null)
                {
                    cmb_LoadRstCatalog.Items.Add(datasetName.Name.Substring(datasetName.Name.LastIndexOf('.') + 1));
                    cmb_ImportRstCatalog.Items.Add(datasetName.Name.Substring(datasetName.Name.LastIndexOf('.') + 1));
                    datasetName = enumDatasetName.Next();
                }
                //����������Ĭ��ѡ��Ϊ��դ��Ŀ¼�������ռ䣩
                if (cmb_LoadRstCatalog.Items.Count > 0)
                {
                    cmb_LoadRstCatalog.SelectedIndex = 0;
                }
                if (cmb_ImportRstCatalog.Items.Count > 0)
                {
                    cmb_ImportRstCatalog.SelectedIndex = 0;
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private IFields CreateFields(string rasterFldName, string shapeFldName, ISpatialReference rasterSF,ISpatialReference shapeSF)
        {
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = fields as IFieldsEdit;

            IField field;
            IFieldEdit fieldEdit;

            //���OID�ֶΣ�ע���ֶ�type
            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = "ObjectID";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(field);

            //���name�ֶΣ�ע���ֶ�type
            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = "NAME";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldsEdit.AddField(field);

            //���raster�ֶΣ�ע���ֶ�type
            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = rasterFldName;
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeRaster;

            //IRasterDef�ӿڶ���դ���ֶ�
            IRasterDef rasterDef =new RasterDefClass();
            rasterDef.SpatialReference=rasterSF;
            ((IFieldEdit2)fieldEdit).RasterDef = rasterDef;
            fieldsEdit.AddField(field);

            //���shape�ֶΣ�ע���ֶ�type
            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = shapeFldName;
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;

            //IGeometryDef��IGeometryDefEdit����ż����ͱ༭�����ֶ�
            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = geometryDef as IGeometryDefEdit;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            geometryDefEdit.SpatialReference_2 = shapeSF;
            ((IFieldEdit2)fieldEdit).GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(field);

            //���xml��Ԫ���ݣ��ֶΣ�ע���ֶ�Type
            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = "METADATA";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeBlob;
            fieldsEdit.AddField(field);

            return fields;
            
        }

        //����դ��Ŀ¼
        private void btn_NewRstCatalog_Click(object sender, EventArgs e)
        {
            if (txb_NewRstCatalog.Text.Trim() == "")
            {
                MessageBox.Show("������դ��Ŀ¼���ƣ�");
            }
            else
            {
                string rasCatalogName = txb_NewRstCatalog.Text.Trim();
                IRasterWorkspaceEx rasterWorkspaceEx = workspace as IRasterWorkspaceEx;
                //����ռ�ο�������WGS84ͶӰ
                ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                ISpatialReference spatialReference = spatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                spatialReference.SetDomain(-180, 180, -90, 90);
                //�ж�դ��Ŀ¼�Ƿ����
                IEnumDatasetName enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTRasterCatalog);
                IDatasetName datasetName = enumDatasetName.Next();
                bool isExit = false;
                //ѭ�������ж��Ƿ��Ѵ��ڸ�դ��Ŀ¼
                while (datasetName != null)
                {
                    if (datasetName.Name.Substring(datasetName.Name.LastIndexOf('.') + 1) == rasCatalogName)
                    {
                        isExit = true;
                        MessageBox.Show("դ��Ŀ¼�Ѿ����ڣ�");
                        txb_NewRstCatalog.Focus();
                        return;
                    }
                    datasetName = enumDatasetName.Next();
                }
                //�������ڣ������µ�դ��Ŀ¼
                if (isExit == false)
                {
                    //����դ��Ŀ¼�ֶμ�
                    IFields fields = CreateFields("RASTER", "SHAPE", spatialReference, spatialReference);

                    rasterWorkspaceEx.CreateRasterCatalog(rasCatalogName, fields, "SHAPE", "RASTER", "DEFAULTS");
                    //���½���դ��Ŀ¼��ӵ������б��У������õ�ǰդ��Ŀ¼
                    cmb_LoadRstCatalog.Items.Add(rasCatalogName);
                    cmb_LoadRstCatalog.SelectedIndex = cmb_LoadRstCatalog.Items.Count - 1;
                    cmb_ImportRstCatalog.Items.Add(rasCatalogName);
                    cmb_ImportRstCatalog.SelectedIndex = cmb_ImportRstCatalog.Items.Count - 1;
                    cmb_LoadRstDataset.Items.Clear();
                    cmb_LoadRstDataset.Text = "";
                }
                MessageBox.Show("դ��Ŀ¼�����ɹ���");
            }
        }

        //��ѡ���դ��Ŀ¼�����仯������Ӧ��դ��ͼ���б�Ҳ�����仯
        private void cmb_LoadRstCatalog_SelectedIndexChanged(object sender, EventArgs e)
        {
            string rstCatalogName = cmb_LoadRstCatalog.SelectedItem.ToString();
            IEnumDatasetName enumDatasetName;
            IDatasetName datasetName;
            if (cmb_LoadRstCatalog.SelectedIndex == 0)
            {
                //���դ��ͼ���������е�ѡ��
                cmb_LoadRstDataset.Items.Clear();
                //��ȡ��դ��Ŀ¼�������ռ䣩�е�դ��ͼ��
                enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTRasterDataset);
                datasetName = enumDatasetName.Next();
                while (datasetName != null)
                {
                    cmb_LoadRstDataset.Items.Add(datasetName.Name.Substring(datasetName.Name.LastIndexOf('.') + 1));
                    datasetName = enumDatasetName.Next();
                }
                //����������Ĭ��ѡ��Ϊ��դ��Ŀ¼�������ռ䣩
                if (cmb_LoadRstDataset.Items.Count > 0)
                {
                    cmb_LoadRstDataset.SelectedIndex = 0;
                }
            }
            else
            {
                //�ӿ�ת��IRasterWorkspaceEx
                IRasterWorkspaceEx workspaceEx = (IRasterWorkspaceEx)workspace;
                //��ȡդ��Ŀ¼
                IRasterCatalog rasterCatalog = workspaceEx.OpenRasterCatalog(rstCatalogName);
                //�ӿ�ת��IFeatureClass
                IFeatureClass featureClass = (IFeatureClass)rasterCatalog;
                //�ӿ�ת��ITable
                ITable pTable = featureClass as ITable;
                //ִ�в�ѯ��ȡָ��
                ICursor cursor = pTable.Search(null, true) as ICursor;
                IRow pRow = null;
                //����������ѡ��
                cmb_LoadRstDataset.Items.Clear();
                cmb_LoadRstDataset.Text = "";
                while ((pRow = cursor.NextRow()) != null)
                {
                    int idxName = pRow.Fields.FindField("NAME");
                    cmb_LoadRstDataset.Items.Add(pRow.get_Value(idxName).ToString());
                }
                //����Ĭ��ѡ��
                if (cmb_LoadRstDataset.Items.Count > 0)
                {
                    cmb_LoadRstDataset.SelectedIndex = 0;
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);

            }
        }

        //����ѡ����դ��Ŀ¼��դ��ͼ����������Ӧ��ͼ��
        private void btn_LoadRstDataset_Click(object sender, EventArgs e)
        {
           
            if (cmb_LoadRstCatalog.SelectedIndex == 0)
            {
                string rstDatasetName = cmb_LoadRstDataset.SelectedItem.ToString();
                //�ӿ�ת��IRasterWorkspaceEx
                IRasterWorkspaceEx workspaceEx = (IRasterWorkspaceEx)workspace;
                //��ȡդ�����ݼ�                
                IRasterDataset rasterDataset = workspaceEx.OpenRasterDataset(rstDatasetName);
                //����դ��Ŀ¼���դ��ͼ��
                IRasterLayer rasterLayer = new RasterLayerClass();
                rasterLayer.CreateFromDataset(rasterDataset);
                rasterLayer.Name = rstDatasetName;
                ILayer layer = rasterLayer as ILayer;
                //��ͼ����ص�MapControl�У������ŵ���ǰͼ��
                axMapControl1.AddLayer(layer);
                axMapControl1.ActiveView.Extent = layer.AreaOfInterest;
                axMapControl1.ActiveView.Refresh();
                axTOCControl1.Update();
                iniCmbItems();
            }
            else
            {
                string rstCatalogName = cmb_LoadRstCatalog.SelectedItem.ToString();
                string rstDatasetName = cmb_LoadRstDataset.SelectedItem.ToString();
                //�ӿ�ת��IRasterWorkspaceEx
                IRasterWorkspaceEx workspaceEx = (IRasterWorkspaceEx)workspace;
                //��ȡդ��Ŀ¼
                IRasterCatalog rasterCatalog = workspaceEx.OpenRasterCatalog(rstCatalogName);
                //�ӿ�ת��IFeatureClass
                IFeatureClass featureClass = (IFeatureClass)rasterCatalog;
                //�ӿ�ת��ITable
                ITable pTable = featureClass as ITable;
                //��ѯ����������QueryFilterClass
                IQueryFilter qf = new QueryFilterClass();
                qf.SubFields = "OBJECTID";
                qf.WhereClause = "NAME='" + rstDatasetName + "'";
                //ִ�в�ѯ��ȡָ��
                ICursor cursor = pTable.Search(qf, true) as ICursor;
                IRow pRow = null;
                int rstOID = 0;
                //�ж϶�ȡ��һ����¼
                if ((pRow = cursor.NextRow()) != null)
                {
                    int idxfld = pRow.Fields.FindField("OBJECTID");
                    rstOID = int.Parse(pRow.get_Value(idxfld).ToString());
                    //��ȡ��������դ��Ŀ¼��
                    IRasterCatalogItem rasterCatalogItem = (IRasterCatalogItem)featureClass.GetFeature(rstOID);
                    //����դ��Ŀ¼���դ��ͼ��
                    IRasterLayer rasterLayer = new RasterLayerClass();
                    rasterLayer.CreateFromDataset(rasterCatalogItem.RasterDataset);
                    rasterLayer.Name = rstDatasetName;
                    ILayer layer = rasterLayer as ILayer;
                    //��ͼ����ص�MapControl�У������ŵ���ǰͼ��
                    axMapControl1.AddLayer(layer);
                    axMapControl1.ActiveView.Extent = layer.AreaOfInterest;
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                }
                iniCmbItems();
                //�ͷ��ڴ�ռ�
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
            }
         
        }

        private void txb_ImportRstDataset_MouseDown(object sender, MouseEventArgs e)
        {
            //���ļ�ѡ��Ի������öԻ�������
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imag file (*.img)|*img|Tiff file(*.tif)|*.tif|Openflight file (*.flt)|*.flt";
            openFileDialog.Title = "��Ӱ������";
            openFileDialog.Multiselect = false;
            string filename = "";
            //����Ի����ѳɹ�ѡ���ļ������ļ�·����Ϣ��д���������
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filename = openFileDialog.FileName;
                txb_ImportRstDataset.Text = filename;
            }
        }

        private void btn_ImportRstDataset_Click(object sender, EventArgs e)
        {
            //��ȡդ��ͼ���·�����ļ�����
            string fileName = txb_ImportRstDataset.Text;
            if (fileName == "")
            {
                MessageBox.Show("�ļ�������Ϊ�գ�");
                return;
            }

            FileInfo fileInfo = new FileInfo(fileName);
            string filePath = fileInfo.DirectoryName;
            string file = fileInfo.Name;
            string strOutName = file.Substring(0, file.LastIndexOf("."));

            //����·�����ļ����ֻ�ȡդ�����ݼ�
            if (cmb_ImportRstCatalog.SelectedIndex == 0)
            {
                //�ж��Ƿ�����������
                IWorkspace2 workspace2 = workspace as IWorkspace2;
                //��������Ѵ���
                if (workspace2.get_NameExists(esriDatasetType.esriDTRasterDataset, strOutName))
                {
                    DialogResult result;
                    result = MessageBox.Show(this, "��Ϊ " + strOutName + " ��դ���ļ������ݿ����Ѵ��ڣ�" + "\n�Ƿ񸲸�", "��ͬ�ļ���",MessageBoxButtons.YesNo,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1);
                    //���ѡ��ȷ��ɾ�����򸲸�ԭդ������
                    if (result == DialogResult.Yes)
                    {
                        IRasterWorkspaceEx rstWorkspaceEx = workspace as IRasterWorkspaceEx;
                        IDataset datasetDel = rstWorkspaceEx.OpenRasterDataset(strOutName) as IDataset;
                        //����IDataset�ӿڵ�Delete�ӿ�ʵ���Ѵ���դ�����ݼ���ɾ��
                        datasetDel.Delete();
                        datasetDel = null;
                    }
                    else if(result == DialogResult.No)
                    {
                        MessageBox.Show("�����ռ��Ѵ���ͬ��դ�����ݼ��������ǲ��ܵ��룡");
                        return;                    
                    }   
                }
                //����ѡ���դ��ͼ���·����դ�����ռ�
                IWorkspaceFactory rstWorkspaceFactoryImport = new RasterWorkspaceFactoryClass();
                IRasterWorkspace rstWorkspaceImport = (IRasterWorkspace)rstWorkspaceFactoryImport.OpenFromFile(filePath, 0);
                IRasterDataset rstDatasetImport = null;
                //���ѡ����ļ�·���ǲ�����Ч��դ�����ռ�
                if (!(rstWorkspaceImport is IRasterWorkspace))
                {
                    MessageBox.Show("�ļ�·��������Ч��դ�����ռ䣡");
                    return;
                }
                //����ѡ���դ��ͼ�����ֻ�ȡդ�����ݼ�
                rstDatasetImport = rstWorkspaceImport.OpenRasterDataset(file);
                //��IRasterDataset�ӿڵ�CreateDefaultRaster���������հ׵�դ�����
                IRaster raster = rstDatasetImport.CreateDefaultRaster();
                //IRasterProps�Ǻ�դ�����Զ����йصĽӿ�
                IRasterProps rasterProp = raster as IRasterProps;
                //IRasterStorageDef�ӿں�դ��洢�������
                IRasterStorageDef storageDef = new RasterStorageDefClass();
                //ָ��ѹ������
                storageDef.CompressionType = esriRasterCompressionType.esriRasterCompressionLZ77;
                //����CellSize
                IPnt pnt = new PntClass();
                pnt.SetCoords(rasterProp.MeanCellSize().X, rasterProp.MeanCellSize().Y);
                storageDef.CellSize = pnt;
                //����դ�����ݼ���ԭ�㣬�������Ͻ�һ���λ��
                IPoint origin = new PointClass();
                origin.PutCoords(rasterProp.Extent.XMin,rasterProp.Extent.YMax);
                storageDef.Origin = origin;
                //�ӿ�ת��Ϊ��դ��洢�йص�ISaveAs2
                ISaveAs2 saveAs2 = (ISaveAs2)rstDatasetImport;

                //�ӿ�ת��Ϊ��դ��洢���Զ�����ص�IRasterStorageDef2
                IRasterStorageDef2 rasterStorageDef2 = (IRasterStorageDef2)storageDef;
                //ָ��ѹ����������Ƭ�߶ȺͿ��
                rasterStorageDef2.CompressionQuality = 100;
                rasterStorageDef2.Tiled = true;
                rasterStorageDef2.TileHeight = 128;
                rasterStorageDef2.TileWidth = 128;
                //������ISaveAs2�ӿڵ�SaveAsRasterDataset����ʵ��դ�����ݼ��Ĵ洢
                //ָ���洢���֣������ռ䣬�洢��ʽ����ش洢����
                saveAs2.SaveAsRasterDataset(strOutName, workspace, "GRID", rasterStorageDef2);
                //��ʾ����ɹ�����Ϣ
                MessageBox.Show("����ɹ�");
            }
            else
            {
                string rasterCatalogName = cmb_ImportRstCatalog.Text;
                //��դ�����ռ�
                IWorkspaceFactory pRasterWsFac = new RasterWorkspaceFactoryClass();
                IWorkspace pWs = pRasterWsFac.OpenFromFile(filePath, 0);
                if (!(pWs is IRasterWorkspace))
                {
                    MessageBox.Show("�ļ�·��������Ч��դ�����ռ䣡");
                    return;
                }
                IRasterWorkspace pRasterWs = pWs as IRasterWorkspace;
                //��ȡդ�����ݼ�
                IRasterDataset pRasterDs = pRasterWs.OpenRasterDataset(file);
                //����դ�����
                IRaster raster = pRasterDs.CreateDefaultRaster();
                IRasterProps rasterProp = raster as IRasterProps;
                //����դ��洢����
                IRasterStorageDef storageDef = new RasterStorageDefClass();
                storageDef.CompressionType = esriRasterCompressionType.esriRasterCompressionLZ77;
                //����CellSize
                IPnt pnt = new PntClass();
                pnt.SetCoords(rasterProp.MeanCellSize().X, rasterProp.MeanCellSize().Y);
                storageDef.CellSize = pnt;
                //����դ�����ݼ���ԭ�㣬�������Ͻ�һ��λ�á�
                IPoint origin = new PointClass();
                origin.PutCoords(rasterProp.Extent.XMin, rasterProp.Extent.YMax);
                storageDef.Origin = origin;
               
                //��Raster Catalog�����դ��
                //�򿪶�Ӧ��Raster Catalog
                IRasterCatalog pRasterCatalog = ((IRasterWorkspaceEx)workspace).OpenRasterCatalog(rasterCatalogName);
                //����Ҫ�����Raster Catalogת����ΪFeature Class
                IFeatureClass pFeatureClass = (IFeatureClass)pRasterCatalog;
                //���������е�������
                int nameIndex = pRasterCatalog.NameFieldIndex;
                //դ�����������е�������
                int rasterIndex = pRasterCatalog.RasterFieldIndex;
                IFeatureBuffer pBuffer = null;
                IFeatureCursor pFeatureCursor = pFeatureClass.Insert(false);
                //����IRasterValue�ӿڵĶ��󡪡�RasterBuffer�����rasterIndex��Ҫʹ��
                IRasterValue pRasterValue = new RasterValueClass();
                //����IRasterValue��RasterDataset
                pRasterValue.RasterDataset = (IRasterDataset)pRasterDs;
                //�洢�����趨
                pRasterValue.RasterStorageDef = storageDef;
                pBuffer = pFeatureClass.CreateFeatureBuffer();
                //����RasterBuffer�����rasterIndex��nameIndex
                pBuffer.set_Value(rasterIndex, pRasterValue);
                pBuffer.set_Value(nameIndex, strOutName);
                //ͨ��cursorʵ��feature��Insert����
                pFeatureCursor.InsertFeature(pBuffer);
                pFeatureCursor.Flush();
                //�ͷ��ڴ���Դ
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pBuffer);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pRasterValue);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
                //��ʾ�ɹ���Ϣ
                MessageBox.Show("����ɹ���");
            }
        }

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            try
            {
                //��ȡ��ǰ�����λ�õ������Ϣ
                esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap basicMap = null;
                ILayer layer = null;
                object unk = null;
                object data = null;
                //�����϶���Ľӿڶ�����Ϊ���ô��뺯���У���ȡ�������ֵ
                this.axTOCControl1.HitTest(e.x, e.y, ref itemType, ref basicMap, ref layer, ref unk, ref data);
                //����������һ��ҵ��λ��Ϊͼ�㣬�򵯳��һ����ܿ�
                if (e.button == 2)
                {
                    if (itemType == esriTOCControlItem.esriTOCControlItemLayer)
                    {
                        //����TOCѡ��ͼ��
                        this.TOCRightLayer = layer;
                        this.contextMenuStrip1.Show(axTOCControl1, e.x, e.y);
                    }
                }
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message,"����",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        //�Ҽ�ѡ�ɾ��ͼ��
        private void TSMI_DeleteLayer_Click(object sender, EventArgs e)
        {
            try
            {
                //ɾ����ǰͼ��
                axMapControl1.Map.DeleteLayer(TOCRightLayer);
                //ˢ�µ�ǰҳ��
                axMapControl1.ActiveView.Refresh();
                //���²�����Ϣͳ�Ƶ�ͼ��Ͳ���������ѡ������
                iniCmbItems();
            }
            catch(System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //�Ҽ�ѡ�PAN
        private void TSMI_ZoomToLayer_Click(object sender, EventArgs e)
        {
            try
            {
                //���ŵ���ǰͼ��
                axMapControl1.ActiveView.Extent = TOCRightLayer.AreaOfInterest;
                //ˢ��ҳ����ʾ
                axMapControl1.ActiveView.Refresh();
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //������ͼ��ʱ�򣬳�ʼ��tabҳ�����ͼ��Ͳ����������ѡ������
        private void iniCmbItems()
        {
            try
            {
                //���������Ϣͳ��ͼ���������ѡ������
                cmb_StatistiicsLayer.Items.Clear();
                //���NDVIָ������ͼ���������ѡ������
                cmb_NDVILayer.Items.Clear();
                //���ֱ��ͼ����ͼ���������ѡ������
                cmb_DrawHisLayer.Items.Clear();
                //��������λҶ���ǿ��ͼ���������ѡ������
                cmb_StretchLayer.Items.Clear();
                //���������α��ɫ��Ⱦ��ͼ���������ѡ������
                cmb_RenderLayer.Items.Clear();
                //����ನ�μٲ�ɫ�ϳɵ�ͼ���������ѡ������
                cmb_RGBLayer.Items.Clear();
                //���ͼ������������ѡ������
                cmb_ClassifyLayer.Items.Clear();
                //���ͼ���ںϵ�������ѡ������
                cmb_PanSharpenSigleLayer.Items.Clear();
                cmb_PanSharpenMultiLayer.Items.Clear();
                //�����������������ѡ������
                cmb_FliterLayer.Items.Clear();
                //���ͼ��任��������ѡ������
                cmb_TransformLayer.Items.Clear();
                //���ɽ����Ӱ��������ѡ������
                cmb_HillShade.Items.Clear();
                //����¶Ⱥ�����������ѡ������
                cmb_Slope.Items.Clear();
                //�����������������ѡ������
                cmb_Aspect.Items.Clear();
                //������������������ѡ������
                cmb_NeighborhoodLayer.Items.Clear();
                //����ü�������������ѡ������
                cmb_NeighborhoodLayer.Items.Clear();
                //���ͨ�ӷ�����������ѡ������
                cmb_LineOfSightLayer.Items.Clear();
                //������������������ѡ������
                cmb_VisibilityLayer.Items.Clear();
                //�������TIN��������ѡ������
                cmb_CreateTINLayer.Items.Clear();
                //���ͨ��DEM�����ȸ��ߵ�������ѡ������
                cmb_DEMContour.Items.Clear();
                //���ͨ��TIN�����ȸ���������ѡ������
                cmb_tinContour.Items.Clear();
                //���ͨ��TIN����̩ɭ�����������ѡ������
                cmb_TinVoronoi.Items.Clear();

                ILayer layer = null;
                IMap map = axMapControl1.Map;
                int count = map.LayerCount;
                if (count > 0)
                {
                    //������ͼ������ͼ�㣬��ȡͼ������ּ���������
                    for (int i = 0; i < count; i++)
                    {
                        layer = map.get_Layer(i);
                        //������Ϣͳ�Ƶ�ͼ��������
                        cmb_StatistiicsLayer.Items.Add(layer.Name);
                        //NDVIָ�������ͼ��������
                        cmb_NDVILayer.Items.Add(layer.Name);
                        //ֱ��ͼ���Ƶ�ͼ��������
                        cmb_DrawHisLayer.Items.Add(layer.Name);
                        //�����λҶ���ǿ��ͼ��������
                        cmb_StretchLayer.Items.Add(layer.Name);
                        //������α��ɫ��Ⱦ��ͼ��������
                        cmb_RenderLayer.Items.Add(layer.Name);
                        //�ನ�μٲ�ɫ�ϳɵ�ͼ��������
                        cmb_RGBLayer.Items.Add(layer.Name);
                        //ͼ������ͼ��������
                        cmb_ClassifyLayer.Items.Add(layer.Name);
                        //ͼ����õ�ͼ��������
                        cmb_ClipFeatureLayer.Items.Add(layer.Name);
                        //ͼ���ںϵ�ͼ��������
                        cmb_PanSharpenMultiLayer.Items.Add(layer.Name);
                        cmb_PanSharpenSigleLayer.Items.Add(layer.Name);
                        //ͼ������ͼ��������
                        cmb_FliterLayer.Items.Add(layer.Name);
                        //ͼ��任��ͼ��������
                        cmb_TransformLayer.Items.Add(layer.Name);
                        //���������ͼ��������
                        cmb_NeighborhoodLayer.Items.Add(layer.Name);
                        //�ü�������ͼ��������
                        cmb_Extraction.Items.Add(layer.Name);

                        ///////////////////////////////////////////
                        ////////////���¶���Ҫ��DEM��ͼ��//////////
                        ///////////////////////////////////////////
                        //ɽ����Ӱ��ͼ��������
                        cmb_HillShade.Items.Add(layer.Name);
                        //�¶Ⱥ�����ͼ��������
                        cmb_Slope.Items.Add(layer.Name);
                        //��������ͼ��������
                        cmb_Aspect.Items.Add(layer.Name);
                        //ͨ�ӷ�����������ѡ������
                        cmb_LineOfSightLayer.Items.Add(layer.Name);
                        //���������������ѡ������
                        cmb_VisibilityLayer.Items.Add(layer.Name);
                        //����TIN��������ѡ������
                        cmb_CreateTINLayer.Items.Add(layer.Name);
                        //ͨ��DEM�����ȸ��ߵ�������ѡ������
                        cmb_DEMContour.Items.Add(layer.Name);

                        ///////////////////////////////////////////
                        ////////////���¶���Ҫ��TIN��ͼ��//////////
                        ///////////////////////////////////////////
                        //ͨ��TIN�����ȸ���������
                        cmb_tinContour.Items.Add(layer.Name);
                        //ͨ��TIN����̩ɭ�����������
                        cmb_TinVoronoi.Items.Add(layer.Name);


                    }
                    //����������Ĭ��ѡ��Ϊ��һ��ͼ��
                    if (cmb_StatistiicsLayer.Items.Count > 0) cmb_StatistiicsLayer.SelectedIndex = 0;
                    if (cmb_NDVILayer.Items.Count > 0) cmb_NDVILayer.SelectedIndex = 0;
                    if (cmb_DrawHisLayer.Items.Count > 0) cmb_DrawHisLayer.SelectedIndex = 0;
                    if (cmb_StretchLayer.Items.Count > 0) cmb_StretchLayer.SelectedIndex = 0;
                    if (cmb_RenderLayer.Items.Count > 0) cmb_RenderLayer.SelectedIndex = 0;
                    if (cmb_RGBLayer.Items.Count > 0) cmb_RGBLayer.SelectedIndex = 0;
                    if (cmb_ClassifyLayer.Items.Count > 0) cmb_ClassifyLayer.SelectedIndex = 0;
                    if (cmb_ClipFeatureLayer.Items.Count > 0) cmb_ClipFeatureLayer.SelectedIndex = 0;
                    if (cmb_FliterLayer.Items.Count > 0) cmb_FliterLayer.SelectedIndex = 0;
                    if (cmb_TransformLayer.Items.Count > 0) cmb_TransformLayer.SelectedIndex = 0;
                    if (cmb_PanSharpenMultiLayer.Items.Count > 0) cmb_PanSharpenMultiLayer.SelectedIndex = 0;
                    if (cmb_PanSharpenSigleLayer.Items.Count > 0) cmb_PanSharpenSigleLayer.SelectedIndex = 0;
                    if (cmb_HillShade.Items.Count > 0) cmb_HillShade.SelectedIndex = 0;
                    if (cmb_Slope.Items.Count > 0) cmb_Slope.SelectedIndex = 0;
                    if (cmb_Aspect.Items.Count > 0) cmb_Aspect.SelectedIndex = 0;
                    if (cmb_NeighborhoodLayer.Items.Count > 0) cmb_NeighborhoodLayer.SelectedIndex = 0;
                    if (cmb_Extraction.Items.Count > 0) cmb_Extraction.SelectedIndex = 0;
                    if (cmb_LineOfSightLayer.Items.Count > 0) cmb_LineOfSightLayer.SelectedIndex = 0;
                    if (cmb_VisibilityLayer.Items.Count > 0) cmb_VisibilityLayer.SelectedIndex = 0;
                    if (cmb_CreateTINLayer.Items.Count > 0) cmb_CreateTINLayer.SelectedIndex = 0;
                    if (cmb_DEMContour.Items.Count > 0) cmb_DEMContour.SelectedIndex = 0;
                    if (cmb_tinContour.Items.Count > 0) cmb_tinContour.SelectedIndex = 0;
                    if (cmb_TinVoronoi.Items.Count > 0) cmb_TinVoronoi.SelectedIndex = 0;
                    //���������Ϣͳ�Ʋ����������ѡ������
                    cmb_StatisticsBand.Items.Clear();
                    //���ֱ��ͼ���ƵĲ����������ѡ������
                    cmb_DrawHisBand.Items.Clear();
                    //��������λҶ���ǿ�Ĳ����������ѡ������
                    cmb_StretchBand.Items.Clear();
                    //���������α��ɫ��Ⱦ�Ĳ����������ѡ������
                    cmb_RenderBand.Items.Clear();
                    //����ನ�μٲ�ɫ�ϳɵĲ���������ѡ�������
                    cmb_RBand.Items.Clear();
                    cmb_GBand.Items.Clear();
                    cmb_BBand.Items.Clear();
                    //��ȡ��һ��ͼ���դ�񲨶�
                    ILayer player = map.get_Layer(0);
                    if (player is IRasterLayer)
                    {
                        IRasterLayer rstLayer = player as IRasterLayer;
                        IRaster2 raster2 = rstLayer.Raster as IRaster2;
                        IRasterDataset rstDataset = raster2.RasterDataset;
                        IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;
                        //��������
                        int bandCount = rstLayer.BandCount;
                        //������в��ε�ѡ��
                        cmb_StatisticsBand.Items.Add("ȫ������");
                        //����ͼ������в��Σ���ȡ�������ּ���������
                        for (int i = 0; i < bandCount; i++)
                        {
                            int bandIdx = i + 1;//���ò������
                            //��Ӳ�����Ϣͳ�ƵĲ����������ѡ������
                            cmb_StatisticsBand.Items.Add("����" + bandIdx);
                            //���ֱ��ͼ���ƵĲ���������ѡ��
                            cmb_DrawHisBand.Items.Add("����" + bandIdx);
                            //��ӵ����λҶ���ǿ�Ĳ����������ѡ������
                            cmb_StretchBand.Items.Add("����" + bandIdx);
                            //��ӵ�����α��ɫ��Ⱦ�Ĳ���������ѡ������
                            cmb_RenderBand.Items.Add("����" + bandIdx);
                            //��Ӷನ�μٲ�ɫ�ϳɵĲ���������ѡ������
                            cmb_RBand.Items.Add("����" + bandIdx);
                            cmb_GBand.Items.Add("����" + bandIdx);
                            cmb_BBand.Items.Add("����" + bandIdx);
                        }
                        //����������Ĭ��ѡ��
                        if (cmb_StatisticsBand.Items.Count > 0) cmb_StatisticsBand.SelectedIndex = 0;
                        if (cmb_DrawHisBand.Items.Count > 0) cmb_DrawHisBand.SelectedIndex = 0;
                        if (cmb_StretchBand.Items.Count > 0) cmb_StretchBand.SelectedIndex = 0;
                        if (cmb_RenderBand.Items.Count > 0) cmb_RenderBand.SelectedIndex = 0;
                        if (cmb_RBand.Items.Count > 0) cmb_RBand.SelectedIndex = 0;
                        if (cmb_BBand.Items.Count > 0) cmb_BBand.SelectedIndex = 0;
                        if (cmb_GBand.Items.Count > 0) cmb_GBand.SelectedIndex = 0;
                    }
                }
            }
            catch(System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public ILayer GetLayerByName(string sLayerName)
        {
            //�ж�ͼ�����ƻ��ߵ�ͼ�����Ƿ�Ϊ�գ���Ϊ�գ��򷵻ؿ�
            if (sLayerName == "" || axMapControl1 == null)
            {
                return null;
            }
            //�Ե�ͼ�����е�����ͼ����б�������ĳһͼ���������ָ��ͼ��������ͬ���򷵻ظ�ͼ��
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                if (axMapControl1.get_Layer(i).Name == sLayerName)
                {
                    return axMapControl1.get_Layer(i);
                }
            }
            //����ͼ������ͼ��������ָ�����ƶ���ƥ�䣬�򷵻ؿ�
            return null;
        }

        //��ң��ͼ���������ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ��Ҳ�ᷢ���仯
        private void selectedIndexChangeFunction(ComboBox cmbLayer, ComboBox cmbBand, string type)
        {
            try
            {
                ILayer layer = GetLayerByName(cmbLayer.SelectedItem.ToString());
                if (layer is IRasterLayer)
                {
                    cmbBand.Items.Clear();
                    //cmbBand.SelectedIndex = 0;
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    IRaster2 raster2 = rstLayer.Raster as IRaster2;
                    IRasterDataset rstDataset = raster2.RasterDataset;
                    IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;
                    //��������
                    int bandCount = rstLayer.BandCount;
                    //������в��ε�ѡ��
                    if (type == "statistics") cmbBand.Items.Add("ȫ������");
                    //����ͼ������в��Σ���ȡ�������ּ���������
                    for (int i = 0; i < bandCount; i++)
                    {
                        int bandIdx = i + 1;//���ò������
                        cmbBand.Items.Add("����" + bandIdx);
                    }
                    cmbBand.SelectedIndex = 0;
                }
            }
            catch(System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //��������Ϣͳ�Ƶ�ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ��Ҳ�ᷢ���仯
        private void cmb_StatistiicsLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                selectedIndexChangeFunction(cmb_StatistiicsLayer, cmb_StatisticsBand, "statistics");
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
        //��ֱ��ͼ���Ƶ�ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ��Ҳ�ᷢ���仯
        private void cmb_DrawHisLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedIndexChangeFunction(cmb_DrawHisLayer, cmb_DrawHisBand, null);
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //�������λҶ���ǿ��ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ��Ҳ�ᷢ���仯
        private void cmb_StretchLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedIndexChangeFunction(cmb_StretchLayer, cmb_StretchBand, null);
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //��������α��ɫ��Ⱦ��ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ��Ҳ�ᷢ���仯
        private void cmb_RenderLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedIndexChangeFunction(cmb_RenderLayer, cmb_RenderBand, null);
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //���ನ�μٲ�ɫ�ϳɵ�ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ��Ҳ�ᷢ���仯
        private void cmb_RGBLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedIndexChangeFunction(cmb_RGBLayer, cmb_RBand, null);
                selectedIndexChangeFunction(cmb_RGBLayer, cmb_GBand, null);
                selectedIndexChangeFunction(cmb_RGBLayer, cmb_BBand, null);
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //���ͳ�ư�ť�󣬽��в�����Ϣͳ��
        private void btn_Statistics_Click(object sender, EventArgs e)
        {
            try
            {
                //��ȡѡ���ͼ��Ͳ��Σ�ת���ӿ�
                ILayer layer = GetLayerByName(cmb_StatistiicsLayer.SelectedItem.ToString());
                IRasterLayer rstLayer = null;
                if (layer is IRasterLayer) rstLayer = layer as IRasterLayer;
                else
                {
                    MessageBox.Show("��ѡ���ͼ�㲢��դ��ͼ�㣬�޷����в���", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //IRasterLayer rstLayer= GetLayerByName(cmb_NDVILayer.SelectedItem.ToString()) as IRasterLayer;
                IRaster2 raster2 = rstLayer.Raster as IRaster2;
                IRasterDataset rstDataset = raster2.RasterDataset;
                IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;

                int index = cmb_StatisticsBand.SelectedIndex;
                if (cmb_StatisticsBand.SelectedItem.ToString()=="ȫ������")
                {
                    string StatRes = "";
                    for (int i = 0; i < rstBandColl.Count; i++)
                    {
                        IRasterBand rstBand = rstBandColl.Item(i);
                        //�жϸò����Ƿ��Ѿ�����ͳ������
                        bool hasStat = false;
                        rstBand.HasStatistics(out hasStat);
                        ////���������ͳ�����ݣ��ͽ��в�����Ϣͳ��
                        if (null == rstBand.Statistics || !hasStat)
                        {
                            IRasterBandEdit rasterBandEdit = rstBand as IRasterBandEdit;
                            rasterBandEdit.ComputeStatsHistogram(0);
                        }
                        //��ȡͳ�ƽ��
                        rstBand.ComputeStatsAndHist();
                        IRasterStatistics rstStat = rstBand.Statistics;
                        StatRes += "��" + (i + 1) + "���Σ�" + "  ƽ��ֵΪ:" + rstStat.Mean + "  ���ֵΪ��" + rstStat.Maximum + "  ��СֵΪ:" + rstStat.Minimum + "  ��׼��Ϊ:" + rstStat.StandardDeviation + "\r\n";                                        
                    }
                    //��ʾ�����ͳ�ƽ��
                    MessageBox.Show(StatRes, "ͳ�ƽ��", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    int bandnum;
                    if (cmb_StatisticsBand.Items.Count > rstBandColl.Count) bandnum = index - 1;
                    else bandnum = index;
                    //��ȡ����
                    IRasterBand rstBand = rstBandColl.Item(bandnum);
                    //�жϸò����Ƿ��Ѿ�����ͳ������
                    bool hasStat = false;
                    rstBand.HasStatistics(out hasStat);
                    ////���������ͳ�����ݣ��ͽ��в�����Ϣͳ��
                    if (null == rstBand.Statistics || !hasStat)
                    {
                        IRasterBandEdit rasterBandEidt = rstBand as IRasterBandEdit;
                        rasterBandEidt.ComputeStatsHistogram(0);
                    }
                    //��ȡͳ�ƽ��
                    rstBand.ComputeStatsAndHist();
                    IRasterStatistics rstStat = rstBand.Statistics;
                    String bandStatRes = null;
                    bandStatRes += "��" + (bandnum + 1) + "���Σ�" + "  ƽ��ֵΪ:" + rstStat.Mean + "  ���ֵΪ��" + rstStat.Maximum + "  ��СֵΪ:" + rstStat.Minimum + "  ��׼��Ϊ:" + rstStat.StandardDeviation + "\r\n";
                    MessageBox.Show(bandStatRes, "ͳ�ƽ��", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                //IRasterBand rstBand = rstBandColl.Item(1);
                //��ȡ��ǰѡ�е�դ��ͼ���դ�񲨶�

                //���ѡ��ȫ�����Σ��������ͼ��ȫ�����Σ���ͳ����Ϣ

            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //����NDVIָ��
        private void btn_CalculateNDVI__Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ���ͼ��Ͳ��Σ�ת���ӿ�
                ILayer layer = GetLayerByName(cmb_NDVILayer.SelectedItem.ToString());
                IRasterLayer rstLayer=null;
                if (layer is IRasterLayer) rstLayer = layer as IRasterLayer;
                else
                {
                    MessageBox.Show("��ѡ���ͼ�㲢��դ��ͼ�㣬�޷����в���", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //IRasterLayer rstLayer= GetLayerByName(cmb_NDVILayer.SelectedItem.ToString()) as IRasterLayer;
                IRaster2 raster2 = rstLayer.Raster as IRaster2;
                IRasterDataset rstDataset = raster2.RasterDataset;
                IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;

                //��ȡ�첨�κͽ����Ⲩ�Σ�ת��IGeoDataset�ӿ�
                if (rstBandColl.Count < 4)
                {
                    MessageBox.Show("��ͼ�㲻�ɼ���NDVI","����",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                else
                {
                    //��ȡ���Ĳ���
                    IRasterBand rstBand4 = rstBandColl.Item(3);
                    //��ȡ��������
                    IRasterBand rstBand3 = rstBandColl.Item(2);
                    //�ֱ𽫵��ġ���������ת����IGeoDataset�ӿ�
                    IGeoDataset geoDataset4 = rstBand4 as IGeoDataset;
                    IGeoDataset geoDataset3 = rstBand3 as IGeoDataset;

                    //����һ������դ���������RasterMathOpsClass
                    IMathOp mathOp = new RasterMathOpsClass();
                    //����IGeoDataset��Math����NDVI��ý��IGeoDataset
                    //band4-band3
                    IGeoDataset upDataset = mathOp.Minus(geoDataset4, geoDataset3);
                    //band4+band3
                    IGeoDataset downDataset = mathOp.Plus(geoDataset4, geoDataset3);
                    //���ӷ�ĸתΪfloat��
                    IGeoDataset fltUpDataset = mathOp.Float(upDataset);
                    IGeoDataset fltDownDataset = mathOp.Float(downDataset);
                    //����õ�NDVI
                    IGeoDataset resultDataset = mathOp.Divide(fltUpDataset, fltDownDataset);

                    //��������浽һ��RasterLayer�У�����ΪNDVI
                    IRaster resRaster = resultDataset as IRaster;
                    IRasterLayer resLayer = new RasterLayerClass();
                    resLayer.CreateFromRaster(resRaster);
                    resLayer.SpatialReference = geoDataset4.SpatialReference;
                    resLayer.Name = "NDVI";
                    //���˵�����ͼ���ûҶ���ʾ�������������Сֵ����
                    IRasterStretchColorRampRenderer grayStretch = null;
                    if (resLayer.Renderer is IRasterStretchColorRampRenderer) grayStretch = resLayer.Renderer as IRasterStretchColorRampRenderer;
                    else grayStretch = new RasterStretchColorRampRendererClass();
                    IRasterStretch2 rstStr2 = grayStretch as IRasterStretch2;
                    rstStr2.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_MinimumMaximum;//��������ģʽΪ�����Сֵ����
                    resLayer.Renderer = grayStretch as IRasterRenderer;
                    resLayer.Renderer.Update();

                    //���NDVIͼ����ʾ����ˢ����ͼ
                    axMapControl1.AddLayer(resLayer);
                    axMapControl1.ActiveView.Extent = resLayer.AreaOfInterest;
                    axMapControl1.ActiveView.Refresh();
                    this.axTOCControl1.Update();
                }
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//������޸���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
                
            }
        }

        private void btn_SingleBandHis_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ���ͼ��Ͳ��Σ�ת���ӿ�
                ILayer layer = GetLayerByName(cmb_DrawHisLayer.SelectedItem.ToString());
                IRasterLayer rstLayer = null;
                if (layer is IRasterLayer) rstLayer = layer as IRasterLayer;
                else
                {
                    MessageBox.Show("��ѡ���ͼ�㲢��դ��ͼ�㣬�޷����в���", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //IRasterLayer rstLayer= GetLayerByName(cmb_NDVILayer.SelectedItem.ToString()) as IRasterLayer;
                IRaster2 raster2 = rstLayer.Raster as IRaster2;
                IRasterDataset rstDataset = raster2.RasterDataset;
                IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;
                IRasterBand rasterBand = rstBandColl.Item(cmb_DrawHisBand.SelectedIndex);

                //����ò��ε�Histogram
                bool hasStat = false;
                rasterBand.HasStatistics(out hasStat);
                if (null == rasterBand.Statistics || !hasStat || rasterBand.Histogram == null)
                {
                    //ת��IRasterBandEdit2�ӿڣ�����ComputeStatsHistogram�������в�����Ϣͳ�ƺ�ֱ��ͼ����
                    IRasterBandEdit rasterBandEdit = rasterBand as IRasterBandEdit;
                    rasterBandEdit.ComputeStatsHistogram(0);
                }

                //��ȡÿ����Ԫֵ��ͳ�Ƹ���
                double[] histo = rasterBand.Histogram.Counts as double[];

                //��ȡͳ�ƽ��
                IRasterStatistics rasterStatistics = rasterBand.Statistics;

                //����ֱ��ͼ���壬������Ԫͳ�ơ���Сֵ�����ֵ��Ϊ��������
                HistogramForm histogramForm = new HistogramForm(histo, rasterStatistics.Minimum, rasterStatistics.Maximum);
                histogramForm.ShowDialog();
            }
            catch (Exception ex)//�����쳣������쳣������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//������������ΪĬ����״
            {
                this.Cursor = Cursors.Default;
            }
        }
        //������ƶನ�εĶԱ�ֱ��ͼ���������θ�ѡ����
        private void btn_MultiBandHis_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡ��ǰѡ�е�ͼ���index
                int indexLayer = cmb_DrawHisLayer.SelectedIndex;
                //��ȡMapControl�е�map���ͼ��
                ILayer layer = this.axMapControl1.get_Layer(indexLayer);
                if (layer is IRasterLayer)
                {
                    IRasterLayer rasterLayer = layer as IRasterLayer;
                    SelectBandsForm SelectBands = new SelectBandsForm(rasterLayer);
                    SelectBands.ShowDialog();
                }
            }
            catch(Exception ex)//�쳣��������쳣������Ϣ
            {
                MessageBox.Show(ex.Message,"����",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�ΪĬ�ϸ�ʽ
            {
                this.Cursor = Cursors.Default;
            }
        }
        //�����λҶ���ǿ
        private void btn_Stretch_Click(object sender, EventArgs e)
        {
            try
            {
                //��ȡ��ǰѡ���ͼ��Ͳ��ζ���
                if (axMapControl1.LayerCount == 0) {MessageBox.Show("��ǰ�ؼ��в���ͼ�㣬�޷���������", "����", MessageBoxButtons.OK, MessageBoxIcon.Error); return;}
                if (cmb_StretchBand.Items.ToString() == "") { MessageBox.Show("δѡ�񲨶Σ��޷���������", "����", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                if (cmb_StretchMethod.SelectedItem.ToString()  == "") { MessageBox.Show("δָ���������޷���������", "����", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                ILayer layer = GetLayerByName(cmb_StretchLayer.SelectedItem.ToString());
                if (layer is IRasterLayer)
                {
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    IRaster2 raster2 = rstLayer.Raster as IRaster2;
                    IRasterDataset rstDataset = raster2.RasterDataset;
                    IRasterRenderer rstRenderer=rstLayer.Renderer;
                    //IRaster resRaster = rstBand as IRaster;
                    //IRasterLayer resLayer = new RasterLayerClass();
                    //resLayer.CreateFromRaster(resRaster);
                    //resLayer.SpatialReference = ((IGeoDataset)rstBand).SpatialReference;
                    //resLayer.Name = cmb_StretchLayer.SelectedItem.ToString()+cmb_StretchBand.SelectedItem.ToString()+cmb_StretchMethod.SelectedItem.ToString();

                    ///////////////////////////////////////////
                    //������Ϣ�����ﵽ����ô���֣�����������///
                    ///////////////////////////////////////////
                    
                    IRasterStretchColorRampRenderer grayRenderer = new RasterStretchColorRampRendererClass();
                    grayRenderer.BandIndex = cmb_StretchBand.SelectedIndex;
                    IRasterStretch2 rstStr2 = grayRenderer as IRasterStretch2;
                    rstRenderer.Raster = (IRaster)raster2;
                                  
                    //�ж����췽ʽ
                    switch (cmb_StretchMethod.SelectedIndex)
                    {
                        case 0://Ĭ������
                            rstStr2.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_DefaultFromSource;
                            break;
                        case 1://��׼������
                            rstStr2.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_StandardDeviations;
                            break;
                        case 2://�����Сֵ����
                            rstStr2.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_MinimumMaximum;
                            break;
                        case 3://�ٷֱ������Сֵ����
                            rstStr2.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_PercentMinimumMaximum;
                            break;
                        case 4://ֱ��ͼ����
                            rstStr2.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_HistogramEqualize;
                            break;
                        case 5://ֱ��ͼƥ��
                            rstStr2.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_HistogramSpecification;
                            break;
                        default:
                            break;
                    }
                    //���ò�Ӧ�÷�ɫ
                    rstStr2.Invert = false;
                    //Ӧ��������Ⱦ
                    rstRenderer = grayRenderer as IRasterRenderer;
                    rstRenderer.Update();
                    rstLayer.Renderer = rstRenderer;
                }
                //ˢ�¿ؼ�
                this.axMapControl1.ActiveView.Refresh();
                this.axTOCControl1.Update();

            }
            catch (Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //����������ɫ�ʹ�С����AlgorithmicColorRamp
        private IAlgorithmicColorRamp GetAlgorithmicColorRamp(Color FromColor, Color ToColor, int size)
        {
            try
            {
                //ʵ�����ӿ�
                IAlgorithmicColorRamp algorithmicColorRamp = new AlgorithmicColorRampClass();
                IColor toColor=new RgbColorClass();
                IColor fromColor = new RgbColorClass();
                toColor.RGB = ToColor.B * 65536 + ToColor.G * 256 + ToColor.R;
                fromColor.RGB = FromColor.B * 65536 + FromColor.G * 256 + FromColor.R;
                //toColor.RGB = ToColor;
                //������ʼ��ɫ����ֹ��ɫ���㷨���ͣ��ߴ��С
                algorithmicColorRamp.ToColor = toColor ;
                algorithmicColorRamp.FromColor = fromColor;
                algorithmicColorRamp.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;
                algorithmicColorRamp.Size = size;

                //����IAlgorithmicColorRamp�ӿڵ�CreateRamp��������ɫ��
                bool bResult;
                algorithmicColorRamp.CreateRamp(out bResult);
                if (bResult)
                {
                    return algorithmicColorRamp;
                }
                return null;
            }
            catch (System.Exception e)//��׽�쳣
            {
                MessageBox.Show(e.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        //ͨ���������ʼ����ֹ��ɫ��������ɫ����Bitmapͼ�񲢷���
        private Bitmap CreateColorRamp(Color FromColor, Color ToColor)
        {
            try
            {
                //��ȡɫ��
                IAlgorithmicColorRamp algorithmicColorRamp = GetAlgorithmicColorRamp(FromColor, ToColor, pb_ColorBar.Size.Width);
                //�����µ�bitmap
                Bitmap bmpColorRamp = new Bitmap(pb_ColorBar.Size.Width, pb_ColorBar.Size.Height);
                //��ȡgraphics����
                Graphics graphic = Graphics.FromImage(bmpColorRamp);
                //��GDI+�ķ�����һ�����ɫ����ʾɫ��
                IColor color = null;
                for (int i = 0; i < pb_ColorBar.Size.Width; i++)
                {
                    //��ȡ��ǰ��ɫ
                    color = algorithmicColorRamp.get_Color(i);
                    if (color == null) continue;
                    IRgbColor rgbColor = new RgbColorClass();
                    rgbColor.RGB = color.RGB;
                    Color customColor = Color.FromArgb(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
                    SolidBrush solidBrush = new SolidBrush(customColor);
                    //����
                    graphic.FillRectangle(solidBrush, i, 0, 1, pb_ColorBar.Size.Height);

                }
                //ɾ��graphics����
                graphic.Dispose();
                return bmpColorRamp;
            }
            catch (System.Exception ex)//�����쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
     
        //ˢ����ɫ�ؼ�
        private void RefreshColors(Color FromColor, Color ToColor)//���»�����ʼ��ɫ����ֹ��ɫ����������������ɫ����ɫ��
        {
            try
            {
                //��ʼ��FromColor
                //����Bitmap
                Bitmap bmpFromColor = new Bitmap(pb_FromColor.Size.Width, pb_FromColor.Size.Height);
                //����graphics����
                Graphics graphicFC = Graphics.FromImage(bmpFromColor);
                SolidBrush solidBrushFC = new SolidBrush(FromColor);
                //������ʼ��ɫ�����µ�����
                graphicFC.FillRectangle(solidBrushFC, 0, 0, pb_FromColor.Size.Width, pb_FromColor.Size.Height);
                //����ͼ��
                this.pb_FromColor.Image = bmpFromColor;
                //ɾ��graphics����
                graphicFC.Dispose();

                //��ʼ��ToColor
                //����bitmap
                Bitmap bmpToColor = new Bitmap(pb_ToColor.Size.Width, pb_ToColor.Size.Height);
                //����graphics����
                Graphics graphicTC = Graphics.FromImage(bmpToColor);
                SolidBrush solidBrushTC = new SolidBrush(ToColor);
                //������ֹ��ɫ�����µ�����
                graphicTC.FillRectangle(solidBrushTC, 0, 0, pb_ToColor.Size.Width, pb_ToColor.Size.Height);
                //����ͼ��
                this.pb_ToColor.Image = bmpToColor;
                //ɾ��graphics����
                graphicTC.Dispose();

                //��ʼ��ɫ��
                Bitmap stretchRamp = CreateColorRamp(FromColor, ToColor);
                //����ͼ��
                this.pb_ColorBar.Image = stretchRamp;
            }
            catch (System.Exception ex)//�����쳣�����������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //�����ʼ��ɫ��������¼�
        private void pb_FromColor_Click(object sender, EventArgs e)//��ʾColorDialogѡ���ѡ�񲢻�ȡɫ����ʼ��ɫ
        {
            try
            {
                if (this.cd_FromColor.ShowDialog() == DialogResult.OK)
                {
                    m_FromColor = this.cd_FromColor.Color;//������ʼ��ɫ
                    RefreshColors(m_FromColor, m_ToColor);//ˢ����ɫ�ؼ�
                }
            }
            catch (System.Exception ex)//�����쳣�����������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //�����ֹ��ɫ��������¼�
        private void pb_ToColor_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cd_ToColor.ShowDialog() == DialogResult.OK)
                {
                    m_ToColor = this.cd_ToColor.Color;//������ֹ��ɫ
                    RefreshColors(m_FromColor, m_ToColor);//ˢ����ɫ�ؼ�
                }
            }
            catch (System.Exception ex)//�����쳣�����������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //������α��ɫ��ǿ
        private void btn_Render_Click(object sender, EventArgs e)
        {
            try
            {
                //��ȡ��ǰѡ���ͼ��Ͳ���
                ILayer layer = GetLayerByName(cmb_RenderLayer.SelectedItem.ToString());
                IRasterLayer rstLayer = layer as IRasterLayer;
                IRaster raster = rstLayer.Raster;

                //����IRasterRenderer
                IRasterStretchColorRampRenderer stretchRenderer = new RasterStretchColorRampRendererClass();
                IRasterRenderer rasterRenderer = (IRasterRenderer)stretchRenderer;
                rasterRenderer.Raster = raster;
                //��ȡ������������ȾӦ�ò���
                stretchRenderer.BandIndex = cmb_RenderBand.SelectedIndex;

                //������������
                IRasterStretch2 rstStretch2 = rasterRenderer as IRasterStretch2;
                rstStretch2.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_HistogramEqualize;//�������췽ʽΪֱ��ͼ���⻯
                //��ȡɫ����256��
                IAlgorithmicColorRamp algorithmicColorRamp = GetAlgorithmicColorRamp(m_FromColor, m_ToColor, 256);
                IColorRamp colorRamp = algorithmicColorRamp as IColorRamp;
                //����������Ⱦ��ɫ��
                stretchRenderer.ColorRamp = colorRamp;

                //����TOC�е�ͼ��
                ILegendInfo legendInfo = stretchRenderer as ILegendInfo;
                ILegendGroup legendGroup = legendInfo.get_LegendGroup(0);
                for (int i = 0; i < legendGroup.ClassCount; i++)
                {
                    ILegendClass legendClass = legendGroup.get_Class(i);
                    legendClass.Symbol = new ColorRampSymbolClass();
                    IColorRampSymbol colorRampSymbol = legendClass.Symbol as IColorRampSymbol;
                    colorRampSymbol.ColorRamp = colorRamp;
                    colorRampSymbol.ColorRampInLegendGroup = colorRamp;
                    colorRampSymbol.LegendClassIndex = i;
                    legendClass.Symbol = colorRampSymbol as ISymbol;
                }
                //Ӧ����Ⱦ����
                rasterRenderer.Update();
                rstLayer.Renderer = rasterRenderer;
                rstLayer.Renderer.Update();
                //ˢ�¿ؼ�
                this.axMapControl1.ActiveView.Refresh();
                this.axTOCControl1.Update();

            }
            catch (System.Exception ex)//�����쳣�����������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //�ನ�μٲ�ɫ�ϳ�
        private void btn_RGB_Click(object sender, EventArgs e)
        {
            try
            {
                ILayer layer = GetLayerByName(cmb_RGBLayer.SelectedItem.ToString());
                if (layer is IRasterLayer)
                {
                    //ת����IRasterLayer�ӿ�
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    //��ȡ������Ⱦ��Ϣ
                    IRasterRenderer rstRenderer = rstLayer.Renderer;
                    //����RGB�ϳ���Ⱦ��
                    IRasterRGBRenderer rgbRenderer = null;
                    if (rstRenderer is IRasterRGBRenderer)
                    {
                        rgbRenderer = rstRenderer as IRasterRGBRenderer;
                    }
                    else
                    {
                        rgbRenderer=new RasterRGBRendererClass();
                    }
                        //��ȡ������RGB��Ӧ����
                        rgbRenderer.RedBandIndex = cmb_RBand.SelectedIndex;
                        rgbRenderer.GreenBandIndex = cmb_GBand.SelectedIndex;
                        rgbRenderer.BlueBandIndex = cmb_BBand.SelectedIndex;
                        //������Ⱦ��
                        rstRenderer = rgbRenderer as IRasterRenderer;
                        rstRenderer.Update();
                        //��RGB��Ⱦ������ֵ��ͼ����Ⱦ��
                        rstLayer.Renderer = rstRenderer;
                    //���¿ؼ�
                    this.axMapControl1.ActiveView.Refresh();
                    this.axTOCControl1.Update();
                }
            }
            catch(System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        //դ��ͼ��Ψһֵ��Ⱦ
        public IRasterRenderer UniqueValueRenderer(ESRI.ArcGIS.Geodatabase.IRasterDataset rasterDataset)
        {
            try
            {
                //���դ��ͼ�����Ա����С
                IRaster2 raster = (IRaster2)rasterDataset.CreateDefaultRaster();
                ITable rasterTable = raster.AttributeTable;
                if (rasterTable == null) { return null; }
                int tableRows = rasterTable.RowCount(null);
                //Ϊÿһ�����Ե�Ψһֵ����������һ��Ψһ��ɫ
                IRandomColorRamp colorRamp = new RandomColorRampClass();
                //�������ɫ�������Բ���
                colorRamp.Size = tableRows;
                colorRamp.Seed = 100;
                //����createRamp����������ɫ��
                bool createColorRamp;
                colorRamp.CreateRamp(out createColorRamp);
                if (createColorRamp == false) { return null; }
                //����һ��Ψһֵ��Ⱦ��
                IRasterUniqueValueRenderer uvRenderer = new RasterUniqueValueRendererClass();
                IRasterRenderer rasterRenderer = (IRasterRenderer)uvRenderer;
                //������Ⱦ����դ�����ݶ������ԣ�
                rasterRenderer.Raster = rasterDataset.CreateDefaultRaster();
                rasterRenderer.Update();
                //������Ⱦ��������ֵ
                uvRenderer.HeadingCount = 1;
                uvRenderer.set_Heading(0, "ALL Data Value");
                uvRenderer.set_ClassCount(0, tableRows);
                uvRenderer.Field = "Value";//���߱��е������ֶ�
                //�������Ա�񣬷ֱ�����Ψһֵ��ɫ
                IRow row;
                //�����������ŽӿڵĶ����û�ÿһ������������ɫ���
                ISimpleFillSymbol fillSymbol;
                for (int i = 0; i < tableRows; i++)
                {
                    row = rasterTable.GetRow(i);
                    //Ϊĳһ���ض���������ֵ
                    uvRenderer.AddValue(0, i, Convert.ToByte(row.get_Value(1)));
                    //Ϊĳһ���ض���������ñ�ǩ
                    uvRenderer.set_Label(0, i, Convert.ToString(row.get_Value(1)));
                    //ʵ��������һ������������Ķ���
                    fillSymbol = new SimpleFillSymbolClass();
                    fillSymbol.Color = colorRamp.get_Color(i);
                    //Ϊĳһ�ض������������Ⱦ����
                    uvRenderer.set_Symbol(0, i, (ISymbol)fillSymbol);
                }
                return rasterRenderer;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
        //������ఴť��ʵ��դ��ͼ��ķ������
        private void btn_Classify_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡ����ķ��������
                string numStr = txb_ClassifyNumber.Text;
                int num = int.Parse(numStr);
                //��ȡѡ�е�ͼ��
                int indexLayer = cmb_ClassifyLayer.SelectedIndex;
                ILayer layer = this.axMapControl1.get_Layer(indexLayer);
                if (layer is IRasterLayer)
                {
                    //ת����IRasterLayer�ӿ�
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    if (rstLayer.BandCount > 1)
                    {
                        //��ȡͼ��raster��ת����IRaster2�ӿ�
                        IRaster2 raster2 = rstLayer.Raster as IRaster2;
                        //��ȡ��raster��RasterDataset
                        IRasterDataset rstDataset = raster2.RasterDataset;
                        //ת��IGeoDataset�ӿ�
                        IGeoDataset geoDataset = rstDataset as IGeoDataset;
                        //������Ԫ������������
                        IMultivariateOp mulop = new RasterMultivariateOpClass();
                        //�趨����ļ�����·��
                        string signatureFilePath = "D:\\RDB";
                        string fullPath = signatureFilePath + "\\" + "classify_signature";
                        string treePath = signatureFilePath + "\\" + "signature_treediagram";
                        //��ȡ�û�����ķ�������
                        int NumClass = num;
                        //ִ��isocluster���෽����ȡ��ά���Կռ��е����ص�Ԫ������ֵ
                        mulop.IsoCluster(geoDataset, fullPath, NumClass, 20, 20, 10);
                        //���������ݼ�
                        IGeoDataset outdataset;
                        //����Missing������
                        object missing = Type.Missing;
                        //���������Ȼ������ң��ͼ��Ǽල����
                        outdataset = mulop.MLClassify(geoDataset, fullPath, false, esriGeoAnalysisAPrioriEnum.esriGeoAnalysisAPrioriEqual, missing, missing);
                        //����������դ��
                        IRaster2 outraster1;
                        outraster1 = (IRaster2)outdataset;
                        //������դ�����ݣ�����դ������ͼ����ʾ������Ψһֵ��Ⱦ
                        IRasterRenderer rasterrender = UniqueValueRenderer(outraster1.RasterDataset);
                        //��դ��ͼ���м�����ʾ
                        IRasterLayer rasterLayer = new RasterLayerClass();
                        rasterLayer.CreateFromDataset(outraster1.RasterDataset);
                        rasterLayer.Name = "MLClassify";
                        //����դ����Ⱦ������
                        if (rasterrender != null)
                        {
                            rasterLayer.Renderer = rasterrender;
                        }
                        //����Ⱦ�õ�դ��ͼ����ص�map��
                        if (rasterLayer != null)
                        {
                            //���¿ؼ�
                            axMapControl1.Map.AddLayer(rasterLayer);
                        }
                        //classprobility
                        IGeoDataset outdataset2;
                        outdataset2 = mulop.ClassProbability(geoDataset, fullPath, esriGeoAnalysisAPrioriEnum.esriGeoAnalysisAPrioriSample, missing, missing);
                        IRaster2 outraster2 = (IRaster2)outdataset2;
                        IRasterLayer rasterLayer2 = new RasterLayerClass();
                        rasterLayer2.CreateFromDataset(outraster2.RasterDataset);
                        rasterLayer2.Name = "ClassProbility";
                        ILayer iLayer2 = rasterLayer2 as ILayer;
                        axMapControl1.Map.AddLayer(iLayer2);

                        axMapControl1.ActiveView.Refresh();
                        axTOCControl1.Update();
                        iniCmbItems();
                        //Dendrogram
                        mulop.Dendrogram(fullPath, treePath, true, Type.Missing);

                    }
                }


            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ڽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }
        //�����������ť���鿴������������ֵ
        private void btn_After_Classify_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ�е�ͼ��
                int indexLayer = cmb_ClassifyLayer.SelectedIndex;
                ILayer layer = this.axMapControl1.get_Layer(indexLayer);
                if (layer is IRasterLayer)
                {
                    //ת����IRasterLayer�ӿ�
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    //��ȡͼ��raster��ת����IRaster2�ӿ�
                    IRaster2 raster2 = rstLayer.Raster as IRaster2;
                    //��ȡ��raster��RasterDataset
                    IRasterDataset rstDataset = raster2.RasterDataset;
                    //ת��IGeoDataset�ӿ�
                    IGeoDataset geoDataset = rstDataset as IGeoDataset;
                    //Create the RasterGeneralizeOP object
                    IGeneralizeOp generalizeOp = new ESRI.ArcGIS.SpatialAnalyst.RasterGeneralizeOpClass();
                    //Declare the output raster object
                    IGeoDataset outdataset1 = generalizeOp.Aggregate(geoDataset, 4, ESRI.ArcGIS.GeoAnalyst.esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsMean, true, true);
                    IRaster2 outraster1 = (IRaster2)outdataset1;
                    IRasterLayer rasterLayer1 = new RasterLayerClass();
                    rasterLayer1.CreateFromDataset(outraster1.RasterDataset);
                    rasterLayer1.Name = "Aggregate";
                    ILayer iLayer1 = rasterLayer1 as ILayer;
                    axMapControl1.Map.AddLayer(iLayer1);

                    IGeoDataset outdataset2 = generalizeOp.BoundaryClean(geoDataset, ESRI.ArcGIS.SpatialAnalyst.esriGeoAnalysisSortEnum.esriGeoAnalysisSortAscending, true);
                    IRaster2 outraster2 = (IRaster2)outdataset2;
                    IRasterLayer rasterLayer2 = new RasterLayerClass();
                    rasterLayer2.CreateFromDataset(outraster2.RasterDataset);
                    rasterLayer2.Name = "BoundaryClean";
                    ILayer iLayer2 = rasterLayer2 as ILayer;
                    axMapControl1.Map.AddLayer(iLayer2);

                    //Majority filter
                    IGeoDataset outdataset3 = generalizeOp.MajorityFilter(geoDataset, true, false);
                    IRaster2 outraster3 = (IRaster2)outdataset3;
                    IRasterLayer rasterLayer3 = new RasterLayerClass();
                    rasterLayer3.CreateFromDataset(outraster3.RasterDataset);
                    rasterLayer3.Name = "MajorityFilter";
                    ILayer iLayer3 = rasterLayer3 as ILayer;
                    axMapControl1.Map.AddLayer(iLayer3);
                    //refresh the active view
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                    iniCmbItems();

                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ڽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }
        //�����ļ�ѡ��Ի��򣬵��ѡ�����ڼ��õ�ʸ���ļ�
        private void txb_ClipFeature_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                //���ļ�ѡ��Ի������öԻ�������
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Shp file (*.shp)|*.shp";
                openFileDialog.Title="ѡ��ʸ���ļ�";
                openFileDialog.Multiselect=false;
                string fileName="";
                //����Ի����ѳɹ�ѡ���ļ������ļ�·����Ϣ��д���������
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = openFileDialog.FileName;
                    txb_ClipFeature.Text = fileName;
                }
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //�������ʸ���ļ���դ���ļ��Ĳü�
        private void btn_Clip_Click(object sender, EventArgs e)
        {
            try
            {
                //��ȡѡ���դ��ͼ�㡢դ�����
                ILayer layer = GetLayerByName(cmb_ClipFeatureLayer.SelectedItem.ToString());
                IRasterLayer rstLayer = layer as IRasterLayer;
                IRaster2 raster2 = rstLayer.Raster as IRaster2;
                IRasterDataset rstDataset = raster2.RasterDataset;

                IRaster raster = rstDataset.CreateDefaultRaster();
                
                //��ȡʸ���ļ���·�����ļ�����
                string fileN = txb_ClipFeature.Text;
                FileInfo fileInfo = new FileInfo(fileN);
                string filePath = fileInfo.DirectoryName;
                string fileName = fileInfo.Name;

                //����ѡ���ʸ���ļ���·���򿪹����ռ�
                IWorkspaceFactory wsf = new ShapefileWorkspaceFactory();
                IWorkspace wp = wsf.OpenFromFile(filePath, 0);
                IFeatureWorkspace fw = (IFeatureWorkspace)wp;
                IFeatureClass featureClass = fw.OpenFeatureClass(fileName);

                //����һ�����ù�����
                IClipFilter2 clipRaster = new ClipFilterClass();
                clipRaster.ClippingType = esriRasterClippingType.esriRasterClippingOutside;
                //��ʸ�����ݵļ������Լӵ���������
                IGeometry clipGeometry;
                IFeature feature;

                //��ʸ�������е�ÿһ��IFeature������״��ӵ�clipGeometry
                for (int i = 0; i < featureClass.FeatureCount(null); i++)
                {
                    feature = featureClass.GetFeature(i);
                    clipGeometry=feature.Shape;
                    clipRaster.Add(clipGeometry);
                }
                //�����������������դ��ͼ��
                IPixelOperation pixelOp = (IPixelOperation)raster;
                pixelOp.PixelFilter = (IPixelFilter)clipRaster;

                //��������դ���в�������NoData������ʹ�ù������������ȣ�������ļ���������Ⱥ�NoData��ֵ
                IRasterProps rasterProps = (IRasterProps)raster;
                rasterProps.NoDataValue = 0;
                rasterProps.PixelType = rstPixelType.PT_USHORT;
                //�洢���ý��դ��ͼ��
                IWorkspace rstWs = wsf.OpenFromFile(@"D:\RDB", 0);
                //�������
                ISaveAs saveas = (ISaveAs)raster;
                saveas.SaveAs("clip_result.tif", rstWs, "TIFF");

                //������ʾ�ü����ͼ��
                IRasterLayer clipLayer = new RasterLayerClass();
                clipLayer.CreateFromRaster(raster);
                clipLayer.Name = "Clip_Result";
                clipLayer.SpatialReference = ((IGeoDataset)raster).SpatialReference;

                //��ӵ��ؼ���
                axMapControl1.AddLayer(clipLayer);
                axMapControl1.ActiveView.Refresh();
                axTOCControl1.Update();
                //����combobox�����ѡ�ͼ��Ͳ��ε�
                iniCmbItems();
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //�����ťʵ�ָ߿ռ�ֱ��ʵ�����ͼ��͵Ϳռ�ֱ��ʶನ��ͼ����ںϲ���
        private void btn_PanSharpen_Click(object sender, EventArgs e)
        {
            try {
                ILayer sigleLayer = GetLayerByName(cmb_PanSharpenSigleLayer.SelectedItem.ToString());
                ILayer multiLayer = GetLayerByName(cmb_PanSharpenMultiLayer.SelectedItem.ToString());
                IRaster2 panRaster2 = ((IRasterLayer)sigleLayer).Raster as IRaster2;
                IRaster2 multiRaster2 = ((IRasterLayer)multiLayer).Raster as IRaster2;
                IRasterDataset panDataset = panRaster2.RasterDataset;
                IRasterDataset multiDataset = multiRaster2.RasterDataset;
                //Ĭ�ϲ���˳��RGB�ͽ�����
                //����ȫɫ�Ͷ����դ�����ݼ���fullդ�����
                IRaster panRaster = ((IRasterDataset2)panDataset).CreateFullRaster();
                IRaster multiRaster = ((IRasterDataset2)multiDataset).CreateFullRaster();
                //���ú��Ⲩ��
                IRasterBandCollection rasterbandCol = (IRasterBandCollection)multiRaster;
                IRasterBandCollection infredRaster = new RasterClass();
                infredRaster.AppendBand(rasterbandCol.Item(3));

                //����ȫɫ���ε�����
                IRasterProps panSharpenRasterProps = (IRasterProps)multiRaster;
                IRasterProps panRasterProps = (IRasterProps)panRaster;
                panSharpenRasterProps.Width = panRasterProps.Width;
                panSharpenRasterProps.Height = panRasterProps.Height;
                panSharpenRasterProps.Extent = panRasterProps.Extent;
                multiRaster.ResampleMethod = rstResamplingTypes.RSP_BilinearInterpolationPlus;

                //����ȫɫ�񻯹����������������
                IPansharpeningFilter pansharpenFilter = new PansharpeningFilterClass();
                pansharpenFilter.InfraredImage = (IRaster)infredRaster;
                pansharpenFilter.PanImage = (IRaster)panRaster;
                pansharpenFilter.PansharpeningType = esriPansharpeningType.esriPansharpeningESRI;
                pansharpenFilter.PutWeights(0.1, 0.167, 0.167, 0.5);

                //��ȫɫ�񻯹����������ڶ����դ�������
                IPixelOperation pixeOperation = (IPixelOperation)multiRaster;
                pixeOperation.PixelFilter = (IPixelFilter)pansharpenFilter;

                //���������ݼ�����������ʾ
                //������ʾ�ü����ͼ��
                IRasterLayer panSharpenLayer = new RasterLayerClass();
                panSharpenLayer.CreateFromRaster(multiRaster);
                panSharpenLayer.Name = "panSharpen_Result";
                panSharpenLayer.SpatialReference = ((IGeoDataset)multiRaster).SpatialReference;

                //��ӵ��ؼ���
                axMapControl1.AddLayer(panSharpenLayer);
                axMapControl1.ActiveView.Refresh();
                axTOCControl1.Update();

                //����combobox�����ѡ�ͼ��Ͳ��εģ�
                iniCmbItems();
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }                     
        }

        //�����Ƕ��ť����ѡ�е�դ��Ŀ¼��ң��Ӱ�������Ƕ����
        private void btn_Mosaic_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //������õ��ַ�����������ʾԭդ�������ļ���·�������դ�����ݱ���·��
                //�ر�أ��ȴ���һ�����˵���ռ����ݿ⣬�������д���һ��դ��Ŀ¼
                string inputFolder = @"D:\RDB\mosaic";
                string outputFolder = @"D:\RDB";
                //string outputName = "mosaic.tif";
                string tempRasterCatalog = "temp_rc";
                string tempPGDB = "temp.mdb";
                string tempPGDBPath = outputFolder + "\\" + tempPGDB;
                string tempRasterCatalogPath = tempPGDBPath + "\\" + tempRasterCatalog;

                //ʹ��geoprocessing�������������ݿ⡢դ��Ŀ¼�Լ�����Ŀ¼��դ��Ŀ¼��
                Geoprocessor geoprocessor = new Geoprocessor();
                //����ʱ�ļ����д������˵������ݿ�
                CreatePersonalGDB createPersonalGDB = new CreatePersonalGDB();
                createPersonalGDB.out_folder_path = outputFolder;
                createPersonalGDB.out_name = tempPGDB;
                //����geopeocessor��execute����ִ�д������˵������ݿ�
                geoprocessor.Execute(createPersonalGDB, null);
                
                //���´����ĸ��˵������ݿ��д���һ�����йܵ�դ��Ŀ¼
                CreateRasterCatalog createRasterCatalog = new CreateRasterCatalog();
                //���ô�����դ��Ŀ¼�����·����������ֺ�դ���й�����
                createRasterCatalog.out_path = tempPGDBPath;
                createRasterCatalog.out_name = tempRasterCatalog;
                createRasterCatalog.raster_management_type = "unmanaged";
                //����geoprocessor��execute����ִ�д���դ��Ŀ¼
                geoprocessor.Execute(createRasterCatalog, null);

                //��������Ƕ��ԭʼդ�����ݼ��ص��´����ķ��йܵ�դ��Ŀ¼��
                WorkspaceToRasterCatalog wsToRasterCatalog = new WorkspaceToRasterCatalog();
                //���ü���դ�����ݵ�դ��Ŀ¼·����դ������·�������ص����ͣ��Ƿ�������ļ��У�
                wsToRasterCatalog.in_raster_catalog = tempRasterCatalogPath;
                wsToRasterCatalog.in_workspace = inputFolder;
                wsToRasterCatalog.include_subdirectories = "INCLUDE_SUBDIRECTORIES";
                //����geoprocessor��execute����ִ�м���դ�����ݵ�դ��Ŀ¼��
                geoprocessor.Execute(wsToRasterCatalog, null);

                //�򿪸ոմ����ĸ��˵������ݿ⣬�Ի�ȡդ��Ŀ¼����
                IWorkspaceFactory wsf = new AccessWorkspaceFactoryClass();
                IWorkspace mworkspace = wsf.OpenFromFile(tempPGDBPath, 0);
                IRasterWorkspaceEx rasterWorkspaceEx = mworkspace as IRasterWorkspaceEx;
                IRasterCatalog rstCatalog = rasterWorkspaceEx.OpenRasterCatalog(tempRasterCatalog);

                //����һ��Ӱ����Ƕ����
                IMosaicRaster mosaicRaster = new MosaicRasterClass();
                //��դ��Ŀ¼�����е�Ӱ����Ƕ��Ϊһ��դ��ͼ��
                mosaicRaster.RasterCatalog = rstCatalog;

                //������Ƕ����ɫӳ���ģʽ��������������
                mosaicRaster.MosaicColormapMode = rstMosaicColormapMode.MM_FIRST;
                mosaicRaster.MosaicOperatorType = rstMosaicOperatorType.MT_MAX;
                
                //�����������ݼ�����·���Ĺ����ռ�
                IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactory();
                string savefile = @"D:\RDB\";
                IWorkspace workspace_save = workspaceFactory.OpenFromFile(outputFolder,0);
                string filename = @"mosaic_result.tif";

                //ת��ISavsAs�ӿڣ�ʵ�ֽ�����ݼ����棬���Ա���ΪTif��������ʽ
                ISaveAs saveas = (ISaveAs)mosaicRaster;
                //ͨ��saves����ʵ����Ƕդ��ͼ��ı���
                saveas.SaveAs(filename, workspace_save, "TIFF");
                //���ݱ����ļ��Ĵ洢�ļ���·����ȡդ�����ռ�
                IWorkspaceFactory pRasterWsFac = new RasterWorkspaceFactoryClass();
                IWorkspace pWs = pRasterWsFac.OpenFromFile(savefile, 0);
                IRasterDataset pRasterDs = null;
                IRasterWorkspace pRasterWs;
                pRasterWs = pWs as IRasterWorkspace;
                pRasterDs = pRasterWs.OpenRasterDataset(filename);

                IRaster praster = pRasterDs.CreateDefaultRaster();
                IRasterLayer pLayer = new RasterLayerClass();
                pLayer.CreateFromRaster(praster);
                pLayer.Name = "mosaic_result";
                //���¿ؼ�
                axMapControl1.AddLayer(pLayer);
                axMapControl1.ActiveView.Refresh();
                axTOCControl1.Update();

            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }

        }
        //����任��ť����ѡ�е�ͼ��ʵʩͼ��任����
        private void btn_Transform_Click(object sender, EventArgs e)
        {
            try
            {
                ILayer layer = GetLayerByName(cmb_FliterLayer.SelectedItem.ToString());
                IRasterLayer rstLayer = layer as IRasterLayer;
                IRaster2 raster2 = rstLayer.Raster as IRaster2;
                IGeoDataset geoDataset = raster2.RasterDataset as IGeoDataset;
                int angle=0;
                if(txb_TransformAngle.Text!="") angle= int.Parse(txb_TransformAngle.Text);
                //����դ��ͼ��任�����ӿڵĶ���
                ITransformationOp transop = new RasterTransformationOpClass();
                //��������������ݼ��Ķ���
                IGeoDataset outdataset;

                switch (cmb_TransformMethod.SelectedIndex)
                {
                    case 0://��ת
                        outdataset = transop.Flip(geoDataset);
                        break;
                    case 1://����
                        outdataset = transop.Mirror(geoDataset);
                        break;
                    case 2://����
                        fClip = true;
                        MessageBox.Show("��ʹ�������ͼ�ϻ���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    case 3://��ת
                        object missing = Type.Missing;
                        outdataset = transop.Rotate(geoDataset, esriGeoAnalysisResampleEnum.esriGeoAnalysisResampleNearest, angle, ref missing);
                        break;
                    default:
                        return;

                }
                //ͨ��ͼ��仯�����ȡդ�����ݼ�����������դ��ͼ�������ʾ
                IRasterDataset rasterdataset;
                IRaster outraster;
                //��ȡ������ݼ�
                rasterdataset = (IRasterDataset)outdataset;
                outraster = rasterdataset.CreateDefaultRaster();
                IRaster resRaster = outraster as IRaster;
                IRasterLayer resLayer = new RasterLayerClass();
                resLayer.CreateFromRaster(resRaster);
                resLayer.Name = "Transformation";
                resLayer.SpatialReference = outdataset.SpatialReference;

                //������ʾդ��ͼ��
                axMapControl1.AddLayer(resLayer);
                axMapControl1.ActiveView.Refresh();
                axTOCControl1.Update();
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }           
        }
        ////���ڼ���״̬ʱ������ڵ�ͼ�ϵİ����¼���Ϊ���Ƽ��þ�����Ӧ
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            try
            {
                //����Ǵ��ڲü�״̬
                if (fClip == true)
                {
                    //��ȡ��Ļ��ʾ�йصĽӿڶ�����ActiveView�йأ�
                    IScreenDisplay screenDisplay = axMapControl1.ActiveView.ScreenDisplay;
                    //rubberband��Ƥ��ӿ���������ͼ��
                    IRubberBand rubberBand = new RubberEnvelopeClass();
                    //��ȡ���Ƶļ���ͼ��IGeometry�ӿ�
                    IGeometry geometry = rubberBand.TrackNew(screenDisplay, null);
                    //��IEnvelop�ӿڻ�ȡ����ͼ�εİ��緶Χ����
                    IEnvelope cutEnv = null;
                    cutEnv = geometry.Envelope;
                    //��ȡѡ�е�ͼ��
                    ILayer layer = GetLayerByName(cmb_TransformLayer.SelectedItem.ToString());
                    if (layer is IRasterLayer)
                    {
                        ITransformationOp transop = new RasterTransformationOpClass();
                        IRasterLayer rstLayer = layer as IRasterLayer;
                        IRaster2 raster2 = rstLayer.Raster as IRaster2;
                        IGeoDataset geoDataset = raster2.RasterDataset as IGeoDataset;

                        //��������������ݼ��Ķ���
                        IGeoDataset outdataset;
                        //ִ�м��ò���
                        outdataset = transop.Clip(geoDataset, cutEnv);

                        //ͨ��ͼ��仯�����ȡդ�����ݼ�����������դ��ͼ�������ʾ
                        IRasterDataset rasterdataset;
                        IRaster outraster;
                        //��ȡ������ݼ�
                        rasterdataset = (IRasterDataset)outdataset;
                        outraster = rasterdataset.CreateDefaultRaster();
                        IRaster resRaster = outraster as IRaster;
                        IRasterLayer resLayer = new RasterLayerClass();
                        resLayer.CreateFromRaster(resRaster);
                        resLayer.Name = "Transformation";
                        resLayer.SpatialReference = outdataset.SpatialReference;

                        //������ʾդ��ͼ��
                        axMapControl1.AddLayer(resLayer);
                        axMapControl1.ActiveView.Refresh();
                        axTOCControl1.Update();
                    }
                }
                //�����extraction����״̬
                else if (fExtraction == true)
                {
                    //��ȡѡ���ͼ��դ������
                    ILayer layer = GetLayerByName(cmb_Extraction.SelectedItem.ToString());
                    //��ȡդ��ͼ���դ�����
                    if (layer is IRasterLayer)
                    {
                        IRasterLayer rasterLayer = layer as IRasterLayer;
                        IRaster2 raster2 = rasterLayer.Raster as IRaster2;
                        IGeoDataset geoDataset = raster2.RasterDataset as IGeoDataset;
                        //�������Ļ���ƶ����
                        IPolygon pPolygon = axMapControl1.TrackPolygon() as IPolygon;
                        //����դ�����ݲü�����������ص������
                        IExtractionOp pExtractionOp = new ESRI.ArcGIS.SpatialAnalyst.RasterExtractionOpClass();
                        //ִ�вü����������õ�������ݼ�
                        IGeoDataset pGeoOutput = pExtractionOp.Polygon(geoDataset, pPolygon, true);

                        //������ʾ�ü��������
                        IRasterLayer resLayer = new RasterLayerClass();
                        //���ݽ��դ�����ݼ������͸�ֵդ��ͼ��
                        resLayer.CreateFromRaster((IRaster)pGeoOutput);
                        //ͼ������
                        resLayer.Name = "Extraction";
                        //���¿ؼ�
                        axMapControl1.Map.AddLayer(resLayer);
                        axMapControl1.ActiveView.Extent = resLayer.AreaOfInterest;
                        axMapControl1.ActiveView.Refresh();
                        axTOCControl1.Update();
                        //����combobox�����ѡ�ͼ��Ͳ��εģ�
                        iniCmbItems();
                    }
                    fExtraction = false;
                }
                //����Ǵ���ͨ�ӷ�����״̬
                else if (fLineOfSight == true)
                {
                    ILayer layer = GetLayerByName(cmb_LineOfSightLayer.SelectedItem.ToString());
                    if (layer is IRasterLayer)
                    {
                        //��ȡ��Ҫ����ͨ�ӷ�����դ�����ݶ���
                        IRasterLayer pRasterLayer = (IRasterLayer)layer;
                        //����դ���������Ķ������ô���դ�����ݶ���
                        IRasterSurface pRasterSurface = new RasterSurfaceClass();
                        pRasterSurface.PutRaster(pRasterLayer.Raster, 0);
                        //�ӿ�ת��ISurface
                        ISurface pSurface = pRasterSurface as ISurface;
                        //��ͼ�ϸ��ٻ���ֱ�ߣ��õ����ζ���
                        IPolyline pPolyline = axMapControl1.TrackLine() as IPolyline;
                        IPoint pPoint = null;
                        IPolyline pVPolyline = null;
                        IPolyline pInPolyline = null;
                        //���ò���
                        object pRef = 0.13;
                        bool pBool = true;
                        //��ȡDem�ĸ߳�
                        double pZ1 = pSurface.GetElevation(pPolyline.FromPoint);
                        double pZ2 = pSurface.GetElevation(pPolyline.ToPoint);
                        //����IPoint���󣬸�ֵ�̺߳�xyֵ
                        IPoint pPoint1 = new PointClass();
                        pPoint1.X = pPolyline.FromPoint.X;
                        pPoint1.Y = pPolyline.FromPoint.Y;
                        pPoint1.Z = pZ1;
                        IPoint pPoint2 = new PointClass();
                        pPoint2.X = pPolyline.ToPoint.X;
                        pPoint2.Y = pPolyline.ToPoint.Y;
                        pPoint2.Z = pZ2;
                        //����Isurface�ӿڵ�getlineofsight�����õ�ͨ�ӷ�Χ
                        pSurface.GetLineOfSight(pPoint1, pPoint2, out pPoint, out pVPolyline, out pInPolyline, out pBool, false, false,ref pRef);
                        if (pVPolyline != null)
                        {
                            //������ӷ�Χ��Ϊnull,�������Ⱦ����ʾ
                            IElement pLineElementV = new LineElementClass();
                            pLineElementV.Geometry = pVPolyline;
                            ILineSymbol pLinesymbolV = new SimpleLineSymbolClass();
                            pLinesymbolV.Width = 2;
                            IRgbColor pColorV = new RgbColorClass();
                            pColorV.Green = 255;
                            pLinesymbolV.Color = pColorV;
                            ILineElement pLineV = pLineElementV as ILineElement;
                            pLineV.Symbol = pLinesymbolV;
                            axMapControl1.ActiveView.GraphicsContainer.AddElement(pLineElementV, 0);
                        }
                        if (pInPolyline != null)
                        {
                            //��������ӷ�Χ��Ϊnull���������Ⱦ����ʾ
                            IElement pLineElementIn = new LineElementClass();
                            pLineElementIn.Geometry = pInPolyline;
                            ILineSymbol pLinesymbolIn = new SimpleLineSymbolClass();
                            pLinesymbolIn.Width = 2;
                            IRgbColor pColorIn = new RgbColorClass();
                            pColorIn.Red = 255;
                            pLinesymbolIn.Color = pColorIn;
                            ILineElement pLineIn = pLineElementIn as ILineElement;
                            pLineIn.Symbol = pLinesymbolIn;
                            axMapControl1.ActiveView.GraphicsContainer.AddElement(pLineElementIn,1);
                        }
                        //����ͼ��Χ���оֲ�ˢ��
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                    }
                    fLineOfSight = false;
                }
                //����Ǵ������������״̬
                else if (fVisibility == true)
                {
                    ILayer layer = GetLayerByName(cmb_VisibilityLayer.SelectedItem.ToString());
                    //��ȡդ��ͼ���դ�����
                    if (layer is IRasterLayer)
                    {
                        IRasterLayer rasterLayer = layer as IRasterLayer;
                        IRaster2 raster2 = rasterLayer.Raster as IRaster2;
                        IGeoDataset geoDataset = raster2.RasterDataset as IGeoDataset;
                        //�����ؼ�
                        IFeatureWorkspace fcw = (IFeatureWorkspace)workspace;
                        //����Ҫ������ֶμ���
                        IFields fields = new FieldsClass();
                        IFieldsEdit fieldsEdit = (IFieldsEdit)fields;
                        //���OID�ֶ�
                        IField oidField = new FieldClass();                      
                        IFieldEdit oidFieldEdit= (IFieldEdit)oidField;
                        oidFieldEdit.Name_2 ="0ID";
                        oidFieldEdit.Type_2= esriFieldType.esriFieldTypeOID;
                        fieldsEdit.AddField(oidField);
                        // ���������ֶ�
                        IGeometryDef geometryDef = new GeometryDefClass();
                        IGeometryDefEdit geometryDefEdit= (IGeometryDefEdit) geometryDef ;
                        geometryDefEdit.GeometryType_2 =esriGeometryType.esriGeometryPoint ;
                        ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                        ISpatialReference spatialReference=spatialReferenceFactory.CreateProjectedCoordinateSystem ((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_20N) ;
                        ISpatialReferenceResolution spatialReferenceResolution=(ISpatialReferenceResolution) spatialReference ;
                        spatialReferenceResolution.ConstructFromHorizon();
                        spatialReferenceResolution.SetDefaultXYResolution();
                        ISpatialReferenceTolerance spatialReferenceTolerance =(ISpatialReferenceTolerance) spatialReference ;
                        spatialReferenceTolerance.SetDefaultXYTolerance();
                        geometryDefEdit.SpatialReference_2 = spatialReference ;
                        // ��Ӽ����ֶ�
                        IField geometryField= new FieldClass();
                        IFieldEdit geometryFieldEdit= (IFieldEdit) geometryField ;
                        geometryFieldEdit.Name_2 = "Shape";
                        geometryFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                        geometryFieldEdit.GeometryDef_2 = geometryDef;
                        fieldsEdit.AddField(geometryField);
                        //����name�ֶ�
                        IField nameField = new FieldClass();
                        IFieldEdit nameFieldEdit = (IFieldEdit)nameField;
                        nameFieldEdit.Name_2 = "Name";
                        nameFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                        nameFieldEdit.Length_2 = 20;
                        fieldsEdit.AddField(nameField);
                        //����IFieldChecker������֤�ֶμ���
                        IFieldChecker fieldChecker = new FieldCheckerClass();
                        IEnumFieldError enumFieldError = null;
                        IFields validatedFields = null;
                        fieldChecker.ValidateWorkspace = (IWorkspace)fcw;
                        fieldChecker.Validate(fields, out enumFieldError, out validatedFields);
                        //����Ҫ����
                        Random rd = new Random();
                        int i = rd.Next();
                        IFeatureClass featureClass = fcw.CreateFeatureClass("visibility_featureclass"  + (i%10000), validatedFields, null, null, esriFeatureType.esriFTSimple, "Shape", "");

                        //�������Ļ���Ƶ�
                        IPoint pt;
                        pt = axMapControl1.ToMapPoint(e.x, e.y);
                        //����Ҫ��
                        IFeature feature = featureClass.CreateFeature();
                        feature.Shape = pt;
                        //Ӧ���ʵ���������ҪҪ����
                        ISubtypes subtypes = (ISubtypes)featureClass;
                        IRowSubtypes rowSubtypes = (IRowSubtypes)feature;
                        if (subtypes.HasSubtype) rowSubtypes.SubtypeCode = 3;
                        //��ʼ��Ҫ�ص�����Ĭ������
                        rowSubtypes.InitDefaultValues();
                        //ʵ�ֱ���
                        feature.Store();
                        //IfeatureClassת��IGeoDataset
                        IGeoDataset geodataset = (IGeoDataset)featureClass;
                        //����դ�����ݱ������������ص������
                        ISurfaceOp surfaceOp = new RasterSurfaceOpClass();
                        //ִ��������������õ�������ݼ�
                        IGeoDataset pGeoOutput = surfaceOp.Visibility(geoDataset, geodataset, esriGeoAnalysisVisibilityEnum.esriGeoAnalysisVisibilityObservers);
                        //ɾ���ոմ�����Ҫ����
                        IDataset dataset = featureClass as IDataset;
                        dataset.Delete();

                        //���غ���ʾ���

                        IRasterLayer resLayer = new RasterLayerClass();
                        resLayer.CreateFromRaster((IRaster)pGeoOutput);
                        resLayer.Name = "Visibility";
                        resLayer.SpatialReference = pGeoOutput.SpatialReference;

                        //������ʾդ��ͼ��
                        axMapControl1.AddLayer(resLayer);
                        axMapControl1.ActiveView.Refresh();
                        axTOCControl1.Update();
                    }
                    fVisibility = false;
                }
                else if (fTIN == true)
                {
                    //��ȡѡ�е�demͼ���raster����
                    ILayer layer = GetLayerByName(cmb_CreateTINLayer.SelectedItem.ToString());
                    //��ȡդ��ͼ���դ�����
                    if (layer is IRasterLayer)
                    {
                        //��ȡ��Ҫ���й���TIN��դ�����ݶ���
                        IRasterLayer pRasterLayer = (IRasterLayer)layer;
                        //Ϊ����TIN���Point���
                        IPoint Point = new PointClass();
                        Point = axMapControl1.ToMapPoint(e.x, e.y);
                        IRasterSurface rasterSurface = new RasterSurfaceClass();
                        rasterSurface.PutRaster(pRasterLayer.Raster, 0);
                        ISurface surface = rasterSurface as ISurface;
                        Point.Z = surface.GetElevation(Point);

                        //���point��TINedit��
                        TinEdit.AddPointZ(Point, 0);

                        //��������
                        //��ȡmap��activeView
                        IGraphicsContainer pGra = (IGraphicsContainer)axMapControl1.Map;
                        IActiveView pActiview = (IActiveView)pGra;

                        //����������element
                        IMarkerElement pMarkerElement = new MarkerElementClass();
                        //����������symbol
                        IMarkerSymbol pMarkSym = new SimpleMarkerSymbolClass();
                        IRgbColor rgbColor = new RgbColorClass();
                        rgbColor.Red = 255;
                        rgbColor.Green = 0;
                        rgbColor.Blue = 0;
                        pMarkSym.Color = rgbColor;
                        pMarkSym.Size = 5;
                        pMarkerElement.Symbol = pMarkSym;
                        //����Geometry
                        IElement pElement;
                        pElement = pMarkerElement as IElement;
                        pElement.Geometry = Point;
                        //��ӵ�marker,ˢ��activeview
                        pGra.AddElement(pElement, 0);
                        pActiview.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                    }
                }
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }      
        }
        //����˲���������ѡ�е�ͼ������˲�����
        private void btn_Filter_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ�е�ͼ��
                ILayer layer = GetLayerByName(cmb_FliterLayer.SelectedItem.ToString());
                IRasterLayer rstLayer = layer as IRasterLayer;
                IRaster raster = rstLayer.Raster;

                IConvolutionFunctionArguments rasterFunctionArguments = (IConvolutionFunctionArguments)new ConvolutionFunctionArguments();

                //��������դ������
                rasterFunctionArguments.Raster = raster;
                rasterFunctionArguments.Type = (esriRasterFilterTypeEnum)cmb_FliterMethod.SelectedIndex;
                //����Raster Function����
                IRasterFunction rasterFunction = new ConvolutionFunction();
                IFunctionRasterDataset functionRasterDataset = new FunctionRasterDataset();
                IFunctionRasterDatasetName functionRasterDatasetName = (IFunctionRasterDatasetName)new FunctionRasterDatasetNameClass();
                functionRasterDatasetName.FullName = @"D:\RDB"+"\\"+cmb_FliterMethod.SelectedItem.ToString();
                functionRasterDataset.FullName = (IName)functionRasterDatasetName;
                functionRasterDataset.Init(rasterFunction, rasterFunctionArguments);

                IRasterDataset rasData = functionRasterDataset as IRasterDataset;
                IRasterLayer pRstLayer = new RasterLayerClass();
                pRstLayer.CreateFromDataset(rasData);

                ILayer iLayer = pRstLayer as ILayer;
                axMapControl1.AddLayer(iLayer);
                axMapControl1.ActiveView.Refresh();
                axTOCControl1.Update();

                //����combobox�����ѡ�ͼ��ĺͲ��εģ�
                iniCmbItems();
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }

        //���ɽ����Ӱ��������ѡ����DEM���ݽ���ɽ����Ӱ��������
        private void btn_HillShade_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
               //��ȡѡ���ͼ��դ������
                ILayer layer = GetLayerByName(cmb_HillShade.SelectedItem.ToString());
                //��ȡդ��ͼ���դ�����
                if (layer is IRasterLayer)
                {
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    IRaster2 raster2 = rstLayer.Raster as IRaster2;
                    IHillshadeFunctionArguments hillshadeFunctionArugments = (IHillshadeFunctionArguments)new HillshadeFunctionArguments();
                    hillshadeFunctionArugments.Azimuth = 50;
                    hillshadeFunctionArugments.ZFactor = 1 / 11111.0;
                    hillshadeFunctionArugments.DEM = raster2;

                    IRasterFunction rasterFunction = new HillshadeFunction();
                    IFunctionRasterDataset functionRasterDataset= new FunctionRasterDataset();
                    IFunctionRasterDatasetName functionRasterDatasetName = (IFunctionRasterDatasetName)new FunctionRasterDatasetNameClass();
                    functionRasterDatasetName.FullName = @"D:\RDB" + "\\" + cmb_HillShade.SelectedItem.ToString()+"HillShade";
                    functionRasterDataset.FullName = (IName)functionRasterDatasetName;
                    functionRasterDataset.Init(rasterFunction, hillshadeFunctionArugments);

                    IRasterDataset rasData = functionRasterDataset as IRasterDataset;
                    IRasterLayer pRstLayer = new RasterLayerClass();
                    pRstLayer.CreateFromDataset(rasData);

                    ILayer iLayer = pRstLayer as ILayer;
                    axMapControl1.AddLayer(iLayer);
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                    //����������ѡ������
                    iniCmbItems();
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }
        //����¶Ⱥ�����ť����ѡ�е�dem���ݽ����¶ȼ���
        private void btn_Slope_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ���ͼ��դ������
                ILayer layer = GetLayerByName(cmb_Slope.SelectedItem.ToString());
                //��ȡդ��ͼ���դ�����
                if (layer is IRasterLayer)
                {
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    IRaster2 raster2 = rstLayer.Raster as IRaster2;
                    ISlopeFunctionArguments slopeFunctionArugments = (ISlopeFunctionArguments)new SlopeFunctionArguments();
                    slopeFunctionArugments.DEM = raster2;
                    slopeFunctionArugments.ZFactor = 1 / 11111.0;
                    IRasterFunction rasterFunction = new SlopeFunction();
                    IFunctionRasterDataset functionRasterDataset = new FunctionRasterDataset();
                    IFunctionRasterDatasetName functionRasterDatasetName = (IFunctionRasterDatasetName)new FunctionRasterDatasetNameClass();
                    functionRasterDatasetName.FullName = @"D:\RDB" + "\\" + cmb_Slope.SelectedItem.ToString() + "Slope";
                    functionRasterDataset.FullName = (IName)functionRasterDatasetName;
                    functionRasterDataset.Init(rasterFunction, slopeFunctionArugments);

                    IRasterDataset rasData = functionRasterDataset as IRasterDataset;
                    IRasterLayer pRstLayer = new RasterLayerClass();
                    pRstLayer.CreateFromDataset(rasData);

                    ILayer iLayer = pRstLayer as ILayer;
                    axMapControl1.AddLayer(iLayer);
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                    //����������ѡ������
                    iniCmbItems();
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }
        //�����������ť����ѡ�е�dem�����������������
        private void btn_Aspect_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ���ͼ��դ������
                ILayer layer = GetLayerByName(cmb_Aspect.SelectedItem.ToString());
                //��ȡդ��ͼ���դ�����
                if (layer is IRasterLayer)
                {
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    IRaster2 raster2 = rstLayer.Raster as IRaster2;

                    IRasterFunction rasterFunction = new AspectFunction();
                    IFunctionRasterDataset functionRasterDataset = new FunctionRasterDataset();
                    IFunctionRasterDatasetName functionRasterDatasetName = (IFunctionRasterDatasetName)new FunctionRasterDatasetNameClass();
                    functionRasterDatasetName.FullName = @"D:\RDB" + "\\" + cmb_Aspect.SelectedItem.ToString() + "Aspect";
                    functionRasterDataset.FullName = (IName)functionRasterDatasetName;
                    functionRasterDataset.Init(rasterFunction, raster2);

                    IRasterDataset rasData = functionRasterDataset as IRasterDataset;
                    IRasterLayer pRstLayer = new RasterLayerClass();
                    pRstLayer.CreateFromDataset(rasData);

                    ILayer iLayer = pRstLayer as ILayer;
                    axMapControl1.AddLayer(iLayer);
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                    //����������ѡ������
                    iniCmbItems();
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }
        //������������ť����ѡ�е�DEM���ݽ����������
        private void btn_Neighborhood_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ���ͼ��դ������
                ILayer layer = GetLayerByName(cmb_Aspect.SelectedItem.ToString());
                int indexMethod = cmb_NeighborhoodMethod.SelectedIndex;
                //��ȡդ��ͼ���դ�����
                if (layer is IRasterLayer)
                {
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    IRaster2 raster2 = rstLayer.Raster as IRaster2;
                    IGeoDataset geoDataset = raster2.RasterDataset as IGeoDataset;

                    //����դ�������������������ص������
                    INeighborhoodOp pNeighborhoodOP = new RasterNeighborhoodOpClass();
                    //����դ���������������������
                    IRasterNeighborhood pRasterNeighborhood = new RasterNeighborhoodClass();
                    //���þ������������Χ
                    pRasterNeighborhood.SetRectangle(3, 3, esriGeoAnalysisUnitsEnum.esriUnitsCells);
                    IGeoDataset pGeoOutput = null;
                    //ִ��������������õ�������ݼ�
                    switch (indexMethod)
                    {
                        case 0:
                            pGeoOutput = pNeighborhoodOP.Filter(geoDataset, esriGeoAnalysisFilterEnum.esriGeoAnalysisFilter3x3HighPass, true);
                            break;
                        case 1:
                            pGeoOutput = pNeighborhoodOP.Filter(geoDataset, esriGeoAnalysisFilterEnum.esriGeoAnalysisFilter3x3LowPass, true);
                            break;
                        case 2:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsMajority, pRasterNeighborhood, true);
                            break;
                        case 3:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsMaximum, pRasterNeighborhood, true);
                            break;
                        case 4:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsMean, pRasterNeighborhood, true);
                            break;
                        case 5:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsMedian, pRasterNeighborhood, true);
                            break;
                        case 6:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsMinimum, pRasterNeighborhood, true);
                            break;
                        case 7:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsMinority, pRasterNeighborhood, true);
                            break;
                        case 8:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsRange, pRasterNeighborhood, true);
                            break;
                        case 9:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsStd, pRasterNeighborhood, true);
                            break;
                        case 10:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsSum, pRasterNeighborhood, true);
                            break;
                        case 11:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsVariety, pRasterNeighborhood, true);
                            break;
                        case 12:
                            pGeoOutput = pNeighborhoodOP.BlockStatistics(geoDataset, esriGeoAnalysisStatisticsEnum.esriGeoAnalysisStatsLength, pRasterNeighborhood, true);
                            break;
                        default:
                            break;
                    }
                    //������ʾ�������������դ��ͼ��
                    IRasterLayer resultRstLayer = new RasterLayerClass();
                    resultRstLayer.CreateFromRaster((IRaster)pGeoOutput);
                    ILayer resultLayer = (ILayer)resultRstLayer;
                    axMapControl1.Map.AddLayer(resultRstLayer);
                    //ˢ����ͼ��ʾ
                    axMapControl1.ActiveView.Extent = resultRstLayer.AreaOfInterest;
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                    //����������ѡ������
                    iniCmbItems();
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btn_Extraction_Click(object sender, EventArgs e)
        {
            fExtraction = true;
            MessageBox.Show("��ʹ�������ͼ�ϻ���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        //���ͨ�ӷ�����ť������ͨ�ӷ����ĺ���
        private void btn_LineOfSight_Click(object sender, EventArgs e)
        {
            fLineOfSight = true;
            MessageBox.Show("��ʹ�������ͼ�ϻ���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        //������������ť��������������ĺ���
        private void btn_Visibility_Click(object sender, EventArgs e)
        {
            fVisibility = true;
            MessageBox.Show("��ʹ�������ͼ�ϻ���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;            
        }
        //����ֻ湹��TIN��ť�����������ƹ���TIN
        private void btm_CreateTin_Click(object sender, EventArgs e)
        {
            fTIN = true;
            ILayer layer = GetLayerByName(cmb_CreateTINLayer.SelectedItem.ToString());
            if (layer is IRasterLayer)
            {
                //��ȡ��Ҫ����TIN��դ�����ݶ���
                IRasterLayer rasterLayer = layer as IRasterLayer;
                //ʵ�����µĿյ�TIN 
                TinEdit = new TinClass();
                //��һ��Envelope��ʼ��TIN��envelope�Ŀռ�ο�Ҳ��ΪTIN�Ŀռ�ο�
                IEnvelope Env = rasterLayer.AreaOfInterest;
                //����envelop��ʼ��tin
                TinEdit.InitNew(Env);
                //��ȡmap�����element�ı��marker
                IGraphicsContainer pGra = (IGraphicsContainer)axMapControl1.Map;
                pGra.DeleteAllElements();
            }
            MessageBox.Show("��ʹ�������ͼ�ϻ���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;    
        }
        //�����DEM���ɵȸ��߰�ť����ѡ�е�DEM���ɵȸ���
        private void btn_DEMContour_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ���ͼ��դ������
                ILayer layer = GetLayerByName(cmb_DEMContour.SelectedItem.ToString());          
                //��ȡդ��ͼ���դ�����
                if (layer is IRasterLayer)
                {
                    IRasterLayer rstLayer = layer as IRasterLayer;
                    IRaster2 raster2 = rstLayer.Raster as IRaster2;
                    IGeoDataset geoDataset = raster2.RasterDataset as IGeoDataset;
                    //������������������
                    ISurfaceOp2 sfop = new RasterSurfaceOpClass();
                    //�������õĵȸ������߶ȡ��߶ȼ��ȶ�dem���ݹ����ȸ���
                    IGeoDataset outputGeoDataset = sfop.Contour(geoDataset, 100, Type.Missing, Type.Missing);

                    //������ʾ��õĵȸ���featureclass
                    IFeatureClass featureClass = outputGeoDataset as IFeatureClass;
                    IFeatureLayer featureLayer = new FeatureLayerClass();
                    featureLayer.Name = "DEM_Contour";
                    featureLayer.FeatureClass = featureClass;
                    axMapControl1.Map.AddLayer(featureLayer);
                    axMapControl1.ActiveView.Extent = featureLayer.AreaOfInterest;
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                    //����������ѡ������
                    iniCmbItems();
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }
        //���������������TIN�İ�ť������ѡ�е�DEM���в�������TIN
        private void btn_CreateTINAuto_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ���޸��������״
            try
            {
                //��ȡѡ���ͼ��դ������
                ILayer layer = GetLayerByName(cmb_CreateTINLayer.SelectedItem.ToString());
                //�ж�ͼ��
                if (layer is IRasterLayer)
                {
                    //��ȡmap���������element�ı��marker
                    IGraphicsContainer pGra = (IGraphicsContainer)axMapControl1.Map;
                    pGra.DeleteAllElements();

                    //��ȡ��Ҫ���й���TIN��դ�����ݶ���
                    IRasterLayer pRasterLayer = (IRasterLayer)layer;
                    IRaster2 iRaster = pRasterLayer.Raster as IRaster2;
                    //תIGeodataset�ӿڣ���ȡextent��Χ
                    IGeoDataset pGeoData = iRaster as IGeoDataset;
                    IEnvelope pExtent = pGeoData.Extent;
                    //ת��IRasterBandCollection�ӿڣ���ȡband����
                    IRasterBandCollection pRasBC = iRaster as IRasterBandCollection;
                    IRasterBand pRasBand = pRasBC.Item(0);
                    //ת��band�ӿ�ΪIRawPixels��IRasterProps����ȡ�߶ȺͿ�ȵ���Ϣ
                    IRawPixels pRawPixels = pRasBand as IRawPixels;
                    IRasterProps pProps = pRawPixels as IRasterProps;
                    int iWid = pProps.Width;
                    int iHei = pProps.Height;
                    double w = iWid / 1000.0f;
                    double h = iHei / 1000.0f;
                    //�������ؿ�ĸ߶ȺͿ��
                    IPnt pBlockSize = new DblPntClass();
                    bool IterationFlag;
                    if (w < 1 && h < 1)//���ݶ�С��1000����
                    {
                        pBlockSize.X = iWid;
                        pBlockSize.Y = iHei;
                        IterationFlag = false;
                    }
                    else
                    {
                        pBlockSize.X = 1001.0f;
                        pBlockSize.Y = 1001.0f;
                        IterationFlag = true;
                    }
                    //��ȡ��Ԫ��ߴ��С
                    double cellsize = 0.0f;//դ���С
                    IPnt pPnt1 = pProps.MeanCellSize();//դ��ƽ����С
                    cellsize = pPnt1.X;
                    //����ITinEdit�������Թ���TIN������ʼ��extent
                    ITinEdit pTinEdit = new TinClass() as ITinEdit;
                    pTinEdit.InitNew(pExtent);
                    //����ISpatialReference���󣬲�����
                    ISpatialReference pSpatial = pGeoData.SpatialReference;
                    pExtent.SpatialReference = pSpatial;
                    IPnt pOrigin = new DblPntClass();
                    //���ؿ�ԭ����Ϣ
                    IPnt pPixelBlockOrigin = new DblPntClass();
                    //դ�����Ͻ�������������
                    double bX = pBlockSize.X;
                    double bY = pBlockSize.Y;
                    pBlockSize.SetCoords(bX, bY);
                    //��ȡIPixelBlock����
                    IPixelBlock pPixelBlock = pRawPixels.CreatePixelBlock(pBlockSize);
                    object nodata = pProps.NoDataValue;//��ֵ���
                    //ת��ITinAdvanced2�ӿڣ���ȡNodeCount
                    ITinAdvanced2 pTinNodeCount = pTinEdit as ITinAdvanced2;
                    int nodeCount = pTinNodeCount.NodeCount;
                    object vtMissing = Type.Missing;
                    object vPixels = null;//����
                    if (!IterationFlag)//��Ϊһ������Ԫ����ʱ
                    {
                        //ԭ��������Ϣ
                        pPixelBlockOrigin.SetCoords(0.0f, 0.0f);
                        //��ȡ���ؿ�
                        pRawPixels.Read(pPixelBlockOrigin, pPixelBlock);
                        //��ȡ��������
                        vPixels = pPixelBlock.get_SafeArray(0);
                        double xMin = pExtent.XMin;
                        double yMax = pExtent.YMax;
                        pOrigin.X = xMin + cellsize / 2;
                        pOrigin.Y = yMax - cellsize / 2;
                        bX = pOrigin.X;
                        bY = pOrigin.Y;
                        //�ɼ�����ӽ��
                        pTinEdit.AddFromPixelBlock(bX, bY, cellsize, cellsize, nodata, vPixels, 20.0f, ref vtMissing, out vtMissing);
                    }
                    else//���ж������Ԫ��ʱ������ѭ������ÿ����Ԫ��
                    {
                        //����ÿ�����ؿ飬����AddFromPixelBlock��ӽ��
                        int i = 0, j = 0, count = 0;
                        int FirstGoNodeCout = 0;
                        while (nodeCount != FirstGoNodeCout)
                        {
                            count++;
                            nodeCount = pTinNodeCount.NodeCount;
                            //����ѭ������
                            for (i = 0; i < h + 1; i++)
                            {
                                for (j = 0; j < w + 1; j++)
                                {
                                    double bX1, bY1, xMin1, yMax1;
                                    //���ؿ�߶ȿ��
                                    bX1 = pBlockSize.X;
                                    bY1 = pBlockSize.Y;
                                    pPixelBlockOrigin.SetCoords(j * bX1, i * bY1);
                                    //��ȡ���ؿ�
                                    pRawPixels.Read(pPixelBlockOrigin, pPixelBlock);
                                    //��ȡ��������
                                    vPixels = pPixelBlock.get_SafeArray(0);
                                    //ԭ��������Ϣ
                                    xMin1 = pExtent.XMin;
                                    yMax1 = pExtent.YMax;
                                    bX1 = pBlockSize.X;
                                    bY1 = pBlockSize.Y;
                                    pOrigin.X = xMin1 + j * bX1 * cellsize + cellsize / 2.0f;
                                    pOrigin.Y = yMax1 + i * bY1 * cellsize - cellsize / 2.0f;
                                    bX1 = pOrigin.X;
                                    bY1 = pOrigin.Y;
                                    //����AddFromPixelBlock��ӽ��
                                    pTinEdit.AddFromPixelBlock(bX1, bY1, cellsize, cellsize, nodata, vPixels, 20.0f, ref vtMissing, out vtMissing);
                                    FirstGoNodeCout = pTinNodeCount.NodeCount;
                                }
                            }
                        }
                    }
                    //����TINͼ��
                    ITinLayer pTinLayer = new TinLayerClass();
                    pTinLayer.Name = "TIN";
                    //����TIN����
                    pTinLayer.Dataset = (ITinAdvanced2)pTinEdit;
                    //ˢ����ͼ
                    axMapControl1.Map.AddLayer(pTinLayer as ILayer);
                    axMapControl1.ActiveView.Extent = pTinLayer.AreaOfInterest;
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                    //����combobox�����ѡ�ͼ��Ͳ��εģ�
                    iniCmbItems();
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }
        //������ɵȸ��߰�ť����ѡ�е�TIN���ɵȸ���
        private void btn_tinContour_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ�е�TINͼ��
                ILayer layer = GetLayerByName(cmb_tinContour.SelectedItem.ToString());
                //�ж�TINͼ��
                if (layer is ITinLayer)
                {
                    //��ȡTIN����
                    ITinLayer tinLayer = layer as ITinLayer;
                    ITin tin = tinLayer.Dataset;
                    ITinSurface tinSurface = tin as ITinSurface;
                    //ʵ����Ҫ��������������ȡ����Ҫ���������ֶμ���
                    IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
                    IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
                    IFields fields = ocDescription.RequiredFields;
                    //�ҵ�shape��״�ֶΣ����ü������ͺ�ͶӰ����ϵͳ
                    int shapeFieldIndex = fields.FindField(fcDescription.ShapeFieldName);
                    IField field = fields.get_Field(shapeFieldIndex);
                    IGeometryDef geometryDef = field.GeometryDef;
                    IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                    //���ü�������
                    geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
                    ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                    ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_20N);
                    ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)spatialReference;
                    spatialReferenceResolution.ConstructFromHorizon();
                    spatialReferenceResolution.SetDefaultXYResolution();
                    ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)spatialReference;
                    spatialReferenceTolerance.SetDefaultXYTolerance();
                    //��������ϵͳ
                    geometryDefEdit.SpatialReference_2 = spatialReference;

                    //ת�������ռ�ӿ�
                    IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                    //����Ҫ����
                    IFeatureClass featureClass = featureWorkspace.CreateFeatureClass("TinContour", fields, ocDescription.InstanceCLSID, ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");

                    //����ITinSurface��Contour�������ɵȸ���
                    tinSurface.Contour(0, 100, featureClass, "Height", 0);

                    IFeatureLayer featureLayer = new FeatureLayerClass();
                    featureLayer.FeatureClass = featureClass;
                    featureLayer.Name = "TinContour";

                    axMapControl1.Map.AddLayer(featureLayer);
                    axMapControl1.ActiveView.Extent = featureLayer.AreaOfInterest;
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();

                    iniCmbItems();
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }
        }
        //�������̩ɭ����εİ�ť����ѡ�е�TIN����̩ɭ�����
        private void btn_Voronoi_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;//����ʱ�޸��������״
            try
            {
                //��ȡѡ�е�TINͼ��
                ILayer layer = GetLayerByName(cmb_TinVoronoi.SelectedItem.ToString());
                //�ж�TINͼ��
                if (layer is ITinLayer)
                {
                    //��ȡTIN����
                    ITinLayer tinLayer = layer as ITinLayer;
                    ITin tin = tinLayer.Dataset;
                    ITinNodeCollection tinNodeCollection = tin as ITinNodeCollection;

                    //ʵ����Ҫ��������������ȡ����Ҫ���������ֶμ���
                    IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
                    IObjectClassDescription ocDescription = (IObjectClassDescription)fcDescription;
                    IFields fields = ocDescription.RequiredFields;
                    //�ҵ�shape��״�ֶΣ����ü������ͺ�ͶӰ����ϵͳ
                    int shapeFieldIndex = fields.FindField(fcDescription.ShapeFieldName);
                    IField field = fields.get_Field(shapeFieldIndex);
                    IGeometryDef geometryDef = field.GeometryDef;
                    IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                    //���ü�������
                    geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
                    ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                    ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_20N);
                    ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)spatialReference;
                    spatialReferenceResolution.ConstructFromHorizon();
                    spatialReferenceResolution.SetDefaultXYResolution();
                    ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)spatialReference;
                    spatialReferenceTolerance.SetDefaultXYTolerance();
                    //��������ϵͳ
                    geometryDefEdit.SpatialReference_2 = spatialReference;

                    //ת�������ռ�ӿ�
                    IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                    //���浽ָ��·��
                    IWorkspaceFactory wsf = new ShapefileWorkspaceFactory();
                    IWorkspace wp = wsf.OpenFromFile("D://RDB", 0);
                    IFeatureWorkspace fw = (IFeatureWorkspace)wp;
                    //����Ҫ����
                    IFeatureClass featureClass = featureWorkspace.CreateFeatureClass("TinVoronoi", fields, ocDescription.InstanceCLSID, ocDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDescription.ShapeFieldName, "");

                    //����ITinSurface��Contour�������ɵȸ���
                    tinNodeCollection.ConvertToVoronoiRegions(featureClass,null,null,"","");

                    IFeatureLayer featureLayer = new FeatureLayerClass();
                    featureLayer.FeatureClass = featureClass;
                    featureLayer.Name = "TinVoronoi";

                    axMapControl1.Map.AddLayer(featureLayer);
                    axMapControl1.ActiveView.Extent = featureLayer.AreaOfInterest;
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();

                    iniCmbItems();
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally//����ٽ���������ó�Ĭ����״
            {
                this.Cursor = Cursors.Default;
            }

        }
        //����TINѡ�����ʱ��������
        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)
        {
            try
            {
                if (fTIN == true)
                {
                    //��ԭfTIN��Ǳ�����ֵ
                    fTIN = false;
                    ITinLayer tinLayer = new TinLayerClass();
                    tinLayer.Name = "TIN";
                    tinLayer.Dataset = (ITinAdvanced2)TinEdit;

                    axMapControl1.Map.AddLayer(tinLayer);
                    axMapControl1.ActiveView.Extent = tinLayer.AreaOfInterest;
                    axMapControl1.ActiveView.Refresh();
                    axMapControl1.Update();

                    iniCmbItems();
                    
                }
            }
            catch (System.Exception ex)//�����쳣������쳣��Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //���������Ϣͳ�Ƶײ㰴ť�󣬴ӵײ�ʵ�ֲ�����Ϣͳ��
        private void btn_Statistics_Bottom_Click(object sender, EventArgs e)
        {
            try
            {
                //��ȡѡ���ͼ��Ͳ��Σ�ת���ӿ�
                ILayer layer = GetLayerByName(cmb_StatistiicsLayer.SelectedItem.ToString());
                IRasterLayer rstLayer = null;
                if (layer is IRasterLayer) rstLayer = layer as IRasterLayer;
                else
                {
                    MessageBox.Show("��ѡ���ͼ�㲢��դ��ͼ�㣬�޷����в���", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //IRasterLayer rstLayer= GetLayerByName(cmb_NDVILayer.SelectedItem.ToString()) as IRasterLayer;
                IRaster2 raster2 = rstLayer.Raster as IRaster2;
                IRasterDataset rstDataset = raster2.RasterDataset;
                IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;

                int index = cmb_StatisticsBand.SelectedIndex;
                if (cmb_StatisticsBand.SelectedItem.ToString() == "ȫ������")
                {
                    string StatRes = null;
                    IRasterProps pRasterProps = raster2 as IRasterProps;
                    int Height = pRasterProps.Height;
                    int Width = pRasterProps.Width;
                    //���岢��ʼ�����飬�û��洢դ����������Ԫ����ֵ
                    double[,] PixelValue = new double[Height, Width];
                    for (int k = 0; k < rstBandColl.Count; k++)
                    {
                        PixelValue = GetPixelValue(rstLayer, k);
                        double sum = 0;
                        int count = 0;
                        double max = PixelValue[0, 0];
                        double min = PixelValue[0, 0];
                        for (int i = 0; i < Height; i++)
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                if (PixelValue[i, j] > max) max = PixelValue[i, j];
                                if (PixelValue[i, j] < min) min = PixelValue[i, j];
                                sum = sum + PixelValue[i, j];
                                count++;
                            }
                        }
                        double mean = sum / count;
                        double stdsum = 0;
                        for (int i = 0; i < Height; i++)
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                stdsum = stdsum + (PixelValue[i, j] - mean) * (PixelValue[i, j] - mean);
                            }
                        }
                        double std = Math.Sqrt(stdsum / (count - 1));
                        StatRes = StatRes+ "��" + (k + 1) + "���Σ�" + "  ƽ��ֵΪ:" + mean.ToString() + "  ���ֵΪ��" + max.ToString() + "  ��СֵΪ:" + min.ToString() + "  ��׼��Ϊ:" + std.ToString() + "\r\n";
                    }
                    //��ʾ�����ͳ�ƽ��
                    MessageBox.Show(StatRes, "ͳ�ƽ��", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    int bandnum;
                    if (cmb_StatisticsBand.Items.Count > rstBandColl.Count) bandnum = index - 1;
                    else bandnum = index;
                    IRasterProps pRasterProps = raster2 as IRasterProps;
                    int Height = pRasterProps.Height;
                    int Width = pRasterProps.Width;
                    //���岢��ʼ�����飬�û��洢դ����������Ԫ����ֵ
                    double[,] PixelValue = new double[Height, Width];
                    PixelValue = GetPixelValue(rstLayer, bandnum);

                    double sum = 0;
                    int count = 0;
                    double max = PixelValue[0, 0];
                    double min = PixelValue[0, 0];
                    for (int i = 0; i < Height; i++)
                    {
                        for (int j = 0; j < Width; j++)
                        {
                            if (PixelValue[i, j] > max) max = PixelValue[i, j];
                            if (PixelValue[i, j] < min) min = PixelValue[i, j];
                            sum = sum + PixelValue[i, j];
                            count++;
                        }
                    }
                    double mean = sum / count;
                    double stdsum=0;
                    for (int i = 0; i < Height; i++)
                    {
                        for (int j = 0; j < Width; j++)
                        {
                            stdsum = stdsum + (PixelValue[i, j] - mean) * (PixelValue[i, j] - mean);
                        }
                    }
                    double std = Math.Sqrt(stdsum / (count-1));
                    string StatRes = "��" + (bandnum + 1) + "���Σ�" + "  ƽ��ֵΪ:" + mean.ToString() + "  ���ֵΪ��" + max.ToString() + "  ��СֵΪ:" + min.ToString() + "  ��׼��Ϊ:" + std.ToString() + "\r\n";
                    MessageBox.Show(StatRes, "ͳ�ƽ��", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //���NDVI����ײ㰴ť�󣬴ӵײ�ʵ��NDVI����
        private void btn_NDVI_Bottom_Click(object sender, EventArgs e)
        {
            try
            {
                //��ȡѡ���ͼ��Ͳ��Σ�ת���ӿ�
                ILayer layer = GetLayerByName(cmb_NDVILayer.SelectedItem.ToString());
                IRasterLayer rstLayer = null;
                if (layer is IRasterLayer) rstLayer = layer as IRasterLayer;
                else
                {
                    MessageBox.Show("��ѡ���ͼ�㲢��դ��ͼ�㣬�޷����в���", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //IRasterLayer rstLayer= GetLayerByName(cmb_NDVILayer.SelectedItem.ToString()) as IRasterLayer;
                IRaster2 raster2 = rstLayer.Raster as IRaster2;
                IGeoDataset geoDataset = raster2.RasterDataset as IGeoDataset;
                IRasterDataset rstDataset = raster2.RasterDataset;
                IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;
                if (rstBandColl.Count < 3)
                {
                    MessageBox.Show("��ѡ���ͼ���դ��ͼ�㲻����NDVI��������", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                IRasterProps pRasterProps = raster2 as IRasterProps;
                int Height = pRasterProps.Height;
                int Width = pRasterProps.Width;
                double cellsizex = pRasterProps.MeanCellSize().X;
                
                double cellsizey = pRasterProps.MeanCellSize().Y;
                rstPixelType pixelType = pRasterProps.PixelType;
                ISpatialReference spatialReference = pRasterProps.SpatialReference;
                //MessageBox.Show(spatialReference.Name.ToString());
                IWorkspaceFactory pRasterWsFac = new RasterWorkspaceFactoryClass();
                IWorkspace pWs = pRasterWsFac.OpenFromFile(@"F:/RDB", 0);
                IRasterWorkspace2 pRasterWs;
                pRasterWs = pWs as IRasterWorkspace2;
                IPoint origin = new PointClass();
                origin.PutCoords(pRasterProps.Extent.XMin, pRasterProps.Extent.YMin);
                //RasterWorkspace rasterWorkspace = (RasterWorkspace)workspace;
                ISpatialReference sr = new UnknownCoordinateSystemClass();
                IRasterDataset2 resultDataset = pRasterWs.CreateRasterDataset("NDVI.tif", "TIFF", origin, Width, Height, cellsizex, cellsizey, 1, rstPixelType.PT_DOUBLE, sr) as IRasterDataset2;
                IRaster resultRaster = resultDataset.CreateFullRaster();
                IRasterCursor resultRasterCursor = ((IRaster2)resultRaster).CreateCursorEx(null);

                IRasterDataset2 rasterDataset = raster2.RasterDataset as IRasterDataset2;
                IRaster2 raster = rasterDataset.CreateFullRaster() as IRaster2;
                IRasterCursor rasterCursor = raster.CreateCursorEx(null);

                IPixelBlock3 resultPixelBlock = null;
                IPixelBlock3 tempPixelBlock = null;
                IRasterEdit resultRasterEdit = resultRaster as IRasterEdit;

                long blockWidth = 0;
                long blockHeight = 0;
               // System.Array pixels;
                do
                {
                     resultPixelBlock = resultRasterCursor.PixelBlock as IPixelBlock3;
                     tempPixelBlock = rasterCursor.PixelBlock as IPixelBlock3;

                     System.Array pixels3 = (System.Array)tempPixelBlock.get_PixelData(2);
                     System.Array pixels4 = (System.Array)tempPixelBlock.get_PixelData(3);
                     //MessageBox.Show(pixels3.GetValue(0, 0).GetType().ToString());
                     blockHeight = resultPixelBlock.Height;
                     blockWidth = resultPixelBlock.Width;
                     System.Array resultPixels = (System.Array)resultPixelBlock.get_PixelData(0);
                     //MessageBox.Show(resultPixels.GetValue(0, 0).GetType().ToString());
                     for (int i = 0; i < blockHeight; i++)
                     {
                         for (int j = 0; j < blockWidth; j++)
                         {
                             double up = double.Parse(pixels4.GetValue(j, i).ToString()) - double.Parse(pixels3.GetValue(j, i).ToString());
                             double down = double.Parse(pixels4.GetValue(j, i).ToString()) + double.Parse(pixels3.GetValue(j, i).ToString());
                             if (down != 0)
                             {
                                 resultPixels.SetValue((up/down) , j, i);
                             }
                             else
                             {
                                 resultPixels.SetValue((0.0), j, i);
                             }
                         }
                     }
                     resultPixelBlock.set_PixelData(0, (System.Array)resultPixels);
                     resultRasterEdit.Write(resultRasterCursor.TopLeft, (IPixelBlock)resultPixelBlock);
                     resultRasterEdit.Refresh();

                } while (resultRasterCursor.Next() == true && rasterCursor.Next() == true);

                IRasterDataset pRasterDs = pRasterWs.OpenRasterDataset("NDVI.tif");
                IRaster praster = pRasterDs.CreateDefaultRaster();

                IRasterLayer resLayer = new RasterLayerClass();
                resLayer.CreateFromRaster(praster);
                //resLayer.SpatialReference = ((IGeoDataset)pRasterDs).SpatialReference;
                resLayer.Name = "NDVI";
                ////���˵�����ͼ���ûҶ���ʾ�������������Сֵ����
                IRasterStretchColorRampRenderer grayStretch = null;
                if (resLayer.Renderer is IRasterStretchColorRampRenderer) grayStretch = resLayer.Renderer as IRasterStretchColorRampRenderer;
                else grayStretch = new RasterStretchColorRampRendererClass();
                IRasterStretch2 rstStr2 = grayStretch as IRasterStretch2;
                rstStr2.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_MinimumMaximum;//��������ģʽΪ�����Сֵ����
                resLayer.Renderer = grayStretch as IRasterRenderer;
                resLayer.Renderer.Update();

                //���NDVIͼ����ʾ����ˢ����ͼ
                axMapControl1.AddLayer(resLayer);
                axMapControl1.ActiveView.Extent = resLayer.AreaOfInterest;
                axMapControl1.ActiveView.Refresh();
                this.axTOCControl1.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //��ȡָ��ͼ��ָ�����ε�դ��ͼ����Ϣ,����һ����ά����
        private double[,] GetPixelValue(IRasterLayer pRasterLayer,int bandIndex)
        {
            IRaster2 raster2 = pRasterLayer.Raster as IRaster2;
            IRasterDataset2 rasterDataset = raster2.RasterDataset as IRasterDataset2;
            IRaster2 raster = rasterDataset.CreateFullRaster() as IRaster2;

            IRasterProps pRasterProps = raster as IRasterProps;
            int Height = pRasterProps.Height;
            int Width = pRasterProps.Width;
            //���岢��ʼ�����飬�û��洢դ����������Ԫ����ֵ
            double[,] PixelValue = new double[Height, Width];
            System.Array pixels;
            double[] PixelArray = new double[256];

            IRasterCursor rasterCursor = raster.CreateCursorEx(null);
            long blockWidth = 0;
            long blockHeight = 0;
            IPixelBlock3 pixelBlock3 = null;
            do
            {
                pixelBlock3 = rasterCursor.PixelBlock as IPixelBlock3;
                int left = (int)rasterCursor.TopLeft.X;
                int top = (int)rasterCursor.TopLeft.Y;

                blockHeight = pixelBlock3.Height;
                blockWidth = pixelBlock3.Width;
                //MessageBox.Show(pixelBlock3.Planes.ToString());
                pixels = (System.Array)pixelBlock3.get_PixelData(bandIndex);
                for (int i = 0; i < blockHeight; i++)
                {
                    for (int j = 0; j < blockWidth; j++)
                    {
                        PixelValue[top + i, left + j] = Convert.ToDouble(pixels.GetValue(j, i));
                    }
                }
            } while (rasterCursor.Next() == true);

            return PixelValue;
        }

        //���Ҽ�ѡȡ��դ������޸�
        private void tsmiEditRasterFunction_Click(object sender, EventArgs e)
        {
            try
            {
                if (TOCRightLayer is IRasterLayer)
                {
                    RasterFunctionEditor rstFunEditor = new RasterFunctionEditor(((IRasterLayer)TOCRightLayer).Raster);
                    rstFunEditor.ShowDialog();
                    if (rstFunEditor.IsFinished)
                    {
                        IRasterLayer resLayer = new RasterLayerClass();
                        resLayer.Name = "Editing";
                        resLayer.CreateFromRaster(rstFunEditor.GetRaster());
                        axMapControl1.AddLayer(resLayer);
                        axMapControl1.ActiveView.Refresh();
                        axTOCControl1.Update();
                        iniCmbItems();
                    }
                }             
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //���ⲿ�ļ������դ������޸�
        private void miRasterFunctionEditor_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Imag file (*.img)|*img|Tiff file(*.tif)|*.tif|Openflight file (*.flt)|*.flt";
                openFileDialog.Title = "��Ӱ������";
                openFileDialog.Multiselect = false;
                string fileName = "";
                //����Ի����ѳɹ�ѡ���ļ������ļ�·����Ϣ��д���������
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = openFileDialog.FileName;
                }
                FileInfo fileInfo = new FileInfo(fileName);
                string filePath = fileInfo.DirectoryName;
                string file = fileInfo.Name;
                IWorkspaceFactory rstWorkspaceFactoryImport = new RasterWorkspaceFactoryClass();
                IRasterWorkspace rstWorkspaceImport = (IRasterWorkspace)rstWorkspaceFactoryImport.OpenFromFile(filePath, 0);
                IRasterDataset rstDatasetImport = null;
                //���ѡ����ļ�·���ǲ�����Ч��դ�����ռ�
                if (!(rstWorkspaceImport is IRasterWorkspace))
                {
                    MessageBox.Show("�ļ�·��������Ч��դ�����ռ䣡");
                    return;
                }
                //����ѡ���դ��ͼ�����ֻ�ȡդ�����ݼ�
                rstDatasetImport = rstWorkspaceImport.OpenRasterDataset(file);
                //��IRasterDataset�ӿڵ�CreateDefaultRaster���������հ׵�դ�����
                IRaster raster = rstDatasetImport.CreateDefaultRaster();

                RasterFunctionEditor rstFunEditor = new RasterFunctionEditor(raster);
                rstFunEditor.ShowDialog();
                if (rstFunEditor.IsFinished)
                {
                    IRasterLayer resLayer = new RasterLayerClass();
                    resLayer.Name = "Editing";
                    resLayer.CreateFromRaster(rstFunEditor.GetRaster());
                    axMapControl1.AddLayer(resLayer);
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update();
                    iniCmbItems();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }     
        }

        //ͼ����и���ʱ��ˢ��ӥ��ѡ��
        private void axMapControl1_OnAfterScreenDraw(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            InitEagleEye();
        }

        //��ӥ��Map��ͼ�����
        private void InitEagleEye()
        {
            if (axMapControl2.LayerCount > 0)
            {
                axMapControl2.ClearLayers();
            }
            for (int i = axMapControl1.LayerCount - 1; i >= 0; i--)
            {
                ILayer pLayer = axMapControl1.get_Layer(i);
                if (pLayer is IGroupLayer || pLayer is ICompositeLayer)
                {
                    ICompositeLayer pCompositeLayer = pLayer as ICompositeLayer;
                    for (int j = pCompositeLayer.Count - 1; j >= 0; j--)
                    {
                        IFeatureLayer pFeatureLayer = pCompositeLayer.get_Layer(j) as IFeatureLayer;
                        if (pFeatureLayer != null)
                        {
                            if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint
                            && pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryMultipoint)
                                axMapControl2.AddLayer(pFeatureLayer);
                        }
                    }
                }
                else
                {
                    if (pLayer is IFeatureLayer)
                    {
                        IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
                        if (pFeatureLayer != null)
                        {
                            if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint
                                && pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryMultipoint)
                                axMapControl2.AddLayer(pLayer);
                        }
                    }
                    else if (pLayer is IRasterLayer)
                    {
                        axMapControl2.AddLayer((IRasterLayer)pLayer);
                    }                  
                }
                axMapControl2.Extent = axMapControl1.FullExtent;
                axMapControl2.SpatialReference = axMapControl1.SpatialReference;
                pEnv = axMapControl1.Extent as IEnvelope;
                DrawRectangle(pEnv);
                axMapControl2.ActiveView.Refresh();
            }

        }

        //����Map1�Ĳ��ֽ���Env�Ļ���
        private void DrawRectangle(IEnvelope pEnvelope)
        {
            IGraphicsContainer pGraphicsContainer = axMapControl2.Map as IGraphicsContainer;
            pGraphicsContainer.DeleteAllElements();
            IActiveView pActiveView = pGraphicsContainer as IActiveView;

            IRectangleElement recElement = new RectangleElementClass();
            IElement pElement = recElement as IElement;
            pElement.Geometry = pEnv;

            ILineSymbol pOutline = new SimpleLineSymbolClass();
            pOutline.Width = 2;
            pOutline.Color = GetRgbColor(255, 0, 0, 255);

            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pFillSymbol.Color = GetRgbColor(0, 0, 0, 0);
            pFillSymbol.Outline = pOutline;

            IFillShapeElement pFillShapeElement = pElement as IFillShapeElement;
            pFillShapeElement.Symbol = pFillSymbol;
            pGraphicsContainer.AddElement((IElement)pFillShapeElement, 0);
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        //����RGBֵ
        private IRgbColor GetRgbColor(int intR, int intG, int intB, int intT)
        {
            IRgbColor pRgbColor = null;
            if (intR < 0 || intR > 255 || intG < 0 || intG > 255 || intB < 0 || intB > 255)
            {
                return pRgbColor;
            }
            pRgbColor = new RgbColorClass();
            pRgbColor.Red = intR;
            pRgbColor.Green = intG;
            pRgbColor.Blue = intB;
            pRgbColor.Transparency = (byte)intT;
            return pRgbColor;
        }

        //����Map2����굥������
        private void axMapControl2_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            if (axMapControl2.Map.LayerCount > 0)
            {
                //�����������ƶ����ο�  
                if (e.button == 1)
                {
                    //���ָ������ӥ�۵ľ��ο��У���ǿ��ƶ�  
                    if (e.mapX > pEnv.XMin && e.mapY > pEnv.YMin && e.mapX < pEnv.XMax && e.mapY < pEnv.YMax)
                    {
                        bCanDrag = !bCanDrag;
                    }
                    pMoveRectPoint = new PointClass();
                    pMoveRectPoint.PutCoords(e.mapX, e.mapY);  //��¼����ĵ�һ���������  
                }
            }
        }

        //����Map2������ƶ�����
        private void axMapControl2_OnMouseMove(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (e.mapX > pEnv.XMin && e.mapY > pEnv.YMin && e.mapX < pEnv.XMax && e.mapY < pEnv.YMax)
            {
                axMapControl2.MousePointer = esriControlsMousePointer.esriPointerHand;
            }
            else
            {
                //������λ�ý������ΪĬ�ϵ���ʽ  
                axMapControl2.MousePointer = esriControlsMousePointer.esriPointerDefault;
            }

            if (bCanDrag)
            {
                double Dx, Dy;  //��¼����ƶ��ľ���  
                Dx = e.mapX - pMoveRectPoint.X;
                Dy = e.mapY - pMoveRectPoint.Y;
                pEnv.Offset(Dx, Dy); //����ƫ�������� pEnv λ��  
                pMoveRectPoint.PutCoords(e.mapX, e.mapY);
                DrawRectangle(pEnv);
                axMapControl1.Extent = pEnv;
            }
        }  
    }
}