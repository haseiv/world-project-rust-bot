import os
import json
import time
import sys
import paramiko
from datetime import datetime, timedelta

# Библиотека для работы с Steam Guard maFile и авто-подтверждениями
try:
    from steampy.client import SteamClient
    from steampy.models import SteamUrl
except ImportError:
    pass

import config
from trade_watcher import fulfill_order

STEAM_LOGIN = os.getenv("STEAM_LOGIN", "")
STEAM_PASSWORD = os.getenv("STEAM_PASSWORD", "")
STEAM_API_KEY = os.getenv("STEAM_API_KEY", "")
MAFILE_PATH = os.getenv("MAFILE_PATH", "bot.maFile")

def run_mafile_trade_bot():
    print("=" * 60)
    print("🤖 Запуск Автономного Steam Trade Бота с поддержкой maFile...")
    print("=" * 60)

    if not os.path.exists(MAFILE_PATH):
        print(f"[-] Ошибка: Файл '{MAFILE_PATH}' не найден!")
        print("👉 Поместите ваш купленный .maFile в папку с ботом и назовите его bot.maFile.")
        return

    with open(MAFILE_PATH, "r", encoding="utf-8") as f:
        steam_guard = json.load(f)

    # Инициализация Steam клиента с maFile
    try:
        from steampy.client import SteamClient
        client = SteamClient(STEAM_API_KEY)
        client.login(STEAM_LOGIN, STEAM_PASSWORD, json.dumps(steam_guard))
        print("✅ Успешный вход в Steam аккаунт бота через maFile 2FA!")
        print(f"🔗 Ссылка на трейд вашего бота: {client.get_trade_url()}")
    except Exception as e:
        print(f"[-] Ошибка авторизации Steam: {e}")
        return

    print("[*] Ожидание входящих предложений обмена со скинами...")

    while True:
        try:
            trade_offers = client.get_trade_offers(merge_with_descriptions=False)
            incoming = trade_offers.get("response", {}).get("trade_offers_received", [])

            orders = {}
            if os.path.exists(config.ORDERS_FILE):
                with open(config.ORDERS_FILE, "r", encoding="utf-8") as f:
                    orders = json.load(f)

            for offer in incoming:
                offer_id = offer["tradeofferid"]
                message = offer.get("message", "").strip()
                partner_id32 = offer.get("accountid_other")
                partner_steamid64 = str(76561197960265728 + partner_id32)

                # Проверяем, что в обмене есть предметы от игрока
                items_to_receive = offer.get("items_to_receive", [])
                items_to_give = offer.get("items_to_give", [])

                # Если бот ничего не отдает, а только принимает скины
                if len(items_to_receive) > 0 and len(items_to_give) == 0:
                    for order_id, order_info in orders.items():
                        if order_info.get("status") == "pending":
                            if order_id.lower() in message.lower() and order_info["steam_id"] == partner_steamid64:
                                print(f"🎉 ПОЛУЧЕН ТРЕЙД ДЛЯ ЗАКАЗА {order_id} (Offer #{offer_id})!")
                                
                                # Автоматически принимаем и подтверждаем трейд через 2FA
                                client.accept_trade_offer(offer_id)
                                print(f"✅ Трейд #{offer_id} успешно принят и подтвержден через 2FA!")

                                # Активируем донат на сервере Rust
                                fulfill_order(order_id)

        except Exception as e:
            print(f"[-] Ошибка проверки трейдов: {e}")

        time.sleep(15)

if __name__ == "__main__":
    run_mafile_trade_bot()
