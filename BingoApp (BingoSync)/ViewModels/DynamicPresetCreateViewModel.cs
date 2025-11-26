using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Xps.Serialization;

namespace BingoApp.ViewModels;

public partial class DynamicPresetCreateViewModel : MyBaseViewModel
{

    [ObservableProperty]
    BingoAppPlayer creator;

    [ObservableProperty]
    BingoAppPlayer secondPlayer;

    [ObservableProperty]
    BingoAppPlayer selectedPlayer;

    [ObservableProperty]
    bool isCreator;

    [ObservableProperty]
    string presetId;

    [ObservableProperty]
    string presetName;

    [ObservableProperty]
    string presetJSON;

    public List<DynamicPresetSquare> PresetSquares { get; set; } = [];

    public ObservableCollection<DynamicPresetSquare> AllSquares { get; set; } = [];
    public ObservableCollection<DynamicPresetSquare> CreatorSquares { get; set; } = [];
    public ObservableCollection<DynamicPresetSquare> SecondPlayerSquares { get; set; } = [];

    public ObservableCollection<BingoAppPlayer> AvailablePlayers { get; set; } = [];


    [ObservableProperty]
    ICollectionView boardSquaresCollection;

    [ObservableProperty]
    bool isIvitePanelVisible = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAllReady))]
    bool isCreatorReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAllReady))]
    bool isSecondPlayerReady;

    [ObservableProperty]
    bool isInviteSent = false;

    [ObservableProperty]
    bool isCreatorEnoughSquares = false;

    [ObservableProperty]
    bool isSecondPlayerEnoughSquares = false;

    [ObservableProperty]
    bool canCheckSquares = false;

    public bool IsAllReady => IsCreatorReady && IsSecondPlayerReady;

    BingoAppPlayer GetPlayerById(string playerId)
    {
        return Creator.Id == playerId ? Creator : SecondPlayer;
    }

    ObservableCollection<DynamicPresetSquare> GetPlayerSquaresById(string playerId)
    {
        return Creator.Id == playerId ? CreatorSquares : SecondPlayerSquares;
    }

    bool IsPlayerCreator(string playerId)
    {
        return Creator.Id == playerId;
    }


    [RelayCommand]
    async Task Appearing()
    {
        MainWindow.HideSettingsButton();
        foreach (var square in PresetSquares)
        {
            AllSquares.Add(square);
        }
        BoardSquaresCollection = CollectionViewSource.GetDefaultView(AllSquares);
        await GetAvailablePlayers();
        App.SignalRHub.OnAcceptInviteRecieved += SignalRHub_OnAcceptInviteRecieved;
        App.SignalRHub.OnRejectInviteRecieved += SignalRHub_OnRejectInviteRecieved;
        App.SignalRHub.OnCheckSquareRecieved += SignalRHub_OnCheckSquareRecieved;
        App.SignalRHub.OnUncheckSquareRecieved += SignalRHub_OnUncheckSquareRecieved;
        App.SignalRHub.OnPlayerReadyRecieved += SignalRHub_OnPlayerReadyRecieved;
        App.SignalRHub.OnPlayerNotReadyRecieved += SignalRHub_OnPlayerNotReadyRecieved;
        App.SignalRHub.OnStartPresetCreationRecieved += SignalRHub_OnStartPresetCreationRecieved;
        App.SignalRHub.OnPresetCreatedRecieved += SignalRHub_OnPresetCreatedRecieved;
        App.SignalRHub.OnPresetCreationCanceledRecieved += SignalRHub_OnPresetCreationCanceledRecieved;

        if (IsCreator)
        {
            IsIvitePanelVisible = true;
        }
        else
        {
            await App.SignalRHub.SendStartPresetCreationAsync(PresetId);
            IsIvitePanelVisible = false;
        }

    }
    [RelayCommand(AllowConcurrentExecutions = true)]
    async Task CheckSquare(DynamicPresetSquare square)
    {
        if (IsCreator)
        {
            if (IsCreatorEnoughSquares)
                return;

            CreatorSquares.Add(square);
            AllSquares.Remove(square);
            IsCreatorEnoughSquares = CreatorSquares.Count == 13;
        }
        else
        {
            if (IsSecondPlayerEnoughSquares)
                return;

            SecondPlayerSquares.Add(square);
            AllSquares.Remove(square);
            IsSecondPlayerEnoughSquares = SecondPlayerSquares.Count == 13;
        }
        await App.SignalRHub.CheckSquareAsync(PresetId, App.CurrentPlayer.Id, square.Id);
    }

    [RelayCommand]
    async Task UncheckSquare(DynamicPresetSquare square)
    {
        if (IsCreator)
        {
            CreatorSquares.Remove(square);
            var index = int.Parse(square.Id.Replace("sq_", ""));
            AllSquares.Insert(index, square);
            IsCreatorEnoughSquares = CreatorSquares.Count == 13;
        }
        else
        {
            SecondPlayerSquares.Remove(square);
            var index = int.Parse(square.Id.Replace("sq_", ""));
            AllSquares.Insert(index, square);
            IsSecondPlayerEnoughSquares = SecondPlayerSquares.Count == 13;
        }
        await App.SignalRHub.UncheckSquareAsync(PresetId, App.CurrentPlayer.Id, square.Id);
    }

    [ObservableProperty]
    string searchQueue;

    [RelayCommand]
    void Search()
    {
        EnterSearch();
    }

    public void EnterSearch()
    {
        if (SearchQueue == null)
            return;

        BoardSquaresCollection.Filter = item => (item as DynamicPresetSquare)?.Name.ToLower().Contains(SearchQueue.ToLower()) ?? false;
    }

    [RelayCommand]
    void ClearSearch()
    {
        SearchQueue = "";
        BoardSquaresCollection.Filter = null;
    }

    [RelayCommand]
    async Task RefreshAvailablePlayers()
    {
        await GetAvailablePlayers();
    }

    async Task GetAvailablePlayers()
    {
        var resp = await App.RestClient.GetAvailablePlayersAsync(App.CurrentPlayer.Id);
        if (resp.IsSuccess)
        {
            AvailablePlayers.Clear();
            foreach (var item in resp.Data)
            {
                AvailablePlayers.Add(item);
            }
        }
    }

    [RelayCommand]
    async Task SendInvite()
    {
        if (App.SignalRHub.IsHubConnected)
        {
            await App.SignalRHub.InvitePlayerAsync(Creator, SelectedPlayer, PresetId, PresetName, PresetJSON);
            IsInviteSent = true;
        }
    }

    [RelayCommand]
    async Task CancelInvite()
    {
        if (App.SignalRHub.IsHubConnected)
        {
            await App.SignalRHub.CancelInvitePlayerAsync(SelectedPlayer, PresetId);
            IsInviteSent = false;
        }
    }

    [RelayCommand]
    async Task PlayerReady()
    {
        if (IsCreator)
        {
            IsCreatorReady = true;
        }
        else
        {
            IsSecondPlayerReady = true;
        }

        await App.SignalRHub.SendPlayerReadyAsync(PresetId, App.CurrentPlayer.Id);
    }

    [RelayCommand]
    async Task PlayerNotReady()
    {
        if (IsCreator)
        {
            IsCreatorReady = false;
        }
        else
        {
            IsSecondPlayerReady = false;
        }

        await App.SignalRHub.SendPlayerNotReadyAsync(PresetId, App.CurrentPlayer.Id);
    }

    [RelayCommand]
    void CreatePreset()
    {
        MainWindow.ShowMessage(App.Current.FindResource("mes_areyousureyouwanttocreatepreset").ToString(), MessageNotificationType.YesNo,
            async () =>
            {
                var playersSquares = new List<DynamicPresetSquare>();
                playersSquares.AddRange(CreatorSquares);
                playersSquares.AddRange(SecondPlayerSquares);

                var presetSquares = playersSquares.Select(i => new PresetSquare() { Name = i.Name }).ToList();
                var json = JsonConvert.SerializeObject(presetSquares);

                App.DynamicPresetJSON = json;
                App.SignalRHub.OnPresetCreatedRecieved -= SignalRHub_OnPresetCreatedRecieved;
                await App.SignalRHub.SendPresetCreatedAsync(PresetId);
                MainWindow.GoBack();
            });
    }

    [RelayCommand]
    void CancelPresetCreation()
    {
        MainWindow.ShowMessage(App.Current.FindResource("mes_areyousureyouwanttocancelpreset").ToString(), MessageNotificationType.YesNo,
            async () =>
            {
                App.NewBoardModel = null;
                await App.SignalRHub.SendPresetCreationCanceledAsync(PresetId, App.CurrentPlayer.Id);
                MainWindow.GoBack();
            });
    }

    private void SignalRHub_OnRejectInviteRecieved(object? sender, Classes.BingoAppSignalRHub.RejectInviteEventArgs e)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            IsInviteSent = false;
            MainWindow.ShowToast(new ToastInfo() { Title = "Внимание!", Detail = "Игрок откланил ваше приглашение!", ToastType = ToastType.Warning });
        });
    }

    private void SignalRHub_OnAcceptInviteRecieved(object? sender, Classes.BingoAppSignalRHub.AcceptInviteEventArgs e)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            SecondPlayer = new BingoAppPlayer()
            {
                NickName = e.NickName,
                Id = e.PlayerId
            };
            IsInviteSent = false;
            IsIvitePanelVisible = false;
        });
    }

    private void SignalRHub_OnUncheckSquareRecieved(object? sender, Classes.BingoAppSignalRHub.UncheckSquareEventArgs e)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            var sqauares = GetPlayerSquaresById(e.PlayerId);
            var square = sqauares.FirstOrDefault(x => x.Id == e.SquareId);
            if (square != null)
            {
                sqauares.Remove(square);
                var index = int.Parse(square.Id.Replace("sq_", ""));
                AllSquares.Insert(index, square);
            }
            if (IsPlayerCreator(e.PlayerId))
            {
                IsCreatorEnoughSquares = CreatorSquares.Count == 13;
            }
            else
            {
                IsSecondPlayerEnoughSquares = SecondPlayerSquares.Count == 13;
            }
        });
    }

    private void SignalRHub_OnCheckSquareRecieved(object? sender, Classes.BingoAppSignalRHub.CheckSquareEventArgs e)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            var sqauares = GetPlayerSquaresById(e.PlayerId);
            var square = AllSquares.FirstOrDefault(x => x.Id == e.SquareId);
            if (square != null)
            {
                AllSquares.Remove(square);
                sqauares.Add(square);
            }
            if (IsPlayerCreator(e.PlayerId))
            {
                IsCreatorEnoughSquares = CreatorSquares.Count == 13;
            }
            else
            {
                IsSecondPlayerEnoughSquares = SecondPlayerSquares.Count == 13;
            }
        });
    }

    private void SignalRHub_OnStartPresetCreationRecieved(object? sender, string e)
    {
        CanCheckSquares = true;
    }

    private void SignalRHub_OnPresetCreationCanceledRecieved(object? sender, string e)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            MainWindow.ShowMessage(App.Current.FindResource("mes_anotherplayerca").ToString(), MessageNotificationType.Ok, okCallback: () =>
            {
                App.NewBoardModel = null;
                MainWindow.GoBack();
            });
        });
    }

    private void SignalRHub_OnPresetCreatedRecieved(object? sender, string e)
    {

        App.Current.Dispatcher.Invoke(() =>
        {
            MainWindow.ShowMessage(App.Current.FindResource("mes_presetsuccessfu").ToString(), MessageNotificationType.Ok, okCallback: () =>
            {
                App.NewBoardModel = null;
                MainWindow.GoBack();
            });
        });

    }

    private void SignalRHub_OnPlayerNotReadyRecieved(object? sender, string e)
    {
        if (IsPlayerCreator(e))
        {
            IsCreatorReady = false;
        }
        else
        {
            IsSecondPlayerReady = false;
        }
    }

    private void SignalRHub_OnPlayerReadyRecieved(object? sender, string e)
    {
        if (IsPlayerCreator(e))
        {
            IsCreatorReady = true;
        }
        else
        {
            IsSecondPlayerReady = true;
        }
    }
}
