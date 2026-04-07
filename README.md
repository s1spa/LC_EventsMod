# Lethal Company: Event Mod 🧨

[![Version](https://img.shields.io/badge/version-0.6.0-orange.svg)](https://github.com/s1spa/LC_ChaosMod)

**Event Mod** is a plugin for Lethal Company that turns a standard quota run into a true test of survival. No more peaceful looting: the game will randomly throw "surprises" at you that directly impact your chances of making it back alive.

## 🛡️ Core Rule: Safe Zone
* **The Ship is a total Safe Zone**.
* Events do not affect players while they are inside the ship.

## 🚀 Key Features
* **Random Events:** From landmines under your feet to sudden turret ambushes and monster spawns.
* **Standalone System:** Operates autonomously via an internal timer without requiring external services.
* **HUD Warnings:** A notification appears (e.g., "Event starting in 5 seconds...") to give you a moment to panic.
* **Localization:** Full support for both English and Ukrainian languages.
* **Full Customization:** Adjust event intervals and difficulty through the in-game settings menu.
* **Network Sync:** Everyone suffers together! Events are synchronized across all players.

## 🛠️ Event Roadmap
You can toggle specific events and adjust their intensity in the settings menu.

- [x] **Mine Rain** — Landmines randomly spawn around the player every 1-3 seconds.
- [x] **Turret Ambush** — 2 to 4 turrets suddenly spawn in your immediate vicinity.
- [x] **Berserk Turrets** — All active turrets enter berserk mode and fire in all directions for a configurable duration.
- [x] **Monster Spawn** — A random monster is instantly spawned near the player.
- [x] **Dungeon Warp** — Teleports players to a random location within the facility (Indoor only).
- [x] **Emergency Recall** — Suddenly teleports a random player back to the safety of the ship.
- [x] **Swap** — Two random players swap positions (Excludes players on the ship).
- [x] **Random Sound** — A random enemy audio clip plays near a player inside the facility.
- [x] **Adrenaline Rush** — Players get infinite stamina but can't stop running forward for a configurable duration.
- [x] **Football** — All dropped items become footballs: running near any item kicks it with a smooth parabolic arc, complete with wall bouncing.
- [x] **Company Alert** — A fake critical message from The Company appears on everyone's screen. Ship leaving? Quota doubled? Assets confiscated? All lies.
- [ ] **Fake Loot** — Deceptive items that explode or vanish when you try to pick them up.
- [ ] **Ghost Hunt** — Fake footsteps, laughter, or visual glitches to trigger paranoia.

Passive mechanics:
- [x] **Firefly** — Picking up the Apparatus causes the player to glow with a warm point light, visible to all players until round end.


## 🖥️ Settings Menu
A dedicated button in the Main Menu allows you to configure the mod:
* **Language:** Toggle between English and Ukrainian.
* **Intervals:** Set minimum and maximum time between chaos events.
* **Severity:** Adjust how "hardcore" the events will be.
* **Event Selector:** Enable or disable specific types of events.

## 📦 Installation
1. Ensure you have [BepInExPack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/) installed.
2. Install [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/).
3. Download `LC_ChaoseEvent.dll` from the Releases section.
4. Place the file into your `Lethal Company/BepInEx/plugins/LC_ChaoseEvent` folder.

---
*Created by [s1spa](https://github.com/s1spa) with chaos in mind.*
