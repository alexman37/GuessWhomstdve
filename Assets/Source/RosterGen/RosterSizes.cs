using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RosterSizes
{
    /// <summary>
    /// Returns one of several pre-configured CPD lists for different roster sizes.
    /// If you're using a different list in the same game instance, you should take steps to clear the old list out. (TODO)
    /// </summary>
    public static RosterSizeData GetRosterSizeData(ushort sizeBracket)
    {
        List<CPD> finalInstances;

        // Don't instantiate these lists unless we have to. Once a CPD is created it adds itself to the registry...
        // There's also no need to instantiate these unless we actually need them
        switch (sizeBracket)
        {
            // 10
            case 0:
                finalInstances = new List<CPD>()
                {
                    new CPD_SimpleIndex(CPD_Type.HairStyle, true, "properties/hairStyles", -1),
                    new CPD_Color(CPD_Type.HairColor, true, "properties/hairTones", -1),
                };
                break;
            // 100
            case 1:
                finalInstances = new List<CPD>()
                {
                    new CPD_SimpleIndex(CPD_Type.HairStyle, true, "properties/hairStyles", -1),
                    new CPD_Color(CPD_Type.HairColor, true, "properties/hairTones", -1),
                    new CPD_Color(CPD_Type.SkinTone, true, "properties/skinTones", -1),
                    new CPD_SimpleIndex(CPD_Type.Gender, true, "properties/gender", -1),
                };
                break;
            // 1,000
            case 2:
                finalInstances = new List<CPD>()
                {
                    new CPD_SimpleIndex(CPD_Type.HairStyle, true, "properties/hairStyles", -1),
                    new CPD_Color(CPD_Type.HairColor, true, "properties/hairTones", -1),
                    new CPD_Color(CPD_Type.SkinTone, true, "properties/skinTones", -1),
                    new CPD_SimpleIndex(CPD_Type.Gender, true, "properties/gender", -1),
                    new CPD_SimpleIndex(CPD_Type.Height, true, "properties/heights", -1),
                    new CPD_SimpleIndex(CPD_Type.Weight, true, "properties/weights", -1),
                };
                break;
            // 10,000
            case 3:
                finalInstances = new List<CPD>()
                {
                    new CPD_SimpleIndex(CPD_Type.HairStyle, true, "properties/hairStyles", -1),
                    new CPD_Color(CPD_Type.HairColor, true, "properties/hairTones", -1),
                    new CPD_Color(CPD_Type.SkinTone, true, "properties/skinTones", -1),
                    new CPD_SimpleIndex(CPD_Type.Gender, true, "properties/gender", -1),
                    new CPD_SimpleIndex(CPD_Type.Height, true, "properties/heights", -1),
                    new CPD_SimpleIndex(CPD_Type.Weight, true, "properties/weights", -1),
                    new CPD_Color(CPD_Type.EyeColor, true, "properties/eyeColors", -1),
                };
                break;
            // 100,000
            case 4:
                finalInstances = new List<CPD>()
                {
                    new CPD_SimpleIndex(CPD_Type.HairStyle, true, "properties/hairStyles", -1),
                    new CPD_Color(CPD_Type.HairColor, true, "properties/hairTones", -1),
                    new CPD_Color(CPD_Type.SkinTone, true, "properties/skinTones", -1),
                    new CPD_SimpleIndex(CPD_Type.Gender, true, "properties/gender", -1),
                    new CPD_SimpleIndex(CPD_Type.Height, true, "properties/heights", -1),
                    new CPD_SimpleIndex(CPD_Type.Weight, true, "properties/weights", -1),
                    new CPD_Color(CPD_Type.EyeColor, true, "properties/eyeColors", -1),
                    new CPD_Color(CPD_Type.FavoriteColor, true, "properties/faveColors", -1),
                };
                break;
            // TODO
            default:
                finalInstances = new List<CPD>()
                {
                    new CPD_SimpleIndex(CPD_Type.HairStyle, true, "properties/hairStyles", -1),
                    new CPD_Color(CPD_Type.HairColor, true, "properties/hairTones", -1),
                    new CPD_Color(CPD_Type.SkinTone, true, "properties/skinTones", -1),
                    new CPD_SimpleIndex(CPD_Type.Gender, true, "properties/gender", -1),
                    new CPD_SimpleIndex(CPD_Type.Height, true, "properties/heights", -1),
                    new CPD_SimpleIndex(CPD_Type.Weight, true, "properties/weights", -1),
                    new CPD_Color(CPD_Type.EyeColor, true, "properties/eyeColors", -1),
                    new CPD_Color(CPD_Type.FavoriteColor, true, "properties/faveColors", -1),
                    new CPD_SimpleIndex(CPD_Type.BloodType, true, "properties/bloodtypes2", -1),
                    new CPD_SimpleIndex(CPD_Type.Zodiac, true, "properties/zodiacs", -1),
                    new CPD_SimpleIndex(CPD_Type.Job, true, "properties/jobs", -1),
                    //new CPD_SimpleIndex(CPD_Type.City_L1, true, "properties/cities_l1", -1),
                    new CPD_SimpleIndex(CPD_Type.Region_L2, true, "properties/regions_l2", -1),
                    new CPD_SimpleIndex(CPD_Type.City_L2, true, "properties/cities_l2", (int) CPD_Type.Region_L2),
                };
                break;
        }



        // Generic CPDs: Add these no matter what
        finalInstances.Add(new CPD_SimpleIndex(CPD_Type.BodyType, false, "properties/bodyTypes", -1));
        finalInstances.Add(new CPD_SimpleIndex(CPD_Type.Face, false, "properties/faceTypes", -1));
        finalInstances.Add(new CPD_SimpleIndex(CPD_Type.HeadType, false, "properties/headTypes", -1));

        return new RosterSizeData
        {
            instances = finalInstances
        };
    }
}


public struct RosterSizeData
{
    public List<CPD> instances;
}