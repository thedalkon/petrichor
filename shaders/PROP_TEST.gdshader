shader_type canvas_item;

const int layers = 30;
global uniform vec2 shadow_offset;
uniform int current_layer = 0;
global uniform sampler2D palette : source_color, filter_nearest;
uniform bool color_key;

void fragment() {
	vec3 tex = COLOR.rgb;
	float depth = tex.g * float(layers - 1);

	if (texture(TEXTURE, UV).rgb == vec3(1, 1, 1))
		discard;

	if (int(floor(depth)) < current_layer)
		discard;
	else
		COLOR.rgb = tex;

	float paletteDepth = 0.9 - floor(depth - 1.0) / 30.0;
	float fog_intensity = texture(palette, vec2(9.0 / 32.0, 0.0)).r;
	vec4 sampleCol = textureLod(TEXTURE, UV - shadow_offset * 0.01, 1.0);
	if (!color_key)
	{
		if (sampleCol.g > tex.g && sampleCol.r < 0.1) // In shadow
			COLOR = texture(palette, vec2(paletteDepth - paletteDepth * fog_intensity , 0.26666666666));
		else
			COLOR = texture(palette, vec2(paletteDepth - paletteDepth * fog_intensity, .2));
	} else
	{
		COLOR = mix(texture(TEXTURE, UV + vec2(0, 0.5)),
					texture(palette, vec2(paletteDepth - paletteDepth * fog_intensity , 0.26666666666)),
					0.5);
	}
}