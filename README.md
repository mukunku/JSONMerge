# JSONMerge
Windows utility to analyze &amp; merge multiple JSON objects to generate one final JSON object with all possible fields

# Download
If you'd like to use a pre-compiled EXE instead of compiling the project yourself, please see the release folder: https://github.com/mukunku/ParquetViewer/tree/master/release

If you'd like to try out an example please download the following JSON file and merge it with the utility:

# Technical Details
The project was written in C# using Visual Studio 2013 and .NET 4.5

If you'd like to build the project yourself you only need to download the src folder.

# Summary
This is a quick and dirty utility that I created in order to easily figure out the Schema for multiple structured JSON objects. 
The issue that made me create this was becase I would receive an collection of JSON data where not all the objects had every field. Or some fields were null for some objects while not for others. So it was difficult to figure out what possible fields and types i should expect.
This utility goes through the entire collection of JSON objects and generates one master record that has every possible field. The utility also tries to make sure none of the fields are null in the master object but if a value has never been provided for that field there is no way for us to figure out what type of field it is.

# Limitations
The utility does not support heterogeneous arrays within the JSON structure. Arrays of simple types and arrays of objects are supported. Arrays of arrays should also be supported but haven't been tested.
