using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Godot;

namespace Petrichor.scripts;

public static class ThreadedLoader
{
    private static readonly Dictionary<string, Thread> ActiveThreads = new();
    private static readonly Dictionary<string, Resource> Requests = new();
    
    public static void LoadFileAsync<T>(string path)
    {
        if (ActiveThreads.ContainsKey(path))
            return;

        Thread thread = typeof(T) != typeof(Texture2D) ? 
            new Thread(() => Requests.Add(path, ResourceLoader.Load<Resource>(path))) 
            : 
            new Thread(() => Requests.Add(path, _LoadTexture(path)));
        
        thread.Start();
        ActiveThreads[path] = thread;
    }

    private static Texture2D _LoadTexture(string path)
    {
        Image image = new Image();
        image.LoadPngFromBuffer(File.ReadAllBytes(path));
        ImageTexture imageTex = ImageTexture.CreateFromImage(image);
        return imageTex;
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