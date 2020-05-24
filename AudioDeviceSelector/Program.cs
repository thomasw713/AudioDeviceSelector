using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using AudioSwitcher.AudioApi.CoreAudio;

namespace AudioDeviceSelector
{
    public static class Program
    {
        public static CoreAudioDevice GetDeviceByName(this CoreAudioController controller, string idOrName)
        {
            return controller.GetDevices().FirstOrDefault(d => d.Id.ToString() == idOrName || d.InterfaceName == idOrName || d.Name == idOrName);
        }

        public static bool SetDeviceByName(this CoreAudioController controller, string idOrName)
        {
            Console.WriteLine("Setting device...");
            var device = controller.GetDeviceByName(idOrName);
            
            if (device == null)
            {
                Console.WriteLine($"Failed to set device, could not find device with Id or Name {idOrName}");
                return false;
            }
            
            if (!controller.SetDefaultDevice(device))
            {
                Console.WriteLine($"Failed to set device");
                return false;
            }

            Console.WriteLine($"{device.DeviceType} device set to {device.InterfaceName}|{device.Name}");
            return true;
        }

        public static bool SetCommDeviceByName(this CoreAudioController controller, string idOrName)
        {
            Console.WriteLine("Setting communication device...");
            var device = controller.GetDeviceByName(idOrName);

            if (device == null)
            {
                Console.WriteLine($"Failed to set communication device, could not find device with Id or Name {idOrName}");
                return false;
            }
            
            if (!controller.SetDefaultCommunicationsDevice(device))
            {
                Console.WriteLine($"Failed to set communication device");
                return false;
            }

            Console.WriteLine($"{device.DeviceType} communication device set to {device.InterfaceName}|{device.Name}");
            return true;
            
        }


        public static void Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option("--list", "List all audio devices"),
                new Option<string>("--set","Set device"),
                new Option<string>("--set-comm","Set communication device")
            };

            rootCommand.Description = "AudioDeviceSelector";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<string, string>(
                (set, setComm) =>
                {
                    var controller = new CoreAudioController();
                    var devices = controller.GetDevices();

                    if (args.Count() < 1)
                    {
                        Console.WriteLine($"{Assembly.GetExecutingAssembly().GetName().Name}.exe --help");
                    }

                    if (args.Any(x => x.ToString().ToLower() == "--list"))
                    {
                        devices
                            .OrderBy(d => d.DeviceType)
                            .ToList()
                            .ForEach(d => Console.WriteLine($"DeviceType: {d.DeviceType}, Id: {d.Id}, InterfaceName: {d.InterfaceName}, Name: {d.Name}"));
                    }

                    _ = set != null && controller.SetDeviceByName(set);
                    _ = setComm != null && controller.SetCommDeviceByName(setComm);

                });

            rootCommand.Invoke(args);

        }
    }
}
