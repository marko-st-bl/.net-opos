//#include "cuda_runtime.h"
//#include "device_launch_parameters.h"
//
//#include <stdio.h>
//#include <string>
//
//
//#include "GameOfLife.cuh"
//
//
//static void readImage(const char* filename, unsigned char*& array, int& width, int& height)
//{
//	FILE* fp = fopen(filename, "rb");
//	if (!fscanf(fp, "P5\n%d %d\n255\n", &width, &height))
//	{
//		throw "Error opening file";
//	}
//	//unsigned char* image = new unsigned char[(size_t)width * (size_t)height];
//	unsigned char* image = new unsigned char[900];
//	fread(image, sizeof(unsigned char), (size_t)width * (size_t)height, fp);
//	fclose(fp);
//	array = image;
//}
//
//static void writeImage(const char* filename, const unsigned char* array, int width, int height)
//{
//	FILE* fp = fopen(filename, "wb");
//	fprintf(fp, "P5\n%d %d\n255\n", width, height);
//	fwrite(array, sizeof(unsigned char), (size_t)width * (size_t)height, fp);
//	fclose(fp);
//}
//
//
//int main()
//{
//	int width = -1;
//	int height = -1;
//	unsigned char* lifeData;
//	unsigned char* resultLifeData;
//	unsigned char* buffer = nullptr;
//	readImage("slika.pgm", buffer, width, height);
//
//	cudaMallocManaged(&lifeData, 900 * sizeof(unsigned char));
//	cudaMallocManaged(&resultLifeData, 900 * sizeof(unsigned char));
//
//	for (int i = 0; i < 30; i++)
//		for (int j = 0; j < 30; j++)
//			lifeData[i * 30 + j] = buffer[i * 30 + j] == 255 ? 1 : 0;
//
//	for (int i = 0; i < 100; i++)
//	{
//		game_of_life(lifeData, 30, resultLifeData);
//		const std::string outputFile = std::string("image") + std::to_string(i+1) + std::string(".pgm");
//		for (int i = 0; i < 30; i++)
//			for (int j = 0; j < 30; j++)
//			{
//				buffer[i * 30 + j] = resultLifeData[i * 30 + j] == 1 ? 255 : 0;
//			}
//		writeImage(outputFile.c_str(), buffer, 30, 30);
//
//		lifeData = resultLifeData;
//	}
//
//	// Free memory
//	cudaFree(lifeData);
//	cudaFree(resultLifeData);
//}