
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
            masterPage.Update();
        }

        public double MasterViewWidth
        {
            get => (double)GetValue(MasterViewWidthProperty);
            set => SetValue(MasterViewWidthProperty, value);
        }

        #endregion

        #region IsLeftAlignedProperty

        public static readonly BindableProperty IsLeftAlignedProperty = BindableProperty.Create(
            nameof(IsLeftAligned),
            typeof(bool),
            typeof(MasterPage),
            false,
            propertyChanged: OnIsLeftAlignedPropertyChanged);

        static void OnIsLeftAlignedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var masterPage = (MasterPage)bindable;
            masterPage.Update();
        }

        public bool IsLeftAligned
        {
            get => (bool)GetValue(IsLeftAlignedProperty);
            set => SetValue(IsLeftAlignedProperty, value);
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="masterView">Master view.</param>
        public MasterPage(View masterView)
		{
			InitializeComponent();
            masterViewHolder.Content = masterView;
            Update();
		}

        /// <summary>
        /// Update master page.
        /// </summary>
        void Update()
        {
            if(IsLeftAligned)
            {
                leftColumnDefinition.Width = 0;
                centerColumnDefinition.Width = MasterViewWidth;
                rightColumnDefinition.Width = GridLength.Star;
            }
            else
            {
                leftColumnDefinition.Width = GridLength.Star;
                centerColumnDefinition.Width = MasterViewWidth;
                rightColumnDefinition.Width = 0;
            }
        }
	}
}