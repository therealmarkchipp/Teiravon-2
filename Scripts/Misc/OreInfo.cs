using System;
using System.Collections;

namespace Server.Items
{
    public enum CraftResource
    {
        None = 0,
        Iron = 1,
        DullCopper,
        ShadowIron,
        Copper,
        Bronze,
        Gold,
        Agapite,
        Verite,
        Valorite,
        Mithril,
        Bloodrock,
        Steel,
        Adamantite,
        Ithilmar,
        Silver,
        Blackrock,
        Ferite,
        Malachite,
        Pyrite,
        Umbrite,
        Amirite,
        Skazz,
        Electrum,

        RegularLeather = 101,
        SpinedLeather,
        HornedLeather,
        BarbedLeather,
        TuftedLeather,
        ScaledLeather,

        RedScales = 201,
        YellowScales,
        BlackScales,
        GreenScales,
        WhiteScales,
        BlueScales,

        Oak = 301,
        Pine,
        Redwood,
        WhitePine,
        Ashwood,
        SilverBirch,
        Yew,
        BlackOak,

        Frost = 401,
        Ice,
        Glacial,
    }

    public enum CraftResourceType
    {
        None,
        Metal,
        Leather,
        Scales,
        Wood,
        Ice
    }

    public class CraftAttributeInfo
    {
        private int m_WeaponFireDamage;
        private int m_WeaponColdDamage;
        private int m_WeaponPoisonDamage;
        private int m_WeaponEnergyDamage;
        private int m_WeaponDurability;
        private int m_WeaponLuck;
        private int m_WeaponGoldIncrease;
        private int m_WeaponLowerRequirements;
        private int m_WeaponDamageIncrease;
        private int m_WeaponMinStr;
        private int m_WeaponSpeed;

        private int m_ArmorPhysicalResist;
        private int m_ArmorFireResist;
        private int m_ArmorColdResist;
        private int m_ArmorPoisonResist;
        private int m_ArmorEnergyResist;
        private int m_ArmorDurability;
        private int m_ArmorLuck;
        private int m_ArmorGoldIncrease;
        private int m_ArmorLowerRequirements;
        private int m_ArmorReflectPhysical;

        private int m_RunicMinAttributes;
        private int m_RunicMaxAttributes;
        private int m_RunicMinIntensity;
        private int m_RunicMaxIntensity;

        public int WeaponFireDamage { get { return m_WeaponFireDamage; } set { m_WeaponFireDamage = value; } }
        public int WeaponColdDamage { get { return m_WeaponColdDamage; } set { m_WeaponColdDamage = value; } }
        public int WeaponPoisonDamage { get { return m_WeaponPoisonDamage; } set { m_WeaponPoisonDamage = value; } }
        public int WeaponEnergyDamage { get { return m_WeaponEnergyDamage; } set { m_WeaponEnergyDamage = value; } }
        public int WeaponDurability { get { return m_WeaponDurability; } set { m_WeaponDurability = value; } }
        public int WeaponLuck { get { return m_WeaponLuck; } set { m_WeaponLuck = value; } }
        public int WeaponGoldIncrease { get { return m_WeaponGoldIncrease; } set { m_WeaponGoldIncrease = value; } }
        public int WeaponLowerRequirements { get { return m_WeaponLowerRequirements; } set { m_WeaponLowerRequirements = value; } }
        public int WeaponDamageIncrease { get { return m_WeaponDamageIncrease; } set { m_WeaponDamageIncrease = value; } }
        public int WeaponMinStr { get { return m_WeaponMinStr; } set { m_WeaponMinStr = value; } }
        public int WeaponSwingSpeed { get { return m_WeaponSpeed; } set { m_WeaponSpeed = value; } }

        public int ArmorPhysicalResist { get { return m_ArmorPhysicalResist; } set { m_ArmorPhysicalResist = value; } }
        public int ArmorFireResist { get { return m_ArmorFireResist; } set { m_ArmorFireResist = value; } }
        public int ArmorColdResist { get { return m_ArmorColdResist; } set { m_ArmorColdResist = value; } }
        public int ArmorPoisonResist { get { return m_ArmorPoisonResist; } set { m_ArmorPoisonResist = value; } }
        public int ArmorEnergyResist { get { return m_ArmorEnergyResist; } set { m_ArmorEnergyResist = value; } }
        public int ArmorDurability { get { return m_ArmorDurability; } set { m_ArmorDurability = value; } }
        public int ArmorLuck { get { return m_ArmorLuck; } set { m_ArmorLuck = value; } }
        public int ArmorGoldIncrease { get { return m_ArmorGoldIncrease; } set { m_ArmorGoldIncrease = value; } }
        public int ArmorLowerRequirements { get { return m_ArmorLowerRequirements; } set { m_ArmorLowerRequirements = value; } }
        public int ArmorReflectPhysical { get { return m_ArmorReflectPhysical; } set { m_ArmorReflectPhysical = value; } }

        public int RunicMinAttributes { get { return m_RunicMinAttributes; } set { m_RunicMinAttributes = value; } }
        public int RunicMaxAttributes { get { return m_RunicMaxAttributes; } set { m_RunicMaxAttributes = value; } }
        public int RunicMinIntensity { get { return m_RunicMinIntensity; } set { m_RunicMinIntensity = value; } }
        public int RunicMaxIntensity { get { return m_RunicMaxIntensity; } set { m_RunicMaxIntensity = value; } }

        public CraftAttributeInfo()
        {
        }

        public static readonly CraftAttributeInfo Blank;
        public static readonly CraftAttributeInfo DullCopper, ShadowIron, Copper, Bronze, Golden, Agapite, Verite, Valorite, Mithril, Bloodrock, Steel, Adamantite, Ithilmar, Silver, Blackrock, Skazz, Electrum;
        public static readonly CraftAttributeInfo Spined, Horned, Barbed, Tufted, Scaled;
        public static readonly CraftAttributeInfo RedScales, YellowScales, BlackScales, GreenScales, WhiteScales, BlueScales;
        public static readonly CraftAttributeInfo Oak, Pine, Redwood, WhitePine, Ashwood, SilverBirch, Yew, BlackOak;
        public static readonly CraftAttributeInfo Frost, Ice, Glacial;

