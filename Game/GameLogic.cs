
using System;
using System.Collections.Generic;

namespace TicTacToeServer
{
    class GameLogic
    {
        public enum CurrentStatus
        {
            WaitingOtherPlayerToConnect = 0,
            Player1Turn = 1,
            Player2Turn = 2,
            FinishedWithTie = 3,
            FinishedWithPlayer1Victory = 4, 
            FinishedWithPlayer2Victory = 5,
            
        }
        public static int[,] WinPossibilities = new int[,] 
        {
            {0,1,2},
            {3,4,5},
            {6,7,8},
            {0,3,6},
            {1,4,7},
            {2,5,8},
            {0,4,8},
            {2,4,6}
        };

        private static int _connectedPlayersCount = 0;
        public static readonly int MaxPlayers = 2;
        public static readonly Dictionary<int, Player> Players = new Dictionary<int, Player>();
        public static int[] Slots { get; private set; }

        public static bool GameStarted { get; private set; }
        public static CurrentStatus Status { get; private set; }

        public static void Update()
        {
            ThreadManager.UpdateMain();
        }

        internal static bool RegisterClientAsPlayer(int fromClient)
        {
            if(_connectedPlayersCount == 0)
            {
                var player = new Player()
                {
                    PlayerId = 1
                };
                Players.Add(fromClient, player);
                InitializeGameBoard();
                Status = CurrentStatus.WaitingOtherPlayerToConnect;
                ServerSend.RegisteredAsPlayer(fromClient);
                ServerSend.SendStatusForAll();
                _connectedPlayersCount++;
                return true;
            }
            else if( _connectedPlayersCount == 1)
            {
                var player = new Player()
                {
                    PlayerId = 2
                };
                Players.Add(fromClient, player);
                ServerSend.RegisteredAsPlayer(fromClient);
                StartGame();
                ServerSend.SendStatusForAll();
                _connectedPlayersCount++;
                return true;
            }
            ServerSend.RegisterAsPlayerError(fromClient);
            return false;
        }

        internal static void PlayerPlay(int fromClient, int slot)
        {
            var player = Players[fromClient];
            if(player.PlayerId == 1 && Status != CurrentStatus.Player1Turn){}
                //TODO: Error
            else if(player.PlayerId == 2 && Status != CurrentStatus.Player2Turn){}
                //TODO: Error
            else if(Status != CurrentStatus.Player1Turn || Status != CurrentStatus.Player2Turn){}
                // TODO: Error

            var isEmptySlot = Slots[slot] == 0;
            if(!isEmptySlot){}
                //TODO: Error

            Slots[slot] = player.PlayerId;

            int result = ComputeGameResult();
            if(result == 0) // Game continues
            {
                Status = Status == CurrentStatus.Player1Turn ? CurrentStatus.Player2Turn : CurrentStatus.Player1Turn;
            }
            else if(result == 1)
            {
                Status = CurrentStatus.FinishedWithPlayer1Victory;
                GameStarted = false;
            }
            else if(result == 2)
            {
                Status = CurrentStatus.FinishedWithPlayer2Victory;
                GameStarted = false;
            }
            else if(result == 3)
            {
                Status = CurrentStatus.FinishedWithTie;
                GameStarted = false;
            } 
            ServerSend.SendStatusForAll();

        }
        private static void PlayerSurrendeded(int client)
        {
            int playerId = Players[client].PlayerId;
            
            if(playerId == 2)
            {
                Status = CurrentStatus.FinishedWithPlayer1Victory;
                GameStarted = false;
            }
            else if(playerId == 1)
            {
                Status = CurrentStatus.FinishedWithPlayer2Victory;
                GameStarted = false;
            }

            ServerSend.SendStatusForAll();
        }
        private static int ComputeGameResult()
        {
            var hasEmpty = false;
            for (int posibilitie = 0; posibilitie < 8; posibilitie++)
            {
                int slotIndex = WinPossibilities[posibilitie, 0];
                int value = Slots[slotIndex];
                var allEquals = true; 
                for (int i = 0; i < 3 ; i++)
                {
                    slotIndex = WinPossibilities[posibilitie, i];
                    allEquals = allEquals && value == Slots[slotIndex]; 
                    hasEmpty = hasEmpty || Slots[slotIndex] == 0;
                }

                if(allEquals)
                {
                    return value;
                }
            }

            if(!hasEmpty)
            {
                 return 3;
            }
            return 0;
        }

        private static void StartGame()
        {
            Status = CurrentStatus.Player1Turn;
            GameStarted = true;
        }

        private static void InitializeGameBoard()
        {
            Slots = new int[9];
            for (int i = 0; i < 9; i++)
            {
                Slots[i] = 0;
            }
        }

        internal static void DisconnectedPlayer(int fromClient)
        {
            Player player;
            if (!Players.TryGetValue(fromClient, out player))
                return;

            if(GameStarted)
            {
                PlayerSurrendeded(fromClient);
            }

            Players.Remove(fromClient);
            _connectedPlayersCount--;
            return;
        }


    }

    public class Player
    {
        public int PlayerId;
    }
}