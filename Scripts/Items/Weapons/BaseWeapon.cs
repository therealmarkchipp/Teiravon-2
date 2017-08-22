using System;
using System.Text;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Necromancy;
using Server.Factions;
using Server.Engines.Craft;
using Server.Items;
using Server.Regions;
using Server.Teiravon;
using Server.Engines.XmlSpawner2;

namespace Server.Items
{
    public abstract class BaseWeapon : Item, IWeapon, IFactionItem, ICraftable
    {
        #region Factions
        private FactionItem m_FactionState;

        public FactionItem FactionItemState
        {
            get { return m_FactionState; }
            set
            {
                m_FactionState = value;

                if (m_FactionState == null)
                    Hue = CraftResources.GetHue(Resource);

                LootType = (m_FactionState == null ? LootType.Regular : LootType.Blessed);
            }
        }
        #endregion



        /* Weapon internals work differently now (Mar 13 2003)
		 *
		 * The attributes defined below default to -1.
		 * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
		 * If not, the attribute value itself is used. Here's the list:
		 *  - MinDamage
		 *  - MaxDamage
		 *  - Speed
		 *  - HitSound
		 *  - MissSound
		 *  - StrRequirement, DexRequirement, IntRequirement
		 *  - WeaponType
		 *  - WeaponAnimation
		 *  - MaxRange
		 */

        // Instance values. These values must are unique to each weapon.
        private WeaponDamageLevel m_DamageLevel;
        private WeaponAccuracyLevel m_AccuracyLevel;
        private WeaponDurabilityLevel m_DurabilityLevel;
        private WeaponQuality m_Quality;
        private Mobile m_Crafter;
        private Poison m_Poison;
        private int m_PoisonCharges;
        private bool m_Identified;
        private int m_Hits;
        private int m_MaxHits;
        private SlayerName m_Slayer;
        private SkillMod m_SkillMod, m_MageMod;
        private CraftResource m_Resource;
        private bool m_PlayerConstructed;

        private bool m_Cursed; // Is this weapon cursed via Curse Weapon necromancer spell? Temporary; not serialized.
        private bool m_Consecrated; // Is this weapon blessed via Consecrate Weapon paladin ability? Temporary; not serialized.

        private AosAttributes m_AosAttributes;
        private AosWeaponAttributes m_AosWeaponAttributes;
        private AosSkillBonuses m_AosSkillBonuses;

        // Overridable values. These values are provided to override the defaults which get defined in the individual weapon scripts.
        private int m_StrReq, m_DexReq, m_IntReq;
        private int m_MinDamage, m_MaxDamage;
        private int m_HitSound, m_MissSound;
        private int m_Speed;
        private int m_MaxRange;
        private SkillName m_Skill;
        private WeaponType m_Type;
        private WeaponAnimation m_Animation;

        public virtual WeaponAbility PrimaryAbility { get { return null; } }
        public virtual WeaponAbility SecondaryAbility { get { return null; } }