        static CraftAttributeInfo()
        {
            Blank = new CraftAttributeInfo();


            // Teiravon Ice

            CraftAttributeInfo frost = Frost = new CraftAttributeInfo();
            frost.WeaponColdDamage = 50;
            frost.WeaponDurability = -80;
            frost.ArmorDurability = -80;
            frost.ArmorColdResist = 4;
            frost.ArmorFireResist = -4;
            frost.ArmorEnergyResist = 1;
            frost.ArmorPhysicalResist = 2;
            frost.ArmorPoisonResist = 1;

            CraftAttributeInfo ice = Ice = new CraftAttributeInfo();
            ice.WeaponColdDamage = 50;
            ice.WeaponDamageIncrease = 20;
            ice.WeaponDurability = -60;
            ice.ArmorDurability = -60;
            ice.WeaponSwingSpeed = 20;
            ice.ArmorColdResist = 6;
            ice.ArmorFireResist = -4;
            ice.ArmorPhysicalResist = 3;
            ice.ArmorPoisonResist = 2;
            ice.ArmorEnergyResist = 2;

            CraftAttributeInfo glacial = Glacial = new CraftAttributeInfo();
            glacial.WeaponColdDamage = 50;
            glacial.WeaponDamageIncrease = 40;
            glacial.WeaponDurability = -40;
            glacial.WeaponSwingSpeed = 40;
            glacial.ArmorDurability = -40;
            glacial.ArmorColdResist = 8;
            glacial.ArmorFireResist = -4;
            glacial.ArmorPhysicalResist = 5;
            glacial.ArmorPoisonResist = 3;
            glacial.ArmorEnergyResist = 3;

            // Teiravon woods
            CraftAttributeInfo pine = Pine = new CraftAttributeInfo();

            pine.WeaponDamageIncrease = 40;
            pine.WeaponMinStr = 25;

            CraftAttributeInfo redwood = Redwood = new CraftAttributeInfo();

            redwood.WeaponDamageIncrease = 50;
            redwood.WeaponMinStr = 35;

            CraftAttributeInfo whitePine = WhitePine = new CraftAttributeInfo();

            whitePine.WeaponDamageIncrease = 60;
            whitePine.WeaponMinStr = 45;

            CraftAttributeInfo ashwood = Ashwood = new CraftAttributeInfo();

            ashwood.WeaponDamageIncrease = 70;
            ashwood.WeaponMinStr = 55;

            CraftAttributeInfo silverBirch = SilverBirch = new CraftAttributeInfo();

            silverBirch.WeaponDamageIncrease = 80;
            silverBirch.WeaponMinStr = 65;

            CraftAttributeInfo yew = Yew = new CraftAttributeInfo();

            yew.WeaponDamageIncrease = 90;
            yew.WeaponMinStr = 75;

            CraftAttributeInfo blackOak = BlackOak = new CraftAttributeInfo();

            blackOak.WeaponDamageIncrease = 100;
            blackOak.WeaponMinStr = 85;

            CraftAttributeInfo dullCopper = DullCopper = new CraftAttributeInfo();

            dullCopper.ArmorPhysicalResist = 6;
            dullCopper.ArmorDurability = 50;
            dullCopper.ArmorLowerRequirements = 20;
            dullCopper.WeaponDurability = 100;
            dullCopper.WeaponLowerRequirements = 50;
            dullCopper.RunicMinAttributes = 1;
            dullCopper.RunicMaxAttributes = 2;
            dullCopper.RunicMinIntensity = 10;
            dullCopper.RunicMaxIntensity = 35;

            CraftAttributeInfo shadowIron = ShadowIron = new CraftAttributeInfo();

            shadowIron.ArmorPhysicalResist = 2;
            shadowIron.ArmorFireResist = 1;
            shadowIron.ArmorEnergyResist = 5;
            shadowIron.ArmorDurability = 100;
            shadowIron.WeaponColdDamage = 20;
            shadowIron.WeaponDurability = 50;
            shadowIron.RunicMinAttributes = 2;
            shadowIron.RunicMaxAttributes = 2;
            shadowIron.RunicMinIntensity = 20;
            shadowIron.RunicMaxIntensity = 45;

            CraftAttributeInfo copper = Copper = new CraftAttributeInfo();

            copper.ArmorPhysicalResist = 1;
            copper.ArmorFireResist = 1;
            copper.ArmorPoisonResist = 5;
            copper.ArmorEnergyResist = 2;
            copper.WeaponPoisonDamage = 10;
            copper.WeaponEnergyDamage = 20;
            copper.RunicMinAttributes = 2;
            copper.RunicMaxAttributes = 3;
            copper.RunicMinIntensity = 25;
            copper.RunicMaxIntensity = 50;

            CraftAttributeInfo bronze = Bronze = new CraftAttributeInfo();

            bronze.ArmorPhysicalResist = 3;
            bronze.ArmorColdResist = 5;
            bronze.ArmorPoisonResist = 1;
            bronze.ArmorEnergyResist = 1;
            bronze.WeaponFireDamage = 40;
            bronze.RunicMinAttributes = 3;
            bronze.RunicMaxAttributes = 3;
            bronze.RunicMinIntensity = 30;
            bronze.RunicMaxIntensity = 65;

            CraftAttributeInfo silver = Silver = new CraftAttributeInfo();

            silver.ArmorPhysicalResist = 2;
            silver.ArmorFireResist = 2;
            silver.ArmorColdResist = 1;
            silver.ArmorPoisonResist = 1;
            silver.ArmorEnergyResist = 2;
            silver.WeaponColdDamage = 10;
            silver.WeaponEnergyDamage = 10;
            silver.WeaponFireDamage = 20;
            silver.WeaponDurability = -40;
            silver.ArmorDurability = -40;
            silver.WeaponSwingSpeed = 20;
            silver.RunicMinAttributes = 3;
            silver.RunicMaxAttributes = 3;
            silver.RunicMinIntensity = 30;
            silver.RunicMaxIntensity = 65;

            CraftAttributeInfo golden = Golden = new CraftAttributeInfo();

            golden.ArmorPhysicalResist = 1;
            golden.ArmorFireResist = 1;
            golden.ArmorColdResist = 2;
            golden.ArmorEnergyResist = 2;
            golden.ArmorLuck = 40;
            golden.ArmorLowerRequirements = 30;
            golden.WeaponLuck = 40;
            golden.WeaponLowerRequirements = 50;
            golden.RunicMinAttributes = 3;
            golden.RunicMaxAttributes = 4;
            golden.RunicMinIntensity = 35;
            golden.RunicMaxIntensity = 75;

            CraftAttributeInfo agapite = Agapite = new CraftAttributeInfo();

            agapite.ArmorPhysicalResist = 2;
            agapite.ArmorFireResist = 3;
            agapite.ArmorColdResist = 2;
            agapite.ArmorPoisonResist = 2;
            agapite.ArmorEnergyResist = 2;
            agapite.WeaponColdDamage = 30;
            agapite.WeaponEnergyDamage = 20;
            agapite.RunicMinAttributes = 4;
            agapite.RunicMaxAttributes = 4;
            agapite.RunicMinIntensity = 40;
            agapite.RunicMaxIntensity = 80;

            CraftAttributeInfo verite = Verite = new CraftAttributeInfo();

            verite.ArmorPhysicalResist = 3;
            verite.ArmorFireResist = 3;
            verite.ArmorColdResist = 2;
            verite.ArmorPoisonResist = 3;
            verite.ArmorEnergyResist = 1;
            verite.WeaponPoisonDamage = 40;
            verite.WeaponEnergyDamage = 20;
            verite.RunicMinAttributes = 4;
            verite.RunicMaxAttributes = 5;
            verite.RunicMinIntensity = 45;
            verite.RunicMaxIntensity = 90;

            CraftAttributeInfo valorite = Valorite = new CraftAttributeInfo();

            valorite.ArmorPhysicalResist = 4;
            valorite.ArmorColdResist = 3;
            valorite.ArmorPoisonResist = 3;
            valorite.ArmorEnergyResist = 3;
            valorite.ArmorDurability = 50;
            valorite.WeaponFireDamage = 10;
            valorite.WeaponColdDamage = 20;
            valorite.WeaponPoisonDamage = 10;
            valorite.WeaponEnergyDamage = 20;
            valorite.RunicMinAttributes = 5;
            valorite.RunicMaxAttributes = 5;
            valorite.RunicMinIntensity = 50;
            valorite.RunicMaxIntensity = 100;

            CraftAttributeInfo blackrock = Blackrock = new CraftAttributeInfo();

            blackrock.ArmorPhysicalResist = 4;
            blackrock.ArmorFireResist = 3;
            blackrock.ArmorColdResist = 1;
            blackrock.ArmorPoisonResist = 2;
            blackrock.ArmorEnergyResist = 3;
            blackrock.ArmorDurability = 100;
            blackrock.ArmorLowerRequirements = -20;
            blackrock.WeaponColdDamage = 30;
            blackrock.WeaponPoisonDamage = 30;
            blackrock.RunicMinAttributes = 5;
            blackrock.RunicMaxAttributes = 5;
            blackrock.RunicMinIntensity = 50;
            blackrock.RunicMaxIntensity = 100;

            CraftAttributeInfo mithril = Mithril = new CraftAttributeInfo();

            mithril.ArmorFireResist = 4;
            mithril.ArmorColdResist = 4;
            mithril.ArmorPoisonResist = 4;
            mithril.ArmorEnergyResist = 4;
            mithril.ArmorDurability = 50;

            CraftAttributeInfo bloodrock = Bloodrock = new CraftAttributeInfo();

            bloodrock.ArmorPhysicalResist = 4;
            bloodrock.ArmorColdResist = 4;
            bloodrock.ArmorPoisonResist = 4;
            bloodrock.ArmorEnergyResist = 4;
            bloodrock.ArmorDurability = 50;
            bloodrock.WeaponPoisonDamage = 50;

            CraftAttributeInfo skazz = Skazz = new CraftAttributeInfo();

            skazz.ArmorPhysicalResist = 2;
            skazz.ArmorFireResist = 2;
            skazz.ArmorColdResist = 2;
            skazz.ArmorEnergyResist = 2;
            skazz.ArmorPoisonResist = 2;
            skazz.ArmorDurability = -80;
            skazz.WeaponPoisonDamage = 80;

            skazz.ArmorLowerRequirements = 90;
            skazz.WeaponLowerRequirements = 90;
            skazz.WeaponDurability = -80;

            CraftAttributeInfo electrum = Electrum = new CraftAttributeInfo();

            electrum.ArmorPhysicalResist = 2;
            electrum.ArmorFireResist = 2;
            electrum.ArmorColdResist = 1;
            electrum.ArmorPoisonResist = 1;
            electrum.ArmorEnergyResist = 2;
            electrum.WeaponColdDamage = 10;
            electrum.WeaponEnergyDamage = 10;
            electrum.WeaponFireDamage = 20;
            electrum.WeaponDurability = -20;
            electrum.ArmorDurability = -20;
            electrum.WeaponSwingSpeed = 20;
            electrum.RunicMinAttributes = 3;
            electrum.RunicMaxAttributes = 3;
            electrum.RunicMinIntensity = 30;
            electrum.RunicMaxIntensity = 65;

            electrum.ArmorLuck = 50;
            electrum.WeaponLuck = 50;
            electrum.ArmorLowerRequirements = 30;
            electrum.WeaponLowerRequirements = 30;
            electrum.ArmorGoldIncrease = 20;
            electrum.WeaponGoldIncrease = 20;

            CraftAttributeInfo steel = Steel = new CraftAttributeInfo();

            steel.ArmorPhysicalResist = 4;
            steel.ArmorColdResist = 4;
            steel.ArmorFireResist = 4;
            steel.ArmorEnergyResist = 4;
            steel.ArmorDurability = 50;
            steel.WeaponEnergyDamage = 50;

            CraftAttributeInfo adamantite = Adamantite = new CraftAttributeInfo();

            adamantite.ArmorPhysicalResist = 4;
            adamantite.ArmorColdResist = 4;
            adamantite.ArmorPoisonResist = 4;
            adamantite.ArmorFireResist = 4;
            adamantite.ArmorDurability = 50;
            adamantite.WeaponColdDamage = 50;

            CraftAttributeInfo ithilmar = Ithilmar = new CraftAttributeInfo();

            ithilmar.ArmorPhysicalResist = 4;
            ithilmar.ArmorFireResist = 4;
            ithilmar.ArmorPoisonResist = 4;
            ithilmar.ArmorEnergyResist = 4;
            ithilmar.ArmorLowerRequirements = 50;
            ithilmar.ArmorDurability = 50;
            ithilmar.WeaponFireDamage = 50;

            CraftAttributeInfo spined = Spined = new CraftAttributeInfo();

            spined.ArmorPhysicalResist = 5;
            spined.ArmorLuck = 40;
            spined.RunicMinAttributes = 1;
            spined.RunicMaxAttributes = 3;
            spined.RunicMinIntensity = 20;
            spined.RunicMaxIntensity = 40;

            CraftAttributeInfo horned = Horned = new CraftAttributeInfo();

            horned.ArmorPhysicalResist = 2;
            horned.ArmorFireResist = 3;
            horned.ArmorColdResist = 2;
            horned.ArmorPoisonResist = 2;
            horned.ArmorEnergyResist = 2;
            horned.RunicMinAttributes = 3;
            horned.RunicMaxAttributes = 4;
            horned.RunicMinIntensity = 30;
            horned.RunicMaxIntensity = 70;

            CraftAttributeInfo barbed = Barbed = new CraftAttributeInfo();

            barbed.ArmorPhysicalResist = 2;
            barbed.ArmorFireResist = 1;
            barbed.ArmorColdResist = 2;
            barbed.ArmorPoisonResist = 3;
            barbed.ArmorEnergyResist = 4;
            barbed.RunicMinAttributes = 4;
            barbed.RunicMaxAttributes = 5;
            barbed.RunicMinIntensity = 40;
            barbed.RunicMaxIntensity = 100;

            CraftAttributeInfo tufted = Tufted = new CraftAttributeInfo();
            
            tufted.ArmorPhysicalResist = 1;
            tufted.ArmorFireResist = 1;
            tufted.ArmorColdResist = 4;
            tufted.ArmorPoisonResist = 1;
            tufted.ArmorEnergyResist = 2;
            tufted.RunicMinAttributes = 3;
            tufted.RunicMaxAttributes = 4;
            tufted.RunicMinIntensity = 30;
            tufted.RunicMaxIntensity = 80;

            CraftAttributeInfo scaled = Scaled = new CraftAttributeInfo();

            scaled.ArmorPhysicalResist = 4;
            scaled.ArmorFireResist = 3;
            scaled.ArmorColdResist = 1;
            scaled.ArmorPoisonResist = 3;
            scaled.ArmorEnergyResist = 3;
            scaled.RunicMinAttributes = 4;
            scaled.RunicMaxAttributes = 5;
            scaled.RunicMinIntensity = 40;
            scaled.RunicMaxIntensity = 100;

            CraftAttributeInfo red = RedScales = new CraftAttributeInfo();

            red.ArmorPhysicalResist = 3;
            red.ArmorFireResist = 3;
            red.ArmorColdResist = 2;
            red.ArmorPoisonResist = 2;
            red.ArmorEnergyResist = 2;

            CraftAttributeInfo yellow = YellowScales = new CraftAttributeInfo();

            yellow.ArmorPhysicalResist = 3;
            yellow.ArmorFireResist = 2;
            yellow.ArmorColdResist = 2;
            yellow.ArmorPoisonResist = 3;
            yellow.ArmorEnergyResist = 2;
            yellow.ArmorLuck = 20;

            CraftAttributeInfo black = BlackScales = new CraftAttributeInfo();

            black.ArmorPhysicalResist = 2;
            black.ArmorFireResist = 3;
            black.ArmorColdResist = 3;
            black.ArmorPoisonResist = 2;
            black.ArmorEnergyResist = 2;

            CraftAttributeInfo green = GreenScales = new CraftAttributeInfo();

            green.ArmorPhysicalResist = 2;
            green.ArmorFireResist = 2;
            green.ArmorColdResist = 2;
            green.ArmorPoisonResist = 3;
            green.ArmorEnergyResist = 3;

            CraftAttributeInfo white = WhiteScales = new CraftAttributeInfo();

            white.ArmorPhysicalResist = 3;
            white.ArmorFireResist = 2;
            white.ArmorColdResist = 2;
            white.ArmorPoisonResist = 2;
            white.ArmorEnergyResist = 3;

            CraftAttributeInfo blue = BlueScales = new CraftAttributeInfo();

            blue.ArmorPhysicalResist = 3;
            blue.ArmorFireResist = 2;
            blue.ArmorColdResist = 3;
            blue.ArmorPoisonResist = 2;
            blue.ArmorEnergyResist = 2;
        }
    }

