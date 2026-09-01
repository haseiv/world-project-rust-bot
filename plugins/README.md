# 📁 Oxide/uMod Plugins for World Project Rust Server

This directory contains the custom in-game C# plugins for the Rust server:

1. **`WelcomePanel.cs`**:
   - In-game GUI window that auto popups on player connect/respawn.
   - Shows rates (2x Trio), rules, store, and discord links.
   - Command: `/info`, `/rules`, `/menu`.

2. **`GUIShop.cs`**:
   - Graphical in-game store showcasing VIP ranks and starter kits.
   - Command: `/shop`, `/store`, `/buy`.

3. **`DonationDelivery.cs`**:
   - Automated VIP grant engine connected with the Discord/Trade bot.
   - Commands: `/redeem <code>`, `/mydonate`, console command `donation.give <steamid> <tier> <days>`.
