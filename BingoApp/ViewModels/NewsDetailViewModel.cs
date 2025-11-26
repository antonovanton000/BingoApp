using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BingoApp.ViewModels
{
    public partial class NewsDetailViewModel : MyBaseViewModel
    {

        [ObservableProperty]
        NewsItem newsItem = new();

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;
            IsBusy = true;
            await LoadNewsItem();
            IsBusy = false;
        }
                
        async Task LoadNewsItem()
        {                     
            try
            {
                var newsRespond = await App.RestClient.GetNewsDetail(NewsItem.FileName);
                if (newsRespond.IsSuccess)
                {
                    NewsItem = newsRespond.Data;                    
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async void Frame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                await LocalNewsHelper.SaveLocalViewedNews(NewsItem);                               
            }
            (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
        }
    }
}
