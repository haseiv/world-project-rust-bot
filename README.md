# 🤖 World Project — Rust AI Trade & Discord Manager Bot

Complete automation bot for the **World Project [EU] 2x Trio** Rust & Discord server.

Features an **AI Store Manager**, **Steam Account Verification**, **Skin Trade Order Generation**, and **Direct In-Game VIP Activation** on the Rust server.

---

## 🌟 Features

- 🤖 **AI 24/7 Store & Server Assistant**: Answers player questions about wipe schedules, server rates, rules, and donation tiers in natural language.
- 🔗 **Steam Account Verification (`/link <SteamID64>`)**: Verifies players' SteamID and automatically grants the `⚔️ | Verified Survivor` role.
- 📦 **Personalized Trade Orders (`/buy <tier>`)**: Generates unique order IDs (`WP-XXXXXX`) and tokens to accept Steam skins (Rust / CS2) securely.
- ⚡ **Automated In-Game VIP Delivery**: Dispatches instant VIP activation to the Rust server via `DonationDelivery.cs`.
- 📁 **Oxide / uMod Server Plugins Included**:
  - `WelcomePanel.cs` — Auto popup welcome window upon first join/spawn.
  - `GUIShop.cs` — In-game graphical store (`/shop`).
  - `DonationDelivery.cs` — Instant VIP grant and promo code engine.

---

## 🚀 Quick Start (Local or VPS Hosting)

### 1. Clone the repository
```bash
git clone https://github.com/YOUR_USERNAME/world-project-rust-bot.git
cd world-project-rust-bot
```

### 2. Configure Environment Variables
Copy `.env.example` to `.env` and fill in your details:
```bash
cp .env.example .env
nano .env
```

### 3. Run with Python
```bash
pip install -r requirements.txt
python main.py
```

### 🐳 4. Run with Docker (Recommended for 24/7 Hosting)
```bash
docker compose up -d --build
```
Check logs:
```bash
docker logs -f world_project_rust_bot
```

---

## 📜 Discord Bot Commands

| Command | Description | Permission |
| :--- | :--- | :--- |
| `/link <SteamID64>` | Links SteamID to Discord and grants Verified role | Everyone |
| `/buy <vip\|vipplus\|elite>` | Creates a unique Trade Offer order for VIP purchase | Verified Only |
| `/fulfill <order_id>` | Manually fulfills/activates an order in Rust | Administrator |
| `@Bot <Question>` | Chat with the AI Store & Server Assistant | Everyone |

---

## 🎮 In-Game Rust Commands

- `/info` / `/menu` / `/rules` — Open in-game welcome & info window.
- `/shop` / `/store` — Open graphical store.
- `/redeem <CODE>` — Redeem a VIP promo code.
- `/mydonate` — View remaining VIP subscription time.
