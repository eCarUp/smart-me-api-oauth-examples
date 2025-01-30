using DeviceCodeSample;

Console.WriteLine("smart-me OAuth Sample with device code");


var deviceCode = new AuthWithDeviceCode();

if (await deviceCode.TryLoginAndCallApiAsync())
{
    Console.WriteLine("Successfully logged in!");
}
else
{
    Console.WriteLine("Failed to log in.");
}
