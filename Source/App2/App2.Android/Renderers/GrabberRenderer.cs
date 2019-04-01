using Android.Content;
using Android.Util;
using Android.Views;
using App2;
using App2.Droid.Renderers;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Grabber), typeof(GrabberRenderer))]
namespace App2.Droid.Renderers
{
    /// <summary>
    /// Android implementation of the grabber.
    /// </summary>
    public class GrabberRenderer : ViewRenderer, Android.Views.View.IOnTouchListener
    {
        /// <summary>
        /// Display metrics.
        /// </summary>
        DisplayMetrics displayMetrics;

        /// <summary>
        /// Grabber.
        /// </summary>
        Grabber grabber;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        public GrabberRenderer(Context context) : base(context)
        {
            displayMetrics = Context.Resources.DisplayMetrics;
        }

        /// <summary>
        /// On element changed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            if(e.NewElement != null)
            {
                // Set native view and set touch listener.
                SetNativeControl(new Android.Views.View(Context));
                Control.SetOnTouchListener(this);

                // Shortcut reference.
                grabber = (Grabber)Element;

                // Start watch.
                watch.Start();
            }
        }

        /// <summary>
        /// Last location when touched.
        /// </summary>
        double lastTouchedLocation;

        /// <summary>
        /// Stop watch for calculating the speed.
        /// </summary>
        readonly Stopwatch watch = new Stopwatch();

        /// <summary>
        /// On touch.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool OnTouch(Android.Views.View v, MotionEvent e)
        {
            double currentTouchedLocation = TranslateToMid(e.RawX) / displayMetrics.Density;


            switch (e.Action)
            {
                case MotionEventActions.Down:

                    // Store location when the down happend.
                    lastTouchedLocation = currentTouchedLocation;

                    // Fire event of first touch.
                    grabber.RaiseGrabEvent(new GrabEventArgs(false, currentTouchedLocation));

                    // Restart watch.
                    watch.Restart();

                    break;

                case MotionEventActions.Up:

                    // Fire event when touch releases.
                    grabber.RaiseGrabEvent(new GrabEventArgs(true, currentTouchedLocation));

                    break;

                case MotionEventActions.Move:

                    watch.Stop();

                    // Fire event for movement.
                    grabber.RaiseGrabEvent(new GrabEventArgs(
                        false,
                        currentTouchedLocation,
                        CalculatedSpeed(lastTouchedLocation, currentTouchedLocation, watch)));

                    watch.Restart();

                    break;
            }

            return true;
        }
      
        /// <summary>
        /// Convert the location to mid line coordinate system.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        double TranslateToMid(double value)
        {
            double halfWidth = displayMetrics.WidthPixels / 2.0;

            double translatedToMidPosition = 0.0;

            if (value < halfWidth)
            {
                translatedToMidPosition = - (halfWidth - value);
            }
            else if(value > halfWidth)
            {
                translatedToMidPosition = value - halfWidth;
            }

            System.Diagnostics.Debug.WriteLine($"Current raw location:        {value}");
            System.Diagnostics.Debug.WriteLine($"Current translated location: {translatedToMidPosition}");

            return translatedToMidPosition;
        }

        /// <summary>
        /// Calculate current speed.
        /// </summary>
        /// <param name="lastTouchedLocation"></param>
        /// <param name="currentTouchedLocation"></param>
        /// <param name="watch"></param>
        /// <returns></returns>
        double CalculatedSpeed(double lastTouchedLocation, double currentTouchedLocation, Stopwatch watch)
        {
            if (watch.Elapsed.TotalSeconds == 0)
                return 0;

            return Math.Abs(lastTouchedLocation - currentTouchedLocation) / watch.Elapsed.TotalSeconds;
        }
    }
}