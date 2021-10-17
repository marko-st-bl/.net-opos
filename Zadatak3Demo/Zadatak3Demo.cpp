// Zadatak3Demo.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#include <iostream>
#include "GameOfLifeLibrary.h"
#include <string>
using namespace std;

int main()
{
    GameOfLifeLibrary::GameOfLife gof;
    int option;

    int width;
    int height;
    int iteration;
    string path;

    while (1)
    {
        std::cout << "\n------------------------------\n";
        std::cout << "M E N U!";
        std::cout << "\n------------------------------\n\n";
        std::cout << "1) Create Game of Life World\n";
        std::cout << "2) Initialize Game of Life World with image\n";
        std::cout << "3) Skip to iteration\n";
        std::cout << "4) Save as image\n";
        std::cout << "5) Next generation with oscilator detection\n";
        std::cout << "0) QUIT\n\n";

        std::cout << "Enter option.\n";
        std::cin >> option;

        switch (option)
        {
        case 1:
            std::cout << "Enter Game of Life's World Dimensions.\n";
            std::cout << "Width: ";
            std::cin >> width;
            std::cout << "\nHeight: ";
            std::cin >> height;
            gof.init_world(width, height);
            break;
        case 2:
            std::cout << "Enter initialization photo path.\n";
            std::cin >> path;
            gof.init_with_image(path);
            break;
        case 3:
            std::cout << "Enter number of iteration to skip on: ";
            std::cin >> iteration;
            gof.game_of_life(iteration);
            break;
        case 4:
            std::cout << "Enter name of image.\n";
            std::cin >> path;
            std::cout << "Enter Width of the image.\n";
            std::cin >> width;
            std::cout << "Enter Height of the image.\n";
            std::cin >> height;
            gof.write_image(path, width, height);
            break;
        case 5:
            gof.next_generation_with_oscilator_detection();
            break;
        case 0:
            gof.free_memory();
            exit(0);
            break;
        default:
            std::cout << "Unknown option! Try again!";
        }
    }
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
