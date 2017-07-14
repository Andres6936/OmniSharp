using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Interfaces;

namespace OmniSharp.Systems
{
    public class SchedulingSystem
    {
        private int _time;
        private SortedDictionary<int, List<IScheduleable>> _scheduleables;

        public SchedulingSystem()
        {
            _time = 0;
            _scheduleables = new SortedDictionary<int, List<IScheduleable>>();
        }

        // Añadimos un nuevo objecto al schedule.
        // Colocamos el tiempo actual más la propiedad del tiempo del objecto.
        public void Add( IScheduleable scheduleable )
        {
            int key = _time + scheduleable.Time;

            if ( !_scheduleables.ContainsKey( key ) )
            {
                _scheduleables.Add(key, new List<IScheduleable>());
            }

            _scheduleables[key].Add(scheduleable);
        }

        // Removemos el objecto especificado del schedule.
        // Útil para cuando un enemigo es asesinado para quitarlo del schedule
        // antes de que vuelva a aparecer en la siguiente acción.
        public void Remove( IScheduleable scheduleable )
        {
            KeyValuePair<int, List<IScheduleable>> scheduleableListFound
                = new KeyValuePair<int, List<IScheduleable>>(-1, null);

            foreach( var scheduleableList in _scheduleables )
            {
                if ( scheduleableList.Value.Contains( scheduleable ) )
                {
                    scheduleableListFound = scheduleableList;
                    break;
                }
            }

            if ( scheduleableListFound.Value != null )
            {
                scheduleableListFound.Value.Remove(scheduleable);

                if ( scheduleableListFound.Value.Count <= 0 )
                {
                    _scheduleables.Remove(scheduleableListFound.Key);
                }
            }
        }

        // Obtenemos el siguiente objecto cuyo turno es siguiente en el schedule.
        // Avanzamos el tiempo si es necesario.
        public IScheduleable Get()
        {
            var firstScheduleableGroup = _scheduleables.First();
            var firstScheduleable = firstScheduleableGroup.Value.First();

            Remove(firstScheduleable);

            _time = firstScheduleableGroup.Key;

            return firstScheduleable;
        }

        // Obtenemos el tiempo actual (turno) desde el schedule.
        public int GetTime()
        {
            return _time;
        }

        // Reseteamos el tiempo y limpiamos el schedule.
        public void Clear()
        {
            _time = 0;
            _scheduleables.Clear();
        }
    }
}
