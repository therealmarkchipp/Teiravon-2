/////////////////////////////////////////////////
//
// Automatically generated by the
// AddonGenerator script by Arya
//
/////////////////////////////////////////////////
using System;
using Server;
using Server.Items;
using Server.Mobiles;
using System.Collections.Generic;
using System.Collections;
using Server.Gumps.MarketGump;
using Server.ContextMenus;

namespace Server.Mobiles
{
    public class Marketeer : BaseCreature
	{
        public String m_Market;

        [Constructable]
        public Marketeer(String market) : this()
        {
            Blessed = true;
            m_Market = market;
            CantWalk = true;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public String MarketName
        {
            get { return m_Market; }
            set
            {
                m_Market = value;
            }
        }

		public Marketeer() : base( AIType.AI_Vendor, FightMode.Closest, 10, 1, 0.17, 0.4 )
		{
			SpeechHue = Utility.RandomDyedHue();
			Hue = Utility.RandomSkinHue();

			if( this.Female = Utility.RandomBool() )
			{
				Body = 0x191;
				Name = NameList.RandomName( "female" );

			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName( "male" );
			}

            Title = "the Marketeer";

			SetStr( 100, 100 );
			SetDex( 100, 100 );
			SetInt( 100, 100 );

			SetHits( 1000, 1000 );

			//SetDamage( 9, 15 );
			SetSkill( SkillName.Anatomy, 90.0, 110.5 );
			SetSkill( SkillName.Healing, 100.0, 100.5 );
			SetSkill( SkillName.MagicResist, 80.0, 120.5 );
			SetSkill( SkillName.Swords, 110.0, 125.5 );
			SetSkill( SkillName.Tactics, 110.0, 130.5 );
			SetSkill( SkillName.Wrestling, 100.0, 100.5 );

            Fame = 5;
            Karma = 5000;

			VirtualArmor = 52;

            InitOutfit();
		}

        public virtual int GetRandomHue()
		{
			switch ( Utility.Random( 5 ) )
			{
				default:
				case 0: return Utility.RandomBlueHue();
				case 1: return Utility.RandomGreenHue();
				case 2: return Utility.RandomRedHue();
				case 3: return Utility.RandomYellowHue();
				case 4: return Utility.RandomNeutralHue();
			}
		}

		public virtual int GetShoeHue()
		{
			if ( 0.1 > Utility.RandomDouble() )
				return 0;

			return Utility.RandomNeutralHue();

		}

        public override void OnDoubleClick( Mobile from )
        {
           // from.SendGump(
        }

        public virtual void InitOutfit()
		{
			switch ( Utility.Random( 3 ) )
			{
				case 0: AddItem( new FancyShirt( GetRandomHue() ) ); break;
				case 1: AddItem( new Doublet( GetRandomHue() ) ); break;
				case 2: AddItem( new Shirt( GetRandomHue() ) ); break;
			}

			switch ( Utility.Random( 4 ) )
			{
				case 0: AddItem( new Shoes( GetShoeHue() ) ); break;
				case 1: AddItem( new Boots( GetShoeHue() ) ); break;
				case 2: AddItem( new Sandals( GetShoeHue() ) ); break;
				case 3: AddItem( new ThighBoots( GetShoeHue() ) ); break;
			}

			int hairHue = Utility.RandomHairHue();
            
            AddItem( Server.Items.Hair.GetRandomHair( Female, hairHue ) );

			if ( Female )
			{
				switch ( Utility.Random( 6 ) )
				{
					case 0: AddItem( new ShortPants( GetRandomHue() ) ); break;
					case 1:
					case 2: AddItem( new Kilt( GetRandomHue() ) ); break;
					case 3:
					case 4:
					case 5: AddItem( new Skirt( GetRandomHue() ) ); break;
				}
			}
			else
			{
				switch ( Utility.Random( 2 ) )
				{
					case 0: AddItem( new LongPants( GetRandomHue() ) ); break;
					case 1: AddItem( new ShortPants( GetRandomHue() ) ); break;
				}
			}
		}

        public override bool OnBeforeDeath()
		{
			IMount mount = this.Mount;

			if ( mount != null ) 
			{
				mount.Rider = null;

				if (mount is Mobile) ((Mobile)mount).Delete();
			}

			return base.OnBeforeDeath();
		}

		public override bool Unprovokable{ get{ return true; } }

		public override bool IsEnemy( Mobile from )
		{
            return false; 
		}

        public Marketeer(Serial serial)
            : base(serial)
		{
            Blessed = true;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
            
			writer.Write( (int) 0 ); // version
            writer.Write( m_Market );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
            m_Market = reader.ReadString();
		}

        public bool WasNamed(string speech)
        {
            return this.Name != null && Insensitive.StartsWith(speech, this.Name);
        }

     
        private class MenuEntry : ContextMenuEntry
        {
            private PlayerMobile m_Player;
            private Marketeer m_Mobile;
            private String m_command;

            public MenuEntry(PlayerMobile from, Marketeer marketeer, String command, int number)
                : base(number, 3) // uses "Configure" entry
            {
                m_Player = from;
                m_Mobile = marketeer;
                m_command = command;
            }

            public override void OnClick()
            {
                // send gump
                m_Mobile.HandleSpeech(m_Player, m_command);
            }
        }

        public override void AddCustomContextEntries(Mobile from, ArrayList list)
        {
            list.Add(new MenuEntry(from as PlayerMobile, this, "vendor information",  98));  //3000098	Information
            list.Add(new MenuEntry(from as PlayerMobile, this, "vendor buy", 6103));  //3006103	Buy
            list.Add(new MenuEntry(from as PlayerMobile, this, "vendor sell", 6104));  //3006104	sell
            list.Add(new MenuEntry(from as PlayerMobile, this, "vendor order", 156));  //3000156	Offer
            list.Add(new MenuEntry(from as PlayerMobile, this, "vendor account", 144));  // 3000144	My Offer
            list.Add(new MenuEntry(from as PlayerMobile, this, "vendor claim",  155));  //3000155	My Inventory

            base.AddCustomContextEntries(from, list);
        }

        public void HandleSpeech(Mobile from, String speech)
        {
            if (speech.Contains("vendor") || speech.Contains(Name.ToLower()))
            {
                if (from is PlayerMobile && speech.Contains("information")) // vendor, *buy*
                {
                    Say("I am a marketeer for the " + m_Market + " market, i manage the buy and sell orders of items or creatures for my customers.");
                    return;
                }

                if (from is PlayerMobile && speech.Contains("buy")) // vendor, *buy*
                {
                    List<MarketDatabase.SellOrder> results = new List<MarketDatabase.SellOrder>();
                    from.SendGump(new MarketItemBuySearchGump(from, "", 0, results, m_Market));
                    Say("Feel free to search the my records of items for sale.");
                    return;
                }

                if (from is PlayerMobile && speech.Contains("sell")) // vendor, *buy*
                {
                    if (from.Target != null)
                    {
                        from.SendMessage("You can't sell something like your target cursor is in use.");
                        return;
                    }
                    Say("What would you like to put up for sale?");

                    from.SendMessage("Select an item in your inventory, bank box or " + m_Market + " market account box, or tamed pet you wish to put for sale.");
                    from.Target = new MarketDatabase.SellOrderTarget(m_Market);
                    return;
                }

                if (from is PlayerMobile && (speech.Contains("claim") || speech.Contains("inventory"))) // vendor, *claim*
                {
                    MarketUserContainer accountBox = MarketDatabase.GetAccountBox(from as PlayerMobile, m_Market);
                    if (accountBox != null)
                    {
                        if (accountBox.Summon(from))
                            Say("Here you go; your account chest.");
                    }
                    return;
                }

                if (from is PlayerMobile && speech.Contains("order")) // vendor, *orders*
                {
                    List<MarketDatabase.BuyOrder> results = new List<MarketDatabase.BuyOrder>();
                    from.SendGump(new MarketBuyOrderSearchGump(from, "", 0, results, m_Market));
                    Say("Your welcome to search my records of customer requests.");
                    return;
                }

                if (from is PlayerMobile && (speech.Contains("account") || speech.Contains("my offer"))) // vendor, *account*
                {
                    from.SendGump(new UserAccountSearchGump(from, 0, null, m_Market));
                    Say("Here are your current market records.");
                    return;
                }
            }
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Handled || !from.Alive || from.GetDistanceToSqrt(this) > 3)
                return;

            HandleSpeech(from, e.Speech.ToLower());
        }
	}
}