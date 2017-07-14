using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using OmniSharp.Core;
using OmniSharp.Systems;
using RogueSharp.Random;

namespace OmniSharp
{
    public static class Game
    {

        private static int screenWidth = 100;
        private static int screenHeight = 70;
        private static RLRootConsole rootConsole;

        private static int mazmorraWidth = 80;
        private static int mazmorraHeight = 48;
        private static RLConsole mazmorraConsole;

        private static int mensajeWidth = 80;
        private static int mensajeHeight = 11;
        private static RLConsole mensajeConsole;

        private static int estadisticasWidth = 20;
        private static int estadisticasHeight = 70;
        private static RLConsole estadisticaConsole;

        private static int inventarioWidth = 80;
        private static int inventarioHeight = 11;
        private static RLConsole inventarioConsole;

        private static bool _renderRequired = true;
        private static int _mapLevel = 1;

        public static Player Player { get; set; }
        public static DungeonMap DungeonMap { get; private set; }
        public static CommandSystem CommandSystem { get; private set; }
        public static MessageLog MessageLog { get; set; }
        public static IRandom Random { get; set; }
        public static SchedulingSystem SchedulingSystem { get; private set; }


        public static void Main(string[] args)
        {
            // Establece la semilla desde un número random obtenido desde el tiempo actual.
            int seed = (int)DateTime.UtcNow.Ticks;
            Random = new DotNetRandom(seed);

            string fontFileName = "terminal8x8.png";

            string consoleTitle = $"OmniSharp - Level {_mapLevel} - Seed {seed}";

            rootConsole = new RLRootConsole(fontFileName, screenWidth, screenHeight, 8, 8, 1f, consoleTitle);

            MessageLog = new MessageLog();
            MessageLog.Add("El Rogue ha llegado al nivel 1");
            MessageLog.Add($"Mazmorra generada con la semilla '{seed}'");

            mazmorraConsole = new RLConsole(mazmorraWidth, mazmorraHeight);
            mensajeConsole = new RLConsole(mensajeWidth, mensajeHeight);
            estadisticaConsole = new RLConsole(estadisticasWidth, estadisticasHeight);
            inventarioConsole = new RLConsole(inventarioWidth, inventarioHeight);

            SchedulingSystem = new SchedulingSystem();

            MapGenerator mapGenerator = new MapGenerator(mazmorraWidth, mazmorraHeight, 20, 13, 7, _mapLevel);
            DungeonMap = mapGenerator.CreateMap();
            DungeonMap.UpdatePlayerFieldOfView();

            CommandSystem = new CommandSystem();

            rootConsole.Render += RootConsole_Render;
            rootConsole.Update += RootConsole_Update; 

            rootConsole.Run();

        }

        private static void RootConsole_Update(object sender, UpdateEventArgs e)
        {

            bool didPlayerAct = false;
            RLKeyPress keyPress = rootConsole.Keyboard.GetKeyPress();

            if ( CommandSystem.IsPlayerTurn )
            {
                if (keyPress != null)
                {
                    if (keyPress.Key == RLKey.Up)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Up);
                    }
                    else if (keyPress.Key == RLKey.Down)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Down);
                    }
                    else if (keyPress.Key == RLKey.Left)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Left);
                    }
                    else if (keyPress.Key == RLKey.Right)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Right);
                    }
                    else if (keyPress.Key == RLKey.Escape)
                    {
                        rootConsole.Close();
                    }
                    else if (keyPress.Key == RLKey.Space)
                    {
                        if ( DungeonMap.CanMoveDownTNextLevel() )
                        {
                            MapGenerator mapGenerator = new MapGenerator(mazmorraWidth, mazmorraHeight, 20, 13, 7, ++_mapLevel);
                            DungeonMap = mapGenerator.CreateMap();
                            MessageLog = new MessageLog();
                            CommandSystem = new CommandSystem();
                            rootConsole.Title = $"OmniSharp - Level {_mapLevel}";
                            didPlayerAct = true;
                        }
                    }
                }

                if (didPlayerAct)
                {
                    _renderRequired = true;
                    CommandSystem.EndPlayerTurn();
                }
            }
            else
            {
                CommandSystem.ActivateMonster();
                _renderRequired = true;
            }
            

        }

        private static void RootConsole_Render(object sender, UpdateEventArgs e)
        {

            if (_renderRequired)
            {
                mazmorraConsole.Clear();
                estadisticaConsole.Clear();
                mensajeConsole.Clear();

                DungeonMap.Draw( mazmorraConsole, estadisticaConsole );
                Player.Draw( mazmorraConsole, DungeonMap );
                Player.DrawStats(estadisticaConsole);
                MessageLog.Draw(mensajeConsole);

                RLConsole.Blit(mazmorraConsole, 0, 0, mazmorraWidth, mazmorraHeight, rootConsole, 0, inventarioHeight);

                RLConsole.Blit(estadisticaConsole, 0, 0, estadisticasWidth, estadisticasHeight, rootConsole, mazmorraWidth, 0);

                RLConsole.Blit(mensajeConsole, 0, 0, mensajeWidth, mensajeHeight, rootConsole, 0, screenHeight - mensajeHeight);

                RLConsole.Blit(inventarioConsole, 0, 0, inventarioWidth, inventarioHeight, rootConsole, 0, 0);

                rootConsole.Draw();

                _renderRequired = false;
            }

        }
    }
}
