using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

namespace Petrichor.scripts;

public static class ThreadedLoader
{
    private static readonly Dictionary<string, Thread> ActiveThreads = new();
    private static readonly Dictionary<string, Resource> Requests = new();
    
    public static void LoadFileAsync(string path)
    {
        if (ActiveThreads.ContainsKey(path))
            return;
        
        Thread thread = new Thread(() => Requests.Add(path, ResourceLoader.Load<Resource>(path)));
        thread.Start();
        ActiveThreads[path] = thread;
    }
    
    public static T GetFileAsync<T>(string path) where T : Resource
    {
        if (ActiveThreads.TryGetValue(path, out Thread thread))
        {
            if (thread.IsAlive)
                thread.Join();

            Resource res = Requests[path];
            ActiveThreads.Remove(path);
            Requests.Remove(path);
            return (T)res;
        }
        
        Debug.WriteLine(Utils.WARNING_STR + "No load request was started for the file " + path + ", loading directly.");
        return ResourceLoader.Load<T>(path);
    }
}