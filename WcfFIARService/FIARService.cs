﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;

namespace WcfFIARService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
          ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class FIARService : IFIARService
    {

        Dictionary<string, IFIARSCallback> clients = new Dictionary<string, IFIARSCallback>();
        Dictionary<string, bool> clientIngame = new Dictionary<string, bool>();
        Dictionary<string, GameBoard> games = new Dictionary<string, GameBoard>();


        public List<PlayerInfo> GetAvalibalePlayers()
        {
            using (var ctx = new FIARDBContext())
            {
                var players = (from p in ctx.Players
                               where p.Status == 1
                               select p).ToList();
                List<PlayerInfo> pi = new List<PlayerInfo>();
                foreach (var player in players)
                {
                    pi.Add(new PlayerInfo(player));
                }

                if (pi == null) return new List<PlayerInfo>();
                return pi;
            }

        }


        public void PlayerLogin(string username, string password)
        {
            using (var ctx = new FIARDBContext())
            {
                var player = (from p in ctx.Players
                              where p.UserName == username && p.Pass == password
                              select p).FirstOrDefault();
                if (player == null)
                {
                    PlayerDoesntExistInDataBase fault = new PlayerDoesntExistInDataBase
                    {
                        Details = "Player " + username + "doesnt exists need to register"
                    };
                    throw new FaultException<PlayerDoesntExistInDataBase>(fault, new FaultReason("Player Doesnt exist in database"));
                }
                if (player.Status != 0)
                {
                    PlayerAlreadyConnectedFault userAlreadyConnected = new PlayerAlreadyConnectedFault
                    {
                        Details = "Player " + username + " already connected"
                    };
                    throw new FaultException<PlayerAlreadyConnectedFault>(userAlreadyConnected, new FaultReason("Player already connected"));
                }

                player.Status = 1;
                ctx.SaveChanges();
                IFIARSCallback callback = OperationContext.Current.GetCallbackChannel<IFIARSCallback>();
                clients.Add(username, callback);
                clientIngame.Add(username, false);

            }
        }

        public void PlayerLogout(string username)
        {
            clients.Remove(username);
            clientIngame.Remove(username);
            using (var ctx = new FIARDBContext())
            {
                var player = (from p in ctx.Players
                              where p.UserName == username
                              select p).FirstOrDefault();
                player.Status = 0;
                ctx.SaveChanges();
            }
        }

        public MoveResult ReportMove(string username, int col)
        {
            MoveResult result = games[username].VerifyMove(username, col);
            if (result == MoveResult.Draw)
            {
                //remove game from games 
                //update database accordingly

            }
            else if (result == MoveResult.YouWon)
            {
                //remove game from games 
                //update database accordingly
            }
            string other_player = ""; // need to get this from the game
            if (!clientIngame.ContainsKey(other_player))
            {
                OpponentDisconnectedFault fault = new OpponentDisconnectedFault();
                fault.Detail = "The other Player quit";
                throw new FaultException<OpponentDisconnectedFault>(fault);
            }
            if (result != MoveResult.NotYourTurn)
            {
                Thread updateOtherPlayerThread = new Thread(() =>
                {
                    clients[other_player].OtherPlayerMoved(result, col);
                });
                updateOtherPlayerThread.Start();
            }
            return result;
        }

        public void RegisterPlayer(string username, string pass)
        {

            try
            {
                using (var ctx = new FIARDBContext())
                {
                    var player = (from p in ctx.Players
                                  where p.UserName == username
                                  select p).FirstOrDefault();
                    if (player == null)
                    {
                        ctx.Players.Add(new Player
                        {
                            UserName = username,
                            Pass = pass
                        });
                        ctx.SaveChanges();
                    }
                    else
                    {
                        PlayerAlreadyExistsInDataBase fault = new PlayerAlreadyExistsInDataBase
                        {
                            Details = "User already exists in data base"
                        };
                        throw new FaultException<PlayerAlreadyExistsInDataBase>(fault);
                    }
                }
            }
            catch (FaultException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public void Disconnected(string username)
        {
            return;
        }

        public bool InvatationSend(string from, string to)
        {
            //var other_player = clients[to];
            if (!clients.ContainsKey(to))
            {
                OpponentDisconnectedFault fault = new OpponentDisconnectedFault();
                fault.Detail = "The Player is offline";
                throw new FaultException<OpponentDisconnectedFault>(fault);
            }
            
            var result = clients[to].SendInvite(from);
            if (result == true)
            {
                //clients.Add(from,)
                //need to add to game
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}