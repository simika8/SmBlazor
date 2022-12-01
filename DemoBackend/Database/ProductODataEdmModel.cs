using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public static class ProductODataEdmModel
    {
        public static Microsoft.OData.Edm.IEdmModel GetEdmModel()
        {
            var builder = new Microsoft.OData.ModelBuilder.ODataConventionModelBuilder();


            builder.EntitySet<DemoModels.Product>(nameof(DemoModels.Product)+"Odata");
            builder.EntitySet<DemoModels.ProductExt>(nameof(DemoModels.ProductExt) + "Odata");
            builder.EntitySet<DemoModels.InventoryStock>(nameof(DemoModels.InventoryStock) + "Odata");

            return builder.GetEdmModel();

        }


    }
}
