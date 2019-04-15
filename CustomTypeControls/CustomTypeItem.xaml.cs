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

namespace CustomTypeControl
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class CustomTypeItem : UserControl
    {

        public Action<string> ClickHandel;

        public string ListNum
        {
            get
            {
                return txtList.Content.ToString();
            }
            set
            {
                txtList.Content = value;
            }
        }

        public CustomTypeItem()
        {
            InitializeComponent();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ClickHandel(ListNum);
        }
    }
}
