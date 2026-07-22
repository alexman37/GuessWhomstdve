using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// The database of characters.
/// How it works at a high level: We don't actually store data for any characters, besides the ones we are showing.
/// We can load a character exactly as they should be by unpacking their "simulated ID"...more on that below.
/// </summary>
public class Roster
{
    const int TOTAL_ROSTER_PERMUTATIONS = 999999; // How many different rosters can there be?
    private int rosterSeedOffset;                 // offset every random seed by this amount. It makes each new game unique.

    // Making certain characteristics appear with probabilities requires a prime number
    // Only rule: No CPD should have that amount of categories, nor any multiple of it
    // Good candidates are 7, 11, 13 and 17
    private int probabilityPrime = 13;

    public ulong simulatedTotalRosterSize = 1; // total number of "characters" we're working with
    private ulong simulatedCurrentRosterSize = 1; // total number of characters given constraints

    public List<Character> shownRoster; // all characters currently being displayed on the screen.
    public HashSet<ulong> currentRosterIDs; // the simulated IDs of all characters we are currently showing.

    private HashSet<ulong> charactersGuessedAsTarget;  // all characters the player has guessed as the target (never show them again)

    // Each agent has their own set of roster constraints they will apply to the general roster.

    public static List<CPD> cpdInstances;      // All CPD singletons
    public static List<CPD> cpdConstrainables; // Only the constrainable CPDs (packaged in sim ID, in this order)
    public static Dictionary<CPD_Type, CPD> cpdByType; // get CPD singleton by type
    protected static List<ulong> cpdCounts; // optimization for simulated ID unpacking
    protected static List<ulong> simIDtourGuide; // the first CPD should be multiplied by index 0...the second by index 1...etc. to get sim ID.

    public ulong targetId; // the simulated ID of the person everyone wants to find
    private Character targetAsChar;

    // You can sort the roster by common constraints that all players have, or just your own.
    public RosterConstraints commonConstraints = new RosterConstraints();
    public bool withCommonConstraints = true;

    // Actions
    public static event Action rosterReady;
    public static event Action clearAllConstraints;
    public static event Action<ulong> guessedWrongCharacter;


    // Optimization for "get Random simulated ID":
    // In that function, if we fail to get a random ID by lucky chance several times, we do an exhaustive search through all possible sim IDs
    // Since it has to be done in order, we save our position in the search here...so if we need more sim IDs we can pick up where we left off.
    protected static bool lastResortSearch = false;
    protected static int savedCPD = 0;
    protected static int savedMod = 0;
    protected static List<ulong> newSimIdModifiers;
    protected static List<ulong> allSimIdModifiers;

