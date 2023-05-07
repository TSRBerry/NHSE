using NHSE.Core;

namespace NHSE.Cli;

public static class Program
{
    private static HorizonSave Save;

    public static int Main(string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("Creating a backup of this save file...");
            var saveFilePath = args[0];

            if (File.Exists(saveFilePath))
            {
                saveFilePath = Path.GetDirectoryName(saveFilePath);
            }

            if (!Directory.Exists(saveFilePath))
            {
                Console.WriteLine("Couldn't find save.");
                return 1;
            }


            var backupSavePath = Path.GetFullPath(Path.Combine(saveFilePath, "..", "..", "ACNH-backup"));
            Directory.CreateDirectory(backupSavePath);

            foreach (var file in Directory.EnumerateFiles(saveFilePath, "*", SearchOption.AllDirectories))
            {
                var newPath = file.Replace(saveFilePath, backupSavePath);

                if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
                }

                if (File.Exists(newPath))
                {
                    Console.WriteLine($"Couldn't create backup. Another backup seems to already exist at this path: {backupSavePath}");
                }

                File.Copy(file, newPath);
            }

            Console.WriteLine($"Backup created: {backupSavePath}");

            Console.WriteLine("Reading save...");

            Save = new HorizonSave(saveFilePath);

            Console.WriteLine("Modifying player ids...");

            foreach (var player in Save.Players)
            {
                var orig = player.Personal.GetPlayerIdentity();

                uint newId = (uint)Random.Shared.Next();
                Console.WriteLine($"{player.Personal.PlayerID} -> {newId}");
                player.Personal.PlayerID = newId;

                var updated = player.Personal.GetPlayerIdentity();

                Save.ChangeIdentity(orig, updated);
            }

            Console.WriteLine("Writing new save...");

            Save.Save((uint)DateTime.Now.Ticks);

            Console.WriteLine("Done.");

            return 0;
        }

        Console.WriteLine("Missing save path.");

        return 1;
    }
}