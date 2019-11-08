using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class MySongPage : Page
    {
        static ObservableCollection<Song> ListSong;
        static bool refresh = true;
        private ISongService _songService;
        private bool running = false;
        private int currentIndex = 0;

        public MySongPage()
        {
            this.Loaded += CheckMemberCredential;
            this.InitializeComponent();
            this._songService = new SongService();
            LoadSongs();
            this.volumeSlider.Value = MyMediaElement.Volume * 100;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimeTick;
            timer.Start();
        }

        private void TimeTick(object sender, object e)
        {
            if (MyMediaElement.Source != null && MyMediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeProgressBar.Minimum = 0;
                TimeProgressBar.Maximum = MyMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                TimeProgressBar.Value = MyMediaElement.Position.TotalSeconds;
            }
        }

        private void LoadSongs()
        {
            if (refresh)
            {
                var list = this._songService.GetMineSongs(ProjectConfiguration.CurrentMemberCredential);
                ListSong = new ObservableCollection<Song>(list);
                refresh = false;
            }
            else
            {
                Debug.WriteLine("Have all song");
            }

            ListViewSong.ItemsSource = ListSong;
        }

        private void CheckMemberCredential(object sender, RoutedEventArgs e)
        {
            if (ProjectConfiguration.CurrentMemberCredential == null)
            {
                this.Frame.Navigate(typeof(LoginPage));
            }
        }

        private void UIElement_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Debug.WriteLine(ListViewSong.SelectedIndex);
            currentIndex = ListViewSong.SelectedIndex;
            var playIcon = sender as SymbolIcon;
            if (playIcon != null)
            {
                var currentSong = playIcon.Tag as Song;
                Debug.WriteLine(currentSong.name);
                MyMediaElement.Source = new Uri(currentSong.link);
                NowPlayingText.Text = "Now playing: " + currentSong.name + " - " + currentSong.singer;
            }

            MyMediaElement.Play();
            PlayAndPauseButton.Icon = new SymbolIcon(Symbol.Pause);
            running = true;
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                MyMediaElement.Pause();
                PlayAndPauseButton.Icon = new SymbolIcon(Symbol.Play);
                running = false;
            }
            else
            {
                MyMediaElement.Play();
                PlayAndPauseButton.Icon = new SymbolIcon(Symbol.Pause);
                running = true;
            }
        }

        private void Next(object sender, RoutedEventArgs e)
        {
            currentIndex += 1;
            if (currentIndex >= ListSong.Count)
            {
                currentIndex = 0;
            }

            var song = ListSong[currentIndex];
            ListViewSong.SelectedIndex = currentIndex;
            MyMediaElement.Source = new Uri(song.link);
            NowPlayingText.Text = "Now playing: " + song.name + " - " + song.singer;
            MyMediaElement.Play();
        }

        private void Previous(object sender, RoutedEventArgs e)
        {
            currentIndex -= 1;
            if (currentIndex < 0)
            {
                currentIndex = ListSong.Count - 1;
            }

            var song = ListSong[currentIndex];
            ListViewSong.SelectedIndex = currentIndex;
            MyMediaElement.Source = new Uri(song.link);
            NowPlayingText.Text = "Now playing: " + song.name + " - " + song.singer;
        }

        private void ListViewSong_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(ListViewSong.SelectedIndex);
            currentIndex = ListViewSong.SelectedIndex;
            var currentSong = ListViewSong.Items[currentIndex] as Song;
            Debug.WriteLine(currentSong.name);
            MyMediaElement.Source = new Uri(currentSong.link);
            NowPlayingText.Text = "Now playing: " + currentSong.name + " - " + currentSong.singer;
            MyMediaElement.Play();
            PlayAndPauseButton.Icon = new SymbolIcon(Symbol.Pause);
            running = true;
        }

        private void VolumeSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                MyMediaElement.Volume = slider.Value / 100;
            }
        }


        private void InitTimeSlider()
        {
            TimeProgressBar.Minimum = 0;
            TimeProgressBar.Maximum = MyMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            TimeProgressBar.Value = MyMediaElement.Position.TotalSeconds;
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {

            TimeProgressBar.IsIndeterminate = false;
            TimeProgressBar.Minimum = 0;
            TimeProgressBar.Maximum = MyMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            TimeProgressBar.Value = MyMediaElement.Position.TotalSeconds;
        }

        private void Test()
        {
            MyMediaElement.Position = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(TimeProgressBar.Value));
        }


        private void TimeProgressBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            MyMediaElement.Position = new TimeSpan(0, 0, 0, Convert.ToInt32(TimeProgressBar.Value));
            Debug.WriteLine(TimeProgressBar.Value);
        }

        private void TimeProgressBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.TimeStatus.Text = TimeSpan.FromSeconds(TimeProgressBar.Value).ToString(@"hh\:mm\:ss") + "/" + MyMediaElement.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
        }

        private void TimeProgressBar_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            MyMediaElement.Position = new TimeSpan(0, 0, 0, Convert.ToInt32(TimeProgressBar.Value));
            Debug.WriteLine(TimeProgressBar.Value);
        }
    }
}
