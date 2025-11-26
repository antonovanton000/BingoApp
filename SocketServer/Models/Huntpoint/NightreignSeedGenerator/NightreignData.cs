using System.Collections;
using static RandomizerCommon.NightreignData;

#nullable enable
namespace RandomizerCommon;

public class NightreignData
{

    public static readonly Dictionary<NightreignData.TargetBoss, string> TargetBossNames = NightreignData.EnumNames<NightreignData.TargetBoss>();
    public enum RareMap
    {
        Unspecified = 0,
        Default = 10, // 0x0000000A
        Mountaintop = 11, // 0x0000000B
        Crater = 12, // 0x0000000C
        Rotted_Woods = 13, // 0x0000000D
        Noklateo = 15, // 0x0000000F
    }

    public static Dictionary<T, string> EnumNames<T>(Dictionary<T, string> overrides = null) where T : struct, System.Enum
    {
        Dictionary<T, string> dictionary = Enumerable.ToDictionary<T, T, string>(Enumerable.Where<T>(Enumerable.Cast<T>((IEnumerable)System.Enum.GetValues<T>()), (Func<T, bool>)(v => (int)(ValueType)v < 10000)), (Func<T, T>)(v => v), (Func<T, string>)(v => v.ToString().Replace('_', ' ')));
        if (overrides != null)
        {
            foreach (KeyValuePair<T, string> keyValuePair in overrides)
                dictionary[keyValuePair.Key] = keyValuePair.Value;
        }
        return dictionary;
    }

    public enum TargetBoss
    {
        Gladius,
        Adel,
        Gnoster,
        Maris,
        Libra,
        Fulghor,
        Caligo,
        Heolstor,
    }

    public enum BaseCategory
    {
        Default = 0,
        Map_Event = 2000, // 0x000007D0
        Fort = 3000, // 0x00000BB8
        Camp = 3200, // 0x00000C80
        Ruins = 3400, // 0x00000D48
        Township = 3790, // 0x00000ECE
        Great_Church = 3800, // 0x00000ED8
        Sorcerers_Rise = 4000, // 0x00000FA0
        Church = 4100, // 0x00001004
        Small_Camp = 4300, // 0x000010CC
        Event = 4553, // 0x000011C9
        Night_Horde = 4600, // 0x000011F8
        Evergaol = 4650, // 0x0000122A
        Field_Boss = 4651, // 0x0000122B
        Strong_Field_Boss = 4652, // 0x0000122C
        Arena_Boss = 4681, // 0x00001249
        Night_Boss = 4770, // 0x000012A2
        Castle = 4941, // 0x0000134D
        None = 10000, // 0x00002710
        TODO = 10001, // 0x00002711
    }

    public enum AttachCategory
    {
        Default = 0,
        Major_Base = 100, // 0x00000064
        Starter_Major_Base = 121, // 0x00000079
        Castle = 190, // 0x000000BE
        Minor_Base = 300, // 0x0000012C
        Rotted_Woods = 309, // 0x00000135
        Starter_Minor_Base = 350, // 0x0000015E
        Night_Boss = 500, // 0x000001F4
        Event = 520, // 0x00000208
        Evergaol = 601, // 0x00000259
        Night_Horde = 615, // 0x00000267
        Field_Boss = 750, // 0x000002EE
        Arena_Boss = 757, // 0x000002F5
        Extra_Night_Boss = 800, // 0x00000320
        None = 10000, // 0x00002710
        TODO = 10001, // 0x00002711
    }
    
}
