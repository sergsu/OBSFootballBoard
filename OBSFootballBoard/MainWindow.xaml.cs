﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace OBSFootballBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dumper.Dumper Dumper = new Dumper.Dumper();
        private readonly Timer TimeTimer = new Timer(1000);
        private uint TimeMinutes = 0;
        private uint TimeSeconds = 0;

        public MainWindow()
        {
            InitializeComponent();

            DependencyPropertyDescriptor dependencyPropertyDescriptor = DependencyPropertyDescriptor
                .FromProperty(
                    TextBlock.TextProperty,
                    typeof(TextBlock)
                );
            dependencyPropertyDescriptor.AddValueChanged(
                HomeScore,
                (sender, args) => Dumper.DumpMatch(OBSFootballBoard.Dumper.FileTypes.TeamHomeScore, ((TextBlock)sender).Text)
            );
            dependencyPropertyDescriptor.AddValueChanged(
                AwayScore,
                (sender, args) => Dumper.DumpMatch(OBSFootballBoard.Dumper.FileTypes.TeamAwayScore, ((TextBlock)sender).Text)
            );
            ChangeScore(HomeScore, 0, HomeScoreDecrement);
            ChangeScore(AwayScore, 0, AwayScoreDecrement);

            TimeTimer.Elapsed += TimeTimer_Elapsed;
        }

        private void HomeNameChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Dumper.DumpMatch(OBSFootballBoard.Dumper.FileTypes.TeamHomeName, ((TextBox)sender).Text);
        }

        private void AwayNameChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Dumper.DumpMatch(OBSFootballBoard.Dumper.FileTypes.TeamAwayName, ((TextBox)sender).Text);
        }

        private void ChangeScore(TextBlock Box, int Change, Button DecrementButton)
        {
            int currentScore = int.Parse(Box.Text);
            currentScore = System.Math.Max(currentScore + Change, 0);

            Box.Text = currentScore.ToString();
            DecrementButton.IsEnabled = currentScore > 0;
        }

        private void HomeScoreIncrement_Click(object sender, RoutedEventArgs e)
        {
            ChangeScore(HomeScore, 1, HomeScoreDecrement);
        }

        private void HomeScoreDecrement_Click(object sender, RoutedEventArgs e)
        {
            ChangeScore(HomeScore, -1, HomeScoreDecrement);
        }

        private void AwayScoreIncrement_Click(object sender, RoutedEventArgs e)
        {
            ChangeScore(AwayScore, 1, AwayScoreDecrement);
        }

        private void AwayScoreDecrement_Click(object sender, RoutedEventArgs e)
        {
            ChangeScore(AwayScore, -1, AwayScoreDecrement);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void TimeHalfSecond_Checked(object sender, RoutedEventArgs e)
        {
            Dumper.DumpMatch(OBSFootballBoard.Dumper.FileTypes.TimeHalf, "2");

            TimeSeconds = 0;
            TimeMinutes = uint.Parse(Properties.Settings.Default.TimeHalfDuration);
            UpdateTimeBox();
        }

        private void TimeHalfFirst_Checked(object sender, RoutedEventArgs e)
        {
            Dumper.DumpMatch(OBSFootballBoard.Dumper.FileTypes.TimeHalf, "1");

            TimeSeconds = 0;
            TimeMinutes = 0;
            UpdateTimeBox();
        }

        private void TimeHalfDurationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((bool)TimeHalfSecond.IsChecked && !TimeTimer.Enabled)
            {
                try
                {
                    TimeMinutes = uint.Parse(Properties.Settings.Default.TimeHalfDuration);
                    TimeSeconds = 0;
                    UpdateTimeBox();
                }
                catch (FormatException) { }
            }
        }

        private void TimeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeSeconds++;
            if (TimeSeconds == 60)
            {
                TimeSeconds = 0;
                TimeMinutes++;
            }

            UpdateTimeBox();
        }

        private void UpdateTimeBox(Nullable<bool> StartIsDisabled = null)
        {
            if (!(StartIsDisabled is null))
            {
                TimeStart.IsEnabled = !(bool)StartIsDisabled;
                TimeStop.IsEnabled = (bool)StartIsDisabled;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                TimeBox.Text = TimeMinutes.ToString("00") + CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator + TimeSeconds.ToString("00");
            }));
        }

        private void TimeStart_Click(object sender, RoutedEventArgs e)
        {
            TimeTimer.Start();
            UpdateTimeBox(true);
        }

        private void TimeStop_Click(object sender, RoutedEventArgs e)
        {
            TimeTimer.Stop();
            UpdateTimeBox(false);
        }

        private void TimeReset_Click(object sender, RoutedEventArgs e)
        {
            TimeTimer.Stop();
            TimeMinutes = 0;
            TimeSeconds = 0;
            UpdateTimeBox(false);
        }

        private void TimeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Dumper.DumpMatch(OBSFootballBoard.Dumper.FileTypes.TimeMain, ((TextBox)sender).Text);
        }
    }
}
