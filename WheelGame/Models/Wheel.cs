using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WheelGame.Models;

public class Wheel : ObservableObject
{
    public Wheel() { }
    public ObservableCollection<Objective> Objectives { get; set; } = new ();
}
