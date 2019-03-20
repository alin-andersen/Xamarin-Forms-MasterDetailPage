
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2
{
    /// <summary>
    /// This is the detail page and contains the detail view.
    /// There is no way to resize a page, therefore the view inside the page is resized.
    /// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MasterPage : ContentPage
	{
        #region MasterViewWidth Property

        public static readonly BindableProperty MasterViewWidthProperty = BindableProperty.Create(
            nameof(MasterViewWidth),
            typeof(double),
            typeof(MasterPage),
            0.0,
            propertyChanged: OnMasterViewWidthPropertyChanged);

        private static void OnMasterViewWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var masterPage = (MasterPage)bindable;
            masterPage.masterColumnDefinition.Width = (double)newValue;
        }

        public double MasterViewWidth
        {
            get => (double)GetValue(MasterViewWidthProperty);
            set => SetValue(MasterViewWidthProperty, value);
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="masterView">Master view.</param>
        public MasterPage(View masterView)
		{
			InitializeComponent();

            masterColumnDefinition.Width = MasterViewWidth;
            masterViewContainer.Children.Add(masterView);
		}
	}
}