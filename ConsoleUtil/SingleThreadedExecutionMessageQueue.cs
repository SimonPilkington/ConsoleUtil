using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace SimonP.ConsoleUtil
{
	public class SingleThreadedExecutionMessageQueue
	{
		public class QueueOperation
		{
			private readonly SendOrPostCallback _callback;
			private readonly object _state;

			private readonly ManualResetEventSlim _invokeEvent;
			private readonly Thread _responsibleThread;

			public bool Synchronous { get; }
			public ExceptionDispatchInfo ExceptionInfo { get; private set; }

			internal QueueOperation(SendOrPostCallback callback, object state, Thread responsibleThread, bool synchronous)
			{
				_callback = callback;
				_state = state;
				_responsibleThread = responsibleThread;

				Synchronous = synchronous;

				if (synchronous)
					_invokeEvent = new ManualResetEventSlim(false);
			}

			public void Invoke()
			{
				try
				{
					_callback(_state);
				}
				catch (Exception x) when (Synchronous)
				{
					ExceptionInfo = ExceptionDispatchInfo.Capture(x);
				}
				finally
				{
					if (Synchronous)
						_invokeEvent.Set();
				}
			}
			
			public void Wait()
			{
				if (!Synchronous)
					throw new InvalidOperationException("Only operations marked as synchronous can be waited on.");

				if (Thread.CurrentThread == _responsibleThread) // That's a deadlock.
					throw new InvalidOperationException("Message queue thread is trying to wait on an operation it is responsible for executing.");

				_invokeEvent.Wait();
				_invokeEvent.Dispose();

				// If an exception was thrown by the operation, rethrow on the waiting thread.
				ExceptionInfo?.Throw();
			}
		}

		private readonly Thread ownerThread = Thread.CurrentThread;
		private BlockingCollection<QueueOperation> actionQueue = new BlockingCollection<QueueOperation>();

		public SingleThreadSynchronizationContext DefaultSynchronizationContext { get; }

		public SingleThreadedExecutionMessageQueue()
		{
			DefaultSynchronizationContext = new SingleThreadSynchronizationContext(this);
		}

		public void EnterMessageLoop()
		{
			while (actionQueue.TryTake(out QueueOperation item, Timeout.Infinite))
				item.Invoke();
		}

		public void Enqueue(SendOrPostCallback d, object state)
		{
			var operation = new QueueOperation(d, state, ownerThread, false);
			actionQueue.Add(operation);
		}

		public void Enqueue(Action a)
		{
			var operation = new QueueOperation(_ => a(), null, ownerThread, false);
			actionQueue.Add(operation);
		}

		public QueueOperation EnqueueSynchronous(SendOrPostCallback d, object state)
		{
			var operation = new QueueOperation(d, state, ownerThread, true);
			actionQueue.Add(operation);

			return operation;
		}

		public QueueOperation EnqueueSynchronous(Action a)
		{
			var operation = new QueueOperation(_ => a(), null, ownerThread, true);
			actionQueue.Add(operation);

			return operation;
		}

		public void TerminateMessageLoop() => actionQueue.CompleteAdding();
		public bool CheckAccess => Thread.CurrentThread == ownerThread;
	}
}
