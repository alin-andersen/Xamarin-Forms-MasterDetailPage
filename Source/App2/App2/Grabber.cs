using System;
using Xamarin.Forms;

namespace App2
{
    /// <summary>
    /// Grabber is the view inside the grab page.
    /// It is responsible for the underlying events on each platform.
    /// </summary>
    public class Grabber : View
    {
        public event EventHandler<GrabEventArgs> GrabEvent = delegate { };

        public void RaiseGrabEvent(GrabEventArgs args)
        {
            GrabEvent(this, args);
        }
    }

    /// <summary>
    /// Grab events.
    /// </summary>
    public class GrabEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public bool TouchUp { get; }

        /// <summary>
        /// XTranslation.
        /// </summary>
        public double Translation { get; }

        /// <summary>
        /// Speed of the translation.
        /// </summary>
        public double Speed { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="showDetail"></param>
        /// <param name="showMaster"></param>
        /// <param name="xTranslation"></param>
        public GrabEventArgs(bool touchUp, double translation, double speed = 0.0)
        {
            TouchUp = touchUp;
            Translation = translation;
            Speed = speed;
        }
    }
}
