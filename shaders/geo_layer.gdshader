shader_type canvas_item;

uniform sampler2D tint_color : source_color;
uniform float tint_offset = 0.0;
uniform float tint_lerp = 0.0;

void fragment() {
	COLOR.rgb = mix(COLOR.rgb, texture(tint_color, vec2(tint_offset, 0)).rgb, tint_lerp);
}