    // Most of this is first-time setup only
    public Roster()
    {
        if (rosterReady == null) rosterReady += () => { };
        if (clearAllConstraints == null) clearAllConstraints += () => { };
        if (guessedWrongCharacter == null) guessedWrongCharacter += (_) => { };

        // No need to recreate CPDs on each load
        if(cpdInstances == null)
        {
            cpdInstances = new List<CPD>
            {
                new CPD_SimpleIndex(CPD_Type.HairStyle, true, "properties/hairStyles", -1),
                new CPD_Color(CPD_Type.HairColor, true, "properties/hairTones", -1),
                new CPD_Color(CPD_Type.SkinTone, true, "properties/skinTones", -1),
                new CPD_Color(CPD_Type.FavoriteColor, true, "properties/faveColors", -1),
                new CPD_Color(CPD_Type.EyeColor, true, "properties/eyeColors", -1),
                new CPD_SimpleIndex(CPD_Type.Gender, true, "properties/gender", -1),
                new CPD_SimpleIndex(CPD_Type.Height, true, "properties/heights", -1),
                new CPD_SimpleIndex(CPD_Type.Weight, true, "properties/weights", -1),
                new CPD_SimpleIndex(CPD_Type.BloodType, true, "properties/bloodtypes2", -1),
                new CPD_SimpleIndex(CPD_Type.Zodiac, true, "properties/zodiacs", -1),
                new CPD_SimpleIndex(CPD_Type.Job, true, "properties/jobs", -1),

                // Locations
                //new CPD_SimpleIndex(CPD_Type.City_L1, true, "properties/cities_l1", -1),

                new CPD_SimpleIndex(CPD_Type.Region_L2, true, "properties/regions_l2", -1),
                new CPD_SimpleIndex(CPD_Type.City_L2, true, "properties/cities_l2", (int) CPD_Type.Region_L2),

                new CPD_SimpleIndex(CPD_Type.BodyType, false, "properties/bodyTypes", -1),
                new CPD_SimpleIndex(CPD_Type.Face, false, "properties/faceTypes", -1),
                new CPD_SimpleIndex(CPD_Type.HeadType, false, "properties/headTypes", -1),
            };
            cpdConstrainables = new List<CPD>();
            cpdCounts = new List<ulong>();
            cpdByType = new Dictionary<CPD_Type, CPD>();
            simIDtourGuide = new List<ulong>();
            currentRosterIDs = new HashSet<ulong>();
            charactersGuessedAsTarget = new HashSet<ulong>();

            // Set constrainables list
            for (int c = 0; c < cpdInstances.Count; c++)
            {
                if (cpdInstances[c].constrainable)
                {
                    cpdConstrainables.Add(cpdInstances[c]);
                }
                cpdByType.Add(cpdInstances[c].cpdType, cpdInstances[c]);
            }
            // Set helpers for constrainables list
            for(int c = 0; c < cpdConstrainables.Count; c++)
            {
                ulong nextOffset = 1;
                cpdCounts.Add((ulong)cpdConstrainables[c].categories.Count);
                simulatedTotalRosterSize *= (ulong)cpdConstrainables[c].categories.Count;

                for (int x = c + 1; x < cpdConstrainables.Count; x++)
                {
                    nextOffset *= (ulong)cpdConstrainables[x].categories.Count;
                }
                simIDtourGuide.Add(nextOffset);
            }
        }

        // Build common constraints
        commonConstraints.clearAllConstraints(true);
        RosterConstraints.NO_CONSTRAINTS.clearAllConstraints(true);

        simulatedCurrentRosterSize = simulatedTotalRosterSize;

        createRoster(UI_Roster.MAX_CHARACTERS_TO_SHOW);
    }

    ~Roster()
    {

    }

    /// <summary>
    /// Called each time you start a new game.
    /// </summary>
    public void createRoster(uint howMany)
    {
        rosterSeedOffset = UnityEngine.Random.Range(0, TOTAL_ROSTER_PERMUTATIONS);
        if (shownRoster != null)
        {
            shownRoster.Clear();
        } else
        {
            shownRoster = new List<Character>();
        }

        clearAllConstraints.Invoke();
        applyConstraints(RosterConstraints.NO_CONSTRAINTS);

        // First list generation
        for (int i = 0; i < howMany; i++)
        {
            ulong simId = SimulatedID.getRandomSimulatedID(RosterConstraints.NO_CONSTRAINTS, currentRosterIDs, simulatedCurrentRosterSize);

            shownRoster.Add(new Character(i, simId));

            //Debug.Log("roster gen " + roster[i]);
            currentRosterIDs.Add(simId);
        }

        lastResortSearch = false;
        savedCPD = 0;
        savedMod = 0;

        // TODO!!!! This is probably doing some shit wrong!!
        targetId = (ulong)UnityEngine.Random.Range(0, simulatedTotalRosterSize - 1);
        targetAsChar = new Character(-1, targetId);

        rosterReady.Invoke();
    }

    public void setCommonConstraints(bool withCommon)
    {
        withCommonConstraints = withCommon;
        if(withCommon)
        {
            applyConstraints(commonConstraints);
        } else
        {
            // TODO PlayerSelf
            applyConstraints(TurnDriver.instance.playersInOrder[0].rosterConstraints);
        }
        redrawRosterVis();
    }

    /// <summary>
    /// Return a list of all the target's CPD stuff
    /// </summary>
    /// <returns></returns>
    public List<CPD_Variant> getTargetAsCPDs()
    {
        return SimulatedID.unpackSimulatedID(targetId);
    }

    /// <summary>
    /// Return a list of the target as a Character instance
    /// </summary>
    public Character getTargetAsCharacter()
    {
        return targetAsChar;
    }

    public bool targetHasProperty(CPD_Type cpdType, string cat)
    {
        return targetAsChar.getCategoryofCharacteristic(cpdType) == cat;
    }


