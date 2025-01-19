using Exiled.API.Enums;
using Exiled.API.Interfaces;
using InventorySystem.Items.Usables.Scp330;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;
using MapGeneration;
using UnityEngine;

namespace SCP956Plugin
{
    public sealed class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        //basic setting
        [Description("The name of schematic to use as model of SCP-956.")]
        public string[] SchematicName { get; set; } = new string[] { "scp956" };
        [Description("If you change the model, you should set value distance schematic center to bottom point.")]
        public float SchematicOffsetHeight { get; set; } = 0.5f;

        //Spawn
        public FacilityZone[] SpawnableZone { get; set; } = new FacilityZone[]
        {
            FacilityZone.LightContainment,
            FacilityZone.HeavyContainment,
            FacilityZone.Entrance
        };
        [Description("When round starts and after the corresponding time, SCP-956 will automatically spawn.")]
        public float SpawnTimer { get; set; } = 15f;
        public int MaximumCountOfScp956 { get; set; } = 1;
        public bool DoNotOverlapSpawnPosition { get; set; } = true;
        public bool LogsItslocation { get; set; } = true;
        public bool OnlySpawnAtLinkingRoomDoor { get; set; } = false;

        //Desapwn
        [Description("SCP-956 will try despawn when the corresponding time elapses after spawn.")]
        public float DespawnTime { get; set; } = 60f;
        public bool DoNotDespawnWhileBeingWatched { get; set; } = true;
        public bool ResetTimerWhenTargetablePlayerObserved { get; set; } = true;
        [Description("If SCP-956 despawn failed, the corresponding time will be taken to try despawn again.")]
        public float DespawnFailedTryAgainTime { get; set; } = 5f;

        //Targeting
        public ItemType[] TargetingItems { get; set; } = new ItemType[]
        {
            ItemType.SCP330
        };
        public float TargetMaximumDistance { get; set; } = 10f;
        public bool CanTargetThroughWindow { get; set; } = true;
        public bool CanTargetThroughDoor { get; set; } = false;
        public float TimeToTargetOnNormalReason { get; set; } = 7.5f;
        public Faction[] TargetableFaction { get; set; } = new Faction[]
        {
            Faction.FoundationStaff,
            Faction.FoundationEnemy
        };
        [Description("The corresponding roles won't be a target of SCP-956.")]
        public RoleTypeId[] WhitelistRoles { get; set; } = new RoleTypeId[]
        {

        };
        public bool CanTargetScp268 { get; set; } = true;
        [Description("If the corresponding is true, SCP-956 can also target ones that even do not own SCP-330.")]
        public bool TargetEveryone { get; set; } = false;
        [Description("Maybe, It could be laggy and probably loud")]
        public int TargetingAmbient { get; set; } = -1;

        //TargetKilling
        public bool FreezeTarget { get; set; } = true;
        public bool SetTargetInvincible { get; set; } = true;
        public bool LockInventory { get; set; } = true;
        public float TimeToRotateTowardTarget { get; set; } = 6f;
        public float TimeToApproachTarget { get; set; } = 7f;
        public float TimeToCharge { get; set; } = 0.5f;
        public bool DestroyWindowWhileApproaching { get; set; } = true;
        public bool DestryDoorWhileApproaching { get; set; } = false;
        [Description("if the corresponding roles interfere SCP-956 course, SCP-956 will kill them.")]
        public RoleTypeId[] KillableDisturbingRoles { get; set; } = new RoleTypeId[] { };
        public bool OnlyKillDisturbingWhileOwnScp330 { get; set; } = true;
        public string DeathReasonDIsturbingWay { get; set; } = "Get out of my way.";
        public string DeathReason { get; set; } = "Died by pony!";
        public float CooldownToFaceAnotherTarget { get; set; } = 1f;
        public ItemType DeathGunSoundSource { get; set; } = ItemType.GunAK;
        public byte DeathGunSoundLoudness { get; set; } = 50;
        public byte DeathGunSoundClipNum { get; set; } = 0;

        //After Death
        public int CandyDropCount { get; set; } = 20;
        public Dictionary<CandyKindID, float> CandyPercentageWeightPerCandy { get; set; } = new Dictionary<CandyKindID, float>
        {
            { CandyKindID.Blue, 1f },
            { CandyKindID.Green, 1f },
            { CandyKindID.Purple, 1f },
            { CandyKindID.Rainbow, 1f },
            { CandyKindID.Red, 1f },
            { CandyKindID.Yellow, 1f },
            { CandyKindID.Pink, 0f }
        };
        [Description("If it passes the probability below, turns the first candy into a pink candy.")]
        public bool UseSpecialCandyPinkSpawnMethod { get; set; } = true;
        public float SpecialMethodPercentage { get; set; } = 5f;

        //Extension
        public bool Scp956CanHit { get; set; } = false;
        public float Scp956Hp { get; set; } = 300f;
        public bool ExplodeWhenScp956Dead { get; set; } = false;
        public DamageType[] DamagableTypes { get; set; } = new DamageType[]
        {
            DamageType.Firearm,
            DamageType.Explosion
        };
        public bool WhenDieMakeCandies { get; set; } = true;
        public bool AddTargetableWhoDamageScp956 { get; set; } = true;
        public float DamageScp956RememberTimer { get; set; } = 30f;
        public float DamageScp956TargetedTimer { get; set; } = 1.5f;
        public bool PriorAngerTargetingTimer { get; set; } = true;
    }
}
