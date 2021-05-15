using JailTime2.Core;
using JailTime2.Core.Manager;
using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JailTime2
{
    public class Configuration : IRocketPluginConfiguration
    {
        public bool IgnoreLastPlayerPosition; // if false then the player will be spawned where he disappeared before he got into the slammer

        public string BanArrestedReasonOnReconnect; // ban reason
        public bool BanArrestedOnReconnect; // whether to ban the player if he left the server
        public uint BanDurationOnReconnect; // time of the player's ban if he left the server while serving his term

        public int WalkDistance;

        public Vector3SE SpawnPointAfterArrest; // spawn point after arrest

        public List<Cell> Cells;

        public void LoadDefaults()
        {
            IgnoreLastPlayerPosition = false;
            BanArrestedReasonOnReconnect = "You were banned for disconnecting from the server while serving your sentence!";
            BanArrestedOnReconnect = false;
            BanDurationOnReconnect = 500;
            WalkDistance = 5;

            SpawnPointAfterArrest = new Vector3SE()
            {
                X = 0,
                Y = 0,
                Z = 0
            };
            Cells = new List<Cell>()
            {
                new Cell()
                {
                    Id = 1,
                    Position = new Vector3SE(0, 0, 0)
                }
            };

        }
    }
}
