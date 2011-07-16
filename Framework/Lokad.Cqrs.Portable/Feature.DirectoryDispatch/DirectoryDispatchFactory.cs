using System;
using Autofac;
using Lokad.Cqrs.Core.Dispatch;

namespace Lokad.Cqrs.Feature.DirectoryDispatch
{
    public static class DirectoryDispatchFactory
    {
        public static ISingleThreadMessageDispatcher CommandBatch(IComponentContext ctx, Action<MessageDirectoryFilter> optionalFilter)
        {
            var builder = ctx.Resolve<MessageDirectoryBuilder>();
            var filter = new MessageDirectoryFilter();
            optionalFilter(filter);
            
            var map = builder.BuildActivationMap(filter.DoesPassFilter);

            var strategy = ctx.Resolve<IMessageDispatchStrategy>();
            return new DispatchCommandBatch(map, strategy);
        }
        public static ISingleThreadMessageDispatcher OneEvent(IComponentContext ctx, Action<MessageDirectoryFilter> optionalFilter)
        {
            var builder = ctx.Resolve<MessageDirectoryBuilder>();
            var filter = new MessageDirectoryFilter();
            optionalFilter(filter);

            var map = builder.BuildActivationMap(filter.DoesPassFilter);

            var strategy = ctx.Resolve<IMessageDispatchStrategy>();
            var observer = ctx.Resolve<ISystemObserver>();
            return new DispatchOneEvent(map, observer, strategy);
        }
    }
}