// <copyright file="DefaultTheme.cs" company="LeagueSharp">
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

namespace LeagueSharp.SDK.UI.Skins.Default
{
    using System.Linq;

    using SharpDX;
    using SharpDX.Direct3D9;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Implements a default ITheme.
    /// </summary>
    /// 
    [Serializable()]
    [KnownType(typeof(DefaultTheme))]
    public class DefaultTheme : ITheme
    {
        #region Static Fields

        /// <summary>
        ///     The line.
        /// </summary>
        private static readonly Line Line = new Line(EloBuddy.Drawing.Direct3DDevice) { GLLines = true, Width = 1 };

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Builds a new handler for the given <see cref="MenuBool" />.
        /// </summary>
        /// <param name="component">The <see cref="MenuBool" /> where this handler is responsible for.</param>
        /// <returns>The handler</returns>
        public ADrawable<MenuBool> BuildBoolHandler(MenuBool component)
        {
            return new DefaultBool(component);
        }

        /// <summary>
        ///     Builds a new handler for the given <see cref="MenuButton" />.
        /// </summary>
        /// <param name="component">The <see cref="MenuButton" /> where this handler is responsible for.</param>
        /// <returns>The handler</returns>
        public ADrawable<MenuButton> BuildButtonHandler(MenuButton component)
        {
            return new DefaultButton(component);
        }

        /// <summary>
        ///     Builds a new handler for the given <see cref="MenuColor" />.
        /// </summary>
        /// <param name="component">The <see cref="MenuColor" /> where this handler is responsible for.</param>
        /// <returns>The handler</returns>
        public ADrawable<MenuColor> BuildColorHandler(MenuColor component)
        {
            return new DefaultColorPicker(component);
        }

        /// <summary>
        ///     Builds a new handler for the given <see cref="MenuKeyBind" />.
        /// </summary>
        /// <param name="component">The <see cref="MenuKeyBind" /> where this handler is responsible for.</param>
        /// <returns>The handler</returns>
        public ADrawable<MenuKeyBind> BuildKeyBindHandler(MenuKeyBind component)
        {
            return new DefaultKeyBind(component);
        }

        /// <summary>
        ///     Builds a new handler for the given <see cref="MenuList" />.
        /// </summary>
        /// <param name="component">The <see cref="MenuList" /> where this handler is responsible for.</param>
        /// <returns>The handler</returns>
        public ADrawable<MenuList> BuildListHandler(MenuList component)
        {
            return new DefaultList(component);
        }

        /// <summary>
        ///     Builds a new handler for the given <see cref="Menu" />.
        /// </summary>
        /// <param name="menu">The <see cref="Menu" /> where this handler is responsible for.</param>
        /// <returns>The handler</returns>
        public ADrawable<Menu> BuildMenuHandler(Menu menu)
        {
            return new DefaultMenu(menu);
        }

        /// <summary>
        ///     Builds a new handler for the given <see cref="MenuSeparator" />.
        /// </summary>
        /// <param name="component">The <see cref="MenuSeparator" /> where this handler is responsible for.</param>
        /// <returns>The handler</returns>
        public ADrawable<MenuSeparator> BuildSeparatorHandler(MenuSeparator component)
        {
            return new DefaultSeparator(component);
        }

        /// <summary>
        ///     Builds a new handler for the given <see cref="MenuSliderButton" />.
        /// </summary>
        /// <param name="component">The <see cref="MenuSliderButton" /> where this handler is responsible for.</param>
        /// <returns>The handler</returns>
        public ADrawable<MenuSliderButton> BuildSliderButtonHandler(MenuSliderButton component)
        {
            return new DefaultSliderButton(component);
        }

        /// <summary>
        ///     Builds a new handler for the given <see cref="MenuSlider" />.
        /// </summary>
        /// <param name="component">The <see cref="MenuSlider" /> where this handler is responsible for.</param>
        /// <returns>The handler</returns>
        public ADrawable<MenuSlider> BuildSliderHandler(MenuSlider component)
        {
            return new DefaultSlider(component);
        }

        /// <summary>
        ///     OnDraw event.
        /// </summary>
        public void Draw()
        {
            var position = MenuSettings.Position;
            var menuManager = MenuManager.Instance;
            var height = MenuSettings.ContainerHeight * menuManager.Menus.Count;
            var width = MenuSettings.ContainerWidth;
            if (menuManager.Menus.Count > 0)
            {
                width = menuManager.Menus.First().MenuWidth;
            }

            Line.Width = width;
            Line.Begin();
            Line.Draw(
                new[]
                    {
                        new SerializableVector2(position.X + (width / 2f), position.Y).ToVector2(),
                        new SerializableVector2(position.X + (width / 2), position.Y + height).ToVector2()
                    },
                MenuSettings.RootContainerColor);
            Line.End();

            for (var i = 0; i < menuManager.Menus.Count; ++i)
            {
                var childPos = new SerializableVector2(position.X, position.Y + (i * MenuSettings.ContainerHeight)).ToVector2();

                if (i < menuManager.Menus.Count - 1)
                {
                    Line.Width = 1f;
                    Line.Begin();
                    Line.Draw(
                        new[]
                            {
                                new SerializableVector2(childPos.X, childPos.Y + MenuSettings.ContainerHeight).ToVector2(),
                                new SerializableVector2(
                                    childPos.X + menuManager.Menus[i].MenuWidth,
                                    childPos.Y + MenuSettings.ContainerHeight).ToVector2()
                            },
                        MenuSettings.ContainerSeparatorColor);
                    Line.End();
                }

                menuManager.Menus[i].OnDraw(childPos);
            }

            Line.Width = 1f;
            Line.Begin();
            Line.Draw(
                new[]
                    {
                        new SerializableVector2(position.X, position.Y).ToVector2(), new SerializableVector2(position.X + width, position.Y).ToVector2(),
                        new SerializableVector2(position.X + width, position.Y + height).ToVector2(), new SerializableVector2(position.X, position.Y + height).ToVector2(),
                        new SerializableVector2(position.X, position.Y).ToVector2()
                    },
                Color.Black);
            Line.End();
        }

        /// <summary>
        ///     PreReset.
        /// </summary>
        public void OnPreReset()
        {
            Line.OnLostDevice();
        }

        /// <summary>
        ///     PostReset.
        /// </summary>
        public void OnPostReset()
        {
            Line.OnResetDevice();
        }

        #endregion
    }
}