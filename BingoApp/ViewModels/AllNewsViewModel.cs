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
    public partial class AllNewsViewModel : MyBaseViewModel
    {

        [ObservableProperty]
        ObservableCollection<NewsItem> newsItems = new ObservableCollection<NewsItem>();

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();            
            IsBusy = true;
            await LoadNews();
            IsBusy = false;
        }
        
        async Task LoadNews()
        {
            NewsItems.Clear();
            var localNewsIds = await LocalNewsHelper.GetLocalViewedNews();
            try
            {
                var newsRespond = await App.RestClient.GetAllNews();
                if (newsRespond.IsSuccess)
                {
                    var news = newsRespond.Data;
                    if (news != null && news.Count > 0)
                    {
                        foreach (var item in news.OrderByDescending(i => i.Date))
                        {
                            item.IsViewed = localNewsIds.Contains(item.FileName);
                            NewsItems.Add(item);
                        }
                    }
                    MainWindow.SetNotificationsCount(news?.Count(i => !i.IsViewed) ?? 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }


        [RelayCommand]
        void ViewNews(NewsItem item)
        {
            var vm = new NewsDetailViewModel() { NewsItem = item };
            var page = new NewsDetailPage() { DataContext = vm };

            MainWindow.NavigateTo(page);
        }

    }
}
