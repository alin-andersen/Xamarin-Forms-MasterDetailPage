using System;
using System.ComponentModel;
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
        public readonly Page DetailPage;

        /// <summary>
        /// Loader page.
        /// </summary>
        readonly Page loaderPage;

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
        /// How much the master page has to be moved in order to 
        /// toggle when released.
        /// </summary>
        double decisionFactor = 0.25;

        /// <summary>
        /// Internal lock.
        /// </summary>
        readonly object internalLock = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="masterPage"></param>
        /// <param name="detailView"></param>
        public MasterDetailPage(View masterView, Page detailPage, Page loaderPage, double openFactor = 0.8, bool menuOnRightSide = false)
        {
            // Setup master page.
            masterPage = new MasterPage(masterView);
            masterPage.MasterViewWidth = App.ScreenWidth * openFactor;
            masterPage.TranslationX = menuOnRightSide ? App.ScreenWidth : -(App.ScreenWidth);
            masterPage.BackgroundColor = masterView.BackgroundColor;

            // Copy arguments.
            this.DetailPage = detailPage;
            this.loaderPage = loaderPage;
            HideLoader();

            this.openFactor = openFactor;
            this.menuOnRightSide = menuOnRightSide;

            // Setup grab page.
            grabPage = new GrabPage()
            {
                // Note: Uncomment this to see and understand the grab page.
                BackgroundColor = new Color(1.0, 0.0, 0.0, 0.2),
                TranslationX = menuOnRightSide ? + App.ScreenWidth - touchThreshold : - App.ScreenWidth + touchThreshold,
            };

            // Add pages to the multi-page (order matters!).
            Children.Add(detailPage);
            Children.Add(masterPage);
            Children.Add(grabPage);
            Children.Add(loaderPage);

            CurrentPage = detailPage;

            // Subscribe to touch events.
            PropertyChanged += OnPropertyChangedEventHandler;
            grabPage.GrabEvent += OnGrabEvent;
        }

        /// <summary>
        /// Called when property changed fires.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IsBusy))
            {
                if(IsBusy)
                {
                    ShowLoader();
                }
                else
                {
                    HideLoader();
                }
            }
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
            // Get clipped translation.
            var clippedTranslation = GetClippedTranslation(e.Translation);

            // Get translated to master page anchor translation value.
            var translatedToMasterPageAnchor = clippedTranslation - App.ScreenWidth / 2.0;

            // Check if released event fired.
            if(e.TouchUp)
            {
                if (detailPageShown)
                {
                    IsMasterShown = ShouldOpen(translatedToMasterPageAnchor);
                }
                else
                {
                    IsMasterShown = !ShouldClose(translatedToMasterPageAnchor);
                }
            }
            else
            {
                masterPage.TranslationX = translatedToMasterPageAnchor;
            }
        }

        /// <summary>
        /// Get clipped translation value.
        /// </summary>
        /// <param name="translation"></param>
        /// <returns></returns>
        double GetClippedTranslation(double translation)
        {
            return Clip(translation, -
                App.ScreenWidth / 2.0, // Min value.
                App.ScreenWidth * openFactor - App.ScreenWidth / 2.0); // Max value.
        }

        /// <summary>
        /// Simple clip method.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        double Clip(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Animation for opening the drawer menu.
        /// </summary>
        Animation openAnimation;

        /// <summary>
        /// Animation for closing the drawer menu.
        /// </summary>
        Animation closeAnimation;

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
                    GetClosedMasterTranslation());

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
                    GetOpenedMasterTranslation(),
                    Easing.SpringOut);

                // Abort running animation and commit open animation.
                masterPage.AbortAnimation("a");
                masterPage.Animate("a", openAnimation, 16, 200);
            }
        }

        /// <summary>
        /// Show loader.
        /// </summary>
        void ShowLoader()
        {
            lock (internalLock)
            {
                if(loaderPage != null)
                    loaderPage.TranslationX = 0;
            }
        }

        /// <summary>
        /// Hide loader.
        /// </summary>
        void HideLoader()
        {
            lock (internalLock)
            {
                if (loaderPage != null)
                    loaderPage.TranslationX = App.ScreenWidth + 100.0;
            }
        }

        #region Helper methods

        /// <summary>
        /// Get opened master translation.
        /// </summary>
        /// <returns></returns>
        double GetOpenedMasterTranslation()
        {
            return menuOnRightSide ? 
                + (App.ScreenWidth / 2.0) - App.ScreenWidth * openFactor + (App.ScreenWidth / 2.0) : 
                - (App.ScreenWidth / 2.0) + App.ScreenWidth * openFactor - (App.ScreenWidth / 2.0);
        }

        /// <summary>
        /// Get closed master translation.
        /// </summary>
        /// <returns></returns>
        double GetClosedMasterTranslation()
        {
            return menuOnRightSide ? 
                +(App.ScreenWidth) : 
                -(App.ScreenWidth);
        }

        /// <summary>
        /// Decides if should open.
        /// </summary>
        /// <param name="translationValue"></param>
        /// <returns></returns>
        bool ShouldOpen(double translationValue)
        {
            if(!menuOnRightSide)
            {
                return -(App.ScreenWidth / 2.0) + GetMasterOpenWidth() * decisionFactor >= translationValue;
            }
            else
            {
                return +(App.ScreenWidth / 2.0) - GetMasterOpenWidth() * decisionFactor <= translationValue;
            }
        }

        /// <summary>
        /// Decides if should close.
        /// </summary>
        /// <param name="translationValue"></param>
        /// <returns></returns>
        bool ShouldClose(double translationValue)
        {
            if (!menuOnRightSide)
            {
                return -(App.ScreenWidth / 2.0) + GetMasterOpenWidth() * (1.0 - decisionFactor) >= translationValue;
            }
            else
            {
                return +(App.ScreenWidth / 2.0) - GetMasterOpenWidth() * (1.0 - decisionFactor) <= translationValue;
            }
        }

        /// <summary>
        /// Get master view open width.
        /// </summary>
        /// <returns></returns>
        double GetMasterOpenWidth()
        {
            return App.ScreenWidth * openFactor;
        }

        #endregion

        protected override Page CreateDefault(object item)
        {
            throw new NotImplementedException();
        }
    }
}
