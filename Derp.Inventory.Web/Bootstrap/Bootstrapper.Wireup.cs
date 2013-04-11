using System;
using Derp.Inventory.Application;
using Derp.Inventory.Domain;
using Derp.Inventory.Messages;
using Derp.Inventory.Web.GetEventStore;
using Derp.Inventory.Web.Projections;
using Derp.Inventory.Web.Services;
using Derp.Inventory.Web.ViewModels;
using Derp.Inventory.Web.ViewWriters;
using Nancy.TinyIoc;
using Raven.Client;

namespace Derp.Inventory.Web.Bootstrap
{
    public partial class Bootstrapper
    {
        private void RegisterProjections(TinyIoCContainer container)
        {
            var itemDetailRepository = new InMemoryItemDetailRepository();
            container.Register<IItemDetailRepository>(itemDetailRepository);
            var itemDetailViewWriter = new InMemoryViewWriter<Guid, ItemDetailViewModel>(itemDetailRepository);
            var itemDetails = new ItemDetailResultProjection(
                itemDetailViewWriter);

            var itemDetailSubscription = new GetEventStoreEventDispatcher(
                EventStoreConnection, serializerSettings, itemDetailViewWriter, bus);
            itemDetailSubscription.Subscribe<ItemTracked>(itemDetails.Handle);
            itemDetailSubscription.Subscribe<ItemPicked>(itemDetails.Handle);
            itemDetailSubscription.Subscribe<ItemLiquidated>(itemDetails.Handle);
            itemDetailSubscription.Subscribe<ItemReceived>(itemDetails.Handle);
            itemDetailSubscription.Subscribe<ItemQuantityAdjusted>(itemDetails.Handle);
            itemDetailSubscription.Subscribe<CycleCountStarted>(itemDetails.Handle);
            itemDetailSubscription.Subscribe<CycleCountCompleted>(itemDetails.Handle);

            itemDetailSubscription.StartDispatching();

            container.Register<IItemSearchRepository>(
                (c, n) => new RavenItemSearchRepository(c.Resolve<IDocumentSession>()));
            var itemSearchSessionObserver = new CatchUpDocumentSessionObserver<ItemSearchResultViewModel>(DocumentStore);
            bus.Register(itemSearchSessionObserver);
            var itemSearch = new ItemSearchResultProjection(
                new RavenDbViewWriter<Guid, ItemSearchResultViewModel>(
                    itemSearchSessionObserver));

            var itemSearchSubscription = new GetEventStoreEventDispatcher(
                EventStoreConnection, serializerSettings, itemSearchSessionObserver, bus);
            itemSearchSubscription.Subscribe<ItemTracked>(itemSearch.Handle);

            itemSearchSubscription.StartDispatching();
        }

        private void RegisterInventoryHandlers()
        {
            var handler = new InventoryHandlers(new GetEventStoreRepository<WarehouseItem>(
                                                    EventStoreConnection, "inventory"));

            Register<TrackItem>(handler);
            Register<PickItem>(handler);
            Register<StartCycleCount>(handler);
            Register<CompleteCycleCount>(handler);
            Register<AdjustItemQuantity>(handler);
            Register<LiquidateItem>(handler);
            Register<ReceiveItem>(handler);
        }
    }
}