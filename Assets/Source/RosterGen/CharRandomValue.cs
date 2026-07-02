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
    public static (uint, string, string) CRV_randomName(uint seed, bool isMale)
    {
        (uint s, int v) firstNameIdx = isMale ? RangedSeedRandomizer(seed, 0, firstNamesMSize) : RangedSeedRandomizer(seed, 0, firstNamesFSize);
        (uint s, int v) lastNameIdx = RangedSeedRandomizer(firstNameIdx.s, 0, lastNamesSize);
        return (lastNameIdx.s, firstNamesM[firstNameIdx.v], lastNames[lastNameIdx.v]);
        return (seed, seed.ToString(), "");
    }

    //Return a random hair index. It depends on the hair length
    //(...may later depend on sex)
    // TODO is this even accurate anymore? check the hair file...
    public static (uint, int) randomHairIndex(uint seed, int HairLen)
    {
        return RangedSeedRandomizer(seed, 0, hairLengthSizes[HairLen]);
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
    public static uint SeedRandomizer(uint input)
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
    public static (uint, int) RangedSeedRandomizer(uint input, int min, int max)
    {
        uint orig = input;
        input ^= input << 13;
        input ^= input >> 7;
        input ^= input << 17;
        float clampedVal = ((float) input) / ((float) uint.MaxValue);
        return (input, min + (int)(clampedVal * (float)(max - min)));
    }
}
