//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSINCLUDE_INCLUDED
#define MYHLSINCLUDE_INCLUDED

// Color codes
static float4 C_HAIR_BASE = float4(0,0,1,1);
static float4 C_HAIR_OUTLINE = float4(0,0,0.6,1);
static float4 C_HAIR_SHADOW = float4(0,0,0.4,1);
static float4 C_HAIR_BACK = float4(0,0,0.2,1);

static float4 C_SKIN_BASE = float4(1,0,0,1);
static float4 C_SKIN_OUTLINE = float4(0.6,0,0,1);

static float4 C_BODY_BASE = float4(0,1,0,1);
static float4 C_BODY_OUTLINE = float4(0,0.6,0,1);

static float4 C_EYE_BASE = float4(0,1,1,1);

static float epsilon = 0.01;

int AppxColor3(float3 Base, float3 Col) {
	if(abs(Base.r - Col.r) <= epsilon && abs(Base.g - Col.g) <= epsilon && abs(Base.b - Col.b) <= epsilon) {
		return 1;
	} else return 0;
}

void Overlay_float(float4 Base, float4 Top, out float4 Out) {
	if(Top.a == 0) 
		Out = Base;
	else Out = Top;
}

// Hair layering: Back hair -> "base" -> All other hair
void OverlayHair_float(float4 Base, float4 Top, out float4 Out) {
	if(distance(Top, C_HAIR_BACK) < epsilon) {
		Out = Base.a == 0 ? Top : Base;
	} else {
		Out = Top.a == 0 ? Base : Top;
	}
}

void ColorSwap_float(float4 Base, float4 LookFor, float4 ReplaceWith, out float4 Res) {
	Res = distance(Base, LookFor) < epsilon ? ReplaceWith : Base;
}

void Colorize_float(float4 Uncolored, float4 SkinColor, float4 HairColor, float4 BodyColor, float4 EyeColor, out float4 Colored) {
	//Colored = Uncolored;
	if(Uncolored.a > epsilon) {
		if(Uncolored.r > epsilon) {
			ColorSwap_float(Uncolored, C_SKIN_BASE, SkinColor, Colored);
			ColorSwap_float(Colored, C_SKIN_OUTLINE, SkinColor * 0.8f, Colored);
		}

		ColorSwap_float(Colored, C_EYE_BASE, EyeColor, Colored);

		if(Uncolored.g > epsilon) {
			ColorSwap_float(Colored, C_BODY_BASE, BodyColor, Colored);
			ColorSwap_float(Colored, C_BODY_OUTLINE, BodyColor * 0.8f, Colored);
		}

		if(Uncolored.b > epsilon) {
			ColorSwap_float(Colored, C_HAIR_BASE, HairColor, Colored);
			ColorSwap_float(Colored, C_HAIR_OUTLINE, HairColor * 0.8f, Colored);
			ColorSwap_float(Colored, C_HAIR_SHADOW, HairColor * 0.6f, Colored);
			ColorSwap_float(Colored, C_HAIR_BACK, HairColor * 0.4f, Colored);
		}
	}
}

#endif