﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Assignment3110.Entity;
using Assignment3110.Service;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Assignment3110.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterPage : Page
    {
        private string _gender = "Gender";
        private StorageFile photo;
        private IMemberService _memberService;
        public RegisterPage()
        {
            this.InitializeComponent();
            this._memberService = new MemberService();
        }

        private void Submit_OnClick(object sender, RoutedEventArgs e)
        {
            var birthdaySelectedDate = this.Birthday.SelectedDate;
            if (birthdaySelectedDate == null)
            {
                birthdaySelectedDate = DateTime.Now;
            }
            var birthday = birthdaySelectedDate.Value.ToString("yyyy-MM-dd");

            var member = new Member
            {
                firstName = this.FirstName.Text,
                lastName = this.LastName.Text,
                avatar = this.AvatarUrl.Text,
                phone = this.Phone.Text,
                address = this.Address.Text,
                introduction = this.Introduction.Text,
                email = this.Email.Text,
                gender = _gender.Equals("Male") ? 1 : (_gender.Equals("Female") ? 0 : 2),
                birthday = birthday,
                password = this.Password.Password
            };
            var responseMember = this._memberService.Register(member);
            if (responseMember != null)
            {
                Debug.WriteLine("Register success !");
                this.Frame.Navigate(typeof(LoginPage));
            }
            else
            {
                Debug.WriteLine("Register fails !");
            }
        }

        private async void ProcessCaptureImage()
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

            this.photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (this.photo == null)
            {
                return;
            }

            string uploadUrl = GetUploadUrl();
            HttpUploadFile(uploadUrl, "myFile", "image/jpeg");
        }

        public async void HttpUploadFile(string url, string paramName, string contentType)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";

            Stream rs = await wr.GetRequestStreamAsync();
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string header = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n", paramName, "path_file", contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            // write file.
            Stream fileStream = await this.photo.OpenStreamForReadAsync();
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);

            WebResponse wresp = null;
            try
            {
                wresp = await wr.GetResponseAsync();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                string imageUrl = reader2.ReadToEnd();
                Avatar.Source = new BitmapImage(new Uri(imageUrl, UriKind.Absolute));
                AvatarUrl.Text = imageUrl;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error uploading file", ex.StackTrace);
                Debug.WriteLine("Error uploading file", ex.InnerException);
                if (wresp != null)
                {
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

        private string GetUploadUrl()
        {
            var httpClient = new HttpClient();
            var response = httpClient.GetAsync(ProjectConfiguration.GET_UPLOAD_URL).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().Result;
        }


        private void RadionButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton checkedRadio)
            {
                this._gender = checkedRadio.Tag.ToString();
            }
        }

        private void ButtonLogin_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }

        private void CapturePhoto(object sender, RoutedEventArgs e)
        {
            ProcessCaptureImage();
        }
    }
}
