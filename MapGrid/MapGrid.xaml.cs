using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapGrid
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class MapGridItem : UserControl
    {
        public bool isSelect = false;
        public double X = 0;
        public double Y = 0;

        //原始大小
        private double _originWidth;
        private double _originHeight;
        private double _originFontSize;

        public string Type
        {
            get
            {
                return (string)txtType.Content;
            }
            set
            {
                txtType.Content = value;
                ChangeColorByType();
            }
        }

        public Color Color
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(gBg.Background.ToString());
            }
            set
            {
                gBg.Background = new SolidColorBrush(value);
            }
        }


        public MapGridItem(double x,double y)
        {
            InitializeComponent();
            _originFontSize = txtType.FontSize;
            _originHeight = Height;
            _originWidth = Width;
            X = x;
            Y = y;
        }

        public void SetSelectEffect(bool flag)
        {
            isSelect = flag;
            if(isSelect)
            {
                borderSelect.BorderBrush = new SolidColorBrush(Colors.PowderBlue);
            }
            else
            {
                borderSelect.BorderBrush = new SolidColorBrush(Colors.Blue);
            }
            
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double offset = Width / _originWidth;
            txtType.FontSize = offset * _originFontSize;
            borderSelect.BorderThickness = new Thickness(offset);
        }

        private void ChangeColorByType()
        {
            switch(Type)
            {
                case "0":
                    Color = Colors.Red;
                    break;
                case "1":
                    Color = Colors.Green;
                    break;
                case "2":
                    Color = Colors.Blue;
                    break;
                case "3":
                    Color = Colors.Yellow;
                    break;
                default:
                    Color = Colors.White;
                    break;
            }
        }
    }
}