    public class CraftResourceInfo
    {
        private int m_Hue;
        private int m_Number;
        private string m_Name;
        private CraftAttributeInfo m_AttributeInfo;
        private CraftResource m_Resource;
        private Type[] m_ResourceTypes;

        public int Hue { get { return m_Hue; } }
        public int Number { get { return m_Number; } }
        public string Name { get { return m_Name; } }
        public CraftAttributeInfo AttributeInfo { get { return m_AttributeInfo; } }
        public CraftResource Resource { get { return m_Resource; } }
        public Type[] ResourceTypes { get { return m_ResourceTypes; } }

        public CraftResourceInfo(int hue, int number, string name, CraftAttributeInfo attributeInfo, CraftResource resource, params Type[] resourceTypes)
        {
            m_Hue = hue;
            m_Number = number;
            m_Name = name;
            m_AttributeInfo = attributeInfo;
            m_Resource = resource;
            m_ResourceTypes = resourceTypes;

            for (int i = 0; i < resourceTypes.Length; ++i)
                CraftResources.RegisterType(resourceTypes[i], resource);
        }

        public CraftResourceInfo(int hue, string name, CraftAttributeInfo attributeInfo, CraftResource resource, params Type[] resourceTypes)
        {
            m_Hue = hue;
            m_Name = name;
            m_AttributeInfo = attributeInfo;
            m_Resource = resource;
            m_ResourceTypes = resourceTypes;

            for (int i = 0; i < resourceTypes.Length; ++i)
                CraftResources.RegisterType(resourceTypes[i], resource);
        }

    }