    /// <summary>
    /// Redraw the roster with new characters meeting constraints
    /// </summary>
    public void redrawRosterVis()
    {
        RosterConstraints currConstraints;
        if (withCommonConstraints)
        {
            currConstraints = commonConstraints;
        } else
        {
            // TODO PlayerSelf
            currConstraints = TurnDriver.instance.playersInOrder[0].rosterConstraints;
        }

        // Must apply constraints first to determine desired size of list.
        applyConstraints(currConstraints);
        uint howMany = UI_Roster.instance.currCharactersToShow;

        HashSet<int> replaceIndices = new HashSet<int>();
        int size = (int) Mathf.Min(howMany, simulatedCurrentRosterSize);

        // Characters to show: first, choose any from the currently shown roster we'd like to keep.
        int count = 0;
        currentRosterIDs.Clear();
        currentRosterIDs = new HashSet<ulong>(charactersGuessedAsTarget);
        int m = Mathf.Min((int)howMany, shownRoster.Count);
        for (int i = 0; i < m && count < size; i++)
        {
            // If we already guessed this character, do not allow it to be added to the roster view again
            if (charactersGuessedAsTarget.Contains(shownRoster[i].simulatedId))
            {
                // pass
            }
            // If the character is unguessed and still meets constraints, keep it around
            else if (SimulatedID.idMeetsConstraints(shownRoster[i].simulatedId, currConstraints))
            {
                currentRosterIDs.Add(shownRoster[i].simulatedId);
                count++;
            } 
            // If the character no longer meets constraints, remove it
            else
            {
                currentRosterIDs.Remove(shownRoster[i].simulatedId);
                replaceIndices.Add(i);
            }
        }

        shownRoster = shownRoster.GetRange(0, Mathf.Max(size, shownRoster.Count));
        Debug.Log("Size is " + size);
        for (int i = 0; i < size; i++)
        {
            if(replaceIndices.Contains(i))
            {
                try
                {
                    ulong simId = SimulatedID.getRandomSimulatedID(currConstraints, currentRosterIDs, simulatedCurrentRosterSize);

                    shownRoster[i] = new Character(i, simId);
                    currentRosterIDs.Add(simId);
                }
                // When targets have been manually guessed, we don't show them anymore
                // If a guessed target still meets all constraints, it will pass all checks but we don't want to show it.
                // This leads to getRandomSimulatedID failing to find the final characters and returning -1
                // It always short circuits when all other possibilities are exhausted - so we know just how many failed.
                catch (ArithmeticException)
                {
                    simulatedCurrentRosterSize = (ulong)i;
                    Debug.LogWarning("Shortened size is now " + simulatedCurrentRosterSize);
                    UI_Roster.instance.updateRosterCount(simulatedCurrentRosterSize);
                    break;
                }
            }
        }

        lastResortSearch = false;
        savedCPD = 0;
        savedMod = 0;

        UI_Roster.instance.regenerateCharCards(simulatedCurrentRosterSize, UI_Roster.instance.rosterLOD, replaceIndices);
    }

    /// <summary>
    /// Apply new constraints to the constraints list
    /// </summary>
    public void applyConstraints(RosterConstraints constraints)
    {
        simulatedCurrentRosterSize = getNewRosterSizeFromConstraints(constraints);

        UI_Roster.instance.updateRosterCount(simulatedCurrentRosterSize);
    }

    /// <summary>
    /// Get the new size of a roster with constraints applied to it
    /// </summary>
    public ulong getNewRosterSizeFromConstraints(RosterConstraints constraints)
    {
        // The roster size will decrease when applying a new constraint (and vice versa)
        ulong newRosterSize = simulatedTotalRosterSize;

        List<CPD_Type> types = new List<CPD_Type>(constraints.allCurrentConstraints.Keys);

        foreach (CPD_Type tp in types)
        {
            // Assuming all probabilities are equal.
            newRosterSize = Utility.RoundToulong(cpdByType[tp].getProportionOfCategories(constraints.allCurrentConstraints[tp]) * (float)newRosterSize);
        }

        // is there any way to optimize this? Hopefully the set of guessed chars never gets too big.
        foreach (ulong guessedChar in charactersGuessedAsTarget)
        {
            if(SimulatedID.idMeetsConstraints(guessedChar, constraints))
            {
                newRosterSize--;
            }
        }
        
        return newRosterSize;
    }

