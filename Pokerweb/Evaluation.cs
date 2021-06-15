using System;
using System.Collections.Generic;
using Pokerweb.Models;

namespace Pokerweb
{
    public static class Evaluation
    {
        private class Winner
        {
            public string Name;
            public int Power;
            public int AditionalPower;
        }

        public static (List<string>, int) Evaluate(List<Player> Players, List<string> RoomsCards)
        {
            Winner actualWinner = new Winner() { Power = 0 };
            List<string> winners = new List<string>();
            int winningPower = 0;

            foreach (Player player in Players)
            {
                for (int i = 0; i < 10; i++)
                {
                    List<string> cardsCombination = SwapCards(i, player.Cards, RoomsCards);

                    var thisBest = FindBest(cardsCombination);

                    if (thisBest.Item1 > actualWinner.Power)
                    {
                        actualWinner.Power = thisBest.Item1;
                        actualWinner.AditionalPower = thisBest.Item2;
                        actualWinner.Name = player.PlayerName;
                        winners.Clear();
                        winners.Add(player.PlayerName);
                        winningPower = thisBest.Item1;
                    }
                    else if (thisBest.Item1 == actualWinner.Power)
                    {
                        if (thisBest.Item2 > actualWinner.AditionalPower)
                        {
                            actualWinner.Power = thisBest.Item1;
                            actualWinner.AditionalPower = thisBest.Item2;
                            actualWinner.Name = player.PlayerName;
                            winners.Clear();
                            winners.Add(player.PlayerName);
                        }
                        else if ((thisBest.Item2 == actualWinner.AditionalPower) && (player.PlayerName != actualWinner.Name))
                        {
                            actualWinner.Name = player.PlayerName;
                            winners.Add(player.PlayerName);
                        }
                    }
                }
            }

            return (winners, winningPower);
        }

        private static List<string> SwapCards(int i, List<string> playersCards, List<string> roomsCards)
        {
            List<string> returnCards = new List<string>();
            returnCards.AddRange(roomsCards);

            switch (i)
            {
                case 0:
                    MakeSwap(0, 1);
                    break;
                case 1:
                    MakeSwap(0, 2);
                    break;
                case 2:
                    MakeSwap(0, 3);
                    break;
                case 3:
                    MakeSwap(0, 4);
                    break;
                case 4:
                    MakeSwap(1, 2);
                    break;
                case 5:
                    MakeSwap(1, 3);
                    break;
                case 6:
                    MakeSwap(1, 4);
                    break;
                case 7:
                    MakeSwap(2, 3);
                    break;
                case 8:
                    MakeSwap(2, 4);
                    break;
                case 9:
                    MakeSwap(3, 4);
                    break;
            }

            return returnCards;

            void MakeSwap(int first, int second)
            {
                returnCards[first] = playersCards[0];
                returnCards[second] = playersCards[1];
            }
        }

        private static (int, int) FindBest(List<string> cards)
        {
            List<string> colors = new List<string>();
            List<int> values = new List<int>();

            foreach (string card in cards)
            {
                colors.Add(card.Substring(0, 2));
                values.Add(Convert.ToInt32(card.Substring(3, 2)));
            }

            colors.Sort();
            values.Sort();

            int highest = values[4];
            int highestMultiple = 0;
            int secondMultiple = 0;
            int highestCoeficient = (((highest * 100 + values[3]) * 100 + values[2]) * 100 + values[1]) * 100 + values[0];

            if (areSameCollor() && isStraight())
            {
                if (values[4] == 14)
                {
                    //Royal flush
                    return (10, highest);
                }
                else
                {
                    //Straight flush
                    return (9, highest);
                }
            }

            //Four of a kind
            if (areSameValue(0, 4) || areSameValue(1, 5))
            {
                return (8, values[2]);
            }

            //Full house
            if ((areSameValue(0, 3) && areSameValue(3, 5)) || (areSameValue(2, 5) && areSameValue(0, 2)))
            {
                return (7, values[2]);
            }

            //Flush
            if (areSameCollor())
            {
                return (6, highestCoeficient);
            }

            //Straight
            if (isStraight())
            {
                return (5, highest);
            }

            //Three of a kind
            if (areSameValue(0, 3) || areSameValue(1, 4) || areSameValue(2, 5))
            {
                return (4, values[2]);
            }

            //Two pair
            if (countOfPairs() == 2)
            {
                int coeficient = highestMultiple * 100 + secondMultiple;

                for (int i = 0; i < 5; i++)
                {
                    if ((values[4 - i] != highestMultiple) && (values[4 - i] != secondMultiple))
                    {
                        coeficient = coeficient * 100 + values[4 - i];
                    }
                }

                return (3, coeficient);
            }

            //One pair
            if (countOfPairs() == 1)
            {
                int coeficient = highestMultiple;

                for (int i = 0; i < 5; i++)
                {
                    if (values[4 - i] != highestMultiple)
                    {
                        coeficient = coeficient * 100 + values[4 - i];
                    }
                }

                return (2, coeficient);
            }

            //High card
            return (1, highestCoeficient);

            //---Helping functions---
            bool areSameCollor()
            {
                string previousColor = colors[0];
                foreach (string color in colors)
                {
                    if (color != previousColor)
                    {
                        return false;
                    }

                    previousColor = color;
                }

                return true;
            }

            bool areSameValue(int start, int end)
            {
                int previousValue = values[start];
                for (int i = start + 1; i < end; i++)
                {
                    if (values[i] != previousValue)
                    {
                        return false;
                    }

                    previousValue = values[i];
                }

                return true;
            }

            bool isStraight()
            {
                int previousValue = values[0];
                for (int i = 1; i < values.Count; i++)
                {
                    if (values[i] != previousValue + 1)
                    {
                        return false;
                    }

                    previousValue = values[i];
                }

                return true;
            }

            int countOfPairs()
            {
                int count = -1;
                int previousValue = values[0];
                foreach (int value in values)
                {
                    if (value == previousValue)
                    {
                        count++;

                        if (value > highestMultiple)
                        {
                            secondMultiple = highestMultiple;
                            highestMultiple = value;
                        }
                    }

                    previousValue = value;
                }

                return count;
            }
        }
    }
}
