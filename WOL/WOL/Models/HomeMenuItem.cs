using System;

namespace WOL.Models
{
    public enum MenuType
    {
        About,
        Devices        
    }
    public class HomeMenuItem : BaseModel
    {
        public HomeMenuItem()
        {
            MenuType = MenuType.About;
        }
        public string Icon { get; set; }
        public MenuType MenuType { get; set; }
    }
}

