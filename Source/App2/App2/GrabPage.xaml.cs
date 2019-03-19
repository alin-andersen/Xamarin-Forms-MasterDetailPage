using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2
{
    /// <summary>
    /// The grab page contains the grabber view.
    /// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GrabPage : ContentPage
	{
        public event EventHandler<GrabEventArgs> GrabEvent = delegate { };

        public GrabPage ()
		{
			InitializeComponent();
            grabber.GrabEvent += (sender, args) => GrabEvent(sender, args);
		}
	}
}