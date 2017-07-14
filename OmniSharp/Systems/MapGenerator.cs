using OmniSharp.Core;
using OmniSharp.Monsters;
using RogueSharp;
using RogueSharp.DiceNotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniSharp.Systems
{
    public class MapGenerator
    {

        private int _ancho;
        private int _alto;
        private int _maxRooms;
        private int _roomMaxSize;
        private int _roomMinsize;

        private DungeonMap _mazmorra;

        public MapGenerator( int ancho, int alto, int maxRooms, int roomMaxSize, int roomMinsize, int mapLevel )
        {
            _ancho = ancho;
            _alto = alto;
            _maxRooms = maxRooms;
            _roomMaxSize = roomMaxSize;
            _roomMinsize = roomMinsize;

            _mazmorra = new DungeonMap();
        }

        public DungeonMap CreateMap()
        {
            _mazmorra.Initialize(_ancho, _alto);


            for(int r = _maxRooms; r > 0; r--)
            {
                int roomAncho = Game.Random.Next(_roomMinsize, _roomMaxSize);
                int roomAlto = Game.Random.Next(_roomMinsize, _roomMaxSize);
                int roomXPosition = Game.Random.Next(0, _ancho - roomAncho - 1);
                int roomYPosition = Game.Random.Next(0, _alto - roomAlto - 1);

                var newRoom = new Rectangle(roomXPosition, roomYPosition, roomAncho, roomAlto);

                bool newRoomIntersects = _mazmorra.Rooms.Any(room => newRoom.Intersects(room));

                if (!newRoomIntersects)
                {
                    _mazmorra.Rooms.Add(newRoom);
                }
            }


            // Iteramos a través de cada habitación que ha sido generada.
            // No hacemos nada con la primera habitación, en su lugar empezamos en r=1.
            for(int r = 1; r < _mazmorra.Rooms.Count; r++)
            {
                // Para todas la habitaciones restantes obtenemos la coordenada central
                // de la primera habitación y la habitación previa a esta.
                int previousRoomCenterX = _mazmorra.Rooms[r - 1].Center.X;
                int previousRoomCenterY = _mazmorra.Rooms[r - 1].Center.Y;
                int currentRoomCenterX = _mazmorra.Rooms[r].Center.X;
                int currentRoomCenterY = _mazmorra.Rooms[r].Center.Y;

                // Tenemos un 50/50 de posibilidades de que la forma del tunel que
                // conecta las habitaciones sea una "L".
                if(Game.Random.Next(1, 2) == 1)
                {
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY);
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX);
                }
                else
                {
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX);
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY);
                }
            }

            foreach (Rectangle room in _mazmorra.Rooms)
            {
                CreateRoom(room);
                CreateDoors(room);
            }

            CreateStairs();

            PlacePlayer();
            PlaceMonsters();

            return _mazmorra;
        }

        private void CreateRoom( Rectangle room )
        {
            for( int x = room.Left + 1; x < room.Right; x++ )
            {
                for( int y = room.Top + 1; y < room.Bottom; y++ )
                {
                    _mazmorra.SetCellProperties(x, y, true, true, true);
                }
            }
        }

        private void CreateHorizontalTunnel(int xStart, int xEnd, int yPosition)
        {
            for(int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                _mazmorra.SetCellProperties(x, yPosition, true, true);
            }
        }

        private void CreateVerticalTunnel(int yStart, int yEnd, int xPosition)
        {
            for(int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                _mazmorra.SetCellProperties(xPosition, y, true, true);
            }
        }

        private void CreateDoors( Rectangle room )
        {
            // Los límites de la habitación.
            int xMin = room.Left;
            int xMax = room.Right;
            int yMin = room.Top;
            int yMax = room.Bottom;

            // Colocamos las celdas que se encuentran en los bordes de
            // la habitación dentro de una lista.
            List<Cell> borderCells = _mazmorra.GetCellsAlongLine(xMin, yMin, xMax, yMin).ToList();
            borderCells.AddRange(_mazmorra.GetCellsAlongLine(xMin, yMin, xMin, yMax));
            borderCells.AddRange(_mazmorra.GetCellsAlongLine(xMin, yMax, xMax, yMax));
            borderCells.AddRange(_mazmorra.GetCellsAlongLine(xMax, yMin, xMax, yMax));

            // Examinamos cada una de las celdas de la habitación que
            // se encuentra en los bordes y buscamos lo lugares idelas para
            // colocar una puerta.
            foreach( Cell cell in borderCells )
            {
                if ( IsPotencialDoor( cell ))
                {
                    // Una puerta debe bloquear el campo-de-visión cuando está cerrada.
                    _mazmorra.SetCellProperties(cell.X, cell.Y, false, true);
                    _mazmorra.Doors.Add(new Door
                    {
                        X = cell.X,
                        Y = cell.Y,
                        IsOpen = false
                    });
                }
            }
        }

        private void CreateStairs()
        {
            _mazmorra.StairsUp = new Stairs
            {
                X = _mazmorra.Rooms.First().Center.X + 1,
                Y = _mazmorra.Rooms.First().Center.Y,
                IsUp = true
            };

            _mazmorra.StairsDown = new Stairs
            {
                X = _mazmorra.Rooms.Last().Center.X,
                Y = _mazmorra.Rooms.Last().Center.Y,
                IsUp = false
            };

        }

        // Revisamos si una celda es una buena candidata para colocar una puerta.
        private bool IsPotencialDoor( Cell cell )
        {
            // Si la celda es no transitable entonces es un pared y no es
            // un buen lugar para colocar una puerta.
            if ( !cell.IsWalkable )
            {
                return false;
            }

            // Almacenamos las referencias a todas las celdas vecinas.
            Cell right = _mazmorra.GetCell(cell.X + 1, cell.Y);
            Cell left = _mazmorra.GetCell(cell.X - 1, cell.Y);
            Cell top = _mazmorra.GetCell(cell.X, cell.Y - 1);
            Cell botom = _mazmorra.GetCell(cell.X, cell.Y + 1);

            // Nos aseguramos de que no haya una puerta aquí.
            if ( _mazmorra.GetDoor(cell.X, cell.Y) != null ||
                 _mazmorra.GetDoor(right.X, right.Y) != null ||
                 _mazmorra.GetDoor(left.X, left.Y) != null ||
                 _mazmorra.GetDoor(top.X, top.Y) != null ||
                 _mazmorra.GetDoor(botom.X, botom.Y) != null)
            {
                return false;
            }

            // Este es un buen lugar para una puerta y es en la
            // izquierda o derecha de la habitación.
            if ( right.IsWalkable && left.IsWalkable && !top.IsWalkable && !botom.IsWalkable )
            {
                return true;
            }

            // Este es un buen lugar para una puerta y es en la
            // parte de arriba o de abajo de la habitación.
            if ( !right.IsWalkable && !left.IsWalkable && top.IsWalkable && botom.IsWalkable )
            {
                return true;
            }

            return false;
        }

        private void PlacePlayer()
        {
            Player player = Game.Player;
            if(player == null)
            {
                player = new Player();
            }

            player.X = _mazmorra.Rooms[0].Center.X;
            player.Y = _mazmorra.Rooms[0].Center.Y;

            _mazmorra.AddPlayer(player);
        }

        private void PlaceMonsters()
        {
            foreach( var room in _mazmorra.Rooms )
            {
                if( Dice.Roll( "1D10" ) < 7 )
                {
                    var numberOfMonster = Dice.Roll("1D4");

                    for( int i = 0; i < numberOfMonster; i++)
                    {
                        Point randomRoomLocation = _mazmorra.GetRandomWalkableLocationInRoom(room);

                        if( randomRoomLocation != null )
                        {
                            var monster = Kobold.Create(1);
                            monster.X = randomRoomLocation.X;
                            monster.Y = randomRoomLocation.Y;
                            _mazmorra.AddMonster(monster);
                        }
                    }
                }
            }
        }

    }
}
