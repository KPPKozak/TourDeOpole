﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TourDeOpole.Services;
using TourDeOpole.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TourDeOpole.ViewModels
{
    //tutaj wszystkie metody, zwykle komendy 

    public partial class MainViewModel : BaseViewModel
    {
        public Command GoToDetailsCommand { get; set; }
        public MainViewModel()
        {
            GoToDetailsCommand = new Command(GoToDetails);
        }
        private async void GoToDetails()
        {
            await NavigationService.GoToPlaceDetails();
        }
        public async void getLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var cts = new CancellationTokenSource();
                var mylocation = await Geolocation.GetLocationAsync(request, cts.Token);
                var location = new Location(50.664286, 17.936186);
                if (location != null)
                {
                    //DisplayAlert("Tytuł", $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}", "OK");
                }

                //DisplayAlert("Tytuł", $"Dystans {CalculateDistanceBetweenLocation(location, mylocation)}", "Super");
            }
            catch (PermissionException pEx)
            {
                if(pEx != null)
                {
                    DisplayAlert("Wystąpił błąd", "Niestety nie mamy uprawnień do pobrania Twojej lokalizacji", "Dobrze");
                }
            }
            catch
            {
                DisplayAlert("Wystąpił błąd", "Niestety nie udało się pobrać Twojej lokalizacji", "Dobrze");
            }
        }

        public async void DisplayAlert(string title, string message, string cancel)
        {
            await App.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        public double CalculateDistanceBetweenLocation(Location location, Location myLocation)
        {
            return Location.CalculateDistance(location, myLocation, DistanceUnits.Kilometers);
        }
    }
}