using EloBuddy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortAIO.Common
{
    public class Init
    {

        public static string loadStatus = "UNKNOWN";

        public static string isLoaded
        {
            get
            {
                return loadStatus;
            }
            internal set
            {
                loadStatus = value;
            }
        }

        public static void LoadCommon(string a)
        {
            if (a.Equals("7E6CBFB7497BE722B8E286ECBDE88"))
            {
                isLoaded = "LOADED";
                Console.WriteLine("PortAIO-Common loaded.");
                Chat.Print("PortAIO-Common has successfully loaded.", System.Drawing.Color.Red);
            }
            else
            {
                isLoaded = "FAILED";
                Chat.Print("Error 1 : Invalid Key. The game will close in 5 seconds.");
                LeagueSharp.Common.Utility.DelayAction.Add(5000, () => Game.QuitGame());
            }
        }
    }
}
