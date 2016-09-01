using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UI.Common.Tracers
{
    /// <summary>
    /// Actor模型接口
    /// </summary>
    internal interface IActor
    {
        /// <summary>
        /// 执行
        /// </summary>
        void Execute();
        /// <summary>
        /// 退出标志
        /// </summary>
        bool Exited { get; }
        /// <summary>
        /// 消息个数
        /// </summary>
        int MessageCount { get; }
        /// <summary>
        /// Actor上下文
        /// </summary>
        ActorContext Context { get; }
    }

    /// <summary>
    /// Actor上下文类
    /// </summary>
    internal class ActorContext
    {
        // 表示某一时期处理消息的状态
        public int Status;

        /// <summary>
        /// 保存Actor模型类的引用
        /// </summary>
        public IActor Actor { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="actor"></param>
        public ActorContext(IActor actor)
        {
            this.Actor = actor;
        }

        // Actor模型执行状态，包括 等待、执行和退出三个状态
        public const int Waiting = 0;
        public const int Executing = 1;
        public const int Exited = 2;
    }

    /// <summary>
    /// Actor模型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Actor<T> : IActor
    {
        // Actor上下文对象
        private readonly ActorContext _context;

        // Exit flag
        private bool _exited = false;

        // Message queue
        private readonly ConcurrentQueue<T> _messageQueue = new ConcurrentQueue<T>();

        /// <summary>
        /// 投递消息
        /// </summary>
        /// <param name="message"></param>
        public void Post(T message)
        {
            if (this._exited)
            {
                return;
            }
            this._messageQueue.Enqueue(message);
            // 准备执行处理一个消息
            Dispatcher.Instance.ReadyToExecute(this);
        }

        // 接收并处理消息
        protected abstract void Receive(T message);

        /// <summary>
        /// Constructor
        /// </summary>
        protected Actor()
        {
            this._context = new ActorContext(this);
        }

        #region Properties
        /// <summary>
        /// Actor上下文
        /// </summary>
        ActorContext IActor.Context
        {
            get { return this._context; }
        }
        /// <summary>
        /// 是否退出标志
        /// </summary>
        bool IActor.Exited
        {
            get { return this._exited; }
        }
        /// <summary>
        /// 消息队列中消息个数
        /// </summary>
        int IActor.MessageCount
        {
            get { return this._messageQueue.Count; }
        }
        #endregion

        /// <summary>
        /// 处理消息
        /// </summary>
        void IActor.Execute()
        {
            T message;
            var dequeueSucess = this._messageQueue.TryDequeue(out message);

            if (dequeueSucess)
            {
                this.Receive(message);
            }
        }

        protected void Start()
        {
            this._exited = false;
        }
        /// <summary>
        /// 退出模式
        /// </summary>
        protected void Exit()
        {
            this._exited = true;
        }
    }

    /// <summary>
    /// 分发类
    /// </summary>
    internal class Dispatcher
    {
        // Singleton
        private static readonly Dispatcher _instance = new Dispatcher();

        public static Dispatcher Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Private Constructor
        /// </summary>
        private Dispatcher()
        {

        }

        /// <summary>
        /// 消息预处理：设置处理状态
        /// </summary>
        /// <param name="actor"></param>
        public void ReadyToExecute(IActor actor)
        {
            if (actor.Exited) return;
            // 修改当前状态为执行态
            int status = Interlocked.CompareExchange(ref actor.Context.Status, ActorContext.Executing, ActorContext.Waiting);

            if (status == ActorContext.Waiting)
            {
                // 线程池的可用工作线程处理消息
                ThreadPool.QueueUserWorkItem(this.Execute, actor);
            }
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="o"></param>
        private void Execute(object o)
        {
            IActor actor = (IActor)o;
            // 
            actor.Execute();
            // 如果退出，则设置退出标志位
            if (actor.Exited)
            {
                Thread.VolatileWrite(ref actor.Context.Status, ActorContext.Exited);
            }
            // 否则进行下一个消息处理
            else
            {
                Thread.VolatileWrite(ref actor.Context.Status, ActorContext.Waiting);
                if (actor.MessageCount > 0)
                {
                    this.ReadyToExecute(actor);
                }
            }
        }
    }
}
