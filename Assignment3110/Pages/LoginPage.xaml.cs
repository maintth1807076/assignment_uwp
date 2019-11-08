using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Assignment3110.Entity;
using Assignment3110.Service;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Assignment3110.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private IMemberService _memberService;
        private IFileService _fileService;

        public LoginPage()
        {
            this.InitializeComponent();
            this._memberService = new MemberService();
            this._fileService = new LocalFileService();
            this.Loaded += LoadUserInformation;
        }



        private void ButtonRegister_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(RegisterPage));
        }

        private void Submit_OnClick(object sender, RoutedEventArgs e)
        {
            var memberLogin = new MemberLogin
            {
                email = this.Email.Text,
                password = this.Password.Password
            };
            var memberCredential = this._memberService.Login(memberLogin);
            if (memberCredential != null)
            {
                Debug.WriteLine(memberCredential.token);
                ProjectConfiguration.CurrentMemberCredential = memberCredential;
                this._fileService.SaveMemberCredentialToFile(memberCredential);
                this.LoginDialog.Visibility = Visibility.Collapsed;
                this.LogOut.Visibility = Visibility.Visible;
                this.Frame.Navigate(typeof(UserInformation));
            }
        }

        private async void LogOut_OnClick(object sender, RoutedEventArgs e)
        {
            ProjectConfiguration.CurrentMemberCredential = null;
            var storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("SavedFile",
                CreationCollisionOption.OpenIfExists);
            var storageFile =
                await storageFolder.GetFileAsync("token.txt");
            if (storageFile != null)
            {
                await storageFile.DeleteAsync();
                this.LoginDialog.Visibility = Visibility.Visible;
                this.LogOut.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadUserInformation(object sender, RoutedEventArgs e)
        {
            MemberCredential memberCredential = ProjectConfiguration.CurrentMemberCredential;
            if (ProjectConfiguration.CurrentMemberCredential == null)
            {
                memberCredential = await this._fileService.ReadMemberCredentialFromFile();
            }
            else
            {
                this.LoginDialog.Visibility = Visibility.Collapsed;
                this.LogOut.Visibility = Visibility.Visible;
            }
        }
    }
}
