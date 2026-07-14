using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
public class Character
{
    //Demographics: they are physical, undeniable descriptions of a person that can be discovered simply by seeing / meeting them.
    public int rosterId; // Where in the roster list of known characters (and sprites) this person is.
    public ulong simulatedId; // The unique ID from (0 - rosterSize - 1) that contains all this character's constrainable CPD values
                     // All other (cosmetic) random values generated using this simulatedId as a seed
    Dictionary<CPD_Type, CPD_Variant> createdCharacteristics; // Once we create a character we can assign them data in here


    //Attributes
    string firstName;
    string lastName;

    // Use this when you need a random number that doesn't really matter in the grand scheme of things
    float magicNumber;

    // The only thing you need to create a character is their position in the roster and their simulated ID!
    // Everything else can be determined on the fly as necessary
    public Character(int rosterId, ulong simulatedId)
    {
        this.rosterId = rosterId;
        this.simulatedId = simulatedId;

        randomizeDemographics();
    }

    /// <summary>
    /// Randomize demographics (CPDs) for this character.
    /// unpackSimulationID does most of the heavy lifting in this regard
    /// </summary>
    public void randomizeDemographics()
    {
        // Generate random demographics
        List<CPD_Variant> temp = Roster.SimulatedID.unpackSimulatedID(simulatedId);
        createdCharacteristics = new Dictionary<CPD_Type, CPD_Variant>();
        foreach (CPD_Variant var in temp)
        {
            createdCharacteristics.Add(var.cpdType, var);
        }

        // There are some other traits we want to give our characters here
        // We can still get away with using "random" traits, since the randomSeed was set to a predictable value in unpackSimulationID
        // and will not be reset until we call it again.

        bool isMale = true;
        ulong workingSeed = simulatedId;
        if(createdCharacteristics.ContainsKey(CPD_Type.Gender))
        {
            int gender = createdCharacteristics[CPD_Type.Gender].cpdID;
            if (gender < 2) isMale = gender == 0;
            else
            {
                (ulong s, int v) nbName = CharRandomValue.RangedSeedRandomizer(simulatedId, 0, 2);
                workingSeed = nbName.s;
                isMale = nbName.v == 0;
            }
        }
        Debug.Log("Will use male name: " + isMale);
        (ulong s, string f, string l) fullName = CharRandomValue.CRV_randomName(workingSeed, isMale);
        firstName = fullName.f;
        lastName = fullName.l;

        (ulong finalSeed, float mn) = CharRandomValue.Random(fullName.s);
        this.magicNumber = mn;
    }

    /// <summary>
    /// Get the CPD id of a characteristic
    /// CPD id == the index of this variant in the CPD's variants list.
    /// </summary>
    public int getCpdIDofCharacteristic(CPD_Type characteristic)
    {
        return createdCharacteristics[characteristic].cpdID;
    }

    /// <summary>
    /// Get the Category name of a characteristic
    /// </summary>
    public string getCategoryofCharacteristic(CPD_Type characteristic)
    {
        return createdCharacteristics[characteristic].category;
    }

    /// <summary>
    /// If second argument passed in, assuming the first depends on it
    /// </summary>
    public string getVariantNameofCharacteristic(CPD_Type characteristic)
    {
        return createdCharacteristics[characteristic].name;
    }

    public int getVariantIndexofCharacteristic(CPD_Type characteristic)
    {
        return createdCharacteristics[characteristic].cpdID;
    }

    /// <summary>
    /// Get the Category index of a characteristic
    /// </summary>
    public int getCategoryIndexofCharacteristic(CPD_Type characteristic)
    {
        return createdCharacteristics[characteristic].categoryIndex;
    }

    /// <summary>
    /// Gets the color value from a CPD assumed to be a color
    /// </summary>
    public (ulong, Color) getColorField(ulong seed, CPD_Type cpdType)
    {
        return (createdCharacteristics[cpdType].critVal as CPD_CritVal_Color).col.getColor(simulatedId);
    }

    /// <summary>
    /// Character's display name is just their first and last name
    /// </summary>
    public string getDisplayName(bool newline)
    {
        if (newline == false) return firstName + " " + lastName;
        else return firstName + "\n" + lastName;
    }

    /// <summary>
    /// Get a random value using the magic number.
    /// </summary>
    public int getOneTimeRandomNumber(int min, int max)
    {
        return min + (int)(magicNumber * (float)(max - min));
    }

    public float getOneTimeRandomNumber(float min, float max)
    {
        return min + (magicNumber * (max - min));
    }

    public override string ToString()
    {
        string str = $"RosterID = [{rosterId}], SimulatedID = [{simulatedId}]\n" +
            "Name: " + firstName + " " + lastName + "\n" + "";
            /*"Body: " + bodyType + "\n" +
            "Head: " + headType + "\n" +
            "Ht: " + height + "\n" +
            "Wt: " + weight + "\n" +
            "Male: " + isMale + "\n" +
            "SkinTone: " + skinTone + "\n" +
            "Hair: " + hairStyle + "," + hairColor + "\n";*/
        return str;
    }
}