    public class CraftResources
    {
        private static CraftResourceInfo[] m_MetalInfo = new CraftResourceInfo[]
			{
				new CraftResourceInfo( 0x000, 1053109, "Iron",			CraftAttributeInfo.Blank,		CraftResource.Iron,				typeof( IronIngot ),		typeof( IronOre ),			typeof( Granite ) ),
				new CraftResourceInfo( 0x973, 1053108, "Dull Copper",	CraftAttributeInfo.DullCopper,	CraftResource.DullCopper,		typeof( DullCopperIngot ),	typeof( DullCopperOre ),	typeof( DullCopperGranite ) ),
				new CraftResourceInfo( 0x966, 1053107, "Shadow Iron",	CraftAttributeInfo.ShadowIron,	CraftResource.ShadowIron,		typeof( ShadowIronIngot ),	typeof( ShadowIronOre ),	typeof( ShadowIronGranite ) ),
				new CraftResourceInfo( 0x96D, 1053106, "Copper",		CraftAttributeInfo.Copper,		CraftResource.Copper,			typeof( CopperIngot ),		typeof( CopperOre ),		typeof( CopperGranite ) ),
				new CraftResourceInfo( 0x972, 1053105, "Bronze",		CraftAttributeInfo.Bronze,		CraftResource.Bronze,			typeof( BronzeIngot ),		typeof( BronzeOre ),		typeof( BronzeGranite ) ),
				new CraftResourceInfo( 0x8A5, 1053104, "Gold",			CraftAttributeInfo.Golden,		CraftResource.Gold,				typeof( GoldIngot ),		typeof( GoldOre ),			typeof( GoldGranite ) ),
				new CraftResourceInfo( 0x979, 1053103, "Agapite",		CraftAttributeInfo.Agapite,		CraftResource.Agapite,			typeof( AgapiteIngot ),		typeof( AgapiteOre ),		typeof( AgapiteGranite ) ),
				new CraftResourceInfo( 0x89F, 1053102, "Verite",		CraftAttributeInfo.Verite,		CraftResource.Verite,			typeof( VeriteIngot ),		typeof( VeriteOre ),		typeof( VeriteGranite ) ),
				new CraftResourceInfo( 0x8AB, 1053101, "Valorite",		CraftAttributeInfo.Valorite,	CraftResource.Valorite,			typeof( ValoriteIngot ),	typeof( ValoriteOre ),		typeof( ValoriteGranite ) ),
				new CraftResourceInfo( 0x8B5, 0,	   "Mithril",		CraftAttributeInfo.Mithril,		CraftResource.Mithril,			typeof( MithrilIngot ),		typeof( MithrilOre ),		typeof( MithrilGranite ) ),
				new CraftResourceInfo( 0x8CD, 0,	   "Bloodrock",		CraftAttributeInfo.Bloodrock,	CraftResource.Bloodrock,		typeof( BloodrockIngot ),	typeof( BloodrockOre ),		typeof( BloodrockGranite ) ),
				new CraftResourceInfo( 0x8FD, 0,	   "Steel",			CraftAttributeInfo.Steel,		CraftResource.Steel,			typeof( SteelIngot ),		typeof( SteelOre ),		typeof( SteelGranite ) ),
				new CraftResourceInfo( 0xA00, 0,	   "Adamantite",	CraftAttributeInfo.Adamantite,	CraftResource.Adamantite,		typeof( AdamantiteIngot ),	typeof( AdamantiteOre ),		typeof( MithrilGranite ) ),
				new CraftResourceInfo( 0x8F4, 0,	   "Ithilmar",		CraftAttributeInfo.Ithilmar,	CraftResource.Ithilmar,			typeof( IthilmarIngot ),	typeof( IthilmarOre ),		typeof( MithrilGranite ) ),
                new CraftResourceInfo( 0xB6E, 0,       "Silver",        CraftAttributeInfo.Silver,      CraftResource.Silver,           typeof( SilverIngot),       typeof( SilverOre),         typeof( SilverGranite) ),
                new CraftResourceInfo(  2626, 0,        "Blackrock",    CraftAttributeInfo.Blackrock,   CraftResource.Blackrock,        typeof(BlackrockIngot),     typeof(BlackrockOre)),
                new CraftResourceInfo( 0x9F4, 0,       "Ferite",        CraftAttributeInfo.Blank,       CraftResource.Ferite,           typeof(FeriteIngot)),
                new CraftResourceInfo( 0x9F3, 0,       "Malachite",     CraftAttributeInfo.Blank,       CraftResource.Malachite,        typeof(MalachiteIngot)),
                new CraftResourceInfo( 0x9B5, 0,       "Pyrite",        CraftAttributeInfo.Blank,       CraftResource.Pyrite,           typeof(PyriteIngot)),
                new CraftResourceInfo( 0x9BA, 0,       "Umbrite",       CraftAttributeInfo.Blank,       CraftResource.Umbrite,          typeof(UmbriteIngot)),
                new CraftResourceInfo( 0x9EF, 0,       "Amirite",       CraftAttributeInfo.Blank,       CraftResource.Amirite,          typeof(AmiriteIngot)),
                new CraftResourceInfo( 0xA53, 0,       "Skazz",         CraftAttributeInfo.Skazz,       CraftResource.Skazz,            typeof(SkazzIngot)),
                new CraftResourceInfo( 0x9B5, 0,       "Electrum",      CraftAttributeInfo.Electrum,    CraftResource.Electrum,         typeof(ElectrumIngot)),
			};

