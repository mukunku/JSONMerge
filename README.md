# JSONMerge
Windows utility to analyze &amp; merge multiple JSON objects to generate one final "master" JSON object with all possible fields

![Main UI](https://github.com/mukunku/JSONMerge/blob/main/wiki_images/main_screenshot.png)

# Download
If you'd like to use a pre-compiled EXE instead of compiling the project yourself, please see the release folder: https://github.com/mukunku/JSONMerge/tree/main/release

If you'd like to try out an example please download the following JSON file and merge it with the utility: https://github.com/mukunku/JSONMerge/blob/main/release/JSONMerge_ExampleSourceFile.json

# Technical Details
The project was written in C# using Visual Studio 2013 and .NET 4.5

If you'd like to build the project yourself you only need to download the src folder.

# Summary
This is a quick and dirty utility that I created in order to easily figure out the common schema for multiple structured JSON objects. 

The problem that made me create this was that I would receive a collection of JSON data where not all the objects had every possible field (which makes sense from a record size perspective). Or some fields were null for some objects while not for others. So it was difficult to figure out what possible fields and field types I should expect while processing such data.

This utility goes through the entire collection of JSON objects and generates one master record that has every possible field. The utility also tries to make sure none of the fields are null in the master object but if a value has never been provided for that field there is no way for us to figure out what type of field it is.

# Limitations
The utility does not support heterogeneous arrays within the JSON structure. Arrays of simple types and arrays of objects are supported. Arrays of arrays should also be supported but haven't been tested.
