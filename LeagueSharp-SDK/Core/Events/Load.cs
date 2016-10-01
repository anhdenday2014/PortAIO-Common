// <copyright file="Load.cs" company="LeagueSharp">
//    Copyright (c) 2015 LeagueSharp.
// 
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
// 
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
// 
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/
// </copyright>

namespace LeagueSharp.SDK
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using EloBuddy;

    

    /// <summary>
    ///     Provides an event for when the game starts.
    /// </summary>
    public static partial class Events
    {
        #region Static Fields

        /// <summary>
        ///     The invocation list.
        /// </summary>
        private static readonly List<Delegate> LoadInvocationList = new List<Delegate>();

        #endregion

        #region Public Events

        #endregion

        #region Methods

        private static void EventLoad()
        {
        }

        #endregion
    }
}