        private static CraftResourceInfo[] m_ScaleInfo = new CraftResourceInfo[]
			{
				new CraftResourceInfo( 0x66D, 1053129, "Red Scales",	CraftAttributeInfo.RedScales,		CraftResource.RedScales,		typeof( RedScales ) ),
				new CraftResourceInfo( 0x8A8, 1053130, "Yellow Scales",	CraftAttributeInfo.YellowScales,	CraftResource.YellowScales,		typeof( YellowScales ) ),
				new CraftResourceInfo( 0x455, 1053131, "Black Scales",	CraftAttributeInfo.BlackScales,		CraftResource.BlackScales,		typeof( BlackScales ) ),
				new CraftResourceInfo( 0x851, 1053132, "Green Scales",	CraftAttributeInfo.GreenScales,		CraftResource.GreenScales,		typeof( GreenScales ) ),
				new CraftResourceInfo( 0x8FD, 1053133, "White Scales",	CraftAttributeInfo.WhiteScales,		CraftResource.WhiteScales,		typeof( WhiteScales ) ),
				new CraftResourceInfo( 0x8B0, 1053134, "Blue Scales",	CraftAttributeInfo.BlueScales,		CraftResource.BlueScales,		typeof( BlueScales ) )
			};

        private static CraftResourceInfo[] m_LeatherInfo = new CraftResourceInfo[]
			{
				new CraftResourceInfo( 0x000, 1049353, "Normal",		CraftAttributeInfo.Blank,		CraftResource.RegularLeather,	typeof( Leather ),			typeof( Hides ) ),
				new CraftResourceInfo( 0x283, 1049354, "Spined",		CraftAttributeInfo.Spined,		CraftResource.SpinedLeather,	typeof( SpinedLeather ),	typeof( SpinedHides ) ),
				new CraftResourceInfo( 0x227, 1049355, "Horned",		CraftAttributeInfo.Horned,		CraftResource.HornedLeather,	typeof( HornedLeather ),	typeof( HornedHides ) ),
				new CraftResourceInfo( 0x1C1, 1049356, "Barbed",		CraftAttributeInfo.Barbed,		CraftResource.BarbedLeather,	typeof( BarbedLeather ),	typeof( BarbedHides ) ),
                new CraftResourceInfo( 0x835, 0, "Tufted",		CraftAttributeInfo.Tufted,		CraftResource.TuftedLeather,	typeof( TuftedLeather ),	typeof( TuftedHides ) ),
                new CraftResourceInfo( 0x5A6, 0, "Scaled",		CraftAttributeInfo.Scaled,		CraftResource.ScaledLeather,	typeof( ScaledLeather ),	typeof( ScaledHides ) ),
		};

