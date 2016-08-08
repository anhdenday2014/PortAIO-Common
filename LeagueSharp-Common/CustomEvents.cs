#region LICENSE

/*
 Copyright 2014 - 2014 LeagueSharp
 CustomEvents.cs is part of LeagueSharp.Common.
 
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

namespace LeagueSharp.Common
{
    using EloBuddy;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Provides custom events.
    /// </summary>
    public static class CustomEvents
    {
        /// <summary>
        ///     Provides custom events regarding the game.
        /// </summary>
        public class Game
        {
            #region Static Fields

            /// <summary>
            ///     The nexus list
            /// </summary>
            private static readonly List<Obj_HQ> NexusList = new List<Obj_HQ>();

            /// <summary>
            ///     The notified subscribers
            /// </summary>
            private static readonly List<Delegate> NotifiedSubscribers = new List<Delegate>();

            /// <summary>
            ///     The end game called
            /// </summary>
            private static bool _endGameCalled;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes static members of the <see cref="Game" /> class.
            /// </summary>
            static Game()
            {
                Utility.DelayAction.Add(0, Initialize);
            }

            #endregion

            #region Delegates

            /// <summary>
            ///     The delegate for <see cref="Game.OnGameEnd" />
            /// </summary>
            /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
            public delegate void OnGameEnded(EventArgs args);

            /// <summary>
            ///     The delegate for <see cref="Game.OnGameLoad" />
            /// </summary>
            /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
            public delegate void OnGameLoaded(EventArgs args);

            #endregion

            #region Public Events

            /// <summary>
            ///     Occurs when the game ends. This is meant as a better replacement to <see cref="LeagueSharp.Game.OnEnd" />.
            /// </summary>
            public static event OnGameEnded OnGameEnd;

            /// <summary>
            ///     Occurs when the game loads. This will be fired if the game is already loaded.
            /// </summary>
            public static event OnGameLoaded OnGameLoad;

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     Initializes this instance.
            /// </summary>
            public static void Initialize()
            {
                foreach (var hq in ObjectManager.Get<Obj_HQ>().Where(hq => hq.IsValid))
                {
                    NexusList.Add(hq);
                }

                if (EloBuddy.Game.Mode == GameMode.Running)
                {
                    //Otherwise the .ctor didn't return yet and no callback will occur
                    Utility.DelayAction.Add(500, () => { Game_OnGameStart(new EventArgs()); });
                }
                else
                {
                    EloBuddy.Game.OnLoad += Game_OnGameStart;
                }
            }

            #endregion

            #region Methods

            /// <summary>
            ///     Fired when the game is started.
            /// </summary>
            /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
            private static void Game_OnGameStart(EventArgs args)
            {
                EloBuddy.Game.OnUpdate += Game_OnGameUpdate;

                if (OnGameLoad != null)
                {
                    foreach (
                        var subscriber in OnGameLoad.GetInvocationList().Where(s => !NotifiedSubscribers.Contains(s)))
                    {
                        NotifiedSubscribers.Add(subscriber);
                        try
                        {
                            subscriber.DynamicInvoke(new EventArgs());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }

            /// <summary>
            ///     Fired when the game updates.
            /// </summary>
            /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
            private static void Game_OnGameUpdate(EventArgs args)
            {
                if (OnGameLoad != null)
                {
                    foreach (
                        var subscriber in OnGameLoad.GetInvocationList().Where(s => !NotifiedSubscribers.Contains(s)))
                    {
                        NotifiedSubscribers.Add(subscriber);
                        try
                        {
                            subscriber.DynamicInvoke(new EventArgs());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }

                if (NexusList.Count == 0 || _endGameCalled)
                {
                    return;
                }

                foreach (var nexus in NexusList)
                {
                    if (nexus != null && nexus.IsValid && nexus.Health <= 0)
                    {
                        if (OnGameEnd != null)
                        {
                            OnGameEnd(new EventArgs());
                            _endGameCalled = true; // Don't spam the event.
                        }
                    }
                }
            }

            #endregion
        }

        /// <summary>
        ///     Provides custom events regarding units.
        /// </summary>
        public class Unit
        {
            #region Constructors and Destructors

            /// <summary>
            ///     Initializes static members of the <see cref="Unit" /> class.
            /// </summary>
            static Unit()
            {
                //Packet. += PacketHandler;

                //Initializes ondash class:
                ObjectManager.Player.LSIsDashing();
            }

            #endregion

            #region Delegates

            /// <summary>
            ///     The delegate for <see cref="Unit.OnDash" />
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="args">The arguments.</param>
            public delegate void OnDashed(Obj_AI_Base sender, Dash.DashItem args);

            /// <summary>
            ///     The delegate for <see cref="Unit.OnLevelUp" />
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="args">The <see cref="OnLevelUpEventArgs" /> instance containing the event data.</param>
            public delegate void OnLeveledUp(Obj_AI_Base sender, OnLevelUpEventArgs args);

            /// <summary>
            ///     The delegate for <see cref="Unit.OnLevelUpSpell" />
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="args">The <see cref="OnLevelUpSpellEventArgs" /> instance containing the event data.</param>
            public delegate void OnLeveledUpSpell(Obj_AI_Base sender, OnLevelUpSpellEventArgs args);

            #endregion

            #region Public Events

            /// <summary>
            ///     Occurs when a unit dashes.
            /// </summary>
            public static event OnDashed OnDash;

            /// <summary>
            ///     Occurs when a unit levels up.
            /// </summary>
            public static event OnLeveledUp OnLevelUp;

            /// <summary>
            ///     Occurs when the player levels up a spell.
            /// </summary>
            public static event OnLeveledUpSpell OnLevelUpSpell;

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     Triggers the on dash.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="args">The arguments.</param>
            public static void TriggerOnDash(Obj_AI_Base sender, Dash.DashItem args)
            {
                var dashHandler = OnDash;
                if (dashHandler != null)
                {
                    dashHandler(sender, args);
                }
            }

            #endregion

            #region Methods

            /// <summary>
            ///     Handles packets.
            /// </summary>
            /// <param name="args">The <see cref="GamePacketEventArgs" /> instance containing the event data.</param>
            private static void PacketHandler(GamePacketEventArgs args)
            {
            }

            #endregion

            /// <summary>
            ///     The event arguments for the <see cref="Unit.OnLevelUp" /> event.
            /// </summary>
            public class OnLevelUpEventArgs : EventArgs
            {
                #region Fields

                /// <summary>
                ///     The new level
                /// </summary>
                public int NewLevel;

                /// <summary>
                ///     The remaining points
                /// </summary>
                public int RemainingPoints;

                #endregion
            }

            /// <summary>
            ///     The event arguments for the <see cref="Unit.OnLevelUpSpell" /> event.
            /// </summary>
            public class OnLevelUpSpellEventArgs : EventArgs
            {
                #region Fields

                /// <summary>
                ///     The remainingpoints
                /// </summary>
                public int Remainingpoints;

                /// <summary>
                ///     The spell identifier
                /// </summary>
                public int SpellId;

                /// <summary>
                ///     The spell level
                /// </summary>
                public int SpellLevel;

                #endregion

                #region Constructors and Destructors

                /// <summary>
                ///     Initializes a new instance of the <see cref="OnLevelUpSpellEventArgs" /> class.
                /// </summary>
                internal OnLevelUpSpellEventArgs()
                {
                }

                #endregion
            }
        }
    }
}