using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Common.Logger
{
    /// <summary>
    /// Actor模型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Actor<T>
    {
        private readonly ActionBlock<T> _action;

        protected Actor()
        {
            _action = new ActionBlock<T>(T => Receive(T));
        }

        // 接收并处理消息
        protected abstract void Receive(T message);

        public void Post(T message)
        {
            _action.Post(message);
        }

        public void Shutdown()
        {
            _action.Complete();
            _action.Completion.Wait();
        }
    }
}
