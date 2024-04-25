shader_type canvas_item;

global uniform sampler2D palette : source_color, filter_nearest;

void fragment() {
	vec4 color = texture(TEXTURE, UV);
	if (color == vec4(1.0))
		discard;
	
	float depth = color.r * 255.0;
	float paletteX = (mod(depth-0.01, 30.0) + 0.01) / 33.0;
	if (depth > 90.0) // LIT
	{
		depth -= 90.0;
		COLOR = texture(palette, vec2(paletteX, (5.0 + floor(-depth / 30.0)) / 16.0));
	}
	else //UNLIT
		COLOR = texture(palette, vec2(paletteX, (8.0 + floor(-depth / 30.0)) / 16.0));
	
	COLOR.rgb = mix(COLOR.rgb, vec3(0.2588, 0.5843, 0.3686), color.b);
}
