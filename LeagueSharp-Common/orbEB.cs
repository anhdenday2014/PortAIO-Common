using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Utils;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK;
/*
namespace LeagueSharp.Common
{
    // Token: 0x0200010B RID: 267
    public static class OrbwalkerEB
    {
        // Token: 0x060008A5 RID: 2213 RVA: 0x000241CC File Offset: 0x000223CC
        internal static void Clear()
        {
            if (LastTarget != null && !LastTarget.IsValidTarget(null, false, null))
            {
                LastTarget = null;
            }
            if (!CanMove && LastTarget == null)
            {
                _autoAttackCompleted = true;
            }
            foreach (TargetMinionType current in Enum.GetValues(typeof(TargetMinionType)).Cast<TargetMinionType>())
            {
                CurrentMinionsLists[current] = new List<Obj_AI_Minion>();
            }
            KeyValuePair<TargetMinionType, Obj_AI_Minion>[] array = (from entry in CurrentMinions
                                                                               where !entry.Value.IsValidTarget(null, false, null) || !Player.Instance.IsInAutoAttackRange(entry.Value)
                                                                               select entry).ToArray<KeyValuePair<TargetMinionType, Obj_AI_Minion>>();
            for (int i = 0; i < array.Length; i++)
            {
                KeyValuePair<TargetMinionType, Obj_AI_Minion> keyValuePair = array[i];
                CurrentMinions[keyValuePair.Key] = null;
            }
            _precalculatedDamage = null;
            TickCachedMonsters.Clear();
            TickCachedMinions.Clear();
            CurrentMinionValues.Clear();
            DamageOnMinions.Clear();
        }

        // Token: 0x060008A3 RID: 2211 RVA: 0x00023AD4 File Offset: 0x00021CD4
        internal static void CreateMenu()
        {
            useTick.OnValueChange += delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
            {
                if (args.NewValue)
                {
                    useUpdate.CurrentValue = false;
                    return;
                }
                if (!useUpdate.CurrentValue)
                {
                    useTick.CurrentValue = true;
                }
            };
            useUpdate.OnValueChange += delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
            {
                if (args.NewValue)
                {
                    useTick.CurrentValue = false;
                    return;
                }
                if (!useTick.CurrentValue)
                {
                    useUpdate.CurrentValue = true;
                }
            };

        }

        // Token: 0x060008B8 RID: 2232 RVA: 0x00024C78 File Offset: 0x00022E78
        private static int GetAttackCastDelay(this Obj_AI_Base target)
        {
            Champion hero = Player.Instance.Hero;
            if (hero == Champion.Azir)
            {
                Obj_AI_Minion obj_AI_Minion = AzirSoldiers.FirstOrDefault((Obj_AI_Minion i) => i.IsInAutoAttackRange(target));
                if (obj_AI_Minion != null)
                {
                    return (int)(obj_AI_Minion.AttackCastDelay * 1000f);
                }
            }
            return (int)(AttackCastDelay * 1000f);
        }

        // Token: 0x060008B9 RID: 2233 RVA: 0x00024CE0 File Offset: 0x00022EE0
        internal static int GetAttackDelay(this Obj_AI_Base target)
        {
            if (Player.Instance.Hero == Champion.Azir)
            {
                Obj_AI_Minion obj_AI_Minion = AzirSoldiers.FirstOrDefault((Obj_AI_Minion i) => i.IsInAutoAttackRange(target));
                if (obj_AI_Minion != null)
                {
                    return (int)(obj_AI_Minion.AttackDelay * 1000f);
                }
            }
            return (int)(AttackDelay * 1000f);
        }

        // Token: 0x060008B6 RID: 2230 RVA: 0x00024C10 File Offset: 0x00022E10
        private static float GetAutoAttackDamage(Obj_AI_Minion minion)
        {
            if (_precalculatedDamage == null)
            {
                _precalculatedDamage = Player.Instance.GetStaticAutoAttackDamage(true);
            }
            if (!DamageOnMinions.ContainsKey(minion.NetworkId))
            {
                DamageOnMinions[minion.NetworkId] = Player.Instance.GetAutoAttackDamage(minion, _precalculatedDamage);
            }
            return DamageOnMinions[minion.NetworkId];
        }

        // Token: 0x060008B4 RID: 2228 RVA: 0x0000B9FA File Offset: 0x00009BFA
        internal static AttackableUnit GetIllaoiGhost()
        {
            if (!IllaoiGhost)
            {
                return null;
            }
            return ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault((Obj_AI_Minion o) => o.IsValidTarget(null, false, null) && o.IsEnemy && o.HasBuff("") && Player.Instance.IsInAutoAttackRange(o));
        }

        // Token: 0x060008B7 RID: 2231 RVA: 0x0000BA4D File Offset: 0x00009C4D
        internal static int GetLasthitTime(this Obj_AI_Minion minion)
        {
            return minion.GetAttackCastDelay() + GetProjectileTravelTime(minion);
        }

        // Token: 0x060008BA RID: 2234 RVA: 0x0000BA5C File Offset: 0x00009C5C
        private static int GetProjectileTravelTime(Obj_AI_Minion minion)
        {
            if (!IsMelee)
            {
                return (int)Math.Max(0f, Player.Instance.Distance(minion, false) / Player.Instance.BasicAttack.MissileSpeed * 1000f);
            }
            return 0;
        }

        // Token: 0x060008B2 RID: 2226 RVA: 0x0002481C File Offset: 0x00022A1C
        internal static AttackableUnit GetTarget()
        {
            if (ForcedTarget != null && ForcedTarget.IsValidTarget(null, false, null))
            {
                if (!Player.Instance.IsInAutoAttackRange(ForcedTarget))
                {
                    return null;
                }
                return ForcedTarget;
            }
            else
            {
                if (ActiveModesFlags == ActiveModes.None)
                {
                    return null;
                }
                List<AttackableUnit> list = new List<AttackableUnit>();
                foreach (ActiveModes current in from ActiveModes mode in Enum.GetValues(typeof(ActiveModes))
                                                          where mode != ActiveModes.None && ActiveModesFlags.HasFlag(mode)
                                                          select mode)
                {
                    ActiveModes activeModes = current;
                    switch (activeModes)
                    {
                        case ActiveModes.Combo:
                            list.Add(GetTarget(TargetTypes.Hero));
                            break;
                        case ActiveModes.Harass:
                            {
                                AttackableUnit target = GetTarget(TargetTypes.Structure);
                                if (target != null)
                                {
                                    if (!LastHitPriority)
                                    {
                                        list.Add(target);
                                    }
                                    list.Add(GetTarget(TargetTypes.LaneMinion));
                                    if (LastHitPriority && !ShouldWait)
                                    {
                                        list.Add(target);
                                    }
                                }
                                else
                                {
                                    if (!LastHitPriority)
                                    {
                                        list.Add(GetTarget(TargetTypes.Hero));
                                    }
                                    list.Add(GetTarget(TargetTypes.JungleMob) ?? GetTarget(TargetTypes.LaneMinion));
                                    if (LastHitPriority && !ShouldWait)
                                    {
                                        list.Add(GetTarget(TargetTypes.Hero));
                                    }
                                }
                                break;
                            }
                        case ActiveModes.Combo | ActiveModes.Harass:
                            break;
                        case ActiveModes.LastHit:
                            list.Add(GetTarget(TargetTypes.LaneMinion));
                            break;
                        default:
                            if (activeModes != ActiveModes.JungleClear)
                            {
                                if (activeModes == ActiveModes.LaneClear)
                                {
                                    AttackableUnit target = GetTarget(TargetTypes.Structure);
                                    AttackableUnit target2 = GetTarget(TargetTypes.LaneMinion);
                                    if (target != null)
                                    {
                                        if (!LastHitPriority)
                                        {
                                            list.Add(target);
                                        }
                                        if (target2.IdEquals(LastHitMinion))
                                        {
                                            list.Add(target2);
                                        }
                                        if (LastHitPriority && !ShouldWait)
                                        {
                                            list.Add(target);
                                        }
                                    }
                                    else
                                    {
                                        if (!LastHitPriority && LaneClearAttackChamps)
                                        {
                                            list.Add(GetTarget(TargetTypes.Hero));
                                        }
                                        if (target2.IdEquals(LastHitMinion))
                                        {
                                            list.Add(target2);
                                        }
                                        if (LastHitPriority && LaneClearAttackChamps && !ShouldWait)
                                        {
                                            list.Add(GetTarget(TargetTypes.Hero));
                                        }
                                        if (target2.IdEquals(LaneClearMinion))
                                        {
                                            list.Add(target2);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                list.Add(GetTarget(TargetTypes.JungleMob));
                            }
                            break;
                    }
                }
                list.RemoveAll((AttackableUnit o) => o == null);
                return list.FirstOrDefault<AttackableUnit>();
            }
        }

        // Token: 0x060008B3 RID: 2227 RVA: 0x00024AC8 File Offset: 0x00022CC8
        internal static AttackableUnit GetTarget(TargetTypes targetType)
        {
            switch (targetType)
            {
                case TargetTypes.Hero:
                    if (Player.Instance.Hero == Champion.Azir)
                    {
                        AIHeroClient target = TargetSelector.GetTarget(from h in EntityManager.Heroes.Enemies
                                                                       where h.IsValidTarget(null, false, null) && ValidAzirSoldiers.Any((Obj_AI_Minion i) => i.IsInAutoAttackRange(h))
                                                                       select h, TargetSelector.DamageType.Magical);
                        if (target != null)
                        {
                            return target;
                        }
                    }
                    return TargetSelector.GetTarget(from h in EntityManager.Heroes.Enemies
                                                    where h.IsValidTarget(null, false, null) && Player.Instance.IsInAutoAttackRange(h)
                                                    select h, TargetSelector.DamageType.Physical) ?? GetIllaoiGhost();
                case TargetTypes.JungleMob:
                    return TickCachedMonsters.FirstOrDefault<Obj_AI_Minion>();
                case TargetTypes.LaneMinion:
                    if (LastHitMinion != null)
                    {
                        if (PriorityLastHitWaitingMinion != null && !PriorityLastHitWaitingMinion.IdEquals(LastHitMinion) && PriorityLastHitWaitingMinion.IsSiegeMinion())
                        {
                            return null;
                        }
                        return LastHitMinion;
                    }
                    else
                    {
                        if (ShouldWait || _onlyLastHit)
                        {
                            return null;
                        }
                        return LaneClearMinion;
                    }
                    break;

                case TargetTypes.Structure:
                    return (from o in EnemyStructures
                            where o.IsValid && !o.IsDead && o.IsTargetable && Player.Instance.Distance(o, true) <= Player.Instance.GetAutoAttackRange(o).Pow()
                            orderby o.MaxHealth descending
                            select o).FirstOrDefault<AttackableUnit>();
                default:
                    return null;
            }
        }

        // Token: 0x060008BB RID: 2235 RVA: 0x00024D44 File Offset: 0x00022F44
        internal static bool HasTurretTargetting(this Obj_AI_Minion minion)
        {
            return LastTargetTurrets.Any((KeyValuePair<int, Obj_AI_Base> o) => o.Value.IdEquals(minion));
        }

        // Token: 0x060008A2 RID: 2210 RVA: 0x00023714 File Offset: 0x00021914
        internal static void Initialize()
        {
            Random = new Random(DateTime.Now.Millisecond);
            Champion hero = Player.Instance.Hero;
            if (hero != Champion.Azir)
            {
                if (hero == Champion.Rengar)
                {
                    bool rengarIsDashing = false;
                    Obj_AI_Base.OnPlayAnimation += delegate (Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
                    {
                        if (sender.IsMe && args.Animation.Equals(""))
                        {
                            rengarIsDashing = true;
                            _OnAttack(null);
                        }
                    };
                    Obj_AI_Base.OnNewPath += delegate (Obj_AI_Base sender, GameObjectNewPathEventArgs args)
                    {
                        if (sender.IsMe && !args.IsDash && rengarIsDashing)
                        {
                            rengarIsDashing = false;
                            _OnPostAttack(sender);
                        }
                    };
                }
            }
            else
            {
                foreach (Obj_AI_Minion current in ObjectManager.Get<Obj_AI_Minion>().Where(delegate (Obj_AI_Minion o)
                {
                    if (o.IsValid && o.IsAlly && o.Name == "")
                    {
                        return o.Buffs.Any((BuffInstance b) => b.IsValid() && b.Caster.IsMe && b.Count == 1 && b.DisplayName == "");
                    }
                    return false;
                }))
                {
                    _azirSoldiers[current.NetworkId] = current;
                    if (Player.Instance.IsInRange(current, 950f))
                    {
                        _validAzirSoldiers[current.NetworkId] = current;
                    }
                }
                AzirSoldierPreDashStatus = new Dictionary<int, bool>();
                Obj_AI_Base.OnPlayAnimation += delegate (Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
                {
                    Obj_AI_Minion soldier = sender as Obj_AI_Minion;
                    string animation;
                    if (soldier != null && soldier.IsAlly && soldier.Name == "" && (animation = args.Animation) != null)
                    {
                        if (!(animation == ""))
                        {
                            if (!(animation == ""))
                            {
                                if (animation == "")
                                {
                                    if (!AzirSoldierPreDashStatus.ContainsKey(soldier.NetworkId))
                                    {
                                        AzirSoldierPreDashStatus.Add(soldier.NetworkId, _validAzirSoldiers.Any((KeyValuePair<int, Obj_AI_Minion> o) => o.Value.IdEquals(soldier)));
                                    }
                                    _validAzirSoldiers.Remove(soldier.NetworkId);
                                    return;
                                }
                                if (!(animation == ""))
                                {
                                    if (!(animation == ""))
                                    {
                                        return;
                                    }
                                    _azirSoldiers.Remove(soldier.NetworkId);
                                    _validAzirSoldiers.Remove(soldier.NetworkId);
                                    AzirSoldierPreDashStatus.Remove(soldier.NetworkId);
                                }
                                else if (AzirSoldierPreDashStatus.ContainsKey(soldier.NetworkId) && AzirSoldierPreDashStatus[soldier.NetworkId])
                                {
                                    _validAzirSoldiers[soldier.NetworkId] = soldier;
                                    AzirSoldierPreDashStatus.Remove(soldier.NetworkId);
                                    return;
                                }
                            }
                            else
                            {
                                _validAzirSoldiers[soldier.NetworkId] = soldier;
                                if (AzirSoldierPreDashStatus.ContainsKey(soldier.NetworkId))
                                {
                                    AzirSoldierPreDashStatus[soldier.NetworkId] = true;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            _validAzirSoldiers.Remove(soldier.NetworkId);
                            if (AzirSoldierPreDashStatus.ContainsKey(soldier.NetworkId))
                            {
                                AzirSoldierPreDashStatus[soldier.NetworkId] = false;
                                return;
                            }
                        }
                    }
                };
                Obj_AI_Base.OnBuffGain += delegate (Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
                {
                    Obj_AI_Minion obj_AI_Minion = sender as Obj_AI_Minion;
                    if (obj_AI_Minion != null && obj_AI_Minion.IsAlly && obj_AI_Minion.Name == "" && args.Buff.Caster.IsMe && args.Buff.DisplayName == "")
                    {
                        _azirSoldiers[obj_AI_Minion.NetworkId] = obj_AI_Minion;
                        _validAzirSoldiers[obj_AI_Minion.NetworkId] = obj_AI_Minion;
                    }
                };
            }
            IllaoiGhost = EntityManager.Heroes.Allies.Any((AIHeroClient h) => h.Hero == Champion.Illaoi);
            CreateMenu();
            EnemyStructures.AddRange(from o in ObjectManager.Get<AttackableUnit>()
                                               where o.IsEnemy && o.IsStructure()
                                               select o);
            Game.OnTick += delegate (EventArgs param0)
            {
                if (UseOnTick)
                {
                    OnTick();
                }
            };
            Game.OnUpdate += delegate (EventArgs param0)
            {
                if (UseOnUpdate)
                {
                    OnTick();
                }
                OnUpdate();
            };
            GameObject.OnCreate += new GameObjectCreate(OnCreate);
            Obj_AI_Base.OnBasicAttack += new Obj_AI_BaseOnBasicAttack(OnBasicAttack);
            Obj_AI_Base.OnProcessSpellCast += new Obj_AI_ProcessSpellCast(OnProcessSpellCast);
            Obj_AI_Base.OnSpellCast += new Obj_AI_BaseDoCastSpell(OnSpellCast);
            Spellbook.OnStopCast += new SpellbookStopCast(OnStopCast);
            Drawing.OnDraw += new DrawingDraw(OnDraw);
            if (AutoAttacks.DashAutoAttackResetSlotsDatabase.ContainsKey(Player.Instance.Hero))
            {
                bool waitingAutoAttackReset = false;
                Vector3 dashEndPosition = default(Vector3);
                if (AutoAttacks.AutoAttackResetAnimationName.ContainsKey(Player.Instance.Hero))
                {
                    Obj_AI_Base.OnPlayAnimation += delegate (Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
                    {
                        if (sender.IsMe && AutoAttacks.IsDashAutoAttackReset(Player.Instance, args))
                        {
                            waitingAutoAttackReset = true;
                        }
                    };
                }
                Obj_AI_Base.OnProcessSpellCast += delegate (Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
                {
                    if (sender.IsMe && AutoAttacks.IsDashAutoAttackReset(Player.Instance, args))
                    {
                        waitingAutoAttackReset = true;
                    }
                };
                Game.OnUpdate += delegate (EventArgs param0)
                {
                    if (dashEndPosition != default(Vector3) && waitingAutoAttackReset && dashEndPosition == Player.Instance.Position)
                    {
                        ResetAutoAttack();
                        waitingAutoAttackReset = false;
                        dashEndPosition = default(Vector3);
                    }
                };
                Obj_AI_Base.OnNewPath += delegate (Obj_AI_Base sender, GameObjectNewPathEventArgs args)
                {
                    if (sender.IsMe)
                    {
                        if (args.IsDash)
                        {
                            if (waitingAutoAttackReset)
                            {
                                dashEndPosition = args.Path.LastOrDefault<Vector3>();
                                return;
                            }
                        }
                        else if (waitingAutoAttackReset)
                        {
                            ResetAutoAttack();
                            waitingAutoAttackReset = false;
                            dashEndPosition = default(Vector3);
                        }
                    }
                };
            }

           var minionBarOffset = new Vector2(36f, 3f);

           var barHeight = 6;

           var barWidth = 62;
            Drawing.OnEndScene += delegate (EventArgs param0)
            {
                if (DrawDamageMarker)
                {
                    foreach (Obj_AI_Minion current2 in from o in TickCachedMinions
                                                       where DamageOnMinions.ContainsKey(o.NetworkId) && o.VisibleOnScreen
                                                       select o)
                    {
                        Vector2 vector = minionBarOffset + current2.HPBarPosition + new Vector2((float)barWidth * Math.Min(GetAutoAttackDamage(current2) / current2.MaxHealth, 1f), 0f);
                        Line.DrawLine(System.Drawing.Color.Black, 2f, new Vector2[]
                        {
                            vector,
                            vector + new Vector2(0f, (float)barHeight)
                        });
                    }
                }
            };
            if (Player.Instance.IsMelee)
            {
                OnUnkillableMinion += delegate (Obj_AI_Base target, UnkillableMinionArgs args)
                {
                    if ((ActiveModesFlags.HasFlag(ActiveModes.LastHit) || ActiveModesFlags.HasFlag(ActiveModes.LaneClear)) && UseTiamat && Player.Instance.Distance(target, true) <= 160000f)
                    {
                        float itemDamage = Player.Instance.GetItemDamage(target, ItemId.Tiamat);
                        float prediction = Prediction.Health.GetPrediction(target, 200);
                        if (prediction <= itemDamage)
                        {
                            foreach (ItemId current2 in new ItemId[]
                            {
                                ItemId.Tiamat,
                                ItemId.Ravenous_Hydra
                            }.Where(new Func<ItemId, bool>(Item.CanUseItem)))
                            {
                                Item.UseItem(current2, null);
                            }
                        }
                    }
                };
            }
            GameObject.OnDelete += delegate (GameObject sender, EventArgs args)
            {
                if (sender.IsStructure())
                {
                    EnemyStructures.RemoveAll((AttackableUnit o) => o.IdEquals(sender));
                }
                if (sender.IdEquals(LastHitMinion))
                {
                    LastHitMinion = null;
                }
                if (sender.IdEquals(PriorityLastHitWaitingMinion))
                {
                    PriorityLastHitWaitingMinion = null;
                }
                if (sender.IdEquals(LaneClearMinion))
                {
                    LaneClearMinion = null;
                }
                if (LastTarget.IdEquals(sender))
                {
                    LastTarget = null;
                }
                if (ForcedTarget.IdEquals(sender))
                {
                    ForcedTarget = null;
                }
            };
            Player.OnPostIssueOrder += delegate (Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
            {
                if (sender.IsMe)
                {
                    _lastIssueOrderStartVector = new Vector3?(sender.Position);
                    _lastIssueOrderEndVector = new Vector3?(args.TargetPosition);
                    _lastIssueOrderType = new GameObjectOrder?(args.Order);
                    _lastIssueOrderTargetId = new int?((args.Target != null) ? args.Target.NetworkId : 0);
                }
            };
        }

        // Token: 0x060008A9 RID: 2217 RVA: 0x00024580 File Offset: 0x00022780
        public static void MoveTo(Vector3 position)
        {
            if (!CanMove || Core.GameTickCount - LastMovementSent + RandomOffset <= MovementDelay || (!FastKiting && Core.GameTickCount - _lastAutoAttackSent + RandomOffset <= MovementDelay))
            {
                return;
            }
            Vector3 vector = (Player.Instance.Distance(position, true) < (float)100.Pow()) ? Player.Instance.Position.Extend(position, 100f).To3DWorld() : position;
            GameObjectOrder gameObjectOrder;
            if (HoldRadius > 0)
            {
                if (position.Distance(Player.Instance, true) > (float)HoldRadius.Pow())
                {
                    gameObjectOrder = GameObjectOrder.MoveTo;
                }
                else
                {
                    gameObjectOrder = GameObjectOrder.Stop;
                }
            }
            else
            {
                gameObjectOrder = GameObjectOrder.MoveTo;
            }
            if (_lastIssueOrderType.HasValue && _lastIssueOrderType == gameObjectOrder)
            {
                GameObjectOrder gameObjectOrder2 = gameObjectOrder;
                if (gameObjectOrder2 != GameObjectOrder.MoveTo)
                {
                    if (gameObjectOrder2 == GameObjectOrder.Stop)
                    {
                        if (_lastIssueOrderStartVector.HasValue)
                        {
                            _lastIssueOrderStartVector.Value == Player.Instance.Position;
                        }
                        return;
                    }
                }
                else if (_lastIssueOrderEndVector.HasValue && _lastIssueOrderEndVector.Value == vector && Player.Instance.IsMoving)
                {
                    return;
                }
            }
            if (Player.IssueOrder(gameObjectOrder, vector))
            {
                LastMovementSent = Core.GameTickCount;
                RandomOffset = Random.Next(30) - 15;
            }
        }

        // Token: 0x0600085E RID: 2142 RVA: 0x000231F8 File Offset: 0x000213F8
        internal static void NotifyEventListeners(string eventName, Delegate[] invocationList, params object[] args)
        {
            for (int i = 0; i < invocationList.Length; i++)
            {
                Delegate @delegate = invocationList[i];
                try
                {
                    @delegate.DynamicInvoke(args);
                }
                catch (Exception exceptionObject)
                {
                    Logger.Exception("", exceptionObject, new object[]
                    {
                        eventName
                    });
                }
            }
        }

        // Token: 0x060008AC RID: 2220 RVA: 0x00024720 File Offset: 0x00022920
        internal static void OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                _OnAttack(args);
                return;
            }
            if (sender is Obj_AI_Turret && args.Target is Obj_AI_Base)
            {
                LastTargetTurrets[sender.NetworkId] = (Obj_AI_Base)args.Target;
            }
        }

        // Token: 0x060008AF RID: 2223 RVA: 0x000247E8 File Offset: 0x000229E8
        internal static void OnCreate(GameObject sender, EventArgs args)
        {
            MissileClient missileClient = sender as MissileClient;
            if (missileClient != null && missileClient.SpellCaster.IsMe && missileClient.IsAutoAttack())
            {
                TriggerPostAttack();
            }
        }

        // Token: 0x060008C9 RID: 2249 RVA: 0x0002536C File Offset: 0x0002356C
        internal static void OnDraw(EventArgs args)
        {
            if (DrawRange)
            {
                EloBuddy.SDK.Rendering.Circle.Draw(SharpDX.Color.LightGreen, Player.Instance.GetAutoAttackRange(null), new GameObject[]
                {
                    Player.Instance
                });
            }
            if (Player.Instance.Hero == Champion.Azir && DrawAzirRange)
            {
                foreach (Obj_AI_Minion current in _validAzirSoldiers.Values)
                {
                    EloBuddy.SDK.Rendering.Circle.Draw(SharpDX.Color.LightGreen, current.GetAutoAttackRange(null), new GameObject[]
                    {
                        current
                    });
                }
            }
            if (DrawHoldRadius)
            {
                EloBuddy.SDK.Rendering.Circle.Draw(SharpDX.Color.LightGreen, (float)HoldRadius, new GameObject[]
                {
                    Player.Instance
                });
            }
            if (DrawEnemyRange)
            {
                foreach (AIHeroClient current2 in from o in EntityManager.Heroes.Enemies
                                                  where o.IsValidTarget(null, false, null)
                                                  select o)
                {
                    float autoAttackRange = current2.GetAutoAttackRange(Player.Instance);
                    EloBuddy.SDK.Rendering.Circle.Draw(current2.IsInRange(Player.Instance, autoAttackRange) ? EnemyRangeColorInRange : EnemyRangeColorNotInRange, autoAttackRange, new GameObject[]
                    {
                        current2
                    });
                }
            }
            if (DrawLastHitMarker)
            {
                if (LastHitMinion != null)
                {
                    EloBuddy.SDK.Rendering.Circle.Draw(SharpDX.Color.White, Math.Max(LastHitMinion.BoundingRadius, 65f), 2f, new GameObject[]
                    {
                        LastHitMinion
                    });
                }
                if (PriorityLastHitWaitingMinion != null && !PriorityLastHitWaitingMinion.IdEquals(LastHitMinion))
                {
                    EloBuddy.SDK.Rendering.Circle.Draw(SharpDX.Color.Orange, Math.Max(PriorityLastHitWaitingMinion.BoundingRadius, 65f), 2f, new GameObject[]
                    {
                        PriorityLastHitWaitingMinion
                    });
                }
            }
        }

        // Token: 0x060008AD RID: 2221 RVA: 0x0002476C File Offset: 0x0002296C
        internal static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                _lastIssueOrderStartVector = null;
                _lastIssueOrderEndVector = null;
                _lastIssueOrderTargetId = null;
                _lastIssueOrderType = null;
                if (args.IsAutoAttack())
                {
                    _OnAttack(args);
                    return;
                }
                if (AutoAttacks.IsAutoAttackReset(Player.Instance, args) && Math.Abs(args.SData.CastTime) < 1.401298E-45f)
                {
                    ResetAutoAttack();
                }
            }
        }

        // Token: 0x060008AE RID: 2222 RVA: 0x0000B970 File Offset: 0x00009B70
        internal static void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.IsAutoAttack())
                {
                    _OnPostAttack(sender);
                    return;
                }
                if (AutoAttacks.IsAutoAttackReset(Player.Instance, args))
                {
                    Core.DelayAction(new Action(ResetAutoAttack), 30);
                }
            }
        }

        // Token: 0x060008B0 RID: 2224 RVA: 0x0000B9A9 File Offset: 0x00009BA9
        internal static void OnStopCast(Obj_AI_Base sender, SpellbookStopCastEventArgs args)
        {
            if (sender.IsMe && IsRanged && (args.DestroyMissile || args.StopAnimation) && !CanBeAborted)
            {
                ResetAutoAttack();
            }
        }

        // Token: 0x060008A6 RID: 2214 RVA: 0x00024308 File Offset: 0x00022508
        internal static void OnTick()
        {
            Clear();
            if (ActiveModesFlags != ActiveModes.None || DrawLastHitMarker || DrawDamageMarker)
            {
                TickCachedMinions.AddRange(from o in EntityManager.MinionsAndMonsters.EnemyMinions
                                                     where Player.Instance.Distance(o, true) <= 2250000f
                                                     select o);
                _onlyLastHit = (!ActiveModesFlags.HasFlag(ActiveModes.LaneClear) && !ActiveModesFlags.HasFlag(ActiveModes.JungleClear));
                if (ActiveModesFlags != ActiveModes.None || DrawLastHitMarker)
                {
                    RecalculateLasthittableMinions();
                }
                else if (DrawDamageMarker)
                {
                    foreach (Obj_AI_Minion current in TickCachedMinions)
                    {
                        GetAutoAttackDamage(current);
                    }
                }
            }
            if (ActiveModesFlags != ActiveModes.None && LastHitMinion == null && !ShouldWait && LaneClearMinion == null)
            {
                TickCachedMonsters.AddRange(from o in EntityManager.MinionsAndMonsters.Monsters.Where(new Func<Obj_AI_Minion, bool>(Player.Instance.IsInAutoAttackRange))
                                                      orderby o.MaxHealth descending
                                                      select o);
            }
        }

        // Token: 0x060008A7 RID: 2215 RVA: 0x0000B933 File Offset: 0x00009B33
        internal static void OnUpdate()
        {
            if (ActiveModesFlags != ActiveModes.None)
            {
                OrbwalkTo(OrbwalkPosition);
            }
        }

        // Token: 0x060008A8 RID: 2216 RVA: 0x00024460 File Offset: 0x00022660
        public static void OrbwalkTo(Vector3 position)
        {
            if (Chat.IsOpen)
            {
                return;
            }
            if (!DisableAttacking && CanAutoAttack)
            {
                AttackableUnit target = GetTarget();
                if (target != null)
                {
                    PreAttackArgs preAttackArgs = new PreAttackArgs(target);
                    TriggerPreAttackEvent(target, preAttackArgs);
                    if (preAttackArgs.Process)
                    {
                        if (_lastIssueOrderType.HasValue && _lastIssueOrderType == GameObjectOrder.AttackUnit && _lastAutoAttackSent > 0 && _lastIssueOrderTargetId.HasValue && _lastIssueOrderTargetId.Value == preAttackArgs.Target.NetworkId && _lastIssueOrderStartVector.HasValue && _lastIssueOrderStartVector.Value == Player.Instance.Position)
                        {
                            _autoAttackStarted = false;
                            _lastAutoAttackSent = Core.GameTickCount;
                            LastTarget = preAttackArgs.Target;
                            return;
                        }
                        if (Player.IssueOrder(GameObjectOrder.AttackUnit, preAttackArgs.Target))
                        {
                            _autoAttackStarted = false;
                            _lastAutoAttackSent = Core.GameTickCount;
                            LastTarget = preAttackArgs.Target;
                            return;
                        }
                    }
                }
            }
            if (!DisableMovement)
            {
                MoveTo(position);
            }
        }

        // Token: 0x060008C6 RID: 2246 RVA: 0x00024D74 File Offset: 0x00022F74
        internal static void RecalculateLasthittableMinions()
        {
            int num = CanAutoAttack ? 0 : ((int)(AttackDelay * 1000f - (float)(Core.GameTickCount - LastAutoAttack)));
            bool canMove = CanMove;
            foreach (Obj_AI_Minion current in TickCachedMinions)
            {
                int lasthitTime = current.GetLasthitTime();
                CalculatedMinionValue value = new CalculatedMinionValue(current)
                {
                    LastHitProjectileTime = lasthitTime + num + Math.Max(0, (int)(1500f * (Player.Instance.Distance(current, false) - Player.Instance.GetAutoAttackRange(current)) / Player.Instance.MoveSpeed)),
                    LaneClearProjectileTime = lasthitTime + (int)(1.4f * (float)current.GetAttackDelay())
                };
                CurrentMinionValues[current.NetworkId] = value;
            }
            foreach (EloBuddy.SDK.Prediction.Health.IncomingAttack current2 in EloBuddy.SDK.Prediction.Health.IncomingAttacks.SelectMany((KeyValuePair<int, List<Prediction.Health.IncomingAttack>> i) => i.Value))
            {
                int networkId = current2.Target.NetworkId;
                if (CurrentMinionValues.ContainsKey(networkId))
                {
                    CurrentMinionValues[networkId].LastHitHealth -= current2.GetDamage(CurrentMinionValues[networkId].LastHitProjectileTime);
                    CurrentMinionValues[networkId].LaneClearHealth -= current2.GetDamage(CurrentMinionValues[networkId].LaneClearProjectileTime);
                }
            }
            foreach (KeyValuePair<int, CalculatedMinionValue> current3 in CurrentMinionValues)
            {
                CalculatedMinionValue value2 = current3.Value;
                Obj_AI_Minion handle = value2.Handle;
                if (value2.IsUnkillable)
                {
                    if (!handle.IdEquals(LastTarget))
                    {
                        if (OnUnkillableMinion != null && canMove)
                        {
                            OnUnkillableMinion(handle, new UnkillableMinionArgs
                            {
                                RemainingHealth = value2.LastHitHealth
                            });
                        }
                        CurrentMinionsLists[TargetMinionType.UnKillable].Add(handle);
                    }
                }
                else if (value2.IsLastHittable)
                {
                    CurrentMinionsLists[TargetMinionType.LastHit].Add(handle);
                }
                else if (value2.IsAlmostLastHittable)
                {
                    CurrentMinionsLists[TargetMinionType.PriorityLastHitWaiting].Add(handle);
                }
                else if (value2.IsLaneClearMinion)
                {
                    CurrentMinionsLists[TargetMinionType.LaneClear].Add(handle);
                }
            }
            SortMinionsAndDefineTargets();
            if (AttackObjects && LastHitMinion == null && !ShouldWait)
            {
                using (IEnumerator<Obj_AI_Minion> enumerator4 = (from minion in EntityManager.MinionsAndMonsters.OtherEnemyMinions.Where(new Func<Obj_AI_Minion, bool>(Player.Instance.IsInAutoAttackRange))
                                                                 orderby minion.MaxHealth descending, minion.Health
                                                                 where minion.Health > 0f
                                                                 select minion).GetEnumerator())
                {
                    if (enumerator4.MoveNext())
                    {
                        Obj_AI_Minion current4 = enumerator4.Current;
                        LastHitMinion = current4;
                    }
                }
            }
        }

        // Token: 0x060008C8 RID: 2248 RVA: 0x00025340 File Offset: 0x00023540
        public static void RegisterKeyBind(KeyBind key, ActiveModes mode)
        {
            key.OnValueChange += delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
            {
                if (args.NewValue)
                {
                    if (!ActiveModesFlags.HasFlag(mode))
                    {
                        ActiveModesFlags |= mode;
                        return;
                    }
                }
                else if (ActiveModesFlags.HasFlag(mode))
                {
                    ActiveModesFlags ^= mode;
                }
            };
        }

        // Token: 0x060008A4 RID: 2212 RVA: 0x0000B925 File Offset: 0x00009B25
        public static void ResetAutoAttack()
        {
            CanAutoAttack = true;
            GotAutoAttackReset = true;
        }

        // Token: 0x060008C7 RID: 2247 RVA: 0x00025128 File Offset: 0x00023328
        internal static void SortMinionsAndDefineTargets()
        {
            KeyValuePair<TargetMinionType, List<Obj_AI_Minion>>[] array = CurrentMinionsLists.ToArray<KeyValuePair<TargetMinionType, List<Obj_AI_Minion>>>();
            for (int i = 0; i < array.Length; i++)
            {
                KeyValuePair<TargetMinionType, List<Obj_AI_Minion>> keyValuePair = array[i];
                switch (keyValuePair.Key)
                {
                    case TargetMinionType.LastHit:
                        CurrentMinionsLists[keyValuePair.Key] = (from o in keyValuePair.Value
                                                                           orderby o.MaxHealth descending, CurrentMinionValues[o.NetworkId].LastHitHealth
                                                                           select o).ToList<Obj_AI_Minion>();
                        LastHitMinion = keyValuePair.Value.FirstOrDefault(new Func<Obj_AI_Minion, bool>(Player.Instance.IsInAutoAttackRange));
                        break;
                    case TargetMinionType.PriorityLastHitWaiting:
                        CurrentMinionsLists[keyValuePair.Key] = (from o in keyValuePair.Value
                                                                           orderby o.MaxHealth descending, CurrentMinionValues[o.NetworkId].LaneClearHealth
                                                                           select o).ToList<Obj_AI_Minion>();
                        PriorityLastHitWaitingMinion = keyValuePair.Value.FirstOrDefault(new Func<Obj_AI_Minion, bool>(Player.Instance.IsInAutoAttackRange));
                        break;
                    case TargetMinionType.LaneClear:
                        if (!_onlyLastHit)
                        {
                            CurrentMinionsLists[keyValuePair.Key] = keyValuePair.Value.OrderByDescending(delegate (Obj_AI_Minion minion)
                            {
                                if (!FreezePriority)
                                {
                                    return 1f / CurrentMinionValues[minion.NetworkId].LaneClearHealth;
                                }
                                return CurrentMinionValues[minion.NetworkId].LaneClearHealth;
                            }).ToList<Obj_AI_Minion>();
                            LaneClearMinion = keyValuePair.Value.FirstOrDefault(new Func<Obj_AI_Minion, bool>(Player.Instance.IsInAutoAttackRange));
                        }
                        break;
                    case TargetMinionType.UnKillable:
                        CurrentMinionsLists[keyValuePair.Key] = (from o in keyValuePair.Value
                                                                           orderby CurrentMinionValues[o.NetworkId].LastHitHealth
                                                                           select o).ToList<Obj_AI_Minion>();
                        break;
                }
            }
        }

        // Token: 0x0600085B RID: 2139 RVA: 0x00023128 File Offset: 0x00021328
        internal static void TriggerAttackEvent(AttackableUnit target, EventArgs args = null)
        {
            if (OnAttack != null)
            {
                NotifyEventListeners("", OnAttack.GetInvocationList(), new object[]
                {
                    target,
                    args ?? EventArgs.Empty
                });
            }
        }

        // Token: 0x060008B1 RID: 2225 RVA: 0x0000B9D6 File Offset: 0x00009BD6
        internal static void TriggerPostAttack()
        {
            if (!_autoAttackCompleted)
            {
                TriggerPostAttackEvent(LastTarget, EventArgs.Empty);
                _autoAttackCompleted = true;
                GotAutoAttackReset = false;
            }
        }

        // Token: 0x0600085C RID: 2140 RVA: 0x00023170 File Offset: 0x00021370
        internal static void TriggerPostAttackEvent(AttackableUnit target, EventArgs args = null)
        {
            if (OnPostAttack != null)
            {
                NotifyEventListeners("", OnPostAttack.GetInvocationList(), new object[]
                {
                    target,
                    args ?? EventArgs.Empty
                });
            }
        }

        // Token: 0x0600085A RID: 2138 RVA: 0x000230E8 File Offset: 0x000212E8
        internal static void TriggerPreAttackEvent(AttackableUnit target, PreAttackArgs args)
        {
            if (OnPreAttack != null)
            {
                NotifyEventListeners("", OnPreAttack.GetInvocationList(), new object[]
                {
                    target,
                    args
                });
            }
        }

        // Token: 0x0600085D RID: 2141 RVA: 0x000231B8 File Offset: 0x000213B8
        internal static void TriggerUnkillableMinionEvent(Obj_AI_Base target, UnkillableMinionArgs args)
        {
            if (OnUnkillableMinion != null)
            {
                NotifyEventListeners("", OnUnkillableMinion.GetInvocationList(), new object[]
                {
                    target,
                    args
                });
            }
        }

        // Token: 0x060008AA RID: 2218 RVA: 0x000246E0 File Offset: 0x000228E0
        internal static void _OnAttack(GameObjectProcessSpellCastEventArgs args)
        {
            CanAutoAttack = false;
            AttackableUnit attackableUnit = (args != null) ? (args.Target as AttackableUnit) : LastTarget;
            if (attackableUnit != null)
            {
                LastTarget = attackableUnit;
                TriggerAttackEvent(attackableUnit, EventArgs.Empty);
            }
        }

        // Token: 0x060008AB RID: 2219 RVA: 0x0000B946 File Offset: 0x00009B46
        internal static void _OnPostAttack(Obj_AI_Base sender)
        {
            if (Game.Ping < 50)
            {
                Core.DelayAction(new Action(TriggerPostAttack), 50 - Game.Ping);
                return;
            }
            TriggerPostAttack();
        }

        // Token: 0x17000229 RID: 553
        public static ActiveModes ActiveModesFlags
        {
            // Token: 0x06000861 RID: 2145 RVA: 0x0000B512 File Offset: 0x00009712
            get;
            // Token: 0x06000862 RID: 2146 RVA: 0x0000B519 File Offset: 0x00009719
            set;
        }

        // Token: 0x17000238 RID: 568
        internal static Menu AdvancedMenu
        {
            // Token: 0x06000879 RID: 2169 RVA: 0x0000B59B File Offset: 0x0000979B
            get;
            // Token: 0x0600087A RID: 2170 RVA: 0x0000B5A2 File Offset: 0x000097A2
            set;
        }

        // Token: 0x1700022D RID: 557
        public static float AttackCastDelay
        {
            // Token: 0x06000868 RID: 2152 RVA: 0x00023398 File Offset: 0x00021598
            get
            {
                Champion hero = Player.Instance.Hero;
                if (hero == Champion.TwistedFate && (Player.Instance.HasBuff("") || Player.Instance.HasBuff("") || Player.Instance.HasBuff("")))
                {
                    return 0.13f;
                }
                return Player.Instance.AttackCastDelay;
            }
        }

        // Token: 0x1700022E RID: 558
        public static float AttackDelay
        {
            // Token: 0x06000869 RID: 2153 RVA: 0x00023408 File Offset: 0x00021608
            get
            {
                Champion hero = Player.Instance.Hero;
                if (hero == Champion.Graves && Player.Instance.HasBuff(""))
                {
                    return 1.07402968f * Player.Instance.AttackDelay - 0.716238141f;
                }
                return Player.Instance.AttackDelay;
            }
        }

        // Token: 0x1700024B RID: 587
        internal static bool AttackObjects
        {
            // Token: 0x06000890 RID: 2192 RVA: 0x0000B7EA File Offset: 0x000099EA
            get
            {
                return Menu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000256 RID: 598
        internal static Dictionary<int, bool> AzirSoldierPreDashStatus
        {
            // Token: 0x0600089E RID: 2206 RVA: 0x0000B907 File Offset: 0x00009B07
            get;
            // Token: 0x0600089F RID: 2207 RVA: 0x0000B90E File Offset: 0x00009B0E
            set;
        }

        // Token: 0x17000254 RID: 596
        public static List<Obj_AI_Minion> AzirSoldiers
        {
            // Token: 0x0600089C RID: 2204 RVA: 0x0000B8E5 File Offset: 0x00009AE5
            get
            {
                return _azirSoldiers.Values.ToList<Obj_AI_Minion>();
            }
        }

        // Token: 0x17000230 RID: 560
        public static bool CanAutoAttack
        {
            // Token: 0x0600086B RID: 2155 RVA: 0x00023504 File Offset: 0x00021704
            get
            {
                if (!Player.Instance.CanAttack)
                {
                    return false;
                }
                if (Player.Instance.Spellbook.IsChanneling)
                {
                    return false;
                }
                switch (Player.Instance.Hero)
                {
                    case Champion.Jhin:
                        if (Player.Instance.HasBuff(""))
                        {
                            return false;
                        }
                        break;
                    case Champion.Kalista:
                        if (Player.Instance.IsDashing())
                        {
                            return false;
                        }
                        break;
                }
                return Core.GameTickCount - _lastAutoAttackSent > 100 + Game.Ping && ((!ParanoidMode && !_autoAttackStarted) || (float)(Core.GameTickCount - LastAutoAttack + 60) >= AttackDelay * 1000f);
            }
            // Token: 0x0600086C RID: 2156 RVA: 0x000235C0 File Offset: 0x000217C0
            internal set
            {
                if (value)
                {
                    _autoAttackStarted = false;
                    _autoAttackCompleted = true;
                    LastAutoAttack = 0;
                    LastMovementSent = 0;
                    _lastAutoAttackSent = 0;
                    return;
                }
                _autoAttackStarted = true;
                _autoAttackCompleted = false;
                LastAutoAttack = Core.GameTickCount;
                if (FastKiting)
                {
                    LastMovementSent -= MovementDelay - RandomOffset;
                    _lastAutoAttackSent -= 100 + Game.Ping;
                }
            }
        }

        // Token: 0x17000231 RID: 561
        public static bool CanBeAborted
        {
            // Token: 0x0600086D RID: 2157 RVA: 0x00023638 File Offset: 0x00021838
            get
            {
                int num = ExtraWindUpTime;
                Champion hero = Player.Instance.Hero;
                if (hero != Champion.Jinx)
                {
                    if (hero == Champion.Rengar)
                    {
                        num += 150;
                    }
                }
                else
                {
                    num += 150;
                }
                return _autoAttackCompleted || (float)(Core.GameTickCount - LastAutoAttack) >= AttackCastDelay * 1000f + (float)num + (float)(IncludePing ? (Game.Ping / 10) : 0);
            }
        }

        // Token: 0x1700022F RID: 559
        public static bool CanMove
        {
            // Token: 0x0600086A RID: 2154 RVA: 0x0002345C File Offset: 0x0002165C
            get
            {
                return (UnabortableAutoDatabase.Contains(Player.Instance.Hero) && Core.GameTickCount - LastAutoAttack >= Math.Min(MovementDelay, 100)) || (Core.GameTickCount - _lastAutoAttackSent > 100 + Game.Ping && CanBeAborted && (!Player.Instance.Spellbook.IsChanneling || (AllowedMovementBuffs.ContainsKey(Player.Instance.Hero) && Player.Instance.HasBuff(AllowedMovementBuffs[Player.Instance.Hero]))));
            }
        }
        // Token: 0x0400004C RID: 76
        internal static readonly HashSet<Champion> UnabortableAutoDatabase = new HashSet<Champion>
        {
            Champion.Kalista
        };
        // Token: 0x17000247 RID: 583
        public static bool DisableAttacking
        {
            // Token: 0x0600088B RID: 2187 RVA: 0x0000B779 File Offset: 0x00009979
            get
            {
                return _disableAttacking || AdvancedMenu[""].Cast<CheckBox>().CurrentValue;
            }
            // Token: 0x0600088C RID: 2188 RVA: 0x0000B7A2 File Offset: 0x000099A2
            set
            {
                _disableAttacking = value;
            }
        }

        // Token: 0x17000246 RID: 582
        public static bool DisableMovement
        {
            // Token: 0x06000889 RID: 2185 RVA: 0x0000B748 File Offset: 0x00009948
            get
            {
                return _disableMovement || AdvancedMenu[""].Cast<CheckBox>().CurrentValue;
            }
            // Token: 0x0600088A RID: 2186 RVA: 0x0000B771 File Offset: 0x00009971
            set
            {
                _disableMovement = value;
            }
        }

        // Token: 0x17000241 RID: 577
        public static bool DrawAzirRange
        {
            // Token: 0x06000884 RID: 2180 RVA: 0x0000B6A8 File Offset: 0x000098A8
            get
            {
                return DrawingsMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000244 RID: 580
        public static bool DrawDamageMarker
        {
            // Token: 0x06000887 RID: 2183 RVA: 0x0000B708 File Offset: 0x00009908
            get
            {
                return DrawingsMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000242 RID: 578
        public static bool DrawEnemyRange
        {
            // Token: 0x06000885 RID: 2181 RVA: 0x0000B6C8 File Offset: 0x000098C8
            get
            {
                return DrawingsMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000245 RID: 581
        public static bool DrawHoldRadius
        {
            // Token: 0x06000888 RID: 2184 RVA: 0x0000B728 File Offset: 0x00009928
            get
            {
                return DrawingsMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000237 RID: 567
        internal static Menu DrawingsMenu
        {
            // Token: 0x06000877 RID: 2167 RVA: 0x0000B58C File Offset: 0x0000978C
            get;
            // Token: 0x06000878 RID: 2168 RVA: 0x0000B593 File Offset: 0x00009793
            set;
        }

        // Token: 0x17000243 RID: 579
        public static bool DrawLastHitMarker
        {
            // Token: 0x06000886 RID: 2182 RVA: 0x0000B6E8 File Offset: 0x000098E8
            get
            {
                return DrawingsMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000240 RID: 576
        public static bool DrawRange
        {
            // Token: 0x06000883 RID: 2179 RVA: 0x0000B688 File Offset: 0x00009888
            get
            {
                return DrawingsMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x1700024F RID: 591
        public static int ExtraFarmDelay
        {
            // Token: 0x06000894 RID: 2196 RVA: 0x0000B878 File Offset: 0x00009A78
            get
            {
                return FarmingMenu[""].Cast<Slider>().CurrentValue;
            }
        }

        // Token: 0x1700023E RID: 574
        public static int ExtraWindUpTime
        {
            // Token: 0x06000881 RID: 2177 RVA: 0x0000B648 File Offset: 0x00009848
            get
            {
                return Menu[""].Cast<Slider>().CurrentValue;
            }
        }

        // Token: 0x17000239 RID: 569
        internal static Menu FarmingMenu
        {
            // Token: 0x0600087B RID: 2171 RVA: 0x0000B5AA File Offset: 0x000097AA
            get;
            // Token: 0x0600087C RID: 2172 RVA: 0x0000B5B1 File Offset: 0x000097B1
            set;
        }

        // Token: 0x1700024D RID: 589
        internal static bool FastKiting
        {
            // Token: 0x06000892 RID: 2194 RVA: 0x0000B838 File Offset: 0x00009A38
            get
            {
                return Menu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000235 RID: 565
        public static AttackableUnit ForcedTarget
        {
            // Token: 0x06000873 RID: 2163 RVA: 0x0000B56E File Offset: 0x0000976E
            get;
            // Token: 0x06000874 RID: 2164 RVA: 0x0000B575 File Offset: 0x00009775
            set;
        }

        // Token: 0x17000250 RID: 592
        internal static bool FreezePriority
        {
            // Token: 0x06000895 RID: 2197 RVA: 0x0000B898 File Offset: 0x00009A98
            get
            {
                return FarmingMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000233 RID: 563
        public static bool GotAutoAttackReset
        {
            // Token: 0x0600086F RID: 2159 RVA: 0x0000B550 File Offset: 0x00009750
            get;
            // Token: 0x06000870 RID: 2160 RVA: 0x0000B557 File Offset: 0x00009757
            internal set;
        }

        // Token: 0x1700023A RID: 570
        public static int HoldRadius
        {
            // Token: 0x0600087D RID: 2173 RVA: 0x0000B5B9 File Offset: 0x000097B9
            get
            {
                return Menu["" + Player.Instance.ChampionName].Cast<Slider>().CurrentValue;
            }
        }

        // Token: 0x17000257 RID: 599
        internal static bool IllaoiGhost
        {
            // Token: 0x060008A0 RID: 2208 RVA: 0x0000B916 File Offset: 0x00009B16
            get;
            // Token: 0x060008A1 RID: 2209 RVA: 0x0000B91D File Offset: 0x00009B1D
            set;
        }

        // Token: 0x1700023F RID: 575
        public static bool IncludePing
        {
            // Token: 0x06000882 RID: 2178 RVA: 0x0000B668 File Offset: 0x00009868
            get
            {
                return Menu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000232 RID: 562
        public static bool IsAutoAttacking
        {
            // Token: 0x0600086E RID: 2158 RVA: 0x0000B53F File Offset: 0x0000973F
            get
            {
                return Player.Instance.Spellbook.IsAutoAttacking;
            }
        }

        // Token: 0x17000227 RID: 551
        public static bool IsMelee
        {
            // Token: 0x0600085F RID: 2143 RVA: 0x00023258 File Offset: 0x00021458
            get
            {
                return Player.Instance.IsMelee || Player.Instance.Hero == Champion.Azir || Player.Instance.Hero == Champion.Thresh || Player.Instance.Hero == Champion.Velkoz || (Player.Instance.Hero == Champion.Viktor && Player.Instance.HasBuff(""));
            }
        }

        // Token: 0x17000228 RID: 552
        public static bool IsRanged
        {
            // Token: 0x06000860 RID: 2144 RVA: 0x0000B508 File Offset: 0x00009708
            get
            {
                return !IsMelee;
            }
        }

        // Token: 0x1700023B RID: 571
        public static bool LaneClearAttackChamps
        {
            // Token: 0x0600087E RID: 2174 RVA: 0x0000B5E8 File Offset: 0x000097E8
            get
            {
                return Menu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x1700025F RID: 607
        public static Obj_AI_Minion LaneClearMinion
        {
            // Token: 0x060008C4 RID: 2244 RVA: 0x0000BB6B File Offset: 0x00009D6B
            get
            {
                return CurrentMinions[TargetMinionType.LaneClear];
            }
            // Token: 0x060008C5 RID: 2245 RVA: 0x0000BB78 File Offset: 0x00009D78
            internal set
            {
                CurrentMinions[TargetMinionType.LaneClear] = value;
            }
        }

        // Token: 0x1700025B RID: 603
        public static List<Obj_AI_Minion> LaneClearMinionsList
        {
            // Token: 0x060008BE RID: 2238 RVA: 0x0000BADE File Offset: 0x00009CDE
            get
            {
                if (!CurrentMinionsLists.ContainsKey(TargetMinionType.LaneClear))
                {
                    return new List<Obj_AI_Minion>();
                }
                return new List<Obj_AI_Minion>(CurrentMinionsLists[TargetMinionType.LaneClear]);
            }
        }

        // Token: 0x1700022C RID: 556
        public static int LastAutoAttack
        {
            // Token: 0x06000866 RID: 2150 RVA: 0x0000B530 File Offset: 0x00009730
            get;
            // Token: 0x06000867 RID: 2151 RVA: 0x0000B537 File Offset: 0x00009737
            internal set;
        }

        // Token: 0x1700025D RID: 605
        public static Obj_AI_Minion LastHitMinion
        {
            // Token: 0x060008C0 RID: 2240 RVA: 0x0000BB28 File Offset: 0x00009D28
            get
            {
                return CurrentMinions[TargetMinionType.LastHit];
            }
            // Token: 0x060008C1 RID: 2241 RVA: 0x0000BB35 File Offset: 0x00009D35
            internal set
            {
                CurrentMinions[TargetMinionType.LastHit] = value;
            }
        }

        // Token: 0x17000259 RID: 601
        public static List<Obj_AI_Minion> LastHitMinionsList
        {
            // Token: 0x060008BC RID: 2236 RVA: 0x0000BA94 File Offset: 0x00009C94
            get
            {
                if (!CurrentMinionsLists.ContainsKey(TargetMinionType.LastHit))
                {
                    return new List<Obj_AI_Minion>();
                }
                return new List<Obj_AI_Minion>(CurrentMinionsLists[TargetMinionType.LastHit]);
            }
        }

        // Token: 0x1700024E RID: 590
        public static bool LastHitPriority
        {
            // Token: 0x06000893 RID: 2195 RVA: 0x0000B858 File Offset: 0x00009A58
            get
            {
                return FarmingMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000251 RID: 593
        public static int LastMovementSent
        {
            // Token: 0x06000896 RID: 2198 RVA: 0x0000B8B8 File Offset: 0x00009AB8
            get;
            // Token: 0x06000897 RID: 2199 RVA: 0x0000B8BF File Offset: 0x00009ABF
            internal set;
        }

        // Token: 0x17000234 RID: 564
        public static AttackableUnit LastTarget
        {
            // Token: 0x06000871 RID: 2161 RVA: 0x0000B55F File Offset: 0x0000975F
            get;
            // Token: 0x06000872 RID: 2162 RVA: 0x0000B566 File Offset: 0x00009766
            internal set;
        }

        // Token: 0x17000236 RID: 566
        internal static Menu Menu
        {
            // Token: 0x06000875 RID: 2165 RVA: 0x0000B57D File Offset: 0x0000977D
            get;
            // Token: 0x06000876 RID: 2166 RVA: 0x0000B584 File Offset: 0x00009784
            set;
        }

        // Token: 0x1700023D RID: 573
        public static int MovementDelay
        {
            // Token: 0x06000880 RID: 2176 RVA: 0x0000B628 File Offset: 0x00009828
            get
            {
                return Menu[""].Cast<Slider>().CurrentValue;
            }
        }

        // Token: 0x1700022B RID: 555
        public static Vector3 OrbwalkPosition
        {
            // Token: 0x06000865 RID: 2149 RVA: 0x000232C4 File Offset: 0x000214C4
            get
            {
                if (OverrideOrbwalkPosition != null)
                {
                    Vector3? vector = OverrideOrbwalkPosition();
                    if (vector.HasValue)
                    {
                        return vector.Value;
                    }
                }
                if (StickToTarget && LastTarget != null && !ActiveModesFlags.HasFlag(ActiveModes.Flee))
                {
                    Obj_AI_Base obj_AI_Base = LastTarget as Obj_AI_Base;
                    if (obj_AI_Base != null && (obj_AI_Base.IsMonster || obj_AI_Base.Type == GameObjectType.AIHeroClient) && Player.Instance.IsInRange(obj_AI_Base, Player.Instance.GetAutoAttackRange(obj_AI_Base) + 150f) && Game.CursorPos.Distance(obj_AI_Base, true) < Game.CursorPos.Distance(Player.Instance, true) && obj_AI_Base.Path.Length > 0)
                    {
                        return obj_AI_Base.Path.Last<Vector3>();
                    }
                }
                return Game.CursorPos;
            }
        }

        // Token: 0x1700022A RID: 554
        public static OrbwalkPositionDelegate OverrideOrbwalkPosition
        {
            // Token: 0x06000863 RID: 2147 RVA: 0x0000B521 File Offset: 0x00009721
            get;
            // Token: 0x06000864 RID: 2148 RVA: 0x0000B528 File Offset: 0x00009728
            set;
        }

        // Token: 0x1700023C RID: 572
        public static bool ParanoidMode
        {
            // Token: 0x0600087F RID: 2175 RVA: 0x0000B608 File Offset: 0x00009808
            get
            {
                return Menu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x1700025E RID: 606
        public static Obj_AI_Minion PriorityLastHitWaitingMinion
        {
            // Token: 0x060008C2 RID: 2242 RVA: 0x0000BB43 File Offset: 0x00009D43
            get
            {
                return CurrentMinions[TargetMinionType.PriorityLastHitWaiting];
            }
            // Token: 0x060008C3 RID: 2243 RVA: 0x0000BB50 File Offset: 0x00009D50
            internal set
            {
                if (value != null)
                {
                    _lastShouldWait = Core.GameTickCount;
                }
                CurrentMinions[TargetMinionType.PriorityLastHitWaiting] = value;
            }
        }

        // Token: 0x1700025A RID: 602
        public static List<Obj_AI_Minion> PriorityLastHitWaitingMinionsList
        {
            // Token: 0x060008BD RID: 2237 RVA: 0x0000BAB9 File Offset: 0x00009CB9
            get
            {
                if (!CurrentMinionsLists.ContainsKey(TargetMinionType.PriorityLastHitWaiting))
                {
                    return new List<Obj_AI_Minion>();
                }
                return new List<Obj_AI_Minion>(CurrentMinionsLists[TargetMinionType.PriorityLastHitWaiting]);
            }
        }

        // Token: 0x17000252 RID: 594
        internal static Random Random
        {
            // Token: 0x06000898 RID: 2200 RVA: 0x0000B8C7 File Offset: 0x00009AC7
            get;
            // Token: 0x06000899 RID: 2201 RVA: 0x0000B8CE File Offset: 0x00009ACE
            set;
        }

        // Token: 0x17000253 RID: 595
        internal static int RandomOffset
        {
            // Token: 0x0600089A RID: 2202 RVA: 0x0000B8D6 File Offset: 0x00009AD6
            get;
            // Token: 0x0600089B RID: 2203 RVA: 0x0000B8DD File Offset: 0x00009ADD
            set;
        }

        // Token: 0x17000258 RID: 600
        public static bool ShouldWait
        {
            // Token: 0x060008B5 RID: 2229 RVA: 0x0000BA2C File Offset: 0x00009C2C
            get
            {
                return Core.GameTickCount - _lastShouldWait <= 400 || PriorityLastHitWaitingMinion != null;
            }
        }

        // Token: 0x1700024C RID: 588
        internal static bool StickToTarget
        {
            // Token: 0x06000891 RID: 2193 RVA: 0x0000B80A File Offset: 0x00009A0A
            get
            {
                return Player.Instance.IsMelee && Menu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x1700025C RID: 604
        public static List<Obj_AI_Minion> UnKillableMinionsList
        {
            // Token: 0x060008BF RID: 2239 RVA: 0x0000BB03 File Offset: 0x00009D03
            get
            {
                if (!CurrentMinionsLists.ContainsKey(TargetMinionType.UnKillable))
                {
                    return new List<Obj_AI_Minion>();
                }
                return new List<Obj_AI_Minion>(CurrentMinionsLists[TargetMinionType.UnKillable]);
            }
        }

        // Token: 0x17000248 RID: 584
        public static bool UseOnTick
        {
            // Token: 0x0600088D RID: 2189 RVA: 0x0000B7AA File Offset: 0x000099AA
            get
            {
                return AdvancedMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x17000249 RID: 585
        public static bool UseOnUpdate
        {
            // Token: 0x0600088E RID: 2190 RVA: 0x0000B7CA File Offset: 0x000099CA
            get
            {
                return AdvancedMenu[""].Cast<CheckBox>().CurrentValue;
            }
        }

        // Token: 0x1700024A RID: 586
        internal static bool UseTiamat
        {
            // Token: 0x0600088F RID: 2191 RVA: 0x000236B0 File Offset: 0x000218B0
            get
            {
                return Player.Instance.IsMelee && FarmingMenu[""].Cast<CheckBox>().CurrentValue && Player.Instance.InventoryItems.HasItem(new ItemId[]
                {
                    ItemId.Tiamat,
                    ItemId.Ravenous_Hydra
                });
            }
        }

        // Token: 0x17000255 RID: 597
        public static List<Obj_AI_Minion> ValidAzirSoldiers
        {
            // Token: 0x0600089D RID: 2205 RVA: 0x0000B8F6 File Offset: 0x00009AF6
            get
            {
                return _validAzirSoldiers.Values.ToList<Obj_AI_Minion>();
            }
        }

        // Token: 0x14000019 RID: 25
        // Token: 0x06000854 RID: 2132 RVA: 0x00022FB0 File Offset: 0x000211B0
        // Token: 0x06000855 RID: 2133 RVA: 0x00022FE4 File Offset: 0x000211E4
        public static event AttackHandler OnAttack;

        // Token: 0x1400001A RID: 26
        // Token: 0x06000856 RID: 2134 RVA: 0x00023018 File Offset: 0x00021218
        // Token: 0x06000857 RID: 2135 RVA: 0x0002304C File Offset: 0x0002124C
        public static event PostAttackHandler OnPostAttack;

        // Token: 0x14000018 RID: 24
        // Token: 0x06000852 RID: 2130 RVA: 0x00022F48 File Offset: 0x00021148
        // Token: 0x06000853 RID: 2131 RVA: 0x00022F7C File Offset: 0x0002117C
        public static event PreAttackHandler OnPreAttack;

        // Token: 0x1400001B RID: 27
        // Token: 0x06000858 RID: 2136 RVA: 0x00023080 File Offset: 0x00021280
        // Token: 0x06000859 RID: 2137 RVA: 0x000230B4 File Offset: 0x000212B4
        public static event UnkillableMinionHandler OnUnkillableMinion;

        // Token: 0x040003F1 RID: 1009
        public static readonly Dictionary<Champion, string> AllowedMovementBuffs = new Dictionary<Champion, string>
        {
            {
                Champion.Lucian,
                ""
            },
            {
                Champion.Varus,
                ""
            },
            {
                Champion.Vi,
                ""
            },
            {
                Champion.Vladimir,
                ""
            },
            {
                Champion.Xerath,
                ""
            }
        };

        // Token: 0x040003E9 RID: 1001
        public const float AzirSoldierAutoAttackRange = 275f;

        // Token: 0x04000405 RID: 1029
        internal static readonly Dictionary<TargetMinionType, Obj_AI_Minion> CurrentMinions = new Dictionary<TargetMinionType, Obj_AI_Minion>
        {
            {
                TargetMinionType.LaneClear,
                null
            },
            {
                TargetMinionType.LastHit,
                null
            },
            {
                TargetMinionType.PriorityLastHitWaiting,
                null
            }
        };

        // Token: 0x04000404 RID: 1028
        internal static readonly Dictionary<TargetMinionType, List<Obj_AI_Minion>> CurrentMinionsLists = new Dictionary<TargetMinionType, List<Obj_AI_Minion>>();

        // Token: 0x04000406 RID: 1030
        internal static readonly Dictionary<int, CalculatedMinionValue> CurrentMinionValues = new Dictionary<int, CalculatedMinionValue>();

        // Token: 0x04000400 RID: 1024
        private static readonly Dictionary<int, float> DamageOnMinions = new Dictionary<int, float>();

        // Token: 0x040003FA RID: 1018
        internal static readonly ColorBGRA EnemyRangeColorInRange = new ColorBGRA(255, 0, 0, 100);

        // Token: 0x040003F9 RID: 1017
        internal static readonly ColorBGRA EnemyRangeColorNotInRange = new ColorBGRA(144, 238, 144, 100);

        // Token: 0x040003FD RID: 1021
        internal static readonly List<AttackableUnit> EnemyStructures = new List<AttackableUnit>();

        // Token: 0x040003F8 RID: 1016
        internal static readonly Dictionary<int, Obj_AI_Base> LastTargetTurrets = new Dictionary<int, Obj_AI_Base>();

        // Token: 0x040003E7 RID: 999
        internal const int MinionsRangeSqr = 2250000;

        // Token: 0x040003FE RID: 1022
        internal static readonly List<Obj_AI_Minion> TickCachedMinions = new List<Obj_AI_Minion>();

        // Token: 0x040003FF RID: 1023
        internal static readonly List<Obj_AI_Minion> TickCachedMonsters = new List<Obj_AI_Minion>();

        // Token: 0x040003E8 RID: 1000
        internal const int TurretRangeSqr = 688900;

        // Token: 0x040003F0 RID: 1008
        private static bool _autoAttackCompleted;

        // Token: 0x040003EF RID: 1007
        private static bool _autoAttackStarted;

        // Token: 0x040003FB RID: 1019
        internal static readonly Dictionary<int, Obj_AI_Minion> _azirSoldiers = new Dictionary<int, Obj_AI_Minion>();

        // Token: 0x040003F3 RID: 1011
        internal static bool _disableAttacking;

        // Token: 0x040003F2 RID: 1010
        internal static bool _disableMovement;

        // Token: 0x040003EE RID: 1006
        private static int _lastAutoAttackSent;

        // Token: 0x040003F6 RID: 1014
        private static Vector3? _lastIssueOrderEndVector;

        // Token: 0x040003F7 RID: 1015
        private static Vector3? _lastIssueOrderStartVector;

        // Token: 0x040003F5 RID: 1013
        private static int? _lastIssueOrderTargetId;

        // Token: 0x040003F4 RID: 1012
        private static GameObjectOrder? _lastIssueOrderType;

        // Token: 0x04000402 RID: 1026
        private static int _lastShouldWait;

        // Token: 0x04000403 RID: 1027
        private static bool _onlyLastHit;

        // Token: 0x04000401 RID: 1025
        private static PrecalculatedAutoAttackDamage _precalculatedDamage;
        // Token: 0x040003FC RID: 1020
        internal static readonly Dictionary<int, Obj_AI_Minion> _validAzirSoldiers = new Dictionary<int, Obj_AI_Minion>();

        // Token: 0x020000E2 RID: 226
        internal class PrecalculatedAutoAttackDamage
        {
            // Token: 0x0400034F RID: 847
            internal DamageType _autoAttackDamageType;

            // Token: 0x0400034D RID: 845
            internal float _calculatedMagical;

            // Token: 0x0400034C RID: 844
            internal float _calculatedPhysical;

            // Token: 0x0400034E RID: 846
            internal float _calculatedTrue;

            // Token: 0x0400034B RID: 843
            internal float _rawMagical;

            // Token: 0x0400034A RID: 842
            internal float _rawPhysical;

            // Token: 0x04000349 RID: 841
            internal float _rawTotal;
        }   

    // Token: 0x0200010C RID: 268
    [Flags]
        public enum ActiveModes
        {
            // Token: 0x04000437 RID: 1079
            None = 0,
            // Token: 0x04000438 RID: 1080
            Combo = 1,
            // Token: 0x04000439 RID: 1081
            Harass = 2,
            // Token: 0x0400043A RID: 1082
            LastHit = 4,
            // Token: 0x0400043B RID: 1083
            JungleClear = 8,
            // Token: 0x0400043C RID: 1084
            LaneClear = 16,
            // Token: 0x0400043D RID: 1085
            Flee = 32
        }

        // Token: 0x02000112 RID: 274
        // Token: 0x060008FC RID: 2300
        public delegate void AttackHandler(AttackableUnit target, EventArgs args);

        // Token: 0x02000116 RID: 278
        internal class CalculatedMinionValue
        {
            // Token: 0x06000915 RID: 2325 RVA: 0x0000BDC0 File Offset: 0x00009FC0
            internal CalculatedMinionValue(Obj_AI_Minion minion)
            {
                this.Handle = minion;
                this.LastHitHealth = this.Handle.Health;
                this.LaneClearHealth = this.Handle.Health;
            }

            // Token: 0x17000263 RID: 611
            internal Obj_AI_Minion Handle
            {
                // Token: 0x06000907 RID: 2311 RVA: 0x0000BD44 File Offset: 0x00009F44
                get;
                // Token: 0x06000908 RID: 2312 RVA: 0x0000BD4C File Offset: 0x00009F4C
                set;
            }

            // Token: 0x1700026A RID: 618
            internal bool IsAlmostLastHittable
            {
                // Token: 0x06000913 RID: 2323 RVA: 0x00025D64 File Offset: 0x00023F64
                get
                {
                    float num = this.Handle.HasTurretTargetting() ? this.LastHitHealth : this.LaneClearHealth;
                    return num <= GetAutoAttackDamage(this.Handle) && num < this.Handle.Health;
                }
            }

            // Token: 0x1700026B RID: 619
            internal bool IsLaneClearMinion
            {
                // Token: 0x06000914 RID: 2324 RVA: 0x00025DAC File Offset: 0x00023FAC
                get
                {
                    if (_onlyLastHit)
                    {
                        return false;
                    }
                    Obj_AI_Turret obj_AI_Turret = EloBuddy.SDK.EntityManager.Turrets.Allies.FirstOrDefault((Obj_AI_Turret t) => t.Distance(this.Handle, true) <= 688900f);
                    if (obj_AI_Turret != null)
                    {
                        float autoAttackDamage = obj_AI_Turret.GetAutoAttackDamage(this.Handle, false);
                        float autoAttackDamage2 = GetAutoAttackDamage(this.Handle);
                        float num = this.Handle.Health;
                        while (num > 0f && autoAttackDamage > 0f)
                        {
                            if (num <= autoAttackDamage2)
                            {
                                return false;
                            }
                            num -= autoAttackDamage;
                        }
                        return true;
                    }
                    return this.LaneClearHealth > 2f * ((Player.Instance.FlatCritChanceMod >= 0.5f && Player.Instance.FlatCritChanceMod < 1f) ? Player.Instance.GetCriticalStrikePercentMod() : 1f) * GetAutoAttackDamage(this.Handle) || Math.Abs(this.LaneClearHealth - this.Handle.Health) < 1.401298E-45f;
                }
            }

            // Token: 0x17000269 RID: 617
            internal bool IsLastHittable
            {
                // Token: 0x06000912 RID: 2322 RVA: 0x0000BDA8 File Offset: 0x00009FA8
                get
                {
                    return this.LastHitHealth <= GetAutoAttackDamage(this.Handle);
                }
            }

            // Token: 0x17000268 RID: 616
            internal bool IsUnkillable
            {
                // Token: 0x06000911 RID: 2321 RVA: 0x0000BD99 File Offset: 0x00009F99
                get
                {
                    return this.LastHitHealth < 0f;
                }
            }

            // Token: 0x17000267 RID: 615
            internal float LaneClearHealth
            {
                // Token: 0x0600090F RID: 2319 RVA: 0x0000BD88 File Offset: 0x00009F88
                get;
                // Token: 0x06000910 RID: 2320 RVA: 0x0000BD90 File Offset: 0x00009F90
                set;
            }

            // Token: 0x17000265 RID: 613
            internal int LaneClearProjectileTime
            {
                // Token: 0x0600090B RID: 2315 RVA: 0x0000BD66 File Offset: 0x00009F66
                get;
                // Token: 0x0600090C RID: 2316 RVA: 0x0000BD6E File Offset: 0x00009F6E
                set;
            }

            // Token: 0x17000266 RID: 614
            internal float LastHitHealth
            {
                // Token: 0x0600090D RID: 2317 RVA: 0x0000BD77 File Offset: 0x00009F77
                get;
                // Token: 0x0600090E RID: 2318 RVA: 0x0000BD7F File Offset: 0x00009F7F
                set;
            }

            // Token: 0x17000264 RID: 612
            internal int LastHitProjectileTime
            {
                // Token: 0x06000909 RID: 2313 RVA: 0x0000BD55 File Offset: 0x00009F55
                get;
                // Token: 0x0600090A RID: 2314 RVA: 0x0000BD5D File Offset: 0x00009F5D
                set;
            }
        }

        // Token: 0x02000110 RID: 272
        // Token: 0x060008F4 RID: 2292
        public delegate Vector3? OrbwalkPositionDelegate();

        // Token: 0x02000113 RID: 275
        // Token: 0x06000900 RID: 2304
        public delegate void PostAttackHandler(AttackableUnit target, EventArgs args);

        // Token: 0x0200010E RID: 270
        public class PreAttackArgs : EventArgs
        {
            // Token: 0x060008EF RID: 2287 RVA: 0x0000BD1D File Offset: 0x00009F1D
            public PreAttackArgs(AttackableUnit target)
            {
                this.Process = true;
                this.Target = target;
            }

            // Token: 0x17000260 RID: 608
            public bool Process
            {
                // Token: 0x060008EB RID: 2283 RVA: 0x0000BCFB File Offset: 0x00009EFB
                get;
                // Token: 0x060008EC RID: 2284 RVA: 0x0000BD03 File Offset: 0x00009F03
                set;
            }

            // Token: 0x17000261 RID: 609
            public AttackableUnit Target
            {
                // Token: 0x060008ED RID: 2285 RVA: 0x0000BD0C File Offset: 0x00009F0C
                get;
                // Token: 0x060008EE RID: 2286 RVA: 0x0000BD14 File Offset: 0x00009F14
                private set;
            }
        }

        // Token: 0x02000111 RID: 273
        // Token: 0x060008F8 RID: 2296
        public delegate void PreAttackHandler(AttackableUnit target, PreAttackArgs args);

        // Token: 0x02000115 RID: 277
        internal enum TargetMinionType
        {
            // Token: 0x04000447 RID: 1095
            LastHit,
            // Token: 0x04000448 RID: 1096
            PriorityLastHitWaiting,
            // Token: 0x04000449 RID: 1097
            LaneClear,
            // Token: 0x0400044A RID: 1098
            UnKillable
        }

        // Token: 0x0200010D RID: 269
        internal enum TargetTypes
        {
            // Token: 0x0400043F RID: 1087
            Hero,
            // Token: 0x04000440 RID: 1088
            JungleMob,
            // Token: 0x04000441 RID: 1089
            LaneMinion,
            // Token: 0x04000442 RID: 1090
            Structure
        }

        // Token: 0x0200010F RID: 271
        public class UnkillableMinionArgs : EventArgs
        {
            // Token: 0x17000262 RID: 610
            public float RemainingHealth
            {
                // Token: 0x060008F0 RID: 2288 RVA: 0x0000BD33 File Offset: 0x00009F33
                get;
                // Token: 0x060008F1 RID: 2289 RVA: 0x0000BD3B File Offset: 0x00009F3B
                internal set;
            }
        }

        // Token: 0x02000114 RID: 276
        // Token: 0x06000904 RID: 2308
        public delegate void UnkillableMinionHandler(Obj_AI_Base target, UnkillableMinionArgs args);
    }
}
//*/