shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform float lod: hint_range(0.0, 5) = 2;

void fragment(){
	if (COLOR.a > .01) {
		COLOR = texture(SCREEN_TEXTURE, SCREEN_UV, lod);
	}
}