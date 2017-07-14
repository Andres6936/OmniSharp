using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Interfaces;
using RogueSharp;
using RLNET;

namespace OmniSharp.Core
{
    public class Door : IDrawable
    {

        //-----------------------------
        // Atributos
        //-----------------------------

        public RLColor Color { get; set; }
        public RLColor BackgroundColor { get; set; }
        public char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsOpen { get; set; }

        //-----------------------------
        // Constructor
        //-----------------------------

        public Door()
        {
            Symbol = '+';
            Color = Core.Color.Door;
            BackgroundColor = Core.Color.DoorBackground;
        }

        //-----------------------------
        // Métodos
        //-----------------------------

        public void Draw( RLConsole console, IMap map )
        {
            if ( !map.GetCell( X, Y ).IsExplored )
            {
                return;
            }

            Symbol = IsOpen ? '-' : '+';

            if ( map.IsInFov( X, Y) )
            {
                Color = Core.Color.DoorFov;
                BackgroundColor = Core.Color.DoorBackgroundFov;
            }
            else
            {
                Color = Core.Color.Door;
                BackgroundColor = Core.Color.DoorBackground;
            }

            console.Set(X, Y, Color, BackgroundColor, Symbol);
        }

    }
}
