import json
import os
import sys
import paramiko
from datetime import datetime, timedelta
import config

def fulfill_order(order_id):
    """
    Вызывается при получении и подтверждении трейда в Steam.
    Автоматически выдает донат на сервере Rust по привязанному SteamID.
    """
    if not os.path.exists(config.ORDERS_FILE):
        print(f"[-] Файл заказов не найден.")
        return False

    with open(config.ORDERS_FILE, "r", encoding="utf-8") as f:
        orders = json.load(f)

    if order_id not in orders:
        print(f"[-] Заказ {order_id} не найден.")
        return False

    order = orders[order_id]
    if order.get("status") == "completed":
        print(f"[!] Заказ {order_id} уже был выполнен ранее.")
        return False

    steam_id = order["steam_id"]
    tier = order["tier"]
    days = int(order.get("days", 30))

    print(f"[*] Обработка заказа {order_id}: выдача {tier.upper()} на {days} дней для SteamID {steam_id}...")

    try:
        ssh = paramiko.SSHClient()
        ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
        ssh.connect(config.SFTP_HOST, port=config.SFTP_PORT, username=config.SFTP_USER, password=config.SFTP_PASS, timeout=15)
        sftp = ssh.open_sftp()
        
        data_path = f"{config.SFTP_BASE_DIR}/oxide/data/DonationDelivery_Data.json"
        
        server_data = {"ActiveDonations": {}, "PromoCodes": {}}
        try:
            with sftp.open(data_path, "r") as f:
                server_data = json.loads(f.read().decode('utf-8'))
        except:
            pass

        expire_time = (datetime.utcnow() + timedelta(days=days)).isoformat()
        
        server_data["ActiveDonations"][str(steam_id)] = {
            "Tier": tier,
            "ExpireTime": expire_time
        }

        with sftp.open(data_path, "w") as f:
            f.write(json.dumps(server_data, indent=2))

        sftp.close()
        ssh.close()
        
        order["status"] = "completed"
        order["completed_at"] = datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S")
        
        with open(config.ORDERS_FILE, "w", encoding="utf-8") as f:
            json.dump(orders, f, indent=2, ensure_ascii=False)

        print(f"🎉 ЗАКАЗ #{order_id} УСПЕШНО ВЫПОЛНЕН! Донат {tier.upper()} активирован для {steam_id}!")
        return True
    except Exception as e:
        print(f"❌ Ошибка при выдаче доната на сервер: {e}")
        return False

if __name__ == "__main__":
    if len(sys.argv) > 1:
        fulfill_order(sys.argv[1])
    else:
        print("Использование: python trade_watcher.py <ORDER_ID>")
