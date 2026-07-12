using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//During character generation, values for a character are randomized. This organizes it.
public static class CharRandomValue
{
    private static List<string> firstNamesM = getNames("FirstNamesM");
    private static List<string> firstNamesF = getNames("FirstNamesF");
    private static List<string> lastNames = getNames("LastNames");

    private static int firstNamesMSize = firstNamesM.Count;
    private static int firstNamesFSize = firstNamesF.Count;
    private static int lastNamesSize = lastNames.Count;

    private static int[] hairLengthSizes = new int[]{ 21, 21, 15 };
    

    //Return a random name
    //The list of first and last names is supplied in "FirstNamesM/F.txt" and "LastNamesM/F.txt"
    //If male or female, use only provided names, if nonbinary use a name from either list
    public static (ulong, string, string) CRV_randomName(ulong seed, bool isMale)
    {
        (ulong s, int v) firstNameIdx = isMale ? RangedSeedRandomizer(seed, 0, firstNamesMSize) : RangedSeedRandomizer(seed, 0, firstNamesFSize);
        (ulong s, int v) lastNameIdx = RangedSeedRandomizer(firstNameIdx.s, 0, lastNamesSize);
        return (lastNameIdx.s, isMale ? firstNamesM[firstNameIdx.v] : firstNamesF[firstNameIdx.v], lastNames[lastNameIdx.v]);
    }

    //Return a random hair index. It depends on the hair length
    //(...may later depend on sex)
    // TODO is this even accurate anymore? check the hair file...
    public static (ulong, int) randomHairIndex(ulong seed, int HairLen, int gender)
    {
        // Not really a better way to do this than by hardcoding...too specific.
        int start = 0;
        int stop = hairLengthSizes[HairLen];
        switch (gender)
        {
            case 0:
                if (HairLen == 0) stop = 16;
                else if (HairLen == 1) stop = 17;
                else if (HairLen == 2) stop = 8;
                break;
            case 1:
                if (HairLen == 0)      start = 7;
                else if (HairLen == 1) start = 13;
                else if (HairLen == 2) start = 1;
                break;
        }
        return RangedSeedRandomizer(seed, start, stop);
    }

    private static List<string> getNames(string path)
    {
        TextAsset file = Resources.Load<TextAsset>(path);
        return new List<string>(file.text.Split('\n'));
    }

    // Should be used as much as possible by all "characters randomly choosing stuff" operations
    // For example, randomizing the exact hair color, or which variant of long hair to use.
    // Why not use built in randomizer? Because you have to set the seed in one place.
    // That won't fly if we are doing async stuff.
    public static ulong SeedRandomizer(ulong input)
    {
        input ^= input << 13;
        input ^= input >> 7;
        input ^= input << 17;
        return input;
    }

    /// <summary>
    /// Returns random value in [min, max), and also the next seed.
    /// Note on ulong vs. int: If you have a ulong but need to pass in an int, try only taking a slice of the end of the ulong?
    /// That should still get pretty decent results.
    /// </summary>
    public static (ulong, int) RangedSeedRandomizer(ulong input, int min, int max)
    {
        ulong orig = input;
        input ^= input << 13;
        input ^= input >> 7;
        input ^= input << 17;
        float clampedVal = ((float) input) / ((float) ulong.MaxValue);
        return (input, min + (int)(clampedVal * (float)(max - min)));
    }
}
