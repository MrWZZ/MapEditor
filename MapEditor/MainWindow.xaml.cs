using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinForm = System.Windows.Forms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MapGrid;
using CustomTypeControl;
using Newtonsoft.Json;
using System.Threading;

namespace MapEditor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //网格高宽
        private int _height = 50;
        private int _width = 50;
        //切割图片个数(显示)
        private int _col = 0;
        private int _row = 0;
        private int _totalNum = 0;
        //源图片高宽
        private double _originHeight;
        private double _originWidth;
        //源图片位图
        private BitmapImage _originBitmap;
        //当前网格引用
        private List<MapGridItem> _itemList = new List<MapGridItem>();
        //当前选择到的网格
        private List<MapGridItem> _itemSelectList = new List<MapGridItem>();
        //总配置文件储存
        private Dictionary<string, MapData> _mapConfig = new Dictionary<string, MapData>();
        //当前地图的配置
        private Dictionary<string, MapData> _curConfig = new Dictionary<string, MapData>();

        public MainWindow()
        {
            InitializeComponent();
            SetDefualtConfig();
        }

        private void SetDefualtConfig()
        {
            _width = Manager.userData.gridWidth;
            _height = Manager.userData.gridHeight;
            tbWidth.Text = _width.ToString();
            tbHeight.Text = _height.ToString();
            tbName.Text = Manager.userData.frontName;
            txtTotalConfig.Content = Manager.userData.mapFileName;
            LoadTotalConfig(Manager.userData.mapFileName);
            lbMap.AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(OnClickMapListItem),true);
        }

        //读取图片
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            canvasGrid.Children.Clear();
            //获取路径
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            Uri fileUrl = new Uri(fileName, UriKind.Absolute);
            _originBitmap = new BitmapImage(fileUrl);
            txtSourceHeight.Content = _originBitmap.Height;
            txtSourceWidth.Content = _originBitmap.Width;
            _originHeight = _originBitmap.Height;
            _originWidth = _originBitmap.Width;
            imageMap.RenderSize = new Size(_originBitmap.Width, _originBitmap.Height);
            imageMap.Source = _originBitmap;

            ChangeTotalNum();
            ImageScaleReset();
        }

        //切割图片
        private void SaveImage<T>(string type) 
            where T : BitmapEncoder , new()
        {
            WinForm.FolderBrowserDialog dialog = new WinForm.FolderBrowserDialog();
            if(!string.IsNullOrEmpty(Manager.userData.path) && Directory.Exists(Manager.userData.path))
            {
                dialog.SelectedPath = Manager.userData.path;
            }

            if (dialog.ShowDialog() == WinForm.DialogResult.OK)
            {
                Manager.userData.path = dialog.SelectedPath;
                int front = 0;
                int next = 0;
                for (int i = 0; i < _totalNum; i++)
                {
                    int x = (i % _col) * _width;
                    int y = Convert.ToInt32(i / _col) * _height;
                    CroppedBitmap croppedBitmap = new CroppedBitmap((BitmapImage)imageMap.Source, new Int32Rect(x, y, _width, _height));
                    T encoder = new T();
                    //自定义名字
                    if (next >= _col)
                    {
                        front++;
                        next = 0; 
                    }
                    string curFileName = $"{dialog.SelectedPath}\\{front}_{next}{type}";
                    next++;
                    encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
                    using (FileStream stream = new FileStream(curFileName, FileMode.Create))
                        encoder.Save(stream);
                }
                MessageBox.Show("图片生成完成");
            }
        }

        //保存图片
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            switch (cbImageType.Text)
            {
                case "PNG":
                    SaveImage<PngBitmapEncoder>(".png");
                    break;
                case "JPG":
                    SaveImage<JpegBitmapEncoder>(".jpg");
                    break;
            }
        }

        //生成网格
        private void btnCreateGrid_Click(object sender, RoutedEventArgs e)
        {
            if(imageMap.Source == null)
            {
                MessageBox.Show("未选择图片资源无法生成网格");
                return;
            }
            CreateGrid();
        }

        //生成网格、按列生成
        private void CreateGrid(MapData md = null)
        {
            _itemList.Clear();
            canvasGrid.Children.Clear();
            canvasGrid.SetValue(Canvas.LeftProperty, borderImage.GetValue(Canvas.LeftProperty));
            canvasGrid.SetValue(Canvas.TopProperty, borderImage.GetValue(Canvas.TopProperty));
            //网格适配图片大小
            double curHeight = imageMap.ActualHeight / _originHeight * _height;
            double curWidth = imageMap.ActualWidth / _originWidth * _width;
            //无传入数据
            if(md == null)
            {
                for (int i = 0; i < _totalNum; i++)
                {
                    double x = Convert.ToInt32(i / _row) * curWidth;
                    double y = (i % _row) * curHeight;
                    MapGridItem mg = new MapGridItem(x, y);
                    _itemList.Add(mg);
                    mg.Width = curWidth;
                    mg.Height = curHeight;
                    canvasGrid.Children.Add(mg);
                    mg.SetValue(Canvas.LeftProperty, x);
                    mg.SetValue(Canvas.TopProperty, y);
                }
            }
            //有传入数据
            else
            {
                for (int i = 0; i < _totalNum; i++)
                {
                    double x = Convert.ToInt32(i / _row) * curWidth;
                    double y = (i % _row) * curHeight;
                    MapGridItem mg = new MapGridItem(x, y);
                    mg.Type = md.grids[i].ToString();
                    _itemList.Add(mg);
                    mg.Width = curWidth;
                    mg.Height = curHeight;
                    canvasGrid.Children.Add(mg);
                    mg.SetValue(Canvas.LeftProperty, x);
                    mg.SetValue(Canvas.TopProperty, y);
                }
            }
            
        }

        //网格宽度修改
        private void tbWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_originBitmap == null) return;
            try
            {
                ChangeTotalNum();
            }
            catch (Exception)
            {
            }
        }

        //网格高度修改
        private void tbHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_originBitmap == null) return;
            try
            {
                ChangeTotalNum();
            }
            catch (Exception)
            {
            }
        }

        //计算当前网格数量
        private void ChangeTotalNum()
        {
            _height = int.Parse(tbHeight.Text);
            _width = int.Parse(tbWidth.Text);
            Manager.userData.gridHeight = _height;
            Manager.userData.gridWidth = _width;
            _row = (int)Math.Floor(Convert.ToDecimal(_originHeight / _height));
            _col = (int)Math.Floor(Convert.ToDecimal(_originWidth / _width));
            _totalNum = _row * _col;

            txtRowNum.Content = _row;
            txtColNum.Content = _col;
            txtTotal.Content = _totalNum;
        }

        #region 鼠标框选
        Point startPoint;
        Point endPoint;
        Rectangle rect = null;
        bool isReadyCreateGrid = false;
        private void canvasGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isReadyCreateGrid = true;
            this.startPoint = e.GetPosition(canvasGrid);
            if(this.rect != null)
            {
                canvasGrid.Children.Remove(rect);
            }
            this.rect = null;
        }

        private void canvasGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(!isReadyCreateGrid) { return; }
            this.endPoint = e.GetPosition(canvasGrid);
            canvasGrid.Children.Remove(rect);
            this.rect = null;
            SelectGridItems();
            isReadyCreateGrid = false;
        }

        private void canvasGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if(!isReadyCreateGrid) { return; }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(this.canvasGrid);
                if (p != this.startPoint && rect == null)
                {
                    rect = new Rectangle() { Stroke = Brushes.GreenYellow };
                    this.canvasGrid.Children.Add(rect);
                }

                rect.Width = Math.Abs(p.X - startPoint.X);
                rect.Height = Math.Abs(p.Y - startPoint.Y);
                Canvas.SetLeft(rect, Math.Min(p.X, startPoint.X));
                Canvas.SetTop(rect, Math.Min(p.Y, startPoint.Y));
            }
        }

        //通过鼠标框选选择网格
        private void SelectGridItems()
        {
            //左上角
            double left = Math.Min(startPoint.X, endPoint.X);
            double up = Math.Min(startPoint.Y, endPoint.Y);

            //右下角
            double right = Math.Max(startPoint.X, endPoint.X);
            double down = Math.Max(startPoint.Y, endPoint.Y);

            foreach (var item in _itemList)
            {
                //网格加上宽度位置
                double itemR = item.X + item.Width;
                //玩个加上高度位置
                double itemD = item.Y + item.Height;

                if(item.X >= left && item.Y >= up && itemR <= right && itemD <= down)
                {
                    item.SetSelectEffect(true);
                    _itemSelectList.Add(item);
                }
            }
           
        }
        #endregion

        //设置
        private void CreateCurMapConfig()
        {
            //生成玩个配置
            List<int> gridKeys = new List<int>();
            foreach (var item in _itemList)
            {
                gridKeys.Add(int.Parse(item.Type));
            }

            MapData md = new MapData();
            md.pixHeight = int.Parse(tbPixHeight.Text);
            md.pixWidth = int.Parse(tbPixWidth.Text);
            md.grids = gridKeys.ToArray();
            md.discardList = new int[0];
            md.maxX = int.Parse(tbMaxX.Text);
            md.maxY = int.Parse(tbMaxY.Text);
            md.title_wh = int.Parse(tbTitelwh.Text);
            md.version = int.Parse(tbVersion.Text);

            _curConfig.Clear();
            _curConfig.Add(tbJsonKey.Text, md);
        }

        //生成单个配置文件
        private void btnCreateData_Click(object sender, RoutedEventArgs e)
        {
            CreateCurMapConfig();
            string jsonString = JsonConvert.SerializeObject(_curConfig);
            
            //创建一个保存文件式的对话框
            SaveFileDialog sfd = new SaveFileDialog();
            //获取或设置一个值，该值指示如果用户省略扩展名，文件对话框是否自动在文件名中添加扩展名。（可以不设置）
            sfd.AddExtension = true;
            //保存对话框是否记忆上次打开的目录
            sfd.RestoreDirectory = true;
            //设置保存的文件的类型，注意过滤器的语法
            sfd.Filter = "JSON|*.json|All files|*.*";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
            if (sfd.ShowDialog() == true)
            {
                string filePath = sfd.FileName;
                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    sw.Write(jsonString);
                }
                MessageBox.Show("生成成功");
            }
        }
        
        //取消选择
        private void btnCancelSelect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _itemSelectList)
            {
                item.SetSelectEffect(false);
            }
            _itemSelectList.Clear();
        }

        //设置当前选择的网格的类型
        private void SetCurSelectItemType(string type)
        {
            foreach (var item in _itemSelectList)
            {
                item.Type = type;
            }
        }

        //设置网格类型
        private void btnSetType_Click(object sender, RoutedEventArgs e)
        {
            ChangeTypeWindow ctw = new ChangeTypeWindow();
            ctw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ctw.ClickOkHandel += SetCurSelectItemType;
            ctw.Show();
        }

        //生成图片的默认命名
        private void tbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Manager.userData.frontName = tbName.Text;
        }

        //读取总配置文件
        private void btnSetTotoalConfig_Click(object sender, RoutedEventArgs e)
        {
            WinForm.OpenFileDialog dialog = new WinForm.OpenFileDialog();
            if (!string.IsNullOrEmpty(Manager.userData.mapTotalPath) && Directory.Exists(Manager.userData.mapTotalPath))
            {
                dialog.InitialDirectory = Manager.userData.mapTotalPath;
            }
            if (dialog.ShowDialog() == WinForm.DialogResult.OK)
            {
                //文件夹路径
                Manager.userData.mapTotalPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                //文件绝对路径
                txtTotalConfig.Content = dialog.FileName;
                Manager.userData.mapFileName = dialog.FileName;
                LoadTotalConfig(dialog.FileName);
            }
        }

        //加载总配置
        private void LoadTotalConfig(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
            {
                string file = sr.ReadToEnd();
                _mapConfig = JsonConvert.DeserializeObject<Dictionary<string, MapData>>(file);
            }
            this.ShowMapList();
        }

        //生成总配置文件
        private void btnCreateTotal_Click(object sender, RoutedEventArgs e)
        {
            if(_mapConfig.Count == 0)
            {
                MessageBox.Show("总配置文件为空，无法生成");
                return;
            }

            string jsonString = JsonConvert.SerializeObject(_mapConfig);
            //创建一个保存文件式的对话框
            SaveFileDialog sfd = new SaveFileDialog();
            //获取或设置一个值，该值指示如果用户省略扩展名，文件对话框是否自动在文件名中添加扩展名。（可以不设置）
            sfd.AddExtension = true;
            //保存对话框是否记忆上次打开的目录
            sfd.RestoreDirectory = true;
            //设置保存的文件的类型，注意过滤器的语法
            sfd.Filter = "JSON|*.json|All files|*.*";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
            if (sfd.ShowDialog() == true)
            {
                string filePath = sfd.FileName;
                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    sw.Write(jsonString);
                }
                MessageBox.Show("生成成功");
            }
        }

        //应用当前地图编辑到配置文件
        private void btnUseToTotal_Click(object sender, RoutedEventArgs e)
        {

            if(imageMap.Source == null)
            {
                MessageBox.Show("图片没有加载，无法应用。");
                return;
            }

            CreateCurMapConfig();
            if(_curConfig[tbJsonKey.Text].grids.Length == 0)
            {
                MessageBox.Show("网格数量为0，无法应用。");
                return;
            }

            if (_mapConfig.Count == 0)
            {
                MessageBox.Show("总配置文件未读取，无法应用。");
                return;
            }

            if (_mapConfig.ContainsKey(tbJsonKey.Text))
            {
                _mapConfig[tbJsonKey.Text] = _curConfig[tbJsonKey.Text];
            }
            else
            {
                _mapConfig.Add(tbJsonKey.Text, _curConfig[tbJsonKey.Text]);
            }
            MessageBox.Show("应用成功");
        }

        //显示地图列表
        public void ShowMapList()
        {
            var list = _mapConfig.Keys.ToList();
            list.Sort();
            lbMap.ItemsSource = list;
        }

        //点击总地图列表数据
        private void OnClickMapListItem(object sender, RoutedEventArgs e)
        {
            if (((ListBox)sender).SelectedValue == null) return;

            string value = ((ListBox)sender).SelectedValue.ToString();
            MapData md = _mapConfig[value];
            tbPixHeight.Text = md.pixHeight.ToString();
            tbPixWidth.Text = md.pixWidth.ToString();
            tbMaxX.Text = md.maxX.ToString();
            tbMaxY.Text = md.maxY.ToString();
            tbTitelwh.Text = md.title_wh.ToString();
            tbVersion.Text = md.version.ToString();
            tbJsonKey.Text = value;
        }

        #region 图片拖动、缩放
        private bool isMouseLeftButtonDown = false;
        Point previousMousePoint = new Point(0, 0);

        //重置图片缩放
        private void ImageScaleReset()
        {
            sfr.ScaleX = 1;
            sfr.ScaleY = 1;
            tlt.X = 0;
            tlt.Y = 0;
            //将图片移动到中心
            double centerX = canvasMap.ActualWidth / 2 - imageMap.ActualWidth / 2;
            double centerY = canvasMap.ActualHeight / 2 - imageMap.ActualHeight / 2;
            borderImage.SetValue(Canvas.LeftProperty, centerX);
            borderImage.SetValue(Canvas.TopProperty, centerY);
        }

        private void imageMap_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double val = (double)e.Delta / 2000;   //描述鼠标滑轮滚动
            if (sfr.ScaleX < 0.1 && sfr.ScaleY < 0.1 && e.Delta < 0)
            {
                return;
            }
            //以画布中心进行缩放
            sfr.CenterX = canvasMap.ActualWidth / 2;
            sfr.CenterY = canvasMap.ActualHeight / 2;

            sfr.ScaleX += val;
            sfr.ScaleY += val;
        }

        private void imageMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseLeftButtonDown = true;
            previousMousePoint = e.GetPosition(scrollMap);
        }

        private void imageMap_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseLeftButtonDown = false;
        }

        private void imageMap_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseLeftButtonDown = false;
        }

        private void imageMap_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(scrollMap);
            if (isMouseLeftButtonDown == true)
            {
                tlt.X += position.X - this.previousMousePoint.X;
                tlt.Y += position.Y - this.previousMousePoint.Y;
                previousMousePoint = position;
            }
        }
        #endregion

        //根据指定配置生成网格
        private void btnConfigGrid_Click(object sender, RoutedEventArgs e)
        {
            if(_mapConfig.Count == 0)
            {
                MessageBox.Show("总配置文件未读取");
                return;
            }
            
            if(lbMap.SelectedValue == null)
            {
                MessageBox.Show("未选取指定配置键名称");
                return;
            }
            
            if(imageMap.Source == null)
            {
                MessageBox.Show("图片未加载");
                return;
            }

            MapData md = _mapConfig[lbMap.SelectedValue.ToString()];

            tbWidth.Text = md.title_wh.ToString();
            tbHeight.Text = md.title_wh.ToString();
            _originHeight = md.pixHeight;
            _originWidth = md.pixWidth;
            _row = md.maxY;
            _col = md.maxX;
            _totalNum = _row * _col;

            if(_totalNum != md.grids.Length)
            {
                MessageBox.Show("原配置文件中网格数量异常，停止生成");
                return;
            }
            CreateGrid(md);
        }
    }
}
