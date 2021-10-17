#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <iostream>
#include "GameOfLife.cuh"

namespace GameOfLifeCUDALibrary {

	__global__ void GameOfLife(const unsigned char* lifeData, const int worldWidth, const int worldHeight, unsigned char* resultLifeData)
	{
		int worldSize = worldWidth * worldHeight;
		int index = blockIdx.x * blockDim.x + threadIdx.x;
		int stride = blockDim.x * gridDim.x;

		for (int i = index; i < worldSize; i += stride)
		{
			int x = i % worldWidth;
			int yAbs = i - x;

			int xLeft = (x + worldWidth - 1) % worldWidth;
			int xRight = (x + 1) % worldWidth;

			int yAbsUp = (yAbs + worldSize - worldHeight) % worldSize;
			int yAbsDown = (yAbs + worldHeight) % worldSize;

			//Count alive cells
			int aliveCells = lifeData[xLeft + yAbsUp] + lifeData[x + yAbsUp] + lifeData[xRight + yAbsUp]
				+ lifeData[xLeft + yAbs] + lifeData[xRight + yAbs]
				+ lifeData[xLeft + yAbsDown] + lifeData[x + yAbsDown] + lifeData[xRight + yAbsDown];

			resultLifeData[x + yAbs] = aliveCells == 3 || (aliveCells == 2 && lifeData[x + yAbs]) ? 1 : 0;
		}
	}

	
	__global__ void InitWorld(unsigned char* lifeData, int worldWidth, int worldHeight, unsigned char* resultLifeData)
	{
		int worldSize = worldWidth * worldHeight;
		int index = blockIdx.x * blockDim.x + threadIdx.x;
		int stride = blockDim.x * gridDim.x;

		for (int i = index; i < worldSize; i += stride)
		{
			lifeData[i] = 0;
			resultLifeData[i] = 0;
		}
	}

	__global__ void InitWithImage(int imageWidth, int imageHeight, const unsigned char* imageBuffer, int worldWidth, int worldHeight,
		unsigned char* lifeData)
	{
		int imageSize = imageWidth * imageHeight;
		int index = blockIdx.x * blockDim.x + threadIdx.x;
		int stride = blockDim.x * gridDim.x;

		for (int i = index; i < imageSize; i += stride)
		{
			int xImage = i % imageWidth;
			int yImage = (i - xImage) / imageWidth;

			lifeData[yImage * worldWidth + xImage] = imageBuffer[xImage + yImage*imageWidth] == 255 ? 1 : 0;
		}
	}

	__global__ void WriteImage(int imageWidth, int imageHeight, unsigned char* imageBuffer, int worldWidth, int worldHeight,
		unsigned char* lifeData)
	{
		int imageSize = imageWidth * imageHeight;
		int index = blockIdx.x * blockDim.x + threadIdx.x;
		int stride = blockDim.x * gridDim.x;

		for (int i = index; i < imageSize; i += stride)
		{
			int xImage = i % imageWidth;
			int yImage = (i - xImage) / imageWidth;
	
			imageBuffer[xImage + yImage * imageWidth] = lifeData[yImage * worldWidth + xImage] == 1 ? 255 : 0;
		}
	}

	__global__ void WriteRGBImage(unsigned char* lifeData, int worldWidth, int worldHeight, Pixel* image)
	{
		int imageSize = worldWidth * worldHeight;
		int index = blockIdx.x * blockDim.x + threadIdx.x;
		int stride = blockDim.x * gridDim.x;

		for (int i = index; i < imageSize; i += stride)
		{
			int x = i % worldWidth;
			int yAbs = i - x;

			if (lifeData[x + yAbs] == 1)
			{
				image[x + yAbs].r = 255;
				image[x + yAbs].b = 255;
				image[x + yAbs].g = 255;

				int xLeft = (x + worldWidth - 1) % worldWidth;
				int xRight = (x + 1) % worldWidth;
				int xLeft2 = (x + worldWidth - 2) % worldWidth;
				int xRight2 = (x + 2) % worldWidth;

				int yAbsUp = (yAbs + imageSize - worldHeight) % imageSize;
				int yAbsDown = (yAbs + worldHeight) % imageSize;
				int yAbsUp2 = (yAbs + imageSize - 2 * worldHeight) % imageSize;
				int yAbsDown2 = (yAbs + 2 * worldHeight) % imageSize;

				if (lifeData[x + yAbs] == 1 &&
					lifeData[xLeft + yAbs] == 1 &&
					lifeData[xRight + yAbs] == 1 &&
					lifeData[xLeft + yAbsUp] == 0 &&
					lifeData[x + yAbsUp] == 0 &&
					lifeData[xRight + yAbsUp] == 0 &&
					lifeData[xLeft + yAbsDown] == 0 &&
					lifeData[x + yAbsDown] == 0 &&
					lifeData[xRight + yAbsDown] == 0 &&
					lifeData[xLeft2 + yAbs] == 0 &&
					lifeData[xLeft2 + yAbsUp] == 0 &&
					lifeData[xLeft2 + yAbsDown] == 0 &&
					lifeData[xRight2 + yAbs] == 0 &&
					lifeData[xRight2 + yAbsUp] == 0 &&
					lifeData[xRight2 + yAbsDown] == 0)
				{
					image[x + yAbs].b = 0;
					image[xLeft + yAbs].b = 0;
					image[xRight + yAbs].b = 0;
					image[x + yAbs].g = 0;
					image[xLeft + yAbs].g = 0;
					image[xRight + yAbs].g = 0;

				}
				else if (lifeData[x + yAbs] == 1 &&
					lifeData[x + yAbsUp] == 1 &&
					lifeData[x + yAbsDown] == 1 &&
					lifeData[xLeft + yAbs] == 0 &&
					lifeData[xLeft + yAbsUp] == 0 &&
					lifeData[xLeft + yAbsDown] == 0 &&
					lifeData[xRight + yAbs] == 0 &&
					lifeData[xRight + yAbsUp] == 0 &&
					lifeData[xRight + yAbsDown] == 0 &&
					lifeData[xLeft + yAbsUp2] == 0 &&
					lifeData[xLeft + yAbsDown2] == 0 &&
					lifeData[xRight + yAbsUp2] == 0 &&
					lifeData[xRight + yAbsDown2] == 0)
				{
					image[x + yAbs].g = 0;
					image[x + yAbsUp].g = 0;
					image[x + yAbsDown].g = 0;
					image[x + yAbs].b = 0;
					image[x + yAbsUp].b = 0;
					image[x + yAbsDown].b = 0;
				}
			}
			else {
				image[x + yAbs].r = 0;
				image[x + yAbs].b = 0;
				image[x + yAbs].g = 0;
			}
		}
	}


