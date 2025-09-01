using CustomPlayerEffects;
using Exiled.API.Features;
using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using InventorySystem;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp330;
using MapGeneration;
using InventorySystem.Items.ThrowableProjectiles;
using Exiled.API.Features.Items;
using Mirror;
using Utils;
using Utils.Networking;
using System.Runtime.InteropServices;
using Exiled.Events.EventArgs.Player;
using System;
using InventorySystem.Items.Firearms.Modules;
using Exiled.API.Enums;

namespace SCP956Plugin.SCP956
{
    public class SCP956AI : MonoBehaviour
    {
        void Start()
        {
            this.lerpRot = UnityEngine.Random.Range(0f, 360f);
            this.gameObject.transform.rotation = Quaternion.Euler(0f, lerpRot, 0f);
            this.gameObject.transform.position += new Vector3(0f, config.SchematicOffsetHeight, 0f);
            this.lerpPos = gameObject.transform.position;
            this._spawnPos = this.gameObject.transform.position;
            this.rotateTime = config.TimeToRotateTowardTarget;
            this.moveTime = config.TimeToApproachTarget;
            this.chargeTime = config.TimeToCharge;
            this.coolTime = config.CooldownToFaceAnotherTarget;
            if (config.Scp956CanHit)
            {
                Health = config.Scp956Hp;
            }
        }

        void Update()
        {
            this.statusTimer += Time.deltaTime;
            if (Spawned)
            {
                if (this.IsFlying)
                {
                    lerpPos.y += Mathf.Sin(Time.timeSinceLevelLoad * 3.14f) * 0.15f;
                }
                if ((this.transform.position - lerpPos).sqrMagnitude > 9f)
                {
                    this.transform.position = this.lerpPos;
                }
                this.transform.SetPositionAndRotation(Vector3.Lerp(this.transform.position, lerpPos, Time.deltaTime * 8f), Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(Vector3.up * this.lerpRot), Time.deltaTime * 10f));
                this.lerpPos = this.transform.position;
                this.lerpRot = this.transform.rotation.eulerAngles.y;
                if (statusTimer >= (DestroyNow ? config.DespawnFailedTryAgainTime : config.DespawnTime))
                {
                    if (!TryDespawn())
                    {
                        DestroyNow = true;
                        statusTimer = 0f;
                    }
                    else
                    {
                        DestroyScp956();
                    }
                }
                Timer += Time.deltaTime;
                foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                {
                    if (hub.isLocalPlayer)
                    {
                        continue;
                    }
                    TargetingReason reason;
                    if (!PlayerCheck(hub, out reason))
                    {
                        Targetable.Remove(hub);
                        Targeted.Remove(hub);
                        TimerPerReferenceHub.Remove(hub);
                        TurnEffects(hub, false);
                    }
                    else
                    {
                        if (!Targetable.Keys.Contains(hub))
                        {
                            Targetable.Add(hub, reason);
                            TimerPerReferenceHub.Add(hub, Timer + TimePerReason[reason]);
                            if (config.ResetTimerWhenTargetablePlayerObserved)
                            {
                                DestroyNow = false;
                                statusTimer = 0f;
                            }
                        }
                        else if (TimerPerReferenceHub.TryGetValue(hub, out float time) && Timer >= time && !Targeted.Contains(hub))
                        {
                            Targeted.Add(hub);
                            TurnEffects(hub, true);
                        }
                        else if (config.TargetingAmbient != -1 && Player.TryGet(hub, out Player target))
                        {
                            Exiled.API.Extensions.MirrorExtensions.SendFakeTargetRpc(target, ReferenceHub.GetHub(Server.Host.ReferenceHub).networkIdentity, typeof(AmbientSoundPlayer), "RpcPlaySound", new object[]
                            {
                            config.TargetingAmbient
                            });
                        }
                    }
                }
                bool flag = Target == null;
                if (!flag && !PlayerCheck(Target, out TargetingReason reason1))
                {
                    Target = null;
                    flag = true;
                }
                if (flag)
                {
                    if (!SetTarget())
                    {
                        return;
                    }
                }
                TargetPos = (Target.roleManager.CurrentRole as FpcStandardRoleBase).FpcModule.Position;
                statusTimer = 0f;
                DestroyNow = false;
                this._sequenceTimer += Time.deltaTime;
                Vector3 normalized = (TargetPos - this.transform.position).normalized;
                float b = Vector3.Angle(normalized, Vector3.forward) * Mathf.Sign(Vector3.Dot(normalized, Vector3.right));
                this.lerpRot = Mathf.LerpAngle(this._initialRot, b, (this._sequenceTimer - coolTime) / (rotateTime - (coolTime + rotateTime / 6f)));
                if (this._sequenceTimer < rotateTime)
                {
                    return;
                }
                Vector3 b2 = this.TargetPos - this.gameObject.transform.forward.NormalizeIgnoreY();
                if (Mathf.Abs(this._spawnPos.y - this.TargetPos.y) < 1.2f)
                {
                    b2.y = this._spawnPos.y;
                    this.IsFlying = false;
                }
                else
                {
                    b2.y = this.TargetPos.y;
                    this.IsFlying = true;
                }
                this.lerpPos = Vector3.Lerp(this._initialPos, b2, (this._sequenceTimer - rotateTime) / moveTime);
                if (Physics.Raycast(this.gameObject.transform.position, b2 - this._initialPos, out RaycastHit hit, 1f))
                {
                    DestroyDisturb(hit);
                }
                if (this._sequenceTimer < chargeTime + moveTime + rotateTime - 0.2f)
                {
                    return;
                }
                if (!triggered)
                {
                    TurnEffects(Target, false, true);
                    triggered = true;
                    return;
                }
                if (this._sequenceTimer < chargeTime + moveTime + rotateTime)
                {
                    return;
                }
                CreateCandies(normalized, Target, Vector3.zero);
                Player.List.ToArray().ForEach(x => { if (Vector3.Distance(x.Position, transform.position) < 15f) x.PlayGunSound(config.DeathGunSoundSource, (byte)(config.DeathGunSoundLoudness / Mathf.RoundToInt(Mathf.Max(1f, Vector3.Distance(x.Position, transform.position)))), config.DeathGunSoundClipNum); });
                this.Target.playerStats.DealDamage(new CustomReasonDamageHandler(config.DeathReason));
                Targeted.Remove(Target);
                SetTarget();
                return;
            }
            else
            {
                if (statusTimer >= config.SpawnTimer && config.SpawnableZone.Length != 0)
                {
                    Vector3 pos;
                    DoorVariant door;
                    statusTimer = 0f;
                    if (!TryGetSpawnPos(out pos, out door))
                    {
                        return;
                    }
                    this.lerpPos = pos;
                    _spawnPos = pos;
                    IsFlying = false;
                    this.gameObject.transform.position = pos;
                    CurrentDoor = door;
                    this.lerpRot = UnityEngine.Random.Range(0f, 360f);
                    this.gameObject.transform.rotation = Quaternion.Euler(0f, this.lerpRot, 0f);
                    DoorList.Add(door);
                    Spawned = true;
                    if (config.Scp956CanHit)
                    {
                        Health = config.Scp956Hp;
                    }
                    return;
                }
            }
        }

