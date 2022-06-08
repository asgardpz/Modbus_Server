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
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace CMS
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {

        //定義一個集合，存儲客戶端信息
        public static Dictionary<string, RowItem> clientConnectionItems = new Dictionary<string, RowItem> { };

        public MainWindow()
        {
            InitializeComponent();
        }

        //強制關閉
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var node in clientConnectionItems)
            {
                node.Value.Disconn();
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    clientConnectionItems.Remove(i.ToString());
                }
            }
            container.Children.Clear();
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    if (i == 0 )
                    {
                        RowItem rowItem = new RowItem(i.ToString(),"127.0.0.1", 80);
                        Canvas.SetLeft(rowItem, i * 100);
                        Canvas.SetTop(rowItem, j * 150);
                        container.Children.Add(rowItem);
                        clientConnectionItems.Add(i.ToString(), rowItem);
                    }
                    else
                    {
                        RowItem rowItem = new RowItem(i.ToString(),"127.0.0.1",i*100);
                        Canvas.SetLeft(rowItem, i * 100);
                        Canvas.SetTop(rowItem, j * 150);
                        container.Children.Add(rowItem);
                        clientConnectionItems.Add(i.ToString(), rowItem);
                    }
                    //clientConnectionItems.Remove(i.ToString());
                }
            }
            //container.Children.Clear();
        }

        private void Btn_End_Click(object sender, RoutedEventArgs e)
        {
            foreach (var node in clientConnectionItems)
            {
                node.Value.Disconn();
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    clientConnectionItems.Remove(i.ToString());
                }
            }
            container.Children.Clear();
        }
    }
}
