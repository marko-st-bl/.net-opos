using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Zadatak2
{
    public static class Utilities
    {
        public static Visibility ToVisibility(this bool isVisible) => isVisible ? Visibility.Visible : Visibility.Collapsed;
    }
}
