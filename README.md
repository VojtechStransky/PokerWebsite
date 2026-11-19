# PokerWebsite

Multiplayer Texas Hold'em in the browser, written in C# in 2020–2021 as a
hobby project. One player creates a room and gets a six-digit key, the
others join with it, and the game runs in real time over SignalR.

## What it does

- Rooms joined by key; the founder can start the game once three players are in.
- Betting rounds with check, raise and fold; blinds of 5 and 10; every
  player starts with 300.
- Cards are dealt from a shuffled 52-card deck; at showdown the server
  evaluates the hands and names the winner or winners.
- Players who leave or run out of money drop out of the next round.

## Layout

| Path | What it is |
|---|---|
| `Pokerweb/Centers/ChatHub.cs` | SignalR hub: every game action (connect, start, play, fold, raise, end of round) |
| `Pokerweb/Evaluation.cs` | hand evaluator: ranks five-card hands from royal flush to high card, with kickers |
| `Pokerweb/Models/Room.cs`, `Player.cs` | game state: deck, shuffle, deal, rotation of players |
| `Pokerweb/Data/RoomsDbContext.cs` | in-memory list of rooms (there is no database) |
| `Pokerweb/Pages/` | Razor Pages: lobby (`Index`) and table (`GamePage`) |
| `Pokerweb/wwwroot/js/Chat.js` | browser side of the SignalR connection |

## Run

```
dotnet run --project Pokerweb
```

The project targets .NET Core 3.1, which is no longer supported. A current
SDK builds it with a warning; running it needs the 3.1 runtime or a retarget
in `Pokerweb/Pokerweb.csproj`.
