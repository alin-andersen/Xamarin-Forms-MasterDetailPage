
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TestMasterView : ContentView
	{
		public TestMasterView()
		{
			InitializeComponent ();
            BindingContext = new TestMasterViewModel();
		}
	}

    public class TestMasterViewModel
    {
        /// <summary>
        /// On menu toggle command.
        /// </summary>
        public ICommand OnShowDetailCommand { get; }

        /// <summary>
        /// Show loader command.
        /// </summary>
        public ICommand OnShowLoader { get; }

        public ICommand OnOpenOtherPageCommand { get; }

        public TestMasterViewModel()
        {
            OnShowDetailCommand = new Command(() =>
            {
                var masterDetailPage = App.MasterDetailPage;
                masterDetailPage.IsMasterShown = false;
            });

            OnShowLoader = new Command(async () =>
            {
                GetMasterDetailPage().IsBusy = true;
                await Task.Delay(3000);
                GetMasterDetailPage().IsBusy = false;
            });

            OnOpenOtherPageCommand = new Command(async () =>
            {
                await ((NavigationPage)GetMasterDetailPage().DetailPage).PushAsync(new TestDetailPage { Title = DateTime.UtcNow.ToLongTimeString() });
            });
        }

        MasterDetailPage GetMasterDetailPage()
        {
            return (MasterDetailPage)Application.Current.MainPage;
        }
    }
}