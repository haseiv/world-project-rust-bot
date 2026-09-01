import os
from dotenv import load_dotenv

# Загружаем переменные из файла .env
load_dotenv()

DISCORD_TOKEN = os.getenv("DISCORD_TOKEN")
GUILD_ID = int(os.getenv("GUILD_ID", 0))

BOT_STEAM_TRADE_URL = os.getenv("BOT_STEAM_TRADE_URL", "")

SFTP_HOST = os.getenv("SFTP_HOST", "")
SFTP_PORT = int(os.getenv("SFTP_PORT", 2222))
SFTP_USER = os.getenv("SFTP_USER", "")
SFTP_PASS = os.getenv("SFTP_PASS", "")
SFTP_BASE_DIR = os.getenv("SFTP_BASE_DIR", "/1. WORLD")

DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
os.makedirs(DATA_DIR, exist_ok=True)

DATA_FILE = os.path.join(DATA_DIR, "users_steam_data.json")
ORDERS_FILE = os.path.join(DATA_DIR, "trade_orders.json")
