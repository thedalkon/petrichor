shader_type canvas_item;

uniform sampler2D tint_color : source_color;
uniform float tint_offset = 0.0;
uniform float tint_lerp = 0.0;
varying flat vec4 modulate;
varying flat vec4 data;

void vertex() {
	modulate = COLOR;
	data = INSTANCE_CUSTOM;
}

void fragment() {
	vec2 tile_size = vec2(data.r * 255.0f, data.g * 255.0f);
	float layer_count = data.b * 255.0f;
	float buffer_tiles = data.a * 255.0f;
	vec2 texture_size = vec2(textureSize(TEXTURE, 0));

	if (UV.y * texture_size.y > tile_size.y * 16.0f)
		discard;

	vec2 uv = (UV * texture_size.y + vec2(0.0f, (tile_size.y + buffer_tiles * 2.0f) * layer_count * 20.0f + 1.05f)) / texture_size.y;
	COLOR = texture(TEXTURE, uv);

	vec3 unmodulatedColor = texture(TEXTURE, uv).rgb;
	if (unmodulatedColor == vec3(1.0, 1.0, 1.0))
		discard;
	if (unmodulatedColor == vec3(0.0, 0.0, 0.0))
		COLOR.rgb = vec3(1.0, 1.0, 1.0) * modulate.rgb;

	COLOR.rgb = mix(COLOR.rgb, texture(tint_color, vec2(tint_offset, 0)).rgb, tint_lerp);
}
