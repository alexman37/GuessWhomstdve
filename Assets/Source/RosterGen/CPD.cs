using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Character Profile Demographic." A single feature that a character has, such as their hair, hair color, or skin tone.
/// A complete character is made up of multiple of these.
/// </summary>
public abstract class CPD
{
    public CPD_Type cpdType;          // Check the enum below.
    protected string propertiesPath;  // Each CPD has a text file that outlines the data of all its variants - this is the path to it
    public bool constrainable;        // Whether or not this CPD is constrainable, e.g. sortable, part of the game.

    protected float probCounter = 0.0f;
    protected float probX = 0.0f;

    public List<CPD_Variant> variants;                // All variants for this CPD in the order they apepared
    public List<string> categories;                   // All categories for this CPD in the order they appeared
    public Dictionary<string, int> categoryIndices;   // All categories matched up with their position in cats list (faster than FindIndex)
    protected Dictionary<string, List<CPD_Variant>> categoriesToVariants;  // All variants associated with a particular category

    public abstract List<CPD_Variant> initialize();

    /// <summary>
    /// Returns a completely random CPD variant
    /// </summary>
    public (uint, CPD_Variant) getRandom(uint seed)
    {
        (uint s, int v) randIndex = CharRandomValue.RangedSeedRandomizer(seed, 0, variants.Count);
        return (randIndex.s, variants[(int)randIndex.v]);
    }

    /// <summary>
    /// Returns a completely random CPD variant index
    /// </summary>
    public int getRandomIndex()
    {
        return UnityEngine.Random.Range(0, variants.Count);
    }

    /// <summary>
    /// Return a random variant with respect to constrained categories
    /// </summary>
    public CPD_Variant getRandomConstrained(HashSet<string> restrictedCats)
    {
        List<CPD_Variant> allPossible = new List<CPD_Variant>();
        foreach(string cat in categories)
        {
            if(!restrictedCats.Contains(cat))
            {
                allPossible.AddRange(categoriesToVariants[cat]);
            }
        }
        if (allPossible.Count == 0) return null;
        else
        {
            return allPossible[UnityEngine.Random.Range(0, allPossible.Count)];
        }
    }

    /// <summary>
    /// Return a random variant index with respect to constrained categories
    /// </summary>
    public (int catId, int varId) getRandomConstrainedIndex(HashSet<string> restrictedCats)
    {
        CPD_Variant chosen = getRandomConstrained(restrictedCats);
        return (categoryIndices[chosen.category], chosen.cpdID);
    }

    /// <summary>
    /// Get all possible category indicies with respect to whichever categories are constrained
    /// </summary>
    public List<uint> getAllConstrainedIndicies(HashSet<string> restrictedCats)
    {
        List<uint> cats = new List<uint>();
        foreach(string cat in categories)
        {
            if(!restrictedCats.Contains(cat))
            {
                cats.Add((uint)categoryIndices[cat]);
            }
        }
        return cats;
    }

    /// <summary>
    /// Get all possible variants from a category
    /// </summary>
    public List<CPD_Variant> getPossibleVariantsFromCategory(string cat)
    {
        return categoriesToVariants[cat];
    }

    /// <summary>
    /// Get all variants from whatever categories are possible (given a set of constraints)
    /// </summary>
    public List<CPD_Variant> getConstrainedCategoryVariants(HashSet<string> restrictedCats)
    {
        List<CPD_Variant> allPossible = new List<CPD_Variant>();
        foreach (string cat in categories)
        {
            if(!restrictedCats.Contains(cat))
            {
                allPossible.AddRange(getPossibleVariantsFromCategory(cat));
            }
        }
        return allPossible;
    }

    /// <summary>
    /// What percentage of categories are still available? (Used for fast roster size recalculations)
    /// </summary>
    public float getProportionOfCategories(HashSet<string> restrictedCats)
    {
        return ((float)(categories.Count - restrictedCats.Count) / (float)categories.Count);
    }

    public TargetCPDGuessReward getGuessReward()
    {
        int cpdDifficulty = categories.Count;
        if (cpdDifficulty > 5) return TargetCPDGuessReward.GoldCard;
        else if (cpdDifficulty > 2) return TargetCPDGuessReward.ActionCard;
        else return TargetCPDGuessReward.None;
    }
}

public enum TargetCPDGuessReward
{
    None,
    ActionCard,
    GoldCard
}


public enum CPD_Type
{
    // Constrainable
    HairStyle,
    HairColor,
    SkinTone,
    FavoriteColor,
    EyeColor,
    Height,
    Weight,
    BloodType,
    Zodiac,

    // Temporary (TODO remove)
    /*Test1,
    Test2,
    Test3,
    Test4,
    Test5,
    Test6,*/

