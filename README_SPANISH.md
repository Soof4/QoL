# QoL

Un Plugin de TShock que agrega varias funciones de calidad de vida. (Traducido por [FrankV22](https://github.com/itsFrankV22))

## Permisos

|   Permisos   |  Comandos    |
| ------------ | ------------ |
| qol.luck     | luck         |
| qol.votekick | votekick     |
| qol.voteban  | voteban      |
| qol.vote     | vote         |
| qol.iteminfo | iteminfo, ii |
| qol.builder  | builder      |

## Características

- Suelta fragmentos para cada jugador que cause daño al pilar. (trabaja igual que las bolsas de tesoro de jefes en Maestro!).
- Desaparición de Reina Abeja si no hay jugadores dentro de 450 bloques.
- Hacer cofres de mazmorra y cofres de sombra _Bloqueados a abrir_ hasta que Skeletron esté muerto.
- Cambia completamente el comado `/item` Y muestra el item en el chat junto com el id para cuando este perdido, o solo te muestre coincidencias. <br>
  _**Nota:** Si estás usando TShock en otro idioma que no sea inglés, todos los textos relacionados con `/item` estarán en inglés.._
- Nuevo comando `/luck` Mostrarás tu suerte en el chat..
- Nuevo comando `/votekick <player name>` Empezara un proceso de votos para Expulsar al jugador.
- Nuevo comando `/voteban <player name>` Empezara un proceso de votos para Banear al jugador, tiempo en la config.
- Nuevo comando `/vote <y/n>` Vota a favor "y" o en contra "n".
- Nuevo comando `/iteminfo <item name>` Muestra informacion de ese item.
- Nuevo sistema de ListaBlanca "WhiteList" basado en nombres.

## Configuracion

Ejemplo del archivo config aqui:

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
