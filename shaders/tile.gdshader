shader_type canvas_item;

global uniform sampler2D palette : source_color, filter_nearest;
varying vec4 modulate;

void vertex() {
	modulate = COLOR;
}

void fragment() {
	vec3 color = texture(TEXTURE, UV).rgb;
	if (color == vec3(1.0, 1.0, 1.0))
		discard;
	else if (color == vec3(1.0, 0.0, 0.0))
		COLOR = texture(palette, vec2(0.0, 4.0f/16.0f));
	else if (color == vec3(0.0, 1.0, 0.0))
		COLOR = texture(palette, vec2(0.0, 3.0f/16.0f));
	else if (color == vec3(0.0, 0.0, 1.0))
		COLOR = texture(palette, vec2(0.0, 2.0f/16.0f));
	COLOR *= modulate;
}
