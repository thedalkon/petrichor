shader_type canvas_item;

varying vec4 modulate;
varying vec4 data;

global uniform vec2 shadow_offset;
global uniform vec2 camera_offset;
global uniform sampler2D palette : source_color, filter_nearest;

void vertex() {
	modulate = COLOR;
}

void fragment() {
	COLOR = texture(TEXTURE, UV);
	if (COLOR.rgb == vec3(1.0))
		discard;
	COLOR.rgb += modulate.rgb;
}
