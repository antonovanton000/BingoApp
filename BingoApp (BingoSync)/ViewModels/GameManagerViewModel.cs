using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BingoApp.ViewModels
{
    public partial class GameManagerViewModel : MyBaseViewModel
    {
        
        [ObservableProperty]
        ObservableCollection<Game> games = [];

        [ObservableProperty]
        bool isModalOpen = false;

        [ObservableProperty]
        bool isGameCreating = false;

        [ObservableProperty]
        Game newGame = new();

        async Task LoadGames()
        {
            Games = [.. await PresetCollection.GetGamesAsync()];
        }

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            await LoadGames();
        }

        [RelayCommand]
        void OpenGame(Game game)
        {
            MainWindow.NavigateTo(new BoardManagerPage() { DataContext = new BoardManagerViewModel() { Game = game } });
        }

        [RelayCommand]
        void CreateNewGame()
        {
            NewGame = new();
            IsGameCreating = false;
            IsModalOpen = true;
        }

        [RelayCommand]
        async Task OpenFile()
        {
            var fo = new OpenFileDialog();
            fo.DefaultExt = ".jpg";
            fo.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (fo.ShowDialog() == true)
            {
                NewGame.CoverFilePath = fo.FileName;
            }            
        }


        [RelayCommand]
        async Task CreateGameFinally()
        {            
            NewGame.IsGameNameError= false;            
            NewGame.IsWebLinkBad = false;
            NewGame.IsDownloadError = false;
            var gamePath = System.IO.Path.Combine(App.Location, "Presets", NewGame.Name);

            if (string.IsNullOrEmpty(NewGame.Name))
            {
                NewGame.IsGameNameError = true;                
                return;
            }

            if (System.IO.Directory.Exists(gamePath))
            {
                MainWindow.ShowToast(new ToastInfo() { 
                    Title = App.Current.FindResource("mes_warning").ToString(), 
                    Detail = App.Current.FindResource("mes_gameexists").ToString(), 
                    ToastType = ToastType.Warning 
                });
                return;
            }


            if (!string.IsNullOrEmpty(NewGame.CoverFilePath))
            {
                var imgPath = System.IO.Path.Combine(App.Location, "GamesImages", NewGame.Name + ".jpg");
                System.IO.File.Copy(NewGame.CoverFilePath, imgPath, true);
            }

            if (!string.IsNullOrEmpty(NewGame.CoverWebLink))
            {

                if (NewGame.CoverWebLink.Contains(".jpg") || NewGame.CoverWebLink.Contains(".jpeg")
                    || NewGame.CoverWebLink.Contains(".png"))
                {
                    IsGameCreating = true;
                    var client = new HttpClient();
                    try
                    {
                        var bytes = await client.GetByteArrayAsync(NewGame.CoverWebLink);
                        var imgPath = System.IO.Path.Combine(App.Location, "GamesImages", NewGame.Name + ".jpg");
                        await System.IO.File.WriteAllBytesAsync(imgPath, bytes);
                        IsGameCreating = false;
                    }
                    catch (Exception)
                    {
                        IsGameCreating = false;
                        NewGame.IsDownloadError = true;
                        return;
                    }
                }
                else
                {
                    NewGame.IsWebLinkBad = true;
                    return;
                }
            }

            
            System.IO.Directory.CreateDirectory(gamePath);
                        
            IsModalOpen = false;
            MainWindow.ShowToast(new ToastInfo() {
                Title = App.Current.FindResource("mes_success").ToString(),
                Detail = App.Current.FindResource("mes_gamecreatesuccess").ToString(),
                ToastType = ToastType.Success 
            });
            await LoadGames();
        }
    }
}
