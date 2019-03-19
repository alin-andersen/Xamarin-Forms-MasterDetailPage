using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace App2
{
	public partial class App : Application
	{
        public static double ScreenHeight { get; set; }
        public static double ScreenWidth { get; set; }

        public App()
        {
            InitializeComponent();

            MainPage = new MasterDetailPage(
                new NavigationPage(new APage()), // Master page
                new DrawerView());              // Detail view
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
