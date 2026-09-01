import sys
import os
import json
import random
import string
import asyncio
from datetime import datetime

if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding='utf-8')
        sys.stderr.reconfigure(encoding='utf-8')
    except Exception:
        pass

import discord
from discord.ext import commands
import config
from trade_watcher import fulfill_order

# ==============================================================================
# 💾 БАЗА ДАННЫХ
# ==============================================================================
def load_data():
    if os.path.exists(config.DATA_FILE):
        with open(config.DATA_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return {}

def save_data(data):
    with open(config.DATA_FILE, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def load_orders():
    if os.path.exists(config.ORDERS_FILE):
        with open(config.ORDERS_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return {}

def save_orders(orders):
    with open(config.ORDERS_FILE, "w", encoding="utf-8") as f:
        json.dump(orders, f, indent=2, ensure_ascii=False)

# ==============================================================================
# 🤖 AI МЕНЕДЖЕР (СИСТЕМА ОТВЕТОВ)
# ==============================================================================
def ai_store_manager_response(user_message, user_name, is_steam_linked=False, steam_id=None):
    msg = user_message.lower()
    
    if any(k in msg for k in ["купить", "донат", "скин", "трейд", "trade", "buy", "vip", "скинами", "donate"]):
        if not is_steam_linked:
            return (
                f"Привет, {user_name}! 👋\n\n"
                f"⚠️ **Чтобы купить донат скинами или картой, сначала привяжи свой Steam-аккаунт!**\n"
                f"👉 Напиши в чат команду: `/link <твой_SteamID64>` (например: `/link 76561198012345678`)\n\n"
                f"После привязки ты сможешь выбрать любой VIP-тариф командой `/buy`."
            )
        else:
            return (
                f"Привет, {user_name}! Твой Steam (`{steam_id}`) успешно подтверждён. ✅\n\n"
                f"💎 **Доступные VIP-тарифы на сервере World Project [EU] 2x:**\n"
                f"• **1. VIP (€5 / скинами)** — Пропуск очереди, `/kit vip`, скины `/skinbox`, 2x плавка.\n"
                f"• **2. VIP+ (€10 / скинами)** — Все плюсы VIP, `/kit vip+`, рисунки на табличках `/sil`.\n"
                f"• **3. ELITE (€15 / скинами)** — Топ приоритет входа, `/kit elite`, роль в Discord.\n\n"
                f"👉 Чтобы получить персональную трейд-ссылку, напиши: `/buy vip` (или `vipplus`, `elite`)."
            )

    if any(k in msg for k in ["рейт", "вайп", "когда вайп", "wipe", "rate", "ip", "айпи", "connect", "онлайн"]):
        return (
            f"🎮 **Параметры сервера World Project [EU] 2x Trio:**\n"
            f"• **Подключение:** `client.connect 157.85.95.101:29416` в F1 консоли.\n"
            f"• **Рейты:** x2 на дерево, камень, серу, металл и скрап. Ускоренная плавка x2.\n"
            f"• **Лимит команды:** Строго Trio (макс. 3 игрока).\n"
            f"• **График вайпов:** Каждый четверг в 14:00 CEST. Force Wipe — 1-й четверг месяца."
        )

    return (
        f"Привет, {user_name}! Я официальный AI-Менеджер сервера **World Project** 🤖.\n"
        f"Я могу помочь с покупкой доната за скины Steam, привязкой аккаунта и информацией о сервере.\n"
        f"Чем я могу помочь? (Например, спроси: *'Как купить VIP за скины?'* или *'Какой IP у сервера?'*)"
    )

# ==============================================================================
# 🎮 DISCORD БОТ
# ==============================================================================
intents = discord.Intents.default()
intents.guilds = True
intents.message_content = True
intents.members = True

bot = commands.Bot(command_prefix="/", intents=intents)

@bot.event
async def on_ready():
    print("=" * 60)
    print(f"🤖 World Project AI Bot запущен: {bot.user.name} ({bot.user.id})")
    print(f"🌐 Целевой сервер ID: {config.GUILD_ID}")
    print("=" * 60)

# /link <SteamID64>
@bot.command(name="link")
async def cmd_link_steam(ctx, steam_id: str = None):
    if not steam_id:
        embed = discord.Embed(
            title="🔗 Привязка Steam аккаунта",
            description=(
                "Чтобы привязать аккаунт, укажите ваш **SteamID64** (17 цифр):\n"
                "👉 Пример: `/link 76561198012345678`\n\n"
                "*(Узнать свой SteamID можно на [steamid.io](https://steamid.io))*"
            ),
            color=0x3498DB
        )
        await ctx.send(embed=embed)
        return

    steam_id = steam_id.strip()
    if not (steam_id.isdigit() and len(steam_id) == 17 and steam_id.startswith("7656119")):
        await ctx.send("❌ **Ошибка:** Неверный формат SteamID64! Он должен состоять из 17 цифр и начинаться на `7656119...`.")
        return

    data = load_data()
    user_id_str = str(ctx.author.id)
    
    data[user_id_str] = {
        "steam_id": steam_id,
        "discord_tag": str(ctx.author),
        "linked_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S")
    }
    save_data(data)

    role = discord.utils.get(ctx.guild.roles, name="⚔️ | Verified Survivor")
    if role:
        try:
            await ctx.author.add_roles(role)
        except Exception as e:
            print(f"[-] Ошибка выдачи роли: {e}")

    embed = discord.Embed(
        title="✅ Steam аккаунт успешно привязан!",
        description=(
            f"👤 **Пользователь:** {ctx.author.mention}\n"
            f"🆔 **SteamID64:** `{steam_id}`\n"
            f"🎖️ **Статус:** Верифицирован (`⚔️ | Verified Survivor`)\n\n"
            f"Теперь вы можете приобретать VIP-привилегии за скины через команду `/buy`!"
        ),
        color=0x2ECC71
    )
    await ctx.send(embed=embed)

# /buy <tier>
@bot.command(name="buy")
async def cmd_buy(ctx, tier: str = None):
    data = load_data()
    user_id_str = str(ctx.author.id)

    if user_id_str not in data:
        embed = discord.Embed(
            title="⚠️ Требуется привязка Steam!",
            description=(
                "Вы не можете совершать покупки без привязанного Steam-профиля.\n\n"
                "👉 Привяжите ваш аккаунт командой:\n"
                "`/link <ваш_SteamID64>`"
            ),
            color=0xE74C3C
        )
        await ctx.send(embed=embed)
        return

    steam_id = data[user_id_str]["steam_id"]

    if not tier or tier.lower() not in ["vip", "vipplus", "vip+", "elite"]:
        embed = discord.Embed(
            title="🛒 Выберите тариф для покупки",
            description=(
                "Укажите желаемый тариф:\n"
                "• `/buy vip` — Тариф VIP (€5 в скинах)\n"
                "• `/buy vipplus` — Тариф VIP+ (€10 в скинах)\n"
                "• `/buy elite` — Тариф ELITE (€15 в скинах)\n"
            ),
            color=0xF1C40F
        )
        await ctx.send(embed=embed)
        return

    tier_clean = "vipplus" if tier.lower() in ["vipplus", "vip+"] else tier.lower()
    
    order_id = "WP-" + "".join(random.choices(string.ascii_uppercase + string.digits, k=6))
    security_token = "".join(random.choices(string.digits, k=4))

    orders = load_orders()
    orders[order_id] = {
        "discord_id": user_id_str,
        "steam_id": steam_id,
        "tier": tier_clean,
        "security_token": security_token,
        "created_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
        "status": "pending"
    }
    save_orders(orders)

    embed = discord.Embed(
        title=f"📦 Персональный трейд-заказ #{order_id}",
        description=(
            f"👤 **Покупатель:** {ctx.author.mention}\n"
            f"🆔 **Привязанный SteamID:** `{steam_id}`\n"
            f"💎 **Выбранный тариф:** `{tier_clean.upper()}` (30 дней)\n\n"
            f"### 📋 Инструкция по оплате скинами:\n"
            f"1. Нажмите на ссылку трейда бота ниже.\n"
            f"2. Выберите скины из вашего инвентаря Rust/CS2 на нужную сумму.\n"
            f"3. ⚠️ **ОБЯЗАТЕЛЬНО укажите в сообщении к обмену ваш код заказа:**\n"
            f"👉 **`{order_id}`** (Токен безопасности: `{security_token}`)\n"
            f"4. Отправьте обмен. Как только бот примет трейд, донат выдастся на сервере **автоматически**!\n\n"
            f"🔗 **[ОТПРАВИТЬ ТРЕЙД БОТУ]({config.BOT_STEAM_TRADE_URL})**"
        ),
        color=0x9B59B6
    )
    embed.set_footer(text=f"Заказ действителен 60 минут • Сервер World Project EU")
    await ctx.send(embed=embed)

# /fulfill <order_id> (Админ-команда для ручного подтверждения)
@bot.command(name="fulfill")
@commands.has_permissions(administrator=True)
async def cmd_fulfill(ctx, order_id: str):
    success = fulfill_order(order_id)
    if success:
        await ctx.send(f"✅ Заказ `{order_id}` успешно выполнен! Донат активирован в игре.")
    else:
        await ctx.send(f"❌ Не удалось выполнить заказ `{order_id}`. Проверьте логи.")

@bot.event
async def on_message(message):
    if message.author.bot:
        return

    if bot.user.mentioned_in(message) or message.content.startswith("?"):
        clean_content = message.content.replace(f"<@{bot.user.id}>", "").strip("? ")
        data = load_data()
        user_info = data.get(str(message.author.id))
        is_linked = user_info is not None
        steam_id = user_info["steam_id"] if is_linked else None

        response = ai_store_manager_response(clean_content, message.author.display_name, is_linked, steam_id)
        await message.reply(response)

    await bot.process_commands(message)

if __name__ == "__main__":
    if not config.DISCORD_TOKEN:
        print("[-] Ошибка: DISCORD_TOKEN не указан в .env файле!")
        sys.exit(1)
    bot.run(config.DISCORD_TOKEN)
