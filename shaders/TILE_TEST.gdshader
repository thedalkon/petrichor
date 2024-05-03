shader_type canvas_item;

varying vec4 modulate;

global uniform vec2 shadow_offset;
global uniform vec2 camera_offset;
global uniform sampler2D palette : source_color, filter_nearest;

void vertex() {
	modulate = COLOR;
}

void fragment() {
	vec2 tile_size = vec2(modulate.r * 255.0f, modulate.g * 255.0f);
	int layer_count = int(modulate.b * 255.0f);
	int buffer_tiles = int(modulate.a * 255.0f);;
	
	vec2 texture_size = vec2(textureSize(TEXTURE, 0));
	vec2 render_size = (vec2(tile_size + vec2(1.0) * float(buffer_tiles) * 2.0) * 20.0) / texture_size;
	vec3 color = vec3(1.0);
	
	COLOR = texture(TEXTURE, UV);
	
	if (UV.y > render_size.y)
		discard;
	
	for (int i = layer_count - 1; i >= 0; i--) // Iterate over every layer
	{
		vec2 layerUV = UV + vec2(0, 1.0) * float(i) * render_size.y + camera_offset / texture_size * float(layer_count - i) + vec2(0, 1.0 / texture_size.y);
		vec3 layerColor = 
		texture(TEXTURE, clamp(layerUV, vec2(0.0), vec2(1.0))).rgb;
		
		if (layerColor == vec3(1.0))
			continue;
		
		float topLayerPresence = 0.0f;
		if (shadow_offset != vec2(0.0))
		{
			for (int j = 0; j < i; j++) // Shadows
			{
				vec2 topLayerUV = layerUV - vec2(0, 1.0) * float(j) * render_size.y;
				vec4 topLayerColor = texture(TEXTURE, topLayerUV + shadow_offset / texture_size);
				topLayerPresence += -(topLayerColor.r * topLayerColor.g * topLayerColor.b) + 1.0f;
			}
		}
		
		topLayerPresence = clamp(topLayerPresence, 0.0, 1.0);
		layerColor = texture(palette, vec2(float(i) / 30.0f, 
			4.0f/16.0f * layerColor.r + 3.0f/16.0f * layerColor.g + 2.0f/16.0f * layerColor.b + topLayerPresence * 1.0f/16.0f)).rgb;
		
		color = layerColor;
	}
	
	if (color.rgb == vec3(1.0))
		discard;
	
	COLOR.rgb = color;
}
