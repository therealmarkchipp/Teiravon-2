using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
    public class GoboFace : BaseArmor
    {

        private Rank m_Rank;
        private TeiravonMobile m_Player;

        //public override int PhysicalResistance{ get{ return 10; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Cloth; } }
        public override bool DisplayLootType { get { return false; } }

        public enum Rank
        {
            None,
            Makur = 1,
            Grunt = 2,
            Sergeant = 3,
            Captain = 4,
            Shaman = 5,
            HighShaman = 6,
            Warboss = 7
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rank GoboRank
        {
            get { return m_Rank; }
            set
            {
                Rank m_Previous = m_Rank;

                m_Rank = value;

                bool equipped = false;

                Attributes.RegenHits = 0;
                Attributes.RegenMana = 0;
                Attributes.RegenStam = 0;
                Attributes.CastSpeed = 0;
                Attributes.LowerManaCost = 0;
                Attributes.WeaponDamage = 0;

                if (this.RootParent != null && this.RootParent is TeiravonMobile)
                {
                    m_Player = (TeiravonMobile)this.RootParent;
                    equipped = true;
                }

                if (m_Rank == Rank.None)
                {
                    if (equipped)
                    {
                        m_Player.Title = "the Goblin";

                        if ((int)m_Previous > (int)m_Rank)
                            m_Player.SendMessage("You have been demoted to Snot!");
                    }

                }
                else if (m_Rank == Rank.Makur)
                {
                    if (equipped)
                    {

                        m_Player.Title = "the Goblin Scrapp";

                        if ((int)m_Previous < (int)m_Rank)
                            m_Player.SendMessage("You have been promoted to Scrapp!");
                        else if ((int)m_Previous != (int)m_Rank)
                            m_Player.SendMessage("You have been demoted to Scrapp!");
                    }

                }
                else if (m_Rank == Rank.Grunt)
                {
                    if (equipped)
                    {

                        m_Player.Title = "the Goblin Runt";

                        if ((int)m_Previous < (int)m_Rank)
                            m_Player.SendMessage("You have been promoted to Runt!");
                        else if ((int)m_Previous != (int)m_Rank)
                            m_Player.SendMessage("You have been demoted to Runt!");
                    }

                }
                else if (m_Rank == Rank.Sergeant)
                {
                    if (equipped)
                    {
                        this.Hue = 2210;
                        //Attributes.RegenHits = 1;

                        m_Player.Hue = 2210;
                        m_Player.Title = "the Goblin Gnob";

                        if ((int)m_Previous < (int)m_Rank)
                            m_Player.SendMessage("You have been promoted to Gnob!");
                        else if ((int)m_Previous != (int)m_Rank)
                            m_Player.SendMessage("You have been demoted to Gnob!");
                    }
                }
                else if (m_Rank == Rank.Captain)
                {
                    if (equipped)
                    {
                        //Attributes.RegenHits = 2;
                        //Attributes.WeaponDamage = 10;

                        m_Player.Title = "the Goblin Cap";

                        if ((int)m_Previous < (int)m_Rank)
                            m_Player.SendMessage("You have been promoted to Cap!");
                        else if ((int)m_Previous != (int)m_Rank)
                            m_Player.SendMessage("You have been demoted to Cap!");

                    }
                }
                else if (m_Rank == Rank.Shaman)
                {
                    if (equipped)
                    {
                        //Attributes.RegenMana = 1;
                       // Attributes.CastRecovery = 1;
                        //Attributes.LowerManaCost = 5;


                        m_Player.Title = "the Goblin Mojo";

                        if ((int)m_Previous < (int)m_Rank)
                            m_Player.SendMessage("You have been promoted to Mojo!");
                        else if ((int)m_Previous != (int)m_Rank)
                            m_Player.SendMessage("You have been demoted to Mojo!");

                    }
                }
                else if (m_Rank == Rank.HighShaman)
                {
                    if (equipped)
                    {
                       // Attributes.RegenMana = 1;
                       // Attributes.CastRecovery = 2;
                       // Attributes.LowerManaCost = 10;

                        m_Player.Title = "the Goblin Big Mojo";

                        if ((int)m_Previous < (int)m_Rank)
                            m_Player.SendMessage("You have been promoted to Big Mojo!");
                        else if ((int)m_Previous != (int)m_Rank)
                            m_Player.SendMessage("You have been demoted to Big Mojo!");

                    }
                }
                else if (m_Rank == Rank.Warboss)
                {
                    if (equipped)
                    {

                       // Attributes.RegenStam = 3;
                       // Attributes.RegenHits = 3;
                       // Attributes.WeaponDamage = 20;

                        m_Player.Title = "the Goblin Klan Chief";

                        if ((int)m_Previous < (int)m_Rank)
                            m_Player.SendMessage("You have been promoted to Klan Chief!");
                    }
                }
            }
        }



        [Constructable]
        public GoboFace()
            : this(null)
        {
        }

        [Constructable]
        public GoboFace(Mobile player)
            : base(0x141B)
        {
            Weight = 0.0;
            Hue = 2414;
            Resource = CraftResource.None;
            Layer = Layer.FacialHair;
            LootType = LootType.Blessed;
            m_Rank = Rank.None;
            GoboRank = Rank.None;

            if (player == null)
                Name = "Orc Face";
            else
            {
                Name = player.Name + "'s face";
                Hue = player.Hue;
            }
        }

        public override bool CanEquip(Mobile m)
        {
            if (m is TeiravonMobile)
            {
                TeiravonMobile m_Player = (TeiravonMobile)m;

                if ((m_Player.IsOrc() || m_Player.IsGoblin()))
                {
                    if (m.Hair != null)
                        (m.Hair).Delete();

                    if (m.Beard != null)
                        (m.Beard).Delete();

                    this.Hue = m_Player.Hue;
                    return true;
                }
            }

            if (!base.CanEquip(m))
                return false;

            return false;
        }

        public override DeathMoveResult OnParentDeath(Mobile parent)
        {
            return DeathMoveResult.RemainEquiped;
        }

        public GoboFace(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_Rank);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Rank = (Rank)reader.ReadInt();

            Attributes.RegenHits = 0;
            Attributes.RegenMana = 0;
            Attributes.RegenStam = 0;
            Attributes.CastSpeed = 0;
            Attributes.LowerManaCost = 0;
            Attributes.WeaponDamage = 0;
            /*
            if (m_Rank == Rank.Captain)
                Attributes.WeaponDamage = 10;
            if (m_Rank == Rank.Warboss)
                Attributes.WeaponDamage = 20;
            if (m_Rank == Rank.Shaman)
            {
                Attributes.RegenMana = 1;
                Attributes.LowerManaCost = 5;
            }
            if (m_Rank == Rank.HighShaman)
            {
                Attributes.RegenMana = 1;
                Attributes.CastSpeed = 1;
                Attributes.LowerManaCost = 10;
            }*/
        }
    }
}
