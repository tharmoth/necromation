shader_type canvas_item;

void fragment() {
	float grayscale = (COLOR.r + COLOR.g + COLOR.b) / 3.0;
	
	COLOR = vec4(grayscale, grayscale, grayscale, COLOR.a);
}
