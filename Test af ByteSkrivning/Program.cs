

using System.Diagnostics;
using System.Runtime.InteropServices;
using Test_af_ByteSkrivning;



int numStructs = 50000000; // Standardværdi

if (args.Length > 0 && int.TryParse(args[0], out int inputNumStructs))
{
    numStructs = inputNumStructs; // Brug argumentet, hvis det er gyldigt
}

MyTestStruct[] myStructArray = new MyTestStruct[numStructs];

// Udfyld myStructArray med data
for (int i = 0; i < myStructArray.Length; i++)
{
    myStructArray[i].id = i + 1;
    unsafe
    {
        for (int j = 0; j < 8; j++)
        {
            myStructArray[i].temperature[j] = (i + 1) * 10.0f + j;
        }
    }
}

string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
string filename = Path.Combine(desktopPath, "myfile.bin");

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
// Skriv til fil og få byte bufferen
byte[] writtenBuffer = await WriteStructArrayToFileAsync(myStructArray, filename);
stopwatch.Stop();
Console.WriteLine($"Time taken to write {myStructArray.Length} structs to file: {stopwatch.ElapsedMilliseconds} ms");

stopwatch.Restart();
// Læs fra fil
MyTestStruct[] readTest = ReadStructsFromFile(filename);
stopwatch.Stop();
Console.WriteLine($"Time taken to read {readTest.Length} structs from file: {stopwatch.ElapsedMilliseconds} ms");

//// Udskrivning af bufferen, der blev skrevet
//Console.WriteLine("Written Data in Byte Form:");
//PrintStructArray(myStructArray, writtenBuffer);

//// Udskrivning af læst data
//Console.WriteLine("Read Struct Data:");
//PrintStructArray(readTest);


static void PrintStructArray(MyTestStruct[] structs, byte[] buffer = null)
{
    for (int i = 0; i < structs.Length; i++)
    {
        Console.WriteLine($"ID: {structs[i].id}");
        unsafe
        {
            for (int j = 0; j < 8; j++)
            {
                Console.WriteLine($"Temperature[{j}]: {structs[i].temperature[j]:F2}");
            }
        }
        if (buffer != null)
        {
            Console.Write("Bytes: ");
            int structSize = Marshal.SizeOf<MyTestStruct>();
            for (int k = i * structSize; k < (i + 1) * structSize; k++)
            {
                Console.Write($"{buffer[k]:X2} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("----------------------------");
    }
}




static async Task<byte[]> WriteStructArrayToFileAsync(MyTestStruct[] arrayOfstructs, string filename)
{
    int structSize = Marshal.SizeOf<MyTestStruct>();
    byte[] buffer = new byte[structSize * arrayOfstructs.Length];
    IntPtr ptr = Marshal.AllocHGlobal(structSize);

    try
    {
        for (long i = 0; i < arrayOfstructs.Length; i++)
        {
            Marshal.StructureToPtr(arrayOfstructs[i], ptr, false);
            Marshal.Copy(ptr, buffer, (int)(i * structSize), structSize);
        }

        using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
        {
            await fileStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
    finally
    {
        Marshal.FreeHGlobal(ptr);
    }

    return buffer;
}





static unsafe MyTestStruct[] ReadStructsFromFile(string filename)
{
    int structSize = Marshal.SizeOf<MyTestStruct>();
    int numStructs = (int)(new FileInfo(filename).Length / structSize);
    MyTestStruct[] structs = new MyTestStruct[numStructs];

    using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
        byte[] buffer = new byte[structSize * numStructs];
        if (fileStream.Read(buffer, 0, buffer.Length) != buffer.Length)
        {
            throw new InvalidOperationException("Unable to read the struct from file.");
        }

        fixed (byte* pBuffer = buffer)
        {
            for (int i = 0; i < numStructs; i++)
            {
                structs[i] = *(MyTestStruct*)(pBuffer + i * structSize);
            }
        }
    }

    return structs;
}


