using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

// Based on https://www.hackster.io/brane/002-how-to-setup-devices-and-register-into-azure-iot-hub-c905c9
namespace AzureIoTRegistryManager
{
    class Manager
    {
        // RegistryManager object which is going to do most of the work for our application
        static RegistryManager registryManager;
        // This here is the Connection String that you copied from the IoT Hub Shared Access Policies > iothubowner > Shared access keys; remember??
        static string connectionString = "SET_KEY_HERE";
        // Keeps track of the registry access status
        static string registryAccessStatus = "";

        static void Main()
        {
            try
            {
                // Let's try and create a Registry Manager using our connection string, shall we?
                registryManager = RegistryManager.CreateFromConnectionString(connectionString);
                registryAccessStatus = "Successfuly connected to the IoT Hub registry"; // Yay!
            }
            catch (Exception ex)
            {
                Console.WriteLine("Registry access failed!  {0}", ex.Message);  // Bummer!!
            }
            // Check if RegistryManager was created successfully
            if (registryManager != null)
            {
                Console.WriteLine("*****************************************************");
                Console.WriteLine("===== Welcome to the Azure IoT Registry Manager =====");
                Console.WriteLine();
                Console.WriteLine("++ {0} ++", registryAccessStatus);

                var menuSelection = 0;
                while (menuSelection != 4)  // Loop to keep you going...
                {
                    Console.WriteLine();
                    Console.WriteLine("  1) Add device into registry");
                    Console.WriteLine("  2) Remove device from the registry");
                    Console.WriteLine("  3) List devices");
                    Console.WriteLine("  4) Close this application");
                    Console.WriteLine("------------------------------------");
                    Console.Write("Enter your selection: ");
                    var selection = Console.ReadLine();
                    menuSelection = int.Parse(selection ?? "0");
                    Console.WriteLine();
                    switch (menuSelection)
                    {
                        case 1:
                            Console.Write("Enter device name that you want to register: ");
                            var deviceName = Console.ReadLine();
                            Console.WriteLine();
                            if (!string.IsNullOrEmpty(deviceName) && !deviceName.Contains(" "))
                            {
                                // Calling method that actually adds the device into the registry
                                AddDevice(deviceName).Wait();
                            }
                            else
                            {
                                Console.WriteLine("---");
                                Console.WriteLine("Enter valid name!");
                                Console.WriteLine("---");
                            }
                            break;
                        case 2:
                            Console.Write("Enter name of the device to be removed: ");
                            var deviceRemoveName = Console.ReadLine();
                            Console.WriteLine();
                            if (!string.IsNullOrEmpty(deviceRemoveName) && !deviceRemoveName.Contains(" "))
                            {
                                // Calling method that actually removes the device from the registry
                                RemoveDevice(deviceRemoveName).Wait();
                            }
                            else
                            {
                                Console.WriteLine("---");
                                Console.WriteLine("Enter valid name!");
                                Console.WriteLine("---");
                            }
                            break;
                        case 3:
                            Console.WriteLine();
                            var devices = ListDevices(100).Result;
                            var i = 1;
                            foreach (var device in devices)
                            {
                                Console.WriteLine($"{i++}: Id: {device.Id}, status: ${device.Status}");
                            }
                            break;
                        case 4:
                            // Breaks out of the loop
                            break;
                        default:
                            Console.WriteLine("---");
                            Console.WriteLine("Choose valid entry!");
                            Console.WriteLine("---");
                            break;
                    }
                }
                // Closes the RegistryManager access right before exiting the application
                registryManager.CloseAsync().Wait();
            }
        }

        // Method used to add device into the Registry, takes in a string as a parameter
        private static async Task AddDevice(string deviceId)
        {
            // A Device object
            Device device;
            try
            {
                // Lets try and create a Device into the Device Registry
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
                if (device != null)
                {
                    Console.WriteLine("Device: {0} added successfully!", deviceId); // Hooray!
                }
            }
            catch (DeviceAlreadyExistsException)  // What?
            {
                Console.WriteLine("---");
                Console.WriteLine("This device has already been registered...");// When did I do that??
                Console.WriteLine("---");
                device = await registryManager.GetDeviceAsync(deviceId);
            }

            Console.WriteLine();
            if (device == null)
            {
                Console.WriteLine($"Could not get already existing device with id {deviceId}");
            }
            else
            {
                Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);  // Now you're talking!      
            }
            Console.WriteLine();
        }

        // Method used to remove a device from the Device Registry, takes a string as a parameter
        private static async Task RemoveDevice(string deviceId)
        {
            try
            {
                // Lets try and get rid of the Device from our registry, using the device id.
                await registryManager.RemoveDeviceAsync(deviceId);
                Console.WriteLine("Device: {0} removed successfully!", deviceId);  // Yup!
            }
            catch (DeviceNotFoundException)
            {
                Console.WriteLine("---");
                Console.WriteLine("This device has not been registered into this registry!");  // Are you sure??
                Console.WriteLine("---");
            }
        }

        /// <summary>
        /// Lists the devices.
        /// </summary>
        /// <param name="maxCount">The maximum count.</param>
        /// <returns>List of devices</returns>
        private static async Task<IEnumerable<Device>> ListDevices(int maxCount)
        {
            // Lets try and get rid of the Device from our registry, using the device id.
            var task = await registryManager.GetDevicesAsync(maxCount);
            Console.WriteLine("Devices listed successfully!");  // Yup!
            return task;

        }
    }
}