using CQRS.Core.Events;
using System;

public abstract class AggregateRoot{
	protected Guid _id { get; set; }
	public readonly List<BaseEvent> _changes = new();
	public Guid Id
	{
		get { return _id; }
	}

	public int Version { get; set; } = -1;

	public IEnumerable<BaseEvent> GetUncommittedChanges()
	{
		return _changes;
	}

	public void MarkChangesAsCommitted()
	{
		_changes.Clear();
	}


	private void ApplyChanges(BaseEvent @event, bool isNew)
	{
		var method = this.GetType().GetMethod("Apply", new Type[] {@event.GetType()});

		if (method is null)
		{
			throw new ArgumentNullException(nameof(method), $"The apply method was not found in the aggregate for {@event.GetType().Name}");	
		}

		method.Invoke(this, new object[] { @event });

		if (isNew)
		{
			_changes.Add(@event);
		}
	}

	protected void RaiseEvent(BaseEvent @event)
	{
		ApplyChanges( @event, true );
	}

	public void ReplayEvents(IEnumerable<BaseEvent> replayEvents)
	{
		foreach (var @event in replayEvents)
		{
			ApplyChanges(@event, false);
		}
	}
}