        private static CraftResourceInfo[] m_AOSLeatherInfo = new CraftResourceInfo[]
			{
				new CraftResourceInfo( 0x000, 1049353, "Normal",		CraftAttributeInfo.Blank,		CraftResource.RegularLeather,	typeof( Leather ),			typeof( Hides ) ),
				new CraftResourceInfo( 0x8AC, 1049354, "Spined",		CraftAttributeInfo.Spined,		CraftResource.SpinedLeather,	typeof( SpinedLeather ),	typeof( SpinedHides ) ),
				new CraftResourceInfo( 0x845, 1049355, "Horned",		CraftAttributeInfo.Horned,		CraftResource.HornedLeather,	typeof( HornedLeather ),	typeof( HornedHides ) ),
				new CraftResourceInfo( 0x851, 1049356, "Barbed",		CraftAttributeInfo.Barbed,		CraftResource.BarbedLeather,	typeof( BarbedLeather ),	typeof( BarbedHides ) ),
                new CraftResourceInfo( 0x835, 0, "Tufted",		CraftAttributeInfo.Tufted,		CraftResource.TuftedLeather,	typeof( TuftedLeather ),	typeof( TuftedHides ) ),
                 new CraftResourceInfo( 0x5A6, 0, "Scaled",		CraftAttributeInfo.Scaled,		CraftResource.ScaledLeather,	typeof( ScaledLeather ),	typeof( ScaledHides ) ),
		};

        private static CraftResourceInfo[] m_WoodInfo = new CraftResourceInfo[]
			{
				new CraftResourceInfo( 0x000,		"Oak",			CraftAttributeInfo.Blank,				CraftResource.Oak,				typeof( Log ),				typeof( Board ) ),
				new CraftResourceInfo( 0x0FA,		"Pine",			CraftAttributeInfo.Pine,				CraftResource.Pine,				typeof( PineLog ),			typeof( PineBoard ) ),
				new CraftResourceInfo( 0x750,		"Redwood",		CraftAttributeInfo.Redwood,			CraftResource.Redwood,			typeof( RedwoodLog ),		typeof( RedwoodBoard ) ),
				new CraftResourceInfo( 0xB9B,		"White Pine",		CraftAttributeInfo.WhitePine,			CraftResource.WhitePine,			typeof( WhitePineLog ),		typeof( WhitePineBoard ) ),
				new CraftResourceInfo( 0x9F9,		"Ashwood",		CraftAttributeInfo.Ashwood,			CraftResource.Ashwood,			typeof( AshwoodLog ),		typeof( AshwoodBoard ) ),
				new CraftResourceInfo( 0x8B5,		"Silver Birch",		CraftAttributeInfo.SilverBirch,			CraftResource.SilverBirch,			typeof( SilverBirchLog ),		typeof( SilverBirchBoard ) ),
				new CraftResourceInfo( 0x715,		"Yew",			CraftAttributeInfo.Yew,				CraftResource.Yew,				typeof( YewLog ),			typeof( YewBoard ) ),
				new CraftResourceInfo( 0x455,		"Black Oak",		CraftAttributeInfo.BlackOak,			CraftResource.BlackOak,			typeof( BlackOakLog ),		typeof( BlackOakBoard ) ),
		};

        private static CraftResourceInfo[] m_IceInfo = new CraftResourceInfo[]
        {
            new CraftResourceInfo( 2291, "Frost",	CraftAttributeInfo.Frost,		CraftResource.Frost,	typeof( Frost ) ),
            new CraftResourceInfo( 2492, "Ice",	    CraftAttributeInfo.Ice,		    CraftResource.Ice,		typeof( Ice ) ),
            new CraftResourceInfo( 2151, "Glacial Ice",	    CraftAttributeInfo.Glacial,		    CraftResource.Glacial,		typeof( GlacialIce ) )
        };

        /// <summary>
        /// Returns true if '<paramref name="resource"/>' is None, Iron, or RegularLeather. False if otherwise.
        /// </summary>
        public static bool IsStandard(CraftResource resource)
        {
            return (resource == CraftResource.None || resource == CraftResource.Iron || resource == CraftResource.RegularLeather);
        }

        private static Hashtable m_TypeTable;

        /// <summary>
        /// Registers that '<paramref name="resourceType"/>' uses '<paramref name="resource"/>' so that it can later be queried by <see cref="CraftResources.GetFromType"/>
        /// </summary>
        public static void RegisterType(Type resourceType, CraftResource resource)
        {
            if (m_TypeTable == null)
                m_TypeTable = new Hashtable();

            m_TypeTable[resourceType] = resource;
        }

        /// <summary>
        /// Returns the <see cref="CraftResource"/> value for which '<paramref name="resourceType"/>' uses -or- CraftResource.None if an unregistered type was specified.
        /// </summary>
        public static CraftResource GetFromType(Type resourceType)
        {
            if (m_TypeTable == null)
                return CraftResource.None;

            object obj = m_TypeTable[resourceType];

            if (!(obj is CraftResource))
                return CraftResource.None;

            return (CraftResource)obj;
        }

