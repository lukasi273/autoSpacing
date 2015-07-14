namespace AutoSpacing
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SFXLibrary;
    using SFXLibrary.Extensions.SharpDX;

    using SharpDX;
    using SharpDX.Direct3D9;

    #endregion

    internal class Program
    {
        private const float CheckInterval = 1 * 100;

        private static readonly Dictionary<int, int> R1 = new Dictionary<int, int>();

        private static readonly Dictionary<int, int> R2 = new Dictionary<int, int>();

        private static readonly Dictionary<int, int> R3 = new Dictionary<int, int>();

        private static float lastCheck;

        private static bool playerAa;

        private static bool targetAa;

        private static bool comparison;

        private static Menu menu;

        private static Obj_AI_Hero player;

        private static Font autoSpacingText;

        public static void OnUpdate(EventArgs args)
        {
            if (lastCheck + CheckInterval > Environment.TickCount)
            {
                return;
            }

            lastCheck = Environment.TickCount;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(e => !e.IsDead && e.IsVisible))
            {
                R1[enemy.NetworkId] =
                    (int)
                    (Math.Round((enemy.Distance(player.Position) - enemy.AttackRange - player.BoundingRadius) / 5f) * 5);
                R2[enemy.NetworkId] =
                    (int)
                    (Math.Round((player.Distance(enemy.Position) - player.AttackRange - enemy.BoundingRadius) / 5f) * 5);
                R3[enemy.NetworkId] = R2[enemy.NetworkId] - R1[enemy.NetworkId];
            }
        }

        private static void Main()
        {
            GameObjects.Initialize();
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            player = ObjectManager.Player;

            menu = new Menu("autoSpacing", "menu", true);

            menu.AddItem(new MenuItem("autoSpacing.TargetAA", "Target AA range").SetValue(true));
            menu.AddItem(new MenuItem("autoSpacing.PlayerAA", "Player AA range").SetValue(true));
            menu.AddItem(new MenuItem("autoSpacing.Comparison", "Comparison between the two").SetValue(true));
            menu.AddItem(
                new MenuItem("autoSpacing.FontSize", "Font size (F5 required)").SetValue(new Slider(13, 3, 30)));

            var whitelistMenu = menu.AddSubMenu(new Menu("Pick a target", "autoSpacing.Whitelist"));
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                whitelistMenu.AddItem(
                    new MenuItem(whitelistMenu.Name + enemy.ChampionName, enemy.ChampionName).SetValue(false));
            }

            autoSpacingText = MDrawing.GetFont(menu.Item("autoSpacing.FontSize").GetValue<Slider>().Value);
            menu.AddToMainMenu();

            Drawing.OnEndScene += OnDrawingEndScene;
            Game.OnUpdate += OnUpdate;
        }

        private static void OnDrawingEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
            {
                return;
            }

            playerAa = menu.Item("autoSpacing.TargetAA").GetValue<bool>();
            targetAa = menu.Item("autoSpacing.PlayerAA").GetValue<bool>();
            comparison = menu.Item("autoSpacing.Comparison").GetValue<bool>();
            {
                foreach (var enemy in
                    GameObjects.EnemyHeroes.Where(
                        e =>
                        !e.IsDead && e.IsVisible && e.Position.IsOnScreen()
                        && menu.Item("autoSpacing.Whitelist" + e.ChampionName).GetValue<bool>()
                        && e.Distance(player) < 2000f))
                {
                    if (playerAa)
                    {
                        autoSpacingText.DrawTextCentered(
                            R1[enemy.NetworkId].ToString(CultureInfo.InvariantCulture), 
                            Drawing.WorldToScreen((Vector3)player.Position.To2D()), 
                            Color.White);
                    }

                    if (targetAa)
                    {
                        autoSpacingText.DrawTextCentered(
                            R2[enemy.NetworkId].ToString(CultureInfo.InvariantCulture), 
                            Drawing.WorldToScreen((Vector3)player.Position.To2D()), 
                            Color.White);
                    }

                    if (comparison)
                    {
                        autoSpacingText.DrawTextCentered(
                            R3[enemy.NetworkId].ToString(CultureInfo.InvariantCulture), 
                            Drawing.WorldToScreen((Vector3)player.Position.To2D()), 
                            Color.White);
                    }
                }
            }
        }
    }
}