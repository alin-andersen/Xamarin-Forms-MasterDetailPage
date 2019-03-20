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

        public static MasterDetailPage MasterDetailPage { get; private set; }

        public App()
        {
            InitializeComponent();

            var detailPage = new TestDetailPage();
            MasterDetailPage = new MasterDetailPage(
                new TestMasterView(),
                detailPage);
            MainPage = new NavigationPage(MasterDetailPage);
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
