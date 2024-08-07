shader_type canvas_item;

uniform sampler2D soil;
uniform sampler2D grass;
uniform sampler2D rock;
uniform sampler2D ocean;

uniform sampler2D map_data;
uniform sampler2D blend;

uniform bool enabled = false;
uniform bool should_blend = true;

varying vec2 world_position;
varying vec2 local_position;

const float tile_size = 32.0f;

int getTileID(vec2 tile_position) {
	float tileRed = texelFetch(map_data, ivec2(tile_position), 0).r;
	return int(tileRed * 255.0f);
}

vec4 getColorForPixel(vec2 position, int tileID) {
	if (tileID == 4) {
		return texture(ocean, position / 1024.0f);
	} else if (tileID == 3) {
		return texture(rock, position / 1024.0f);
	} else if (tileID == 2) {
		return texture(grass, position / 1024.0f);
	} else if (tileID == 1) {
		return texture(soil, position / 1024.0f);
	} else {
		return vec4(0, 0, 0, 0);
	}
}

void vertex() {
	world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	if (enabled) {
		vec2 tile = floor(world_position / tile_size);
		int tileID = getTileID(tile);
		vec2 pixel_pos_in_tile = mod(world_position, tile_size);
		vec2 texture_position = mod(world_position, 1024.0f);
		vec4 colorSelf = getColorForPixel(texture_position, tileID);

		if (!should_blend) {
			COLOR = colorSelf;
		} else {
			// Do blending

			// Find closest edges
			// If these numbers are negative they are left / up
			// Could be 0 at center of tile but does not matter as we will just sample ourselves
			// and corner / sides should have no effect at center
			float horizontal = clamp((tile_size / 2.0f * -1.0f) + pixel_pos_in_tile.x, -1.0f, 1.0f);
			float vertical = clamp((tile_size / 2.0f * -1.0f) + pixel_pos_in_tile.y, -1.0f, 1.0f);
			// Corner pos is 0,0 - 0,64 - 64,0 - 64,64
			vec2 cornerPos = vec2(max(horizontal * tile_size, 0.0f),
								  max(vertical * tile_size, 0.0f));


			// Grab the tile ID of the surrounding tiles we care about
			int tileIdHorizontal = getTileID(tile+vec2(horizontal,0));
			int tileIdVertical = getTileID(tile+vec2(0,vertical));
			int tileIdCorner = getTileID(tile+vec2(horizontal,vertical));

			// Grab for the current pixel with the neighbour tile IDs
			vec4 colorHorizontal = getColorForPixel(texture_position, tileIdHorizontal);
			vec4 colorVertical = getColorForPixel(texture_position, tileIdVertical);
			vec4 colorCorner = getColorForPixel(texture_position, tileIdCorner);

			// Fetch the blend strength from our blend texture
			// First figure out which tile in the blend texture to use, the blend texture
			// simply repeats forever
			//float modX = (mod(tile.x, blendTextureTiles) * tile_size) + pixel_pos_in_tile.x;
			//float modY = (mod(tile.y, blendTextureTiles) * tile_size) + pixel_pos_in_tile.y;

			// Extract blend strength as a number from 0 to 255 and convert it to 0 to 1 value
			vec4 blendStrength = texelFetch(blend, ivec2(pixel_pos_in_tile), 0);
			if (tileIdHorizontal == 0) {
				blendStrength.a += blendStrength.r;
				blendStrength.r = 0.0f;
			}
			if (tileIdVertical == 0) {
				blendStrength.a += blendStrength.g;
				blendStrength.g = 0.0f;
			}
			if (tileIdCorner == 0) {
				blendStrength.a += blendStrength.b;
				blendStrength.b = 0.0f;
			}
			//blendStrength.rgb = blendStrength.rgb * 4.0f;
			//blendStrength = blendStrength / (blendStrength.r + blendStrength.g + blendStrength.b + blendStrength.a);
			float strHorizontal = (blendStrength.r * 255.) / 100.0f;
			float strVertical = (blendStrength.g * 255.) / 100.0f;
			float strCorner = (blendStrength.b * 255.) / 100.0f;
			float strSelf = (blendStrength.a * 255.) / 100.0f;

			// Blend based on percentages

			COLOR = (colorSelf * strSelf) + colorHorizontal * strHorizontal
			+ colorVertical * strVertical + colorCorner * strCorner;

			//COLOR = (colorSelf * strSelf) + (colorHorizontal * strHorizontal) +
					//(colorVertical * strVertical) + (colorCorner * strCorner);
					//(colorVertical * strVertical) + (colorCorner * strCorner);
		}
	}
}
