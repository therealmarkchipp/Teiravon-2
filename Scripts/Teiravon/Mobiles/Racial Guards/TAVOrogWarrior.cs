using System; 
using System.Collections; 
using Server.Misc; 
using Server.Items; 
using Server.Mobiles; 

namespace Server.Mobiles 
{ 
	[CorpseName( "an orog corpse" )]
	public class OrogWarrior : BaseCreature 
	{ 
		public override bool ClickTitle{ get{ return false; } }
		
		[Constructable] 
		public OrogWarrior() : base( AIType.AI_Melee, FightMode.Agressor, 10, 1, 0.2, 0.4 ) 
		{ 
			Name = NameList.RandomName( "orc" );
			Title = "the Orog Warrior";
			SpeechHue = Utility.RandomDyedHue(); 
			Hue = 0x455; 
			Body = 0x190; 
			Level = 11;
			NameHue = 2545;

			LongHair hair = new LongHair();
			hair.Movable = false;
			hair.Hue = 0x3CE;
			AddItem( hair );
			OrcFace face = new OrcFace();
			face.Movable = false;
			face.Hue = 0x455; 
			AddItem( face );
			PlateChest chest = new PlateChest();
			chest.Hue = 0x9F1;
			chest.Name = "Orog Armor";
			chest.Movable = false;
			AddItem( chest );
			PlateArms arms = new PlateArms();
			arms.Hue = 0x9F1;
			arms.Name = "Orog Armor";
			arms.Movable = false;
			AddItem( arms );
			PlateGloves gloves = new PlateGloves();
			gloves.Hue = 0x9F1;
			gloves.Name = "Orog Armor";
			gloves.Movable = false;
			AddItem( gloves );
			PlateLegs legs = new PlateLegs();
			legs.Hue = 0x9F1;
			legs.Name = "Orog Armor";
			legs.Movable = false;
			AddItem( legs );
			
			Cloak cloak = new Cloak();
			cloak.Hue = 0xA42;
			cloak.Movable = false;
			AddItem( cloak );
			HeaterShield shield = new HeaterShield();
			shield.Hue = 0xA42;
			shield.Name = "Orog Shield";
			shield.Movable = false;
			AddItem( shield );			

			PackGold( 10, 17 );

			SetSkill( SkillName.MagicResist, 80.0, 99.5 );
			SetSkill( SkillName.Swords, 100.0, 113.5 );
			SetSkill( SkillName.Tactics, 85.0, 97.5 );
			SetSkill( SkillName.Wrestling, 65.0, 77.5 );
			SetSkill( SkillName.Macing, 100.0, 110.5 );
			
			SetStr( 316, 470 );
			SetDex( 151, 215 );
			SetInt( 141, 165 );
			SetHits( 480, 533 );
			SetDamage( 9, 12 );
			
			Karma = -5000;
			Fame = 5000;
			
			switch ( Utility.Random( 4 ) )
			{
				case 0: 
					OrogScimitar scimitar = new OrogScimitar();
					scimitar.Movable = false;
					AddItem( scimitar );
					break;
				case 1: 
					OrogKryss kryss = new OrogKryss();
					kryss.Movable = false;
					AddItem( kryss );
					break;
				case 2:
					OrogBroadsword bsword = new OrogBroadsword();
					bsword.Movable = false;
					AddItem( bsword );
					break;
				case 3:
					OrogVikingSword vsword = new OrogVikingSword();
					vsword.Movable = false;
					AddItem( vsword );
					break;
			}
		}
		
		public OrogWarrior( Serial serial ) : base( serial ) 
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