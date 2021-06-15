using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pokerweb.Models
{
    public class Room
    {
        public Room()
        {
            List<string> barva = new List<string>() { "kr_", "sr_", "ka_", "pi_" };
            List<string> hodnota = new List<string>() {"02","03","04","05","06","07","08","09","10","11","12","13","14"};

            foreach (string b in barva)
            {
                foreach (string h in hodnota)
                {
                    cardsList.Add(b + h);
                }
            }

            Shuffle(ref cardsList);

            Queue<string> cards = new Queue<string>(cardsList);
            Packet = cards;

            Cards = GetChunk(5);

        }

        //properties
        public List<Player> Players = new List<Player>();
        public int presetMoney { get; set; } = 300;
        public List<string> Winners { get; set; } = new List<string>();
        public Queue<string> Packet { get; set; }
        public int Round { get; set; } = 0;
        public int Last { get; set; } = 0;
        public string Playing { get; set; }
        public int KeyNumber { get; set; }
        public List<string> Cards { get; set; }
        public bool InGame { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public string PagePartialHelper { get; set; }
        public string Message { get; set; }
        public string PlayersJson { get; set; } = "";
        public int endedCase { get; set; } = 1;
        public int Sum { 
            get
            {
                int i = 0;
                foreach(Player player in Players)
                {
                    i += player.Money;
                }
                return i;
            } 
        }

        //metod
        public void AddPlayer(Player player)
        {
            player.Cards = GetChunk(2);
            this.Players.Add(player);
        }

        public void PrepareNextRound()
        {
            this.Message = "";

            Shuffle(ref cardsList);

            Queue<string> cards = new Queue<string>(cardsList);
            Packet = cards;

            Cards = GetChunk(5);

            if (InGame == false)
            {
                foreach (Player player in Players)
                {
                    player.LastMoney = presetMoney;
                    player.NonFailed = true;
                    player.Played = false;
                    player.Money = 0;
                }
            } 
            else
            {
                foreach (Player player in Players)
                {
                    if (!Winners.Contains(player.PlayerName))
                    {
                        player.LastMoney = player.MoneyFinal;
                    }

                    if ((player.Left == true) || (player.LastMoney <= 0))
                    {
                        player.NonFailed = false;
                    }

                    player.Cards = GetChunk(2);
                    player.Money = 0;
                    player.Played = false;
                    player.InGame = player.NonFailed;
                }
            }

            Winners.Clear();
            Rotate(ref Players);
            Round = 0;
        }
        public Room ShallowCopy()
        {
            return (Room)this.MemberwiseClone();
        }

        //private
        private Random rnd = new Random();
        private List<string> cardsList = new List<string>();
        private void Shuffle<T>(ref List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        private void Rotate<T>( ref List<T> items)
        {
            T nItem;
            int count = items.Count;
            nItem = items[count - 1];
            items.RemoveAt(count - 1);
            items.Insert(0, nItem);
        }
        private List<string> GetChunk(int count)
        {
            List<string> chunk = new List<string>();
            for(int i = 0; i < count; i++)
            {
                chunk.Add(this.Packet.Dequeue());
            }
            return chunk;
        }
    }
}
