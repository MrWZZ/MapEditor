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
using System.Windows.Shapes;

namespace MapEditor
{
    /// <summary>
    /// ChangeTypeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeTypeWindow : Window
    {
        //点击确定按钮回调
        public Action<string> ClickOkHandel;

        public ChangeTypeWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbType.Text))
            {
                MessageBox.Show("不能是空值");
                return;
            }
            if(!int.TryParse(tbType.Text,out int a))
            {
                MessageBox.Show("当前只支持数字类型");
                return;
            }
            ClickOkHandel(tbType.Text);
            Close();
        }
    }
}
