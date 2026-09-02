using System;
using System.Collections.Generic;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SteamLink", "WorldProject", "1.0.0")]
    [Description("In-game Steam account linking with Discord via /link code")]
    public class SteamLink : RustPlugin
    {
        private class VerificationCode
        {
            public string Code;
            public ulong SteamID;
            public string PlayerName;
            public DateTime ExpiresAt;
        }

        private class LinkData
        {
            public Dictionary<string, VerificationCode> PendingCodes = new Dictionary<string, VerificationCode>();
            public HashSet<ulong> LinkedSteamIDs = new HashSet<ulong>();
        }

        private LinkData data;

        private void Loaded()
        {
            data = Interface.Oxide.DataFileSystem.ReadObject<LinkData>("SteamLink_Data") ?? new LinkData();
            permission.RegisterPermission("steamlink.verified", this);
            timer.Every(300f, CleanExpiredCodes);
        }

        private void Unload()
        {
            SaveData();
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("SteamLink_Data", data);
        }

        [ChatCommand("link")]
        private void CmdLink(BasePlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected) return;

            // Проверяем, привязан ли уже аккаунт
            data = Interface.Oxide.DataFileSystem.ReadObject<LinkData>("SteamLink_Data") ?? data;
            if (data.LinkedSteamIDs.Contains(player.userID))
            {
                SendReply(player, "<color=#2ECC71>[VERIFIED]</color> Your Steam account is already linked to Discord! You have full access to perks.");
                return;
            }

            // Генерируем 6-значный цифровой код
            string code = UnityEngine.Random.Range(100000, 999999).ToString();

            // Удаляем старые коды этого игрока
            var toRemove = new List<string>();
            foreach (var kvp in data.PendingCodes)
            {
                if (kvp.Value.SteamID == player.userID)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var key in toRemove)
            {
                data.PendingCodes.Remove(key);
            }

            // Добавляем новый код со сроком 15 минут
            data.PendingCodes[code] = new VerificationCode
            {
                Code = code,
                SteamID = player.userID,
                PlayerName = player.displayName,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            SaveData();

            // Отправляем игроку подробное уведомление
            SendReply(player, "<color=#E67E22>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            SendReply(player, "<color=#F1C40F>★ [DISCORD LINK VERIFICATION]</color>");
            SendReply(player, $"Your one-time verification code is: <color=#55EFC4><b>{code}</b></color>");
            SendReply(player, $"Go to our Discord (<color=#74B9FF>https://discord.gg/worldrust</color>) and type:");
            SendReply(player, $"<color=#F1C40F><b>/link {code}</b></color> in any chat channel.");
            SendReply(player, "<color=#BDC3C7>This code is valid for 15 minutes.</color>");
            SendReply(player, "<color=#E67E22>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");

            Effect.server.Run("assets/bundled/prefabs/fx/notice/loot.drag.finish.prefab", player.transform.position);
        }

        [ConsoleCommand("steamlink.complete")]
        private void ConsoleCompleteLink(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin && arg.Connection != null) return;
            if (arg.Args == null || arg.Args.Length < 1) return;

            if (ulong.TryParse(arg.Args[0], out ulong steamId))
            {
                data = Interface.Oxide.DataFileSystem.ReadObject<LinkData>("SteamLink_Data") ?? data;
                data.LinkedSteamIDs.Add(steamId);
                SaveData();

                permission.GrantUserPermission(steamId.ToString(), "steamlink.verified", this);

                var player = BasePlayer.FindByID(steamId);
                if (player != null && player.IsConnected)
                {
                    SendReply(player, "<color=#2ECC71><b>🎉 [SUCCESS]</b> Your Steam account has been successfully verified in Discord!</color>");
                    Effect.server.Run("assets/bundled/prefabs/fx/invite_notice.prefab", player.transform.position);
                }
            }
        }

        private void CleanExpiredCodes()
        {
            var now = DateTime.UtcNow;
            var toRemove = new List<string>();
            foreach (var kvp in data.PendingCodes)
            {
                if (kvp.Value.ExpiresAt <= now)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            if (toRemove.Count > 0)
            {
                foreach (var k in toRemove) data.PendingCodes.Remove(k);
                SaveData();
            }
        }
    }
}
