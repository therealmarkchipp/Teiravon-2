using System;
using System.Collections;
using System.IO;
using Server;
using Server.Accounting;

namespace Knives.Utils
{
	public class GumpInfo
	{
		private static Hashtable s_Infos = new Hashtable();
        private static ArrayList s_Backgrounds = new ArrayList();
        private static ArrayList s_TextColors = new ArrayList();

		public static void Configure()
		{
			EventSink.WorldLoad += new WorldLoadEventHandler( OnLoad );
			EventSink.WorldSave += new WorldSaveEventHandler( OnSave );
		}

		public static void Initialize()
		{
			s_Backgrounds.Add( 0xA3C );
			s_Backgrounds.Add( 0x53 );
			s_Backgrounds.Add( 0x2486 );
			s_Backgrounds.Add( 0xDAC );
			s_Backgrounds.Add( 0xE10 );
			s_Backgrounds.Add( 0x13EC );
			s_Backgrounds.Add( 0x1400 );
			s_Backgrounds.Add( 0x2422 );
			s_Backgrounds.Add( 0x242C );
			s_Backgrounds.Add( 0x13BE );
			s_Backgrounds.Add( 0x2436 );
			s_Backgrounds.Add( 0x2454 );
			s_Backgrounds.Add( 0x251C );
			s_Backgrounds.Add( 0x254E );
			s_Backgrounds.Add( 0x24A4 );
			s_Backgrounds.Add( 0x24AE );

            s_TextColors.Add("FFFFFF");
            s_TextColors.Add("111111");
            s_TextColors.Add("FF0000");
            s_TextColors.Add("FF9999");
            s_TextColors.Add("00FF00");
            s_TextColors.Add("0000FF");
            s_TextColors.Add("999999");
            s_TextColors.Add("333333");
            s_TextColors.Add("FFFF00");
            s_TextColors.Add("990099");
            s_TextColors.Add("CC00FF");
        }

		private static void OnSave( WorldSaveEventArgs e )
		{try{

			if ( !Directory.Exists( "Saves/Gumps/" ) )
				Directory.CreateDirectory( "Saves/Gumps/" );

			GenericWriter writer = new BinaryFileWriter( Path.Combine( "Saves/Gumps/", "Gumps.bin" ), true );

			writer.Write( 0 ); // version

			ArrayList list = new ArrayList();
			GumpInfo info;

			foreach( object o in new ArrayList( s_Infos.Values ) )
			{
				if ( !(o is Hashtable) )
					continue;

				foreach( object ob in new ArrayList( ((Hashtable)o).Values ) )
				{
					if ( !(ob is GumpInfo ) )
						continue;

					info = (GumpInfo)ob;

					if ( info.Mobile != null
					&& info.Mobile.Player
					&& !info.Mobile.Deleted
					&& info.Mobile.Account != null
					&& ((Account)info.Mobile.Account).LastLogin > DateTime.Now - TimeSpan.FromDays( 30 ) )
						list.Add( ob );
				}
			}

			writer.Write( list.Count );

			foreach( GumpInfo ginfo in list )
				ginfo.Save( writer );

			writer.Close();

		}catch{ Errors.Report( "GumpInfo-> OnSave" ); } }

