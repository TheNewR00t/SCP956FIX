using MapEditorReborn.Events.EventArgs;
using SCP956Plugin.SCP956;
using System;
using System.Collections.Generic;

namespace SCP956Plugin.Handlers
{
    class SchematicHandler
    {
        public static List<SCP956AI> aIs = new List<SCP956AI> { };

        public void OnSpawn(SchematicSpawnedEventArgs ev)
        {
            if (SCP956Plugin.Instance.Config.SchematicName.Contains(ev.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                SCP956AI aI = ev.Schematic.gameObject.AddComponent<SCP956AI>();
                if (aI.gameObject.transform.position.y == -300f)
                {
                    aI.Spawned = false;
                }
                aI.Name = ev.Schematic.gameObject.name;
                aIs.Add(aI);
            }
        }

        public void OnDestroy(SchematicDestroyedEventArgs ev)
        {
            if (SCP956Plugin.Instance.Config.SchematicName.Contains(ev.Name, StringComparison.InvariantCultureIgnoreCase) && ev.Schematic.gameObject.TryGetComponent<SCP956AI>(out SCP956AI aI))
            {
                aIs.Remove(aI);
            }
        }
    }
}
