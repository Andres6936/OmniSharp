using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Interfaces;
using RLNET;
using RogueSharp;

namespace OmniSharp.Core
{
    public class Stairs : IDrawable
    {
        public RLColor Color
        {
            get; set;
        }


        public char Symbol
        {
            get; set;
        }


        public int X
        {
            get; set;
        }


        public int Y
        {
            get; set;
        }


        public bool IsUp
        {
            get; set;
        }


        public void Draw(RLConsole console, IMap map)
        {
            if ( !map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            Symbol = IsUp ? '<' : '>';

            if ( map.IsInFov(X, Y))
            {
                Color = Core.Color.Player;
            }
            else
            {
                Color = Core.Color.Floor;
            }

            console.Set(X, Y, Color, null, Symbol);
        }
    }
}
