﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WcfFIARService
{
    [DataContract]
    public class PlayerInfo 
    {

        public PlayerInfo(Player player)
        {
            username = player.UserName;
            Wins = 0;
            Loses = 0;
            Score = 0;
            using (var ctx = new FIARDBContext())
            {
                var allGames = (from g in ctx.Games
                                where g.Player_PlayerId == player.PlayerId || g.PlayedAgainst_PlayerId == player.PlayerId
                                select g).ToList();
                PlayedAgainst = new List<string>();
                foreach (var game in allGames)
                {
                    PlayedAgainst.Add(game.ToString());
                    if (game.Player1 == player)
                    {
                        this.Score += game.Player1Points;
                        if (game.Player1Points > game.Player2Points)
                            this.Wins++;
                        else
                            this.Loses++;
                    }

                    else
                    {
                        this.Score += game.Player2Points;
                        if (game.Player2Points > game.Player1Points)
                            this.Wins++;
                        else
                            this.Loses++;
                    }
                }
            }

        }

        [DataMember]
        public string username{ get; set; }
        [DataMember]
        public int Wins { get; set; }

        [DataMember]
        public int Loses { get; set; }
        [DataMember]
        public int Score { get; set; }

        [DataMember]
        public List<string> PlayedAgainst { get; set; }


    }
}