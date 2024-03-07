shader_type canvas_item;

void fragment() {
	vec4 image = texture(TEXTURE, UV);

	COLOR = image;
}