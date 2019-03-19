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
        /// <summary>
        /// Grab page.
        /// </summary>
        readonly GrabPage grabPage;

        /// <summary>
        /// Detail page.
        /// </summary>
        readonly DetailPage detailPage;

        /// <summary>
        /// Master page.
        /// </summary>
        readonly Page masterPage;

        /// <summary>
        /// Determines how much of the screen, the detail page takes when opened.
        /// </summary>
        /// todo: use this
        readonly double openFactor;

        /// <summary>
        /// Master page shown.
        /// </summary>
        bool masterPageShown = true;

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
        public MasterDetailPage(Page masterPage, View detailView, double openFactor = 0.8)
        {
            // setup pages
            this.masterPage = new ExtendedNavigationPage(masterPage);
            this.openFactor = openFactor;

            detailPage = new DetailPage(detailView, App.ScreenWidth * openFactor);
            detailPage.TranslationX = -(App.ScreenWidth);

            grabPage = new GrabPage()
            {
                // Note: Uncomment this to see and understand the grab page.
                //BackgroundColor = new Color(1.0, 0.0, 0.0, 0.2),
            };
            grabPage.TranslationX = -(App.ScreenWidth) + touchThreshold;

            // add pages to multi page
            Children.Add(masterPage);
            Children.Add(detailPage);
            Children.Add(grabPage);

            // wire event
            grabPage.GrabEvent += OnGrabEvent;
        }

        /// <summary>
        /// On grab event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGrabEvent(object sender, GrabEventArgs e)
        {
            double clippedTranslation = Clip(
                -(App.ScreenWidth) + App.ScreenWidth / 2 + e.Translation,
                -App.ScreenWidth,
                -App.ScreenWidth + App.ScreenWidth * openFactor);

            if(e.TouchUp)
            {
                if (masterPageShown)
                {
                    if (e.Translation >= -(App.ScreenWidth / 5.0))
                    {
                        ShowDetail();
                    }
                    else
                    {
                        ShowMaster();
                    }
                }
                else
                {
                    if (e.Translation < App.ScreenWidth / 4.0)
                    {
                        ShowMaster();
                    }
                    else
                    {
                        ShowDetail();
                    }
                }
            }

            detailPage.TranslationX = clippedTranslation;
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
        /// Show master.
        /// </summary>
        void ShowMaster()
        {
            lock (internalLock)
            {
                masterPageShown = true;

                grabPage.TranslationX = -(App.ScreenWidth) + touchThreshold;

                //detailPage.TranslationX = -(App.ScreenWidth);
                closeAnimation = new Animation(
                    (value) => { detailPage.TranslationX = value; },
                    detailPage.TranslationX,
                    -(App.ScreenWidth));

                // Abort running animation and commit close animation.
                detailPage.AbortAnimation("a");
                closeAnimation.Commit(detailPage, "a", 16, 200, Easing.CubicOut);
            }
        }

        /// <summary>
        /// Show detail.
        /// </summary>
        void ShowDetail()
        {
            lock (internalLock)
            {
                masterPageShown = false;

                grabPage.TranslationX = App.ScreenWidth - App.ScreenWidth * (1.0 - openFactor) - touchThreshold;

                //detailPage.TranslationX = -App.ScreenWidth + App.ScreenWidth * openFactor;
                openAnimation = new Animation(
                    (value) => { detailPage.TranslationX = value; },
                    detailPage.TranslationX,
                    -App.ScreenWidth + App.ScreenWidth * openFactor,
                    Easing.SpringOut);

                // Abort running animation and commit open animation.
                detailPage.AbortAnimation("a");
                detailPage.Animate("a", openAnimation, 16, 200);
            }
        }

        /// <summary>
        /// Toggle menu.
        /// </summary>
        public void ToggleMenu()
        {
            lock (internalLock)
            {
                if (masterPageShown) ShowDetail();
                else ShowMaster();
            }
        }

        protected override Page CreateDefault(object item)
        {
            throw new NotImplementedException();
        }
    }
}
