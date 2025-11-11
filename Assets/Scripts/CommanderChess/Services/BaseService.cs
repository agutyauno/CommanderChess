using System;
using VContainer;
using VContainer.Unity;

namespace CommanderChess.Services
{
    public abstract class BaseService : IDisposable, IInitializable
    {
        [Inject] protected EventBus eventBus;
        bool disposed = false;
        bool initialized = false;

        protected virtual void SubscribeEvents() { }
        protected virtual void UnsubscribeEvents() { }
        protected virtual void OnDispose() { }
        protected virtual void OnInitialize() { }

        public void Initialize()
        {
            if (!initialized)
            {
                SubscribeEvents();
                OnInitialize();
                initialized = true;
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                UnsubscribeEvents();
                OnDispose();
                disposed = true;
            }
        }
    }
}
