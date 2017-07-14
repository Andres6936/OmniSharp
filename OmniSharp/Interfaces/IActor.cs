using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniSharp.Interfaces
{
    public interface IActor
    {

        int Attack { get; set; }
        int AttackChance { get; set; }
        int Awareness { get; set; }
        int Defensa { get; set; }
        int DefensaChance { get; set; }
        int Gold { get; set; }
        int Health { get; set; }
        int MaxHealth { get; set; }
        string Name { get; set; }
        int Speed { get; set; }

    }
}
