using Tasks.Service;
using Tasks.Models;
using Tasks.Constants;
using System.Windows.Input;
using Tasks.Pages;

namespace Tasks.Pages;

public partial class TasksDetailsPage : ContentPage
{

    public TaskModelMain Task { get; set; }

    public TasksDetailsPage(TaskModelMain task)
    {
        InitializeComponent();
        Task = task;
        BindingContext = this;
    }



    private async void BackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

