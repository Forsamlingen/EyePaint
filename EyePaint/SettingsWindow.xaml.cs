using FlickrNet;
using System;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace EyePaint
{
    /// <summary>
    /// Displays editable settings to the museum staff.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        OAuthRequestToken request;

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = Properties.Settings.Default;
            ShowDialog();
        }

        void onContentRendered(object s, EventArgs e)
        {
            (FindResource("CountdownAnimation") as Storyboard).Begin();
        }

        void onClose(object s, EventArgs e)
        {
            (FindResource("CountdownAnimation") as Storyboard).Stop();
            Close();
        }

        void onShowSettingsButtonClick(object s, RoutedEventArgs e)
        {
            Countdown.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Visible;
            (FindResource("CountdownAnimation") as Storyboard).Stop();
        }

        void onSaveButtonClick(object s, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            MessageBox.Show("Settings saved.");
            Close();
        }

        void onGetVerificationCodeButtonClick(object s, RoutedEventArgs e)
        {
            var f = new Flickr(Properties.Settings.Default.FlickrKey, Properties.Settings.Default.FlickrSecret);
            request = f.OAuthGetRequestToken("oob");
            var url = f.OAuthCalculateAuthorizationUrl(request.Token, AuthLevel.Write);
            System.Diagnostics.Process.Start(url);
        }

        void onStoreVerificationCodeButtonClick(object s, RoutedEventArgs e)
        {
            if (request == null) MessageBox.Show("Request a new verification code first.");
            else
            {
                try
                {
                    var f = new Flickr(Properties.Settings.Default.FlickrKey, Properties.Settings.Default.FlickrSecret);
                    Properties.Settings.Default.FlickrAccessToken = f.OAuthGetAccessToken(request, FlickrCode.Text);
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Login complete.");
                }
                catch (FlickrApiException)
                {
                    MessageBox.Show("Login failed.");
                }
            }
        }

        void onSendEmailButtonClick(object s, RoutedEventArgs e)
        {
            try
            {
                var m = new MailMessage(
                    EyePaint.Properties.Settings.Default.AdminEmail,
                    EyePaint.Properties.Settings.Default.AdminEmail,
                    "(EyePaint) Test Email",
                    ""
                );
                var c = new SmtpClient(EyePaint.Properties.Settings.Default.SmtpServer);
                c.Credentials = CredentialCache.DefaultNetworkCredentials;
                c.Send(m);
            }
            catch (Exception)
            {
                MessageBox.Show("Email not sent.");
                return;
            }
            MessageBox.Show("Email sent.");
        }
    }
}
