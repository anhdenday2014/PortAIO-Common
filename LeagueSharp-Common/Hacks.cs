namespace LeagueSharp.Common
{
    using EloBuddy;
    /// <summary>
    /// Adds hacks to the menu.
    /// </summary>
    internal class Hacks
    {
        private static Menu menu;

        private static MenuItem MenuAntiAfk;

        private static MenuItem MenuDisableDrawings;

        private static MenuItem MenuDisableSay;

        private static MenuItem MenuTowerRange;

        private const int WM_KEYDOWN = 0x100;

        private const int WM_KEYUP = 0x101;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        internal static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                menu = new Menu("Hacks", "Hacks");

                MenuAntiAfk = menu.AddItem(new MenuItem("AfkHack", "Anti-AFK").SetValue(false));
                MenuAntiAfk.ValueChanged += (sender, args) => EloBuddy.Hacks.AntiAFK = args.GetNewValue<bool>();

                MenuDisableDrawings = menu.AddItem(new MenuItem("DrawingHack", "Disable Drawing").SetValue(false));
                MenuDisableDrawings.ValueChanged += (sender, args) => EloBuddy.Hacks.DisableDrawings = args.GetNewValue<bool>();
                MenuDisableDrawings.SetValue(EloBuddy.Hacks.DisableDrawings);

                MenuDisableSay = menu.AddItem(new MenuItem("SayHack", "Disable L# Send Chat").SetValue(false).SetTooltip("Block Game.Say from Assemblies"));
                MenuDisableSay.ValueChanged += (sender, args) => EloBuddy.Hacks.IngameChat = args.GetNewValue<bool>();

                MenuTowerRange = menu.AddItem(new MenuItem("TowerHack", "Show Tower Ranges").SetValue(false));
                MenuTowerRange.ValueChanged += (sender, args) => EloBuddy.Hacks.TowerRanges = args.GetNewValue<bool>();

                EloBuddy.Hacks.AntiAFK = MenuAntiAfk.GetValue<bool>();
                EloBuddy.Hacks.DisableDrawings = MenuDisableDrawings.GetValue<bool>();

                EloBuddy.Hacks.IngameChat = MenuDisableSay.GetValue<bool>();

                EloBuddy.Hacks.TowerRanges = MenuTowerRange.GetValue<bool>();

                CommonMenu.Instance.AddSubMenu(menu);

                Game.OnWndProc += args =>
                {
                    if (!MenuDisableDrawings.GetValue<bool>())
                    {
                        return;
                    }

                    if ((int)args.WParam != Config.ShowMenuPressKey)
                    {
                        return;
                    }

                    if (args.Msg == WM_KEYDOWN)
                    {
                        EloBuddy.Hacks.DisableDrawings = false;
                    }

                    if (args.Msg == WM_KEYUP)
                    {
                        EloBuddy.Hacks.DisableDrawings = true;
                    }
                };
            };
        }

        public static void Shutdown()
        {
            Menu.Remove(menu);
        }
    }
}