        private bool SetTarget()
        {
            Target = null;
            float MinDistance = float.MaxValue;
            foreach (ReferenceHub referenceHub in Targeted)
            {
                float sqrMagnitude = Vector3.Distance(referenceHub.PlayerCameraReference.position, this.gameObject.transform.position);
                if (sqrMagnitude <= MinDistance)
                {
                    Target = referenceHub;
                    MinDistance = sqrMagnitude;
                }
            }
            if (Target == null)
            {
                return false;
            }
            this._sequenceTimer = 0f;
            triggered = false;
            this._initialRot = this.lerpRot;
            this._initialPos = this.lerpPos;
            return true;
        }

        public void OnShot(Exiled.Events.EventArgs.Player.ShotEventArgs ev)
        {
            if (ev.RaycastHit.collider.transform.root == this.transform)
            {
                Damage(ev.Damage, ev.Player.ReferenceHub);
            }
        }

        public void OnExplosion(Exiled.Events.EventArgs.Map.ExplodingGrenadeEventArgs ev)
        {
            if (CheckVisibility(this.transform.position, ev.Position) && Vector3.Distance(ev.Position, this.transform.position) <= 9f)
            {
                Damage(400f / Mathf.Max(1f, Mathf.Pow(Vector3.Distance(ev.Position, this.transform.position), 2f)), ev.Player.ReferenceHub);
            }
        }

