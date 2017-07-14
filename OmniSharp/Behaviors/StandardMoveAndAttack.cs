using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Interfaces;
using OmniSharp.Core;
using OmniSharp.Systems;
using RogueSharp;

namespace OmniSharp.Behaviors
{
    public class StandardMoveAndAttack : IBehavior
    {

        public bool Act( Monster monster, CommandSystem commandSystem )
        {
            DungeonMap dungeonMap = Game.DungeonMap;
            Player player = Game.Player;
            FieldOfView monsterFov = new FieldOfView(dungeonMap);

            // Si el enemigo no ha sido alertado, calculamos el campo-de-visión.
            // Usamos el valor de {Awareness} del enemigo para cacular la distancia
            // a la cual puede llegar su campo-de-visión.
            // Si el jugador ésta en el campo-de-visión del enemigo, alertamos al enemigo.
            // Añadimos un mensaje en el MessageLog respecto al estado de esta alarma.
            if ( !monster.TurnsAlerted.HasValue )
            {
                monsterFov.ComputeFov(monster.X, monster.Y, monster.Awareness, true);
                if ( monsterFov.IsInFov( player.X, player.Y ) )
                {
                    Game.MessageLog.Add($"{monster.Name} está ansioso por luchar con {player.Name}");
                    monster.TurnsAlerted = 1;
                }
            }

            if ( monster.TurnsAlerted.HasValue )
            {
                // Antes de encontrar un camino, nos aseguramos que las celdas
                // entre el enemigo y el jugador son transitables.
                dungeonMap.SetIsWalkable(monster.X, monster.Y, true);
                dungeonMap.SetIsWalkable(player.X, player.Y, true);

                PathFinder pathFinder = new PathFinder(dungeonMap);
                Path path = null;

                try
                {
                    path = pathFinder.ShortestPath(
                    dungeonMap.GetCell(monster.X, monster.Y),
                    dungeonMap.GetCell(player.X, player.Y) );
                }
                catch (PathNotFoundException)
                {
                    // El enemigo puede ver al jugador, pero no puede encontrar una ruta hasta él.
                    // Esto podría deberse a que otros enemigos bloquearon el camino.
                    // Añadimos un mensaje en el MessageLog mencionando que el enemigo espera un turno.
                    Game.MessageLog.Add($"{monster.Name} espera un turno");
                }

                // No se olvide de establecer el estado de las celdas a intransitable.
                dungeonMap.SetIsWalkable(monster.X, monster.Y, false);
                dungeonMap.SetIsWalkable(player.X, player.Y, false);


                // En caso de que encontremos un camino, llamamos a CommandSystem a mover al enemigo.
                if ( path != null )
                {
                    try
                    {
                        // TODO: Esto debería de ser path.StepForward(), pero esto es un bug en RogueSharp V3.
                        // El Bug es que un Path devuelve desde un PathFinder no incluye la fuente de celdas.
                        commandSystem.MoveMonster(monster, path.Steps.First());
                    }
                    catch(NoMoreStepsException)
                    {
                        Game.MessageLog.Add($"{monster.Name} ruge de frustración");
                    }
                }

                monster.TurnsAlerted++;

                // Perdemos el estado de alerta después de 15 turnos.
                // Mientras el jugador siga estando en el campo-de-visión del enemigo,
                // esta seguirá estando alerta.
                // De lo contrario el enemigo dejará de perseguir al jugador.
                if ( monster.TurnsAlerted > 15 )
                {
                    monster.TurnsAlerted = null;
                }
            }

            return true;
        }

    }
}
