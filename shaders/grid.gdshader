shader_type canvas_item;
render_mode blend_mix;

uniform vec2 grid_size;
uniform float cell_size = 16.0f;

void fragment() {
	float small_lines = 1.0f - clamp(mod(UV.x * grid_size.x, cell_size), 0.0f, 1.0f) * clamp(mod(UV.y * grid_size.y, cell_size), 0.0f, 1.0f); 
	float large_lines = 1.0f - clamp(mod(UV.x * grid_size.x, cell_size * 2.0f), 0.0f, 1.0f) * clamp(mod(UV.y * grid_size.y, cell_size * 2.0f), 0.0f, 1.0f); 
	COLOR.a = small_lines * 0.025f + large_lines * 0.05f;
}