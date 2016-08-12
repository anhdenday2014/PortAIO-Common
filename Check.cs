using EloBuddy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
                Chat.Print("<b><font color=\"#FFFFFF\">[</font></b><b><font color=\"#3366CC\">PortAIO-Common has successfully loaded.</font></b><b><b><font color=\"#FFFFFF\">]</font></b>");
                CheckVersion();
            }
            else
            {
                isLoaded = "FAILED";
                Chat.Print("Error 1 : Invalid Key. The game will close in 5 seconds.");
                LeagueSharp.Common.Utility.DelayAction.Add(5000, () => Game.QuitGame());
            }
        }

        private static string DownloadServerVersion
        {
            get
            {
                using (var wC = new WebClient()) return wC.DownloadString("https://raw.githubusercontent.com/berbb/PortAIO-Updater/master/PortAIO.version");// example link check version
            }
        }

        public static void CheckVersion()
        {
            try
            {
                var match = new Regex(@"(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})").Match(DownloadServerVersion);

                if (!match.Success) return;
                Chat.Print("<b><font color=\"#FFFFFF\">[</font></b><b><font color=\"#3366CC\">PortAIO-Common</font></b><b><font color=\"#FFFFFF\">]</font></b> <font color=\"#FFFFFF\">You are up-to-date. Enjoy the game.</font></b>");

                var gitVersion = new System.Version($"{match.Groups[1]}.{match.Groups[2]}.{match.Groups[3]}.{match.Groups[4]}");

                if (gitVersion <= System.Reflection.Assembly.GetExecutingAssembly().GetName().Version) return;
                Chat.Print("<b><font color=\"#FFFFFF\">[</font></b><b><font color=\"#00e5e5\">PortAIO-Common</font></b><b><font color=\"#FFFFFF\">]</font></b> <font color=\"#FFFFFF\">Oudated:</font>You are using {1}, while the latest is {0}, please run the PortAIO-Updater.", gitVersion, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Chat.Print("<b><font color=\"#FFFFFF\">[</font></b><b><font color=\"#00e5e5\"> PortAIO-Common</font></b><b><font color=\"#FFFFFF\">]</font></b><b><font color=\"#FFFFFF\"> Unable to fetch latest version</font></b>");
            }
        }
    }
}