    /// <summary>
    /// When a FormButton unconfirms, clear all constraints (quick optimization)
    /// </summary>
    public void reInitializeVariants(CPD_Type onType, List<string> buttonsAreOff)
    {
        CPD cpd = cpdByType[onType];
        // TODO PlayerSelf
        TurnDriver.instance.playersInOrder[0].rosterConstraints.clearConstraints(cpd, false);
        foreach(string exclude in buttonsAreOff)
        {
            // TODO PlayerSelf
            TurnDriver.instance.playersInOrder[0].rosterConstraints.addConstraint(cpd.cpdType, exclude, false);
        }
    }

    /// <summary>
    /// What to do when the player guesses a character as the target
    /// </summary>
    public void GuessedCharacterAsTarget(ulong guessId)
    {
        // TODO if wrong
        charactersGuessedAsTarget.Add(guessId);
        redrawRosterVis();
        guessedWrongCharacter.Invoke(guessId);
    }

    public void DebugLogRoster()
    {
        for (int i = 0; i < shownRoster.Count; i++)
        {
            Debug.Log(shownRoster[i]);
        }
    }


    /// Everything to do with the simulated ID
    public static class SimulatedID
    {
        /// <summary>
        /// Given a "simulated ID" in [0, rosterSize), return all CPD variants this character would generate with.
        ///     The ID itself contains what category each field will be. Every character is guaranteed a unique set of categories,
        ///     and the specific variants within those categories are chosen by random seed.
        /// For variants and all other non-constrainable, "cosmetic" CPD's, the simulated ID also acts as a random seed.
        /// Setting the random seed before getting those values ensures we always "randomly generate" the same output for the character.
        /// There's just one catch: We have to offset every random seed by a constant amount, so we do not get the exact same roster every time.
        /// </summary>
        /// <param name="simulatedId">Simulated id in [0, rosterSize)</param>
        /// <returns>All variants of the character with this simulated ID</returns>
        public static List<CPD_Variant> unpackSimulatedID(ulong simulatedId)
        {
            // TODO: Add by rosterOffset to make every game random.
            ulong randomizerSeed = simulatedId;

            // We gotta get all the categories associated with each sim ID - one at a time.
            List<CPD_Variant> vars = new List<CPD_Variant>();
            int c = 0;
            ulong prevCPDcategory = 0;

            for (int iter = 0; iter < cpdInstances.Count; iter++)
            {
                // Two distinct cases. If the CPD is constrainable it directly affects simulated ID. Otherwise it's just "random".
                if(cpdInstances[iter].constrainable)
                {
                    ulong currCPDcategory = 0;

                    CPD currCpd = cpdConstrainables[c];
                    currCPDcategory = Utility.FloorToulong(simulatedId / simIDtourGuide[c]);
                    List<CPD_Variant> possibles = currCpd.getPossibleVariantsFromCategory(currCpd.categories[(int)currCPDcategory], (int)prevCPDcategory);

                    (ulong s, int v) randTemp = CharRandomValue.RangedSeedRandomizer(randomizerSeed, 0, possibles.Count);
                    //Debug.Log("In list of size " + possibles.Count + " I give you " + randTemp.v);
                    randomizerSeed = randTemp.s;
                    vars.Add(possibles[randTemp.v]);

                    simulatedId -= simIDtourGuide[c] * currCPDcategory;
                    c++;

                    prevCPDcategory = currCPDcategory;
                } else
                {
                    (ulong s, CPD_Variant v) rand = cpdInstances[iter].getRandom(randomizerSeed);
                    randomizerSeed = rand.s;
                    vars.Add(rand.v);
                }
                
            }

            return vars;
        }