    // Not constrainable
    BodyType,
    Face,
    HeadType
}


/// <summary>
/// A CPD without a "critical value", it just uses categoryIndex with a 2D texture array somewhere in shader
/// </summary>
public class CPD_SimpleIndex : CPD
{

    // Given the path of this CPD's properties file, we can initialize all variants.
    public CPD_SimpleIndex(CPD_Type cat, bool constrainable, string propertiesPath)
    {
        this.constrainable = constrainable;
        this.cpdType = cat;
        this.propertiesPath = propertiesPath;
        initialize(); 
    }

    public override List<CPD_Variant> initialize()
    {
        TextAsset txt = Resources.Load<TextAsset>(propertiesPath); // TODO use assetbundles instead?
        string[] lines = txt.text.Split('\n');

        variants = new List<CPD_Variant>();
        categoriesToVariants = new Dictionary<string, List<CPD_Variant>>();
        categories = new List<string>();
        categoryIndices = new Dictionary<string, int>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Split('#')[0].Trim(); //ignore comments
            if (line.Length > 0)
            {
                string[] fields = line.Split(';');

                //probability - probably not what you think it is!
                //the higher the number, the lower chance it has of appearing
                int p;
                if (fields[2] == "X")
                {
                    p = 0;
                }
                else
                {
                    p = int.Parse(fields[2]);
                }

                string cat = fields[3];
                if (!categoriesToVariants.ContainsKey(cat))
                {
                    categoriesToVariants.Add(cat, new List<CPD_Variant>());
                    categories.Add(cat);
                    categoryIndices.Add(cat, categories.Count - 1);
                }

                CPD_Variant variant = new CPD_Variant(
                    cpdType,
                    categoriesToVariants[cat].Count,
                    variants.Count,
                    null,
                    fields[1],
                    cat,
                    categoryIndices[cat],
                    p
                );

                variants.Add(variant);
                categoriesToVariants[cat].Add(variant);
            }
        }

        if (probCounter > 1)
        {
            Debug.LogError("Failed to use the CPD FileName " + cpdType + ", probabilities do not equal 1");
            return null;
        }
        else if (probX > 0)
        {
            probX = (1 - probCounter) / probX;
        }

        return variants;
    }
}

/// <summary>
/// A CPD whose critical value is a color
/// </summary>
public class CPD_Color : CPD
{
    // Given the path of this CPD's properties file and where its sprites are stored, we can initialize all variants.
    public CPD_Color(CPD_Type cat, bool constrainable, string propertiesPath)
    {
        this.constrainable = constrainable;
        this.cpdType = cat;
        this.propertiesPath = propertiesPath;
        initialize();
    }

    public override List<CPD_Variant> initialize()
    {
        TextAsset txt = Resources.Load<TextAsset>(propertiesPath);
        string[] lines = txt.text.Split('\n');

        variants = new List<CPD_Variant>();
        categoriesToVariants = new Dictionary<string, List<CPD_Variant>>();
        categoryIndices = new Dictionary<string, int>();
        categories = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Split('#')[0].Trim(); //ignore comments
            if (line.Length > 0)
            {
                string[] fields = line.Split(';');

                //color
                string[] strColors = fields[2].Split(',');
                CPD_ColorType critVal;

                // Color range
                if (fields[2].Contains("-"))
                {
                    string[] rs = strColors[0].Split('-');
                    int rMin = int.Parse(rs[0]); int rMax = rs.Length > 1 ? int.Parse(rs[1]) : int.Parse(rs[0]);
                    string[] gs = strColors[1].Split('-');
                    int gMin, gMax;
                    if (gs[0] == "X0") { gMin = rMin; gMax = rMin; }
                    else { gMin = int.Parse(gs[0]); gMax = gs.Length > 1 ? int.Parse(gs[1]) : int.Parse(gs[0]); }
                    string[] bs = strColors[2].Split('-');
                    int bMin, bMax;
                    if (bs[0] == "X0") { bMin = rMin; bMax = rMin; }
                    else if (bs[0] == "X1") { bMin = gMin; bMax = gMin; }
                    else { bMin = int.Parse(bs[0]); bMax = bs.Length > 1 ? int.Parse(bs[1]) : int.Parse(bs[0]); }
                    critVal = new ColorRange(rMin, rMax, gMin, gMax, bMin, bMax);
                } 
                // Single color value
                else
                {
                    critVal = new ConstantColor(new Color(float.Parse(strColors[0]) / 255f, float.Parse(strColors[1]) / 255f, float.Parse(strColors[2]) / 255f));
                }

                //probability - TODO
                int p;
                if (fields[1] == "X")
                {
                    p = 0;
                }
                else
                {
                    p = int.Parse(fields[1]);
                }

                string cat = fields[3];
                if (!categoriesToVariants.ContainsKey(cat))
                {
                    categoriesToVariants.Add(cat, new List<CPD_Variant>());
                    categories.Add(cat);
                    categoryIndices.Add(cat, categories.Count - 1);
                }

                CPD_Variant variant = new CPD_Variant(
                    cpdType,
                    categoriesToVariants[cat].Count,
                    variants.Count,
                    new CPD_CritVal_Color(critVal),
                    fields[0],
                    cat,
                    categoryIndices[cat],
                    p
                );

                variants.Add(variant);
                categoriesToVariants[cat].Add(variant);
            }
        }

