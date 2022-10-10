using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoModels
{
    public static class ProductODataEdmModel
    {
        public static Microsoft.OData.Edm.IEdmModel GetEdmModel()
        {
            var builder = new Microsoft.OData.ModelBuilder.ODataConventionModelBuilder();


            builder.EntitySet<DemoModels.Product>(nameof(DemoModels.Product));
            builder.EntitySet<DemoModels.ProductExt>(nameof(DemoModels.ProductExt));
            builder.EntitySet<DemoModels.InventoryStock>(nameof(DemoModels.InventoryStock));

            return builder.GetEdmModel();

        }


    }
}
