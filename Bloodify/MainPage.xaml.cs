using System;
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
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                FirstNameTxt.Text = localSettings.Values["First Name"].ToString();
                LastNameTxt.Text = localSettings.Values["Last Name"].ToString();
                GenderComboBox.SelectedIndex = (int)localSettings.Values["Gender"];
                BloodTypeCompoBox.SelectedIndex = (int)localSettings.Values["Blood Type"];
            }
            catch { }

            try
            {                
                var donorList = new ObservableCollection<Donor>(await App.MobileService.GetTable<Donor>().ToListAsync());

                bool flag = false;
                foreach (var c in donorList)
                {
                    if (FirstNameTxt.Text + " " + LastNameTxt.Text == c.Id)
                    {
                        LastDonationTxt.Text = " " + c.Date;
                        flag = true;
                    }
                }
                if (flag == false)
                    LastDonationTxt.Text = " Never";

                List<Donor> sortedDonorList = donorList.OrderByDescending(o => o.Donations).ToList();
                DonorsList.ItemsSource = sortedDonorList;
            }
            catch {}
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (checkPeriodBtwDonations())
            {
                var dialog = new Windows.UI.Popups.MessageDialog(
                    "If you donated Blood today, click yes. The app will notify you when the time comes to donate again." +
                    " By clicking yes you agree to share your Name and Donations to other donors.",
                    "Submit a Donation");

                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });

                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;

                var result = await dialog.ShowAsync();

                if (result.Label == "Yes")
                    SubmitADonation();
            }
            else
            {
                var dialog = new Windows.UI.Popups.MessageDialog("You must wait at least 56 Days between Blood Donations.");
                await dialog.ShowAsync();
            }

            
        }

        private  bool checkPeriodBtwDonations()
        {
            if (LastDonationTxt.Text == " Never")
                return true;
            string lastDonation = LastDonationTxt.Text;
            string today = DateTime.Today.ToString("d");
            List<int> lastDonationList = lastDonation.Split('/').Select(int.Parse).ToList();
            List<int> todayList = today.Split('/').Select(int.Parse).ToList();

            int period = (todayList[1] - lastDonationList[1]) + (todayList[0] - lastDonationList[0]) * 30 +
                (todayList[2] - lastDonationList[2]) * 365;

            if (period > 56)
                return true;            
            return false;
        }

        private async void SubmitADonation()
        {
            if (FirstNameTxt.Text != null && LastNameTxt.Text != null && GenderComboBox.SelectedItem != null)
            {
                Donor donor = new Donor();
                donor.Id = FirstNameTxt.Text + " " + LastNameTxt.Text;
                donor.Gender = (GenderComboBox.SelectedItem as TextBlock).Text;
                if (BloodTypeCompoBox.SelectedItem != null)
                    donor.BloodType = (BloodTypeCompoBox.SelectedItem as TextBlock).Text;
                donor.Date = DateTime.Today.ToString("d");
                donor.Donations = 1;                

                try
                {
                    bool update = false;
                    var donorList = new ObservableCollection<Donor>(await App.MobileService.GetTable<Donor>().ToListAsync());
                    foreach (var c in donorList)
                    {
                        if (donor.Id == c.Id)
                        {
                            donor.Donations = c.Donations + 1;
                            update = true;
                        }
                    }
                    if (update == false)
                        await App.MobileService.GetTable<Donor>().InsertAsync(donor);
                    else
                        await App.MobileService.GetTable<Donor>().UpdateAsync(donor);
                }
                catch (Exception e)
                {
                    var dialog = new Windows.UI.Popups.MessageDialog(
                        "Sorry something went wrong. Check your internet connection and try again.");
                    await dialog.ShowAsync();
                    Debug.WriteLine(e.ToString());
                }

                LoadData();

            }
            else
            {
                var dialog = new Windows.UI.Popups.MessageDialog(
                "Please fill in the information about you (Name, Gender) first. Blood Type is not necessary.");
                await dialog.ShowAsync();
            }
        }


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
                ReceiveTxt.Text = " 0- A-";
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
            LoadData();
        }

        private void LastNameTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            localSettings.Values["Last Name"] = LastNameTxt.Text;
            LoadData();
        }

        private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["Gender"] = GenderComboBox.SelectedIndex;
        }
    }

    public class Donor
    {
        public string Id { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }
        public int Donations { get; set; }
        public string Date { get; set; }
    }
}
