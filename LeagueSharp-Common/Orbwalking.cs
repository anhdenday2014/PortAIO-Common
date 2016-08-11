namespace LeagueSharp.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharpDX;

    using Color = System.Drawing.Color;
    using EloBuddy;
    using EloBuddy.SDK;
    using SDK.Utils;
    using SDK;

    //    using EloBuddy.SDK;

    /// <summary>
    ///     This class offers everything related to auto-attacks and orbwalking.
    /// </summary>
    public static class Orbwalking
    {
        #region Static Fields

        /// <summary>
        ///     <c>true</c> if the orbwalker will attack.
        /// </summary>
        public static bool Attack = true;

        /// <summary>
        ///     <c>true</c> if the orbwalker will skip the next attack.
        /// </summary>
        public static bool DisableNextAttack;

        /// <summary>
        ///     The last auto attack tick
        /// </summary>
        public static int LastAATick;

        /// <summary>
        ///     The tick the most recent attack command was sent.
        /// </summary>
        public static int LastAttackCommandT;

        /// <summary>
        ///     The last move command position
        /// </summary>
        public static Vector3 LastMoveCommandPosition = Vector3.Zero;

        /// <summary>
        ///     The tick the most recent move command was sent.
        /// </summary>
        public static int LastMoveCommandT;

        /// <summary>
        ///     <c>true</c> if the orbwalker will move.
        /// </summary>
        public static bool Move = true;

        /// <summary>
        ///     The champion name
        /// </summary>
        private static readonly string _championName;

        /// <summary>
        ///     The random
        /// </summary>
        private static readonly Random _random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        ///     Spells that reset the attack timer.
        /// </summary>
        private static readonly string[] AttackResets =
            {
                "dariusnoxiantacticsonh", "fiorae", "garenq", "gravesmove",
                "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge",
                "leonashieldofdaybreak", "luciane", "monkeykingdoubleattack",
                "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze",
                "netherblade", "gangplankqwrapper", "powerfist",
                "renektonpreexecute", "rengarq", "shyvanadoubleattack",
                "sivirw", "takedown", "talonnoxiandiplomacy",
                "trundletrollsmash", "vaynetumble", "vie", "volibearq",
                "xenzhaocombotarget", "yorickspectral", "reksaiq",
                "itemtitanichydracleave", "masochism", "illaoiw",
                "elisespiderw", "fiorae", "meditate", "sejuaninorthernwinds",
                "asheq"
            };

        /// <summary>
        ///     Spells that are attacks even if they dont have the "attack" word in their name.
        /// </summary>
        private static readonly string[] Attacks =
            {
                "caitlynheadshotmissile", "frostarrow", "garenslash2",
                "kennenmegaproc", "masteryidoublestrike", "quinnwenhanced",
                "renektonexecute", "renektonsuperexecute",
                "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust",
                "xenzhaothrust2", "xenzhaothrust3", "viktorqbuff",
                "lucianpassiveshot"
            };

        /// <summary>
        ///     Spells that are not attacks even if they have the "attack" word in their name.
        /// </summary>
        private static readonly string[] NoAttacks =
            {
                "volleyattack", "volleyattackwithsound",
                "jarvanivcataclysmattack", "monkeykingdoubleattack",
                "shyvanadoubleattack", "shyvanadoubleattackdragon",
                "zyragraspingplantattack", "zyragraspingplantattack2",
                "zyragraspingplantattackfire", "zyragraspingplantattack2fire",
                "viktorpowertransfer", "sivirwattackbounce", "asheqattacknoonhit",
                "elisespiderlingbasicattack", "heimertyellowbasicattack",
                "heimertyellowbasicattack2", "heimertbluebasicattack",
                "annietibbersbasicattack", "annietibbersbasicattack2",
                "yorickdecayedghoulbasicattack", "yorickravenousghoulbasicattack",
                "yorickspectralghoulbasicattack", "malzaharvoidlingbasicattack",
                "malzaharvoidlingbasicattack2", "malzaharvoidlingbasicattack3",
                "kindredwolfbasicattack"
            };

        /// <summary>
        ///     Champs whose auto attacks can't be cancelled
        /// </summary>
        private static readonly string[] NoCancelChamps = { "Kalista" };

        /// <summary>
        ///     The player
        /// </summary>
        private static readonly AIHeroClient Player;

        private static int _autoattackCounter;

        /// <summary>
        ///     The delay
        /// </summary>
        private static int _delay;

        /// <summary>
        ///     The last target
        /// </summary>
        private static AttackableUnit _lastTarget;

        /// <summary>
        ///     The minimum distance
        /// </summary>
        private static float _minDistance = 400;

        /// <summary>
        ///     <c>true</c> if the auto attack missile was launched from the player.
        /// </summary>
        private static bool _missileLaunched;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="Orbwalking" /> class.
        /// </summary>
        static Orbwalking()
        {
            GameObjects.Initialize();

            Player = ObjectManager.Player;
            _championName = Player.ChampionName;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell222;
            Obj_AI_Base.OnSpellCast += OnProcessSpell;

            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnDoCast;
            Spellbook.OnStopCast += SpellbookOnStopCast;

            //            EloBuddy.Player.OnIssueOrder += OnIssueOrder;

            if (_championName == "Rengar")
            {
                Obj_AI_Base.OnPlayAnimation += delegate (Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
                    {
                        if (sender.IsMe && args.Animation == "Spell5")
                        {
                            var t = 0;

                            if (_lastTarget != null && _lastTarget.IsValid)
                            {
                                t += (int)Math.Min(ObjectManager.Player.Distance(_lastTarget) / 1.5f, 0.6f);
                            }

                            LastAATick = Utils.GameTimeTickCount - Game.Ping / 2 + t;
                        }
                    };
            }
        }

        private static void OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (sender.IsMe && args.Order == GameObjectOrder.MoveTo)
            {
                //                LastMoveCommandT = Core.GameTickCount + Game.Ping;
            }
            if (sender.IsMe && args.Order == GameObjectOrder.AttackUnit)
            {
                //                LastMoveCommandT = Core.GameTickCount + Game.Ping;
                //                _lastAutoAttackSent = Core.GameTickCount;
                _missileLaunched = true;
                _lastTarget = args.Target as AttackableUnit;
            }
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     Delegate AfterAttackEvenH
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="target">The target.</param>
        public delegate void AfterAttackEvenH(AttackableUnit unit, AttackableUnit target);

        /// <summary>
        ///     Delegate BeforeAttackEvenH
        /// </summary>
        /// <param name="args">The <see cref="BeforeAttackEventArgs" /> instance containing the event data.</param>
        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);

        /// <summary>
        ///     Delegate OnAttackEvenH
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="target">The target.</param>
        public delegate void OnAttackEvenH(AttackableUnit unit, AttackableUnit target);

        /// <summary>
        ///     Delegate OnNonKillableMinionH
        /// </summary>
        /// <param name="minion">The minion.</param>
        public delegate void OnNonKillableMinionH(AttackableUnit minion);

        /// <summary>
        ///     Delegate OnTargetChangeH
        /// </summary>
        /// <param name="oldTarget">The old target.</param>
        /// <param name="newTarget">The new target.</param>
        public delegate void OnTargetChangeH(AttackableUnit oldTarget, AttackableUnit newTarget);

        #endregion

        #region Public Events

        /// <summary>
        ///     This event is fired after a unit finishes auto-attacking another unit (Only works with player for now).
        /// </summary>
        public static event AfterAttackEvenH AfterAttack;

        /// <summary>
        ///     This event is fired before the player auto attacks.
        /// </summary>
        public static event BeforeAttackEvenH BeforeAttack;

        /// <summary>
        ///     This event is fired when a unit is about to auto-attack another unit.
        /// </summary>
        public static event OnAttackEvenH OnAttack;

        /// <summary>
        ///     Occurs when a minion is not killable by an auto attack.
        /// </summary>
        public static event OnNonKillableMinionH OnNonKillableMinion;

        /// <summary>
        ///     Gets called on target changes
        /// </summary>
        public static event OnTargetChangeH OnTargetChange;

        #endregion

        #region Enums

        /// <summary>
        ///     The orbwalking mode.
        /// </summary>
        public enum OrbwalkingMode
        {
            /// <summary>
            ///     The orbwalker will only last hit minions.
            /// </summary>
            LastHit,

            /// <summary>
            ///     The orbwalker will alternate between last hitting and auto attacking champions.
            /// </summary>
            Mixed,

            /// <summary>
            ///     The orbwalker will clear the lane of minions as fast as possible while attempting to get the last hit.
            /// </summary>
            LaneClear,

            /// <summary>
            ///     The orbwalker will only attack the target.
            /// </summary>
            Combo,

            /// <summary>
            ///     The orbwalker will only last hit minions as late as possible.
            /// </summary>
            Freeze,

            /// <summary>
            ///     The orbwalker will only move.
            /// </summary>
            CustomMode,

            Flee,

            //Burst,

            /// <summary>
            ///     The orbwalker does nothing.
            /// </summary>
            None
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Returns if the player's auto-attack is ready.
        /// </summary>
        /// <returns><c>true</c> if this instance can attack; otherwise, <c>false</c>.</returns>
        /// <summary>
        ///     Returns if the player's auto-attack is ready.
        /// </summary>
        /// <returns><c>true</c> if this instance can attack; otherwise, <c>false</c>.</returns>
        public static bool CanAttack()
        {
            if (Player.ChampionName == "Graves")
            {
                var attackDelay = 1.0740296828d * 1000 * Player.AttackDelay - 716.2381256175d;
                if (Utils.GameTimeTickCount + Game.Ping / 2 + 25 >= LastAATick + attackDelay
                    && Player.HasBuff("GravesBasicAttackAmmo1"))
                {
                    return true;
                }

                return false;
            }

            if (Player.ChampionName == "Jhin")
            {
                if (Player.HasBuff("JhinPassiveReload"))
                {
                    return false;
                }
            }

            if (Player.IsCastingInterruptableSpell())
            {
                return false;
            }

            return Utils.GameTimeTickCount + Game.Ping / 2 >= LastAttackCommandT + Player.AttackDelay * 1000;
        }

        /// <summary>
        ///     Returns true if moving won't cancel the auto-attack.
        /// </summary>
        /// <param name="extraWindup">The extra windup.</param>
        /// <returns><c>true</c> if this instance can move the specified extra windup; otherwise, <c>false</c>.</returns>
        public static bool CanMove(float extraWindup, bool disableMissileCheck = false)
        {
            if (_missileLaunched && Orbwalker.MissileCheck && !disableMissileCheck)
            {
                return true;
            }

            var localExtraWindup = 0;
            if (_championName == "Rengar" && (Player.HasBuff("rengarqbase") || Player.HasBuff("rengarqemp")))
            {
                localExtraWindup = 200;
            }

            return NoCancelChamps.Contains(_championName) || (Utils.GameTimeTickCount + Game.Ping / 2 + 25 >= LastAttackCommandT + Player.AttackCastDelay * 1000 + extraWindup + localExtraWindup);
        }

        /// <summary>
        ///     Returns the auto-attack range of the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>System.Single.</returns>
        public static float GetAttackRange(AIHeroClient target)
        {
            var result = target.AttackRange + target.BoundingRadius;
            return result;
        }

        /// <summary>
        ///     Gets the last move position.
        /// </summary>
        /// <returns>Vector3.</returns>
        public static Vector3 GetLastMovePosition()
        {
            return LastMoveCommandPosition;
        }

        /// <summary>
        ///     Gets the last move time.
        /// </summary>
        /// <returns>System.Single.</returns>
        public static float GetLastMoveTime()
        {
            return LastMoveCommandT;
        }

        /// <summary>
        ///     Returns player auto-attack missile speed.
        /// </summary>
        /// <returns>System.Single.</returns>
        public static float GetMyProjectileSpeed()
        {
            return IsMelee(Player) || _championName == "Azir" || _championName == "Velkoz"
                   || _championName == "Viktor" && Player.HasBuff("ViktorPowerTransferReturn")
                       ? float.MaxValue
                       : Player.BasicAttack.MissileSpeed;
        }

        /// <summary>
        ///     Returns the auto-attack range of local player with respect to the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>System.Single.</returns>
        public static float GetRealAutoAttackRange(AttackableUnit target)
        {
            var result = Player.AttackRange;
            if (target.IsValidTarget())
            {
                var aiBase = target as Obj_AI_Base;
                if (aiBase != null && Player.ChampionName == "Caitlyn")
                {
                    if (aiBase.HasBuff("caitlynyordletrapinternal"))
                    {
                        result += 650;
                    }
                }

                return result;
            }

            return result;
        }

        /// <summary>
        ///     Returns true if the target is in auto-attack range.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool InAutoAttackRange(AttackableUnit target)
        {
            if (!target.IsValidTarget())
            {
                return false;
            }
            var myRange = GetRealAutoAttackRange(target);
            return
                Vector2.DistanceSquared(
                    target is Obj_AI_Base ? ((Obj_AI_Base)target).ServerPosition.To2D() : target.Position.To2D(),
                    Player.ServerPosition.To2D()) <= myRange * myRange;
        }

        /// <summary>
        ///     Returns true if the spellname is an auto-attack.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the name is an auto attack; otherwise, <c>false</c>.</returns>
        public static bool IsAutoAttack(string name)
        {
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower()))
                   || Attacks.Contains(name.ToLower());
        }

        /// <summary>
        ///     Returns true if the spellname resets the attack timer.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the specified name is an auto attack reset; otherwise, <c>false</c>.</returns>
        public static bool IsAutoAttackReset(string name)
        {
            return AttackResets.Contains(name.ToLower());
        }

        /// <summary>
        ///     Returns true if the unit is melee
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns><c>true</c> if the specified unit is melee; otherwise, <c>false</c>.</returns>
        public static bool IsMelee(this Obj_AI_Base unit)
        {
            return unit.CombatType == GameObjectCombatType.Melee;
        }

        /// <summary>
        ///     The random.
        /// </summary>
        private static readonly Random random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        ///     Moves to the position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="holdAreaRadius">The hold area radius.</param>
        /// <param name="overrideTimer">if set to <c>true</c> [override timer].</param>
        /// <param name="useFixedDistance">if set to <c>true</c> [use fixed distance].</param>
        /// <param name="randomizeMinDistance">if set to <c>true</c> [randomize minimum distance].</param>
        public static void MoveTo(
            Vector3 position,
            float holdAreaRadius = 0,
            bool overrideTimer = false,
            bool useFixedDistance = true,
            bool randomizeMinDistance = true)
        {
            var playerPosition = Player.ServerPosition;

            if (playerPosition.Distance(position, true) < holdAreaRadius * holdAreaRadius)
            {
                if (Player.Path.Length > 0)
                {
                    EloBuddy.Player.IssueOrder(GameObjectOrder.Stop, playerPosition);
                    LastMoveCommandPosition = playerPosition;
                    LastMoveCommandT = Utils.GameTimeTickCount - 70;
                }
                return;
            }

            if (position.Distance(GameObjects.Player.ServerPosition) < GameObjects.Player.BoundingRadius)
            {
                position = GameObjects.Player.ServerPosition.Extend(position, GameObjects.Player.BoundingRadius + random.Next(0, 51));
            }

            var angle = 0f;
            var currentPath = GameObjects.Player.GetWaypoints();
            if (currentPath.Count > 1 && currentPath.PathLength() > 100)
            {
                var movePath = GameObjects.Player.GetPath(position);
                if (movePath.Length > 1)
                {
                    var v1 = currentPath[1] - currentPath[0];
                    var v2 = movePath[1] - movePath[0];
                    angle = v1.AngleBetween(v2);
                    var distance = movePath.Last().DistanceSquared(currentPath.Last());
                    if ((angle < 10 && distance < 500 * 500) || distance < 50 * 50)
                    {
                        return;
                    }
                }
            }

            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, position);
            LastMoveCommandPosition = position;
            LastMoveCommandT = Utils.GameTimeTickCount;
        }

        /// <summary>
        ///     Orbwalks a target while moving to Position.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="position">The position.</param>
        /// <param name="extraWindup">The extra windup.</param>
        /// <param name="holdAreaRadius">The hold area radius.</param>
        /// <param name="useFixedDistance">if set to <c>true</c> [use fixed distance].</param>
        /// <param name="randomizeMinDistance">if set to <c>true</c> [randomize minimum distance].</param>
        public static void Orbwalk(
            AttackableUnit target,
            Vector3 position,
            float extraWindup = 90,
            float holdAreaRadius = 0,
            bool useFixedDistance = true,
            bool randomizeMinDistance = true)
        {
            try
            {
                if (target.IsValidTarget() && EloBuddy.SDK.Orbwalker.CanAutoAttack && Attack)
                {
                    DisableNextAttack = false;
                    FireBeforeAttack(target);

                    if (!DisableNextAttack)
                    {
                        if (!NoCancelChamps.Contains(_championName))
                        {
                            _missileLaunched = false;
                        }

                        if (InAutoAttackRange(target) && target != null && !target.IsDead && target.IsVisible && target.IsTargetable)
                        {
                            if (EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target))
                            {
                                LastAttackCommandT = Utils.GameTimeTickCount;
                                _lastTarget = target;
                            }
                        }

                        return;
                    }
                }

                if (EloBuddy.SDK.Orbwalker.CanMove && Move)
                {
                    if (Orbwalker.LimitAttackSpeed && (Player.AttackDelay < 1 / 2.6f) && _autoattackCounter % 3 != 0)
                    {
                        return;
                    }

                    MoveTo(position, Math.Max(holdAreaRadius, 30), false, useFixedDistance, randomizeMinDistance);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     Resets the Auto-Attack timer.
        /// </summary>
        public static void ResetAutoAttackTimer()
        {
            LastAATick = 0;
        }

        /// <summary>
        ///     Sets the minimum orbwalk distance.
        /// </summary>
        /// <param name="d">The d.</param>
        public static void SetMinimumOrbwalkDistance(float d)
        {
            _minDistance = d;
        }

        /// <summary>
        ///     Sets the movement delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        public static void SetMovementDelay(int delay)
        {
            _delay = delay;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fires the after attack event.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="target">The target.</param>
        private static void FireAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (AfterAttack != null && target.IsValidTarget())
            {
                AfterAttack(unit, target);
            }
        }

        /// <summary>
        ///     Fires the before attack event.
        /// </summary>
        /// <param name="target">The target.</param>
        private static void FireBeforeAttack(AttackableUnit target)
        {
            if (BeforeAttack != null)
            {
                BeforeAttack(new BeforeAttackEventArgs { Target = target });
            }
            else
            {
                DisableNextAttack = false;
            }
        }

        /// <summary>
        ///     Fires the on attack event.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="target">The target.</param>
        private static void FireOnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (OnAttack != null)
            {
                OnAttack(unit, target);
            }
        }

        /// <summary>
        ///     Fires the on non killable minion event.
        /// </summary>
        /// <param name="minion">The minion.</param>
        private static void FireOnNonKillableMinion(AttackableUnit minion)
        {
            if (OnNonKillableMinion != null)
            {
                OnNonKillableMinion(minion);
            }
        }

        /// <summary>
        ///     Fires the on target switch event.
        /// </summary>
        /// <param name="newTarget">The new target.</param>
        private static void FireOnTargetSwitch(AttackableUnit newTarget)
        {
            if (OnTargetChange != null && (!_lastTarget.IsValidTarget() || _lastTarget != newTarget))
            {
                OnTargetChange(_lastTarget, newTarget);
            }
        }

        /// <summary>
        ///     Fired when an auto attack is fired.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                var ping = Game.Ping;
                if (ping <= 30) //First world problems kappa
                {
                    Utility.DelayAction.Add(30 - ping, () => Obj_AI_Base_OnDoCast_Delayed(sender, args));
                    return;
                }

                Obj_AI_Base_OnDoCast_Delayed(sender, args);
            }
        }

        /// <summary>
        ///     Fired 30ms after an auto attack is launched.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void Obj_AI_Base_OnDoCast_Delayed(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (IsAutoAttackReset(args.SData.Name))
            {
                ResetAutoAttackTimer();
            }

            if (IsAutoAttack(args.SData.Name))
            {
                FireAfterAttack(sender, args.Target as AttackableUnit);
                _missileLaunched = true;
            }
        }

        /// <summary>
        ///     Handles the <see cref="E:ProcessSpell" /> event.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="Spell">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs Spell)
        {
            try
            {
                var spellName = Spell.SData.Name;

                if (!IsAutoAttack(spellName))
                {
                    return;
                }

                if (unit.IsMe
                    && (Spell.Target is Obj_AI_Base || Spell.Target is Obj_BarracksDampener || Spell.Target is Obj_HQ))
                {
                    LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
                    _missileLaunched = false;
                    LastMoveCommandT = 0;
                    _autoattackCounter++;

                    if (Spell.Target is Obj_AI_Base)
                    {
                        var target = (Obj_AI_Base)Spell.Target;
                        if (target.IsValid)
                        {
                            FireOnTargetSwitch(target);
                            _lastTarget = target;
                        }
                    }
                }

                FireOnAttack(unit, _lastTarget);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OnProcessSpell222(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs Spell)
        {
            try
            {
                var spellName = Spell.SData.Name;

                if (unit.IsMe && IsAutoAttackReset(spellName) && Spell.SData.SpellCastTime == 0)
                {
                    ResetAutoAttackTimer();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Fired when the spellbook stops casting a spell.
        /// </summary>
        /// <param name="spellbook">The spellbook.</param>
        /// <param name="args">The <see cref="SpellbookStopCastEventArgs" /> instance containing the event data.</param>
        private static void SpellbookOnStopCast(Obj_AI_Base spellbook, SpellbookStopCastEventArgs args)
        {
            if (spellbook.IsValid && spellbook.IsMe && args.DestroyMissile && args.StopAnimation)
            {
                ResetAutoAttackTimer();
            }
        }

        #endregion

        /// <summary>
        ///     The before attack event arguments.
        /// </summary>
        public class BeforeAttackEventArgs : EventArgs
        {
            #region Fields

            /// <summary>
            ///     The target
            /// </summary>
            public AttackableUnit Target;

            /// <summary>
            ///     The unit
            /// </summary>
            public Obj_AI_Base Unit = ObjectManager.Player;

            /// <summary>
            ///     <c>true</c> if the orbwalker should continue with the attack.
            /// </summary>
            private bool _process = true;

            #endregion

            #region Public Properties

            /// <summary>
            ///     Gets or sets a value indicating whether this <see cref="BeforeAttackEventArgs" /> should continue with the attack.
            /// </summary>
            /// <value><c>true</c> if the orbwalker should continue with the attack; otherwise, <c>false</c>.</value>
            public bool Process
            {
                get
                {
                    return this._process;
                }
                set
                {
                    DisableNextAttack = !value;
                    this._process = value;
                }
            }

            #endregion
        }

        /// <summary>
        ///     This class allows you to add an instance of "Orbwalker" to your assembly in order to control the orbwalking in an
        ///     easy way.
        /// </summary>
        public class Orbwalker : IDisposable
        {
            #region Constants

            /// <summary>
            ///     The lane clear wait time modifier.
            /// </summary>
            private const float LaneClearWaitTimeMod = 2f;

            #endregion

            #region Static Fields

            /// <summary>
            ///     The instances of the orbwalker.
            /// </summary>
            public static List<Orbwalker> Instances = new List<Orbwalker>();

            /// <summary>
            ///     The configuration
            /// </summary>
            private static Menu _config;

            #endregion

            #region Fields

            /// <summary>
            ///     The player
            /// </summary>
            private readonly AIHeroClient Player;

            /// <summary>
            ///     The forced target
            /// </summary>
            private Obj_AI_Base _forcedTarget;

            /// <summary>
            ///     The orbalker mode
            /// </summary>
            private OrbwalkingMode _mode = OrbwalkingMode.None;

            /// <summary>
            ///     The orbwalking point
            /// </summary>
            private Vector3 _orbwalkingPoint;

            /// <summary>
            ///     The previous minion the orbwalker was targeting.
            /// </summary>
            private Obj_AI_Minion _prevMinion;

            /// <summary>
            ///     The name of the CustomMode if it is set.
            /// </summary>
            private string CustomModeName;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="Orbwalker" /> class.
            /// </summary>
            /// <param name="attachToMenu">The menu the orbwalker should attach to.</param>
            public Orbwalker(Menu attachToMenu)
            {
                _config = attachToMenu;
                /* Drawings submenu */
                var drawings = new Menu("Drawings", "drawings");
                drawings.AddItem(
                    new MenuItem("AACircle", "AACircle").SetShared()
                        .SetValue(new Circle(true, Color.FromArgb(155, 255, 255, 0))));
                drawings.AddItem(
                    new MenuItem("AACircle2", "Enemy AA circle").SetShared()
                        .SetValue(new Circle(false, Color.FromArgb(155, 255, 255, 0))));
                drawings.AddItem(
                    new MenuItem("HoldZone", "HoldZone").SetShared()
                        .SetValue(new Circle(false, Color.FromArgb(155, 255, 255, 0))));
                drawings.AddItem(new MenuItem("AALineWidth", "Line Width")).SetShared().SetValue(new Slider(2, 1, 6));
                drawings.AddItem(new MenuItem("LastHitHelper", "Last Hit Helper").SetShared().SetValue(false));
                _config.AddSubMenu(drawings);

                /* Misc options */
                var misc = new Menu("Misc", "Misc");
                misc.AddItem(
                    new MenuItem("HoldPosRadius", "Hold Position Radius").SetShared().SetValue(new Slider(50, 50, 250)));
                misc.AddItem(new MenuItem("PriorizeFarm", "Priorize farm over harass").SetShared().SetValue(true));
                misc.AddItem(new MenuItem("AttackWards", "Auto attack wards").SetShared().SetValue(false));
                misc.AddItem(new MenuItem("AttackStructures", "Auto attack structures").SetShared().SetValue(false));
                misc.AddItem(new MenuItem("AttackPetsnTraps", "Auto attack pets & traps").SetShared().SetValue(true));
                misc.AddItem(new MenuItem("AttackGPBarrel", "Auto attack gangplank barrel").SetShared().SetValue(true));
                misc.AddItem(new MenuItem("prioritizeSpecialMinions", "Prioritze Special minions").SetShared().SetValue(true));
                misc.AddItem(new MenuItem("Smallminionsprio", "Jungle clear small first").SetShared().SetValue(false));
                misc.AddItem(
                    new MenuItem("LimitAttackSpeed", "Don't kite if Attack Speed > 2.5").SetShared().SetValue(false));
                misc.AddItem(
                    new MenuItem("FocusMinionsOverTurrets", "Focus minions over objectives").SetShared()
                        .SetValue(new KeyBind('M', KeyBindType.Toggle)));

                _config.AddSubMenu(misc);

                /* Missile check */
                _config.AddItem(new MenuItem("MissileCheck", "Use Missile Check").SetShared().SetValue(true));

                /* Delay sliders */
                _config.AddItem(
                    new MenuItem("ExtraWindup", "Extra windup time").SetShared().SetValue(new Slider(80, 0, 200)));
                _config.AddItem(new MenuItem("FarmDelay", "Farm delay").SetShared().SetValue(new Slider(0, 0, 200)));

                /*Load the menu*/
                _config.AddItem(
                    new MenuItem("LastHit", "Last hit").SetShared().SetValue(new KeyBind('X', KeyBindType.Press)));

                _config.AddItem(new MenuItem("Farm", "Mixed").SetShared().SetValue(new KeyBind('C', KeyBindType.Press)));

                _config.AddItem(
                    new MenuItem("Freeze", "Freeze").SetShared().SetValue(new KeyBind('N', KeyBindType.Press)));

                _config.AddItem(
                    new MenuItem("LaneClear", "LaneClear").SetShared().SetValue(new KeyBind('V', KeyBindType.Press)));

                _config.AddItem(new MenuItem("Flee", "Flee").SetShared().SetValue(new KeyBind('Z', KeyBindType.Press)));

                //_config.AddItem(new MenuItem("Burst", "Burst").SetShared().SetValue(new KeyBind('T', KeyBindType.Press)));

                _config.AddItem(
                    new MenuItem("Orbwalk", "Combo").SetShared().SetValue(new KeyBind(32, KeyBindType.Press)));

                _config.AddItem(
                    new MenuItem("StillCombo", "Combo without moving").SetShared()
                        .SetValue(new KeyBind('N', KeyBindType.Press)));
                _config.Item("StillCombo").ValueChanged +=
                    (sender, args) => { Move = !args.GetNewValue<KeyBind>().Active; };

                this.Player = ObjectManager.Player;
                Game.OnUpdate += this.GameOnOnGameUpdate;
                Drawing.OnDraw += this.DrawingOnOnDraw;
                Instances.Add(this);
            }

            #endregion

            #region Public Properties

            public static bool LimitAttackSpeed
            {
                get
                {
                    return _config.Item("LimitAttackSpeed").GetValue<bool>();
                }
            }

            /// <summary>
            ///     Gets a value indicating whether the orbwalker is orbwalking by checking the missiles.
            /// </summary>
            /// <value><c>true</c> if the orbwalker is orbwalking by checking the missiles; otherwise, <c>false</c>.</value>
            public static bool MissileCheck
            {
                get
                {
                    return _config.Item("MissileCheck").GetValue<bool>();
                }
            }

            /// <summary>
            ///     Gets or sets the active mode.
            /// </summary>
            /// <value>The active mode.</value>
            public OrbwalkingMode ActiveMode
            {
                get
                {
                    if (this._mode != OrbwalkingMode.None)
                    {
                        return this._mode;
                    }

                    if (_config.Item("Orbwalk").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Combo;
                    }

                    if (_config.Item("StillCombo").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Combo;
                    }

                    if (_config.Item("LaneClear").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.LaneClear;
                    }

                    if (_config.Item("Farm").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Mixed;
                    }

                    if (_config.Item("Freeze").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Freeze;
                    }

                    if (_config.Item("LastHit").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.LastHit;
                    }

                    if (_config.Item("Flee").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Flee;
                    }

                    //if (_config.Item("Burst").GetValue<KeyBind>().Active)
                    //{
                    //return OrbwalkingMode.Burst;
                    //}

                    if (_config.Item(this.CustomModeName) != null
                        && _config.Item(this.CustomModeName).GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.CustomMode;
                    }

                    return OrbwalkingMode.None;
                }
                set
                {
                    this._mode = value;
                }
            }

            #endregion

            #region Properties

            /// <summary>
            ///     Gets the farm delay.
            /// </summary>
            /// <value>The farm delay.</value>
            private int FarmDelay
            {
                get
                {
                    return _config.Item("FarmDelay").GetValue<Slider>().Value;
                }
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Menu.Remove(_config);
                Game.OnUpdate -= this.GameOnOnGameUpdate;
                Drawing.OnDraw -= this.DrawingOnOnDraw;
                Instances.Remove(this);
            }

            /// <summary>
            ///     Forces the orbwalker to attack the set target if valid and in range.
            /// </summary>
            /// <param name="target">The target.</param>
            public void ForceTarget(Obj_AI_Base target)
            {
                this._forcedTarget = target;
            }

            /// <summary>
            ///     Gets or sets the last minion used for lane clear.
            /// </summary>
            private Obj_AI_Base LaneClearMinion { get; set; }

            /// <summary>
            ///     Orders the enemy minions.
            /// </summary>
            /// <param name="minions">
            ///     The minions.
            /// </param>
            /// <returns>
            ///     The <see cref="List{T}" /> of <see cref="Obj_AI_Minion" />.
            /// </returns>
            private static List<Obj_AI_Minion> OrderEnemyMinions(IEnumerable<Obj_AI_Minion> minions)
            {
                return
                    minions?.OrderByDescending(minion => minion.GetMinionType().HasFlag(LeagueSharp.SDK.Enumerations.MinionTypes.Siege))
                        .ThenBy(minion => minion.GetMinionType().HasFlag(LeagueSharp.SDK.Enumerations.MinionTypes.Super))
                        .ThenBy(minion => minion.Health)
                        .ThenByDescending(minion => minion.MaxHealth)
                        .ToList();
            }

            /// <summary>
            ///     The clones
            /// </summary>
            private readonly string[] clones = { "shaco", "monkeyking", "leblanc" };

            /// <summary>
            ///     The special minions
            /// </summary>
            private readonly string[] specialMinions =
                {
                "zyrathornplant", "zyragraspingplant", "heimertyellow",
                "heimertblue", "malzaharvoidling", "yorickdecayedghoul",
                "yorickravenousghoul", "yorickspectralghoul", "shacobox",
                "annietibbers", "teemomushroom", "elisespiderling"
            };

            /// <summary>
            ///     Orders the jungle minions.
            /// </summary>
            /// <param name="minions">
            ///     The minions.
            /// </param>
            /// <returns>
            ///     The <see cref="IEnumerable{T}" /> of <see cref="Obj_AI_Minion" />.
            /// </returns>
            private IEnumerable<Obj_AI_Minion> OrderJungleMinions(List<Obj_AI_Minion> minions)
            {
                return minions != null
                           ? (_config.Item("Smallminionsprio").GetValue<bool>()
                                  ? minions.OrderBy(m => m.MaxHealth)
                                  : minions.OrderByDescending(m => m.MaxHealth)).ToList()
                           : null;
            }

            /// <summary>
            ///     Returns possible minions based on settings.
            /// </summary>
            /// <param name="mode">
            ///     The requested mode
            /// </param>
            /// <returns>
            ///     The <see cref="List{Obj_AI_Minion}" />.
            /// </returns>
            private List<Obj_AI_Minion> GetMinions(OrbwalkingMode mode)
            {
                var minions = mode != OrbwalkingMode.Combo;
                var attackWards = _config.Item("AttackWards").IsActive();
                var attackClones = _config.Item("AttackPetsnTraps").GetValue<bool>();
                var attackSpecialMinions = _config.Item("AttackPetsnTraps").GetValue<bool>();
                var prioritizeWards = _config.Item("AttackWards").IsActive();
                var prioritizeSpecialMinions = _config.Item("prioritizeSpecialMinions").GetValue<bool>();
                var minionList = new List<Obj_AI_Minion>();
                var specialList = new List<Obj_AI_Minion>();
                var cloneList = new List<Obj_AI_Minion>();
                var wardList = new List<Obj_AI_Minion>();
                foreach (var minion in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => IsValidUnit(m) && !m.IsDead && m.IsVisible && m.IsHPBarRendered && m.Distance(Player) < Player.AttackRange))
                {
                    var baseName = minion.CharData.BaseSkinName.ToLower();
                    if (minions && minion.IsMinion())
                    {
                        minionList.Add(minion);
                    }
                    else if (attackSpecialMinions && this.specialMinions.Any(s => s.Equals(baseName)))
                    {
                        specialList.Add(minion);
                    }
                    else if (attackClones && this.clones.Any(c => c.Equals(baseName)))
                    {
                        cloneList.Add(minion);
                    }
                }

                if (minions)
                {
                    minionList = OrderEnemyMinions(minionList);
                    minionList.AddRange(
                        OrderJungleMinions(
                            EntityManager.MinionsAndMonsters.Monsters.Where(
                                j => IsValidUnit(j) && !j.CharData.BaseSkinName.Equals("gangplankbarrel") && !j.IsDead && j.IsVisible && j.IsHPBarRendered && j.IsHPBarRendered && j.Distance(Player) < Player.AttackRange).ToList()));
                }

                if (attackWards)
                {
                    wardList.AddRange(GameObjects.EnemyWards.Where(w => IsValidUnit(w) && !w.IsDead && w.IsVisible && w.IsHPBarRendered && w.Distance(Player) < Player.AttackRange));
                }

                var finalMinionList = new List<Obj_AI_Minion>();
                if (attackWards && prioritizeWards && attackSpecialMinions && prioritizeSpecialMinions)
                {
                    finalMinionList.AddRange(wardList);
                    finalMinionList.AddRange(specialList);
                    finalMinionList.AddRange(minionList);
                }
                else if ((!attackWards || !prioritizeWards) && attackSpecialMinions && prioritizeSpecialMinions)
                {
                    finalMinionList.AddRange(specialList);
                    finalMinionList.AddRange(minionList);
                    finalMinionList.AddRange(wardList);
                }
                else if (attackWards && prioritizeWards)
                {
                    finalMinionList.AddRange(wardList);
                    finalMinionList.AddRange(minionList);
                    finalMinionList.AddRange(specialList);
                }
                else
                {
                    finalMinionList.AddRange(minionList);
                    finalMinionList.AddRange(specialList);
                    finalMinionList.AddRange(wardList);
                }

                if (_config.Item("AttackGPBarrel").GetValue<bool>())
                {
                    finalMinionList.AddRange(
                        GameObjects.Jungle.Where(
                            j => IsValidUnit(j) && j.Health <= 1 && !j.IsDead && j.IsVisible && j.IsHPBarRendered && j.CharData.BaseSkinName.Equals("gangplankbarrel") && j.Distance(Player) < Player.AttackRange)
                            .ToList());
                }

                if (attackClones)
                {
                    finalMinionList.AddRange(cloneList);
                }

                return finalMinionList.Where(m => !this.ignoreMinions.Any(b => b.Equals(m.CharData.BaseSkinName)) && !m.IsDead && m.IsVisible && m.IsHPBarRendered).ToList();
            }

            /// <summary>
            ///     Gets the target.
            /// </summary>
            /// <returns>AttackableUnit.</returns>
            public virtual AttackableUnit GetTarget()
            {
                var mode = ActiveMode;
                if ((mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LaneClear)
                    && !_config.Item("PriorizeFarm").GetValue<bool>())
                {
                    var target = TargetSelector.GetTarget(-1f, TargetSelector.DamageType.Physical);
                    if (target != null && InAutoAttackRange(target) && target.IsVisible && target.IsHPBarRendered && !target.IsDead)
                    {
                        return target;
                    }
                }

                var minions = new List<Obj_AI_Minion>();
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LastHit)
                {
                    minions = this.GetMinions(mode);
                }

                // Killable Minion
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LastHit)
                {
                    foreach (var minion in minions.Where(m => !m.IsDead && m.IsHPBarRendered && m.IsVisible).OrderBy(m => m.Health))
                    {
                        if (minion.IsHPBarRendered && minion.Health < Player.CalculateDamageOnUnit(minion, DamageType.Physical, (float)Player.GetAutoAttackDamage(minion, true)))
                        {
                            return minion;
                        }
                        if (minion.MaxHealth <= 10)
                        {
                            if (minion.Health <= 1)
                            {
                                return minion;
                            }
                        }
                        else
                        {
                            var predHealth = Health.GetPrediction(minion, (int)minion.GetTimeToHit(), this.FarmDelay);
                            if (predHealth <= 0)
                            {
                                FireOnNonKillableMinion(minion);
                            }

                            if (predHealth > 0 && predHealth < Player.CalculateDamageOnUnit(minion, DamageType.Physical, (float)Player.GetAutoAttackDamage(minion, true)))
                            {
                                return minion;
                            }
                        }
                    }
                }

                // Forced Target
                if (_forcedTarget.IsValidTarget() && InAutoAttackRange(_forcedTarget))
                {
                    if (_forcedTarget.IsVisible && _forcedTarget.IsHPBarRendered && !_forcedTarget.IsDead)
                        return _forcedTarget;
                }

                // Turrets | Inhibitors | Nexus
                if (mode == OrbwalkingMode.LaneClear && (!_config.Item("FocusMinionsOverTurrets").GetValue<KeyBind>().Active || !minions.Any()) && _config.Item("AttackStructures").GetValue<bool>())
                {
                    foreach (var turret in EntityManager.Turrets.Enemies.Where(t => t.IsValidTarget() && InAutoAttackRange(t) && t.Distance(Player) < Player.AttackRange))
                    {
                        return turret;
                    }

                    foreach (var inhib in
                        GameObjects.EnemyInhibitors.Where(i => i.IsValidTarget() && InAutoAttackRange(i)))
                    {
                        return inhib;
                    }

                    if (GameObjects.EnemyNexus != null && GameObjects.EnemyNexus.IsValidTarget()
                        && InAutoAttackRange(GameObjects.EnemyNexus))
                    {
                        return GameObjects.EnemyNexus;
                    }
                }

                // Champions
                if (mode != OrbwalkingMode.LastHit)
                {
                    var target = TargetSelector.GetTarget(-1f, TargetSelector.DamageType.Physical);
                    if (target.IsValidTarget() && InAutoAttackRange(target))
                    {
                        if (!target.IsDead && target.IsHPBarRendered && target.IsVisible)
                            return target;
                    }
                }

                // Jungle Minions
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed)
                {
                    var minion = minions.FirstOrDefault(m => m.Team == GameObjectTeam.Neutral);
                    if (minion != null)
                    {
                        if (minion.IsHPBarRendered && minion.IsVisible && !minion.IsDead)
                            return minion;
                    }
                }

                // Under-Turret Farming
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LastHit)
                {
                    Obj_AI_Minion farmUnderTurretMinion = null;
                    Obj_AI_Minion noneKillableMinion = null;

                    // return all the minions under turret
                    var turretMinions = minions.Where(m => m.IsMinion() && m.Position.IsUnderAllyTurret() && !m.IsDead && m.IsHPBarRendered && m.IsVisible).ToList();
                    if (turretMinions.Any())
                    {
                        // get the turret aggro minion
                        var turretMinion = turretMinions.FirstOrDefault(Health.HasTurretAggro);
                        if (turretMinion != null)
                        {
                            var hpLeftBeforeDie = 0;
                            var hpLeft = 0;
                            var turretAttackCount = 0;
                            var turret = Health.GetAggroTurret(turretMinion);
                            if (turret != null)
                            {
                                var turretStarTick = Health.TurretAggroStartTick(turretMinion);

                                // from healthprediction (blame Lizzaran)
                                var turretLandTick = turretStarTick + (int)(turret.AttackCastDelay * 1000) + (1000 * Math.Max(0, (int)(turretMinion.Distance(turret) - turret.BoundingRadius)) / (int)(turret.BasicAttack.MissileSpeed + 70));

                                // calculate the HP before try to balance it
                                for (float i = turretLandTick + 50; i < turretLandTick + (3 * turret.AttackDelay * 1000) + 50; i = i + (turret.AttackDelay * 1000))
                                {
                                    var time = (int)i - Variables.TickCount + (Game.Ping / 2);
                                    var predHp =
                                        (int)
                                        Health.GetPrediction(
                                            turretMinion,
                                            time > 0 ? time : 0,
                                            70,
                                            SDK.Enumerations.HealthPredictionType.Simulated);
                                    if (predHp > 0)
                                    {
                                        hpLeft = predHp;
                                        turretAttackCount += 1;
                                        continue;
                                    }

                                    hpLeftBeforeDie = hpLeft;
                                    hpLeft = 0;
                                    break;
                                }

                                // calculate the hits is needed and possibilty to balance
                                if (hpLeft == 0 && turretAttackCount != 0 && hpLeftBeforeDie != 0)
                                {
                                    var damage = (int)Player.GetAutoAttackDamage(turretMinion, true);
                                    var hits = hpLeftBeforeDie / damage;
                                    var timeBeforeDie = turretLandTick
                                                        + ((turretAttackCount + 1) * (int)(turret.AttackDelay * 1000))
                                                        - Variables.TickCount;
                                    var timeUntilAttackReady = LastAttackCommandT
                                                               + (int)(Player.AttackDelay * 1000)
                                                               > (Variables.TickCount + (Game.Ping / 2) + 25)
                                                                   ? LastAttackCommandT
                                                                     + (int)(Player.AttackDelay * 1000)
                                                                     - (Variables.TickCount + (Game.Ping / 2) + 25)
                                                                   : 0;
                                    var timeToLandAttack = turretMinion.GetTimeToHit();
                                    if (hits >= 1
                                        && (hits * Player.AttackDelay * 1000) + timeUntilAttackReady
                                        + timeToLandAttack < timeBeforeDie)
                                    {
                                        farmUnderTurretMinion = turretMinion;
                                    }
                                    else if (hits >= 1
                                             && (hits * Player.AttackDelay * 1000) + timeUntilAttackReady
                                             + timeToLandAttack > timeBeforeDie)
                                    {
                                        noneKillableMinion = turretMinion;
                                    }
                                }
                                else if (hpLeft == 0 && turretAttackCount == 0 && hpLeftBeforeDie == 0)
                                {
                                    noneKillableMinion = turretMinion;
                                }

                                // should wait before attacking a minion.
                                if (this.ShouldWaitUnderTurret(noneKillableMinion))
                                {
                                    return null;
                                }

                                if (farmUnderTurretMinion != null)
                                {
                                    return farmUnderTurretMinion;
                                }

                                // balance other minions
                                return
                                    (from minion in
                                         turretMinions.Where(
                                             x => x.NetworkId != turretMinion.NetworkId && !Health.HasMinionAggro(x))
                                     where
                                         (int)minion.Health % (int)turret.GetAutoAttackDamage(minion, true)
                                         > Player.CalculateDamageOnUnit(minion, DamageType.Physical, (float)Player.GetAutoAttackDamage(minion, true))
                                     select minion).FirstOrDefault();
                            }
                        }
                        else
                        {
                            if (this.ShouldWaitUnderTurret())
                            {
                                return null;
                            }

                            // balance other minions
                            return (from minion in turretMinions.Where(x => !Health.HasMinionAggro(x))
                                    let turret =
                                        GameObjects.AllyTurrets.FirstOrDefault(
                                            x => x.IsValidTarget(950f, false, minion.Position) && x.Distance(Player) < Player.AttackRange)
                                    where
                                        turret != null
                                        && (int)minion.Health % (int)turret.GetAutoAttackDamage(minion, true)
                                        > Player.CalculateDamageOnUnit(minion, DamageType.Physical, (float)Player.GetAutoAttackDamage(minion, true))
                                    select minion).FirstOrDefault();
                        }

                        return null;
                    }
                }

                // Lane Clear Minions
                if (mode == OrbwalkingMode.LaneClear)
                {
                    if (!this.ShouldWait())
                    {
                        if (this.LaneClearMinion.IsValidTarget() && InAutoAttackRange(this.LaneClearMinion) && !this.LaneClearMinion.IsDead && this.LaneClearMinion.IsHPBarRendered && this.LaneClearMinion.IsVisible)
                        {
                            if (this.LaneClearMinion.MaxHealth <= 10)
                            {
                                return this.LaneClearMinion;
                            }

                            var predHealth = Health.GetPrediction(this.LaneClearMinion, (int)(Player.AttackDelay * 1000 * LaneClearWaitTimeMod), this.FarmDelay, SDK.Enumerations.HealthPredictionType.Simulated);
                            if (predHealth >= 2 * Player.GetAutoAttackDamage(this.LaneClearMinion, true) || Math.Abs(predHealth - this.LaneClearMinion.Health) < float.Epsilon)
                            {
                                return this.LaneClearMinion;
                            }
                        }

                        foreach (var minion in minions.Where(m => m.Team != GameObjectTeam.Neutral && !m.IsDead && m.IsHPBarRendered && m.IsVisible))
                        {
                            if (minion.MaxHealth <= 10)
                            {
                                this.LaneClearMinion = minion;
                                return minion;
                            }

                            var predHealth = Health.GetPrediction(minion, (int)(Player.AttackDelay * 1000 * LaneClearWaitTimeMod), this.FarmDelay, SDK.Enumerations.HealthPredictionType.Simulated);
                            if (predHealth >= 2 * Player.GetAutoAttackDamage(minion, true)
                                || Math.Abs(predHealth - minion.Health) < float.Epsilon)
                            {
                                this.LaneClearMinion = minion;
                                return minion;
                            }
                        }
                    }
                }

                // Special Minions if no enemy is near
                if (mode == OrbwalkingMode.Combo)
                {
                    if (minions.Any() && !EntityManager.Heroes.Enemies.Any(e => e.IsValidTarget(GetRealAutoAttackRange(e) * 2f) && !e.IsDead && e.IsHPBarRendered && e.IsVisible && e.Distance(Player) < Player.AttackRange))
                    {
                        return minions.FirstOrDefault();
                    }
                }

                return null;
            }


            /// <summary>
            ///     Determines if a target is in auto attack range.
            /// </summary>
            /// <param name="target">The target.</param>
            /// <returns><c>true</c> if a target is in auto attack range, <c>false</c> otherwise.</returns>
            public virtual bool InAutoAttackRange(AttackableUnit target)
            {
                return Orbwalking.InAutoAttackRange(target);
            }

            /// <summary>
            ///     Registers the Custom Mode of the Orbwalker. Useful for adding a flee mode and such.
            /// </summary>
            /// <param name="name">The name of the mode Ex. "Myassembly.FleeMode" </param>
            /// <param name="displayname">The name of the mode in the menu. Ex. Flee</param>
            /// <param name="key">The default key for this mode.</param>
            public virtual void RegisterCustomMode(string name, string displayname, uint key)
            {
                this.CustomModeName = name;
                if (_config.Item(name) == null)
                {
                    _config.AddItem(
                        new MenuItem(name, displayname).SetShared().SetValue(new KeyBind(key, KeyBindType.Press)));
                }
            }

            /// <summary>
            ///     Enables or disables the auto-attacks.
            /// </summary>
            /// <param name="b">if set to <c>true</c> the orbwalker will attack units.</param>
            public void SetAttack(bool b)
            {
                Attack = b;
            }

            /// <summary>
            ///     Enables or disables the movement.
            /// </summary>
            /// <param name="b">if set to <c>true</c> the orbwalker will move.</param>
            public void SetMovement(bool b)
            {
                Move = b;
            }

            /// <summary>
            ///     Forces the orbwalker to move to that point while orbwalking (Game.CursorPos by default).
            /// </summary>
            /// <param name="point">The point.</param>
            public void SetOrbwalkingPoint(Vector3 point)
            {
                this._orbwalkingPoint = point;
            }

            /// <summary>
            ///     Indicates whether the depended process should wait before executing.
            /// </summary>
            /// <returns>
            ///     The <see cref="bool" />.
            /// </returns>
            public bool ShouldWait()
            {
                return this.GetEnemyMinions().Any(m => SDK.Health.GetPrediction(m, (int)(Player.AttackDelay * 1000 * LaneClearWaitTimeMod), this.FarmDelay, SDK.Enumerations.HealthPredictionType.Simulated) < Player.CalculateDamageOnUnit(m, DamageType.Physical, (float)Player.GetAutoAttackDamage(m, true)));
            }

            /// <summary>
            ///     The ignored minions
            /// </summary>
            private readonly string[] ignoreMinions = { "jarvanivstandard" };

            /// <summary>
            ///     Gets the enemy minions.
            /// </summary>
            /// <param name="range">
            ///     The range.
            /// </param>
            /// <returns>
            ///     The <see cref="List{T}" /> of <see cref="Obj_AI_Minion" />.
            /// </returns>
            public List<Obj_AI_Minion> GetEnemyMinions(float range = 0)
            {
                return EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => IsValidUnit(m, range) && !this.ignoreMinions.Any(b => b.Equals(m.CharData.BaseSkinName)) && m.Distance(Player) < Player.AttackRange).ToList();
            }

            /// <summary>
            ///     Determines whether the unit is valid.
            /// </summary>
            /// <param name="unit">
            ///     The unit.
            /// </param>
            /// <param name="range">
            ///     The range.
            /// </param>
            /// <returns>
            ///     The <see cref="bool" />.
            /// </returns>
            private static bool IsValidUnit(AttackableUnit unit, float range = 0f)
            {
                return unit.IsValidTarget(range > 0 ? range : GetRealAutoAttackRange(unit));
            }

            #endregion

            #region Methods

            /// <summary>
            ///     Fired when the game is drawn.
            /// </summary>
            /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
            private void DrawingOnOnDraw(EventArgs args)
            {
                if (_config.Item("AACircle").GetValue<Circle>().Active)
                {
                    Render.Circle.DrawCircle(
                        this.Player.Position,
                        GetRealAutoAttackRange(null) + 65,
                        _config.Item("AACircle").GetValue<Circle>().Color,
                        _config.Item("AALineWidth").GetValue<Slider>().Value);
                }
                if (_config.Item("AACircle2").GetValue<Circle>().Active)
                {
                    foreach (var target in
                        HeroManager.Enemies.FindAll(target => target.IsValidTarget(1175)))
                    {
                        Render.Circle.DrawCircle(
                            target.Position,
                            GetAttackRange(target),
                            _config.Item("AACircle2").GetValue<Circle>().Color,
                            _config.Item("AALineWidth").GetValue<Slider>().Value);
                    }
                }

                if (_config.Item("HoldZone").GetValue<Circle>().Active)
                {
                    Render.Circle.DrawCircle(
                        this.Player.Position,
                        _config.Item("HoldPosRadius").GetValue<Slider>().Value,
                        _config.Item("HoldZone").GetValue<Circle>().Color,
                        _config.Item("AALineWidth").GetValue<Slider>().Value,
                        true);
                }
                _config.Item("FocusMinionsOverTurrets")
                    .Permashow(_config.Item("FocusMinionsOverTurrets").GetValue<KeyBind>().Active);

                if (_config.Item("LastHitHelper").GetValue<bool>())
                {
                    foreach (var minion in
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                x => x.Name.ToLower().Contains("minion") && x.IsHPBarRendered && x.IsValidTarget(1000)))
                    {
                        if (minion.Health < ObjectManager.Player.GetAutoAttackDamage(minion, true))
                        {
                            Render.Circle.DrawCircle(minion.Position, 50, Color.LimeGreen);
                        }
                    }
                }
            }

            /// <summary>
            ///     Fired when the game is updated.
            /// </summary>
            /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
            private void GameOnOnGameUpdate(EventArgs args)
            {
                try
                {
                    if (this.ActiveMode == OrbwalkingMode.None)
                    {
                        return;
                    }

                    //Prevent canceling important spells
                    if (this.Player.IsCastingInterruptableSpell(true))
                    {
                        return;
                    }

                    var target = this.GetTarget();
                    Orbwalk(
                        target,
                        this._orbwalkingPoint.To2D().IsValid() ? this._orbwalkingPoint : Game.CursorPos,
                        _config.Item("ExtraWindup").GetValue<Slider>().Value,
                        Math.Max(_config.Item("HoldPosRadius").GetValue<Slider>().Value, 30));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            /// <summary>
            ///     Returns if a minion should be attacked
            /// </summary>
            /// <param name="minion">The <see cref="Obj_AI_Minion" /></param>
            /// <param name="includeBarrel">Include Gangplank Barrel</param>
            /// <returns><c>true</c> if the minion should be attacked; otherwise, <c>false</c>.</returns>
            private bool ShouldAttackMinion(Obj_AI_Minion minion)
            {
                if (minion.Name == "WardCorpse" || minion.CharData.BaseSkinName == "jarvanivstandard")
                {
                    return false;
                }

                if (MinionManager.IsWard(minion))
                {
                    return _config.Item("AttackWards").IsActive();
                }

                return (_config.Item("AttackPetsnTraps").GetValue<bool>() || MinionManager.IsMinion(minion))
                       && minion.CharData.BaseSkinName != "gangplankbarrel";
            }

            /// <summary>
            ///     Determines if the orbwalker should wait before attacking a minion under turret.
            /// </summary>
            /// <param name="noneKillableMinion">
            ///     The non killable minion.
            /// </param>
            /// <returns>
            ///     The <see cref="bool" />.
            /// </returns>
            public bool ShouldWaitUnderTurret(Obj_AI_Minion noneKillableMinion = null)
            {
                return
                    this.GetEnemyMinions()
                        .Any(
                            m =>
                            (noneKillableMinion == null || noneKillableMinion.NetworkId != m.NetworkId) && m.IsValidTarget()
                            && InAutoAttackRange(m)
                            && SDK.Health.GetPrediction(
                                m,
                                (int)((Player.AttackDelay * 1000) + m.GetTimeToHit()),
                                this.FarmDelay,
                                SDK.Enumerations.HealthPredictionType.Simulated) < Player.CalculateDamageOnUnit(m, DamageType.Physical, (float)Player.GetAutoAttackDamage(m, true)));
            }

            #endregion
        }
    }
}