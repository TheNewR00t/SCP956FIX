using Exiled.Events.EventArgs.Player;
using SCP956Plugin.SCP956;

namespace SCP956Plugin.Handlers
{
    public class PlayerHandler
    {
        bool IsAllowed(ReferenceHub hub)
        {
            if (SCP956Plugin.Instance.Config.LockInventory)
            {
                foreach (SCP956AI aI in SchematicHandler.aIs)
                {
                    if (aI.Targeted.Contains(hub))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void ChangeItem(ChangingItemEventArgs ev)
        {
            ev.IsAllowed = IsAllowed(ev.Player.ReferenceHub) ? ev.IsAllowed : false;
        }

        public void DropItem(DroppingItemEventArgs ev)
        {
            ev.IsAllowed = IsAllowed(ev.Player.ReferenceHub) ? ev.IsAllowed : false;
        }

        public void InteractDoor(InteractingDoorEventArgs ev)
        {
            ev.IsAllowed = IsAllowed(ev.Player.ReferenceHub) ? ev.IsAllowed : false;
        }

        public void InteractElevator(InteractingElevatorEventArgs ev)
        {
            ev.IsAllowed = IsAllowed(ev.Player.ReferenceHub) ? ev.IsAllowed : false;
        }

        public void InteractLocker(InteractingLockerEventArgs ev)
        {
            ev.IsAllowed = IsAllowed(ev.Player.ReferenceHub) ? ev.IsAllowed : false;
        }

        public void Jump(JumpingEventArgs ev)
        {
            ev.IsAllowed = IsAllowed(ev.Player.ReferenceHub) ? ev.IsAllowed : false;
        }

        public void OnQuit(LeftEventArgs ev)
        {
            foreach (SCP956AI aI in SchematicHandler.aIs)
            {
                if (aI.Target == ev.Player.ReferenceHub)
                {
                    aI.Target = null;
                }
                aI.Targetable.Remove(ev.Player.ReferenceHub);
                aI.Targeted.Remove(ev.Player.ReferenceHub);
            }
        }

        public void OnGrenadeExplode(Exiled.Events.EventArgs.Map.ExplodingGrenadeEventArgs ev)
        {
            foreach (SCP956AI aI in SchematicHandler.aIs)
            {
                aI.OnExplosion(ev);
            }
        }

        public void OnShooting(ShootingEventArgs ev)
        {
            ev.IsAllowed = IsAllowed(ev.Player.ReferenceHub) ? ev.IsAllowed : false;
            if (ev.IsAllowed && SCP956Plugin.Instance.Config.Scp956CanHit && SCP956Plugin.Instance.Config.DamagableTypes.Contains(Exiled.API.Enums.DamageType.Firearm))
            {
                foreach (SCP956AI aI in SchematicHandler.aIs)
                {
                    aI.OnShot(ev);
                }
            }
        }
    }
}
