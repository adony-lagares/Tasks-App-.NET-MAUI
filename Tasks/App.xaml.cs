namespace Tasks
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell(); //page is loaded in another tab
            /*MainPage = new NavigationPage(new AppShell());*/ //page is loaded on the same page
        }
    }
}