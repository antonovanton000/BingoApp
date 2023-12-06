using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BingoApp.ViewModels
{
    public partial class MyBaseViewModel : ObservableObject
    {

        public MyBaseViewModel()
        {
            source = new CancellationTokenSource();
        }

        protected CancellationTokenSource source;

        bool _isBusy;
        public bool IsBusy { get { return _isBusy; } set { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); } }

        string _title;

        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(nameof(Title)); } }

        bool hasValidationErrors;
        public bool HasValidationErrors
        {
            get => hasValidationErrors;
            set { hasValidationErrors = value; OnPropertyChanged(nameof(HasValidationErrors)); }
        }

        public string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
    }
}