		private static void OnLoad()
		{try{

			if ( !File.Exists( Path.Combine( "Saves/Gumps/", "Gumps.bin" ) ) )
				return;

			using ( FileStream bin = new FileStream( Path.Combine( "Saves/Gumps/", "Gumps.bin" ), FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				GenericReader reader = new BinaryFileReader( new BinaryReader( bin ) );

				int version = reader.ReadInt();

				if ( version >= 0 )
				{
					int count = reader.ReadInt();
					GumpInfo info;

					for( int i = 0; i < count; ++i )
					{
						info = new GumpInfo();
						info.Load( reader );

						if ( info.Mobile == null || info.Type == null )
							continue;

						if ( s_Infos[info.Mobile] == null )
							s_Infos[info.Mobile] = new Hashtable();

						((Hashtable)s_Infos[info.Mobile])[info.Type] = info;
					}
				}

				reader.End();
			}

		}catch{ Errors.Report( "GumpInfo-> OnLoad" ); } }

		public static GumpInfo GetInfo( Mobile m, Type type )
		{
			if ( s_Infos[m] == null )
				s_Infos[m] = new Hashtable();

			Hashtable table = (Hashtable)s_Infos[m];

			if ( table[type] == null )
				table[type] = new GumpInfo( m, type );

			return (GumpInfo)table[type];
		}

		public static bool HasMods( Mobile m, Type type )
		{
			if ( s_Infos[m] == null )
				s_Infos[m] = new Hashtable();

			if ( ((Hashtable)s_Infos[m])[type] == null )
				return false;

			return true;
		}

		private Mobile c_Mobile;
		private Type c_Type;
		private bool c_Transparent, c_DefaultTrans;
		private string c_TextColorRGB;
		private int c_Background;

		public Mobile Mobile{ get{ return c_Mobile; } }
		public Type Type{ get{ return c_Type; } }
        public bool Transparent { get { return c_Transparent; } set { c_Transparent = value; c_DefaultTrans = false; } }
		public bool DefaultTrans{ get{ return c_DefaultTrans; } set{ c_DefaultTrans = value; } }
		public string TextColorRGB{ get{ return c_TextColorRGB; } set{ c_TextColorRGB = value; } }
		public string TextColor{ get{ return String.Format( "<BASEFONT COLOR=#{0}>", c_TextColorRGB ); } }
		public int Background{ get{ return c_Background; } }

		public GumpInfo()
		{
		}

		public GumpInfo( Mobile m, Type type )
		{
			c_Mobile = m;
			c_Type = type;
			c_TextColorRGB = "";
			c_Background = -1;
			c_DefaultTrans = true;
		}

        public void BackgroundUp()
        {
            if (c_Background == -1)
            {
                c_Background = (int)s_Backgrounds[0];
                return;
            }

            for (int i = 0; i < s_Backgrounds.Count; ++i)
                if (c_Background == (int)s_Backgrounds[i])
                {
                    if (i == s_Backgrounds.Count - 1)
                    {
                        c_Background = (int)s_Backgrounds[0];
                        return;
                    }

                    c_Background = (int)s_Backgrounds[i + 1];
                    return;
                }
        }

        public void BackgroundDown()
        {
            if (c_Background == -1)
            {
                c_Background = (int)s_Backgrounds[s_Backgrounds.Count - 1];
                return;
            }

            for (int i = 0; i < s_Backgrounds.Count; ++i)
                if (c_Background == (int)s_Backgrounds[i])
                {
                    if (i == 0)
                    {
                        c_Background = (int)s_Backgrounds[s_Backgrounds.Count - 1];
                        return;
                    }

                    c_Background = (int)s_Backgrounds[i - 1];
                    return;
                }
        }

        public void TextColorUp()
        {
            if (c_TextColorRGB == "")
            {
                c_TextColorRGB = s_TextColors[0].ToString();
                return;
            }

            for (int i = 0; i < s_TextColors.Count; ++i)
                if (c_TextColorRGB == s_TextColors[i].ToString())
                {
                    if (i == s_TextColors.Count - 1)
                    {
                        c_TextColorRGB = s_TextColors[0].ToString();
                        return;
                    }

                    c_TextColorRGB = s_TextColors[i + 1].ToString();
                    return;
                }
        }

        public void TextColorDown()
        {
            if (c_TextColorRGB == "")
            {
                c_TextColorRGB = s_TextColors[s_TextColors.Count - 1].ToString();
                return;
            }

            for (int i = 0; i < s_TextColors.Count; ++i)
                if (c_TextColorRGB == s_TextColors[i].ToString())
                {
                    if (i == 0)
                    {
                        c_TextColorRGB = s_TextColors[s_TextColors.Count - 1].ToString();
                        return;
                    }

                    c_TextColorRGB = s_TextColors[i - 1].ToString();
                    return;
                }
        }

        public void Default()
        {
            if (c_Mobile == null || s_Infos[c_Mobile] == null)
                return;

            ((Hashtable)s_Infos[c_Mobile]).Remove(c_Type);
        }

        private void Save(GenericWriter writer)
		{try{

			writer.Write( 0 ); // version

			// Version 0
			writer.Write( c_Mobile );
			writer.Write( c_Type.ToString() );
			writer.Write( c_Transparent );
			writer.Write( c_DefaultTrans );
			writer.Write( c_TextColorRGB );
			writer.Write( c_Background );

		}catch{ Errors.Report( "GumpInfo -> Save" ); } }

		private void Load( GenericReader reader )
		{try{
			int version = reader.ReadInt();

			if ( version >= 0 )
			{
				c_Mobile = reader.ReadMobile();
				c_Type = ScriptCompiler.FindTypeByFullName( reader.ReadString() );
				c_Transparent = reader.ReadBool();
				c_DefaultTrans = reader.ReadBool();
				c_TextColorRGB = reader.ReadString();
				c_Background = reader.ReadInt();
			}

		}catch{ Errors.Report( "GumpInfo -> Load" ); } }
	}
}