        public virtual int DefMaxRange { get { return 1; } }
        public virtual int DefHitSound { get { return 0; } }
        public virtual int DefMissSound { get { return 0; } }
        public virtual SkillName DefSkill { get { return SkillName.Swords; } }
        public virtual WeaponType DefType { get { return WeaponType.Slashing; } }
        public virtual WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash1H; } }

        public virtual int AosStrengthReq { get { return 0; } }
        public virtual int AosDexterityReq { get { return 0; } }
        public virtual int AosIntelligenceReq { get { return 0; } }
        public virtual int AosMinDamage { get { return 0; } }
        public virtual int AosMaxDamage { get { return 0; } }
        public virtual int AosSpeed { get { return 0; } }
        public virtual int AosMaxRange { get { return DefMaxRange; } }
        public virtual int AosHitSound { get { return DefHitSound; } }
        public virtual int AosMissSound { get { return DefMissSound; } }
        public virtual SkillName AosSkill { get { return DefSkill; } }
        public virtual WeaponType AosType { get { return DefType; } }
        public virtual WeaponAnimation AosAnimation { get { return DefAnimation; } }

        public virtual int OldStrengthReq { get { return 0; } }
        public virtual int OldDexterityReq { get { return 0; } }
        public virtual int OldIntelligenceReq { get { return 0; } }
        public virtual int OldMinDamage { get { return 0; } }
        public virtual int OldMaxDamage { get { return 0; } }
        public virtual int OldSpeed { get { return 0; } }
        public virtual int OldMaxRange { get { return DefMaxRange; } }
        public virtual int OldHitSound { get { return DefHitSound; } }
        public virtual int OldMissSound { get { return DefMissSound; } }
        public virtual SkillName OldSkill { get { return DefSkill; } }
        public virtual WeaponType OldType { get { return DefType; } }
        public virtual WeaponAnimation OldAnimation { get { return DefAnimation; } }

        public virtual int InitMinHits { get { return 0; } }
        public virtual int InitMaxHits { get { return 0; } }

        public override int PhysicalResistance { get { return m_AosWeaponAttributes.ResistPhysicalBonus; } }
        public override int FireResistance { get { return m_AosWeaponAttributes.ResistFireBonus; } }
        public override int ColdResistance { get { return m_AosWeaponAttributes.ResistColdBonus; } }
        public override int PoisonResistance { get { return m_AosWeaponAttributes.ResistPoisonBonus; } }
        public override int EnergyResistance { get { return m_AosWeaponAttributes.ResistEnergyBonus; } }

        private SkillMod m_Skilmod;
        private SkillMod m_Skilmod2;

        [CommandProperty(AccessLevel.GameMaster)]
        public AosAttributes Attributes
        {
            get { return m_AosAttributes; }
            set { m_AosAttributes = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosWeaponAttributes WeaponAttributes
        {
            get { return m_AosWeaponAttributes; }
            set { m_AosWeaponAttributes = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SkillBonuses
        {
            get { return m_AosSkillBonuses; }
            set { m_AosSkillBonuses = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Cursed
        {
            get { return m_Cursed; }
            set { m_Cursed = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Consecrated
        {
            get { return m_Consecrated; }
            set { m_Consecrated = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Identified
        {
            get { return m_Identified; }
            set { m_Identified = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Hits
        {
            get { return m_Hits; }
            set
            {
                if (m_Hits == value)
                    return;

                if (value > m_MaxHits)
                    value = m_MaxHits;

                m_Hits = value;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxHits
        {
            get { return m_MaxHits; }
            set { m_MaxHits = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonCharges
        {
            get { return m_PoisonCharges; }
            set { m_PoisonCharges = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Poison Poison
        {
            get { return m_Poison; }
            set { m_Poison = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponQuality Quality
        {
            get { return m_Quality; }
            set { UnscaleDurability(); m_Quality = value; ScaleDurability(); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter
        {
            get { return m_Crafter; }
            set { m_Crafter = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SlayerName Slayer
        {
            get { return m_Slayer; }
            set { m_Slayer = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set
            {
                UnscaleDurability();
                m_Resource = value;
                Hue = CraftResources.GetHue(m_Resource);
                InvalidateProperties();
                ScaleDurability();

                // Teiravon wood mods
                CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

                if ((int)Resource >= 301)
                {
                    Attributes.WeaponDamage = 0;

                    if (Quality == WeaponQuality.Exceptional)
                        Attributes.WeaponDamage += 20;
                }

                if (info != null && m_Resource != CraftResource.None)
                {
                    CraftAttributeInfo attrInfo = info.AttributeInfo;

                    if (attrInfo != null)
                    {
                        Attributes.WeaponDamage += attrInfo.WeaponDamageIncrease;

                        if (attrInfo.WeaponSwingSpeed > 0)


                            if (attrInfo.WeaponMinStr > 0)
                                StrRequirement = attrInfo.WeaponMinStr;
                            else
                                StrRequirement = AosStrengthReq;

                        if (this is BaseStaff)
                            Attributes.WeaponDamage -= (int)(0.5 * attrInfo.WeaponDamageIncrease);

                        InvalidateProperties();
                    }
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponDamageLevel DamageLevel
        {
            get { return m_DamageLevel; }
            set { m_DamageLevel = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponDurabilityLevel DurabilityLevel
        {
            get { return m_DurabilityLevel; }
            set { UnscaleDurability(); m_DurabilityLevel = value; InvalidateProperties(); ScaleDurability(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PlayerConstructed
        {
            get { return m_PlayerConstructed; }
            set { m_PlayerConstructed = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxRange
        {
            get { return (m_MaxRange == -1 ? Core.AOS ? AosMaxRange : OldMaxRange : m_MaxRange); }
            set { m_MaxRange = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponAnimation Animation
        {
            get { return (m_Animation == (WeaponAnimation)(-1) ? Core.AOS ? AosAnimation : OldAnimation : m_Animation); }
            set { m_Animation = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponType Type
        {
            get { return (m_Type == (WeaponType)(-1) ? Core.AOS ? AosType : OldType : m_Type); }
            set { m_Type = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill
        {
            get { return (m_Skill == (SkillName)(-1) ? Core.AOS ? AosSkill : OldSkill : m_Skill); }
            set { m_Skill = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitSound
        {
            get { return (m_HitSound == -1 ? Core.AOS ? AosHitSound : OldHitSound : m_HitSound); }
            set { m_HitSound = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MissSound
        {
            get { return (m_MissSound == -1 ? Core.AOS ? AosMissSound : OldMissSound : m_MissSound); }
            set { m_MissSound = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinDamage
        {
            get { return (m_MinDamage == -1 ? Core.AOS ? AosMinDamage : OldMinDamage : m_MinDamage); }
            set { m_MinDamage = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxDamage
        {
            get { return (m_MaxDamage == -1 ? Core.AOS ? AosMaxDamage : OldMaxDamage : m_MaxDamage); }
            set { m_MaxDamage = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Speed
        {
            get { return (m_Speed == -1 ? Core.AOS ? AosSpeed : OldSpeed : m_Speed); }
            set { m_Speed = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StrRequirement
        {
            get { return (m_StrReq == -1 ? Core.AOS ? AosStrengthReq : OldStrengthReq : m_StrReq); }
            set { m_StrReq = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DexRequirement
        {
            get { return (m_DexReq == -1 ? Core.AOS ? AosDexterityReq : OldDexterityReq : m_DexReq); }
            set { m_DexReq = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int IntRequirement
        {
            get { return (m_IntReq == -1 ? Core.AOS ? AosIntelligenceReq : OldIntelligenceReq : m_IntReq); }
            set { m_IntReq = value; }
        }

        public virtual SkillName AccuracySkill { get { return SkillName.Tactics; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponAccuracyLevel AccuracyLevel
        {
            get
            {
                return m_AccuracyLevel;
            }
            set
            {
                if (m_AccuracyLevel != value)
                {
                    m_AccuracyLevel = value;

                    if (UseSkillMod)
                    {
                        if (m_AccuracyLevel == WeaponAccuracyLevel.Regular)
                        {
                            if (m_SkillMod != null)
                                m_SkillMod.Remove();

                            m_SkillMod = null;
                        }
                        else if (m_SkillMod == null && Parent is Mobile)
                        {
                            m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                            ((Mobile)Parent).AddSkillMod(m_SkillMod);
                        }
                        else if (m_SkillMod != null)
                        {
                            m_SkillMod.Value = (int)m_AccuracyLevel * 5;
                        }
                    }

                    InvalidateProperties();
                }
            }
        }

        public void UnscaleDurability()
        {
            int scale = 100 + GetDurabilityBonus();

            m_Hits = ((m_Hits * 100) + (scale - 1)) / scale;
            m_MaxHits = ((m_MaxHits * 100) + (scale - 1)) / scale;
            InvalidateProperties();
        }

        public void ScaleDurability()
        {
            int scale = 100 + GetDurabilityBonus();

            m_Hits = ((m_Hits * scale) + 99) / 100;
            m_MaxHits = ((m_MaxHits * scale) + 99) / 100;
            InvalidateProperties();
        }

        public int GetDurabilityBonus()
        {
            int bonus = 0;

            if (m_Quality == WeaponQuality.Exceptional)
                bonus += 20;

            switch (m_DurabilityLevel)
            {
                case WeaponDurabilityLevel.Durable: bonus += 20; break;
                case WeaponDurabilityLevel.Substantial: bonus += 50; break;
                case WeaponDurabilityLevel.Massive: bonus += 70; break;
                case WeaponDurabilityLevel.Fortified: bonus += 100; break;
                case WeaponDurabilityLevel.Indestructible: bonus += 120; break;
            }

            if (Core.AOS)
            {
                bonus += m_AosWeaponAttributes.DurabilityBonus;

                CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);
                CraftAttributeInfo attrInfo = null;

                if (resInfo != null)
                    attrInfo = resInfo.AttributeInfo;

                if (attrInfo != null)
                    bonus += attrInfo.WeaponDurability;
            }

            return bonus;
        }

        public int GetLowerStatReq()
        {
            if (!Core.AOS)
                return 0;

            int v = m_AosWeaponAttributes.LowerStatReq;

            CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

            if (info != null)
            {
                CraftAttributeInfo attrInfo = info.AttributeInfo;

                if (attrInfo != null)
                    v += attrInfo.WeaponLowerRequirements;
            }

            if (v > 100)
                v = 100;

            return v;
        }

        public static void BlockEquip(Mobile m, TimeSpan duration)
        {
            if (m.BeginAction(typeof(BaseWeapon)))
                new ResetEquipTimer(m, duration).Start();
        }

        private class ResetEquipTimer : Timer
        {
            private Mobile m_Mobile;

            public ResetEquipTimer(Mobile m, TimeSpan duration)
                : base(duration)
            {
                m_Mobile = m;
            }

            protected override void OnTick()
            {
                m_Mobile.EndAction(typeof(BaseWeapon));
            }
        }

        public override bool CheckConflictingLayer(Mobile m, Item item, Layer layer)
        {
            if (base.CheckConflictingLayer(m, item, layer))
                return true;

            if (this.Layer == Layer.TwoHanded && layer == Layer.OneHanded)
                return true;
            else if (this.Layer == Layer.OneHanded && layer == Layer.TwoHanded && !(item is BaseShield) && !(item is BaseEquipableLight))
                return true;

            return false;
        }

        public override bool CanEquip(Mobile from)
        {
            if (from.Dex < DexRequirement)
            {
                from.SendMessage("You are not nimble enough to equip that.");
                return false;
            }
            else if (from.Str < AOS.Scale(StrRequirement, 100 - GetLowerStatReq()))
            {
                from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
                return false;
            }
            else if (from.Int < IntRequirement)
            {
                from.SendMessage("You are not smart enough to equip that.");
                return false;
            }
            else if (!from.CanBeginAction(typeof(BaseWeapon)))
            {
                return false;
            }
            else
            {
                return base.CanEquip(from);
            }
        }

        public virtual bool UseSkillMod { get { return !Core.AOS; } }

        public override bool OnEquip(Mobile from)
        {
            int strBonus = m_AosAttributes.BonusStr;
            int dexBonus = m_AosAttributes.BonusDex;
            int intBonus = m_AosAttributes.BonusInt;

            if ((strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = from;

                string modName = this.Serial.ToString();

                if (strBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            from.NextCombatTime = DateTime.Now + GetDelay(from);

            if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular)
            {
                if (m_SkillMod != null)
                    m_SkillMod.Remove();

                m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                from.AddSkillMod(m_SkillMod);
            }

            if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30)
            {
                if (m_MageMod != null)
                    m_MageMod.Remove();

                m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
                from.AddSkillMod(m_MageMod);
            }

            BandageContext c = BandageContext.GetContext(from);
            if (c != null)
            {
                c.StopHeal();
                from.SendMessage("You stop bandaging the wound to equip your weapon.");
            }
            // XmlAttachment check for OnEquip
            Server.Engines.XmlSpawner2.XmlAttach.CheckOnEquip(this, from);

            return true;
        }

        public override void OnAdded(object parent)
        {
            base.OnAdded(parent);

            if (parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                if (Core.AOS)
                    m_AosSkillBonuses.AddTo(from);
                from.CheckStatTimers();
                from.Delta(MobileDelta.WeaponDamage);
            }

            if (parent is TeiravonMobile)
            {
                TeiravonMobile m_parent = (TeiravonMobile)parent;

                if ((m_parent.IsKensai()) && (this is Katana))
                {
                    if (m_Skilmod != null)
                        m_Skilmod.Remove();

                    int kensaikatbonus = m_parent.PlayerLevel - 5;

                    if (kensaikatbonus > 0)
                    {
                        m_Skilmod = new DefaultSkillMod(SkillName.Swords, true, kensaikatbonus);
                        ((Mobile)parent).AddSkillMod(m_Skilmod);
                    }
                }
                bool ElfBuff = false;
                
                if ((m_parent.IsElf()) && (this is Longsword || this is ElvenLongsword))
                {

                    if (m_Skilmod != null)
                        m_Skilmod.Remove();
                    double total = 10.0;
                    if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Slashing)
                        total += m_parent.PlayerLevel - 10;
                    m_Skilmod = new DefaultSkillMod(SkillName.Swords, true, total);
                    ((Mobile)parent).AddSkillMod(m_Skilmod);

                    m_parent.SendMessage(0x9F2, "The weapon hums in your hand...");
                    ElfBuff = true;
                }


                if (m_parent.HasFeat(TeiravonMobile.Feats.WeaponMaster))
                {
                    SkillName wpnskill = SkillName.Alchemy;
                    int skilladj = m_parent.PlayerLevel;

                    if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Axe && this.DefType == WeaponType.Axe )
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Slashing && this.DefType == WeaponType.Slashing)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Staff && this.DefType == WeaponType.Staff)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Bashing && this.DefType == WeaponType.Bashing)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Piercing && this.DefType == WeaponType.Piercing)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Polearm && this.DefType == WeaponType.Polearm)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Ranged && this.DefType == WeaponType.Ranged)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Thrown && this.DefType == WeaponType.Thrown)
                        wpnskill = this.Skill;

                    if (wpnskill != SkillName.Alchemy)
                    {
                        if (!ElfBuff)
                        {
                            m_Skilmod = new DefaultSkillMod(wpnskill, true, skilladj);
                            ((Mobile)parent).AddSkillMod(m_Skilmod);
                        }
                        m_Skilmod2 = new DefaultSkillMod(SkillName.Tactics, true, skilladj);
                        ((Mobile)parent).AddSkillMod(m_Skilmod2);
                    }

                }
            }
        }

        public override void OnRemoved(object parent)
        {
            if (m_Skilmod != null)
                m_Skilmod.Remove();

            m_Skilmod = null;

            if (m_Skilmod2 != null)
                m_Skilmod2.Remove();

            m_Skilmod2 = null;


            if (parent is Mobile)
            {
                Mobile m = (Mobile)parent;
                BaseWeapon weapon = m.Weapon as BaseWeapon;

                string modName = this.Serial.ToString();

                m.RemoveStatMod(modName + "Str");
                m.RemoveStatMod(modName + "Dex");
                m.RemoveStatMod(modName + "Int");

                if (weapon != null)
                    m.NextCombatTime = DateTime.Now + weapon.GetDelay(m);

                if (UseSkillMod && m_SkillMod != null)
                {
                    m_SkillMod.Remove();
                    m_SkillMod = null;
                }

                if (m_MageMod != null)
                {
                    m_MageMod.Remove();
                    m_MageMod = null;
                }

                if (Core.AOS)
                    m_AosSkillBonuses.Remove();

                m.CheckStatTimers();

                m.Delta(MobileDelta.WeaponDamage);
            }
            // XmlAttachment check for OnRemoved
            Server.Engines.XmlSpawner2.XmlAttach.CheckOnRemoved(this, parent);
        }

        public virtual SkillName GetUsedSkill(Mobile m, bool checkSkillAttrs)
        {
            SkillName sk;

            if (checkSkillAttrs && m_AosWeaponAttributes.UseBestSkill != 0)
            {
                double swrd = m.Skills[SkillName.Swords].Value;
                double fenc = m.Skills[SkillName.Fencing].Value;
                double mcng = m.Skills[SkillName.Macing].Value;
                double val;

                sk = SkillName.Swords;
                val = swrd;

                if (fenc > val) { sk = SkillName.Fencing; val = fenc; }
                if (mcng > val) { sk = SkillName.Macing; val = mcng; }
            }
            else if (m_AosWeaponAttributes.MageWeapon != 0)
            {
                if (m.Skills[SkillName.Magery].Value > m.Skills[Skill].Value)
                    sk = SkillName.Magery;
                else
                    sk = Skill;
            }
            else
            {
                sk = Skill;

                if (sk != SkillName.Wrestling && !m.Player && !m.Body.IsHuman && m.Skills[SkillName.Wrestling].Value > m.Skills[sk].Value)
                    sk = SkillName.Wrestling;
            }

            return sk;
        }

        public virtual double GetAttackSkillValue(Mobile attacker, Mobile defender)
        {
            return attacker.Skills[GetUsedSkill(attacker, true)].Value;
        }

        public virtual double GetDefendSkillValue(Mobile attacker, Mobile defender)
        {
            return defender.Skills[GetUsedSkill(defender, true)].Value;
        }
        public virtual bool CheckDodge(Mobile attacker, Mobile defender)
        {
            Item check = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
            BaseShield shield = null;
            if (check is BaseShield)
                shield = check as BaseShield;

            if (shield == null && defender is TeiravonMobile)
            {
                //defender.SendMessage("Has dodge");
                double chance = (defender.Skills.Ninjitsu.Value / 100.0) * .5;
                double stamRatio = 1;
                stamRatio = (double)((double)defender.Stam / (double)defender.StamMax);
                int val = 0;
                if (stamRatio > .5)
                {
                    XmlValue counter = (XmlValue)XmlAttach.FindAttachmentOnMobile(defender, typeof(XmlValue), "dodge");
                    if (counter != null)
                        val = counter.Value;


                    chance -= val * .3;
                    chance = chance * stamRatio;
                    if (defender.CheckSkill(SkillName.Ninjitsu, chance))
                    {
                        defender.Stam -= val;
                        counter = new XmlValue("dodge", val + 2, 0.15);
                        XmlAttach.AttachTo(defender, counter);
                        defender.PlaySound(0x51B);
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool CheckHit(Mobile attacker, Mobile defender)
        {
            bool blinded = XmlAttach.FindAttachmentOnMobile(attacker, typeof(TavBlind), "blind") != null;

            if (blinded)
                return false;

            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

            Skill atkSkill = attacker.Skills[atkWeapon.Skill];
            Skill defSkill = defender.Skills[defWeapon.Skill];

            double atkValue = atkWeapon.GetAttackSkillValue(attacker, defender);
            double defValue = defWeapon.GetDefendSkillValue(attacker, defender);

            // The chance to hit with ranged is not comparative, it's handled by the aim calculation.
            if (atkWeapon is BaseRanged)
                return true;

            //attacker.CheckSkill( atkSkill.SkillName, defValue - 20.0, 120.0 );
            //defender.CheckSkill( defSkill.SkillName, atkValue - 20.0, 120.0 );

            double ourValue, theirValue;
            int ourDex, theirDex;

            int bonus = GetHitChanceBonus();

            if (Core.AOS)
            {
                if (atkValue <= -20.0)
                    atkValue = -19.9;

                if (defValue <= -20.0)
                    defValue = -19.9;

                // Hit Chance Increase = 45%
                int atkChance = AosAttributes.GetValue(attacker, AosAttribute.AttackChance);
                if (atkChance > 45)
                    atkChance = 45;

                bonus += atkChance;

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
                    bonus += 10; // attacker gets 10% bonus when they're under divine fury

                if (atkWeapon is BaseRanged)
                    bonus += 15; // increase accuracy of bows 15%

                if (HitLower.IsUnderAttackEffect(attacker))
                    bonus -= 25; // Under Hit Lower Attack effect -> 25% malus

                ourValue = (atkValue + 20.0) * (100 + bonus);

                ArrayList rotten = XmlAttach.FindAttachments(attacker, typeof(TavRotten));
                if (rotten != null && rotten.Count > 0)
                {
                    for (int i = 0; i < rotten.Count; i++)
                    {
                        ourValue *= ((TavRotten)rotten[i]).Value;
                    }
                }
                //attacker.SendMessage("old value: {0} , new value: {1}", oldvalue, ourValue);
                

                // Defense Chance Increase = 45%
                bonus = AosAttributes.GetValue(defender, AosAttribute.DefendChance);
                if (bonus > 45)
                    bonus = 45;

                if (defender is TeiravonMobile && ((TeiravonMobile)defender).GetActiveFeats(TeiravonMobile.Feats.DefensiveStance))
                    bonus += 2 * ((TeiravonMobile)defender).PlayerLevel;

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(defender))
                    bonus -= 20; // defender loses 20% bonus when they're under divine fury

                if (HitLower.IsUnderDefenseEffect(defender))
                    bonus -= 25; // Under Hit Lower Defense effect -> 25% malus

                double discordanceScalar = 0.0;

                if (SkillHandlers.Discordance.GetScalar(attacker, ref discordanceScalar))
                    bonus += (int)(discordanceScalar * 100);

                theirValue = (defValue + 20.0) * (100 + bonus);

                TavFeint feinted = (TavFeint)XmlAttach.FindAttachmentOnMobile(defender, typeof(TavFeint), defender.Name);
                if (feinted != null)
                    theirValue *= .4;
                bonus = 0;

            }
            else
            {
                if (atkValue <= -50.0)
                    atkValue = -49.9;

                if (defValue <= -50.0)
                    defValue = -49.9;

                ourValue = (atkValue + 50.0);
                theirValue = (defValue + (IsBehind(attacker, defender) ? 0 : 50.0));
            }

            ourDex = attacker.Dex;
            theirDex = defender.Dex;

            double dexDif = 1.0;
            double chance = ourValue / (theirValue * 2.0);

            chance *= 1.0 + ((double)bonus / 100);

            if (theirDex > ourDex)
                dexDif = 1.0 - ((double)((theirDex - ourDex) * .5) / (double)theirDex);

            if (ourDex > theirDex)
                dexDif = 1.0 + ((1 - (double)theirDex / ourDex) * .5);

            chance *= dexDif;

            chance *= 1.0 + ((double)bonus / 100);

            if (Core.AOS && chance < 0.02)
                chance = 0.02;

            WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

            if (ability != null)
                chance *= ability.AccuracyScalar;

            bool success = attacker.CheckSkill(atkSkill.SkillName, chance);

            if (!success && attacker is TeiravonMobile && ((TeiravonMobile)attacker).HasFeat(TeiravonMobile.Feats.RottenLuck))
            {
                XmlAttach.AttachTo(defender, new TavRotten("rottenluck", chance * .5, 4.0));
            }
            return success;

            //return ( chance >= Utility.RandomDouble() );
        }

        public virtual TimeSpan GetDelay(Mobile m)
        {
            TimeSpan delay = TimeSpan.FromSeconds(0.0);

            return GetDelay(m, delay);
        }

        public virtual TimeSpan GetDelay(Mobile m, TimeSpan delay)
        {
            int speed = this.Speed;

            if (speed == 0)
                return TimeSpan.FromMinutes(5.0);

            double delayInSeconds;

            if (Core.AOS)
            {
                //STARTMOD: TEIRAVON Swing Speed

                int dexbonus = m.Dex;

                if (dexbonus > 150)
                    dexbonus = 150 + (int)((dexbonus - 150) * 2);

                int v = ((int)(m.Stam / (double)(2 * ( 1 + (m.Stam * .0075))) ) + 100 + (int)(dexbonus / 1.25)) * speed;

                if (m is TeiravonMobile)
                {
                    TeiravonMobile tav = m as TeiravonMobile;
                    if (tav.HasFeat(TeiravonMobile.Feats.BerserkerRage))
                    {
                        double HITSfact = (double)tav.Hits / (double)tav.HitsMax;
                        if (HITSfact <= .75)
                        {
                            int RAGEfact = 120 - (int)(HITSfact * 100);
                            AOS.Scale(RAGEfact, 100 + tav.PlayerLevel * 2);
                            v += RAGEfact;
                            v += AOS.Scale(v, RAGEfact);
                        }
                    }


                    if ((double)(((double)m.Stam / (double)((TeiravonMobile)m).MaxStam)) < 0.05)
                    {
                        v -= 100 * speed;

                        if (Utility.Random(3) == 0)
                            m.SendMessage("You feel fatigued.");
                    }
                }

                //ENDMOD: TEIRAVON Swing SPeed
                //Original formula
                //int v = (m.Stam + 100) * speed;

                int bonus = AosAttributes.GetValue(m, AosAttribute.WeaponSpeed);

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(m))
                    bonus += 10;

                if (Spells.Third.BlessSpell.gruumsh.Contains(m))
                    bonus += 30;

                TransformContext context = TransformationSpell.GetContext(m);

                double discordanceScalar = 0.0;

                if (SkillHandlers.Discordance.GetScalar(m, ref discordanceScalar))
                    bonus += (int)(discordanceScalar * 100);

                v += AOS.Scale(v, bonus);

                if (TransformationSpell.UnderTransformation(m, typeof(HorrificBeastSpell)))
                    AOS.Scale(v, 150);

                if (TransformationSpell.UnderTransformation(m, typeof(VampiricEmbraceSpell)))
                    AOS.Scale(v, 80);

                if (v <= 0)
                    v = 1;

                delayInSeconds = Math.Round(40000.0 / v, 2) * 0.5;

                // Maximum swing rate capped at one swing per .85 seconds.
                if (delayInSeconds < 0.85)
                    delayInSeconds = 0.85;
            }
            else
            {
                int v = (m.Stam + 100) * speed;

                if (v <= 0)
                    v = 1;

                delayInSeconds = 15000.0 / v;
            }
            delayInSeconds += delay.TotalSeconds;

            return TimeSpan.FromSeconds(delayInSeconds);
        }

        public virtual TimeSpan OnSwing(Mobile attacker, Mobile defender)
        {
            bool canSwing = true;

            if (attacker is TeiravonMobile)
            {
                TeiravonMobile m_Person = (TeiravonMobile)attacker;

                if (m_Person.Mounted)
                {
                    int fallchance = 0;

                    if (m_Person.RidingSkill == 0)
                        fallchance = 20;
                    else if (m_Person.RidingSkill == 1)
                        fallchance = 10;
                    else if (m_Person.RidingSkill == 2)
                        fallchance = 5;

                    BaseMount mnt = m_Person.Mount as BaseMount;
                    TeiravonRegion r = m_Person.Region as TeiravonRegion;

                    if (r != null)
                    {
                        if (mnt != null && (mnt is Warg && (r.Dungeon && r.Name != "Shadowrealm") || mnt is Tizzin && !r.Dungeon))
                            fallchance = 100;
                    }

                    if (fallchance > Utility.RandomMinMax(1, 100))
                    {
                        m_Person.OnDoubleClick(m_Person);
                        m_Person.Damage(Utility.RandomMinMax(1, 10), m_Person);
                        m_Person.SendMessage("You fall off your mount while swinging");
                    }
                }
            }

            if (Core.AOS)
            {
                canSwing = (!attacker.Paralyzed && !attacker.Frozen);

                if (canSwing)
                {
                    Spell sp = attacker.Spell as Spell;

                    canSwing = (sp == null || !sp.IsCasting || !sp.BlocksMovement);
                }
            }

            if (canSwing && attacker.HarmfulCheck(defender))
            {
                attacker.DisruptiveAction();

                if (attacker.NetState != null)
                    attacker.Send(new Swing(0, attacker, defender));

                if (attacker is BaseCreature)
                {
                    BaseCreature bc = (BaseCreature)attacker;
                    WeaponAbility ab = bc.GetWeaponAbility();

                    if (ab != null)
                    {
                        if (bc.WeaponAbilityChance > Utility.RandomDouble())
                            WeaponAbility.SetCurrentAbility(bc, ab);
                        else
                            WeaponAbility.ClearCurrentAbility(bc);
                    }
                }
                if (attacker is TeiravonMobile && ((TeiravonMobile)attacker).CripplingShotReady)
                {
                    attacker.SendMessage("You deftly aim your strike to draw the target's defence away from your next strike.");
                    XmlAttach.AttachTo(defender, new TavFeint(attacker.Name, 3.0));
                    OnMiss(attacker, defender);
                    ((TeiravonMobile)attacker).CripplingShotReady = false;
                }
                else if (attacker.GetDistanceToSqrt(defender) >= 2)
                {
                    if (BaseRanged.CheckAim(attacker, defender))
                    {
                        if (CheckHit(attacker, defender) && !CheckDodge(attacker, defender))
                            OnHit(attacker, defender);
                        else
                            OnMiss(attacker, defender);
                    }
                    else
                        OnMiss(attacker, defender);
                }
                else if (CheckHit(attacker, defender) && !CheckDodge(attacker, defender))
                    OnHit(attacker, defender);
                else
                    OnMiss(attacker, defender);
            }
            if (defender is TeiravonMobile)
            {
                TeiravonMobile tavDef = defender as TeiravonMobile;
                if (tavDef.HasFeat(TeiravonMobile.Feats.ChilblainTouch))
                    return GetDelay(attacker, TimeSpan.FromSeconds(.3));
                if (tavDef.HasFeat(TeiravonMobile.Feats.Riposte))
                {
                    if (Utility.RandomDouble() * 100 < (10 + (tavDef.PlayerLevel * 2)))
                    {
                        if (tavDef.IsRavager())
                        {
                            int level = (int)(tavDef.Skills.Poisoning.Value - 50.0) / 20;
                            level = level > 3 ? 3 : level;
                            if (level >= 0)
                            {
                                Poison newPoison = Poison.GetPoison( level );

                                if (newPoison != null)
                                {
                                    attacker.ApplyPoison(defender, newPoison);
                                    defender.PlaySound(0xDD);
                                    defender.FixedParticles(0x3728, 244, 25, 9941, 1266, 0, EffectLayer.Waist);
                                }
                            }
                            
                        }
                        else
                        {
                            BaseWeapon defWep = defender.Weapon as BaseWeapon;
                            if (CheckRiposte(defender, attacker))
                                defWep.OnHit(defender, attacker);
                        }
                    }
                }
            }
            return GetDelay(attacker);
        }

        public bool CheckRiposte(Mobile attacker, Mobile defender)
        {
            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

            Skill atkSkill = attacker.Skills[atkWeapon.Skill];
            Skill defSkill = defender.Skills[defWeapon.Skill];

            double atkValue = atkWeapon.GetAttackSkillValue(attacker, defender);
            double defValue = defWeapon.GetDefendSkillValue(attacker, defender);
            
            //Makesure the riposter is in range
            if (!attacker.InRange(defender, atkWeapon.MaxRange))
                return false;
            
            //No Riposting Behind yourself
            //if (IsBehind(defender,attacker))
            //   return false;

            // The chance to hit with ranged is not comparative, it's handled by the aim calculation.
            if (atkWeapon is BaseRanged)
                return false;

            //attacker.CheckSkill( atkSkill.SkillName, defValue - 20.0, 120.0 );
            //defender.CheckSkill( defSkill.SkillName, atkValue - 20.0, 120.0 );

            double ourValue, theirValue;
            int ourDex, theirDex;

            int bonus = GetHitChanceBonus();

                // Hit Chance Increase = 45%
                int atkChance = AosAttributes.GetValue(attacker, AosAttribute.AttackChance);
                if (atkChance > 45)
                    atkChance = 45;

                bonus += atkChance;

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
                    bonus += 10; // attacker gets 10% bonus when they're under divine fury

                if (HitLower.IsUnderAttackEffect(attacker))
                    bonus -= 25; // Under Hit Lower Attack effect -> 25% malus

                ourValue = (atkValue + 20.0) * (100 + bonus);

                // Defense Chance Increase = 45%
                bonus = AosAttributes.GetValue(defender, AosAttribute.DefendChance);
                if (bonus > 45)
                    bonus = 45;

                if (defender is TeiravonMobile && ((TeiravonMobile)defender).GetActiveFeats(TeiravonMobile.Feats.DefensiveStance))
                    bonus += 2 * ((TeiravonMobile)defender).PlayerLevel;

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(defender))
                    bonus -= 20; // defender loses 20% bonus when they're under divine fury

                if (HitLower.IsUnderDefenseEffect(defender))
                    bonus -= 25; // Under Hit Lower Defense effect -> 25% malus

                double discordanceScalar = 0.0;

                if (SkillHandlers.Discordance.GetScalar(attacker, ref discordanceScalar))
                    bonus += (int)(discordanceScalar * 100);

                theirValue = (defValue + 20.0) * (100 + bonus);

                bonus = 0;
            
            ourDex = attacker.Dex;
            theirDex = defender.Dex;

            double dexDif = 1.0;
            double chance = ourValue / (theirValue * 2.0);

            chance *= 1.0 + ((double)bonus / 100);

            if (theirDex > ourDex)
                dexDif = 1.0 - ((double)((theirDex - ourDex) * .5) / (double)theirDex);

            if (ourDex > theirDex)
                dexDif = 1.0 + ((1 - (double)theirDex / ourDex) * .5);

            chance *= dexDif;

            chance *= 1.0 + ((double)bonus / 100);

            //Riposte bonus to hit
            chance *= 1.5;

            return attacker.CheckSkill(atkSkill.SkillName, chance);
        }


        public virtual int GetHitAttackSound(Mobile attacker, Mobile defender)
        {
            int sound = attacker.GetAttackSound();

            if (sound == -1)
                sound = HitSound;

            return sound;
        }

        public virtual int GetHitDefendSound(Mobile attacker, Mobile defender)
        {
            return defender.GetHurtSound();
        }

        public virtual int GetMissAttackSound(Mobile attacker, Mobile defender)
        {
            if (attacker.GetAttackSound() == -1)
                return MissSound;
            else
                return -1;
        }

        public virtual int GetMissDefendSound(Mobile attacker, Mobile defender)
        {
            return -1;
        }

        public virtual int AbsorbDamageAOS(Mobile attacker, Mobile defender, int damage)
        {
            double positionChance = Utility.RandomDouble();
            BaseArmor armor;

            if (positionChance < 0.07)
                armor = defender.NeckArmor as BaseArmor;
            else if (positionChance < 0.14)
                armor = defender.HandArmor as BaseArmor;
            else if (positionChance < 0.28)
                armor = defender.ArmsArmor as BaseArmor;
            else if (positionChance < 0.43)
                armor = defender.HeadArmor as BaseArmor;
            else if (positionChance < 0.65)
                armor = defender.LegsArmor as BaseArmor;
            else
                armor = defender.ChestArmor as BaseArmor;

            Item check = defender.FindItemOnLayer(Layer.TwoHanded);
            BaseShield shield = null;
            if (check is BaseShield)
                shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

            if (shield != null)
            {
                bool blocked = false;
                double ratio = (double)((defender.Str - (shield.StrRequirement * 1.5)) / (double)defender.Str);
                double chance = defender.Skills.Parry.Value * ratio *.01;
                blocked = defender.CheckSkill(SkillName.Parry, chance);
                double toReduce = 0;
                if (blocked)
                {
                    defender.FixedEffect(0x37B9, 10, 16);
                    toReduce = (ratio * shield.ArmorBase * (1 + (defender.Skills.Parry.Value * .005)));
                    if (defender is TeiravonMobile && ((TeiravonMobile)defender).IsCavalier() && ((TeiravonMobile)defender).IsUndead() && ((TeiravonMobile)defender).Shapeshifted)
                        toReduce = toReduce * 1.25;
                    //defender.SendMessage("Incoming damage {0} : Reduction {1} ; Total damage {2}", damage, toReduce, damage - toReduce);
                    if (toReduce < 1)
                    {
                        // defender.SendMessage("The shield is too heavy to weild effectively.");
                        toReduce = 1;
                    }
                    if (damage - toReduce < 1)
                    {
                        damage = 0;
                    }
                    else
                        damage -= (int)toReduce;

                    double halfArmor = shield.ArmorRating / 2.0;
                    int absorbed = (int)(halfArmor + (halfArmor * Utility.RandomDouble()));

                    if (absorbed < 2)
                        absorbed = 2;

                    int wear;

                    if (Type == WeaponType.Bashing)
                        wear = (absorbed / 2);
                    else
                        wear = Utility.Random(2);

                    if (wear > 0 && shield.MaxHitPoints > 0)
                    {
                        if (shield.HitPoints >= wear)
                        {
                            shield.HitPoints -= wear;
                            wear = 0;
                        }
                        else
                        {
                            wear -= shield.HitPoints;
                            shield.HitPoints = 0;
                        }

                        if (wear > 0)
                        {
                            if (shield.MaxHitPoints > wear)
                            {
                                shield.MaxHitPoints -= wear;

                                if (shield.Parent is Mobile)
                                    ((Mobile)shield.Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                            }
                            else
                            {
                                shield.Delete();
                            }
                        }
                    }
                }
            }
            /*
            if (defender.Player || defender.Body.IsHuman)
            {

                bool blocked = false;

                // Dexterity below 80 reduces the chance to parry
                double chance = (defender.Skills[SkillName.Parry].Value * 0.0030);
                if (defender.Dex < 80)
                    chance = chance * (20 + defender.Dex) / 100;

                //if (defender.Str > 80)
                //    chance = chance * (20 + defender.Str) / 100;

                TeiravonMobile tavDef;

                if (defender is TeiravonMobile)
                {
                    tavDef = defender as TeiravonMobile;
                    if (tavDef.HasFeat(TeiravonMobile.Feats.Riposte))
                        chance = chance * 1.5;
                    if (tavDef.IsCavalier() && tavDef.IsUndead() && tavDef.Shapeshifted)
                        chance = chance * 1.25;

                    if (tavDef.IsMonk() && tavDef.Weapon is Fists)
                        blocked = defender.CheckSkill(SkillName.Parry, chance);
                }
                if (shield != null)
                {
                    blocked = defender.CheckSkill(SkillName.Parry, chance);
                }
                else if (!(defender.Weapon is Fists) && !(defender.Weapon is BaseRanged))
                {
                    chance /= 2;

                    blocked = (chance > Utility.RandomDouble()); // Only skillcheck if wielding a shield
                }

                if (blocked && !IsBehind(attacker, defender))
                {
                    defender.FixedEffect(0x37B9, 10, 16);
                    damage = 0;

                    if (defender is TeiravonMobile)
                    {
                        TeiravonMobile ripDef = defender as TeiravonMobile;
                        if (ripDef.HasFeat(TeiravonMobile.Feats.Riposte))
                        {
                            BaseWeapon defWep = defender.Weapon as BaseWeapon;
                            defWep.OnHit(defender, attacker);
                        }
                    }

                    if (shield != null)
                    {
                        double halfArmor = shield.ArmorRating / 2.0;
                        int absorbed = (int)(halfArmor + (halfArmor * Utility.RandomDouble()));

                        if (absorbed < 2)
                            absorbed = 2;

                        int wear;

                        if (Type == WeaponType.Bashing)
                            wear = (absorbed / 2);
                        else
                            wear = Utility.Random(2);

                        if (wear > 0 && shield.MaxHitPoints > 0)
                        {
                            if (shield.HitPoints >= wear)
                            {
                                shield.HitPoints -= wear;
                                wear = 0;
                            }
                            else
                            {
                                wear -= shield.HitPoints;
                                shield.HitPoints = 0;
                            }

                            if (wear > 0)
                            {
                                if (shield.MaxHitPoints > wear)
                                {
                                    shield.MaxHitPoints -= wear;

                                    if (shield.Parent is Mobile)
                                        ((Mobile)shield.Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                                }
                                else
                                {
                                    shield.Delete();
                                }
                            }
                        }
                    }
                }
            }
                */

            /*
            else if (defender is TeiravonMobile && ((TeiravonMobile)defender).HasFeat(TeiravonMobile.Feats.ShieldMastery) && shield != null)
            {
                double toReduce = 0;
                if (defender.Str > shield.StrRequirement * 2)
                {
                    toReduce = ((double)(defender.Str - (shield.StrRequirement * 2)) / defender.Str) * shield.ArmorBase;
                    shield.OnHit(this, damage);
                    //defender.SendMessage("Incoming damage {0} : Reduction {1} ; Total damage {2}", damage, toReduce, damage - toReduce);
                    if (damage - toReduce < 1)
                    {
                        damage = 0;
                    }
                    else
                        damage -= (int)toReduce;
                }

            }
            */
            if (armor != null)
                armor.OnHit(this, damage); // call OnHit to lose durability

            //STARTMOD: TEIRAVON: Armor classification system

            if (armor != null && damage > 0)
            {
                BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
                //BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

                if (armor.MaterialType == ArmorMaterialType.Plate)
                {
                    int reduce = 0;
                    reduce = Utility.RandomMinMax(40, 60);


                    if (Utility.RandomBool())
                    {
                        damage -= AOS.Scale(damage, reduce);
                        defender.SendMessage("Your heavy armor greatly reduces the damage you take.");
                    }
                    else
                    {
                        if (defender is TeiravonMobile)
                        {
                            TeiravonMobile m_Player = (TeiravonMobile)defender;
                            int deflectchance = m_Player.PlayerLevel / 4 + 1;

                            if (Utility.Random(100) < deflectchance)
                            {
                                damage = 0;
                                defender.SendMessage("The attack bounces off your armor!");
                            }
                        }
                    }
                }

                else if (armor.MaterialType == ArmorMaterialType.Chainmail || armor.MaterialType == ArmorMaterialType.Dwarven)
                {
                    int reduce = 0;
                    reduce = Utility.RandomMinMax(20, 40);

                    if (Utility.RandomBool())
                    {
                        damage -= AOS.Scale(damage, reduce);
                        defender.SendMessage("Your medium armor reduces the damage you take.");
                    }
                }

                else if (armor.MaterialType == ArmorMaterialType.Ringmail)
                {
                    int reduce = 0;
                    reduce = Utility.RandomMinMax(15, 30);

                    if (Utility.RandomBool())
                    {
                        damage -= AOS.Scale(damage, reduce);
                        defender.SendMessage("Your armor reduces the damage you take.");
                    }
                }
                else if (armor.MaterialType == ArmorMaterialType.Bone)
                {
                    int reduce = 0;
                    reduce = Utility.RandomMinMax(20, 40);

                    if (Utility.RandomBool())
                    {
                        damage -= AOS.Scale(damage, reduce);
                        defender.SendMessage("Your bone armor reduces the damage you take.");
                        if (Utility.RandomBool())
                        {
                            armor.HitPoints--;
                            defender.SendMessage("But it is damaged in the process.");
                        }
                    }
                }
                    
                else if (armor.MaterialType == ArmorMaterialType.Studded)
                {
                    int reduce = 0;
                    reduce = Utility.RandomMinMax(5, 10);

                    if (Utility.RandomBool())
                    {
                        damage -= AOS.Scale(damage, reduce);
                        defender.SendMessage("Your armor reduces the damage you take.");
                    }
                }
                else if (armor.MaterialType == ArmorMaterialType.Leather)
                {
                    int reduce = 0;
                    reduce = Utility.RandomMinMax(2, 5);

                    if (Utility.RandomBool())
                    {
                        damage -= AOS.Scale(damage, reduce);
                        defender.SendMessage("Your armor slightly reduces the damage you take.");
                    }
                }

            }
            //ENDMOD: TEIRAVON: Armor classification system


            return damage;
        }

        public virtual int AbsorbDamage(Mobile attacker, Mobile defender, int damage)
        {
            if (Core.AOS)
                return AbsorbDamageAOS(attacker, defender, damage);

            double chance = Utility.RandomDouble();
            BaseArmor armor;

            if (chance < 0.07)
                armor = defender.NeckArmor as BaseArmor;
            else if (chance < 0.14)
                armor = defender.HandArmor as BaseArmor;
            else if (chance < 0.28)
                armor = defender.ArmsArmor as BaseArmor;
            else if (chance < 0.43)
                armor = defender.HeadArmor as BaseArmor;
            else if (chance < 0.65)
                armor = defender.LegsArmor as BaseArmor;
            else
                armor = defender.ChestArmor as BaseArmor;

            if (armor != null)
                damage = armor.OnHit(this, damage);

            BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
            if (shield != null)
                damage = shield.OnHit(this, damage);

            // XmlAttachment check for OnArmorHit
            damage -= Server.Engines.XmlSpawner2.XmlAttach.OnArmorHit(attacker, defender, armor, this, damage);
            damage -= Server.Engines.XmlSpawner2.XmlAttach.OnArmorHit(attacker, defender, shield, this, damage);

            int virtualArmor = defender.VirtualArmor + defender.VirtualArmorMod;

            /*
            if (defender is TeiravonMobile)
            {
                TeiravonMobile m_Player = defender as TeiravonMobile;

                if (m_Player.IsMonk() && defender.Weapon is Fists)
                {
                    MonkBlock stance = (MonkBlock)m_Player.FindItemOnLayer(Layer.Unused_xF);

                    if (stance != null)
                        Server.Engines.XmlSpawner2.XmlAttach.OnArmorHit(attacker, m_Player, stance, this, damage);
                }

            }
            */
            if (virtualArmor > 0)
            {
                double scalar;

                if (chance < 0.14)
                    scalar = 0.07;
                else if (chance < 0.28)
                    scalar = 0.14;
                else if (chance < 0.43)
                    scalar = 0.15;
                else if (chance < 0.65)
                    scalar = 0.22;
                else
                    scalar = 0.35;

                int from = (int)(virtualArmor * scalar) / 2;
                int to = (int)(virtualArmor * scalar);

                damage -= Utility.Random(from, (to - from) + 1);
            }

            return damage;
        }

        public virtual int GetPackInstinctBonus(Mobile attacker, Mobile defender)
        {
            if (attacker.Player || defender.Player)
                return 0;

            BaseCreature bc = attacker as BaseCreature;

            if (bc == null || bc.PackInstinct == PackInstinct.None || (!bc.Controled && !bc.Summoned))
                return 0;

            Mobile master = bc.ControlMaster;

            if (master == null)
                master = bc.SummonMaster;

            if (master == null)
                return 0;

            int inPack = 1;

            foreach (Mobile m in defender.GetMobilesInRange(1))
            {
                if (m != attacker && m is BaseCreature)
                {
                    BaseCreature tc = (BaseCreature)m;

                    if ((tc.PackInstinct & bc.PackInstinct) == 0 || (!tc.Controled && !tc.Summoned))
                        continue;

                    Mobile theirMaster = tc.ControlMaster;

                    if (theirMaster == null)
                        theirMaster = tc.SummonMaster;

                    if (master == theirMaster && tc.Combatant == defender)
                        ++inPack;
                }
            }

            if (inPack >= 5)
                return 100;
            else if (inPack >= 4)
                return 75;
            else if (inPack >= 3)
                return 50;
            else if (inPack >= 2)
                return 25;

            return 0;
        }

        private static bool m_InDoubleStrike;

        public static bool InDoubleStrike
        {
            get { return m_InDoubleStrike; }
            set { m_InDoubleStrike = value; }
        }

        public virtual void OnHit(Mobile attacker, Mobile defender)
        {
            if (MirrorImage.HasClone(defender))
            {
                Clone bc;

                foreach (Mobile m in defender.GetMobilesInRange(4))
                {
                    bc = m as Clone;

                    if (m is Clone && bc != null && bc.Summoned && bc.SummonMaster == defender)
                    {
                        defender.SendMessage("Your watery illusion intercedes on the attack.");
                        defender = m;
                        break;
                    }
                }
            }
            PlaySwingAnimation(attacker);
            PlayHurtAnimation(defender);

            attacker.PlaySound(GetHitAttackSound(attacker, defender));
            defender.PlaySound(GetHitDefendSound(attacker, defender));

            int damage = ComputeDamage(attacker, defender);

            CheckSlayerResult cs = CheckSlayers(attacker, defender);



            if (cs != CheckSlayerResult.None)
            {
                if (cs == CheckSlayerResult.Slayer)
                    defender.FixedEffect(0x37B9, 10, 5);

                damage *= 2;
            }

            if (!attacker.Player)
            {
                if (defender is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)defender;

                    if (pm.EnemyOfOneType != null && pm.EnemyOfOneType != attacker.GetType())
                        damage *= 2;
                }
            }
            else if (!defender.Player)
            {
                if (attacker is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)attacker;

                    if (pm.WaitingForEnemy)
                    {
                        pm.EnemyOfOneType = defender.GetType();
                        pm.WaitingForEnemy = false;
                    }

                    if (pm.EnemyOfOneType == defender.GetType())
                    {
                        defender.FixedEffect(0x37B9, 10, 5, 1160, 0);
                        damage += AOS.Scale(damage, 50);
                    }
                }
            }

            int packInstinctBonus = GetPackInstinctBonus(attacker, defender);

            if (packInstinctBonus != 0)
                damage += AOS.Scale(damage, packInstinctBonus);

            if (m_InDoubleStrike)
                damage -= AOS.Scale(damage, 10); // 10% loss when attacking with double-strike

            TransformContext context = TransformationSpell.GetContext(defender);

            if (m_Slayer == SlayerName.Silver && context != null && context.Type != typeof(HorrificBeastSpell))
                damage += AOS.Scale(damage, 25); // Every necromancer transformation other than horrific beast take an additional 25% damage

            if (attacker is BaseCreature)
                ((BaseCreature)attacker).AlterMeleeDamageTo(defender, ref damage);

            if (defender is BaseCreature)
                ((BaseCreature)defender).AlterMeleeDamageFrom(attacker, ref damage);

            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

            damage = AbsorbDamage(attacker, defender, damage);


            if (defender.VirtualArmorMod >= 1)
            {
                if (Utility.Random(3) > 1)
                {
                    int amount = damage - defender.VirtualArmorMod;
                    
                    defender.VirtualArmorMod -= damage;
                    if (amount < 0)
                        amount = 0;
                    
                    damage = amount;

                    if (defender.VirtualArmorMod < 1)
                    {
                        defender.VirtualArmorMod = 0;
                        defender.FixedParticles(14276, 10, 20, 14276, EffectLayer.Waist);
                        defender.PlaySound(0x38F);
                    }
                    else
                    {
                        string shield = defender.VirtualArmorMod.ToString();
                        defender.LocalOverheadMessage(MessageType.Emote, 90, true, shield);
                        defender.FixedParticles(14265, 10, 20, 14276, EffectLayer.Waist);
                        defender.PlaySound(0x525);
                    }
                }
            }

            if (!Core.AOS && damage < 1)
                damage = 1;
            else if (Core.AOS && damage == 0) // parried
            {
                if (a != null && a.Validate(attacker) /*&& a.CheckMana( attacker, true )*/ ) // Parried special moves have no mana cost
                {
                    a = null;
                    WeaponAbility.ClearCurrentAbility(attacker);

                    attacker.SendLocalizedMessage(1061140); // Your attack was parried!
                }
            }

            int phys, fire, cold, pois, nrgy;

            GetDamageTypes(attacker, out phys, out fire, out cold, out pois, out nrgy);

            if (m_Consecrated || Spells.Third.BlessSpell.talathas.Contains(attacker))
            {
                phys = defender.PhysicalResistance;
                if (Spells.Third.BlessSpell.talathas.Contains(defender))
                    phys = 95;

                fire = defender.FireResistance;
                cold = defender.ColdResistance;
                pois = defender.PoisonResistance;
                nrgy = defender.EnergyResistance;

                int low = phys, type = 0;

                if (fire < low) { low = fire; type = 1; }
                if (cold < low) { low = cold; type = 2; }
                if (pois < low) { low = pois; type = 3; }
                if (nrgy < low) { low = nrgy; type = 4; }

                phys = fire = cold = pois = nrgy = 0;

                if (type == 0) phys = 100;
                else if (type == 1) fire = 100;
                else if (type == 2) cold = 100;
                else if (type == 3) pois = 100;
                else if (type == 4) nrgy = 100;
            }

            int damageGiven = damage;

            AOS.ArmorIgnore = (a is ArmorIgnore);
            bool Ambushing = false;
            //Ambush change mod TEIRAVON
            if (attacker is TeiravonMobile)
            {
                TeiravonMobile tav = attacker as TeiravonMobile;
                if (tav.Ambush)
                {
                    Ambushing = true;
                    tav.Ambush = false;
                    attacker.SendMessage("You ambush {0}", defender.Name);
                }
            }
            //STARTMOD: TEIRAVON customized attack/defend functions:

            //Kensai parry functions
            #region Kensai
            bool KensaiParried = false;

            if (defender is TeiravonMobile)
            {
                bool rangedattacker = false;

                rangedattacker = attacker.GetDistanceToSqrt(defender) > 1;

                TeiravonMobile m_Player = (TeiravonMobile)defender;
                BaseWeapon m_Weapon = defender.Weapon as BaseWeapon;
                BaseWeapon m_AttackWeapon = attacker.Weapon as BaseWeapon;

                if (m_AttackWeapon is BaseRanged)
                    rangedattacker = true;

                if (((TeiravonMobile)defender).IsKensai() && !IsBehind(attacker, defender))
                {

                    if ((m_Weapon != null) && (defender != attacker) && (m_Weapon.Skill == m_Player.Skills.Swords.SkillName))
                    {
                        int chanceOfBlock = 25 + (m_Player.PlayerLevel) + (int)(m_Player.Skills.Parry.Base / 10) + (int)(m_Player.Dex / 12);

                        if (m_Player.PlayerLevel >= 20)
                            chanceOfBlock += 5;

                        if (rangedattacker)
                            chanceOfBlock /= 4;

                        //m_Player.SendMessage( "Chance of block is {0}", (int)chanceOfBlock );

                        if (Utility.RandomMinMax(0, 100) < chanceOfBlock)
                        {
                            m_Player.SendMessage("You parry the attack!");
                            attacker.SendMessage("{0} parries your attack!", m_Player.Name);

                            KensaiParried = true;
                            damageGiven = 0;

                            int dmgchance = 10 - ((int)(m_Player.PlayerLevel / 2.5));
                            if (Utility.RandomMinMax(1, 100) < dmgchance)
                            {
                                int wpndmg = (int)(attacker.Str / 100);
                                if (wpndmg < 1)
                                    wpndmg = 1;
                                else if (wpndmg > 10)
                                    wpndmg = 10;

                                m_Player.SendMessage("Your parry damages your weapon");

                                if (m_Weapon.Hits >= wpndmg)
                                    m_Weapon.Hits -= wpndmg;
                                else
                                {
                                    wpndmg -= m_Weapon.Hits;
                                    m_Weapon.Hits = 0;
                                    if (m_Weapon.MaxHits > wpndmg)
                                    {
                                        m_Weapon.MaxHits -= wpndmg;
                                        m_Player.SendMessage("Your parry seriously damages your weapon");
                                    }
                                    else
                                    {
                                        m_Weapon.Delete();
                                        m_Player.SendMessage("Your weapon has been destroyed!");
                                    }
                                }
                            }
                        }
                    }
                }


                if (m_Player.HasFeat(TeiravonMobile.Feats.AcrobaticCombat) && (!KensaiParried) && rangedattacker && (attacker != defender))
                {

                    int maxavoid = 20 + (m_Player.PlayerLevel) + (int)(m_Player.Dex / 15);

                    if (maxavoid > 75)
                        maxavoid = 75;

                    if (Utility.RandomMinMax(0, 100) < maxavoid)
                    {
                        m_Player.SendMessage("You skillfully dodge the attack!");
                        attacker.SendMessage("{0} skillfully dodges your attack!", m_Player.Name);

                        KensaiParried = true;
                        damageGiven = 0;
                        return;
                    }
                }
            }
            #endregion
            if (attacker is TeiravonMobile)
            {
                TeiravonMobile m_Player = (TeiravonMobile)attacker;

                if (m_Player.FatalShotReady)
                {
                    int dbonus;

                    dbonus = Utility.RandomMinMax(m_Player.PlayerLevel, m_Player.PlayerLevel * 2) + 10;
                    if (!(defender is TeiravonMobile))
                        dbonus *= 2;

                    damage += dbonus;
                    if (Utility.RandomMinMax(1, 40) <= m_Player.PlayerLevel)
                        m_Player.FatalShotReady = false;
                }
            }

            //STANDARD DAMAGE CALL
            if (!KensaiParried)
            {
                int ignoring = 0;
                if (Ambushing)
                    ignoring = 50;
                damageGiven = AOS.Damage(defender, attacker, damage, phys, fire, cold, pois, nrgy, ignoring);
                AddBlood(attacker, defender, damage);
                #region magicdrain
                if (attacker is IMagicDrain || defender is IMagicDrain)
                {
                    int drainchance = 0;
                    IMagicDrain imd;
                    Mobile Drainer;
                    Mobile Drained;

                    if (attacker is IMagicDrain)
                    {
                        imd = (IMagicDrain)attacker;
                        drainchance = imd.DrainIntensity;
                        Drainer = attacker;
                        Drained = defender;
                    }
                    else
                    {
                        imd = (IMagicDrain)defender;
                        drainchance = imd.DrainIntensity;
                        Drainer = defender;
                        Drained = attacker;
                    }

                    if (Drained is TeiravonMobile)
                    {
                        TeiravonMobile tm = (TeiravonMobile)Drained;
                        if (Utility.RandomMinMax(1, 100) < drainchance)
                        {
                            double reduction = 0;
                            int totalreduction = 0;
                            BaseArmor armor;
                            BaseWeapon weapon;
                            BaseJewel jewel;
                            BaseClothing clothing;
                            SkillName skill;
                            ArrayList items = tm.Items;
                            for (int i = 0; i < items.Count; ++i)
                            {
                                object obj = items[i];

                                //								if (obj is OrcFace)
                                //									continue;

                                if (obj is BaseArmor)
                                {
                                    armor = (BaseArmor)obj;
                                    for (int ii = 0; ii < 5; ++ii) //AosSkillBonuses
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.SkillBonuses.GetValues(ii, out skill, out reduction);
                                            reduction -= Utility.RandomMinMax(0, (int)reduction);
                                            totalreduction += 1;
                                            armor.SkillBonuses.SetValues(ii, skill, reduction);
                                        }
                                    }

                                    if (armor.ArmorAttributes.LowerStatReq > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.ArmorAttributes.LowerStatReq -= Utility.RandomMinMax(0, armor.ArmorAttributes.LowerStatReq);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.ArmorAttributes.SelfRepair > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.ArmorAttributes.SelfRepair -= Utility.RandomMinMax(0, armor.ArmorAttributes.SelfRepair);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.ArmorAttributes.MageArmor > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.ArmorAttributes.MageArmor -= Utility.RandomMinMax(0, armor.ArmorAttributes.MageArmor);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.ArmorAttributes.DurabilityBonus > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.ArmorAttributes.DurabilityBonus -= Utility.RandomMinMax(0, armor.ArmorAttributes.DurabilityBonus);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.RegenHits > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.RegenHits -= Utility.RandomMinMax(0, armor.Attributes.RegenHits);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.RegenStam > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.RegenStam -= Utility.RandomMinMax(0, armor.Attributes.RegenStam);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.RegenMana > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.RegenMana -= Utility.RandomMinMax(0, armor.Attributes.RegenMana);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.DefendChance > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.DefendChance -= Utility.RandomMinMax(0, armor.Attributes.DefendChance);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.AttackChance > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.AttackChance -= Utility.RandomMinMax(0, armor.Attributes.AttackChance);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.BonusStr > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.BonusStr -= Utility.RandomMinMax(0, armor.Attributes.BonusStr);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.BonusDex > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.BonusDex -= Utility.RandomMinMax(0, armor.Attributes.BonusDex);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.BonusInt > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.BonusInt -= Utility.RandomMinMax(0, armor.Attributes.BonusInt);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.BonusHits > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.BonusHits -= Utility.RandomMinMax(0, armor.Attributes.BonusHits);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.BonusStam > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.BonusStam -= Utility.RandomMinMax(0, armor.Attributes.BonusStam);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.BonusMana > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.BonusMana -= Utility.RandomMinMax(0, armor.Attributes.BonusMana);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.WeaponDamage > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.WeaponDamage -= Utility.RandomMinMax(0, armor.Attributes.WeaponDamage);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.WeaponSpeed > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.WeaponSpeed -= Utility.RandomMinMax(0, armor.Attributes.WeaponSpeed);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.SpellDamage > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.SpellDamage -= Utility.RandomMinMax(0, armor.Attributes.SpellDamage);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.CastRecovery > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.CastRecovery -= Utility.RandomMinMax(0, armor.Attributes.CastRecovery);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.CastSpeed > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.CastSpeed -= Utility.RandomMinMax(0, armor.Attributes.CastSpeed);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.LowerManaCost > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.LowerManaCost -= Utility.RandomMinMax(0, armor.Attributes.LowerManaCost);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.LowerRegCost > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.LowerRegCost -= Utility.RandomMinMax(0, armor.Attributes.LowerRegCost);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.ReflectPhysical > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.ReflectPhysical -= Utility.RandomMinMax(0, armor.Attributes.ReflectPhysical);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.EnhancePotions > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.EnhancePotions -= Utility.RandomMinMax(0, armor.Attributes.EnhancePotions);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.Luck > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.Luck -= Utility.RandomMinMax(0, armor.Attributes.Luck);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.SpellChanneling > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.SpellChanneling -= Utility.RandomMinMax(0, armor.Attributes.SpellChanneling);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (armor.Attributes.NightSight > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            armor.Attributes.NightSight -= Utility.RandomMinMax(0, armor.Attributes.NightSight);
                                            totalreduction += 1;
                                        }
                                    }


                                }
                                else if (obj is BaseWeapon)
                                {
                                    weapon = (BaseWeapon)obj;
                                    for (int ii = 0; ii < 5; ++ii) //AosSkillBonuses
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.SkillBonuses.GetValues(ii, out skill, out reduction);
                                            reduction -= Utility.RandomMinMax(0, (int)reduction);
                                            totalreduction += 1;
                                            weapon.SkillBonuses.SetValues(ii, skill, reduction);
                                        }
                                    }

                                    if (weapon.WeaponAttributes.LowerStatReq > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.LowerStatReq -= Utility.RandomMinMax(0, weapon.WeaponAttributes.LowerStatReq);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.SelfRepair > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.SelfRepair -= Utility.RandomMinMax(0, weapon.WeaponAttributes.SelfRepair);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitLeechHits > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitLeechHits -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitLeechHits);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitLeechStam > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitLeechStam -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitLeechStam);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitLeechMana > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitLeechMana -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitLeechMana);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitLowerAttack > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitLowerAttack -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitLowerAttack);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitLowerDefend > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitLowerDefend -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitLowerDefend);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitMagicArrow > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitMagicArrow -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitMagicArrow);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitHarm > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitHarm -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitHarm);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitFireball > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitFireball -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitFireball);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitLightning > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitLightning -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitLightning);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitDispel > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitDispel -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitDispel);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitColdArea > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitColdArea -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitColdArea);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitFireArea > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitFireArea -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitFireArea);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitPoisonArea > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitPoisonArea -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitPoisonArea);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitEnergyArea > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitEnergyArea -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitEnergyArea);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.HitPhysicalArea > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.HitPhysicalArea -= Utility.RandomMinMax(0, weapon.WeaponAttributes.HitPhysicalArea);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.ResistPhysicalBonus > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.ResistPhysicalBonus -= Utility.RandomMinMax(0, weapon.WeaponAttributes.ResistPhysicalBonus);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.ResistFireBonus > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.ResistFireBonus -= Utility.RandomMinMax(0, weapon.WeaponAttributes.ResistFireBonus);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.ResistColdBonus > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.ResistColdBonus -= Utility.RandomMinMax(0, weapon.WeaponAttributes.ResistColdBonus);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.ResistPoisonBonus > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.ResistPoisonBonus -= Utility.RandomMinMax(0, weapon.WeaponAttributes.ResistPoisonBonus);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.ResistEnergyBonus > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.ResistEnergyBonus -= Utility.RandomMinMax(0, weapon.WeaponAttributes.ResistEnergyBonus);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.UseBestSkill > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.UseBestSkill -= Utility.RandomMinMax(0, weapon.WeaponAttributes.UseBestSkill);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.MageWeapon > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.MageWeapon -= Utility.RandomMinMax(0, weapon.WeaponAttributes.MageWeapon);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.WeaponAttributes.DurabilityBonus > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.WeaponAttributes.DurabilityBonus -= Utility.RandomMinMax(0, weapon.WeaponAttributes.DurabilityBonus);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.RegenHits > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.RegenHits -= Utility.RandomMinMax(0, weapon.Attributes.RegenHits);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.RegenStam > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.RegenStam -= Utility.RandomMinMax(0, weapon.Attributes.RegenStam);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.RegenMana > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.RegenMana -= Utility.RandomMinMax(0, weapon.Attributes.RegenMana);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.DefendChance > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.DefendChance -= Utility.RandomMinMax(0, weapon.Attributes.DefendChance);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.AttackChance > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.AttackChance -= Utility.RandomMinMax(0, weapon.Attributes.AttackChance);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.BonusStr > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.BonusStr -= Utility.RandomMinMax(0, weapon.Attributes.BonusStr);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.BonusDex > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.BonusDex -= Utility.RandomMinMax(0, weapon.Attributes.BonusDex);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.BonusInt > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.BonusInt -= Utility.RandomMinMax(0, weapon.Attributes.BonusInt);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.BonusHits > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.BonusHits -= Utility.RandomMinMax(0, weapon.Attributes.BonusHits);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.BonusStam > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.BonusStam -= Utility.RandomMinMax(0, weapon.Attributes.BonusStam);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.BonusMana > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.BonusMana -= Utility.RandomMinMax(0, weapon.Attributes.BonusMana);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.WeaponDamage > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.WeaponDamage -= Utility.RandomMinMax(0, weapon.Attributes.WeaponDamage);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.WeaponSpeed > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.WeaponSpeed -= Utility.RandomMinMax(0, weapon.Attributes.WeaponSpeed);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.SpellDamage > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.SpellDamage -= Utility.RandomMinMax(0, weapon.Attributes.SpellDamage);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.CastRecovery > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.CastRecovery -= Utility.RandomMinMax(0, weapon.Attributes.CastRecovery);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.CastSpeed > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.CastSpeed -= Utility.RandomMinMax(0, weapon.Attributes.CastSpeed);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.LowerManaCost > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.LowerManaCost -= Utility.RandomMinMax(0, weapon.Attributes.LowerManaCost);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.LowerRegCost > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.LowerRegCost -= Utility.RandomMinMax(0, weapon.Attributes.LowerRegCost);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.ReflectPhysical > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.ReflectPhysical -= Utility.RandomMinMax(0, weapon.Attributes.ReflectPhysical);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.EnhancePotions > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.EnhancePotions -= Utility.RandomMinMax(0, weapon.Attributes.EnhancePotions);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.Luck > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.Luck -= Utility.RandomMinMax(0, weapon.Attributes.Luck);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.SpellChanneling > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.SpellChanneling -= Utility.RandomMinMax(0, weapon.Attributes.SpellChanneling);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (weapon.Attributes.NightSight > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            weapon.Attributes.NightSight -= Utility.RandomMinMax(0, weapon.Attributes.NightSight);
                                            totalreduction += 1;
                                        }
                                    }

                                }
                                else if (obj is BaseJewel)
                                {
                                    int iii;
                                    jewel = (BaseJewel)obj;
                                    for (int ii = 0; ii < 5; ++ii) //AosSkillBonuses
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.SkillBonuses.GetValues(ii, out skill, out reduction);
                                            reduction -= Utility.RandomMinMax(0, (int)reduction);
                                            totalreduction += 1;
                                            jewel.SkillBonuses.SetValues(ii, skill, reduction);
                                        }

                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            switch (ii)
                                            {
                                                case 0:
                                                    iii = 1; break;
                                                case 1:
                                                    iii = 2; break;
                                                case 2:
                                                    iii = 4; break;
                                                case 3:
                                                    iii = 8; break;
                                                case 4:
                                                    iii = 16; break;
                                                default:
                                                    iii = 0; break;
                                            }
                                            reduction = jewel.Resistances.GetValue(iii);
                                            reduction -= Utility.RandomMinMax(0, (int)reduction);
                                            totalreduction += 1;
                                            jewel.Resistances.SetValue(iii, (int)reduction);
                                        }
                                    }

                                    if (jewel.Attributes.RegenHits > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.RegenHits -= Utility.RandomMinMax(0, jewel.Attributes.RegenHits);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.RegenStam > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.RegenStam -= Utility.RandomMinMax(0, jewel.Attributes.RegenStam);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.RegenMana > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.RegenMana -= Utility.RandomMinMax(0, jewel.Attributes.RegenMana);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.DefendChance > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.DefendChance -= Utility.RandomMinMax(0, jewel.Attributes.DefendChance);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.AttackChance > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.AttackChance -= Utility.RandomMinMax(0, jewel.Attributes.AttackChance);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.BonusStr > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.BonusStr -= Utility.RandomMinMax(0, jewel.Attributes.BonusStr);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.BonusDex > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.BonusDex -= Utility.RandomMinMax(0, jewel.Attributes.BonusDex);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.BonusInt > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.BonusInt -= Utility.RandomMinMax(0, jewel.Attributes.BonusInt);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.BonusHits > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.BonusHits -= Utility.RandomMinMax(0, jewel.Attributes.BonusHits);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.BonusStam > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.BonusStam -= Utility.RandomMinMax(0, jewel.Attributes.BonusStam);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.BonusMana > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.BonusMana -= Utility.RandomMinMax(0, jewel.Attributes.BonusMana);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.WeaponDamage > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.WeaponDamage -= Utility.RandomMinMax(0, jewel.Attributes.WeaponDamage);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.WeaponSpeed > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.WeaponSpeed -= Utility.RandomMinMax(0, jewel.Attributes.WeaponSpeed);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.SpellDamage > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.SpellDamage -= Utility.RandomMinMax(0, jewel.Attributes.SpellDamage);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.CastRecovery > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.CastRecovery -= Utility.RandomMinMax(0, jewel.Attributes.CastRecovery);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.CastSpeed > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.CastSpeed -= Utility.RandomMinMax(0, jewel.Attributes.CastSpeed);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.LowerManaCost > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.LowerManaCost -= Utility.RandomMinMax(0, jewel.Attributes.LowerManaCost);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.LowerRegCost > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.LowerRegCost -= Utility.RandomMinMax(0, jewel.Attributes.LowerRegCost);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.ReflectPhysical > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.ReflectPhysical -= Utility.RandomMinMax(0, jewel.Attributes.ReflectPhysical);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.EnhancePotions > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.EnhancePotions -= Utility.RandomMinMax(0, jewel.Attributes.EnhancePotions);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.Luck > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.Luck -= Utility.RandomMinMax(0, jewel.Attributes.Luck);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.SpellChanneling > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.SpellChanneling -= Utility.RandomMinMax(0, jewel.Attributes.SpellChanneling);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (jewel.Attributes.NightSight > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            jewel.Attributes.NightSight -= Utility.RandomMinMax(0, jewel.Attributes.NightSight);
                                            totalreduction += 1;
                                        }
                                    }

                                }
                                else if (obj is BaseClothing)
                                {
                                    int iii;
                                    clothing = (BaseClothing)obj;
                                    for (int ii = 0; ii < 5; ++ii) //AosSkillBonuses
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.SkillBonuses.GetValues(ii, out skill, out reduction);
                                            reduction -= Utility.RandomMinMax(0, (int)reduction);
                                            totalreduction += 1;
                                            clothing.SkillBonuses.SetValues(ii, skill, reduction);
                                        }

                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            switch (ii)
                                            {
                                                case 0:
                                                    iii = 1; break;
                                                case 1:
                                                    iii = 2; break;
                                                case 2:
                                                    iii = 4; break;
                                                case 3:
                                                    iii = 8; break;
                                                case 4:
                                                    iii = 16; break;
                                                default:
                                                    iii = 0; break;
                                            }
                                            reduction = clothing.Resistances.GetValue(iii);
                                            reduction -= Utility.RandomMinMax(0, (int)reduction);
                                            totalreduction += 1;
                                            clothing.Resistances.SetValue(iii, (int)reduction);
                                        }
                                    }

                                    if (clothing.Attributes.RegenHits > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.RegenHits -= Utility.RandomMinMax(0, clothing.Attributes.RegenHits);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.RegenStam > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.RegenStam -= Utility.RandomMinMax(0, clothing.Attributes.RegenStam);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.RegenMana > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.RegenMana -= Utility.RandomMinMax(0, clothing.Attributes.RegenMana);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.DefendChance > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.DefendChance -= Utility.RandomMinMax(0, clothing.Attributes.DefendChance);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.AttackChance > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.AttackChance -= Utility.RandomMinMax(0, clothing.Attributes.AttackChance);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.BonusStr > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.BonusStr -= Utility.RandomMinMax(0, clothing.Attributes.BonusStr);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.BonusDex > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.BonusDex -= Utility.RandomMinMax(0, clothing.Attributes.BonusDex);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.BonusInt > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.BonusInt -= Utility.RandomMinMax(0, clothing.Attributes.BonusInt);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.BonusHits > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.BonusHits -= Utility.RandomMinMax(0, clothing.Attributes.BonusHits);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.BonusStam > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.BonusStam -= Utility.RandomMinMax(0, clothing.Attributes.BonusStam);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.BonusMana > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.BonusMana -= Utility.RandomMinMax(0, clothing.Attributes.BonusMana);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.WeaponDamage > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.WeaponDamage -= Utility.RandomMinMax(0, clothing.Attributes.WeaponDamage);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.WeaponSpeed > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.WeaponSpeed -= Utility.RandomMinMax(0, clothing.Attributes.WeaponSpeed);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.SpellDamage > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.SpellDamage -= Utility.RandomMinMax(0, clothing.Attributes.SpellDamage);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.CastRecovery > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.CastRecovery -= Utility.RandomMinMax(0, clothing.Attributes.CastRecovery);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.CastSpeed > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.CastSpeed -= Utility.RandomMinMax(0, clothing.Attributes.CastSpeed);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.LowerManaCost > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.LowerManaCost -= Utility.RandomMinMax(0, clothing.Attributes.LowerManaCost);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.LowerRegCost > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.LowerRegCost -= Utility.RandomMinMax(0, clothing.Attributes.LowerRegCost);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.ReflectPhysical > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.ReflectPhysical -= Utility.RandomMinMax(0, clothing.Attributes.ReflectPhysical);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.EnhancePotions > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.EnhancePotions -= Utility.RandomMinMax(0, clothing.Attributes.EnhancePotions);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.Luck > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.Luck -= Utility.RandomMinMax(0, clothing.Attributes.Luck);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.SpellChanneling > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.SpellChanneling -= Utility.RandomMinMax(0, clothing.Attributes.SpellChanneling);
                                            totalreduction += 1;
                                        }
                                    }

                                    if (clothing.Attributes.NightSight > 0)
                                    {
                                        if (Utility.RandomMinMax(1, 100) < drainchance)
                                        {
                                            clothing.Attributes.NightSight -= Utility.RandomMinMax(0, clothing.Attributes.NightSight);
                                            totalreduction += 1;
                                        }
                                    }

                                }
                            }

                            Drainer.Hits += totalreduction * 5;
                            Drainer.FixedParticles(0x374A, 1, 30, 9503, EffectLayer.Waist);

                        }
                    }
                }

            }
                #endregion
            if (attacker is TeiravonMobile)
            {
                //Dwarven Axe
                if (attacker.Weapon is DwarvenAxe || attacker.Weapon is OrcMace || (((TeiravonMobile)attacker).IsDragoon() && ((TeiravonMobile)attacker).IsUndead() && ((TeiravonMobile)attacker).Shapeshifted))
                {
                    if (Utility.Random(100) < 8)
                    {
                        if (defender is BaseCreature)
                        {
                            BaseCreature m_Target = (BaseCreature)defender;

                            attacker.SendMessage("You deliver a stunning blow!");

                            m_Target.Freeze(TimeSpan.FromSeconds(6.0));
                            m_Target.Stam = (int)(m_Target.Stam * .5) - 3;

                            m_Target.FixedEffect(0x376A, 9, 32);
                            m_Target.PlaySound(0x204);
                        }
                        else if (defender is TeiravonMobile)
                        {
                            TeiravonMobile m_Target = (TeiravonMobile)defender;

                            attacker.SendMessage("You deliver a stunning blow!");
                            m_Target.SendMessage("The attack has stunned you!");

                            m_Target.Freeze(TimeSpan.FromSeconds(1.0));
                            m_Target.Stam = (int)(m_Target.Stam * .85) - 3;

                            m_Target.FixedEffect(0x376A, 9, 32);
                            m_Target.PlaySound(0x204);
                        }
                    }
                }

                //Lance shattering
                /*
                if (attacker.Mounted && attacker.Weapon is Lance)
                {
                    BaseWeapon m_Lance = (BaseWeapon)attacker.Weapon;
                    int breakchance;
                    if (m_Lance is HumanBandedLance)
                        breakchance = 1;
                    else
                        breakchance = 4;

                    if (Utility.Random(100) < breakchance)
                    {
                        m_Lance.Delete();
                        attacker.SendMessage("Your lance shatters on impact!");
                    }
                }
                */
                //Druid snakeform poison damage
                if (Spells.Third.BlessSpell.lloth.Contains(attacker))
                {
                    bool poisonattack = true;
                    Poison p = null;

                    if (attacker.BodyMod == 173)
                        p = Poison.Deadly;
                    if (attacker.BodyMod == 0x9D)
                        p = Poison.Greater;
                    if (attacker.BodyMod == 28)
                        p = Poison.Regular;

                    if (p != null && poisonattack && !defender.Poisoned)
                    {
                        if (Utility.RandomDouble() > 0.25)
                        {
                            attacker.SendMessage("You sink your venomous teeth into your target!");
                            defender.ApplyPoison(attacker, p);
                        }
                    }
                }

                if ((((TeiravonMobile)attacker).IsShifted() && ((TeiravonMobile)attacker).Shapeshifted))
                {
                    TeiravonMobile m_Shapeshifter = (TeiravonMobile)attacker;
                    bool poisonattack = false;
                    Poison p = null;

                    if (m_Shapeshifter.DruidFormGroup == 8)
                    {
                        p = Poison.Lesser;

                        if (Utility.RandomDouble() < m_Shapeshifter.PlayerLevel * .1)
                            p = PoisonImpl.IncreaseLevel(p);
                        if (Utility.RandomDouble() < m_Shapeshifter.PlayerLevel * .05)
                            p = PoisonImpl.IncreaseLevel(p);
                        if (Utility.RandomDouble() < m_Shapeshifter.PlayerLevel * .025)
                            p = PoisonImpl.IncreaseLevel(p);
                        if (Utility.RandomDouble() < m_Shapeshifter.PlayerLevel * .0125)
                            p = PoisonImpl.IncreaseLevel(p);

                        poisonattack = true;

                    }

                    if (p != null && poisonattack && !defender.Poisoned)
                    {
                        if (Utility.RandomDouble() > 0.85)
                        {
                            m_Shapeshifter.SendMessage("You sink your venomous teeth into your target!");
                            defender.ApplyPoison(m_Shapeshifter, p);
                        }
                    }
                }

                // Drow Dagger and Drow Hand Xbow poison attack

                if (attacker.Weapon is Drowdagger || attacker.Weapon is Drowhandbow || (((TeiravonMobile)attacker).IsAssassin() && ((TeiravonMobile)attacker).IsUndead() && ((TeiravonMobile)attacker).Shapeshifted))
                {
                    int poisonchance = 5 + ((int)(attacker.Skills.Poisoning.Value / 20));
                    if (Utility.RandomMinMax(1, 100) <= poisonchance)
                    {
                        Poison p = Poison.Lesser;
                        int advchk = Utility.RandomMinMax(1, 100);
                        if (advchk <= poisonchance)
                            p = PoisonImpl.IncreaseLevel(p);
                        if (advchk <= (int)(poisonchance / 5))
                            p = PoisonImpl.IncreaseLevel(p);
                        for (int i = 0; i < (int)(attacker.Skills.Poisoning.Value / 50); i++)
                            p = PoisonImpl.IncreaseLevel(p);

                        TeiravonMobile tplayer = (TeiravonMobile)attacker;
                        tplayer.SendMessage("You poison your victim!");
                        defender.ApplyPoison(attacker, p);
                    }
                }

                //Ki Strike Damage Boost

                if (this is Fists && ((TeiravonMobile)attacker).GetActiveFeats(TeiravonMobile.Feats.KiStrike))
                {
                    TeiravonMobile m_Monk = (TeiravonMobile)attacker;

                    int extradamage = (int)(5 + (m_Monk.Int/20) + (m_Monk.Mana / 10));

                    if (extradamage <= 0)
                        extradamage = 1;

                    if (defender is BaseCreature)
                    {
                        extradamage = (int)(extradamage * 2);
                    }

                    defender.Damage(extradamage, attacker);

                    if (extradamage > 0)
                    {
                        m_Monk.SendMessage("Ki burns your target for " + extradamage + " extra damage.");
                        m_Monk.Mana -= (int)(2 + (m_Monk.Mana * .05));
                    }


                    if (m_Monk.Mana < (2 + (m_Monk.Mana * .05)))
                    {
                        m_Monk.SetActiveFeats(TeiravonMobile.Feats.KiStrike, false);
                        m_Monk.SendMessage("Your Ki is too weak to stay concentrated.");
                    }


                }

                // Divine Might and Unholy Might damage boost

                if (((TeiravonMobile)attacker).IsCleric() && ((TeiravonMobile)attacker).GetActiveFeats(TeiravonMobile.Feats.DivineMight))
                {
                    bool evil = false;

                    if (defender is TeiravonMobile)
                    {
                        if (((TeiravonMobile)defender).IsEvil() || ((TeiravonMobile)defender).IsChaoticNeutral())
                            evil = true;
                    }

                    if (defender.Alive && (( evil || defender is BaseCreature  ) && Utility.RandomBool()))
                    {
                        TeiravonMobile m_DivineMighter = (TeiravonMobile)attacker;

                        int extradamage = 3 + (int)(m_DivineMighter.PlayerLevel / 2);

                        if (defender is BaseCreature)
                        {
                            extradamage = (int)(extradamage * 2);
                        }

                        defender.Damage(extradamage, attacker);
                        m_DivineMighter.SendMessage("The divine burns your target for " + extradamage + " extra damage.");
                    }
                }

                else if (((TeiravonMobile)attacker).IsDarkCleric() && ((TeiravonMobile)attacker).GetActiveFeats(TeiravonMobile.Feats.UnholyMight))
                {
                    bool lawful = false;

                    if (defender is TeiravonMobile)
                    {
                        if (((TeiravonMobile)defender).IsGood() || ((TeiravonMobile)defender).IsLawfulNeutral() )
                            lawful = true;
                    }

                    if (defender.Alive && ((defender is BaseCreature  || lawful )&& Utility.RandomBool()))
                    {

                        TeiravonMobile m_UnholyMighter = (TeiravonMobile)attacker;
                        int extradamage = 3 + (int)(m_UnholyMighter.PlayerLevel / 2);

                        if (defender is BaseCreature)
                        {
                            extradamage = (int)(extradamage * 2);
                        }

                        defender.Damage(extradamage, attacker);
                        m_UnholyMighter.SendMessage("The unholy might inflicts your target " + extradamage + " extra damage.");
                    }
                }

            }

            if (Spells.Third.BlessSpell.talathas.Contains(defender))
            {
                if (Utility.Random(5) > 2)
                {
                    AOS.Damage(attacker, defender, Utility.RandomMinMax(5, 10), 0, 100, 0, 0, 0);
                    Effects.SendTargetEffect(attacker, 0x3709, 15, 250, 2);
                }

            }

            AOS.ArmorIgnore = false;
            AOS.ArmorHalf = false;


            //ENDMOD: TEIRAVON customized attack/defend functions

            if (Core.AOS)
            {
                int lifeLeech = 0;//m_AosWeaponAttributes.HitLeechHits;
                int stamLeech = 0;//m_AosWeaponAttributes.HitLeechStam;
                int manaLeech = 0;//m_AosWeaponAttributes.HitLeechMana;

                if (m_AosWeaponAttributes.HitLeechHits > Utility.Random(100))
                    lifeLeech += 30; // HitLeechHits% chance to leech 30% of damage as hit points

                if (m_AosWeaponAttributes.HitLeechStam > Utility.Random(100))
                    stamLeech += 50; // HitLeechStam% chance to leech 100% of damage as stamina

                if (m_AosWeaponAttributes.HitLeechMana > Utility.Random(100))
                    manaLeech += 40; // HitLeechMana% chance to leech 40% of damage as mana

                if (m_Cursed)
                    lifeLeech += 50; // Additional 50% life leech for cursed weapons (necro spell)

                context = TransformationSpell.GetContext(attacker);

                if (context != null && context.Type == typeof(VampiricEmbraceSpell))
                    lifeLeech += 20; // Vampiric embrace gives an additional 20% life leech

                if (context != null && context.Type == typeof(WraithFormSpell))
                    manaLeech += (5 + (int)((15 * attacker.Skills.SpiritSpeak.Value) / 100)); // Wraith form gives an additional 5-20% mana leech

                if (attacker is TeiravonMobile)
                    if (((TeiravonMobile)attacker).IsBerserker() && ((TeiravonMobile)attacker).IsUndead() && ((TeiravonMobile)attacker).Shapeshifted)
                        lifeLeech += 10;

                if (lifeLeech != 0)
                    attacker.Hits += AOS.Scale(damageGiven, lifeLeech);

                if (stamLeech != 0)
                    attacker.Stam += AOS.Scale(damageGiven, stamLeech);

                if (manaLeech != 0)
                {
                    attacker.Mana += AOS.Scale(damageGiven, manaLeech);
                    defender.Mana -= AOS.Scale(damageGiven, manaLeech);
                }

                if (lifeLeech != 0 || stamLeech != 0 || manaLeech != 0)
                    attacker.PlaySound(0x44D);
            }

            if (m_MaxHits > 0 && ((MaxRange <= 1 && (defender is Slime || defender is ToxicElemental)) || Utility.Random(16) == 0)) // Stratics says 50% chance, seems more like 4%..
            {
                if (MaxRange <= 1 && (defender is Slime || defender is ToxicElemental))
                    attacker.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500263); // *Acid blood scars your weapon!*

                if (Core.AOS && m_AosWeaponAttributes.SelfRepair > Utility.Random(10))
                {
                    Hits += 2;
                }
                else
                {
                    if (m_Hits > 0)
                    {
                        --Hits;
                    }
                    else if (m_MaxHits > 1)
                    {
                        --MaxHits;

                        if (Parent is Mobile)
                            ((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                    }
                    else
                    {
                        Delete();
                    }
                }
            }

            if (attacker is VampireBatFamiliar)
            {
                BaseCreature bc = (BaseCreature)attacker;
                Mobile caster = bc.ControlMaster;

                if (caster == null)
                    caster = bc.SummonMaster;

                if (caster != null && caster.Map == bc.Map && caster.InRange(bc, 2))
                    caster.Hits += damage;
                else
                    bc.Hits += damage;
            }

            if (Core.AOS)
            {
                int physChance = m_AosWeaponAttributes.HitPhysicalArea;
                int fireChance = m_AosWeaponAttributes.HitFireArea;
                int coldChance = m_AosWeaponAttributes.HitColdArea;
                int poisChance = m_AosWeaponAttributes.HitPoisonArea;
                int nrgyChance = m_AosWeaponAttributes.HitEnergyArea;

                if (physChance != 0 && physChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x10E, 50, 100, 0, 0, 0, 0);

                if (fireChance != 0 && fireChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x11D, 1160, 0, 100, 0, 0, 0);

                if (coldChance != 0 && coldChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x0FC, 2100, 0, 0, 100, 0, 0);

                if (poisChance != 0 && poisChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x205, 1166, 0, 0, 0, 100, 0);

                if (nrgyChance != 0 && nrgyChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x1F1, 120, 0, 0, 0, 0, 100);

                int maChance = m_AosWeaponAttributes.HitMagicArrow;
                int harmChance = m_AosWeaponAttributes.HitHarm;
                int fireballChance = m_AosWeaponAttributes.HitFireball;
                int lightningChance = m_AosWeaponAttributes.HitLightning;
                int dispelChance = m_AosWeaponAttributes.HitDispel;

                if (maChance != 0 && maChance > Utility.Random(100))
                    DoMagicArrow(attacker, defender);

                if (harmChance != 0 && harmChance > Utility.Random(100))
                    DoHarm(attacker, defender);

                if (fireballChance != 0 && fireballChance > Utility.Random(100))
                    DoFireball(attacker, defender);

                if (lightningChance != 0 && lightningChance > Utility.Random(100))
                    DoLightning(attacker, defender);

                if (dispelChance != 0 && dispelChance > Utility.Random(100))
                    DoDispel(attacker, defender);

                int laChance = m_AosWeaponAttributes.HitLowerAttack;
                int ldChance = m_AosWeaponAttributes.HitLowerDefend;

                if (laChance != 0 && laChance > Utility.Random(100))
                    DoLowerAttack(attacker, defender);

                if (ldChance != 0 && ldChance > Utility.Random(100))
                    DoLowerDefense(attacker, defender);
            }

            if (attacker is BaseCreature)
                ((BaseCreature)attacker).OnGaveMeleeAttack(defender);

            if (defender is BaseCreature)
                ((BaseCreature)defender).OnGotMeleeAttack(attacker);

            if (a != null)
                a.OnHit(attacker, defender, damage);

            // hook for attachment OnWeaponHit method
            Server.Engines.XmlSpawner2.XmlAttach.OnWeaponHit(this, attacker, defender, damageGiven);

            if (attacker is TeiravonMobile)
            {
                TeiravonMobile m_Player = attacker as TeiravonMobile;

                if (m_Player.IsMonk() && this is Fists)
                {
                    m_Player.MonkAttack(defender, damageGiven);
                }

                if (m_Player.HasFeat(TeiravonMobile.Feats.EnchantedQuiver) && this is BaseRanged)
                {
                    m_Player.EnchantedShot(defender, damageGiven);
                }

                if (m_Player.BurningHands)
                {
                    IWeapon weapon = attacker.Weapon;
                    m_Player.BurningHands = false;
                    TimerHelper m_TimerHelper = new TimerHelper((int)m_Player.Serial);
                    m_TimerHelper.DoFeat = true;
                    m_TimerHelper.Feat = TeiravonMobile.Feats.Flurry;
                    m_TimerHelper.Duration = AbilityCoolDown.AtWill;
                    m_TimerHelper.Start();

                    m_Player.SetActiveFeats(TeiravonMobile.Feats.Flurry, true);
                    for (int i = 0; i < 3; ++i)
                    {
                        attacker.PlaySound(Utility.RandomList(0x533, 0x534, 0x535));
                        attacker.Stam -= 10;
                        attacker.NextCombatTime = DateTime.Now + weapon.OnSwing(attacker, defender);

                    }
                }
            }
        }
        public virtual double GetAosDamage(Mobile attacker, int min, int random, double div)
        {
            double scale = 1.0;

            scale += attacker.Skills[SkillName.Inscribe].Value * 0.001;

            if (attacker.Player)
            {
                scale += attacker.Int * 0.001;
                scale += AosAttributes.GetValue(attacker, AosAttribute.SpellDamage) * 0.01;
            }

            int baseDamage = min + (int)(attacker.Skills[SkillName.EvalInt].Value / div);

            double damage = Utility.RandomMinMax(baseDamage, baseDamage + random);

            return damage * scale;
        }

        public virtual bool IsBehind(Mobile attacker, Mobile defender)
        {
            Direction DefDirection = defender.Direction;
            Direction DefToAtt = defender.GetDirectionTo(attacker.Location.X, attacker.Location.Y);

            if (defender is TeiravonMobile && ((TeiravonMobile)defender).HasFeat(TeiravonMobile.Feats.UncannyDefense))
                return false;

            if (Math.Abs((int)DefDirection - (int)DefToAtt) > 2)
                return true;
            else
                return false;
        }

        public virtual void DoMagicArrow(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = GetAosDamage(attacker, 3, 1, 10.0);

            attacker.MovingParticles(defender, 0x36E4, 5, 0, false, true, 3006, 4006, 0);
            attacker.PlaySound(0x1E5);

            SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);
        }

        public virtual void DoHarm(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = GetAosDamage(attacker, 6, 3, 6.5);

            if (!defender.InRange(attacker, 2))
                damage *= 0.25; // 1/4 damage at > 2 tile range
            else if (!defender.InRange(attacker, 1))
                damage *= 0.50; // 1/2 damage at 2 tile range

            defender.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
            defender.PlaySound(0x0FC);

            SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 100, 0, 0);
        }

        public virtual void DoFireball(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = GetAosDamage(attacker, 6, 3, 5.5);

            attacker.MovingParticles(defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
            attacker.PlaySound(0x15E);

            SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);
        }

        public virtual void DoLightning(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = GetAosDamage(attacker, 6, 3, 5.0);

            defender.BoltEffect(0);

            SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 0, 0, 100);
        }

        public virtual void DoDispel(Mobile attacker, Mobile defender)
        {
            bool dispellable = false;

            if (defender is BaseCreature)
                dispellable = ((BaseCreature)defender).Summoned && !((BaseCreature)defender).IsAnimatedDead;

            if (!dispellable)
                return;

            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            Spells.Spell sp = new Spells.Sixth.DispelSpell(attacker, null);

            if (sp.CheckResisted(defender))
            {
                defender.FixedEffect(0x3779, 10, 20);
            }
            else
            {
                Effects.SendLocationParticles(EffectItem.Create(defender.Location, defender.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                Effects.PlaySound(defender, defender.Map, 0x201);

                defender.Delete();
            }
        }

        public virtual void DoLowerAttack(Mobile from, Mobile defender)
        {
            if (HitLower.ApplyAttack(defender))
            {
                defender.PlaySound(0x28E);
                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0xA, 3);
            }
        }

        public virtual void DoLowerDefense(Mobile from, Mobile defender)
        {
            if (HitLower.ApplyDefense(defender))
            {
                defender.PlaySound(0x28E);
                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x23, 3);
            }
        }

        public virtual void DoAreaAttack(Mobile from, Mobile defender, int sound, int hue, int phys, int fire, int cold, int pois, int nrgy)
        {
            Map map = from.Map;

            if (map == null)
                return;

            ArrayList list = new ArrayList();

            foreach (Mobile m in from.GetMobilesInRange(10))
            {
                if (from != m && defender != m && SpellHelper.ValidIndirectTarget(from, m) && from.CanBeHarmful(m, false) && from.InLOS(m))
                    list.Add(m);
            }

            if (list.Count == 0)
                return;

            Effects.PlaySound(from.Location, map, sound);

            // TODO: What is the damage calculation?

            for (int i = 0; i < list.Count; ++i)
            {
                Mobile m = (Mobile)list[i];

                double scalar = (11 - from.GetDistanceToSqrt(m)) / 10;

                if (scalar > 1.0)
                    scalar = 1.0;
                else if (scalar < 0.0)
                    continue;

                from.DoHarmful(m, true);
                m.FixedEffect(0x3779, 1, 15, hue, 0);
                AOS.Damage(m, from, (int)(GetBaseDamage(from) * scalar), phys, fire, cold, pois, nrgy);
            }
        }

        public virtual CheckSlayerResult CheckSlayers(Mobile attacker, Mobile defender)
        {
            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(atkWeapon.Slayer);

            if (atkSlayer != null && atkSlayer.Slays(defender))
                return CheckSlayerResult.Slayer;

            BaseWeapon defWeapon = defender.Weapon as BaseWeapon;
            SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defWeapon.Slayer);

            if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(attacker))
                return CheckSlayerResult.Opposition;

            return CheckSlayerResult.None;
        }

        public virtual void AddBlood(Mobile attacker, Mobile defender, int damage)
        {
            if (damage > 0)
            {
                new Blood().MoveToWorld(defender.Location, defender.Map);

                if (Utility.RandomBool())
                {
                    new Blood().MoveToWorld(new Point3D(
                        defender.X + Utility.RandomMinMax(-1, 1),
                        defender.Y + Utility.RandomMinMax(-1, 1),
                        defender.Z), defender.Map);
                }
            }

            /* if ( damage <= 2 )
                return;

            Direction d = defender.GetDirectionTo( attacker );

            int maxCount = damage / 15;

            if ( maxCount < 1 )
                maxCount = 1;
            else if ( maxCount > 4 )
                maxCount = 4;

            for( int i = 0; i < Utility.Random( 1, maxCount ); ++i )
            {
                int x = defender.X;
                int y = defender.Y;

                switch( d )
                {
                    case Direction.North:
                        x += Utility.Random( -1, 3 );
                        y += Utility.Random( 2 );
                        break;
                    case Direction.East:
                        y += Utility.Random( -1, 3 );
                        x += Utility.Random( -1, 2 );
                        break;
                    case Direction.West:
                        y += Utility.Random( -1, 3 );
                        x += Utility.Random( 2 );
                        break;
                    case Direction.South:
                        x += Utility.Random( -1, 3 );
                        y += Utility.Random( -1, 2 );
                        break;
                    case Direction.Up:
                        x += Utility.Random( 2 );
                        y += Utility.Random( 2 );
                        break;
                    case Direction.Down:
                        x += Utility.Random( -1, 2 );
                        y += Utility.Random( -1, 2 );
                        break;
                    case Direction.Left:
                        x += Utility.Random( 2 );
                        y += Utility.Random( -1, 2 );
                        break;
                    case Direction.Right:
                        x += Utility.Random( -1, 2 );
                        y += Utility.Random( 2 );
                        break;
                }

                new Blood().MoveToWorld( new Point3D( x, y, defender.Z ), defender.Map );
            }*/
        }

        public virtual void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy)
        {
            if (wielder is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)wielder;

                phys = bc.PhysicalDamage;
                fire = bc.FireDamage;
                cold = bc.ColdDamage;
                pois = bc.PoisonDamage;
                nrgy = bc.EnergyDamage;
            }
            else
            {
                CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

                if (resInfo != null)
                {
                    CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

                    if (attrInfo != null)
                    {
                        fire = attrInfo.WeaponFireDamage;
                        cold = attrInfo.WeaponColdDamage;
                        pois = attrInfo.WeaponPoisonDamage;
                        nrgy = attrInfo.WeaponEnergyDamage;
                        phys = 100 - fire - cold - pois - nrgy;
                        return;
                    }
                }

                phys = 100;
                fire = 0;
                cold = 0;
                pois = 0;
                nrgy = 0;
            }
        }

        public virtual void OnMiss(Mobile attacker, Mobile defender)
        {
            PlaySwingAnimation(attacker);
            attacker.PlaySound(GetMissAttackSound(attacker, defender));
            defender.PlaySound(GetMissDefendSound(attacker, defender));

            WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

            if (ability != null)
                ability.OnMiss(attacker, defender);
        }

        public virtual void GetBaseDamageRange(Mobile attacker, out int min, out int max)
        {
            TeiravonMobile tav;

            if (attacker is BaseCreature)
            {
                BaseCreature c = (BaseCreature)attacker;

                if (c.DamageMin >= 0)
                {
                    min = c.DamageMin;
                    max = c.DamageMax;
                    return;
                }

                if (this is Fists && !attacker.Body.IsHuman)
                {
                    min = attacker.Str / 28;
                    max = attacker.Str / 28;
                    return;
                }
            }

            else if (attacker is TeiravonMobile)
            {
                tav = (TeiravonMobile)attacker;

                if (tav.IsMonk() && this is Fists)
                {
                    //int slotlevel = tav.PlayerLevel / 4;

                    min = 6 + (tav.PlayerLevel / 4);
                    max = 6 + (tav.PlayerLevel / 2);
                    return;
                }
                else if (tav.IsMonk() && this is BaseStaff || this is Scimitar || this is Club  || this is CrescentBlade || this is DoubleBladedStaff )
                {
                    min = this.MinDamage + (tav.Str / 80);
                    max = this.MaxDamage + (tav.Str / 60);
                }
            }
            min = MinDamage;
            max = MaxDamage;
        }

        public virtual double GetBaseDamage(Mobile attacker)
        {
            int min, max;

            GetBaseDamageRange(attacker, out min, out max);

            return Utility.RandomMinMax(min, max);
        }

        public virtual double GetBonus(double value, double scalar, double threshold, double offset)
        {
            double bonus = value * scalar;

            if (value >= threshold)
                bonus += offset;

            return bonus / 100;
        }

        public virtual int GetHitChanceBonus()
        {
            if (!Core.AOS)
                return 0;

            int bonus = 0;

            switch (m_AccuracyLevel)
            {
                case WeaponAccuracyLevel.Accurate: bonus += 02; break;
                case WeaponAccuracyLevel.Surpassingly: bonus += 04; break;
                case WeaponAccuracyLevel.Eminently: bonus += 06; break;
                case WeaponAccuracyLevel.Exceedingly: bonus += 08; break;
                case WeaponAccuracyLevel.Supremely: bonus += 10; break;
            }

            return bonus;
        }

        public virtual int GetDamageBonus()
        {
            int bonus = VirtualDamageBonus;

            switch (m_Quality)
            {
                case WeaponQuality.Low: bonus -= 20; break;
                case WeaponQuality.Exceptional: bonus += 20; break;
            }

            switch (m_DamageLevel)
            {
                case WeaponDamageLevel.Ruin: bonus += 15; break;
                case WeaponDamageLevel.Might: bonus += 20; break;
                case WeaponDamageLevel.Force: bonus += 25; break;
                case WeaponDamageLevel.Power: bonus += 30; break;
                case WeaponDamageLevel.Vanq: bonus += 35; break;
            }

            return bonus;
        }

        public virtual void GetStatusDamage(Mobile from, out int min, out int max)
        {
            int baseMin, baseMax;

            GetBaseDamageRange(from, out baseMin, out baseMax);

            if (Core.AOS)
            {
                min = (int)ScaleDamageAOS(from, baseMin, false, false);
                max = (int)ScaleDamageAOS(from, baseMax, false, false);
            }
            else
            {
                min = (int)ScaleDamageOld(from, baseMin, false, false);
                max = (int)ScaleDamageOld(from, baseMax, false, false);
            }

            if (min < 1)
                min = 1;

            if (max < 1)
                max = 1;
        }

        public virtual double ScaleDamageAOS(Mobile attacker, double damage, bool checkSkills, bool checkAbility)
        {
            if (checkSkills)
            {
                attacker.CheckSkill(SkillName.Tactics, 0.0, 120.0); // Passively check tactics for gain
                attacker.CheckSkill(SkillName.Anatomy, 0.0, 120.0); // Passively check Anatomy for gain

                if (Type == WeaponType.Axe)
                    attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
            }

            double strengthBonus = GetBonus(attacker.Str, 0.250, 100.0, 5.00);
            double anatomyBonus = GetBonus(attacker.Skills[SkillName.Anatomy].Value, 0.500, 100.0, 5.00);
            double tacticsBonus = GetBonus(attacker.Skills[SkillName.Tactics].Value, 0.625, 100.0, 6.25);
            double lumberBonus = GetBonus(attacker.Skills[SkillName.Lumberjacking].Value, 0.200, 100.0, 10.00);
            double rottenBonus = 0.0;

            if (attacker is TeiravonMobile && ((TeiravonMobile)attacker).HasFeat(TeiravonMobile.Feats.RottenLuck))
                rottenBonus = GetBonus(attacker.Luck, 0.1, 500.0, 0.00);

            if (attacker is TeiravonMobile && ((TeiravonMobile)attacker).IsMonk() && this is BaseStaff || this is Scimitar || this is Club || this is CrescentBlade || this is DoubleBladedStaff)
                strengthBonus *= 2.0;

            if (Type != WeaponType.Axe)
                lumberBonus = 0.0;

            // Damage Increase = 100%
            int damageBonus = AosAttributes.GetValue(attacker, AosAttribute.WeaponDamage);
            if ( damageBonus > 150 )
            	damageBonus = 150;

            double totalBonus = strengthBonus + anatomyBonus + tacticsBonus + lumberBonus + rottenBonus + ((double)(GetDamageBonus() + damageBonus) / 100);

            if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
                totalBonus += 0.1;

            //STARTMOD: TEIRAVON Mounted Combat
            if (attacker is TeiravonMobile && attacker.Mounted)
            {
                TeiravonMobile m_Player = (TeiravonMobile)attacker;

                if (m_Player.HasFeat(TeiravonMobile.Feats.MountedCombat))
                {
                    double levelbonus = .30 + m_Player.PlayerLevel * .1;
                    double mountedbonus = levelbonus + 0.10;

                    //attacker.SendMessage( "Levelbonus is: {0}", levelbonus );

                    if (((BaseWeapon)m_Player.Weapon).AccuracySkill == SkillName.Fencing)
                        mountedbonus += .1;
                    if (m_Player.Weapon is Lance)
                        mountedbonus += .2;

                    if (m_Player.Mount is WarMount)
                        mountedbonus += .50;

                    if ((m_Player.Direction & Direction.Running) != 0)
                        mountedbonus *= 2;

                    totalBonus += mountedbonus;

                }
            }
            //ENDMOD: TEIRAVON Mounted Combat
            double BGH = 0.0;
            //STARDMOD: TEIRAVON Berserker Rage
            if (attacker is TeiravonMobile)
            {
                TeiravonMobile m_Player = attacker as TeiravonMobile;

                if (m_Player.HasFeat(TeiravonMobile.Feats.BerserkerRage))
                {
                    double HITSfact = (double)m_Player.Hits / (double)m_Player.HitsMax;

                    double rageDmg = ((1 - HITSfact) * m_Player.PlayerLevel / 4) ;
                    //rageDmg = ((m_Player.BAC / 5) * (m_Player.PlayerLevel / 4) * .01);
                    totalBonus += rageDmg;
                    /*
                    m_Player.BAC += 2;
                    if (m_Player.BAC >= 100)
                        m_Player.BAC = 100;

                    BeverageBottle.CheckHeaveTimer(m_Player);
                    */
                }

                if (((TeiravonMobile)attacker).IsFighter() && ((TeiravonMobile)attacker).IsUndead() && ((TeiravonMobile)attacker).Shapeshifted)
                {
                    totalBonus += ((m_Player.PlayerLevel / 2) * .035);
                }
                if (m_Player.HasFeat(TeiravonMobile.Feats.BigGameHunter))
                {
                    if (attacker.Combatant != null)
                        BGH += (Math.Log10(attacker.Combatant.HitsMax) - 2.0);
                    BGH *= (.6 + (m_Player.PlayerLevel * .02));
                    if (BGH < 0.05)
                        BGH = 0.0;
                }
            }
            //ENDMOD: TEIRAVON Berserker Rage
            if (Spells.Third.BlessSpell.gruumsh.Contains(attacker))
            {
                totalBonus += .75;
            }
            double discordanceScalar = 0.0;

            if (SkillHandlers.Discordance.GetScalar(attacker, ref discordanceScalar))
                totalBonus += discordanceScalar * 2;

            if (TransformationSpell.UnderTransformation(attacker, typeof(HorrificBeastSpell)))
                totalBonus += 0.75;

            damage += (damage * totalBonus);
            damage += (damage * BGH);

            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);
            /*
            if (attacker is TeiravonMobile)
            {
                if (((TeiravonMobile)attacker).ChargedMissileReady)
                    a = ((XmlWeaponAbility)XmlAttach.FindAttachmentOnMobile(attacker, typeof(XmlWeaponAbility), "flourish")).WeaponAbility;

                ((TeiravonMobile)attacker).ChargedMissileReady = false;
            }*/

            if (checkAbility && a != null)
            {
                if (attacker is TeiravonMobile && ((TeiravonMobile)attacker).ChargedMissileReady == true)
                    damage *= (a.DamageScalar - 0.1);
                else
                    damage *= a.DamageScalar;  
            }

            return damage;
        }

        public virtual int VirtualDamageBonus { get { return 0; } }

        public virtual int ComputeDamageAOS(Mobile attacker, Mobile defender)
        {
            return (int)ScaleDamageAOS(attacker, GetBaseDamage(attacker), true, true);
        }

        public virtual double ScaleDamageOld(Mobile attacker, double damage, bool checkSkills, bool checkAbility)
        {
            if (attacker == null)
                return damage;

            if (checkSkills)
            {
                attacker.CheckSkill(SkillName.Tactics, 0.0, 120.0); // Passively check tactics for gain
                attacker.CheckSkill(SkillName.Anatomy, 0.0, 120.0); // Passively check Anatomy for gain

                if (Type == WeaponType.Axe)
                    attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
            }

            /* Compute tactics modifier
             * :   0.0 = 50% loss
             * :  50.0 = unchanged
             * : 100.0 = 50% bonus
             */
            double tacticsBonus = (attacker.Skills[SkillName.Tactics].Value - 50.0) / 100.0;

            /* Compute strength modifier
             * : 1% bonus for every 5 strength
             */
            double strBonus = (attacker.Str / 5.0) / 100.0;

            /* Compute anatomy modifier
             * : 1% bonus for every 5 points of anatomy
             * : +10% bonus at Grandmaster or higher
             */
            double anatomyValue = attacker.Skills[SkillName.Anatomy].Value;
            double anatomyBonus = (anatomyValue / 5.0) / 100.0;

            if (anatomyValue >= 100.0)
                anatomyBonus += 0.1;

            /* Compute lumberjacking bonus
             * : 1% bonus for every 5 points of lumberjacking
             * : +10% bonus at Grandmaster or higher
             */
            double lumberBonus;

            if (Type == WeaponType.Axe)
            {
                double lumberValue = attacker.Skills[SkillName.Lumberjacking].Value;

                lumberBonus = (lumberValue / 5.0) / 100.0;

                if (lumberValue >= 100.0)
                    lumberBonus += 0.1;
            }
            else
            {
                lumberBonus = 0.0;
            }

            // New quality bonus:
            double qualityBonus = ((int)m_Quality - 1) * 0.2;

            // Apply bonuses
            damage += (damage * tacticsBonus) + (damage * strBonus) + (damage * anatomyBonus) + (damage * lumberBonus) + (damage * qualityBonus) + ((damage * VirtualDamageBonus) / 100);

            // Old quality bonus:
#if false
			/* Apply quality offset
			 * : Low         : -4
			 * : Regular     :  0
			 * : Exceptional : +4
			 */
			damage += ((int)m_Quality - 1) * 4.0;
#endif

            /* Apply damage level offset
			 * : Regular : 0
			 * : Ruin    : 1
			 * : Might   : 3
			 * : Force   : 5
			 * : Power   : 7
			 * : Vanq    : 9
			 */
            if (m_DamageLevel != WeaponDamageLevel.Regular)
                damage += (2.0 * (int)m_DamageLevel) - 1.0;

            // Halve the computed damage and return
            damage /= 2.0;

            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

            if (checkAbility && a != null)
                damage *= a.DamageScalar;

            return ScaleDamageByDurability((int)damage);
        }

        public virtual int ScaleDamageByDurability(int damage)
        {
            int scale = 100;

            if (m_MaxHits > 0 && m_Hits < m_MaxHits)
                scale = 50 + ((50 * m_Hits) / m_MaxHits);

            return AOS.Scale(damage, scale);
        }

        public virtual int ComputeDamage(Mobile attacker, Mobile defender)
        {
            if (Core.AOS)
                return ComputeDamageAOS(attacker, defender);

            return (int)ScaleDamageOld(attacker, GetBaseDamage(attacker), true, true);
        }

        public virtual void PlayHurtAnimation(Mobile from)
        {
            int action;
            int frames;

            switch (from.Body.Type)
            {
                case BodyType.Sea:
                case BodyType.Animal:
                    {
                        action = 7;
                        frames = 5;
                        break;
                    }
                case BodyType.Monster:
                    {
                        action = 10;
                        frames = 4;
                        break;
                    }
                case BodyType.Human:
                    {
                        action = 20;
                        frames = 5;
                        break;
                    }
                default: return;
            }

            if (from.Mounted)
                return;

            from.Animate(action, frames, 1, true, false, 0);
        }

        public virtual void PlaySwingAnimation(Mobile from)
        {
            int action;

            switch (from.Body.Type)
            {
                case BodyType.Sea:
                case BodyType.Animal:
                    {
                        action = Utility.Random(5, 2);
                        break;
                    }
                case BodyType.Monster:
                    {
                        switch (Animation)
                        {
                            default:
                            case WeaponAnimation.Wrestle:
                            case WeaponAnimation.Bash1H:
                            case WeaponAnimation.Pierce1H:
                            case WeaponAnimation.Slash1H:
                            case WeaponAnimation.Bash2H:
                            case WeaponAnimation.Pierce2H:
                            case WeaponAnimation.Slash2H: action = Utility.Random(4, 3); break;
                            case WeaponAnimation.ShootBow: return; // 7
                            case WeaponAnimation.ShootXBow: return; // 8
                        }

                        break;
                    }
                case BodyType.Human:
                    {
                        if (!from.Mounted)
                        {
                            action = (int)Animation;
                        }
                        else
                        {
                            switch (Animation)
                            {
                                default:
                                case WeaponAnimation.Wrestle:
                                case WeaponAnimation.Bash1H:
                                case WeaponAnimation.Pierce1H:
                                case WeaponAnimation.Slash1H: action = 26; break;
                                case WeaponAnimation.Bash2H:
                                case WeaponAnimation.Pierce2H:
                                case WeaponAnimation.Slash2H: action = 29; break;
                                case WeaponAnimation.ShootBow: action = 27; break;
                                case WeaponAnimation.ShootXBow: action = 28; break;
                            }
                        }

                        break;
                    }
                default: return;
            }

            from.Animate(action, 7, 1, true, false, 0);
        }

        private static void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
        {
            if (setIf)
                flags |= toSet;
        }

        private static bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
        {
            return ((flags & toGet) != 0);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)9); // version

            SaveFlag flags = SaveFlag.None;

            SetSaveFlag(ref flags, SaveFlag.DamageLevel, m_DamageLevel != WeaponDamageLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.AccuracyLevel, m_AccuracyLevel != WeaponAccuracyLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.DurabilityLevel, m_DurabilityLevel != WeaponDurabilityLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.Quality, m_Quality != WeaponQuality.Regular);
            SetSaveFlag(ref flags, SaveFlag.Hits, m_Hits != 0);
            SetSaveFlag(ref flags, SaveFlag.MaxHits, m_MaxHits != 0);
            SetSaveFlag(ref flags, SaveFlag.Slayer, m_Slayer != SlayerName.None);
            SetSaveFlag(ref flags, SaveFlag.Poison, m_Poison != null);
            SetSaveFlag(ref flags, SaveFlag.PoisonCharges, m_PoisonCharges != 0);
            SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);
            SetSaveFlag(ref flags, SaveFlag.Identified, m_Identified != false);
            SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);
            SetSaveFlag(ref flags, SaveFlag.DexReq, m_DexReq != -1);
            SetSaveFlag(ref flags, SaveFlag.IntReq, m_IntReq != -1);
            SetSaveFlag(ref flags, SaveFlag.MinDamage, m_MinDamage != -1);
            SetSaveFlag(ref flags, SaveFlag.MaxDamage, m_MaxDamage != -1);
            SetSaveFlag(ref flags, SaveFlag.HitSound, m_HitSound != -1);
            SetSaveFlag(ref flags, SaveFlag.MissSound, m_MissSound != -1);
            SetSaveFlag(ref flags, SaveFlag.Speed, m_Speed != -1);
            SetSaveFlag(ref flags, SaveFlag.MaxRange, m_MaxRange != -1);
            SetSaveFlag(ref flags, SaveFlag.Skill, m_Skill != (SkillName)(-1));
            SetSaveFlag(ref flags, SaveFlag.Type, m_Type != (WeaponType)(-1));
            SetSaveFlag(ref flags, SaveFlag.Animation, m_Animation != (WeaponAnimation)(-1));
            SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != CraftResource.Iron);
            SetSaveFlag(ref flags, SaveFlag.xAttributes, !m_AosAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !m_AosWeaponAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, m_PlayerConstructed);
            SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);

            writer.Write((int)flags);

            if (GetSaveFlag(flags, SaveFlag.DamageLevel))
                writer.Write((int)m_DamageLevel);

            if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
                writer.Write((int)m_AccuracyLevel);

            if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
                writer.Write((int)m_DurabilityLevel);

            if (GetSaveFlag(flags, SaveFlag.Quality))
                writer.Write((int)m_Quality);

            if (GetSaveFlag(flags, SaveFlag.Hits))
                writer.Write((int)m_Hits);

            if (GetSaveFlag(flags, SaveFlag.MaxHits))
                writer.Write((int)m_MaxHits);

            if (GetSaveFlag(flags, SaveFlag.Slayer))
                writer.Write((int)m_Slayer);

            if (GetSaveFlag(flags, SaveFlag.Poison))
                Poison.Serialize(m_Poison, writer);

            if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
                writer.Write((int)m_PoisonCharges);

            if (GetSaveFlag(flags, SaveFlag.Crafter))
                writer.Write((Mobile)m_Crafter);

            if (GetSaveFlag(flags, SaveFlag.StrReq))
                writer.Write((int)m_StrReq);

            if (GetSaveFlag(flags, SaveFlag.DexReq))
                writer.Write((int)m_DexReq);

            if (GetSaveFlag(flags, SaveFlag.IntReq))
                writer.Write((int)m_IntReq);

            if (GetSaveFlag(flags, SaveFlag.MinDamage))
                writer.Write((int)m_MinDamage);

            if (GetSaveFlag(flags, SaveFlag.MaxDamage))
                writer.Write((int)m_MaxDamage);

            if (GetSaveFlag(flags, SaveFlag.HitSound))
                writer.Write((int)m_HitSound);

            if (GetSaveFlag(flags, SaveFlag.MissSound))
                writer.Write((int)m_MissSound);

            if (GetSaveFlag(flags, SaveFlag.Speed))
                writer.Write((int)m_Speed);

            if (GetSaveFlag(flags, SaveFlag.MaxRange))
                writer.Write((int)m_MaxRange);

            if (GetSaveFlag(flags, SaveFlag.Skill))
                writer.Write((int)m_Skill);

            if (GetSaveFlag(flags, SaveFlag.Type))
                writer.Write((int)m_Type);

            if (GetSaveFlag(flags, SaveFlag.Animation))
                writer.Write((int)m_Animation);

            if (GetSaveFlag(flags, SaveFlag.Resource))
                writer.Write((int)m_Resource);

            if (GetSaveFlag(flags, SaveFlag.xAttributes))
                m_AosAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                m_AosWeaponAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                m_AosSkillBonuses.Serialize(writer);
        }

        [Flags]
        private enum SaveFlag
        {
            None = 0x00000000,
            DamageLevel = 0x00000001,
            AccuracyLevel = 0x00000002,
            DurabilityLevel = 0x00000004,
            Quality = 0x00000008,
            Hits = 0x00000010,
            MaxHits = 0x00000020,
            Slayer = 0x00000040,
            Poison = 0x00000080,
            PoisonCharges = 0x00000100,
            Crafter = 0x00000200,
            Identified = 0x00000400,
            StrReq = 0x00000800,
            DexReq = 0x00001000,
            IntReq = 0x00002000,
            MinDamage = 0x00004000,
            MaxDamage = 0x00008000,
            HitSound = 0x00010000,
            MissSound = 0x00020000,
            Speed = 0x00040000,
            MaxRange = 0x00080000,
            Skill = 0x00100000,
            Type = 0x00200000,
            Animation = 0x00400000,
            Resource = 0x00800000,
            xAttributes = 0x01000000,
            xWeaponAttributes = 0x02000000,
            PlayerConstructed = 0x04000000,
            SkillBonuses = 0x08000000
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 9:
                case 8:
                case 7:
                case 6:
                case 5:
                    {
                        SaveFlag flags = (SaveFlag)reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.DamageLevel))
                        {
                            m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();

                            if (m_DamageLevel > WeaponDamageLevel.Vanq)
                                m_DamageLevel = WeaponDamageLevel.Ruin;
                        }

                        if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
                        {
                            m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();

                            if (m_AccuracyLevel > WeaponAccuracyLevel.Supremely)
                                m_AccuracyLevel = WeaponAccuracyLevel.Accurate;
                        }

                        if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
                        {
                            m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();

                            if (m_DurabilityLevel > WeaponDurabilityLevel.Indestructible)
                                m_DurabilityLevel = WeaponDurabilityLevel.Durable;
                        }

                        if (GetSaveFlag(flags, SaveFlag.Quality))
                            m_Quality = (WeaponQuality)reader.ReadInt();
                        else
                            m_Quality = WeaponQuality.Regular;

                        if (GetSaveFlag(flags, SaveFlag.Hits))
                            m_Hits = reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.MaxHits))
                            m_MaxHits = reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.Slayer))
                            m_Slayer = (SlayerName)reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.Poison))
                            m_Poison = Poison.Deserialize(reader);

                        if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
                            m_PoisonCharges = reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.Crafter))
                            m_Crafter = reader.ReadMobile();

                        if (GetSaveFlag(flags, SaveFlag.Identified))
                            m_Identified = (version >= 6 || reader.ReadBool());

                        if (GetSaveFlag(flags, SaveFlag.StrReq))
                            m_StrReq = reader.ReadInt();
                        else
                            m_StrReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.DexReq))
                            m_DexReq = reader.ReadInt();
                        else
                            m_DexReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.IntReq))
                            m_IntReq = reader.ReadInt();
                        else
                            m_IntReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.MinDamage))
                            m_MinDamage = reader.ReadInt();
                        else
                            m_MinDamage = -1;

                        if (GetSaveFlag(flags, SaveFlag.MaxDamage))
                            m_MaxDamage = reader.ReadInt();
                        else
                            m_MaxDamage = -1;

                        if (GetSaveFlag(flags, SaveFlag.HitSound))
                            m_HitSound = reader.ReadInt();
                        else
                            m_HitSound = -1;

                        if (GetSaveFlag(flags, SaveFlag.MissSound))
                            m_MissSound = reader.ReadInt();
                        else
                            m_MissSound = -1;

                        if (GetSaveFlag(flags, SaveFlag.Speed))
                            m_Speed = reader.ReadInt();
                        else
                            m_Speed = -1;

                        if (GetSaveFlag(flags, SaveFlag.MaxRange))
                            m_MaxRange = reader.ReadInt();
                        else
                            m_MaxRange = -1;

                        if (GetSaveFlag(flags, SaveFlag.Skill))
                            m_Skill = (SkillName)reader.ReadInt();
                        else
                            m_Skill = (SkillName)(-1);

                        if (GetSaveFlag(flags, SaveFlag.Type))
                            m_Type = (WeaponType)reader.ReadInt();
                        else
                            m_Type = (WeaponType)(-1);

                        if (GetSaveFlag(flags, SaveFlag.Animation))
                            m_Animation = (WeaponAnimation)reader.ReadInt();
                        else
                            m_Animation = (WeaponAnimation)(-1);

                        if (GetSaveFlag(flags, SaveFlag.Resource))
                            m_Resource = (CraftResource)reader.ReadInt();
                        else
                            m_Resource = CraftResource.Iron;

                        if (GetSaveFlag(flags, SaveFlag.xAttributes))
                            m_AosAttributes = new AosAttributes(this, reader);
                        else
                            m_AosAttributes = new AosAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                            m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
                        else
                            m_AosWeaponAttributes = new AosWeaponAttributes(this);

                        if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile)
                        {
                            m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                            ((Mobile)Parent).AddSkillMod(m_SkillMod);
                        }

                        if (version < 7 && m_AosWeaponAttributes.MageWeapon != 0)
                            m_AosWeaponAttributes.MageWeapon = 30 - m_AosWeaponAttributes.MageWeapon;

                        if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30 && Parent is Mobile)
                        {
                            m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
                            ((Mobile)Parent).AddSkillMod(m_MageMod);
                        }

                        if (GetSaveFlag(flags, SaveFlag.PlayerConstructed))
                            m_PlayerConstructed = true;

                        if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                            m_AosSkillBonuses = new AosSkillBonuses(this, reader);

                        break;
                    }
                case 4:
                    {
                        m_Slayer = (SlayerName)reader.ReadInt();

                        goto case 3;
                    }
                case 3:
                    {
                        m_StrReq = reader.ReadInt();
                        m_DexReq = reader.ReadInt();
                        m_IntReq = reader.ReadInt();

                        goto case 2;
                    }
                case 2:
                    {
                        m_Identified = reader.ReadBool();

                        goto case 1;
                    }
                case 1:
                    {
                        m_MaxRange = reader.ReadInt();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version == 0)
                            m_MaxRange = 1; // default

                        if (version < 9)
                        {
                            if (m_Resource == CraftResource.Bloodrock && (Attributes.WeaponDamage >= 20))
                            {
                                Attributes.WeaponDamage -= 20;
                            }

                        }

                        if (version < 5)
                        {
                            m_Resource = CraftResource.Iron;
                            m_AosAttributes = new AosAttributes(this);
                            m_AosWeaponAttributes = new AosWeaponAttributes(this);
                        }

                        m_MinDamage = reader.ReadInt();
                        m_MaxDamage = reader.ReadInt();

                        m_Speed = reader.ReadInt();

                        m_HitSound = reader.ReadInt();
                        m_MissSound = reader.ReadInt();

                        m_Skill = (SkillName)reader.ReadInt();
                        m_Type = (WeaponType)reader.ReadInt();
                        m_Animation = (WeaponAnimation)reader.ReadInt();
                        m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();
                        m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();
                        m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();
                        m_Quality = (WeaponQuality)reader.ReadInt();

                        m_Crafter = reader.ReadMobile();

                        m_Poison = Poison.Deserialize(reader);
                        m_PoisonCharges = reader.ReadInt();

                        if (m_StrReq == OldStrengthReq)
                            m_StrReq = -1;

                        if (m_DexReq == OldDexterityReq)
                            m_DexReq = -1;

                        if (m_IntReq == OldIntelligenceReq)
                            m_IntReq = -1;

                        if (m_MinDamage == OldMinDamage)
                            m_MinDamage = -1;

                        if (m_MaxDamage == OldMaxDamage)
                            m_MaxDamage = -1;

                        if (m_HitSound == OldHitSound)
                            m_HitSound = -1;

                        if (m_MissSound == OldMissSound)
                            m_MissSound = -1;

                        if (m_Speed == OldSpeed)
                            m_Speed = -1;

                        if (m_MaxRange == OldMaxRange)
                            m_MaxRange = -1;

                        if (m_Skill == OldSkill)
                            m_Skill = (SkillName)(-1);

                        if (m_Type == OldType)
                            m_Type = (WeaponType)(-1);

                        if (m_Animation == OldAnimation)
                            m_Animation = (WeaponAnimation)(-1);

                        if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile)
                        {
                            m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                            ((Mobile)Parent).AddSkillMod(m_SkillMod);
                        }

                        break;
                    }
            }

            if (m_AosSkillBonuses == null)
                m_AosSkillBonuses = new AosSkillBonuses(this);

            if (Core.AOS && Parent is Mobile)
                m_AosSkillBonuses.AddTo((Mobile)Parent);

            int strBonus = m_AosAttributes.BonusStr;
            int dexBonus = m_AosAttributes.BonusDex;
            int intBonus = m_AosAttributes.BonusInt;

            if (this.Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = (Mobile)this.Parent;

                string modName = this.Serial.ToString();

                if (strBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            if (Parent is Mobile)
                ((Mobile)Parent).CheckStatTimers();

            if (m_Hits <= 0 && m_MaxHits <= 0)
            {
                m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);
            }

            if (version < 6)
                m_PlayerConstructed = true; // we don't know, so, assume it's crafted

            if (Parent is TeiravonMobile)
            {
                TeiravonMobile m_parent = (TeiravonMobile)Parent;

                if ((m_parent.IsElf()) && (this is Longsword || this is ElvenLongsword))
                {
                    m_Skilmod = new DefaultSkillMod(SkillName.Swords, true, 10.0);
                    ((Mobile)Parent).AddSkillMod(m_Skilmod);
                }

                if (m_parent.IsKensai() && this is Katana)
                {
                    int kensaikatbonus = m_parent.PlayerLevel - 5;

                    if (kensaikatbonus > 0)
                    {
                        m_Skilmod = new DefaultSkillMod(SkillName.Swords, true, kensaikatbonus);
                        ((Mobile)Parent).AddSkillMod(m_Skilmod);
                    }
                }

                if (m_parent.HasFeat(TeiravonMobile.Feats.WeaponMaster))
                {
                    SkillName wpnskill = SkillName.Alchemy;
                    int skilladj = m_parent.PlayerLevel - 10;

                    if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Axe && this.DefType == WeaponType.Axe)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Slashing && this.DefType == WeaponType.Slashing)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Staff && this.DefType == WeaponType.Staff)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Bashing && this.DefType == WeaponType.Bashing)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Piercing && this.DefType == WeaponType.Piercing)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Polearm && this.DefType == WeaponType.Polearm)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Ranged && this.DefType == WeaponType.Ranged)
                        wpnskill = this.Skill;
                    else if (m_parent.MasterWeapon == TeiravonMobile.MasterWeapons.Thrown && this.DefType == WeaponType.Thrown)
                        wpnskill = this.Skill;

                    if (wpnskill != SkillName.Alchemy)
                    {
                        m_Skilmod = new DefaultSkillMod(wpnskill, true, skilladj);
                        ((Mobile)m_parent).AddSkillMod(m_Skilmod);
                        m_Skilmod2 = new DefaultSkillMod(SkillName.Tactics, true, skilladj);
                        ((Mobile)m_parent).AddSkillMod(m_Skilmod2);
                    }

                }


            }

        }

        public BaseWeapon(int itemID)
            : base(itemID)
        {
            Layer = (Layer)ItemData.Quality;

            m_Quality = WeaponQuality.Regular;
            m_StrReq = -1;
            m_DexReq = -1;
            m_IntReq = -1;
            m_MinDamage = -1;
            m_MaxDamage = -1;
            m_HitSound = -1;
            m_MissSound = -1;
            m_Speed = -1;
            m_MaxRange = -1;
            m_Skill = (SkillName)(-1);
            m_Type = (WeaponType)(-1);
            m_Animation = (WeaponAnimation)(-1);

            m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);

            m_Resource = CraftResource.Iron;

            m_AosAttributes = new AosAttributes(this);
            m_AosWeaponAttributes = new AosWeaponAttributes(this);
            m_AosSkillBonuses = new AosSkillBonuses(this);
        }

        public BaseWeapon(Serial serial)
            : base(serial)
        {
        }

        private string GetNameString()
        {
            string name = this.Name;

            if (name == null)
                name = String.Format("#{0}", LabelNumber);

            return name;
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get { return base.Hue; }
            set { base.Hue = value; InvalidateProperties(); }
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            string oreType = "";

            if (Hue == 0)
            {
                oreType = "";
            }
            else
            {
                switch (m_Resource)
                {
                    case CraftResource.DullCopper: oreType = "dull copper"; break; // dull copper
                    case CraftResource.ShadowIron: oreType = "shadow iron"; break; // shadow iron
                    case CraftResource.Copper: oreType = "copper"; break; // copper
                    case CraftResource.Bronze: oreType = "bronze"; break; // bronze
                    case CraftResource.Gold: oreType = "gold"; break; // golden
                    case CraftResource.Agapite: oreType = "agapite"; break; // agapite
                    case CraftResource.Verite: oreType = "verite"; break; // verite
                    case CraftResource.Valorite: oreType = "valorite"; break; // valorite
                    case CraftResource.Mithril: oreType = "mithril"; break; // mithril
                    case CraftResource.Bloodrock: oreType = "bloodrock"; break; // bloodrock
                    case CraftResource.Steel: oreType = "steel"; break; // steel
                    case CraftResource.Adamantite: oreType = "adamantite"; break; // adamantite
                    case CraftResource.Ithilmar: oreType = "ithilmar"; break; // ithilmar
                    case CraftResource.Silver: oreType = "silver"; break; // Silver
                    case CraftResource.Blackrock: oreType = "blackrock"; break;
                    case CraftResource.SpinedLeather: oreType = "spined leather"; break; // spined
                    case CraftResource.HornedLeather: oreType = "horned leather"; break; // horned
                    case CraftResource.BarbedLeather: oreType = "barbed leather"; break; // barbed
                    case CraftResource.RedScales: oreType = "red scales"; break; // red
                    case CraftResource.YellowScales: oreType = "yellow scales"; break; // yellow
                    case CraftResource.BlackScales: oreType = "black scales"; break; // black
                    case CraftResource.GreenScales: oreType = "green scales"; break; // green
                    case CraftResource.WhiteScales: oreType = "white scales"; break; // white
                    case CraftResource.BlueScales: oreType = "blue scales"; break; // blue
                    case CraftResource.Oak: oreType = "Oak"; break;
                    case CraftResource.Pine: oreType = "Pine"; break;
                    case CraftResource.Redwood: oreType = "Redwood"; break;
                    case CraftResource.WhitePine: oreType = "White Pine"; break;
                    case CraftResource.Ashwood: oreType = "Ashwood"; break;
                    case CraftResource.SilverBirch: oreType = "Silver Birch"; break;
                    case CraftResource.Yew: oreType = "Yew"; break;
                    case CraftResource.BlackOak: oreType = "Black Oak"; break;
                    case CraftResource.Frost: oreType = "frosty"; break; // Frost
                    case CraftResource.Ice: oreType = "icy"; break; // Ice
                    case CraftResource.Glacial: oreType = "glacial"; break; // Glacial Ice
                    case CraftResource.Skazz: oreType = "skazz"; break; // Gobo Skazz
                    case CraftResource.Electrum: oreType = "electrum"; break; // Gnomish Electrum
                    default: oreType = ""; break;
                }
            }

            //			if ( oreType != "" )
            list.Add(1053099, "{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
            //			else if ( oreName != null )
            //				list.Add( 1053099, "{0}\t{1}", oreName, GetNameString() ); // ~1_oretype~ ~2_armortype~
            //			else if ( Name == null )
            //				list.Add( LabelNumber );
            //			else
            //				list.Add( Name );
        }

        public override bool AllowEquipedCast(Mobile from)
        {
            if (base.AllowEquipedCast(from))
                return true;

            return (m_AosAttributes.SpellChanneling != 0);
        }

        public virtual int ArtifactRarity
        {
            get { return 0; }
        }

        public virtual int GetLuckBonus()
        {
            CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

            if (resInfo == null)
                return 0;

            CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

            if (attrInfo == null)
                return 0;

            return attrInfo.WeaponLuck;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_Crafter != null)
                list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~

            #region Factions
            if (m_FactionState != null)
                list.Add(1041350); // faction item
            #endregion

            if (m_AosSkillBonuses != null)
                m_AosSkillBonuses.GetProperties(list);

            if (m_Quality == WeaponQuality.Exceptional)
                list.Add(1060636); // exceptional


            if (ArtifactRarity > 0)
                list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~

            if (this is IUsesRemaining && ((IUsesRemaining)this).ShowUsesRemaining)
                list.Add(1060584, ((IUsesRemaining)this).UsesRemaining.ToString()); // uses remaining: ~1_val~

            if (m_Poison != null && m_PoisonCharges > 0)
                list.Add(1062412 + m_Poison.Level, m_PoisonCharges.ToString());

            if (m_Slayer != SlayerName.None)
                list.Add(SlayerGroup.GetEntryByName(m_Slayer).Title);


            base.AddResistanceProperties(list);

            int prop;

            if ((prop = m_AosWeaponAttributes.UseBestSkill) != 0)
                list.Add(1060400); // use best weapon skill

            if ((prop = (GetDamageBonus() + m_AosAttributes.WeaponDamage)) != 0)
                list.Add(1060401, prop.ToString()); // damage increase ~1_val~%

            if ((prop = m_AosAttributes.DefendChance) != 0)
                list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%

            if ((prop = m_AosAttributes.BonusDex) != 0)
                list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~

            if ((prop = m_AosAttributes.EnhancePotions) != 0)
                list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%

            if ((prop = m_AosAttributes.CastRecovery) != 0)
                list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~

            if ((prop = m_AosAttributes.CastSpeed) != 0)
                list.Add(1060413, prop.ToString()); // faster casting ~1_val~

            if ((prop = (GetHitChanceBonus() + m_AosAttributes.AttackChance)) != 0)
                list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitColdArea) != 0)
                list.Add(1060416, prop.ToString()); // hit cold area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitDispel) != 0)
                list.Add(1060417, prop.ToString()); // hit dispel ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitEnergyArea) != 0)
                list.Add(1060418, prop.ToString()); // hit energy area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitFireArea) != 0)
                list.Add(1060419, prop.ToString()); // hit fire area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitFireball) != 0)
                list.Add(1060420, prop.ToString()); // hit fireball ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitHarm) != 0)
                list.Add(1060421, prop.ToString()); // hit harm ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLeechHits) != 0)
                list.Add(1060422, prop.ToString()); // hit life leech ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLightning) != 0)
                list.Add(1060423, prop.ToString()); // hit lightning ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLowerAttack) != 0)
                list.Add(1060424, prop.ToString()); // hit lower attack ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLowerDefend) != 0)
                list.Add(1060425, prop.ToString()); // hit lower defense ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitMagicArrow) != 0)
                list.Add(1060426, prop.ToString()); // hit magic arrow ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLeechMana) != 0)
                list.Add(1060427, prop.ToString()); // hit mana leech ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitPhysicalArea) != 0)
                list.Add(1060428, prop.ToString()); // hit physical area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitPoisonArea) != 0)
                list.Add(1060429, prop.ToString()); // hit poison area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLeechStam) != 0)
                list.Add(1060430, prop.ToString()); // hit stamina leech ~1_val~%

            if ((prop = m_AosAttributes.BonusHits) != 0)
                list.Add(1060431, prop.ToString()); // hit point increase ~1_val~

            if ((prop = m_AosAttributes.BonusInt) != 0)
                list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~

            if ((prop = m_AosAttributes.LowerManaCost) != 0)
                list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%

            if ((prop = m_AosAttributes.LowerRegCost) != 0)
                list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%

            if ((prop = GetLowerStatReq()) != 0)
                list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%

            if ((prop = (GetLuckBonus() + m_AosAttributes.Luck)) != 0)
                list.Add(1060436, prop.ToString()); // luck ~1_val~

            if ((prop = m_AosWeaponAttributes.MageWeapon) != 0)
                list.Add(3010064, (30 - prop).ToString()); // mage weapon -~1_val~ skill

            if ((prop = m_AosAttributes.BonusMana) != 0)
                list.Add(1060439, prop.ToString()); // mana increase ~1_val~

            if ((prop = m_AosAttributes.RegenMana) != 0)
                list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~

            if ((prop = m_AosAttributes.NightSight) != 0)
                list.Add(1060441); // night sight

            if ((prop = m_AosAttributes.ReflectPhysical) != 0)
                list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%

            if ((prop = m_AosAttributes.RegenStam) != 0)
                list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~

            if ((prop = m_AosAttributes.RegenHits) != 0)
                list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~

            if ((prop = m_AosWeaponAttributes.SelfRepair) != 0)
                list.Add(1060450, prop.ToString()); // self repair ~1_val~

            if ((prop = m_AosAttributes.SpellChanneling) != 0)
                list.Add(1060482); // spell channeling

            if ((prop = m_AosAttributes.SpellDamage) != 0)
                list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%

            if ((prop = m_AosAttributes.BonusStam) != 0)
                list.Add(1060484, prop.ToString()); // stamina increase ~1_val~

            if ((prop = m_AosAttributes.BonusStr) != 0)
                list.Add(1060485, prop.ToString()); // strength bonus ~1_val~

            if ((prop = m_AosAttributes.WeaponSpeed) != 0)
                list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%

            int phys, fire, cold, pois, nrgy;

            GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy);

            if (phys != 0)
                list.Add(1060403, phys.ToString()); // physical damage ~1_val~%

            if (fire != 0)
                list.Add(1060405, fire.ToString()); // fire damage ~1_val~%

            if (cold != 0)
                list.Add(1060404, cold.ToString()); // cold damage ~1_val~%

            if (pois != 0)
                list.Add(1060406, pois.ToString()); // poison damage ~1_val~%

            if (nrgy != 0)
                list.Add(1060407, nrgy.ToString()); // energy damage ~1_val~%

            list.Add(1061168, "{0}\t{1}", MinDamage.ToString(), MaxDamage.ToString()); // weapon damage ~1_val~ - ~2_val~
            list.Add(1061167, Speed.ToString()); // weapon speed ~1_val~

            if (MaxRange > 1)
                list.Add(1061169, MaxRange.ToString()); // range ~1_val~

            int strReq = AOS.Scale(StrRequirement, 100 - GetLowerStatReq());

            if (strReq > 0)
                list.Add(1061170, strReq.ToString()); // strength requirement ~1_val~

            if (Layer == Layer.TwoHanded)
                list.Add(1061171); // two-handed weapon
            else
                list.Add(1061824); // one-handed weapon

            if (m_AosWeaponAttributes.UseBestSkill == 0)
            {
                switch (Skill)
                {
                    case SkillName.Swords: list.Add(1061172); break; // skill required: swordsmanship
                    case SkillName.Macing: list.Add(1061173); break; // skill required: mace fighting
                    case SkillName.Fencing: list.Add(1061174); break; // skill required: fencing
                    case SkillName.Archery: list.Add(1061175); break; // skill required: archery
                }
            }

            if (m_Hits >= 0 && m_MaxHits > 0)
                list.Add(1060639, "{0}\t{1}", m_Hits, m_MaxHits); // durability ~1_val~ / ~2_val~
        }

        public override void OnSingleClick(Mobile from)
        {
            ArrayList attrs = new ArrayList();

            if (DisplayLootType)
            {
                if (LootType == LootType.Blessed)
                    attrs.Add(new EquipInfoAttribute(1038021)); // blessed
                else if (LootType == LootType.Cursed)
                    attrs.Add(new EquipInfoAttribute(1049643)); // cursed
            }

            #region Factions
            if (m_FactionState != null)
                attrs.Add(new EquipInfoAttribute(1041350)); // faction item
            #endregion

            if (m_Quality == WeaponQuality.Exceptional)
                attrs.Add(new EquipInfoAttribute(1018305 - (int)m_Quality));

            if (m_Identified)
            {
                if (m_Slayer != SlayerName.None)
                    attrs.Add(new EquipInfoAttribute(SlayerGroup.GetEntryByName(m_Slayer).Title));

                if (m_DurabilityLevel != WeaponDurabilityLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038000 + (int)m_DurabilityLevel));

                if (m_DamageLevel != WeaponDamageLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038015 + (int)m_DamageLevel));

                if (m_AccuracyLevel != WeaponAccuracyLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038010 + (int)m_AccuracyLevel));
            }
            else if (m_Slayer != SlayerName.None || m_DurabilityLevel != WeaponDurabilityLevel.Regular || m_DamageLevel != WeaponDamageLevel.Regular || m_AccuracyLevel != WeaponAccuracyLevel.Regular)
            {
                attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
            }

            if (m_Poison != null && m_PoisonCharges > 0)
                attrs.Add(new EquipInfoAttribute(1017383, m_PoisonCharges));

            int number;

            if (Name == null)
            {
                number = LabelNumber;
            }
            else
            {
                this.LabelTo(from, Name);
                number = 1041000;
            }

            if (attrs.Count == 0 && Crafter == null && Name != null)
                return;

            EquipmentInfo eqInfo = new EquipmentInfo(number, m_Crafter, false, (EquipInfoAttribute[])attrs.ToArray(typeof(EquipInfoAttribute)));

            from.Send(new DisplayEquipmentInfo(this, eqInfo));
        }

        private static BaseWeapon m_Fists; // This value holds the default--fist--weapon

        public static BaseWeapon Fists
        {
            get { return m_Fists; }
            set { m_Fists = value; }
        }

        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            Quality = (WeaponQuality)quality;

            if (makersMark)
                Crafter = from;

            PlayerConstructed = true;

            Type resourceType = typeRes;


            if (resourceType == null)
                resourceType = craftItem.Ressources.GetAt(0).ItemType;

            if (Core.AOS)
            {
                Resource = CraftResources.GetFromType(resourceType);

                CraftContext context = craftSystem.GetContext(from);

                if (context != null && context.DoNotColor)
                    Hue = 0;

                if (tool is BaseRunicTool)
                    ((BaseRunicTool)tool).ApplyAttributesTo(this);

                if (quality == 2)
                {
                    if (Attributes.WeaponDamage > 35)
                        Attributes.WeaponDamage -= 20;
                    else
                        Attributes.WeaponDamage = 15;
                }
            }
            else if (tool is BaseRunicTool)
            {
                CraftResource thisResource = CraftResources.GetFromType(resourceType);

                if (thisResource == ((BaseRunicTool)tool).Resource)
                {
                    Resource = thisResource;

                    CraftContext context = craftSystem.GetContext(from);

                    if (context != null && context.DoNotColor)
                        Hue = 0;

                    switch (thisResource)
                    {
                        case CraftResource.DullCopper:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Durable;
                                AccuracyLevel = WeaponAccuracyLevel.Accurate;
                                break;
                            }
                        case CraftResource.ShadowIron:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Durable;
                                DamageLevel = WeaponDamageLevel.Ruin;
                                break;
                            }
                        case CraftResource.Copper:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Fortified;
                                DamageLevel = WeaponDamageLevel.Ruin;
                                AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
                                break;
                            }
                        case CraftResource.Bronze:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Fortified;
                                DamageLevel = WeaponDamageLevel.Might;
                                AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
                                break;
                            }
                        case CraftResource.Gold:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                DamageLevel = WeaponDamageLevel.Force;
                                AccuracyLevel = WeaponAccuracyLevel.Eminently;
                                break;
                            }
                        case CraftResource.Agapite:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                DamageLevel = WeaponDamageLevel.Power;
                                AccuracyLevel = WeaponAccuracyLevel.Eminently;
                                break;
                            }
                        case CraftResource.Verite:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                DamageLevel = WeaponDamageLevel.Power;
                                AccuracyLevel = WeaponAccuracyLevel.Exceedingly;
                                break;
                            }
                        case CraftResource.Valorite:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                DamageLevel = WeaponDamageLevel.Vanq;
                                AccuracyLevel = WeaponAccuracyLevel.Supremely;
                                break;
                            }
                    }
                }
            }

            Resource = CraftResources.GetFromType(resourceType);

            if (Resource == CraftResource.Mithril)
            {
                Attributes.WeaponDamage += 15;
                Attributes.WeaponSpeed += 10;
            }

            if (Resource == CraftResource.Blackrock)
            {
                Attributes.WeaponDamage += 20;
            }

            if (Resource == CraftResource.Bloodrock)
            {
                Attributes.WeaponDamage += 30;
                Attributes.WeaponSpeed -= 15;
            }

            if (Resource == CraftResource.Steel)
            {
                Attributes.WeaponDamage += 10;
                Attributes.WeaponSpeed += 10;
            }

            if (Resource == CraftResource.Silver)
            {
                Attributes.WeaponSpeed += 10;
            }

            if (Resource == CraftResource.Copper)
            {
                Attributes.WeaponDamage += 10;
            }

            if (Resource == CraftResource.Adamantite)
            {
                Attributes.WeaponDamage += 5;
                Attributes.WeaponSpeed += 15;
            }

            if (Resource == CraftResource.Ithilmar)
            {
                Attributes.WeaponDamage += 5;
                Attributes.WeaponSpeed += 15;
            }

            return quality;
        }

        #endregion
    }

    public enum CheckSlayerResult
    {
        None,
        Slayer,
        Opposition
    }
}
