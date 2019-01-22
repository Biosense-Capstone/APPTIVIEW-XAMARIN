using System;
using System.Collections.Generic;
using System.Text;

namespace Apptiview.Models
{
    public enum MenuItemType
    {
        Apptiview,
        About,
        Settings
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
