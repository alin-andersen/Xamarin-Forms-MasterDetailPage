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
    /// This is the detail page and contains the detail view.
    /// There is no way to resize a page, therefore the view inside the page is resized.
    /// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DetailPage : ContentPage
	{
		public DetailPage(View view, double detailWidth)
		{
			InitializeComponent();

            var grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition() { Width = GridLength.Star },
                    new ColumnDefinition() { Width = new GridLength(detailWidth) }
                }
            };

            grid.Children.Add(view, 1, 0);

            Content = grid;
		}
	}
}