using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a gazer corpse" )]
	public class Gazer : BaseCreature
	{
		[Constructable]
		public Gazer () : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a gazer";
			Body = 22;
			BaseSoundID = 377;
			Level = 9;

			SetStr( 96, 125 );
			SetDex( 86, 105 );
			SetInt( 141, 165 );

			SetHits( 88, 115 );

			SetDamage( 10, 13 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 35, 40 );
			SetResistance( ResistanceType.Fire, 40, 50 );
			SetResistance( ResistanceType.Cold, 20, 30 );
			SetResistance( ResistanceType.Poison, 10, 20 );
			SetResistance( ResistanceType.Energy, 20, 30 );

			SetSkill( SkillName.EvalInt, 50.1, 65.0 );
			SetSkill( SkillName.Magery, 50.1, 65.0 );
			SetSkill( SkillName.MagicResist, 60.1, 75.0 );
			SetSkill( SkillName.Tactics, 50.1, 70.0 );
			SetSkill( SkillName.Wrestling, 50.1, 70.0 );

			Fame = 3500;
			Karma = -3500;

			VirtualArmor = 36;

			PackItem( new Nightshade( 7 ) );

			//PackScroll( 1, 6 );

			if ( Utility.Random( 10 ) == 0 )
				PackScroll( 1, 4 );

			//PackGold( 30, 35 );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Meager );
			AddLoot( LootPack.Potions );
		}

		public override int TreasureMapLevel{ get{ return 1; } }
		public override int Meat{ get{ return 1; } }

		public Gazer( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}