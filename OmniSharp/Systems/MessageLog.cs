using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;

namespace OmniSharp.Systems
{
    // Representa una cola (Queue) de mensajes que pueden ser añadidos
    // y eliminados. Estos mensajes tienen un método que los dibuja
    // en un RLConsole.
    public class MessageLog
    {
        // Define el número máximo de lineas que puede almacenar la cola (Queue).
        private static int _maxLines = 9;

        // Usa una cola (Queue) para recordar las lineas de texto que serán mostradas.
        // La primera linea en ser añadida es la primera en ser eliminada.
        private Queue<string> _lines;

        public MessageLog()
        {
            _lines = new Queue<string>();
        }

        // Añadimos una linea a la cola (Queue) de MesssageLog.
        public void Add(string message)
        {
            _lines.Enqueue(message);

            // Cuando excedamos el número máximo de lineas removemos la linea vieja.
            if(_lines.Count > _maxLines)
            {
                _lines.Dequeue();
            }
        }

        // Dibujamos cada linea que hay en la cola (Queue) de MessageLog en la consola.
        public void Draw( RLConsole console )
        {
            string[] lines = _lines.ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                console.Print(1, i + 1, lines[i], RLColor.White);
            }
        }

    }
}
