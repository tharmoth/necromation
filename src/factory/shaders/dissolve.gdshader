shader_type canvas_item;

uniform vec2 character_position;
uniform sampler2D dissolve_texture : hint_default_white;

void fragment() {
	vec4 main_texture = texture(TEXTURE, UV);
    vec4 noise_texture = texture(dissolve_texture, UV);
    main_texture.a *= floor(0.5 + min(1, noise_texture.x));
    COLOR = main_texture;
}