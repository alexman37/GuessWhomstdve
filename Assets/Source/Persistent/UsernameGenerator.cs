using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A mostly harmless class that generates a default username if none is given
/// </summary>
public static class UsernameGenerator
{
    // Just define the lists here. They're not that important
    private static string[] namePrefix = new string[]
    {
        "Red","Orange","Yellow","Green","Blue","Purple","Brown","Black","White","Pink","Crimson","Scarlet","Magenta","Maroon",
        "Olive","Golden","Lime","Aqua","Cyan","Teal","Violet","Amber","Gray"
    };

    private static string[] nameSuffix = new string[]
    {
        "Cat","Dog","Whale","Fish","Rooster","Chicken","Cow","Pig","Rabbit","Sheep","Duck","Shark","Turtle","Bear","Lion","Zebra",
        "Giraffe","Hippo","Snake","Lizard","Bird","Seagull","Eagle","Hawk","Falcon","Panther","Buffalo","Gopher","Dolphin","Snail",
        "Jaguar","Tiger","Raven","Crow","Beagle","Terrier","Horse"
    };

    public static string getNewUsername()
    {
        string prefix = namePrefix[UnityEngine.Random.Range(0, namePrefix.Length)];
        string suffix = nameSuffix[UnityEngine.Random.Range(0, nameSuffix.Length)];
        string number = UnityEngine.Random.Range(1, 100).ToString();
        return prefix + suffix + number;
    }
}
