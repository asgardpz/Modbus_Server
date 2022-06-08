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
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace CMS
{
    /// <summary>
    /// RowItem.xaml 的互動邏輯
    /// </summary>
    public partial class RowItem : UserControl
    {
        public string IP;
        public int Port;
        private delegate void delUpdateUI(string sMessage);
        public TcpListener m_server;
        public Thread m_thrListening; // 持續監聽是否有Client連線及收值的執行緒
        private TcpClient m_client;
        public string id;
        public string Step;
        public StreamWriter sw;

        public RowItem(string open,string ip, int port)
        {
            InitializeComponent();
            Loaded += UserControl1_Loaded;
            id = open;
            IP = ip;
            Port = port;
            //開啟就Open
            try
            {
                int nPort = Convert.ToInt32(port); // 設定 Port
                IPAddress localAddr = IPAddress.Parse(ip); // 設定 IP

                // Create TcpListener 並開始監聽
                m_server = new TcpListener(localAddr, nPort);
                m_server.Start();
                m_thrListening = new Thread(Listening);
                m_thrListening.Start();
                Step = "";
                Log_file("Start");
            }
            catch
            {

            }
        }

        public void Connect(string sStatus)
        {
            if (!Dispatcher.CheckAccess())
            {
                delUpdateUI del = new delUpdateUI(Connect);
                this.Dispatcher.Invoke(del, sStatus);
            }
            else
            {
                try
                {
                    // Create Tcp client.
                    m_client = new TcpClient(IP, Port + 1);
                    if (m_client != null)
                    {

                    }
                }
                catch
                {

                }
            }
        }

        public void Disconn()
        {
            if (m_server != null)
            {
                m_server.Stop();
            }
            if (m_thrListening != null)
            {
                m_thrListening.Abort();
            }
            if (m_client != null)
            {
                byte[] btData = System.Text.Encoding.UTF8.GetBytes("Close");
                NetworkStream stream = m_client.GetStream();
                stream.Write(btData, 0, btData.Length); // Write data to server.
                m_client.Close();
                m_client = null;
            }
            Log_file("End");
        }

        private void Listening()
        {
            try
            {
                byte[] btDatas = new byte[256]; // Receive data buffer
                string sData = null;

                while (true)
                {
                    UpdateStatus("Waiting for connection...");
                    TcpClient client = m_server.AcceptTcpClient(); // 要等有Client建立連線後才會繼續往下執行
                    UpdateStatus("Connect to client!");
                    sData = null;
                    NetworkStream stream = client.GetStream();
                    int i;
                    try
                    {
                        while ((i = stream.Read(btDatas, 0, btDatas.Length)) != 0) // 當有資料傳入時將資料顯示至介面上
                        {
                            //sData = System.Text.Encoding.ASCII.GetString(btDatas, 0, i);
                            sData = System.Text.Encoding.UTF8.GetString(btDatas, 0, i);
                            UdpateMessage(sData);
                            Thread.Sleep(1);
                            UdpateMessage2("UNDO");
                        }
                        client.Close();
                        Thread.Sleep(1);
                    }
                    catch
                    {
                        //UpdateStatus("Waiting for connection...");
                    }
                }
            }
            catch
            {

            }
        }

        private void UpdateStatus(string sStatus)
        {
            if (!Dispatcher.CheckAccess())
            {
                delUpdateUI del = new delUpdateUI(UpdateStatus);
                this.Dispatcher.Invoke(del, sStatus);
            }
            else
            {
                if ( sStatus == "Waiting for connection..." )
                {
                    LB_Conn.Content = "NG";
                    LB_Conn.Background = Brushes.Red;

                    BitmapImage bitmap = new BitmapImage(new Uri(@"Image/Error.png", UriKind.Relative));
                    Ima_Conn.BeginInit();
                    Ima_Conn.Source = bitmap;
                    Ima_Conn.EndInit();
                }
                else
                {
                    LB_Conn.Content = "OK";
                    LB_Conn.Background = Brushes.Green;

                    BitmapImage bitmap = new BitmapImage(new Uri(@"Image/On.png", UriKind.Relative));
                    Ima_Conn.BeginInit();
                    Ima_Conn.Source = bitmap;
                    Ima_Conn.EndInit();
                }
            }
        }

        private void UdpateMessage(string sReceiveData)
        {
            if (!Dispatcher.CheckAccess())
            {
                delUpdateUI del = new delUpdateUI(UdpateMessage);
                this.Dispatcher.Invoke(del, sReceiveData);
            }
            else
            {
                TB_InputData.Text = sReceiveData + Environment.NewLine;
                //判斷指令
                switch (Step)
                {
                    case "":
                        if (sReceiveData == "UNDO")
                        {
                            TB_OutputData.Text = "UNDO OK                 Cmd: 員工代碼 ?";
                            Step = "UNDO";
                        }
                        break;

                    case "UNDO":
                        TB_OutputData.Text = "Msg: 員工代碼 OK        Cmd: TEST_TYPE ?";
                        Step = "Type";
                        break;

                    case "Type":
                        TB_OutputData.Text = "Msg: TEST_TYPE OK       Cmd: 序號 ?";
                        Step = "SN";
                        break;

                    case "SN":
                        if (sReceiveData == "END")
                        {
                            TB_OutputData.Text = "END OK                 Cmd: UNDO ?";
                            Step = "";
                        }
                        else
                        {
                            TB_OutputData.Text = "Msg: 序號 OK            Cmd: 序號 ?";
                            Step = "SN";
                        }
                        break;
                }
                Log_file(TB_OutputData.Text.ToString());
            }
        }

        private void UdpateMessage2(string sReceiveData)
        {
            if (!Dispatcher.CheckAccess())
            {
                delUpdateUI del = new delUpdateUI(UdpateMessage2);
                this.Dispatcher.Invoke(del, sReceiveData);
            }
            else
            {

                byte[] btData = System.Text.Encoding.UTF8.GetBytes(TB_OutputData.Text); // Convert string to byte array.
                if (m_client == null)
                {
                    try
                    {
                        Connect("UNDO");
                    }
                    catch
                    {

                    }
                }
                try
                {
                    if (m_client != null)
                    {
                        NetworkStream stream = m_client.GetStream();
                        stream.Write(btData, 0, btData.Length); // Write data to server.
                    }

                }
                catch
                {

                }
            }
        }

        public void Log_file(string message)
        {
            try
            {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Log\\" + DateTime.Now.ToString("yyyyMMdd") + "\\"))
                {

                }
                else
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Log\\" + DateTime.Now.ToString("yyyyMMdd") + "\\");
                }
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + id + ".txt", true);
                sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + message);
                sw.Close();
            }
            catch
            {

            }
        }

        void UserControl1_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Closing += Window_Closing;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
             e.Cancel = true;
        }

        private void UserItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           Log newWindow = new Log();
           newWindow.ShowDialog();
        }

    }
}
