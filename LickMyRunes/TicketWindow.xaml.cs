using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace LickMyRunes
{
    /// <summary>
    /// Interaction logic for TicketWindow.xaml
    /// </summary>
    public partial class TicketWindow : Window
    {
        public TicketWindow()
        {
            InitializeComponent();
            
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        public static void sendWebHook(string URL, string msg, string username, string senderName)
        {
            //http.Post(URL, new NameValueCollection()
            //{
            //    { "username", username },
            //    { "content", msg }
            //});
            WebRequest wr = (HttpWebRequest)WebRequest.Create(URL);
            wr.ContentType = "application/json";
            wr.Method = "POST";
            using (var sw = new StreamWriter(wr.GetRequestStream()))
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    username = "PoniTest",
                    embeds = new[]
                    {
                        new
                        {
                            description="LickMyRunes : \n"+ msg,
                            title="From : "+senderName,
                            color="786687",
                            timestamp = DateTime.Now,
                        }
                    }
                });
                sw.Write(json);
            }
            var response = (HttpWebResponse)wr.GetResponse();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message =$"``` {txtmessageBody.Text} ```" ;
            sendWebHook("a private webhook url", message, "LickMyRunes Tickets", discordTagtxt.Text);
            MainWindow.notificationManager.Show(new Notifications.Wpf.NotificationContent
            {
                Title = "Success !",
                Message = $"Ticket sent succesfully !",
                Type = Notifications.Wpf.NotificationType.Success
            });

        }
    }
}
