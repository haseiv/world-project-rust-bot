import sys
import os

if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding='utf-8')
        sys.stderr.reconfigure(encoding='utf-8')
    except Exception:
        pass

from dotenv import load_dotenv
load_dotenv()

import discord
from discord.ext import commands

TOKEN = os.getenv("DISCORD_TOKEN", "")
GUILD_ID = int(os.getenv("GUILD_ID", "1544384719230345306"))

intents = discord.Intents.default()
intents.guilds = True

bot = commands.Bot(command_prefix="!", intents=intents)

@bot.event
async def on_ready():
    print(f"[*] Connected as {bot.user.name}")
    guild = bot.get_guild(GUILD_ID)
    if not guild:
        print("[-] Guild not found!")
        await bot.close()
        return

    # 1. 🛒・store
    store_ch = discord.utils.get(guild.text_channels, name="🛒・store")
    if store_ch:
        try:
            await store_ch.purge(limit=15)
        except:
            pass

        embed_header = discord.Embed(
            title="⚡ WORLD PROJECT — OFFICIAL STORE & VIP",
            description=(
                "Support the server and get instant access to exclusive in-game perks, kits, and priority queue.\n\n"
                "🌐 **Webstore:** [https://worldproject.tebex.io](https://worldproject.tebex.io)\n"
                "📦 **Pay with Skins:** Type `/buy` in <#1544384719230345306> to trade Rust/CS2 skins!\n"
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
            ),
            color=0xF1C40F
        )

        embed_header.add_field(
            name="💎 VIP TIER  •  €5.00 / mo",
            value=(
                "```yaml\n"
                "• Queue Skip: Instant Server Entry\n"
                "• In-Game Kit: /kit vip (SAR, Hazmat, 2x Meds, Resources)\n"
                "• Skinbox: Access to 2,000+ custom weapon/armor skins\n"
                "• Smelting: 2x Furnace Speed boost\n"
                "• Discord: [VIP] Chat Tag & Exclusive Role\n"
                "```"
            ),
            inline=False
        )

        embed_header.add_field(
            name="⭐ VIP+ TIER  •  €10.00 / mo",
            value=(
                "```yaml\n"
                "• All VIP Perks Included\n"
                "• In-Game Kit: /kit vip+ (Custom SMG, Roadsign, Meds)\n"
                "• Sign Artist: /sil (Paint any image from URL onto signs)\n"
                "• Chest Sort: /boxsort (Organize storage in 1 click)\n"
                "• Discord: [VIP+] Gold Role & Private Chat Access\n"
                "```"
            ),
            inline=False
        )

        embed_header.add_field(
            name="👑 ELITE TIER  •  €15.00 / mo",
            value=(
                "```yaml\n"
                "• Ultimate Priority: Highest Queue Skip level\n"
                "• In-Game Kit: /kit elite (AK-47 / LR-300, Full Metal)\n"
                "• All Features: Every perk from VIP & VIP+ included\n"
                "• Custom Color: Exclusive Discord Nitro-style colored role\n"
                "• Support: Priority ticket assistance from Head Admins\n"
                "```"
            ),
            inline=False
        )

        embed_header.set_footer(text="Instant Delivery (within 60s) • Fair Play Policy Applies")
        await store_ch.send(embed=embed_header)

    # 2. 🌐・server-info
    info_ch = discord.utils.get(guild.text_channels, name="🌐・server-info")
    if info_ch:
        try:
            await info_ch.purge(limit=15)
        except:
            pass

        embed_info = discord.Embed(
            title="⚡ WORLD PROJECT [EU] 2x TRIO",
            description=(
                "Welcome to **World Project** — high performance, active staff, and optimized EU gameplay.\n\n"
                "### 🎮 QUICK CONNECT:\n"
                "Press **`F1`** in Rust and paste the command:\n"
                "```bash\nclient.connect 157.85.95.101:29416\n```\n"
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
            ),
            color=0x2ECC71
        )

        embed_info.add_field(
            name="⚙️ Server Specifications & Rates",
            value=(
                "```yaml\n"
                "• Region: Europe (Frankfurt / Low Ping)\n"
                "• Gather: 2x Resources | 2x Components | 2x Scrap\n"
                "• Team Limit: Trio (Strict Max 3 Players)\n"
                "• Smelting: 2x Speed in all Furnaces\n"
                "• Stacks: 2x Stacks for Resources & Meds\n"
                "• Map Size: 3500 Procedural (Optimized FPS)\n"
                "```"
            ),
            inline=False
        )

        embed_info.add_field(
            name="⏰ Wipe Schedule (CEST / UTC+2)",
            value=(
                "```yaml\n"
                "• Map Wipe: Every Thursday @ 14:00 CEST (BPs Kept)\n"
                "• Force Wipe: 1st Thursday of every month @ 20:00 CEST\n"
                "```"
            ),
            inline=False
        )

        embed_info.add_field(
            name="🔗 Quick Links",
            value="[🌐 Webstore](https://worldproject.tebex.io)  •  [💬 Discord Invite](https://discord.gg/worldrust)",
            inline=False
        )

        embed_info.set_footer(text="World Project EU • Good luck surviving!")
        await info_ch.send(embed=embed_info)

    # 3. 📜・rules
    rules_ch = discord.utils.get(guild.text_channels, name="📜・rules")
    if rules_ch:
        try:
            await rules_ch.purge(limit=15)
        except:
            pass

        embed_rules = discord.Embed(
            title="📜 WORLD PROJECT — COMMUNITY & SERVER RULES",
            description="All players must adhere to these rules. Ignorance of the rules is not an excuse.\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━",
            color=0xE74C3C
        )

        embed_rules.add_field(
            name="🚫 1. Anti-Cheat & Third-Party Software",
            value=(
                "```diff\n"
                "- Cheats, recoil macros (Bloody/A4Tech), or overlays = PERMANENT BAN.\n"
                "- Playing with a banned cheater within 24h = Association Ban.\n"
                "- Refusing or dodging a staff PC-Check (AnyDesk) = PERMANENT BAN.\n"
                "```"
            ),
            inline=False
        )

        embed_rules.add_field(
            name="👥 2. Group Limits (Trio — Max 3)",
            value=(
                "```yaml\n"
                "• Strictly maximum 3 players per base, roam, raid, or fight.\n"
                "• No alliances, cross-teaming, or neutral agreements.\n"
                "• Swapping members requires clearing TC, bags, and 2h cooldown.\n"
                "```"
            ),
            inline=False
        )

        embed_rules.add_field(
            name="💬 3. Chat & Community Etiquette",
            value=(
                "```yaml\n"
                "• English only in public text & voice channels.\n"
                "• Zero tolerance for racism, doxxing, or extreme harassment.\n"
                "```"
            ),
            inline=False
        )

        embed_rules.set_footer(text="Report rule breakers in 🚨・report-cheater with video evidence")
        await rules_ch.send(embed=embed_rules)

    await bot.close()

if __name__ == "__main__":
    bot.run(TOKEN)
