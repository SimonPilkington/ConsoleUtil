using System;
using System.Threading;

namespace SimonP.ConsoleUtil
{
	public sealed class SingleThreadSynchronizationContext : SynchronizationContext
	{
		private readonly SingleThreadedExecutionMessageQueue _actionMessageQueue;

		public SingleThreadSynchronizationContext(SingleThreadedExecutionMessageQueue messageQueue)
		{
			_actionMessageQueue = messageQueue;
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			// This would deadlock. Our client should know what thread they're on, so this shouldn't happen.
			if (_actionMessageQueue.CheckAccess) 
				throw new InvalidOperationException("Send called from the message queue thread.");

			var operation = _actionMessageQueue.EnqueueSynchronous(d, state);

			operation.Wait();
		}

		public override void Post(SendOrPostCallback d, object state) => _actionMessageQueue.Enqueue(d, state);

		// no point actually copying since we're immutable
		public override SynchronizationContext CreateCopy() => this;
	}
}
