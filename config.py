import os
import sys
from dotenv import load_dotenv

load_dotenv()

# ==============================================================================
# 🔑 КОНФИГУРАЦИЯ БОТА WORLD PROJECT
# ==============================================================================

DISCORD_TOKEN = os.getenv("DISCORD_TOKEN", "MTIxMjg0NzcxMzU5MTQ5Njc0Ng.GYD7PO.CQlRQpkTC8FgoUz6ZQ2R-0kJrWibbS5k1tK2Gs").strip()

guild_id_raw = os.getenv("GUILD_ID", "1544384719230345306")
GUILD_ID = int(guild_id_raw) if guild_id_raw.isdigit() else 1544384719230345306

# Официальная ссылка на трейд купленного бота
BOT_STEAM_TRADE_URL = os.getenv(
    "BOT_STEAM_TRADE_URL", 
    "https://steamcommunity.com/tradeoffer/new/?partner=811445940&token=stw_E1-n"
)

# Steam учетные данные
STEAM_LOGIN = os.getenv("STEAM_LOGIN", "dannygaines7p")
STEAM_PASSWORD = os.getenv("STEAM_PASSWORD", "XWBJEx2cYy1987")
STEAM_API_KEY = os.getenv("STEAM_API_KEY", "")
MAFILE_PATH = os.getenv("MAFILE_PATH", "bot.maFile")

# SFTP данные сервера Rust (Shockbyte)
SFTP_HOST = os.getenv("SFTP_HOST", "sftp.discord.fra1.shockbyte.host")
SFTP_PORT = int(os.getenv("SFTP_PORT", 2222))
SFTP_USER = os.getenv("SFTP_USER", "default@38bdde82-0ac4-4728-b5ac-1125fcfe1922")
SFTP_PASS = os.getenv("SFTP_PASS", "RustServerPass123!")
SFTP_BASE_DIR = os.getenv("SFTP_BASE_DIR", "/1. WORLD")

# База данных
DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
os.makedirs(DATA_DIR, exist_ok=True)

DATA_FILE = os.path.join(DATA_DIR, "users_steam_data.json")
ORDERS_FILE = os.path.join(DATA_DIR, "trade_orders.json")
