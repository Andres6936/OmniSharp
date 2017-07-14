using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;

namespace OmniSharp.Core
{
    public class Player : Actor
    {

        public Player()
        {
            Attack = 2;
            AttackChance = 50;
            Awareness = 15;
            Color = Core.Color.Player;
            Defensa = 2;
            DefensaChance = 40;
            Gold = 0;
            Health = 100;
            MaxHealth = 100;
            Name = "Rogue";
            Speed = 10;
            Symbol = '@';
        }

        public void DrawStats (RLConsole statConsole)
        {
            statConsole.Print(1, 1, $"Name    :{Name}", Core.Color.Text);
            statConsole.Print(1, 3, $"Health  :{Health}/{MaxHealth}", Core.Color.Text);
            statConsole.Print(1, 5, $"Attack  :{Attack} ({AttackChance}%)", Core.Color.Text);
            statConsole.Print(1, 7, $"Defensa :{Defensa} ({DefensaChance}%)", Core.Color.Text);
            statConsole.Print(1, 9, $"Gold    :{Gold}", Core.Color.Gold);
        }

    }
}
