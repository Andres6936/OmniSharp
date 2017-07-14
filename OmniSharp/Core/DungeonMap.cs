using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;

namespace OmniSharp.Core
{
    public class DungeonMap : Map
    {

        //-----------------------------
        // Atributos
        //-----------------------------

        private List<Monster> _monsters;

        public List<Rectangle> Rooms { get; set; }
        public List<Door> Doors { get; set; }

        public Stairs StairsUp { get; set; }
        public Stairs StairsDown { get; set; }

        //-----------------------------
        // Constructor
        //-----------------------------

        public DungeonMap()
        {
            Game.SchedulingSystem.Clear();

            // Inicializamos todas las listas cuando nosotros creamos una nueva DungeonMap.
            _monsters = new List<Monster>();
            Rooms = new List<Rectangle>();
            Doors = new List<Door>();
            
        }

        //-----------------------------
        // Métodos
        //-----------------------------

        public void Draw( RLConsole mazmorraConsole, RLConsole statConsole )
        {
            foreach( Cell cell in GetAllCells( ))
            {
                SetConsoleSymbolForCell(mazmorraConsole, cell);
            }

            // Recordamos el índice para saber en cual posición dibujar la estadísticas del enemigo.
            int indice = 0;

            // Iteramos a través de cada enemigo en el mapa y lo dibujamos depués de haber dibujado las Celdas.
            foreach( Monster monster in _monsters )
            {
                // Cuando el enemigo se encuentre en el FOV (Campo de Visión) del jugador
                // dibujamos sus estadisticas.
                if(IsInFov(monster.X, monster.Y))
                {
                    monster.Draw(mazmorraConsole, this);

                    // Pasamos el índice a DrawStats y luego lo incrementamos.
                    monster.DrawStats(statConsole, indice);
                    indice += 1;
                }
            }

            foreach ( Door door in Doors )
            {
                door.Draw(mazmorraConsole, this);
            }

            StairsUp.Draw(mazmorraConsole, this);
            StairsDown.Draw(mazmorraConsole, this);
        }

        public bool DoesRoomHaveWalkableSpace( Rectangle room )
        {
            for( int x = 1; x <= room.Width - 2; x++ )
            {
                for(int y = 1; y <= room.Height - 2; y++)
                {
                    if (IsWalkable(x + room.X, y + room.Y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // El actor abre la puerta localizada en la coordenada (x, y).
        private void OpenDoor ( Actor actor, int x, int y )
        {
            Door door = GetDoor(x, y);

            if (door != null && !door.IsOpen)
            {
                door.IsOpen = true;
                var cell = GetCell(x, y);

                // Una vez que la puerta se abre se debe de marcar como transparente
                // para que no impida el paso de la luz en el campo-de-visión del jugador.
                SetCellProperties(x, y, true, cell.IsWalkable, cell.IsExplored);

                Game.MessageLog.Add($"{actor.Name} abre la puerta");
            }
        }

        public Point GetRandomWalkableLocationInRoom( Rectangle room )
        {
            if( DoesRoomHaveWalkableSpace( room ))
            {
                for(int i = 0; i < 100; i++)
                {
                    int x = Game.Random.Next(1, room.Width - 2) + room.X;
                    int y = Game.Random.Next(1, room.Height - 2) + room.Y;

                    if (IsWalkable(x, y))
                    {
                        return new Point(x, y);
                    }
                }
            }

            return null;
        }

        public bool CanMoveDownTNextLevel()
        {
            Player player = Game.Player;

            return StairsDown.X == player.X && StairsDown.Y == player.Y;
        }

        public Monster GetMonsterAt( int x, int y )
        {
            return _monsters.FirstOrDefault( m => m.X == x && m.Y == y );
        }

        private void SetConsoleSymbolForCell(RLConsole mazmorraConsole, Cell cell)
        {
            if (!cell.IsExplored)
            {
                return;
            }

            if(IsInFov(cell.X, cell.Y))
            {
                if(cell.IsWalkable)
                {
                    mazmorraConsole.Set(cell.X, cell.Y, Color.FloorFov, Color.FloorBackgroundFov, '.');
                }
                else
                {
                    mazmorraConsole.Set(cell.X, cell.Y, Color.WallFov, Color.WallBackgroundFov, '#');
                }
            }
            else
            {
                if(cell.IsWalkable)
                {
                    mazmorraConsole.Set(cell.X, cell.Y, Color.Floor, Color.FloorBackground, '.');
                }
                else
                {
                    mazmorraConsole.Set(cell.X, cell.Y, Color.Wall, Color.WallBackground, '#');
                }
            }

        }

        // Devuelve una Door en la coordenada (x, y) o null si no encuentra una Door.
        public Door GetDoor( int x, int y )
        {
            return Doors.SingleOrDefault(d => d.X == x && d.Y == y);
        }


        public bool SetActorPosition( Actor actor, int x, int y)
        {
            if (GetCell(x, y).IsWalkable)
            {
                SetIsWalkable(actor.X, actor.Y, true);

                actor.X = x;
                actor.Y = y;

                SetIsWalkable(actor.X, actor.Y, false);

                // Intentamos abrir una puerta si existe una aquí.
                OpenDoor(actor, x, y);

                if(actor is Player)
                {
                    UpdatePlayerFieldOfView();
                }
                return true;
            }
            return false;
        }


        public void SetIsWalkable(int x, int y, bool isWalkable)
        {
            Cell cell = GetCell(x, y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
        }


        public void AddPlayer(Player player)
        {
            Game.Player = player;
            SetIsWalkable(player.X, player.Y, false);
            UpdatePlayerFieldOfView();

            Game.SchedulingSystem.Add(player);
        }

        public void AddMonster ( Monster monster )
        {
            _monsters.Add(monster);
            // Después de añadir el monster al mapa nos aseguramos de que la celda del
            // mapa no sea transitable. (walkable)
            SetIsWalkable(monster.X, monster.Y, false);

            Game.SchedulingSystem.Add(monster);
        }

        public void RemoveMonster( Monster monster )
        {
            _monsters.Remove(monster);
            // Después de remover al enemigo del mapa, nos aseguramos de que la celda
            // sea transitable nuevamente.
            SetIsWalkable(monster.X, monster.Y, true);

            Game.SchedulingSystem.Remove(monster);
        }

        public void UpdatePlayerFieldOfView()
        {
            Player player = Game.Player;

            ComputeFov(player.X, player.Y, player.Awareness, true);

            foreach(Cell cell in GetAllCells())
            {
                if(IsInFov(cell.X, cell.Y))
                {
                    SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                }
            }
        }
    }
}
