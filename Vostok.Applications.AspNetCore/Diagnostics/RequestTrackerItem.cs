using Vostok.Applications.AspNetCore.Models;

namespace Vostok.Applications.AspNetCore.Diagnostics
{
    internal class RequestTrackerItem
    {
        public RequestTrackerItem(string path, IRequestInfo info)
        {
            Path = path;
            Info = info;
        }

        public string Path { get; }

        public IRequestInfo Info { get; }
    }
}