	void game_of_life_cuda(unsigned char*& lifeData, const int worldWidth, const int worldHeight, unsigned char*& resultLifeData, int generations)
	{
		int blockSize = 256;
		int numBlocks = (blockSize + worldWidth * worldHeight - 1) / blockSize;
		for (int i = 0; i < generations; i++)
		{
			GameOfLife <<<numBlocks, blockSize>>> (lifeData, worldWidth, worldHeight, resultLifeData);
			cudaDeviceSynchronize();
			std::swap(lifeData, resultLifeData);
		}
	}

	void init_world_cuda(unsigned char*& lifeData, int worldWidth, int worldHeight, unsigned char*& resultLifeData)
	{
		if (cudaMalloc(&lifeData, (size_t)worldWidth * worldHeight * sizeof(unsigned char)) ||
			cudaMalloc(&resultLifeData, (size_t)worldWidth * worldHeight * sizeof(unsigned char)))
		{
			printf("Allocation failed!");
		}

		int blockSize = 256;
		int numBlocks = (blockSize + worldWidth * worldHeight - 1) / blockSize;
		InitWorld <<<numBlocks, blockSize >>> (lifeData, worldWidth, worldHeight, resultLifeData);
		cudaDeviceSynchronize();
	}

	void write_image_cuda(int imageWidth, int imageHeight, unsigned char* imageBuffer, int worldWidth, int worldHeight, unsigned char* lifeData)
	{
		int blockSize = 256;
		int numBlocks = (blockSize + imageWidth * imageHeight - 1) / blockSize;
		WriteImage <<<numBlocks, blockSize >>> (imageWidth, imageHeight, imageBuffer, worldWidth, worldHeight, lifeData);
		cudaDeviceSynchronize();
	}

	void oscilator_detection(unsigned char*& lifeData, int worldWidth, int worldHeight, unsigned char*& resultLifeData, Pixel* image)
	{
		int blockSize = 256;
		int numBlocks = (blockSize + worldWidth * worldHeight - 1) / blockSize;
		GameOfLife <<<numBlocks, blockSize >>> (lifeData, worldWidth, worldHeight, resultLifeData);
		cudaDeviceSynchronize();
		std::swap(lifeData, resultLifeData);
		WriteRGBImage <<<numBlocks, blockSize >>> (lifeData, worldWidth, worldHeight, image);
		cudaDeviceSynchronize();
	}

	void init_world_with_image_cuda(int imageWidth, int imageHeight, unsigned char* imageBuffer, int worldWidth, int worldHeight,
		unsigned char* lifeData)
	{
		int blockSize = 256;
		int numBlocks = (blockSize + imageWidth * imageHeight - 1) / blockSize;
		InitWithImage <<<numBlocks, blockSize >>> (imageWidth, imageHeight, imageBuffer, worldWidth, worldHeight, lifeData);
		cudaDeviceSynchronize();
	}
	
	__device__ bool isHorizontalBlinker(unsigned char* lifeData, int x, int yAbs, int xLeft, int xRight, int yAbsDown, int yAbsUp)
	{
		return lifeData[x + yAbs] == 1 &&
			lifeData[xLeft + yAbs] == 1 &&
			lifeData[xRight + yAbs] == 1 &&
			lifeData[xLeft + yAbsUp] == 0 &&
			lifeData[x + yAbsUp] == 0 &&
			lifeData[xRight + yAbsUp] == 0 &&
			lifeData[xLeft + yAbsDown] == 0 &&
			lifeData[x + yAbsDown] == 0 &&
			lifeData[xRight + yAbsDown] == 0;
	}

	__device__ bool isVerticalBlinker(unsigned char* lifeData, int x, int yAbs, int xLeft, int xRight, int yAbsDown, int yAbsUp)
	{
		return false;
	}
}
