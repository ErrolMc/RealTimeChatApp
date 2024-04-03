using System.Threading.Tasks;
using System;
using Zenject;

public interface IBroadcastService
{
    public Task BroadcastMessage(string message);
    public event Action<string> OnMessageReceived;
}
