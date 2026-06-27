//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSINCLUDE_INCLUDED
#define MYHLSINCLUDE_INCLUDED

float epsilon = 0.01f;

// Color codes
static float3 C_HAIR_BASE = float3(0,0,1);
static float3 C_HAIR_OUTLINE = float3(0,0,0.6);
static float3 C_HAIR_SHADOW = float3(0,0,0.4);
static float3 C_HAIR_BACK = float3(0,0,0.2);

static float3 C_SKIN_BASE = float3(1,0,0);
static float3 C_SKIN_OUTLINE = float3(0.6,0,0);

static float3 C_BODY_BASE = float3(0,1,0);
static float3 C_BODY_OUTLINE = float3(0,0.6,0);

static float3 C_EYE_BASE = float3(0,1,1);


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

void OverlayHair_float(float4 Base, float4 Top, out float4 Out) {
	if(Top.a == 0) 
		Out = Base;
	else if(AppxColor3(Top.rgb, C_HAIR_BACK) && Base.a > epsilon) Out = Base;
	else Out = Top;
}

void Colorize_float(float4 Uncolored, float4 SkinColor, float4 HairColor, float4 BodyColor, float EyeColor, out float4 Colored) {
	if(Uncolored.a > epsilon) {
		// Red: Skin color
		if(Uncolored.r > epsilon) {
			if(AppxColor3(Uncolored.rgb, C_SKIN_BASE)) {
				Colored = SkinColor;
			} else if(AppxColor3(Uncolored.rgb, C_SKIN_OUTLINE)) {
				Colored = SkinColor * 0.8f;
			}
			else {
				Colored = Uncolored;
			}
		}
		// Green: Body / Shirt color
		else if(Uncolored.g > epsilon) {
			if(AppxColor3(Uncolored.rgb, C_BODY_BASE)) {
				Colored = BodyColor;
			} else if(AppxColor3(Uncolored.rgb, C_BODY_OUTLINE)) {
				Colored = BodyColor * 0.8f;
			} else if(AppxColor3(Uncolored.rgb, C_EYE_BASE)) {
				Colored = EyeColor;
			}
			else {
				Colored = Uncolored;
			}
		}
		// Blue: Hair color
		else if(Uncolored.b > epsilon) {
			if(AppxColor3(Uncolored.rgb, C_HAIR_BASE)) {
				Colored = HairColor;
			} else if(AppxColor3(Uncolored.rgb, C_HAIR_OUTLINE)) {
				Colored = HairColor * 0.8f;
			} else if(AppxColor3(Uncolored.rgb, C_HAIR_SHADOW)) {
				Colored = HairColor * 0.6f;
			} else if(AppxColor3(Uncolored.rgb, C_HAIR_BACK)) {
				Colored = HairColor * 0.2f;
			}
			else {
				Colored = Uncolored;
			}
		}
		else {
			Colored = Uncolored;
		}
	}
}

#endif