        /// <summary>
        /// Returns a <see cref="CraftResourceInfo"/> instance describing '<paramref name="resource"/>' -or- null if an invalid resource was specified.
        /// </summary>
        public static CraftResourceInfo GetInfo(CraftResource resource)
        {
            CraftResourceInfo[] list = null;

            switch (GetType(resource))
            {
                case CraftResourceType.Metal: list = m_MetalInfo; break;
                case CraftResourceType.Leather: list = Core.AOS ? m_AOSLeatherInfo : m_LeatherInfo; break;
                case CraftResourceType.Scales: list = m_ScaleInfo; break;
                case CraftResourceType.Wood: list = m_WoodInfo; break;
                case CraftResourceType.Ice: list = m_IceInfo; break;
            }

            if (list != null)
            {
                int index = GetIndex(resource);

                if (index >= 0 && index < list.Length)
                    return list[index];
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="CraftResourceType"/> value indiciating the type of '<paramref name="resource"/>'.
        /// </summary>
        public static CraftResourceType GetType(CraftResource resource)
        {
            if (resource >= CraftResource.Iron && resource <= CraftResource.Electrum)
                return CraftResourceType.Metal;

            if (resource >= CraftResource.RegularLeather && resource <= CraftResource.ScaledLeather)
                return CraftResourceType.Leather;

            if (resource >= CraftResource.RedScales && resource <= CraftResource.BlueScales)
                return CraftResourceType.Scales;

            if (resource >= CraftResource.Oak && resource <= CraftResource.BlackOak)
                return CraftResourceType.Wood;

            if (resource >= CraftResource.Frost && resource <= CraftResource.Glacial)
                return CraftResourceType.Ice;

            return CraftResourceType.None;
        }

        /// <summary>
        /// Returns the first <see cref="CraftResource"/> in the series of resources for which '<paramref name="resource"/>' belongs.
        /// </summary>
        public static CraftResource GetStart(CraftResource resource)
        {
            switch (GetType(resource))
            {
                case CraftResourceType.Metal: return CraftResource.Iron;
                case CraftResourceType.Leather: return CraftResource.RegularLeather;
                case CraftResourceType.Scales: return CraftResource.RedScales;
                case CraftResourceType.Wood: return CraftResource.Oak;
                case CraftResourceType.Ice: return CraftResource.Frost;
            }

            return CraftResource.None;
        }

        /// <summary>
        /// Returns the index of '<paramref name="resource"/>' in the seriest of resources for which it belongs.
        /// </summary>
        public static int GetIndex(CraftResource resource)
        {
            CraftResource start = GetStart(resource);

            if (start == CraftResource.None)
                return 0;

            return (int)(resource - start);
        }

        /// <summary>
        /// Returns the <see cref="CraftResourceInfo.Number"/> property of '<paramref name="resource"/>' -or- 0 if an invalid resource was specified.
        /// </summary>
        public static int GetLocalizationNumber(CraftResource resource)
        {
            CraftResourceInfo info = GetInfo(resource);

            return (info == null ? 0 : info.Number);
        }

        /// <summary>
        /// Returns the <see cref="CraftResourceInfo.Hue"/> property of '<paramref name="resource"/>' -or- 0 if an invalid resource was specified.
        /// </summary>
        public static int GetHue(CraftResource resource)
        {
            CraftResourceInfo info = GetInfo(resource);

            return (info == null ? 0 : info.Hue);
        }

        /// <summary>
        /// Returns the <see cref="CraftResourceInfo.Name"/> property of '<paramref name="resource"/>' -or- an empty string if the resource specified was invalid.
        /// </summary>
        public static string GetName(CraftResource resource)
        {
            CraftResourceInfo info = GetInfo(resource);

            return (info == null ? String.Empty : info.Name);
        }

        /// <summary>
        /// Returns the <see cref="CraftResource"/> value which represents '<paramref name="info"/>' -or- CraftResource.None if unable to convert.
        /// </summary>
        public static CraftResource GetFromOreInfo(OreInfo info)
        {
            if (info.Name.IndexOf("Spined") >= 0)
                return CraftResource.SpinedLeather;
            else if (info.Name.IndexOf("Horned") >= 0)
                return CraftResource.HornedLeather;
            else if (info.Name.IndexOf("Barbed") >= 0)
                return CraftResource.BarbedLeather;
            else if (info.Name.IndexOf("Tufted") >= 0)
                return CraftResource.TuftedLeather;
            else if (info.Name.IndexOf("Scaled") >= 0)
                return CraftResource.ScaledLeather;
            else if (info.Name.IndexOf("Leather") >= 0)
                return CraftResource.RegularLeather;

            if (info.Name.IndexOf("Frost") >= 0)
                return CraftResource.Frost;
            else if (info.Name.IndexOf("Ice") >= 0)
                return CraftResource.Ice;
            else if (info.Name.IndexOf("Glacial Ice") >= 0)
                return CraftResource.Glacial;

            if (info.Level == 0)
                return CraftResource.Iron;
            else if (info.Level == 1)
                return CraftResource.DullCopper;
            else if (info.Level == 2)
                return CraftResource.ShadowIron;
            else if (info.Level == 3)
                return CraftResource.Copper;
            else if (info.Level == 4)
                return CraftResource.Bronze;
            else if (info.Level == 5)
                return CraftResource.Gold;
            else if (info.Level == 6)
                return CraftResource.Agapite;
            else if (info.Level == 7)
                return CraftResource.Verite;
            else if (info.Level == 8)
                return CraftResource.Valorite;
            else if (info.Level == 9)
                return CraftResource.Mithril;
            else if (info.Level == 10)
                return CraftResource.Bloodrock;
            else if (info.Level == 11)
                return CraftResource.Steel;
            else if (info.Level == 12)
                return CraftResource.Adamantite;
            else if (info.Level == 13)
                return CraftResource.Ithilmar;
            else if (info.Level == 14)
                return CraftResource.Silver;
            else if (info.Level == 15)
                return CraftResource.Blackrock;
            else if (info.Level == 16)
                return CraftResource.Ferite;
            else if (info.Level == 17)
                return CraftResource.Malachite;
            else if (info.Level == 18)
                return CraftResource.Pyrite;
            else if (info.Level == 19)
                return CraftResource.Umbrite;
            else if (info.Level == 20)
                return CraftResource.Amirite;
            else if (info.Level == 21)
                return CraftResource.Oak;
            else if (info.Level == 22)
                return CraftResource.Pine;
            else if (info.Level == 23)
                return CraftResource.Redwood;
            else if (info.Level == 24)
                return CraftResource.WhitePine;
            else if (info.Level == 25)
                return CraftResource.Ashwood;
            else if (info.Level == 26)
                return CraftResource.SilverBirch;
            else if (info.Level == 27)
                return CraftResource.Yew;
            else if (info.Level == 28)
                return CraftResource.BlackOak;
            else if (info.Level == 29)
                return CraftResource.RedScales;
            else if (info.Level == 30)
                return CraftResource.YellowScales;
            else if (info.Level == 31)
                return CraftResource.BlackScales;
            else if (info.Level == 32)
                return CraftResource.GreenScales;
            else if (info.Level == 33)
                return CraftResource.WhiteScales;
            else if (info.Level == 34)
                return CraftResource.BlueScales;


            return CraftResource.None;
        }

        /// <summary>
        /// Returns the <see cref="CraftResource"/> value which represents '<paramref name="info"/>', using '<paramref name="material"/>' to help resolve leather OreInfo instances.
        /// </summary>
        public static CraftResource GetFromOreInfo(OreInfo info, ArmorMaterialType material)
        {
            if (material == ArmorMaterialType.Studded || material == ArmorMaterialType.Leather || material == ArmorMaterialType.Spined ||
                material == ArmorMaterialType.Horned || material == ArmorMaterialType.Barbed || material == ArmorMaterialType.Tufted || material == ArmorMaterialType.Scaled)
            {
                if (info.Level == 0)
                    return CraftResource.RegularLeather;
                else if (info.Level == 1)
                    return CraftResource.SpinedLeather;
                else if (info.Level == 2)
                    return CraftResource.HornedLeather;
                else if (info.Level == 3)
                    return CraftResource.BarbedLeather;
                else if (info.Level == 4)
                    return CraftResource.TuftedLeather;
                else if (info.Level == 5)
                    return CraftResource.ScaledLeather;

                return CraftResource.None;
            }
           
            return GetFromOreInfo(info);
        }
    }

    // NOTE: This class is only for compatability with very old RunUO versions.
    // No changes to it should be required for custom resources.
    public class OreInfo
    {
        public static readonly OreInfo Iron = new OreInfo(0, 0x000, "Iron");
        public static readonly OreInfo DullCopper = new OreInfo(1, 0x973, "Dull Copper");
        public static readonly OreInfo ShadowIron = new OreInfo(2, 0x966, "Shadow Iron");
        public static readonly OreInfo Copper = new OreInfo(3, 0x96D, "Copper");
        public static readonly OreInfo Bronze = new OreInfo(4, 0x972, "Bronze");
        public static readonly OreInfo Gold = new OreInfo(5, 0x8A5, "Gold");
        public static readonly OreInfo Agapite = new OreInfo(6, 0x979, "Agapite");
        public static readonly OreInfo Verite = new OreInfo(7, 0x89F, "Verite");
        public static readonly OreInfo Valorite = new OreInfo(8, 0x8AB, "Valorite");
        public static readonly OreInfo Mithril = new OreInfo(9, 0x8B5, "Mithril");
        public static readonly OreInfo Bloodrock = new OreInfo(10, 0x8CD, "Bloodrock");
        public static readonly OreInfo Steel = new OreInfo(11, 0x8FD, "Steel");
        public static readonly OreInfo Adamantite = new OreInfo(12, 0xA00, "Adamantite");
        public static readonly OreInfo Ithilmar = new OreInfo(13, 0x8F4, "Ithilmar");
        public static readonly OreInfo Silver = new OreInfo(14, 0xB6E, "Silver");
        public static readonly OreInfo Blackrock = new OreInfo(15, 2626, "Blackrock");
        public static readonly OreInfo Ferite = new OreInfo(16, 0x9F4, "Ferite");
        public static readonly OreInfo Malachite = new OreInfo(17, 0x9F3, "Malachite");
        public static readonly OreInfo Pyrite = new OreInfo(18, 0x9B5, "Pyrite");
        public static readonly OreInfo Umbrite = new OreInfo(19, 0x9BA, "Umbrite");
        public static readonly OreInfo Amirite = new OreInfo(20, 0x9EF, "Amirite");
        public static readonly OreInfo Oak = new OreInfo(21, 0x000, "Oak");
        public static readonly OreInfo Pine = new OreInfo(22, 0x0FA, "Pine");
        public static readonly OreInfo Redwood = new OreInfo(23, 0x750, "Redwood");
        public static readonly OreInfo WhitePine = new OreInfo(24, 0xB9B, "White Pine");
        public static readonly OreInfo Ashwood = new OreInfo(25, 0x9F9, "Ashwood");
        public static readonly OreInfo SilverBirch = new OreInfo(26, 0x8B5, "Silver Birch");
        public static readonly OreInfo Yew = new OreInfo(27, 0x715, "Yew");
        public static readonly OreInfo BlackOak = new OreInfo(28, 0x455, "Black Oak");
        public static readonly OreInfo RedScales = new OreInfo(29, 0x66D, "Red Scales");
        public static readonly OreInfo YellowScales = new OreInfo(30, 0x8A8, "Yellow Scales");
        public static readonly OreInfo BlackScales = new OreInfo(31, 0x455, "Black Scales");
        public static readonly OreInfo GreenScales = new OreInfo(32, 0x851, "Green Scales");
        public static readonly OreInfo WhiteScales = new OreInfo(33, 0x8FD, "White Scales");
        public static readonly OreInfo BlueScales = new OreInfo(34, 0x8B0, "Blue Scales");
        public static readonly OreInfo Frost = new OreInfo(35, 2291, "Frosty");
        public static readonly OreInfo Ice = new OreInfo(36, 2492, "Icy");
        public static readonly OreInfo Glacial = new OreInfo(37, 2151, "Glacial");



        private int m_Level;
        private int m_Hue;
        private string m_Name;

        public OreInfo(int level, int hue, string name)
        {
            m_Level = level;
            m_Hue = hue;
            m_Name = name;
        }

        public int Level
        {
            get
            {
                return m_Level;
            }
        }

        public int Hue
        {
            get
            {
                return m_Hue;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
        }
    }
}
