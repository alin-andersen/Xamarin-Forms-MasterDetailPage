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

        static void OnIsMasterShownPropertyChanged(BindableObject bindable, object oldValue, object newValue)
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

        #region IsRightAlignedProperty

        public static readonly BindableProperty IsRightAlignedProperty = BindableProperty.Create(
            nameof(IsRightAligned),
            typeof(bool),
            typeof(MasterDetailPage),
            false,
            propertyChanged: OnIsRightAlignedPropertyChanged);

        static void OnIsRightAlignedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var masterPage = (MasterDetailPage)bindable;
            masterPage.Update();
        }

        public bool IsRightAligned
        {
            get => (bool)GetValue(IsRightAlignedProperty);
            set => SetValue(IsRightAlignedProperty, value);
        }

        #endregion

        /// <summary>
        /// Grab page.
        /// </summary>
        readonly GrabPage grabPage = new GrabPage();

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
        /// Touch threshold for grabbing and opening the drawer.
        /// </summary>
        readonly double touchThreshold = 22.0;

        /// <summary>
        /// How much the master page has to be moved in order to 
        /// toggle when released.
        /// </summary>
        readonly double decisionFactor = 0.25;

        /// <summary>
        /// Internal lock.
        /// </summary>
        readonly object internalLock = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="masterPage"></param>
        /// <param name="detailView"></param>
        public MasterDetailPage(View masterView, Page detailPage, Page loaderPage, double openFactor = 0.8)
        {
            // Setup master page.
            masterPage = new MasterPage(masterView);
            masterPage.MasterViewWidth = App.ScreenWidth * openFactor;
            masterPage.BackgroundColor = masterView.BackgroundColor;

            DetailPage = detailPage;
            this.loaderPage = loaderPage;
            HideLoader();
            this.openFactor = openFactor;
            
            // Uncomment this to see the grab page.
            //grabPage.BackgroundColor = new Color(1.0, 0.0, 0.0, 0.2);

            // Add pages to the multi-page (order matters!).
            Children.Add(detailPage);
            Children.Add(masterPage);
            Children.Add(grabPage);
            Children.Add(loaderPage);

            CurrentPage = detailPage;

            // Subscribe to touch events.
            PropertyChanged += OnPropertyChangedEventHandler;
            grabPage.GrabEvent += OnGrabEvent;
            if(detailPage is NavigationPage navigationPage)
            {
                navigationPage.Pushed += OnPushed;
            }

            // Update master detail page.
            Update();
        }

        void OnPushed(object sender, NavigationEventArgs e)
        {
            IsMasterShown = false;
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
        /// On grab event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnGrabEvent(object sender, GrabEventArgs e)
        {
            // Get anchor based translation value.
            var anchorTranslation = GetAnchorBasedValue(e.Translation);
            Debug.WriteLine($"Anchor translation: {anchorTranslation}");

            // Get translation x value.
            var translationX = GetXTranslationValue(e.Translation);
            Debug.WriteLine($"translation X: {translationX}");

            if(e.TouchUp)
            {
                var shouldOpen = ShouldOpen(translationX);

                if(IsMasterShown == shouldOpen)
                {
                    if (shouldOpen)
                        ShowMaster();
                    else
                        ShowDetail();
                }
                else
                {
                    IsMasterShown = shouldOpen;
                }
            }
            else
            {
                // Get clipped translation value.
                var clippedTranslation = GetClippedTranslation(anchorTranslation);
                Debug.WriteLine($"Clipped translation: {clippedTranslation}");

                masterPage.TranslationX = clippedTranslation;
            }
        }

        bool ShouldOpen(double translation)
        {
            var distanceToOpened = Math.Abs(translation - GetOpenedMasterPageTranslation());
            var distanceToClosed = Math.Abs(translation - GetClosedMasterPageTranslation());

            if (distanceToOpened <= 2 * touchThreshold)
                return true;

            if (distanceToClosed <= 2 * touchThreshold)
                return false;

            return !IsMasterShown;
        }

        #region Show/hide master

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
                // Set x translation of grab page.
                grabPage.TranslationX = GetClosedGrabPageTranslation();

                //detailPage.TranslationX = -(App.ScreenWidth);
                closeAnimation = new Animation(
                    (value) => { masterPage.TranslationX = value; },
                    masterPage.TranslationX,
                    GetClosedMasterPageTranslation());

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
                // Set x translation of grab page.
                grabPage.TranslationX = GetOpenedGrabPageTranslation();

                //detailPage.TranslationX = -App.ScreenWidth + App.ScreenWidth * openFactor;
                openAnimation = new Animation(
                    (value) => { masterPage.TranslationX = value; },
                    masterPage.TranslationX,
                    GetOpenedMasterPageTranslation(),
                    Easing.SpringOut);

                // Abort running animation and commit open animation.
                masterPage.AbortAnimation("a");
                masterPage.Animate("a", openAnimation, 16, 200);
            }
        }

        #endregion

        #region Loader methods

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

        #endregion

        #region Position helper methods

        /// <summary>
        /// Get clipped translation value.
        /// </summary>
        /// <param name="translation"></param>
        /// <returns></returns>
        double GetClippedTranslation(double translation)
        {
            if(IsRightAligned)
            {
                return Clip(
                        translation,
                        GetOpenedMasterPageTranslation(),  // Max value.
                        GetClosedMasterPageTranslation()); // Min value.
            }
            else
            {
                return Clip(
                        translation,
                        GetClosedMasterPageTranslation(),  // Min value.
                        GetOpenedMasterPageTranslation()); // Max value.
            }
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
        /// Get x translation value.
        /// </summary>
        /// <param name="translation"></param>
        /// <returns></returns>
        double GetXTranslationValue(double translation)
        {
            if (IsRightAligned)
            {
                return translation + App.ScreenWidth / 2.0;
            }
            else
            {
                return translation - App.ScreenWidth / 2.0;
            }
        }

        /// <summary>
        /// Get anchor based value.
        /// </summary>
        /// <param name="translation"></param>
        /// <returns></returns>
        double GetAnchorBasedValue(double translation)
        {
            if(IsRightAligned)
            {
                return translation + (App.ScreenWidth / 2.0);
            }
            else
            {
                return translation - (App.ScreenWidth / 2.0);
            }
        }

        /// <summary>
        /// Get opened master translation.
        /// </summary>
        /// <returns></returns>
        double GetOpenedMasterPageTranslation()
        {
            if (IsRightAligned)
            {
                return  App.ScreenWidth - App.ScreenWidth * openFactor;
            }
            else
            {
                return -App.ScreenWidth + App.ScreenWidth * openFactor;
            }
        }

        /// <summary>
        /// Get opened master translation.
        /// </summary>
        /// <returns></returns>
        double GetOpenedGrabPageTranslation()
        {
            if (IsRightAligned)
            {
                return -(App.ScreenWidth) + App.ScreenWidth * (1.0 - openFactor) + touchThreshold;
            }
            else
            {
                return (App.ScreenWidth) - App.ScreenWidth * (1.0 - openFactor) - touchThreshold;
            }
        }

        /// <summary>
        /// Get closed master translation.
        /// </summary>
        /// <returns></returns>
        double GetClosedMasterPageTranslation()
        {
            if (IsRightAligned)
            {
                return +(App.ScreenWidth);
            }
            else
            {
                return -(App.ScreenWidth);
            }
        }

        /// <summary>
        /// Get closed master translation.
        /// </summary>
        /// <returns></returns>
        double GetClosedGrabPageTranslation()
        {
            if (IsRightAligned)
            {
                return GetClosedMasterPageTranslation() - touchThreshold;
            }
            else
            {
                return GetClosedMasterPageTranslation() + touchThreshold;
            }
        }

        #endregion

        /// <summary>
        /// Update master detail page.
        /// </summary>
        void Update()
        {
            // If the master detail page is right aligned, the master has to be left aligned.
            masterPage.IsLeftAligned = IsRightAligned;

            if (IsMasterShown)
            {
                grabPage.TranslationX = GetOpenedGrabPageTranslation();
                masterPage.TranslationX = GetOpenedMasterPageTranslation();
            }
            else
            {
                grabPage.TranslationX = GetClosedGrabPageTranslation();
                masterPage.TranslationX = GetClosedMasterPageTranslation();
            }
        }

        protected override Page CreateDefault(object item)
        {
            throw new NotImplementedException();
        }
    }
}