        public void Damage(float damage, ReferenceHub attacker)
        {
            if (Health <= 0f || !Spawned)
            {
                return;
            }
            Health -= damage;
            Hitmarker.SendHitmarkerDirectly(attacker, 1f);
            if (config.AddTargetableWhoDamageScp956)
            {
                TimerForAnger[attacker] = config.DamageScp956RememberTimer + Timer;
            }
            if (Health <= 0f)
            {
                if (config.WhenDieMakeCandies)
                {
                    CreateCandies(Vector3.zero, attacker, gameObject.transform.position);
                }
                if (config.ExplodeWhenScp956Dead)
                {
                    ItemBase itemBase;
                    ThrowableItem throwableItem;
                    ExplosionGrenade explosionGrenade;
                    if (InventoryItemLoader.AvailableItems.TryGetValue(ItemType.GrenadeHE, out itemBase) && (throwableItem = (itemBase as ThrowableItem)) != null && (explosionGrenade = (throwableItem.Projectile as ExplosionGrenade)) != null)
                    {
                        ExplosionUtils.ServerSpawnEffect(this.transform.position, ItemType.GrenadeHE);
                        ExplosionGrenade.Explode(new Footprinting.Footprint(attacker), this.transform.position, explosionGrenade, ExplosionType.PinkCandy);
                    }
                }
                DestroyScp956();
            }
        }

        void DestroyScp956()
        {
            DestroyNow = false;
            this.gameObject.transform.position = new Vector3(0f, -300f, 0f);
            statusTimer = 0f;
            DoorList.Remove(CurrentDoor);
            CurrentDoor = null;
            Timer = 0f;
            TimerPerReferenceHub.Clear();
            Targetable.Clear();
            TimerForAnger.Clear();
            Target = null;
            Spawned = false;
            TargetPos = Vector3.zero;
            foreach (ReferenceHub hub in Targeted)
            {
                TurnEffects(hub, false);
            }
            Targeted.Clear();
        }

        public enum TargetingReason
        {
            NormalCondition,
            AngerScp956
        }

        public Dictionary<TargetingReason, float> TimePerReason = new Dictionary<TargetingReason, float>
        {
            {TargetingReason.NormalCondition, SCP956Plugin.Instance.Config.TimeToTargetOnNormalReason},
            {TargetingReason.AngerScp956, SCP956Plugin.Instance.Config.DamageScp956TargetedTimer }
        };

        ExplosionGrenade grenade = null;

