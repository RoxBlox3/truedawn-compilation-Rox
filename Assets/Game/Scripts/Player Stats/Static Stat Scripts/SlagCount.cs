using System;
using System.Collections.Generic;
using System.Linq;

public class SlagCount
{
    private static ulong[] Slags = new ulong[Enum.GetNames(typeof(SlagTypes)).Length];

    private static readonly List<ISlagTextDisplay> SlagDisplays = new();

    private static bool initiated = false;
    public static bool Initiate(ulong[] slag)
    {
        if(!initiated)
        {
            // Default Slag:
            for (int i = 0; i < Slags.Length; i++) 
            {
                try
                {
                    Slags[i] = slag[i];
                }
                catch 
                {
                    Slags[i] = 0;
                }
            }
            initiated = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Add(ulong slagAdd, SlagTypes type)
    {
        Slags[(int)type] += slagAdd;
        Display(type);
        return true;
    }

    public static bool Sub(ulong slagSub, SlagTypes type)
    {
        if (Slags[(int)type] - slagSub > Slags[(int)type])
        {
            return false;
        }
        else
        {
            Slags[(int)type] -= slagSub;
            Display(type);
            return true;
        }
    }

    private static void Display(SlagTypes type)
    {
        SlagDisplays.ForEach(delegate (ISlagTextDisplay x) {
            if (x.SlagType == type) 
            {
                x.SetText(Slags[(int)type] + " g");
            }
        });
    }

    public static void DisplayAll() 
    {
        SlagDisplays.ForEach(delegate (ISlagTextDisplay x) {
            x.SetText(Slags[(int)x.SlagType] + " g");
        });
    }

    public static void RegisterDisplay(ISlagTextDisplay display) 
    {
        SlagDisplays.Add(display);
    }

    public static void SlagSell(SlagTypes type)
    {
        if (Slags[(int)type] > 100)
        {
            ulong slag = Slags[(int)type];
            Slags[(int)type] %= 100;
            slag -= Slags[(int)type];
            SystemPointsCount.Add(Convert(slag, type));
            Display(type);
        }
        else 
        {
            SkillController.Log("Not Enough Slag");
        }
    }

    private static ulong Convert(ulong slag, SlagTypes type) 
    {
        return (ulong)(slag*Math.Pow(10,((int)type)-1));
        // Math.Pow             // Run exponential growth based on Type
        // 10                   // We are multiplying by a specific amount of 10.
        // (uint)type           // get the integer representation of the type
        //              This will be 0 for the lowest setting of Slag
        // -1                   // Offset the power by -1 so that we are dividing by 10 at the lowest level.
        // This results in each tier selling for 10 times more than the last per gram.
    }

    public static string ToJson(byte tabcount) 
    {
        string tabs = "";
        for (byte i = 0; i < tabcount; i++)
        {
            tabs += "\t";
        }

        string json = "";

        foreach (SlagTypes type in typeof(SlagTypes).GetEnumValues()) 
        {
            json += ",\n" 
                + tabs + "\t\t" + Slags[(int)type];
        }

        json = "\n" 
            + tabs + "\t\"Slag\":[" 
            + json[1..] + "\n" 
            + tabs + "\t]";

        return tabs + "{\n" 
            + tabs + "\t\"Stat\":\""+ StatEnums.Slag +"\","
            + json + "\n" 
            + tabs +"}";
    }

    /// <summary>
    /// Simply returns the SlagType enumerator representing the highest tier of Slag that the player has more than 0 grams of.
    /// </summary>
    /// <returns> The highest tier of slag that the player posesses, as a SlagTypes enumerator. </returns>
    public static SlagTypes GetHighestTier() 
    {
        SlagTypes tier = SlagTypes.InferiorSlag;
        
        for (int i = 0; i < Slags.Count(); i++) 
        {
            if (Slags[i] > 0) 
            {
                tier = (SlagTypes) i;
            }
        }

        return tier;
    }

    /// <summary>
    /// Simply returns the quantity of slag for the given type.
    /// </summary>
    /// <param name="slagType"> The type of slag that you need to know the amount for. </param>
    /// <returns> The quantity of slag that the player has for the provided type. </returns>
    public static ulong GetSlagQuantity(SlagTypes slagType) 
    {
        return Slags[(int)slagType];
    }
}
