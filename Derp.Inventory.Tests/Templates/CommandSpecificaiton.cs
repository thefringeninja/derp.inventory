using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Derp.Inventory.Infrastructure;
using Simple.Testing.ClientFramework;

namespace Derp.Inventory.Tests.Templates
{
    public class CommandSpecification<TAggregate, TCommand> :
        TypedSpecification<CommandSpecification<TAggregate, TCommand>.Results>
        where TAggregate : AggregateRoot
        where TCommand : class
    {
        public Guid AggregateId = Guid.NewGuid();
        public Action Before;
        public List<Expression<Func<Results, bool>>> Expect = new List<Expression<Func<Results, bool>>>();
        public Action Finally;
        public List<Event> Given = new List<Event>();
        public string Name;

        public Func<IRepository<TAggregate>, Handles<TCommand>> OnHandler;

        public TCommand When;

        #region TypedSpecification<CommandSpecification<TAggregate,TCommand>.Results> Members

        public string GetName()
        {
            return Name;
        }

        public Action GetBefore()
        {
            return Before;
        }

        public Delegate GetOn()
        {
            return new Func<List<object>>(() => Given.OfType<object>().Union(new[] {When}).ToList());
        }

        public Delegate GetWhen()
        {
            return new Func<List<object>, Results>(messages =>
            {
                var results = new Results();


                try
                {
                    if (When == null) throw new InvalidOperationException("When not set.");

                    var events = messages.Take(messages.Count() - 1).Cast<Event>();

                    var storage = new ConcurrentDictionary<Guid, List<Event>>
                        (new Dictionary<Guid, List<Event>>
                        {
                            {
                                AggregateId, events.ToList()
                            }
                        });

                    var repository = new InMemoryRepository<TAggregate>(storage);

                    if (OnHandler == null)
                    {
                        throw new InvalidOperationException("Did you forget to set OnHandler?");
                    }
                    var handler = OnHandler(repository);

                    if (handler == null)
                    {
                        throw new InvalidOperationException("OnHandler returned null.");
                    }

                    handler.Handle(When);

                    results.Decisions = storage[AggregateId].Skip(events.Count());
                }
                catch (Exception ex)
                {
                    results.Exception = ex;
                }
                return results;
            });
        }

        public IEnumerable<Expression<Func<Results, bool>>> GetAssertions()
        {
            return Expect;
        }

        public Action GetFinally()
        {
            return Finally;
        }

        #endregion

        #region Nested type: Results

        public class Results
        {
            public IEnumerable<object> Decisions;
            public Exception Exception = new ExpectedExceptionButNoExceptionWasThrownException();

            public bool ThrewAnException
            {
                get { return false == Exception is ExpectedExceptionButNoExceptionWasThrownException; }
            }

            public override string ToString()
            {
                return (Decisions == null
                        || false == Decisions.Any())
                           ? "Nothing"
                           : Decisions.Aggregate(
                               new StringBuilder(), (builder, e) => builder.Append(e).AppendLine())
                                      .ToString();
            }
        }

        #endregion
    }
}