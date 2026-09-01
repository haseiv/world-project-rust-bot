import time
import requests
import json
import os
import sys
import config
from trade_watcher import fulfill_order

# ==============================================================================
# 🤖 STEAM TRADE AUTO-POLLER (ПРОВЕРКА ВХОДЯЩИХ ТРЕЙДОВ ПО STEAM API)
# ==============================================================================

STEAM_API_KEY = os.getenv("STEAM_API_KEY", "")

def get_incoming_trade_offers():
    """Получает список входящих предложений обмена через Steam Web API"""
    if not STEAM_API_KEY:
        return []

    url = "https://api.steampowered.com/IEconService/GetTradeOffers/v1/"
    params = {
        "key": STEAM_API_KEY,
        "get_received_offers": 1,
        "get_descriptions": 1,
        "active_only": 1
    }

    try:
        res = requests.get(url, params=params, timeout=10)
        if res.status_code == 200:
            data = res.json()
            return data.get("response", {}).get("trade_offers_received", [])
    except Exception as e:
        print(f"[-] Ошибка опроса Steam API: {e}")
    return []

def check_and_process_trades():
    print("[*] Запущен автоматический сканер трейдов Steam...")
    
    if not STEAM_API_KEY:
        print("[-] Внимание: STEAM_API_KEY не указан в .env/переменных хостинга.")
        print("[-] Бот ожидает ручного подтверждения через /fulfill <ORDER_ID>.")
        return

    while True:
        offers = get_incoming_trade_offers()
        orders = {}
        if os.path.exists(config.ORDERS_FILE):
            with open(config.ORDERS_FILE, "r", encoding="utf-8") as f:
                orders = json.load(f)

        for offer in offers:
            message = offer.get("message", "").strip()
            partner_account_id = offer.get("accountid_other")
            
            # Конвертируем accountid_32 в SteamID64
            partner_steamid64 = str(76561197960265728 + partner_account_id)

            # Ищем совпадение по номеру заказа WP-XXXXXX в комментарии трейда
            for order_id, order_info in orders.items():
                if order_info.get("status") == "pending":
                    if order_id.lower() in message.lower() and order_info["steam_id"] == partner_steamid64:
                        print(f"🎉 НАЙДЕН ТРЕЙД ДЛЯ ЗАКАЗА {order_id} от SteamID {partner_steamid64}!")
                        # Выполняем авто-выдачу на сервере Rust
                        fulfill_order(order_id)

        time.sleep(30)

if __name__ == "__main__":
    check_and_process_trades()
