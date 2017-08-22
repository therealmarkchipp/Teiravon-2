using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	public class HumanContinentEscort : BaseEscortable
	{
		public void GetDest()
		{
			string dest = "Evermeet";

			switch ( Utility.RandomMinMax( 2, 5 ) )
			{
				//case 1:	dest = "Evermeet"; break;
				case 2:	dest = "Tilverton"; break;
				case 3:	dest = "Trailsend"; break;
				case 4:	dest = "Darmshall"; break;
				case 5:	dest = "Westgate"; break;
			}

			Destination = dest;
		}

		[Constructable]
		public HumanContinentEscort()
		{
			Title = "the Explorer";

			SetSkill( SkillName.Parry, 80.0, 100.0 );
			SetSkill( SkillName.Swords, 80.0, 100.0 );
			SetSkill( SkillName.Tactics, 80.0, 100.0 );

			GetDest();
		}

		public override bool CanTeach{ get{ return false; } }
		public override bool ClickTitle{ get{ return false; } }

		private static int GetRandomHue()
		{
			switch ( Utility.Random( 6 ) )
			{
				default:
				case 0: return 0;
				case 1: return Utility.RandomBlueHue();
				case 2: return Utility.RandomGreenHue();
				case 3: return Utility.RandomRedHue();
				case 4: return Utility.RandomYellowHue();
				case 5: return Utility.RandomNeutralHue();
			}
		}

		public override void InitOutfit()
		{
			if ( Female )
				AddItem( new FancyDress() );
			else
				AddItem( new FancyShirt( GetRandomHue() ) );

			int lowHue = GetRandomHue();

			AddItem( new ShortPants( lowHue ) );

			if ( Female )
				AddItem( new ThighBoots( lowHue ) );
			else
				AddItem( new Boots( lowHue ) );

			if ( !Female )
				AddItem( new BodySash( lowHue ) );

			AddItem( new Cloak( GetRandomHue() ) );

			if ( !Female )
				AddItem( new Longsword() );

			switch ( Utility.Random( 4 ) )
			{
				case 0: AddItem( new ShortHair( Utility.RandomHairHue() ) ); break;
				case 1: AddItem( new TwoPigTails( Utility.RandomHairHue() ) ); break;
				case 2: AddItem( new ReceedingHair( Utility.RandomHairHue() ) ); break;
				case 3: AddItem( new KrisnaHair( Utility.RandomHairHue() ) ); break;
			}
		}

		public HumanContinentEscort( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}