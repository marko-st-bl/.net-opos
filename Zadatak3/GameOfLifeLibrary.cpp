#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include "GameOfLifeLibrary.h"
#include <iostream>

#include <stdio.h>
#include <string>


#include "GameOfLife.cuh"
#include "GameOfLifeLibrary.h"

namespace GameOfLifeLibrary {

	/*struct Pixel
	{
		unsigned char R, G, B;
	};*/

	static void writeRGBImage(const char* filename, const GameOfLifeCUDALibrary::Pixel* array, const int width, const int height)
	{
		FILE* fp = fopen(filename, "wb"); /* b - binary mode */
		fprintf(fp, "P6\n%d %d\n255\n", width, height);
		fwrite(array, sizeof(GameOfLifeCUDALibrary::Pixel), (size_t)width * (size_t)height, fp);
		fclose(fp);
	}

	static void readImage(const char* filename, unsigned char*& array, int& width, int& height) {
		FILE* fp = fopen(filename, "rb");
		if (!fscanf(fp, "P5\n%d %d\n255\n", &width, &height))
		{
			throw "Error opening file";
		}
		unsigned char* image = new unsigned char[(size_t)width * (size_t)height];
		//unsigned char* image = new unsigned char[900];
		fread(image, sizeof(unsigned char), (size_t)width * (size_t)height, fp);
		fclose(fp);
		array = image;
	}

	static void writeImage(const char* filename, const unsigned char* array, int width, int height) {
		FILE* fp = fopen(filename, "wb");
		fprintf(fp, "P5\n%d %d\n255\n", width, height);
		fwrite(array, sizeof(unsigned char), (size_t)width * (size_t)height, fp);
		fclose(fp);
	}

	void GameOfLife::game_of_life(int generations) {
		GameOfLifeCUDALibrary::game_of_life_cuda(d_lifeData, worldWidth, worldHeight, d_resultLifeData, generations);
	}

	void GameOfLife::init_world(int width, int height) {
		worldWidth = width;
		worldHeight = height;
		GameOfLifeCUDALibrary::init_world_cuda(d_lifeData, worldWidth, worldHeight, d_resultLifeData);
	}

	void GameOfLife::free_memory() {
		cudaFree(d_lifeData);
		cudaFree(d_resultLifeData);
	}

	void GameOfLife::init_with_image(std::string path)
	{
		int width = -1;
		int height = -1;
		unsigned char* h_buffer = nullptr;
		unsigned char* d_buffer = nullptr;
		readImage(path.c_str(), h_buffer, width, height);

		if (cudaMalloc(&d_buffer, (size_t)width * height * sizeof(unsigned char))) {
			printf("Allocation failed!!!");
		}
		cudaMemcpy(d_buffer, h_buffer, (size_t)width * (size_t)height * sizeof(unsigned char), cudaMemcpyHostToDevice);
		GameOfLifeCUDALibrary::init_world_with_image_cuda(width, height, d_buffer, worldWidth, worldHeight, d_lifeData);
		free(h_buffer);
		cudaFree(d_buffer);
	}

	void GameOfLife::write_image(std::string name, int imageWidth, int imageHeight)
	{
		unsigned char* h_buffer = nullptr;
		unsigned char* d_buffer = nullptr;
		const std::string outputFile = std::string(name) + std::string(".pgm");
		if (cudaMalloc(&d_buffer, (size_t)imageWidth * imageHeight * sizeof(unsigned char))) {
			printf("Allocation failed!!!");
		}
		h_buffer = (unsigned char*)malloc((size_t)imageWidth * (size_t)worldWidth);
		GameOfLifeCUDALibrary::write_image_cuda(imageWidth, imageHeight, d_buffer, worldWidth, worldHeight, d_lifeData);
		cudaMemcpy(h_buffer, d_buffer, (size_t)imageWidth * (size_t)imageHeight * sizeof(unsigned char), cudaMemcpyDeviceToHost);
		writeImage(outputFile.c_str(), h_buffer, imageWidth, imageHeight);
		//free host memory
		free(h_buffer);
		//free device memory
		cudaFree(d_buffer);
	}

	void GameOfLife::next_generation_with_oscilator_detection()
	{
		GameOfLifeCUDALibrary::Pixel* h_image;
		GameOfLifeCUDALibrary::Pixel* d_image;
		int bytes = worldWidth * worldHeight * sizeof(GameOfLifeCUDALibrary::Pixel);

		//allocate memory
		h_image = (GameOfLifeCUDALibrary::Pixel*)malloc(bytes);
		cudaMalloc(&d_image, bytes);

		//run kernel
		GameOfLifeCUDALibrary::oscilator_detection(d_lifeData, worldWidth, worldHeight, d_resultLifeData, d_image);
		//copy memory from device
		cudaMemcpy(h_image, d_image, bytes, cudaMemcpyDeviceToHost);
		//write image
		writeRGBImage("blinker.ppm", h_image, worldWidth, worldHeight);
		//free memory
		free(h_image);
		cudaFree(d_image);
	}

}