        /// <summary>
        /// Given some roster constraints, generate a random simulated ID for this character.
        /// For any CPDs with constraints, we must choose a value allowed by them.
        /// For any other CPDs without constraints, we can randomize them.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public static ulong getRandomSimulatedID(RosterConstraints constraints, HashSet<ulong> takenIDs, ulong currentRosterSize)
        {
            // How this works on a technical level:
            //   - We will attempt to generate a random ID, see if it's already taken (for big current rosters, this is unlikely.)
            //   - If it is taken, we'll try the same process again a few more times.
            //   - If we have failed multiple times, we assume the constrained list is too crowded,
            //          so we resort to iterating through all possible constrained IDs; in order, until finding one that works.
            //   - Optimization: if the roster size is below a certain threshold, automatically resort to iterating through all IDs.
            if (currentRosterSize > 20)
            {
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ulong workingID = 0;
                    for (int c = 0; c < cpdConstrainables.Count; c++)
                    {
                        CPD currCpd = cpdConstrainables[c];

                        // If being constrained, carefully consider which values are allowed...
                        if (constraints.allCurrentConstraints.ContainsKey(currCpd.cpdType))
                        {
                            (int catId, int varId) = currCpd.getRandomConstrainedIndex(constraints.allCurrentConstraints[currCpd.cpdType]);
                            workingID += simIDtourGuide[c] * (ulong)catId;
                        }
                        // Otherwise you can just pick anything...
                        else
                        {
                            int v = currCpd.getRandomIndex();
                            workingID += simIDtourGuide[c] * (ulong)v;
                        }
                    }
                    if (takenIDs == null || !takenIDs.Contains(workingID))
                    {
                        return workingID;
                    }
                }
            }

            // Worst case scenario: Resort to iteration through all possible IDs. Return the first success.
            for(int cpdIndex = savedCPD; cpdIndex < cpdConstrainables.Count; cpdIndex++)
            {
                CPD currCpd = cpdConstrainables[cpdIndex];

                ulong magicNumber = simIDtourGuide[cpdIndex];
                List<ulong> currSimIdModifiers = currCpd.getAllConstrainedIndicies(constraints.allCurrentConstraints[currCpd.cpdType]);
                for(int i = 0; i < currSimIdModifiers.Count; i++)
                {
                    currSimIdModifiers[i] = magicNumber * currSimIdModifiers[i];
                }
                ulong catZeroes = 0;
                for(int i = cpdIndex + 1; i < cpdConstrainables.Count; i++)
                {
                    // There must be at least one category or there's a problem...
                    catZeroes += simIDtourGuide[i] * cpdConstrainables[i].getAllConstrainedIndicies(constraints.allCurrentConstraints[cpdConstrainables[i].cpdType])[0];
                }

                if (savedCPD == 0)
                {
                    allSimIdModifiers = new List<ulong>();
                }
                if (savedMod == 0)
                {
                    newSimIdModifiers = new List<ulong>();
                }
                // First pass
                if (allSimIdModifiers.Count == 0)
                {
                    for (int l = 0; l < currSimIdModifiers.Count; l++)
                    {
                        ulong aNewModifier = currSimIdModifiers[l];
                        ulong aNewIndex = aNewModifier + catZeroes;
                        newSimIdModifiers.Add(aNewModifier);
                        if (takenIDs == null || !takenIDs.Contains(aNewIndex))
                        {
                            return aNewIndex;
                        }
                    }
                } 
                // Every subsequent pass
                else
                {
                    for (int i = savedMod; i < allSimIdModifiers.Count; i++)
                    {
                        for (int l = 0; l < currSimIdModifiers.Count; l++)
                        {
                            ulong mod = allSimIdModifiers[i];
                            ulong aNewModifier = mod + currSimIdModifiers[l];
                            ulong aNewIndex = aNewModifier + catZeroes;
                            newSimIdModifiers.Add(aNewModifier);
                            if (takenIDs == null || !takenIDs.Contains(aNewIndex))
                            {
                                return aNewIndex;
                            }
                        }
                        savedMod++;
                    }
                }

                allSimIdModifiers = newSimIdModifiers;
                savedCPD++;
                savedMod = 0;
            }

            // Should be impossible
            Debug.LogError("Could not find any valid simulated ID");
            throw new ArithmeticException("Could not find any valid simulated ID");
        }

        /// <summary>
        /// Returns whether or not this ID is valid given a set of constraints
        /// </summary>
        /// <param name="simulatedId"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public static bool idMeetsConstraints(ulong simulatedId, RosterConstraints constraints)
        {
            for (int iter = 0; iter < cpdConstrainables.Count; iter++)
            {
                ulong currCPDcategory = 0;

                CPD currCpd = cpdConstrainables[iter];
                currCPDcategory = Utility.FloorToulong(simulatedId / simIDtourGuide[iter]);
                if(constraints.allCurrentConstraints[currCpd.cpdType].Contains(currCpd.categories[(int)currCPDcategory]))
                {
                    return false;
                } else
                {
                    simulatedId -= simIDtourGuide[iter] * currCPDcategory;
                }
            }

            return true;
        }
    }
}





