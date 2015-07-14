namespace AutoSpacing
{
    #region

    using System;
    using System.Globalization;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SFXLibrary;
    using SFXLibrary.Extensions.SharpDX;

    using Color = SharpDX.Color;
    using Font = SharpDX.Direct3D9.Font;

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

            autoSpacingText = MDrawing.GetFont(menu.Item("autoSpacing.FontSize").GetValue<Slider>().Value);
            menu.AddToMainMenu();

            Drawing.OnEndScene += OnDrawingEndScene;
        }

        private static void OnDrawingEndScene(EventArgs args)
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
                    var r1 =
                        Math.Round((enemy.Distance(player.Position) - enemy.AttackRange - player.BoundingRadius) / 5f)
                        * 5;
                    var r2 =
                        Math.Round((player.Distance(enemy.Position) - player.AttackRange - enemy.BoundingRadius) / 5f)
                        * 5;
                    var r3 = r2 - r1;
                    if (playerAa)
                    {
                        autoSpacingText.DrawTextCentered(
                            r1.ToString(CultureInfo.InvariantCulture),
                            Drawing.WorldToScreen(player.Position),
                            Color.White);
                    }

                    if (targetAa)
                    {
                        autoSpacingText.DrawTextCentered(
                            r2.ToString(CultureInfo.InvariantCulture),
                            Drawing.WorldToScreen(player.Position),
                            Color.White);
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