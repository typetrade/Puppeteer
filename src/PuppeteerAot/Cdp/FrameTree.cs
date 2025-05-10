using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Puppeteer.Helpers;

namespace Puppeteer.Cdp
{
    /// <summary>
    /// Represents a tree of frames.
    /// </summary>
    public class FrameTree
    {
        private readonly AsyncDictionaryHelper<string, CdpFrame> frames = new("Frame {0} not found");
        private readonly ConcurrentDictionary<string, string> _parentIds = new();
        private readonly ConcurrentDictionary<string, List<string>> _childIds = new();
        private readonly ConcurrentDictionary<string, List<TaskCompletionSource<CdpFrame>>> _waitRequests = new();

        /// <summary>
        /// Gets or sets the main frame of the page. The main frame is the first frame in the page and <see langword="is"/>.
        /// </summary>
        public CdpFrame? MainFrame { get; set; }

        /// <summary>
        /// Gets the main frame of the page. The main frame is the first frame in the page and <see langword="is"/>.
        /// </summary>
        public CdpFrame[] Frames => this.frames.Values.ToArray();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameId"></param>
        /// <returns></returns>
        public Task<CdpFrame> GetFrameAsync(string frameId) => this.frames.GetItemAsync(frameId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameId"></param>
        /// <returns></returns>
        public Task<CdpFrame> TryGetFrameAsync(string frameId) => this.frames.TryGetItemAsync(frameId);

        /// <summary>
        /// Gets the frame by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CdpFrame GetById(string id)
        {
            this.frames.TryGetValue(id, out var result);
            return result;
        }

        public Task<CdpFrame> WaitForFrameAsync(string frameId)
        {
            var frame = GetById(frameId);
            if (frame != null)
            {
                return Task.FromResult(frame);
            }

            var deferred = new TaskCompletionSource<CdpFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callbacks = _waitRequests.GetOrAdd(frameId, static _ => new());
            callbacks.Add(deferred);

            return deferred.Task;
        }

        /// <summary>
        /// Adds a frame to the frame tree.
        /// </summary>
        /// <param name="frame"></param>
        public void AddFrame(CdpFrame frame)
        {
            this.frames.AddItem(frame.Id, frame);
            if (frame.ParentId != null)
            {
                this._parentIds.TryAdd(frame.Id, frame.ParentId);

                var childIds = this._childIds.GetOrAdd(frame.ParentId, static _ => new());
                childIds.Add(frame.Id);
            }
            else
            {
                this.MainFrame = frame;
            }

            this._waitRequests.TryGetValue(frame.Id, out var requests);

            if (requests != null)
            {
                foreach (var request in requests)
                {
                    request.TrySetResult(frame);
                }
            }
        }

        /// <summary>
        /// Removes a frame from the frame tree.
        /// </summary>
        /// <param name="frame"></param>
        public void RemoveFrame(Frame frame)
        {
            this.frames.TryRemove(frame.Id, out _);
            this._parentIds.TryRemove(frame.Id, out var _);

            if (frame.ParentId != null)
            {
                _childIds.TryGetValue(frame.ParentId, out var childs);
                childs?.Remove(frame.Id);
            }
            else
            {
                this.MainFrame = null;
            }
        }

        public CdpFrame[] GetChildFrames(string frameId)
        {
            this._childIds.TryGetValue(frameId, out var childIds);
            if (childIds == null)
            {
                return Array.Empty<CdpFrame>();
            }

            return childIds
                .Select(id => this.GetById(id))
                .Where(frame => frame != null)
                .ToArray();
        }

        /// <summary>
        /// Gets the parent frame of the specified frame.
        /// </summary>
        /// <param name="frameId"></param>
        /// <returns></returns>
        public Frame? GetParentFrame(string frameId)
        {
            this._parentIds.TryGetValue(frameId, out var parentId);
            return !string.IsNullOrEmpty(parentId) ? this.GetById(parentId) : null;
        }
    }
}
