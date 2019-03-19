using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace App2
{
    class ExtendedNavigationPage : NavigationPage
    {
        public ExtendedNavigationPage(Page page ) : base(page)
        {

        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

    }
}
