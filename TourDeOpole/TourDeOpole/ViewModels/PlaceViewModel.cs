﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TourDeOpole.Models;
using TourDeOpole.Repository;
using TourDeOpole.Services;
using TourDeOpole.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace TourDeOpole.ViewModels
{
    //tutaj wszystkie metody, zwykle komendy 

    public partial class PlaceViewModel : BaseViewModel
    {
        public ICommand GoToDetailsCommand { get; set; }
        public ICommand GoToAddCommand { get; set; }
        public ICommand GoToScanQRCommand { get; set; }
        public ICommand ToggleFavoriteCommand { get; set; }
        public ICommand CheckClosest { get; set; }
        public ICommand ReloadPlaces { get; set; }

        public bool IsLocationAvaible { get; set; }
        public ObservableCollection<Place> FilteredPlaces { get; set; } = new ObservableCollection<Place>();
        public ObservableCollection<Category> Category { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<HasCategory> HasCategory { get; set; } = new ObservableCollection<HasCategory>();

        public PlaceViewModel()
        {
            GoToDetailsCommand = new Command<Place>(async (place) =>
            {
                await Shell.Current.GoToAsync($"{nameof(PlaceDetailsView)}?{nameof(PlaceDetailsViewModel.PlaceID)}={place.PlaceID}");
            });

            GoToAddCommand = new Command(async () =>
            {
                await NavigationService.GoToAdd();
            });
            GoToScanQRCommand = new Command(async () =>
            {
                await NavigationService.GoToScanQR();
            });
            ToggleFavoriteCommand = new Command<Place>(async (place) =>
            {
                place.IsFavourite = !place.IsFavourite;
                await App.Database.EditPlaceAsync(place);
                LoadPlace();
            });
            CheckClosest = new Command<Place>(async (place) =>
            {
                await CalcClose();
            });
            ReloadPlaces = new Command<Place>(async (place) =>
            {
                LoadPlace();
            });
            LoadCategory();
            LoadPlace();
            LoadHasCategory();
        }

        /// <summary>
        /// Loads the list of places from the database or URL and sets it as the current list of places.
        /// </summary>
        public async Task CalcClose()
        {
            FilteredPlaces.Clear();
            try
            {
                var placemark = LocationService.Placemark;

                if (placemark == null)
                {
                    await App.Current.MainPage.DisplayAlert("Wystąpił błąd", "Nie udało się pobrać lokalizacji", "Dobrze");
                    return;
                }
                else
                {
                    var max = double.PositiveInfinity;
                    Place closest = new Place();
                    var places = App.Database.GetPlaceAsync().Result;
                    foreach (var place in places)
                    {
                        var distance = CalculateDistanceBetweenLocation(place, placemark);
                        if (distance < max)
                        {
                            max = distance;
                            closest = place;
                        }
                    }
                    if (closest != null)
                    {
                        FilteredPlaces.Add(closest);
                    }
                    OnPropertyChanged(nameof(FilteredPlaces));
                }

            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Błąd", "Wystąpił błąd podczas szukania lokalizacji", "Dobrze");
            }
        }
        public async void LoadPlace()
        {
            FilteredPlaces.Clear();
            var databaseEmpty = false;
            var places = App.Database.GetPlaceAsync().Result;
            if (places == null || places.Count == 0)
            {
                places = await URLService.GetPlaces();
                databaseEmpty = true;
            }
            foreach (var place in places)
            {
                if (!place.Image.StartsWith("http"))
                    place.Image = URLService.SetURL(place.Image);
                FilteredPlaces.Add(place);
                if (databaseEmpty)
                    await App.Database.SavePlaceAsync(place);
            }
            Place.ListOfPlaces = FilteredPlaces;

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

        private async void LoadHasCategory()
        {
            var hasCategory = App.Database.GetHasCategoryAsync().Result;
            if (hasCategory == null || hasCategory.Count == 0)
            {
                hasCategory = await URLService.GetHasCategory();
                foreach (var hs in hasCategory)
                    await App.Database.SaveHasCategoryAsync(hs);
            }
            foreach (var hs in hasCategory)
            {
                HasCategory.Add(hs);
            }
        }

        /// <summary>
        /// This function loads the list of categories from the database or URL and sets it as the current list of categories.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="searchParameter"></param>

        public void OnSearchTextChanged(TextChangedEventArgs e, string searchParameter)
        {
            PropertyInfo propertyInfo = typeof(Place).GetProperty(searchParameter);
            Place.ListOfPlaces = new ObservableCollection<Place>(App.Database.GetPlaceAsync().Result);
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

        public void OnCategoryButtonPressed(string filterParameter)
        {
            Place.ListOfPlaces = new ObservableCollection<Place>(App.Database.GetPlaceAsync().Result);
            if (filterParameter == "Wszystkie")
            {
                FilteredPlaces = new ObservableCollection<Place>(App.Database.GetPlaceAsync().Result);
                OnPropertyChanged(nameof(FilteredPlaces));
            }
            else
            {
                int categoryId = Category.Where(c => c.Name == filterParameter).FirstOrDefault().CategoryID;
                FilteredPlaces = new ObservableCollection<Place>(Place.ListOfPlaces.Where(p => HasCategory.Any(hc => hc.PlaceID == p.PlaceID && hc.CategoryID == categoryId)));
                OnPropertyChanged(nameof(FilteredPlaces));
            }
        }

        /// <summary>
        /// This method uses Xamarin.Essentials to retrieve the user's location and then converts it to a readable address format.
        /// </summary>
    }
}
