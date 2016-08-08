// <copyright file="MenuItem.cs" company="LeagueSharp">
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

namespace LeagueSharp.SDK.UI
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    //    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Collections.Generic;

    using LeagueSharp.SDK.Utils;
    using EloBuddy;
    using SharpDX;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using System.Runtime.Serialization.Json;
    using SharpDX.Text;

    /// <summary>
    ///     Menu Item
    /// </summary>
    /// .
    /// 
    [Serializable()]
    [KnownType(typeof(MenuItem))]
    [KnownType(typeof(MenuSeparator))]
    [KnownType(typeof(MenuBool))]
    [KnownType(typeof(MenuList))]
    [KnownType(typeof(MenuKeyBind))]
    [KnownType(typeof(MenuColor))]
    [KnownType(typeof(MenuSliderButton))]
    [KnownType(typeof(MenuButton))]
    [KnownType(typeof(MenuSlider))]
    [KnownType(typeof(MenuList<List<string>>))]
    [DataContract(IsReference = false)]
    public abstract class MenuItem : AMenuComponent
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuItem" /> class.
        /// </summary>
        internal MenuItem()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuItem" /> class.
        /// </summary>
        /// <param name="name">
        ///     Item Name
        /// </param>
        /// <param name="displayName">
        ///     Item Display Name
        /// </param>
        /// <param name="uniqueString">
        ///     Unique string
        /// </param>
        protected MenuItem(string name, string displayName, string uniqueString = "")
            : base(name, displayName, uniqueString)
        {
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     Delegate for <see cref="ValueChanged" />
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">The OnValueChangedEventArgs instance containing the event data.</param>
        public delegate void OnValueChanged(object sender, EventArgs e);

        #endregion

        #region Public Events

        /// <summary>
        ///     Occurs when a value is changed.
        /// </summary>
        public event OnValueChanged ValueChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the path.
        /// </summary>
        /// <value>
        ///     The path.
        /// </value>
        public override string Path
        {
            get
            {
                var fileName = this.Name + this.UniqueString + "." + this.GetType().Name + ".bin";

                if (this.Parent == null)
                {
                    return System.IO.Path.Combine(MenuManager.ConfigFolder.CreateSubdirectory(this.AssemblyName).FullName, fileName);
                }

                return System.IO.Path.Combine(this.Parent.Path, fileName);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the settings are loaded.
        /// </summary>
        public bool SettingsLoaded { get; set; }

        /// <summary>
        ///     Returns if the item is toggled.
        /// </summary>
        public override bool Toggled { get; set; }

        /// <summary>
        ///     Returns the item visibility.
        /// </summary>
        public override sealed bool Visible { get; set; }

        #endregion

        #region Public Indexers

        /// <summary>
        ///     Gets the Component Dynamic Object accessibility.
        /// </summary>
        /// <param name="name">
        ///     Child Menu Component name
        /// </param>
        /// <returns>Null, a menu item is unable to hold an access-able sub component</returns>
        public override AMenuComponent this[string name] => null;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Drawing callback.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        ///     Extracts the specified component.
        /// </summary>
        /// <param name="component">
        ///     The component.
        /// </param>
        public abstract void Extract(MenuItem component);

        /// <summary>
        ///     Event Handler
        /// </summary>
        public void FireEvent()
        {
            this.Parent?.FireEvent(this);
            this.ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the 1.
        /// </typeparam>
        /// <returns>Returns the value as the given type</returns>
        /// <exception cref="Exception">Cannot cast value  + Value.GetType() +  to  + typeof(T1)</exception>
        public override T GetValue<T>()
        {
            return (T)this;
        }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>An Exception, there is no child for a MenuItem.</returns>
        /// <exception cref="Exception">Cannot get child of a MenuItem</exception>
        public override T GetValue<T>(string name)
        {
            throw new Exception("Cannot get child of a MenuItem");
        }

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public override void Load()
        {
            if (!this.SettingsLoaded && File.Exists(this.Path) && this.GetType().IsSerializable)
            {
                this.SettingsLoaded = true;
                try
                {
                    //File.ReadAllBytes(this.Path), typeof(MenuItem)
                    var obj2 = BinarySerializer.Deserialize<MenuItem>(File.ReadAllBytes(this.Path));
                    this.Extract(obj2);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public BinarySerializer Serializer;

        /// <summary>
        ///     Saves this instance.
        /// </summary>
        public override void Save()
        {
            if (this.GetType().IsSerializable)
            {
                //var a = Serializer.Serialize(this, typeof(MenuItem));

                var stream = new MemoryStream();
                //Serializer = new BinarySerializer();

                File.WriteAllBytes(this.Path, BinarySerializer.Serialize(this));
            }
        }

        /// <summary>
        ///     Item Draw callback.
        /// </summary>
        /// <param name="position">
        ///     The position.
        /// </param>
        public override void OnDraw(SerializableVector2 position)
        {
            if (this.Visible)
            {
                this.Position = position;
                try
                {
                    this.Draw();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        /// <summary>
        ///     Item Update callback.
        /// </summary>
        public override void OnUpdate()
        {
        }

        /// <summary>
        ///     Item Windows Process Messages callback.
        /// </summary>
        /// <param name="args">
        ///     <see cref="WindowsKeys" /> data
        /// </param>
        public override void OnWndProc(WindowsKeys args)
        {
            if (!args.Process)
            {
                return;
            }

            this.WndProc(args);
        }

        /// <summary>
        ///     Item PreReset callback.
        /// </summary>
        public override void OnPreReset()
        {
            this.PreReset();
        }

        /// <summary>
        ///     Item PostReset callback.
        /// </summary>
        public override void OnPostReset()
        {
            this.PostReset();
        }

        /// <summary>
        ///     Windows Process Messages callback.
        /// </summary>
        /// <param name="args"><see cref="WindowsKeys" /> data</param>
        public abstract void WndProc(WindowsKeys args);

        /// <summary>
        ///     Item PreReset callback.
        /// </summary>
        public abstract void PreReset();

        /// <summary>
        ///     Item PostReset callback.
        /// </summary>
        public abstract void PostReset();

        #endregion
    }
}