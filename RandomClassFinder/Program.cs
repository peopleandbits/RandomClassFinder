using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RandomClassFinder
{
    /// <summary>
    /// Finds a random class from dlls.
    ///
    /// No arguments: 
    ///     Looks from all dlls in the current folder.
    ///     
    /// One argument eg. ""ADJUST.Models.dll, ADJUST.ViewModels.dll": 
    ///     Looks from these two dlls. Referenced dlls must be present!
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string[] whiteList = (args != null && args.Length == 1) ? args[0].Split(',').Select(c => c.Trim()).ToArray() : null;
            var processor = new Processor();
            string randomClass = whiteList == null ? processor.ProcessAll() : processor.ProcessWhiteList(whiteList);
            Console.WriteLine(randomClass);
        }
    }

    /// <summary>
    /// Orchestrates the process of finding one random class from all the dll's in the current folder.
    /// </summary>
    public class Processor
    {
        public Processor()
        {
            _Loader = new Loader();
            _AsmFinder = new AsmFinder();
            _ClsFinder = new ClsFinder();
        }

        public string ProcessAll()
        {
            var assemblies = _Loader.LoadAll();
            return Find(assemblies);
        }

        public string ProcessWhiteList(string[] whiteList)
        {
            var assemblies = _Loader.LoadWhiteList(whiteList);
            return Find(assemblies);
        }

        string Find(Assembly[] assemblies)
        {
            try
            {
                Assembly asm = _AsmFinder.FindRandomAssembly(assemblies);
                return _ClsFinder.FindRandomClass(asm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException}");
                Environment.Exit(-1);
            }
            return null;
        }

        Loader _Loader;
        AsmFinder _AsmFinder;
        ClsFinder _ClsFinder;
    }

    /// <summary>
    /// Class loads all dll's in the current folder.
    /// </summary>
    public class Loader
    {
        public Assembly[] LoadAll()
        {
            var dllFileNames = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll").ToArray();
            return LoadWhiteList(dllFileNames);
        }

        public Assembly[] LoadWhiteList(string[] dllFileNames)
        {
            try
            {
                return Load(dllFileNames);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not load assembly.", ex);
            }
        }

        Assembly[] Load(string[] dllFileNames) => dllFileNames.Select(c => Assembly.LoadFrom(c)).ToArray();
    }

    /// <summary>
    /// Class finds a random assembly from many.
    /// </summary>
    public class AsmFinder
    {
        public Assembly FindRandomAssembly(Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                throw new InvalidOperationException("No assemblies.");

            return assemblies[RandomHelper.Rnd.Next(assemblies.Length)];
        }
    }

    /// <summary>
    /// Class finds a random class from an assembly.
    /// </summary>
    public class ClsFinder
    {
        public string FindRandomClass(Assembly asm)
        {
            var classes = asm.GetTypes().Where(c => c.IsClass).ToArray();
            return classes[RandomHelper.Rnd.Next(classes.Length)].FullName;
        }
    }

    public static class RandomHelper
    {
        static RandomHelper() { Rnd = new Random(); }
        public static Random Rnd { get; set; }
    }
}