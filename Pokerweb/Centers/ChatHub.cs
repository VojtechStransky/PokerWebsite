using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Pokerweb.Data;
using Pokerweb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Pokerweb.Hubs
{
    public class ChatHub : Hub
    {
        //On connected
        public void Connected(string _key, string username)
        {
            int key = Convert.ToInt32(_key);

            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            List<Player> players = room.Players;
            Player player = players.Find(x => x.PlayerName == username);
            string founderAdress = players.Find(x => x.Founder == true).Address;

            if ((player.Address == string.Empty) || (player.Left == true))
            {
                player.Address = Context.ConnectionId;
                player.Left = false;
            }

            actualisePlayerNames(room);

            foreach (var x in players)
            {
                Clients.Client(x.Address).SendAsync("ReceiveMessage", room.PlayersJson);
            }

            if (players.Count >= 3 && !room.InGame) 
            {
                Clients.Client(founderAdress).SendAsync("ShowPlaybutton");
            }
        }

        //Game start
        public void StartMessage(string _key)
        {
            int key = Convert.ToInt32(_key);

            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            List<Player> players = room.Players;

            room.PrepareNextRound();

            List<string> playersInGame = players.Where(x => x.InGame == true).Select(x => x.PlayerName).ToList();

            Player first = players.Find(x => x.PlayerName == playersInGame[0]);
            int secondIndex = players.FindIndex(x => x.PlayerName == playersInGame[1]);
            Player second = players[secondIndex];

            first.Money = 5;
            second.Money = 10;

            first.Played = true;
            second.Played = true;

            room.Last = players.FindIndex(x => x.PlayerName == playersInGame[1]);

            room.InGame = true;

            PlayMessage(_key, playersInGame[1]);         
        }

        //player has played so this is executed
        public void PlayMessage(string _key, string username)
        {
            int key = Convert.ToInt32(_key);

            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            List<Player> players = room.Players;
            int playerIndex = players.FindIndex(x => x.PlayerName == username);
            Player player = players[playerIndex];

            int i = Convert.ToInt32(PlaySignal(username, key).Item1);
            bool ended = PlaySignal(username, key).Item2;

            if (NewRoundIsNext(key, i, playerIndex) && !ended)
            {
                Player actualPlayer = room.Players[i];
                actualPlayer.Played = true;
                room.Playing = actualPlayer.PlayerName;

                Clients.Client(players[i].Address).SendAsync("ReceivePlayMessage");
            }
            else if (ended)
            {
                GameEnded(key, 1);
            }

            RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key).TimeStamp = DateTime.UtcNow;

            foreach (var x in RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key).Players)
            {
                Clients.Client(x.Address).SendAsync("ReceiveMessage", room.PlayersJson);
            }
        }

        //fold button clicked
        public void FoldMessage(string _key, string username)
        {
            int key = Convert.ToInt32(_key);

            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);

            room.Players.Find(x => x.PlayerName == username).InGame = false;

            room.Message = "Fold";

            PlayMessage(_key, username);
        }

        //check message clicked
        public void CheckMessage(string _key, string username)
        {
            int key = Convert.ToInt32(_key);
            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            room.Message = "Check";

            ProcessMoney(key, username);

            PlayMessage(_key, username);
        }

        //raise message clicked
        public void RaiseMessage(string _key, string username, string money)
        {
            int key = Convert.ToInt32(_key);
            int _money = Convert.ToInt32(money);

            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);

            Player player = room.Players.Find(x => x.PlayerName == username);

            ProcessMoney(key, username, _money);

            room.Message = "Raise " + _money;

            PlayMessage(_key, username);
        }

        //on leave
        public void LeaveMessage(string _key, string username, string isPlaying)
        {
            int key = Convert.ToInt32(_key);

            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            List<Player> players = room.Players;
            Player player = players.Find(x => x.PlayerName == username);

            player.InGame = false;
            player.Left = true;
            player.Founder = false;

            actualisePlayerNames(room);

            List<Player> nextPlayerList = players.Where(x => x.Left == false).ToList();
            Player nextPlayer = nextPlayerList[0];
            nextPlayer.Founder = true;

            if (isPlaying == "true")
            {
                PlayMessage(_key, username);
            }
        }

        //------------------------------------Chat------------------------------
        public void ChatSend(string user, string message, string _key)
        {
            int key = Convert.ToInt32(_key);

            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);

            foreach (var x in room.Players)
            {
                Clients.Client(x.Address).SendAsync("ChatReceive", user, message);
            }
        }

        //------------------------------------------ Helping functions ------------------------------------------
        //get money to check

        private void actualisePlayerNames(Room room)
        {
            List<string> playerNames = room.Players.Where(x => x.Left == false).Select(x => x.PlayerName).ToList();
            room.PlayersJson = JsonConvert.SerializeObject(playerNames);
        }

        private void ProcessMoney(int key, string username, int money = 0)
        {
            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            List<Player> players = room.Players;
            Player player = players.Find(x => x.PlayerName == username);
            int previous = room.Last;
            Player previousPlayer = players[previous];

            int roundChange = previousPlayer.Money - player.Money;
            roundChange += money;

            if (player.MoneyFinal < roundChange)
            {
                player.Money = player.LastMoney;
            } 
            else
            {
                player.Money += roundChange;
            }

            room.Last = players.FindIndex(x => x.PlayerName == username);
        }

        //executed when game is ended
        private void GameEnded(int key, int Case)
        {
            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            List<Player> players = room.Players;

            if (Case == 0) //natural end
            {
                List<string> winners = new List<string>();
                winners.AddRange(EvaluateRound(key));

                room.Winners.AddRange(winners);

                int prize = room.Sum / winners.Count;

                foreach (string winnerName in winners)
                {
                    Player player = players.Find(x => x.PlayerName == winnerName);
                    player.LastMoney = player.MoneyFinal + prize;
                }
            }
            else //last stand
            {
                Player player = players.Find(x => x.InGame == true);
                string winner = player.PlayerName;
                room.Winners.Add(winner);
                player.LastMoney = player.MoneyFinal + room.Sum;
                room.endedCase = 0;
            }

            if (room.Players.FindAll(x => x.MoneyFinal > 0).Count <= 1)
            {
                GameAbsolutelyEnded(room);
            }

            foreach (var x in players)
            {
                Clients.Client(x.Address).SendAsync("ReceiveMessage", room.PlayersJson);
            }

            if (players.Count >= 3)
            {
                Clients.Client(players.Find(x => x.Founder == true).Address).SendAsync("ShowPlaybutton");
            }
        }

        //execute when game has absolutelly ended
        public void GameAbsolutelyEnded(Room room)
        {
            room.endedCase = -1;
            room.InGame = false;
        }

        //check if round has ended
        public bool NewRoundIsNext(int key, int i, int y)
        {
            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            List<Player> players = room.Players;

            if ((players[i].Money == players[y].Money) && (players[i].Played == true))
            {
                room.Round++;           

                if (room.Round >= 4)
                {
                    GameEnded(key, 0);

                    return false;
                }

                foreach (Player x in players)
                {
                    x.Played = false;
                }
            }

            return true;
        }

        //get next playing(non-folded) player
        private (int?, bool) PlaySignal(string username, int key)
        {
            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            List<Player> players = room.Players;
            int index = players.FindIndex(x => x.PlayerName == username);
            int? finalIndex = null;
            int count = players.Count;
            int countInGame = 0;

            for (int i = 0; i < count; i++)
            {
                index++;
                index %= count;

                Player player = players[index];

                if (player.InGame == true)
                {
                    countInGame++;
                    
                    if (finalIndex == null)
                    {
                        finalIndex = index;
                    }
                }
            }

            if (countInGame <= 1)
            {
                return (finalIndex, true);
            }

            return (finalIndex, false);
        }

        //round evaluating function calling external Evaluation.cs
        private List<string> EvaluateRound(int key)
        {
            Room room = RoomsDbContext.RoomsList.Find(x => x.KeyNumber == key);
            List<Player> survivors = room.Players.Where(x => x.InGame == true).ToList();
            List<string> roomsCards = room.Cards;

            room.endedCase = Evaluation.Evaluate(survivors, roomsCards).Item2;
            
            return Evaluation.Evaluate(survivors, roomsCards).Item1;
        }
    }
}
