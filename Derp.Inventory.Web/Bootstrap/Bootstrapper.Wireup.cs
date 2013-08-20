using System;
using Derp.Inventory.Application;
using Derp.Inventory.Domain;
using Derp.Inventory.Messages;
using Derp.Inventory.Web.Infrastructure.GetEventStore;
using Derp.Inventory.Web.Projections;
using Derp.Inventory.Web.Projections.InMemory;
using Derp.Inventory.Web.Projections.Raven;
using Derp.Inventory.Web.Services;
using Derp.Inventory.Web.ViewModels;
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
                EventStore, serializerSettings, itemDetailViewWriter, () => { });
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
            var itemSearch = new ItemSearchResultProjection(
                new RavenDbViewWriter<Guid, ItemSearchResultViewModel>(
                    itemSearchSessionObserver));

            var itemSearchSubscription = new GetEventStoreEventDispatcher(
                EventStore, serializerSettings, itemSearchSessionObserver, itemSearchSessionObserver.CaughtUp);
            itemSearchSubscription.Subscribe<ItemTracked>(itemSearch.Handle);

            itemSearchSubscription.StartDispatching();
        }

        private void RegisterInventoryHandlers()
        {
            var handler = new InventoryHandlers(new GetEventStoreRepository<WarehouseItem>(
                                                    EventStore, "inventory"));

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