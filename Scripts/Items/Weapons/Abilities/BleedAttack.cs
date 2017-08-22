using System;
using System.Collections;
using Server.Mobiles;

namespace Server.Items
{
	/// <summary>
	/// Make your opponent bleed profusely with this wicked use of your weapon.
	/// When successful, the target will bleed for several seconds, taking damage as time passes for up to ten seconds.
	/// The rate of damage slows down as time passes, and the blood loss can be completely staunched with the use of bandages.
	/// </summary>
	public class BleedAttack : WeaponAbility
	{
		public BleedAttack()
		{
		}

//		public override int BaseMana{ get{ return 30; } }

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
				return;

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1060159 ); // Your target is bleeding!
			defender.SendLocalizedMessage( 1060160 ); // You are bleeding!

			defender.PlaySound( 0x133 );
			defender.FixedParticles( 0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist );

			BeginBleed( defender, attacker );
		}

		private static Hashtable m_Table = new Hashtable();

		public static bool IsBleeding( Mobile m )
		{
			return m_Table.Contains( m );
		}

		public static void BeginBleed( Mobile m, Mobile from )
		{
			InternalTimer t = (InternalTimer)m_Table[m];
            int l = 1;
            if (t != null)
            {
                l = ++t.Count;
                t.Stop();
            }

			t = new InternalTimer( from, m, l );
			m_Table[m] = t;

			t.Start();
		}

		public static void DoBleed( Mobile m, Mobile from, int level )
		{
			if ( m.Alive )
			{
                int damage = Utility.RandomMinMax(level*2, (level*2)+5);

				if ( !m.Player )
					damage *= 2;

				m.PlaySound( 0x133 );
				m.Damage( damage, from );

				Blood blood = new Blood();

				blood.ItemID = Utility.Random( 0x122A, 5 );

				blood.MoveToWorld( m.Location, m.Map );
			}
			else
			{
				EndBleed( m, false );
			}
		}

		public static void EndBleed( Mobile m, bool message )
		{
			Timer t = (Timer)m_Table[m];

			if ( t == null )
				return;

			t.Stop();
			m_Table.Remove( m );

			m.SendLocalizedMessage( 1060167 ); // The bleeding wounds have healed, you are no longer bleeding!
		}

		private class InternalTimer : Timer
		{
			private Mobile m_From;
			private Mobile m_Mobile;
			private int m_Count;

            public int Count { get { return m_Count; } set { m_Count = value; } }

			public InternalTimer( Mobile from, Mobile m, int level ) : base( TimeSpan.FromSeconds( 2.0 ), TimeSpan.FromSeconds( 2.0 ) )
			{
                m_Count = level;
				m_From = from;
				m_Mobile = m;
				Priority = TimerPriority.TwoFiftyMS;
			}

			protected override void OnTick()
			{
                if ((m_Mobile is Mobiles.BaseCreature && Utility.Random(40) == 1 ) || (m_Mobile is TeiravonMobile && ((TeiravonMobile)m_Mobile).IsUndead()))
                {
                    
                    EndBleed(m_Mobile, true);
                    ((Mobiles.BaseCreature)m_Mobile).LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, "*Their bleeding stops*");
                }
                else

                    DoBleed(m_Mobile, m_From, m_Count);
			}
		}
	}
}