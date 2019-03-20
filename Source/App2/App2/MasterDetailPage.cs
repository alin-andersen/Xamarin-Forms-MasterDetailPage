using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace App2
{
    /// <summary>
    /// This is the implementation of the alternative MasterDetailPage.
    /// </summary>
    public class MasterDetailPage : MultiPage<Page>
    {
        #region IsMasterShown Property

        public static readonly BindableProperty IsMasterShownProperty = BindableProperty.Create(
            nameof(IsMasterShown),
            typeof(bool),
            typeof(MasterDetailPage),
            false,
            propertyChanged: OnIsMasterShownPropertyChanged);

        private static void OnIsMasterShownPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var masterPage = (MasterDetailPage)bindable;

            if((bool)newValue)
            {
                masterPage.ShowMaster();
            }
            else
            {
                masterPage.ShowDetail();
            }
        }

        public bool IsMasterShown
        {
            get => (bool)GetValue(IsMasterShownProperty);
            set => SetValue(IsMasterShownProperty, value);
        }

        #endregion

        /// <summary>
        /// Grab page.
        /// </summary>
        readonly GrabPage grabPage;

        /// <summary>
        /// Detail page.
        /// </summary>
        readonly MasterPage masterPage;

        /// <summary>
        /// Master page.
        /// </summary>
        readonly Page detailPage;

        /// <summary>
        /// Determines how much of the screen, the detail page takes when opened.
        /// </summary>
        /// todo: use this
        readonly double openFactor;

        /// <summary>
        /// Side to open the menu.
        /// </summary>
        /// todo: Use this!
        readonly bool menuOnRightSide;

        /// <summary>
        /// Current state of the master detail page.
        /// If true, the detail page is shown.
        /// If false, the master page is shown (the menu is visible).
        /// </summary>
        bool detailPageShown = true;

        /// <summary>
        /// Touch threshold for grabbing and opening the drawer.
        /// </summary>
        double touchThreshold = 22.0;

        /// <summary>
        /// Animation for opening the drawer menu.
        /// </summary>
        Animation openAnimation;

        /// <summary>
        /// Animation for closing the drawer menu.
        /// </summary>
        Animation closeAnimation;

        /// <summary>
        /// Internal lock.
        /// </summary>
        readonly object internalLock = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="masterPage"></param>
        /// <param name="detailView"></param>
        public MasterDetailPage(View masterView, Page detailPage, double openFactor = 0.8, bool menuOnRightSide = false)
        {
            // Set pages.
            masterPage = new MasterPage(masterView);
            masterPage.MasterViewWidth = App.ScreenWidth * openFactor;
            masterPage.TranslationX = -(App.ScreenWidth);
            this.detailPage = detailPage;
            this.openFactor = openFactor;
            this.menuOnRightSide = menuOnRightSide;

            grabPage = new GrabPage()
            {
                // Note: Uncomment this to see and understand the grab page.
                //BackgroundColor = new Color(1.0, 0.0, 0.0, 0.2),
            };
            grabPage.TranslationX = -(App.ScreenWidth) + touchThreshold;

            // Add pages to the multi page.
            Children.Add(detailPage);
            Children.Add(masterPage);
            Children.Add(grabPage);
            CurrentPage = detailPage;

            // Lastly subscribe to events.

            // Subscribe to touch events.
            grabPage.GrabEvent += OnGrabEvent;
        }

        /// <summary>
        /// Called when the navigation page raises pushed or popped event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnNavigationPagePushedOrPopped(object sender, NavigationEventArgs e)
        {
            IsMasterShown = false;
        }

        /// <summary>
        /// On grab event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnGrabEvent(object sender, GrabEventArgs e)
        {
            double clippedTranslation = Clip(
                -(App.ScreenWidth) + App.ScreenWidth / 2 + e.Translation,
                -App.ScreenWidth,
                -App.ScreenWidth + App.ScreenWidth * openFactor);

            if(e.TouchUp)
            {
                if (detailPageShown)
                {
                    if (e.Translation >= -(App.ScreenWidth / 5.0))
                    {
                        IsMasterShown = true;
                    }
                    else
                    {
                        IsMasterShown = false;
                    }
                }
                else
                {
                    if (e.Translation < App.ScreenWidth / 4.0)
                    {
                        IsMasterShown = false;
                    }
                    else
                    {
                        IsMasterShown = true;
                    }
                }
            }

            masterPage.TranslationX = clippedTranslation;
        }

        private double Clip(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        /// <summary>
        /// Show detail.
        /// </summary>
        void ShowDetail()
        {
            lock (internalLock)
            {
                detailPageShown = true;

                grabPage.TranslationX = -(App.ScreenWidth) + touchThreshold;

                //detailPage.TranslationX = -(App.ScreenWidth);
                closeAnimation = new Animation(
                    (value) => { masterPage.TranslationX = value; },
                    masterPage.TranslationX,
                    -(App.ScreenWidth));

                // Abort running animation and commit close animation.
                masterPage.AbortAnimation("a");
                masterPage.Animate("a", closeAnimation, 16, 200, Easing.CubicOut);
            }
        }

        /// <summary>
        /// Show master.
        /// </summary>
        void ShowMaster()
        {
            lock (internalLock)
            {
                detailPageShown = false;

                grabPage.TranslationX = App.ScreenWidth - App.ScreenWidth * (1.0 - openFactor) - touchThreshold;

                //detailPage.TranslationX = -App.ScreenWidth + App.ScreenWidth * openFactor;
                openAnimation = new Animation(
                    (value) => { masterPage.TranslationX = value; },
                    masterPage.TranslationX,
                    -App.ScreenWidth + App.ScreenWidth * openFactor,
                    Easing.CubicOut);

                // Abort running animation and commit open animation.
                masterPage.AbortAnimation("a");
                masterPage.Animate("a", openAnimation, 16, 200);
            }
        }

        protected override Page CreateDefault(object item)
        {
            throw new NotImplementedException();
        }
    }
}
