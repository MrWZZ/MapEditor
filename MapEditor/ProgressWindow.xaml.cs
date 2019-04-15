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
    /// ProgressWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public Action OnClickHandle;

        public ProgressWindow(int maxPro)
        {
            InitializeComponent();
            proProgress.Maximum = maxPro;
        }

        public void ChangePro(int curPro)
        {   
            proProgress.Value = curPro + 1;
            txtPro.Content = $"{curPro}/{proProgress.Maximum}";
        }

        private void btnCanel_Click(object sender, RoutedEventArgs e)
        {
            OnClickHandle();
            Close();
        }
    }
}
