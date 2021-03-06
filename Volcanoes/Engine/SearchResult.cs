﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class SearchResult
    {
        public int Score { get; set; }

        public long Evaluations { get; set; }

        public long Simulations { get; set; }

        public long Milliseconds { get; set; }

        public decimal HashPercentage { get; set; }

        public int BestMove { get; set; }

        public int NodesPerSecond
        {
            get
            {
                if (Milliseconds > 0)
                {
                    return (int)(Evaluations / (Milliseconds / 1000.0));
                }

                return 0;
            }
        }

        public SearchResult()
        {
            BestMove = -3;
        }

        public SearchResult(int bestMove)
        {
            BestMove = bestMove;
        }
    }
}
