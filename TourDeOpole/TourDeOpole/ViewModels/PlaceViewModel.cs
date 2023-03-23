﻿using System;
using System.Threading;
using TourDeOpole.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TourDeOpole.ViewModels
{
    //tutaj wszystkie metody, zwykle komendy 

    public partial class PlaceViewModel : BaseViewModel
    {
        public Command GoToDetailsCommand { get; set; }
        public Command GoToAddCommand { get; set; }
        public PlaceViewModel()
        {
            GoToDetailsCommand = new Command(GoToDetails);
            GoToAddCommand = new Command(GoToAddPlace);
        }

        #region GetLocation
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
                    Alert.DisplayAlert("Wystąpił błąd", "Niestety nie mamy uprawnień do pobrania Twojej lokalizacji", "Dobrze");
                }
            }
            catch
            {
                Alert.DisplayAlert("Wystąpił błąd", "Niestety nie udało się pobrać Twojej lokalizacji", "Dobrze");
            }
        }

        public double CalculateDistanceBetweenLocation(Location location, Location myLocation)
        {
            return Location.CalculateDistance(location, myLocation, DistanceUnits.Kilometers);
        }
        #endregion 

        #region Navigation
        private async void GoToDetails()
        {
            await NavigationService.GoToPlaceDetails();
        }
        private async void GoToAddPlace()
        {
            await NavigationService.GoToAdd();
        }
        #endregion
    }
}