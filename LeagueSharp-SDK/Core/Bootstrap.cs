// <copyright file="Bootstrap.cs" company="LeagueSharp">
//    Copyright (c) 2015 LeagueSharp.
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/
// </copyright>

namespace LeagueSharp.SDK
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Threading;

    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.UI.Skins;
    using LeagueSharp.SDK.Utils;

    /// <summary>
    ///     Bootstrap is an initialization pointer for the AppDomainManager to initialize the library correctly once loaded in
    ///     game.
    /// </summary>
    
    public class Bootstrap
    {
        #region Static Fields

        /// <summary>
        ///     Indicates whether the bootstrap has been initialized.
        /// </summary>
        private static bool initialized;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Initializes the whole SDK. It is safe to call in your code at any point.
        /// </summary>
        /// <param name="args">Not currently used or needed.</param>
        /// <returns>true if SDK is loaded, false if it is not</returns>
 //       [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static bool Init()
        {
            if (initialized)
            {
                return true;
            }

            initialized = true;

            // Initial notification.
            //logger.Info("SDKEx Loading");

            // Load Resource Content.
            ResourceLoader.Initialize();
            //logger.Info("Resources Initialized.");

            // Load GameObjects.
            GameObjects.Initialize();
            //logger.Info("GameObjects Initialized.");

            // Create L# menu
            Variables.LeagueSharpMenu = new Menu("LeagueSharp", "LeagueSharp", true).Attach();
            MenuCustomizer.Initialize(Variables.LeagueSharpMenu);
            //logger.Info("LeagueSharp Menu Created.");

            // Load the Orbwalker
            Variables.Orbwalker = new Orbwalker(Variables.LeagueSharpMenu);
            //logger.Info("Orbwalker Initialized.");

            // Load the TargetSelector.
            Variables.TargetSelector = new TargetSelector(Variables.LeagueSharpMenu);
            //logger.Info("TargetSelector Initialized.");

            // Load the Notifications
            Notifications.Initialize(Variables.LeagueSharpMenu);
            //logger.Info("Notifications Initialized.");

            // Load the ThemeManager
            ThemeManager.Initialize(Variables.LeagueSharpMenu);
            //logger.Info("ThemeManager Initialized.");

            // Load Damages.
            Damage.Initialize();
            //logger.Info("Damage Library Initialized.");

            // Tell the developer everything succeeded
            return initialized;
        }

        #endregion
    }
}