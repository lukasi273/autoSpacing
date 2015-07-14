namespace AutoSpacing
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Events;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.UI.IMenu;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;

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

        private static Menu config;

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
            Load.OnLoad += OnLoad;
        }

        private static void OnLoad(object sender, EventArgs e)
        {
            Bootstrap.Init(null);

            player = ObjectManager.Player;

            config = new Menu("autoSpacing", "menu", true);

            config.Add(new MenuBool("autoSpacing.TargetAA", "Target AA range", true));
            config.Add(new MenuBool("autoSpacing.PlayerAA", "Player AA range", true));
            config.Add(new MenuBool("autoSpacing.Comparison", "Comparison between the two", true));
            config.Add(
                new MenuSlider("autoSpacing.FontSize", "Font size (F5 required)", 13, 3, 30));

            var whitelistMenu = config.Add(new Menu("autoSpacing.Whitelist", "Pick a target"));
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                whitelistMenu.Add(
                    new MenuBool(whitelistMenu.Name + enemy.ChampionName, enemy.ChampionName));
            }

            autoSpacingText = MDrawing.GetFont(config["autoSpacing.FontSize"].GetValue<MenuSlider>().Value);
            config.Attach();

            Drawing.OnEndScene += OnDrawingEndScene;
            Game.OnUpdate += OnUpdate;
        }

        private static void OnDrawingEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
            {
                return;
            }

            playerAa = config["autoSpacing.TargetAA"].GetValue<MenuBool>().Value;
            targetAa = config["autoSpacing.PlayerAA"].GetValue<MenuBool>().Value;
            comparison = config["autoSpacing.Comparison"].GetValue<MenuBool>().Value;
            {
                foreach (var enemy in
                    GameObjects.EnemyHeroes.Where(
                        e =>
                        !e.IsDead && e.IsVisible && e.Position.IsOnScreen()
                        && (config["autoSpacing.Whitelist" + e.ChampionName].GetValue<MenuBool>().Value
                        && e.Distance(player) < 2000f)))
                {
                    if (playerAa)
                    {
                        autoSpacingText.DrawTextCentered(
                            R1[enemy.NetworkId].ToString(CultureInfo.InvariantCulture), 
                            Drawing.WorldToScreen((Vector3)player.Position.ToVector2()), 
                            Color.White);
                    }

                    if (targetAa)
                    {
                        autoSpacingText.DrawTextCentered(
                            R2[enemy.NetworkId].ToString(CultureInfo.InvariantCulture), 
                            Drawing.WorldToScreen((Vector3)player.Position.ToVector2()), 
                            Color.White);
                    }

                    if (comparison)
                    {
                        autoSpacingText.DrawTextCentered(
                            R3[enemy.NetworkId].ToString(CultureInfo.InvariantCulture), 
                            Drawing.WorldToScreen((Vector3)player.Position.ToVector2()), 
                            Color.White);
                    }
                }
            }
        }
    }
}