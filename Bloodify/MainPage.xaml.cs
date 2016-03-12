﻿using System;
using System.Collections.Generic;
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
using Microsoft.WindowsAzure.Messaging;
using Windows.Networking.PushNotifications;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Bloodify
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            this.InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                FirstNameTxt.Text = localSettings.Values["First Name"].ToString();
                LastNameTxt.Text = localSettings.Values["Last Name"].ToString();
                GenderComboBox.SelectedIndex = (int)localSettings.Values["Gender"];
                BloodTypeCompoBox.SelectedIndex = (int)localSettings.Values["Blood Type"];
                Debug.WriteLine(localSettings.Values["Blood Type"]);

                var items = new ObservableCollection<Donor>(await App.MobileService.GetTable<Donor>().ToListAsync());
                DonorsList.ItemsSource = items;
            }
            catch
            {

            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(
                "If you donated Blood today, click yes. The app will notify you again when the time comes to donate again.",
                "Submit a Donation");

            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });
            

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();

            if (result.Label == "Yes")
                SubmitADonation();
          }

        private async void SubmitADonation()
        {
            if (FirstNameTxt.Text != null && LastNameTxt.Text != null && GenderComboBox.SelectedItem != null)
            {
                try
                {
                    Donor donor = new Donor();
                    donor.Name = FirstNameTxt.Text + " " + LastNameTxt.Text;
                    donor.Gender = (GenderComboBox.SelectedItem as TextBlock).Text;
                    if (BloodTypeCompoBox.SelectedItem != null)
                        donor.BloodType = (BloodTypeCompoBox.SelectedItem as TextBlock).Text;

                    await App.MobileService.GetTable<Donor>().InsertAsync(donor);
                    Initialize();
                }
                catch (Exception e)
                {
                    var dialog = new Windows.UI.Popups.MessageDialog(
                        "Sorry something went wrong. Check your internet connection and try again.");
                    await dialog.ShowAsync();
                }

                
            }
            else
            {
                var dialog = new Windows.UI.Popups.MessageDialog(
                "Please fill in the information about you (Name, Gender) first. Blood Type is not necessary.");
                await dialog.ShowAsync();
            }
        }


        //private async void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new Windows.UI.Popups.MessageDialog(
        //        "If you enable Most Active, you agree that your name and number of donations will be visible to other blood donors.",
        //        "Enable Most Active Donors");

        //    dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
        //    dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });


        //    dialog.DefaultCommandIndex = 0;
        //    dialog.CancelCommandIndex = 1;

        //    var result = await dialog.ShowAsync();

        //    var btn = sender as Button;
        //    btn.Content = $"Result: {result.Label} ({result.Id})";

        //}


        private void BloodTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["Blood Type"] = BloodTypeCompoBox.SelectedIndex;

            var selection = BloodTypeCompoBox.SelectedItem as TextBlock;

            if (selection.Text == "0-")
            {
                GiveTxt.Text = " Everyone";
                ReceiveTxt.Text = " 0-";
            }
            else if (selection.Text == "0+")
            {
                GiveTxt.Text = " 0+ A+ B+ AB+";
                ReceiveTxt.Text = " 0+ 0-";
            }
            else if (selection.Text == "A-")
            {
                GiveTxt.Text = " A+ A- AB+ AB-";
                ReceiveTxt.Text = " 0- A+";
            }
            else if (selection.Text == "A+")
            {
                GiveTxt.Text = " A+ AB+";
                ReceiveTxt.Text = " 0+ 0- A+ A-";
            }
            else if (selection.Text == "B-")
            {
                GiveTxt.Text = " B+ B- AB+ AB-";
                ReceiveTxt.Text = " 0- B-";
            }
            else if (selection.Text == "B+")
            {
                GiveTxt.Text = " B+ AB+";
                ReceiveTxt.Text = " 0+ 0- B+ B-";
            }
            else if (selection.Text == "AB-")
            {
                GiveTxt.Text = " AB+ AB-";
                ReceiveTxt.Text = " 0- A- B- AB-";
            }
            else if (selection.Text == "AB+")
            {
                GiveTxt.Text = " AB+";
                ReceiveTxt.Text = " Everyone";
            }
        }


        private void FirstNameTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            localSettings.Values["First Name"] = FirstNameTxt.Text;
        }

        private void LastNameTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            localSettings.Values["Last Name"] = LastNameTxt.Text;
        }

        private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["Gender"] = GenderComboBox.SelectedIndex;
        }
    }

    public class Donor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }

    }
}
