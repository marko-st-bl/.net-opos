#pragma once
#include <string>
#include <string>

namespace GameOfLifeLibrary {
	extern "C" class GameOfLife {

		private:
			unsigned char* d_lifeData;
			unsigned char* d_resultLifeData;

			size_t worldWidth;
			size_t worldHeight;

		public:

			GameOfLife() {
				d_lifeData = nullptr;
				d_resultLifeData = nullptr;
				worldWidth = 0;
				worldHeight = 0;
			}

			void init_world(int width, int height);
			void game_of_life(int iteration);
			void next_generation_with_oscilator_detection();
			void free_memory();
			void init_with_image(std::string path);
			void write_image(std::string path, int imageWidth, int imageHeight);
			};
}