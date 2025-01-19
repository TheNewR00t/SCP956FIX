using Exiled.API.Features;
using MapEditorReborn.Events.Handlers;
using MEC;
using SCP956Plugin.Handlers;
using SCP956Plugin.SCP956;
using System;
using UnityEngine;
using Exiled.Events.Handlers;
using SchematicHan = SCP956Plugin.Handlers.SchematicHandler;
using PlayerHan = SCP956Plugin.Handlers.PlayerHandler;

namespace SCP956Plugin
{
    public class SCP956Plugin : Plugin<Config>
    {
        public static SCP956Plugin Instance { get; private set; }

        private SchematicHandler SchematicHandler;
        private PlayerHandler PlayerHandler;

        public override void OnEnabled()
        {
            Instance = this;
            RegisterEvent();
        }

        public override void OnDisabled()
        {
            UnRegisterEvent();
            Instance = null;
        }

        void RegisterEvent()
        {
            SchematicHandler = new SchematicHan();
            PlayerHandler = new PlayerHan();

            Schematic.SchematicSpawned += SchematicHandler.OnSpawn;
            Schematic.SchematicDestroyed += SchematicHandler.OnDestroy;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnd;
            Exiled.Events.Handlers.Player.ChangingItem += PlayerHandler.ChangeItem;
            Exiled.Events.Handlers.Player.DroppingItem += PlayerHandler.DropItem;
            Exiled.Events.Handlers.Player.InteractingDoor += PlayerHandler.InteractDoor;
            Exiled.Events.Handlers.Player.InteractingElevator += PlayerHandler.InteractElevator;
            Exiled.Events.Handlers.Player.InteractingLocker += PlayerHandler.InteractLocker;
            Exiled.Events.Handlers.Player.Jumping += PlayerHandler.Jump;
            Exiled.Events.Handlers.Player.Left += PlayerHandler.OnQuit;
            Exiled.Events.Handlers.Player.Shooting += PlayerHandler.OnShooting;
            if (Config.Scp956CanHit)
            {
                if (Config.DamagableTypes.Contains(Exiled.API.Enums.DamageType.Explosion))
                {
                    Exiled.Events.Handlers.Map.ExplodingGrenade += PlayerHandler.OnGrenadeExplode;
                }
            }
        }

        void UnRegisterEvent()
        {
            Schematic.SchematicSpawned -= SchematicHandler.OnSpawn;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Schematic.SchematicDestroyed -= SchematicHandler.OnDestroy;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnd;
            Exiled.Events.Handlers.Player.ChangingItem -= PlayerHandler.ChangeItem;
            Exiled.Events.Handlers.Player.DroppingItem -= PlayerHandler.DropItem;
            Exiled.Events.Handlers.Player.InteractingDoor -= PlayerHandler.InteractDoor;
            Exiled.Events.Handlers.Player.InteractingElevator -= PlayerHandler.InteractElevator;
            Exiled.Events.Handlers.Player.InteractingLocker -= PlayerHandler.InteractLocker;
            Exiled.Events.Handlers.Player.Jumping -= PlayerHandler.Jump;
            Exiled.Events.Handlers.Player.Left -= PlayerHandler.OnQuit;
            Exiled.Events.Handlers.Player.Shooting -= PlayerHandler.OnShooting;
            if (Config.Scp956CanHit)
            {
                if (Config.DamagableTypes.Contains(Exiled.API.Enums.DamageType.Explosion))
                {
                    Exiled.Events.Handlers.Map.ExplodingGrenade -= PlayerHandler.OnGrenadeExplode;
                }
            }

            SchematicHandler = null;
            PlayerHandler = null;
        }

        void OnRoundStart()
        {
            for (int i = 0; i < Config.MaximumCountOfScp956; i++)
            {
                MapEditorReborn.API.Features.ObjectSpawner.SpawnSchematic(SCP956Plugin.Instance.Config.SchematicName.RandomItem(), new Vector3(0f, -300f, 0f), Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), Vector3.one, null);
            }
        }

        void OnRoundEnd(Exiled.Events.EventArgs.Server.RoundEndedEventArgs ev)
        {
            SchematicHandler.aIs.Clear();
        }
    }
}
