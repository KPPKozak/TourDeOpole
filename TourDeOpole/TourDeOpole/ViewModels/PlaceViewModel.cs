﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using TourDeOpole.Models;
using TourDeOpole.Services;
using TourDeOpole.Views;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace TourDeOpole.ViewModels
{
    //tutaj wszystkie metody, zwykle komendy 

    public partial class PlaceViewModel : BaseViewModel 
    {
        public Command GoToDetailsCommand { get; set; }
        public Command GoToAddCommand { get; set; }
        public Command GoToScanQRCommand { get; set; }
        public Command ToggleFavoriteCommand { get; set; }

        public ObservableCollection<Place> myPlace { get; set; }
        public ObservableCollection<Place> FilteredPlaces { get; set; }
        public ObservableCollection<Category> Category { get; set; }
        public PlaceViewModel()
        {
            GoToDetailsCommand = new Command<Place>(GoToDetails);
            GoToAddCommand = new Command(GoToAddPlace);
            GoToScanQRCommand = new Command(GoToScanQR);
            ToggleFavoriteCommand = new Command(FavoritePlace);
            Category = new ObservableCollection<Category>();
            myPlace = new ObservableCollection<Place>();
            LoadCategory();
            LoadPlace();
        }

        private void FavoritePlace()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Loads the list of places from the database or URL and sets it as the current list of places.
        /// </summary>
        public async void LoadPlace()
        {
            myPlace.Clear();
            var places = App.Database.GetPlaceAsync().Result;
            var databaseEmpty = false;
            if (places == null || places.Count == 0)
            {
                places = await URLService.GetPlaces();
                databaseEmpty = true;
            }

            foreach (var place in places)
            {
                myPlace.Add(place);
                if (databaseEmpty)
                    await App.Database.SavePlaceAsync(place);
            }
            Place.ListOfPlaces = myPlace;

            FilteredPlaces = new ObservableCollection<Place>(myPlace);

            databaseEmpty = false;
        }
        /// <summary>
        /// Loads the list of categories from the database or URL and sets it as the current list of categories.
        /// </summary>
        private async void LoadCategory()
        {
            var categories = App.Database.GetCategoryeAsync().Result;
            var databaseEmpty = false;
            if (categories == null || categories.Count == 0)
            {
                databaseEmpty = true;
                categories = await URLService.GetCategory();
            }

            foreach (var catgory in categories)
            {
                Category.Add(catgory);
                if (databaseEmpty)
                    await App.Database.SaveCategoryAsync(catgory);
            }
            databaseEmpty = false;
        }
        /// <summary>
        /// This function loads the list of categories from the database or URL and sets it as the current list of categories.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="searchParameter"></param>
        public void OnSearchTextChanged(object sender, TextChangedEventArgs e, string searchParameter)
        {
            PropertyInfo propertyInfo = typeof(Place).GetProperty(searchParameter);
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                FilteredPlaces = new ObservableCollection<Place>(Place.ListOfPlaces);
                OnPropertyChanged(nameof(FilteredPlaces));
            }
            else
            {
                FilteredPlaces = new ObservableCollection<Place>(Place.ListOfPlaces.Where(x => propertyInfo.GetValue(x, null).ToString().ToUpper().Contains(e.NewTextValue.ToUpper())));
                OnPropertyChanged(nameof(FilteredPlaces));
            }
        }

        private async void GoToScanQR()
        {
            await NavigationService.GoToScanQR();
        }


        #region GetLocation
        public string myLocCity { get; set; }
        public string myLocAdress { get; set; }
        /// <summary>
        /// This method uses Xamarin.Essentials to retrieve the user's location and then converts it to a readable address format.
        /// </summary>
        public async void getLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var cts = new CancellationTokenSource();
                var mylocation = await Geolocation.GetLocationAsync(request, cts.Token);
                var location = new Location(50.664286, 17.936186);//do remove
                if (location != null)
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();

                    var geocodeAddress =
                 $"{placemark.Thoroughfare} " +
                 $" {placemark.SubThoroughfare}";
           
                 myLocAdress =geocodeAddress;
                    myLocCity = $"{placemark.Locality}";

                    OnPropertyChanged(nameof(myLocAdress));
                    OnPropertyChanged(nameof(myLocCity));
                    //DisplayAlert("Tytuł", $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}", "OK");
                }

                //DisplayAlert("Tytuł", $"Dystans {CalculateDistanceBetweenLocation(location, mylocation)}", "Super");
            }
            catch (PermissionException pEx)
            {
                if (pEx != null)
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
        private async void GoToDetails(Place place)
        {
            await Shell.Current.GoToAsync($"{nameof(PlaceDetailsView)}?{nameof(PlaceDetailsViewModel.PlaceID)}={place.PlaceID}");
        }
        private async void GoToAddPlace()
        {
            await NavigationService.GoToAdd();
        }
        #endregion
    }
}
