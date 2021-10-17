#include <stdio.h>

namespace GameOfLifeCUDALibrary {

	struct Pixel {
		unsigned char r, g, b;
	};

	void game_of_life_cuda(unsigned char*& lifeData, const int worldWidth, const int worldHeight, unsigned char*& resultLifeData, int generations);
	void init_world_with_image_cuda(int imageWidth, int imageHeight, unsigned char* imageBuffer, int worldWidth, int worldHeight,
		unsigned char* lifeData);
	void init_world_cuda(unsigned char*& lifeData, int worldWidth, int worldHeight, unsigned char*& resultLifeData);
	void write_image_cuda(int imageWidth, int imageHeight, unsigned char* imageBuffer, int worldWidth, int worldHeight, unsigned char* lifeData);
	void oscilator_detection(unsigned char*& lifeData, int worldWidth, int worldHeight, unsigned char*& resultLifeData, Pixel* image);
}