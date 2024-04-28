shader_type canvas_item;

uniform sampler2D tint_color : source_color;
uniform float tint_offset = 0.0;
uniform float tint_lerp = 0.0;
varying flat vec4 modulate;

void vertex() {
	modulate = COLOR;
}

void fragment() {
	vec3 unmodulatedColor = texture(TEXTURE, UV).rgb;
	if (unmodulatedColor == vec3(1.0, 1.0, 1.0))
		discard;
	if (unmodulatedColor == vec3(0.0, 0.0, 0.0))
		COLOR.rgb = vec3(1.0, 1.0, 1.0) * modulate.rgb;
	
	COLOR.rgb = mix(COLOR.rgb, texture(tint_color, vec2(tint_offset, 0)).rgb, tint_lerp);
}
