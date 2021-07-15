using IWshRuntimeLibrary;
using Newtonsoft.Json.Linq;
using Notifications.Wpf;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using PoniLCU;
using System.Drawing;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Brushes = System.Windows.Media.Brushes;
using System.Diagnostics;
using System.Security.Principal;

namespace LickMyRunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static LeagueClient LeagueClient = new LeagueClient();
        public string path = System.Windows.Forms.Application.StartupPath + "/Config.Ponita";
        public static NotificationManager notificationManager = new NotificationManager();
        public System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        private System.Windows.Forms.NotifyIcon icon = new System.Windows.Forms.NotifyIcon();
        #region SystemSettings
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            this.Top = 0; this.Left = 0; this.Topmost = true;
            lblTitle.Content = "Ponita0 - LickMyRunes " + utils.Version;
            ni.Icon = Properties.Resources.main;
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            ni.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            var bmp = new System.Drawing.Bitmap(LickMyRunes.Properties.Resources.restart_icon_32261);
            var Qmark = new System.Drawing.Bitmap(LickMyRunes.Properties.Resources.red_question_mark_png_11552242990hpigioc6g8);
            ni.ContextMenuStrip.Items.Add("LickMyRunes " + utils.Version).Enabled = false;
            ni.ContextMenuStrip.Items.Add("Show Application", null, onShowClicked);
            ni.ContextMenuStrip.Items.Add("Show Tutorial", Qmark, onTutorialClicked);
            ni.ContextMenuStrip.Items.Add("Restart", bmp, onRestartClicked);
            ni.ContextMenuStrip.Items.Add("Add to start menu", null, onAddShortcutClicked);
            ni.ContextMenuStrip.Items.Add("Add shortcut to desktop", null, onAddShortcutClickedDesktop);
            if (!System.IO.File.Exists(path))
            {
                using (FileStream fs = System.IO.File.Create(System.Windows.Forms.Application.StartupPath + "/Config.Ponita"))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes("firsttime");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
            }
            else
            {
                return;
            }

        }

        private void onAddShortcutClickedDesktop(object sender, EventArgs e)
        {
            AddShortcut("desktop");
        }
        private void SetShortcut(string location)
        {
            if (!WindowsSecurity.isAdminStrator())
            {
                var resault = MessageBox.Show("You need to run the app as an administrator , to proceed click yes", "Run as administrator", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (resault == MessageBoxResult.Yes)
                {
                    var currentProcessInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = System.Reflection.Assembly.GetEntryAssembly().Location,
                        Verb = "runas"
                    };
                    Process.Start(currentProcessInfo);

                    Environment.Exit(0);
                }
                else { return; }



            }
            else
            {
                AddShortcut(location);
            }
        }
        private void onAddShortcutClicked(object sender, EventArgs e)
        {
            SetShortcut("anyStringIDontCare");
        }

        private static void AddShortcut(string target)
        {
            string pathToExe = System.Windows.Forms.Application.ExecutablePath;
            string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            string appStartMenuPath;

            if (target == "desktop")
            {
                appStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else
            {
                appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs", "Ponita0");
            }
            if (!Directory.Exists(appStartMenuPath))
                Directory.CreateDirectory(appStartMenuPath);
            string shortcutLocation = Path.Combine(appStartMenuPath, "LickMyRunes" + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "League of legends runes maker ";
            shortcut.TargetPath = pathToExe;
            shortcut.Save();
            MessageBox.Show("Shortcut added");
        }
        private void onShowClicked(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void onTutorialClicked(object sender, EventArgs e)
        {
            MessageBox.Show("its simple ! \n" +
                "1- Just go into champ select \n" +
                "2- pick your champion \n" +
                "3- click push runes \n" +
                "simple as that !");
        }

        private void onRestartClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
      
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // check for updates
            await utils.CheckForUpdates();
            // subscribe to gameflow phase Event
            LeagueClient.Subscribe("/lol-gameflow/v1/gameflow-phase", GameFlowPhase);
            if (LeagueClient.IsConnected)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    lblStatus.Content = "Connected to league of legends";
                    System.Windows.Media.Brush myBrush = new SolidColorBrush(Color.FromRgb(39, 174, 96)); lblStatus.Foreground = myBrush;

                });
            }

            LeagueClient.OnConnected += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                { lblStatus.Content = "Connected to league of legends"; System.Windows.Media.Brush myBrush = new SolidColorBrush(Color.FromRgb(39, 174, 96)); lblStatus.Foreground = myBrush; });
            };
            LeagueClient.OnDisconnected += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    lblStatus.Content = "Disconnected"; System.Windows.Media.Brush myBrush = new SolidColorBrush(Color.FromRgb(192, 57, 43)); lblStatus.Foreground = myBrush;
                });
            };
            if (LeagueClient.IsConnected)
            {
                var CurrentPhase = LeagueClient.Request("get", "/lol-gameflow/v1/gameflow-phase", null).Result.Content.ReadAsStringAsync().Result;
                if (CurrentPhase == "\"ChampSelect\"")
                {
                    LeagueClient.Subscribe("/lol-champ-select/v1/session", champSelectSession);
                    var GameFlowSession = LeagueClient.Request("get", "/lol-gameflow/v1/session", null).Result.Content.ReadAsStringAsync().Result;
                    var myData = JObject.Parse(GameFlowSession);
                    this.GameMode = myData["map"]["gameMode"].ToString();
                }
            }

        }
        public void GameFlowPhase(OnWebsocketEventArgs obj)
        {
            if (obj.Data == null)
            {
                return;
            }
            switch (obj.Data)
            {
                case "ChampSelect":
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        lblCurrentPhase.Content = "Champion select";
                    });
                    LeagueClient.Subscribe("/lol-champ-select/v1/session", champSelectSession);
                    var GameFlowSession = LeagueClient.Request("get", "/lol-gameflow/v1/session", null).Result.Content.ReadAsStringAsync().Result;
                    var myData = JObject.Parse(GameFlowSession);
                    this.GameMode = myData["map"]["gameMode"].ToString();
                    break;
                case "Lobby":
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        lblCurrentPhase.Content = "Lobby";
                        btnPush.IsEnabled = false;

                    });
                    LeagueClient.Unsubscribe("/lol-champ-select/v1/session", champSelectSession);
                    break;
                case "None":
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        lblCurrentPhase.Content = "None"; btnPush.IsEnabled = false;

                    });
                    break;
                case "GameStart":
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        btnPush.IsEnabled = false;

                    });
                    break;              
                default:
                    break;
            }

        }
        public string GameMode;
        private void champSelectSession(OnWebsocketEventArgs obj)
        {
            if (obj.Data != null && obj.Type != "Delete")
            {
               var myData = JObject.Parse(obj.Data.ToString());
                //if (obj.Type=="Create" && myData["timer"]["phase"].ToString()=="FINALIZATION")
                //{
                //    ChampionPosition[1] = "ARAM";
                //    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                //        btnPush.IsEnabled = true;
                //    });
                //}
                if (GameMode == "ARAM")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        btnPush.IsEnabled = true;
                        ChampionPosition[0] = LeagueClient.Request("get", "/lol-champ-select/v1/current-champion", null).Result.Content.ReadAsStringAsync().Result;
                        ChampionPosition[1] = "ARAM";
                    });

                }

               int playerCellId = int.Parse(myData["localPlayerCellId"].ToString());
                string gameMode = this.GameMode;
                int lastChamp = -1;               
                foreach (var action in myData["actions"])
                {
                    foreach (var actionItem in action)
                    {
                        if (int.Parse(actionItem["actorCellId"].ToString()) == playerCellId)
                        {
                           
                            if (actionItem["type"] == "pick" && actionItem["completed"] == true)
                            {
                                foreach (var teamPlayer in myData["myTeam"])
                                {
                                    if (teamPlayer["cellId"] == playerCellId)
                                    {
                                        string pos = teamPlayer["assignedPosition"];
                                        int champ = teamPlayer["championId"];
                                        ChampionPosition[0] = champ.ToString(); ChampionPosition[1] = pos;
                                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            if (int.Parse((string)actionItem["championId"]) != 0)
                                            {
                                                var ChampID = int.Parse((string)actionItem["championId"]);
                                                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                                                {
                                                    Image finalImage = new Image();
                                                    BitmapImage logo = new BitmapImage();
                                                    logo.BeginInit();
                                                    logo.UriSource = new Uri("https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/champion-icons/" + ChampID + ".png");
                                                    logo.EndInit();
                                                    finalImage.Source = logo;
                                                    imgChampicon.Source = finalImage.Source;
                                                    ChampionPosition[0] = ChampID.ToString();
                                                    lastChamp = ChampID;

                                                });
                                                System.Threading.Thread.Sleep(200);

                                            }
                                            btnPush.IsEnabled = true;
                                        });
                                    }
                                }

                            }
                        }
                    }

                }

            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                ReleaseCapture();
                SendMessage(new WindowInteropHelper(this).Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        string[] ChampionPosition = new string[2];
        private async void GetAndPushRunes(string lane, int ChampionId)
        {
            if (lane == "Utility")
            {
                lane = "support";
            }
            try
            {
                if (lane.Contains("ARAM"))
                {
                    var aram = await utils.GetRunePage(ChampionId, lane, true);
                    utils.pushRunes(aram.RuneIDs, aram.PrimaryTree, aram.SecondaryTree, "Ponita - " + Enum.GetName(typeof(utils.Champion), ChampionId).ToLower() + " - " + "Aram");
                    notificationManager.Show(new NotificationContent
                    {
                        Title = "Success !",
                        Message = $"Your runes for {Enum.GetName(typeof(utils.Champion), ChampionId)} - Aram Pushed Successfully !",
                        Type = NotificationType.Success
                    });
                    LeagueClient.Unsubscribe("/lol-champ-select/v1/session", champSelectSession);
                    return;

                }
                var runepage = await utils.GetRunePage(ChampionId, lane, false);
                utils.pushRunes(runepage.RuneIDs, runepage.PrimaryTree, runepage.SecondaryTree, "Ponita - " + Enum.GetName(typeof(utils.Champion), ChampionId).ToLower() + " - " + lane);
                notificationManager.Show(new NotificationContent
                {
                    Title = "Success !",
                    Message = $"Your runes for {Enum.GetName(typeof(utils.Champion), ChampionId)} Pushed Successfully !",
                    Type = NotificationType.Success
                });
                System.Threading.Thread.Sleep(500);
                notificationManager.Show(new NotificationContent
                {
                    Title = "Careful !",
                    Message = $"Dont Forget your summoner spells , King ",
                    Type = NotificationType.Warning
                });
                LeagueClient.Unsubscribe("/lol-champ-select/v1/session", champSelectSession);
            }
            catch (Exception ex)
            {

                MessageBox.Show("Something went wrong !\n" +
                    "Please make sure : \n" +
                    "1 - Your client is running and you are logged in \n" +
                    "2 - you clicked \"Lock In\"...  dont try to push runes before lock in \n" +
                    "Or you can restart the app from the system tray\n " + ex.Message, App.LickMyRunesTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                notificationManager.Show(new NotificationContent
                {
                    Title = "You can restart from here",
                    Message = "Right click the icon in the system tray then click restart and there you are ",
                    Type = NotificationType.Information
                });
            }


        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (roleComboBox.Text== "Select lane")
            //{
            //    MessageBox.Show("please Select your lane !", "Error !", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you don't want to LickMyRunes ?", "LickMyRunes", MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    System.Windows.MessageBox.Show("Dont forget to check the rest of my applications \n you can contact me on discord : Ponita0#8322", "LickMyRunes", MessageBoxButton.OK, MessageBoxImage.Information);
                    System.Windows.Application.Current.Shutdown();
                    break;
                case MessageBoxResult.No:
                    return;
            }
        }

        private void githubBTN_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Ponita0");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/hjFD9tYYAk");

        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            ni.ShowBalloonTip(500, "Minimized !", "LickMyRunes now is minimized to system tray", System.Windows.Forms.ToolTipIcon.Info);
            this.WindowState = WindowState.Minimized;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ni.Dispose();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

            var req = LeagueClient.Request("POST", "/lol-login/v1/session/invoke?destination=lcdsServiceProxy&method=call&args=[\"\",\"teambuilder-draft\",\"quitV2\",\"\"]", null).Result.Content.ReadAsStringAsync().Result;
            MessageBox.Show("I hate people who dodge \n \n" +
                "but I love my users <3 \n \n " +
                "So I will let you dodge");
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                btnPush.IsEnabled = false;

            });
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                btnPush.IsEnabled = true;

            });
        }

        private void btnPush_Click(object sender, RoutedEventArgs e)
        {
            if (ChampionPosition[1] != null)
            {
                if (this.GameMode == "ARAM")
                {
                    ChampionPosition[1] = "ARAM";
                }
               // else ChampionPosition[1] = GameMode;

            }
            if (ChampionPosition[0] == null)
            {
                ChampionPosition[0] = LeagueClient.Request("get", "/lol-champ-select/v1/current-champion", null).Result.Content.ReadAsStringAsync().Result;
            }
            GetAndPushRunes(ChampionPosition[1], int.Parse(ChampionPosition[0]));
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            TicketWindow tw = new TicketWindow();
            tw.Show();
        }
    }
}
