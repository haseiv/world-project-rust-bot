using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("DonationDelivery", "WorldProject", "1.0.0")]
    [Description("Automated VIP donation delivery, SteamID grants and redeemable promo codes")]
    public class DonationDelivery : RustPlugin
    {
        private class DonationData
        {
            public Dictionary<ulong, PlayerDonation> ActiveDonations = new Dictionary<ulong, PlayerDonation>();
            public Dictionary<string, PromoCode> PromoCodes = new Dictionary<string, PromoCode>(StringComparer.OrdinalIgnoreCase);
        }

        private class PlayerDonation
        {
            public string Tier;
            public DateTime ExpireTime;
        }

        private class PromoCode
        {
            public string Tier;
            public int Days;
            public int MaxUses;
            public int CurrentUses;
        }

        private DonationData data;

        private void Loaded()
        {
            data = Interface.Oxide.DataFileSystem.ReadObject<DonationData>("DonationDelivery_Data") ?? new DonationData();
            
            // Регистрируем разрешения (Permissions) для донат-групп
            permission.RegisterPermission("donationdelivery.vip", this);
            permission.RegisterPermission("donationdelivery.vipplus", this);
            permission.RegisterPermission("donationdelivery.elite", this);

            // Проверяем истекшие донаты каждые 10 минут
            timer.Every(600f, CheckExpiredDonations);
        }

        private void Unload()
        {
            SaveData();
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("DonationDelivery_Data", data);
        }

        // =====================================================================
        // 🎮 КОМАНДЫ ДЛЯ ИГРОКОВ В ИГРЕ
        // =====================================================================

        [ChatCommand("redeem")]
        private void CmdRedeem(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                SendReply(player, "<color=#F1C40F>[STORE]</color> Usage: <color=#FFEAA7>/redeem <YOUR-CODE></color>");
                return;
            }

            string code = args[0].Trim();
            if (!data.PromoCodes.ContainsKey(code))
            {
                SendReply(player, "<color=#E74C3C>[ERROR]</color> Invalid or expired promo code!");
                return;
            }

            var promo = data.PromoCodes[code];
            if (promo.CurrentUses >= promo.MaxUses && promo.MaxUses > 0)
            {
                SendReply(player, "<color=#E74C3C>[ERROR]</color> This promo code has already reached its maximum uses!");
                return;
            }

            promo.CurrentUses++;
            GrantDonation(player.userID, player.displayName, promo.Tier, promo.Days);

            if (promo.MaxUses > 0 && promo.CurrentUses >= promo.MaxUses)
            {
                data.PromoCodes.Remove(code);
            }
            SaveData();

            SendReply(player, $"<color=#2ECC71>[SUCCESS]</color> You have successfully activated <b>{promo.Tier.ToUpper()}</b> for <b>{promo.Days} days</b>!");
        }

        [ChatCommand("mydonate")]
        private void CmdMyDonate(BasePlayer player, string command, string[] args)
        {
            if (data.ActiveDonations.TryGetValue(player.userID, out var don))
            {
                var remaining = don.ExpireTime - DateTime.UtcNow;
                if (remaining.TotalSeconds > 0)
                {
                    SendReply(player, $"<color=#F1C40F>[VIP STATUS]</color> Your <b>{don.Tier.ToUpper()}</b> status is active! Remaining: <b>{remaining.Days}d {remaining.Hours}h {remaining.Minutes}m</b>");
                    return;
                }
            }
            SendReply(player, "<color=#F1C40F>[VIP STATUS]</color> You do not have an active VIP subscription. Type <color=#FFEAA7>/shop</color> to get one!");
        }

        // =====================================================================
        // 🤖 КОНСОЛЬНЫЕ КОМАНДЫ ДЛЯ RCON / БОТА / МАГАЗИНА (TEBEX / DISCORD)
        // =====================================================================

        // Выдача доната напрямую игроку по SteamID:
        // donation.give 76561198000000000 vip 30
        [ConsoleCommand("donation.give")]
        private void ConsoleGiveDonation(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin && arg.Connection != null) return;

            if (arg.Args == null || arg.Args.Length < 3)
            {
                Puts("Usage: donation.give <SteamID> <vip|vipplus|elite> <days>");
                return;
            }

            if (!ulong.TryParse(arg.Args[0], out ulong steamId))
            {
                Puts("Invalid SteamID!");
                return;
            }

            string tier = arg.Args[1].ToLower();
            if (!int.TryParse(arg.Args[2], out int days))
            {
                days = 30;
            }

            var player = BasePlayer.FindByID(steamId);
            string name = player != null ? player.displayName : steamId.ToString();

            GrantDonation(steamId, name, tier, days);
            Puts($"[DonationDelivery] Successfully granted {tier.ToUpper()} to {name} ({steamId}) for {days} days.");
        }

        // Создание промокода для бота/раздачи:
        // donation.addcode VIP-PROMO-123 vip 30 1
        [ConsoleCommand("donation.addcode")]
        private void ConsoleAddCode(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin && arg.Connection != null) return;

            if (arg.Args == null || arg.Args.Length < 3)
            {
                Puts("Usage: donation.addcode <code> <vip|vipplus|elite> <days> [max_uses]");
                return;
            }

            string code = arg.Args[0];
            string tier = arg.Args[1].ToLower();
            int days = int.Parse(arg.Args[2]);
            int maxUses = arg.Args.Length > 3 ? int.Parse(arg.Args[3]) : 1;

            data.PromoCodes[code] = new PromoCode
            {
                Tier = tier,
                Days = days,
                MaxUses = maxUses,
                CurrentUses = 0
            };
            SaveData();

            Puts($"[DonationDelivery] Created promo code '{code}' for {tier.ToUpper()} ({days} days, max uses: {maxUses})");
        }

        private void GrantDonation(ulong steamId, string playerName, string tier, int days)
        {
            DateTime newExpire = DateTime.UtcNow.AddDays(days);
            if (data.ActiveDonations.TryGetValue(steamId, out var existing))
            {
                if (existing.ExpireTime > DateTime.UtcNow && existing.Tier.Equals(tier, StringComparison.OrdinalIgnoreCase))
                {
                    newExpire = existing.ExpireTime.AddDays(days);
                }
            }

            data.ActiveDonations[steamId] = new PlayerDonation
            {
                Tier = tier,
                ExpireTime = newExpire
            };
            SaveData();

            // Выдаем права Oxide
            string perm = $"donationdelivery.{tier}";
            permission.GrantUserPermission(steamId.ToString(), perm, this);

            // Оповещение всему серверу о поддержке
            PrintToChat($"<color=#F1C40F>★ [STORE]</color> Player <color=#55EFC4>{playerName}</color> just activated <color=#F39C12>{tier.ToUpper()}</color> for <b>{days} days</b>! Thank you for supporting the server!");

            // Звуковой эффект игроку, если он онлайн
            var player = BasePlayer.FindByID(steamId);
            if (player != null)
            {
                Effect.server.Run("assets/bundled/prefabs/fx/invite_notice.prefab", player.transform.position);
            }
        }

        private void CheckExpiredDonations()
        {
            var now = DateTime.UtcNow;
            var toRemove = new List<ulong>();

            foreach (var kvp in data.ActiveDonations)
            {
                if (kvp.Value.ExpireTime <= now)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var id in toRemove)
            {
                var don = data.ActiveDonations[id];
                permission.RevokeUserPermission(id.ToString(), $"donationdelivery.{don.Tier}");
                data.ActiveDonations.Remove(id);

                var player = BasePlayer.FindByID(id);
                if (player != null)
                {
                    SendReply(player, "<color=#E74C3C>[DONATE]</color> Your VIP subscription has expired. Thank you for your support!");
                }
            }

            if (toRemove.Count > 0)
            {
                SaveData();
            }
        }
    }
}