        public bool TryDespawn(bool Override = false)
        {
            if (config.DoNotDespawnWhileBeingWatched && !Override)
            {
                Vector3 currentPos = this.gameObject.transform.position;
                foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                {
                    if (hub.isLocalPlayer)
                    {
                        continue;
                    }
                    if (CheckVisibility(currentPos, (hub.roleManager.CurrentRole as FpcStandardRoleBase).FpcModule.Position))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool PlayerCheck(ReferenceHub hub, out TargetingReason reason)
        {
            reason = TargetingReason.NormalCondition;
            if (hub.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Spectator)
            {
                return false;
            }
            Vector3 position = this.gameObject.transform.position;
            Vector3 position2 = (hub.roleManager.CurrentRole as FpcStandardRoleBase).FpcModule.Position;
            float num = config.TargetMaximumDistance * config.TargetMaximumDistance;
            if (!config.TargetableFaction.Contains(hub.roleManager.CurrentRole.Team.GetFaction()) || config.WhitelistRoles.Contains(hub.roleManager.CurrentRole.RoleTypeId) || (!config.CanTargetScp268 && hub.playerEffectsController.GetEffect<Invisible>().IsEnabled))
            {
                return false;
            }
            if ((position - position2).sqrMagnitude > num || !this.CheckVisibility(position, position2))
            {
                return false;
            }
            bool flag = TimerForAnger.TryGetValue(hub, out float time) && Timer <= time;
            if (hub.inventory.UserInventory.Items.Any((KeyValuePair<ushort, ItemBase> x) => config.TargetingItems.Contains(x.Value.ItemTypeId)) || config.TargetEveryone)
            {
                if (config.PriorAngerTargetingTimer && flag)
                {
                    reason = TargetingReason.AngerScp956;
                }
                return true;
            }
            reason = TargetingReason.AngerScp956;
            return flag;
        }

        public void CreateCandies(Vector3 _velocity, ReferenceHub ply, Vector3 vector3)
        {
            _velocity = (_velocity * 3f + Vector3.up) * 9f;
            Scp330Bag scp330Bag;
            if (!InventoryItemLoader.TryGetItem<Scp330Bag>(ItemType.SCP330, out scp330Bag))
            {
                return;
            }
            if (vector3 == Vector3.zero)
            {
                vector3 = ply.transform.position;
            }
            int num = (UnityEngine.Random.value < config.SpecialMethodPercentage * 0.05f && config.UseSpecialCandyPinkSpawnMethod) ? 1 : 0;
            for (int i = 0; i < SCP956Plugin.Instance.Config.CandyDropCount; i++)
            {
                PickupSyncInfo psi = new PickupSyncInfo(ItemType.SCP330, scp330Bag.Weight);
                Scp330Pickup scp330Pickup = ply.inventory.ServerCreatePickup(scp330Bag, psi, true) as Scp330Pickup;
                if (!(scp330Pickup == null) && scp330Pickup.gameObject.TryGetComponent(out Rigidbody rigidbody))
                {
                    CandyKindID candyKindID = (num-- > 0) ? CandyKindID.Pink : TryGet();
                    scp330Pickup.StoredCandies.Add(candyKindID);
                    scp330Pickup.NetworkExposedCandy = candyKindID;
                    rigidbody.linearVelocity = _velocity;
                    rigidbody.position = vector3;
                }
            }
        }

        CandyKindID TryGet()
        {
            float total = config.CandyPercentageWeightPerCandy.Values.Sum();
            if (total == 0)
            {
                return CandyKindID.Red;
            }
            total = UnityEngine.Random.Range(0f, total);
            foreach (KeyValuePair<CandyKindID, float> pair in config.CandyPercentageWeightPerCandy)
            {
                if ((total -= pair.Value) <= 0f)
                {
                    return pair.Key;
                }
            }
            return CandyKindID.Red;
        }

        void TurnEffects(ReferenceHub hub, bool Enable, bool Override = false)
        {
            if (!Enable && !Override)
            {
                foreach (SCP956AI aI in Handlers.SchematicHandler.aIs)
                {
                    if (aI != this && aI.Targeted.Contains(hub))
                    {
                        return;
                    }
                }
            }
            if (config.FreezeTarget)
            {
                if (Enable)
                {
                    hub.playerEffectsController.EnableEffect<Ensnared>();
                }
                else
                {
                    hub.playerEffectsController.DisableEffect<Ensnared>();
                }
            }
            if (config.SetTargetInvincible)
            {
                hub.characterClassManager.GodMode = Enable;
            }
            if (Enable && config.LockInventory)
            {
                hub.inventory.ServerSelectItem(0);
            }
        }

        private bool CheckVisibility(Vector3 checkPos, Vector3 humanPos)
        {
            return !Physics.Linecast(humanPos, checkPos, out RaycastHit hit, WallMask) || hit.collider.transform.root.gameObject.TryGetComponent<SCP956AI>(out SCP956AI aI);
        }

        public bool TryGetSpawnPos(out Vector3 pos, out DoorVariant doo)
        {
            pos = Vector3.zero;
            doo = null;
            Dictionary<ZoneType, int> ZoneIntensity = new Dictionary<ZoneType, int> { };
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub.isLocalPlayer)
                {
                    continue;
                }
                if (!(hub.roleManager.CurrentRole is FpcStateProcessor))
                {
                    continue;
                }

                Player player = Player.Get(hub);
                Room room = Room.Get(player.CurrentRoom.Type);
                if (room != null && config.SpawnableZone.Contains(room.Zone))
                {
                    int num;
                    ZoneIntensity.TryGetValue(room.Zone, out num);
                    ZoneIntensity[room.Zone] = num + 1;
                }
            }
            ZoneType zone = ZoneType.Other;
            if (ZoneIntensity.Count == 0)
            {
                zone = config.SpawnableZone.RandomItem();
            }
            else
            {
                int num2 = 0;
                foreach (KeyValuePair<ZoneType, int> keyValuePair in ZoneIntensity)
                {
                    if (keyValuePair.Value >= num2)
                    {
                        zone = keyValuePair.Key;
                        num2 = keyValuePair.Value;
                    }
                }
            }
            List<DoorVariant> list = new List<DoorVariant> { };
            foreach (DoorVariant door in DoorVariant.AllDoors)
            {
                if (door is BreakableDoor && door.Rooms != null && door.Rooms.Length != 0 && door.Rooms[0].Zone == (FacilityZone)zone)
                {
                    if (config.DoNotOverlapSpawnPosition && DoorList.Contains(door))
                    {
                        continue;
                    }
                    if (config.OnlySpawnAtLinkingRoomDoor && door.Rooms.Length == 1)
                    {
                        continue;
                    }
                    list.Add(door);
                    continue;
                }
            }
        IL_01:
            if (list.Count == 0)
            {
                return false;
            }
            int index = UnityEngine.Random.Range(0, list.Count - 1);
            doo = list[index];
            list.Remove(doo);
            Transform transform = doo.transform;
            pos = transform.position + 0.75f * (UnityEngine.Random.Range(0, 1) * 2f - 1f) * transform.forward + Vector3.up * config.SchematicOffsetHeight;
            if (Physics.SphereCast(pos, 0.5f, transform.right * (UnityEngine.Random.Range(0, 1) * 2f - 1f), out RaycastHit hit, 10f, FpcStateProcessor.Mask) && hit.distance >= 0.3f)
            {
                Vector3 vector = hit.point + hit.normal * 0.5f;
                pos = new Vector3(vector.x, transform.position.y + config.SchematicOffsetHeight, vector.z);
                if (config.LogsItslocation)
                {

                    Room room1 = Room.Get(pos);
                    ServerConsole.AddLog(Name + " Spawned in: " + room1.name);
                }
                return true;
            }
            goto IL_01;
        }

        void DestroyDisturb(RaycastHit hit)
        {
            IDamageableDoor damageableDoor;
            if (config.DestroyWindowWhileApproaching && hit.collider.TryGetComponent<BreakableWindow>(out BreakableWindow breakableWindow))
            {
                breakableWindow.Damage(500f, null, Vector3.zero);
            }
            if (config.DestryDoorWhileApproaching && hit.collider.TryGetComponent<InteractableCollider>(out InteractableCollider interactableCollider) && (damageableDoor = (interactableCollider.Target as IDamageableDoor)) != null)
            {
                damageableDoor.ServerDamage(500f, DoorDamageType.ServerCommand);
            }
            if (ReferenceHub.TryGetHub(hit.transform.root.gameObject, out ReferenceHub hub) && config.KillableDisturbingRoles.Contains(hub.roleManager.CurrentRole.RoleTypeId) && hub != Target)
            {
                if (!config.OnlyKillDisturbingWhileOwnScp330 || PlayerCheck(hub, out TargetingReason reason))
                {
                    hub.playerStats.DealDamage(new CustomReasonDamageHandler(config.DeathReasonDIsturbingWay));
                }
            }
        }

        internal void OnShot(ShootingEventArgs ev)
        {
            throw new NotImplementedException();
        }

        public float statusTimer = 0f;

        float Timer;

        public bool Spawned = true;

        public bool DestroyNow = false;

        Config config = SCP956Plugin.Instance.Config;

        static LayerMask _mask = 0;

        public static LayerMask WallMask
        {
            get
            {
                if (SCP956AI._mask == 0)
                {
                    _mask = FpcStateProcessor.Mask;
                    if (SCP956Plugin.Instance.Config.CanTargetThroughWindow)
                    {
                        _mask ^= 1 << LayerMask.NameToLayer("Glass");
                    }
                    if (SCP956Plugin.Instance.Config.CanTargetThroughDoor)
                    {
                        _mask ^= 1 << LayerMask.NameToLayer("Door");
                    }
                    _mask ^= 1 << LayerMask.NameToLayer("Player");
                }
                return SCP956AI._mask;
            }
        }

        public static List<DoorVariant> DoorList = new List<DoorVariant> { };

        public List<ReferenceHub> Targeted = new List<ReferenceHub> { };

        public bool IsFlying = false;

        public ReferenceHub Target = null;

        public DoorVariant CurrentDoor;

        public Dictionary<ReferenceHub, TargetingReason> Targetable = new Dictionary<ReferenceHub, TargetingReason> { };

        private float _sequenceTimer;

        public Vector3 _spawnPos;

        private float _initialRot;

        private Vector3 _initialPos;

        private Vector3 TargetPos;

        public Dictionary<ReferenceHub, float> TimerPerReferenceHub = new Dictionary<ReferenceHub, float> { };

        public Dictionary<ReferenceHub, float> TimerForAnger = new Dictionary<ReferenceHub, float> { };

        private float rotateTime;

        private float moveTime;

        private float chargeTime;

        private float coolTime;

        Vector3 lerpPos;

        float lerpRot;

        bool triggered = false;

        public float Health;

        public string Name = "";
    }
}
