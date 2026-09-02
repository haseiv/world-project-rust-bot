import json
import os
import sys
import asyncio
from datetime import datetime, timedelta

import paramiko
import websockets

import config


def _normalize_tier(tier):
    t = (tier or "").lower().strip()
    if t in ("vip+", "vipplus", "vip_plus"):
        return "vipplus"
    if t in ("elite", "vip"):
        return t
    return "vip"


async def _rcon_give(steam_id, tier, days):
    if not config.RCON_PASS:
        raise RuntimeError("RCON_PASS is empty in .env")
    uri = f"ws://{config.RCON_HOST}:{config.RCON_PORT}/{config.RCON_PASS}"
    cmd = f"donation.give {steam_id} {tier} {days}"
    payload = {"Identifier": 9001, "Message": cmd, "Name": "WebRcon"}
    async with websockets.connect(uri, ping_interval=None, open_timeout=10) as ws:
        await ws.send(json.dumps(payload))
        try:
            await asyncio.wait_for(ws.recv(), timeout=5)
        except asyncio.TimeoutError:
            pass


def _sftp_write_donation(steam_id, tier, days):
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    ssh.connect(
        config.SFTP_HOST,
        port=config.SFTP_PORT,
        username=config.SFTP_USER,
        password=config.SFTP_PASS,
        timeout=15,
    )
    sftp = ssh.open_sftp()
    data_path = f"{config.SFTP_BASE_DIR}/oxide/data/DonationDelivery_Data.json"
    server_data = {"ActiveDonations": {}, "PromoCodes": {}}
    try:
        with sftp.open(data_path, "r") as f:
            server_data = json.loads(f.read().decode("utf-8"))
    except Exception:
        pass

    expire_time = (datetime.utcnow() + timedelta(days=days)).isoformat()
    server_data.setdefault("ActiveDonations", {})[str(steam_id)] = {
        "Tier": tier,
        "ExpireTime": expire_time,
    }
    with sftp.open(data_path, "w") as f:
        f.write(json.dumps(server_data, indent=2))
    sftp.close()
    ssh.close()


def fulfill_order(order_id):
    """
    After a Steam trade is accepted: grant VIP on the live Rust server.
    Prefers WebRCON donation.give so kits, chat tags and queue skip apply immediately.
    """
    if not os.path.exists(config.ORDERS_FILE):
        print("[-] Orders file not found.")
        return False

    with open(config.ORDERS_FILE, "r", encoding="utf-8") as f:
        orders = json.load(f)

    if order_id not in orders:
        print(f"[-] Order {order_id} not found.")
        return False

    order = orders[order_id]
    if order.get("status") == "completed":
        print(f"[!] Order {order_id} already completed.")
        return False

    steam_id = order["steam_id"]
    tier = _normalize_tier(order["tier"])
    days = int(order.get("days", 30))

    print(f"[*] Fulfilling {order_id}: {tier.upper()} x{days}d -> {steam_id}")

    try:
        asyncio.run(_rcon_give(steam_id, tier, days))
        print("[+] RCON donation.give sent.")
    except Exception as e:
        print(f"[!] RCON failed ({e}), writing data file via SFTP...")
        try:
            _sftp_write_donation(steam_id, tier, days)
        except Exception as e2:
            print(f"[-] SFTP fallback failed: {e2}")
            return False

    order["status"] = "completed"
    order["completed_at"] = datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S")
    with open(config.ORDERS_FILE, "w", encoding="utf-8") as f:
        json.dump(orders, f, indent=2, ensure_ascii=False)

    print(f"[+] Order #{order_id} completed.")
    return True


if __name__ == "__main__":
    if len(sys.argv) > 1:
        fulfill_order(sys.argv[1])
    else:
        print("Usage: python trade_watcher.py <ORDER_ID>")
