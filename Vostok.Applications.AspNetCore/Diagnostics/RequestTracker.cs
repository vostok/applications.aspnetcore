using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Commons.Collections;
using Vostok.Commons.Helpers.Disposable;

namespace Vostok.Applications.AspNetCore.Diagnostics
{
    internal class RequestTracker : IRequestTracker
    {
        private readonly ConcurrentDictionary<RequestTrackerItem, byte> items;

        public RequestTracker()
            => items = new ConcurrentDictionary<RequestTrackerItem, byte>(64, 256, ByReferenceEqualityComparer<RequestTrackerItem>.Instance);

        public IEnumerable<RequestTrackerItem> CurrentItems => items.Select(pair => pair.Key);

        public IDisposable Track(HttpContext context, IRequestInfo info)
        {
            var item = new RequestTrackerItem(context.Request.Path, info);

            items[item] = 0;

            return new ActionDisposable(() => items.TryRemove(item, out _));
        }
    }
}