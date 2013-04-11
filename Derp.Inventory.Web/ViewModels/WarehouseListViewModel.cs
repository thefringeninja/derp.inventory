using System;
using System.Collections;
using System.Collections.Generic;

namespace Derp.Inventory.Web.ViewModels
{
    public class WarehouseListViewModel : IEnumerable<WarehouseNameViewModel>
    {
        public static WarehouseListViewModel Instance = new WarehouseListViewModel(new[]
        {
            new WarehouseNameViewModel("Lilburn", Guid.Parse("11B7F7D7-936D-42F4-8C15-EC13BC41F3EB"))
        });

        private readonly IEnumerable<WarehouseNameViewModel> warehouses;

        public WarehouseListViewModel(IEnumerable<WarehouseNameViewModel> warehouses)
        {
            this.warehouses = warehouses;
        }

        #region IEnumerable<WarehouseNameViewModel> Members

        public IEnumerator<WarehouseNameViewModel> GetEnumerator()
        {
            return warehouses.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}