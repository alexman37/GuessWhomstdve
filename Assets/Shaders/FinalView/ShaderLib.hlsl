//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSINCLUDE_INCLUDED
#define MYHLSINCLUDE_INCLUDED

// Color codes
static float4 C_HAIR_BASE = float4(0,0,1,1);
static float4 C_HAIR_OUTLINE = float4(0,0,0.6,1);
static float4 C_HAIR_SHADOW = float4(0,0,0.4,1);
static float4 C_HAIR_BACK = float4(0,0,0.2,1);
static float4 C_HAIR_CUT = float4(1,0,1,1);
static float4 C_HAIR_CUT_LINE = float4(1,0,0.6,1);

static float4 C_SKIN_BASE = float4(1,0,0,1);
static float4 C_SKIN_OUTLINE = float4(0.6,0,0,1);

static float4 C_BODY_BASE = float4(0,1,0,1);
static float4 C_BODY_OUTLINE = float4(0,0.6,0,1);

static float4 C_EYE_BASE = float4(0,1,1,1);
static float4 C_EYE_OUTLINE = float4(0,0.8,0.8,1);

static float epsilon = 0.01;

int AppxColor3(float3 Base, float3 Col) {
	if(abs(Base.r - Col.r) <= epsilon && abs(Base.g - Col.g) <= epsilon && abs(Base.b - Col.b) <= epsilon) {
		return 1;
	} else return 0;
}

float4 Overlay_float(float4 Base, float4 Top) {
	if(Top.a == 0) 
		return Base;
	else return Top;
}

// Hair layering: Back hair -> "base" -> All other hair
float4 OverlayHair_float(float4 Base, float4 Top) {
	if(distance(Top, C_HAIR_BACK) < epsilon) {
		return Base.a == 0 ? Top : Base;
	} else {
		return Top.a == 0 ? Base : Top;
	}
}

// Jobs sometimes have hats, which can restrict how long/wide hair is with pink regions
float4 OverlayJob_float(float4 Base, float4 Top) {
	if(distance(Top, C_HAIR_CUT) < epsilon) {
		return float4(0,0,0,0);
	} else if(distance(Top, C_HAIR_CUT_LINE) < epsilon) {
		// Probably hair
		if(Base.b > 0) {
			return C_HAIR_OUTLINE;
		}
		// Probably clothes or empty space
		else {
			return Base.a > 0 ? Base : float4(0,0,0,0);
		}
	} else {
		return Top.a == 0 ? Base : Top;
	}
}

// Some textures are slightly larger on top (e.g. 64x96) to account for height differences
float4 OverlayTaller_float(float4 Base, float4 Top) {
	if(Top.a == 0) 
		return Base;
	else return Top;
}

float4 ColorSwap_float(float4 Base, float4 LookFor, float4 ReplaceWith) {
	return distance(Base, LookFor) < epsilon ? ReplaceWith : Base;
}

float4 Colorize_float(float4 Uncolored, float4 SkinColor, float4 HairColor, float4 BodyColor, float4 EyeColor) {
	//Colored = Uncolored;
	if(Uncolored.a > epsilon) {
		float4 Colored = Uncolored;
		if(Uncolored.r > epsilon) {
			Colored = ColorSwap_float(Uncolored, C_SKIN_BASE, SkinColor);
			Colored = ColorSwap_float(Colored, C_SKIN_OUTLINE, SkinColor * 0.8f);
		}

		if(Uncolored.g > epsilon) {
			Colored = ColorSwap_float(Colored, C_BODY_BASE, BodyColor);
			Colored = ColorSwap_float(Colored, C_BODY_OUTLINE, BodyColor * 0.8f);
		}

		if(Uncolored.b > epsilon) {
			Colored = ColorSwap_float(Colored, C_HAIR_BASE, HairColor);
			Colored = ColorSwap_float(Colored, C_HAIR_OUTLINE, HairColor * 0.8f);
			Colored = ColorSwap_float(Colored, C_HAIR_SHADOW, HairColor * 0.6f);
			Colored = ColorSwap_float(Colored, C_HAIR_BACK, HairColor * 0.4f);

			Colored = ColorSwap_float(Colored, C_EYE_BASE, EyeColor);
			Colored = ColorSwap_float(Colored, C_EYE_OUTLINE, EyeColor * 0.6);
		}
		return Colored;
	}
	return Uncolored;
}

#endif