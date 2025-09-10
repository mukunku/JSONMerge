# JSONMerge
Windows utility to figure out the common schema for a collection of JSON objects.

# Summary
This utility can process a `.json` file containing an array of objects by analyzing all objects in the array and generate a "master" object containing all possible fields.

<img width="325" height="334" alt="image" src="https://github.com/user-attachments/assets/3e70c6a0-6f63-4eb7-8337-2f6c9c6c6738" />

# Download
Releases can be found here: https://github.com/mukunku/JSONMerge/releases

Test file: [JSONMerge_ExampleSourceFile.json](https://github.com/mukunku/JSONMerge/blob/main/data/JSONMerge_ExampleSourceFile.json)

# Technical Details
The project was written in C# using Visual Studio Community 2022 and .NET Framework 4.8

# Limitations
The utility does not support heterogeneous arrays within the JSON structure. Arrays of simple types and arrays of objects are supported. Arrays of arrays should also be supported but haven't been tested.
