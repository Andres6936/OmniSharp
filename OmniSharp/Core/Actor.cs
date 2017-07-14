using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Core;
using OmniSharp.Interfaces;
using RLNET;
using RogueSharp;

namespace OmniSharp.Core
{
    public class Actor : IActor, IDrawable, IScheduleable
    {
        // IActor
        private int _attack;
        private int _attackChance;
        private int _awareness;
        private int _defensa;
        private int _defensaChance;
        private int _gold;
        private int _health;
        private int _maxHealth;
        private string _name;
        private int _speed;

        // IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }


        // --------------------------------------
        // Getter y Setter
        // --------------------------------------

        public int Attack
        {
            get
            {
                return _attack;
            }
            set
            {
                _attack = value;
            }
        }


        public int AttackChance
        {
            get
            {
                return _attackChance;
            }
            set
            {
                _attackChance = value;
            }
        }


        public int Awareness
        {
            get
            {
                return _awareness;
            }
            set
            {
                _awareness = value;
            }
        }


        public int Defensa
        {
            get
            {
                return _defensa;
            }
            set
            {
                _defensa = value;
            }
        }


        public int DefensaChance
        {
            get
            {
                return _defensaChance;
            }
            set
            {
                _defensaChance = value;
            }
        }


        public int Gold
        {
            get
            {
                return _gold;
            }
            set
            {
                _gold = value;
            }
        }


        public int Health
        {
            get
            {
                return _health;
            }
            set
            {
                _health = value;
            }
        }


        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }
            set
            {
                _maxHealth = value;
            }
        }


        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }


        public int Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
            }
        }


        public int Time
        {
            get
            {
                return Speed;
            }
        }


        // --------------------------------------
        // Métodos
        // --------------------------------------

        public void Draw( RLConsole console, IMap map )
        {
            if(!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            if(map.IsInFov(X, Y))
            {
                console.Set(X, Y, Color, Core.Color.FloorBackgroundFov, Symbol);
            }
            else
            {
                console.Set(X, Y, Core.Color.Floor, Core.Color.FloorBackground, '.');
            }
        }

    }
}
