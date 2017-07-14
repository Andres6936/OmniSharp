using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Systems;
using OmniSharp.Behaviors;
using RLNET;

namespace OmniSharp.Core
{
    public class Monster : Actor
    {

        public int? TurnsAlerted { get; set; }

        public void DrawStats( RLConsole statConsole, int position )
        {
            // Empezamos en Y=13 con lo cual situamos los stats de los enemigos
            // por debajo de las stats del jugador.
            // Multiplicamos la {position} por 2 para dejar un espacio entre cada stat.
            int yPosition = 13 + (position * 2);

            // Comenzamos la linea imprimiendo el simbolo del enemigo con su color respectivo.
            statConsole.Print(1, yPosition, Symbol.ToString(), Color);

            // Calculamos el ancho de la barra de vida de la siguiente manera:
            // Dividimos la vida actual por el máximo de vida.
            int width = Convert.ToInt32(((double)Health / (double)MaxHealth) * 16.0);
            int remainingWidth = 16 - width;

            // Establecemos el color de fondo de la barra de vida para mostrar el daño hecho al monster.
            statConsole.SetBackColor(3, yPosition, width, 1, Swatch.Primary);
            statConsole.SetBackColor(3 + width, yPosition, remainingWidth, 1, Swatch.PrimaryDarkest);

            // Imprimimos el nombre de los monster arriba sobre la barra de vida.
            statConsole.Print(2, yPosition, $": {Name}", Swatch.DbLight);
        }

        public virtual void PerformAction( CommandSystem commandSystem )
        {
            var behavior = new StandardMoveAndAttack();
            behavior.Act(this, commandSystem);
        }

    }
}
