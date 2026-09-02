import os
import sys
from dotenv import load_dotenv

load_dotenv()

# ==============================================================================
# 🔑 КОНФИГУРАЦИЯ БОТА WORLD PROJECT (БЕРЕТСЯ ТОЛЬКО ИЗ .ENV / ПАНЕЛИ ХОСТИНГА)
# ==============================================================================

# Токен берется исключительно из переменных окружения
DISCORD_TOKEN = os.getenv("DISCORD_TOKEN", "").strip()

guild_id_raw = os.getenv("GUILD_ID", "1544384719230345306")
GUILD_ID = int(guild_id_raw) if guild_id_raw.isdigit() else 1544384719230345306

BOT_STEAM_TRADE_URL = os.getenv(
    "BOT_STEAM_TRADE_URL", 
    "https://steamcommunity.com/tradeoffer/new/?partner=811445940&token=stw_E1-n"
)

STEAM_LOGIN = os.getenv("STEAM_LOGIN", "dannygaines7p")
STEAM_PASSWORD = os.getenv("STEAM_PASSWORD", "")
STEAM_API_KEY = os.getenv("STEAM_API_KEY", "")
MAFILE_PATH = os.getenv("MAFILE_PATH", "bot.maFile")

SFTP_HOST = os.getenv("SFTP_HOST", "sftp.discord.fra1.shockbyte.host")
SFTP_PORT = int(os.getenv("SFTP_PORT", 2222))
SFTP_USER = os.getenv("SFTP_USER", "default@38bdde82-0ac4-4728-b5ac-1125fcfe1922")
SFTP_PASS = os.getenv("SFTP_PASS", "")
SFTP_BASE_DIR = os.getenv("SFTP_BASE_DIR", "/1. WORLD")

RCON_HOST = os.getenv("RCON_HOST", "157.85.95.101")
RCON_PORT = int(os.getenv("RCON_PORT", "29416"))
RCON_PASS = os.getenv("RCON_PASS", "")

DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
os.makedirs(DATA_DIR, exist_ok=True)

DATA_FILE = os.path.join(DATA_DIR, "users_steam_data.json")
ORDERS_FILE = os.path.join(DATA_DIR, "trade_orders.json")
