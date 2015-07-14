﻿namespace AutoSpacing
{
    #region

    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SFXLibrary;

    #endregion

    internal class Program
    {
        private static bool playerAa;

        private static bool targetAa;

        private static bool comparison;

        private static Menu menu;

        private static Obj_AI_Hero player;

        private static Font autoSpacingText;

        private static void Main()
        {
            GameObjects.Initialize();
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            player = ObjectManager.Player;
            autoSpacingText = MDrawing.GetFont(menu.Item("AutoSpacing.FontSize").GetValue<Slider>().Value);

            menu = new Menu("autoSpacing", "menu", true);

            menu.AddItem(new MenuItem("autoSpacing.PlayerAA", "Player AA range").SetValue(true));
            menu.AddItem(new MenuItem("autoSpacing.TargetAA", "Target AA range").SetValue(true));
            menu.AddItem(new MenuItem("autoSpacing.Comparison", "Comparison between the two").SetValue(true));
            menu.AddItem(new MenuItem("autoSpacing.FontSize", "Font size").SetValue(new Slider(13, 3, 30)));

            var whitelistMenu = menu.AddSubMenu(new Menu("Pick a target", "autoSpacing.Whitelist"));
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                whitelistMenu.AddItem(
                    new MenuItem(whitelistMenu.Name + enemy.ChampionName, enemy.ChampionName).SetValue(false));
            }

            menu.AddToMainMenu();

            Drawing.OnEndScene += OnDrawingEndScene;
        }

        private void OnDrawingEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
            {
                return;
            }

            playerAa = menu.Item("autoSpacing.PlayerAA").GetValue<bool>();
            targetAa = menu.Item("autoSpacing.TargetAA").GetValue<bool>();
            comparison = menu.Item("autoSpacing.Comparison").GetValue<bool>();
            {
                foreach (var enemy in
                    GameObjects.EnemyHeroes.Where(
                        e =>
                        !e.IsDead && e.IsVisible && e.Position.IsOnScreen()
                        && menu.Item("autoSpacing.Whitelist" + e.ChampionName).GetValue<bool>()
                        && e.Distance(player) < 2000f))
                {
                    var pos = Drawing.WorldToScreen(player.Position);
                    var r1 =
                        Math.Round((enemy.Distance(player.Position) - enemy.AttackRange - player.BoundingRadius) / 5f)
                        * 5;
                    var r2 =
                        Math.Round((player.Distance(enemy.Position) - player.AttackRange - enemy.BoundingRadius) / 5f)
                        * 5;
                    var r3 = r2 - r1;
                    if (playerAa)
                    {
                        Drawing.DrawText(pos.X, pos.Y, Color.White, r1.ToString(CultureInfo.InvariantCulture));
                    }

                    if (targetAa)
                    {
                        Drawing.DrawText(pos.X, pos.Y - 25, Color.White, r2.ToString(CultureInfo.InvariantCulture));
                    }

                    if (comparison)
                    {
                        autoSpacingText.DrawTextCentered(
                            r3.ToString(CultureInfo.InvariantCulture),
                            Drawing.WorldToScreen(player.Position),
                            Color.White);
                    }
                }
            }
        }
    }
}