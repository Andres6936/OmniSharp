using System.Text;
using OmniSharp.Core;
using OmniSharp.Interfaces;
using RogueSharp.DiceNotation;
using RogueSharp;

namespace OmniSharp.Systems
{
    public class CommandSystem
    {

        public bool IsPlayerTurn { get; set; }

        public bool MovePlayer ( Direction direction )
        {
            int x = Game.Player.X;
            int y = Game.Player.Y;

            switch (direction)
            {
                case Direction.Up:
                    {
                        y = Game.Player.Y - 1;
                        break;
                    }
                case Direction.Down:
                    {
                        y = Game.Player.Y + 1;
                        break;
                    }
                case Direction.Left:
                    {
                        x = Game.Player.X - 1;
                        break;
                    }
                case Direction.Right:
                    {
                        x = Game.Player.X + 1;
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            if (Game.DungeonMap.SetActorPosition(Game.Player, x, y))
            {
                return true;
            }

            Monster monster = Game.DungeonMap.GetMonsterAt(x, y);

            if ( monster != null )
            {
                Attack(Game.Player, monster);
                return true;
            }

            return false;
        }

        public void Attack ( Actor attacker, Actor defender )
        {
            StringBuilder attackMessage = new StringBuilder();
            StringBuilder defenseMessage = new StringBuilder();

            int hits = ResolveAttack(attacker, defender, attackMessage);

            int blocks = ResolveDefense(defender, hits, attackMessage, defenseMessage);

            Game.MessageLog.Add(attackMessage.ToString());

            if(!string.IsNullOrWhiteSpace(defenseMessage.ToString()))
            {
                Game.MessageLog.Add(defenseMessage.ToString());
            }

            int damage = hits - blocks;

            ResolveDamage(defender, damage);
        }

        // El atacante tira en función de sus estadísticas para ver si obtiene
        // algún resultado.
        private static int ResolveAttack( Actor attacker, Actor defender, StringBuilder attackMessage)
        {
            int hits = 0;

            attackMessage.AppendFormat("{0} ataca a {1} y tira: ", attacker.Name, defender.Name);

            // Rodamos un número de 100 dados de igual lados, que será igual al valor de
            // ataque del atacante {attacker}.
            DiceExpression attackDice = new DiceExpression().Dice(attacker.Attack, 100);
            DiceResult attackResult = attackDice.Roll();

            // Miramos el valor de la cara de cada dado que fue rodado.
            foreach ( TermResult termResult in attackResult.Results )
            {
                attackMessage.Append(termResult.Value + ", ");
                // Comparamos el valor a 100, menos la probabilidad de
                // ataque y añadimos el golpe si este es mayor.
                if ( termResult.Value >= 100 - attacker.AttackChance )
                {
                    hits += 1;
                }
            }

            return hits;
        }

        // El defensor tira el dado basado en sus estadísticas para ver si
        // bloquea cuaqluiera de los golpes del atacante.
        private static int ResolveDefense( Actor defender, int hits, StringBuilder attackMessage, StringBuilder defenderMessage)
        {
            int blocks = 0;

            if ( hits > 0 )
            {
                attackMessage.AppendFormat("puntuación {0} golpes.", hits);
                defenderMessage.AppendFormat(" {0} defiende y tira: ", defender.Name);

                // Rodamos un número de 100 dados de igual lado con el valor de la
                // defensa del defensor.
                DiceExpression defenseDice = new DiceExpression().Dice(defender.Defensa, 100);
                DiceResult defenseRoll = defenseDice.Roll();

                // Miramos el valor de la cara de cada dado que fue rodado.
                foreach( TermResult termResult in defenseRoll.Results )
                {
                    defenderMessage.Append(termResult.Value + ", ");
                    // Comparamos el valor a 100, menos la probabilidad de
                    // defender y añadimos un bloqueo si este es mayor.
                    if ( termResult.Value >= 100 - defender.DefensaChance )
                    {
                        blocks += 1;
                    }
                }

                defenderMessage.AppendFormat("resultando en {0} bloqueo(s).", blocks);
            }
            else
            {
                attackMessage.Append("y falla completamente.");
            }

            return blocks;
        }

        // Aplicamos cualquier tipo daño que el defensor no haya bloqueado.
        private static void ResolveDamage( Actor defender, int damage )
        {
            if( damage > 0 )
            {
                defender.Health = defender.Health - damage;

                Game.MessageLog.Add($" {defender.Name} ha sido golpeado por {damage} de daño");

                if(defender.Health <= 0)
                {
                    ResolveDeath(defender);
                }
            }
            else
            {
                Game.MessageLog.Add($" {defender.Name} bloquea el ataque");
            }
        }

        // Removemos al defensor del mapa y añadimos algunos mensajes acerca de su muerte.
        private static void ResolveDeath( Actor defender )
        {
            if(defender is Player)
            {
                Game.MessageLog.Add($" {defender.Name} ha muerto. ¡FIN DEL JUEGO!");
            }
            else if(defender is Monster)
            {
                Game.DungeonMap.RemoveMonster((Monster)defender);
                Game.MessageLog.Add($"{defender.Name} ha muerto y ha soltado {defender.Gold} monedas de oro");
            }
        }

        public void EndPlayerTurn()
        {
            IsPlayerTurn = false;
        }

        public void ActivateMonster()
        {
            IScheduleable scheduleable = Game.SchedulingSystem.Get();

            if ( scheduleable is Player )
            {
                IsPlayerTurn = true;
                Game.SchedulingSystem.Add(Game.Player);
            }
            else
            {
                Monster monster = scheduleable as Monster;

                if ( monster != null )
                {
                    monster.PerformAction(this);
                    Game.SchedulingSystem.Add(monster);
                }

                ActivateMonster();
            }
        }

        public void MoveMonster( Monster monster, Cell cell )
        {
            if ( !Game.DungeonMap.SetActorPosition(monster, cell.X, cell.Y) )
            {
                if ( Game.Player.X == cell.X && Game.Player.Y == cell.Y )
                {
                    Attack(monster, Game.Player);
                }
            }
        }

    }
}
