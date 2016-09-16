using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Vector.Role.Interface;
using Vector.Role;
using Library.Assist;


namespace MQGhost
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadWindow();
        }

        const string INIFile = "MQGhost.ini";
        const string IniDivide = "=";
        const string WindowTitle = "MQGhost Version 1.8";
        const string SeveralSpaces = "        ";
        ListeningState listeningState = ListeningState.Stopped;
        IProducer producer;
        IConsumer consumer;
        IList<IProducer> producers = new List<IProducer>();
        static System.Diagnostics.Stopwatch watchReceive = new System.Diagnostics.Stopwatch();
        static System.Diagnostics.Stopwatch watchSend = new System.Diagnostics.Stopwatch();
        static System.Diagnostics.Stopwatch watchPop = new System.Diagnostics.Stopwatch();

        private void LoadWindow()
        {
            InitialUI();
            LoadSetting();
        }

        private void InitialUI()
        {
            Title = WindowTitle;
            System.Collections.ObjectModel.ObservableCollection<string> MQTypes = new System.Collections.ObjectModel.ObservableCollection<string>(Enum.GetNames(typeof(MQTypeEnum)));
            MsgType_CB.ItemsSource = MQTypes;
            MsgType_CB.SelectedIndex = 0;
            System.Collections.ObjectModel.ObservableCollection<string> MSMQFormats = new System.Collections.ObjectModel.ObservableCollection<string>(Enum.GetNames(typeof(MSMQFormatEnum)));
            MSMQFormat_CB.ItemsSource = MSMQFormats;
            MSMQFormat_CB.SelectedIndex = 0;

            List<Control> boxs = new List<Control>();
            boxs.Add(UserName_TB);
            boxs.Add(Password_PB);
            boxs.Add(RevIP_TB);
            boxs.Add(RevPort_TB);
            boxs.Add(ReceiveQueue_TB);
            boxs.Add(SendIP_TB);
            boxs.Add(SendPort_TB);
            boxs.Add(SendLabel_TB);
            boxs.Add(SendQueue_TB);
            boxs.Add(SendAmount_TB);
            boxs.Add(ConfirmPublishTimeout_TB);
            bindSelectAll(boxs);

            LaunchTimer();
        }

        private void bindSelectAll(IList<Control> boxs)
        {
            foreach (Control box in boxs)
            {
                box.PreviewMouseDown += Box_PreviewMouseDown;
                box.LostFocus += Box_LostFocus;
                box.GotFocus += Box_GotFocus;
            }
        }

        private void SaveSetting()
        {
            INISetting iniSetting = new INISetting(INIFile);
            List<string> lines = new List<string>();
            lines.Add("MsgType_CB=" + MsgType_CB.SelectedItem);
            lines.Add("UserName_TB=" + UserName_TB.Text);
            lines.Add("Password_PB=" + Password_PB.Password);
            lines.Add("MSMQFormat_CB=" + MSMQFormat_CB.SelectedItem);
            lines.Add("Persistent_ChB=" + Check2NO(Persistent_ChB.IsChecked));

            lines.Add("RevIP_TB=" + RevIP_TB.Text);
            lines.Add("RevPort_TB=" + RevPort_TB.Text);
            lines.Add("RevLabel_TB=" + RevLabel_TB.Text);
            lines.Add("ReceiveQueue_TB=" + ReceiveQueue_TB.Text);
            lines.Add("HideMessage_ChB=" + Check2NO(HideMessage_ChB.IsChecked));

            lines.Add("SendIP_TB=" + SendIP_TB.Text);
            lines.Add("SendPort_TB=" + SendPort_TB.Text);
            lines.Add("SendLabel_TB=" + SendLabel_TB.Text);
            lines.Add("SendQueue_TB=" + SendQueue_TB.Text);
            lines.Add("ConfirmPublishTimeout_TB=" + ConfirmPublishTimeout_TB.Text);
            lines.Add("Sending_TB=" + Sending_TB.Text);
            lines.Add("CreateQueue_ChB=" + Check2NO(CreateQueue_ChB.IsChecked));
            lines.Add("RequiredReply_ChB=" + Check2NO(RequiredReply_ChB.IsChecked));

            iniSetting.WriteLines(lines, false);
        }

        public static bool NO2Check(string persistNO)
        {
            bool b = persistNO == "1" ? true : false;
            return b;
        }
        public static int Check2NO(bool? b)
        {
            int NO = b == true ? 1 : 0;
            return NO;
        }

        private void LoadSetting()
        {
            if (File.Exists(INIFile))
            {
                INISetting iniSetting = new INISetting(INIFile);
                MsgType_CB.SelectedItem = iniSetting.Get("MsgType_CB", IniDivide);
                UserName_TB.Text = iniSetting.Get("UserName_TB", IniDivide);
                Password_PB.Password = iniSetting.Get("Password_PB", IniDivide);
                MSMQFormat_CB.SelectedItem = iniSetting.Get("MSMQFormat_CB", IniDivide);
                Persistent_ChB.IsChecked = NO2Check(iniSetting.Get("Persistent_ChB", IniDivide));

                RevIP_TB.Text = iniSetting.Get("RevIP_TB", IniDivide);
                RevPort_TB.Text = iniSetting.Get("RevPort_TB", IniDivide);
                RevLabel_TB.Text = iniSetting.Get("RevLabel_TB", IniDivide);
                ReceiveQueue_TB.Text = iniSetting.Get("ReceiveQueue_TB", IniDivide);
                HideMessage_ChB.IsChecked = NO2Check(iniSetting.Get("HideMessage_ChB", IniDivide));

                SendIP_TB.Text = iniSetting.Get("SendIP_TB", IniDivide);
                SendPort_TB.Text = iniSetting.Get("SendPort_TB", IniDivide);
                SendLabel_TB.Text = iniSetting.Get("SendLabel_TB", IniDivide);
                SendQueue_TB.Text = iniSetting.Get("SendQueue_TB", IniDivide);
                ConfirmPublishTimeout_TB.Text = iniSetting.Get("ConfirmPublishTimeout_TB", IniDivide);                
                Sending_TB.Text = iniSetting.Get("Sending_TB", IniDivide);
                CreateQueue_ChB.IsChecked = NO2Check(iniSetting.Get("CreateQueue_ChB", IniDivide));
                RequiredReply_ChB.IsChecked = NO2Check(iniSetting.Get("RequiredReply_ChB", IniDivide));
            }
        }



        private void UpdateTime()
        {
            //this.Title = WindowTitle + SeveralSpaces + DateTime.Now.ToLongTimeString();
        }

        private void Listening_Btn_Click(object sender, RoutedEventArgs e)
        {
            UpdateTime();
            try
            {
                if (listeningState == ListeningState.Stopped)
                {
                    QConfig qConfig = wrapQConfig();
                    qConfig.PersistentMode = (QConfig.PersistentSet)Check2NO(Persistent_ChB.IsChecked);
                    qConfig.RequiredReply = RequiredReply_ChB.IsChecked == true;
                    qConfig.Callback = ReceiveCallback;

                    consumer = MQFactory.GenerateConsumer(qConfig);
                    watchReceive.Restart();
                    consumer.Listen();
                    Listening_Btn.Content = ListeningState.Stopped.ToString() + Words.Listening;
                    listeningState = ListeningState.Started;
                }
                else if (listeningState == ListeningState.Started)
                {
                    watchReceive.Reset();
                    DisposeConsumer();
                }
                SuccessInfo();
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(new ShowMsgDel(showPrompt), Words.Fail + ex.Message);
            }
        }

        private void Pop_Btn_Click(object sender, RoutedEventArgs e)
        {
            UpdateTime();
            DisposeConsumer();
            QConfig qConfig = wrapQConfig();
            try
            {
                qConfig.PersistentMode = (QConfig.PersistentSet)Check2NO(Persistent_ChB.IsChecked);
                qConfig.RequiredReply = RequiredReply_ChB.IsChecked == true;
                qConfig.Callback = PopCallback;

                consumer = MQFactory.GenerateConsumer(qConfig);
                watchPop.Restart();
                consumer.Pop();
                SuccessInfo();
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(new ShowMsgDel(showPrompt), Words.Fail + ex.Message);
            }
        }

        private void DisposeConsumer()
        {
            if (consumer != null)
            {
                consumer.Dispose();
                Listening_Btn.Content = ListeningState.Started.ToString() + Words.Listening;
                listeningState = ListeningState.Stopped;
            }

        }

        private void DisposeProducer()
        {
            if (producer != null)
            {
                producer.Dispose();
            }
        }

        private void DisposeAllBackgroundProducer()
        {
            Parallel.ForEach(producers, prd => { prd.Dispose(); });
        }

        private void Dispose()
        {
            DisposeConsumer();
            DisposeProducer();
            DisposeAllBackgroundProducer();
        }

        private void PopCallback(string msg)
        {
            this.Dispatcher.Invoke(new ShowMsgDel(showMsg), msg);
            if (watchPop.IsRunning)
            {
                watchPop.Stop();
                long recordMilliseconds = watchReceive.ElapsedMilliseconds;
                SuccessInfo(recordMilliseconds);
            }
        }


        private void ReceiveCallback(string msg)
        {
            this.Dispatcher.Invoke(new ShowMsgDel(showMsg), msg);
            if (watchReceive.IsRunning)
            {
                long recordMilliseconds = watchReceive.ElapsedMilliseconds;
                SuccessInfo(recordMilliseconds);
            }
        }

        delegate void ShowMsgDel(string msg);
        void showMsg(string msg)
        {
            if (HideMessage_ChB.IsChecked == false)
            {
                string SeparatorBar = string.Join("", System.Linq.Enumerable.Repeat("-", (int)(Listening_TB.ViewportWidth / 5.5)).ToArray());

                if (string.IsNullOrWhiteSpace(Listening_TB.Text))
                    Listening_TB.Text = msg;
                else
                    Listening_TB.Text += Environment.NewLine + SeparatorBar + Environment.NewLine + msg;
                Listening_TB.ScrollToEnd();
            }
        }

        private void showPrompt(string msg)
        {
            Prompt_TB.Text = DateTime.Now.ToLongTimeString() + SeveralSpaces + msg;
        }

        private void showReply(string msg)
        {
            Reply_TB.Text = DateTime.Now.ToLongTimeString() + SeveralSpaces + msg;
        }

        private QConfig wrapQConfig()
        {
            QConfig qConfig = new QConfig();
            qConfig.MQType = (MQTypeEnum)Enum.Parse(typeof(MQTypeEnum), MsgType_CB.SelectedItem.ToString());
            qConfig.MSMQFormat = (MSMQFormatEnum)Enum.Parse(typeof(MSMQFormatEnum), MSMQFormat_CB.SelectedItem.ToString());
            qConfig.UserName = UserName_TB.Text;
            qConfig.Password = Password_PB.Password;
            qConfig.IP = SendIP_TB.Text;
            int _port;
            int.TryParse(SendPort_TB.Text, out _port);
            qConfig.Port = _port;
            qConfig.Label = SendLabel_TB.Text;
            qConfig.QueueName = SendQueue_TB.Text;
            return qConfig;
        }

        private void Sending_Btn_Click(object sender, RoutedEventArgs e)
        {
            //DisposeProducer();
            if (producer != null)
                producers.Add(producer);
            UpdateTime();
            QConfig qConfig = wrapQConfig();
            try
            {
                qConfig.PersistentMode = (QConfig.PersistentSet)Check2NO(Persistent_ChB.IsChecked);
                qConfig.CreateNewQueue = CreateQueue_ChB.IsChecked == true;
                qConfig.RequiredReply = RequiredReply_ChB.IsChecked == true;
                qConfig.Callback = SendCallbackInfo;
                qConfig.ConfirmPublishTimeout = long.Parse(ConfirmPublishTimeout_TB.Text);
                producer = MQFactory.GenerateProducer(qConfig);

                long amount = Convert.ToInt64(SendAmount_TB.Text);
                string content = Sending_TB.Text;
                qConfig.Amount = amount;
                long recordMilliseconds = Timing(() =>
                {
                    producer.SendTextMsg(content);
                });
                SuccessInfo(recordMilliseconds);
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(new ShowMsgDel(showPrompt), Words.Fail + ex.Message);
            }
        }

        public static long Timing(Action action)
        {
            watchSend.Restart();
            action();
            watchSend.Stop();
            long recordMilliseconds = watchSend.ElapsedMilliseconds;
            return recordMilliseconds;
        }

        private void Tap_Btn_Click(object sender, RoutedEventArgs e)
        {
            UpdateTime();
            QConfig qConfig = wrapQConfig();
            /////////
            qConfig.Callback = ReceiveCallback;
            IConsumer consumer_Tap = MQFactory.GenerateConsumer(qConfig);
            try
            {
                consumer_Tap.Tap();
                TapSuccessInfo(RevIP_TB.Text + ":" + RevPort_TB.Text);
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(new ShowMsgDel(showPrompt), Words.Fail + ex.Message);
            }
        }

        enum ListeningState
        {
            Started = 0,
            Stopped = 1
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSetting();
            Dispose();
        }

        private void Box_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox box = ((TextBox)sender);
                box.SelectAll();
                box.PreviewMouseDown -= new MouseButtonEventHandler(Box_PreviewMouseDown);
            }
            else if (sender is PasswordBox)
            {
                PasswordBox box = ((PasswordBox)sender);
                box.SelectAll();
                box.PreviewMouseDown -= new MouseButtonEventHandler(Box_PreviewMouseDown);
            }
        }

        private void Box_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox box = ((TextBox)sender);
                box.Focus();
                e.Handled = true;
            }
            else if (sender is PasswordBox)
            {
                PasswordBox box = ((PasswordBox)sender);
                box.Focus();
                e.Handled = true;
            }
        }

        private void Box_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox box = ((TextBox)sender);
                box.PreviewMouseDown += new MouseButtonEventHandler(Box_PreviewMouseDown);
            }
            else if (sender is PasswordBox)
            {
                PasswordBox box = ((PasswordBox)sender);
                box.PreviewMouseDown += new MouseButtonEventHandler(Box_PreviewMouseDown);
            }
        }

        private void ClearInfo()
        {
            this.Dispatcher.Invoke(new ShowMsgDel(showPrompt), string.Empty);
        }
        private void SuccessInfo()
        {
            this.Dispatcher.Invoke(new ShowMsgDel(showPrompt), Words.Success);
        }
        private void SuccessInfo(long ElapsedTime)
        {
            this.Dispatcher.Invoke(new ShowMsgDel(showPrompt), string.Format(Words.Success + Words.ElapsedTime, ElapsedTime));
        }

        private void SendCallbackInfo(string obj)
        {
            this.Dispatcher.Invoke(new ShowMsgDel(showReply), string.Format(Words.ReplyInfo, obj));
        }

        private void TapSuccessInfo(string obj)
        {
            this.Dispatcher.Invoke(new ShowMsgDel(showPrompt), string.Format(Words.TapSuccess, obj));
        }


        private void TimerTick(object sender, EventArgs e)
        {
            Clock_TB.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        private void LaunchTimer()
        {
            DispatcherTimer innerTimer = new DispatcherTimer(TimeSpan.FromSeconds(1.0),
                    DispatcherPriority.Loaded, new EventHandler(this.TimerTick), this.Dispatcher);
            innerTimer.Start();
        }
    }


}