        if (probCounter > 1)
        {
            Debug.LogError("Failed to use the CPD Color " + cpdType + ", probabilities do not equal 1");
            return null;
        }
        else if (probX > 0)
        {
            probX = (1 - probCounter) / probX;
        }

        return variants;
    }
}

public class CPD_Number : CPD
{
    int min, max; // inclusive

    // CPD Number type is assumed to be an int in the range given.
    public CPD_Number(CPD_Type cat, bool constrainable, int min, int max)
    {
        this.constrainable = constrainable;
        this.cpdType = cat;
        this.propertiesPath = null;
        this.min = min;
        this.max = max;
        initialize();
    }

    public override List<CPD_Variant> initialize()
    {
        // Definitely don't need a properties file unless we re-introduced probabilities again somehow
        List<CPD_Variant> vars = new List<CPD_Variant>();
        for(int i = min; i <= max; i++)
        {
            vars.Add(new CPD_Variant(
                    cpdType,
                    i - min,
                    i - min,
                    new CPD_CritVal_Number(i),
                    i.ToString(),
                    i.ToString(),
                    i - min,
                    1 / (max - min)
            ));
        }
        return vars;
    }
}


/// <summary>
/// Defines a specific variant of a CPD
/// For example, the HairStyle CPD might have a "normal1" and "normal2" variant, both of which fall under the "normal" category
/// </summary>
public class CPD_Variant
{
    public CPD_Type cpdType;
    public int categoryID; // order within category.
    public int cpdID; // order within CPD.
    public CPD_CriticalValue critVal; // the actual "thing" stored in this CPD (filepath? color? number? etc...)
    public string name;
    public string category;
    public int categoryIndex; // order of category in CPD.
    public int probability;

    public CPD_Variant(CPD_Type cpdCat, int catID, int cpdID, CPD_CriticalValue critVal, string name, string cat, int catIndex, int prob)
    {
        this.cpdType = cpdCat;
        this.categoryID = catID;
        this.cpdID = cpdID;
        this.critVal = critVal;
        this.name = name;
        this.category = cat;
        this.categoryIndex = catIndex;
        this.probability = prob;
    }

    // All CPD fields print out their name.
    public override string ToString()
    {
        return this.name;
    }
}

public abstract class CPD_CriticalValue
{

}

public class CPD_CritVal_Number : CPD_CriticalValue
{
    public int value;

    public CPD_CritVal_Number(int num)
    {
        value = num;
    }
}

public class CPD_CritVal_Color : CPD_CriticalValue
{
    public CPD_ColorType col;

    public CPD_CritVal_Color(Color c)
    {
        col = new ConstantColor(c);
    }

    public CPD_CritVal_Color(CPD_ColorType c)
    {
        col = c;
    }
}

public abstract class CPD_ColorType
{
    public abstract (uint, Color) getColor(uint simId);
}

public class ConstantColor : CPD_ColorType
{
    private Color col;

    public ConstantColor(Color co)
    {
        col = co;
    }
    public override (uint, Color) getColor(uint seed)
    {
        return (seed, col);
    }
}

// Color Range
public class ColorRange : CPD_ColorType
{
    // all color values from 0-255 (for now)
    private int minR;
    private int maxR;
    private int minG;
    private int maxG;
    private int minB;
    private int maxB;

    public ColorRange(int minR, int maxR, int minG, int maxG, int minB, int maxB)
    {
        this.minR = minR;
        this.maxR = maxR;
        this.minG = minG;
        this.maxG = maxG;
        this.minB = minB;
        this.maxB = maxB;
    }

    // TODO: Do we want to pass the seed along or not? Seems like a minor thing...prob OK without it...
    public override (uint, Color) getColor(uint seed)
    {
        (uint s, float v) r = CharRandomValue.RangedSeedRandomizer(seed, minR, maxR);
        (uint s, float v) g = CharRandomValue.RangedSeedRandomizer(r.s, minG, maxG);
        (uint s, float v) b = CharRandomValue.RangedSeedRandomizer(g.s, minB, maxB);
        return (b.s, new Color(r.v / 255f, g.v / 255f, b.v / 255f, 1));
    }
}