/// <summary>
/// List of roster constraints - what categories of what CPD's to sort by
/// The Player uses this
/// </summary>
public class RosterConstraints
{
    public static RosterConstraints NO_CONSTRAINTS = new RosterConstraints();

    // What CPD type are you restricting, and, what categories in that CPD are you allowing?
    public Dictionary<CPD_Type, HashSet<string>> allCurrentConstraints;
    private HashSet<(CPD_Type, string)> inflexibles;

    private object lockObj = new object();

    public RosterConstraints()
    {
        this.allCurrentConstraints = new Dictionary<CPD_Type, HashSet<string>>();
        this.inflexibles = new HashSet<(CPD_Type, string)>();
    }

    /// <summary>
    /// Adds a category to the constrained list for a particular CPD (do not accept this category.)
    /// Set inflexible to true if this is 100% confirmed and should never be changed the rest of the round.
    /// </summary>
    /// <param name="cpd">CPD to constrain (EG HairStyle, HairColor...)</param>
    /// <param name="constraint">This category will no longer be accepted</param>
    public void addConstraint(CPD_Type onType, string constraint, bool inflexible)
    {
        lock(lockObj)
        {
            if(!inflexibles.Contains((onType, constraint))) {
                allCurrentConstraints[onType].Add(constraint);
                if (inflexible) inflexibles.Add((onType, constraint));
            }
        }
    }

    /// <summary>
    /// Removes a category from the constrained list for a particular CPD (the category will be allowed again.)
    /// </summary>
    /// <param name="cpd">CPD to constrain (EG HairStyle, HairColor...)</param>
    /// <param name="constraint">This category will once again be allowed</param>
    public void removeConstraint(CPD_Type onType, string constraint)
    {
        lock(lockObj)
        {
            if(!inflexibles.Contains((onType, constraint)))
            {
                allCurrentConstraints[onType].Remove(constraint);
            }
        }
    }

    /// <summary>
    /// Makes the given value the only acceptable value for a particular field's constraints
    /// </summary>
    /// <param name="cpd">CPD to constrain (EG HairStyle, HairColor...)</param>
    /// <param name="constraint">Restricts everything but this category</param>
    public void onlyConstraint(CPD_Type onType, string constraint)
    {
        lock (lockObj)
        {
            allCurrentConstraints[onType].Clear();
            Roster.cpdByType[onType].categories.ForEach(cat =>
            {

                allCurrentConstraints[onType].Add(cat);
            });
            allCurrentConstraints[onType].Remove(constraint);
        }
    }

    private void smartClear(CPD_Type onType)
    {
        // can't modify the hashset while it is being used, so have to make a temporary new one.
        foreach (string cat in new HashSet<string>(allCurrentConstraints[onType]))
        {
            if (!inflexibles.Contains((onType, cat)))
            {
                allCurrentConstraints[onType].Remove(cat);
            }
        }
    }

    /// <summary>
    /// Removes all constraints from a CPD (all categories will be allowed again)
    /// </summary>
    /// <param name="cpd">Clear all constraints from this CPD</param>
    public void clearConstraints(CPD_Type onType, bool cleanSweep)
    {
        lock (lockObj)
        {
            if (allCurrentConstraints.ContainsKey(onType))
            {
                if (cleanSweep)
                    allCurrentConstraints[onType].Clear();
                else
                    smartClear(onType);
            }

            else
            {
                //Debug.LogWarning($"Setting up constraints for {onType}");
                allCurrentConstraints.Add(onType, new HashSet<string>());
            }
        }
    }

    public void clearConstraints(CPD cpd, bool cleanSweep)
    {
        clearConstraints(cpd.cpdType, cleanSweep);
    }

    public void clearAllConstraints(bool cleanSweep)
    {
        lock(lockObj)
        {
            foreach (CPD cpd in Roster.cpdConstrainables)
            {
                clearConstraints(cpd, cleanSweep);
            }
        }
    }
}


// ----------------------------------------------------------
// For CPU's version of RosterConstraints, see the file CPURosterLogic
// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^