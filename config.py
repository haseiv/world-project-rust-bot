import os
import sys
from dotenv import load_dotenv

# Загружаем .env, если он есть локально (на хостинге переменные берутся напрямую из системы/панели)
load_dotenv()

# ==============================================================================
# 🔑 ПЕРЕМЕННЫЕ ОКРУЖЕНИЯ (ЗАДАЮТСЯ В ПАНЕЛИ ХОСТИНГА)
# ==============================================================================

# Токен Discord бота
DISCORD_TOKEN = os.getenv("DISCORD_TOKEN", "").strip()

# ID сервера Discord (World Project)
guild_id_raw = os.getenv("GUILD_ID", "1544384719230345306")
GUILD_ID = int(guild_id_raw) if guild_id_raw.isdigit() else 1544384719230345306

# Ссылка на трейд вашего Steam бота/аккаунта
BOT_STEAM_TRADE_URL = os.getenv(
    "BOT_STEAM_TRADE_URL", 
    "https://steamcommunity.com/tradeoffer/new/?partner=YOUR_PARTNER_ID&token=YOUR_TOKEN"
)

# SFTP данные сервера Rust (Shockbyte)
SFTP_HOST = os.getenv("SFTP_HOST", "sftp.discord.fra1.shockbyte.host")
SFTP_PORT = int(os.getenv("SFTP_PORT", 2222))
SFTP_USER = os.getenv("SFTP_USER", "default@38bdde82-0ac4-4728-b5ac-1125fcfe1922")
SFTP_PASS = os.getenv("SFTP_PASS", "RustServerPass123!")
SFTP_BASE_DIR = os.getenv("SFTP_BASE_DIR", "/1. WORLD")

# Папка для сохранения базы данных
DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
os.makedirs(DATA_DIR, exist_ok=True)

DATA_FILE = os.path.join(DATA_DIR, "users_steam_data.json")
ORDERS_FILE = os.path.join(DATA_DIR, "trade_orders.json")
