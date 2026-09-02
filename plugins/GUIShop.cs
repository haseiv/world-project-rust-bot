using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("GUIShop", "WorldProject", "2.0.0")]
    [Description("Sleek Modern Graphical Store & VIP Package Browser")]
    public class GUIShop : RustPlugin
    {
        private const string ShopPanel = "WP_Shop_Main";

        private void Init()
        {
            cmd.AddChatCommand("shop", this, nameof(CmdOpenShop));
            cmd.AddChatCommand("store", this, nameof(CmdOpenShop));
            cmd.AddChatCommand("buy", this, nameof(CmdOpenShop));
        }

        private void CmdOpenShop(BasePlayer player, string command, string[] args)
        {
            string category = args.Length > 0 ? args[0].ToLower() : "vip";
            OpenShopUI(player, category);
        }

        [ConsoleCommand("guishop.category")]
        private void ConsoleCategory(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            string category = arg.GetString(0, "vip");
            OpenShopUI(player, category);
        }

        [ConsoleCommand("guishop.close")]
        private void ConsoleClose(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            CuiHelper.DestroyUi(player, ShopPanel);
        }

        private void OpenShopUI(BasePlayer player, string category)
        {
            CuiHelper.DestroyUi(player, ShopPanel);

            var elements = new CuiElementContainer();

            // 1. Главный фон окна (Тёмный кинематографичный блюр)
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.04 0.05 0.07 0.98", Material = "assets/content/ui/uibackgroundblur.mat" },
                RectTransform = { AnchorMin = "0.10 0.10", AnchorMax = "0.90 0.90" },
                CursorEnabled = true
            }, "Overlay", ShopPanel);

            // 2. Шапка магазина
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.08 0.10 0.14 1.00" },
                RectTransform = { AnchorMin = "0 0.90", AnchorMax = "1 1" }
            }, ShopPanel, "ShopHeader");

            elements.Add(new CuiPanel
            {
                Image = { Color = "0.95 0.60 0.07 1.00" },
                RectTransform = { AnchorMin = "0 0.895", AnchorMax = "1 0.90" }
            }, ShopPanel, "GoldLine");

            elements.Add(new CuiLabel
            {
                Text = { 
                    Text = "<size=20><b><color=#F1C40F>🛒 WORLD PROJECT</color></b></size>  <size=12><color=#7F8C8D>•</color></size>  <size=15><color=#ECF0F1>OFFICIAL IN-GAME STORE</color></size>", 
                    FontSize = 16, 
                    Align = TextAnchor.MiddleLeft, 
                    Color = "1 1 1 1" 
                },
                RectTransform = { AnchorMin = "0.03 0", AnchorMax = "0.75 1" }
            }, "ShopHeader");

            // Кнопка закрытия [✕]
            elements.Add(new CuiButton
            {
                Button = { Command = "guishop.close", Color = "0.75 0.22 0.17 0.85" },
                RectTransform = { AnchorMin = "0.94 0.18", AnchorMax = "0.98 0.82" },
                Text = { Text = "<b>✕</b>", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, "ShopHeader");

            // 3. Панель выбора категорий (Category Bar)
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.06 0.07 0.10 1.00" },
                RectTransform = { AnchorMin = "0 0.80", AnchorMax = "1 0.895" }
            }, ShopPanel, "CategoryBar");

            AddCategoryPill(elements, "CategoryBar", "💎 VIP MEMBERSHIPS", "vip", "0.03 0.25", category == "vip");
            AddCategoryPill(elements, "CategoryBar", "📦 STARTER & DAILY KITS", "kits", "0.27 0.49", category == "kits");
            AddCategoryPill(elements, "CategoryBar", "🌐 WEBSTORE & SKINS", "web", "0.51 0.73", category == "web");

            // 4. Контентная зона магазина
            elements.Add(new CuiPanel
            {
                Image = { Color = "0.07 0.08 0.12 0.80" },
                RectTransform = { AnchorMin = "0.02 0.03", AnchorMax = "0.98 0.78" }
            }, ShopPanel, "ShopContent");

            if (category == "vip")
            {
                AddVipCard(elements, "ShopContent", "💎 VIP TIER", "€5.00 / MONTH", 
                    "• <b>Queue Skip</b> (Instant Server Access)\n• <b>/kit vip</b> (SAR, Hazmat, 2x Meds, Resources)\n• <b>/skinbox</b> (Access to 2,000+ custom skins)\n• <b>2x Furnace Smelt</b> bonus\n• <b>[VIP]</b> Chat & Discord Role", 
                    "0.02 0.05", "0.32 0.95", "0.10 0.14 0.20 0.95", "#3498DB");

                AddVipCard(elements, "ShopContent", "⭐ VIP+ TIER", "€10.00 / MONTH", 
                    "• <b>All VIP Features Included</b>\n• <b>/kit vip+</b> (Custom SMG, Roadsign, Meds)\n• <b>/sil</b> (Paint custom images on signs)\n• <b>/boxsort</b> (Auto-sort chests in 1 click)\n• <b>[VIP+]</b> Gold Tag & Role", 
                    "0.35 0.05", "0.65 0.95", "0.15 0.13 0.10 0.95", "#F39C12");

                AddVipCard(elements, "ShopContent", "👑 ELITE TIER", "€15.00 / MONTH", 
                    "• <b>Ultimate Priority Queue</b> (Highest level)\n• <b>/kit elite</b> (AK-47 / LR-300, Full Metal)\n• <b>All VIP & VIP+ Features</b>\n• <b>Exclusive Discord Nitro</b> colored role\n• <b>Priority Ticket</b> assistance", 
                    "0.68 0.05", "0.98 0.95", "0.14 0.10 0.18 0.95", "#9B59B6");
            }
            else if (category == "kits")
            {
                AddKitCard(elements, "ShopContent", "📦 STARTER KIT", "FREE • COOLDOWN: 1 HOUR", 
                    "• Stone Hatchet & Stone Pickaxe\n• Hunting Bow + 30 Arrows\n• 2x Bandages\n• 1x Campfire\n\n<i>Type <color=#F1C40F>/kit starter</color> in chat to claim!</i>", 
                    "0.05 0.12", "0.47 0.88", "0.10 0.13 0.18 0.95");

                AddKitCard(elements, "ShopContent", "🎁 DAILY REWARD", "FREE • COOLDOWN: 24 HOURS", 
                    "• 1,500 Wood & 1,500 Stone\n• 100 Low Grade Fuel\n• 2x Medical Syringes\n• 50 Scrap\n\n<i>Type <color=#F1C40F>/kit daily</color> in chat to claim!</i>", 
                    "0.53 0.12", "0.95 0.88", "0.10 0.13 0.18 0.95");
            }
            else
            {
                elements.Add(new CuiPanel
                {
                    Image = { Color = "0.10 0.13 0.19 0.95" },
                    RectTransform = { AnchorMin = "0.08 0.08", AnchorMax = "0.92 0.92" }
                }, "ShopContent", "WebCard");

                elements.Add(new CuiLabel
                {
                    Text = { 
                        Text = "<b><size=22><color=#F1C40F>OFFICIAL SERVER WEBSTORE & SKIN TRADES</color></size></b>\n\n" +
                               "Browse our online store or pay with Steam Skins in Discord:\n\n" +
                               "🌐 <b><size=20><color=#74B9FF>https://worldproject.tebex.io</color></size></b>\n\n" +
                               "• ⚡ <b>Instant Delivery:</b> Packages activate in-game within 60 seconds\n" +
                               "• 🔒 <b>Secure Payments:</b> PayPal, Cards, Apple Pay, Google Pay\n" +
                               "• 📦 <b>Pay With Skins:</b> Type <color=#55EFC4>/buy</color> in our Discord to trade Rust/CS2 skins for VIP!", 
                        FontSize = 14, 
                        Align = TextAnchor.MiddleCenter, 
                        Color = "0.95 0.95 0.95 1.0" 
                    },
                    RectTransform = { AnchorMin = "0.05 0.05", AnchorMax = "0.95 0.95" }
                }, "WebCard");
            }

            CuiHelper.AddUi(player, elements);
        }

        private void AddCategoryPill(CuiElementContainer elements, string parent, string title, string catName, string xAnchor, bool active)
        {
            string[] x = xAnchor.Split(' ');
            string btnColor = active ? "0.95 0.60 0.07 1.00" : "0.12 0.14 0.18 0.80";
            string textColor = active ? "0.05 0.05 0.05 1.0" : "0.75 0.75 0.75 1.0";

            elements.Add(new CuiButton
            {
                Button = { Command = $"guishop.category {catName}", Color = btnColor },
                RectTransform = { AnchorMin = $"{x[0]} 0.15", AnchorMax = $"{x[1]} 0.85" },
                Text = { Text = $"<b>{title}</b>", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = textColor }
            }, parent);
        }

        private void AddVipCard(CuiElementContainer elements, string parent, string title, string price, string desc, string min, string max, string bgColor, string colorHex)
        {
            string cardId = CuiHelper.GetGuid();
            elements.Add(new CuiPanel
            {
                Image = { Color = bgColor },
                RectTransform = { AnchorMin = min, AnchorMax = max }
            }, parent, cardId);

            // Заголовок карточки
            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b><size=16><color={colorHex}>{title}</color></size></b>", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.86", AnchorMax = "1 0.98" }
            }, cardId);

            // Бейдж цены
            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b><color=#F1C40F>{price}</color></b>", FontSize = 13, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.76", AnchorMax = "1 0.86" }
            }, cardId);

            // Описание перков
            elements.Add(new CuiLabel
            {
                Text = { Text = desc, FontSize = 11, Align = TextAnchor.UpperLeft, Color = "0.88 0.88 0.88 1.0" },
                RectTransform = { AnchorMin = "0.08 0.18", AnchorMax = "0.92 0.73" }
            }, cardId);

            // Кнопка перехода
            elements.Add(new CuiButton
            {
                Button = { Command = "guishop.category web", Color = "0.18 0.65 0.38 0.95" },
                RectTransform = { AnchorMin = "0.08 0.04", AnchorMax = "0.92 0.14" },
                Text = { Text = "<b>GET THIS RANK</b>", FontSize = 11, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, cardId);
        }

        private void AddKitCard(CuiElementContainer elements, string parent, string title, string badge, string desc, string min, string max, string bgColor)
        {
            string cardId = CuiHelper.GetGuid();
            elements.Add(new CuiPanel
            {
                Image = { Color = bgColor },
                RectTransform = { AnchorMin = min, AnchorMax = max }
            }, parent, cardId);

            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b><size=18><color=#55EFC4>{title}</color></size></b>", FontSize = 15, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.84", AnchorMax = "1 0.96" }
            }, cardId);

            elements.Add(new CuiLabel
            {
                Text = { Text = $"<b><color=#F1C40F>{badge}</color></b>", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.72", AnchorMax = "1 0.84" }
            }, cardId);

            elements.Add(new CuiLabel
            {
                Text = { Text = desc, FontSize = 13, Align = TextAnchor.MiddleCenter, Color = "0.90 0.90 0.90 1.0" },
                RectTransform = { AnchorMin = "0.05 0.15", AnchorMax = "0.95 0.70" }
            }, cardId);
        }
    }
}
