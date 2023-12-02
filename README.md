# QoL
A TShock plugin that adds various Quality of Life features

## Permissions
| Permissions  | Commands |
|--------------|----------|
| qol.luck     | luck     |
| qol.votekick | votekick |
| qol.voteban  | voteban  |
| qol.vote     | vote     |

## Features
* Despawning Queen Bee if there is no players within 450 blocks.
* Making dungeon chests and shadow chests _unopenable_ until Skeletron is dead.
* Overrides TShock's built-in ``/item`` command to show items in chat when the item is not found. <br>
  _**Note:** If you're using TShock in another language than English, all the texts related to ``/item`` command will be in English._
* New command ``/luck`` will show your luck in chat.
* New command ``/votekick`` will start a voting process to kick the target player.
* New command ``/voteban`` will start a voting process to ban the target player.
* New command ``/vote`` will vote for/against the current voting process.
* New whitelist system based on character names.
* A config file to enable/disable any features. (You need to restart the server to apply changes. ``/reload`` is not enough.)

## Configuration
Here is an example config file:
```json
{
    "QueenBeeRangeCheck": true,
    "LockDungeonChestsTillSkeletron": true,
    "LockShadowChestsTillSkeletron": true,
    "EnableNewItemCommand": true,
    "EnableLuckCommand": true,
    "EnableVotebanCommand": true,
    "VotebanTimeInMinutes": 60,
    "EnableVotekickCommand": true,
    "EnableNameWhitelist": true,
    "WhitelistedNames": [
        "Soofa",
        "Larret",
        "soof"
    ]
}
```
_**Note:** You need to restart the server to apply changes. (``/reload`` command is not enough.)_
