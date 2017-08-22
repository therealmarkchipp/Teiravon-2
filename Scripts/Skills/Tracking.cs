using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Spells.Necromancy;
using Server.Mobiles;

namespace Server.SkillHandlers
{
	public class Tracking
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Tracking].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			m.SendLocalizedMessage( 1011350 ); // What do you wish to track?

			m.CloseGump( typeof( TrackWhatGump ) );
			m.CloseGump( typeof( TrackWhoGump ) );
			m.SendGump( new TrackWhatGump( m ) );

			return TimeSpan.FromSeconds( 3.0 ); // 10 second delay before beign able to re-use a skill
		}
	}

	public class TrackWhatGump : Gump
	{
		private Mobile m_From;
		private bool m_Success;

		public TrackWhatGump( Mobile from ) : base( 20, 30 )
		{
			m_From = from;
			m_Success = from.CheckSkill( SkillName.Tracking, 0.0, 21.1 );

			AddPage( 0 );

			AddBackground( 0, 0, 440, 135, 5054 );

			AddBackground( 10, 10, 420, 75, 2620 );
			AddBackground( 10, 85, 420, 25, 3000 );

			AddItem( 20, 20, 9682 );
			AddButton( 20, 110, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 20, 90, 100, 20, 1018087, false, false ); // Animals

			AddItem( 120, 20, 9607 );
			AddButton( 120, 110, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 120, 90, 100, 20, 1018088, false, false ); // Monsters

			AddItem( 220, 20, 8454 );
			AddButton( 220, 110, 4005, 4007, 3, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 220, 90, 100, 20, 1018089, false, false ); // Human NPCs

			AddItem( 320, 20, 8455 );
			AddButton( 320, 110, 4005, 4007, 4, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 320, 90, 100, 20, 1018090, false, false ); // Players
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( info.ButtonID >= 1 && info.ButtonID <= 4 )
				TrackWhoGump.DisplayTo( m_Success, m_From, info.ButtonID - 1 );
		}
	}

	public delegate bool TrackTypeDelegate( Mobile m );

	public class TrackWhoGump : Gump
	{
		private Mobile m_From;
		private int m_Range;

		private static TrackTypeDelegate[] m_Delegates = new TrackTypeDelegate[]
			{
				new TrackTypeDelegate( IsAnimal ),
				new TrackTypeDelegate( IsMonster ),
				new TrackTypeDelegate( IsHumanNPC ),
				new TrackTypeDelegate( IsPlayer )
			};

		private class InternalSorter : IComparer
		{
			private Mobile m_From;

			public InternalSorter( Mobile from )
			{
				m_From = from;
			}

			public int Compare( object x, object y )
			{
				if ( x == null && y == null )
					return 0;
				else if ( x == null )
					return -1;
				else if ( y == null )
					return 1;

				Mobile a = x as Mobile;
				Mobile b = y as Mobile;

				if ( a == null || b == null )
					throw new ArgumentException();

				return m_From.GetDistanceToSqrt( a ).CompareTo( m_From.GetDistanceToSqrt( b ) );
			}
		}

		public static void DisplayTo( bool success, Mobile from, int type )
		{
			if ( !success )
			{
				from.SendLocalizedMessage( 1018092 ); // You see no evidence of those in the area.
				return;
			}

			Map map = from.Map;

			if ( map == null )
				return;

			TrackTypeDelegate check = m_Delegates[type];

			from.CheckSkill( SkillName.Tracking, 21.1, 100.0 ); // Passive gain

			int range = 10 + (int)(from.Skills[SkillName.Tracking].Value / 5);

			ArrayList list = new ArrayList();

			foreach ( Mobile m in from.GetMobilesInRange( range ) )
			{
				// Ghosts can no longer be tracked
				if ( m != from && (!Core.AOS || m.Alive) && (!m.Hidden || m.AccessLevel == AccessLevel.Player || from.AccessLevel > m.AccessLevel) && check( m ) && CheckDifficulty( from, m ) )
					list.Add( m );
			}

			if ( list.Count > 0 )
			{
				list.Sort( new InternalSorter( from ) );

				from.SendGump( new TrackWhoGump( from, list, range ) );
				from.SendLocalizedMessage( 1018093 ); // Select the one you would like to track.
			}
			else
			{
				if ( type == 0 )
					from.SendLocalizedMessage( 502991 ); // You see no evidence of animals in the area.
				else if ( type == 1 )
					from.SendLocalizedMessage( 502993 ); // You see no evidence of creatures in the area.
				else
					from.SendLocalizedMessage( 502995 ); // You see no evidence of people in the area.
			}
		}

		// Tracking players uses tracking and detect hidden and random (1 to 20) vs. hiding and stealth
		private static bool CheckDifficulty( Mobile from, Mobile m )
		{
			if ( !Core.AOS || !m.Player )
				return true;

			int tracking = from.Skills[SkillName.Tracking].Fixed;
			int detectHidden = from.Skills[SkillName.DetectHidden].Fixed;

            int stealth = m.Skills[SkillName.Stealth].Fixed;
            int divisor = stealth * 2;

			// Necromancy forms affect tracking difficulty
			if ( TransformationSpell.UnderTransformation( m, typeof( HorrificBeastSpell ) ) )
				divisor -= 200;
			else if ( TransformationSpell.UnderTransformation( m, typeof( VampiricEmbraceSpell ) ) && divisor < 500 )
				divisor = 500;
			else if ( TransformationSpell.UnderTransformation( m, typeof( WraithFormSpell ) ) && divisor <= 2000 )
				divisor += 200;

			int chance;
			if ( divisor > 0 )
				chance = 50 * (tracking + detectHidden + 10 * Utility.RandomMinMax( 1, 20 )) / divisor;
			else
				chance = 100;

			return chance > Utility.Random( 100 );
		}

		private static bool IsAnimal( Mobile m )
		{
			return ( !m.Player && m.Body.IsAnimal );
		}

		private static bool IsMonster( Mobile m )
		{
			return ( !m.Player && m.Body.IsMonster );
		}

		private static bool IsHumanNPC( Mobile m )
		{
			return ( !m.Player && m.Body.IsHuman );
		}

		private static bool IsPlayer( Mobile m )
		{
			return m.Player;
		}

		private ArrayList m_List;

		private TrackWhoGump( Mobile from, ArrayList list, int range ) : base( 20, 30 )
		{
			m_From = from;
			m_List = list;
			m_Range = range;

			AddPage( 0 );

			AddBackground( 0, 0, 440, 155, 5054 );

			AddBackground( 10, 10, 420, 75, 2620 );
			AddBackground( 10, 85, 420, 45, 3000 );

			if ( list.Count > 4 )
			{
				AddBackground( 0, 155, 440, 155, 5054 );

				AddBackground( 10, 165, 420, 75, 2620 );
				AddBackground( 10, 240, 420, 45, 3000 );

				if ( list.Count > 8 )
				{
					AddBackground( 0, 310, 440, 155, 5054 );

					AddBackground( 10, 320, 420, 75, 2620 );
					AddBackground( 10, 395, 420, 45, 3000 );
				}
			}

			for ( int i = 0; i < list.Count && i < 12; ++i )
			{
				Mobile m = (Mobile)list[i];

				AddItem( 20 + ((i % 4) * 100), 20 + ((i / 4) * 155), ShrinkTable.Lookup( m ) );
				AddButton( 20 + ((i % 4) * 100), 130 + ((i / 4) * 155), 4005, 4007, i + 1, GumpButtonType.Reply, 0 );

				if ( m.Name != null )
					AddHtml( 20 + ((i % 4) * 100), 90 + ((i / 4) * 155), 90, 40, m.Name, false, false );
			}
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			int index = info.ButtonID - 1;

			if ( index >= 0 && index < m_List.Count && index < 12 )
			{
				Mobile m = (Mobile)m_List[index];

				m_From.QuestArrow = new TrackArrow( m_From, m, m_Range * 2 );
			}
		}
	}

	public class TrackArrow : QuestArrow
	{
		private Mobile m_From;
		private Mobile m_Target;
		private Timer m_Timer;

		public TrackArrow( Mobile from, Mobile target, int range ) : base( from )
		{
			m_From = from;
			m_Target = target;
			m_Timer = new TrackTimer( from, target, range, this );
			m_Timer.Start();
		}
        public Mobile Target
        {
            get { return m_Target; }
        }
		public override void OnClick( bool rightClick )
		{
			if ( rightClick )
			{
				m_From = null;

				Stop();
			}

			if ( m_From is TeiravonMobile )
			{
				TeiravonMobile m_Tracker = (TeiravonMobile) m_From;

				if ( m_Tracker.HasFeat( TeiravonMobile.Feats.AdvancedTracking ) && m_From != m_Target )
				{
					if ( m_Tracker.PlayerLevel < 5 )
						m_Tracker.SendMessage( "You figure they have about {0} hitpoints left...", m_Target.Hits );
					else if ( m_Tracker.PlayerLevel < 10 )
						m_Tracker.SendMessage( "You figure they have about {0} hitpoints and {1} stamina left...", m_Target.Hits, m_Target.Stam );
					else if ( m_Tracker.PlayerLevel < 15 )
						m_Tracker.SendMessage( "You figure they have about {0} hitpoints, {1} stamina, and {2} mana left...", m_Target.Hits, m_Target.Stam, m_Target.Mana );
					else if ( m_Tracker.PlayerLevel >= 15 )
						m_Tracker.SendMessage( "They have {0}/{1} hits, {2}/{3} stamina, and {4}/{5} mana left.", m_Target.Hits, m_Target.HitsMax, m_Target.Stam, m_Target.StamMax, m_Target.Mana, m_Target.ManaMax );
				}
			}
		}

		public override void OnStop()
		{
			m_Timer.Stop();

			if ( m_From != null )
				m_From.SendLocalizedMessage( 503177 ); // You have lost your quarry.
		}
	}

	public class TrackTimer : Timer
	{
		private Mobile m_From, m_Target;
		private int m_Range;
		private int m_LastX, m_LastY;
		private QuestArrow m_Arrow;

		public TrackTimer( Mobile from, Mobile target, int range, QuestArrow arrow ) : base( TimeSpan.FromSeconds( 0.25 ), TimeSpan.FromSeconds( 2.5 ) )
		{
			m_From = from;
			m_Target = target;
			m_Range = range;

			m_Arrow = arrow;
		}

		protected override void OnTick()
		{
			if ( !m_Arrow.Running )
			{
				Stop();
				return;
			}
			else if ( m_From.NetState == null || m_From.Deleted || m_Target.Deleted || m_From.Map != m_Target.Map || !m_From.InRange( m_Target, m_Range ) )
			{
				m_From.Send( new CancelArrow() );
				m_From.SendLocalizedMessage( 503177 ); // You have lost your quarry.

				Stop();
				return;
			}

			if ( m_LastX != m_Target.X || m_LastY != m_Target.Y )
			{
				m_LastX = m_Target.X;
				m_LastY = m_Target.Y;

				m_Arrow.Update( m_LastX, m_LastY );
			}
		}
	}
}