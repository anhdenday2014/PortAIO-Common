#region LICENSE

/*
 Copyright 2014 - 2014 LeagueSharp
 Config.cs is part of LeagueSharp.Common.
 
 LeagueSharp.Common is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 LeagueSharp.Common is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with LeagueSharp.Common. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using EloBuddy;
#endregion

namespace LeagueSharp.Common
{
    using System.Globalization;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Gets information about the L# system.
    /// </summary>
    public static class Config
    {
        static Config()
        {
            //        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            //        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            //        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            //        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        private static string _appDataDirectory;

        public static string AppDataDirectory
        {
            get
            {
                if (_appDataDirectory == null)
                {
                    _appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EloBuddy");
                }

                return _appDataDirectory;
            }
        }

        /// <summary>
        /// The show menu hotkey
        /// </summary>
        private static byte _showMenuHotkey;

        /// <summary>
        /// The show menu toggle hotkey
        /// </summary>
        private static byte _showMenuToggleHotkey;

        /// <summary>
        /// Gets the show menu press key.
        /// </summary>
        /// <value>
        /// The show menu press key.
        /// </value>
        public static byte ShowMenuPressKey
        {
            get
            {
                if (_showMenuHotkey == 0)
                {
                    try
                    {
                        _showMenuHotkey = (byte)SandboxConfig.MenuKey;
                        _showMenuHotkey = Utils.FixVirtualKey(_showMenuHotkey);
                        Console.WriteLine(@"Menu press key set to {0}", _showMenuHotkey);
                    }
                    catch
                    {
                        _showMenuHotkey = 16;
                        Console.WriteLine(@"Could not get the menu press key");
                    }
                }

                return _showMenuHotkey;
            }
        }

        /// <summary>
        /// Gets the show menu toggle key.
        /// </summary>
        /// <value>
        /// The show menu toggle key.
        /// </value>
        public static byte ShowMenuToggleKey
        {
            get
            {
                if (_showMenuToggleHotkey == 0)
                {
                    try
                    {
                        _showMenuToggleHotkey = (byte)SandboxConfig.MenuToggleKey;
                        _showMenuToggleHotkey = _showMenuToggleHotkey == 0 ? (byte)120 : _showMenuToggleHotkey;
                        _showMenuToggleHotkey = Utils.FixVirtualKey(_showMenuToggleHotkey);
                        Console.WriteLine(@"Menu toggle key set to {0}", _showMenuToggleHotkey);
                    }
                    catch
                    {
                        _showMenuToggleHotkey = 120;
                        Console.WriteLine(@"Could not get the menu toggle key");
                    }
                }
                return _showMenuToggleHotkey;
            }
        }
    }
}
