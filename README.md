# QoL

A TShock plugin that adds various Quality of Life features.

If you want to read this in another language: [Spanish](https://github.com/Soof4/QoL/blob/main/README_SPANISH.md)

## Permissions

| Permissions  | Commands     |
| ------------ | ------------ |
| qol.luck     | luck         |
| qol.votekick | votekick     |
| qol.voteban  | voteban      |
| qol.vote     | vote         |
| qol.iteminfo | iteminfo, ii |
| qol.builder  | builder      |
| qol.tpn      | tpn          |

## Features

- Drop fragments for each player that deals damage to the pillar (like how Treasure Bags work!).
- Despawning Queen Bee if there is no players within 450 blocks.
- Making dungeon chests and shadow chests _unopenable_ until Skeletron is dead.
- Overrides TShock's built-in `/item` command to show items in chat when the item is not found. <br>
  _**Note:** If you're using TShock in another language other than English, all the texts related to `/item` command will be in English._
- New command `/luck` will show your luck in chat.
- New command `/votekick <player name>` will start a voting process to kick the target player.
- New command `/voteban <player name>` will start a voting process to ban the target player.
- New command `/vote <y/n>` will vote for/against the current voting process.
- New command `/iteminfo <item name>` will show information about the item.
- New command `/tpn <npc name>` will teleport player to the town NPCs only.
- New whitelist system based on character names.

## Configuration

Here is an example config file:

```json
{
  "FragmentsFunctionLikeTreasureBags": true,
  "QueenBeeRangeCheck": true,
  "LockDungeonChestsTillSkeletron": true,
  "LockShadowChestsTillSkeletron": true,
  "VotebanTimeInMinutes": 60,
  "DisableQuickStack": false,
  "EnableNameWhitelist": true,
  "WhitelistedNames": ["Soofa", "Larret", "soof"]
}
```
