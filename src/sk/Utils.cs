﻿using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Utils
{
	public static readonly Color ManaColor = new Color("4f8fba");
	
    public static int RollDice(string damageString)
    {
	    var dropLowest = damageString.EndsWith("l");
        if (dropLowest) damageString = damageString.Substring(0, damageString.Length - 1);
		
        var damage = 0;
        var numDiceLoc = damageString.IndexOf("d", StringComparison.Ordinal);
        var numDice = 0;
        var diceLoc = damageString.IndexOf("+", StringComparison.Ordinal);
        var dice = 0;

        var addition = 0;
        if (numDiceLoc > -1) numDice = damageString.Substring(0, numDiceLoc).ToInt();
        dice = diceLoc > -1 ? 
	        damageString.Substring(numDiceLoc + 1, diceLoc - 2).ToInt() 
	        : damageString.Substring(numDiceLoc + 1).ToInt();;
		
        switch (diceLoc)
        {
	        case > -1:
		        addition = damageString.Substring(diceLoc + 1).ToInt();
		        break;
	        case -1 when numDiceLoc == -1:
		        damage = damageString.ToInt();
		        break;
        }

        List<int> rolls = new();
        for (var i = 0; i < numDice; i++)
        {
	        rolls.Add(new RandomNumberGenerator().RandiRange(1, dice));
        }

        if (dropLowest) rolls.Remove(rolls.Min());
        damage += rolls.Sum() + addition;
        return damage;
    }

    public static Vector2 Abs(Vector2 vector2)
    {
	    return new Vector2(Math.Abs(vector2.X), Math.Abs(vector2.Y));
    }

    public static float Max(Vector2 vector2)
    {
	    return Math.Max(vector2.X, vector2.Y);
    }

    public static bool IsEqualApprox(Vector2 a, Vector2 b, float tolerance = .001f) => Mathf.Abs(a.X - b.X) < tolerance && Mathf.Abs(a.Y - b.Y) < tolerance;
    public static bool IsEqualApprox(float a, float b, float tolerance = .001f) => Mathf.Abs(a - b) < tolerance;

    public static Vector2 GetSpriteSize(Sprite2D sprite)
	{
		var apparentSize = new Vector2(sprite.Texture.GetWidth() / (float)sprite.Hframes, sprite.Texture.GetHeight() / (float)sprite.Vframes);
	    return apparentSize * sprite.Scale;
	}
    
    public static double NoiseNorm(Noise noise, Vector2 position)
    {
	    return (noise.GetNoise2Dv(position) + 1) / 2.0;
    }

    public static Vector2 ScaleForSize(Sprite2D sprite, float width, float height)
    {
	    return ScaleForSize(sprite, new Vector2(width, height));
    }
    
    public static Vector2 ScaleForSize(Sprite2D sprite, Vector2 size)
    {
	    return new Vector2(size.X / (float) sprite.Texture.GetWidth(),
		    size.Y / (float) sprite.Texture.GetHeight());
    }
}