shader_type canvas_item;

uniform int layers = 5;
global uniform vec2 shadow_offset;
uniform int current_layer = 0;
uniform ivec2 tex_size;
global uniform sampler2D palette : source_color, filter_nearest;

void fragment() {
	
	ivec2 atlas_size = textureSize(TEXTURE, 0);
	vec4 final_col = vec4(0.0);
	
	if (UV.y > float(tex_size.y) / float(atlas_size.y) + 1.0 / float(atlas_size.y)) discard;
	
	for (float i = 30.0; i > 0.0; i--)
	{
		float f = round(i * float(layers - 1) / 30.0);
		vec2 layerUV = UV + vec2(0.0, float(tex_size.y) / float(atlas_size.y) * f) + vec2(0.005 * (2.0 * SCREEN_UV.x - 1.0), 0.001 * (2.0 * SCREEN_UV.y - 1.0)) * i;
		if (layerUV.y > 1.0 || layerUV.y < 0.0) continue;
		vec4 col = texture(TEXTURE, layerUV);
		if (col.rgb == vec3(1.0) || col.rgb == vec3(0.0)) {continue;}
		COLOR.rgb = col.rgb * (1.0 / i);
	}
	
	if (COLOR.rgb == vec3(1.0)) discard;
	
}
